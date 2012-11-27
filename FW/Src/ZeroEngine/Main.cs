using System;
using System.Collections.Generic;
using System.Text;

using SlimDX;
using SlimDX.Windows;

namespace ZeroEngine
{
    public class EngineEntry
    {
        [STAThread]
        static void Main()
        {
            // initialize main window
            var form = new RenderForm("SlimDX - ZeroEngine");

            // initialize Engine
            Engine.GlobalInit globalInit = new Engine.GlobalInit();
            globalInit.hWindow = form;
            Engine.Global.Initialize(globalInit);

            // messages loop
            MessagePump.Run(form, () =>
            {
                // game loop goes here
                Engine.Global.Update();
            });

            // cleanup
        }
    }
}
