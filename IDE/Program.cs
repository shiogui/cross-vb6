namespace IDE;
public static class Program
{
    public static void Main(string[] args)
    {
        var t = typeof(Hexa.NET.ImGui.ImGui);
        foreach (var m in t.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
        {
            if (m.Name.Contains("Dock") || m.Name.Contains("Viewport") || m.Name.Contains("Separator") || m.Name.Contains("DrawList"))
                Console.WriteLine(m.ReturnType.Name + " " + m.Name + "(" + string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name)) + ")");
        }
    }
}
