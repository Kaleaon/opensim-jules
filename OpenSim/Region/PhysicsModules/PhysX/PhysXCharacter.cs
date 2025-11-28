using System;
using OpenMetaverse;
using OpenSim.Framework;
using OpenSim.Region.PhysicsModules.SharedBase;

namespace OpenSim.Region.PhysicsModule.PhysX
{
    public class PhysXCharacter : PhysicsActor
    {
        private IntPtr _controller;
        private IntPtr _manager;
        private Vector3 _position;
        private Vector3 _size;
        private Vector3 _velocity;
        private bool _flying;
        private bool _isPhysical;

        public IntPtr Controller => _controller;

        public PhysXCharacter(IntPtr manager, uint localID, string name, Vector3 position, Vector3 size)
        {
            _manager = manager;
            _position = position;
            _size = size;
            _isPhysical = true;

            // Create Controller
            if (_manager != IntPtr.Zero)
            {
                // Create desc
                // Assuming defaults for callbacks for now (IntPtr.Zero)
                PxCapsuleControllerDesc desc = new PxCapsuleControllerDesc
                {
                    position = new PxVec3(position),
                    height = size.Z,
                    radius = size.X * 0.5f,
                    stepOffset = 0.5f,
                    upDirection = new PxVec3(0, 0, 1), // Z up
                    slopeLimit = 0.707f, // 45 degrees
                    contactOffset = 0.1f,
                    density = 10.0f,
                    scaleCoeff = 0.8f,
                    volumeGrowth = 1.5f,
                    reportCallback = IntPtr.Zero,
                    behaviorCallback = IntPtr.Zero,
                    climbingMode = IntPtr.Zero,
                    material = IntPtr.Zero // Should use default material
                };

                _controller = PhysXNative.PxControllerManagerCreateController(_manager, ref desc);
                if (_controller == IntPtr.Zero)
                {
                    // Fallback or error
                    Console.WriteLine("[PhysX] Failed to create character controller");
                }
            }

            base.LocalID = localID;
        }

        public override Vector3 Position
        {
            get
            {
                if (_controller != IntPtr.Zero)
                {
                    var pos = PhysXNative.PxControllerGetPosition(_controller);
                    _position = pos.ToVector3();
                }
                return _position;
            }
            set
            {
                _position = value;
                if (_controller != IntPtr.Zero)
                {
                    var pos = new PxVec3(value);
                    PhysXNative.PxControllerSetPosition(_controller, ref pos);
                }
            }
        }

        public override Vector3 Velocity
        {
            get { return _velocity; }
            set { _velocity = value; }
        }

        public override Vector3 Size
        {
            get { return _size; }
            set { _size = value; }
        }

        public override bool IsPhysical
        {
            get { return _isPhysical; }
            set { _isPhysical = value; }
        }

        public override bool Flying
        {
            get { return _flying; }
            set { _flying = value; }
        }

        // Apply movement for this frame
        public void Move(float timeStep)
        {
            if (_controller != IntPtr.Zero && _velocity != Vector3.Zero)
            {
                // Displacement = velocity * time
                var disp = new PxVec3(_velocity * timeStep);

                // Add gravity if not flying
                if (!_flying)
                {
                    disp.z -= 9.81f * timeStep * timeStep; // Simple gravity approximation for displacement
                }

                PhysXNative.PxControllerMove(_controller, ref disp, 0.001f, timeStep, IntPtr.Zero, IntPtr.Zero);
            }
        }

        public override Vector3 Torque { get; set; }
        public override float CollisionScore { get; set; }
        public override Vector3 CenterOfMass { get { return Position; } }
        public override Vector3 Force { get; set; }
        public override float Mass { get; set; }
        public override Quaternion Orientation { get; set; }
        public override bool SetAlwaysRun { get; set; }
        public override bool ThrottleUpdates { get; set; }
        public override bool FloatOnWater { get; set; }
        public override Vector3 RotationalVelocity { get; set; }
        public override float Buoyancy { get; set; }

        public void UpdateFromPhysX()
        {
            if (_controller != IntPtr.Zero)
            {
                var pos = PhysXNative.PxControllerGetPosition(_controller);
                Vector3 newPos = pos.ToVector3();

                // Check if moved significantly
                if (Vector3.DistanceSquared(_position, newPos) > 0.0001f)
                {
                    _position = newPos;
                    base.RequestPhysicsterseUpdate();
                }
            }
        }

        public override void AddForce(Vector3 force, bool pushforce) { }
        public override void AddAngularForce(Vector3 force, bool pushforce) { }
        public override void SubscribeEvents(int ms) { }
        public override void UnSubscribeEvents() { }
        public override bool SubscribedEvents() { return false; }
    }
}
