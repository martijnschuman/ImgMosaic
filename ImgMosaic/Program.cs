using ImgMosaic.Models;


string inputPath = Path.Combine(Directory.GetCurrentDirectory(), "src", "input");
string outputPath = Path.Combine(Directory.GetCurrentDirectory(), "src", "output");
string targetPath = Path.Combine(Directory.GetCurrentDirectory(), "src", "target");

ImgMosaicGenerator mosaic = new ImgMosaicGenerator();

List<Image> inputImages = mosaic.loadImages(inputPath);
List<Image> outputImages = mosaic.loadImages(outputPath);
List<Image> targetImage = mosaic.loadImages(targetPath);

