using System.Numerics;
using Godot;
using ImGuiNET;
using Vector2 = Godot.Vector2;

namespace DearGoTeria.scenes.DearGoTeria.WorldGeneration.Concepts
{
    public class GradientLine
    {
        private readonly ImageTexture worldTexture = new ImageTexture();
        private Vector2 worldSize = new Vector2(500, 500);
        private Vector2 gradientStart = new Vector2(50, 50);
        private Vector2 gradientDirection = new Vector2(400, 400);
        private Vector4 gradientStartColour = new Vector4(0, 0, 0, 1);
        private Vector4 gradientEndColour = new Vector4(1, 1, 1, 1);

        public void Run()
        {
            ImGui.PushID("GradientLine");
            ImGuiSlider.Vector2("World size", ref worldSize, 100, 1000);
            ImGuiSlider.Vector2("Gradient start", ref gradientStart, 0, (int) worldSize.x);
            ImGuiSlider.Vector2("Gradient direction", ref gradientDirection, 0, 1000);
            ImGui.ColorEdit4("Starting colour", ref gradientStartColour);
            ImGui.ColorEdit4("Ending colour", ref gradientEndColour);
            ImGui.PopID();
        
            var image = ImageTools.BlankImage(worldSize, Extensions.BlankColour);
            ImageTools.Gradient(
                image,
                gradientStart,
                gradientDirection,
                Extensions.SystemFloatVec4ToGodotColor(gradientStartColour),
                Extensions.SystemFloatVec4ToGodotColor(gradientEndColour));
            ImGuiImage.Create(worldTexture, image);
        }
    }
}