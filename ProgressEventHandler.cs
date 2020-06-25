using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMViewerPlugin
{
    internal class ProgressEventArgs : EventArgs
    {
        public ProgressEventArgs() : base() { }

        public int Percent { get; set; }
    }
}
