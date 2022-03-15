using Godot;

public class ImGuiViewport : Viewport
{
    public override void _Process(float delta)
    {
        Size = OS.WindowSize;
    }
}
