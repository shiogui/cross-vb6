using Raylib_cs;

namespace CrossVB6;

public class IDE
{
    internal void UpdateAndDraw()
    {
        Raylib.ClearBackground(Color.White);
        Raylib.DrawText("Congrats Reflection host worked or not!", 190, 200, 20, Color.DarkGray);
    }
}