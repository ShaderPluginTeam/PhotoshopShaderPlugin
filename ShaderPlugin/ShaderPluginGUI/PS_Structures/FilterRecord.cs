using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PS_Structures
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
    public struct FilterRecord
    {

        public int serialNumber;     /**< DEPRECATED - Formerly the host serial number.
									   The host now reports
									   zero for the \c serialNumber. Plug-ins should use 
									   the @ref PropertySuite by using the @ref propertyProcs callback, specifying 
									   the property \c propSerialString2 to get the serial string. */

        public IntPtr abortProc;        /**< A pointer to the \c TestAbortProc callback. */

        public IntPtr progressProc;     /**< A pointer the the \c ProgressProc callback. */

        public IntPtr parameters;      /**< Parameters the plug-in can request from a user.
									     Photoshop initializes this handle to NULL at startup. 
							  			 If the plug-in filter has any parameters that the user can 
							  			 set, it should allocate a relocatable block in the 
							  			 \c filterSelectorParameters handler, store the parameters 
							  			 in the block, and store the block’s handle in this field. */

        public PSPoint imageSize;            /**< DEPRECATED, use \c BigDocumentStruct::imageSize32. The width, imageSize.h, and height, imageSize.v,
										 of the image in pixels. If the selection is floating, this field instead 
										 holds the size of the floating selection.  */

        public short planes;           /**< The number of planes in the image. For version 4+ filters, this 
	                                     field contains the total number 
										 of active planes in the image, including alpha channels. The 
						   				 image mode should be determined by looking at \c imageMode. 
						   				 For version 0-3 filters, this field is equal to 3 if 
						   				 filtering the RGB channel of an RGB color image, or 4 if 
						   				 filtering the CMYK channel of a CMYK color image. 
					   				 Otherwise it is equal to 1. */

        public PSRect filterRect;        /**< DEPRECATED, use \c BigDocumentStruct::filterRect32.
										 The area of the image to be filtered. This is the bounding 
	                                     box of the selection, or if there is no selection, the 
	                                     bounding box of the image. If the selection is not a perfect 
	                                     rectangle, Photoshop automatically masks the changes to the
	                                     area actually selected (unless the plug-in turns off this 
	                                     feature using autoMask). This allows most filters to ignore 
	                                     the selection mask, and still operate correctly. */

        public PSRGBColor background;      /**< DEPRECATED: Use @ref backColor. */

        public PSRGBColor foreground;      /**< DEPRECATED: Use @ref foreColor. */

        public Int32 maxSpace;         /**< The maximum number of bytes of 
	                                     information the plug-in can expect to be able to access at once 
	                                     (input area size + output area size 
										 + mask area size + bufferSpace). Set by Photoshop.*/

        public int bufferSpace;      /**< Allows the plug-in to specify how much buffer space it needs.
	                                     If the plug-in is planning on allocating any large internal 
	                                     buffers or tables, it should set this field during the 
	                                     \c filterSelectorPrepare call to the number of bytes it  
	                                     plans to allocate. Photoshop then tries to free up the 
	                                     requested amount of space before calling the 
	                                     \c filterSelectorStart routine. Relocatable blocks should
									     be used if possible. */

        public PSRect inRect;            /**< DEPRECATED, use \c BigDocumentStruct::inRect32. 
	                                     The area of the input image to access. The plug-in should set 
										 this field in the \c filterSelectorStart and 
	                                     \c filterSelectorContinue handlers to request access to an 
	                                     area of the input image. The area requested must be a subset 
	                                     of the image’s bounding rectangle. After the entire 
	                                     \c filterRect has been filtered, this field should be set to 
	                                     an empty rectangle.  */

        public short inLoPlane;            /**< The first input plane to process next. The \c filterSelectorStart and 
										 \c filterSelectorContinue handlers should set this field. */

        public short inHiPlane;            /**< The last input plane to process next. The \c filterSelectorStart and 
	                                     \c filterSelectorContinue handlers should set this field.  */

        public PSRect outRect;           /**< DEPRECATED, use \c BigDocumentStruct::outRect32.
	                                     The area of the output image to access. The plug-in should set 
	                                     this field in its \c filterSelectorStart and \c filterSelectorContinue 
	                                     handlers to request access to an area of the output image. 
	                                     The area requested must be a subset of \c filterRect. After 
	                                     the entire \c filterRect has been filtered, this field 
	                                     should be set to an empty rectangle.  */

        public short outLoPlane;       /**< The first output plane to process next. The \c filterSelectorStart 
	                                     and \c filterSelectorContinue handlers should set this field.  */

        public short outHiPlane;       /**< The last output plane to process next. The \c filterSelectorStart and 
	                                     \c filterSelectorContinue handlers should set this field. */

        public IntPtr inData;           /**< A pointer to the requested input image data. If more than 
	                                     one plane has been requested (\c inLoPlane¦\c inHiPlane), the 
	                                     data is interleaved. */

        public int inRowBytes;       /**< The offset between rows of the input image data. 
										 The end of each row may or may not include pad bytes. */

        public IntPtr outData;          /**< A pointer to the requested output image 
	                                     data. If more than one plane has been requested 
	                                     (\c outLoPlane¦\c outHiPlane), the data is interleaved. */

        public int outRowBytes;      /**< The offset between rows of the output image data. 
	                                     The end of each row may or may not include pad bytes. */

        public byte isFloating;         /**< Indicates if the selection is floating. Set to TRUE if and only if 
	                                     the selection is floating. */

        public byte haveMask;           /**< Indicates if the selection has a mask. Set to true if and only if  
	                                     non-rectangular area has been selected. */

        public byte autoMask;           /**< Enables or disables auto-masking. By default, Photoshop 
	                                     automatically masks any changes to the area actually selected.
	                                     If \c isFloating=FALSE, and \c haveMask=TRUE, the plug-in can 
	                                     turn off this feature by setting this field to FALSE. It can 
	                                     then perform its own masking. <br><br>
										 If the plug-in has set the PiPL bit \c writesOutsideSelection, this 
										 will always be FALSE and the plug-in must supply its own mask, if 
										 needed. */

        public PSRect maskRect;          /**< DEPRECATED, use \c BigDocumentStruct::maskRect32.
	                                     Provides a mask rectangle. If \c haveMask=TRUE, and the 
	                                     plug-in needs access to the selection mask, the plug-in
	                                     should set this field in your \c filterSelectorStart and 
	                                     \c filterSelectorContinue handlers to request access to an 
	                                     area of the selection mask. The requested area must be a 
	                                     subset of \c filterRect. This field is ignored if there is 
	                                     no selection mask.  */

        public IntPtr maskData;         /**< A pointer to the requested mask data. The data is in the 
	                                     form of an array of bytes, one byte per pixel of the selected 
	                                     area. The bytes range from (0...255), where 0=no mask 
	                                     (selected) and 255=masked (not selected). 
	                                     Use \c maskRowBytes to iterate over the scan lines of the 
	                                     mask. */

        public int maskRowBytes;     /**< The offset between rows of the mask data. */

        public FilterColor backColor;          /**< The current background color, in the 
	                                     color space native to the image.*/

        public FilterColor foreColor;          /**< The current foreground color, in the 
	                                     color space native to the image.*/

        public UInt32 hostSig;         /**< The signature of the host, provided by the host.
	                                     The signature for Photoshop is signature is 8BIM. */

        public IntPtr hostProc;          /**< A pointer to a host-defined callback procedure. May be NULL. */

        public short imageMode;            /**< The mode of the image being filtered, for example, Gray Scale, RGB Color, 
	                                     and so forth. See @ref ImageModes "Image Modes" for values. The \c filterSelectorStart 
	                                     handler should return \c filterBadMode if it is unable to 
	                                     process this mode of image. */

        public Int32 imageHRes;            /**< The  horizontal resolution of the image in terms of 
	                                     pixels per inch. These are fixed point numbers (16.16). */

        public Int32 imageVRes;            /**< The vertical resolution of the image in terms of 
	                                     pixels per inch. These are fixed point numbers (16.16). */

        public PSPoint floatCoord;       /**< DEPRECATED, use \c BigDocumentStruct::floatCoord32.
	                                     The coordinate of the top-left corner of the selection 
	                                     in the main image’s coordinate space. */

        public PSPoint wholeSize;            /**< DEPRECATED, use \c BigDocumentStruct::wholeSize32.
	                                     The size in pixels of the entire main image. 
										 */

        public PlugInMonitor monitor;      /**< Monitor setup information for the host. */

        public IntPtr platformData;     /**< A pointer to platform specific data. 
	                                     Not used under Mac OS. See PlatformData in PITypes.h. */

        public IntPtr bufferProcs;       /**< A pointer to the Buffer suite 
	                                     if it is supported by the host, otherwise NULL. 
	                                     See @ref BufferSuite. */

        public IntPtr resourceProcs;   /**< A pointer to the Pseudo-Resource suite 
	                                     if it is supported by the host, otherwise NULL. 
	                                     See chapter @ref ResourceSuite. */

        public IntPtr processEvent;  /**< A pointer to the \c ProcessEventProc callback 
	                                     if it is supported by the host, otherwise NULL. */

        public IntPtr displayPixels;/**< A pointer to the \c DisplayPixelsProc callback 
	                                     if it is supported by the host, otherwise NULL.  */

        public IntPtr handleProcs;       /**< A pointer to the Handle callback suite 
	                                     if it is supported by the host, otherwise NULL.  
	                                     See @ref HandleSuite. */

        ///@name New in 3.0. 
        //@{

        public byte supportsDummyChannels;  /**< Indicates whether the host supports the plug-in 
	                                         requesting nonexistent planes. (See 
	                                         @ref dummyPlaneValue, @ref inPreDummyPlanes, 
	                                         @ref inPostDummyPlanes, @ref outPreDummyPlanes, 
	                                         @ref outPostDummyPlanes.) */

        public byte supportsAlternateLayouts;   /**< Indicates whether the host support data layouts 
	                                             other than rows of columns of planes. This field 
	                                             is set by the plug-in host to indicate whether 
	                                             it respects the \c wantLayout field. */

        public short wantLayout;           /**< The desired layout for the data. 
	                                     The plug-in host only looks at this field if it has also set 
	                                     \c supportsAlternateLayouts. See @ref LayoutConstants "Layout Constants"
										 for values. */

        public short filterCase;           /**< The type of data being filtered. Flat, floating, layer 
	                                     with editable transparency, layer with preserved transparency, 
	                                     with and without a selection. A zero indicates that the host 
	                                     did not set this field, and the plug-in should look at 
	                                     \c haveMask and \c isFloating. See @ref FilterCaseIdentifiers for values. */

        public short dummyPlaneValue;  /**< The value to store into any dummy planes. Values from 0 to 
	                                     255 indicates a specific value. A value of -1 indicates 
	                                     to leave undefined. All other values generate an error. */

        public IntPtr premiereHook;     /**< DEPRECATED. */

        public IntPtr advanceState;  /**< \c AdvanceState callback. */

        public byte supportsAbsolute;   /**< Indicates whether the host supports absolute channel 
	                                     indexing. Absolute channel indexing ignores visibility 
	                                     concerns and numbers the channels from zero starting with 
	                                     the first composite channel. If existing, transparency 
	                                     follows, followed by any layer masks, then alpha channels. */

        public byte wantsAbsolute;      /**< Enables absolute channel indexing for the input. This is 
									     only useful if supportsAbsolute=TRUE. Absolute indexing 
									     is useful for things like accessing alpha channels.  */

        public IntPtr getPropertyObsolete;    /**< The \c GetProperty callback. This direct callback 
											     pointer has been superceded by the Property callback 
											     suite, but is maintained here for backwards compatibility. 
											     See @ref PropertySuite. */

        public byte cannotUndo;         /**< Indicates whether a filter plug-in makes changes that the user cannot undo.
	                                     If the filter makes a change that cannot be undone, then setting 
										 this field to TRUE prevents Photoshop from offering undo 
										 for the filter. This is rarely needed and usually frustrates 
										 users.	 */

        public byte supportsPadding;    /**< Indicates whether the host supports requests outside the image 
	                                     area. If so, see padding fields below. */

        public short inputPadding;     /**< Instructions for padding the input. 
										 The input can be padded when loaded. See @ref FilterPadding 
										 "Filter Padding Constants." \n\n
										 The options for padding include specifying a specific value 
										 (0...255), specifying \c plugInWantsEdgeReplication, specifying 
										 that the data be left random (\c plugInDoesNotWantPadding), or 
										 requesting that an error be signaled for an out of bounds 
										 request (\c plugInWantsErrorOnBoundsException). Default value: 
										 \c plugInWantsErrorOnBoundsException. */

        public short outputPadding;        /**< Instructions for padding the output. 
	                                     The output can be padded when loaded. See @ref FilterPadding 
	                                     "Filter Padding Constants." \n\n
										 The options for padding include specifying a specific value 
										 (0...255), specifying \c plugInWantsEdgeReplication, specifying 
										 that the data be left random (\c plugInDoesNotWantPadding), or 
										 requesting that an error be signaled for an out of bounds 
										 request (\c plugInWantsErrorOnBoundsException). Default value: 
										 \c plugInWantsErrorOnBoundsException. */

        public short maskPadding;      /**< Padding instructions for the mask. 
	                                     The mask can be padded when loaded. See @ref FilterPadding 
	                                     "Filter Padding Constants." \n\n
										 The options for padding include specifying a specific value 
										 (0...255), specifying \c plugInWantsEdgeReplication, specifying 
										 that the data be left random (\c plugInDoesNotWantPadding), or 
										 requesting that an error be signaled for an out of bounds 
										 request (\c plugInWantsErrorOnBoundsException). Default value: 
										 \c plugInWantsErrorOnBoundsException. */

        public char samplingSupport;   /**< Indicates whether the host support non- 1:1 sampling 
	                                     of the input and mask. Photoshop 3.0.1+ supports 
	                                     integral sampling steps (it will round up to get there). 
	                                     This is indicated by the value #hostSupportsIntegralSampling. 
	                                     Future versions may support non-integral sampling steps. 
	                                     This will be indicated with #hostSupportsFractionalSampling. */

        public char reservedByte;      /**< For alignment. */

        public Int32 inputRate;            /**< The sampling rate for the input. The effective input 
	                                     rectangle in normal sampling coordinates is 
	                                     <code> inRect * inputRate. </code> For example, 
	                                     <code> (inRect.top * inputRate, 
	                                     inRect.left * inputRate, inRect.bottom * inputRate, 
	                                     inRect.right * inputRate). </code> The value for
	                                     \c inputRate is rounded to the 
	                                     nearest integer in Photoshop 3.0.1+. Since the scaled 
	                                     rectangle may exceed the real source data, it is a good 
	                                     idea to set some sort of padding for the input as well. */

        public Int32 maskRate;         /**< Like \c inputRate, but as applied to the mask data. */

        [MarshalAs(UnmanagedType.FunctionPtr)]
        public ColorServicesProc colorServices; /**< Function pointer to access color services routine. */

        /* Photoshop structures its data as follows for plug-ins when processing
           layer data:
                target layer channels
                transparency mask for target layer
                layer mask channels for target layer
                inverted layer mask channels for target layer
                non-layer channels
            When processing non-layer data (including running a filter on the
            layer mask alone), Photoshop structures the data as consisting only
            of non-layer channels.  It indicates this structure through a series
            of short counts.  The transparency count must be either 0 or 1. */

        public short inLayerPlanes;          /**< The number of target layer planes for the input data.  
	                                       If all the input plane values are zero, 
	                                       then the plug-in should assume the host has not set them.
	                                       <br><br>
	                                       @note When processing layer data, Photoshop structures 
	                                       its input and output data for plug-ins as follows:
	                                       - target layer channels
	                                       - transparency mask for target layer
	                                       - layer mask channels for target layer
	                                       - inverted layer mask channels for target layer
	                                       - non-layer channels	

	                                       The output planes are a prefix of the input planes. 
	                                       For example, in the protected transparency case, the input 
	                                       can contain a transparency mask and a layer mask while the 
	                                       output can contain just the layerPlanes.
										   <br><br>
										   When processing non-layer data (including running a filter 
										   on the layer mask alone), Photoshop structures the data 
										   as consisting only of non-layer channels.  It indicates 
										   this structure through a series of short counts.  The 
										   transparency count must be either 0 or 1.  
										   */

        public short inTransparencyMask;     /**< The number of transparency masks for the input target 
	                                       layer data.  
	                                       See @ref inLayerPlanes for additional information. */

        public short inLayerMasks;       /**< The number of layer mask channels for the input 
	                                       target layer.  
	                                       See @ref inLayerPlanes for additional information.*/

        public short inInvertedLayerMasks; /**< The number of inverted layer mask channels for the 
	                                       input target layer. <br> 
	                                       With the inverted layer masks, 0 = fully visible 
	                                       and 255 = completely hidden.  See @ref inLayerPlanes for 
	                                       additional information.*/

        public short inNonLayerPlanes;   /**< The number of non-layer channels for the input data.  
	                                       See @ref inLayerPlanes for additional information. */

        public short outLayerPlanes;         /**< The number of target layer planes for the output data.
										   See @ref inLayerPlanes for additional information about 
										   the structure of the output data. */

        public short outTransparencyMask;  /**< The number of transparency masks for the output data.
										   See @ref inLayerPlanes for additional information about 
										   the structure of the output data. */

        public short outLayerMasks;          /**< The number of layer mask channels for the output data.
										   See @ref inLayerPlanes for additional information about 
										   the structure of the output data. */

        public short outInvertedLayerMasks; /**< The number of inverted layer mask channels for the output data.
										   See @ref inLayerPlanes for additional information about 
										   the structure of the output data. */

        public short outNonLayerPlanes;       /**< The number of non-layer channels for the output data.
										   See @ref inLayerPlanes for additional information about 
										   the structure of the output data. */

        public short absLayerPlanes;         /**< The number of target layer planes for the input data,
										   used for the structure of the input data when
										   \c wantsAbsolute is TRUE.
										   See @ref inLayerPlanes for additional information about 
										   the structure of the input data. */

        public short absTransparencyMask;  /**< The number of transparency masks for the input data,
										   used for the structure of the input data when
										   \c wantsAbsolute is TRUE.
										   See @ref inLayerPlanes for additional information about 
										   the structure of the input data. */

        public short absLayerMasks;         /**< The number of layer mask channels for the input data,
										   used for the structure of the input data when
										   \c wantsAbsolute is TRUE.
										   See @ref inLayerPlanes for additional information about 
										   the structure of the input data. */

        public short absInvertedLayerMasks; /**< The number of inverted layer mask channels for the input 
										   data, used for the structure of the input data when
										   \c wantsAbsolute is TRUE.
										   See @ref inLayerPlanes for additional information about 
										   the structure of the input data. */

        public short absNonLayerPlanes;     /**< The number of target layer planes for the input data,
										   used for the structure of the input data when
										   \c wantsAbsolute is TRUE.
										   See @ref inLayerPlanes for additional information about 
										   the structure of the input data. */

        /* We allow for extra planes in the input and the output.  These planes
           will be filled with dummyPlaneValue at those times when we build the
           buffers.  These features will only be available if supportsDummyPlanes
           is TRUE. */

        public short inPreDummyPlanes; /**< The number of extra planes before  
									     the input data. Only available if 
									     \c supportsDummyChannels=TRUE. Used for 
									     things like forcing RGB data to appear as RGBA.  */

        public short inPostDummyPlanes;    /**< The number of extra planes after 
									     the input data. Only available if 
									     \c supportsDummyChannels=TRUE. Used for 
									     things like forcing RGB data to appear as RGBA.  */

        public short outPreDummyPlanes;    /**< The number of extra planes before  
									     the output data. Only available if 
									     \c supportsDummyChannels=TRUE. */

        public short outPostDummyPlanes;   /**< The number of extra planes after  
									     the output data. Only available if 
									     \c supportsDummyChannels=TRUE. */

        /* If the plug-in makes use of the layout options, then the following
           fields should be obeyed for identifying the steps between components.
           The last component in the list will always have a step of one. */

        public int inColumnBytes;        /**< The step from column to column in the input. 
										 If using the layout options, this value may change 
										 from being equal to the number of planes. If zero, 
										 assume the host has not set it. */

        public int inPlaneBytes;     /**< The step from plane to plane in the input. Normally 1, 
										 but this changes if the plug-in uses the layout options. 
										 If zero, assume the host has not set it. */

        public int outColumnBytes;       /**< The output equivalent of @ref inColumnBytes. */

        public int outPlaneBytes;        /**< The output equivalent of @ref inPlaneBytes. */
        //@}
        ///@name New in 3.0.4 
        //@{

        public IntPtr imageServicesProcs;
        /**< Image Services callback suite. */

        public IntPtr propertyProcs;   /**< Property callback suite.  The plug-in needs to
									     dispose of the handle returned for	complex properties (the 
									     plug-in also maintains ownership of handles for
										 set properties. */

        public short inTileHeight;     /**< Tiling height for the input, set by host. Zero if not set. 
	                                     The plug-in should work at this size, if possible. */

        public short inTileWidth;      /**< Tiling width for the input, set by host. Zero if not set. 
	                                     Best to work at this size, if possible. */

        public PSPoint inTileOrigin;     /**< Tiling origin for the input, set by host. Zero if not set. */

        public short absTileHeight;        /**< Tiling height the absolute data, set by host. 
	                                     The plug-in should work at this size, if possible.*/

        public short absTileWidth;     /**< Tiling width the absolute data, set by host. 
	                                     The plug-in should work at this size, if possible.*/

        public PSPoint absTileOrigin;        /**< Tiling origin the absolute data, set by host. */

        public short outTileHeight;        /**< Tiling height for the output, set by host. 
	                                     The plug-in should work at this size, if possible. */

        public short outTileWidth;     /**< Tiling width for the output, set by host. 
	                                     The plug-in should work at this size, if possible. */

        public PSPoint outTileOrigin;        /**< Tiling origin for the output, set by host.  */

        public short maskTileHeight;       /**< Tiling height for the mask, set by host. 
	                                     The plug-in should work at this size, if possible. */

        public short maskTileWidth;        /**< Tiling width for the mask, set by host. 
	                                     The plug-in should work at this size, if possible. */

        public PSPoint maskTileOrigin;       /**< Tiling origin for the mask, set by host. */
        //@}
        ///@name New in 4.0 
        //@{

        public IntPtr descriptorParameters;   /**< Descriptor callback suite.  */

        public IntPtr errorString;    /**< An error reporting string to return to Photoshop.
	                                             If the plug-in returns with result=errReportString then 
	                                             this string is displayed as: 
	                                             "Cannot complete operation because " + \c errorString. */

        public IntPtr channelPortProcs;
        /**< Channel Ports callback suite. */

        public IntPtr documentInfo;    /**< The Channel Ports document information
												 for the document being filtered. */
        //@}
        ///@name New in 5.0 
        //@{

        public IntPtr sSPBasic;     /**< PICA basic suite.  Provides the mechanism to access all PICA suites. */

        public IntPtr plugInRef;        /**< Plug-in reference used by PICA. */

        public int depth;            /**< Bit depth per channel (1,8,16,32). */
        //@}
        ///@name New in 6.0 
        //@{

        public IntPtr iCCprofileData;  /**< Handle containing the ICC profile for the image. (NULL if none)
										Photoshop allocates the handle using its handle suite
										The handle is unlocked while calling the plug-in.
										The handle is valid from  \c FilterSelectorStart to \c FilterSelectorFinish
										Photoshop will free the handle after \c FilterSelectorFinish. */

        public int iCCprofileSize;       /**< Size of profile. */

        public int canUseICCProfiles;    /**< Indicates if the host uses ICC Profiles. Non-zero if the host can 
	                                         accept or export ICC profiles .
											 If this is zero, don't set or dereference \c iCCprofileData.
										*/

        //@}
        ///@name New in 7.0 
        //@{

        public int hasImageScrap;        /**< Indicates if Photoshop has image scrap; non-zero if it does. 
	                                         The plug-in can ask for the 
	                                         exporting of image scrap by setting the PiPL resource, 
	                                         @ref PIWantsScrapProperty. The document info for the image scrap is 
	                                         chained right behind the targeted document pointed by the 
	                                         @ref documentInfo field. Photoshop sets \c hasImageScrap to indicate 
	                                         that an image scrap is available. A plug-in can use it to tell whether 
	                                         Photoshop failed to export the scrap because some unknown 
	                                         reasons or there is no scrap at all. */
        //@}
        ///@name New in 8.0 
        //@{

        public IntPtr bigDocumentData; /**< Support for documents larger than 30,000 pixels. 
												NULL if host does not support big documents.*/


        //@}
        ///@name New in 10.0 
        //@{

        public IntPtr input3DScene;   /**< support for 3d scene data to be sent into the plugin */

        public IntPtr output3DScene;  /**< support for 3d scene to come out of the plugin */

        public Boolean createNewLayer; /**< set by plugin this only works for 3D layers */

        //@}
        ///@name New in 13.0 
        //@{

        public IntPtr iCCWorkingProfileData;   /**< Handle containing the ICC profile for the working
											profile set via color settings dialog. (NULL if none)
											Photoshop allocates the handle using its handle suite
											The handle is unlocked while calling the plug-in.
											The handle is valid from  \c FilterSelectorStart to \c FilterSelectorFinish
											Photoshop will free the handle after \c FilterSelectorFinish. */

        public int iCCWorkingProfileSize;    /**< Size of working profile. */

        //@}
        ///@name Reserved Space for Expansion
        //@{

        public char reserved;  /**< Reserved for future use. Set to zero. */
        //@}

    }
}
