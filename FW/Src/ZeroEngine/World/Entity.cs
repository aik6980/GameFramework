using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlimDX;
using ZeroEngine.GraphicRenderer;

namespace ZeroEngine.World
{
    // basic entity
    class Entity
    {
        // name
        string m_Name;
        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        // transform
        Vector3 m_Position;
        public Vector3 Position
        {
            get { return m_Position; }
            set { m_Position = value; }
        }
        Vector3 m_Scale;
        public SlimDX.Vector3 Scale
        {
            get { return m_Scale; }
            set { m_Scale = value; }
        }
        Quaternion m_Rotation;
        public SlimDX.Quaternion Rotation
        {
            get { return m_Rotation; }
            set { m_Rotation = value; }
        }
        
        // children
        List<Entity> m_Members;

        // components
        string m_Renderer;
        public string Renderer
        {
            get { return m_Renderer; }
            set { m_Renderer = value; }
        }
    }
}
