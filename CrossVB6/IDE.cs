using System;
using System.IO;
using System.Collections.Generic;
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

    private Dictionary<string, Texture2D> textures = new();
    private Font msSansSerif;

    public IDE()
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string fontPath = Path.GetFullPath(Path.Combine(baseDir, "../../../../AvaloniaVisualBasic/Resources/MS-Sans-Serif.ttf"));
        
        if (File.Exists(fontPath))
        {
            msSansSerif = Raylib.LoadFontEx(fontPath, 13, null, 0);
        }
        else
        {
            msSansSerif = Raylib.GetFontDefault();
        }

        string iconsDir = Path.GetFullPath(Path.Combine(baseDir, "../../../../AvaloniaVisualBasic/Icons"));
        string[] iconsToLoad = {
            "cursor.gif", "picture.gif", "label.gif", "textbox.gif", "groupbox.gif",
            "button.gif", "checkbox.gif", "radio.gif", "combo.gif", "listbox.gif",
            "hscroll.gif", "vscroll.gif", "timer.gif", "drives.gif", "directories.gif",
            "files.gif", "shape.gif", "line.gif", "image.gif", "data.gif", "ole.gif",
            "addproject.gif", "addform.gif", "menubar.gif", "open.gif", "save.gif",
            "cut.gif", "copy.gif", "paste.gif", "find.gif", "undo.gif", "redo.gif",
            "play.gif", "pause.gif", "stop.gif", "projectexplorer.gif", "properties.gif",
            "formlayout.gif", "objectbrowser.gif", "toolbox.gif", "dataview.gif", "visualcomponent.gif",
            "project_folder.gif", "form.gif", "project.gif", "viewcode.gif", "viewobject.gif", "folder.gif"
        };

        if (Directory.Exists(iconsDir))
        {
            foreach (var icon in iconsToLoad)
            {
                string path = Path.Combine(iconsDir, icon);
                if (File.Exists(path))
                {
                    textures[icon] = Raylib.LoadTexture(path);
                }
            }
        }
    }

    private void DrawText(string text, int x, int y, Color color, bool bold = false)
    {
        // MS Sans Serif 13 usually corresponds to size 13 in Raylib context too.
        Raylib.DrawTextEx(msSansSerif, text, new Vector2(x, y), 13, 0, color);
        if (bold)
        {
            Raylib.DrawTextEx(msSansSerif, text, new Vector2(x + 1, y), 13, 0, color);
        }
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
        int rightPanelWidth = 220;
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
            DrawText(menu, currentX, y + 4, Color.Black);
            currentX += (int)Raylib.MeasureTextEx(msSansSerif, menu, 13, 0).X + 16;
        }
    }

    private void DrawToolbar(int x, int y, int width, int height)
    {
        Raylib.DrawRectangle(x, y, width, height, bgColor);
        Draw3DBorder(x, y, width, height, false);

        string[] toolbarIcons = {
            "addproject.gif", "addform.gif", "menubar.gif", "SEP",
            "open.gif", "save.gif", "SEP",
            "cut.gif", "copy.gif", "paste.gif", "find.gif", "SEP",
            "undo.gif", "redo.gif", "SEP",
            "play.gif", "pause.gif", "stop.gif", "SEP",
            "projectexplorer.gif", "properties.gif", "formlayout.gif", "objectbrowser.gif", "toolbox.gif", "dataview.gif", "visualcomponent.gif"
        };

        int buttonSize = 22;
        int currentX = x + 5;
        foreach (var icon in toolbarIcons)
        {
            if (icon == "SEP")
            {
                Draw3DBorder(currentX, y + 2, 2, height - 4, false);
                currentX += 6;
                continue;
            }

            Draw3DBorder(currentX, y + 3, buttonSize, buttonSize, false);
            if (textures.TryGetValue(icon, out Texture2D tex))
            {
                int dx = currentX + (buttonSize - tex.Width) / 2;
                int dy = y + 3 + (buttonSize - tex.Height) / 2;
                Raylib.DrawTexture(tex, dx, dy, Color.White);
            }
            
            currentX += buttonSize + 2;
        }
        
        DrawText("0, 0", width - 150, y + 8, Color.Black);
        DrawText("4800 x 3600", width - 80, y + 8, Color.Black);
    }

    private void DrawToolbox(int x, int y, int width, int height)
    {
        DrawWindowFrame(x, y, width, height, "General", false);
        
        string[] toolboxIcons = {
            "cursor.gif", "picture.gif", "label.gif", "textbox.gif", "groupbox.gif",
            "button.gif", "checkbox.gif", "radio.gif", "combo.gif", "listbox.gif",
            "hscroll.gif", "vscroll.gif", "timer.gif", "drives.gif", "directories.gif",
            "files.gif", "shape.gif", "line.gif", "image.gif", "data.gif", "ole.gif"
        };

        int startX = x + 10;
        int startY = y + 22;
        int itemSize = 24;
        
        for (int i=0; i<toolboxIcons.Length; i++) {
            int col = i % 2;
            int row = i / 2;
            int bx = startX + col*(itemSize + 10);
            int by = startY + row*(itemSize + 4);

            if (i == 0) // pointer is usually selected
            {
                Raylib.DrawRectangle(bx, by, itemSize, itemSize, lightGray);
                Draw3DBorder(bx, by, itemSize, itemSize, true);
            }
            
            if (textures.TryGetValue(toolboxIcons[i], out Texture2D tex))
            {
                int dx = bx + (itemSize - tex.Width) / 2;
                int dy = by + (itemSize - tex.Height) / 2;
                Raylib.DrawTexture(tex, dx, dy, Color.White);
            }
        }
    }

    private void DrawProjectExplorer(int x, int y, int width, int height)
    {
        DrawWindowFrame(x, y, width, height, "Project - Project1", true);
        
        // Internal toolbar area
        Raylib.DrawRectangle(x + 2, y + 20, width - 4, 26, bgColor);
        Draw3DBorder(x + 2, y + 20, width - 4, 26, false);
        
        int btnY = y + 22;
        if (textures.TryGetValue("viewcode.gif", out var t1)) { Draw3DBorder(x + 5, btnY, 22, 22, false); Raylib.DrawTexture(t1, x + 8, btnY + 3, Color.White); }
        if (textures.TryGetValue("viewobject.gif", out var t2)) { Draw3DBorder(x + 29, btnY, 22, 22, false); Raylib.DrawTexture(t2, x + 32, btnY + 3, Color.White); }
        if (textures.TryGetValue("folder.gif", out var t3)) { Draw3DBorder(x + 53, btnY, 22, 22, true); Raylib.DrawTexture(t3, x + 56, btnY + 3, Color.White); }

        // TreeView area
        Raylib.DrawRectangle(x + 2, y + 46, width - 4, height - 48, Color.White);
        Draw3DBorder(x + 2, y + 46, width - 4, height - 48, true);
        
        int textY = y + 50;
        
        if (textures.TryGetValue("project.gif", out var proj)) Raylib.DrawTexture(proj, x + 5, textY, Color.White);
        DrawText("Project1 (Project1.vbp)", x + 25, textY + 2, Color.Black, true);
        
        if (textures.TryGetValue("project_folder.gif", out var pfolder)) Raylib.DrawTexture(pfolder, x + 20, textY + 20, Color.White);
        DrawText("Forms", x + 40, textY + 22, Color.Black);

        if (textures.TryGetValue("form.gif", out var pform)) Raylib.DrawTexture(pform, x + 35, textY + 40, Color.White);
        DrawText("Form1 (Form1.frm)", x + 55, textY + 42, Color.Black);
    }

    private void DrawPropertiesWindow(int x, int y, int width, int height)
    {
        DrawWindowFrame(x, y, width, height, "Properties - Form1", true);
        
        // ComboBox
        Raylib.DrawRectangle(x + 2, y + 20, width - 4, 22, Color.White);
        Draw3DBorder(x + 2, y + 20, width - 4, 22, true);
        DrawText("Form1 Form", x + 5, y + 25, Color.Black, true);

        // Tabs (Alphabetic / Categorized) - fake tabs
        Raylib.DrawRectangle(x + 2, y + 44, width - 4, 22, bgColor);
        Draw3DBorder(x + 2, y + 44, width - 4, 22, false);
        Draw3DBorder(x + 4, y + 46, 70, 20, true);
        DrawText("Alphabetic", x + 10, y + 50, Color.Black);
        DrawText("Categorized", x + 85, y + 50, Color.Black);

        // Property Grid
        int gridY = y + 68;
        int gridH = height - 70 - 40; // leave room at bottom for description box
        Raylib.DrawRectangle(x + 2, gridY, width - 4, gridH, Color.White);
        Draw3DBorder(x + 2, gridY, width - 4, gridH, true);

        int propY = gridY;
        Raylib.DrawLine(x + width/2, propY, x + width/2, propY + gridH, Color.LightGray);
        
        string[] props = { "(Name)", "Appearance", "AutoRedraw", "BackColor", "BorderStyle", "Caption", "ClipControls", "ControlBox", "DrawMode", "DrawStyle", "DrawWidth", "Enabled", "FillColor", "FillStyle" };
        string[] vals =  { "Form1", "1 - 3D", "False", "&H8000000F&", "2 - Sizable", "Form1", "True", "True", "13 - Copy Pen", "0 - Solid", "1", "True", "&H00000000&", "1 - Transparent" };

        for (int i = 0; i < props.Length; i++)
        {
            if (propY + 2 + i * 16 + 14 > gridY + gridH) break;
            DrawText(props[i], x + 5, propY + 3 + i * 16, Color.Black);
            DrawText(vals[i], x + width/2 + 5, propY + 3 + i * 16, Color.Black, i == 0 || i == 5); // bold some values
            Raylib.DrawLine(x + 2, propY + 16 + i * 16, x + width - 3, propY + 16 + i * 16, Color.LightGray);
        }

        // Description box
        int descY = gridY + gridH + 2;
        Raylib.DrawRectangle(x + 2, descY, width - 4, 36, bgColor);
        Draw3DBorder(x + 2, descY, width - 4, 36, false);
        DrawText("Caption", x + 5, descY + 4, Color.Black, true);
        DrawText("Returns/sets the text displayed in an object's", x + 5, descY + 20, Color.Black);
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
        
        // Draw some dummy controls on the form
        // A Button
        Draw3DBorder(innerX + 50, innerY + 50, 100, 30, false);
        DrawText("Command1", innerX + 70, innerY + 60, Color.Black);
        
        // A Textbox
        Raylib.DrawRectangle(innerX + 50, innerY + 100, 150, 20, Color.White);
        Draw3DBorder(innerX + 50, innerY + 100, 150, 20, true);
        DrawText("Text1", innerX + 55, innerY + 104, Color.Black);
    }

    private void DrawWindowFrame(int x, int y, int width, int height, string title, bool isToolWindow)
    {
        Raylib.DrawRectangle(x, y, width, height, bgColor);
        Draw3DBorder(x, y, width, height, false);

        int titleHeight = isToolWindow ? 18 : 20;
        Raylib.DrawRectangle(x + 2, y + 2, width - 4, titleHeight, winBlue);
        DrawText(title, x + 4, y + 4, Color.White, true);

        // Close button
        int btnSize = titleHeight - 4;
        int btnX = x + width - 2 - btnSize - 1;
        int btnY = y + 4;
        Raylib.DrawRectangle(btnX, btnY, btnSize, btnSize, bgColor);
        Draw3DBorder(btnX, btnY, btnSize, btnSize, false);
        
        // Draw the X inside the close button
        Raylib.DrawLine(btnX + 3, btnY + 3, btnX + btnSize - 3, btnY + btnSize - 3, Color.Black);
        Raylib.DrawLine(btnX + 4, btnY + 3, btnX + btnSize - 2, btnY + btnSize - 3, Color.Black);
        Raylib.DrawLine(btnX + 3, btnY + btnSize - 3, btnX + btnSize - 3, btnY + 3, Color.Black);
        Raylib.DrawLine(btnX + 4, btnY + btnSize - 3, btnX + btnSize - 2, btnY + 3, Color.Black);
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