using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

struct GamepadAxisInputMapping
{
    public int axis;
    public string property;
    public double value_max;
    public double value_min;
}

struct GamepadButtonInputMapping
{
    public int key;
    public bool absolute;
    public string property;
    public double value_down;
    public double value_up;
}

namespace OpenTrainER.source.vehicle.component
{
    internal class GamepadInputComponent : VehicleComponent, InputFunctionInterface
    {
        public GamepadAxisInputMapping[] axis_mappings;
        public GamepadButtonInputMapping[] button_mappings;

        protected override void OnInit()
        {
            for (int i = 0; i < axis_mappings.Length; i++)
            {
                Vehicle.InitProperty(axis_mappings[i].property);
            }
            for (int i = 0; i < button_mappings.Length; i++)
            {
                Vehicle.InitProperty(button_mappings[i].property);
            }
        }

        protected override void OnTick(double delta)
        {
            foreach (GamepadAxisInputMapping mapping in axis_mappings)
            {
                double val = Input.GetJoyAxis(0, (JoyAxis)mapping.axis);
                Vehicle.SetProperty(mapping.property, (val / 2 + 0.5) * (mapping.value_max - mapping.value_min) + mapping.value_min);
            }
            foreach (GamepadButtonInputMapping mapping in button_mappings)
            {
                double val;
                
                if (Input.IsJoyButtonPressed(0, (JoyButton)mapping.key))
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

        void InputFunctionInterface.InputFunction(Godot.InputEvent inputEvent)
        {

        }
    }
}
