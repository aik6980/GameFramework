using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace ZeroEngine.GraphicDevice
{
    struct Device3dInit
    {
        public Form hWindow;
        public int iScreenWidth;
        public int iScreenHeight;
    }

    interface Device3d
    {
        // device initialization
        void Initialize(Device3dInit initStruct);
        void Destroy();

        // scene drawing
        void BeginFrame();
        void EndFrame();

        // render states
        void SetRenderTarget(string name, uint id = 0);
        void SetDepthStencilTarget(string name);

        // resource creation
    }
}
