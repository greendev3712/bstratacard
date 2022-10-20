using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Lib {
    public class TryLock : IDisposable {
        private object locked;
        private object thing_to_lock;

        public bool HasLock { get; private set; }

        public TryLock(object obj) {
            thing_to_lock = obj;
        }

        public bool GetLock() {
            if (Monitor.TryEnter(thing_to_lock, 500)) {
                HasLock = true;
                locked = thing_to_lock;
            }

            return HasLock;
        }

        public void Dispose() {
            if (HasLock) {
                Monitor.Exit(locked);
                locked = null;
                HasLock = false;
            }
        }
    }
}
