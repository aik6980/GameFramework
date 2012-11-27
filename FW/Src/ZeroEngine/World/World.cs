using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlimDX;

namespace ZeroEngine.World
{
    class CWorld
    {
        // world const
        public static Vector3 WORLD_UP = Vector3.UnitY;

        List<Entity> m_EntityList = new List<Entity>();

        internal List<Entity> EntityList
        {
            get { return m_EntityList; }
            //set { m_EntityList = value; }
        }
    }
}
