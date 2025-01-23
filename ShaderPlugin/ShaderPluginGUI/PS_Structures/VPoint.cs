using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PS_Structures
{
    //Size 8
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct VPoint
    {
        public int v;
        public int h;
    }
}
