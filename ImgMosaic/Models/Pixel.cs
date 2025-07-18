namespace ImgMosaic.Models;

public class Pixel {
    public int Red { get; set; }
    public int Green { get; set; }
    public int Blue { get; set; }

    public Pixel(int red, int green, int blue) {
        Red = red;
        Green = green;
        Blue = blue;
    }
}
