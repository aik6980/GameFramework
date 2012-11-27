using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

using SlimDX;
using SlimDX.DXGI;
using SlimDX.Direct3D11;
using SlimDX.D3DCompiler;

using ZeroEngine.Engine;

using SlimDXDevice11 = SlimDX.Direct3D11.Device;
using SlimDXBuffer11 = SlimDX.Direct3D11.Buffer;

namespace ZeroEngine.GraphicDevice.Direct3d11
{
    class Device3dD3d11 : Device3d
    {
        Form m_hWindow;  
        SlimDXDevice11 m_D3dDevice = null;       
        SwapChain m_SwapChain = null;

        // render pipeline states
        EffectExD3d11 m_CurrEffect;
        RenderTargetView[] m_CurrRenderTargets = null; // MRTS
        DepthStencilView m_CurrDepthStencil = null;
        Viewport[] m_CurrViewports = null;

        // resources
        Dictionary<string, SlimDXBuffer11> m_BufferList = new Dictionary<string, SlimDXBuffer11>();

        Dictionary<string, RenderTargetView>        m_RTVList = new Dictionary<string, RenderTargetView>();
        Dictionary<string, DepthStencilView>        m_DSVList = new Dictionary<string, DepthStencilView>();
        Dictionary<string, ShaderResourceView>      m_SRVList = new Dictionary<string, ShaderResourceView>();
        Dictionary<string, UnorderedAccessView>     m_UAVList = new Dictionary<string, UnorderedAccessView>();

        Dictionary<string, Viewport> m_ViewportList = new Dictionary<string, Viewport>();

        Dictionary<string, EffectExD3d11> m_EffectList = new Dictionary<string, EffectExD3d11>();
        Dictionary<VertexLayoutD3d11, InputLayout> m_InputLayoutList = new Dictionary<VertexLayoutD3d11, InputLayout>();

        public SlimDXDevice11 GetDevice() { return m_D3dDevice;  }
        public DeviceContext GetImmediateContext() { return m_D3dDevice.ImmediateContext; }

        public void Initialize(Device3dInit initStruct)
        {
            // store info from initStruct
            m_hWindow = initStruct.hWindow;

            // device state
            m_CurrRenderTargets = new RenderTargetView[4];
            m_CurrViewports = new Viewport[4];

            {
                // swap chain
                var desc = new SwapChainDescription()
                {
                    BufferCount = 1,
                    ModeDescription = new ModeDescription(initStruct.iScreenWidth, initStruct.iScreenHeight, new Rational(60, 1), Format.R8G8B8A8_UNorm),
                    IsWindowed = true,
                    OutputHandle = m_hWindow.Handle,
                    SampleDescription = new SampleDescription(1, 0),
                    SwapEffect = SwapEffect.Discard,
                    Usage = Usage.RenderTargetOutput
                };
                // create device and swap chain 
                DeviceCreationFlags flags = DeviceCreationFlags.Debug;
                SlimDXDevice11.CreateWithSwapChain(DriverType.Hardware, flags, desc, out m_D3dDevice, out m_SwapChain);

                // [Q] what SetWindowAssociation() is about?
#warning "[Q] what SetWindowAssociation() is about?"
                m_D3dDevice.Factory.SetWindowAssociation(initStruct.hWindow.Handle, WindowAssociationFlags.IgnoreAll);
            }

            // create back buffers
            Texture2D backBuffer = Texture2D.FromSwapChain<Texture2D>(m_SwapChain, 0);
            var renderTargetView = new RenderTargetView(m_D3dDevice, backBuffer);
            m_RTVList.Add("BackBuffer", renderTargetView);
            // create depth buffer
            CreateDepthStencilBuffer(new CreateDepthStencilBufferCmd()
            {
                Name = "BackBuffer",
                Width = m_hWindow.ClientSize.Width,
                Height = m_hWindow.ClientSize.Height
            } );

            // create viewport
            var viewport = new Viewport(0.0f, 0.0f, m_hWindow.ClientSize.Width, m_hWindow.ClientSize.Height, 0.0f, 1.0f);
            m_ViewportList.Add("Default", viewport);
            SetViewport("Default");

            // set our render pipeline to the default state
            m_D3dDevice.ImmediateContext.Rasterizer.SetViewports(m_CurrViewports);
            m_D3dDevice.ImmediateContext.OutputMerger.SetTargets(m_CurrDepthStencil, m_CurrRenderTargets);

            // success
            Debug.Helper.Log("Device3d D3d11 is successfully initialized");
        }

        public void Destroy()
        {

        }

        public void SetDefaultstate()
        {
            // shader pipeline
            m_D3dDevice.ImmediateContext.InputAssembler.InputLayout = null;
            m_D3dDevice.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            m_D3dDevice.ImmediateContext.VertexShader.Set(null);
            m_D3dDevice.ImmediateContext.HullShader.Set(null);
            m_D3dDevice.ImmediateContext.DomainShader.Set(null);
            m_D3dDevice.ImmediateContext.GeometryShader.Set(null);
            m_D3dDevice.ImmediateContext.PixelShader.Set(null);

            // render state
            m_D3dDevice.ImmediateContext.Rasterizer.State = RasterizerState.FromDescription(m_D3dDevice, new RasterizerStateDescription()
                {
                    CullMode = CullMode.Back,
                    FillMode = FillMode.Solid,
                });

            m_D3dDevice.ImmediateContext.OutputMerger.DepthStencilState = DepthStencilState.FromDescription(m_D3dDevice, new DepthStencilStateDescription()
                {
                    DepthComparison     = Comparison.Less,
                    DepthWriteMask      = DepthWriteMask.All,
                    IsDepthEnabled      = true,
                    IsStencilEnabled    = false
                });

            var blendState = new RenderTargetBlendDescription()
                {
                    BlendEnable = false,
                    BlendOperation = BlendOperation.Add,
                    DestinationBlend = BlendOption.InverseSourceAlpha,
                    SourceBlend = BlendOption.SourceAlpha,
                    RenderTargetWriteMask = ColorWriteMaskFlags.All
                };

            var blendStateDesc = new BlendStateDescription();
            blendStateDesc.AlphaToCoverageEnable = false;
            blendStateDesc.IndependentBlendEnable = false;
            blendStateDesc.RenderTargets[0] = blendState;

            m_D3dDevice.ImmediateContext.OutputMerger.BlendState = BlendState.FromDescription(m_D3dDevice, blendStateDesc);
        }

        // Basic GPU Commands
        public void BeginFrame()
        {
            m_D3dDevice.ImmediateContext.OutputMerger.SetTargets(m_CurrDepthStencil, m_CurrRenderTargets);
        }

        public void EndFrame()
        {
            m_SwapChain.Present(0, PresentFlags.None);
        }

        public void ClearColor(Color color, uint id = 0)
        {
            var renderTarget = m_CurrRenderTargets[id];
            m_D3dDevice.ImmediateContext.ClearRenderTargetView(renderTarget, color);
        }

        public void ClearDepthStencil(float clearDepthVal, byte clearStencilVal )
        {
            m_D3dDevice.ImmediateContext.ClearDepthStencilView(m_CurrDepthStencil, DepthStencilClearFlags.Stencil | DepthStencilClearFlags.Depth,
                clearDepthVal, clearStencilVal);
        }

        public void SetRenderTarget(string name, uint id = 0)
        {
            RenderTargetView target = null;
            m_RTVList.TryGetValue(name, out target);
            m_CurrRenderTargets[id] = target;
        }

        public void SetViewport(string name, uint id = 0)
        {
            Viewport target;
            m_ViewportList.TryGetValue(name, out target);
            m_CurrViewports[id] = target;
        }

        public void SetDepthStencilTarget(string name)
        {
            DepthStencilView target = null;
            m_DSVList.TryGetValue(name, out target);
            m_CurrDepthStencil = target;
        }

        // Resources
        public struct SCreateBufferCmd
        {
            public string      Name;
            public int         SizeInBytes;
            public BindFlags   BufferBindFlags;
        }

        public SlimDXBuffer11 CreateBuffer(SCreateBufferCmd cmd)
        {
            var newBuffer = new SlimDX.Direct3D11.Buffer(Engine.Global.Device3d.GetDevice(), new BufferDescription()
            {
                BindFlags = cmd.BufferBindFlags,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = cmd.SizeInBytes,
                Usage = ResourceUsage.Default
            });

            m_BufferList.Add(cmd.Name, newBuffer);

            return newBuffer;
        }

        public void UpdateBufferData(string bufferName, DataStream data)
        {
            var buffer = GetBuffer(bufferName);

            DataBox src = new DataBox((int)data.Length, 0, data);
            Engine.Global.Device3d.GetImmediateContext().UpdateSubresource(src, buffer, 0);
        }

        public SlimDXBuffer11 GetBuffer(string name)
        {
            SlimDXBuffer11 obj = null;
            m_BufferList.TryGetValue(name, out obj);
            return obj;
        }

        public struct CreateTextureBufferCmd
        {
            public string           Name;
            public int              Width, Height;
            public float            RatioW, RatioH;
            public Format           UsageFmt;
            public BindFlags        UsageFlags;          
        };

        public void CreateTextureBuffer(CreateTextureBufferCmd cmd)
        {
            var w = cmd.Width;
            var h = cmd.Height;
            if (cmd.RatioW > 0.0f && cmd.RatioH > 0.0f)
            {
                w = (int)(m_hWindow.ClientSize.Width * cmd.RatioW);
                h = (int)(m_hWindow.ClientSize.Height * cmd.RatioH);
            }

            var desc = new Texture2DDescription
            {
                ArraySize = 1,
                BindFlags = cmd.UsageFlags,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = cmd.UsageFmt,
                Height = h,
                Width = w,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.None,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default
            };

            using (var buffer = new Texture2D(m_D3dDevice, desc))
            {
                CreateViews(cmd.Name, cmd.UsageFlags, buffer);
            }
        }

        void CreateViews(string name, BindFlags flags, SlimDX.Direct3D11.Resource resource)
        {
            if (flags.HasFlag(BindFlags.RenderTarget))
            {
                var view = new RenderTargetView(m_D3dDevice, resource);
                m_RTVList.Add(name, view);
            }

            if (flags.HasFlag(BindFlags.DepthStencil))
            {
                var view = new DepthStencilView(m_D3dDevice, resource);
                m_DSVList.Add(name, view);
            }

            if (flags.HasFlag(BindFlags.ShaderResource))
            {
                var view = new ShaderResourceView(m_D3dDevice, resource);
                m_SRVList.Add(name, view);
            }

            if (flags.HasFlag(BindFlags.UnorderedAccess))
            {
                var view = new UnorderedAccessView(m_D3dDevice, resource);
                m_UAVList.Add(name, view);
            }
        }

        public struct CreateDepthStencilBufferCmd
        {
            public string Name;
            public int Width;
            public int Height;
        };
        public void CreateDepthStencilBuffer(CreateDepthStencilBufferCmd cmd)
        {
            CreateTextureBuffer(new CreateTextureBufferCmd
                {
                    Name = cmd.Name,
                    Width = cmd.Width,
                    Height = cmd.Height,
                    UsageFlags = BindFlags.DepthStencil,
                    UsageFmt = Format.D32_Float
                });
        }

        public struct CreateEffectCmd
        {
            public string Name;
            public string SrcFileName;
        };
        public void CreateEffect(CreateEffectCmd cmd)
        {
            EffectExD3d11 fx = new EffectExD3d11();
            fx.m_D3dDevice = m_D3dDevice;
            fx.CreateFromFile(cmd.SrcFileName);

            m_EffectList.Add(cmd.Name, fx);
        }

        public EffectExD3d11 GetEffect(string name)
        {
            EffectExD3d11 fx = null;
            m_EffectList.TryGetValue(name, out fx);

            return fx;
        }

        public InputLayout GetInputLayout(string effectName, VertexLayoutD3d11 decl)
        {
            InputLayout layout = null;
            EffectExD3d11 fx = null;
            if (m_EffectList.TryGetValue(effectName, out fx))
            {
                if (!m_InputLayoutList.TryGetValue(decl, out layout))
                {
                    // if there isn't a match, try to create a new layout 
                    // create a new layout using input elems
                    layout = new InputLayout(m_D3dDevice, fx.GetMainTechnique().Description.Signature, decl.Elems);
                    m_InputLayoutList.Add(decl, layout);
                }
            }

            return layout;
        }

        // Extended GPU Commands
        void Draw(VertexBufferD3d11 vb, 
            IndexBufferD3d11 ib, RenderParamsListArray renderParamsListArry)
        {
            // apply vertex and index buffers
            

            // apply gpu program parameters
            m_CurrEffect.Apply(renderParamsListArry);

            if (ib != null)
            {
                // draw with index buffer
            }
            else
            {
                uint vertexCount = 0;
                int vertexStartLocation = 0;
                m_D3dDevice.ImmediateContext.Draw((int)vertexCount, vertexStartLocation);
            }
        }
    }
}
