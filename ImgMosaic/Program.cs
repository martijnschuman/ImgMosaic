using ImgMosaic.Models;


string inputPath = Path.Combine(Directory.GetCurrentDirectory(), "src", "input");
string targetPath = Path.Combine(Directory.GetCurrentDirectory(), "src", "target");
string outputPath = Path.Combine(Directory.GetCurrentDirectory(), "src", "output");

ImgMosaicGenerator mosaic = new ImgMosaicGenerator();

List<Image> inputImages = mosaic.loadImages(inputPath);
Image targetImage = mosaic.loadImages(targetPath)[0];

foreach (var image in inputImages) {
    Console.WriteLine($"Input Image: {image.FileName}, Size: {image.SizeInBytes} bytes, Resolution: {image.Cols}x{image.Rows}");
    if (image.DownSampledMatrix != null) {
        Console.WriteLine($"Downsampled Matrix: {image.DownSampledMatrix.Length} pixels");

        mosaic.printMatrix(image.DownSampledMatrix);
    }
}

