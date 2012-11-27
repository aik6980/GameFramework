using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ZeroEngine.GraphicDevice;

namespace ZeroEngine.GraphicRenderer
{
    abstract class Renderer
    {
        string m_Name;
        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        public virtual void Load() { }
        public virtual void PreRender() { }
        public virtual void Render(RenderParamsListArray paramsListArray) { }
        public virtual void PostRender() { }
        public virtual void Unload() { }
    }
}
