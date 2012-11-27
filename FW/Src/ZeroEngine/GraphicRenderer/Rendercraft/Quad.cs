using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlimDX;

using ZeroEngine.Resource.GeomGenerator;

namespace ZeroEngine.GraphicRenderer.Rendercraft
{
    class CQuad
    {
        public FatVertex[] verts   = new FatVertex[6];

        public CQuad Clone()
        {
            CQuad newQuad = new CQuad();
            verts.CopyTo(newQuad.verts, 0);
            return newQuad;
        }

        public static CQuad CreateFromPlane(Vector3 normal, float dist, float size)
        {
            Vector3 u, v;

            if (normal == Vector3.UnitZ)
            {
                u = Vector3.UnitX;
                v = Vector3.UnitY;
            }
            else if (normal == -Vector3.UnitZ)
            {
                u = Vector3.UnitY;
                v = Vector3.UnitX;
            }
            else
            {
                u = Vector3.Cross(normal, Vector3.UnitZ);
                v = Vector3.Cross(normal, u);
            }

            Vector3 p0 = normal * dist;
            Vector3 fu = u * size;
            Vector3 fv = v * size;

            Vector3 p1 = p0 - fu + fv;
            Vector3 p2 = p0 - fu - fv;
            Vector3 p3 = p0 + fu - fv;
            Vector3 p4 = p0 + fu + fv;

            CQuad newQuad = new CQuad();
            newQuad.verts[0] = new FatVertex()
            {
                Position = new Vector4(p1, 1.0f),
                Normal = new Vector4(normal, 0.0f)
            };
            newQuad.verts[1] = new FatVertex()
            {
                Position = new Vector4(p2, 1.0f),
                Normal = new Vector4(normal, 0.0f)
            };
            newQuad.verts[2] = new FatVertex()
            {
                Position = new Vector4(p3, 1.0f),
                Normal = new Vector4(normal, 0.0f)
            };
            newQuad.verts[3] = new FatVertex()
            {
                Position = new Vector4(p1, 1.0f),
                Normal = new Vector4(normal, 0.0f)
            };
            newQuad.verts[4] = new FatVertex()
            {
                Position = new Vector4(p3, 1.0f),
                Normal = new Vector4(normal, 0.0f)
            };
            newQuad.verts[5] = new FatVertex()
            {
                Position = new Vector4(p4, 1.0f),
                Normal = new Vector4(normal, 0.0f)
            };

            return newQuad;
        }

        public void Translate(Vector3 p)
        {
            for (int i = 0; i < verts.Length; ++i)
            {
                verts[i].Position += new Vector4(p, 0.0f);
            }
        }
    }
}
