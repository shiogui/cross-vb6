using System;
using System.Numerics;
using Raylib_cs;

namespace CrossVB6;

public class IDE
{
    private Color bgColor = new Color(212, 208, 200, 255);
    private Color mdiBgColor = new Color(128, 128, 128, 255);
    private Color winBlue = new Color(0, 0, 128, 255);
    private Color darkGray = new Color(128, 128, 128, 255);
    private Color lightGray = new Color(223, 223, 223, 255);

    public IDE()
    {
    }

    internal void UpdateAndDraw()
    {
        int screenWidth = Raylib.GetScreenWidth();
        int screenHeight = Raylib.GetScreenHeight();

        Raylib.ClearBackground(bgColor);

        // 1. Menu Bar
        int menuHeight = 20;
        DrawMenuBar(0, 0, screenWidth, menuHeight);

        // 2. Toolbar
        int toolbarHeight = 28;
        DrawToolbar(0, menuHeight, screenWidth, toolbarHeight);

        int topOffset = menuHeight + toolbarHeight;
        int workspaceHeight = screenHeight - topOffset;

        // 3. Toolbox (Left)
        int toolboxWidth = 90;
        DrawToolbox(0, topOffset, toolboxWidth, workspaceHeight);

        // 4. Right Panels
        int rightPanelWidth = 200;
        int rightPanelX = screenWidth - rightPanelWidth;
        
        int projExplorerHeight = workspaceHeight / 3;
        int propertiesHeight = workspaceHeight / 2;
        int formLayoutHeight = workspaceHeight - projExplorerHeight - propertiesHeight;

        DrawProjectExplorer(rightPanelX, topOffset, rightPanelWidth, projExplorerHeight);
        DrawPropertiesWindow(rightPanelX, topOffset + projExplorerHeight, rightPanelWidth, propertiesHeight);
        DrawFormLayout(rightPanelX, topOffset + projExplorerHeight + propertiesHeight, rightPanelWidth, formLayoutHeight);

        // 5. MDI Workspace (Middle)
        int mdiX = toolboxWidth;
        int mdiY = topOffset;
        int mdiWidth = screenWidth - toolboxWidth - rightPanelWidth;
        int mdiHeight = workspaceHeight;

        Raylib.DrawRectangle(mdiX, mdiY, mdiWidth, mdiHeight, mdiBgColor);
        Draw3DBorder(mdiX, mdiY, mdiWidth, mdiHeight, true);

        // 6. Draw an open Form window in MDI workspace
        DrawFormWindow(mdiX + 20, mdiY + 20, 500, 400, "Project1 - Form1 (Form)");
    }

    private void DrawMenuBar(int x, int y, int width, int height)
    {
        Raylib.DrawRectangle(x, y, width, height, bgColor);
        string[] menus = { "File", "Edit", "View", "Project", "Format", "Debug", "Run", "Query", "Diagram", "Tools", "Add-Ins", "Window", "Help" };
        int currentX = x + 8;
        foreach (var menu in menus)
        {
            Raylib.DrawText(menu, currentX, y + 5, 10, Color.Black);
            currentX += Raylib.MeasureText(menu, 10) + 16;
        }
    }

    private void DrawToolbar(int x, int y, int width, int height)
    {
        Raylib.DrawRectangle(x, y, width, height, bgColor);
        Draw3DBorder(x, y, width, height, false);

        int buttonSize = 22;
        int currentX = x + 5;
        for (int i = 0; i < 20; i++)
        {
            if (i == 3 || i == 8 || i == 12 || i == 16)
            {
                // separator
                Draw3DBorder(currentX, y + 2, 2, height - 4, false);
                currentX += 6;
            }
            Draw3DBorder(currentX, y + 3, buttonSize, buttonSize, false);
            
            // tiny icon rect
            Raylib.DrawRectangle(currentX + 4, y + 7, buttonSize - 8, buttonSize - 8, Color.Gray);
            
            currentX += buttonSize + 2;
        }
        
        // draw position and size coordinates to simulate VB6 toolbar
        Raylib.DrawText("0, 0", width - 150, y + 8, 10, Color.Black);
        Raylib.DrawText("4800 x 3600", width - 80, y + 8, 10, Color.Black);
    }

    private void DrawToolbox(int x, int y, int width, int height)
    {
        DrawWindowFrame(x, y, width, height, "General", false);
        
        // Draw toolbox grid
        int startX = x + 4;
        int startY = y + 22;
        int itemSize = 24;
        
        for (int i=0; i<10; i++) {
            for (int j=0; j<2; j++) {
                int bx = startX + j*(itemSize + 4);
                int by = startY + i*(itemSize + 4);
                Draw3DBorder(bx, by, itemSize, itemSize, false);
                Raylib.DrawRectangle(bx + 4, by + 4, itemSize - 8, itemSize - 8, Color.Gray);
            }
        }
    }

    private void DrawProjectExplorer(int x, int y, int width, int height)
    {
        DrawWindowFrame(x, y, width, height, "Project - Project1", true);
        Raylib.DrawRectangle(x + 2, y + 20, width - 4, height - 22, Color.White);
        Draw3DBorder(x + 2, y + 20, width - 4, height - 22, true);
        
        int textY = y + 25;
        Raylib.DrawText("- Project1 (Project1.vbp)", x + 5, textY, 10, Color.Black);
        Raylib.DrawText("- Forms", x + 15, textY + 15, 10, Color.Black);
        Raylib.DrawText("Form1 (Form1.frm)", x + 30, textY + 30, 10, Color.Black);
        Raylib.DrawText("- Modules", x + 15, textY + 45, 10, Color.Black);
        Raylib.DrawText("Module1 (Module1.bas)", x + 30, textY + 60, 10, Color.Black);
    }

    private void DrawPropertiesWindow(int x, int y, int width, int height)
    {
        DrawWindowFrame(x, y, width, height, "Properties - Form1", true);
        
        // ComboBox
        Raylib.DrawRectangle(x + 2, y + 20, width - 4, 20, Color.White);
        Draw3DBorder(x + 2, y + 20, width - 4, 20, true);
        Raylib.DrawText("Form1 Form", x + 5, y + 25, 10, Color.Black);

        // Property Grid
        Raylib.DrawRectangle(x + 2, y + 42, width - 4, height - 44, Color.White);
        Draw3DBorder(x + 2, y + 42, width - 4, height - 44, true);

        int propY = y + 44;
        Raylib.DrawLine(x + width/2, propY, x + width/2, propY + height - 44, Color.LightGray);
        
        string[] props = { "(Name)", "Appearance", "AutoRedraw", "BackColor", "BorderStyle", "Caption", "ClipControls", "ControlBox", "DrawMode", "DrawStyle", "DrawWidth", "Enabled", "FillColor", "FillStyle" };
        string[] vals =  { "Form1", "1 - 3D", "False", "&H8000000F&", "2 - Sizable", "Form1", "True", "True", "13 - Copy Pen", "0 - Solid", "1", "True", "&H00000000&", "1 - Transparent" };

        for (int i = 0; i < props.Length; i++)
        {
            if (propY + 2 + i * 14 + 10 > y + height) break;
            Raylib.DrawText(props[i], x + 5, propY + 2 + i * 14, 10, Color.Black);
            Raylib.DrawText(vals[i], x + width/2 + 5, propY + 2 + i * 14, 10, Color.Black);
            Raylib.DrawLine(x + 2, propY + 14 + i * 14, x + width - 3, propY + 14 + i * 14, Color.LightGray);
        }
    }

    private void DrawFormLayout(int x, int y, int width, int height)
    {
        DrawWindowFrame(x, y, width, height, "Form Layout", true);
        Raylib.DrawRectangle(x + 2, y + 20, width - 4, height - 22, Color.White);
        Draw3DBorder(x + 2, y + 20, width - 4, height - 22, true);

        // Monitor graphic
        int mx = x + width/2 - 30;
        int my = y + 20 + height/2 - 25;
        Raylib.DrawRectangle(mx, my, 60, 45, bgColor);
        Draw3DBorder(mx, my, 60, 45, false);
        Raylib.DrawRectangle(mx + 4, my + 4, 52, 37, Color.Black);
        
        // Small form
        Raylib.DrawRectangle(mx + 10, my + 10, 20, 15, bgColor);
        Raylib.DrawRectangle(mx + 10, my + 10, 20, 4, winBlue);
    }

    private void DrawFormWindow(int x, int y, int width, int height, string title)
    {
        DrawWindowFrame(x, y, width, height, title, true);
        
        // Inner form area (dotted grid)
        int innerX = x + 4;
        int innerY = y + 22;
        int innerW = width - 8;
        int innerH = height - 26;

        Raylib.DrawRectangle(innerX, innerY, innerW, innerH, bgColor);
        
        // Draw grid dots
        for (int i = 0; i < innerW; i += 8)
        {
            for (int j = 0; j < innerH; j += 8)
            {
                Raylib.DrawPixel(innerX + i, innerY + j, Color.Black);
            }
        }
        
        // Let's draw some dummy controls on the form to make it look active
        // A Button
        Draw3DBorder(innerX + 50, innerY + 50, 100, 30, false);
        Raylib.DrawText("Command1", innerX + 70, innerY + 60, 10, Color.Black);
        
        // A Textbox
        Raylib.DrawRectangle(innerX + 50, innerY + 100, 150, 20, Color.White);
        Draw3DBorder(innerX + 50, innerY + 100, 150, 20, true);
        Raylib.DrawText("Text1", innerX + 55, innerY + 105, 10, Color.Black);
    }

    private void DrawWindowFrame(int x, int y, int width, int height, string title, bool isToolWindow)
    {
        Raylib.DrawRectangle(x, y, width, height, bgColor);
        Draw3DBorder(x, y, width, height, false);

        int titleHeight = isToolWindow ? 18 : 20;
        Raylib.DrawRectangle(x + 2, y + 2, width - 4, titleHeight, winBlue);
        Raylib.DrawText(title, x + 4, y + 6, 10, Color.White);

        // Close button
        int btnSize = titleHeight - 4;
        int btnX = x + width - 2 - btnSize - 1;
        int btnY = y + 4;
        Raylib.DrawRectangle(btnX, btnY, btnSize, btnSize, bgColor);
        Draw3DBorder(btnX, btnY, btnSize, btnSize, false);
        Raylib.DrawLine(btnX + 3, btnY + 3, btnX + btnSize - 4, btnY + btnSize - 4, Color.Black);
        Raylib.DrawLine(btnX + 3, btnY + btnSize - 4, btnX + btnSize - 4, btnY + 3, Color.Black);
    }

    private void Draw3DBorder(int x, int y, int width, int height, bool sunken)
    {
        Color topLeftOuter = sunken ? darkGray : Color.White;
        Color topLeftInner = sunken ? Color.Black : lightGray;
        Color bottomRightOuter = sunken ? Color.White : Color.Black;
        Color bottomRightInner = sunken ? lightGray : darkGray;

        // Outer
        Raylib.DrawLine(x, y, x + width - 1, y, topLeftOuter); // Top
        Raylib.DrawLine(x, y, x, y + height - 1, topLeftOuter); // Left
        Raylib.DrawLine(x, y + height - 1, x + width - 1, y + height - 1, bottomRightOuter); // Bottom
        Raylib.DrawLine(x + width - 1, y, x + width - 1, y + height - 1, bottomRightOuter); // Right

        // Inner
        Raylib.DrawLine(x + 1, y + 1, x + width - 2, y + 1, topLeftInner); // Top
        Raylib.DrawLine(x + 1, y + 1, x + 1, y + height - 2, topLeftInner); // Left
        Raylib.DrawLine(x + 1, y + height - 2, x + width - 2, y + height - 2, bottomRightInner); // Bottom
        Raylib.DrawLine(x + width - 2, y + 1, x + width - 2, y + height - 2, bottomRightInner); // Right
    }
}