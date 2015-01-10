#include "Shaders\\rVectors.fx"
static const int X_NOISE_GEN = 1619;
static const int Y_NOISE_GEN = 31337;
static const int Z_NOISE_GEN = 6971;
static const int SEED_NOISE_GEN = 1013;
static const int SHIFT_NOISE_GEN = 8;
float octavePersistence[] = { 1.0, 0.5, 0.25, 0.125, 0.0625, 0.03125, 0.015625, 0.0078125, 0.00390625, 0.001953125, 0.0009765625 };
/// Maps a value onto a cubic S-curve.
/// The derivitive of a cubic S-curve is zero at a = (float)0.0 and a =
/// 1.0
float SCurve3(float a)
{
	return (a * a * (3.0f - 2.0f * a));
}
/// Maps a value onto a quintic S-curve.
/// 
/// The first derivitive of a quintic S-curve is zero at a = (float)0.0 and
/// a = 1.0
///
/// The second derivitive of a quintic S-curve is zero at a = (float)0.0 and
/// a = 1.0
float SCurve5(float a)
{
	float a3 = a * a * a;
	float a4 = a3 * a;
	float a5 = a4 * a;
	return (6.0f * a5) - (15.0f * a4) + (10.0f * a3);
}

// Int value noise
float IntValueNoise3D(int x, int y, int z, int seed)
{
	// All constants are primes and must remain prime in order for this noise
	// function to work correctly.
	int n = (
		X_NOISE_GEN * x
		+ Y_NOISE_GEN * y
		+ Z_NOISE_GEN * z
		+ SEED_NOISE_GEN * seed)
		& 0x7fffffff;
	n = (n >> 13) ^ n;
	return (n * (n * n * 60493 + 19990303) + 1376312589) & 0x7fffffff;
}
// Gradient noise 3D
float GradientNoise3D(float fx, float fy, float fz, int ix,
	int iy, int iz, int seed)
{
	// Randomly generate a gradient vector given the integer coordinates of the
	// input value.  This implementation generates a random number and uses it
	// as an index into a normalized-vector lookup table.
	int vectorIndex = (
		X_NOISE_GEN * ix
		+ Y_NOISE_GEN * iy
		+ Z_NOISE_GEN * iz
		+ SEED_NOISE_GEN * seed)
		& 0xffffffff;
	vectorIndex ^= (vectorIndex >> SHIFT_NOISE_GEN);
	vectorIndex &= 0xff;

	float xvGradient = randomVectors[(vectorIndex << 2)];
	float yvGradient = randomVectors[(vectorIndex << 2) + 1];
	float zvGradient = randomVectors[(vectorIndex << 2) + 2];

	// Set up us another vector equal to the distance between the two vectors
	// passed to this function.
	float xvPoint = (fx - (float)ix);
	float yvPoint = (fy - (float)iy);
	float zvPoint = (fz - (float)iz);

	// Now compute the dot product of the gradient vector with the distance
	// vector.  The resulting value is gradient noise.  Apply a scaling value
	// so that this noise value ranges from -1.0 to 1.0.
	return ((xvGradient * xvPoint)
		+ (yvGradient * yvPoint)
		+ (zvGradient * zvPoint)) * 2.12f;
}

// Gradient coherent noise 3D
float GradientCoherentNoise3D(float x, float y, float z, float seed)
{
	// Create a unit-length cube aligned along an integer boundary.  This cube
	// surrounds the input point.
	int x0 = (x > 0.0 ? (int)x : (int)x - 1);
	int x1 = x0 + 1;
	int y0 = (y > 0.0 ? (int)y : (int)y - 1);
	int y1 = y0 + 1;
	int z0 = (z > 0.0 ? (int)z : (int)z - 1);
	int z1 = z0 + 1;

	// Map the difference between the coordinates of the input value and the
	// coordinates of the cube's outer-lower-left vertex onto an S-curve.
	float xs = 0, ys = 0, zs = 0;
	xs = SCurve5(x - (float)x0);
	ys = SCurve5(y - (float)y0);
	zs = SCurve5(z - (float)z0);
	

	// Now calculate the noise values at each vertex of the cube.  To generate
	// the coherent-noise value at the input point, interpolate these eight
	// noise values using the S-curve value as the interpolant (trilinear
	// interpolation.)
	float n0, n1, ix0, ix1, iy0, iy1;
	n0 = GradientNoise3D(x, y, z, x0, y0, z0, seed);
	n1 = GradientNoise3D(x, y, z, x1, y0, z0, seed);
	ix0 = lerp(n0, n1, xs);
	n0 = GradientNoise3D(x, y, z, x0, y1, z0, seed);
	n1 = GradientNoise3D(x, y, z, x1, y1, z0, seed);
	ix1 = lerp(n0, n1, xs);
	iy0 = lerp(ix0, ix1, ys);
	n0 = GradientNoise3D(x, y, z, x0, y0, z1, seed);
	n1 = GradientNoise3D(x, y, z, x1, y0, z1, seed);
	ix0 = lerp(n0, n1, xs);
	n0 = GradientNoise3D(x, y, z, x0, y1, z1, seed);
	n1 = GradientNoise3D(x, y, z, x1, y1, z1, seed);
	ix1 = lerp(n0, n1, xs);
	iy1 = lerp(ix0, ix1, ys);

	return lerp(iy0, iy1, zs);
}

float ValueNoise3D(float x, float y, float z, float seed = 0)
{
	return 1.0f - ((float)IntValueNoise3D(x, y, z, seed) / 1073741824.0f);
}

float ValueCoherentNoise3D(float x, float y, float z, int seed)
{
	// Create a unit-length cube aligned along an integer boundary.  This cube
	// surrounds the input point.
	int x0 = (x > (float)0.0 ? (int)x : (int)x - 1);
	int x1 = x0 + 1;
	int y0 = (y > (float)0.0 ? (int)y : (int)y - 1);
	int y1 = y0 + 1;
	int z0 = (z > (float)0.0 ? (int)z : (int)z - 1);
	int z1 = z0 + 1;

	// Map the difference between the coordinates of the input value and the
	// coordinates of the cube's outer-lower-left vertex onto an S-curve.
	float xs = 0, ys = 0, zs = 0;


	xs = SCurve5(x - (float)x0);
	ys = SCurve5(y - (float)y0);
	zs = SCurve5(z - (float)z0);


	// Now calculate the noise values at each vertex of the cube.  To generate
	// the coherent-noise value at the input point, interpolate these eight
	// noise values using the S-curve value as the interpolant (trilinear
	// interpolation.)
	float n0, n1, ix0, ix1, iy0, iy1;
	n0 = ValueNoise3D(x0, y0, z0, seed);
	n1 = ValueNoise3D(x1, y0, z0, seed);
	ix0 = lerp(n0, n1, xs);
	n0 = ValueNoise3D(x0, y1, z0, seed);
	n1 = ValueNoise3D(x1, y1, z0, seed);
	ix1 = lerp(n0, n1, xs);
	iy0 = lerp(ix0, ix1, ys);
	n0 = ValueNoise3D(x0, y0, z1, seed);
	n1 = ValueNoise3D(x1, y0, z1, seed);
	ix0 = lerp(n0, n1, xs);
	n0 = ValueNoise3D(x0, y1, z1, seed);
	n1 = ValueNoise3D(x1, y1, z1, seed);
	ix1 = lerp(n0, n1, xs);
	iy1 = lerp(ix0, ix1, ys);
	return lerp(iy0, iy1, zs);
}