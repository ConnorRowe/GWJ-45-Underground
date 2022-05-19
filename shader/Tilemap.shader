shader_type canvas_item;

uniform sampler2D tileset;
uniform sampler2D letters;
uniform vec2 offset;

const vec2 letter_offset_div = vec2(60, 60);
const vec2 rand_offset_div = vec2(20, 20);

float rand_from_seed(inout uint seed) {
	int k;
	int s = int(seed);
	if (s == 0)
	s = 305420679;
	k = s / 127773;
	s = 16807 * (s - k * 127773) - 2836 * k;
	if (s < 0)
		s += 2147483647;
	seed = uint(s);
	return float(seed % uint(65536)) / 65535.0;
}

float rand_from_seed_m1_p1(inout uint seed) {
	return rand_from_seed(seed) * 2.0 - 1.0;
}

uint hash(uint x) {
	x = ((x >> uint(16)) ^ x) * uint(73244475);
	x = ((x >> uint(16)) ^ x) * uint(73244475);
	x = (x >> uint(16)) ^ x;
	return x;
}

void fragment()
{
	// RGB packed letters sample
	vec4 lettersCol = texture(letters, ((offset / letter_offset_div) + (SCREEN_UV / SCREEN_PIXEL_SIZE) / 60.) * vec2(1, -1));
	// Base tileset texture
	vec4 tilesetCol = texture(tileset, UV);
	
	// Getting 'random' 20px blocks for showing the letters randomly
	vec2 screen_tile_uv = (SCREEN_UV / SCREEN_PIXEL_SIZE) / 20. + vec2(20, 20) + (offset / rand_offset_div);
	uint seed = hash(uint(screen_tile_uv.x) * uint(screen_tile_uv.y));
	vec3 letters_rand = vec3(rand_from_seed_m1_p1(seed),rand_from_seed_m1_p1(seed),rand_from_seed_m1_p1(seed));	
	
	COLOR = vec4(0);
	
	if(letters_rand.r > .9)
		COLOR = mix(COLOR, vec4(1,1,1,.25), lettersCol.r);
	if(letters_rand.g > .9)
		COLOR = mix(COLOR, vec4(1,1,1,.25), lettersCol.g);
	if(letters_rand.b > .9)
		COLOR = mix(COLOR, vec4(1,1,1,.25), lettersCol.b);
		
	COLOR = mix(COLOR, tilesetCol, tilesetCol.a);
}