# ZOMBIE RUNNER: CITY RUINS  (Unity endless runner)

A 3-lane endless runner through a burning, collapsed city while a zombie horde chases you.
**No imported assets needed.** The city, characters, UI and sound effects are all generated in code.

## Run it
**Option A - open this folder as a project**
1. Unity Hub -> Add -> select the `ZombieRunner` folder.
2. If Hub says the editor version isn't installed, choose any **Unity 2022.3 LTS or Unity 6** and let it open.
3. Wait for the import. A scene is created automatically (`Assets/Scenes/ZombieRunner.unity`).
4. Press **Play**.

**Option B - drop into an existing project (3D Built-in or URP)**
Copy `Assets/ZombieRunner` into your project's `Assets` folder, open any scene, press **Play**.
The game bootstraps itself in whatever scene is open (it makes its own camera, light, and fog).

## Controls
| Action | Keyboard | Touch |
|---|---|---|
| Change lane | A / D or Left / Right | swipe left / right |
| Jump (barriers, zombies) | Space / W / Up | swipe up or tap |
| Slide (fallen beams) / fast-fall | S / Down | swipe down |
| Pause | P / Esc | - |
| Start / Restart | Space, Enter, R | tap |

## Rules
- Concrete barriers: **jump**. Fallen sign beams: **slide**. Wrecked cars: **change lanes**.
- Shambling zombies walk toward you: dodge them or hop over them.
- Hitting anything shoves the horde closer (red bar at top). If it reaches you, it's over.
- Medkits push the horde back and give bonus score. Supplies add 10 points each.
- Speed ramps up the longer you survive.

## Code map (`Assets/ZombieRunner/Scripts`)
- `GameManager.cs` - auto-bootstrap, game loop, camera, collisions, HUD
- `World.cs` - endless chunk generator: buildings, rubble, lamps, cars, obstacle patterns
- `Player.cs`, `ZombieRig.cs`, `Horde.cs`, `Models.cs`, `Prims.cs` - primitive-built characters and props
- `GameInput.cs` - keyboard/swipe input (works with legacy Input Manager and new Input System)
- `Sfx.cs` - procedural sound effects

Tweak difficulty in `GameManager.PlayTick` (speed ramp) and `World.SpawnRow` (obstacle mix).
