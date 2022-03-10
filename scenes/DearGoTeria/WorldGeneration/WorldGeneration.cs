using Godot;
using System;
using System.Collections.Generic;
using System.Numerics;
using DearGoTeria.scenes.DearGoTeria.WorldGeneration.Concepts;
using ImGuiNET;
using Vector2 = Godot.Vector2;

public class WorldGeneration : Node, ISceneImgui
{
    private const double GroundLevelPercent = 0.2;
    private const double StoneLevelPercent = 0.45;
    private const double HellLevelPercent = 0.8;

    public struct TeriaWorld
    {
        public readonly Image Blocks;
        public readonly ImageTexture BlocksTexture;
        private readonly Vector2 size;
        private readonly List<object> loot;
        public readonly int GroundLevel;
        public readonly int StoneLevel;
        public readonly int HellLevel;

        public TeriaWorld(Vector2 size)
        {
            this.size = size;
            Blocks = ImageTools.BlankImage(size, Colors.White);
            BlocksTexture = new ImageTexture();
            BlocksTexture.CreateFromImage(Blocks, 0);
            loot = new List<object>();

            GroundLevel = (int) (size.y * GroundLevelPercent);
            StoneLevel = (int) (size.y * StoneLevelPercent);
            HellLevel = (int) (size.y * HellLevelPercent);
        }
    }

    private readonly CaveGeneration caveGeneration = new CaveGeneration();
    private readonly DrunkardsLivePreview drunkardsLivePreview = new DrunkardsLivePreview();
    private readonly GradientLine gradientLine = new GradientLine();
    private readonly SimplexLine simplexLine = new SimplexLine();
    private readonly SimplexNoise simplexNoise = new SimplexNoise();
    private readonly WorldSurface worldSurface = new WorldSurface();
    
    public void SceneImGui()
    {
        ImGui.Begin("World Generation");

        if (ImGui.CollapsingHeader("Drunkard live preview"))
            drunkardsLivePreview.Run();

        if (ImGui.CollapsingHeader("Cave generation"))
            caveGeneration.Run();

        if (ImGui.CollapsingHeader("Simplex line"))
            simplexLine.Run();

        if (ImGui.CollapsingHeader("Simplex noise"))
            simplexNoise.Run();

        if (ImGui.CollapsingHeader("Gradient line"))
            gradientLine.Run();

        if (ImGui.CollapsingHeader("World surface"))
            worldSurface.Run();

        ImGui.End();
    }
}