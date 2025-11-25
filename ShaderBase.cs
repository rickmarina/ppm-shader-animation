using System.Buffers;

public abstract class ShaderBase : IShader
{
    protected int TOTAL_FRAMES = 240;
    protected int w = 16 * 60;
    protected int h = 9 * 60;

    public void CreateShader()
    {
        int maxParallel = Math.Max(1, Environment.ProcessorCount);
        var pOptions = new ParallelOptions { MaxDegreeOfParallelism = maxParallel };
        Enumerable.Range(0, TOTAL_FRAMES).AsParallel().WithDegreeOfParallelism(maxParallel).ForAll(frameId => GenerateFrame(frameId));
    }

    public abstract void GenerateFrame(int frameId);

    public static float SmoothStep(float edge0, float edge1, float x)
    {
        float t = Math.Clamp((x - edge0) / (edge1 - edge0), 0f, 1f);
        return t * t * (3f - 2f * t);
    }
    public static float Frac(float x) => x - MathF.Floor(x);
    public static float Mod(float x, float y) => x - y * MathF.Floor(x / y);
}