using System.Buffers;
using System.Numerics;
using System.Text;

public class Water : ShaderBase
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
                    float wave = MathF.Sin((p.X + t) * 6f + MathF.Sin((p.Y + t * 0.6f) * 2f))
                               + MathF.Sin((p.Y + t * 0.5f) * 7f);

                    float shade = 0.5f + 0.5f * MathF.Sin(wave + t);

                    float rCol = 0.2f + 0.15f * shade;
                    float gCol = 0.4f + 0.25f * shade;
                    float bCol = 0.8f + 0.18f * shade;

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