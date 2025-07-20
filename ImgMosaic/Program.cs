using ImgMosaic.Models;
using OpenCvSharp;
using static ImgMosaic.Models.ImgMosaicGenerator;


string inputPath = Path.Combine(Directory.GetCurrentDirectory(), "Src", "input");
string targetPath = Path.Combine(Directory.GetCurrentDirectory(), "Src", "target");
string outputPath = Path.Combine(Directory.GetCurrentDirectory(), "Src", "output");

ImgMosaicGenerator mosaic = new ImgMosaicGenerator(5, 750);

List<Image> inputImages = mosaic.LoadImages(InputTypes.Input, inputPath);
Image targetImage = mosaic.LoadImages(InputTypes.Target, targetPath)[0];

//foreach (var image in inputImages) {
//    Console.WriteLine($"Input Image: {image.FileName}, Size: {image.SizeInBytes} bytes, Resolution: {image.Cols}x{image.Rows}");
//    if (image.DownSampledMatrix != null) {
//        Console.WriteLine($"Downsampled Matrix: {image.DownSampledMatrix.Length} pixels");

//        PrintMatrix(image.DownSampledMatrix);
//        Console.WriteLine();
//    }
//}

//PrintMatrix(targetImage.DownSampledMatrix);

Mat finalImage = mosaic.ConstructFinalImage(inputImages, targetImage.FullMatrix);
SaveImage(outputPath, "mosaic.png", finalImage);

