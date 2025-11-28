using System;
using System.Collections.Generic;
using OpenMetaverse;
using OpenSim.Framework;
using OpenSim.Region.PhysicsModules.SharedBase;

namespace OpenSim.Region.PhysicsModule.PhysX
{
    public class PhysXActorManager
    {
        private IntPtr _scene;
        private Dictionary<uint, PhysXPrim> _actors = new Dictionary<uint, PhysXPrim>();

        public PhysXActorManager(IntPtr scene)
        {
            _scene = scene;
        }

        public void AddActor(PhysXPrim prim)
        {
            if (prim.PhysXActor != IntPtr.Zero)
            {
                PhysXNative.PxSceneAddActor(_scene, prim.PhysXActor);
                lock (_actors)
                {
                    _actors[prim.LocalID] = prim;
                }
            }
        }

        public void RemoveActor(PhysXPrim prim)
        {
            if (prim.PhysXActor != IntPtr.Zero)
            {
                PhysXNative.PxSceneRemoveActor(_scene, prim.PhysXActor, true);
                lock (_actors)
                {
                    _actors.Remove(prim.LocalID);
                }
            }
        }

        public void UpdateToOpenSim()
        {
            // Iterate over active actors and update their position in OpenSim
            lock (_actors)
            {
                foreach (var prim in _actors.Values)
                {
                    if (prim.IsPhysical) // Only physical objects move
                    {
                        prim.UpdateFromPhysX();
                    }
                }
            }
        }
    }
}
