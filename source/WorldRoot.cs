using Godot;
internal partial class WorldRoot : Node
{
    public bool idle = false;

    public override void _EnterTree()
    {
        base._EnterTree();
    }

    public override void _Ready()
    {
        base._Ready();
        WorldManager.worldRoot = this;
        if (!idle)
        {
            WorldManager.Setup();
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (!idle)
        {
            WorldManager.Tick(delta);
        }
    }
}

