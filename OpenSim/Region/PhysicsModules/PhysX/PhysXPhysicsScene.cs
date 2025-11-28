using System;
using System.Collections.Generic;
using Mono.Addins;
using Nini.Config;
using OpenMetaverse;
using OpenSim.Framework;
using OpenSim.Region.Framework.Interfaces;
using OpenSim.Region.Framework.Scenes;
using OpenSim.Region.PhysicsModules.SharedBase;

namespace OpenSim.Region.PhysicsModule.PhysX
{
    [Extension(Path = "/OpenSim/RegionModules", NodeName = "RegionModule", Id = "PhysXPhysicsScene")]
    public class PhysXPhysicsScene : PhysicsScene, INonSharedRegionModule
    {
        private IntPtr _pxScene;
        private PhysXFoundation _foundation;
        private PhysXShapeManager _shapeManager;
        private PhysXActorManager _actorManager;
        private bool _enabled = false;
        private IConfigSource _config;
        private string _regionName;

        public void Initialise(IConfigSource config)
        {
            IConfig startupConfig = config.Configs["Startup"];
            if (startupConfig != null)
            {
                string physics = startupConfig.GetString("physics", string.Empty);
                if (physics == Name)
                {
                    _enabled = true;
                    _config = config;
                }
            }
        }

        public void AddRegion(Scene scene)
        {
            if (!_enabled) return;

            _regionName = scene.RegionInfo.RegionName;
            scene.RegisterModuleInterface<PhysicsScene>(this);

            base.Initialise(scene.PhysicsRequestAsset,
                (scene.Heightmap != null ? scene.Heightmap.GetFloatsSerialised() : new float[scene.RegionInfo.RegionSizeX * scene.RegionInfo.RegionSizeY]),
                (float)scene.RegionInfo.RegionSettings.WaterHeight);

            // Initialize physics engine
            InitializePhysics();
        }

        public void RemoveRegion(Scene scene)
        {
            if (!_enabled) return;
        }

        public void RegionLoaded(Scene scene)
        {
            if (!_enabled) return;
        }

        public void Close()
        {
            Dispose();
        }

        public string Name => "PhysX";

        public Type ReplaceableInterface => null;

        private void InitializePhysics()
        {
            try
            {
                _foundation = new PhysXFoundation();

                var sceneDesc = new PxSceneDesc
                {
                    gravity = new PxVec3(0, 0, -9.81f), // Default gravity
                    flags = (uint)(PxSceneFlag.ENABLE_CCD | PxSceneFlag.ENABLE_STABILIZATION)
                };

                // In a real implementation we would set cpuDispatcher here

                _pxScene = PhysXNative.PxPhysicsCreateScene(_foundation.Physics, ref sceneDesc);

                if (_pxScene == IntPtr.Zero)
                    throw new Exception("Failed to create PhysX scene");

                _shapeManager = new PhysXShapeManager(_foundation.Physics);
                _actorManager = new PhysXActorManager(_pxScene);
            }
            catch (Exception e)
            {
                // Log error
                Console.WriteLine($"[PhysX] Failed to initialize: {e.Message}");
                _enabled = false;
            }
        }

        public override float Simulate(float timeStep)
        {
            if (_pxScene == IntPtr.Zero) return 0;

            try
            {
                PhysXNative.PxSceneSimulate(_pxScene, timeStep, IntPtr.Zero, 0, true);
                PhysXNative.PxSceneFetchResults(_pxScene, true, IntPtr.Zero);

                _actorManager.UpdateToOpenSim();
                return 1.0f; // Return hypothetical frames simulated
            }
            catch (Exception e)
            {
                Console.WriteLine($"[PhysX] Simulation failed: {e.Message}");
                return 0;
            }
        }

        public override PhysicsActor AddAvatar(string avName, Vector3 position, Vector3 velocity, Vector3 size, bool isFlying)
        {
            // Stub for now. Avatars typically need character controllers (PxController).
            return null;
        }

        public override void RemoveAvatar(PhysicsActor actor)
        {
            // Stub
        }

        public override void RemovePrim(PhysicsActor prim)
        {
            if (_actorManager != null && prim is PhysXPrim pxPrim)
            {
                _actorManager.RemoveActor(pxPrim);
            }
        }

        public override PhysicsActor AddPrimShape(string primName, PrimitiveBaseShape pbs, Vector3 position, Vector3 size, Quaternion rotation, bool isPhysical, uint localid)
        {
            if (_pxScene == IntPtr.Zero) return null;

            try
            {
                // Create Geometry
                var geometry = _shapeManager.CreateGeometry(pbs, size);
                if (geometry == IntPtr.Zero) return null;

                // Create Material
                var material = _shapeManager.GetDefaultMaterial();

                // Create Transform
                var transform = new PxTransform(position, rotation);

                // Create Actor (RigidStatic or RigidDynamic)
                IntPtr actor;
                if (isPhysical)
                {
                    actor = PhysXNative.PxPhysicsCreateRigidDynamic(_foundation.Physics, ref transform);
                }
                else
                {
                    actor = PhysXNative.PxPhysicsCreateRigidStatic(_foundation.Physics, ref transform);
                }

                if (actor == IntPtr.Zero) return null;

                // Create Shape and attach to Actor
                var relativePose = new PxTransform(Vector3.Zero, Quaternion.Identity);
                var shape = PhysXNative.PxRigidActorCreateShape(actor, geometry, material, ref relativePose);

                // Create PhysXPrim wrapper
                var prim = new PhysXPrim(actor, localid, position, size, rotation, isPhysical);

                // Add to Manager/Scene
                _actorManager.AddActor(prim);

                return prim;
            }
            catch (Exception e)
            {
                Console.WriteLine($"[PhysX] AddPrimShape failed: {e.Message}");
                return null;
            }
        }

        public override void SetTerrain(float[] heightMap)
        {
            // Stub. Requires PxHeightField.
        }

        public override void SetWaterLevel(float baseheight)
        {
            // Stub
        }

        public override void DeleteTerrain()
        {
            // Stub
        }

        public override void Dispose()
        {
            if (_pxScene != IntPtr.Zero)
            {
                PhysXNative.PxSceneRelease(_pxScene);
                _pxScene = IntPtr.Zero;
            }

            if (_foundation != null)
            {
                _foundation.Dispose();
                _foundation = null;
            }
        }

        public override Dictionary<uint, float> GetTopColliders()
        {
            return new Dictionary<uint, float>();
        }
    }
}
