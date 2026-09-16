# Differential Report: Our `main` vs. OpenSim `master`

**1. Documentation & Planning Files**
*   **Removed:** `MASTER_PLAN.md` - This file exists in our branch containing a detailed roadmap for modernizing OpenSimulator (performance optimizations, PBR, Animesh, non-Euclidean worlds, etc.), but it is not part of the upstream OpenSim master branch.

**2. Database Layer (MySQL)**
*   **Modified:** `MySQLEstateData.cs` and `MySQLSimulationData.cs`.
    *   *Details:* The diff shows significant structural changes to how database transactions and connections are handled in these classes (specifically, the addition/removal of `MySqlTransaction` wrapping logic).

**3. Physics Modules (BulletS)**
*   **Modified:** `BSShapes.cs`
    *   *Details:* Fixes applied for memory leaks associated with fallback native shapes. Comments indicate considerations for when native shapes should be freed (e.g., checking in the `Dereference` method).

**4. Core Scene and Framework (OpenSim/Region/Framework)**
*   **Modified:** `Scene.Permissions.cs`, `SceneGraph.cs`, `SceneObjectGroup.cs`
    *   *Details:* Various permission checks and internal scene graph management routines have been touched, representing internal architectural tweaks or optimizations specific to our branch.

**5. Scripting Engine (Shared API)**
*   **Modified:** `ApiManager.cs`, `ScriptBase.cs`
    *   *Details:* Minor updates to the implementation and runtime execution environment for scripts.

**6. Binaries & Build Artifacts**
*   **Removed:** Various `.runtimeconfig.json` files and pre-compiled binaries in the `bin/` directory (`OpenSim`, `Robust`, `pCampBot`, `OpenSim.ConsoleClient`, etc.) are present in our branch's tracking history but do not (and should not) exist in the source-only upstream `master`.
