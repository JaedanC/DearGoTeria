using System;
using System.Collections.Generic;
using Godot;
using DearGoTeria.scenes.DearGoTeria.WorldGeneration.Concepts;
using ImGuiNET;

public class WorldGeneration : Node, ISceneImgui
{
    private readonly Dictionary<string, IWorldGenConcept> concepts = 
        new Dictionary<string, IWorldGenConcept>();

    public WorldGeneration()
    {
        concepts.Add("Cave Generation", new CaveGeneration());
        concepts.Add("Drunkards Live Preview", new DrunkardsLivePreview());
        concepts.Add("Gradient Line", new GradientLine());
        concepts.Add("Simplex Line", new SimplexLine());
        concepts.Add("World Surface", new WorldSurface());
        concepts.Add("Smooth Cave", new SmoothCave());
        concepts.Add("Ore Placement", new OrePlacement());
        concepts.Add("Gradient Stickers", new GradientStickers());
    }

    public void SceneImGui()
    {
        ImGui.Begin("World Generation");
        foreach (var item in concepts)
        {
            var name = item.Key;
            var concept = item.Value;
            ImGui.PushID(name);
            
            if (ImGui.CollapsingHeader(name))
                concept.Run();
            ImGui.PopID();
        }
        ImGui.End();
    }
}