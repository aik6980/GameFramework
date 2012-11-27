using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlimDX;

namespace ZeroEngine.GraphicRenderer.ParticleSystem
{
    struct Particle
    {
        public Vector3 pos;
        public Vector3 vel;
        public Vector3 age;
    }

    class Emitter
    {
        Particle[] m_Particles = null;
    }

    class ParticleSimulator
    {
        public void Update(float dt)
        {

        }
    }

    class ParticleSystemRenderer : Renderer
    {
        Emitter[] m_Emitters = null;

        ParticleSimulator m_Simulator;
    }
}
