# OpenSimulator to Godot Port: Detailed Plan

This document provides a detailed, granular breakdown of the tasks required to port the OpenSimulator platform to the Godot Engine. It is an extension of the high-level roadmap.

## Phase 1: Core Systems & Headless Server

**Goal:** Establish a minimal, headless OpenSim region server running within a Godot project. The server should be capable of managing core simulation loops and handling basic client connections from a standard viewer.

---

### 1.1. Project Setup & Configuration

*   **1.1.1. Create Godot Project:**
    *   Action: Create a new Godot project.
    *   Specification: Enable .NET/C# support during creation.
    *   Verification: A `.sln` and `.csproj` file are present in the project root.

*   **1.1.2. Configure for Headless Mode:**
    *   Action: Research and apply settings for a headless server build.
    *   Investigation:
        *   Check Godot documentation for command-line flags to run in headless mode (e.g., `--headless`).
        *   Investigate if a custom export template without rendering/audio features is needed for optimized server builds.
    *   Implementation: Create a script (`run_server.sh` or similar) that starts the Godot executable with the correct headless flags.

*   **1.1.3. Version Control Setup:**
    *   Action: Initialize version control for the new project.
    *   Specification: Ensure the `.godot/` directory and other transient files are included in `.gitignore`.

### 1.2. Port Core OpenSimulator Modules

*   **1.2.1. Identify Core Source Files:**
    *   Action: Analyze the original OpenSimulator solution and identify the minimal set of C# projects required for a basic region server.
    *   Initial Project List:
        *   `OpenSim.Framework`
        *   `OpenSim.Framework.Console`
        *   `OpenSim.Framework.Servers.HttpServer`
        *   `OpenSim.Region.Framework`
        *   `OpenSim.Services.Interfaces`
        *   Dependencies like `Nini` and `log4net`.

*   **1.2.2. Create Core Library:**
    *   Action: Create a new C# library project within the Godot solution.
    *   Name: `OpenSim.Godot.Core`
    *   Purpose: This will contain all the ported business logic from OpenSimulator, keeping it separate from the Godot-specific "glue" code.

*   **1.2.3. Migrate and Refactor Code:**
    *   Action: Copy the source files from the projects identified in 1.2.1 into the `OpenSim.Godot.Core` library.
    *   Refactoring tasks:
        *   Fix all compilation errors resulting from the move.
        *   Adjust namespaces to be consistent with the new project structure.
        *   Replace .NET Framework-specific API calls with .NET 8 equivalents.
        *   Stub out or remove code that is not essential for the initial headless server (e.g., the interactive console, platform-specific utilities).

*   **1.2.4. Validate Configuration System:**
    *   Action: Ensure the `Nini` configuration library functions correctly.
    *   Tasks:
        *   Verify that `OpenSim.ini` can be loaded and read at runtime.
        *   Adapt file paths to work within the Godot environment (e.g., using `OS.GetExecutablePath()` to find the config file).

### 1.3. Dependency Management

*   **1.3.1. Identify Third-Party Libraries:**
    *   Action: List all third-party DLLs that the core modules depend on.
    *   Examples: `Nini.dll`, `log4net.dll`, `XMLRPC.dll`.

*   **1.3.2. Migrate to NuGet:**
    *   Action: Replace direct DLL references with NuGet package references.
    *   Tasks:
        *   For each library, find a compatible .NET 8 version on NuGet.
        *   Add the package reference to the `OpenSim.Godot.Core.csproj` file.
        *   Remove the old `<Reference>` tags from the `.csproj` file.

*   **1.3.3. Handle Missing Packages:**
    *   Action: Formulate a strategy for any libraries that are not available on NuGet.
    *   Options:
        1.  Find a modern, alternative library.
        2.  If the source code is available, attempt to port it.
        3.  For non-critical features, stub out the functionality for now.

### 1.4. Godot Headless Server Implementation

*   **1.4.1. Create Server Scene:**
    *   Action: Create a new Godot scene named `RegionServer.tscn`.
    *   Action: Attach a C# script, `RegionServer.cs`, to the root node.

*   **1.4.2. Integrate OpenSim Main Loop:**
    *   Action: Instantiate and start the OpenSimulator application from within the Godot script.
    *   Implementation:
        *   In the `_Ready()` method of `RegionServer.cs`, create an instance of the main `OpenSim.Application` class.
        *   Modify the `OpenSim.Application` startup sequence to bypass any console or UI initialization.
        *   Use Godot's `_Process()` method to call the main update tick of the OpenSim engine, effectively using Godot as the host for the simulation loop.

### 1.5. Network Layer Replacement

*   **1.5.1. Create Godot Network Server:**
    *   Action: Implement a new network server class that uses Godot's networking API.
    *   Name: `GodotNetworkServer.cs`
    *   Specification: This class must implement the relevant server interfaces from `OpenSim.Framework` so it can be integrated into the core application.

*   **1.5.2. Implement ENet Server:**
    *   Action: Use Godot's `MultiplayerPeer` to create a server.
    *   Implementation:
        *   In `GodotNetworkServer`, instantiate an `ENetMultiplayerPeer`.
        *   Call `create_server()` to start listening for connections.
        *   Connect to `MultiplayerAPI.PeerConnected` and `MultiplayerAPI.PeerDisconnected` signals to manage the lifecycle of client connections.

*   **1.5.3. Replace Original Server:**
    *   Action: Modify the OpenSim application startup logic.
    *   Task: Ensure that the `GodotNetworkServer` is used instead of the default `HttpServer` and UDP servers. This will likely involve changing the code where the `ISimulationBase` is initialized.

### 1.6. Implement Basic Viewer Login

*   **1.6.1. Handle HTTP Login (if necessary):**
    *   Action: Decide on a strategy for the initial HTTP-based login used by standard viewers.
    *   Option A (Full Compatibility): Use C#'s built-in `HttpClient` or Godot's `HTTPRequest` node to create a minimal HTTP listener that handles the `login_to_simulator` request and responds with the region's address and circuit information.
    *   Option B (Bypass for PoC): Modify a viewer's source code to skip the HTTP login and connect directly to the Godot server's IP and port. (This is faster for a proof-of-concept but less compatible).

*   **1.6.2. Implement UDP Handshake:**
    *   Action: Handle the initial UDP messages from the viewer.
    *   Implementation:
        *   Create an RPC function in `GodotNetworkServer.cs` to handle the `UseCircuitCode` message from the viewer.
        *   When this message is received, validate the circuit code and move the client to an "authenticated" state.

*   **1.6.3. Send Region Handshake:**
    *   Action: Send the initial world state information to the newly connected client.
    *   Task: Implement the logic to send the sequence of packets that a viewer expects after a successful login (e.g., `RegionHandshake`, `LayerData`, etc.). This will allow the viewer to render the initial scene.
