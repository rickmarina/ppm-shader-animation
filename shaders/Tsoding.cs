using System.Buffers;
using System.Numerics;
using System.Text;

public class TSoding : ShaderBase
{
    public override void GenerateFrame(int frameId)
    {
        Console.WriteLine($"Generating frame {frameId + 1}/{TOTAL_FRAMES}");

        Vector2 r = new(w, h);
        float t = ((float)frameId / TOTAL_FRAMES) * 2 * MathF.PI;
        byte[] buffer = ArrayPool<byte>.Shared.Rent(w * h * 3);
        // byte[] buffer = new byte[w * h * 3];
        int idx = 0;

        try
        {
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Vector4 o = Vector4.Zero;
                    Vector2 FC = new(x, y);

                    //////////////////////////////
                    // https://x.com/XorDev/status/1894123951401378051
                    // Vector2 p=(FC*2.-r)/r.y,l,i,v=p*(l+=4.-4.*abs(.7-dot(p,p)));
                    Vector2 p = (FC * 2f - r) / r.Y;
                    Vector2 l = Vector2.Zero;
                    Vector2 i_vec = Vector2.Zero;
                    float dotPP = Vector2.Dot(p, p);
                    l = Vector2.Add(l, new Vector2(4f - 4f * MathF.Abs(0.7f - dotPP)));
                    Vector2 v = p * l;

                    // for(;i.y++<8.;o+=(sin(v.xyyx())+1.)*abs(v.x-v.y))v+=cos(v.yx()*i.y+i+t)/i.y+.7;
                    while (i_vec.Y++ < 8f)
                    {
                        // v.xyyx() = (v.x, v.y, v.y, v.x)
                        Vector4 vXYYX = new(v.X, v.Y, v.Y, v.X);
                        Vector4 sinV = new(MathF.Sin(vXYYX.X), MathF.Sin(vXYYX.Y), MathF.Sin(vXYYX.Z), MathF.Sin(vXYYX.W));
                        o += (sinV + Vector4.One) * MathF.Abs(v.X - v.Y);

                        // v.yx() = (v.y, v.x)
                        Vector2 vYX = new(v.Y, v.X);
                        Vector2 cosV = new(MathF.Cos(vYX.X * i_vec.Y + i_vec.X + t), MathF.Cos(vYX.Y * i_vec.Y + i_vec.Y + t));
                        v += cosV / i_vec.Y + new Vector2(0.7f, 0.7f);
                    }

                    // o=tanh(5.*exp(l.x-4.-p.y*vec4(-1,1,2,0))/o);
                    Vector4 pYVec = new Vector4(-p.Y, p.Y, 2f * p.Y, 0);
                    Vector4 expVec = new Vector4(
                        MathF.Exp(l.X - 4f - pYVec.X),
                        MathF.Exp(l.X - 4f - pYVec.Y),
                        MathF.Exp(l.X - 4f - pYVec.Z),
                        MathF.Exp(l.X - 4f - pYVec.W)
                    );
                    o = new Vector4(
                        MathF.Tanh(5f * expVec.X / o.X),
                        MathF.Tanh(5f * expVec.Y / o.Y),
                        MathF.Tanh(5f * expVec.Z / o.Z),
                        MathF.Tanh(5f * expVec.W / o.W)
                    );

                    // Convert to bytes RGB
                    buffer[idx++] = (byte)(Math.Clamp(o.X, 0f, 1f) * 255); // R
                    buffer[idx++] = (byte)(Math.Clamp(o.Y, 0f, 1f) * 255); // G
                    buffer[idx++] = (byte)(Math.Clamp(o.Z, 0f, 1f) * 255); // B
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

