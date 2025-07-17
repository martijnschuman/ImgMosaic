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
                    channels: tileImage.Channels()
                );
                images.Add(img);
            }
        }
        else {
            throw new Exception("Folder not found: " + folderPath);
        }

        return images;
    }
}