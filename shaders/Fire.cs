using System.Buffers;
using System.Numerics;
using System.Text;

public class Fire : ShaderBase
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
                    float n = MathF.Sin(10 * p.X + t * 1.8f) * 0.25f + MathF.Sin(25 * p.X - t * 0.7f) * 0.10f;

                    float v = 1.2f - (p.Y + 0.4f + n);

                    float fire = Math.Clamp(MathF.Pow(v, 4), 0, 1);

                    float flicker = MathF.Sin(t * 6 + p.X * 8) * 0.15f;
                    fire = Math.Clamp(fire + flicker, 0f, 1f);

                    float rCol = MathF.Min(1f, 2.2f * fire);
                    float gCol = MathF.Min(1f, 1.5f * fire * (1f - 0.4f * fire));
                    float bCol = 0.14f * fire * (1 - fire);

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