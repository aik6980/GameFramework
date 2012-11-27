using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlimDX;

namespace ZeroEngine.GraphicRenderer.Rendercraft
{
    class CVolumeDataGenerator_FloatingRock
    {
        public void Generate(CVolumeBuffer outVolume)
        {
            int size = outVolume.Size;

            for (int z = 1; z < size - 1; ++z)
            {
                for (int y = 1; y < size - 1; ++y)
                {
                    for (int x = 1; x < size - 1; ++x)
                    {
                        float xf = (float)x / size;
                        float yf = (float)y / size;
                        float zf = (float)z / size;


                    }
                }
            }
        }
    }

    class CVolumeDataGenerator_Cube
    {
        public void Generate(CVolumeBuffer outVolume)
        {
            int size = outVolume.Size;

            for (int z = 1; z < size - 1; ++z)
            {
                for (int y = 1; y < size - 1; ++y)
                {
                    for (int x = 1; x < size - 1; ++x)
                    {
                        Byte value = 1;
                        outVolume.SetData(value, new Vector3(x, y, z));
                    }
                }
            }

            //outVolume.SetData(1, new Vector3(size/2, size/2, size/2));
        }
    }

    class CVolumeDataGenerator_Rnd
    {
        public void Generate(CVolumeBuffer outVolume)
        {
            Random rnd = new Random();
            int size = outVolume.Size;

            for (int z = 1; z < size - 1; ++z)
            {
                for (int y = 1; y < size - 1; ++y)
                {
                    for (int x = 1; x < size - 1; ++x)
                    {
                        Byte value = 0;
                        if (rnd.NextDouble() > 0.5f)
                        {
                            value = 1;
                        }
                        outVolume.SetData(value, new Vector3(x, y, z));
                    }
                }
            }
        }
    }
}
