using Godot;
using ImGuiNET;

namespace DearGoTeria.scenes.DearGoTeria.WorldGeneration.Concepts
{
    public class SimplexLine
    {
        private readonly ImageTexture worldTexture = new ImageTexture();
        private OpenSimplexNoise noise = new TeriaSimplex();
        private Vector2 worldSize = new Vector2(500, 500);
        private int lineOffset = 250;
        private float lineAmplitude = 100;

        public void Run()
        {
            ImGui.PushID("SimplexLine");
            ImGuiSlider.Vector2("World size", ref worldSize, 100, 1000);
            ImGui.SliderInt("Line offset", ref lineOffset, 0, 1000);
            ImGui.SliderFloat("Line amplitude", ref lineAmplitude, 0, 1000);
            ImGuiSlider.Simplex(ref noise);
            ImGui.PopID();

            var image = ImageTools.BlankImage(worldSize, Extensions.BlankColour);
            ImageTools.SimplexLine(noise, image, lineAmplitude, lineOffset);
            ImGuiImage.Create(worldTexture, image);
        }

    }
}