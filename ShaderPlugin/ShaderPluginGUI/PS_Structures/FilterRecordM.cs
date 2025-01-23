using PS_Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ShaderPluginGUI.PS_Structures
{
    public class FilterRecordM
    {
#pragma warning disable IDE1006
        public IntPtr pointer;
        public FilterRecord ptrData;

        /// <summary>
        /// DEPRECATED - Formerly the host serial number.
        /// The host now reports zero for the \c serialNumber.
        /// <para></para>
        /// Plug-ins should use the @ref PropertySuite by using the @ref propertyProcs callback, specifying
        /// <para></para>
        /// the property \c propSerialString2 to get the serial string.
        /// </summary>
        public int serialNumber { get { return ptrData.serialNumber; } set { ptrData.serialNumber = value; } }

        /// <summary>
        /// A pointer to the \c TestAbortProc callback
        /// </summary>
        public TestAbortProc abortProc { get; private set; }

        /// <summary>
        /// A pointer the the \c ProgressProc callback.
        /// </summary>
        public ProgressProc progressProc { get; private set; }

        /// <summary>
        /// Parameters the plug-in can request from a user.
        /// Photoshop initializes this handle to NULL at startup.
        /// If the plug-in filter has any parameters that the user can
        /// set, it should allocate a relocatable block in the 
		///  \c filterSelectorParameters handler, store the parameters 
		///  in the block, and store the block’s handle in this field.
        /// </summary>
        public IntPtr parameters { get { return ptrData.parameters; } set { ptrData.parameters = value; } }

        /// <summary>
        /// DEPRECATED, use \c BigDocumentStruct::imageSize32. The width, imageSize.h, and height, imageSize.v, 
        /// of the image in pixels.If the selection is floating, this field instead
        /// holds the size of the floating selection.
        /// </summary>
        public PSPoint imageSize { get { return ptrData.imageSize; } set { ptrData.imageSize = value; } }

        /// <summary>
        /// The number of planes in the image. For version 4+ filters, this 
        /// field contains the total number
        /// of active planes in the image, including alpha channels.The
        ///
        /// image mode should be determined by looking at \c imageMode. 
        ///
        /// For version 0-3 filters, this field is equal to 3 if 
        ///
        /// filtering the RGB channel of an RGB color image, or 4 if 
        ///
        /// filtering the CMYK channel of a CMYK color image.
        /// Otherwise it is equal to 1.
        /// </summary>
        public short planes { get { return ptrData.planes; } set { ptrData.planes = value; } }

        /// <summary>
        /// DEPRECATED, use \c BigDocumentStruct::filterRect32.
        /// The area of the image to be filtered.This is the bounding
        ///
        /// box of the selection, or if there is no selection, the
        /// bounding box of the image. If the selection is not a perfect
        /// rectangle, Photoshop automatically masks the changes to the
        /// area actually selected (unless the plug-in turns off this
        /// feature using autoMask). This allows most filters to ignore
        ///
        /// the selection mask, and still operate correctly.
        /// </summary>
        public PSRect filterRect { get { return ptrData.filterRect; } set { ptrData.filterRect = value; } }

        /// <summary>
        /// DEPRECATED: Use @ref backColor. 
        /// </summary>
        public PSRGBColor background { get { return ptrData.background; } set { ptrData.background = value; } }

        /// <summary>
        ///  DEPRECATED: Use @ref foreColor.
        /// </summary>
        public PSRGBColor foreground { get { return ptrData.foreground; } set { ptrData.foreground = value; } }

        /// <summary>
        /// The maximum number of bytes of 
        /// information the plug-in can expect to be able to access at once
        /// (input area size + output area size + mask area size + bufferSpace). Set by Photoshop.
        /// </summary>
        public Int32 maxSpace { get { return ptrData.maxSpace; } set { ptrData.maxSpace = value; } }

        /// <summary>
        /// Allows the plug-in to specify how much buffer space it needs.
        /// If the plug-in is planning on allocating any large internal
        /// buffers or tables, it should set this field during the
	    /// \c filterSelectorPrepare call to the number of bytes it
        /// plans to allocate. Photoshop then tries to free up the
        /// requested amount of space before calling the 
	    /// \c filterSelectorStart routine.Relocatable blocks should
        /// be used if possible.
        /// </summary>
        public int bufferSpace { get { return ptrData.bufferSpace; } set { ptrData.bufferSpace = value; } }

        /// <summary>
        /// DEPRECATED, use \c BigDocumentStruct::inRect32. 
        /// The area of the input image to access.The plug-in should set
        /// this field in the \c filterSelectorStart and
	    /// \c filterSelectorContinue handlers to request access to an
        /// area of the input image.The area requested must be a subset
        /// of the image’s bounding rectangle.After the entire
	    /// \c filterRect has been filtered, this field should be set to
        /// an empty rectangle. 
        /// </summary>
        public PSRect inRect { get { return ptrData.inRect; } set { ptrData.inRect = value; } }

        /// <summary>
        /// The first input plane to process next. The \c filterSelectorStart and \c filterSelectorContinue handlers should set this field.
        /// </summary>
        public short inLoPlane { get { return ptrData.inLoPlane; } set { ptrData.inLoPlane = value; } }

        /// <summary>
        /// The last input plane to process next. The \c filterSelectorStart and \c filterSelectorContinue handlers should set this field.
        /// </summary>
        public short inHiPlane { get { return ptrData.inHiPlane; } set { ptrData.inHiPlane = value; } }

        /// <summary>
        /// DEPRECATED, use \c BigDocumentStruct::outRect32.
        /// The area of the output image to access.The plug-in should set
        ///
        /// this field in its \c filterSelectorStart and \c filterSelectorContinue
        ///
        /// handlers to request access to an area of the output image.
        /// The area requested must be a subset of \c filterRect. After
        /// the entire \c filterRect has been filtered, this field
        /// should be set to an empty rectangle. 
        /// </summary>
        public PSRect outRect { get { return ptrData.outRect; } set { ptrData.outRect = value; } }
        /// <summary>
        /// The first output plane to process next. The \c filterSelectorStart and \c filterSelectorContinue handlers should set this field.
        /// </summary>
        public short outLoPlane { get { return ptrData.outLoPlane; } set { ptrData.outLoPlane = value; } }

        /// <summary>
        /// The last output plane to process next. The \c filterSelectorStart and \c filterSelectorContinue handlers should set this field.
        /// </summary>
        public short outHiPlane { get { return ptrData.outHiPlane; } set { ptrData.outHiPlane = value; } }

        /// <summary>
        ///  A pointer to the requested input image data. If more than one plane has been requested(\c inLoPlane¦\c inHiPlane), the data is interleaved.
        /// </summary>
        public IntPtr inData { get { return ptrData.inData; } set { ptrData.inData = value; } }
        /// <summary>
        /// The offset between rows of the input image data. The end of each row may or may not include pad bytes.
        /// </summary>
        public int inRowBytes { get { return ptrData.inRowBytes; } set { ptrData.inRowBytes = value; } }

        /// <summary>
        /// A pointer to the requested output image data.If more than one plane has been requested (\c outLoPlane¦\c outHiPlane), the data is interleaved.
        /// </summary>
        public IntPtr outData { get { return ptrData.outData; } set { ptrData.outData = value; } }

        /// <summary>
        /// The offset between rows of the output image data. The end of each row may or may not include pad bytes.
        /// </summary>
        public int outRowBytes { get { return ptrData.outRowBytes; } set { ptrData.outRowBytes = value; } }

        ///<summary>
        /// Indicates if the selection is floating. Set to TRUE if and only if the selection is floating.
        ///</summary>
        public bool isFloating { get { return ptrData.isFloating == 1; } set { ptrData.isFloating = Bool2Byte(value); } }

        ///<summary>
        ///Indicates if the selection has a mask. Set to true if and only if non-rectangular area has been selected.
        /// </summary>
        public bool haveMask { get { return ptrData.haveMask == 1; } set { ptrData.haveMask = Bool2Byte(value); } }

        /// <summary>
        /// Enables or disables auto-masking. By default, Photoshop 
        /// automatically masks any changes to the area actually selected.
        /// If \c isFloating = FALSE, and \c haveMask = TRUE, the plug-in can
        /// turn off this feature by setting this field to FALSE.It can
        /// then perform its own masking. <br><br>
        /// If the plug-in has set the PiPL bit \c writesOutsideSelection, this
        /// will always be FALSE and the plug-in must supply its own mask, if 
        /// needed.
        /// </summary>
        public bool autoMask { get { return ptrData.autoMask == 1; } set { ptrData.autoMask = Bool2Byte(value); } }
        ///<summary>
        ///DEPRECATED, use \c BigDocumentStruct::maskRect32.
        /// Provides a mask rectangle.If \c haveMask = TRUE, and the
        /// plug-in needs access to the selection mask, the plug-in
        /// should set this field in your \c filterSelectorStart and
        /// \c filterSelectorContinue handlers to request access to an
        /// area of the selection mask.The requested area must be a
        /// subset of \c filterRect. This field is ignored if there is 
        /// no selection mask.
        ///</summary>
        public PSRect maskRect { get { return ptrData.maskRect; } set { ptrData.maskRect = value; } }

        /// <summary>
        /// A pointer to the requested mask data. The data is in the 
        ///  form of an array of bytes, one byte per pixel of the selected
        ///
        ///   area.The bytes range from(0...255), where 0=no mask
        /// (selected) and 255=masked(not selected). 
        ///   Use \c maskRowBytes to iterate over the scan lines of the
        ///
        ///   mask.
        /// </summary>
        public IntPtr maskData { get { return ptrData.maskData; } set { ptrData.maskData = value; } }

        /// <summary>
        /// The offset between rows of the mask data.
        /// </summary>
        public int maskRowBytes { get { return ptrData.maskRowBytes; } set { ptrData.maskRowBytes = value; } }
        /// <summary>
        /// The current background color, in the color space native to the image.
        /// </summary>
        public FilterColor backColor { get { return ptrData.backColor; } set { ptrData.backColor = value; } }

        /// <summary>
        /// The current foreground color, in the color space native to the image.
        /// </summary>
        public FilterColor foreColor { get { return ptrData.foreColor; } set { ptrData.foreColor = value; } }

        /// <summary>
        /// The signature of the host, provided by the host. The signature for Photoshop is signature is 8BIM.
        /// </summary>
        public UInt32 hostSig { get { return ptrData.hostSig; } set { ptrData.hostSig = value; } }

        /// <summary>
        /// A pointer to a host-defined callback procedure. May be NULL.
        /// </summary>
        public HostProc hostProc { get; private set; }

        /// <summary>
        /// The mode of the image being filtered, for example, Gray Scale, RGB Color, 
        /// and so forth.See @ref ImageModes "Image Modes" for values.The \c filterSelectorStart
        /// handler should return \c filterBadMode if it is unable to
        /// process this mode of image.
        /// </summary>
        public short imageMode { get { return ptrData.imageMode; } set { ptrData.imageMode = value; } }

        /// <summary>
        /// The  horizontal resolution of the image in terms of pixels per inch.These are fixed point numbers (16.16).
        /// </summary>
        public Int32 imageHRes { get { return ptrData.imageHRes; } set { ptrData.imageHRes = value; } }

        /// <summary>
        /// The vertical resolution of the image in terms of pixels per inch.These are fixed point numbers (16.16). 
        /// </summary>
        public Int32 imageVRes { get { return ptrData.imageVRes; } set { ptrData.imageVRes = value; } }

        /// <summary>
        /// DEPRECATED, use \c BigDocumentStruct::floatCoord32. The coordinate of the top-left corner of the selection in the main image’s coordinate space.
        /// </summary>
        public PSPoint floatCoord { get { return ptrData.floatCoord; } set { ptrData.floatCoord = value; } }

        /// <summary>
        ///  DEPRECATED, use \c BigDocumentStruct::wholeSize32. The size in pixels of the entire main image.
        /// </summary>
        public PSPoint wholeSize { get { return ptrData.wholeSize; } set { ptrData.wholeSize = value; } }

        /// <summary>
        /// Monitor setup information for the host.
        /// </summary>
        public PlugInMonitor monitor { get { return ptrData.monitor; } set { ptrData.monitor = value; } }
        /// <summary>
        /// A pointer to platform specific data. Not used under Mac OS.See PlatformData in PITypes.h.
        /// </summary>
        public IntPtr platformData { get { return ptrData.platformData; } set { ptrData.platformData = value; } }
        /// <summary>
        /// The Buffer suite if it is supported by the host, otherwise NULL. See @ref BufferSuite.
        /// </summary>
        public BufferProcs bufferProcs { get; private set; }
        /// <summary>
        /// A pointer to the Pseudo-Resource suite if it is supported by the host, otherwise NULL. See chapter @ref ResourceSuite.
        /// </summary>
        public IntPtr resourceProcs { get { return ptrData.resourceProcs; } set { ptrData.resourceProcs = value; } }
        /// <summary>
        /// A pointer to the \c ProcessEventProc callback if it is supported by the host, otherwise NULL.
        /// </summary>
        public ProcessEventProc processEvent { get; protected set; }

        /// <summary>
        /// A pointer to the \c DisplayPixelsProc callback if it is supported by the host, otherwise NULL.
        /// </summary>
        public DisplayPixelsProc displayPixels { get; private set; }

        /// <summary>
        /// A pointer to the Handle callback suite 
	    /// if it is supported by the host, otherwise NULL.
        /// See @ref HandleSuite.
        /// </summary>
        public IntPtr handleProcs { get { return ptrData.handleProcs; } set { ptrData.handleProcs = value; } }

        /// <summary>
        /// Indicates whether the host supports the plug-in 
        /// requesting nonexistent planes. (See
        /// @ref dummyPlaneValue, @ref inPreDummyPlanes, 
	    /// @ref inPostDummyPlanes, @ref outPreDummyPlanes, 
	    /// @ref outPostDummyPlanes.)
        /// </summary>
        public bool supportsDummyChannels { get { return ptrData.supportsDummyChannels == 1; } set { ptrData.supportsDummyChannels = Bool2Byte(value); } }
        /// <summary>
        /// Indicates whether the host support data layouts 
        /// other than rows of columns of planes.This field 
	    /// is set by the plug-in host to indicate whether
        /// it respects the \c wantLayout field.
        /// </summary>
        public bool supportsAlternateLayouts { get { return ptrData.supportsAlternateLayouts == 1; } set { ptrData.supportsAlternateLayouts = Bool2Byte(value); } }

        /// <summary>
        /// The desired layout for the data. 
        /// The plug-in host only looks at this field if it has also set 
	    /// \c supportsAlternateLayouts.See @ref LayoutConstants "Layout Constants"
		/// for values.
        /// </summary>
        public short wantLayout { get { return ptrData.wantLayout; } set { ptrData.wantLayout = value; } }
        /// <summary>
        /// The type of data being filtered. Flat, floating, layer 
        /// with editable transparency, layer with preserved transparency,
        /// with and without a selection.A zero indicates that the host
        /// did not set this field, and the plug-in should look at
	    /// \c haveMask and \c isFloating. See @ref FilterCaseIdentifiers for values.
        /// </summary>
        public short filterCase { get { return ptrData.filterCase; } set { ptrData.filterCase = value; } }
        /// <summary>
        ///  The value to store into any dummy planes. Values from 0 to 
        ///  255 indicates a specific value.A value of -1 indicates
        ///  to leave undefined. All other values generate an error.
        /// </summary>
        public short dummyPlaneValue { get { return ptrData.dummyPlaneValue; } set { ptrData.dummyPlaneValue = value; } }

        public IntPtr premiereHook { get { return ptrData.premiereHook; } set { ptrData.premiereHook = value; } }     /**< DEPRECATED. */

        /// AdvanceState callback.
        public AdvanceStateProc advanceState { get; private set; }

        /// <summary>
        /// Indicates whether the host supports absolute channel 
        /// indexing.Absolute channel indexing ignores visibility
        /// concerns and numbers the channels from zero starting with
        /// 
        /// the first composite channel. If existing, transparency
        /// follows, followed by any layer masks, then alpha channels.
        /// </summary>
        public byte supportsAbsolute { get { return ptrData.supportsAbsolute; } set { ptrData.supportsAbsolute = value; } }
        /// <summary>
        /// Enables absolute channel indexing for the input. This is 
        /// only useful if supportsAbsolute=TRUE.Absolute indexing 
		/// is useful for things like accessing alpha channels.
        /// </summary>
        public byte wantsAbsolute { get { return ptrData.wantsAbsolute; } set { ptrData.wantsAbsolute = value; } }

        /// <summary>
        /// The \c GetProperty callback. This direct callback 
        /// pointer has been superceded by the Property callback
        /// suite, but is maintained here for backwards compatibility.
        /// See @ref PropertySuite.
        /// </summary>
        public IntPtr getPropertyObsolete { get { return ptrData.getPropertyObsolete; } set { ptrData.getPropertyObsolete = value; } }

        /// <summary>
        /// Indicates whether a filter plug-in makes changes that the user cannot undo.
        /// If the filter makes a change that cannot be undone, then setting
        /// this field to TRUE prevents Photoshop from offering undo 
		/// for the filter. This is rarely needed and usually frustrates
        /// users.
        /// </summary>
        public bool cannotUndo { get { return ptrData.cannotUndo == 1; } set { ptrData.cannotUndo = Bool2Byte(value); } }
        /// <summary>
        /// Indicates whether the host supports requests outside the image 
        /// area.If so, see padding fields below.
        /// </summary>
        public bool supportsPadding { get { return ptrData.supportsPadding == 1; } set { ptrData.supportsPadding = Bool2Byte(value); } }

        /// <summary>
        /// Instructions for padding the input. 
        /// The input can be padded when loaded.See @ref FilterPadding 
		/// "Filter Padding Constants." \n\n
        /// The options for padding include specifying a specific value
        /// (0...255), specifying \c plugInWantsEdgeReplication, specifying
        /// that the data be left random(\c plugInDoesNotWantPadding), or
        /// requesting that an error be signaled for an out of bounds
        /// request(\c plugInWantsErrorOnBoundsException). Default value: 
		/// \c plugInWantsErrorOnBoundsException.
        /// </summary>
        public short inputPadding { get { return ptrData.inputPadding; } set { ptrData.inputPadding = value; } }
        /// <summary>
        /// Instructions for padding the output. 
        /// The output can be padded when loaded.See @ref FilterPadding 
	    /// "Filter Padding Constants." \n\n
        /// The options for padding include specifying a specific value
        /// (0...255), specifying \c plugInWantsEdgeReplication, specifying
        /// that the data be left random(\c plugInDoesNotWantPadding), or
        /// requesting that an error be signaled for an out of bounds
        /// request(\c plugInWantsErrorOnBoundsException). Default value: 
		/// \c plugInWantsErrorOnBoundsException.
        /// </summary>
        public short outputPadding { get { return ptrData.outputPadding; } set { ptrData.outputPadding = value; } }
        /// <summary>
        /// Padding instructions for the mask. 
        /// The mask can be padded when loaded.See @ref FilterPadding 
	    /// "Filter Padding Constants." \n\n
        /// The options for padding include specifying a specific value
        /// (0...255), specifying \c plugInWantsEdgeReplication, specifying
        /// that the data be left random(\c plugInDoesNotWantPadding), or
        /// requesting that an error be signaled for an out of bounds
        /// request(\c plugInWantsErrorOnBoundsException). Default value: 
		/// \c plugInWantsErrorOnBoundsException.
        /// </summary>
        public short maskPadding { get { return ptrData.maskPadding; } set { ptrData.maskPadding = value; } }
        /// <summary>
        /// Indicates whether the host support non- 1:1 sampling 
        /// of the input and mask.Photoshop 3.0.1+ supports
        /// integral sampling steps(it will round up to get there). 
	    /// This is indicated by the value #hostSupportsIntegralSampling. 
	    /// Future versions may support non-integral sampling steps.
        /// This will be indicated with #hostSupportsFractionalSampling.
        /// </summary>
        public char samplingSupport { get { return ptrData.samplingSupport; } set { ptrData.samplingSupport = value; } }
        /// <summary>
        /// For alignment. 
        /// </summary>
        public char reservedByte { get { return ptrData.reservedByte; } set { ptrData.reservedByte = value; } }
        /// <summary>
        /// The sampling rate for the input. The effective input 
        /// rectangle in normal sampling coordinates is 
	    /// <code> inRect* inputRate. </code> For example,
	    /// <code> (inRect.top* inputRate,
        /// inRect.left* inputRate, inRect.bottom* inputRate,
        /// inRect.right* inputRate). </code> The value for
	    /// \c inputRate is rounded to the
        /// nearest integer in Photoshop 3.0.1+. Since the scaled
        /// rectangle may exceed the real source data, it is a good
        /// idea to set some sort of padding for the input as well.
        /// </summary>
        public Int32 inputRate { get { return ptrData.inputRate; } set { ptrData.inputRate = value; } }
        /// <summary>
        /// Like \c inputRate, but as applied to the mask data.
        /// </summary>
        public Int32 maskRate { get { return ptrData.maskRate; } set { ptrData.maskRate = value; } }
        /// <summary>
        /// Function pointer to access color services routine.
        /// </summary>
        public ColorServicesProcM colorServices { get; private set; }

        /// <summary>
        /// Photoshop structures its data as follows for plug-ins when processing
        /// layer data:
        ///    target layer channels
        ///    transparency mask for target layer
        ///    layer mask channels for target layer
        ///    inverted layer mask channels for target layer
        ///    non-layer channels
        ///  When processing non-layer data (including running a filter on the
        ///  layer mask alone), Photoshop structures the data as consisting only
        ///  of non-layer channels.  It indicates this structure through a series
        ///  of short counts.  The transparency count must be either 0 or 1. */
        ///
        ///
        ///  The number of target layer planes for the input data.  
        ///                               If all the input plane values are zero, 
        ///                               then the plug-in should assume the host has not set them.
        ///                              
        ///                               @note When processing layer data, Photoshop structures 
        ///                               its input and output data for plug-ins as follows:
        ///                               - target layer channels
        ///                               - transparency mask for target layer
        ///                               - layer mask channels for target layer
        ///                               - inverted layer mask channels for target layer
        ///                               - non-layer channels	
        ///
        ///                               The output planes are a prefix of the input planes. 
        ///                               For example, in the protected transparency case, the input 
        ///                               can contain a transparency mask and a layer mask while the 
        ///                               output can contain just the layerPlanes.
        ///							   
        ///							   When processing non-layer data (including running a filter 
        ///							   on the layer mask alone), Photoshop structures the data 
        ///							   as consisting only of non-layer channels.  It indicates 
        ///							   this structure through a series of short counts.  The 
        ///							   transparency count must be either 0 or 1. 
        /// </summary>
        public short inLayerPlanes { get { return ptrData.inLayerPlanes; } set { ptrData.inLayerPlanes = value; } }

        /// <summary>
        /// The number of transparency masks for the input target layer data.
        /// See @ref inLayerPlanes for additional information.
        /// </summary>
        public short inTransparencyMask { get { return ptrData.inTransparencyMask; } set { ptrData.inTransparencyMask = value; } }

        /// <summary>
        /// The number of layer mask channels for the input 
        /// target layer.
        /// See @ref inLayerPlanes for additional information.
        /// </summary>
        public short inLayerMasks { get { return ptrData.inLayerMasks; } set { ptrData.inLayerMasks = value; } }
        /// <summary>
        /// The number of inverted layer mask channels for the 
        /// input target layer. <br> 
	    /// With the inverted layer masks, 0 = fully visible
        /// and 255 = completely hidden.See @ref inLayerPlanes for 
        /// additional information.
        /// </summary>
        public short inInvertedLayerMasks { get { return ptrData.inInvertedLayerMasks; } set { ptrData.inInvertedLayerMasks = value; } }

        /// <summary>
        /// The number of non-layer channels for the input data.  
        /// See @ref inLayerPlanes for additional information.
        /// </summary>
        public short inNonLayerPlanes { get { return ptrData.inNonLayerPlanes; } set { ptrData.inNonLayerPlanes = value; } }
        /// <summary>
        /// The number of target layer planes for the output data.
        /// See @ref inLayerPlanes for additional information about
        /// the structure of the output data.
        /// </summary>
        public short outLayerPlanes { get { return ptrData.outLayerPlanes; } set { ptrData.outLayerPlanes = value; } }

        /// <summary>
        /// The number of transparency masks for the output data.
        /// See @ref inLayerPlanes for additional information about
        /// the structure of the output data.
        /// </summary>
        public short outTransparencyMask { get { return ptrData.outTransparencyMask; } set { ptrData.outTransparencyMask = value; } }
        /// <summary>
        /// The number of layer mask channels for the output data.
        /// See @ref inLayerPlanes for additional information about
        /// the structure of the output data.
        /// </summary>
        public short outLayerMasks { get { return ptrData.outLayerMasks; } set { ptrData.outLayerMasks = value; } }
        /// <summary>
        /// The number of inverted layer mask channels for the output data.
        /// See @ref inLayerPlanes for additional information about
        /// the structure of the output data.
        /// </summary>
        public short outInvertedLayerMasks { get { return ptrData.outInvertedLayerMasks; } set { ptrData.outInvertedLayerMasks = value; } }
        /// <summary>
        /// The number of non-layer channels for the output data.
        /// See @ref inLayerPlanes for additional information about
        /// the structure of the output data.
        /// </summary>
        public short outNonLayerPlanes { get { return ptrData.outNonLayerPlanes; } set { ptrData.outNonLayerPlanes = value; } }
        /// <summary>
        /// The number of target layer planes for the input data,
        /// used for the structure of the input data when
		/// \c wantsAbsolute is TRUE.
        /// See @ref inLayerPlanes for additional information about
        /// the structure of the input data.
        /// </summary>
        public short absLayerPlanes { get { return ptrData.absLayerPlanes; } set { ptrData.absLayerPlanes = value; } }
        /// <summary>
        /// The number of transparency masks for the input data,
        /// used for the structure of the input data when
		/// \c wantsAbsolute is TRUE.
        /// See @ref inLayerPlanes for additional information about
        /// the structure of the input data.
        /// </summary>
        public short absTransparencyMask { get { return ptrData.absTransparencyMask; } set { ptrData.absTransparencyMask = value; } }
        /// <summary>
        /// The number of layer mask channels for the input data,
        /// used for the structure of the input data when
		/// \c wantsAbsolute is TRUE.
        /// See @ref inLayerPlanes for additional information about
        /// the structure of the input data.
        /// </summary>
        public short absLayerMasks { get { return ptrData.absLayerMasks; } set { ptrData.absLayerMasks = value; } }
        /// <summary>
        /// The number of inverted layer mask channels for the input 
        /// data, used for the structure of the input data when
		/// \c wantsAbsolute is TRUE.
        /// See @ref inLayerPlanes for additional information about
        /// the structure of the input data.
        /// </summary>
        public short absInvertedLayerMasks { get { return ptrData.absInvertedLayerMasks; } set { ptrData.absInvertedLayerMasks = value; } }
        /// <summary>
        /// The number of target layer planes for the input data,
        /// used for the structure of the input data when
		/// \c wantsAbsolute is TRUE.
        /// See @ref inLayerPlanes for additional information about
        /// the structure of the input data.
        /// </summary>
        public short absNonLayerPlanes { get { return ptrData.absNonLayerPlanes; } set { ptrData.absNonLayerPlanes = value; } }

        /* We allow for extra planes in the input and the output.  These planes
           will be filled with dummyPlaneValue at those times when we build the
           buffers.  These features will only be available if supportsDummyPlanes
           is TRUE. */
        /// <summary>
        /// The number of extra planes before  
        /// the input data.Only available if 
		/// \c supportsDummyChannels = TRUE.Used for 
        /// things like forcing RGB data to appear as RGBA.
        /// </summary>
        public short inPreDummyPlanes { get { return ptrData.inPreDummyPlanes; } set { ptrData.inPreDummyPlanes = value; } }
        /// <summary>
        /// The number of extra planes after 
        /// the input data.Only available if 
		/// \c supportsDummyChannels = TRUE.Used for 
        /// things like forcing RGB data to appear as RGBA.
        /// </summary>
        public short inPostDummyPlanes { get { return ptrData.inPostDummyPlanes; } set { ptrData.inPostDummyPlanes = value; } }
        /// <summary>
        /// The number of extra planes before  
        /// the output data.Only available if 
		/// \c supportsDummyChannels = TRUE.
        /// </summary>
        public short outPreDummyPlanes { get { return ptrData.outPreDummyPlanes; } set { ptrData.outPreDummyPlanes = value; } }
        /// <summary>
        /// The number of extra planes after the output data.Only available if 
		/// \c supportsDummyChannels = TRUE.
        /// </summary>
        public short outPostDummyPlanes { get { return ptrData.outPostDummyPlanes; } set { ptrData.outPostDummyPlanes = value; } }

        /* If the plug-in makes use of the layout options, then the following
           fields should be obeyed for identifying the steps between components.
           The last component in the list will always have a step of one. */
        /// <summary>
        /// The step from column to column in the input. 
        /// If using the layout options, this value may change
        /// from being equal to the number of planes.If zero,
        /// assume the host has not set it.
        /// </summary>
        public int inColumnBytes { get { return ptrData.inColumnBytes; } set { ptrData.inColumnBytes = value; } }
        /// <summary>
        /// The step from plane to plane in the input. Normally 1, 
        /// but this changes if the plug-in uses the layout options. 
        /// If zero, assume the host has not set it.
        /// </summary>
        public int inPlaneBytes { get { return ptrData.inPlaneBytes; } set { ptrData.inPlaneBytes = value; } }
        /// <summary>
        /// The output equivalent of @ref inColumnBytes.
        /// </summary>
        public int outColumnBytes { get { return ptrData.outColumnBytes; } set { ptrData.outColumnBytes = value; } }
        /// <summary>
        /// The output equivalent of @ref inPlaneBytes. 
        /// </summary>
        public int outPlaneBytes { get { return ptrData.outPlaneBytes; } set { ptrData.outPlaneBytes = value; } }

        /// <summary>
        /// Image Services callback suite.
        /// </summary>
        public IntPtr imageServicesProcs { get { return ptrData.imageServicesProcs; } set { ptrData.imageServicesProcs = value; } }

        /// <summary>
        /// Property callback suite.  The plug-in needs to
        /// dispose of the handle returned for	complex properties(the
        /// plug-in also maintains ownership of handles for
        /// set properties.
        /// </summary>
        public IntPtr propertyProcs { get { return ptrData.propertyProcs; } set { ptrData.propertyProcs = value; } }
        /// <summary>
        /// Tiling height for the input, set by host. Zero if not set. 
        /// The plug-in should work at this size, if possible.
        /// </summary>
        public short inTileHeight { get { return ptrData.inTileHeight; } set { ptrData.inTileHeight = value; } }
        /// <summary>
        /// Tiling width for the input, set by host. Zero if not set. Best to work at this size, if possible.
        /// </summary>
        public short inTileWidth { get { return ptrData.inTileWidth; } set { ptrData.inTileWidth = value; } }
        /// <summary>
        /// Tiling origin for the input, set by host. Zero if not set.
        /// </summary>
        public PSPoint inTileOrigin { get { return ptrData.inTileOrigin; } set { ptrData.inTileOrigin = value; } }
        /// <summary>
        /// Tiling height the absolute data, set by host. The plug-in should work at this size, if possible.
        /// </summary>
        public short absTileHeight { get { return ptrData.absTileHeight; } set { ptrData.absTileHeight = value; } }
        /// <summary>
        /// Tiling width the absolute data, set by host. The plug-in should work at this size, if possible.
        /// </summary>
        public short absTileWidth { get { return ptrData.absTileWidth; } set { ptrData.absTileWidth = value; } }
        /// <summary>
        /// Tiling origin the absolute data, set by host.
        /// </summary>
        public PSPoint absTileOrigin { get { return ptrData.absTileOrigin; } set { ptrData.absTileOrigin = value; } }
        /// <summary>
        /// Tiling height for the output, set by host. 
        /// The plug-in should work at this size, if possible.
        /// </summary>
        public short outTileHeight { get { return ptrData.outTileHeight; } set { ptrData.outTileHeight = value; } }
        /// <summary>
        /// Tiling width for the output, set by host. The plug-in should work at this size, if possible.
        /// </summary>
        public short outTileWidth { get { return ptrData.outTileWidth; } set { ptrData.outTileWidth = value; } }
        /// <summary>
        /// Tiling origin for the output, set by host.
        /// </summary>
        public PSPoint outTileOrigin { get { return ptrData.outTileOrigin; } set { ptrData.outTileOrigin = value; } }
        /// <summary>
        /// Tiling height for the mask, set by host. The plug-in should work at this size, if possible.
        /// </summary>
        public short maskTileHeight { get { return ptrData.maskTileHeight; } set { ptrData.maskTileHeight = value; } }
        /// <summary>
        /// Tiling width for the mask, set by host. 
        /// The plug-in should work at this size, if possible.
        /// </summary>
        public short maskTileWidth { get { return ptrData.maskTileWidth; } set { ptrData.maskTileWidth = value; } }
        /// <summary>
        /// Tiling origin for the mask, set by host.
        /// </summary>
        public PSPoint maskTileOrigin { get { return ptrData.maskTileOrigin; } set { ptrData.maskTileOrigin = value; } }
        /// <summary>
        /// Descriptor callback suite.
        /// </summary>
        public IntPtr descriptorParameters { get { return ptrData.descriptorParameters; } set { ptrData.descriptorParameters = value; } }
        /// <summary>
        /// An error reporting string to return to Photoshop.
        /// If the plug-in returns with result=errReportString then 
	    /// this string is displayed as: 
	    /// "Cannot complete operation because " + \c errorString.
        /// </summary>
        public IntPtr errorString { get { return ptrData.errorString; } set { ptrData.errorString = value; } }
        /// <summary>
        /// Channel Ports callback suite.
        /// </summary>
        public IntPtr channelPortProcs { get { return ptrData.channelPortProcs; } set { ptrData.channelPortProcs = value; } }
        /// <summary>
        /// The Channel Ports document information for the document being filtered.
        /// </summary>
        public IntPtr documentInfo { get { return ptrData.documentInfo; } set { ptrData.documentInfo = value; } }

        /// <summary>
        ///  PICA basic suite.  Provides the mechanism to access all PICA suites. 
        /// </summary>
        public IntPtr sSPBasic { get { return ptrData.sSPBasic; } set { ptrData.sSPBasic = value; } }

        /// <summary>
        /// Plug-in reference used by PICA.
        /// </summary>
        public IntPtr plugInRef { get { return ptrData.plugInRef; } set { ptrData.plugInRef = value; } }
        /// <summary>
        /// Bit depth per channel (1,8,16,32).
        /// </summary>
        public int depth { get { return ptrData.depth; } set { ptrData.depth = value; } }
        /// <summary>
        /// Handle containing the ICC profile for the image. (NULL if none)
        /// Photoshop allocates the handle using its handle suite
        /// The handle is unlocked while calling the plug-in.
        /// The handle is valid from  \c FilterSelectorStart to \c FilterSelectorFinish
        /// Photoshop will free the handle after \c FilterSelectorFinish.
        /// </summary>
        public IntPtr iCCprofileData { get { return ptrData.iCCprofileData; } set { ptrData.iCCprofileData = value; } }
        /// <summary>
        /// Size of profile.
        /// </summary>
        public int iCCprofileSize { get { return ptrData.iCCprofileSize; } set { ptrData.iCCprofileSize = value; } }
        /// <summary>
        /// Indicates if the host uses ICC Profiles. Non-zero if the host can 
        /// accept or export ICC profiles.
        /// If this is zero, don't set or dereference \c iCCprofileData.
        /// </summary> 
        public int canUseICCProfiles { get { return ptrData.canUseICCProfiles; } set { ptrData.canUseICCProfiles = value; } }


        //@name New in 7.0 

        /// <summary>
        /// Indicates if Photoshop has image scrap { get { return ptrData.scrap; } set { ptrData.scrap = value; }} non-zero if it does. 
        /// The plug-in can ask for the
        /// exporting of image scrap by setting the PiPL resource,
        /// @ref PIWantsScrapProperty.The document info for the image scrap is 
	    /// chained right behind the targeted document pointed by the
        /// @ref documentInfo field.Photoshop sets \c hasImageScrap to indicate
        /// that an image scrap is available.A plug-in can use it to tell whether
        /// Photoshop failed to export the scrap because some unknown
        /// reasons or there is no scrap at all.
        /// </summary>
        public int hasImageScrap { get { return ptrData.hasImageScrap; } set { ptrData.hasImageScrap = value; } }

        //@name New in 8.0 

        /// <summary>
        /// Support for documents larger than 30,000 pixels. NULL if host does not support big documents.
        /// </summary>
        public BigDocumentStruct bigDocumentData;

        //@name New in 10.0 

        /// <summary>
        /// Support for 3d scene data to be sent into the plugin
        /// </summary>
        public IntPtr input3DScene { get { return ptrData.input3DScene; } set { ptrData.input3DScene = value; } }

        /// <summary>
        /// Support for 3d scene to come out of the plugin
        /// </summary>
        public IntPtr output3DScene { get { return ptrData.output3DScene; } set { ptrData.output3DScene = value; } }

        /// <summary>
        /// Set by plugin this only works for 3D layers 
        /// </summary>
        public Boolean createNewLayer { get { return ptrData.createNewLayer; } set { ptrData.createNewLayer = value; } }

        //@name New in 13.0 

        /// <summary>
        /// Handle containing the ICC profile for the working
        /// profile set via color settings dialog. (NULL if none)
        /// Photoshop allocates the handle using its handle suite
        /// The handle is unlocked while calling the plug-in.
		/// The handle is valid from  \c FilterSelectorStart to \c FilterSelectorFinish
        /// Photoshop will free the handle after \c FilterSelectorFinish.
        /// </summary>
        public IntPtr iCCWorkingProfileData { get { return ptrData.iCCWorkingProfileData; } set { ptrData.iCCWorkingProfileData = value; } }   
        ///<summary>
        /// Size of working profile.
        /// </summary>
        public int iCCWorkingProfileSize { get { return ptrData.iCCWorkingProfileSize; } set { ptrData.iCCWorkingProfileSize = value; } }    /**<  */

        //name Reserved Space for Expansion

        ///<summary>
        /// Reserved for future use. Set to zero. 
        ///</summary>
        public char reserved { get { return ptrData.reserved; } set { ptrData.reserved = value; } }  

#pragma warning restore IDE1006

        public FilterRecordM()
        { }

        public static FilterRecordM Load(IntPtr ptr)
        {
            FilterRecordM filter = new FilterRecordM
            {
                pointer = ptr
            };
            filter.colorServices = filter.RunColorServices;
            filter.Read();
            return filter;
        }

        public void Read()
        {
            ptrData = (FilterRecord)Marshal.PtrToStructure(pointer, typeof(FilterRecord));
            bigDocumentData = (BigDocumentStruct)Marshal.PtrToStructure(ptrData.bigDocumentData, typeof(BigDocumentStruct));
            advanceState = (AdvanceStateProc)Marshal.GetDelegateForFunctionPointer(ptrData.advanceState, typeof(AdvanceStateProc));
        }

        public void Write()
        {
            Marshal.StructureToPtr(ptrData, pointer, true);
            Marshal.StructureToPtr(bigDocumentData, ptrData.bigDocumentData, true);
            Read();
        }

        private static byte Bool2Byte(bool value)
        {
            return (byte)(value ? 1 : 0);
        }

        private Int16 RunColorServices(ref ColorServicesInfo info)
        {
            IntPtr ptr = Marshal.AllocHGlobal(info.infoSize);
            Marshal.StructureToPtr(info, ptr, true);
            short result = ptrData.colorServices(ptr);
            info = (ColorServicesInfo)Marshal.PtrToStructure(ptr, typeof(ColorServicesInfo));
            Marshal.FreeHGlobal(ptr);
            return result;
        }
    }
}
