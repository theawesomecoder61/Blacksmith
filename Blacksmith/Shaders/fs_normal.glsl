#version 330
 
in vec3 v_norm;
out vec4 outputColor;
 
void main()
{
	vec3 n = normalize(v_norm); 
	outputColor = vec4(0.5 + 0.5 * n, 1.0);
}