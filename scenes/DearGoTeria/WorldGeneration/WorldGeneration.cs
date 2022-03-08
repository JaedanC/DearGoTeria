using Godot;
using System;
using System.Collections.Generic;
using ImGuiNET;

public class WorldGeneration : Node, ISceneImgui
{
    private const double GroundLevelPercent = 0.2;
    private const double StoneLevelPercent = 0.45;
    private const double HellLevelPercent = 0.8;

    public struct TeriaWorld
    {
        public readonly Image Blocks;
        private readonly ImageTexture blocksTexture;
        private readonly Vector2 size;
        private readonly List<object> loot;
        public readonly int GroundLevel;
        public readonly int StoneLevel;
        public readonly int HellLevel;

        public TeriaWorld(Vector2 size)
        {
            this.size = size;
            Blocks = new Image();
            Blocks.Create((int) size.x, (int) size.y, false, Image.Format.Rgba8);
            blocksTexture = new ImageTexture();
            blocksTexture.CreateFromImage(Blocks, 0);
            loot = new List<object>();

            GroundLevel = (int) (size.y * GroundLevelPercent);
            StoneLevel = (int) (size.y * StoneLevelPercent);
            HellLevel = (int) (size.y * HellLevelPercent);
        }
    };

    private OpenSimplexNoise? noise;
    private int drunkards = 1;
    private int steps = 15;
    private Vector2 imageSize;
    private bool floodFillFilter = true;
    private int seed = 1337;
    private int imageCount = 1;
    private int canvasWidth = 1000;
    private int canvasHeight = 300;
    private List<ImageTexture>? textures;
    private ImageTexture? jobTexture;

    public override void _Ready()
    {
        base._Ready();

        noise = new OpenSimplexNoise();
        noise.Seed = 1337;
        noise.Octaves = 0;
        noise.Period = 1;
        noise.Persistence = 0.4f;

        imageSize = new Vector2(500, 500);
        textures = new List<ImageTexture>();
        for (var i = 0; i < 10; i++)
        {
            textures.Add(new ImageTexture());
        }

        jobTexture = new ImageTexture();
    }

    public void SceneImGui()
    {
        ImGui.Begin("World Generation");
        ImGui.Text("Hello world");
        ImGui.SliderInt("Drunkards", ref drunkards, 1, 20);
        ImGui.SliderInt("Steps", ref steps, 1, 5000);
        ImGui.SliderInt("Seed", ref seed, 1, 500);
        ImGui.SliderInt("Image Count", ref imageCount, 1, 10);
        ImGui.SliderInt("Canvas Width", ref canvasWidth, 100, 10000);
        ImGui.SliderInt("Canvas Height", ref canvasHeight, 10, 1000);

        ImGui.Checkbox("Flood fill filter", ref floodFillFilter);

        for (var i = 0; i < imageCount; i++)
        {
            var image = ImageTools.BlankImage(imageSize, Extensions.BlankColour);
            GenerationAlgorithms.DrunkardWalk(
                noise!, (ulong) (seed + i), image, drunkards, steps, image.GetSize() / 2, Colors.White);
            ImageTools.CropUnused(image);

            if (floodFillFilter)
            {
                image = ImageTools.AddBorder(image, 1, Extensions.BlankColour);
                ImageTools.FloodFill(image, Vector2.Zero, Colors.Blue);
                ImageTools.ChangeColour(image, Extensions.BlankColour, Colors.White);
                ImageTools.ChangeColour(image, Colors.Blue, Extensions.BlankColour);
                ImageTools.CropUnused(image);
            }

            var texture = textures![i];
            texture!.CreateFromImage(image);
            var id = ImGuiGD.BindTexture(texture!);
            if (i > 0)
                ImGui.SameLine();
            ImGui.Image(id, Extensions.GodotVec2ToSystemVec2(image.GetSize()));
        }

        {
            var defineWorldJob = new Job("Define World", WorldGenerationJobs.DefineWorld,
                new Vector2(canvasWidth, canvasHeight));
            var defineAirDirtStoneJob = new Job("Define Air/Dirt/Stone", WorldGenerationJobs.DefineAirDirtStone);

            var jobs = new JobGroup(new Dictionary<object, object[]>
            {
                {defineWorldJob, Array.Empty<object>()},
                {defineAirDirtStoneJob, new object[] {defineWorldJob}}
            });
            var jobGraph = new JobGraph(jobs);
            while (jobGraph.HasJobs())
            {
                var job = jobGraph.DequeueJob();
                if (job == null)
                    break;
                
                job.Run(jobGraph);
            }

            var world = (TeriaWorld)defineAirDirtStoneJob.Result!;
            jobTexture!.CreateFromImage(world.Blocks);
            var imageHandle = ImGuiGD.BindTexture(jobTexture);
            ImGui.Image(imageHandle, Extensions.GodotVec2ToSystemVec2(world.Blocks.GetSize()));
        }

        ImGui.End();
    }

    public static Image CreateCaveSticker(int drunkard, int steps, int seed, Vector2 maxSize)
    {
        var image = ImageTools.BlankImage(maxSize, Extensions.BlankColour);
        GenerationAlgorithms.DrunkardWalk(new OpenSimplexNoise(), (ulong) seed, image, drunkard, steps,
            image.GetSize() / 2, Colors.White);
        return image;
    }
}