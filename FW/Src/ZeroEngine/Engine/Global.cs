using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ZeroEngine.GraphicDevice;

namespace ZeroEngine.Engine
{
    struct GlobalInit
    {
        public System.Windows.Forms.Form hWindow;
    }

    class Global
    {
        // frame time
        public static readonly float FIXEDFPS = 60.0f;
        public static readonly float FIXEDFRAMETIME = 1 / FIXEDFPS; 

        // high level subsystem
        static World.CWorld m_World = null;
        internal static World.CWorld World
        {
            get { return Global.m_World; }
        }

        static Scene.Scene m_Scene = null;

        // low level subsystem
        static Filesystem.FileSystem m_FileSystem = null;
        internal static Filesystem.FileSystem FileSystem
        {
            get { return m_FileSystem; }
        }

        static GraphicDevice.Device3d m_Device3d = null;
        internal static GraphicDevice.Direct3d11.Device3dD3d11 Device3d
        {
            get { return (GraphicDevice.Direct3d11.Device3dD3d11)Global.m_Device3d; }
        }

        static Input.CInputManager m_InputManager = null;
        internal static Input.CInputManager InputManager
        {
            get { return Global.m_InputManager; }
        }

        public static void Initialize(GlobalInit initStruct)
        {
            // subsystem initialization
            
            // initialize low level system
            // File System
            m_FileSystem = new Filesystem.FileSystem();
            m_FileSystem.Initialize();

            // graphic
            m_Device3d = new GraphicDevice.Direct3d11.Device3dD3d11();
            Device3dInit dev3dInit = new Device3dInit();
            dev3dInit.hWindow = initStruct.hWindow;
            dev3dInit.iScreenWidth = initStruct.hWindow.ClientSize.Width;
            dev3dInit.iScreenHeight = initStruct.hWindow.ClientSize.Height;
            m_Device3d.Initialize(dev3dInit);

            // input
            m_InputManager = new Input.CInputManager();
            m_InputManager.Initialize(initStruct.hWindow);

            // initialize high level
            // world
            m_World = new World.CWorld();

            // scene
            m_Scene = new Scene.Scene();
            m_Scene.Load();
            m_Scene.CurrWorld = m_World;
        }

        public static void Update()
        {
            // pre-render
            m_InputManager.Update();
            m_Scene.Update();

            // draw
            m_Scene.Render();
        }

        public static void Destroy()
        {

        }
    }
}
