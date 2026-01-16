using OpenCvSharp;

public static class DeepZoomGenerator {
    public static void Generate(Mat source, string outputDir, int tileSize = 256) {
        Directory.CreateDirectory(outputDir);

        using Mat safeSource = source.Clone();

        int width = source.Width;
        int height = source.Height;

        int maxLevel = (int)Math.Ceiling(Math.Log(Math.Max(width, height), 2));

        for (int level = maxLevel; level >= 0; level--) {
            int levelWidth = Math.Max(1, width >> (maxLevel - level));
            int levelHeight = Math.Max(1, height >> (maxLevel - level));

            Mat levelImage = new();
            Cv2.Resize(
                safeSource,
                levelImage,
                new Size(levelWidth, levelHeight),
                0,
                0,
                InterpolationFlags.Area
            );

            string levelDir = Path.Combine(outputDir, level.ToString());
            Directory.CreateDirectory(levelDir);

            int cols = (int)Math.Ceiling(levelWidth / (double)tileSize);
            int rows = (int)Math.Ceiling(levelHeight / (double)tileSize);

            for (int y = 0; y < rows; y++) {
                for (int x = 0; x < cols; x++) {
                    int w = Math.Min(tileSize, levelWidth - x * tileSize);
                    int h = Math.Min(tileSize, levelHeight - y * tileSize);

                    if (w <= 0 || h <= 0)
                        continue;

                    Rect roi = new(x * tileSize, y * tileSize, w, h);
                    using Mat tile = new(levelImage, roi);

                    Cv2.ImWrite(
                        Path.Combine(levelDir, $"{x}_{y}.jpg"),
                        tile
                    );
                }
            }
        }
    }
}
