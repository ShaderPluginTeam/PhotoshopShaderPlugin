#version 330
layout(location = 0) in vec2 v_UV;

uniform int EditingMode = 0;

out vec2 f_UV;

void main()
{
	f_UV = v_UV;
	vec2 Position = v_UV * 2.0 - 1.0;
	gl_Position = vec4(Position, 0.0, 1.0);

	if ((EditingMode & 1024) == 1024) // FlipUV_Y
		f_UV.y = 1.0 - f_UV.y;
}