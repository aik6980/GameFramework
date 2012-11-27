using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using SlimDX;
using SlimDX.DXGI;
using SlimDX.Direct3D11;

using ZeroEngine.World;
using ZeroEngine.GraphicDevice;
using ZeroEngine.GraphicDevice.Direct3d11;

using ZeroEngine.Resource.GeomGenerator;

namespace ZeroEngine.GraphicRenderer
{
    class MeshDesc
    {
        public PrimitiveTopology    Topology = PrimitiveTopology.PatchListWith4ControlPoints;
        public VertexBufferBinding VBBinding;
        public int  IndexCount;
    }

    class BasicRenderer : Renderer
    {
        Entity m_Owner;

        FatVertexDesc m_VertDesc = new FatVertexDesc();
        MeshDesc m_MeshDesc = new MeshDesc();

        public override void Load()
        {
            // create a primitive
            var geomGenerator = new CGeomGenerator();
            var meshData = geomGenerator.CreateSphere(new CGeomGenerator.SCreateSphereCmd()
                {
                    Radius = 0.5f,
                    SlicesCount = 8,
                    StackCount = 8
                }); 

            // create a buffer
            var vb = Engine.Global.Device3d.CreateBuffer(new Device3dD3d11.SCreateBufferCmd()
                {
                    Name = "QuadVB",
                    SizeInBytes = (int)meshData.VB.Length,
                    BufferBindFlags = BindFlags.VertexBuffer
                });
            m_MeshDesc.VBBinding = new VertexBufferBinding(vb, m_VertDesc.SizeInBytes, 0);

            var ib = Engine.Global.Device3d.CreateBuffer(new Device3dD3d11.SCreateBufferCmd()
            {
                Name = "QuadIB",
                SizeInBytes = (int)meshData.IB.Length,
                BufferBindFlags = BindFlags.IndexBuffer
            });

            // update data buffer
            Engine.Global.Device3d.UpdateBufferData("QuadVB", meshData.VB);
            Engine.Global.Device3d.UpdateBufferData("QuadIB", meshData.IB);
            m_MeshDesc.IndexCount = meshData.IndexCount;

            // load shader
            Engine.Global.Device3d.CreateEffect(new Device3dD3d11.CreateEffectCmd()
            {
                Name = "Tess_Generic",
                SrcFileName = Engine.Global.FileSystem.GetResourcePath("Engine/Shaders/TessQuad_Generic.fx")
            });
        }

        int m_TessParam = 1;
        public override void PreRender()
        {
            if(Engine.Global.InputManager.IsKeyPressed(Keys.Y)) m_TessParam++;
            if(Engine.Global.InputManager.IsKeyPressed(Keys.H)) m_TessParam--;
        }

        public override void Render(RenderParamsListArray paramsListArray )
        {
            {
                var device = Engine.Global.Device3d.GetDevice();
                // try to get the matched input-layout
                var layout = Engine.Global.Device3d.GetInputLayout("Tess_Generic", m_VertDesc.Decl);
                device.ImmediateContext.InputAssembler.InputLayout = layout;
                device.ImmediateContext.InputAssembler.PrimitiveTopology = m_MeshDesc.Topology;
                device.ImmediateContext.InputAssembler.SetIndexBuffer(Engine.Global.Device3d.GetBuffer("QuadIB"), Format.R32_UInt, 0);
                device.ImmediateContext.InputAssembler.SetVertexBuffers(0, m_MeshDesc.VBBinding);

                // setup effect
                float scale = 5.0f;
                Matrix objToWorld = Matrix.Transformation(new Vector3(scale),
                    Quaternion.Identity, new Vector3(1.0f, 1.0f, 1.0f), Vector3.Zero,
                    Quaternion.Identity, new Vector3(0.0f, 0.0f, 20.0f));

                var paramslist = new RenderParamsList();
                paramslist.Set("WorldXf", objToWorld);
                paramslist.Set("g_TessFactor", m_TessParam);

                paramsListArray.Push(paramslist);

                var fx = Engine.Global.Device3d.GetEffect("Tess_Generic");
                fx.Apply(paramsListArray);
                
                // draw
                device.ImmediateContext.DrawIndexed(m_MeshDesc.IndexCount, 0, 0);
                Engine.Global.Device3d.SetDefaultstate();

                paramsListArray.Pop();
            }
        }
    }
}
