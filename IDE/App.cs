
namespace IDE;

using Hexa.NET.ImGui;
using Hexa.NET.ImGui.Backends.SDL3;
using System.Numerics;

public unsafe class App : Application
{
    private Window _window = null!;
    private bottlenoselabs.Interop.SDL.SDL_Renderer* _renderer;
    private TextureManager _textureManager = null!;
    private bool _layoutInitialized = false;

    // toolbar icon handles
    private nint _texAddProject, _texAddForm, _texMenuBar;
    private nint _texOpen, _texSave;
    private nint _texCut, _texCopy, _texPaste, _texFind;
    private nint _texUndo, _texRedo;
    private nint _texPlay, _texPause, _texStop;
    private nint _texProjectExplorer, _texProperties, _texFormLayout;
    private nint _texObjectBrowser, _texToolbox;

    protected override void OnStart()
    {
        // 1. Initialize ImGui context FIRST
        var ctx = ImGui.CreateContext();
        ImGui.SetCurrentContext(ctx);

        ImGuiIOPtr io = ImGui.GetIO();
        io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard
                        | ImGuiConfigFlags.DockingEnable;

        // Load MS Sans Serif font
        var fontPath = System.IO.Path.Combine(AppContext.BaseDirectory, "Resources", "MS-Sans-Serif.ttf");
        if (System.IO.File.Exists(fontPath))
            io.Fonts.AddFontFromFileTTF(fontPath, 13.0f);

        SetClassicTheme();

        // 2. Create window
        _window = CreateWindow(new WindowOptions
        {
            Title = "CrossVB6 IDE",
            Width = 1280,
            Height = 800
        });

        // 3. Create renderer
        _renderer = SDL.SDL_CreateRenderer(
            (bottlenoselabs.Interop.SDL.SDL_Window*)(void*)_window.Handle, null);

        _textureManager = new TextureManager(_renderer);

        // 4. Sync ImGui context with backend
        ImGuiImplSDL3.SetCurrentContext(ImGui.GetCurrentContext());

        var winPtr = new SDLWindowPtr((Hexa.NET.ImGui.Backends.SDL3.SDLWindow*)(void*)_window.Handle);
        var renPtr = new SDLRendererPtr((Hexa.NET.ImGui.Backends.SDL3.SDLRenderer*)(void*)_renderer);
        ImGuiImplSDL3.InitForSDLRenderer(winPtr, renPtr);
        ImGuiImplSDL3.SDLRenderer3Init(renPtr);

        // 5. Pre-load toolbar icons (paths relative to exe output dir)
        string ico(string name) => System.IO.Path.Combine(AppContext.BaseDirectory, "Icons", name);
        _texAddProject      = _textureManager.GetTexture(ico("addproject.gif"));
        _texAddForm         = _textureManager.GetTexture(ico("addform.gif"));
        _texMenuBar         = _textureManager.GetTexture(ico("menubar.gif"));
        _texOpen            = _textureManager.GetTexture(ico("open.gif"));
        _texSave            = _textureManager.GetTexture(ico("save.gif"));
        _texCut             = _textureManager.GetTexture(ico("cut.gif"));
        _texCopy            = _textureManager.GetTexture(ico("copy.gif"));
        _texPaste           = _textureManager.GetTexture(ico("paste.gif"));
        _texFind            = _textureManager.GetTexture(ico("find.gif"));
        _texUndo            = _textureManager.GetTexture(ico("undo.gif"));
        _texRedo            = _textureManager.GetTexture(ico("redo.gif"));
        _texPlay            = _textureManager.GetTexture(ico("play.gif"));
        _texPause           = _textureManager.GetTexture(ico("pause.gif"));
        _texStop            = _textureManager.GetTexture(ico("stop.gif"));
        _texProjectExplorer = _textureManager.GetTexture(ico("projectexplorer.gif"));
        _texProperties      = _textureManager.GetTexture(ico("properties.gif"));
        _texFormLayout      = _textureManager.GetTexture(ico("formlayout.gif"));
        _texObjectBrowser   = _textureManager.GetTexture(ico("objectbrowser.gif"));
        _texToolbox         = _textureManager.GetTexture(ico("toolbox.gif"));
    }

    protected override void OnExit()
    {
        _textureManager.Dispose();
        ImGuiImplSDL3.SDLRenderer3Shutdown();
        ImGuiImplSDL3.Shutdown();
        ImGui.DestroyContext();
        SDL.SDL_DestroyRenderer(_renderer);
    }

    protected override void OnUpdate(TimeSpan deltaTime) { }

    protected override void OnDraw(TimeSpan deltaTime)
    {
        var renPtr = new SDLRendererPtr((Hexa.NET.ImGui.Backends.SDL3.SDLRenderer*)(void*)_renderer);

        ImGuiImplSDL3.SDLRenderer3NewFrame();
        ImGuiImplSDL3.NewFrame();
        ImGui.NewFrame();

        SetupDockspace();
        DrawMenuBar();
        DrawToolbar();
        DrawToolWindows();

        ImGui.Render();

        SDL.SDL_SetRenderDrawColor(_renderer, 212, 208, 200, 255);
        SDL.SDL_RenderClear(_renderer);
        ImGuiImplSDL3.SDLRenderer3RenderDrawData(ImGui.GetDrawData(), renPtr);
        SDL.SDL_RenderPresent(_renderer);
    }

    // ──────────────────────────────────────────────────────────────────────
    //  Dockspace + initial layout
    // ──────────────────────────────────────────────────────────────────────
    private void SetupDockspace()
    {
        var viewport = ImGui.GetMainViewport();
        float menuH    = ImGui.GetFrameHeight();
        float toolbarH = 30f;
        float topOffset = menuH + toolbarH;

        ImGui.SetNextWindowPos(new Vector2(viewport->Pos.X, viewport->Pos.Y + topOffset));
        ImGui.SetNextWindowSize(new Vector2(viewport->Size.X, viewport->Size.Y - topOffset));
        ImGui.SetNextWindowViewport(viewport->ID);

        var hostFlags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse
                      | ImGuiWindowFlags.NoResize   | ImGuiWindowFlags.NoMove
                      | ImGuiWindowFlags.NoBringToFrontOnFocus
                      | ImGuiWindowFlags.NoNavFocus  | ImGuiWindowFlags.NoBackground;

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0f);
        ImGui.Begin("DockHost", hostFlags);
        ImGui.PopStyleVar(2);

        uint dockId = ImGui.GetID("MainDockspace");
        ImGui.DockSpace(dockId, new Vector2(0, 0), ImGuiDockNodeFlags.PassthruCentralNode);

        if (!_layoutInitialized)
        {
            _layoutInitialized = true;
            BuildInitialLayout(dockId, viewport->Size, topOffset);
        }

        ImGui.End();
    }

    private static void BuildInitialLayout(uint dockId, Vector2 size, float topOffset)
    {
        ImGui.DockBuilderRemoveNode(dockId);
        ImGui.DockBuilderAddNode(dockId, ImGuiDockNodeFlags.DockSpace);
        ImGui.DockBuilderSetNodeSize(dockId, new Vector2(size.X, size.Y - topOffset));

        // Split: left toolbox | centre | right panels
        uint nodeLeft, nodeCenter;
        ImGui.DockBuilderSplitNode(dockId, ImGuiDir.Left, 0.14f, out nodeLeft, out nodeCenter);

        uint nodeRight, nodeWork;
        ImGui.DockBuilderSplitNode(nodeCenter, ImGuiDir.Right, 0.22f, out nodeRight, out nodeWork);

        // Right column: project explorer | properties | form layout
        uint nodeRightTop, nodeRightMid, nodeRightBot;
        ImGui.DockBuilderSplitNode(nodeRight, ImGuiDir.Up, 0.35f, out nodeRightTop, out nodeRightMid);
        ImGui.DockBuilderSplitNode(nodeRightMid, ImGuiDir.Down, 0.45f, out nodeRightMid, out nodeRightBot);

        ImGui.DockBuilderDockWindow("Toolbox",              nodeLeft);
        ImGui.DockBuilderDockWindow("Project - Project1",   nodeRightTop);
        ImGui.DockBuilderDockWindow("Properties - Form1",   nodeRightMid);
        ImGui.DockBuilderDockWindow("Form Layout",          nodeRightBot);

        ImGui.DockBuilderFinish(dockId);
    }

    // ──────────────────────────────────────────────────────────────────────
    //  Menu bar
    // ──────────────────────────────────────────────────────────────────────
    private static void DrawMenuBar()
    {
        if (!ImGui.BeginMainMenuBar()) return;

        if (ImGui.BeginMenu("File"))
        {
            ImGui.MenuItem("New Project");
            ImGui.MenuItem("Open Project...");
            ImGui.Separator();
            ImGui.MenuItem("Save Project");
            ImGui.MenuItem("Save Project As...");
            ImGui.Separator();
            ImGui.MenuItem("Print...");
            ImGui.Separator();
            ImGui.MenuItem("Make Project...");
            ImGui.Separator();
            ImGui.MenuItem("Exit");
            ImGui.EndMenu();
        }
        if (ImGui.BeginMenu("Edit"))
        {
            ImGui.MenuItem("Undo");  ImGui.MenuItem("Redo"); ImGui.Separator();
            ImGui.MenuItem("Cut");   ImGui.MenuItem("Copy"); ImGui.MenuItem("Paste");
            ImGui.Separator();
            ImGui.MenuItem("Find..."); ImGui.MenuItem("Replace...");
            ImGui.EndMenu();
        }
        if (ImGui.BeginMenu("View"))
        {
            ImGui.MenuItem("Code");   ImGui.MenuItem("Object"); ImGui.Separator();
            ImGui.MenuItem("Immediate Window"); ImGui.MenuItem("Locals Window");
            ImGui.MenuItem("Watch Window"); ImGui.Separator();
            ImGui.MenuItem("Project Explorer"); ImGui.MenuItem("Properties Window");
            ImGui.MenuItem("Form Layout Window"); ImGui.Separator();
            ImGui.MenuItem("Toolbox");
            ImGui.EndMenu();
        }
        if (ImGui.BeginMenu("Project"))
        {
            ImGui.MenuItem("Add Form"); ImGui.MenuItem("Add Module");
            ImGui.MenuItem("Add Class Module"); ImGui.Separator();
            ImGui.MenuItem("References..."); ImGui.MenuItem("Components...");
            ImGui.Separator(); ImGui.MenuItem("Properties...");
            ImGui.EndMenu();
        }
        if (ImGui.BeginMenu("Format"))   { ImGui.MenuItem("Align"); ImGui.EndMenu(); }
        if (ImGui.BeginMenu("Debug"))
        {
            ImGui.MenuItem("Step Into"); ImGui.MenuItem("Step Over"); ImGui.MenuItem("Step Out");
            ImGui.Separator();
            ImGui.MenuItem("Toggle Breakpoint"); ImGui.MenuItem("Clear All Breakpoints");
            ImGui.EndMenu();
        }
        if (ImGui.BeginMenu("Run"))
        {
            ImGui.MenuItem("Start"); ImGui.MenuItem("Break"); ImGui.MenuItem("End");
            ImGui.MenuItem("Restart");
            ImGui.EndMenu();
        }
        if (ImGui.BeginMenu("Tools"))
        {
            ImGui.MenuItem("Menu Editor..."); ImGui.Separator(); ImGui.MenuItem("Options...");
            ImGui.EndMenu();
        }
        if (ImGui.BeginMenu("Add-Ins"))
        {
            ImGui.MenuItem("Add-In Manager...");
            ImGui.EndMenu();
        }
        if (ImGui.BeginMenu("Window"))
        {
            ImGui.MenuItem("Tile Horizontally"); ImGui.MenuItem("Tile Vertically");
            ImGui.MenuItem("Cascade");
            ImGui.EndMenu();
        }
        if (ImGui.BeginMenu("Help"))
        {
            ImGui.MenuItem("Contents..."); ImGui.MenuItem("Index...");
            ImGui.Separator(); ImGui.MenuItem("About Visual Basic...");
            ImGui.EndMenu();
        }

        ImGui.EndMainMenuBar();
    }

    // ──────────────────────────────────────────────────────────────────────
    //  Standard toolbar — pinned just under the menu bar
    // ──────────────────────────────────────────────────────────────────────
    private void DrawToolbar()
    {
        var viewport = ImGui.GetMainViewport();
        float menuH = ImGui.GetFrameHeight();

        ImGui.SetNextWindowPos(new Vector2(viewport->Pos.X, viewport->Pos.Y + menuH));
        ImGui.SetNextWindowSize(new Vector2(viewport->Size.X, 30f));

        var flags = ImGuiWindowFlags.NoTitleBar    | ImGuiWindowFlags.NoResize
                  | ImGuiWindowFlags.NoMove        | ImGuiWindowFlags.NoScrollbar
                  | ImGuiWindowFlags.NoScrollWithMouse
                  | ImGuiWindowFlags.NoBringToFrontOnFocus
                  | ImGuiWindowFlags.NoSavedSettings;

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(4, 4));
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing,   new Vector2(2, 2));
        ImGui.Begin("##toolbar", flags);
        ImGui.PopStyleVar(2);

        // Group 1: Project/Form
        Btn("##addProject", _texAddProject); ImGui.SameLine();
        Btn("##addForm",    _texAddForm);    ImGui.SameLine();
        Btn("##menuBar",    _texMenuBar);    ImGui.SameLine();
        TSep(); ImGui.SameLine();

        // Group 2: File
        Btn("##open",  _texOpen);  ImGui.SameLine();
        Btn("##save",  _texSave);  ImGui.SameLine();
        TSep(); ImGui.SameLine();

        // Group 3: Edit
        Btn("##cut",   _texCut);   ImGui.SameLine();
        Btn("##copy",  _texCopy);  ImGui.SameLine();
        Btn("##paste", _texPaste); ImGui.SameLine();
        Btn("##find",  _texFind);  ImGui.SameLine();
        TSep(); ImGui.SameLine();

        // Group 4: Undo/Redo
        Btn("##undo",  _texUndo);  ImGui.SameLine();
        Btn("##redo",  _texRedo);  ImGui.SameLine();
        TSep(); ImGui.SameLine();

        // Group 5: Run
        Btn("##play",  _texPlay);  ImGui.SameLine();
        Btn("##pause", _texPause); ImGui.SameLine();
        Btn("##stop",  _texStop);  ImGui.SameLine();
        TSep(); ImGui.SameLine();

        // Group 6: View tools
        Btn("##projExp", _texProjectExplorer); ImGui.SameLine();
        Btn("##props",   _texProperties);      ImGui.SameLine();
        Btn("##formLay", _texFormLayout);      ImGui.SameLine();
        Btn("##objBrow", _texObjectBrowser);   ImGui.SameLine();
        Btn("##toolbox", _texToolbox);

        ImGui.End();
    }

    /// Renders a small 16×16 toolbar icon button.
    private static void Btn(string id, nint tex)
    {
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(2, 2));
        if (tex != 0)
        {
            var texRef = new ImTextureRef((ImTextureData*)(void*)tex);
            ImGui.ImageButton(id, texRef, new Vector2(16, 16));
        }
        else
        {
            // invisible placeholder so spacing stays consistent
            ImGui.InvisibleButton(id, new Vector2(20, 20));
        }
        ImGui.PopStyleVar();
    }

    /// Vertical separator line inside toolbar.
    private static void TSep()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0, 0));
        ImGui.SeparatorEx(ImGuiSeparatorFlags.Vertical);
        ImGui.PopStyleVar();
    }

    // ──────────────────────────────────────────────────────────────────────
    //  Tool windows
    // ──────────────────────────────────────────────────────────────────────
    private static void DrawToolWindows()
    {
        // Toolbox
        ImGui.Begin("Toolbox");
        ImGui.TextDisabled("(General)");
        ImGui.Separator();
        string[] tools = { "Pointer", "PictureBox", "Label", "TextBox", "Frame",
                           "CommandButton", "CheckBox", "OptionButton", "ComboBox",
                           "ListBox", "HScrollBar", "VScrollBar", "Timer",
                           "DriveListBox", "DirListBox", "FileListBox", "Shape",
                           "Line", "Image", "Data", "OLE" };
        foreach (var t in tools)
        {
            ImGui.Selectable(t);
        }
        ImGui.End();

        // Project Explorer
        ImGui.Begin("Project - Project1");
        ImGui.SetNextItemOpen(true, ImGuiCond.Once);
        if (ImGui.TreeNode("Project1 (Project1.vbp)"))
        {
            ImGui.SetNextItemOpen(true, ImGuiCond.Once);
            if (ImGui.TreeNode("Forms"))
            {
                ImGui.Selectable("Form1 (Form1.frm)");
                ImGui.TreePop();
            }
            ImGui.TreePop();
        }
        ImGui.End();

        // Properties
        ImGui.Begin("Properties - Form1");
        ImGui.TextDisabled("Form1  Form");
        ImGui.Separator();
        if (ImGui.BeginTable("##props", 2, ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.RowBg))
        {
            (string, string)[] props = {
                ("(Name)",       "Form1"),
                ("BackColor",    "&H8000000F&"),
                ("BorderStyle",  "2 - Sizable"),
                ("Caption",      "Form1"),
                ("Height",       "3600"),
                ("Left",         "0"),
                ("StartUpPos",   "3 - Windows Default"),
                ("Top",          "0"),
                ("Width",        "4800"),
            };
            foreach (var (k, v) in props)
            {
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0); ImGui.TextUnformatted(k);
                ImGui.TableSetColumnIndex(1); ImGui.TextUnformatted(v);
            }
            ImGui.EndTable();
        }
        ImGui.End();

        // Form Layout
        ImGui.Begin("Form Layout");
        ImGui.TextDisabled("Form1");
        var avail = ImGui.GetContentRegionAvail();
        var canvas = new Vector2(avail.X - 4, avail.Y - 4);
        if (canvas.X > 10 && canvas.Y > 10)
        {
            var p = ImGui.GetCursorScreenPos();
            var dl = ImGui.GetWindowDrawList();
            // Desktop background
            dl->AddRectFilled(p, new Vector2(p.X + canvas.X, p.Y + canvas.Y),
                0xFF808080);
            // Form representation (small rectangle)
            float fx = p.X + canvas.X * 0.15f;
            float fy = p.Y + canvas.Y * 0.15f;
            float fw = canvas.X * 0.55f;
            float fh = canvas.Y * 0.55f;
            dl->AddRectFilled(new Vector2(fx, fy), new Vector2(fx + fw, fy + fh), 0xFFD4D0C8);
            dl->AddRect(new Vector2(fx, fy), new Vector2(fx + fw, fy + fh), 0xFF000080);
            // Title bar
            dl->AddRectFilled(new Vector2(fx, fy), new Vector2(fx + fw, fy + 12), 0xFF800000);
            ImGui.Dummy(canvas);
        }
        ImGui.End();
    }

    // ──────────────────────────────────────────────────────────────────────
    //  Classic Win95 theme
    // ──────────────────────────────────────────────────────────────────────
    private static void SetClassicTheme()
    {
        var style = ImGui.GetStyle();
        var c     = style.Colors;

        // Base colors
        var bg      = new Vector4(0.831f, 0.815f, 0.784f, 1f); // #D4D0C8
        var dark    = new Vector4(0.502f, 0.502f, 0.502f, 1f); // #808080
        var darker  = new Vector4(0.251f, 0.251f, 0.251f, 1f); // #404040
        var light   = new Vector4(1f,     1f,     1f,     1f);
        var titleBg = new Vector4(0.000f, 0.000f, 0.502f, 1f); // #000080
        var sel     = new Vector4(0.000f, 0.000f, 0.502f, 1f);
        var text    = new Vector4(0f, 0f, 0f, 1f);
        var white   = new Vector4(1f, 1f, 1f, 1f);

        c[(int)ImGuiCol.Text]                 = text;
        c[(int)ImGuiCol.TextDisabled]         = dark;
        c[(int)ImGuiCol.WindowBg]             = bg;
        c[(int)ImGuiCol.ChildBg]              = bg;
        c[(int)ImGuiCol.PopupBg]              = bg;
        c[(int)ImGuiCol.Border]               = darker;
        c[(int)ImGuiCol.BorderShadow]         = light;
        c[(int)ImGuiCol.FrameBg]              = light;
        c[(int)ImGuiCol.FrameBgHovered]       = light;
        c[(int)ImGuiCol.FrameBgActive]        = bg;
        c[(int)ImGuiCol.TitleBg]              = titleBg;
        c[(int)ImGuiCol.TitleBgActive]        = titleBg;
        c[(int)ImGuiCol.TitleBgCollapsed]     = titleBg;
        c[(int)ImGuiCol.MenuBarBg]            = bg;
        c[(int)ImGuiCol.ScrollbarBg]          = bg;
        c[(int)ImGuiCol.ScrollbarGrab]        = new Vector4(0.75f, 0.75f, 0.75f, 1f);
        c[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.85f, 0.85f, 0.85f, 1f);
        c[(int)ImGuiCol.ScrollbarGrabActive]  = dark;
        c[(int)ImGuiCol.CheckMark]            = text;
        c[(int)ImGuiCol.Button]               = bg;
        c[(int)ImGuiCol.ButtonHovered]        = new Vector4(0.88f, 0.88f, 0.85f, 1f);
        c[(int)ImGuiCol.ButtonActive]         = new Vector4(0.70f, 0.70f, 0.68f, 1f);
        c[(int)ImGuiCol.Header]               = sel;
        c[(int)ImGuiCol.HeaderHovered]        = new Vector4(0.10f, 0.10f, 0.60f, 1f);
        c[(int)ImGuiCol.HeaderActive]         = sel;
        c[(int)ImGuiCol.Separator]            = dark;
        c[(int)ImGuiCol.SeparatorHovered]     = dark;
        c[(int)ImGuiCol.SeparatorActive]      = text;
        c[(int)ImGuiCol.ResizeGrip]           = bg;
        c[(int)ImGuiCol.ResizeGripHovered]    = dark;
        c[(int)ImGuiCol.ResizeGripActive]     = darker;
        c[(int)ImGuiCol.Tab]                  = bg;
        c[(int)ImGuiCol.TabHovered]           = new Vector4(0.88f, 0.88f, 0.85f, 1f);
        c[(int)ImGuiCol.TabSelected]          = bg;
        c[(int)ImGuiCol.DockingPreview]       = new Vector4(0.0f, 0.0f, 0.5f, 0.5f);
        c[(int)ImGuiCol.DockingEmptyBg]       = bg;
        c[(int)ImGuiCol.TextSelectedBg]       = new Vector4(0.0f, 0.0f, 0.5f, 0.5f);

        style.WindowRounding    = 0f;
        style.ChildRounding     = 0f;
        style.FrameRounding     = 0f;
        style.PopupRounding     = 0f;
        style.ScrollbarRounding = 0f;
        style.GrabRounding      = 0f;
        style.TabRounding       = 0f;
        style.WindowBorderSize  = 1f;
        style.FrameBorderSize   = 1f;
        style.ItemSpacing       = new Vector2(4, 3);
        style.WindowPadding     = new Vector2(4, 4);
        style.FramePadding      = new Vector2(4, 2);
    }

    protected override void OnMouseMove(in MouseMoveEvent e) { }
    protected override void OnMouseDown(in MouseButtonEvent e) { }
    protected override void OnMouseUp(in MouseButtonEvent e) { }
    protected override void OnKeyDown(in KeyboardEvent e) { }
    protected override void OnKeyUp(in KeyboardEvent e) { }
}