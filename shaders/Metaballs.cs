using System.Buffers;
using System.Numerics;
using System.Text;

public class Metaballs : ShaderBase
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
                    const int NUM_BALLS = 5;
                    float meta = 0f;

                    for (int i = 0; i < NUM_BALLS; i++)
                    {
                        float a = t * (0.6f + 0.4f * i) + i * 1.6f;
                        float s = 0.3f + 0.08f * MathF.Sin(i * 2.1f + t * 0.9f); 
                        Vector2 center = new Vector2(MathF.Sin(a), MathF.Cos(a)) * 0.6f;

                        float dist = Vector2.Distance(p, center);
                        meta += s * s / (dist * dist + 0.02f); 
                    }

                    meta = MathF.Min(meta, 1.2f);

                    float edge = SmoothStep(0.6f, 1.0f, meta); 

                    float rCol = edge;
                    float gCol = 0.5f + 0.5f * MathF.Sin(t + meta * 6); 
                    float bCol = 1.0f - edge * 0.5f;

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