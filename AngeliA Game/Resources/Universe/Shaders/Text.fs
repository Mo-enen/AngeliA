#version 330

in vec2 fragTexCoord;
in vec4 fragColor;
uniform sampler2D texture0;

out vec4 finalColor;

void main() {
    vec4 txColor = texture(texture0, fragTexCoord);
    finalColor =  txColor* fragColor;
    finalColor.a = txColor.r;
}