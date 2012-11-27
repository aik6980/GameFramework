using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using SlimDX.Direct3D11;

namespace ZeroEngine.GraphicDevice
{
    class PrimitiveType
    {
        public enum Type
        {
            Undefined,
            TriangleList,
            TriangleStrip,
            LineList,
            LineStrip
        }

        public static int CalcNumPrimitive(Type type, int numVerts)
        {
            int numPrims = 0;
            switch (type)
            {
                case Type.TriangleList: numPrims = numVerts / 3; break;
                case Type.TriangleStrip: numPrims = numVerts - 2; break;
                case Type.LineList: numPrims = numVerts / 2; break;
                case Type.LineStrip: numPrims = numVerts - 1; break;
                default:
                    {
                        Debug.Helper.Assert(false, "Unsupported PrimitiveType");
                        return 0;
                    }
            }

            Debug.Helper.Assert(numPrims >= 0, "Neg Primitive number?");
            return numPrims;
        }

        public static int CalcNumIndices(Type type, int numPrims)
        {
            int numVerts = 0;
            switch (type)
            {
                case Type.TriangleList: numVerts = numPrims * 3; break;
                case Type.TriangleStrip: numVerts = numPrims + 2; break;
                case Type.LineList: numVerts = numPrims * 2; break;
                case Type.LineStrip: numVerts = numPrims + 1; break;
                default:
                    {
                        Debug.Helper.Assert(false, "Unsupported PrimitiveType");
                        return 0;
                    }
            }

            return numVerts;
        }
    }

    abstract class VertexLayout
    {

    }

    abstract class VertexBuffer
    {
        public enum Type
        {
            STATIC,
            DYNAMIC,
        }

        protected MemoryStream m_RawData;
        protected Type m_Usage;

        public abstract uint GetVertexCount(VertexLayout layout);

        public VertexBuffer()
        {
            m_RawData = null;
            m_Usage = Type.DYNAMIC;
        }
    }
}
