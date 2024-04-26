using AngeliA;

namespace AngeliaRaylib;

public class BuiltInShader {

	public const string BASIC_VS = @"
#version 330
in vec3 vertexPosition;
in vec2 vertexTexCoord;
in vec3 vertexNormal;
in vec4 vertexColor;
uniform mat4 mvp;
out vec2 fragTexCoord;
out vec4 fragColor;
void main(){
    fragTexCoord = vertexTexCoord;
    fragColor = vertexColor;
    gl_Position = mvp*vec4(vertexPosition, 1.0);
}";

	public const string LERP_FS = @"
#version 330
in vec2 fragTexCoord;
in vec4 fragColor;
uniform sampler2D texture0;
out vec4 finalColor;
void main(){
    vec4 texelColor = texture(texture0, fragTexCoord);
    vec4 rgb = fragColor;
    rgb.a = texelColor.a;
    finalColor = rgb + (texelColor - rgb) * fragColor.a;
}";
	public const string COLOR_FS = @"
#version 330
in vec2 fragTexCoord;
in vec4 fragColor;
uniform sampler2D texture0;
out vec4 finalColor;
void main() {
    finalColor = texture(texture0, fragTexCoord).a * fragColor;
}";
	public const string TEXT_FS = @"
#version 330
in vec2 fragTexCoord;
in vec4 fragColor;
uniform sampler2D texture0;
out vec4 finalColor;
void main() {
    vec4 txColor = texture(texture0, fragTexCoord);
    finalColor = txColor * fragColor;
    finalColor.a = txColor.r * fragColor.a;
}";

	public const string CHROMATIC_ABERRATION_FS = @"
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
}";
	public const string TINT_FS = @"
#version 330
in vec2 fragTexCoord;
uniform sampler2D texture0;
uniform vec4 Tint;
out vec4 finalColor;
void main() {
    finalColor = texture(texture0, fragTexCoord) * Tint;
}";
	public const string RETRO_DARKEN_FS = @"
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
}";
	public const string RETRO_LIGHTEN_FS = @"
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
}";
	public const string VIGNETTE_FS = @"
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
}";
	public const string GREYSCALE_FS = @"
#version 330
in vec2 fragTexCoord;
uniform sampler2D texture0;
out vec4 finalColor;
void main() {
    vec4 texelColor = texture(texture0, fragTexCoord);
    float gray = dot(texelColor.rgb, vec3(0.299, 0.587, 0.114));
    finalColor = vec4(gray, gray, gray, texelColor.a);
}";
	public const string INVERT_FS = @"
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
}";
	public const string INV_FS = @"
#version 330
in vec2 fragTexCoord;
uniform vec2 screenSize;
uniform sampler2D screenTexture;
uniform sampler2D texture0;
out vec4 finalColor;
void main() {
	vec2 pos = vec2(gl_FragCoord.x / screenSize.x, gl_FragCoord.y / screenSize.y);
    vec4 tColor = texture(texture0, fragTexCoord);
	vec4 screenColor = texture(screenTexture, pos);
	if(screenColor.r + screenColor.g + screenColor.b < 1.5){
		finalColor = tColor;		
	}else{
		finalColor.r = 1 - tColor.r;
		finalColor.g = 1 - tColor.g;
		finalColor.b = 1 - tColor.b;
	}
    finalColor.a = tColor.a;
}";

	public static readonly string[] EFFECTS = new string[Const.SCREEN_EFFECT_COUNT] { CHROMATIC_ABERRATION_FS, TINT_FS, RETRO_DARKEN_FS, RETRO_LIGHTEN_FS, VIGNETTE_FS, GREYSCALE_FS, INVERT_FS, };

}