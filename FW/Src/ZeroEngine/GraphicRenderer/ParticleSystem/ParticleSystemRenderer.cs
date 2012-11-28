using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlimDX;

namespace ZeroEngine.GraphicRenderer.ParticleSystem
{
    class Particle
    {
        public Vector3 pos;
        public Vector3 vel;
        public Vector3 age;

        // initial condition
        public Vector3 initial_pos;
        public Vector3 initial_vel;
    }

    class Emitter
    {
        Particle[] m_Particles = null;
        public Particle[] Particles
        {
            get { return m_Particles; }
        }
    }

    class ParticleSimulator
    {
        Emitter[] m_Emitters = null;

        public void Update(float dt)
        {
            foreach (Emitter emitter in m_Emitters)
            {
                for (int i = 0; i < emitter.Particles.Length; ++i)
                {
                    var p = emitter.Particles[i];
                    // simple simulation here
                    p.pos = p.vel * dt;
                }
            }
        }
    }

    class ParticleSystemRenderer : Renderer
    {
        Emitter[] m_Emitters = null;

        ParticleSimulator m_Simulator;

        public void PreRender()
        {

        }
    }
}
