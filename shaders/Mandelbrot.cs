using System.Buffers;
using System.Numerics;
using System.Text;

public class Mandelbrot : ShaderBase
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
                    Vector2 c = p * 1.5f;
                    Vector2 z = c;
                    int iter = 0;
                    for (; iter < 32; iter++)
                    {
                        float x1 = z.X * z.X - z.Y * z.Y + c.X;
                        float y1 = 2 * z.X * z.Y + c.Y;
                        if ((x1 * x1 + y1 * y1) > 4.0f) break;
                        z = new Vector2(x1, y1);
                    }
                    float v = (float)iter / 32f;
                    buffer[idx++] = (byte)(v * 255);   
                    buffer[idx++] = (byte)((1 - v) * 255);
                    buffer[idx++] = (byte)(MathF.Sin(v * 6.28f + t) * 127 + 128);
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