using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

struct KeyboardInputMapping
{
    Godot.Key key;
    bool absolute;
    string property;
    double value_down;
    double value_up;
}

namespace OpenTrainER.source.vehicle.component
{
    internal class KeyboardInputComponent : VehicleComponent
    {
        public KeyboardInputMapping[] mappings;
    }
}
