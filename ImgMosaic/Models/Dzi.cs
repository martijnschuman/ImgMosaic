namespace ImgMosaic.Models;

public static class Dzi {
    public static void WriteDzi(
    string path,
    int width,
    int height,
    int tileSize = 256) {
        string dzi = $"""
            <?xml version="1.0" encoding="UTF-8"?>
            <Image TileSize="{tileSize}" Overlap="0" Format="jpg"
                    xmlns="http://schemas.microsoft.com/deepzoom/2008">
                <Size Width="{width}" Height="{height}"/>
            </Image>
            """;

        File.WriteAllText(path, dzi);
    }
}

