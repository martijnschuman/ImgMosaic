using ImgMosaic.Models;
using OpenCvSharp;
using static ImgMosaic.Models.ImgMosaicGenerator;

string inputPath = Path.Combine(Directory.GetCurrentDirectory(), "Src", "input");
string targetPath = Path.Combine(Directory.GetCurrentDirectory(), "Src", "target");
string outputPath = Path.Combine(Directory.GetCurrentDirectory(), "Src", "output");

ImgMosaicGenerator mosaic = new(tileWidth: 75, tileHeight: 42);

List<Image> inputImages = mosaic.LoadImages(InputTypes.Input, inputPath);
Image targetImage = mosaic.LoadImages(InputTypes.Target, targetPath)[0];
Mat upscaledTarget = new Mat();
Cv2.Resize(targetImage.FullMatrix, upscaledTarget,
    new Size(targetImage.Cols * 3, targetImage.Rows * 3),
    interpolation: InterpolationFlags.Lanczos4);

Mat finalImage = mosaic.ConstructFinalImage(inputImages, upscaledTarget);
string fileName = $"mosaic_{(int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds}.png";
SaveImage(outputPath, fileName, finalImage);
