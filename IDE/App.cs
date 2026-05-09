
namespace IDE;

using Hexa.NET.ImGui;
using Hexa.NET.ImGui.Backends.SDL3;
using System.Numerics;

public unsafe class App : Application
{
    private Window _window = null!;
    private bottlenoselabs.Interop.SDL.SDL_Renderer* _renderer;
    private TextureManager _tex = null!;

    // toolbar/toolbox icons
    private nint _addProject, _addForm, _menuBar, _open, _save;
    private nint _cut, _copy, _paste, _find, _undo, _redo;
    private nint _play, _pause, _stop;
    private nint _projExplorer, _properties, _formLayout, _objBrowser, _toolbox, _dataView, _visualComp;
    private nint _cursor, _picture, _label, _textbox, _groupbox, _button, _checkbox, _radio;
    private nint _combo, _listbox, _hscroll, _vscroll, _timer, _drives, _directories;
    private nint _files, _shape, _line, _image, _data, _ole;
    private nint _viewCode, _viewObject, _folder, _project, _projFolder, _form;

    protected override void OnStart()
    {
        var ctx = ImGui.CreateContext();
        ImGui.SetCurrentContext(ctx);
        var io = ImGui.GetIO();
        io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;

        var fontPath = System.IO.Path.Combine(AppContext.BaseDirectory, "Resources", "MS-Sans-Serif.ttf");
        if (System.IO.File.Exists(fontPath))
            io.Fonts.AddFontFromFileTTF(fontPath, 13f);

        ApplyClassicTheme();

        _window = CreateWindow(new WindowOptions { Title = "Visual Basic", Width = 1280, Height = 800 });
        _renderer = SDL.SDL_CreateRenderer((bottlenoselabs.Interop.SDL.SDL_Window*)(void*)_window.Handle, null);
        _tex = new TextureManager(_renderer);

        ImGuiImplSDL3.SetCurrentContext(ImGui.GetCurrentContext());
        var wp = new SDLWindowPtr((Hexa.NET.ImGui.Backends.SDL3.SDLWindow*)(void*)_window.Handle);
        var rp = new SDLRendererPtr((Hexa.NET.ImGui.Backends.SDL3.SDLRenderer*)(void*)_renderer);
        ImGuiImplSDL3.InitForSDLRenderer(wp, rp);
        ImGuiImplSDL3.SDLRenderer3Init(rp);

        string i(string n) => System.IO.Path.Combine(AppContext.BaseDirectory, "Icons", n);
        _addProject = _tex.GetTexture(i("addproject.png"));
        _addForm    = _tex.GetTexture(i("addform.png"));
        _menuBar    = _tex.GetTexture(i("menubar.png"));
        _open       = _tex.GetTexture(i("open.png"));
        _save       = _tex.GetTexture(i("save.png"));
        _cut        = _tex.GetTexture(i("cut.png"));
        _copy       = _tex.GetTexture(i("copy.png"));
        _paste      = _tex.GetTexture(i("paste.png"));
        _find       = _tex.GetTexture(i("find.png"));
        _undo       = _tex.GetTexture(i("undo.png"));
        _redo       = _tex.GetTexture(i("redo.png"));
        _play       = _tex.GetTexture(i("play.png"));
        _pause      = _tex.GetTexture(i("pause.png"));
        _stop       = _tex.GetTexture(i("stop.png"));
        _projExplorer = _tex.GetTexture(i("projectexplorer.png"));
        _properties   = _tex.GetTexture(i("properties.png"));
        _formLayout   = _tex.GetTexture(i("formlayout.png"));
        _objBrowser   = _tex.GetTexture(i("objectbrowser.png"));
        _toolbox      = _tex.GetTexture(i("toolbox.png"));
        _dataView     = _tex.GetTexture(i("dataview.png"));
        _visualComp   = _tex.GetTexture(i("visualcomponent.png"));
        _cursor       = _tex.GetTexture(i("cursor.png"));
        _picture      = _tex.GetTexture(i("picture.png"));
        _label        = _tex.GetTexture(i("label.png"));
        _textbox      = _tex.GetTexture(i("textbox.png"));
        _groupbox     = _tex.GetTexture(i("groupbox.png"));
        _button       = _tex.GetTexture(i("button.png"));
        _checkbox     = _tex.GetTexture(i("checkbox.png"));
        _radio        = _tex.GetTexture(i("radio.png"));
        _combo        = _tex.GetTexture(i("combo.png"));
        _listbox      = _tex.GetTexture(i("listbox.png"));
        _hscroll      = _tex.GetTexture(i("hscroll.png"));
        _vscroll      = _tex.GetTexture(i("vscroll.png"));
        _timer        = _tex.GetTexture(i("timer.png"));
        _drives       = _tex.GetTexture(i("drives.png"));
        _directories  = _tex.GetTexture(i("directories.png"));
        _files        = _tex.GetTexture(i("files.png"));
        _shape        = _tex.GetTexture(i("shape.png"));
        _line         = _tex.GetTexture(i("line.png"));
        _image        = _tex.GetTexture(i("image.png"));
        _data         = _tex.GetTexture(i("data.png"));
        _ole          = _tex.GetTexture(i("ole.png"));
        _viewCode     = _tex.GetTexture(i("viewcode.png"));
        _viewObject   = _tex.GetTexture(i("viewobject.png"));
        _folder       = _tex.GetTexture(i("folder.png"));
        _project      = _tex.GetTexture(i("project.png"));
        _projFolder   = _tex.GetTexture(i("project_folder.png"));
        _form         = _tex.GetTexture(i("form.png"));
    }

    protected override void OnExit()
    {
        _tex.Dispose();
        ImGuiImplSDL3.SDLRenderer3Shutdown();
        ImGuiImplSDL3.Shutdown();
        ImGui.DestroyContext();
        SDL.SDL_DestroyRenderer(_renderer);
    }

    protected override void OnUpdate(TimeSpan dt) { }

    protected override void OnDraw(TimeSpan dt)
    {
        var rp = new SDLRendererPtr((Hexa.NET.ImGui.Backends.SDL3.SDLRenderer*)(void*)_renderer);
        ImGuiImplSDL3.SDLRenderer3NewFrame();
        ImGuiImplSDL3.NewFrame();
        ImGui.NewFrame();

        var vp   = ImGui.GetMainViewport();
        float sw = vp.Size.X;
        float sh = vp.Size.Y;

        const float MenuH    = 20f;
        const float ToolbarH = 30f;
        const float ToolboxW = 92f;
        const float RightW   = 220f;
        float topOffset = MenuH + ToolbarH;
        float workH     = sh - topOffset;

        DrawMenuBar(sw);
        DrawToolbar(MenuH, sw, ToolbarH);
        DrawToolbox(topOffset, ToolboxW, workH);
        DrawRightPanels(sw, topOffset, RightW, workH);
        DrawMdiArea(ToolboxW, topOffset, sw - ToolboxW - RightW, workH);

        ImGui.Render();
        SDL.SDL_SetRenderDrawColor(_renderer, 212, 208, 200, 255);
        SDL.SDL_RenderClear(_renderer);
        ImGuiImplSDL3.SDLRenderer3RenderDrawData(ImGui.GetDrawData(), rp);
        SDL.SDL_RenderPresent(_renderer);
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    static ImGuiWindowFlags PanelFlags =>
        ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize |
        ImGuiWindowFlags.NoMove     | ImGuiWindowFlags.NoScrollbar |
        ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoSavedSettings;

    // Draw a 16x16 icon inline (no button frame).
    static void Icon(string id, nint t)
    {
        if (t != 0)
            ImGui.Image(new ImTextureRef((ImTextureData*)(void*)t), new Vector2(16, 16));
        else
            ImGui.Dummy(new Vector2(16, 16));
    }

    // 22x22 toolbar icon button (1px frame padding each side = 16px icon + 3*2px padding).
    static void IconBtn(string id, nint t, Vector2 size = default)
    {
        if (size == default) size = new Vector2(22, 22);
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(3, 3));
        if (t != 0)
            ImGui.ImageButton(id, new ImTextureRef((ImTextureData*)(void*)t), new Vector2(16, 16));
        else
            ImGui.InvisibleButton(id, size); // keep spacing but show nothing
        ImGui.PopStyleVar();
    }

    // ── Menu bar ─────────────────────────────────────────────────────────

    static void DrawMenuBar(float sw)
    {
        // ImGui.SetNextWindowPos(Vector2.Zero);
        // ImGui.SetNextWindowSize(new Vector2(sw, 20));
        // ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(4, 2));
        // ImGui.Begin("##menubar", PanelFlags | ImGuiWindowFlags.MenuBar);
        // ImGui.PopStyleVar();
  
        if (ImGui.BeginMainMenuBar())
        {
            foreach (var m in new[]{"File","Edit","View","Project","Format","Debug","Run","Query","Diagram","Tools","Add-Ins","Window","Help"})
            {
                if (ImGui.BeginMenu(m))
                {
                    ImGui.EndMenu();
                }
            }

            ImGui.EndMainMenuBar();
        }
    }

    // ── Toolbar ──────────────────────────────────────────────────────────

    void DrawToolbar(float y, float sw, float h)
    {
        ImGui.SetNextWindowPos(new Vector2(0, y));
        ImGui.SetNextWindowSize(new Vector2(sw, h));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(3, 3));
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(1, 1));
        ImGui.Begin("##toolbar", PanelFlags);
        ImGui.PopStyleVar(2);

        int sepN = 0;
        // Groups separated by vertical lines
        void TBtn(string id, nint t) { IconBtn(id, t); ImGui.SameLine(); }
        void TSep() {
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0, 4));
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.5f,0.5f,0.5f,1));
            ImGui.Button($"##sep{sepN++}", new Vector2(2, 22));
            ImGui.PopStyleColor(); ImGui.PopStyleVar();
            ImGui.SameLine();
        }

        TBtn("##ap",_addProject); TBtn("##af",_addForm); TBtn("##mb",_menuBar); TSep();
        TBtn("##op",_open);       TBtn("##sv",_save);    TSep();
        TBtn("##ct",_cut); TBtn("##cp",_copy); TBtn("##ps",_paste); TBtn("##fn",_find); TSep();
        TBtn("##un",_undo);       TBtn("##rd",_redo);    TSep();
        TBtn("##pl",_play); TBtn("##pa",_pause); TBtn("##st",_stop); TSep();
        TBtn("##pe",_projExplorer); TBtn("##pr",_properties); TBtn("##fl",_formLayout);
        TBtn("##ob",_objBrowser);   TBtn("##tb",_toolbox);     TBtn("##dv",_dataView); IconBtn("##vc",_visualComp);

        ImGui.End();
    }

    // ── Toolbox ──────────────────────────────────────────────────────────

    void DrawToolbox(float y, float w, float h)
    {
        ImGui.SetNextWindowPos(new Vector2(0, y));
        ImGui.SetNextWindowSize(new Vector2(w, h));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(5, 4));
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(2, 2));
        ImGui.Begin("##toolbox", PanelFlags);
        ImGui.PopStyleVar(2);

        // "General" header label — dark background like win95 group header
        var dl = ImGui.GetWindowDrawList();
        var p  = ImGui.GetCursorScreenPos();
        dl.AddRectFilled(p, new Vector2(p.X + w, p.Y + 18), 0xFFD4D0C8);
        dl.AddRect(p, new Vector2(p.X + w - 1, p.Y + 17), 0xFF808080);
        ImGui.TextUnformatted("General");

        nint[] icons = {
            _cursor,_picture,_label,_textbox,_groupbox,
            _button,_checkbox,_radio,_combo,_listbox,
            _hscroll,_vscroll,_timer,_drives,_directories,
            _files,_shape,_line,_image,_data,_ole
        };

        // 2-column icon grid, 24x24 cells
        for (int idx = 0; idx < icons.Length; idx++)
        {
            if (idx % 2 != 0) ImGui.SameLine();
            bool selected = idx == 0;
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(3, 3));
            ImGui.PushStyleColor(ImGuiCol.Button,
                selected ? new Vector4(0.78f,0.78f,0.78f,1) : new Vector4(0.831f,0.815f,0.784f,1));
            if (icons[idx] != 0)
                ImGui.ImageButton($"##tb{idx}", new ImTextureRef((ImTextureData*)(void*)icons[idx]), new Vector2(16,16));
            else
                ImGui.InvisibleButton($"##tb{idx}", new Vector2(22,22));
            ImGui.PopStyleColor();
            ImGui.PopStyleVar();
        }
        ImGui.End();
    }

    // ── Right panels ─────────────────────────────────────────────────────

    void DrawRightPanels(float sw, float topY, float w, float workH)
    {
        float x = sw - w;
        float projH = workH / 3f;
        float propH = workH / 2f;
        float layH  = workH - projH - propH;

        // Project Explorer
        ImGui.SetNextWindowPos(new Vector2(x, topY));
        ImGui.SetNextWindowSize(new Vector2(w, projH));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(4, 4));
        ImGui.Begin("Project - Project1", PanelFlags & ~ImGuiWindowFlags.NoTitleBar);
        ImGui.PopStyleVar();
        IconBtn("##vc2",_viewCode); ImGui.SameLine();
        IconBtn("##vo2",_viewObject); ImGui.SameLine();
        IconBtn("##fld",_folder);
        ImGui.Separator();
        ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(1,1,1,1));
        ImGui.BeginChild("##ptree", new Vector2(-1, -1), ImGuiChildFlags.Borders);
        ImGui.PopStyleColor();
        Icon("##pi",_project); ImGui.SameLine(); ImGui.TextUnformatted("Project1 (Project1.vbp)");
        ImGui.Indent();
        Icon("##pf",_projFolder); ImGui.SameLine(); ImGui.TextUnformatted("Forms");
        ImGui.Indent();
        Icon("##frm",_form); ImGui.SameLine(); ImGui.TextUnformatted("Form1 (Form1.frm)");
        ImGui.Unindent(); ImGui.Unindent();
        ImGui.EndChild();
        ImGui.End();

        // Properties
        ImGui.SetNextWindowPos(new Vector2(x, topY + projH));
        ImGui.SetNextWindowSize(new Vector2(w, propH));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(4, 4));
        ImGui.Begin("Properties - Form1", PanelFlags & ~ImGuiWindowFlags.NoTitleBar);
        ImGui.PopStyleVar();
        ImGui.SetNextItemWidth(-1);
        ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(1,1,1,1));
        string combo = "Form1  Form"; ImGui.InputText("##obj", ref combo, 64, ImGuiInputTextFlags.ReadOnly);
        ImGui.PopStyleColor();
        if (ImGui.BeginTabBar("##proptabs"))
        {
            if (ImGui.BeginTabItem("Alphabetic")) ImGui.EndTabItem();
            if (ImGui.BeginTabItem("Categorized")) ImGui.EndTabItem();
            ImGui.EndTabBar();
        }
        ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(1,1,1,1));
        ImGui.BeginChild("##propgrid", new Vector2(-1, -40), ImGuiChildFlags.Borders);
        ImGui.PopStyleColor();
        if (ImGui.BeginTable("##pt", 2, ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY))
        {
            ImGui.TableSetupColumn("Property", ImGuiTableColumnFlags.WidthFixed, w/2f - 8f);
            ImGui.TableSetupColumn("Value");
            string[][] rows = {
                new[]{"(Name)","Form1"}, new[]{"Appearance","1 - 3D"},
                new[]{"AutoRedraw","False"}, new[]{"BackColor","&H8000000F&"},
                new[]{"BorderStyle","2 - Sizable"}, new[]{"Caption","Form1"},
                new[]{"ClipControls","True"}, new[]{"ControlBox","True"},
                new[]{"DrawMode","13 - Copy Pen"}, new[]{"DrawStyle","0 - Solid"},
                new[]{"DrawWidth","1"}, new[]{"Enabled","True"},
                new[]{"FillColor","&H00000000&"}, new[]{"FillStyle","1 - Transparent"},
            };
            foreach (var row in rows)
            {
                ImGui.TableNextRow(); ImGui.TableSetColumnIndex(0); ImGui.TextUnformatted(row[0]);
                ImGui.TableSetColumnIndex(1); ImGui.TextUnformatted(row[1]);
            }
            ImGui.EndTable();
        }
        ImGui.EndChild();
        ImGui.Separator();
        ImGui.TextUnformatted("Caption"); ImGui.TextDisabled("Returns/sets the text displayed");
        ImGui.End();

        // Form Layout
        ImGui.SetNextWindowPos(new Vector2(x, topY + projH + propH));
        ImGui.SetNextWindowSize(new Vector2(w, layH));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(4, 4));
        ImGui.Begin("Form Layout", PanelFlags & ~ImGuiWindowFlags.NoTitleBar);
        ImGui.PopStyleVar();
        ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(1,1,1,1));
        ImGui.BeginChild("##flchild", new Vector2(-1,-1), ImGuiChildFlags.Borders);
        ImGui.PopStyleColor();
        var dl   = ImGui.GetWindowDrawList();
        var pos  = ImGui.GetCursorScreenPos();
        var avail = ImGui.GetContentRegionAvail();
        float mx = pos.X + avail.X/2f - 30f, my = pos.Y + avail.Y/2f - 25f;
        dl.AddRectFilled(new Vector2(mx,my), new Vector2(mx+60,my+45), 0xFFD4D0C8);
        dl.AddRect(new Vector2(mx,my), new Vector2(mx+60,my+45), 0xFF808080);
        dl.AddRectFilled(new Vector2(mx+4,my+4), new Vector2(mx+56,my+41), 0xFF000000);
        dl.AddRectFilled(new Vector2(mx+10,my+10), new Vector2(mx+30,my+25), 0xFFD4D0C8);
        dl.AddRectFilled(new Vector2(mx+10,my+10), new Vector2(mx+30,my+14), 0xFF800000);
        ImGui.Dummy(avail);
        ImGui.EndChild();
        ImGui.End();
    }

    // ── MDI area ─────────────────────────────────────────────────────────

    static void DrawMdiArea(float x, float y, float w, float h)
    {
        ImGui.SetNextWindowPos(new Vector2(x, y));
        ImGui.SetNextWindowSize(new Vector2(w, h));
        ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.5f,0.5f,0.5f,1f));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        ImGui.Begin("##mdi", PanelFlags);
        ImGui.PopStyleVar(); ImGui.PopStyleColor();

        // Form window inside MDI
        ImGui.SetNextWindowPos(new Vector2(x + 20, y + 20), ImGuiCond.Once);
        ImGui.SetNextWindowSize(new Vector2(500, 380), ImGuiCond.Once);
        ImGui.PushStyleColor(ImGuiCol.TitleBg,       new Vector4(0,0,0.5f,1));
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive, new Vector4(0,0,0.5f,1));
        ImGui.PushStyleColor(ImGuiCol.WindowBg,      new Vector4(0.831f,0.815f,0.784f,1));
        ImGui.Begin("Project1 - Form1 (Form)", ImGuiWindowFlags.NoSavedSettings);
        ImGui.PopStyleColor(3);

        var dl  = ImGui.GetWindowDrawList();
        var p   = ImGui.GetCursorScreenPos();
        var sz  = ImGui.GetContentRegionAvail();

        // Grid dots
        for (float gx = 0; gx < sz.X; gx += 8f)
            for (float gy = 0; gy < sz.Y; gy += 8f)
                dl.AddCircleFilled(new Vector2(p.X + gx, p.Y + gy), 0.7f, 0xFF000000, 4);

        // Dummy button control
        ImGui.SetCursorPos(new Vector2(50, 50));
        ImGui.Button("Command1", new Vector2(100, 30));

        // Dummy textbox
        ImGui.SetCursorPos(new Vector2(50, 100));
        ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(1,1,1,1));
        string t1 = "Text1"; ImGui.InputText("##t1", ref t1, 32, ImGuiInputTextFlags.ReadOnly);
        ImGui.PopStyleColor();

        ImGui.End();
        ImGui.End(); // MDI host
    }

    // ── Classic Win95 theme ───────────────────────────────────────────────

    static void ApplyClassicTheme()
    {
        var s = ImGui.GetStyle();
        var c = s.Colors;
        var bg   = new Vector4(0.831f,0.815f,0.784f,1);
        var navy = new Vector4(0,0,0.502f,1);
        var dark = new Vector4(0.502f,0.502f,0.502f,1);
        var blk  = new Vector4(0,0,0,1);
        var wht  = new Vector4(1,1,1,1);

        c[(int)ImGuiCol.Text]                 = blk;
        c[(int)ImGuiCol.TextDisabled]         = dark;
        c[(int)ImGuiCol.WindowBg]             = bg;
        c[(int)ImGuiCol.ChildBg]              = bg;
        c[(int)ImGuiCol.PopupBg]              = bg;
        c[(int)ImGuiCol.Border]               = new Vector4(0.4f,0.4f,0.4f,1);
        c[(int)ImGuiCol.BorderShadow]         = wht;
        c[(int)ImGuiCol.FrameBg]              = wht;
        c[(int)ImGuiCol.FrameBgHovered]       = wht;
        c[(int)ImGuiCol.FrameBgActive]        = bg;
        c[(int)ImGuiCol.TitleBg]              = navy;
        c[(int)ImGuiCol.TitleBgActive]        = navy;
        c[(int)ImGuiCol.TitleBgCollapsed]     = navy;
        c[(int)ImGuiCol.MenuBarBg]            = bg;
        c[(int)ImGuiCol.ScrollbarBg]          = bg;
        c[(int)ImGuiCol.ScrollbarGrab]        = new Vector4(0.75f,0.75f,0.75f,1);
        c[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.85f,0.85f,0.85f,1);
        c[(int)ImGuiCol.ScrollbarGrabActive]  = dark;
        c[(int)ImGuiCol.CheckMark]            = blk;
        c[(int)ImGuiCol.Button]               = bg;
        c[(int)ImGuiCol.ButtonHovered]        = new Vector4(0.88f,0.88f,0.85f,1);
        c[(int)ImGuiCol.ButtonActive]         = new Vector4(0.70f,0.70f,0.68f,1);
        c[(int)ImGuiCol.Header]               = navy;
        c[(int)ImGuiCol.HeaderHovered]        = new Vector4(0.1f,0.1f,0.6f,1);
        c[(int)ImGuiCol.HeaderActive]         = navy;
        c[(int)ImGuiCol.Tab]                  = bg;
        c[(int)ImGuiCol.TabHovered]           = new Vector4(0.88f,0.88f,0.85f,1);
        c[(int)ImGuiCol.TabSelected]          = bg;
        c[(int)ImGuiCol.Separator]            = dark;
        c[(int)ImGuiCol.TextSelectedBg]       = new Vector4(0,0,0.5f,0.5f);

        s.WindowRounding   = 0; s.ChildRounding  = 0; s.FrameRounding  = 0;
        s.PopupRounding    = 0; s.TabRounding    = 0; s.GrabRounding   = 0;
        s.WindowBorderSize = 1; s.FrameBorderSize = 1;
        s.ItemSpacing      = new Vector2(4,3);
        s.WindowPadding    = new Vector2(4,4);
        s.FramePadding     = new Vector2(3,2);
    }

    protected override void OnMouseMove(in MouseMoveEvent e) { }
    protected override void OnMouseDown(in MouseButtonEvent e) { }
    protected override void OnMouseUp(in MouseButtonEvent e) { }
    protected override void OnKeyDown(in KeyboardEvent e) { }
    protected override void OnKeyUp(in KeyboardEvent e) { }
}