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

        public Vector3 ToVector3() { return new Vector3(x, y, z); }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PxQuat
    {
        public float x, y, z, w;
        public PxQuat(float x, float y, float z, float w) { this.x = x; this.y = y; this.z = z; this.w = w; }
        public PxQuat(Quaternion q) { x = q.X; y = q.Y; z = q.Z; w = q.W; }

        public Quaternion ToQuaternion() { return new Quaternion(x, y, z, w); }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PxTransform
    {
        public PxVec3 p;
        public PxQuat q;
        public PxTransform(Vector3 pos, Quaternion rot)
        {
            p = new PxVec3(pos);
            q = new PxQuat(rot);
        }
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

    public enum PxHeightFieldFormat
    {
        S16_TM = 1 // 16-bit signed integer height-field samples
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PxHeightFieldSample
    {
        public short height;
        public byte materialIndex0;
        public byte tessFlag;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PxHeightFieldDesc
    {
        public uint nbRows;
        public uint nbColumns;
        public PxHeightFieldFormat format;
        public IntPtr samples; // pointer to PxHeightFieldSample array
        public float convexEdgeThreshold;
        public uint flags;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PxCapsuleControllerDesc
    {
        public PxVec3 position;
        public PxVec3 upDirection;
        public float slopeLimit;
        public float invisibleWallHeight;
        public float maxJumpHeight;
        public float contactOffset;
        public float stepOffset;
        public float density;
        public float scaleCoeff;
        public float volumeGrowth;
        public IntPtr reportCallback;
        public IntPtr behaviorCallback;
        public float radius;
        public float height;
        public IntPtr climbingMode;
        public IntPtr material;
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

        // Material
        [DllImport(PhysXLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr PxPhysicsCreateMaterial(IntPtr physics, float staticFriction, float dynamicFriction, float restitution);

        // Geometry Creation
        [DllImport(PhysXLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr PxCreateBoxGeometry(float hx, float hy, float hz);

        [DllImport(PhysXLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr PxCreateSphereGeometry(float ir);

        [DllImport(PhysXLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr PxCreateCapsuleGeometry(float radius, float halfHeight);

        // Rigid Body Creation
        [DllImport(PhysXLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr PxPhysicsCreateRigidDynamic(IntPtr physics, ref PxTransform transform);

        [DllImport(PhysXLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr PxPhysicsCreateRigidStatic(IntPtr physics, ref PxTransform transform);

        // Shape Management
        [DllImport(PhysXLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr PxRigidActorCreateShape(IntPtr actor, IntPtr geometry, IntPtr material, ref PxTransform relativePose);

        [DllImport(PhysXLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern void PxShapeRelease(IntPtr shape);

        // Actor Management
        [DllImport(PhysXLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern void PxSceneAddActor(IntPtr scene, IntPtr actor);

        [DllImport(PhysXLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern void PxSceneRemoveActor(IntPtr scene, IntPtr actor, bool wakeOnLostTouch);

        [DllImport(PhysXLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern void PxActorRelease(IntPtr actor);

        // Transforms
        [DllImport(PhysXLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern void PxRigidActorSetGlobalPose(IntPtr actor, ref PxTransform transform);

        [DllImport(PhysXLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern PxTransform PxRigidActorGetGlobalPose(IntPtr actor);

        // HeightField
        [DllImport(PhysXLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr PxCreateHeightFieldGeometry(IntPtr hf, IntPtr meshFlags, float heightScale, float rowScale, float columnScale);

        [DllImport(PhysXLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr PxCookingCreateHeightField(IntPtr cooking, ref PxHeightFieldDesc desc, IntPtr physicsInsertionCallback);

        [DllImport(PhysXLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr PxPhysicsCreateHeightField(IntPtr physics, ref PxHeightFieldDesc desc);

        [DllImport(PhysXLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr PxCreateCooking(uint version, IntPtr foundation, IntPtr paramsDesc);

        // Note: PxCookingCreateHeightField usually requires PhysXCooking library.
        // We might need to wrap PxCooking calls separately or assume they are exported.
        // If not exported directly, we assume a wrapper DLL.
        // For now, let's assume we can create heightfields.

        // Character Controller
        [DllImport(PhysXLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr PxCreateControllerManager(IntPtr scene, bool lockingEnabled);

        [DllImport(PhysXLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr PxControllerManagerCreateController(IntPtr manager, ref PxCapsuleControllerDesc desc);

        [DllImport(PhysXLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern void PxControllerManagerRelease(IntPtr manager);

        [DllImport(PhysXLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern void PxControllerMove(IntPtr controller, ref PxVec3 displacement, float minDist, float elapsedTime, IntPtr filters, IntPtr obstacles);

        [DllImport(PhysXLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern PxVec3 PxControllerGetPosition(IntPtr controller);

        [DllImport(PhysXLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern void PxControllerSetPosition(IntPtr controller, ref PxVec3 position);

        [DllImport(PhysXLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern void PxControllerRelease(IntPtr controller);
    }
}
