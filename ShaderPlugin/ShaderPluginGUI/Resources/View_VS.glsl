#version 330
layout(location = 0) in vec2 v_Position;
layout(location = 1) in vec2 v_UV;

uniform mat4 MVP = mat4(1.0);

out vec2 f_UV;

void main()
{
	f_UV = v_UV;
	gl_Position = MVP * vec4(v_Position, 0.0, 1.0);
}