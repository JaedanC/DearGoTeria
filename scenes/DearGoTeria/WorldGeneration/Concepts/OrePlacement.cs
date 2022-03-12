using System;
using System.Collections.Generic;
using System.Numerics;
using Godot;
using ImGuiNET;
using Vector2 = Godot.Vector2;

namespace DearGoTeria.scenes.DearGoTeria.WorldGeneration.Concepts
{
    public class OrePlacement : IWorldGenConcept
    {
        private readonly ImageTexture worldTexture = new ImageTexture();
        private Vector2 worldSize = new Vector2(500, 500);
        private Image baseImage;
        private int seed = 0;
        private int oreCount = 0;
        private int desiredOreCount = 0;
        private readonly List<int> steps = new List<int>();
        private readonly List<int> drunkards = new List<int>();
        private readonly List<int> stickerCount = new List<int>();
        private readonly List<float> maxRadius = new List<float>();
        private readonly List<bool> useRadius = new List<bool>();
        private readonly List<Vector4> colour = new List<Vector4>();
        private readonly List<OpenSimplexNoise> noise = new List<OpenSimplexNoise>();

        private int stepTemplate = 10;
        private int drunkardsTemplate = 1;
        private int stickerCountTemplate = 15;
        private float maxRadiusTemplate = 2.2f;
        private bool useRadiusTemplate = true;
        private OpenSimplexNoise noiseTemplate = new TeriaSimplex();

        public OrePlacement()
        {
            baseImage = ImageTools.BlankImage(worldSize);
            AddOre();
            AddOre();
            AddOre();
            AddOre();
        }

        private void AddOre()
        {
            oreCount++;
            var random = new Random();
            steps.Add(stepTemplate);
            drunkards.Add(drunkardsTemplate);
            stickerCount.Add(stickerCountTemplate);
            maxRadius.Add(maxRadiusTemplate);
            useRadius.Add(useRadiusTemplate);
            colour.Add(new Vector4(
                (float) random.NextDouble(),
                (float) random.NextDouble(),
                (float) random.NextDouble(),
                1
            ));
            noise.Add(((TeriaSimplex)noiseTemplate).Copy());
        }

        private void RemoveOre()
        {
            if (oreCount == 0)
                return;

            oreCount--;
            steps.RemoveAt(oreCount);
            drunkards.RemoveAt(oreCount);
            stickerCount.RemoveAt(oreCount);
            maxRadius.RemoveAt(oreCount);
            useRadius.RemoveAt(oreCount);
            colour.RemoveAt(oreCount);
            noise.RemoveAt(oreCount);
        }

        public void Run()
        {
            if (ImGui.BeginCombo("Load Presets", ""))
            {
                if (ImGui.Selectable("Default ore"))
                    DefaultOre();

                ImGui.EndCombo();
            }

            ImGuiSlider.Vector2("World size", ref worldSize, 100, 1500);
            ImGui.DragInt("Seed", ref seed, 1);
            
            if (ImGui.TreeNode("Ore Template"))
            {
                ImGui.SliderInt("Steps", ref stepTemplate, 1, 5000);
                ImGui.SliderInt("Drunkards", ref drunkardsTemplate, 1, 20);
                ImGui.SliderInt("Sticker count", ref stickerCountTemplate, 1, 200);
                ImGui.SliderFloat("Max dig radius", ref maxRadiusTemplate, 1, 3);
                ImGui.Checkbox("Use radius", ref useRadiusTemplate);
                ImGuiSlider.Simplex(ref noiseTemplate);
                ImGui.TreePop();
            }

            ImGui.SliderInt("Ore count", ref desiredOreCount, 0, 100);
            while (desiredOreCount > oreCount)
                AddOre();
            while (desiredOreCount < oreCount)
                RemoveOre();

            if (ImGui.TreeNode($"Edit Ores x {oreCount}###Edit Ores"))
            {
                for (var i = 0; i < oreCount; i++)
                {
                    if (ImGui.TreeNode($"{i}. Ore"))
                    {
                        var localDrunkards = drunkards[i];
                        var localSteps = steps[i];
                        var localStickerCount = stickerCount[i];
                        var localMaxRadius = maxRadius[i];
                        var localUseRadius = useRadius[i];
                        var localNoise = noise[i];
                        
                        ImGuiSlider.CaveSticker(
                            ref localDrunkards, ref localSteps, ref localStickerCount,
                            ref localMaxRadius, ref localUseRadius, ref localNoise);
                        
                        drunkards[i] = localDrunkards;
                        steps[i] = localSteps;
                        stickerCount[i] = localStickerCount;
                        maxRadius[i] = localMaxRadius;
                        useRadius[i] = localUseRadius;
                        noise[i] = localNoise;

                        var localColour = colour[i];
                        ImGui.ColorEdit4("Ore colour", ref localColour);
                        colour[i] = localColour;

                        ImGui.TreePop();
                    }
                }
                ImGui.TreePop();
            }

            var shouldCreate = false;

            if (ImGui.Button("Randomise"))
            {
                seed = new Random().Next();
                shouldCreate = true;
            }
            
            ImGui.SameLine();

            if (ImGui.Button("Create caves") || shouldCreate)
            {
                var random = new Random(seed);
                baseImage = ImageTools.BlankImage(worldSize, Colors.White);
                for (var oreIndex = 0; oreIndex < oreCount; oreIndex++)
                {
                    for (var i = 0; i < stickerCount[oreIndex]; i++)
                    {
                        var sticker = ImageTools.BlankImage(new Vector2(500, 500));
                        ImageTools.DrunkardWalk(
                            noise[oreIndex], (ulong) random.Next(), sticker,
                            drunkards[oreIndex], steps[oreIndex], useRadius[oreIndex], maxRadius[oreIndex],
                            sticker.GetSize() / 2, Extensions.SystemVec4ToGodotColor(colour[oreIndex]));
                        ImageTools.CropUnused(sticker);
                        sticker = ImageTools.ColourIslands(
                            sticker, Extensions.BlankColour, 
                            Extensions.SystemVec4ToGodotColor(colour[oreIndex]));
                        ImageTools.CropUnused(sticker);

                        var stickerLocation = new Vector2(
                            Extensions.RangedRandom(random, -sticker.GetWidth(), (int) worldSize.x),
                            Extensions.RangedRandom(random, -sticker.GetHeight(), (int) worldSize.y));
                        ImageTools.BlendImages(baseImage, sticker, ImageTools.Blend.Flatten, stickerLocation);
                    }
                }
            }

            ImGuiImage.Create(worldTexture, baseImage, worldSize);
        }

        public void DefaultOre()
        {
        }
    }
}