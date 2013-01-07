using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlimDX;
using ZeroEngine.Engine;
using ZeroEngine.World;

using SlimDX.DXGI;
using SlimDX.Direct3D11;
using ZeroEngine.GraphicDevice;
using ZeroEngine.GraphicDevice.Direct3d11;

namespace ZeroEngine.GraphicRenderer.ParticleSystem
{
    class Particle
    {
        public struct ParticleVSIn
        {
            public Vector4  pos;
            public float    size;

            static public int SizeInBytes()
            {
                return Vector4.SizeInBytes + sizeof(float);
            }

            static VertexLayoutD3d11 Decl = null;
            static public VertexLayoutD3d11 GetVertexDesc()
            {
                if (Decl == null)
                {
                    Decl = new VertexLayoutD3d11(new[]
                    {
                        new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                        new InputElement("SIZE", 0, Format.R32_Float, Vector4.SizeInBytes, 0),
                    });
                }

                return Decl;
            }
        };

        public Vector3  pos  = Vector3.Zero;
        public Vector3  vel  = Vector3.Zero;
        public float    size     = 0.05f;

        public Vector3 angularVel = Vector3.Zero;

        // data per particles
        public float    lifeTime    = 0.0f;
        public float    createTime  = 0.0f;

        public bool IsAlive(float currTime)
        {
            return (currTime - createTime) < lifeTime;
        }
    }

    class Emitter
    {
        public Vector3 m_Pos = Vector3.Zero;

        // config
        int     m_MaxParticles          = 100;
        float   m_ParticlesEmitRate     = 25;   // numParticles/s
        Vector2 m_VelRange              = new Vector2(0.005f, 0.01f);
        Vector2 m_LifeTimeRange         = new Vector2(0.5f, 1.0f);
        // rotation movement
        Vector2 m_AngularVelRange       = new Vector2(0.01f, 0.02f); 

        // particles
        Particle[]      m_Particles = null;
        List<Particle>  m_Alive     = null;
        List<Particle>  m_Dead      = null;

        // internal timer
        float           m_EmitterTime       = 0.0f;
        float           m_EmitParticleTimer = 0.0f;

        public void Initialize()
        {
            m_Particles = new Particle[m_MaxParticles];
            for (int i=0; i < m_MaxParticles; ++i)
            {
                m_Particles[i] = new Particle();
            }

            m_Alive = new List<Particle>();
            m_Dead = new List<Particle>();

            // add all particles to the dead list
            m_Dead.AddRange(m_Particles);
        }

        public void Update(float dt)
        {
            m_EmitterTime += dt/1000.0f;

            for (int i = 0; i < m_Alive.Count; ++i)
            {
                if (m_Alive[i].IsAlive(m_EmitterTime))
                {
                    var p = m_Alive[i];
                    // simple simulation here
                    //Vector3 radiusVec = p.pos - m_Pos;
                    Vector3 radiusVec = (m_Pos - p.pos) - Vector3.Dot((m_Pos - p.pos), CWorld.WORLD_UP) * CWorld.WORLD_UP;
                    Vector3 velRotational = Vector3.Cross(p.angularVel, radiusVec);

                    Vector3 velFinal = p.vel + velRotational;
                    p.pos += velFinal * dt;
                }
                else
                {
                    m_Dead.Add(m_Alive[i]);
                    m_Alive.Remove(m_Alive[i]);
                }
            }

            // add new particle
            float timePerParticle = 1.0f / m_ParticlesEmitRate;
            m_EmitParticleTimer += dt;
            while (m_EmitParticleTimer >= timePerParticle)
            {
                AddParticle();
                m_EmitParticleTimer -= timePerParticle;
            }
        }

        void AddParticle()
        {
            // don't add if we don't have any available particles
            if (m_Dead.Count == 0)
                return;

            var newParticle = m_Dead[0];

            var bias = CWorld.RAND.GetFloatRange(0.0f, 2.0f);
            newParticle.pos = m_Pos + CWorld.WORLD_UP * bias;
            newParticle.vel = CWorld.RAND.GetFloat3Norm() * CWorld.RAND.GetFloatRange(m_VelRange.X, m_VelRange.Y) * bias;
            newParticle.vel.Y = Math.Abs(newParticle.vel.Y);
            newParticle.lifeTime    = CWorld.RAND.GetFloatRange(m_LifeTimeRange.X, m_LifeTimeRange.Y);
            newParticle.angularVel = CWorld.WORLD_UP * CWorld.RAND.GetFloatRange(m_AngularVelRange.X, m_AngularVelRange.Y);

            newParticle.createTime = m_EmitterTime;

            m_Dead.RemoveAt(0);
            m_Alive.Add(newParticle);
        }

        public int GetNumAliveParticles()
        {
            return m_Alive.Count;
        }

        // rendering
        public DataStream          m_VB;
        public VertexBufferBinding m_VBBinding;

        public void BuildVB()
        {
            if (m_VB == null)
            {
                m_VB = new DataStream(Particle.ParticleVSIn.SizeInBytes() * m_MaxParticles, false, true);
                var vb = Engine.Global.Device3d.CreateBuffer(new Device3dD3d11.SCreateBufferCmd()
                {
                    Name = "Particle",
                    SizeInBytes = (int)Particle.ParticleVSIn.SizeInBytes() * m_MaxParticles,
                    BufferBindFlags = BindFlags.VertexBuffer
                });
                m_VBBinding = new VertexBufferBinding(vb, Particle.ParticleVSIn.SizeInBytes(), 0);
            }

            // fill vb with particle data
            Particle.ParticleVSIn[] particle_vsstream = new Particle.ParticleVSIn[m_Alive.Count];
            for (int i = 0; i < m_Alive.Count; ++i)
            {
                particle_vsstream[i].pos = new Vector4(m_Alive[i].pos, 1.0f);
                particle_vsstream[i].size = m_Alive[i].size;
            }

            m_VB = new DataStream(particle_vsstream, false, true);

            // update gpu buffer
            Engine.Global.Device3d.UpdateBufferData("Particle", m_VB);
        }
    }

    class CParticleSystemRenderer : Renderer
    {
        Emitter[] m_Emitters = null;

        public void Load()
        {
            // load shaders
            Engine.Global.Device3d.CreateEffect(new Device3dD3d11.CreateEffectCmd()
            {
                Name = "ParticleFX",
                SrcFileName = Engine.Global.FileSystem.GetResourcePath("Engine/Shaders/ParticleFX.fx")
            });

            m_Emitters = new Emitter[1] { new Emitter() };
            foreach (Emitter it in m_Emitters)
            {
                it.Initialize();
            }
        }

        public void PreRender()
        {
            float dt = Global.AppTimer.GetIntervalMS();
            foreach (Emitter it in m_Emitters)
            {
                it.Update(dt);
                it.BuildVB();
            }
        }

        public void Render(RenderParamsListArray paramsListArray)
        {
            foreach(Emitter it in m_Emitters)
            {
                var device = Engine.Global.Device3d.GetDevice();
                // try to get the matched input-layout
                var layout = Engine.Global.Device3d.GetInputLayout("ParticleFX", Particle.ParticleVSIn.GetVertexDesc());
                device.ImmediateContext.InputAssembler.InputLayout = layout;
                device.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.PointList;
                device.ImmediateContext.InputAssembler.SetVertexBuffers(0, it.m_VBBinding);

                // setup effect
                float scale = 5.0f;
                Matrix objToWorld = Matrix.Transformation(new Vector3(scale),
                    Quaternion.Identity, new Vector3(1.0f, 1.0f, 1.0f), Vector3.Zero,
                    Quaternion.Identity, new Vector3(0.0f, 0.0f, 5.0f));

                var paramslist = new RenderParamsList();
                paramslist.Set("WorldXf", objToWorld);

                paramsListArray.Push(paramslist);

                var fx = Engine.Global.Device3d.GetEffect("ParticleFX");
                fx.Apply(paramsListArray);

                // draw
                device.ImmediateContext.Draw(it.GetNumAliveParticles(), 0);
                Engine.Global.Device3d.SetDefaultstate();

                paramsListArray.Pop();
            }
        }
    }
}
