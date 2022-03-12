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
        ImGui.DragInt("Simplex seed", ref seed, 1);
        ImGui.SliderInt("Simplex octaves", ref octaves, 1, 9);
        ImGui.SliderFloat("Simplex period", ref period, 1, 500);
        ImGui.SliderFloat("Simplex persistence", ref persistence, 0, 5);
        noise.Seed = seed;
        noise.Octaves = octaves;
        noise.Period = period;
        noise.Persistence = persistence;
    }

    public static void CaveSticker(
        ref int drunkards, ref int steps, ref int stickerCount, ref float maxDigRadius,
        ref bool useRadius, ref OpenSimplexNoise noise)
    {
        ImGui.SliderInt("Drunkards", ref drunkards, 1, 20);
        ImGui.SliderInt("Steps", ref steps, 1, 5000);
        ImGui.SliderInt("Sticker count", ref stickerCount, 1, 300);
        ImGui.SliderFloat("Max dig radius", ref maxDigRadius, 1, 3);
        Simplex(ref noise);
        ImGui.Checkbox("Use radius", ref useRadius);
    }
}
