#version 330

in vec2 fragTexCoord;

uniform sampler2D texture0;

out vec4 finalColor;

void main() {
    vec4 tColor = texture(texture0, fragTexCoord);
    finalColor.r = 1- tColor.r;
    finalColor.g = 1- tColor.g;
    finalColor.b = 1- tColor.b;
    finalColor.a = tColor.a;
}