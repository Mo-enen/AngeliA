#version 330

in vec2 fragTexCoord;
in vec4 fragColor;
uniform sampler2D texture0;

out vec4 finalColor;

void main()
{
    vec4 texelColor = texture(texture0, fragTexCoord);
    vec4 rgb = fragColor;
    rgb.a = texelColor.a;

    finalColor = rgb + (texelColor - rgb) * fragColor.a;
    
}