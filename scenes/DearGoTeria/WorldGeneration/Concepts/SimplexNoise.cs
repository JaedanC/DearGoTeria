using Godot;
using ImGuiNET;

namespace DearGoTeria.scenes.DearGoTeria.WorldGeneration.Concepts
{
    public class SimplexNoise
    {
        private readonly ImageTexture worldTexture = new ImageTexture();
        private OpenSimplexNoise noise = new TeriaSimplex();
        private Vector2 worldSize = new Vector2(500, 500);

        public void Run()
        {
            ImGui.PushID("SimplexNoise");
            ImGuiSlider.Vector2("World size", ref worldSize, 100, 1000);
            ImGuiSlider.Simplex(ref noise);
            ImGui.PopID();

            var image = ImageTools.BlankImage(worldSize, Extensions.BlankColour);
            ImageTools.SimplexNoise(noise, image);
            ImGuiImage.Create(worldTexture, image);
        }
    }
}