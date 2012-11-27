using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlimDX.Direct3D11;
using SlimDX;

namespace ZeroEngine.GraphicDevice.Direct3d11
{
    class VertexBufferD3d11 : VertexBuffer
    {
        public SlimDX.Direct3D11.Device m_D3dDevice;
        SlimDX.Direct3D11.Buffer m_DeviceBuffer = null;

        public SlimDX.Direct3D11.Buffer GetVertexBuffer()
        {
            // dynamically create buffer
            if (m_DeviceBuffer == null)
            {
                ResourceUsage resourceUsage = ResourceUsage.Default;
                CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None;
                
                if(m_Usage == Type.DYNAMIC )
                {
                    resourceUsage = ResourceUsage.Dynamic;
                    cpuAccessFlags = CpuAccessFlags.Write;
                }

                BufferDescription desc = new BufferDescription()
                {
                    BindFlags = BindFlags.VertexBuffer,
                    CpuAccessFlags = cpuAccessFlags,
                    OptionFlags = ResourceOptionFlags.None,
                    SizeInBytes = (int)m_RawData.Length,
                    Usage = resourceUsage
                };

                if (m_Usage == Type.DYNAMIC)
                {
                    m_DeviceBuffer = new SlimDX.Direct3D11.Buffer(m_D3dDevice, desc);
                }
                else
                {
                    DataStream dataStream = new DataStream(m_RawData.ToArray(), true, true);
                    m_DeviceBuffer = new SlimDX.Direct3D11.Buffer(m_D3dDevice, dataStream, desc);
                    dataStream.Dispose();
                }
            }

            return m_DeviceBuffer;
        }

        public override uint GetVertexCount(VertexLayout layout)
        {

            return 0;
        }
    }

    class IndexBufferD3d11
    {

    }
}
