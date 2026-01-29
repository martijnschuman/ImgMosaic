using OpenCvSharp;

namespace ImgMosaic.Models;

public class ImgMosaicGenerator {
    private readonly int MatchTileWidth;
    private readonly int MatchTileHeight;
    private readonly int RenderTileWidth;
    private readonly int RenderTileHeight;

    public enum InputTypes {
        Input,
        Target
    }

    public ImgMosaicGenerator(
    int matchTileWidth = 80,
    int matchTileHeight = 45,
    int renderTileWidth = 180,
    int renderTileHeight = 102) {
        MatchTileWidth = matchTileWidth;
        MatchTileHeight = matchTileHeight;
        RenderTileWidth = renderTileWidth;
        RenderTileHeight = renderTileHeight;
    }

    public List<Image> PreLoadImages(InputTypes type, List<string> inputPath) {
        List<Image> images = [];

        foreach (var path in inputPath) {
            Console.WriteLine($"Loading images from path: {path}");

            if (Directory.Exists(path)) {
                var filePaths = GetFilesInFolder(path);
                if (filePaths.Count() == 0) {
                    Console.WriteLine($"No images found in folder {path}");
                    Environment.Exit(0);
                }

                Console.WriteLine($"Found {filePaths.Count()} images in folder");

                foreach (var filePath in filePaths) {
                    if (images.Count > 750) {
                        break;
                    }

                    Mat image = LoadImage(filePath);

                    if (image.Empty()) {
                        Console.WriteLine($"Failed to load image: {filePath}");
                        continue;
                    }

                    // Create MatchRes depending on input type
                    Mat matchRes;
                    if (type == InputTypes.Input) {
                        matchRes = CreateMatchRes(image);
                    }
                    else {
                        // For target images we keep original resolution for matching
                        matchRes = image;
                    }

                    Image img = new(
                        filePath: filePath,
                        fileName: Path.GetFileNameWithoutExtension(filePath),
                        cols: image.Width,
                        rows: image.Height,
                        fullRes: image,
                        matchRes: matchRes,
                        avgColor: GetAverageColor(image)
                    );

                    images.Add(img);
                }
            }
            else {
                throw new Exception("Folder not found: " + path);
            }
        }

        return images;
    }

    private Mat CreateMatchRes(Mat image) {
        Mat matchRes = new Mat();
        Cv2.Resize(image, matchRes,
            new Size(MatchTileWidth, MatchTileHeight),
            0, 0, InterpolationFlags.Area);

        return matchRes;
    }

    public Mat LoadImage(string filePath) {
        var tileImage = Cv2.ImRead(filePath, ImreadModes.Color);

        if (tileImage.Empty()) {
            Console.WriteLine($"Failed to load image: {filePath}");
        }

        return tileImage;
    }

    private IEnumerable<string> GetFilesInFolder(string folderPath) {
        return Directory.EnumerateFiles(folderPath, "*.*", SearchOption.AllDirectories) // , SearchOption.AllDirectories
            .Where(f =>
                f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
            );
    }

    public Mat ConstructFinalImage(List<Image> inputImages, Mat targetFullMatrix) {
        Console.WriteLine("Constructing final image");

        int targetCols = targetFullMatrix.Width / MatchTileWidth;
        int targetRows = targetFullMatrix.Height / MatchTileHeight;

        Pixel[,] targetMatrix = CalculateImageMatrixForSpecificResolution(targetFullMatrix, targetCols, targetRows);

        Mat result = new(targetRows * RenderTileHeight, targetCols * RenderTileWidth, MatType.CV_8UC3);

        // Track which image was placed in each tile for neighbor penalties
        Image[,] usedImages = new Image[targetRows, targetCols];

        Random rand = new Random();

        for (int row = 0; row < targetRows; row++) {
            for (int col = 0; col < targetCols; col++) {
                Pixel targetPixel = targetMatrix[row, col];

                var ranked = inputImages
                    .Select(img => {
                        double colorDist = CalculateColorDistance(img.AvgColor, targetPixel);

                        // Add neighbor penalty
                        double neighborPenalty = 0;
                        if (row > 0 && usedImages[row - 1, col] == img) neighborPenalty += 1000;
                        if (col > 0 && usedImages[row, col - 1] == img) neighborPenalty += 1000;
                        if (row > 0 && col > 0 && usedImages[row - 1, col - 1] == img) neighborPenalty += 500;
                        if (row > 0 && col < targetCols - 1 && usedImages[row - 1, col + 1] == img) neighborPenalty += 500;

                        // Combine color distance, global penalty, and neighbor penalty
                        double score = colorDist + 5 * img.Penalty + neighborPenalty;
                        return new { Image = img, Score = score };
                    })
                    .OrderBy(x => x.Score)
                    .ToList();

                // Take top N candidates
                int topN = Math.Min(5, ranked.Count);
                var topCandidates = ranked.Take(topN).ToList();

                // Weighted random pick (lower score = more likely)
                double totalWeight = topCandidates.Sum(c => 1.0 / (c.Score + 1e-6));
                double r = rand.NextDouble() * totalWeight;

                Image bestMatchImage = topCandidates[0].Image; // fallback
                foreach (var c in topCandidates) {
                    r -= 1.0 / (c.Score + 1e-6);
                    if (r <= 0) {
                        bestMatchImage = c.Image;
                        break;
                    }
                }

                // Increase global penalty for fairness
                bestMatchImage.Penalty++;

                // Decay penalties slowly so images come back
                if ((row * targetCols + col) % 10 == 0) {
                    foreach (var img in inputImages){
                        if (img.Penalty > 0){
                            img.Penalty--;
                        }
                    }
                }

                // Place the tile
                Rect roi = new Rect(col * RenderTileWidth, row * RenderTileHeight, RenderTileWidth, RenderTileHeight);

                using Mat tileBuffer = new(RenderTileHeight, RenderTileWidth, MatType.CV_8UC3);

                Cv2.Resize(bestMatchImage.FullRes, tileBuffer, new Size(RenderTileWidth, RenderTileHeight));
                tileBuffer.CopyTo(new Mat(result, roi));

                // Store in used matrix for neighbor checks
                usedImages[row, col] = bestMatchImage;
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
