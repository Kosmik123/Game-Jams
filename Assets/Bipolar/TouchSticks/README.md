# Touch Sticks

Virtual on-screen joysticks for mobile, drawn with IMGUI. No Canvas, no EventSystem, no prefabs, no assets (textures are generated at runtime).

- Namespace: `Bipolar.TouchSticks`, own assembly `Bipolar.TouchSticks`.
- Input backends: legacy Input Manager and Input System (chosen at compile time, see below).

## Quick start

1. Add one `VirtualJoystick` component per stick. A twin-stick game has two, for example one on the left half of the screen
   (movement) and one on the right half (aiming).
2. Read from each stick:
   - `Value` - deflection after the dead zone (length 0..1, or exactly 1 with `Normalize Output`),
   - `IsPressed` - whether a finger controls the stick (for example "is firing"),
   - events `Pressed` / `Released`.
3. From game code set `Color` (alpha is controlled by the stick state) and `InputEnabled`
   (false = the stick ignores new touches, drops the current one and is not drawn, for example while paused).

```csharp
var move = gameObject.AddComponent<VirtualJoystick>();
move.Configure(new Rect(0f, 0f, 0.5f, 1f), new Vector2(0f, 0f), 0.12f, false);

var aim = gameObject.AddComponent<VirtualJoystick>();
aim.Configure(new Rect(0.5f, 0f, 0.5f, 1f), new Vector2(1f, 0f), 0.2f, true);

// every frame
direction = move.Value;
isFiring = aim.IsPressed;
```

`Configure(captureZone, idleAnchor, deadZone, normalizeOutput)` sets the main parameters when the component is added from code.
Sticks added in the editor are configured in the Inspector.

## Platform check

Sticks do nothing unless `TouchPointers.Enabled` is true: on mobile platforms, or when the page address contains `touch=1`
(then the mouse emulates one finger, handy for testing a WebGL build in a desktop browser). Games can also use
`TouchPointers.Enabled` to decide whether to read touch or mouse/keyboard input.

## Inspector fields

Layout (screen fractions, origin bottom-left):

| Field | Meaning |
|---|---|
| Capture Zone | Area where a new touch can grab this stick. |
| Idle Anchor | Resting position inside the screen reduced by the margin: (0,0) bottom-left corner, (1,1) top-right. |
| Radius Screen Height / Min Radius Pixels | Stick radius as a fraction of the screen height, with a pixel minimum. |
| Margin Screen Height / Min Margin Pixels | Distance of the resting stick from the screen edge. |

Behaviour:

| Field | Meaning |
|---|---|
| Dead Zone | Deflection below this value gives `Value == (0,0)`. |
| Normalize Output | `Value` always has length 1 (or 0 inside the dead zone). |
| Floating | The stick base appears where the finger lands. Otherwise it stays at the resting position. |
| Follow Finger | When the finger leaves the radius, the stick base follows it. |

Look: `Color`. The `States` header shows the id of the pointer currently held (debugging only).

## How it works

- `TouchPointers` reads input at most once per frame (compares `Time.frameCount`), no matter how many sticks exist.
  It also keeps the set of claimed pointers, so two sticks with overlapping zones never grab the same finger.
- A hidden `Updater` (private, nested in `VirtualJoystick`, so it cannot be added in the editor) is created at runtime when
  the first stick is enabled. It gathers input, updates every stick and draws them all (on `Repaint` only).
  Sticks themselves have no `Update` or `OnGUI`.
- The `Updater` runs at execution order -100. Code that reads `Value` in a normal `Update` always sees the current frame.
  Code that sets `InputEnabled` should run earlier if it must take effect in the same frame (for example order -200).
- Static state is reset on `SubsystemRegistration`, so the module works with Domain Reload disabled.

## Input backends

`IPointerSource` abstracts raw input (`Collect(List<Pointer>)`). The default is chosen at compile time:

- `InputSystemPointerSource` when Active Input Handling includes the Input System (`ENABLE_INPUT_SYSTEM`) and the
  `com.unity.inputsystem` package is installed (the asmdef sets `BIPOLAR_TOUCHSTICKS_INPUT_SYSTEM` through `versionDefines`),
- otherwise `LegacyPointerSource` (`ENABLE_LEGACY_INPUT_MANAGER`).

With both handlers active the Input System wins. Both sources use the mouse as one finger when there is no touchscreen.
Call `TouchPointers.SetSource(...)` to plug in your own source.

## Known limits

- `InputSystemPointerSource` has not been compiled or tested yet (it was written in a project without the package).
- No preview of the sticks outside Play Mode.
- Rendering uses IMGUI, which allocates a little; fine for two sticks, not meant for dozens.
