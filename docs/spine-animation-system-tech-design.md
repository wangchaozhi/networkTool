# Spine Direction Animation System Technical Design

## Goal

Replace the current file-drop visual with an original "chubby yellow baby dragon eating a file" action. The file should enter from the same direction as the user's drag movement, then fly into the character's mouth.

The implementation should move the app toward a Spine-style action system so future actions such as `idle`, `walk`, `sleep`, `drag`, `angry`, `happy`, and `notify` can share one animation entry point.

## Constraints

- Continue using WPF for the app shell.
- Avoid direct reproduction of a specific copyrighted character; use an original yellow baby-dragon mascot inspired by the requested mood.
- Keep project assets under `NetFloat/res` and reference them as WPF resources.
- Keep drag direction calculation in `MainWindow`; keep visual animation details outside `MainWindow`.
- Avoid one-off WPF animation code in window code-behind.
- Avoid the face-only zoom animation as the primary effect. Use file movement, rotation, fade, and character bob/tilt instead.

## Runtime Direction

Spine official runtime notes:

- Spine exports runtime data as skeleton JSON/binary plus texture atlas files.
- The official generic C# runtime can load and manipulate skeletons, but it does not render by itself.
- WPF is not an official renderer target, so this project should isolate the animation player behind an interface.

Chosen first step:

- Build a Spine-style action layer in WPF:
  - `SpineActionName`
  - `SpineActionRequest`
  - `ISpineAnimationPlayer`
  - `WpfSpineActionPlayer`
- Keep `WpfSpineActionPlayer` as the temporary WPF renderer for `eat_file`.
- Later, replace only the `ISpineAnimationPlayer` implementation with a real Spine renderer/export pipeline.

## Asset Plan

- Add `NetFloat/res/nailong_eater.png`.
- Use a flat chroma-key source and local background removal to create a transparent PNG.
- Replace the previous `bean_eater.png` reference in `FaceWindow.xaml` with `nailong_eater.png`.
- Keep `NetFloat/res/file.png` as the first file prop attachment.

Future Spine asset layout:

- `NetFloat/res/spine/nailong/nailong.json`
- `NetFloat/res/spine/nailong/nailong.atlas`
- `NetFloat/res/spine/nailong/nailong.png`

Initial action names:

- `eat_file`
- `idle`
- `walk`
- `sleep`
- `drag`
- `angry`
- `happy`
- `notify`

## Code Structure

- Add `NetFloat/Animation/Spine/SpineActionName.cs`.
  - Centralizes supported action names.
- Add `NetFloat/Animation/Spine/SpineActionRequest.cs`.
  - Carries drop point, drag direction, and completion callback.
- Add `NetFloat/Animation/Spine/ISpineAnimationPlayer.cs`.
  - Defines one entry point for all character actions.
- Add `NetFloat/Animation/Spine/WpfSpineActionPlayer.cs`.
  - Temporary WPF renderer for the `eat_file` action.
  - Owns creating the flying file image.
  - Computes directional start points.
  - Runs file path, rotation, scale-down, fade, and character bob/tilt animations.
- Keep `FaceWindow` responsible only for window lifecycle and wiring controls into the player.
- Keep `MainWindow` responsible for:
  - drag point tracking,
  - converting drop coordinates into the animation window coordinate space,
  - showing `FaceWindow`.

## Animation Behavior

1. User drags files over the floating window.
2. `MainWindow` records the latest drag vector.
3. On drop, `FaceWindow` opens centered over the original floating window.
4. `ISpineAnimationPlayer.Play(SpineActionName.EatFile, request)` starts the action.
5. `WpfSpineActionPlayer` starts the file icon outside/near the character, based on drag direction.
6. The file rotates toward the movement direction and flies into the mouth.
7. The character lightly bobs/tilts as the file arrives.
8. The animation window closes and the main window returns.

## Later Real-Spine Swap

When real Spine exports are available:

1. Keep `MainWindow` and `FaceWindow` unchanged.
2. Add a real Spine-backed implementation of `ISpineAnimationPlayer`.
3. Map action names to Spine animation names:
   - `SpineActionName.EatFile` -> `eat_file`
   - `SpineActionName.Idle` -> `idle`
   - `SpineActionName.Walk` -> `walk`
   - `SpineActionName.Sleep` -> `sleep`
   - `SpineActionName.Drag` -> `drag`
   - `SpineActionName.Angry` -> `angry`
   - `SpineActionName.Happy` -> `happy`
   - `SpineActionName.Notify` -> `notify`
4. Use one event boundary for action completion so windows do not know whether the renderer is WPF-native or Spine-native.

## Verification

- Run `dotnet build NetFloat.sln`.
- Confirm the new PNG is included as a WPF resource.
- Confirm the drag-drop logic still copies dropped file paths to clipboard.
