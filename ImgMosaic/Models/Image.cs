using OpenCvSharp;

namespace ImgMosaic.Models;

public class Image {
    public string FilePath { get; set; }
    public string FileName { get; set; }
    public int Cols { get; set; }
    public int Rows { get; set; }
    public Mat FullMatrix { get; set; }

    public Image(string filePath, string fileName, int cols, int rows, Mat fullMatrix) {
        FilePath = filePath;
        FileName = fileName;
        Cols = cols;
        Rows = rows;
        FullMatrix = fullMatrix;
    }
}