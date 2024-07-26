Vehicles are the OpenTrainER representation of any kind of rolling stock (locomotive, train carriage, multiple unit, tender, caboose, etc.). Vehicles are constructed in a modular fashion using Vehicle Components defined in a json file.

Vehicle files are placed in `Documents/OpenTrainER/vehicles`.

```json
<vehicle-name>.json
{
	"name": "test_vehicle",
	"components": {
		"ElectricDriverComponent": {
		...
		},
		"KeyboardInputComponent": {
			...
		}
		...
	}
}
```

Each vehicle component has its own settings (see [Vehicle Components](https://github.com/lexag/OpenTrainER/wiki/Vehicle%20Components)). To move, a vehicle requires some DriverComponent.