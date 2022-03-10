using System;
using System.Collections.Generic;
using Godot;
using ImGuiNET;

namespace DearGoTeria.scenes.DearGoTeria.WorldGeneration.Concepts
{
    public class CaveGeneration
    {
        private readonly ImageTexture worldTexture = new ImageTexture();
        private OpenSimplexNoise noise = new TeriaSimplex();
        private Vector2 worldSize = new Vector2(500, 500);
        private int steps = 1000;
        private int drunkards = 1;
        private int seed = 1;
        private int stickerCount = 170;
        private float maxRadius = 2.2f;
        private bool useRadius = true;

        public void Run()
        {
            ImGui.PushID("CaveGeneration");
            ImGuiSlider.Vector2("World size", ref worldSize, 100, 1000);
            ImGui.SliderInt("Seed", ref seed, 1, 100);
            ImGui.SliderInt("Drunkards", ref drunkards, 1, 20);
            ImGui.SliderInt("Steps", ref steps, 1, 5000);
            ImGui.SliderInt("Sticker count", ref stickerCount, 1, 300);
            ImGui.SliderFloat("Max dig radius", ref maxRadius, 1, 3);
            ImGuiSlider.Simplex(ref noise);
            ImGui.Checkbox("Use radius", ref useRadius);
            ImGui.PopID();

            if (ImGui.Button("Create caves"))
            {
                var defineWorldJob = new Job("Define World", WorldGenerationJobs.DefineWorld, worldSize);

                var defineAirDirtStoneJob = new Job("Define Air/Dirt/Stone", WorldGenerationJobs.DefineAirDirtStone);

                var caveStickersJobs = new List<Job>();
                for (var i = 0; i < stickerCount; i++)
                {
                    caveStickersJobs.Add(new Job("Cave sticker", WorldGenerationJobs.CreateCaveSticker, (
                        noise, drunkards, steps, useRadius, maxRadius, new Vector2(500, 500))));
                }

                var blendCavesJob = new Job("Blend Caves", WorldGenerationJobs.BlendCaveStickers);
                var blendDependencies = new List<object> {defineAirDirtStoneJob};
                blendDependencies.AddRange(caveStickersJobs);

                var jobs = new JobGroup(new Dictionary<object, object[]>
                {
                    {defineWorldJob, Array.Empty<object>()},
                    {defineAirDirtStoneJob, new object[] {defineWorldJob}},
                    {caveStickersJobs.ToArray(), new object[] {defineWorldJob}},
                    {blendCavesJob, blendDependencies.ToArray()}
                });

                var jobGraph = new JobGraph(jobs, null, seed);

                while (jobGraph.HasJobs())
                {
                    var job = jobGraph.DequeueJob();
                    if (job == null)
                        break;

                    job.Run(jobGraph);
                }


                var world = (global::WorldGeneration.TeriaWorld) defineAirDirtStoneJob.Result;
                worldTexture.CreateFromImage(world.Blocks);
            }

            var imageHandle = ImGuiGD.BindTexture(worldTexture);
            ImGui.Image(imageHandle, Extensions.GodotVec2ToSystemVec2(worldSize));
        }
    }
}