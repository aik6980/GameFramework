using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.D3DCompiler;

using ZeroEngine.Engine;

using SlimDXDevice11 = SlimDX.Direct3D11.Device;

namespace ZeroEngine.GraphicDevice.Direct3d11
{

    class EffectExD3d11 : EffectEx
    {
        public SlimDXDevice11 m_D3dDevice;
        Effect m_Effect;

        class IncludeFx : Include
        {
            List<string> m_IncludeDirs;

            public IncludeFx(string filePath)
            {
                m_IncludeDirs = new List<string>();
                m_IncludeDirs.Add(new DirectoryInfo(filePath).Parent.FullName);
                m_IncludeDirs.Add(Global.FileSystem.GetResourceRootPath());
            }

            public void Close(Stream stream)
            {
                stream.Close();
            }

            public void Open(IncludeType type, string fileName, Stream parentStream, out Stream stream)
            {
                stream = null;
                foreach (string searchPath in m_IncludeDirs)
                {
                    string includePath = Path.Combine(searchPath, fileName);
                    if (File.Exists(includePath))
                    {
                        stream = new FileStream(includePath, FileMode.Open);
                        break;
                    }
                }

                Debug.Helper.Assert(stream != null, "Include file not found " + fileName);
            }
        }
        Include m_IncludeHandler;

        public EffectPass GetMainTechnique() { return m_Effect.GetTechniqueByIndex(0).GetPassByIndex(0); }

        public void CreateFromFile(string strFileName)
        {
            var fs = new FileStream(strFileName, FileMode.Open);
            var len = (int)fs.Length;

            var data = new byte[len];
            fs.Read(data, 0, len);

            // create the #include handler
            m_IncludeHandler = new IncludeFx(strFileName);
            CreateFromMemory(data);
        }

        public void CreateFromMemory(Byte[] rawData)
        {
            var shaderFlags = ShaderFlags.None;
#if DEBUG
            shaderFlags |= ShaderFlags.Debug;
#endif

            string errMessage;
            var byteCode = ShaderBytecode.Compile(rawData, "fx_5_0",
                shaderFlags, EffectFlags.None, null, m_IncludeHandler, out errMessage);

            if (errMessage != "")
            {
                Debug.Helper.Warning(false, "Shader Compile Error : " + errMessage);
            }

            m_Effect = new Effect(m_D3dDevice, byteCode);
            
            // get all params and store for later use
            GetEffectParams();
        }

        void GetEffectParams()
        {
            for (int i = 0; i < m_Effect.Description.GlobalVariableCount; ++i)
            {
                var effectVar = m_Effect.GetVariableByIndex(i);
                var name = effectVar.Description.Name;
                var type = effectVar.GetVariableType();

                if(effectVar.IsValid)
                    StoreEffectVariableByType(type);
            }
        }

        void StoreEffectVariableByType(EffectType type)
        {
            Debug.Helper.Log(type.Description.TypeName);
            switch(type.Description.Type)
            {
                case ShaderVariableType.Float:
                    {

                    }break;
            }
        }

        public override void Apply(RenderParamsListArray paramsListArry)
        {
            // apply effect parameters
            for (int i = 0; i < m_Effect.Description.GlobalVariableCount; ++i)
            {
                var effectVar = m_Effect.GetVariableByIndex(i);
                var name = effectVar.Description.Name;
                var type = effectVar.GetVariableType();

                switch (type.Description.TypeName)
                {
                    case "float":
                        {
                            float v;
                            if (paramsListArry.TryGetFloat(name, out v))    effectVar.AsScalar().Set(v);
                        } break;
                    case "float4":
                        {
                            Vector4 v;
                            if (paramsListArry.TryGetFloat4(name, out v))    effectVar.AsVector().Set(v);
                        } break;
                    case "float4x4":
                        {
                            Matrix v;
                            if (paramsListArry.TryGetFloat4x4(name, out v)) effectVar.AsMatrix().SetMatrix(v);
                        }break;
                }
            }

            // apply effect state
            GetMainTechnique().Apply(m_D3dDevice.ImmediateContext);
        }
    }
}
