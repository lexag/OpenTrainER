using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using OpenTrainER.source.file;

public struct SignalLightStruct
{
    public float x;
    public float y;
    public float scale;
    public string color;
}

public struct SignalFeature
{
    public string id;
    public string layout;
    public int speed;
    public bool atc;
    public float[] direction;
    public float[] offset;
}

namespace OpenTrainER.source.renderer
{
    internal partial class SignalRenderer : Node3D
    {
        string nodeId;
        SignalFeature signalFeature;

        public SignalRenderer(string nodeId, SignalFeature feature) 
        {
            this.nodeId = nodeId;
            this.signalFeature = feature;
        }

        public override void _Ready()
        {
            base._Ready();
            Vector3 pos = Util.ToVector(WorldManager.track.points[nodeId].position);
            Position = Util.ToVector(signalFeature.offset) + pos + Vector3.Up * 30;
            Vector3 dir = Util.ToVector(signalFeature.direction);
            LookAt(GlobalPosition + dir);

            Dictionary<string, SignalLightStruct> layout = WorldManager.signalingData.layouts[signalFeature.layout];

            float lightDistance = 0.5f;

            Sprite3D background = new Sprite3D();
            AddChild(background);
            background.Texture = TextureLoader.LoadFile("lines/" + WorldManager.lineName + "/signal", signalFeature.layout + ".png");
            background.PixelSize = 0.002f;
            background.CastShadow = GeometryInstance3D.ShadowCastingSetting.DoubleSided;
            background.Shaded = true;

            foreach (var kv in layout)
            {
                SignalLightStruct lightStruct = kv.Value;
                Sprite3D sprite = new Sprite3D();
                background.AddChild(sprite);
                sprite.Shaded = false;
                sprite.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
                sprite.Texture = (Texture2D)GD.Load("res://assets/signal/light.png");
                sprite.PixelSize = 0.0015f * lightStruct.scale;
                sprite.TranslateObjectLocal(new Vector3(lightStruct.x * lightDistance, -lightStruct.y * lightDistance, 0.1f));
                sprite.Modulate = Color.FromHtml(lightStruct.color);
            }
        }
    }
}
