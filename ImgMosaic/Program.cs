using ImgMosaic.Models;
using OpenCvSharp;
using System.Diagnostics;
using static ImgMosaic.Models.ImgMosaicGenerator;

Stopwatch stopwatch = new Stopwatch();
stopwatch.Start();

List<string> inputPath = new() {
    Path.Combine("D:\\OneDrive\\Afbeeldingen\\2025\\US Exchange\\2025 U.S Exchange - telefoon"),
    //Path.Combine("C:\\Users\\marti\\OneDrive\\Afbeeldingen\\2025\\US Exchange\\08-23 Rasmussen Woods"),
};

List<string> targetPath = new() {
    Path.Combine(Directory.GetCurrentDirectory(), "Src", "target")
};

string outputPath = Path.Combine(Directory.GetCurrentDirectory(), "Src", "output");

ImgMosaicGenerator mosaic = new();

List<Image> inputImages = mosaic.LoadImages(InputTypes.Input, inputPath);
Image targetImage = mosaic.LoadImages(InputTypes.Target, targetPath)[0];
Mat upscaledTarget = new Mat();

// Resizes the target -> determins amount of tiles which are needed -> resolution
Cv2.Resize(targetImage.MatchRes, upscaledTarget,
    new Size(targetImage.Cols * 3, targetImage.Rows * 3),
    interpolation: InterpolationFlags.Lanczos4);

Mat finalImage = mosaic.ConstructFinalImage(inputImages, upscaledTarget);
string fileName = $"mosaic_{(int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds}.png";
SaveImage(outputPath, fileName, finalImage);

string deepZoomDir = Path.Combine(outputPath, "mosaic_files");
DeepZoomGenerator.Generate(finalImage, deepZoomDir);

Dzi.WriteDzi(
    Path.Combine(outputPath, "mosaic.dzi"),
    finalImage.Width,
    finalImage.Height
);

stopwatch.Stop();
TimeSpan elapsedTime = stopwatch.Elapsed;

Console.WriteLine($"Ellapsed time: {elapsedTime.ToString("mm\\:ss\\.ff")}");