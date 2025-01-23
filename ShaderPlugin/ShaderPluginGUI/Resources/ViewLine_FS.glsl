#version 330

uniform vec4 LineColor = vec4(0.8, 0.8, 0.8, 0.0);

layout(location = 0) out vec4 FragColor;

void main()
{
    FragColor = LineColor;
}