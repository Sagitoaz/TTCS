# Build Instructions - Sprint 3

## Prerequisites
- Unity Editor: 6000.3.x (the same version used by project)
- .NET SDK: installed (for optional csproj build checks)
- Open project path: `e:/WINDOW/Unity Project/TTCS/TTCS`

## Build Steps (Recommended in Unity)
1. Open Unity Hub.
2. Add/Open project `TTCS`.
3. Wait until package import and script recompile finish.
4. In Unity, open `Window > General > Console` and clear old logs.
5. Validate there are no red compile errors.
6. Open `File > Build Settings`.
7. Select target platform (Windows recommended for local verification).
8. Ensure all required scenes are added (MainMenu, Tutorial, LevelSelect, Combat).
9. Click `Build` and output to a temporary folder (for example `Builds/Sprint3`).

## Optional CLI Check
```bash
dotnet build Assembly-CSharp.csproj -nologo
```
Note: In this workspace, CLI build may fail without compiler diagnostics due Unity toolchain/project-reference behavior. Unity Editor compile is source of truth.

## Build Success Criteria
- Unity Console has no compile errors.
- Build completes and executable starts.
- No immediate crash on launch.

## Troubleshooting
1. If script compile fails, reimport changed scripts: right click `Assets/Scripts` > `Reimport`.
2. If package issues occur, close Unity, reopen from Hub.
3. If scene reference missing, re-add scenes in `Build Settings`.
