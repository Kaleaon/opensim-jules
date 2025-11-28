using System;

namespace OpenSim.Region.PhysicsModule.PhysX
{
    // Managed resource wrapper
    public class PhysXFoundation : IDisposable
    {
        private IntPtr _foundation;
        private IntPtr _physics;
        private bool _disposed = false;

        public IntPtr Physics => _physics;
        public IntPtr Foundation => _foundation;

        // PX_PHYSICS_VERSION is typically (major<<24 + minor<<16 + bugfix<<8)
        // PhysX 5.1 would be 0x05010000?
        // Documentation used 0x40400000 which seems like an older version (4.something) or a placeholder.
        // Let's stick to what was in the doc for now, or check standard PhysX versioning.
        // 0x05040400 for 5.4.4 maybe?
        // The example had 0x40400000. I'll use that as per example, but it might need adjustment.
        private const uint PX_PHYSICS_VERSION = 0x40400000;

        public PhysXFoundation()
        {
            _foundation = PhysXNative.PxCreateFoundation(PX_PHYSICS_VERSION, IntPtr.Zero, IntPtr.Zero);
            if (_foundation == IntPtr.Zero)
            {
                throw new Exception("Failed to create PhysX Foundation");
            }

            var scale = new PxTolerancesScale { length = 1.0f, speed = 10.0f };
            _physics = PhysXNative.PxCreatePhysics(PX_PHYSICS_VERSION, _foundation, ref scale, false);
            if (_physics == IntPtr.Zero)
            {
                // Clean up foundation if physics creation fails
                PhysXNative.PxFoundationRelease(_foundation);
                _foundation = IntPtr.Zero;
                throw new Exception("Failed to create PhysX Physics instance");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (_physics != IntPtr.Zero)
                {
                    PhysXNative.PxPhysicsRelease(_physics);
                    _physics = IntPtr.Zero;
                }

                if (_foundation != IntPtr.Zero)
                {
                    PhysXNative.PxFoundationRelease(_foundation);
                    _foundation = IntPtr.Zero;
                }

                _disposed = true;
            }
        }

        ~PhysXFoundation()
        {
            Dispose(false);
        }
    }
}
