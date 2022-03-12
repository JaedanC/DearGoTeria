using Godot;
using ImGuiNET;

namespace DearGoTeria.scenes.DearGoTeria.WorldGeneration.Concepts
{
    public class SimplexLine : IWorldGenConcept
    {
        private readonly ImageTexture worldTexture = new ImageTexture();
        private OpenSimplexNoise noise = new TeriaSimplex(0, 5, 550, 0.5f);
        private Vector2 worldSize = new Vector2(500, 500);
        private int lineOffset = 250;
        private float lineAmplitude = 200;

        public void Run()
        {
            if (ImGui.BeginCombo("Load Presets", ""))
            {
                if (ImGui.Selectable("Terrain surface"))
                    TerrainSurfacePreset();
                
                if (ImGui.Selectable("Hell roof"))
                    HellRoofPreset();

                ImGui.EndCombo();
            }

            ImGuiSlider.Vector2("World size", ref worldSize, 100, 1500);
            ImGui.SliderInt("Line offset", ref lineOffset, 0, (int)worldSize.y);
            ImGui.SliderFloat("Line amplitude", ref lineAmplitude, 0, 1000);
            ImGuiSlider.Simplex(ref noise);

            var image = ImageTools.BlankImage(worldSize);
            ImageTools.SimplexLine(noise, image, lineAmplitude, lineOffset);
            ImGuiImage.Create(worldTexture, image);
        }

        private void TerrainSurfacePreset()
        {
            worldSize = new Vector2(500, 220);
            noise.Period = 550;
            noise.Persistence = 0.5f;
            noise.Octaves = 4;
            lineOffset = 110;
            lineAmplitude = 200;
        }
        
        private void HellRoofPreset()
        {
            worldSize = new Vector2(500, 100);
            noise.Period = 20;
            noise.Persistence = 0.5f;
            noise.Octaves = 6;
            lineOffset = 5;
            lineAmplitude = 40;
        }
    }
}