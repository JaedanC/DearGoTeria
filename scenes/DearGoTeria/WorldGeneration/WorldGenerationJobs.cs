using System;
using System.Collections.Generic;
using Godot;

public class WorldGenerationJobs
{
    public static object DefineAirDirtStone(Random _, List<object> results, object? __)
    {
        return 0;
        // var teriaWorld = (WorldGeneration.TeriaWorld) results![0];
        // return teriaWorld;
        // Assert.GreaterThan(teriaWorld.GroundLevel, 0);
        // Assert.GreaterThan(teriaWorld.StoneLevel, 0);
        // Assert.GreaterThan(teriaWorld.HellLevel, 0);
        // Assert.GreaterThan(teriaWorld.StoneLevel, teriaWorld.GroundLevel);
        // Assert.GreaterThan(teriaWorld.HellLevel, teriaWorld.StoneLevel);
        //
        // teriaWorld.Blocks.Lock();
        // for (var i = 0; i < teriaWorld.Blocks.GetWidth(); i++)
        // for (var j = 0; j < teriaWorld.GroundLevel; j++)
        // {
        //     teriaWorld.Blocks.SetPixel(i, j, Constants.Blocks.Air);
        // }
        //
        // for (var i = 0; i < teriaWorld.Blocks.GetWidth(); i++)
        // for (var j = teriaWorld.GroundLevel; j < teriaWorld.StoneLevel; j++)
        // {
        //     teriaWorld.Blocks.SetPixel(i, j, Constants.Blocks.Dirt);
        // }
        //
        // for (var i = 0; i < teriaWorld.Blocks.GetWidth(); i++)
        // for (var j = teriaWorld.StoneLevel; j < teriaWorld.Blocks.GetHeight(); j++)
        // {
        //     teriaWorld.Blocks.SetPixel(i, j, Constants.Blocks.Stone);
        // }
        //
        // teriaWorld.Blocks.Unlock();
        // return teriaWorld;
    }
}