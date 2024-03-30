
using Godot;

static internal class WorldRenderer
{
    public static Node3D worldRoot = null;
    public static void RenderTrackNode(TrackNode trackNode)
    {
        Vector2 localPosition = trackNode.WorldCoordinate.ToLocal(VehicleManager.vehicleWorldCoordinate);

        Node3D point = new Node3D();
        worldRoot.AddChild(point);
        point.Position = new Vector3(localPosition.X, 0, localPosition.Y);
      
        Sprite3D sprite = new Sprite3D();
        point.AddChild(sprite);
        sprite.Texture = (Texture2D)GD.Load("res://icon.svg");
        sprite.PixelSize = 0.05f;
        sprite.RotateX(Mathf.Pi / 2);
    }
}

