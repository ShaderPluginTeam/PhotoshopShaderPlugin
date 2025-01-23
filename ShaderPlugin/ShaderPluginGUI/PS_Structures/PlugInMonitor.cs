using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PS_Structures
{
    //Size 40
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
    public struct PlugInMonitor
    {
        public Int32 gamma;      /**< The monitor’s gamma value or zero if the whole record is invalid. */
        public Int32 redX;       /**< The chromaticity coordinates of the monitor’s phosphors */
        public Int32 redY;       /**< The chromaticity coordinates of the monitor’s phosphors */
        public Int32 greenX;     /**< The chromaticity coordinates of the monitor’s phosphors */
        public Int32 greenY;     /**< The chromaticity coordinates of the monitor’s phosphors */
        public Int32 blueX;      /**< The chromaticity coordinates of the monitor’s phosphors */
        public Int32 blueY;      /**< The chromaticity coordinates of the monitor’s phosphors */
        public Int32 whiteX;     /**< The chromaticity coordinates of the monitor’s white point. */
        public Int32 whiteY;     /**< The chromaticity coordinates of the monitor’s white point. */
        public Int32 ambient;    /**< The relative amount of ambient light in the room. 
						   Zero means a relatively dark room, 0.5 means an average room, 
						   and 1.0 means a bright room. */
    }
}
