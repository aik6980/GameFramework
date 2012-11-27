using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

namespace ZeroEngine.Filesystem
{
    class FileSystem
    {
        string m_ExecutePath;
        string m_ResourcePath;

        public void Initialize()
        {
            GetExecPath();
            GetResourceRootPath();
        }

        public string GetExecPath()
        {
            if (m_ExecutePath == null)
            {
                // our root path is the Application's working Directory
                m_ExecutePath = Directory.GetCurrentDirectory();

                Debug.Helper.Log(string.Format("Application Exec Path: {0}", m_ExecutePath));
            }

            return m_ExecutePath;
        }

        public string GetResourceRootPath()
        {
            if (m_ResourcePath == null)
            {
                SearchResourcePath();

                Debug.Helper.Log(string.Format("Application Resources Path: {0}", m_ResourcePath));
            }

            return m_ResourcePath;
        }

        public string GetResourcePath(string resourceName)
        {
            string path = Path.Combine(GetResourceRootPath(), resourceName);
            if (File.Exists(path))
            {
                return path;
            }
            else
            {
                Debug.Helper.Warning(false, "Resource Not Found");
                return null;
            }
        }

        void SearchResourcePath()
        {
            string resourceDirName = "Resources";

            // search for the resource path
            string currSearchDir = GetExecPath();
            for (int searchLevel = 0; searchLevel < 6; ++searchLevel)
            {
                string resourcesFullPath = Path.Combine(currSearchDir, resourceDirName);
                if (Directory.Exists(resourcesFullPath))
                {
                    m_ResourcePath = resourcesFullPath;
                    break;
                }
                
                // search upper level
                currSearchDir = Directory.GetParent(currSearchDir).FullName;
            }
        }
    }
}
