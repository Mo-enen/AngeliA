#version 330

in vec2 fragTexCoord;

uniform float RedX;
uniform float RedY;
uniform float GreenX;
uniform float GreenY;
uniform float BlueX;
uniform float BlueY;
uniform sampler2D texture0;

out vec4 finalColor;

void main() {
   finalColor.r = texture(texture0, fragTexCoord + vec2(RedX, RedY)).r; 
   finalColor.g = texture(texture0, fragTexCoord + vec2(GreenX, GreenY)).g; 
   finalColor.b = texture(texture0, fragTexCoord + vec2(BlueX, BlueY)).b;
   finalColor.a = 1;
}