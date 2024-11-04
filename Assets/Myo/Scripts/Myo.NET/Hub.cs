using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Thalmic.Myo
{
    public class Hub : IDisposable
    {
        private static readonly DateTime TIMESTAMP_EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        private bool _disposed = false;
        private IntPtr _handle;

        private Thread _eventThread;
        private bool _eventThreadShutdown = false;

        private Dictionary<IntPtr, Myo> _myos = new Dictionary<IntPtr, Myo>();

        private Vector3 firstFirmwareVer = new Vector3(0f, 0f, 0f);
        private Vector3 secondFirmwareVer = new Vector3(0f, 0f, 0f);

        public Vector3 GetFirstFirmwareVer()
        {
            return firstFirmwareVer;
        }
        public Vector3 GetSecondFirmwareVer()
        {
            return secondFirmwareVer;
        }

        public Hub(string applicationIdentifier, EventHandler<MyoEventArgs> OnPaired)
        {
            if (OnPaired != null) {
                Paired += OnPaired;
            }

            if (libmyo.init_hub(out _handle, applicationIdentifier, IntPtr.Zero) != libmyo.Result.Success)
            {
                throw new InvalidOperationException("Unable to initialize Hub.");
            }

            // spawn the event thread
            StartEventThread();
        }

        // Deterministic destructor
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                StopEventThread();

                if (disposing)
                {
                    // free IDisposable managed objects (none right now)
                }

                // free unmanaged objects
                libmyo.shutdown_hub(_handle, IntPtr.Zero);

                _disposed = true;
            }
        }

        // Finalizer (non-deterministic)
        ~Hub()
        {
            Dispose(false);
        }

        public void SetLockingPolicy(LockingPolicy lockingPolicy)
        {
            libmyo.set_locking_policy(_handle, (libmyo.LockingPolicy)lockingPolicy, IntPtr.Zero);
        }

        public event EventHandler<MyoEventArgs> Paired;

        internal void StartEventThread()
        {
            _eventThreadShutdown = false;
            _eventThread = new Thread(new ThreadStart(EventThreadFn));
            _eventThread.Start();
        }

        internal void StopEventThread()
        {
            _eventThreadShutdown = true;
            if (_eventThread != null)
            {
                _eventThread.Join();
            }
        }

        private void EventThreadFn()
        {
            while (!_eventThreadShutdown)
            {
                GCHandle gch = GCHandle.Alloc(this);

                libmyo.run(_handle, 1000, (libmyo.Handler)HandleEvent, (IntPtr)gch, IntPtr.Zero);
            }
        }

        private static libmyo.HandlerResult HandleEvent(IntPtr userData, IntPtr evt)
        {
            GCHandle handle = (GCHandle)userData;
            Hub self = (Hub)handle.Target;

            var type = libmyo.event_get_type(evt);
            var timestamp = TIMESTAMP_EPOCH.AddMilliseconds(libmyo.event_get_timestamp(evt) / 1000);
            var myoHandle = libmyo.event_get_myo(evt);

            if (type == libmyo.EventType.Connected)
            {
                uint major = libmyo.event_get_firmware_version(evt, Thalmic.Myo.libmyo.VersionComponent.Major);
                uint minor = libmyo.event_get_firmware_version(evt, Thalmic.Myo.libmyo.VersionComponent.Minor);
                uint patch = libmyo.event_get_firmware_version(evt, Thalmic.Myo.libmyo.VersionComponent.Patch);
                // UnityEngine.Debug.Log("hub.cs major: " + major.ToString());
                // UnityEngine.Debug.Log("hub.cs minor: " + minor.ToString());
                // UnityEngine.Debug.Log("hub.cs patch: " + patch.ToString());

                // setting firmwares
                if (self.firstFirmwareVer.X == 0 &&
                    self.firstFirmwareVer.Y == 0 &&
                    self.firstFirmwareVer.Z == 0 &&
                    self.secondFirmwareVer.X == 0 &&
                    self.secondFirmwareVer.Y == 0 &&
                    self.secondFirmwareVer.Z == 0)
                {
                    // if both are empty, set the first one
                    self.firstFirmwareVer = new Vector3(major, minor, patch);
                } else if (self.firstFirmwareVer.X != 0 && 
                    self.firstFirmwareVer.Y != 0 &&
                    self.firstFirmwareVer.Z != 0 &&
                    self.secondFirmwareVer.X == 0 &&
                    self.secondFirmwareVer.Y == 0 &&
                    self.secondFirmwareVer.Z == 0)
                {
                    // if one is filled, set the second one
                    self.secondFirmwareVer = new Vector3(major, minor, patch);
                }
            }

            switch (type)
            {
                case libmyo.EventType.Paired:
                    var myo = new Myo(self, myoHandle);
                    self._myos.Add(myoHandle, myo);
                    if (self.Paired != null)
                    {
                        self.Paired(self, new MyoEventArgs(myo, DateTime.Now));
                    }
                    break;

                default:
                    System.Diagnostics.Debug.Assert(self._myos[myoHandle] != null);
                    self._myos[myoHandle].HandleEvent(type, timestamp, evt);
                    break;
            }

            return libmyo.HandlerResult.Continue;
        }
    }

    public enum LockingPolicy
    {
        None,
        Standard
    }
}
