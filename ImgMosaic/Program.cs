using ImgMosaic.Models;
using OpenCvSharp;
using System.Diagnostics;
using static ImgMosaic.Models.ImgMosaicGenerator;

Stopwatch stopwatch = new Stopwatch();
stopwatch.Start();

List<string> inputPath = new() {
    Path.Combine("D:\\OneDrive\\Afbeeldingen\\2025\\US Exchange"),
    //Path.Combine("D:\\OneDrive\\Afbeeldingen\\2025\\US Exchange\\11 Florida"),
};

List<string> targetPath = new() {
    Path.Combine(Directory.GetCurrentDirectory(), "Src", "target")
};

string outputPath = Path.Combine(Directory.GetCurrentDirectory(), "Src", "output");

ImgMosaicGenerator mosaic = new();

List<Image> inputImages = mosaic.PreLoadImages(InputTypes.Input, inputPath);
Image targetImage = mosaic.PreLoadImages(InputTypes.Target, targetPath)[0];

// Resizes the target -> determins amount of tiles which are needed -> resolution
Mat upscaledTarget = new Mat();
int multiplier = 4;
Cv2.Resize(targetImage.MatchRes, upscaledTarget,
    new Size(targetImage.Cols * multiplier, targetImage.Rows * multiplier),
    interpolation: InterpolationFlags.Lanczos4);

// Makes the final image
Mat finalImage = mosaic.ConstructFinalImage(inputImages, upscaledTarget);
string fileName = $"mosaic_{(int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds}.png";
SaveImage(outputPath, fileName, finalImage);

// Makes the OpenSeaDragon mosaic
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