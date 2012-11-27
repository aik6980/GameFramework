using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlimDX;

using ZeroEngine.GraphicDevice;

namespace ZeroEngine.Resource.GeomGenerator
{
    class CGeomGenerator
    {

        public CMeshData CreateQuad()
        {
            // create a simple primitive
            FatVertex[] verts = new FatVertex[4];
            verts[0].Position = new Vector4(1.0f, -1.0f, 0.5f, 1.0f);
            verts[1].Position = new Vector4(1.0f, 1.0f, 0.5f, 1.0f);
            verts[2].Position = new Vector4(-1.0f, -1.0f, 0.5f, 1.0f);
            verts[3].Position = new Vector4(-1.0f, 1.0f, 0.5f, 1.0f);
            DataStream vertexStream = new DataStream(verts, false, false);

            // index buffer
            var indices = new uint[4] { 0, 1, 2, 3 };
            DataStream indexStream = new DataStream(indices, false, false);

            // return mesh data
            var meshData = new CMeshData()
            {
                VB = vertexStream,
                IB = indexStream,
                IndexCount = indices.Length
            };
            return meshData;
        }

        public struct SCreateSphereCmd
        {
            public float Radius;
            public int SlicesCount;
            public int StackCount;
        }
        public CMeshData CreateSphere(SCreateSphereCmd cmd)
        {
            // output data
            List<FatVertex> vb = new List<FatVertex>();
            List<int> ib = new List<int>();
      
            // main vertices
            FatVertex topVertex = new FatVertex()
            {
                Position = new Vector4(0.0f, cmd.Radius, 0.0f, 1.0f)
            };

            FatVertex btmVertex = new FatVertex()
            {
                Position = new Vector4(0.0f, -cmd.Radius, 0.0f, 1.0f)
            };

            vb.Add(topVertex);

            float phiStep   = (float)(Math.PI / cmd.StackCount);
            float thetaStep = (float)(2.0f * Math.PI / cmd.SlicesCount);

            // Compute vertices for each stack ring (do not count the poles as rings)
            for (int i = 1; i <= cmd.StackCount - 1; ++i)
            {
                float phi = phiStep * i;

                // Vertices of ring
                for (int j = 0; j <= cmd.SlicesCount; ++j)
                {
                    float theta = thetaStep * j;
                    FatVertex v = new FatVertex();
                    // spherical to cartesian
                    v.Position.X = (float)(cmd.Radius * Math.Sin(phi) * Math.Cos(theta));
                    v.Position.Y = (float)(cmd.Radius * Math.Cos(phi));
                    v.Position.Z = (float)(cmd.Radius * Math.Sin(phi) * Math.Sin(theta));
                    v.Position.W = 1.0f;

                    // calculate tangent (1st degree partial derivative of theta (fixed y))
                    Vector3 tangent;
                    tangent.X = (float)(-cmd.Radius * Math.Sin(phi) * Math.Sin(theta));
                    tangent.Y = (float)(0.0f);
                    tangent.Z = (float)( cmd.Radius * Math.Sin(phi) * Math.Cos(theta));
                    tangent.Normalize();

                    // normal is basically our normalized position
                    v.Normal = v.Position;
                    v.Normal.W = 0.0f;
                    v.Normal.Normalize();

                    // uv coord
                    v.TexCoord0.X = (float)(theta / (2.0f * Math.PI));
                    v.TexCoord0.Y = (float)(phi / (Math.PI));

                    vb.Add(v);
                }
            }

            vb.Add(btmVertex);

            // indices generator (Quad based)
            
            // compute indices for the top stack
            for (int i = 1; i <= cmd.SlicesCount; ++i)
            {
                ib.Add(0);
                ib.Add(0);
                ib.Add(i);
                ib.Add(i + 1);
            }

            // offset indices to the first ring (skipping the north pole vertex)
            int baseIndex = 1; // skipping the top pole
            int ringVertexCount = cmd.SlicesCount + 1;

            for (int i = 0; i < cmd.StackCount - 2; ++i)
            {
                for (int j = 0; j < cmd.SlicesCount; ++j)
                {
                    ib.Add(baseIndex + i * ringVertexCount + j);
                    ib.Add(baseIndex + i * ringVertexCount + (j + 1));
                    ib.Add(baseIndex + (i + 1) * ringVertexCount + j);
                    ib.Add(baseIndex + (i + 1) * ringVertexCount + (j + 1));
                }
            }

            // now the bottom ring
            int southPoleIndex = vb.Count - 1; // the last vertex in the list is the bottom pole
            baseIndex = southPoleIndex - ringVertexCount;

            for (int i = 0; i < cmd.SlicesCount; ++i)
            {
                ib.Add(baseIndex + i);
                ib.Add(baseIndex + (i + 1));
                ib.Add(southPoleIndex);
                ib.Add(southPoleIndex);
            }

            // return mesh data
            var meshData = new CMeshData()
            {
                VB = new DataStream(vb.ToArray(), false, false),
                IB = new DataStream(ib.ToArray(), false, false),
                IndexCount = ib.Count
            };
            return meshData;
        }
    }
}
