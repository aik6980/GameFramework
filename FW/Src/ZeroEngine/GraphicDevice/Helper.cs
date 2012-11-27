using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlimDX;

namespace ZeroEngine.GraphicDevice
{
    class Helper
    {
    }

    class RenderParamsList
    {
        Dictionary<string, float> m_FloatList = new Dictionary<string, float>();
        Dictionary<string, Vector4> m_Float4List = new Dictionary<string, Vector4>();
        Dictionary<string, Matrix> m_Float4x4List = new Dictionary<string, Matrix>();

        public void Set(string key, float val) { m_FloatList[key] = val; }
        public void Set(string key, Vector4 val) { m_Float4List[key] = val; }
        public void Set(string key, Matrix val) { m_Float4x4List[key] = val; }

        public bool TryGetFloat(string name, out float v) { return m_FloatList.TryGetValue(name, out v); }
        public bool TryGetFloat4(string name, out Vector4 v) { return m_Float4List.TryGetValue(name, out v); }
        public bool TryGetFloat4x4(string name, out Matrix v) { return m_Float4x4List.TryGetValue(name, out v); }
    }

    class RenderParamsListArray
    {
        List<RenderParamsList> m_ParamsList = new List<RenderParamsList>();

        public void Push(RenderParamsList paramsList) { m_ParamsList.Add(paramsList); }
        public void Pop() { m_ParamsList.RemoveAt(m_ParamsList.Count - 1); }

        public bool TryGetFloat(string name, out float v) 
        {
            for (int i = m_ParamsList.Count - 1; i >= 0; --i )
            {
                if (m_ParamsList[i].TryGetFloat(name, out v))
                    return true;
            }

            v = 0.0f;
            return false;
        }

        public bool TryGetFloat4(string name, out Vector4 v)
        {
            for (int i = m_ParamsList.Count - 1; i >= 0; --i)
            {
                if (m_ParamsList[i].TryGetFloat4(name, out v))
                    return true;
            }

            v = Vector4.Zero;
            return false;
        }

        public bool TryGetFloat4x4(string name, out Matrix v)
        {
            for (int i = m_ParamsList.Count - 1; i >= 0; --i)
            {
                if (m_ParamsList[i].TryGetFloat4x4(name, out v))
                    return true;
            }

            v = Matrix.Identity;
            return false;
        }
    }

    abstract class EffectEx
    {
        public abstract void Apply(RenderParamsListArray paramsListArry);
    }
}
