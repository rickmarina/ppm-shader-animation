using System.Buffers;
using System.Numerics;
using System.Text;

public class Rain : ShaderBase
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
                    float rainAmount = 120.0f; // cuántas gotas
                    float dropWidth = 0.03f;
                    float dropLength = 0.19f;
                    float intensity = 0.0f;

                    for (int i = 0; i < 48; i++)
                    {
                        float f = i * 31.714f;
                        // semilla random
                        float x1 = Frac(MathF.Sin(f) * 5178.123f + i * 0.03f + t * 0.25f) * 2f - 1f;
                        float y1 = Frac(MathF.Sin(f * 1.1f) * 313.555f + i * 0.02f + t * 1.0f) * 2f - 1f;

                        // Caída vertical rápida+offset
                        y1 += Mod(t * (0.9f + 0.23f * i), 2.0f) - 1.0f;

                        Vector2 drop = new Vector2(x1, y1);

                        // Dibuja vertical: ovalo largo
                        float dx = p.X - drop.X;
                        float dy = (p.Y - drop.Y) / dropLength;
                        float d = MathF.Sqrt(dx * dx + dy * dy);

                        intensity += MathF.Exp(-d * d / (dropWidth * dropWidth));
                    }

                    intensity = Math.Clamp(intensity, 0f, 1f);

                    float rCol = 0.23f + intensity * 0.6f;
                    float gCol = 0.28f + intensity * 0.75f;
                    float bCol = 0.34f + intensity * 0.9f;

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