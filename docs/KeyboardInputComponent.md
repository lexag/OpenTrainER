# KeyboardInputComponent

## Json Properties
|name|type|description|
|---|---|---|
|`mappings`|list|list of KeyboardInputMapping(s)|

### KeyboardInputMapping
|name|type|description|
|---|---|---|
|key|int|Godot keycode of the mapped key|
|absolute|bool| If true, the key writes `value_down` and `value_up` to property `property` on press/release. If false, the key changes `property` by `value_up` and `value_down` units per second when held/released.|
|property|string|Name of the property. Generally `controls:<name-of-control>`
|value_up|double|Value to be set or changed when key is released.|
|value_down|double|Value to be set or changed when key is pressed/held.|

## Function
Provides absolute and hold-to-change keyboard input mapping.
