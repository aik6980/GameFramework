using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Xml;

namespace ZeroEngine.Resource.Collada
{
    class CColladaMeshLoader
    {
        public void LoadMeshFromFile(string strFileName)
        {
            // Load file
            Stream stream = File.OpenRead(strFileName);
            StreamReader streamReader = new StreamReader(stream);
            string strColladaXml = streamReader.ReadToEnd();
            XmlNode xml = CXmlHelper.LoadXmlFromText(strColladaXml);
        }

//         public Skeleton LoadSkeleton(XmlNode data)
//         {
//             // output
//             Skeleton skeleton = new Skeleton();
// 
//             // Load bones
//             LoadBones(skeleton, xml);
// 
//             streamReader.Close();
//             streamReader.Dispose();
//             stream.Close();
//             stream.Dispose();
// 
//             return skeleton;
//         }
    }
}
