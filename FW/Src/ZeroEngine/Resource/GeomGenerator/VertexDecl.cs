using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlimDX;
using SlimDX.DXGI;
using SlimDX.Direct3D11;
using ZeroEngine.GraphicDevice.Direct3d11;

namespace ZeroEngine.Resource.GeomGenerator
{
    struct FatVertex
    {
        public Vector4 Position;
        public Vector4 Normal;
        public Vector4 TexCoord0;
        public Vector4 TexCoord1;
    };

    class FatVertexDesc
    {
        public int SizeInBytes = 4 * Vector4.SizeInBytes;
        public VertexLayoutD3d11 Decl;

        public FatVertexDesc()
        {
            Decl = new VertexLayoutD3d11(new[]
            {
                new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                new InputElement("NORMAL", 0, Format.R32G32B32A32_Float, Vector4.SizeInBytes, 0),
                new InputElement("TEXCOORD", 0, Format.R32G32B32A32_Float, Vector4.SizeInBytes * 2, 0),
                new InputElement("TEXCOORD", 1, Format.R32G32B32A32_Float, Vector4.SizeInBytes * 3, 0),
            });
        }
    }
}
