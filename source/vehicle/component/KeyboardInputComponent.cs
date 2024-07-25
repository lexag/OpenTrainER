using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

struct KeyboardInputMapping
{
    public int key;
    public bool absolute;
    public string property;
    public double value_down;
    public double value_up;
}

namespace OpenTrainER.source.vehicle.component
{
    internal class KeyboardInputComponent : VehicleComponent
    {
        public KeyboardInputMapping[] mappings;

        protected override void OnInit()
        {
            for (int i = 0; i < mappings.Length; i++)
            {
                Vehicle.InitProperty(mappings[i].property);
            }
        }

        protected override void OnTick(double delta)
        {
            foreach (KeyboardInputMapping mapping in mappings)
            {
                double val;
                if (Input.IsPhysicalKeyPressed((Godot.Key)mapping.key))
                {
                    val = mapping.value_down;
                }
                else
                {
                    val = mapping.value_up;
                }

                if (mapping.absolute)
                {
                    Vehicle.SetProperty(mapping.property, val);
                }
                else
                {
                    Vehicle.ChangeProperty(mapping.property, val * delta);
                }
            }
        }
    }
}
