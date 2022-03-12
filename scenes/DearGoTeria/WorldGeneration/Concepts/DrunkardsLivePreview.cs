using System.Collections.Generic;
using Godot;
using ImGuiNET;

namespace DearGoTeria.scenes.DearGoTeria.WorldGeneration.Concepts
{
    public class DrunkardsLivePreview : IWorldGenConcept
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
            ImGui.DragInt("Seed", ref seed, 1);
            ImGuiSlider.CaveSticker(
                ref drunkards, ref steps, ref stickerCount,
                ref maxRadius, ref useRadius, ref noise);         
            ImGui.Checkbox("Flood fill filter", ref doFloodFill);

            for (var i = 0; i < stickerCount; i++)
            {
                var sticker = ImageTools.BlankImage(new Vector2(500, 500));
                ImageTools.DrunkardWalk(
                    noise, (ulong) (seed + i * stickerCount), sticker, drunkards, steps,
                    useRadius, maxRadius, sticker.GetSize() / 2, Colors.White);
                ImageTools.CropUnused(sticker);

                if (doFloodFill)
                {
                    sticker = ImageTools.ColourIslands(sticker, Extensions.BlankColour, Colors.White);
                    ImageTools.CropUnused(sticker);
                }

                if (textures.Count == i)
                {
                    textures.Add(new ImageTexture());
                }

                var texture = textures[i];
                if (i > 0)
                    ImGui.SameLine();
                ImGuiImage.Create(texture, sticker);
            }
        }
    }
}