//========================================================================================
//  FRACTAL BROWNIAN MOTION IMPLEMENTATION OBTAINED FROM  https://thebookofshaders.com/13/
//========================================================================================
//  Translated to HLSL
//  FBM is pretty cool, look it up!

float random(float2 st) {
	return frac(sin(dot(st.xy, float2(12.9898, 78.233)))*43758.5453123);
}

float noise(float2 st) 
{

	float2 i = floor(st);
	float2 f = frac(st);

	float k = random(i);
	float l = random(i + float2(1.0, 0.0));
	float m = random(i + float2(0.0, 1.0));
	float n = random(i + float2(1.0, 1.0));

	// Smooth Interpolation
	float2 u = smoothstep(0.0, 1.0, f);

	// Mix 4 coorners percentages
	return lerp(k, l, u.x) +
		(m - k)* u.y * (1.0 - u.x) +
		(n - l) * u.x * u.y;
}

#define OCTAVES 6
float fbm(in float2 st) {
	// Initial values
	float value = 0.0;
	float amplitude = .5;
	float frequency = 0.;

	// Loop through octaves
	[unroll(OCTAVES)] //unroll the loop for sweet performance
	for (int i = 0; i < OCTAVES; i++) {
		value += amplitude * noise(st);
		st *= 2.;
		amplitude *= .5;
	}
	return value;
}

//========================================================================================
//========================================================================================
//========================================================================================
