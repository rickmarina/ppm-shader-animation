using System.Buffers;
using System.Numerics;
using System.Text;

public class TunnelVortex : ShaderBase
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
                    float angle = MathF.Atan2(p.Y, p.X);
                    float radius = p.Length();

                    float speed = t * 1.5f;

                    float waves = MathF.Sin(10.0f * radius - speed + 5.0f * angle);

                    float tunnel = 0.5f + 0.5f * MathF.Sin(20.0f * angle + 12.0f * speed - 5.0f * radius);
                    float mask = MathF.Exp(-3.0f * radius); 

                    float rCol = tunnel * mask;
                    float gCol = (1.0f - tunnel) * mask;
                    float bCol = waves * mask;
                    buffer[idx++] = (byte)(Math.Clamp(rCol, 0f, 1f) * 255);
                    buffer[idx++] = (byte)(Math.Clamp(gCol, 0f, 1f) * 255);
                    buffer[idx++] = (byte)(Math.Clamp(Math.Abs(bCol), 0f, 1f) * 255);
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