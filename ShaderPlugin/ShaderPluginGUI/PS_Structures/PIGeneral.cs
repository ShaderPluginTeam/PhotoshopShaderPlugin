using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PS_Structures
{
    public delegate Int16 DisplayPixelsProc(ref PSPixelMap source,
        ref VRect srcVRect,
        Int32 dstRow,
        Int32 dstCol,
        IntPtr platformContext);

    public delegate Int16 AdvanceStateProc();
    public delegate byte TestAbortProc();
    public delegate void ProgressProc(Int32 done, Int32 total);
    public delegate void HostProc(Int16 selector, IntPtr data);
    public delegate void ProcessEventProc(IntPtr eventPtr);
 
    public delegate Int16 ColorServicesProc(IntPtr infoPtr);
    public delegate Int16 ColorServicesProcM(ref ColorServicesInfo infoPtr);
 

    public struct PSPixelMap
    {
        public Int32 version;      /**< Set to 1. Future versions of Photoshop may support additional 
						* parameters and will support higher version numbers for \c PSPixelMap. */
        public VRect bounds;       /**< The bounds for the pixel map. */
        public Int32 imageMode;    /**< The mode for the image data. The supported modes are grayscale, 
						* RGB, CMYK, and Lab. See @ref ImageModes "Image Modes" for values. Additionally, if the 
						* mode of the document being 
						* processed is DuotoneMode or IndexedMode, you can pass \c plugInModeDuotone 
						* or \c plugInModeIndexedColor. */
        public Int32 rowBytes;     /**< The offset from one row to the next of pixels. */
        public Int32 colBytes;     /**< The offset from one column to the next of pixels. */
        public Int32 planeBytes;   /**< The offset from one plane of pixels to the next. In RGB, the planes are 
						* ordered red, green, blue; in CMYK, the planes are ordered cyan, magenta, 
						* yellow, black; in Lab, the planes are ordered L, a, b. */
        public IntPtr baseAddr;     /**< The address of the byte value for the first plane of the top left pixel. */

        //---------------------------------------------------------------------------
        // Fields new in version 1:
        //---------------------------------------------------------------------------	
        ///@name New in version 1 
        //@{
        public IntPtr mat_PSPixelMask;   /**< A pixel mask to use for matting correction. Can be specified in 
	                    * all modes except indexed color. For example, if you have white matted 
						* data to display, you can specify a mask in this field  
						* to remove the white fringe. This field points to a \c PSPixelMask structure 
						* with a \c maskDescription that indicates what type of matting needs 
						* to be compensated for. If this field is NULL, Photoshop performs no matting 
						* compensation. If the masks are chained, only the first mask in the chain is used. */
        public IntPtr masks_PSPixelMask; /**< A pointer to a chain of \c PSPixelMasks that are multiplied together 
						(with the possibility of inversion) to establish which areas of the image are 
						transparent and should have the checkerboard displayed. /c kSimplePSMask, /c kBlackMatPSMask, 
						/c kWhiteMatPSMask, and /c kGrayMatPSMask all operate such that 255=opaque and 0=transparent. 
						/c kInvertPSMask has 255=transparent and 0=opaque. */

        // Use to set the phase of the checkerboard:
        public Int32 maskPhaseRow; /**< The phase of the checkerboard with respect to the top left corner of the /c PSPixelMap */
        public Int32 maskPhaseCol; /**< The phase of the checkerboard with respect to the top left corner of the /c PSPixelMap */
        //@}
        //---------------------------------------------------------------------------
        // Fields new in version 2:
        //---------------------------------------------------------------------------	
        ///@name New in version 2 
        //@{

        public IntPtr pixelOverlays;
        public UInt32 colorManagementOptions; /**< Options for color management.  See @ref ColorManagement */

        //@}
    }

    public struct PSPixelMask
    {
        public IntPtr next_PSPixelMask; /**< A pointer to the next mask in the chain. */
        public IntPtr maskData;             /**< A pointer to the mask data. */
        public Int32 rowBytes;             /**< The row step for the mask. */
        public Int32 colBytes;             /**< The column step for the mask.*/
        public Int32 maskDescription;      /**< The mask description value.
								     See @ref MaskDescription "Mask Description Constants" for values.	  
								     */
    }
}
