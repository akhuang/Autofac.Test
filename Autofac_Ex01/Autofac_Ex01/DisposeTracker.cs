using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac_Ex01
{
    public class DisposeTracker : IDisposable
    {
        public event EventHandler<EventArgs> Disposing;

        public bool IsDisposed;

        public void Dispose()
        {
            IsDisposed = true; 

            if (Disposing != null)
                Disposing(this, EventArgs.Empty);
        }
    }
}
