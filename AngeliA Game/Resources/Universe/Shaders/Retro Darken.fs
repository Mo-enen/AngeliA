#version 330

in vec2 fragTexCoord;

uniform sampler2D texture0;
uniform float Amount;

out vec4 finalColor;

void main() {
    vec4 col = texture(texture0, fragTexCoord);
	float lum = 0.299 * col.r + 0.587 * col.g + 0.114 * col.b;
    if (lum < Amount){
        finalColor.rgb = col.rgb - col.rgb * Amount;
        finalColor.a = col.a;
    } else {
        finalColor.rgb = col.rgb - col.rgb * Amount * Amount;
        finalColor.a = col.a;
    }
}