using Godot;
using System;

public class ImGuiContainer : ViewportContainer
{
    public override void _Process(float delta)
    {
        SetSize(OS.WindowSize);
    }
}
