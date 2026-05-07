using Raylib_cs;

namespace CrossVB6;

public class IDE
{
    internal void UpdateAndDraw()
    {
        Raylib.ClearBackground(Color.White);
        Raylib.DrawText("HOT RELOAD BABY!", 190, 200, 20, Color.DarkGray);
        Raylib.DrawText("Press F11 to toggle fullscreen", 10, 40, 10, Color.DarkGray);
    }
}