using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;

namespace ZeroEngine.GraphicDevice.Direct3d11
{
    class VertexLayoutD3d11 : IEqualityComparer 
    {
        InputElement[] m_Elems = null;
        public InputElement[] Elems
        {
            get { return m_Elems; }
        }

        public VertexLayoutD3d11(InputElement[] elems)
        {
            m_Elems = elems;
        }

        public override bool Equals(object rhs)
        {
            var rhsObj = (VertexLayoutD3d11)rhs;
            for (int i = 0; i < m_Elems.Length; ++i)
            {
                if (!m_Elems[i].Equals(rhsObj.m_Elems[i]))
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            int hashVal = 0;
            foreach (InputElement e in m_Elems)
                hashVal += e.GetHashCode();

            return hashVal;
        }

        int IEqualityComparer.GetHashCode(object obj)
        {
            return obj.GetHashCode();
        }

        bool IEqualityComparer.Equals(object lhs, object rhs)
        {
            return lhs.Equals(rhs);
        }
    }
}
