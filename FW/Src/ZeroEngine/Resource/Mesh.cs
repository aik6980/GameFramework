using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using SlimDX;
using ZeroEngine.GraphicDevice;


namespace ZeroEngine.Resource
{
    class CMeshData
    {
        public DataStream VB;
        public DataStream IB;
        public int IndexCount;
    }

    class CMeshManager
    {
        Dictionary< string, CMeshData> m_MeshList = new Dictionary< string, CMeshData>();
    }
}
