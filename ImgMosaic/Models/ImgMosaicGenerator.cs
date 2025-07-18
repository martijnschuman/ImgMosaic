using OpenCvSharp;

namespace ImgMosaic.Models;

public class ImgMosaicGenerator {
    public List<Image> loadImages(string folderPath) {
        List<Image> images = new List<Image>();

        if (Directory.Exists(folderPath)) {
            foreach (var imagePath in Directory.EnumerateFiles(folderPath, "*.png")) {
                using var tileImage = Cv2.ImRead(imagePath, ImreadModes.Color);
                if (tileImage.Empty()) {
                    Console.WriteLine($"Failed to load image: {imagePath}");
                    continue;
                }

                var img = new Image(
                    filePath: imagePath,
                    fileName: Path.GetFileNameWithoutExtension(imagePath),
                    fileExtension: Path.GetExtension(imagePath),
                    cols: tileImage.Width,
                    rows: tileImage.Height,
                    sizeInBytes: tileImage.Total() * tileImage.ElemSize(),
                    channels: tileImage.Channels(),
                    fullMatrix: tileImage,
                    downSampledMatrix: calcuateImageMatrixForSpecificResolution(tileImage, 3)
                );
                images.Add(img);
            }
        }
        else {
            throw new Exception("Folder not found: " + folderPath);
        }

        return images;
    }

    private Pixel[,] calcuateImageMatrixForSpecificResolution(Mat image, int resolution) {
        int cols = image.Width;
        int rows = image.Height;

        // Calculate the width and height of each block based on the resolution
        int blockWidth = cols / resolution;
        int blockHeight = rows / resolution;

        // Create a 2D array to hold the average color values
        Pixel[,] matrix = new Pixel[resolution, resolution];

        for (int y = 0; y < resolution; y++) {
            for (int x = 0; x < resolution; x++) {
                int startX = x * blockWidth;
                int startY = y * blockHeight;
                int endX = (x == resolution - 1) ? cols : startX + blockWidth;
                int endY = (y == resolution - 1) ? rows : startY + blockHeight;

                long sumB = 0, sumG = 0, sumR = 0;
                int count = 0;

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
                byte avgB = (byte)(sumB / count);
                byte avgG = (byte)(sumG / count);
                byte avgR = (byte)(sumR / count);

                // Store the average color in the matrix
                matrix[y, x] = new Pixel(avgR, avgG, avgB);
            }
        }

        return matrix;
    }

    public void printMatrix(Pixel[,] matrix) {
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

    public void saveImage(string outputPath, Mat image) {
        if (image.Empty()) {
            throw new Exception("Image is empty, cannot save.");
        }
        Cv2.ImWrite(outputPath, image);
    }
}