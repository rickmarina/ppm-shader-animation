using System.Buffers;
using System.Numerics;
using System.Text;

public class Warp : ShaderBase
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
                    float warpStrength = 0.25f;
                    Vector2 q = p + new Vector2(
                        MathF.Sin(3.0f * p.Y + t * 1.3f),
                        MathF.Cos(4.0f * p.X + t * 1.6f)
                    ) * warpStrength;

                    float v = MathF.Sin(q.X * q.X * 10.0f + q.Y * q.Y * 10.0f - t * 2.0f);

                    float rCol = 0.5f + 0.5f * MathF.Sin(q.X * 5 + t + v * 2.0f);
                    float gCol = 0.5f + 0.5f * MathF.Sin(q.Y * 5 + t * 1.3f + v * 2.2f);
                    float bCol = 0.5f + 0.5f * MathF.Sin(t + v * 3.0f);

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