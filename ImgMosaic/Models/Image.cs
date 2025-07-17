using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImgMosaic.Models {
    public class Image {
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string FileExtension { get; set; }
        public int Cols { get; set; }
        public int Rows { get; set; }
        public int SizeInBytes { get; set; }
        public int Channels { get; set; }

        public Image(string filePath, string fileName, string fileExtension, int cols, int rows, int sizeInBytes, int channels) {
            FilePath = filePath;
            FileName = fileName;
            FileExtension = fileExtension;
            Cols = cols;
            Rows = rows;
            SizeInBytes = sizeInBytes;
            Channels = channels;
        }
    }
}
