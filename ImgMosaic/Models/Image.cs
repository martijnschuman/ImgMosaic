using OpenCvSharp;

namespace ImgMosaic.Models;

public class Image {
    public string FilePath { get; set; }
    public string FileName { get; set; }
    public string FileExtension { get; set; }
    public int Cols { get; set; }
    public int Rows { get; set; }
    public long SizeInBytes { get; set; }
    public int Channels { get; set; }
    public Mat FullMatrix { get; set; }
    public Pixel[,] DownSampledMatrix { get; set; }

    public Image(string filePath, string fileName, string fileExtension, int cols, int rows, long sizeInBytes, int channels, Mat fullMatrix, Pixel[,] downSampledMatrix) {
        FilePath = filePath;
        FileName = fileName;
        FileExtension = fileExtension;
        Cols = cols;
        Rows = rows;
        SizeInBytes = sizeInBytes;
        Channels = channels;
        FullMatrix = fullMatrix;
        DownSampledMatrix = downSampledMatrix;
    }
}