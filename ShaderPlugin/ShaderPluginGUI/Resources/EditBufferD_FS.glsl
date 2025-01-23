#version 330
uniform sampler2D TextureUnit0; // Original
uniform sampler2D TextureUnit1; // Buffer A
uniform sampler2D TextureUnit2; // Buffer B
uniform sampler2D TextureUnit3; // Buffer C

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
	FragColor = texture(TextureUnit3, f_UV * 2);
}