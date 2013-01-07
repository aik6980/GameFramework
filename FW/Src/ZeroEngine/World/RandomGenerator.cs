using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlimDX;

namespace ZeroEngine.World
{
    class CRandomGenerator
    {
        Random  m_rand = new Random();

        public float GetFloat()
        {
            return (float)m_rand.NextDouble();
        }

        // [ -1 .. 1 ]
        public float GetFloatNorm()
        {
            return (GetFloat() * 2.0f) - 1.0f;
        }

        public float GetFloatRange(float min, float max)
        {
            return (GetFloat() * (max - min)) + min;
        }

        public Vector3 GetFloat3Norm()
        {
            var v = new Vector3(GetFloatNorm(), GetFloatNorm(), GetFloatNorm());
            v.Normalize();
            return v;
        }
    }
}
