using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class ImageTools
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
    /// This method returns a blank (black) image of the specified size
    ///
    public static Image BlankImage(Vector2 imageSize)
    {
        return BlankImage(imageSize, Extensions.BlankColour);
    }

    public static Image BlankImage(Vector2 imageSize, Color colour)
    {
        var image = new Image();
        image.Create((int) imageSize.x, (int) imageSize.y, false, Image.Format.Rgba8);
        image.Fill(colour);
        return image;
    }

    ///
    /// This method returns a new image where all the pixels that aren't alpha==0,
    /// are inverted. A new image is returned. Requires the image to be unlocked.
    /// 
    public static Image InvertImage(Image image)
    {
        var newImage = BlankImage(image.GetSize());
        image.Lock();
        newImage.Lock();

        for (var i = 0; i < image.GetWidth(); i++)
        for (var j = 0; j < image.GetHeight(); j++)
        {
            var oldColour = image.GetPixel(i, j);
            newImage.SetPixel(i, j, oldColour.Inverted());
        }

        image.Unlock();
        newImage.Unlock();
        return newImage;
    }

    ///
    /// Draws a circle of a desired radius, at location. Any pixels out of range are ignored.
    /// Requires the image to be unlocked.
    /// 
    public static void DigCircle(Image image, Vector2 location, double radius, Color colour)
    {
        image.Lock();
        for (var i = location.x - radius; i < location.x + radius + 1; i++)
        for (var j = location.y - radius; j < location.y + radius + 1; j++)
        {
            if (i < 0 || i > image.GetWidth() - 1 || j < 0 || j > image.GetHeight() - 1)
                continue;
            var blockLocation = new Vector2((int) Math.Round(i), (int) Math.Round(j));
            var distance = location.DistanceTo(blockLocation);
            if (distance <= radius)
                image.SetPixelv(blockLocation, colour);
        }

        image.Unlock();
    }

    ///
    /// This algorithm will flood fill the image from the starting location and
    /// change the colour to be the desired colour. There is no tolerance. The
    /// colours must match exactly. This uses a naive algorithm of a DFS to find
    /// connected pixels of the same colour. Requires the image to be unlocked.
    /// 
    public static void FloodFill(Image image, Vector2 startingAt, Color toColour)
    {
        image.Lock();
        var baseColour = image.GetPixelv(startingAt);
        var fringe = new System.Collections.Generic.List<Vector2> {startingAt};
        var explored = new System.Collections.Generic.HashSet<Vector2>();

        while (fringe.Count > 0)
        {
            var currentNode = fringe[fringe.Count - 1];
            fringe.RemoveAt(fringe.Count - 1);
            var x = currentNode.x;
            var y = currentNode.y;

            // Add to explored
            explored.Add(currentNode);

            // This location is out of bounds
            if (x < 0 || x >= image.GetWidth() || y < 0 || y >= image.GetHeight())
                continue;

            var currentNodeColour = image.GetPixelv(currentNode);
            if (currentNodeColour != baseColour)
                continue;

            image.SetPixelv(currentNode, toColour);

            // Add unexplored neighbours
            Vector2[] neighbours =
            {
                new Vector2(x - 1, y),
                new Vector2(x + 1, y),
                new Vector2(x, y - 1),
                new Vector2(x, y + 1),
            };
            fringe.AddRange(neighbours.Where(o => !explored.Contains(o)));
        }

        image.Unlock();
    }

    ///
    /// This function will add a 'border' thick pixel border to the image and set those
    /// pixels to be the supplied colour. A new image is returned. Requires the image to
    /// be unlocked.
    /// 
    public static Image AddBorder(Image image, int borderSize, Color colour)
    {
        Assert.GreaterThan(borderSize, 0);
        var newImage = BlankImage(new Vector2(
            image.GetWidth() + 2 * borderSize,
            image.GetHeight() + 2 * borderSize
        ), colour);

        image.Lock();
        newImage.Lock();

        for (var i = 0; i < image.GetWidth(); i++)
        for (var j = 0; j < image.GetHeight(); j++)
        {
            var pixelColour = image.GetPixel(i, j);
            newImage.SetPixel(i + borderSize, j + borderSize, pixelColour);
        }

        image.Unlock();
        newImage.Unlock();

        return newImage;
    }

    ///
    /// This function will change all pixel in the input image that are of the colour
    /// 'from' and change them to 'to'. Requires the image to be unlocked.
    ///
    public static void ChangeColour(Image image, Color from, Color to)
    {
        image.Lock();
        for (var i = 0; i < image.GetWidth(); i++)
        for (var j = 0; j < image.GetHeight(); j++)
        {
            var colour = image.GetPixel(i, j);
            if (colour == from)
                image.SetPixel(i, j, to);
        }

        image.Unlock();
    }

    ///
    /// Crops the image to only include pixels which have alpha > 0.
    /// 
    public static void CropUnused(Image image)
    {
        var usedRect = image.GetUsedRect();
        image.BlitRect(image, usedRect, Vector2.Zero);
        image.Crop((int) usedRect.Size.x, (int) usedRect.Size.y);
    }

    public enum Blend
    {
        Flatten, // Top will replace base if top.a > 0
        Dig, // Base will be blank if top.a > 0
        Add, // Equals top + base
        Subtract, // Equals top - base
        Multiply, // Equals top * base
        Overlay // Uses overlay algorithm from wikipedia
    }

    ///
    /// This method blends the topLayer image onto the bottom image like in paint.
    /// Select the correct mode using the 'type' parameter. The 'baseLayer' is
    /// modified in-place. Requires the baseLayer and the topLayer to be unlocked.
    ///
    public static void BlendImages(Image baseLayer, Image topLayer, Blend blendType, Vector2 baseOffset)
    {
        baseLayer.Lock();
        topLayer.Lock();
        for (var i = 0; i < topLayer.GetWidth(); i++)
        for (var j = 0; j < topLayer.GetHeight(); j++)
        {
            // Calculate the offset for where to read from the baseLayer
            // If reading outside the bounds of the baseLayer, continue
            var basePixel = new Vector2(i, j) + baseOffset;
            if (basePixel.x < 0 || basePixel.x >= baseLayer.GetWidth() ||
                basePixel.y < 0 || basePixel.y >= baseLayer.GetHeight())
                continue;

            // Retrieve each colour for blending
            var topColour = topLayer.GetPixel(i, j);
            var baseColour = baseLayer.GetPixelv(basePixel);

            switch (blendType)
            {
                case Blend.Flatten:
                    if (topColour.a > 0)
                        baseLayer.SetPixelv(basePixel, topColour);
                    break;
                case Blend.Dig:
                    if (topColour.a > 0)
                        baseLayer.SetPixelv(basePixel, Extensions.BlankColour);
                    break;
                case Blend.Add:
                    var addedColour = new Color(
                        topColour.r + baseColour.r,
                        topColour.g + baseColour.g,
                        topColour.b + baseColour.b
                    );
                    baseLayer.SetPixelv(basePixel, addedColour);
                    break;
                case Blend.Subtract:
                    var subtractedColour = new Color(
                        topColour.r - baseColour.r,
                        topColour.g - baseColour.g,
                        topColour.b - baseColour.b
                    );
                    baseLayer.SetPixelv(basePixel, subtractedColour);
                    break;
                case Blend.Multiply:
                    var multiplyColour = new Color(
                        topColour.r * baseColour.r,
                        topColour.g * baseColour.g,
                        topColour.b * baseColour.b
                    );
                    baseLayer.SetPixelv(basePixel, multiplyColour);
                    break;
                case Blend.Overlay:
                    // From https://en.wikipedia.org/wiki/Blend_modes#Overlay
                    var newColour = new Color();

                    // Red
                    if (baseColour.r < 0.5)
                        newColour.r = 2 * baseColour.r * topColour.r;
                    else
                        newColour.r = 1 - 2 * (1 - baseColour.r) * (1 - topColour.r);

                    // Green
                    if (baseColour.g < 0.5)
                        newColour.g = 2 * baseColour.g * topColour.g;
                    else
                        newColour.g = 1 - 2 * (1 - baseColour.g) * (1 - topColour.g);

                    // Blue
                    if (baseColour.b < 0.5)
                        newColour.b = 2 * baseColour.b * topColour.b;
                    else
                        newColour.b = 1 - 2 * (1 - baseColour.b) * (1 - topColour.b);

                    newColour.a = 1;

                    baseLayer.SetPixelv(basePixel, newColour);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(blendType), blendType, null);
            }
        }

        baseLayer.Unlock();
        topLayer.Unlock();
    }

    ///
    /// This function takes in an image and maps all pixels to black or white depending
    /// on the supplied step value [0, 1]. If the colour is above the threshold it is
    /// white. If it is below, the colour is black. In future it may be cool to use
    /// Simplex noise to modify the threshold over time. Requires the image to be unlocked.
    /// 
    public static void BlackWhiteStep(Image image, float step)
    {
        image.Lock();
        for (var i = 0; i < image.GetWidth(); i++)
        for (var j = 0; j < image.GetHeight(); j++)
        {
            var greyness = image.GetPixel(i, j).v;
            image.SetPixel(i, j, greyness < step ? Colors.Black : Colors.White);
        }

        image.Unlock();
    }

    ///
    /// This function writes a gradient of startColour to endColour using the vector
    /// direction starting at startPoint. Requires the image to be unlocked.
    /// 
    public static void Gradient(Image image, Vector2 startPoint, Vector2 endPoint, Color startColour, Color endColour)
    {
        image.Lock();
        for (var i = 0; i < image.GetWidth(); i++)
        for (var j = 0; j < image.GetHeight(); j++)
        {
            // From https://stackoverflow.com/a/521538
            var a = endPoint.x - startPoint.x;
            var b = endPoint.y - startPoint.y;
            var c1 = a * startPoint.x + b * startPoint.y;
            var c2 = a * endPoint.x + b * endPoint.y;
            var c = a * i + b * j;
            var percentage = (c - c1) / (c2 - c1);
            var newColour = Extensions.ColorLerp(startColour, endColour, percentage);
            image.SetPixel(i, j, newColour);
        }

        image.Unlock();
    }
    
    public static void Gradient(Image image, Vector2 startPoint, Vector2 endPoint, List<ValueTuple<Color, float>> gaps)
    {
        Assert.GreaterThanEquals(gaps.Count, 2);
        Assert.Equals(0, gaps[0].Item2, 0.0001f);
        Assert.Equals(1, gaps[gaps.Count - 1].Item2, 0.0001f);
        image.Lock();
        for (var i = 0; i < image.GetWidth(); i++)
        for (var j = 0; j < image.GetHeight(); j++)
        {
            // From https://stackoverflow.com/a/521538
            var a = endPoint.x - startPoint.x;
            var b = endPoint.y - startPoint.y;
            var c1 = a * startPoint.x + b * startPoint.y;
            var c2 = a * endPoint.x + b * endPoint.y;
            var c = a * i + b * j;
            var percentage = (c - c1) / (c2 - c1);

            if (percentage < 0)
                image.SetPixel(i, j, gaps[0].Item1);
            
            if (percentage > 1)
                image.SetPixel(i, j, gaps[gaps.Count - 1].Item1);

            for (var colourIdx = 0; colourIdx < gaps.Count - 1; colourIdx++)
            {
                var (firstColour, firstFloat) = gaps[colourIdx];
                var (secondColour, secondFloat) = gaps[colourIdx + 1];
                
                Assert.GreaterThanEquals(secondFloat, firstFloat);
                if (firstFloat <= percentage && percentage <= secondFloat)
                {
                    var gapPercentage = (percentage - firstFloat) / (secondFloat - firstFloat);
                    var newColour = Extensions.ColorLerp(firstColour, secondColour, gapPercentage);
                    image.SetPixel(i, j, newColour);
                    break;
                }
            }
        }

        image.Unlock();
    }

    ///
    /// This function takes in an OpenSimplexNoise and draws the resultant 1d noise
    /// to the screen as a line. Offset is the y value for the midpoint of the simplex
    /// line to render. This can be used to visualise a 1d OpenSimplexNoise value for
    /// use elsewhere. Requires the image to be unlocked.
    /// 
    public static void SimplexLine(OpenSimplexNoise noise, Image image, float amplitude, int offset)
    {
        image.Lock();
        for (var i = 0; i < image.GetWidth(); i++)
        {
            var noiseValue = noise.GetNoise1d(i) * amplitude;
            for (var j = 0; j < image.GetHeight(); j++)
                image.SetPixel(i, j, j - offset > noiseValue ? Colors.White : Colors.Black);
        }

        image.Unlock();
    }

    ///
    /// This algorithm writes simplex noise onto the image provided.
    /// Google the details for more information on each of the parameters.
    /// Requires the image to be unlocked.
    /// 
    public static void SimplexNoise(OpenSimplexNoise noise, Image image)
    {
        image.Lock();
        for (var i = 0; i < image.GetWidth(); i++)
        for (var j = 0; j < image.GetHeight(); j++)
        {
            var noiseValue = (noise.GetNoise2d(i, j) + 1) / 2;
            image.SetPixel(i, j, new Color(noiseValue, noiseValue, noiseValue));
        }

        image.Unlock();
    }

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
                    DigCircle(image, drunkardPosition, randomRadius, colour);
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

    ///
    /// Given an image, recolour all instanced of colour 'floodFillColour' that are
    /// 'islands'. Islands are pieces of land that would not be found by a
    /// horizontally or vertically flood fill starting at all edges of the image. All
    /// islands are coloured to 'newIslandColour'. This function will assume the
    /// islands are the same colour as the 'floodFillColour'.
    /// 
    public static Image ColourIslands(Image image, Color floodFillColour, Color newIslandColour)
    {
        return ColourIslands(image, floodFillColour, floodFillColour, newIslandColour);
    }

    public static Image ColourIslands(Image image, Color floodFillColour, Color oldIslandColour, Color newIslandColour)
    {
        var newImage = AddBorder(image, 1, floodFillColour);
        FloodFill(newImage, Vector2.Zero, Colors.Blue);
        ChangeColour(newImage, oldIslandColour, newIslandColour);
        ChangeColour(newImage, Colors.Blue, floodFillColour);
        return newImage;
    }

    public static void GradientPlaceStickers(Random random, Image baseImage, Image gradient, List<Image> stickers)
    {
        Assert.Equals(gradient.GetSize(), baseImage.GetSize(), "Gradient and base image must be the same size");
        gradient.Lock();
        foreach (var sticker in stickers)
        {
            while (true)
            {
                var stickerLocation = new Vector2(
                    Extensions.RangedRandom(random, -sticker.GetWidth(), baseImage.GetWidth()),
                    Extensions.RangedRandom(random, -sticker.GetHeight(), baseImage.GetHeight()));

                var gradientTest =
                    gradient.GetPixelv(Extensions.ClampGodotVector2(stickerLocation, gradient.GetSize()));
                var testResult = random.NextDouble();
                if (testResult > gradientTest.v)
                    continue;

                BlendImages(baseImage, sticker, Blend.Dig, stickerLocation);
                break;
            }
        }

        gradient.Unlock();
    }
}