#version 330

in vec2 fragTexCoord;

uniform sampler2D texture0;

uniform float OffsetX;
uniform float OffsetY;
uniform float Round;
uniform float Aspect;
uniform float Radius;
uniform float Feather;

out vec4 finalColor;

void main() {
    vec4 col = texture(texture0, fragTexCoord);
    vec2 newUV = vec2(fragTexCoord.x * 2 - 1 - OffsetX, fragTexCoord.y * 2 - 1 - OffsetY);
    vec2 round = vec2(1 + (Aspect - 1) * Round, 1);
    float lenX = newUV.x * round.x;
    float lenY = newUV.y * round.y;
    float circle = sqrt(lenX * lenX + lenY * lenY);
    float t = clamp((circle - Radius) / Feather, 0, 1);
    float mask = 1- t * t * (3 - 2 * t);
    finalColor = vec4(col.r * mask, col.g * mask, col.b * mask, 1);
}