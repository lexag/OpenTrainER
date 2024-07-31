using Godot;
using System.Collections.Generic;
using System.IO;

namespace OpenTrainER.source.vehicle.component
{
    internal class DashboardGUIComponent : VehicleComponent
    {
        internal struct Gauge
        {
            public string id;
            public string property;
            public double max;
            public double min;
            public float max_angle;
            public float min_angle;
            public float radius;
            public float x;
            public float y;
            public string image;
        }


        public string background = "background is not set";
        public Gauge[] gauges = System.Array.Empty<Gauge>();

        Dictionary<string, Node> instruments = new Dictionary<string, Node>();

        Window guiWindow;
        Node2D instrumentParent;
        TextureRect backgroundTextureRect;

        protected override void OnInit()
        {
            base.OnInit();
            var vehiclePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "OpenTrainER", "vehicles");
            var filePath = Path.Combine(vehiclePath, background);

            Image backgroundImage = Image.LoadFromFile(filePath);
            if (backgroundImage == null)
            {
                return;
            }

            Vector2I size = backgroundImage.GetSize();
            guiWindow = new Window
            {
                ContentScaleAspect = Window.ContentScaleAspectEnum.Keep,
                ContentScaleStretch = Window.ContentScaleStretchEnum.Fractional,
                Size = size,
                MaxSize = size,
                MinSize = size,
            };
            WorldManager.renderer.AddChild(guiWindow);
            guiWindow.Show();

            backgroundTextureRect = new TextureRect
            {
                Texture = ImageTexture.CreateFromImage(backgroundImage),
                Size = backgroundImage.GetSize(),
                StretchMode = TextureRect.StretchModeEnum.KeepAspect,
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                LayoutMode = 1,
                AnchorsPreset = (int)Control.LayoutPreset.FullRect,
            };
            guiWindow.AddChild(backgroundTextureRect);



            instrumentParent = new Node2D();
            backgroundTextureRect.AddChild(instrumentParent);


            foreach (Gauge gauge in gauges)
            {
                Image image = Image.LoadFromFile(Path.Combine(vehiclePath, gauge.image));
                if (image == null) { continue; }

                float scaleFactor = image.GetSize().X / gauge.radius;
                Sprite2D sprite = new Sprite2D
                {
                    Texture = ImageTexture.CreateFromImage(image),
                    Scale = new Vector2(scaleFactor, scaleFactor),
                    Position = new Vector2(gauge.x, gauge.y),
                };
                instrumentParent.AddChild(sprite);

                instruments.Add(gauge.id, sprite);
            }

        }

        protected override void OnTick(double delta)
        {
            instrumentParent.Scale = backgroundTextureRect.Size / backgroundTextureRect.Texture.GetSize();

            foreach (Gauge gauge in gauges)
            {
                float percent = (float)((Vehicle.GetProperty(gauge.property) - gauge.min) / (gauge.max - gauge.min));
                float angle = Mathf.Lerp(gauge.min_angle, gauge.max_angle, percent);
                ((Sprite2D)instruments[gauge.id]).RotationDegrees = angle;
            }
        }
    }
}
