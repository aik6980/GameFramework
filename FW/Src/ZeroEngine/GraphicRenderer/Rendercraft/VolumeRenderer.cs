using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlimDX;
using SlimDX.DXGI;
using SlimDX.Direct3D11;

using ZeroEngine.GraphicDevice;
using ZeroEngine.GraphicDevice.Direct3d11;
using ZeroEngine.Resource.GeomGenerator;

namespace ZeroEngine.GraphicRenderer.Rendercraft
{
    class CMeshDesc
    {
        public PrimitiveTopology   Topology = PrimitiveTopology.TriangleList;
        public VertexBufferBinding VBBinding;
        public int  VertexCount = 0;
        public int  IndexCount = 0;
    }

    class CVolumeRenderer : Renderer
    {
        CQuad[] m_LookupMesh = new CQuad[6];

        FatVertexDesc m_VertDesc = new FatVertexDesc();
        CMeshDesc m_MeshDesc = new CMeshDesc();

        public override void Load()
        {
            // load shader
            Engine.Global.Device3d.CreateEffect(new Device3dD3d11.CreateEffectCmd()
            {
                Name = "FxRendercraft_Terrain",
                SrcFileName = Engine.Global.FileSystem.GetResourcePath("Engine/Shaders/FxRendercraft_Terrain.fx")
            });

            // create meshes for the lookup
            m_LookupMesh[0] = CQuad.CreateFromPlane(Vector3.UnitX, 0.5f, 0.5f);
            m_LookupMesh[1] = CQuad.CreateFromPlane(-Vector3.UnitX, 0.5f, 0.5f);
            m_LookupMesh[2] = CQuad.CreateFromPlane(Vector3.UnitY, 0.5f, 0.5f);
            m_LookupMesh[3] = CQuad.CreateFromPlane(-Vector3.UnitY, 0.5f, 0.5f);
            m_LookupMesh[4] = CQuad.CreateFromPlane(Vector3.UnitZ, 0.5f, 0.5f);
            m_LookupMesh[5] = CQuad.CreateFromPlane(-Vector3.UnitZ, 0.5f, 0.5f);
        }

        public override void PreRender()
        {

        }

        public override void Render(RenderParamsListArray paramsListArray)
        {
            var device = Engine.Global.Device3d.GetDevice();
            // try to get the matched input-layout
            var layout = Engine.Global.Device3d.GetInputLayout("FxRendercraft_Terrain", m_VertDesc.Decl);
            device.ImmediateContext.InputAssembler.InputLayout = layout;
            device.ImmediateContext.InputAssembler.PrimitiveTopology = m_MeshDesc.Topology;
            //device.ImmediateContext.InputAssembler.SetIndexBuffer(Engine.Global.Device3d.GetBuffer("QuadIB"), Format.R32_UInt, 0);
            device.ImmediateContext.InputAssembler.SetVertexBuffers(0, m_MeshDesc.VBBinding);

            var paramslist = new RenderParamsList();
            paramslist.Set("WorldXf", Matrix.Identity);

            paramsListArray.Push(paramslist);

            var fx = Engine.Global.Device3d.GetEffect("FxRendercraft_Terrain");
            fx.Apply(paramsListArray);

            // draw
            device.ImmediateContext.Draw(m_MeshDesc.VertexCount, 0);
            Engine.Global.Device3d.SetDefaultstate();

            paramsListArray.Pop();
        }

        public void Generate(CVolumeBuffer volume)
        {
            int quad_count = Primitive_Count(volume);
            int tri_count = quad_count * 2;
            int vert_count = tri_count * 3;
            // calculate total vertices we need
            List<FatVertex> verts = new List<FatVertex>(vert_count);

            int size = volume.Size;

            for (int z = 0; z < size; ++z)
            {
                for (int y = 0; y < size; ++y)
                {
                    for (int x = 0; x < size; ++x)
                    {
                        int value = volume.GetData(new Vector3(x, y, z));
                        if (value == 0)
                        {
                            if (volume.GetData_BoundarySafe(new Vector3(x + 1, y, z)) > 0)
                            {
                                CQuad newQuad = m_LookupMesh[1].Clone();
                                newQuad.Translate(new Vector3(x + 1, y, z));
                                verts.AddRange(newQuad.verts);
                            }
                            if (volume.GetData_BoundarySafe(new Vector3(x - 1, y, z)) > 0)
                            {
                                CQuad newQuad = m_LookupMesh[0].Clone();
                                newQuad.Translate(new Vector3(x - 1, y, z));
                                verts.AddRange(newQuad.verts);
                            }
                            if (volume.GetData_BoundarySafe(new Vector3(x, y + 1, z)) > 0)
                            {
                                CQuad newQuad = m_LookupMesh[3].Clone();
                                newQuad.Translate(new Vector3(x, y + 1, z));
                                verts.AddRange(newQuad.verts);
                            }
                            if (volume.GetData_BoundarySafe(new Vector3(x, y - 1, z)) > 0)
                            {
                                CQuad newQuad = m_LookupMesh[2].Clone();
                                newQuad.Translate(new Vector3(x, y - 1, z));
                                verts.AddRange(newQuad.verts);
                            }
                            if (volume.GetData_BoundarySafe(new Vector3(x, y, z + 1)) > 0)
                            {
                                CQuad newQuad = m_LookupMesh[5].Clone();
                                newQuad.Translate(new Vector3(x, y, z + 1));
                                verts.AddRange(newQuad.verts);
                            }
                            if (volume.GetData_BoundarySafe(new Vector3(x, y, z - 1)) > 0)
                            {
                                CQuad newQuad = m_LookupMesh[4].Clone();
                                newQuad.Translate(new Vector3(x, y, z - 1));
                                verts.AddRange(newQuad.verts);
                            }
                        }
                    }
                }
            }

            var vb = Engine.Global.Device3d.CreateBuffer(new Device3dD3d11.SCreateBufferCmd()
            {
                Name = "VolumeDataVB",
                SizeInBytes = (int)vert_count * m_VertDesc.SizeInBytes,
                BufferBindFlags = BindFlags.VertexBuffer
            });
            m_MeshDesc.VBBinding = new VertexBufferBinding(vb, m_VertDesc.SizeInBytes, 0);

            // update data buffer
            Engine.Global.Device3d.UpdateBufferData("VolumeDataVB", new DataStream(verts.ToArray(), false, false));
            m_MeshDesc.VertexCount = vert_count;
        }

        // this function will count the number of quads for our terrain
        int Primitive_Count(CVolumeBuffer volume)
        {
            int count = 0;

            int size = volume.Size;

            for (int z = 0; z < size; ++z)
            {
                for (int y = 0; y < size; ++y)
                {
                    for (int x = 0; x < size; ++x)
                    {
                        int value = volume.GetData(new Vector3(x, y, z));
                        // if we are in the blank space check and see if there is any valid terrain around us 
                        if (value == 0)
                        {
                            if (volume.GetData_BoundarySafe(new Vector3(x + 1, y, z)) > 0) count++;
                            if (volume.GetData_BoundarySafe(new Vector3(x - 1, y, z)) > 0) count++;
                            if (volume.GetData_BoundarySafe(new Vector3(x, y + 1, z)) > 0) count++;
                            if (volume.GetData_BoundarySafe(new Vector3(x, y - 1, z)) > 0) count++;
                            if (volume.GetData_BoundarySafe(new Vector3(x, y, z + 1)) > 0) count++;
                            if (volume.GetData_BoundarySafe(new Vector3(x, y, z - 1)) > 0) count++;
                        }
                    }
                }
            }

            return count;
        }
    }
}
