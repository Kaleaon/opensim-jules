using System;
using OpenMetaverse;
using OpenSim.Framework;
using OpenSim.Region.PhysicsModules.SharedBase;

namespace OpenSim.Region.PhysicsModule.PhysX
{
    public class PhysXPrim : PhysicsActor
    {
        private IntPtr _actor;
        private Vector3 _position;
        private Vector3 _size;
        private Quaternion _orientation;
        private bool _isPhysical;
        private uint _localID;

        public IntPtr PhysXActor => _actor;

        public PhysXPrim(IntPtr actor, uint localID, Vector3 position, Vector3 size, Quaternion rotation, bool isPhysical)
        {
            _actor = actor;
            _localID = localID;
            _position = position;
            _size = size;
            _orientation = rotation;
            _isPhysical = isPhysical;

            base.LocalID = localID; // PhysicsActor property
        }

        public override bool IsPhysical
        {
            get { return _isPhysical; }
            set { _isPhysical = value; }
        }

        public override Vector3 Position
        {
            get { return _position; }
            set
            {
                _position = value;
                // Update PhysX
                if (_actor != IntPtr.Zero)
                {
                    var transform = new PxTransform(_position, _orientation);
                    PhysXNative.PxRigidActorSetGlobalPose(_actor, ref transform);
                }
            }
        }

        public override Vector3 Size
        {
            get { return _size; }
            set { _size = value; }
        }

        public override Quaternion Orientation
        {
            get { return _orientation; }
            set
            {
                _orientation = value;
                // Update PhysX
                if (_actor != IntPtr.Zero)
                {
                    var transform = new PxTransform(_position, _orientation);
                    PhysXNative.PxRigidActorSetGlobalPose(_actor, ref transform);
                }
            }
        }

        public override Vector3 Velocity { get; set; }
        public override Vector3 Torque { get; set; }
        public override float CollisionScore { get; set; }
        public override Vector3 CenterOfMass { get { return Vector3.Zero; } } // Stub
        public override Vector3 Force { get; set; }
        public override float Mass { get; set; }
        public override bool Flying { get; set; }
        public override bool SetAlwaysRun { get; set; }
        public override bool ThrottleUpdates { get; set; }
        public override bool FloatOnWater { get; set; }
        public override Vector3 RotationalVelocity { get; set; }
        public override float Buoyancy { get; set; }

        public void UpdateFromPhysX()
        {
            if (_actor != IntPtr.Zero)
            {
                var transform = PhysXNative.PxRigidActorGetGlobalPose(_actor);
                _position = transform.p.ToVector3();
                _orientation = transform.q.ToQuaternion();

                // Also update velocity etc if we had bindings for it

                base.RequestPhysicsterseUpdate();
            }
        }

        public override void AddForce(Vector3 force, bool pushforce) { }
        public override void AddAngularForce(Vector3 force, bool pushforce) { }
        public override void SubscribeEvents(int ms) { }
        public override void UnSubscribeEvents() { }
        public override bool SubscribedEvents() { return false; }
    }
}
