using System;
using System.Runtime.InteropServices;
using OpenMetaverse;

namespace OpenSim.Region.PhysicsModule.PhysX
{
    // PhysX types needed for P/Invoke
    [StructLayout(LayoutKind.Sequential)]
    public struct PxTolerancesScale
    {
        public float length;
        public float speed;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PxVec3
    {
        public float x, y, z;
        public PxVec3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public PxVec3(Vector3 v) { x = v.X; y = v.Y; z = v.Z; }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PxSceneDesc
    {
        public PxVec3 gravity;
        public PxVec3 boundsMin;
        public PxVec3 boundsMax;
        public IntPtr cpuDispatcher;
        public IntPtr filterShader;
        public uint flags;
    }

    public enum PxSceneFlag : uint
    {
        ENABLE_CCD = (1 << 2),
        ENABLE_STABILIZATION = (1 << 4)
    }

    public static class PhysXNative
    {
        // This DLL name would need to be correct for the platform.
        // For now we assume "PhysX_64" as per documentation, but in Linux it might be libPhysX_64.so
        // The DllImport will try to find it.
        private const string PhysXLibrary = "PhysX_64";

        // Foundation management
        [DllImport(PhysXLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr PxCreateFoundation(uint version,
            IntPtr allocator, IntPtr errorCallback);

        // Physics instance creation
        [DllImport(PhysXLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr PxCreatePhysics(uint version,
            IntPtr foundation, ref PxTolerancesScale scale,
            bool trackOutstandingAllocations);

        // Scene management
        [DllImport(PhysXLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr PxPhysicsCreateScene(IntPtr physics,
            ref PxSceneDesc sceneDesc);

        [DllImport(PhysXLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern void PxSceneSimulate(IntPtr scene, float timeStep, IntPtr scratchMemBlock, uint scratchMemBlockSize, bool controlSimulation);

        [DllImport(PhysXLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool PxSceneFetchResults(IntPtr scene, bool block, IntPtr errorState);

        [DllImport(PhysXLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern void PxSceneRelease(IntPtr scene);

        [DllImport(PhysXLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern void PxPhysicsRelease(IntPtr physics);

        [DllImport(PhysXLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern void PxFoundationRelease(IntPtr foundation);
    }
}
