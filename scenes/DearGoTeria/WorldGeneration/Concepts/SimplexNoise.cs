using Godot;

namespace DearGoTeria.scenes.DearGoTeria.WorldGeneration.Concepts
{
    public class SimplexNoise : IWorldGenConcept
    {
        private readonly ImageTexture worldTexture = new ImageTexture();
        private OpenSimplexNoise noise = new TeriaSimplex();
        private Vector2 worldSize = new Vector2(500, 500);

        public void Run()
        {
            ImGuiSlider.Vector2("World size", ref worldSize, 100, 1500);
            ImGuiSlider.Simplex(ref noise);

            var image = ImageTools.BlankImage(worldSize);
            ImageTools.SimplexNoise(noise, image);
            ImGuiImage.Create(worldTexture, image);
        }
    }
}