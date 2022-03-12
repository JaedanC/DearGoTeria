using System;
using System.Numerics;
using Godot;
using Vector2 = System.Numerics.Vector2;

///
/// Provides helper methods/abstractions and/or extensions to assist in writing
/// cleaner C# code.
/// 
public static class Extensions
{
    public static Color BlankColour = new Color(0, 0, 0, 0);
    private static readonly long TimeNow = System.Diagnostics.Stopwatch.GetTimestamp();

    ///
    /// From https://stackoverflow.com/a/47816154
    ///
    public static (T first, S second) Destruct<T, S>(object[] items)
    {
        Assert.Equals(items.Length, 2);
        return ((T) items[0], (S) items[1]);
    }

    public static (T first, S second) Destruct<T, S>(object items)
    {
        Assert.TupleLength(items, 2);
        return (ValueTuple<T, S>) (items);
    }

    ///
    /// Returns the number of nanoseconds elapsed since the beginning of the
    /// program.
    ///
    public static long NanosecondsElapsed()
    {
        var ticksPerNanosecond = System.Diagnostics.Stopwatch.Frequency / (1_000_000);
        return (System.Diagnostics.Stopwatch.GetTimestamp() - TimeNow) / ticksPerNanosecond;
    }

    public static Vector2 GodotVec2ToSystemVec2(Godot.Vector2 vec2)
    {
        return new Vector2(vec2.x, vec2.y);
    }

    public static Godot.Vector2 SystemVec2ToGodotVec2(Vector2 vec2)
    {
        return new Godot.Vector2(vec2.X, vec2.Y);
    }

    public static Color SystemVec4ToGodotColor(Vector4 colour)
    {
        return new Color(colour.X, colour.Y, colour.Z, colour.W);
    }
    
    public static Vector4 GodotColorToSystemVec4(Color colour)
    {
        return new Vector4(colour.r, colour.g, colour.b, colour.a);
    }

    public static Vector4 ColorToFloat(Vector4 colour)
    {
        return new Vector4(
            colour.X / 255,
            colour.Y / 255,
            colour.Z / 255,
            colour.W / 255
        );
    }

    ///
    /// Returns a random number between start and finish without being bias
    /// by naively using the % operator.
    /// 
    public static int RangedRandom(Random rng, int start, int end)
    {
        Assert.LessThan(start, end);
        var range = end - start;
        Assert.GreaterThan(range, 0);
        var safeRange = int.MaxValue - range;
        int guess;
        do
        {
            guess = rng.Next();
        } while (guess > safeRange);

        return (guess % range) + start;
    }
    
    public static Color ColorLerp(Color first, Color second, float mix)
    {
        if (mix <= 0)
            return first;
        if (mix >= 1)
            return second;
        return new Color(
            Mathf.Lerp(first.r, second.r, mix),
            Mathf.Lerp(first.g, second.g, mix),
            Mathf.Lerp(first.b, second.b, mix),
            Mathf.Lerp(first.a, second.a, mix)
        );
    }

    public static Godot.Vector2 ClampGodotVector2(Godot.Vector2 vec, Godot.Vector2 clamp)
    {
        vec.x = Mathf.Max(vec.x, 0);
        vec.y = Mathf.Max(vec.y, 0);
        vec.x = Mathf.Min(vec.x, clamp.x);
        vec.y = Mathf.Min(vec.y, clamp.y);
        return vec;
    }
}