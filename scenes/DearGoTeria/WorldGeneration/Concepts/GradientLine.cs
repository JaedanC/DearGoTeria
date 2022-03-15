using System;
using System.Collections.Generic;
using System.Numerics;
using Godot;
using ImGuiNET;
using Vector2 = Godot.Vector2;

namespace DearGoTeria.scenes.DearGoTeria.WorldGeneration.Concepts
{
    public class GradientLine : IWorldGenConcept
    {
        private readonly ImageTexture worldTexture = new ImageTexture();
        private Vector2 worldSize = new Vector2(500, 500);
        private int colourCount = 2;
        private Vector2 gradientStart = new Vector2(50, 50);
        private Vector2 gradientEnd = new Vector2(400, 400);
        private Vector4 gradientStartColour = new Vector4(0, 0, 0, 1);
        private Vector4 gradientEndColour = new Vector4(1, 1, 1, 1);
        private readonly List<Color> colourGaps = new List<Color>();
        private readonly List<float> floatGaps = new List<float>();
        
        public void Run()
        {
            ImGuiSlider.Vector2("World size", ref worldSize, 100, 1500);
            ImGuiSlider.Vector2("Gradient start", ref gradientStart, 0, (int) worldSize.x);
            ImGuiSlider.Vector2("Gradient end", ref gradientEnd, -1000, 1000);

            ImGui.SliderInt("Colour count", ref colourCount, 2, 10);

            var random = new Random();
            var recalculateFloats = false;
            while (colourCount - 2 > colourGaps.Count)
            {
                colourGaps.Add(new Color(
                    (float) random.NextDouble(),
                    (float) random.NextDouble(),
                    (float) random.NextDouble()));
                floatGaps.Add(1);
                recalculateFloats = true;
            }

            while (colourCount - 2 < colourGaps.Count)
            {
                colourGaps.RemoveAt(colourGaps.Count - 1);
                floatGaps.RemoveAt(floatGaps.Count - 1);
                recalculateFloats = true;
            }

            if (recalculateFloats)
            {
                for (var i = 0; i < colourCount - 2; i++)
                {
                    var value = (1.0f / (colourCount - 1)) * (i + 1);
                    floatGaps[i] = value;
                }
            }

            if (ImGui.TreeNode("Edit Colours"))
            {
                ImGui.ColorEdit4("Starting colour", ref gradientStartColour);

                for (var i = 0; i < colourCount - 2; i++)
                {
                    float previousFloat;
                    float nextFloat;

                    if (i == 0)
                        previousFloat = 0;
                    else
                        previousFloat = floatGaps[i - 1];

                    if (i == colourCount - 3)
                        nextFloat = 1;
                    else
                        nextFloat = floatGaps[i + 1];

                    var colour = Extensions.GodotColorToSystemVec4(colourGaps[i]);
                    ImGui.ColorEdit4($"Middle colour: {i + 1}", ref colour);
                    colourGaps[i] = Extensions.SystemVec4ToGodotColor(colour);

                    var gap = floatGaps[i];
                    ImGui.SliderFloat($"Float: {i + 1}", ref gap, previousFloat, nextFloat);
                    gap = Mathf.Clamp(gap, previousFloat, nextFloat);
                    floatGaps[i] = gap;
                }

                ImGui.ColorEdit4("Ending colour", ref gradientEndColour);
                ImGui.TreePop();
            }

            var colourFloatList = new List<ValueTuple<Color, float>>();
            colourFloatList.Add((Extensions.SystemVec4ToGodotColor(gradientStartColour), 0));
            for (var i = 0; i < colourCount - 2; i++)
            {
                colourFloatList.Add((colourGaps[i], floatGaps[i]));
            }

            colourFloatList.Add((Extensions.SystemVec4ToGodotColor(gradientEndColour), 1));

            var image = ImageTools.BlankImage(worldSize);
            ImageTools.Gradient(
                image,
                gradientStart,
                gradientEnd,
                colourFloatList);
            ImGuiImage.Create(worldTexture, image);
        }
    }
}