using System;
using System.IO;
using OpenCvSharp;

string inputPath = Path.Combine(Directory.GetCurrentDirectory(), "input");
string outputPath = Path.Combine(Directory.GetCurrentDirectory(), "output");
string targetPath = Path.Combine(Directory.GetCurrentDirectory(), "target");





if (Directory.Exists(inputPath)) {
    string[] imageFiles = Directory.GetFiles(inputPath, "*.png");

    foreach (string imagePath in imageFiles) {
        Mat tileImage = Cv2.ImRead(imagePath, ImreadModes.Color);

        if (tileImage.Empty()) {
            Console.WriteLine($"Failed to load image: {imagePath}");
        }
        else {
            Console.WriteLine($"Loaded tile: {Path.GetFileName(imagePath)}, Size: {tileImage.Width}x{tileImage.Height}");
            string fileName = Path.GetFileNameWithoutExtension(imagePath);

        }
    }
}
else {
    Console.WriteLine("Folder not found: " + inputPath);
}
