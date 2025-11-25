using System.Buffers;
using System.Numerics;
using System.Text;

public class RayMarchingSpheres : ShaderBase
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
                    Vector2 p = (FC * 1.5f - r) / r.Y;
                    float dist = Vector2.Distance(p, new Vector2(MathF.Sin(t), MathF.Cos(t))) - 0.5f;
                    float shade = MathF.Exp(-15 * dist * dist);
                    buffer[idx++] = (byte)(shade * 255);
                    buffer[idx++] = (byte)(shade * 128);
                    buffer[idx++] = (byte)((1 - shade) * 255);
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