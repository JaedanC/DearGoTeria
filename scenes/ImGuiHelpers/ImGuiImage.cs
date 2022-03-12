using Godot;
using ImGuiNET;

public static class ImGuiImage
{
    public static void Create(ImageTexture texture, Image image)
    {
        Create(texture, image, image.GetSize());
    }
    
    public static void Create(ImageTexture texture, Image image, Vector2 imageSize)
    {
        texture.CreateFromImage(image);
        var id = ImGuiGD.BindTexture(texture);
        ImGui.Image(id, Extensions.GodotVec2ToSystemVec2(imageSize));
    }
}
