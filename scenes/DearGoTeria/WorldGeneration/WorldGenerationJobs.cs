using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using ImGuiNET;

public class WorldGenerationJobs
{
    private static Dictionary<int, ImageTexture>? textures;
    private static readonly object TexturesLock = new object();
    private static int texturesCount = 0;

    private static int NewTexture()
    {
        lock (TexturesLock)
        {
            if (textures == null)
            {
                ImGuiLog.Info("Creating texture dictionary");
                textures = new Dictionary<int, ImageTexture>();
            }

            var currentTextureCount = texturesCount++;
            textures.Add(currentTextureCount, new ImageTexture());
            return currentTextureCount;
        }
    }

    private static ImageTexture GetTexture(int textureHandle)
    {
        Assert.True(textures!.ContainsKey(textureHandle));
        return textures[textureHandle];
    }

    public static object DefineWorld(Random _, object[]? __, object? args)
    {
        var worldSize = (Vector2) args!;
        return new WorldGeneration.TeriaWorld(worldSize);
    }

    public static object DefineAirDirtStone(Random _, object[]? results, object? __)
    {
        var teriaWorld = (WorldGeneration.TeriaWorld) results![0];
        return teriaWorld;
        Assert.GreaterThan(teriaWorld.GroundLevel, 0);
        Assert.GreaterThan(teriaWorld.StoneLevel, 0);
        Assert.GreaterThan(teriaWorld.HellLevel, 0);
        Assert.GreaterThan(teriaWorld.StoneLevel, teriaWorld.GroundLevel);
        Assert.GreaterThan(teriaWorld.HellLevel, teriaWorld.StoneLevel);

        teriaWorld.Blocks.Lock();
        for (var i = 0; i < teriaWorld.Blocks.GetWidth(); i++)
        for (var j = 0; j < teriaWorld.GroundLevel; j++)
        {
            teriaWorld.Blocks.SetPixel(i, j, Constants.Blocks.Air);
        }

        for (var i = 0; i < teriaWorld.Blocks.GetWidth(); i++)
        for (var j = teriaWorld.GroundLevel; j < teriaWorld.StoneLevel; j++)
        {
            teriaWorld.Blocks.SetPixel(i, j, Constants.Blocks.Dirt);
        }

        for (var i = 0; i < teriaWorld.Blocks.GetWidth(); i++)
        for (var j = teriaWorld.StoneLevel; j < teriaWorld.Blocks.GetHeight(); j++)
        {
            teriaWorld.Blocks.SetPixel(i, j, Constants.Blocks.Stone);
        }

        teriaWorld.Blocks.Unlock();
        return teriaWorld;
    }

    public static object CreateCaveSticker(Random random, object[]? __, object? args)
    {
        Assert.NotNull(args);
        var (noise, drunkard, steps, useRadius, maxRadius, maxSize) = (ValueTuple<OpenSimplexNoise, int, int, bool, float, Vector2>) (args!);
        var image = ImageTools.BlankImage(maxSize, Extensions.BlankColour);
        
        // Drunkard walk
        GenerationAlgorithms.DrunkardWalk(
            noise, (ulong) random.Next(), image, drunkard, steps,
            useRadius, maxRadius, image.GetSize() / 2, Colors.White);
        ImageTools.CropUnused(image);
        
        // Flood fill filter
        image = ImageTools.AddBorder(image, 1, Extensions.BlankColour);
        ImageTools.FloodFill(image, Vector2.Zero, Colors.Blue);
        ImageTools.ChangeColour(image, Extensions.BlankColour, Colors.White);
        ImageTools.ChangeColour(image, Colors.Blue, Extensions.BlankColour);
        ImageTools.CropUnused(image);
        
        return image;
    }

    public static object BlendCaveStickers(Random random, object[]? results, object? __)
    {
        var teriaWorld = (WorldGeneration.TeriaWorld) results![0];
        var caveStickers = new ArraySegment<object>(results, 1, results.Length - 1);

        foreach (Image sticker in caveStickers)
        {
            var randomX = Extensions.RangedRandom(random, -sticker.GetWidth(), teriaWorld.Blocks.GetWidth());
            var randomY = Extensions.RangedRandom(random, -sticker.GetHeight(), teriaWorld.Blocks.GetHeight());
            var stickerLocation = new Vector2(randomX, randomY);
            ImageTools.BlendImages(teriaWorld.Blocks, sticker, ImageTools.Blend.Dig, stickerLocation);
        }

        return teriaWorld;
    }
}