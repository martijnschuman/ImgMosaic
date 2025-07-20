using OpenCvSharp;

namespace ImgMosaic.Models;

public class ImgMosaicGenerator {
    private int TileWidth;
    private int TileHeight;

    public enum InputTypes {
        Input,
        Target
    }

    public ImgMosaicGenerator(int tileWidth = 75, int tileHeight = 50) {
        TileWidth = tileWidth;
        TileHeight = tileHeight;
    }

    public List<Image> LoadImages(InputTypes type, string folderPath) {
        Console.WriteLine($"Loading images from path: {folderPath}");
        List<Image> images = new List<Image>();

        if (Directory.Exists(folderPath)) {
            foreach (var imagePath in GetFilesInFolder(folderPath)) {
                var tileImage = Cv2.ImRead(imagePath, ImreadModes.Color);

                if (tileImage.Empty()) {
                    Console.WriteLine($"Failed to load image: {imagePath}");
                    continue;
                }

                // Resize all input images to fixed size
                if (type == InputTypes.Input) {
                    Cv2.Resize(tileImage, tileImage, new Size(TileWidth, TileHeight));
                }

                Image img = new(
                    filePath: imagePath,
                    fileName: Path.GetFileNameWithoutExtension(imagePath),
                    cols: tileImage.Width,
                    rows: tileImage.Height,
                    fullMatrix: tileImage
                );

                images.Add(img);
            }
        }
        else {
            throw new Exception("Folder not found: " + folderPath);
        }

        return images;
    }

    private IEnumerable<string> GetFilesInFolder(string folderPath) {
        return Directory.EnumerateFiles(folderPath)
            .Where(
                f => f.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                || f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                || f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
            );
    }

    public Mat ConstructFinalImage(List<Image> inputImages, Mat targetFullMatrix) {
        Console.WriteLine("Constructing final image");

        int targetCols = targetFullMatrix.Width / TileWidth;
        int targetRows = targetFullMatrix.Height / TileHeight;

        Pixel[,] targetMatrix = CalculateImageMatrixForSpecificResolution(targetFullMatrix, targetCols, targetRows);

        Mat result = new(targetRows * TileHeight, targetCols * TileWidth, MatType.CV_8UC3);

        for (int rows = 0; rows < targetRows; rows++) {
            for (int cols = 0; cols < targetCols; cols++) {
                Pixel targetPixel = targetMatrix[rows, cols];

                Image bestMatch = inputImages
                    .OrderBy(img => CalculateColorDistance(GetAverageColor(img.FullMatrix), targetPixel))
                    .First();

                Rect roi = new Rect(cols * TileWidth, rows * TileHeight, TileWidth, TileHeight);
                bestMatch.FullMatrix.CopyTo(new Mat(result, roi));
            }
        }

        return result;
    }

    private Pixel[,] CalculateImageMatrixForSpecificResolution(Mat image, int resolutionCols, int resolutionRows) {
        int rows = image.Height;
        int cols = image.Width;

        // Calculate the width and height of each block based on the resolution
        int blockWidth = cols / resolutionCols;
        int blockHeight = rows / resolutionRows;

        // Create a 2D array to hold the average color values
        Pixel[,] matrix = new Pixel[resolutionRows, resolutionCols];

        for (int row = 0; row < resolutionRows; row++) {
            for (int col = 0; col < resolutionCols; col++) {
                int startRow = row * blockHeight;
                int startCol = col * blockWidth;
                int endRow = (row == resolutionRows - 1) ? rows : startRow + blockHeight;
                int endCol = (col == resolutionCols - 1) ? cols : startCol + blockWidth;

                long sumB = 0, sumG = 0, sumR = 0;
                long count = 0;

                // Calculate the average color in the block
                for (int blockRow = startRow; blockRow < endRow; blockRow++) {
                    for (int blockCol = startCol; blockCol < endCol; blockCol++) {
                        var color = image.At<Vec3b>(blockRow, blockCol);
                        sumB += color.Item0;
                        sumG += color.Item1;
                        sumR += color.Item2;
                        count++;
                    }
                }

                // Calculate the average color values
                if (count == 0) {
                    matrix[row, col] = new Pixel(0, 0, 0);
                }
                else {
                    byte avgB = (byte)(sumB / count);
                    byte avgG = (byte)(sumG / count);
                    byte avgR = (byte)(sumR / count);
                    matrix[row, col] = new Pixel(avgR, avgG, avgB);
                }
            }
        }

        return matrix;
    }

    private static double CalculateColorDistance(Pixel p1, Pixel p2) {
        return Math.Sqrt(
            Math.Pow(p1.Red - p2.Red, 2) +
            Math.Pow(p1.Green - p2.Green, 2) +
            Math.Pow(p1.Blue - p2.Blue, 2)
        );
    }

    private static Pixel GetAverageColor(Mat image) {
        Scalar mean = Cv2.Mean(image);
        return new Pixel((int)mean.Val2, (int)mean.Val1, (int)mean.Val0); // OpenCV uses BGR
    }

    public static void SaveImage(string outputPath, string fileName, Mat image) {
        if (image.Empty()) {
            throw new Exception("Image is empty, cannot save.");
        }
        Console.WriteLine($"Saving to path {outputPath}");
        
        if (!Path.Exists(outputPath)) {
            Directory.CreateDirectory(outputPath);
        }

        string finalPath = Path.Combine(outputPath, fileName);

        Cv2.ImWrite(finalPath, image);
    }
}