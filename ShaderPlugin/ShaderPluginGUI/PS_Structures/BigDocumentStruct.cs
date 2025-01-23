using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PS_Structures
{
    //Size 92
    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 92)]
    public struct BigDocumentStruct
    {
        public int PluginUsing32BitCoordinates;  /**< Set to nonzero by the plug-in if it is
		4											using the 32 bit fields. */

        public VPoint imageSize32;                 /**< Size of image in 32 bit coordinates. 
		8											Replaces \c FilterRecord::imageSize. */

        public VRect filterRect32;                 /**< Rectangle to filter in 32 bit coordinates. 
		16											Replaces \c FilterRecord::filterRect. */

        public VRect inRect32;                     /**< Requested input rectangle in 32 bit coordinates. 
		16											Replaces \c FilterRecord::inRect. */

        public VRect outRect32;                        /**< Requested output rectangle in 32 bit coordinates. 
		16											Replaces \c FilterRecord::outRect. */

        public VRect maskRect32;                       /**< Requested mask rectangle in 32 bit coordinates. 
		16										    Replaces \c FilterRecord::maskRect. */

        public VPoint floatCoord32;                    /**< Top left coordinate of selection in 32 bit coordinates. 
		8											Replaces \c FilterRecord::floatCoord. */

        public VPoint wholeSize32;					/**< Size of image the selection is over in 32 bit coordinates. 
		8											Replaces \c FilterRecord::wholeSize. */
    }
}
