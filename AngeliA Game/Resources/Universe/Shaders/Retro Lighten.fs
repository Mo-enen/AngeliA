#version 330

in vec2 fragTexCoord;

uniform sampler2D texture0;
uniform float Amount;

out vec4 finalColor;

void main() {
    vec4 col = texture(texture0, fragTexCoord);
	float lum = 0.299 * col.r + 0.587 * col.g + 0.114 * col.b;
    if (lum < Amount){
        finalColor.r = col.r + (1 - col.r) * Amount;
        finalColor.g = col.g + (1 - col.g) * Amount;
        finalColor.b = col.b + (1 - col.b) * Amount;
        finalColor.a = col.a;
    } else {
        finalColor.r = col.r + (1 - col.r) * Amount * Amount;
        finalColor.g = col.g + (1 - col.g) * Amount * Amount;
        finalColor.b = col.b + (1 - col.b) * Amount * Amount;
        finalColor.a = col.a;
    }
}