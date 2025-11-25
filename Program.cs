//./ffmpeg.exe -i .\images\output_%d.ppm -r 60 output.mp4

using System.Diagnostics;

Stopwatch sw = new();
sw.Start();

new Metaballs().CreateShader();

sw.Stop();
Console.WriteLine($"Elapsed time: {sw.Elapsed}");

