using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PS_Structures
{
    //Size 16
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct VRect
    {
        public int top;
        public int left;
        public int bottom;
        public int right;
    }
}