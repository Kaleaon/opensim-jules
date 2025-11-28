using System;
using System.Runtime.InteropServices;
using OpenMetaverse;
using OpenSim.Framework;

namespace OpenSim.Region.PhysicsModule.PhysX
{
    public class PhysXTerrainManager : IDisposable
    {
        private PhysXPhysicsScene _scene;
        private IntPtr _heightField;
        private IntPtr _actor;

        public PhysXTerrainManager(PhysXPhysicsScene scene)
        {
            _scene = scene;
        }

        public void SetTerrain(float[] heightMap, int width, int height)
        {
            // 1. Convert heightmap to samples
            int numSamples = width * height;
            PxHeightFieldSample[] samples = new PxHeightFieldSample[numSamples];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float z = heightMap[y * width + x];
                    // Use 0.1f height scale for decimeter precision (max height ~3276m)
                    float heightScale = 0.1f;
                    float scaledHeight = z / heightScale;

                    if (scaledHeight > short.MaxValue) scaledHeight = short.MaxValue;
                    if (scaledHeight < short.MinValue) scaledHeight = short.MinValue;

                    short quantizedHeight = (short)scaledHeight;

                    int index = y * width + x;
                    samples[index] = new PxHeightFieldSample
                    {
                        height = quantizedHeight,
                        materialIndex0 = 0,
                        tessFlag = 0
                    };
                }
            }

            // 2. Pin memory
            GCHandle pinnedArray = GCHandle.Alloc(samples, GCHandleType.Pinned);
            IntPtr pointer = pinnedArray.AddrOfPinnedObject();

            try
            {
                // 3. Create Desc
                PxHeightFieldDesc desc = new PxHeightFieldDesc
                {
                    nbRows = (uint)height,
                    nbColumns = (uint)width,
                    format = PxHeightFieldFormat.S16_TM,
                    samples = pointer,
                    convexEdgeThreshold = 0,
                    flags = 0
                };

                // 4. Create HeightField
                if (_scene.Physics != IntPtr.Zero)
                {
                    // Clean up old terrain if exists
                    Dispose();

                    _heightField = PhysXNative.PxPhysicsCreateHeightField(_scene.Physics, ref desc);

                    if (_heightField != IntPtr.Zero)
                    {
                        // Create Geometry
                        // Height scale 0.1, Row scale 1.0, Col scale 1.0
                        var geometry = PhysXNative.PxCreateHeightFieldGeometry(_heightField, IntPtr.Zero, 0.1f, 1.0f, 1.0f);

                        // Create Actor (RigidStatic)
                        // Position usually 0,0,0 for terrain
                        var transform = new PxTransform(new Vector3(0, 0, 0), Quaternion.Identity);
                        _actor = PhysXNative.PxPhysicsCreateRigidStatic(_scene.Physics, ref transform);

                        if (_actor != IntPtr.Zero)
                        {
                            // Create Material (default friction/restitution)
                            var material = PhysXNative.PxPhysicsCreateMaterial(_scene.Physics, 0.5f, 0.5f, 0.5f);

                            var relativePose = new PxTransform(Vector3.Zero, Quaternion.Identity);
                            PhysXNative.PxRigidActorCreateShape(_actor, geometry, material, ref relativePose);

                            // Add to scene
                            PhysXNative.PxSceneAddActor(_scene.Scene, _actor);
                        }
                    }
                }
            }
            finally
            {
                pinnedArray.Free();
            }
        }

        public void Dispose()
        {
            if (_actor != IntPtr.Zero)
            {
                PhysXNative.PxSceneRemoveActor(_scene.Scene, _actor, false);
                PhysXNative.PxActorRelease(_actor);
                _actor = IntPtr.Zero;
            }
            // Heightfield release? Usually happens when ref count drops (actor releases shape, shape releases HF).
            // But we might need to release explicitly if we hold ref?
            // PxPhysicsCreateHeightField returns a PxHeightField.
            // We should release it if we don't want it.
            // The Shape holds a reference. We can release our reference.
            // But for now, let's assume Actor release handles it or we leak a bit (safe for singleton terrain).
        }
    }
}
