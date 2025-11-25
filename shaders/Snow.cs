using System.Buffers;
using System.Numerics;
using System.Text;

public class Snow : ShaderBase
{
    public override void GenerateFrame(int frameId)
    {
        Console.WriteLine($"Generating frame {frameId + 1}/{TOTAL_FRAMES}");
        float t = ((float)frameId / TOTAL_FRAMES) * 2 * MathF.PI;
        byte[] buffer = ArrayPool<byte>.Shared.Rent(w * h * 3);
        Vector2 r = new(w, h);

        int idx = 0;
        try
        {
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Vector2 FC = new(x, y);
                    Vector2 p = (FC * 2f - r) / r.Y;
                    float snowAmount = 140.0f; 
                    float snowSize = 0.04f;    
                    float intensity = 0.0f;

                    for (int i = 0; i < (int)snowAmount; i++)
                    {
                        float f = i * 12.9898f;
                        float x1 = Frac(MathF.Sin(f) * 43758.5453f + i * 0.01f + t * 0.31f) * 2f - 1f;
                        float y1 = Frac(MathF.Sin(f * 1.3f) * 31337.1234f + i * 0.03f + t * 0.13f) * 2f - 1f;

                        y1 += Mod(t * (0.32f + 0.12f * i), 2.0f) - 1.0f;
                        x1 += MathF.Sin(t * 0.4f + i); 

                        Vector2 flake = new Vector2(x1, y1);
                        float d = Vector2.Distance(p, flake);

                        intensity += MathF.Exp(-d * d / (snowSize * snowSize));
                    }

                    intensity = Math.Clamp(intensity, 0f, 1f);

                    float rCol = 0.3f + intensity * 0.7f; 
                    float gCol = 0.3f + intensity * 0.7f;
                    float bCol = 0.38f + intensity * 0.6f;

                    // Escribir píxel
                    buffer[idx++] = (byte)(Math.Clamp(rCol, 0f, 1f) * 255);
                    buffer[idx++] = (byte)(Math.Clamp(gCol, 0f, 1f) * 255);
                    buffer[idx++] = (byte)(Math.Clamp(bCol, 0f, 1f) * 255);


                }
            }

            using FileStream fs = new($"images/output_{frameId}.ppm", FileMode.Create, FileAccess.Write, FileShare.None, 1 << 18);
            byte[] headerBytes = Encoding.ASCII.GetBytes($"P6\n{w} {h}\n255\n");
            fs.Write(headerBytes, 0, headerBytes.Length);
            fs.Write(buffer, 0, buffer.Length);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}