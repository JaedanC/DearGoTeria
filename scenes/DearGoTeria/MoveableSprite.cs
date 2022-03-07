using Godot;
using System;
using ImGuiNET;

public class MoveableSprite : Sprite
{
    private bool mouseInside;
    private bool follow;
    
    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        Rotate(0.05f);
    }
    
    public override void _Process(float delta)
    {
        base._Process(delta);
        if (follow)
            Position = GetViewport().GetMousePosition();
    }
    
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey)
        {
            ImGuiLog.Debug(@event + "");
        }

        if (!mouseInside)
            return;

        if (@event is InputEventMouseButton mouseEvent)
        {
            if ((ButtonList) mouseEvent.ButtonIndex == ButtonList.Left)
            {
                follow = mouseEvent.Pressed;
            }
        }
    }

    public void OnMouseEntered()
    {
        mouseInside = true;
        ImGuiLog.Debug("In");
    }
    
    public void OnMouseExited()
    {
        mouseInside = false;
        ImGuiLog.Debug("Out");
    }
}