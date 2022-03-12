using System;
using Godot;
using ImGuiNET;

namespace DearGoTeria.scenes.DearGoTeria.WorldGeneration.Concepts
{
    public class SmoothCave : IWorldGenConcept
    {
        private readonly ImageTexture worldTexture = new ImageTexture();
        private OpenSimplexNoise noise = new TeriaSimplex(0, 6, 50, 0.5f);
        private Vector2 worldSize = new Vector2(500, 300);
        private int steps = 1000;
        private int drunkards = 1;
        private int seed = 1;
        private int stickerCount = 21;
        private float maxRadius = 2.2f;
        private bool useRadius = true;
        private float blackAndWhiteThreshold = 0.7f;

        public void Run()
        {
            if (ImGui.BeginCombo("Load Presets", ""))
            {
                if (ImGui.Selectable("Stealthy simplex caves"))
                    StealthySimplexCavesPreset();

                ImGui.EndCombo();
            }

            ImGuiSlider.Vector2("World size", ref worldSize, 100, 1500);
            ImGui.DragInt("Seed", ref seed, 1);
            ImGuiSlider.CaveSticker(
                ref drunkards, ref steps, ref stickerCount,
                ref maxRadius, ref useRadius, ref noise);         
            ImGui.SliderFloat("Black/white threshold", ref blackAndWhiteThreshold, 0, 1);

            if (ImGui.Button("Create caves") || true)
            {
                var baseImage = ImageTools.BlankImage(worldSize);
                ImageTools.SimplexNoise(noise, baseImage);
                ImageTools.BlackWhiteStep(baseImage, blackAndWhiteThreshold);

                var random = new Random(seed);

                for (var i = 0; i < stickerCount; i++)
                {
                    var sticker = ImageTools.BlankImage(new Vector2(500, 500), Extensions.BlankColour);
                    ImageTools.DrunkardWalk(
                        noise, (ulong) (seed * stickerCount + i), sticker, drunkards, steps, useRadius,
                        maxRadius, sticker.GetSize() / 2, Colors.White);
                    ImageTools.CropUnused(sticker);
                    sticker = ImageTools.ColourIslands(sticker, Extensions.BlankColour, Colors.White);
                    ImageTools.CropUnused(sticker);

                    var stickerLocation = new Vector2(
                        Extensions.RangedRandom(random, -sticker.GetWidth(), (int) worldSize.x),
                        Extensions.RangedRandom(random, -sticker.GetHeight(), (int) worldSize.y));
                    ImageTools.BlendImages(baseImage, sticker, ImageTools.Blend.Flatten, stickerLocation);
                }

                worldTexture.CreateFromImage(baseImage);
            }

            var imageHandle = ImGuiGD.BindTexture(worldTexture);
            ImGui.Image(imageHandle, Extensions.GodotVec2ToSystemVec2(worldSize));
        }

        private void StealthySimplexCavesPreset()
        {
            worldSize = new Vector2(500, 300);
            steps = 1000;
            drunkards = 1;
            stickerCount = 21;
            noise.Octaves = 6;
            noise.Period = 50;
            noise.Persistence = 0.5f;
            blackAndWhiteThreshold = 0.7f;
        }
    }
}