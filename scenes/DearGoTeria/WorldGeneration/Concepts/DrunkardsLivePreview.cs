using System.Collections.Generic;
using Godot;
using ImGuiNET;

namespace DearGoTeria.scenes.DearGoTeria.WorldGeneration.Concepts
{
    public class DrunkardsLivePreview
    {
        private readonly List<ImageTexture> textures = new List<ImageTexture>();
        private OpenSimplexNoise noise = new TeriaSimplex();
        private int drunkards = 1;
        private int steps = 1000;
        private int seed = 1;
        private int stickerCount = 4;
        private float maxRadius = 2.2f;
        private bool doFloodFill = true;
        private bool useRadius = true;

        public void Run()
        {
            ImGui.PushID("DrunkardLivePreview");
            ImGui.SliderInt("Seed", ref seed, 1, 100);
            ImGui.SliderInt("Drunkards", ref drunkards, 1, 20);
            ImGui.SliderInt("Steps", ref steps, 1, 5000);
            ImGui.SliderInt("Sticker count", ref stickerCount, 1, 10);
            ImGui.SliderFloat("Max radius", ref maxRadius, 1, 3);
            ImGuiSlider.Simplex(ref noise);
            ImGui.Checkbox("Flood fill filter", ref doFloodFill);
            ImGui.SameLine();
            ImGui.Checkbox("Use radius", ref useRadius);
            ImGui.PopID();

            for (var i = 0; i < stickerCount; i++)
            {
                var image = ImageTools.BlankImage(new Vector2(500, 500), Extensions.BlankColour);
                GenerationAlgorithms.DrunkardWalk(
                    noise, (ulong) (seed + i * stickerCount), image, drunkards, steps,
                    useRadius, maxRadius, image.GetSize() / 2, Colors.White);
                ImageTools.CropUnused(image);

                if (doFloodFill)
                {
                    image = ImageTools.AddBorder(image, 1, Extensions.BlankColour);
                    ImageTools.FloodFill(image, Vector2.Zero, Colors.Blue);
                    ImageTools.ChangeColour(image, Extensions.BlankColour, Colors.White);
                    ImageTools.ChangeColour(image, Colors.Blue, Extensions.BlankColour);
                    ImageTools.CropUnused(image);
                }

                if (textures.Count == i)
                {
                    textures.Add(new ImageTexture());
                }

                var texture = textures[i];
                texture.CreateFromImage(image);
                var id = ImGuiGD.BindTexture(texture);
                if (i > 0)
                    ImGui.SameLine();
                ImGui.Image(id, Extensions.GodotVec2ToSystemVec2(image.GetSize()));
            }
        }
    }
}