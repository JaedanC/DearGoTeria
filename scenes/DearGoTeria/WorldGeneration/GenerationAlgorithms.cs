using System;
using Godot;
using ImGuiNET;

public class GenerationAlgorithms
{
    private static readonly Vector2[] Directions =
    {
        Vector2.Left,
        Vector2.Left + Vector2.Up,
        Vector2.Up,
        Vector2.Up + Vector2.Right,
        Vector2.Right,
        Vector2.Right + Vector2.Down,
        Vector2.Down,
        Vector2.Down + Vector2.Left
    };
    
    ///
    /// This algorithm spawns a number of drunkards (starting pixels) and then walks
    /// them the desired number of steps in any random (8 point) direction. Where ever
    /// drunkards walk they turn the pixels to the input colour. The resulting image
    /// is cropped to only include the drunkard. The original image is modified then
    /// returned.
    /// 
    /// This algorithm will stop when the drunkard goes out of bounds so it is advised
    /// to make the starting position the centre of the image. The noise parameter
    /// is used to fluctuate to radius of the dig when carving out random holes.
    /// 
    public static void DrunkardWalk(
        OpenSimplexNoise noise, ulong seed, Image image, int drunkards,
        int steps, bool useRadius, float maxRadius, Vector2? startingPoint, Color colour)
    {
        image.Lock();

        var rng = new RandomNumberGenerator();
        rng.Seed = seed;
        for (var i = 0; i < drunkards; i++)
        {
            Vector2 drunkardPosition;
            if (startingPoint == null)
            {
                drunkardPosition = new Vector2(
                    (int) rng.Randi() % image.GetWidth(),
                    (int) rng.Randi() % image.GetHeight()
                );
            }
            else
            {
                drunkardPosition = startingPoint.Value;
            }
            
            // Choose a random direction
            var reverseDirectionOffset = Directions.Length / 2;
            var previousDirection = rng.Randi() % Directions.Length;

            var minX = drunkardPosition.x;
            var maxX = drunkardPosition.x;
            var minY = drunkardPosition.y;
            var maxY = drunkardPosition.y;
            for (var step = 0; step < steps + 1; step++)
            {
                // Only let the next direction go in 7 directions -> not backward. This
                // makes caves look more narrow as the algorithm will much more rarely
                // turn back on itself.
                // A number in this range [1, Directions.Length]
                var safeOffset = rng.Randi() % (Directions.Length - 1) + 1;
                var nextDirection = (previousDirection + safeOffset) % Directions.Length;
                drunkardPosition += Directions[nextDirection];
                
                // Treat the previous direction as the reversed Vector2
                previousDirection = (nextDirection + reverseDirectionOffset) % Directions.Length;

                if (drunkardPosition.x < 0 || drunkardPosition.x >= image.GetWidth() ||
                    drunkardPosition.y < 0 || drunkardPosition.y >= image.GetHeight())
                    continue;

                minX = Math.Min(drunkardPosition.x, minX);
                maxX = Math.Max(drunkardPosition.x, maxX);
                minY = Math.Min(drunkardPosition.y, minY);
                maxY = Math.Max(drunkardPosition.y, maxY);

                if (useRadius)
                {
                    // Multi-pixel Dig
                    var randomRadius = ((noise.GetNoise1d(step) + 1) / 2) * maxRadius;
                    ImageTools.DigCircle(image, drunkardPosition, randomRadius, colour);
                }
                else
                {
                    // 1 Pixel Dig
                    image.SetPixelv(drunkardPosition, colour);
                }
            }
        }
        image.Unlock();
    }
}