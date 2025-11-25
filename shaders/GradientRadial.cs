using System.Buffers;
using System.Numerics;
using System.Text;

public class GradientRadial : ShaderBase
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
                    float d = Vector2.Distance(p, Vector2.Zero);
                    float grad = 0.5f + 0.5f * MathF.Sin(30 * d - t * 4);
                    buffer[idx++] = (byte)(grad * 255);
                    buffer[idx++] = (byte)(MathF.Abs(grad - 0.5f) * 510);
                    buffer[idx++] = (byte)((1 - grad) * 255);
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