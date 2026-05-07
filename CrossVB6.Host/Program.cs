using System.Reflection;
using Raylib_cs;

namespace CrossVB6.Host;

public static class Program
{
    private static bool _shouldReload;
    private static DateTime _lastReload = DateTime.MinValue;
    private static Action? _updateDelegate;
    private static FileSystemWatcher? _watcher;
    private static string _assemblyPath;
    
    private const string ProjectName = "CrossVB6";
    private const string AssemblyPath = $"{ProjectName}.dll";

    private static void LogError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Host App Error: {0}", message);
        Console.ResetColor();
    }

    private static void StartWatcher()
    {
        _watcher = new FileSystemWatcher(_assemblyPath);
        _watcher.Filter = AssemblyPath;
        _watcher.NotifyFilter = NotifyFilters.LastWrite;
        _watcher.Changed += (_, _) =>
        {
            if (DateTime.UtcNow - _lastReload >= TimeSpan.FromMilliseconds(500)) return;
            _shouldReload = true;
            _lastReload = DateTime.UtcNow;
        };
        _watcher.EnableRaisingEvents = true;
    }
    
    private static void LoadLibrary()
    {
        var fileData = File.ReadAllBytes($"{_assemblyPath}{AssemblyPath}");
        var assembly = Assembly.Load(fileData);
       
        var type = assembly.GetType($"{ProjectName}.IDE");
        if (type is null)
        {
            LogError("IDE type not found.");
            Environment.Exit(1);
        }

        var instance = Activator.CreateInstance(type);
        if (instance is null)
        {
            LogError("Failed to instantiate the IDE.");
            Environment.Exit(1);
        }
        
        var method = type.GetMethod(
            "UpdateAndDraw",
            BindingFlags.Instance | BindingFlags.NonPublic
        );
        if (method is null)
        {
            LogError("Failed to get the UpdateAndDraw method.");
            Environment.Exit(1);
        }
        
        _updateDelegate = (Action)Delegate.CreateDelegate(typeof(Action), instance, method);
        
        var now = TimeProvider.System.GetUtcNow();
        Console.WriteLine("{0} Code updated, ready to be reloaded", now.ToUnixTimeMilliseconds());
    }
    
    public static void Main()
    {
        _assemblyPath = AppDomain.CurrentDomain.BaseDirectory;
        
        StartWatcher();
        
        Raylib.InitWindow(1024, 768, "CrossVB6 - Raylib");
        Raylib.SetTargetFPS(60);
        
        LoadLibrary();
        
        while (!Raylib.WindowShouldClose())
        {
            if (_shouldReload)
            {
                Thread.Sleep(100);
                LoadLibrary();
                _shouldReload = false;
            }
            
            Raylib.BeginDrawing();
            _updateDelegate?.Invoke();
            Raylib.EndDrawing();
        }
        
        Raylib.CloseWindow();
    }
}