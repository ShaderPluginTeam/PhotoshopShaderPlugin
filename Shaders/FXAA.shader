<Shader>
	<MultiPassBuffers>BufferA</MultiPassBuffers>
	<Image>
		<Shader_VS>#version 330
layout(location = 0) in vec2 v_UV;

out vec2 f_UV;

void main()
{
	f_UV = v_UV;
	vec2 Position = v_UV * 2.0 - 1.0;
	gl_Position = vec4(Position, 0.0, 1.0);
}</Shader_VS>
		<Shader_FS>#version 330
uniform sampler2D TextureUnit0; // Original
uniform sampler2D TextureUnit1; // Luma at Alpha

uniform vec2	iImageSize;	// Image Size

in vec2 f_UV;

layout(location = 0) out vec4 FragColor;

vec4 FxaaPixelShader(vec2 pos, sampler2D tex,
	vec2 fxaaQualityRcpFrame, float fxaaQualitySubpix,
	float fxaaQualityEdgeThreshold, float fxaaQualityEdgeThresholdMin);

#define FXAA_QUALITY__PRESET 39

// Choose the amount of sub-pixel aliasing removal. This can effect sharpness.
//   1.00 - upper limit (softer)
//   0.75 - default amount of filtering
//   0.50 - lower limit (sharper, less sub-pixel aliasing removal)
//   0.25 - almost off
//   0.00 - completely off
float fxaaQualitySubpix = 1.0;

// The minimum amount of local contrast required to apply algorithm.
//   0.333 - too little (faster)
//   0.250 - low quality
//   0.166 - default
//   0.125 - high quality 
//   0.063 - overkill (slower)
float fxaaQualityEdgeThreshold = 0.063;

// Trims the algorithm from processing darks.
//   0.0833 - upper limit (default, the start of visible unfiltered edges)
//   0.0625 - high quality (faster)
//   0.0312 - visible limit (slower)
float fxaaQualityEdgeThresholdMin = 0.0312;

void main()
{
	FragColor.rgb = FxaaPixelShader(f_UV, TextureUnit1, 1 / iImageSize,
		fxaaQualitySubpix, fxaaQualityEdgeThreshold, fxaaQualityEdgeThresholdMin).rgb;
	FragColor.a = texture(TextureUnit0, f_UV).a;
}

/*============================================================================
					NVIDIA FXAA 3.11 by TIMOTHY LOTTES
	COPYRIGHT (C) 2010, 2011 NVIDIA CORPORATION. ALL RIGHTS RESERVED.

------------------------------------------------------------------------------
					INTEGRATION - RGBL AND COLORSPACE
------------------------------------------------------------------------------
FXAA3 requires RGBL as input unless the following is set, 

  #define FXAA_GREEN_AS_LUMA 1

In which case the engine uses green in place of luma,
and requires RGB input is in a non-linear colorspace.

RGB should be LDR (low dynamic range).
Specifically do FXAA after tonemapping.

RGB data as returned by a texture fetch can be non-linear,
or linear when FXAA_GREEN_AS_LUMA is not set.
Note an "sRGB format" texture counts as linear,
because the result of a texture fetch is linear data.
Regular "RGBA8" textures in the sRGB colorspace are non-linear.

If FXAA_GREEN_AS_LUMA is not set,
luma must be stored in the alpha channel prior to running FXAA.
This luma should be in a perceptual space (could be gamma 2.0).
Example pass before FXAA where output is gamma 2.0 encoded,

  color.rgb = ToneMap(color.rgb); // linear color output
  color.rgb = sqrt(color.rgb);	// gamma 2.0 color output
  return color;

To use FXAA,

  color.rgb = ToneMap(color.rgb);  // linear color output
  color.rgb = sqrt(color.rgb);	 // gamma 2.0 color output
  color.a = dot(color.rgb, vec3(0.299, 0.587, 0.114)); // compute luma
  return color;

Another example where output is linear encoded,
say for instance writing to an sRGB formated render target,
where the render target does the conversion back to sRGB after blending,

  color.rgb = ToneMap(color.rgb); // linear color output
  return color;

To use FXAA,

  color.rgb = ToneMap(color.rgb); // linear color output
  color.a = sqrt(dot(color.rgb, vec3(0.299, 0.587, 0.114))); // compute luma
  return color;

Getting luma correct is required for the algorithm to work correctly.

------------------------------------------------------------------------------
						  BEING LINEARLY CORRECT?
------------------------------------------------------------------------------
Applying FXAA to a framebuffer with linear RGB color will look worse.
This is very counter intuitive, but happends to be true in this case.
The reason is because dithering artifacts will be more visiable 
in a linear colorspace.

------------------------------------------------------------------------------
							 COMPLEX INTEGRATION
------------------------------------------------------------------------------
Q. What if the engine is blending into RGB before wanting to run FXAA?

A. In the last opaque pass prior to FXAA,
   have the pass write out luma into alpha.
   Then blend into RGB only.
   FXAA should be able to run ok
   assuming the blending pass did not any add aliasing.
   This should be the common case for particles and common blending passes.

A. Or use FXAA_GREEN_AS_LUMA.

============================================================================*/

/*============================================================================

							 INTEGRATION KNOBS

============================================================================*/
//
// 1 = Use API.
// 0 = Don't use API.
//
/*==========================================================================*/
#extension GL_ARB_gpu_shader5: enable

#ifndef FXAA_GREEN_AS_LUMA
	//
	// For those using non-linear color,
	// and either not able to get luma in alpha, or not wanting to,
	// this enables FXAA to run using green as a proxy for luma.
	// So with this enabled, no need to pack luma in alpha.
	//
	// This will turn off AA on anything which lacks some amount of green.
	// Pure red and blue or combination of only R and B, will get no AA.
	//
	// In order to insure AA does not get turned off on colors 
	// which contain a minor amount of green.
	//
	// 1 = On.
	// 0 = Off.
	//
	#define FXAA_GREEN_AS_LUMA 0
#endif
/*--------------------------------------------------------------------------*/
#ifndef FXAA_DISCARD
	//
	// Only valid for PC OpenGL currently.
	// Probably will not work when FXAA_GREEN_AS_LUMA = 1.
	//
	// 1 = Use discard on pixels which don't need AA.
	//	 For APIs which enable concurrent TEX+ROP from same surface.
	// 0 = Return unchanged color on pixels which don't need AA.
	//
	#define FXAA_DISCARD 0
#endif
/*--------------------------------------------------------------------------*/
#ifndef FXAA_GATHER4_ALPHA
	//
	// 1 = API supports gather4 on alpha channel.
	// 0 = API does not support gather4 on alpha channel.
	//
	#ifdef GL_ARB_gpu_shader5
		#define FXAA_GATHER4_ALPHA 1
	#endif
	#ifdef GL_NV_gpu_shader5
		#define FXAA_GATHER4_ALPHA 1
	#endif
	#ifndef FXAA_GATHER4_ALPHA
		#define FXAA_GATHER4_ALPHA 0
	#endif
#endif

/*============================================================================
						FXAA QUALITY - TUNING KNOBS
------------------------------------------------------------------------------
NOTE the other tuning knobs are now in the shader function inputs!
============================================================================*/
#ifndef FXAA_QUALITY__PRESET
	//
	// Choose the quality preset.
	// This needs to be compiled into the shader as it effects code.
	// Best option to include multiple presets is to 
	// in each shader define the preset, then include this file.
	// 
	// OPTIONS
	// -----------------------------------------------------------------------
	// 10 to 15 - default medium dither (10=fastest, 15=highest quality)
	// 20 to 29 - less dither, more expensive (20=fastest, 29=highest quality)
	// 39	   - no dither, very expensive 
	//
	// NOTES
	// -----------------------------------------------------------------------
	// 12 = slightly faster then FXAA 3.9 and higher edge quality (default)
	// 13 = about same speed as FXAA 3.9 and better than 12
	// 23 = closest to FXAA 3.9 visually and performance wise
	//  _ = the lowest digit is directly related to performance
	// _  = the highest digit is directly related to style
	// 
	#define FXAA_QUALITY__PRESET 12
#endif


/*============================================================================

						   FXAA QUALITY - PRESETS

============================================================================*/

/*============================================================================
					 FXAA QUALITY - MEDIUM DITHER PRESETS
============================================================================*/
#if (FXAA_QUALITY__PRESET == 10)
	#define FXAA_QUALITY__PS 3
	#define FXAA_QUALITY__P0 1.5
	#define FXAA_QUALITY__P1 3.0
	#define FXAA_QUALITY__P2 12.0
#endif
/*--------------------------------------------------------------------------*/
#if (FXAA_QUALITY__PRESET == 11)
	#define FXAA_QUALITY__PS 4
	#define FXAA_QUALITY__P0 1.0
	#define FXAA_QUALITY__P1 1.5
	#define FXAA_QUALITY__P2 3.0
	#define FXAA_QUALITY__P3 12.0
#endif
/*--------------------------------------------------------------------------*/
#if (FXAA_QUALITY__PRESET == 12)
	#define FXAA_QUALITY__PS 5
	#define FXAA_QUALITY__P0 1.0
	#define FXAA_QUALITY__P1 1.5
	#define FXAA_QUALITY__P2 2.0
	#define FXAA_QUALITY__P3 4.0
	#define FXAA_QUALITY__P4 12.0
#endif
/*--------------------------------------------------------------------------*/
#if (FXAA_QUALITY__PRESET == 13)
	#define FXAA_QUALITY__PS 6
	#define FXAA_QUALITY__P0 1.0
	#define FXAA_QUALITY__P1 1.5
	#define FXAA_QUALITY__P2 2.0
	#define FXAA_QUALITY__P3 2.0
	#define FXAA_QUALITY__P4 4.0
	#define FXAA_QUALITY__P5 12.0
#endif
/*--------------------------------------------------------------------------*/
#if (FXAA_QUALITY__PRESET == 14)
	#define FXAA_QUALITY__PS 7
	#define FXAA_QUALITY__P0 1.0
	#define FXAA_QUALITY__P1 1.5
	#define FXAA_QUALITY__P2 2.0
	#define FXAA_QUALITY__P3 2.0
	#define FXAA_QUALITY__P4 2.0
	#define FXAA_QUALITY__P5 4.0
	#define FXAA_QUALITY__P6 12.0
#endif
/*--------------------------------------------------------------------------*/
#if (FXAA_QUALITY__PRESET == 15)
	#define FXAA_QUALITY__PS 8
	#define FXAA_QUALITY__P0 1.0
	#define FXAA_QUALITY__P1 1.5
	#define FXAA_QUALITY__P2 2.0
	#define FXAA_QUALITY__P3 2.0
	#define FXAA_QUALITY__P4 2.0
	#define FXAA_QUALITY__P5 2.0
	#define FXAA_QUALITY__P6 4.0
	#define FXAA_QUALITY__P7 12.0
#endif

/*============================================================================
					 FXAA QUALITY - LOW DITHER PRESETS
============================================================================*/
#if (FXAA_QUALITY__PRESET == 20)
	#define FXAA_QUALITY__PS 3
	#define FXAA_QUALITY__P0 1.5
	#define FXAA_QUALITY__P1 2.0
	#define FXAA_QUALITY__P2 8.0
#endif
/*--------------------------------------------------------------------------*/
#if (FXAA_QUALITY__PRESET == 21)
	#define FXAA_QUALITY__PS 4
	#define FXAA_QUALITY__P0 1.0
	#define FXAA_QUALITY__P1 1.5
	#define FXAA_QUALITY__P2 2.0
	#define FXAA_QUALITY__P3 8.0
#endif
/*--------------------------------------------------------------------------*/
#if (FXAA_QUALITY__PRESET == 22)
	#define FXAA_QUALITY__PS 5
	#define FXAA_QUALITY__P0 1.0
	#define FXAA_QUALITY__P1 1.5
	#define FXAA_QUALITY__P2 2.0
	#define FXAA_QUALITY__P3 2.0
	#define FXAA_QUALITY__P4 8.0
#endif
/*--------------------------------------------------------------------------*/
#if (FXAA_QUALITY__PRESET == 23)
	#define FXAA_QUALITY__PS 6
	#define FXAA_QUALITY__P0 1.0
	#define FXAA_QUALITY__P1 1.5
	#define FXAA_QUALITY__P2 2.0
	#define FXAA_QUALITY__P3 2.0
	#define FXAA_QUALITY__P4 2.0
	#define FXAA_QUALITY__P5 8.0
#endif
/*--------------------------------------------------------------------------*/
#if (FXAA_QUALITY__PRESET == 24)
	#define FXAA_QUALITY__PS 7
	#define FXAA_QUALITY__P0 1.0
	#define FXAA_QUALITY__P1 1.5
	#define FXAA_QUALITY__P2 2.0
	#define FXAA_QUALITY__P3 2.0
	#define FXAA_QUALITY__P4 2.0
	#define FXAA_QUALITY__P5 3.0
	#define FXAA_QUALITY__P6 8.0
#endif
/*--------------------------------------------------------------------------*/
#if (FXAA_QUALITY__PRESET == 25)
	#define FXAA_QUALITY__PS 8
	#define FXAA_QUALITY__P0 1.0
	#define FXAA_QUALITY__P1 1.5
	#define FXAA_QUALITY__P2 2.0
	#define FXAA_QUALITY__P3 2.0
	#define FXAA_QUALITY__P4 2.0
	#define FXAA_QUALITY__P5 2.0
	#define FXAA_QUALITY__P6 4.0
	#define FXAA_QUALITY__P7 8.0
#endif
/*--------------------------------------------------------------------------*/
#if (FXAA_QUALITY__PRESET == 26)
	#define FXAA_QUALITY__PS 9
	#define FXAA_QUALITY__P0 1.0
	#define FXAA_QUALITY__P1 1.5
	#define FXAA_QUALITY__P2 2.0
	#define FXAA_QUALITY__P3 2.0
	#define FXAA_QUALITY__P4 2.0
	#define FXAA_QUALITY__P5 2.0
	#define FXAA_QUALITY__P6 2.0
	#define FXAA_QUALITY__P7 4.0
	#define FXAA_QUALITY__P8 8.0
#endif
/*--------------------------------------------------------------------------*/
#if (FXAA_QUALITY__PRESET == 27)
	#define FXAA_QUALITY__PS 10
	#define FXAA_QUALITY__P0 1.0
	#define FXAA_QUALITY__P1 1.5
	#define FXAA_QUALITY__P2 2.0
	#define FXAA_QUALITY__P3 2.0
	#define FXAA_QUALITY__P4 2.0
	#define FXAA_QUALITY__P5 2.0
	#define FXAA_QUALITY__P6 2.0
	#define FXAA_QUALITY__P7 2.0
	#define FXAA_QUALITY__P8 4.0
	#define FXAA_QUALITY__P9 8.0
#endif
/*--------------------------------------------------------------------------*/
#if (FXAA_QUALITY__PRESET == 28)
	#define FXAA_QUALITY__PS 11
	#define FXAA_QUALITY__P0 1.0
	#define FXAA_QUALITY__P1 1.5
	#define FXAA_QUALITY__P2 2.0
	#define FXAA_QUALITY__P3 2.0
	#define FXAA_QUALITY__P4 2.0
	#define FXAA_QUALITY__P5 2.0
	#define FXAA_QUALITY__P6 2.0
	#define FXAA_QUALITY__P7 2.0
	#define FXAA_QUALITY__P8 2.0
	#define FXAA_QUALITY__P9 4.0
	#define FXAA_QUALITY__P10 8.0
#endif
/*--------------------------------------------------------------------------*/
#if (FXAA_QUALITY__PRESET == 29)
	#define FXAA_QUALITY__PS 12
	#define FXAA_QUALITY__P0 1.0
	#define FXAA_QUALITY__P1 1.5
	#define FXAA_QUALITY__P2 2.0
	#define FXAA_QUALITY__P3 2.0
	#define FXAA_QUALITY__P4 2.0
	#define FXAA_QUALITY__P5 2.0
	#define FXAA_QUALITY__P6 2.0
	#define FXAA_QUALITY__P7 2.0
	#define FXAA_QUALITY__P8 2.0
	#define FXAA_QUALITY__P9 2.0
	#define FXAA_QUALITY__P10 4.0
	#define FXAA_QUALITY__P11 8.0
#endif

/*============================================================================
					 FXAA QUALITY - EXTREME QUALITY
============================================================================*/
#if (FXAA_QUALITY__PRESET == 39)
	#define FXAA_QUALITY__PS 12
	#define FXAA_QUALITY__P0 1.0
	#define FXAA_QUALITY__P1 1.0
	#define FXAA_QUALITY__P2 1.0
	#define FXAA_QUALITY__P3 1.0
	#define FXAA_QUALITY__P4 1.0
	#define FXAA_QUALITY__P5 1.5
	#define FXAA_QUALITY__P6 2.0
	#define FXAA_QUALITY__P7 2.0
	#define FXAA_QUALITY__P8 2.0
	#define FXAA_QUALITY__P9 2.0
	#define FXAA_QUALITY__P10 4.0
	#define FXAA_QUALITY__P11 8.0
#endif


/*============================================================================
				   GREEN AS LUMA OPTION SUPPORT FUNCTION
============================================================================*/
#if (FXAA_GREEN_AS_LUMA == 0)
	float FxaaLuma(vec4 rgba) { return rgba.w; }
#else
	float FxaaLuma(vec4 rgba) { return rgba.y; }
#endif	




/*============================================================================

							 FXAA3 QUALITY - PC

============================================================================*/
vec4 FxaaPixelShader(
	//
	// Use noperspective interpolation here (turn off perspective interpolation).
	// {xy} = center of pixel
	vec2 pos,
	//
	// Input color texture.
	// {rgb_} = color in linear or perceptual color space
	// if (FXAA_GREEN_AS_LUMA == 0)
	//	 {___a} = luma in perceptual color space (not linear)
	sampler2D tex,
	//
	// Only used on FXAA Quality.
	// This must be from a constant/uniform.
	// {x_} = 1.0/screenWidthInPixels
	// {_y} = 1.0/screenHeightInPixels
	vec2 fxaaQualityRcpFrame,
	//
	// Only used on FXAA Quality.
	// This used to be the FXAA_QUALITY__SUBPIX define.
	// It is here now to allow easier tuning.
	// Choose the amount of sub-pixel aliasing removal.
	// This can effect sharpness.
	//   1.00 - upper limit (softer)
	//   0.75 - default amount of filtering
	//   0.50 - lower limit (sharper, less sub-pixel aliasing removal)
	//   0.25 - almost off
	//   0.00 - completely off
	float fxaaQualitySubpix,
	//
	// Only used on FXAA Quality.
	// This used to be the FXAA_QUALITY__EDGE_THRESHOLD define.
	// It is here now to allow easier tuning.
	// The minimum amount of local contrast required to apply algorithm.
	//   0.333 - too little (faster)
	//   0.250 - low quality
	//   0.166 - default
	//   0.125 - high quality 
	//   0.063 - overkill (slower)
	float fxaaQualityEdgeThreshold,
	//
	// Only used on FXAA Quality.
	// This used to be the FXAA_QUALITY__EDGE_THRESHOLD_MIN define.
	// It is here now to allow easier tuning.
	// Trims the algorithm from processing darks.
	//   0.0833 - upper limit (default, the start of visible unfiltered edges)
	//   0.0625 - high quality (faster)
	//   0.0312 - visible limit (slower)
	// Special notes when using FXAA_GREEN_AS_LUMA,
	//   Likely want to set this to zero.
	//   As colors that are mostly not-green
	//   will appear very dark in the green channel!
	//   Tune by looking at mostly non-green content,
	//   then start at zero and increase until aliasing is a problem.
	float fxaaQualityEdgeThresholdMin
) {
/*--------------------------------------------------------------------------*/
	vec2 posM;
	posM.x = pos.x;
	posM.y = pos.y;
	#if (FXAA_GATHER4_ALPHA == 1)
		#if (FXAA_DISCARD == 0)
			vec4 rgbyM = textureLod(tex, posM, 0.0);
			#if (FXAA_GREEN_AS_LUMA == 0)
				#define lumaM rgbyM.w
			#else
				#define lumaM rgbyM.y
			#endif
		#endif
		#if (FXAA_GREEN_AS_LUMA == 0)
			vec4 luma4A = textureGather(tex, posM, 3);
			vec4 luma4B = textureGatherOffset(tex, posM, ivec2(-1, -1), 3);
		#else
			vec4 luma4A = textureGather(tex, posM, 1);
			vec4 luma4B = textureGatherOffset(tex, posM, ivec2(-1, -1), 1);
		#endif
		#if (FXAA_DISCARD == 1)
			#define lumaM luma4A.w
		#endif
		#define lumaE luma4A.z
		#define lumaS luma4A.x
		#define lumaSE luma4A.y
		#define lumaNW luma4B.w
		#define lumaN luma4B.z
		#define lumaW luma4B.x
	#else
		vec4 rgbyM = textureLod(tex, posM, 0.0);
		#if (FXAA_GREEN_AS_LUMA == 0)
			#define lumaM rgbyM.w
		#else
			#define lumaM rgbyM.y
		#endif
		float lumaS = FxaaLuma(textureLodOffset(tex, posM, 0.0, ivec2( 0, 1)));
		float lumaE = FxaaLuma(textureLodOffset(tex, posM, 0.0, ivec2( 1, 0)));
		float lumaN = FxaaLuma(textureLodOffset(tex, posM, 0.0, ivec2( 0,-1)));
		float lumaW = FxaaLuma(textureLodOffset(tex, posM, 0.0, ivec2(-1, 0)));
	#endif
/*--------------------------------------------------------------------------*/
	float maxSM = max(lumaS, lumaM);
	float minSM = min(lumaS, lumaM);
	float maxESM = max(lumaE, maxSM);
	float minESM = min(lumaE, minSM);
	float maxWN = max(lumaN, lumaW);
	float minWN = min(lumaN, lumaW);
	float rangeMax = max(maxWN, maxESM);
	float rangeMin = min(minWN, minESM);
	float rangeMaxScaled = rangeMax * fxaaQualityEdgeThreshold;
	float range = rangeMax - rangeMin;
	float rangeMaxClamped = max(fxaaQualityEdgeThresholdMin, rangeMaxScaled);
	bool earlyExit = range &lt; rangeMaxClamped;
/*--------------------------------------------------------------------------*/
	if(earlyExit)
		#if (FXAA_DISCARD == 1)
			discard;
		#else
			return rgbyM;
		#endif
/*--------------------------------------------------------------------------*/
	#if (FXAA_GATHER4_ALPHA == 0)
		float lumaNW = FxaaLuma(textureLodOffset(tex, posM, 0.0, ivec2(-1,-1)));
		float lumaSE = FxaaLuma(textureLodOffset(tex, posM, 0.0, ivec2( 1, 1)));
		float lumaNE = FxaaLuma(textureLodOffset(tex, posM, 0.0, ivec2( 1,-1)));
		float lumaSW = FxaaLuma(textureLodOffset(tex, posM, 0.0, ivec2(-1, 1)));
	#else
		float lumaNE = FxaaLuma(textureLodOffset(tex, posM, 0.0, ivec2(1, -1)));
		float lumaSW = FxaaLuma(textureLodOffset(tex, posM, 0.0, ivec2(-1, 1)));
	#endif
/*--------------------------------------------------------------------------*/
	float lumaNS = lumaN + lumaS;
	float lumaWE = lumaW + lumaE;
	float subpixRcpRange = 1.0/range;
	float subpixNSWE = lumaNS + lumaWE;
	float edgeHorz1 = (-2.0 * lumaM) + lumaNS;
	float edgeVert1 = (-2.0 * lumaM) + lumaWE;
/*--------------------------------------------------------------------------*/
	float lumaNESE = lumaNE + lumaSE;
	float lumaNWNE = lumaNW + lumaNE;
	float edgeHorz2 = (-2.0 * lumaE) + lumaNESE;
	float edgeVert2 = (-2.0 * lumaN) + lumaNWNE;
/*--------------------------------------------------------------------------*/
	float lumaNWSW = lumaNW + lumaSW;
	float lumaSWSE = lumaSW + lumaSE;
	float edgeHorz4 = (abs(edgeHorz1) * 2.0) + abs(edgeHorz2);
	float edgeVert4 = (abs(edgeVert1) * 2.0) + abs(edgeVert2);
	float edgeHorz3 = (-2.0 * lumaW) + lumaNWSW;
	float edgeVert3 = (-2.0 * lumaS) + lumaSWSE;
	float edgeHorz = abs(edgeHorz3) + edgeHorz4;
	float edgeVert = abs(edgeVert3) + edgeVert4;
/*--------------------------------------------------------------------------*/
	float subpixNWSWNESE = lumaNWSW + lumaNESE;
	float lengthSign = fxaaQualityRcpFrame.x;
	bool horzSpan = edgeHorz &gt;= edgeVert;
	float subpixA = subpixNSWE * 2.0 + subpixNWSWNESE;
/*--------------------------------------------------------------------------*/
	if(!horzSpan) lumaN = lumaW;
	if(!horzSpan) lumaS = lumaE;
	if(horzSpan) lengthSign = fxaaQualityRcpFrame.y;
	float subpixB = (subpixA * (1.0/12.0)) - lumaM;
/*--------------------------------------------------------------------------*/
	float gradientN = lumaN - lumaM;
	float gradientS = lumaS - lumaM;
	float lumaNN = lumaN + lumaM;
	float lumaSS = lumaS + lumaM;
	bool pairN = abs(gradientN) &gt;= abs(gradientS);
	float gradient = max(abs(gradientN), abs(gradientS));
	if(pairN) lengthSign = -lengthSign;
	float subpixC = clamp(abs(subpixB) * subpixRcpRange, 0.0, 1.0);
/*--------------------------------------------------------------------------*/
	vec2 posB;
	posB.x = posM.x;
	posB.y = posM.y;
	vec2 offNP;
	offNP.x = (!horzSpan) ? 0.0 : fxaaQualityRcpFrame.x;
	offNP.y = ( horzSpan) ? 0.0 : fxaaQualityRcpFrame.y;
	if(!horzSpan) posB.x += lengthSign * 0.5;
	if( horzSpan) posB.y += lengthSign * 0.5;
/*--------------------------------------------------------------------------*/
	vec2 posN;
	posN.x = posB.x - offNP.x * FXAA_QUALITY__P0;
	posN.y = posB.y - offNP.y * FXAA_QUALITY__P0;
	vec2 posP;
	posP.x = posB.x + offNP.x * FXAA_QUALITY__P0;
	posP.y = posB.y + offNP.y * FXAA_QUALITY__P0;
	float subpixD = ((-2.0)*subpixC) + 3.0;
	float lumaEndN = FxaaLuma(textureLod(tex, posN, 0.0));
	float subpixE = subpixC * subpixC;
	float lumaEndP = FxaaLuma(textureLod(tex, posP, 0.0));
/*--------------------------------------------------------------------------*/
	if(!pairN) lumaNN = lumaSS;
	float gradientScaled = gradient * 1.0/4.0;
	float lumaMM = lumaM - lumaNN * 0.5;
	float subpixF = subpixD * subpixE;
	bool lumaMLTZero = lumaMM &lt; 0.0;
/*--------------------------------------------------------------------------*/
	lumaEndN -= lumaNN * 0.5;
	lumaEndP -= lumaNN * 0.5;
	bool doneN = abs(lumaEndN) &gt;= gradientScaled;
	bool doneP = abs(lumaEndP) &gt;= gradientScaled;
	if(!doneN) posN.x -= offNP.x * FXAA_QUALITY__P1;
	if(!doneN) posN.y -= offNP.y * FXAA_QUALITY__P1;
	bool doneNP = (!doneN) || (!doneP);
	if(!doneP) posP.x += offNP.x * FXAA_QUALITY__P1;
	if(!doneP) posP.y += offNP.y * FXAA_QUALITY__P1;
/*--------------------------------------------------------------------------*/
	if(doneNP) {
		if(!doneN) lumaEndN = FxaaLuma(textureLod(tex, posN.xy, 0.0));
		if(!doneP) lumaEndP = FxaaLuma(textureLod(tex, posP.xy, 0.0));
		if(!doneN) lumaEndN = lumaEndN - lumaNN * 0.5;
		if(!doneP) lumaEndP = lumaEndP - lumaNN * 0.5;
		doneN = abs(lumaEndN) &gt;= gradientScaled;
		doneP = abs(lumaEndP) &gt;= gradientScaled;
		if(!doneN) posN.x -= offNP.x * FXAA_QUALITY__P2;
		if(!doneN) posN.y -= offNP.y * FXAA_QUALITY__P2;
		doneNP = (!doneN) || (!doneP);
		if(!doneP) posP.x += offNP.x * FXAA_QUALITY__P2;
		if(!doneP) posP.y += offNP.y * FXAA_QUALITY__P2;
/*--------------------------------------------------------------------------*/
		#if (FXAA_QUALITY__PS &gt; 3)
		if(doneNP) {
			if(!doneN) lumaEndN = FxaaLuma(textureLod(tex, posN.xy, 0.0));
			if(!doneP) lumaEndP = FxaaLuma(textureLod(tex, posP.xy, 0.0));
			if(!doneN) lumaEndN = lumaEndN - lumaNN * 0.5;
			if(!doneP) lumaEndP = lumaEndP - lumaNN * 0.5;
			doneN = abs(lumaEndN) &gt;= gradientScaled;
			doneP = abs(lumaEndP) &gt;= gradientScaled;
			if(!doneN) posN.x -= offNP.x * FXAA_QUALITY__P3;
			if(!doneN) posN.y -= offNP.y * FXAA_QUALITY__P3;
			doneNP = (!doneN) || (!doneP);
			if(!doneP) posP.x += offNP.x * FXAA_QUALITY__P3;
			if(!doneP) posP.y += offNP.y * FXAA_QUALITY__P3;
/*--------------------------------------------------------------------------*/
			#if (FXAA_QUALITY__PS &gt; 4)
			if(doneNP) {
				if(!doneN) lumaEndN = FxaaLuma(textureLod(tex, posN.xy, 0.0));
				if(!doneP) lumaEndP = FxaaLuma(textureLod(tex, posP.xy, 0.0));
				if(!doneN) lumaEndN = lumaEndN - lumaNN * 0.5;
				if(!doneP) lumaEndP = lumaEndP - lumaNN * 0.5;
				doneN = abs(lumaEndN) &gt;= gradientScaled;
				doneP = abs(lumaEndP) &gt;= gradientScaled;
				if(!doneN) posN.x -= offNP.x * FXAA_QUALITY__P4;
				if(!doneN) posN.y -= offNP.y * FXAA_QUALITY__P4;
				doneNP = (!doneN) || (!doneP);
				if(!doneP) posP.x += offNP.x * FXAA_QUALITY__P4;
				if(!doneP) posP.y += offNP.y * FXAA_QUALITY__P4;
/*--------------------------------------------------------------------------*/
				#if (FXAA_QUALITY__PS &gt; 5)
				if(doneNP) {
					if(!doneN) lumaEndN = FxaaLuma(textureLod(tex, posN.xy, 0.0));
					if(!doneP) lumaEndP = FxaaLuma(textureLod(tex, posP.xy, 0.0));
					if(!doneN) lumaEndN = lumaEndN - lumaNN * 0.5;
					if(!doneP) lumaEndP = lumaEndP - lumaNN * 0.5;
					doneN = abs(lumaEndN) &gt;= gradientScaled;
					doneP = abs(lumaEndP) &gt;= gradientScaled;
					if(!doneN) posN.x -= offNP.x * FXAA_QUALITY__P5;
					if(!doneN) posN.y -= offNP.y * FXAA_QUALITY__P5;
					doneNP = (!doneN) || (!doneP);
					if(!doneP) posP.x += offNP.x * FXAA_QUALITY__P5;
					if(!doneP) posP.y += offNP.y * FXAA_QUALITY__P5;
/*--------------------------------------------------------------------------*/
					#if (FXAA_QUALITY__PS &gt; 6)
					if(doneNP) {
						if(!doneN) lumaEndN = FxaaLuma(textureLod(tex, posN.xy, 0.0));
						if(!doneP) lumaEndP = FxaaLuma(textureLod(tex, posP.xy, 0.0));
						if(!doneN) lumaEndN = lumaEndN - lumaNN * 0.5;
						if(!doneP) lumaEndP = lumaEndP - lumaNN * 0.5;
						doneN = abs(lumaEndN) &gt;= gradientScaled;
						doneP = abs(lumaEndP) &gt;= gradientScaled;
						if(!doneN) posN.x -= offNP.x * FXAA_QUALITY__P6;
						if(!doneN) posN.y -= offNP.y * FXAA_QUALITY__P6;
						doneNP = (!doneN) || (!doneP);
						if(!doneP) posP.x += offNP.x * FXAA_QUALITY__P6;
						if(!doneP) posP.y += offNP.y * FXAA_QUALITY__P6;
/*--------------------------------------------------------------------------*/
						#if (FXAA_QUALITY__PS &gt; 7)
						if(doneNP) {
							if(!doneN) lumaEndN = FxaaLuma(textureLod(tex, posN.xy, 0.0));
							if(!doneP) lumaEndP = FxaaLuma(textureLod(tex, posP.xy, 0.0));
							if(!doneN) lumaEndN = lumaEndN - lumaNN * 0.5;
							if(!doneP) lumaEndP = lumaEndP - lumaNN * 0.5;
							doneN = abs(lumaEndN) &gt;= gradientScaled;
							doneP = abs(lumaEndP) &gt;= gradientScaled;
							if(!doneN) posN.x -= offNP.x * FXAA_QUALITY__P7;
							if(!doneN) posN.y -= offNP.y * FXAA_QUALITY__P7;
							doneNP = (!doneN) || (!doneP);
							if(!doneP) posP.x += offNP.x * FXAA_QUALITY__P7;
							if(!doneP) posP.y += offNP.y * FXAA_QUALITY__P7;
/*--------------------------------------------------------------------------*/
	#if (FXAA_QUALITY__PS &gt; 8)
	if(doneNP) {
		if(!doneN) lumaEndN = FxaaLuma(textureLod(tex, posN.xy, 0.0));
		if(!doneP) lumaEndP = FxaaLuma(textureLod(tex, posP.xy, 0.0));
		if(!doneN) lumaEndN = lumaEndN - lumaNN * 0.5;
		if(!doneP) lumaEndP = lumaEndP - lumaNN * 0.5;
		doneN = abs(lumaEndN) &gt;= gradientScaled;
		doneP = abs(lumaEndP) &gt;= gradientScaled;
		if(!doneN) posN.x -= offNP.x * FXAA_QUALITY__P8;
		if(!doneN) posN.y -= offNP.y * FXAA_QUALITY__P8;
		doneNP = (!doneN) || (!doneP);
		if(!doneP) posP.x += offNP.x * FXAA_QUALITY__P8;
		if(!doneP) posP.y += offNP.y * FXAA_QUALITY__P8;
/*--------------------------------------------------------------------------*/
		#if (FXAA_QUALITY__PS &gt; 9)
		if(doneNP) {
			if(!doneN) lumaEndN = FxaaLuma(textureLod(tex, posN.xy, 0.0));
			if(!doneP) lumaEndP = FxaaLuma(textureLod(tex, posP.xy, 0.0));
			if(!doneN) lumaEndN = lumaEndN - lumaNN * 0.5;
			if(!doneP) lumaEndP = lumaEndP - lumaNN * 0.5;
			doneN = abs(lumaEndN) &gt;= gradientScaled;
			doneP = abs(lumaEndP) &gt;= gradientScaled;
			if(!doneN) posN.x -= offNP.x * FXAA_QUALITY__P9;
			if(!doneN) posN.y -= offNP.y * FXAA_QUALITY__P9;
			doneNP = (!doneN) || (!doneP);
			if(!doneP) posP.x += offNP.x * FXAA_QUALITY__P9;
			if(!doneP) posP.y += offNP.y * FXAA_QUALITY__P9;
/*--------------------------------------------------------------------------*/
			#if (FXAA_QUALITY__PS &gt; 10)
			if(doneNP) {
				if(!doneN) lumaEndN = FxaaLuma(textureLod(tex, posN.xy, 0.0));
				if(!doneP) lumaEndP = FxaaLuma(textureLod(tex, posP.xy, 0.0));
				if(!doneN) lumaEndN = lumaEndN - lumaNN * 0.5;
				if(!doneP) lumaEndP = lumaEndP - lumaNN * 0.5;
				doneN = abs(lumaEndN) &gt;= gradientScaled;
				doneP = abs(lumaEndP) &gt;= gradientScaled;
				if(!doneN) posN.x -= offNP.x * FXAA_QUALITY__P10;
				if(!doneN) posN.y -= offNP.y * FXAA_QUALITY__P10;
				doneNP = (!doneN) || (!doneP);
				if(!doneP) posP.x += offNP.x * FXAA_QUALITY__P10;
				if(!doneP) posP.y += offNP.y * FXAA_QUALITY__P10;
/*--------------------------------------------------------------------------*/
				#if (FXAA_QUALITY__PS &gt; 11)
				if(doneNP) {
					if(!doneN) lumaEndN = FxaaLuma(textureLod(tex, posN.xy, 0.0));
					if(!doneP) lumaEndP = FxaaLuma(textureLod(tex, posP.xy, 0.0));
					if(!doneN) lumaEndN = lumaEndN - lumaNN * 0.5;
					if(!doneP) lumaEndP = lumaEndP - lumaNN * 0.5;
					doneN = abs(lumaEndN) &gt;= gradientScaled;
					doneP = abs(lumaEndP) &gt;= gradientScaled;
					if(!doneN) posN.x -= offNP.x * FXAA_QUALITY__P11;
					if(!doneN) posN.y -= offNP.y * FXAA_QUALITY__P11;
					doneNP = (!doneN) || (!doneP);
					if(!doneP) posP.x += offNP.x * FXAA_QUALITY__P11;
					if(!doneP) posP.y += offNP.y * FXAA_QUALITY__P11;
/*--------------------------------------------------------------------------*/
					#if (FXAA_QUALITY__PS &gt; 12)
					if(doneNP) {
						if(!doneN) lumaEndN = FxaaLuma(textureLod(tex, posN.xy, 0.0));
						if(!doneP) lumaEndP = FxaaLuma(textureLod(tex, posP.xy, 0.0));
						if(!doneN) lumaEndN = lumaEndN - lumaNN * 0.5;
						if(!doneP) lumaEndP = lumaEndP - lumaNN * 0.5;
						doneN = abs(lumaEndN) &amp;gt;= gradientScaled;
						doneP = abs(lumaEndP) &amp;gt;= gradientScaled;
						if(!doneN) posN.x -= offNP.x * FXAA_QUALITY__P12;
						if(!doneN) posN.y -= offNP.y * FXAA_QUALITY__P12;
						doneNP = (!doneN) || (!doneP);
						if(!doneP) posP.x += offNP.x * FXAA_QUALITY__P12;
						if(!doneP) posP.y += offNP.y * FXAA_QUALITY__P12;
/*--------------------------------------------------------------------------*/
					}
					#endif
/*--------------------------------------------------------------------------*/
				}
				#endif
/*--------------------------------------------------------------------------*/
			}
			#endif
/*--------------------------------------------------------------------------*/
		}
		#endif
/*--------------------------------------------------------------------------*/
	}
	#endif
/*--------------------------------------------------------------------------*/
						}
						#endif
/*--------------------------------------------------------------------------*/
					}
					#endif
/*--------------------------------------------------------------------------*/
				}
				#endif
/*--------------------------------------------------------------------------*/
			}
			#endif
/*--------------------------------------------------------------------------*/
		}
		#endif
/*--------------------------------------------------------------------------*/
	}
/*--------------------------------------------------------------------------*/
	float dstN = posM.x - posN.x;
	float dstP = posP.x - posM.x;
	if(!horzSpan) dstN = posM.y - posN.y;
	if(!horzSpan) dstP = posP.y - posM.y;
/*--------------------------------------------------------------------------*/
	bool goodSpanN = (lumaEndN &lt; 0.0) != lumaMLTZero;
	float spanLength = (dstP + dstN);
	bool goodSpanP = (lumaEndP &lt; 0.0) != lumaMLTZero;
	float spanLengthRcp = 1.0/spanLength;
/*--------------------------------------------------------------------------*/
	bool directionN = dstN &lt; dstP;
	float dst = min(dstN, dstP);
	bool goodSpan = directionN ? goodSpanN : goodSpanP;
	float subpixG = subpixF * subpixF;
	float pixelOffset = (dst * (-spanLengthRcp)) + 0.5;
	float subpixH = subpixG * fxaaQualitySubpix;
/*--------------------------------------------------------------------------*/
	float pixelOffsetGood = goodSpan ? pixelOffset : 0.0;
	float pixelOffsetSubpix = max(pixelOffsetGood, subpixH);
	if(!horzSpan) posM.x += pixelOffsetSubpix * lengthSign;
	if( horzSpan) posM.y += pixelOffsetSubpix * lengthSign;
	#if (FXAA_DISCARD == 1)
		return textureLod(tex, posM, 0.0);
	#else
		return vec4(textureLod(tex, posM, 0.0).xyz, lumaM);
	#endif
}</Shader_FS>
		<TextureFilter>
			<TextureMagFilter>Linear</TextureMagFilter>
			<TextureMinFilter>Linear</TextureMinFilter>
			<TextureWrapModeS>ClampToEdge</TextureWrapModeS>
			<TextureWrapModeT>ClampToEdge</TextureWrapModeT>
		</TextureFilter>
		<TextureFilterBufferA>
			<TextureMagFilter>Linear</TextureMagFilter>
			<TextureMinFilter>Linear</TextureMinFilter>
			<TextureWrapModeS>ClampToEdge</TextureWrapModeS>
			<TextureWrapModeT>ClampToEdge</TextureWrapModeT>
		</TextureFilterBufferA>
		<TextureFilterBufferB>
			<TextureMagFilter>Linear</TextureMagFilter>
			<TextureMinFilter>Linear</TextureMinFilter>
			<TextureWrapModeS>Repeat</TextureWrapModeS>
			<TextureWrapModeT>Repeat</TextureWrapModeT>
		</TextureFilterBufferB>
		<TextureFilterBufferC>
			<TextureMagFilter>Linear</TextureMagFilter>
			<TextureMinFilter>Linear</TextureMinFilter>
			<TextureWrapModeS>Repeat</TextureWrapModeS>
			<TextureWrapModeT>Repeat</TextureWrapModeT>
		</TextureFilterBufferC>
		<TextureFilterBufferD>
			<TextureMagFilter>Linear</TextureMagFilter>
			<TextureMinFilter>Linear</TextureMinFilter>
			<TextureWrapModeS>Repeat</TextureWrapModeS>
			<TextureWrapModeT>Repeat</TextureWrapModeT>
		</TextureFilterBufferD>
	</Image>
	<BufferA>
		<Shader_VS>#version 330
layout(location = 0) in vec2 v_UV;

out vec2 f_UV;

void main()
{
	f_UV = v_UV;
	vec2 Position = v_UV * 2.0 - 1.0;
	gl_Position = vec4(Position, 0.0, 1.0);
}</Shader_VS>
		<Shader_FS>#version 330
uniform sampler2D TextureUnit0;

in vec2 f_UV;

layout(location = 0) out vec4 FragColor;

void main()
{
	vec4 T = texture(TextureUnit0, f_UV);
	vec3 C = T.rgb;
	float L = T.a;
	//C = sqrt(C);
	L = dot(C, vec3(0.299, 0.587, 0.114));
	//L = sqrt(L);
	FragColor = vec4(T.rgb, L);
}</Shader_FS>
		<TextureFilter>
			<TextureMagFilter>Nearest</TextureMagFilter>
			<TextureMinFilter>Nearest</TextureMinFilter>
			<TextureWrapModeS>Repeat</TextureWrapModeS>
			<TextureWrapModeT>Repeat</TextureWrapModeT>
		</TextureFilter>
	</BufferA>
</Shader>