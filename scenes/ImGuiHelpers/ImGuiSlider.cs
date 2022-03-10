using Godot;
using ImGuiNET;

public static class ImGuiSlider
{
    public static void Vector2(string label, ref Vector2 value, int min, int max)
    {
        int[] worldSurfaceSize = {(int)value.x, (int)value.y};
        ImGui.SliderInt2(label, ref worldSurfaceSize[0], min, max);
        value.x = worldSurfaceSize[0];
        value.y = worldSurfaceSize[1];
    }
    
    public static void Simplex(ref OpenSimplexNoise noise)
    {
        var seed = noise.Seed;
        var octaves = noise.Octaves;
        var period = noise.Period;
        var persistence = noise.Persistence;
        ImGui.SliderInt("Simplex seed", ref seed, 0, 50);
        ImGui.SliderInt("Simplex octaves", ref octaves, 0, 9);
        ImGui.SliderFloat("Simplex period", ref period, 0, 100);
        ImGui.SliderFloat("Simplex persistence", ref persistence, 0, 5);
        noise.Seed = seed;
        noise.Octaves = octaves;
        noise.Period = period;
        noise.Persistence = persistence;
    }
}
