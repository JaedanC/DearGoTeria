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
        public readonly ImageTexture BlocksTexture;
        private readonly Vector2 size;
        private readonly List<object> loot;
        public readonly int GroundLevel;
        public readonly int StoneLevel;
        public readonly int HellLevel;

        public TeriaWorld(Vector2 size)
        {
            this.size = size;
            Blocks = ImageTools.BlankImage(size, Colors.White);
            BlocksTexture = new ImageTexture();
            BlocksTexture.CreateFromImage(Blocks, 0);
            loot = new List<object>();

            GroundLevel = (int) (size.y * GroundLevelPercent);
            StoneLevel = (int) (size.y * StoneLevelPercent);
            HellLevel = (int) (size.y * HellLevelPercent);
        }
    };

    private readonly List<ImageTexture> textures = new List<ImageTexture>();
    
    

    public override void _Ready()
    {
        base._Ready();
    }

    public void SceneImGui()
    {
        ImGui.Begin("World Generation");
        
        if (ImGui.CollapsingHeader("Live Preview"))
        {
            DrunkardLivePreview();
        }

        if (ImGui.CollapsingHeader("Cave Generation"))
        {
            CaveGeneration();
        }

        ImGui.End();
    }
    

    
    private int liveDrunkards = 1;
    private int liveSteps = 1000;
    private int liveSeed = 1;
    private int liveStickerCount = 4;
    private bool liveUseRadius = true;
    private float liveMaxRadius = 2.2f;
    
    private readonly OpenSimplexNoise liveNoise = new OpenSimplexNoise();
    private int liveSimplexSeed = 1337;
    private int liveSimplexOctaves = 0;
    private float liveSimplexPeriod = 1;
    private float liveSimplexPersistence = 0.4f;
    
    private bool liveFloodFill = true;
    
    private void DrunkardLivePreview()
    {
        ImGui.SliderInt("Live Drunkards", ref liveDrunkards, 1, 20);
        ImGui.SliderInt("Live Steps", ref liveSteps, 1, 5000);
        ImGui.SliderInt("Live Seed", ref liveSeed, 1, 100);
        ImGui.SliderInt("Live Sticker count", ref liveStickerCount, 1, 10);
        
        ImGui.SliderFloat("Live Max radius", ref liveMaxRadius, 1, 3);

        ImGui.SliderInt("Live Simplex seed", ref liveSimplexSeed, 0, 50);
        ImGui.SliderInt("Live Simplex octaves", ref liveSimplexOctaves, 0, 9);
        ImGui.SliderFloat("Live Simplex period", ref liveSimplexPeriod, 0, 5);
        ImGui.SliderFloat("Live Simplex persistence", ref liveSimplexPersistence, 0, 5);
        
        liveNoise.Seed = liveSimplexSeed;
        liveNoise.Octaves = liveSimplexOctaves;
        liveNoise.Period = liveSimplexPeriod;
        liveNoise.Persistence = liveSimplexPersistence;
        
        ImGui.Checkbox("Live Flood fill filter", ref liveFloodFill); ImGui.SameLine();
        ImGui.Checkbox("Live Use radius", ref liveUseRadius);

        for (var i = 0; i < liveStickerCount; i++)
        {
            var image = ImageTools.BlankImage(new Vector2(500, 500), Extensions.BlankColour);
            GenerationAlgorithms.DrunkardWalk(
                liveNoise, (ulong) (liveSeed + i * liveStickerCount), image, liveDrunkards, liveSteps,
                liveUseRadius, liveMaxRadius, image.GetSize() / 2, Colors.White);
            ImageTools.CropUnused(image);

            if (liveFloodFill)
            {
                image = ImageTools.AddBorder(image, 1, Extensions.BlankColour);
                ImageTools.FloodFill(image, Vector2.Zero, Colors.Blue);
                ImageTools.ChangeColour(image, Extensions.BlankColour, Colors.White);
                ImageTools.ChangeColour(image, Colors.Blue, Extensions.BlankColour);
                ImageTools.CropUnused(image);
            }

            if (textures.Count == i)
            {
                textures.Add(new ImageTexture());
            }

            var texture = textures[i];
            texture.CreateFromImage(image);
            var id = ImGuiGD.BindTexture(texture);
            if (i > 0)
                ImGui.SameLine();
            ImGui.Image(id, Extensions.GodotVec2ToSystemVec2(image.GetSize()));
        }
    }
    
    
    private readonly ImageTexture caveWorldTexture = new ImageTexture();
    private int caveDrunkards = 1;
    private int caveSteps = 1000;
    private int caveSeed = 1;
    private int caveStickerCount = 170;
    private bool caveUseRadius = true;
    private float caveMaxRadius = 2.2f;
    
    private readonly OpenSimplexNoise caveNoise = new OpenSimplexNoise();
    private int caveSimplexSeed = 1337;
    private int caveSimplexOctaves = 0;
    private float caveSimplexPeriod = 1;
    private float caveSimplexPersistence = 0.4f;
    
    private int caveWorldWidth = 500;
    private int caveWorldHeight = 500;
    
    private void CaveGeneration()
    {
        ImGui.SliderInt("Cave Drunkards", ref caveDrunkards, 1, 20);
        ImGui.SliderInt("Cave Steps", ref caveSteps, 1, 5000);
        ImGui.SliderInt("Cave Seed", ref caveSeed, 1, 100);
        ImGui.SliderInt("Cave Sticker count", ref caveStickerCount, 1, 300);
       
        ImGui.SliderFloat("Cave Max dig radius", ref caveMaxRadius, 1, 3);

        ImGui.SliderInt("Cave Canvas width", ref caveWorldWidth, 100, 2000);
        ImGui.SliderInt("Cave Canvas height", ref caveWorldHeight, 10, 1000);
        
        ImGui.SliderInt("Cave Simplex seed", ref caveSimplexSeed, 0, 50);
        ImGui.SliderInt("Cave Simplex octaves", ref caveSimplexOctaves, 0, 9);
        ImGui.SliderFloat("Cave Simplex period", ref caveSimplexPeriod, 0, 5);
        ImGui.SliderFloat("Cave Simplex persistence", ref caveSimplexPersistence, 0, 5);
        
        caveNoise.Seed = caveSimplexSeed;
        caveNoise.Octaves = caveSimplexOctaves;
        caveNoise.Period = caveSimplexPeriod;
        caveNoise.Persistence = caveSimplexPersistence;
        
        ImGui.Checkbox("Use radius", ref caveUseRadius);
        
        if (ImGui.Button("Run Job"))
        {
            var defineWorldJob = new Job("Define World", WorldGenerationJobs.DefineWorld,
                new Vector2(caveWorldWidth, caveWorldHeight));

            var defineAirDirtStoneJob = new Job("Define Air/Dirt/Stone", WorldGenerationJobs.DefineAirDirtStone);

            var caveStickersJobs = new List<Job>();
            for (var i = 0; i < caveStickerCount; i++)
            {
                caveStickersJobs.Add(new Job("Cave sticker", WorldGenerationJobs.CreateCaveSticker, (
                    caveNoise, caveDrunkards, caveSteps, caveUseRadius, caveMaxRadius, new Vector2(500, 500))));
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

            var jobGraph = new JobGraph(jobs, null, liveSeed);

            while (jobGraph.HasJobs())
            {
                var job = jobGraph.DequeueJob();
                if (job == null)
                    break;

                job.Run(jobGraph);
            }
            

            var world = (TeriaWorld) defineAirDirtStoneJob.Result;
            caveWorldTexture.CreateFromImage(world.Blocks);
        }
        
        var imageHandle = ImGuiGD.BindTexture(caveWorldTexture);
        ImGui.Image(imageHandle, new System.Numerics.Vector2(caveWorldWidth, caveWorldHeight));
    }
}
