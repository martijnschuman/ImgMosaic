using OpenCvSharp;

namespace ImgMosaic.Models;

public class ImgMosaicGenerator {
    private int InputResolution = 3;
    private int TargetResolution = 3;
    private int TileWidth = 24;
    private int TileHeight = 15;

    public enum InputTypes {
        Input,
        Target
    }

    public ImgMosaicGenerator(int inputResolution, int targetResolution) {
        InputResolution = inputResolution;
        TargetResolution = targetResolution;
    }

    public List<Image> LoadImages(InputTypes type, string folderPath) {
        List<Image> images = new List<Image>();

        if (Directory.Exists(folderPath)) {
            foreach (var imagePath in Directory.EnumerateFiles(folderPath).Where(f => f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))) {
                var tileImage = Cv2.ImRead(imagePath, ImreadModes.Color);

                if (tileImage.Empty()) {
                    Console.WriteLine($"Failed to load image: {imagePath}");
                    continue;
                }

                // Calculate aspectRatio for width
                if (type == InputTypes.Target) {
                    double aspectRatio = (double)tileImage.Cols / tileImage.Rows;
                    int width = (int)Math.Round(TileHeight * aspectRatio);

                }

                // Resize all input images to fixed size
                if (type == InputTypes.Input) {
                    Cv2.Resize(tileImage, tileImage, new Size(TileWidth, TileHeight));
                }

                int resolution = type == InputTypes.Input ? InputResolution : TargetResolution; 

                var img = new Image(
                    filePath: imagePath,
                    fileName: Path.GetFileNameWithoutExtension(imagePath),
                    fileExtension: Path.GetExtension(imagePath),
                    cols: tileImage.Width,
                    rows: tileImage.Height,
                    sizeInBytes: tileImage.Total() * tileImage.ElemSize(),
                    channels: tileImage.Channels(),
                    fullMatrix: tileImage,
                    downSampledMatrix: CalcuateImageMatrixForSpecificResolution(tileImage, resolution)
                );
                images.Add(img);
            }
        }
        else {
            throw new Exception("Folder not found: " + folderPath);
        }

        return images;
    }

    private Pixel[,] CalcuateImageMatrixForSpecificResolution(Mat image, int resolutionCols, int resolutionRows) {
        int cols = image.Width;
        int rows = image.Height;

        // Calculate the width and height of each block based on the resolution
        int blockWidth = cols / resolutionCols;
        int blockHeight = rows / resolutionRows;

        // Create a 2D array to hold the average color values
        Pixel[,] matrix = new Pixel[resolutionRows, resolutionCols];

        for (int row = 0; row < resolutionRows; row++) {
            for (int x = 0; x < resolutionCols; x++) {
                int startY = row * blockHeight;
                int startX = x * blockWidth;
                int endY = (row == resolutionRows - 1) ? rows : startY + blockHeight;
                int endX = (x == resolutionCols - 1) ? cols : startX + blockWidth;

                long sumB = 0, sumG = 0, sumR = 0;
                long count = 0;

                // Calculate the average color in the block
                for (int j = startY; j < endY; j++) {
                    for (int i = startX; i < endX; i++) {
                        var color = image.At<Vec3b>(j, i);
                        sumB += color.Item0;
                        sumG += color.Item1;
                        sumR += color.Item2;
                        count++;
                    }
                }

                // Calculate the average color values
                if (count == 0) {
                    matrix[row, x] = new Pixel(0, 0, 0);
                }
                else {
                    byte avgB = (byte)(sumB / count);
                    byte avgG = (byte)(sumG / count);
                    byte avgR = (byte)(sumR / count);
                    matrix[row, x] = new Pixel(avgR, avgG, avgB);
                }
            }
        }

        return matrix;
    }

    public static void PrintMatrix(Pixel[,] matrix) {
        if (matrix == null || matrix.Length == 0) {
            Console.WriteLine("Matrix is empty or null.");
            return;
        }

        for (int i = 0; i < matrix.GetLength(0); i++) {
            for (int j = 0; j < matrix.GetLength(1); j++) {
                var pixel = matrix[i, j];
                Console.Write($"Pixel[{i},{j}]: R={pixel.Red}, G={pixel.Green}, B={pixel.Blue} | ");
            }
            Console.WriteLine();
        }
    }

    public Mat ConstructFinalImage(List<Image> inputImages, Mat targetFullMatrix) {
        int targetCols = targetFullMatrix.Width / TileWidth;
        int targetRows = targetFullMatrix.Height / TileHeight;

        Mat resizedTarget = new Mat();
        Cv2.Resize(targetFullMatrix, resizedTarget, new Size(targetCols * TileWidth, targetRows * TileHeight));

        Pixel[,] targetMatrix = CalcuateImageMatrixForSpecificResolution(resizedTarget, targetCols);

        Mat result = new Mat(targetRows * TileHeight, targetCols * TileWidth, MatType.CV_8UC3);

        for (int y = 0; y < targetRows; y++) {
            for (int x = 0; x < targetCols; x++) {
                Console.WriteLine($"Y: {y}");
                Console.WriteLine($"X: {x}");
                Console.WriteLine($"GetLength 0: {targetMatrix.GetLength(0)}");
                Console.WriteLine($"GetLength 1: {targetMatrix.GetLength(1)}");

                Pixel targetPixel = targetMatrix[y, x];

                Image bestMatch = inputImages
                    .OrderBy(img => CalculateColorDistance(AverageMatrix(img.DownSampledMatrix), targetPixel))
                    .First();

                Rect roi = new Rect(x * TileWidth, y * TileHeight, TileWidth, TileHeight);
                bestMatch.FullMatrix.CopyTo(new Mat(result, roi));
            }
        }

        return result;
    }

    private static double CalculateColorDistance(Pixel p1, Pixel p2) {
        return Math.Sqrt(
            Math.Pow(p1.Red - p2.Red, 2) +
            Math.Pow(p1.Green - p2.Green, 2) +
            Math.Pow(p1.Blue - p2.Blue, 2)
        );
    }

    private static Pixel AverageMatrix(Pixel[,] matrix) {
        long sumR = 0, sumG = 0, sumB = 0;
        int width = matrix.GetLength(1);
        int height = matrix.GetLength(0);
        int count = width * height;

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                sumR += matrix[y, x].Red;
                sumG += matrix[y, x].Green;
                sumB += matrix[y, x].Blue;
            }
        }

        return new Pixel((int)(sumR / count), (int)(sumG / count), (int)(sumB / count));
    }


    public static void SaveImage(string outputPath, string fileName, Mat image) {
        if (image.Empty()) {
            throw new Exception("Image is empty, cannot save.");
        }
        Console.WriteLine($"Saving to path {outputPath}");
        
        if (!Path.Exists(outputPath)) {
            Directory.CreateDirectory(outputPath);
        }

        Cv2.ImWrite(Path.Combine(outputPath, fileName), image);
    }
}