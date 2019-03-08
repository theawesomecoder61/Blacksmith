#version 330
 
in vec3 vPosition;
in vec3 vNormal;
out vec3 v_norm;
 
uniform mat4 modelview;
 
void main()
{
	gl_Position = modelview * vec4(vPosition, 1.0);
	v_norm = normalize(mat3(modelview) * vNormal);
	v_norm = vNormal;
}