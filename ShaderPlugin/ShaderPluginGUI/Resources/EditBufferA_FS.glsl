#version 330
uniform sampler2D TextureUnit0; // Original

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
	FragColor = texture(TextureUnit0, f_UV).bgra;
}