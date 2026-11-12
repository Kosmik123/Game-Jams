# TouchSticks

Virtual on-screen joysticks for mobile, drawn with IMGUI (no Canvas, no EventSystem, no prefabs).

## Usage

Add one `VirtualJoystick` component per stick (a twin-stick game has two). Read from each:

- `Value` - deflection after the dead zone (length 0..1, or exactly 1 with `normalizeOutput`),
- `IsPressed` - whether a finger controls the stick (for example "is firing"),
- events `Pressed` / `Released`.

Set `Color` and `InputEnabled` from game code. `Configure(...)` sets the main parameters when the component is added at runtime.

Sticks work only when `TouchPointers.Enabled` is true: on mobile platforms, or when the page address contains `touch=1`
(then the mouse emulates one finger, handy for testing in a desktop browser). Otherwise the sticks do nothing.

## How it works

- `TouchPointers` reads input at most once per frame (compares `Time.frameCount`) and keeps the list of claimed fingers,
  so two sticks never grab the same one.
- A hidden `Updater` (nested in `VirtualJoystick`) is created when the first stick is enabled. It updates and draws all sticks,
  so the sticks themselves have no `Update` or `OnGUI`.
- Layout is set in screen fractions: `captureZone` (where a touch can grab the stick) and `idleAnchor` + margin (where the stick rests).

## Input backends

`IPointerSource` abstracts raw input. `LegacyPointerSource` (legacy Input Manager) is the default;
call `TouchPointers.SetSource(...)` to plug in another one (for example for the Input System).
