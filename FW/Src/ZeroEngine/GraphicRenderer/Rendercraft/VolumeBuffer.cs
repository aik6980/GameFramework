using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlimDX;

namespace ZeroEngine.GraphicRenderer.Rendercraft
{
    class CMathHelper
    {
        public static float Clamp(float x, float a, float b)
        {
            return (x < a) ? a : ((x > b) ? b : x);
        }
    }

    class CVolumeBuffer
    {
        // raw volume data
        int m_size = 0;

        public int Size
        {
            get { return m_size; }
        }

        Byte[] m_data = null;

        public void Initialize(int size)
        {
            m_size = size;
            m_data = new Byte[sizeof(Byte) * m_size * m_size * m_size];
        }

        // accessors
        int GetIndex(Vector3 p)
        {
            return (int)(p.X + p.Y * m_size + p.Z * m_size * m_size);
        }

        bool ValidateIndex(Vector3 p)
        {
            if (p.X >= m_size || p.X < 0) return false;
            if (p.Y >= m_size || p.Y < 0) return false;
            if (p.Z >= m_size || p.Z < 0) return false;

            return true;
        }

        int GetIndex_BoundaryClamp(Vector3 p)
        {
            int x = (int)CMathHelper.Clamp(p.X, 0, m_size - 1);
            int y = (int)CMathHelper.Clamp(p.Y, 0, m_size - 1);
            int z = (int)CMathHelper.Clamp(p.Z, 0, m_size - 1);
            return (int)(x + y * m_size + z * m_size * m_size);
        }

        public Byte GetData(Vector3 p)
        {
            int idx = GetIndex(p);
            return m_data[idx];
        }

        public Byte GetData_BoundarySafe(Vector3 p)
        {
            if (ValidateIndex(p))
            {
                int idx = GetIndex(p);
                return m_data[idx];
            }
            else
            {
                return 0;
            }
        }

        public void SetData(Byte v, Vector3 p)
        {
            int idx = GetIndex(p);
            m_data[idx] = v;
        }
    }
}
