<Shader>
	<MultiPassBuffers>NoBuffers</MultiPassBuffers>
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
uniform sampler2D TextureUnit0;

//region Photoshop Uniforms
uniform vec3	iColorBG;	// Photoshop Background Color
uniform vec3	iColorFG;	// Photoshop Foreground Color
uniform vec2	iImageSize;	// Image Size
uniform vec2	iViewSize;	// Viewport Size
uniform vec4	iRandom;	// Random values [0 .. 1]
uniform float	iTime;		// Plugin running time
uniform vec4	iDate;		// Year, Month, Day, Time in seconds
//endregion

in vec2 f_UV;

layout(location = 0) out vec4 FragColor;

void main()
{
	float Amount = 1;
	float Offset = 1.0;
	vec2 UV = Offset / iImageSize;

	vec4 T1 = texture2D(TextureUnit0, f_UV - UV);
	vec4 T2 = texture2D(TextureUnit0, f_UV + vec2(0, -UV.y));
	vec4 T3 = texture2D(TextureUnit0, f_UV + vec2(UV.x, -UV.y));
	vec4 T4 = texture2D(TextureUnit0, f_UV + vec2(-UV.x, 0));
	vec4 T5 = texture2D(TextureUnit0, f_UV);
	vec4 T6 = texture2D(TextureUnit0, f_UV + vec2(UV.x, 0));
	vec4 T7 = texture2D(TextureUnit0, f_UV + vec2(-UV.x, UV.y));
	vec4 T8 = texture2D(TextureUnit0, f_UV + vec2(0, UV.y));
	vec4 T9 = texture2D(TextureUnit0, f_UV + UV);

	vec4 SobelX = T1 - T3 + 2 * (T4 - T6) + T7 - T9;
  	vec4 SobelY = T1 + T3 + 2 * (T2 - T8) - T7 - T9;
	vec4 Sobel = sqrt(SobelX*SobelX + SobelY*SobelY);
	
	vec3 Borders = clamp(Sobel.rgb * Amount, 0, 1);
	float Result = dot(Borders, vec3(0.299, 0.587, 0.114));
    FragColor = vec4(vec3(1 - Result), T5.a);
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
			<TextureWrapModeS>Repeat</TextureWrapModeS>
			<TextureWrapModeT>Repeat</TextureWrapModeT>
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
</Shader>