using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

using ZeroEngine.Engine;
using ZeroEngine.GraphicDevice;

using ZeroEngine.Scene.Camera;

using ZeroEngine.GraphicRenderer;
using ZeroEngine.GraphicRenderer.Rendercraft;
using ZeroEngine.GraphicRenderer.ParticleSystem;

namespace ZeroEngine.Scene
{
    class CPostprocessor
    {

        public bool m_bEnable = false;
        public bool Enable
        {
            get { return m_bEnable; }
            set { m_bEnable = value; }
        }
    }

    class Scene
    {
        World.CWorld     m_currWorld;
        public World.CWorld CurrWorld
        {
            get { return m_currWorld; }
            set { m_currWorld = value; }
        }
        
        CCamera             m_currCamera = new CFreeCamKB();
        CPostprocessor      m_currPostprocessChain;

        // Renderer type
        Dictionary<string, Renderer> m_RendererList;

        // test renderer
        BasicRenderer           m_BasicRenderer = new BasicRenderer();
        CVolumeRenderer         m_TerrainRenderer = new CVolumeRenderer();
        CParticleSystemRenderer m_ParticleRenderer = new CParticleSystemRenderer();

        // scene parameters
        RenderParamsListArray m_GlobalParams = new RenderParamsListArray();

        public void Load()
        {
            m_BasicRenderer.Load();
            m_currPostprocessChain = new CPostprocessor();


            CVolumeBuffer volData = new CVolumeBuffer();
            volData.Initialize(32);

            // generator
            var dataGenerator = new CVolumeDataGenerator_Rnd();
            dataGenerator.Generate(volData);

            // terrain graphic
            CVolumeRenderer dataRenderer = new CVolumeRenderer();
            dataRenderer.Load();
            dataRenderer.Generate(volData);
            m_TerrainRenderer = dataRenderer;

            m_ParticleRenderer.Load();
        }

        public void Update()
        {
            m_currCamera.Update();

            m_BasicRenderer.PreRender();
            m_ParticleRenderer.PreRender();
        }

        public void Render()
        {
            // set the scene variable
            var paramslist = new RenderParamsList();
            paramslist.Set("ViewXf", m_currCamera.WorldToCam());
            paramslist.Set("ViewIXf", m_currCamera.CamToWorld());
            paramslist.Set("ProjectionXf", m_currCamera.CamToProj());

            m_GlobalParams.Push(paramslist);
            
            // set RenderTarget and ViewPort
            if (m_currPostprocessChain.Enable)
            {
                Global.Device3d.SetRenderTarget("MainGameRT");
            }
            else
            {
                Global.Device3d.SetRenderTarget("BackBuffer");
                Global.Device3d.SetDepthStencilTarget("BackBuffer");
            }

            Global.Device3d.BeginFrame();

            // clear the current back buffer
            Global.Device3d.ClearColor(Color.Turquoise);
            Global.Device3d.ClearDepthStencil(1.0f, 0);

            m_TerrainRenderer.Render(m_GlobalParams);
            m_BasicRenderer.Render(m_GlobalParams);
            m_ParticleRenderer.Render(m_GlobalParams);

            // draw the world
            foreach (World.Entity ent in m_currWorld.EntityList)
            {
                // draw code

            }

            Global.Device3d.EndFrame();

            m_GlobalParams.Pop();
        }
    }
}
