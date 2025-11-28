using System;
using System.Collections.Generic;
using OpenMetaverse;
using OpenSim.Framework;

namespace OpenSim.Region.PhysicsModule.PhysX
{
    public class PhysXShapeManager
    {
        private IntPtr _physics;
        private IntPtr _defaultMaterial;

        public PhysXShapeManager(IntPtr physics)
        {
            _physics = physics;
            // Create default material: 0.5 friction, 0.5 restitution
            _defaultMaterial = PhysXNative.PxPhysicsCreateMaterial(_physics, 0.5f, 0.5f, 0.5f);
        }

        public IntPtr CreateGeometry(PrimitiveBaseShape pbs, Vector3 size)
        {
            // Simple mapping based on ProfileShape
            // Note: This is simplified. Real implementation needs to handle hollow, cut, etc.

            if (pbs.ProfileShape == ProfileShape.Circle)
            {
                // Sphere or Cylinder. Assuming Sphere if X=Y=Z roughly
                // But simplified: Use Sphere if PathCurve is default
                if (size.X == size.Y && size.Y == size.Z)
                {
                    return PhysXNative.PxCreateSphereGeometry(size.X * 0.5f);
                }
                // Fallback to box if not perfect sphere for now, or implement capsule/cylinder
                return PhysXNative.PxCreateBoxGeometry(size.X * 0.5f, size.Y * 0.5f, size.Z * 0.5f);
            }

            // Default to Box
            return PhysXNative.PxCreateBoxGeometry(size.X * 0.5f, size.Y * 0.5f, size.Z * 0.5f);
        }

        public IntPtr GetDefaultMaterial()
        {
            return _defaultMaterial;
        }
    }
}
