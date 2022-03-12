using System;
using System.Collections.Generic;
using System.Numerics;
using Godot;
using ImGuiNET;
using Vector2 = Godot.Vector2;

namespace DearGoTeria.scenes.DearGoTeria.WorldGeneration.Concepts
{
    public class GradientStickers : IWorldGenConcept
    {
        private readonly ImageTexture worldTexture = new ImageTexture();
        private readonly ImageTexture gradientTexture = new ImageTexture();
        private OpenSimplexNoise noise = new TeriaSimplex();
        private Vector2 worldSize = new Vector2(350, 350);
        private Image baseImage;
        private int steps = 10;
        private int drunkards = 1;
        private int seed = 1;
        private int stickerCount = 50;
        private float maxRadius = 2.2f;
        private bool useRadius = true;
        private Image gradientImage;
        private Vector2 gradientStart = new Vector2(50, 50);
        private Vector2 gradientDirection = new Vector2(400, 400);
        private Vector4 gradientStartColour = new Vector4(0, 0, 0, 1);
        private Vector4 gradientEndColour = new Vector4(1, 1, 1, 1);

        public GradientStickers()
        {
            baseImage = ImageTools.BlankImage(worldSize);
        }

        public void Run()
        {
            if (ImGui.BeginCombo("Load Presets", ""))
            {
                if (ImGui.Selectable("Default caves"))
                    SafePlacement();

                ImGui.EndCombo();
            }

            ImGuiSlider.Vector2("World size", ref worldSize, 100, 1500);
            ImGui.DragInt("Seed", ref seed, 1);
            ImGuiSlider.CaveSticker(
                ref drunkards, ref steps, ref stickerCount,
                ref maxRadius, ref useRadius, ref noise);            
            ImGuiSlider.Vector2("Gradient start", ref gradientStart, 0, (int) worldSize.x);
            ImGuiSlider.Vector2("Gradient direction", ref gradientDirection, -1000, 1000);
            ImGui.ColorEdit4("Starting colour", ref gradientStartColour);
            ImGui.ColorEdit4("Ending colour", ref gradientEndColour);

            gradientImage = ImageTools.BlankImage(worldSize);
            ImageTools.Gradient(
                gradientImage, gradientStart, gradientDirection,
                Extensions.SystemVec4ToGodotColor(gradientStartColour),
                Extensions.SystemVec4ToGodotColor(gradientEndColour));

            var shouldCreate = false;
            if (ImGui.Button("Randomise"))
            {
                seed = new Random().Next();
                shouldCreate = true;
            }
            
            ImGui.SameLine();
            
            if (ImGui.Button("Create caves") || shouldCreate)
            {
                baseImage = ImageTools.BlankImage(worldSize, Colors.White);

                var stickers = new List<Image>();
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
                    stickers.Add(sticker);
                }

                ImageTools.GradientPlaceStickers(random, baseImage, gradientImage, stickers);
            }

            ImGuiImage.Create(gradientTexture, gradientImage, worldSize);
            ImGui.SameLine();
            ImGuiImage.Create(worldTexture, baseImage, worldSize);
        }

        private void SafePlacement()
        {
        }
    }
}