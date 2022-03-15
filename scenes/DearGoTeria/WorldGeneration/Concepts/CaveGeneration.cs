using System;
using Godot;
using ImGuiNET;

namespace DearGoTeria.scenes.DearGoTeria.WorldGeneration.Concepts
{
    public class CaveGeneration : IWorldGenConcept
    {
        private readonly ImageTexture worldTexture = new ImageTexture();
        private OpenSimplexNoise noise = new TeriaSimplex();
        private Vector2 worldSize = new Vector2(500, 500);
        private Image baseImage;
        private int steps = 1000;
        private int drunkards = 1;
        private int seed = 1;
        private int stickerCount = 170;
        private float maxRadius = 2.2f;
        private bool useRadius = true;

        public CaveGeneration()
        {
            baseImage = ImageTools.BlankImage(worldSize);
        }

        public void Run()
        {
            if (ImGui.BeginCombo("Load Presets", ""))
            {
                if (ImGui.Selectable("Default caves"))
                    DefaultCavesPreset();

                ImGui.EndCombo();
            }

            ImGuiSlider.Vector2("World size", ref worldSize, 100, 1500);
            ImGui.DragInt("Seed", ref seed, 1);
            ImGuiSlider.CaveSticker(
                ref drunkards, ref steps, ref stickerCount,
                ref maxRadius, ref useRadius, ref noise);         

            if (ImGui.Button("Create caves"))
            {
                baseImage = ImageTools.BlankImage(worldSize, Colors.White);
                var random = new Random(seed);
                for (var i = 0; i < stickerCount; i++)
                {
                    var sticker = ImageTools.BlankImage(new Vector2(500, 500));
                    ImageTools.DrunkardWalk(
                        noise, (ulong) (seed + i * stickerCount), sticker, drunkards, steps,
                        useRadius, maxRadius, sticker.GetSize() / 2, Colors.White);
                    ImageTools.CropUnused(sticker);
                    sticker = ImageTools.ColourIslands(sticker, Extensions.BlankColour, Colors.White);
                    ImageTools.CropUnused(sticker);

                    var stickerLocation = new Vector2(
                        Extensions.RangedRandom(random, -sticker.GetWidth(), (int) worldSize.x),
                        Extensions.RangedRandom(random, -sticker.GetHeight(), (int) worldSize.y));
                    ImageTools.BlendImages(baseImage, sticker, ImageTools.Blend.Dig, stickerLocation);
                }
            }

            ImGuiImage.Create(worldTexture, baseImage, worldSize);
        }

        private void DefaultCavesPreset()
        {
            worldSize = new Vector2(500, 500);
            steps = 1000;
            drunkards = 1;
            stickerCount = 170;
            noise.Octaves = 0;
            noise.Period = 1;
            noise.Persistence = 0.4f;
        }
    }
}