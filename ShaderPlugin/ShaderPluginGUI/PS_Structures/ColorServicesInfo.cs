using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PS_Structures
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 8)]
    public struct ColorServicesInfo
    {

        public static ColorServicesInfo Instantiate()
        {
            ColorServicesInfo info = new ColorServicesInfo();
            info.colorComponents = new short[] { 0, 0, 0, 0 };
            info.selector = ColorServicesConsts.plugIncolorServicesChooseColor;
            info.sourceSpace = ColorServicesConsts.plugIncolorServicesRGBSpace;
            info.resultSpace = ColorServicesConsts.plugIncolorServicesRGBSpace;
            info.reservedSourceSpaceInfo = IntPtr.Zero; // must be NULL
            info.reservedResultSpaceInfo = IntPtr.Zero; // must be NULL

            info.infoSize = Marshal.SizeOf(info);

            info.resultGamutInfoValid = 1;
            info.resultInGamut = 1;

            info.reserved = IntPtr.Zero; // Must be NULL
            info.selectorParameter.specialColorID = ColorServicesConsts.plugIncolorServicesForegroundColor;
            return info;
        }

        public Int32 infoSize;        /**< Size of the ColorServicesInfo record in bytes. 
	                          		  *	 The value is used as a version identifier in case this 
	                          		  *	 record is expanded in the future. It can be filled in as follows:
									  *	 \code
									  *	 ColorServicesInfo requestInfo;
									  *	 requestInfo.infoSize = sizeof(requestInfo); \endcode 
									  */
        public Int16 selector;        /**< Operation performed by the ColorServices callback.
									  * See @ref ColorServiceSelectors.
									  */
        public Int16 sourceSpace;     /**< Indicates the color space of the input color contained in 
	                                  * @ref colorComponents.
									  * For \c plugIncolorServicesChooseColor the input color is used 
									  * as an initial value for the picker.
									  * For plugIncolorServicesConvertColor the input color is converted 
									  * from the color space indicated by \c sourceSpace to the one indicated by 
									  * @ref resultSpace.  See @ref ColorSpace for values.
									  */
        public Int16 resultSpace;     /**<
								 	  * Desired color space of the result color. The result is contained in the 
								 	  * @ref colorComponents field.	For the \c plugIncolorServicesChooseColor selector, 
								 	  * \c resultSpace can be set to \c -1=plugIncolorServicesChosenSpace to return the 
								 	  * color in whichever color space the user chose. In that case, \c resultSpace 
								 	  * contains the chosen color space on output.	See @ref ColorSpace for values.
									  */
        [MarshalAs(UnmanagedType.U1)]
        public byte resultGamutInfoValid;    /**< This output only field indicates whether the @ref resultInGamut field has 
									  * been set. In Photoshop 3.0 and later, this is only  true for colors returned 
									  * in the \c plugIncolorServicesCMYKSpace color space. 
									  */
        [MarshalAs(UnmanagedType.U1)]
        public byte resultInGamut;   /**< Indicates whether the returned color is in gamut for the currently selected 
									 * printing setup. Only meaningful if @ref resultGamutInfoValid=TRUE.
									 */

        // Both voids must be NULL or will return paramErr:
        public IntPtr reservedSourceSpaceInfo;  /**< Must be NULL, otherwise returns parameter error. */
        public IntPtr reservedResultSpaceInfo;  /**< Must be NULL, otherwise returns parameter error. */

        /** Actual color components of the input or output color. The values of the array
         * depend on the color space.
         * <TABLE border="1"
         *      summary="colorComponents array structure.">
         * <CAPTION><EM>colorComponents array structure</EM></CAPTION>
         * <TR><TH>Color Space<TH>[0]<TH>[1]<TH>[2]<TH>[3]
         * <TR><TH>RGB<TD>red from 0...255<TD>green from 0...255<TD>blue from 0...255<TD>undefined
         * <TR><TH>HSB<TD>hue from 0...359 degrees<TD>saturation from 0...359 degrees (representing 0%...100%)<TD>brightness from 0...359 degrees(representing 0%...100%)<TD>undefined
         * <TR><TH>CMYK<TD>cyan from 0...255 (representing 100%...0%)<TD>magenta from 0...255(representing 100%...0%)<TD>yellow from 0...255(representing 100%...0%)<TD>black from 0...255(representing 100%...0%) 
         * <TR><TH>HSL<TD>hue from 0...359 degrees<TD>saturation from 0...359 degrees(representing 0%...100%)<TD>luminance from 0...359 degrees(representing 0%...100%)<TD>undefined
         * <TR><TH>Lab<TD>Luminance from 0...255(representing 0...100)<TD>a. chromanance from 0...255 degrees(representing -128...127)<TD>b. chromanance from 0...255 degrees(representing -128...127)<TD>undefined
         * <TR><TH>Gray Scale<TD>gray value from 0...255<TD>undefined<TD>undefined<TD>undefined
         * <TR><TH>XYZ<TD>x value from 0...255<TD>Y value from 0...255<TD>Z value from 0...255<TD>undefined
         * </TABLE> 
        */

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public Int16[] colorComponents;

        // Reserved must be NULL or will return paramErr:
        public IntPtr reserved; /**< Must be NULL, otherwise returns parameter error. */

        // Max strucure field size from union
        // https://stackoverflow.com/questions/28074239/how-much-memory-does-a-union-need
        public SelectorParameter selectorParameter;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct SelectorParameter // union
    {
        [FieldOffset(0)]
        public IntPtr pickerPromt;

        [FieldOffset(0)]
        public IntPtr globalSamplePoint;

        [FieldOffset(0)]
        public Int32 specialColorID;
    }

    public struct ColorServicesConsts
    {

        /// <summary>
        /// Choose a color using the user's preferred color picker.
        /// </summary>
        public const Int16 plugIncolorServicesChooseColor = 0;
        /// <summary>
        /// Convert color values fro one color space to another.
        /// </summary>
        public const Int16 plugIncolorServicesConvertColor = 1;
        /// <summary>
        /// Return the current sample point.
        /// </summary>
        public const Int16 plugIncolorServicesSamplePoint = 2;
        /// <summary>
        /// Return either the foreground or background color. 
        /// To select foreground color, set specialColorID in selectorParameter to 0=plugIncolorServicesForegroundColor. 
        /// To select background color, set \c specialColorID in selectorParameter to 1=plugIncolorServicesBackgroundColor.
        /// </summary>
        public const Int16 plugIncolorServicesGetSpecialColor = 3;

        /// <summary>
        /// RGB color space.
        /// </summary>
        public const Int16 plugIncolorServicesRGBSpace = 0;
        /// <summary>
        /// HSB color space.
        /// </summary>
        public const Int16 plugIncolorServicesHSBSpace = 1;
        /// <summary>
        /// CMYK color space.
        /// </summary>
        public const Int16 plugIncolorServicesCMYKSpace = 2;
        /// <summary>
        /// Lab color space.
        /// </summary>
        public const Int16 plugIncolorServicesLabSpace = 3;
        /// <summary>
        /// Gray color space.
        /// </summary>
        public const Int16 plugIncolorServicesGraySpace = 4;
        /// <summary>
        /// HSL color space. 
        /// </summary>
        public const Int16 plugIncolorServicesHSLSpace = 5;
        /// <summary>
        /// XYZ color space. 
        /// </summary>
        public const Int16 plugIncolorServicesXYZSpace = 6;
        /// <summary>
        /// Leaves the color in the space the user chose, returning  resultSpace as the user chosen space.Only valid in the  resultSpace field. 
        /// </summary>
        public const Int16 plugIncolorServicesChosenSpace = -1;

        /// Selects foreground color when ColorServices operation is \c	plugIncolorServicesGetSpecialColor
        public const Int16 plugIncolorServicesForegroundColor = 0;
        /// Selects background color when ColorServices operation is \c	plugIncolorServicesGetSpecialColor
        public const Int16 plugIncolorServicesBackgroundColor = 1;
    }
}

