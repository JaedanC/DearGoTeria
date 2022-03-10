using System.Numerics;
using Godot;
using ImGuiNET;
using Vector2 = Godot.Vector2;

namespace DearGoTeria.scenes.DearGoTeria.WorldGeneration.Concepts
{
    public class WorldSurface
    {
        private readonly ImageTexture baseTexture = new ImageTexture();
        private readonly ImageTexture topTexture = new ImageTexture();
        private readonly ImageTexture blendedTexture = new ImageTexture();
        private readonly ImageTexture blackAndWhiteTexture = new ImageTexture();
        private OpenSimplexNoise noise = new TeriaSimplex(0, 4, 50, 0.5f);
        private Vector2 worldSize = new Vector2(500, 250);
        private Vector2 gradientStart = new Vector2(0, 60);
        private Vector2 gradientDirection = new Vector2(0, 90);
        private Vector4 gradientStartColour = new Vector4(0, 0, 0, 1);
        private Vector4 gradientEndColour = new Vector4(1, 1, 1, 1);
        private float blackAndWhiteThreshold = 0.5f;

        public void Run()
        {
            ImGui.PushID("WorldSurface");
            ImGuiSlider.Vector2("World size", ref worldSize, 100, 750);
            ImGuiSlider.Vector2("Gradient start", ref gradientStart, 0, (int) worldSize.x);
            ImGuiSlider.Vector2("Gradient direction", ref gradientDirection, -(int) worldSize.x,
                (int) worldSize.x);
            ImGui.ColorEdit4("Gradient starting colour", ref gradientStartColour);
            ImGui.ColorEdit4("Gradient ending colour", ref gradientEndColour);
            ImGuiSlider.Simplex(ref noise);
            ImGui.SliderFloat("Black/white threshold", ref blackAndWhiteThreshold, 0, 1);
            ImGui.PopID();

            var baseImage = ImageTools.BlankImage(worldSize, Extensions.BlankColour);
            var topImage = ImageTools.BlankImage(worldSize, Extensions.BlankColour);

            ImGui.BeginChild("WorldSurface child");

            ImageTools.Gradient(
                baseImage, gradientStart,
                gradientDirection,
                Extensions.SystemFloatVec4ToGodotColor(gradientStartColour),
                Extensions.SystemFloatVec4ToGodotColor(gradientEndColour));

            ImageTools.SimplexNoise(noise, topImage);
            ImGuiImage.Create(baseTexture, baseImage);
            ImGui.SameLine();
            ImGuiImage.Create(topTexture, topImage);

            ImageTools.BlendImages(baseImage, topImage, ImageTools.Blend.Overlay, Vector2.Zero);
            ImGuiImage.Create(blendedTexture, baseImage);
            ImGui.SameLine();

            ImageTools.BlackWhiteStep(baseImage, blackAndWhiteThreshold);
            ImGuiImage.Create(blackAndWhiteTexture, baseImage);

            ImGui.EndChild();
        }
    }
}