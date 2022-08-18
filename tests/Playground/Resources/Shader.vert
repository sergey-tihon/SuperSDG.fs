#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aColor;

out vec3 ourColor;
uniform float dx;

void main()
{
    gl_Position = vec4(dx+aPos.x, aPos.yz, 1.0);
    ourColor = aColor;
}
