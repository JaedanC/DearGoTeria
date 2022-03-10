﻿using System;
using System.Linq;
using Godot;
using ImGuiNET;

public static class ImageTools
{
    ///
    /// This method returns a blank (black) image of the specified size
    ///
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
        var newImage = BlankImage(image.GetSize(), Colors.White);
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
            var blockLocation = new Vector2((int)Math.Round(i), (int)Math.Round(j));
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
        Flatten,    // Top will replace base if top.a > 0
        Dig,        // Base will be blank if top.a > 0
        Add,        // Equals top + base
        Subtract,   // Equals top - base
        Multiply,   // Equals top * base
        Overlay     // Uses overlay algorithm from wikipedia
    }

    ///
    /// This method blends the topLayer image onto the bottom image like in paint.
    /// Select the correct mode using the 'type' parameter. The 'baseLayer' is
    /// modified in-place.
    ///
    public static void BlendImages(Image baseLayer, Image topLayer, Blend blendType, Vector2 offset)
    {
        baseLayer.Lock();
        topLayer.Lock();

        for (var topX = 0; topX < topLayer.GetWidth(); topX++)
        for (var topY = 0; topY < topLayer.GetHeight(); topY++)
        {
            // Calculate the offset for where to read from the image_base
            // If reading outside the bounds of the image_base, continue
            var basePixel = new Vector2(topX, topY) + offset;
            if (basePixel.x < 0 || basePixel.x >= baseLayer.GetWidth() ||
                basePixel.y < 0 || basePixel.y >= baseLayer.GetHeight())
                continue;

            // Retrieve each colour for blending
            var topColour = topLayer.GetPixel(topX, topY);
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

                    baseLayer.SetPixelv(basePixel, newColour);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(blendType), blendType, null);
            }
        }

        baseLayer.Unlock();
        topLayer.Unlock();
    }
}