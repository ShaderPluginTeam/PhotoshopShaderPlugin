#version 330
uniform sampler2D TextureUnit0; // Original
uniform sampler2D TextureUnit1; // Processed
uniform sampler2D TextureUnit2; // Buffer A
uniform sampler2D TextureUnit3; // Buffer B
uniform sampler2D TextureUnit4; // Buffer C
uniform sampler2D TextureUnit5; // Buffer D

in vec2 f_UV;

uniform ivec2 DrawMode = ivec2(0); // Draw Mode; Buffer Draw Mode (Image, A, B, C, D)
uniform float PreviewPosition = 0.5;

layout(location = 0) out vec4 FragColor;

void main()
{
	vec4 Original = texture(TextureUnit0, f_UV);
	vec4 Processed = texture(TextureUnit1, f_UV);

	switch(DrawMode.y) // Buffer Draw Mode
	{
		case 1:
			Processed = texture(TextureUnit2, f_UV);
			break;
		case 2:
			Processed = texture(TextureUnit3, f_UV);
			break;
		case 3:
			Processed = texture(TextureUnit4, f_UV);
			break;
		case 4:
			Processed = texture(TextureUnit5, f_UV);
			break;
	}

	vec4 Result = mix(Original, Processed, 1.0 - step(PreviewPosition, f_UV.x)); // Original at right side
	FragColor.a = 1.0;

	switch(DrawMode.x)
	{
		default:
		case 0: // RGBA
			FragColor = Result.rgba;
			break;
		case 1: // RGB
			FragColor.rgb = Result.rgb;
			break;
		case 2: // Red
			FragColor.rgb = Result.rrr;
			break;
		case 3: // Green
			FragColor.rgb = Result.ggg;
			break;
		case 4: // Blue
			FragColor.rgb = Result.bbb;
			break;
		case 5: // Alpha
			FragColor.rgb = Result.aaa;
			break;
	}
}