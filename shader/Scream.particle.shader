// NOTE: Shader automatically converted from Godot Engine 3.4.4.stable.mono's ParticlesMaterial.

shader_type particles;
const vec3 direction = vec3(1,0,0);
const float spread = 3.25;
const float initial_linear_velocity = 28.62;
const float scale = 1.;
const float initial_linear_velocity_random = 0.43;
uniform vec4 color_value : hint_color;
const vec3 gravity = vec3(0, -98, 0);

const float max_letters = 5.;
uniform int current_letter: hint_range(0, 5) = 0;


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

void vertex() {
	uint base_number = NUMBER;
	uint alt_seed = hash(base_number + uint(1) + RANDOM_SEED);

	float pi = 3.14159;
	float degree_to_rad = pi / 180.0;

	bool restart = false;
	float tv = 0.0;
	if (CUSTOM.y > CUSTOM.w) {
		restart = true;
		tv = 1.0;
	}

	if (RESTART || restart) {
		uint alt_restart_seed = hash(base_number + uint(301184) + RANDOM_SEED);
		float tex_linear_velocity = 0.0;
		float tex_angle = 0.0;
		float anim_offset = float(current_letter) / max_letters;
		float tex_anim_offset = 0.0;
		float spread_rad = spread * degree_to_rad;
		{
			float angle1_rad = rand_from_seed_m1_p1(alt_restart_seed) * spread_rad;
			angle1_rad += direction.x != 0.0 ? atan(direction.y, direction.x) : sign(direction.y) * (pi / 2.0);
			vec3 rot = vec3(cos(angle1_rad), sin(angle1_rad), 0.0);
			VELOCITY = rot * initial_linear_velocity * mix(1.0, rand_from_seed(alt_restart_seed), initial_linear_velocity_random);
		}
		CUSTOM.y = 0.0;
		CUSTOM.w = (1.0);
		CUSTOM.z = anim_offset + tex_anim_offset;
		VELOCITY = (EMISSION_TRANSFORM * vec4(VELOCITY, 0.0)).xyz;
		TRANSFORM = EMISSION_TRANSFORM * TRANSFORM;
		VELOCITY.z = 0.0;
		TRANSFORM[3].z = 0.0;
	} else {
		CUSTOM.y += DELTA / LIFETIME;
		tv = CUSTOM.y / CUSTOM.w;
		float tex_linear_velocity = 0.0;
		float tex_anim_offset = 0.0;
		vec3 force = gravity;
		vec3 pos = TRANSFORM[3].xyz;
		pos.z = 0.0;
		// apply attractor forces
		VELOCITY += force * DELTA;
	}
	float tex_scale = 1.0;

	TRANSFORM[0] = vec4(cos(CUSTOM.x), -sin(CUSTOM.x), 0.0, 0.0);
	TRANSFORM[1] = vec4(sin(CUSTOM.x), cos(CUSTOM.x), 0.0, 0.0);
	TRANSFORM[2] = vec4(0.0, 0.0, 1.0, 0.0);

	VELOCITY.z = 0.0;
	TRANSFORM[3].z = 0.0;
	if (CUSTOM.y > CUSTOM.w) {		ACTIVE = false;
	}
}

