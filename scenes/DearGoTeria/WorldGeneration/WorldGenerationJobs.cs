using System;
using System.Collections.Generic;
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
        var teriaWorld = (WorldGeneration.TeriaWorld)results![0];
        Assert.GreaterThan(teriaWorld.GroundLevel, 0);
        Assert.GreaterThan(teriaWorld.StoneLevel, 0);
        Assert.GreaterThan(teriaWorld.HellLevel, 0);
        Assert.GreaterThan(teriaWorld.GroundLevel, teriaWorld.StoneLevel);
        Assert.GreaterThan(teriaWorld.StoneLevel, teriaWorld.HellLevel);

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
}
