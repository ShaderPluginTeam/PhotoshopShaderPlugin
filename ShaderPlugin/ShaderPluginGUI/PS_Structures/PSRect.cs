using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PS_Structures
{
    //Size 8
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 16)]
    public struct PSRect
    {
        public short top;
        public short left;
        public short bottom;
        public short right;
    }
}
