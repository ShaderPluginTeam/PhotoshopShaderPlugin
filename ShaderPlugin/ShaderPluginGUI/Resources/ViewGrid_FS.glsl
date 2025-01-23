#version 330

in vec2 f_UV;

uniform vec2 Size = vec2(32); // TextureSize / GridSize
uniform vec4 ColorDark  = vec4(vec3(0.3), 0.0);
uniform vec4 ColorLight = vec4(vec3(0.7), 0.0);

layout(location = 0) out vec4 FragColor;

void main()
{
	// vec2 UV = vec2(f_UV.x, 1.0 - f_UV.y); // Starting from Top Left corner (Photoshop Like)
	vec2 UV = f_UV - 0.5; // Starting from Center
    vec2 Pos = floor(UV * Size);
	float Checker = mod(Pos.x + Pos.y, 2.0);
    FragColor = mix(ColorDark, ColorLight, Checker);
}