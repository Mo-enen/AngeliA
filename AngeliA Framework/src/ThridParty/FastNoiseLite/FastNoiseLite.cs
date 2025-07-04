// Fast Noise Lite
// .'',;:cldxkO00KKXXNNWWWNNXKOkxdollcc::::::;:::ccllloooolllllllllooollc:,'...        ...........',;cldxkO000Okxdlc::;;;,,;;;::cclllllll
// ..',;:ldxO0KXXNNNNNNNNXXK0kxdolcc::::::;;;,,,,,,;;;;;;;;;;:::cclllllc:;'....       ...........',;:ldxO0KXXXK0Okxdolc::;;;;::cllodddddo
// ...',:loxO0KXNNNNNXXKK0Okxdolc::;::::::::;;;,,'''''.....''',;:clllllc:;,'............''''''''',;:loxO0KXNNNNNXK0Okxdollccccllodxxxxxxd
// ....';:ldkO0KXXXKK00Okxdolcc:;;;;;::cclllcc:;;,''..... ....',;clooddolcc:;;;;,,;;;;;::::;;;;;;:cloxk0KXNWWWWWWNXKK0Okxddoooddxxkkkkkxx
// .....';:ldxkOOOOOkxxdolcc:;;;,,,;;:cllooooolcc:;'...      ..,:codxkkkxddooollloooooooollcc:::::clodkO0KXNWWWWWWNNXK00Okxxxxxxxxkkkkxxx
// . ....';:cloddddo___________,,,,;;:clooddddoolc:,...      ..,:ldx__00OOOkkk___kkkkkkxxdollc::::cclodkO0KXXNNNNNNXXK0OOkxxxxxxxxxxxxddd
// .......',;:cccc:|           |,,,;;:cclooddddoll:;'..     ..';cox|  \KKK000|   |KK00OOkxdocc___;::clldxxkO0KKKKK00Okkxdddddddddddddddoo
// .......'',,,,,''|   ________|',,;;::cclloooooolc:;'......___:ldk|   \KK000|   |XKKK0Okxolc|   |;;::cclodxxkkkkxxdoolllcclllooodddooooo
// ''......''''....|   |  ....'',,,,;;;::cclloooollc:;,''.'|   |oxk|    \OOO0|   |KKK00Oxdoll|___|;;;;;::ccllllllcc::;;,,;;;:cclloooooooo
// ;;,''.......... |   |_____',,;;;____:___cllo________.___|   |___|     \xkk|   |KK_______ool___:::;________;;;_______...'',;;:ccclllloo
// c:;,''......... |         |:::/     '   |lo/        |           |      \dx|   |0/       \d|   |cc/        |'/       \......',,;;:ccllo
// ol:;,'..........|    _____|ll/    __    |o/   ______|____    ___|   |   \o|   |/   ___   \|   |o/   ______|/   ___   \ .......'',;:clo
// dlc;,...........|   |::clooo|    /  |   |x\___   \KXKKK0|   |dol|   |\   \|   |   |   |   |   |d\___   \..|   |  /   /       ....',:cl
// xoc;'...  .....'|   |llodddd|    \__|   |_____\   \KKK0O|   |lc:|   |'\       |   |___|   |   |_____\   \.|   |_/___/...      ...',;:c
// dlc;'... ....',;|   |oddddddo\          |          |Okkx|   |::;|   |..\      |\         /|   |          | \         |...    ....',;:c
// ol:,'.......',:c|___|xxxddollc\_____,___|_________/ddoll|___|,,,|___|...\_____|:\ ______/l|___|_________/...\________|'........',;::cc
// c:;'.......';:codxxkkkkxxolc::;::clodxkOO0OOkkxdollc::;;,,''''',,,,''''''''''',,'''''',;:loxkkOOkxol:;,'''',,;:ccllcc:;,'''''',;::ccll
// ;,'.......',:codxkOO0OOkxdlc:;,,;;:cldxxkkxxdolc:;;,,''.....'',;;:::;;,,,'''''........,;cldkO0KK0Okdoc::;;::cloodddoolc:;;;;;::ccllooo
// .........',;:lodxOO0000Okdoc:,,',,;:clloddoolc:;,''.......'',;:clooollc:;;,,''.......',:ldkOKXNNXX0Oxdolllloddxxxxxxdolccccccllooodddd
// .    .....';:cldxkO0000Okxol:;,''',,;::cccc:;,,'.......'',;:cldxxkkxxdolc:;;,'.......';coxOKXNWWWNXKOkxddddxxkkkkkkxdoollllooddxxxxkkk
//       ....',;:codxkO000OOxdoc:;,''',,,;;;;,''.......',,;:clodkO00000Okxolc::;,,''..',;:ldxOKXNWWWNNK0OkkkkkkkkkkkxxddooooodxxkOOOOO000
//       ....',;;clodxkkOOOkkdolc:;,,,,,,,,'..........,;:clodxkO0KKXKK0Okxdolcc::;;,,,;;:codkO0XXNNNNXKK0OOOOOkkkkxxdoollloodxkO0KKKXXXXX

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace JordanPeck;

public partial class FastNoiseLite {




	#region --- VAR ---


	public int Seed { get => mSeed; set => mSeed = value; }
	public float Min {
		get => mMin;
		set {
			mMin = value;
			mRange = mMax - mMin;
		}
	}
	public float Max {
		get => mMax;
		set {
			mMax = value;
			mRange = mMax - mMin;
		}
	}
	public float Frequency { get => mFrequency; set => mFrequency = value; }
	public NoiseType NoiseType {
		get => mNoiseType;
		set {
			mNoiseType = value;
			UpdateTransformType3D();
		}
	}
	public FractalType FractalType { get => mFractalType; set => mFractalType = value; }
	public RotationType3D RotationType3D {
		get => mRotationType3D;
		set {
			mRotationType3D = value;
			UpdateTransformType3D();
			UpdateWarpTransformType3D();
		}
	}
	public int Octaves {
		get => mOctaves;
		set {
			mOctaves = value;
			CalculateFractalBounding();
		}
	}
	public float Lacunarity { get => mLacunarity; set => mLacunarity = value; }
	public float Gain {
		get => mGain;
		set {
			mGain = value;
			CalculateFractalBounding();
		}
	}
	public float WeightedStrength { get => mWeightedStrength; set => mWeightedStrength = value; }
	public float PingPongStrength { get => mPingPongStrength; set => mPingPongStrength = value; }
	public CellularDistanceFunction CellularDistanceFunction { get => mCellularDistanceFunction; set => mCellularDistanceFunction = value; }
	public CellularReturnType CellularReturnType { get => mCellularReturnType; set => mCellularReturnType = value; }
	public float CellularJitterModifier { get => mCellularJitterModifier; set => mCellularJitterModifier = value; }
	public DomainWarpType DomainWarpType {
		get => mDomainWarpType;
		set {
			mDomainWarpType = value;
			UpdateWarpTransformType3D();
		}
	}
	public int DomainWarpAmp { get => mDomainWarpAmp; set => mDomainWarpAmp = value; }

	private int mSeed;
	private float mMin = 0f;
	private float mMax = 1f;
	private float mFrequency = 0.01f;
	private NoiseType mNoiseType = NoiseType.OpenSimplex2;
	private RotationType3D mRotationType3D = RotationType3D.None;
	private FractalType mFractalType = FractalType.None;
	private int mOctaves = 3;
	private float mLacunarity = 2.0f;
	private float mGain = 0.5f;
	private float mWeightedStrength = 0.0f;
	private float mPingPongStrength = 2.0f;
	private CellularDistanceFunction mCellularDistanceFunction = CellularDistanceFunction.EuclideanSq;
	private CellularReturnType mCellularReturnType = CellularReturnType.Distance;
	private float mCellularJitterModifier = 1.0f;
	private int mDomainWarpAmp = 0;
	private DomainWarpType mDomainWarpType = DomainWarpType.OpenSimplex2;

	// Data
	private TransformType3D mWarpTransformType3D = TransformType3D.DefaultOpenSimplex2;
	private TransformType3D mTransformType3D = TransformType3D.DefaultOpenSimplex2;
	private float mFractalBounding = 1 / 1.75f;
	private float mRange = 1f;


	#endregion




	#region --- API ---


	public FastNoiseLite () => mSeed = 28926;


	[MethodImpl(OPTIMISE)]
	public float GetNoise (float x, float y) {
		// Domain Warp
		if (DomainWarpAmp != 0) {
			switch (mFractalType) {
				default:
					DomainWarpSingle(ref x, ref y);
					break;
				case FractalType.DomainWarpProgressive:
					DomainWarpFractalProgressive(ref x, ref y);
					break;
				case FractalType.DomainWarpIndependent:
					DomainWarpFractalIndependent(ref x, ref y);
					break;
			}
		}
		// Get Noise
		TransformNoiseCoordinate(ref x, ref y);
		float value = mFractalType switch {
			FractalType.FBm => GenFractalFBm(x, y),
			FractalType.Ridged => GenFractalRidged(x, y),
			FractalType.PingPong => GenFractalPingPong(x, y),
			_ => GenNoiseSingle(mSeed, x, y),
		};
		return mMin + mRange * ((value + 1f) / 2f);
	}


	[MethodImpl(OPTIMISE)]
	public float GetNoise (float x, float y, float z) {
		// Domain Warp
		if (DomainWarpAmp != 0) {
			switch (mFractalType) {
				default:
					DomainWarpSingle(ref x, ref y, ref z);
					break;
				case FractalType.DomainWarpProgressive:
					DomainWarpFractalProgressive(ref x, ref y, ref z);
					break;
				case FractalType.DomainWarpIndependent:
					DomainWarpFractalIndependent(ref x, ref y, ref z);
					break;
			}
		}
		// Get Noise
		TransformNoiseCoordinate(ref x, ref y, ref z);
		float value = mFractalType switch {
			FractalType.FBm => GenFractalFBm(x, y, z),
			FractalType.Ridged => GenFractalRidged(x, y, z),
			FractalType.PingPong => GenFractalPingPong(x, y, z),
			_ => GenNoiseSingle(mSeed, x, y, z),
		};
		return mMin + mRange * ((value + 1f) / 2f);
	}


	#endregion




	#region --- LGC ---


	// Update Cache
	private void CalculateFractalBounding () {
		float gain = FastAbs(mGain);
		float amp = gain;
		float ampFractal = 1.0f;
		for (int i = 1; i < mOctaves; i++) {
			ampFractal += amp;
			amp *= gain;
		}
		mFractalBounding = 1 / ampFractal;
	}

	private void UpdateTransformType3D () => mTransformType3D = mRotationType3D switch {
		RotationType3D.ImproveXYPlanes => TransformType3D.ImproveXYPlanes,
		RotationType3D.ImproveXZPlanes => TransformType3D.ImproveXZPlanes,
		_ => mNoiseType switch {
			NoiseType.OpenSimplex2 or NoiseType.OpenSimplex2S => TransformType3D.DefaultOpenSimplex2,
			_ => TransformType3D.None,
		},
	};

	private void UpdateWarpTransformType3D () => mWarpTransformType3D = mRotationType3D switch {
		RotationType3D.ImproveXYPlanes => TransformType3D.ImproveXYPlanes,
		RotationType3D.ImproveXZPlanes => TransformType3D.ImproveXZPlanes,
		_ => mDomainWarpType switch {
			DomainWarpType.OpenSimplex2 or DomainWarpType.OpenSimplex2Reduced => TransformType3D.DefaultOpenSimplex2,
			_ => TransformType3D.None,
		},
	};


	[MethodImpl(INLINE)]
	private void TransformDomainWarpCoordinate (ref float x, ref float y) {
		switch (mDomainWarpType) {
			case DomainWarpType.OpenSimplex2:
			case DomainWarpType.OpenSimplex2Reduced: {
				const float SQRT3 = (float)1.7320508075688772935274463415059;
				const float F2 = 0.5f * (SQRT3 - 1);
				float t = (x + y) * F2;
				x += t; y += t;
			}
			break;
			default:
				break;
		}
	}


	[MethodImpl(INLINE)]
	private void TransformDomainWarpCoordinate (ref float x, ref float y, ref float z) {
		switch (mWarpTransformType3D) {
			case TransformType3D.ImproveXYPlanes: {
				float xy = x + y;
				float s2 = xy * -(float)0.211324865405187;
				z *= (float)0.577350269189626;
				x += s2 - z;
				y = y + s2 - z;
				z += xy * (float)0.577350269189626;
			}
			break;
			case TransformType3D.ImproveXZPlanes: {
				float xz = x + z;
				float s2 = xz * -(float)0.211324865405187;
				y *= (float)0.577350269189626;
				x += s2 - y; z += s2 - y;
				y += xz * (float)0.577350269189626;
			}
			break;
			case TransformType3D.DefaultOpenSimplex2: {
				const float R3 = (float)(2.0 / 3.0);
				float r = (x + y + z) * R3; // Rotation, not skew
				x = r - x;
				y = r - y;
				z = r - z;
			}
			break;
			default:
				break;
		}
	}


	// Generic noise gen
	private float GenNoiseSingle (int seed, float x, float y) => mNoiseType switch {
		NoiseType.OpenSimplex2 => SingleSimplex(seed, x, y),
		NoiseType.OpenSimplex2S => SingleOpenSimplex2S(seed, x, y),
		NoiseType.Cellular => SingleCellular(seed, x, y),
		NoiseType.Perlin => SinglePerlin(seed, x, y),
		NoiseType.ValueCubic => SingleValueCubic(seed, x, y),
		NoiseType.Value => SingleValue(seed, x, y),
		_ => 0,
	};

	private float GenNoiseSingle (int seed, float x, float y, float z) {
		return mNoiseType switch {
			NoiseType.OpenSimplex2 => SingleOpenSimplex2(seed, x, y, z),
			NoiseType.OpenSimplex2S => SingleOpenSimplex2S(seed, x, y, z),
			NoiseType.Cellular => SingleCellular(seed, x, y, z),
			NoiseType.Perlin => SinglePerlin(seed, x, y, z),
			NoiseType.ValueCubic => SingleValueCubic(seed, x, y, z),
			NoiseType.Value => SingleValue(seed, x, y, z),
			_ => 0,
		};
	}


	// Noise Coordinate Transforms (frequency, and possible skew or rotation)
	[MethodImpl(INLINE)]
	private void TransformNoiseCoordinate (ref float x, ref float y) {
		x *= mFrequency;
		y *= mFrequency;

		switch (mNoiseType) {
			case NoiseType.OpenSimplex2:
			case NoiseType.OpenSimplex2S: {
				const float SQRT3 = (float)1.7320508075688772935274463415059;
				const float F2 = 0.5f * (SQRT3 - 1);
				float t = (x + y) * F2;
				x += t;
				y += t;
			}
			break;
			default:
				break;
		}
	}


	[MethodImpl(INLINE)]
	private void TransformNoiseCoordinate (ref float x, ref float y, ref float z) {
		x *= mFrequency;
		y *= mFrequency;
		z *= mFrequency;

		switch (mTransformType3D) {
			case TransformType3D.ImproveXYPlanes: {
				float xy = x + y;
				float s2 = xy * -(float)0.211324865405187;
				z *= (float)0.577350269189626;
				x += s2 - z;
				y = y + s2 - z;
				z += xy * (float)0.577350269189626;
			}
			break;
			case TransformType3D.ImproveXZPlanes: {
				float xz = x + z;
				float s2 = xz * -(float)0.211324865405187;
				y *= (float)0.577350269189626;
				x += s2 - y;
				z += s2 - y;
				y += xz * (float)0.577350269189626;
			}
			break;
			case TransformType3D.DefaultOpenSimplex2: {
				const float R3 = (float)(2.0 / 3.0);
				float r = (x + y + z) * R3; // Rotation, not skew
				x = r - x;
				y = r - y;
				z = r - z;
			}
			break;
			default:
				break;
		}
	}


	// Fractal FBm
	private float GenFractalFBm (float x, float y) {
		int seed = mSeed;
		float sum = 0;
		float amp = mFractalBounding;

		for (int i = 0; i < mOctaves; i++) {
			float noise = GenNoiseSingle(seed++, x, y);
			sum += noise * amp;
			amp *= Lerp(1.0f, FastMin(noise + 1, 2) * 0.5f, mWeightedStrength);

			x *= mLacunarity;
			y *= mLacunarity;
			amp *= mGain;
		}

		return sum;
	}

	private float GenFractalFBm (float x, float y, float z) {
		int seed = mSeed;
		float sum = 0;
		float amp = mFractalBounding;

		for (int i = 0; i < mOctaves; i++) {
			float noise = GenNoiseSingle(seed++, x, y, z);
			sum += noise * amp;
			amp *= Lerp(1.0f, (noise + 1) * 0.5f, mWeightedStrength);

			x *= mLacunarity;
			y *= mLacunarity;
			z *= mLacunarity;
			amp *= mGain;
		}

		return sum;
	}


	// Fractal Ridged
	private float GenFractalRidged (float x, float y) {
		int seed = mSeed;
		float sum = 0;
		float amp = mFractalBounding;

		for (int i = 0; i < mOctaves; i++) {
			float noise = FastAbs(GenNoiseSingle(seed++, x, y));
			sum += (noise * -2 + 1) * amp;
			amp *= Lerp(1.0f, 1 - noise, mWeightedStrength);

			x *= mLacunarity;
			y *= mLacunarity;
			amp *= mGain;
		}

		return sum;
	}

	private float GenFractalRidged (float x, float y, float z) {
		int seed = mSeed;
		float sum = 0;
		float amp = mFractalBounding;

		for (int i = 0; i < mOctaves; i++) {
			float noise = FastAbs(GenNoiseSingle(seed++, x, y, z));
			sum += (noise * -2 + 1) * amp;
			amp *= Lerp(1.0f, 1 - noise, mWeightedStrength);

			x *= mLacunarity;
			y *= mLacunarity;
			z *= mLacunarity;
			amp *= mGain;
		}

		return sum;
	}


	// Fractal PingPong 
	private float GenFractalPingPong (float x, float y) {
		int seed = mSeed;
		float sum = 0;
		float amp = mFractalBounding;

		for (int i = 0; i < mOctaves; i++) {
			float noise = PingPong((GenNoiseSingle(seed++, x, y) + 1) * mPingPongStrength);
			sum += (noise - 0.5f) * 2 * amp;
			amp *= Lerp(1.0f, noise, mWeightedStrength);

			x *= mLacunarity;
			y *= mLacunarity;
			amp *= mGain;
		}

		return sum;
	}

	private float GenFractalPingPong (float x, float y, float z) {
		int seed = mSeed;
		float sum = 0;
		float amp = mFractalBounding;

		for (int i = 0; i < mOctaves; i++) {
			float noise = PingPong((GenNoiseSingle(seed++, x, y, z) + 1) * mPingPongStrength);
			sum += (noise - 0.5f) * 2 * amp;
			amp *= Lerp(1.0f, noise, mWeightedStrength);

			x *= mLacunarity;
			y *= mLacunarity;
			z *= mLacunarity;
			amp *= mGain;
		}

		return sum;
	}


	// Simplex/OpenSimplex2 Noise

	private float SingleSimplex (int seed, float x, float y) {
		// 2D OpenSimplex2 case uses the same algorithm as ordinary Simplex.

		const float SQRT3 = 1.7320508075688772935274463415059f;
		const float G2 = (3 - SQRT3) / 6;

		/*
		 * --- Skew moved to TransformNoiseCoordinate method ---
		 * const FNfloat F2 = 0.5f * (SQRT3 - 1);
		 * FNfloat s = (x + y) * F2;
		 * x += s; y += s;
		*/

		int i = FastFloor(x);
		int j = FastFloor(y);
		float xi = (float)(x - i);
		float yi = (float)(y - j);

		float t = (xi + yi) * G2;
		float x0 = (float)(xi - t);
		float y0 = (float)(yi - t);

		i *= PRIME_X;
		j *= PRIME_Y;

		float n0, n1, n2;

		float a = 0.5f - x0 * x0 - y0 * y0;
		if (a <= 0) n0 = 0;
		else {
			n0 = (a * a) * (a * a) * GradCoord(seed, i, j, x0, y0);
		}

		float c = (float)(2 * (1 - 2 * G2) * (1 / G2 - 2)) * t + ((float)(-2 * (1 - 2 * G2) * (1 - 2 * G2)) + a);
		if (c <= 0) n2 = 0;
		else {
			float x2 = x0 + (2 * (float)G2 - 1);
			float y2 = y0 + (2 * (float)G2 - 1);
			n2 = (c * c) * (c * c) * GradCoord(seed, i + PRIME_X, j + PRIME_Y, x2, y2);
		}

		if (y0 > x0) {
			float x1 = x0 + (float)G2;
			float y1 = y0 + ((float)G2 - 1);
			float b = 0.5f - x1 * x1 - y1 * y1;
			if (b <= 0) n1 = 0;
			else {
				n1 = (b * b) * (b * b) * GradCoord(seed, i, j + PRIME_Y, x1, y1);
			}
		} else {
			float x1 = x0 + ((float)G2 - 1);
			float y1 = y0 + (float)G2;
			float b = 0.5f - x1 * x1 - y1 * y1;
			if (b <= 0) n1 = 0;
			else {
				n1 = (b * b) * (b * b) * GradCoord(seed, i + PRIME_X, j, x1, y1);
			}
		}

		return (n0 + n1 + n2) * 99.83685446303647f;
	}

	private float SingleOpenSimplex2 (int seed, float x, float y, float z) {
		// 3D OpenSimplex2 case uses two offset rotated cube grids.

		/*
		 * --- Rotation moved to TransformNoiseCoordinate method ---
		 * const FNfloat R3 = (FNfloat)(2.0 / 3.0);
		 * FNfloat r = (x + y + z) * R3; // Rotation, not skew
		 * x = r - x; y = r - y; z = r - z;
		*/

		int i = FastRound(x);
		int j = FastRound(y);
		int k = FastRound(z);
		float x0 = (float)(x - i);
		float y0 = (float)(y - j);
		float z0 = (float)(z - k);

		int xNSign = (int)(-1.0f - x0) | 1;
		int yNSign = (int)(-1.0f - y0) | 1;
		int zNSign = (int)(-1.0f - z0) | 1;

		float ax0 = xNSign * -x0;
		float ay0 = yNSign * -y0;
		float az0 = zNSign * -z0;

		i *= PRIME_X;
		j *= PRIME_Y;
		k *= PRIME_Z;

		float value = 0;
		float a = (0.6f - x0 * x0) - (y0 * y0 + z0 * z0);

		for (int l = 0; ; l++) {
			if (a > 0) {
				value += (a * a) * (a * a) * GradCoord(seed, i, j, k, x0, y0, z0);
			}

			if (ax0 >= ay0 && ax0 >= az0) {
				float b = a + ax0 + ax0;
				if (b > 1) {
					b -= 1;
					value += (b * b) * (b * b) * GradCoord(seed, i - xNSign * PRIME_X, j, k, x0 + xNSign, y0, z0);
				}
			} else if (ay0 > ax0 && ay0 >= az0) {
				float b = a + ay0 + ay0;
				if (b > 1) {
					b -= 1;
					value += (b * b) * (b * b) * GradCoord(seed, i, j - yNSign * PRIME_Y, k, x0, y0 + yNSign, z0);
				}
			} else {
				float b = a + az0 + az0;
				if (b > 1) {
					b -= 1;
					value += (b * b) * (b * b) * GradCoord(seed, i, j, k - zNSign * PRIME_Z, x0, y0, z0 + zNSign);
				}
			}

			if (l == 1) break;

			ax0 = 0.5f - ax0;
			ay0 = 0.5f - ay0;
			az0 = 0.5f - az0;

			x0 = xNSign * ax0;
			y0 = yNSign * ay0;
			z0 = zNSign * az0;

			a += (0.75f - ax0) - (ay0 + az0);

			i += (xNSign >> 1) & PRIME_X;
			j += (yNSign >> 1) & PRIME_Y;
			k += (zNSign >> 1) & PRIME_Z;

			xNSign = -xNSign;
			yNSign = -yNSign;
			zNSign = -zNSign;

			seed = ~seed;
		}

		return value * 32.69428253173828125f;
	}


	// OpenSimplex2S Noise

	private float SingleOpenSimplex2S (int seed, float x, float y) {
		// 2D OpenSimplex2S case is a modified 2D simplex noise.

		const float SQRT3 = (float)1.7320508075688772935274463415059;
		const float G2 = (3 - SQRT3) / 6;

		/*
		 * --- Skew moved to TransformNoiseCoordinate method ---
		 * const FNfloat F2 = 0.5f * (SQRT3 - 1);
		 * FNfloat s = (x + y) * F2;
		 * x += s; y += s;
		*/

		int i = FastFloor(x);
		int j = FastFloor(y);
		float xi = (float)(x - i);
		float yi = (float)(y - j);

		i *= PRIME_X;
		j *= PRIME_Y;
		int i1 = i + PRIME_X;
		int j1 = j + PRIME_Y;

		float t = (xi + yi) * (float)G2;
		float x0 = xi - t;
		float y0 = yi - t;

		float a0 = (2.0f / 3.0f) - x0 * x0 - y0 * y0;
		float value = (a0 * a0) * (a0 * a0) * GradCoord(seed, i, j, x0, y0);

		float a1 = (float)(2 * (1 - 2 * G2) * (1 / G2 - 2)) * t + ((float)(-2 * (1 - 2 * G2) * (1 - 2 * G2)) + a0);
		float x1 = x0 - (float)(1 - 2 * G2);
		float y1 = y0 - (float)(1 - 2 * G2);
		value += (a1 * a1) * (a1 * a1) * GradCoord(seed, i1, j1, x1, y1);

		// Nested conditionals were faster than compact bit logic/arithmetic.
		float xmyi = xi - yi;
		if (t > G2) {
			if (xi + xmyi > 1) {
				float x2 = x0 + (float)(3 * G2 - 2);
				float y2 = y0 + (float)(3 * G2 - 1);
				float a2 = (2.0f / 3.0f) - x2 * x2 - y2 * y2;
				if (a2 > 0) {
					value += (a2 * a2) * (a2 * a2) * GradCoord(seed, i + (PRIME_X << 1), j + PRIME_Y, x2, y2);
				}
			} else {
				float x2 = x0 + (float)G2;
				float y2 = y0 + (float)(G2 - 1);
				float a2 = (2.0f / 3.0f) - x2 * x2 - y2 * y2;
				if (a2 > 0) {
					value += (a2 * a2) * (a2 * a2) * GradCoord(seed, i, j + PRIME_Y, x2, y2);
				}
			}

			if (yi - xmyi > 1) {
				float x3 = x0 + (float)(3 * G2 - 1);
				float y3 = y0 + (float)(3 * G2 - 2);
				float a3 = (2.0f / 3.0f) - x3 * x3 - y3 * y3;
				if (a3 > 0) {
					value += (a3 * a3) * (a3 * a3) * GradCoord(seed, i + PRIME_X, j + (PRIME_Y << 1), x3, y3);
				}
			} else {
				float x3 = x0 + (float)(G2 - 1);
				float y3 = y0 + (float)G2;
				float a3 = (2.0f / 3.0f) - x3 * x3 - y3 * y3;
				if (a3 > 0) {
					value += (a3 * a3) * (a3 * a3) * GradCoord(seed, i + PRIME_X, j, x3, y3);
				}
			}
		} else {
			if (xi + xmyi < 0) {
				float x2 = x0 + (float)(1 - G2);
				float y2 = y0 - (float)G2;
				float a2 = (2.0f / 3.0f) - x2 * x2 - y2 * y2;
				if (a2 > 0) {
					value += (a2 * a2) * (a2 * a2) * GradCoord(seed, i - PRIME_X, j, x2, y2);
				}
			} else {
				float x2 = x0 + (float)(G2 - 1);
				float y2 = y0 + (float)G2;
				float a2 = (2.0f / 3.0f) - x2 * x2 - y2 * y2;
				if (a2 > 0) {
					value += (a2 * a2) * (a2 * a2) * GradCoord(seed, i + PRIME_X, j, x2, y2);
				}
			}

			if (yi < xmyi) {
				float x2 = x0 - (float)G2;
				float y2 = y0 - (float)(G2 - 1);
				float a2 = (2.0f / 3.0f) - x2 * x2 - y2 * y2;
				if (a2 > 0) {
					value += (a2 * a2) * (a2 * a2) * GradCoord(seed, i, j - PRIME_Y, x2, y2);
				}
			} else {
				float x2 = x0 + (float)G2;
				float y2 = y0 + (float)(G2 - 1);
				float a2 = (2.0f / 3.0f) - x2 * x2 - y2 * y2;
				if (a2 > 0) {
					value += (a2 * a2) * (a2 * a2) * GradCoord(seed, i, j + PRIME_Y, x2, y2);
				}
			}
		}

		return value * 18.24196194486065f;
	}

	private float SingleOpenSimplex2S (int seed, float x, float y, float z) {
		// 3D OpenSimplex2S case uses two offset rotated cube grids.

		/*
		 * --- Rotation moved to TransformNoiseCoordinate method ---
		 * const FNfloat R3 = (FNfloat)(2.0 / 3.0);
		 * FNfloat r = (x + y + z) * R3; // Rotation, not skew
		 * x = r - x; y = r - y; z = r - z;
		*/

		int i = FastFloor(x);
		int j = FastFloor(y);
		int k = FastFloor(z);
		float xi = (float)(x - i);
		float yi = (float)(y - j);
		float zi = (float)(z - k);

		i *= PRIME_X;
		j *= PRIME_Y;
		k *= PRIME_Z;
		int seed2 = seed + 1293373;

		int xNMask = (int)(-0.5f - xi);
		int yNMask = (int)(-0.5f - yi);
		int zNMask = (int)(-0.5f - zi);

		float x0 = xi + xNMask;
		float y0 = yi + yNMask;
		float z0 = zi + zNMask;
		float a0 = 0.75f - x0 * x0 - y0 * y0 - z0 * z0;
		float value = (a0 * a0) * (a0 * a0) * GradCoord(seed,
			i + (xNMask & PRIME_X), j + (yNMask & PRIME_Y), k + (zNMask & PRIME_Z), x0, y0, z0);

		float x1 = xi - 0.5f;
		float y1 = yi - 0.5f;
		float z1 = zi - 0.5f;
		float a1 = 0.75f - x1 * x1 - y1 * y1 - z1 * z1;
		value += (a1 * a1) * (a1 * a1) * GradCoord(seed2,
			i + PRIME_X, j + PRIME_Y, k + PRIME_Z, x1, y1, z1);

		float xAFlipMask0 = ((xNMask | 1) << 1) * x1;
		float yAFlipMask0 = ((yNMask | 1) << 1) * y1;
		float zAFlipMask0 = ((zNMask | 1) << 1) * z1;
		float xAFlipMask1 = (-2 - (xNMask << 2)) * x1 - 1.0f;
		float yAFlipMask1 = (-2 - (yNMask << 2)) * y1 - 1.0f;
		float zAFlipMask1 = (-2 - (zNMask << 2)) * z1 - 1.0f;

		bool skip5 = false;
		float a2 = xAFlipMask0 + a0;
		if (a2 > 0) {
			float x2 = x0 - (xNMask | 1);
			float y2 = y0;
			float z2 = z0;
			value += (a2 * a2) * (a2 * a2) * GradCoord(seed,
				i + (~xNMask & PRIME_X), j + (yNMask & PRIME_Y), k + (zNMask & PRIME_Z), x2, y2, z2);
		} else {
			float a3 = yAFlipMask0 + zAFlipMask0 + a0;
			if (a3 > 0) {
				float x3 = x0;
				float y3 = y0 - (yNMask | 1);
				float z3 = z0 - (zNMask | 1);
				value += (a3 * a3) * (a3 * a3) * GradCoord(seed,
					i + (xNMask & PRIME_X), j + (~yNMask & PRIME_Y), k + (~zNMask & PRIME_Z), x3, y3, z3);
			}

			float a4 = xAFlipMask1 + a1;
			if (a4 > 0) {
				float x4 = (xNMask | 1) + x1;
				float y4 = y1;
				float z4 = z1;
				value += (a4 * a4) * (a4 * a4) * GradCoord(seed2,
					i + (xNMask & (PRIME_X * 2)), j + PRIME_Y, k + PRIME_Z, x4, y4, z4);
				skip5 = true;
			}
		}

		bool skip9 = false;
		float a6 = yAFlipMask0 + a0;
		if (a6 > 0) {
			float x6 = x0;
			float y6 = y0 - (yNMask | 1);
			float z6 = z0;
			value += (a6 * a6) * (a6 * a6) * GradCoord(seed,
				i + (xNMask & PRIME_X), j + (~yNMask & PRIME_Y), k + (zNMask & PRIME_Z), x6, y6, z6);
		} else {
			float a7 = xAFlipMask0 + zAFlipMask0 + a0;
			if (a7 > 0) {
				float x7 = x0 - (xNMask | 1);
				float y7 = y0;
				float z7 = z0 - (zNMask | 1);
				value += (a7 * a7) * (a7 * a7) * GradCoord(seed,
					i + (~xNMask & PRIME_X), j + (yNMask & PRIME_Y), k + (~zNMask & PRIME_Z), x7, y7, z7);
			}

			float a8 = yAFlipMask1 + a1;
			if (a8 > 0) {
				float x8 = x1;
				float y8 = (yNMask | 1) + y1;
				float z8 = z1;
				value += (a8 * a8) * (a8 * a8) * GradCoord(seed2,
					i + PRIME_X, j + (yNMask & (PRIME_Y << 1)), k + PRIME_Z, x8, y8, z8);
				skip9 = true;
			}
		}

		bool skipD = false;
		float aA = zAFlipMask0 + a0;
		if (aA > 0) {
			float xA = x0;
			float yA = y0;
			float zA = z0 - (zNMask | 1);
			value += (aA * aA) * (aA * aA) * GradCoord(seed,
				i + (xNMask & PRIME_X), j + (yNMask & PRIME_Y), k + (~zNMask & PRIME_Z), xA, yA, zA);
		} else {
			float aB = xAFlipMask0 + yAFlipMask0 + a0;
			if (aB > 0) {
				float xB = x0 - (xNMask | 1);
				float yB = y0 - (yNMask | 1);
				float zB = z0;
				value += (aB * aB) * (aB * aB) * GradCoord(seed,
					i + (~xNMask & PRIME_X), j + (~yNMask & PRIME_Y), k + (zNMask & PRIME_Z), xB, yB, zB);
			}

			float aC = zAFlipMask1 + a1;
			if (aC > 0) {
				float xC = x1;
				float yC = y1;
				float zC = (zNMask | 1) + z1;
				value += (aC * aC) * (aC * aC) * GradCoord(seed2,
					i + PRIME_X, j + PRIME_Y, k + (zNMask & (PRIME_Z << 1)), xC, yC, zC);
				skipD = true;
			}
		}

		if (!skip5) {
			float a5 = yAFlipMask1 + zAFlipMask1 + a1;
			if (a5 > 0) {
				float x5 = x1;
				float y5 = (yNMask | 1) + y1;
				float z5 = (zNMask | 1) + z1;
				value += (a5 * a5) * (a5 * a5) * GradCoord(seed2,
					i + PRIME_X, j + (yNMask & (PRIME_Y << 1)), k + (zNMask & (PRIME_Z << 1)), x5, y5, z5);
			}
		}

		if (!skip9) {
			float a9 = xAFlipMask1 + zAFlipMask1 + a1;
			if (a9 > 0) {
				float x9 = (xNMask | 1) + x1;
				float y9 = y1;
				float z9 = (zNMask | 1) + z1;
				value += (a9 * a9) * (a9 * a9) * GradCoord(seed2,
					i + (xNMask & (PRIME_X * 2)), j + PRIME_Y, k + (zNMask & (PRIME_Z << 1)), x9, y9, z9);
			}
		}

		if (!skipD) {
			float aD = xAFlipMask1 + yAFlipMask1 + a1;
			if (aD > 0) {
				float xD = (xNMask | 1) + x1;
				float yD = (yNMask | 1) + y1;
				float zD = z1;
				value += (aD * aD) * (aD * aD) * GradCoord(seed2,
					i + (xNMask & (PRIME_X << 1)), j + (yNMask & (PRIME_Y << 1)), k + PRIME_Z, xD, yD, zD);
			}
		}

		return value * 9.046026385208288f;
	}


	// Cellular Noise

	private float SingleCellular (int seed, float x, float y) {
		int xr = FastRound(x);
		int yr = FastRound(y);

		float distance0 = float.MaxValue;
		float distance1 = float.MaxValue;
		int closestHash = 0;

		float cellularJitter = 0.43701595f * mCellularJitterModifier;

		int xPrimed = (xr - 1) * PRIME_X;
		int yPrimedBase = (yr - 1) * PRIME_Y;

		switch (mCellularDistanceFunction) {
			default:
			case CellularDistanceFunction.Euclidean:
			case CellularDistanceFunction.EuclideanSq:
				for (int xi = xr - 1; xi <= xr + 1; xi++) {
					int yPrimed = yPrimedBase;

					for (int yi = yr - 1; yi <= yr + 1; yi++) {
						int hash = Hash(seed, xPrimed, yPrimed);
						int idx = hash & (255 << 1);

						float vecX = (float)(xi - x) + RandVecs2D[idx] * cellularJitter;
						float vecY = (float)(yi - y) + RandVecs2D[idx | 1] * cellularJitter;

						float newDistance = vecX * vecX + vecY * vecY;

						distance1 = FastMax(FastMin(distance1, newDistance), distance0);
						if (newDistance < distance0) {
							distance0 = newDistance;
							closestHash = hash;
						}
						yPrimed += PRIME_Y;
					}
					xPrimed += PRIME_X;
				}
				break;
			case CellularDistanceFunction.Manhattan:
				for (int xi = xr - 1; xi <= xr + 1; xi++) {
					int yPrimed = yPrimedBase;

					for (int yi = yr - 1; yi <= yr + 1; yi++) {
						int hash = Hash(seed, xPrimed, yPrimed);
						int idx = hash & (255 << 1);

						float vecX = (float)(xi - x) + RandVecs2D[idx] * cellularJitter;
						float vecY = (float)(yi - y) + RandVecs2D[idx | 1] * cellularJitter;

						float newDistance = FastAbs(vecX) + FastAbs(vecY);

						distance1 = FastMax(FastMin(distance1, newDistance), distance0);
						if (newDistance < distance0) {
							distance0 = newDistance;
							closestHash = hash;
						}
						yPrimed += PRIME_Y;
					}
					xPrimed += PRIME_X;
				}
				break;
			case CellularDistanceFunction.Hybrid:
				for (int xi = xr - 1; xi <= xr + 1; xi++) {
					int yPrimed = yPrimedBase;

					for (int yi = yr - 1; yi <= yr + 1; yi++) {
						int hash = Hash(seed, xPrimed, yPrimed);
						int idx = hash & (255 << 1);

						float vecX = (float)(xi - x) + RandVecs2D[idx] * cellularJitter;
						float vecY = (float)(yi - y) + RandVecs2D[idx | 1] * cellularJitter;

						float newDistance = (FastAbs(vecX) + FastAbs(vecY)) + (vecX * vecX + vecY * vecY);

						distance1 = FastMax(FastMin(distance1, newDistance), distance0);
						if (newDistance < distance0) {
							distance0 = newDistance;
							closestHash = hash;
						}
						yPrimed += PRIME_Y;
					}
					xPrimed += PRIME_X;
				}
				break;
		}

		if (mCellularDistanceFunction == CellularDistanceFunction.Euclidean && mCellularReturnType >= CellularReturnType.Distance) {
			distance0 = FastSqrt(distance0);

			if (mCellularReturnType >= CellularReturnType.Distance2) {
				distance1 = FastSqrt(distance1);
			}
		}

		return mCellularReturnType switch {
			CellularReturnType.CellValue => closestHash * (1 / 2147483648.0f),
			CellularReturnType.Distance => distance0 - 1,
			CellularReturnType.Distance2 => distance1 - 1,
			CellularReturnType.Distance2Add => (distance1 + distance0) * 0.5f - 1,
			CellularReturnType.Distance2Sub => distance1 - distance0 - 1,
			CellularReturnType.Distance2Mul => distance1 * distance0 * 0.5f - 1,
			CellularReturnType.Distance2Div => distance0 / distance1 - 1,
			_ => 0,
		};
	}

	private float SingleCellular (int seed, float x, float y, float z) {
		int xr = FastRound(x);
		int yr = FastRound(y);
		int zr = FastRound(z);

		float distance0 = float.MaxValue;
		float distance1 = float.MaxValue;
		int closestHash = 0;

		float cellularJitter = 0.39614353f * mCellularJitterModifier;

		int xPrimed = (xr - 1) * PRIME_X;
		int yPrimedBase = (yr - 1) * PRIME_Y;
		int zPrimedBase = (zr - 1) * PRIME_Z;

		switch (mCellularDistanceFunction) {
			case CellularDistanceFunction.Euclidean:
			case CellularDistanceFunction.EuclideanSq:
				for (int xi = xr - 1; xi <= xr + 1; xi++) {
					int yPrimed = yPrimedBase;

					for (int yi = yr - 1; yi <= yr + 1; yi++) {
						int zPrimed = zPrimedBase;

						for (int zi = zr - 1; zi <= zr + 1; zi++) {
							int hash = Hash(seed, xPrimed, yPrimed, zPrimed);
							int idx = hash & (255 << 2);

							float vecX = (float)(xi - x) + RandVecs3D[idx] * cellularJitter;
							float vecY = (float)(yi - y) + RandVecs3D[idx | 1] * cellularJitter;
							float vecZ = (float)(zi - z) + RandVecs3D[idx | 2] * cellularJitter;

							float newDistance = vecX * vecX + vecY * vecY + vecZ * vecZ;

							distance1 = FastMax(FastMin(distance1, newDistance), distance0);
							if (newDistance < distance0) {
								distance0 = newDistance;
								closestHash = hash;
							}
							zPrimed += PRIME_Z;
						}
						yPrimed += PRIME_Y;
					}
					xPrimed += PRIME_X;
				}
				break;
			case CellularDistanceFunction.Manhattan:
				for (int xi = xr - 1; xi <= xr + 1; xi++) {
					int yPrimed = yPrimedBase;

					for (int yi = yr - 1; yi <= yr + 1; yi++) {
						int zPrimed = zPrimedBase;

						for (int zi = zr - 1; zi <= zr + 1; zi++) {
							int hash = Hash(seed, xPrimed, yPrimed, zPrimed);
							int idx = hash & (255 << 2);

							float vecX = (float)(xi - x) + RandVecs3D[idx] * cellularJitter;
							float vecY = (float)(yi - y) + RandVecs3D[idx | 1] * cellularJitter;
							float vecZ = (float)(zi - z) + RandVecs3D[idx | 2] * cellularJitter;

							float newDistance = FastAbs(vecX) + FastAbs(vecY) + FastAbs(vecZ);

							distance1 = FastMax(FastMin(distance1, newDistance), distance0);
							if (newDistance < distance0) {
								distance0 = newDistance;
								closestHash = hash;
							}
							zPrimed += PRIME_Z;
						}
						yPrimed += PRIME_Y;
					}
					xPrimed += PRIME_X;
				}
				break;
			case CellularDistanceFunction.Hybrid:
				for (int xi = xr - 1; xi <= xr + 1; xi++) {
					int yPrimed = yPrimedBase;

					for (int yi = yr - 1; yi <= yr + 1; yi++) {
						int zPrimed = zPrimedBase;

						for (int zi = zr - 1; zi <= zr + 1; zi++) {
							int hash = Hash(seed, xPrimed, yPrimed, zPrimed);
							int idx = hash & (255 << 2);

							float vecX = (float)(xi - x) + RandVecs3D[idx] * cellularJitter;
							float vecY = (float)(yi - y) + RandVecs3D[idx | 1] * cellularJitter;
							float vecZ = (float)(zi - z) + RandVecs3D[idx | 2] * cellularJitter;

							float newDistance = (FastAbs(vecX) + FastAbs(vecY) + FastAbs(vecZ)) + (vecX * vecX + vecY * vecY + vecZ * vecZ);

							distance1 = FastMax(FastMin(distance1, newDistance), distance0);
							if (newDistance < distance0) {
								distance0 = newDistance;
								closestHash = hash;
							}
							zPrimed += PRIME_Z;
						}
						yPrimed += PRIME_Y;
					}
					xPrimed += PRIME_X;
				}
				break;
			default:
				break;
		}

		if (mCellularDistanceFunction == CellularDistanceFunction.Euclidean && mCellularReturnType >= CellularReturnType.Distance) {
			distance0 = FastSqrt(distance0);

			if (mCellularReturnType >= CellularReturnType.Distance2) {
				distance1 = FastSqrt(distance1);
			}
		}

		return mCellularReturnType switch {
			CellularReturnType.CellValue => closestHash * (1 / 2147483648.0f),
			CellularReturnType.Distance => distance0 - 1,
			CellularReturnType.Distance2 => distance1 - 1,
			CellularReturnType.Distance2Add => (distance1 + distance0) * 0.5f - 1,
			CellularReturnType.Distance2Sub => distance1 - distance0 - 1,
			CellularReturnType.Distance2Mul => distance1 * distance0 * 0.5f - 1,
			CellularReturnType.Distance2Div => distance0 / distance1 - 1,
			_ => 0,
		};
	}


	// Perlin Noise

	private float SinglePerlin (int seed, float x, float y) {
		int x0 = FastFloor(x);
		int y0 = FastFloor(y);

		float xd0 = (float)(x - x0);
		float yd0 = (float)(y - y0);
		float xd1 = xd0 - 1;
		float yd1 = yd0 - 1;

		float xs = InterpQuintic(xd0);
		float ys = InterpQuintic(yd0);

		x0 *= PRIME_X;
		y0 *= PRIME_Y;
		int x1 = x0 + PRIME_X;
		int y1 = y0 + PRIME_Y;

		float xf0 = Lerp(GradCoord(seed, x0, y0, xd0, yd0), GradCoord(seed, x1, y0, xd1, yd0), xs);
		float xf1 = Lerp(GradCoord(seed, x0, y1, xd0, yd1), GradCoord(seed, x1, y1, xd1, yd1), xs);

		return Lerp(xf0, xf1, ys) * 1.4247691104677813f;
	}

	private float SinglePerlin (int seed, float x, float y, float z) {
		int x0 = FastFloor(x);
		int y0 = FastFloor(y);
		int z0 = FastFloor(z);

		float xd0 = (float)(x - x0);
		float yd0 = (float)(y - y0);
		float zd0 = (float)(z - z0);
		float xd1 = xd0 - 1;
		float yd1 = yd0 - 1;
		float zd1 = zd0 - 1;

		float xs = InterpQuintic(xd0);
		float ys = InterpQuintic(yd0);
		float zs = InterpQuintic(zd0);

		x0 *= PRIME_X;
		y0 *= PRIME_Y;
		z0 *= PRIME_Z;
		int x1 = x0 + PRIME_X;
		int y1 = y0 + PRIME_Y;
		int z1 = z0 + PRIME_Z;

		float xf00 = Lerp(GradCoord(seed, x0, y0, z0, xd0, yd0, zd0), GradCoord(seed, x1, y0, z0, xd1, yd0, zd0), xs);
		float xf10 = Lerp(GradCoord(seed, x0, y1, z0, xd0, yd1, zd0), GradCoord(seed, x1, y1, z0, xd1, yd1, zd0), xs);
		float xf01 = Lerp(GradCoord(seed, x0, y0, z1, xd0, yd0, zd1), GradCoord(seed, x1, y0, z1, xd1, yd0, zd1), xs);
		float xf11 = Lerp(GradCoord(seed, x0, y1, z1, xd0, yd1, zd1), GradCoord(seed, x1, y1, z1, xd1, yd1, zd1), xs);

		float yf0 = Lerp(xf00, xf10, ys);
		float yf1 = Lerp(xf01, xf11, ys);

		return Lerp(yf0, yf1, zs) * 0.964921414852142333984375f;
	}


	// Value Cubic Noise

	private float SingleValueCubic (int seed, float x, float y) {
		int x1 = FastFloor(x);
		int y1 = FastFloor(y);

		float xs = (float)(x - x1);
		float ys = (float)(y - y1);

		x1 *= PRIME_X;
		y1 *= PRIME_Y;
		int x0 = x1 - PRIME_X;
		int y0 = y1 - PRIME_Y;
		int x2 = x1 + PRIME_X;
		int y2 = y1 + PRIME_Y;
		int x3 = x1 + unchecked(PRIME_X * 2);
		int y3 = y1 + unchecked(PRIME_Y * 2);

		return CubicLerp(
			CubicLerp(ValCoord(seed, x0, y0), ValCoord(seed, x1, y0), ValCoord(seed, x2, y0), ValCoord(seed, x3, y0),
			xs),
			CubicLerp(ValCoord(seed, x0, y1), ValCoord(seed, x1, y1), ValCoord(seed, x2, y1), ValCoord(seed, x3, y1),
			xs),
			CubicLerp(ValCoord(seed, x0, y2), ValCoord(seed, x1, y2), ValCoord(seed, x2, y2), ValCoord(seed, x3, y2),
			xs),
			CubicLerp(ValCoord(seed, x0, y3), ValCoord(seed, x1, y3), ValCoord(seed, x2, y3), ValCoord(seed, x3, y3),
			xs),
			ys) * (1 / (1.5f * 1.5f));
	}

	private float SingleValueCubic (int seed, float x, float y, float z) {
		int x1 = FastFloor(x);
		int y1 = FastFloor(y);
		int z1 = FastFloor(z);

		float xs = (float)(x - x1);
		float ys = (float)(y - y1);
		float zs = (float)(z - z1);

		x1 *= PRIME_X;
		y1 *= PRIME_Y;
		z1 *= PRIME_Z;

		int x0 = x1 - PRIME_X;
		int y0 = y1 - PRIME_Y;
		int z0 = z1 - PRIME_Z;
		int x2 = x1 + PRIME_X;
		int y2 = y1 + PRIME_Y;
		int z2 = z1 + PRIME_Z;
		int x3 = x1 + unchecked(PRIME_X * 2);
		int y3 = y1 + unchecked(PRIME_Y * 2);
		int z3 = z1 + unchecked(PRIME_Z * 2);


		return CubicLerp(
			CubicLerp(
			CubicLerp(ValCoord(seed, x0, y0, z0), ValCoord(seed, x1, y0, z0), ValCoord(seed, x2, y0, z0), ValCoord(seed, x3, y0, z0), xs),
			CubicLerp(ValCoord(seed, x0, y1, z0), ValCoord(seed, x1, y1, z0), ValCoord(seed, x2, y1, z0), ValCoord(seed, x3, y1, z0), xs),
			CubicLerp(ValCoord(seed, x0, y2, z0), ValCoord(seed, x1, y2, z0), ValCoord(seed, x2, y2, z0), ValCoord(seed, x3, y2, z0), xs),
			CubicLerp(ValCoord(seed, x0, y3, z0), ValCoord(seed, x1, y3, z0), ValCoord(seed, x2, y3, z0), ValCoord(seed, x3, y3, z0), xs),
			ys),
			CubicLerp(
			CubicLerp(ValCoord(seed, x0, y0, z1), ValCoord(seed, x1, y0, z1), ValCoord(seed, x2, y0, z1), ValCoord(seed, x3, y0, z1), xs),
			CubicLerp(ValCoord(seed, x0, y1, z1), ValCoord(seed, x1, y1, z1), ValCoord(seed, x2, y1, z1), ValCoord(seed, x3, y1, z1), xs),
			CubicLerp(ValCoord(seed, x0, y2, z1), ValCoord(seed, x1, y2, z1), ValCoord(seed, x2, y2, z1), ValCoord(seed, x3, y2, z1), xs),
			CubicLerp(ValCoord(seed, x0, y3, z1), ValCoord(seed, x1, y3, z1), ValCoord(seed, x2, y3, z1), ValCoord(seed, x3, y3, z1), xs),
			ys),
			CubicLerp(
			CubicLerp(ValCoord(seed, x0, y0, z2), ValCoord(seed, x1, y0, z2), ValCoord(seed, x2, y0, z2), ValCoord(seed, x3, y0, z2), xs),
			CubicLerp(ValCoord(seed, x0, y1, z2), ValCoord(seed, x1, y1, z2), ValCoord(seed, x2, y1, z2), ValCoord(seed, x3, y1, z2), xs),
			CubicLerp(ValCoord(seed, x0, y2, z2), ValCoord(seed, x1, y2, z2), ValCoord(seed, x2, y2, z2), ValCoord(seed, x3, y2, z2), xs),
			CubicLerp(ValCoord(seed, x0, y3, z2), ValCoord(seed, x1, y3, z2), ValCoord(seed, x2, y3, z2), ValCoord(seed, x3, y3, z2), xs),
			ys),
			CubicLerp(
			CubicLerp(ValCoord(seed, x0, y0, z3), ValCoord(seed, x1, y0, z3), ValCoord(seed, x2, y0, z3), ValCoord(seed, x3, y0, z3), xs),
			CubicLerp(ValCoord(seed, x0, y1, z3), ValCoord(seed, x1, y1, z3), ValCoord(seed, x2, y1, z3), ValCoord(seed, x3, y1, z3), xs),
			CubicLerp(ValCoord(seed, x0, y2, z3), ValCoord(seed, x1, y2, z3), ValCoord(seed, x2, y2, z3), ValCoord(seed, x3, y2, z3), xs),
			CubicLerp(ValCoord(seed, x0, y3, z3), ValCoord(seed, x1, y3, z3), ValCoord(seed, x2, y3, z3), ValCoord(seed, x3, y3, z3), xs),
			ys),
			zs) * (1 / (1.5f * 1.5f * 1.5f));
	}


	// Value Noise
	private float SingleValue (int seed, float x, float y) {
		int x0 = FastFloor(x);
		int y0 = FastFloor(y);

		float xs = InterpHermite((float)(x - x0));
		float ys = InterpHermite((float)(y - y0));

		x0 *= PRIME_X;
		y0 *= PRIME_Y;
		int x1 = x0 + PRIME_X;
		int y1 = y0 + PRIME_Y;

		float xf0 = Lerp(ValCoord(seed, x0, y0), ValCoord(seed, x1, y0), xs);
		float xf1 = Lerp(ValCoord(seed, x0, y1), ValCoord(seed, x1, y1), xs);

		return Lerp(xf0, xf1, ys);
	}

	private float SingleValue (int seed, float x, float y, float z) {
		int x0 = FastFloor(x);
		int y0 = FastFloor(y);
		int z0 = FastFloor(z);

		float xs = InterpHermite((float)(x - x0));
		float ys = InterpHermite((float)(y - y0));
		float zs = InterpHermite((float)(z - z0));

		x0 *= PRIME_X;
		y0 *= PRIME_Y;
		z0 *= PRIME_Z;
		int x1 = x0 + PRIME_X;
		int y1 = y0 + PRIME_Y;
		int z1 = z0 + PRIME_Z;

		float xf00 = Lerp(ValCoord(seed, x0, y0, z0), ValCoord(seed, x1, y0, z0), xs);
		float xf10 = Lerp(ValCoord(seed, x0, y1, z0), ValCoord(seed, x1, y1, z0), xs);
		float xf01 = Lerp(ValCoord(seed, x0, y0, z1), ValCoord(seed, x1, y0, z1), xs);
		float xf11 = Lerp(ValCoord(seed, x0, y1, z1), ValCoord(seed, x1, y1, z1), xs);

		float yf0 = Lerp(xf00, xf10, ys);
		float yf1 = Lerp(xf01, xf11, ys);

		return Lerp(yf0, yf1, zs);
	}


	// Domain Warp
	private void DoSingleDomainWarp (int seed, float amp, float freq, float x, float y, ref float xr, ref float yr) {
		switch (mDomainWarpType) {
			case DomainWarpType.OpenSimplex2:
				SingleDomainWarpSimplexGradient(seed, amp * 38.283687591552734375f, freq, x, y, ref xr, ref yr, false);
				break;
			case DomainWarpType.OpenSimplex2Reduced:
				SingleDomainWarpSimplexGradient(seed, amp * 16.0f, freq, x, y, ref xr, ref yr, true);
				break;
			case DomainWarpType.BasicGrid:
				SingleDomainWarpBasicGrid(seed, amp, freq, x, y, ref xr, ref yr);
				break;
		}
	}

	private void DoSingleDomainWarp (int seed, float amp, float freq, float x, float y, float z, ref float xr, ref float yr, ref float zr) {
		switch (mDomainWarpType) {
			case DomainWarpType.OpenSimplex2:
				SingleDomainWarpOpenSimplex2Gradient(seed, amp * 32.69428253173828125f, freq, x, y, z, ref xr, ref yr, ref zr, false);
				break;
			case DomainWarpType.OpenSimplex2Reduced:
				SingleDomainWarpOpenSimplex2Gradient(seed, amp * 7.71604938271605f, freq, x, y, z, ref xr, ref yr, ref zr, true);
				break;
			case DomainWarpType.BasicGrid:
				SingleDomainWarpBasicGrid(seed, amp, freq, x, y, z, ref xr, ref yr, ref zr);
				break;
		}
	}


	// Domain Warp Single Wrapper
	private void DomainWarpSingle (ref float x, ref float y) {
		int seed = mSeed;
		float amp = mDomainWarpAmp / mFrequency / 100f;
		float freq = mFrequency;

		float xs = x;
		float ys = y;
		TransformDomainWarpCoordinate(ref xs, ref ys);

		DoSingleDomainWarp(seed, amp, freq, xs, ys, ref x, ref y);
	}

	private void DomainWarpSingle (ref float x, ref float y, ref float z) {
		int seed = mSeed;
		float amp = mDomainWarpAmp / mFrequency / 100f;
		float freq = mFrequency;

		float xs = x;
		float ys = y;
		float zs = z;
		TransformDomainWarpCoordinate(ref xs, ref ys, ref zs);

		DoSingleDomainWarp(seed, amp, freq, xs, ys, zs, ref x, ref y, ref z);
	}


	// Domain Warp Fractal Progressive
	private void DomainWarpFractalProgressive (ref float x, ref float y) {
		int seed = mSeed;
		float amp = mDomainWarpAmp / mFrequency / 100f;
		float freq = mFrequency;

		for (int i = 0; i < mOctaves; i++) {
			float xs = x;
			float ys = y;
			TransformDomainWarpCoordinate(ref xs, ref ys);

			DoSingleDomainWarp(seed, amp, freq, xs, ys, ref x, ref y);

			seed++;
			amp *= mGain;
			freq *= mLacunarity;
		}
	}

	private void DomainWarpFractalProgressive (ref float x, ref float y, ref float z) {
		int seed = mSeed;
		float amp = mDomainWarpAmp / mFrequency / 100f;
		float freq = mFrequency;

		for (int i = 0; i < mOctaves; i++) {
			float xs = x;
			float ys = y;
			float zs = z;
			TransformDomainWarpCoordinate(ref xs, ref ys, ref zs);

			DoSingleDomainWarp(seed, amp, freq, xs, ys, zs, ref x, ref y, ref z);

			seed++;
			amp *= mGain;
			freq *= mLacunarity;
		}
	}


	// Domain Warp Fractal Independant
	private void DomainWarpFractalIndependent (ref float x, ref float y) {
		float xs = x;
		float ys = y;
		TransformDomainWarpCoordinate(ref xs, ref ys);

		int seed = mSeed;
		float amp = mDomainWarpAmp / mFrequency / 100f;
		float freq = mFrequency;

		for (int i = 0; i < mOctaves; i++) {
			DoSingleDomainWarp(seed, amp, freq, xs, ys, ref x, ref y);

			seed++;
			amp *= mGain;
			freq *= mLacunarity;
		}
	}

	private void DomainWarpFractalIndependent (ref float x, ref float y, ref float z) {
		float xs = x;
		float ys = y;
		float zs = z;
		TransformDomainWarpCoordinate(ref xs, ref ys, ref zs);

		int seed = mSeed;
		float amp = mDomainWarpAmp / mFrequency / 100f;
		float freq = mFrequency;

		for (int i = 0; i < mOctaves; i++) {
			DoSingleDomainWarp(seed, amp, freq, xs, ys, zs, ref x, ref y, ref z);

			seed++;
			amp *= mGain;
			freq *= mLacunarity;
		}
	}


	// Domain Warp Basic Grid

	private void SingleDomainWarpBasicGrid (int seed, float warpAmp, float frequency, float x, float y, ref float xr, ref float yr) {
		float xf = x * frequency;
		float yf = y * frequency;

		int x0 = FastFloor(xf);
		int y0 = FastFloor(yf);

		float xs = InterpHermite((float)(xf - x0));
		float ys = InterpHermite((float)(yf - y0));

		x0 *= PRIME_X;
		y0 *= PRIME_Y;
		int x1 = x0 + PRIME_X;
		int y1 = y0 + PRIME_Y;

		int hash0 = Hash(seed, x0, y0) & (255 << 1);
		int hash1 = Hash(seed, x1, y0) & (255 << 1);

		float lx0x = Lerp(RandVecs2D[hash0], RandVecs2D[hash1], xs);
		float ly0x = Lerp(RandVecs2D[hash0 | 1], RandVecs2D[hash1 | 1], xs);

		hash0 = Hash(seed, x0, y1) & (255 << 1);
		hash1 = Hash(seed, x1, y1) & (255 << 1);

		float lx1x = Lerp(RandVecs2D[hash0], RandVecs2D[hash1], xs);
		float ly1x = Lerp(RandVecs2D[hash0 | 1], RandVecs2D[hash1 | 1], xs);

		xr += Lerp(lx0x, lx1x, ys) * warpAmp;
		yr += Lerp(ly0x, ly1x, ys) * warpAmp;
	}

	private void SingleDomainWarpBasicGrid (int seed, float warpAmp, float frequency, float x, float y, float z, ref float xr, ref float yr, ref float zr) {
		float xf = x * frequency;
		float yf = y * frequency;
		float zf = z * frequency;

		int x0 = FastFloor(xf);
		int y0 = FastFloor(yf);
		int z0 = FastFloor(zf);

		float xs = InterpHermite((float)(xf - x0));
		float ys = InterpHermite((float)(yf - y0));
		float zs = InterpHermite((float)(zf - z0));

		x0 *= PRIME_X;
		y0 *= PRIME_Y;
		z0 *= PRIME_Z;
		int x1 = x0 + PRIME_X;
		int y1 = y0 + PRIME_Y;
		int z1 = z0 + PRIME_Z;

		int hash0 = Hash(seed, x0, y0, z0) & (255 << 2);
		int hash1 = Hash(seed, x1, y0, z0) & (255 << 2);

		float lx0x = Lerp(RandVecs3D[hash0], RandVecs3D[hash1], xs);
		float ly0x = Lerp(RandVecs3D[hash0 | 1], RandVecs3D[hash1 | 1], xs);
		float lz0x = Lerp(RandVecs3D[hash0 | 2], RandVecs3D[hash1 | 2], xs);

		hash0 = Hash(seed, x0, y1, z0) & (255 << 2);
		hash1 = Hash(seed, x1, y1, z0) & (255 << 2);

		float lx1x = Lerp(RandVecs3D[hash0], RandVecs3D[hash1], xs);
		float ly1x = Lerp(RandVecs3D[hash0 | 1], RandVecs3D[hash1 | 1], xs);
		float lz1x = Lerp(RandVecs3D[hash0 | 2], RandVecs3D[hash1 | 2], xs);

		float lx0y = Lerp(lx0x, lx1x, ys);
		float ly0y = Lerp(ly0x, ly1x, ys);
		float lz0y = Lerp(lz0x, lz1x, ys);

		hash0 = Hash(seed, x0, y0, z1) & (255 << 2);
		hash1 = Hash(seed, x1, y0, z1) & (255 << 2);

		lx0x = Lerp(RandVecs3D[hash0], RandVecs3D[hash1], xs);
		ly0x = Lerp(RandVecs3D[hash0 | 1], RandVecs3D[hash1 | 1], xs);
		lz0x = Lerp(RandVecs3D[hash0 | 2], RandVecs3D[hash1 | 2], xs);

		hash0 = Hash(seed, x0, y1, z1) & (255 << 2);
		hash1 = Hash(seed, x1, y1, z1) & (255 << 2);

		lx1x = Lerp(RandVecs3D[hash0], RandVecs3D[hash1], xs);
		ly1x = Lerp(RandVecs3D[hash0 | 1], RandVecs3D[hash1 | 1], xs);
		lz1x = Lerp(RandVecs3D[hash0 | 2], RandVecs3D[hash1 | 2], xs);

		xr += Lerp(lx0y, Lerp(lx0x, lx1x, ys), zs) * warpAmp;
		yr += Lerp(ly0y, Lerp(ly0x, ly1x, ys), zs) * warpAmp;
		zr += Lerp(lz0y, Lerp(lz0x, lz1x, ys), zs) * warpAmp;
	}


	// Domain Warp Simplex/OpenSimplex2
	private void SingleDomainWarpSimplexGradient (int seed, float warpAmp, float frequency, float x, float y, ref float xr, ref float yr, bool outGradOnly) {
		const float SQRT3 = 1.7320508075688772935274463415059f;
		const float G2 = (3 - SQRT3) / 6;

		x *= frequency;
		y *= frequency;

		/*
		 * --- Skew moved to TransformNoiseCoordinate method ---
		 * const FNfloat F2 = 0.5f * (SQRT3 - 1);
		 * FNfloat s = (x + y) * F2;
		 * x += s; y += s;
		*/

		int i = FastFloor(x);
		int j = FastFloor(y);
		float xi = (float)(x - i);
		float yi = (float)(y - j);

		float t = (xi + yi) * G2;
		float x0 = (float)(xi - t);
		float y0 = (float)(yi - t);

		i *= PRIME_X;
		j *= PRIME_Y;

		float vx, vy;
		vx = vy = 0;

		float a = 0.5f - x0 * x0 - y0 * y0;
		if (a > 0) {
			float aaaa = (a * a) * (a * a);
			float xo, yo;
			if (outGradOnly)
				GradCoordOut(seed, i, j, out xo, out yo);
			else
				GradCoordDual(seed, i, j, x0, y0, out xo, out yo);
			vx += aaaa * xo;
			vy += aaaa * yo;
		}

		float c = (float)(2 * (1 - 2 * G2) * (1 / G2 - 2)) * t + ((float)(-2 * (1 - 2 * G2) * (1 - 2 * G2)) + a);
		if (c > 0) {
			float x2 = x0 + (2 * (float)G2 - 1);
			float y2 = y0 + (2 * (float)G2 - 1);
			float cccc = (c * c) * (c * c);
			float xo, yo;
			if (outGradOnly)
				GradCoordOut(seed, i + PRIME_X, j + PRIME_Y, out xo, out yo);
			else
				GradCoordDual(seed, i + PRIME_X, j + PRIME_Y, x2, y2, out xo, out yo);
			vx += cccc * xo;
			vy += cccc * yo;
		}

		if (y0 > x0) {
			float x1 = x0 + (float)G2;
			float y1 = y0 + ((float)G2 - 1);
			float b = 0.5f - x1 * x1 - y1 * y1;
			if (b > 0) {
				float bbbb = (b * b) * (b * b);
				float xo, yo;
				if (outGradOnly)
					GradCoordOut(seed, i, j + PRIME_Y, out xo, out yo);
				else
					GradCoordDual(seed, i, j + PRIME_Y, x1, y1, out xo, out yo);
				vx += bbbb * xo;
				vy += bbbb * yo;
			}
		} else {
			float x1 = x0 + ((float)G2 - 1);
			float y1 = y0 + (float)G2;
			float b = 0.5f - x1 * x1 - y1 * y1;
			if (b > 0) {
				float bbbb = (b * b) * (b * b);
				float xo, yo;
				if (outGradOnly)
					GradCoordOut(seed, i + PRIME_X, j, out xo, out yo);
				else
					GradCoordDual(seed, i + PRIME_X, j, x1, y1, out xo, out yo);
				vx += bbbb * xo;
				vy += bbbb * yo;
			}
		}

		xr += vx * warpAmp;
		yr += vy * warpAmp;
	}

	private void SingleDomainWarpOpenSimplex2Gradient (int seed, float warpAmp, float frequency, float x, float y, float z, ref float xr, ref float yr, ref float zr, bool outGradOnly) {
		x *= frequency;
		y *= frequency;
		z *= frequency;

		/*
		 * --- Rotation moved to TransformDomainWarpCoordinate method ---
		 * const FNfloat R3 = (FNfloat)(2.0 / 3.0);
		 * FNfloat r = (x + y + z) * R3; // Rotation, not skew
		 * x = r - x; y = r - y; z = r - z;
		*/

		int i = FastRound(x);
		int j = FastRound(y);
		int k = FastRound(z);
		float x0 = (float)x - i;
		float y0 = (float)y - j;
		float z0 = (float)z - k;

		int xNSign = (int)(-x0 - 1.0f) | 1;
		int yNSign = (int)(-y0 - 1.0f) | 1;
		int zNSign = (int)(-z0 - 1.0f) | 1;

		float ax0 = xNSign * -x0;
		float ay0 = yNSign * -y0;
		float az0 = zNSign * -z0;

		i *= PRIME_X;
		j *= PRIME_Y;
		k *= PRIME_Z;

		float vx, vy, vz;
		vx = vy = vz = 0;

		float a = (0.6f - x0 * x0) - (y0 * y0 + z0 * z0);
		for (int l = 0; ; l++) {
			if (a > 0) {
				float aaaa = (a * a) * (a * a);
				float xo, yo, zo;
				if (outGradOnly)
					GradCoordOut(seed, i, j, k, out xo, out yo, out zo);
				else
					GradCoordDual(seed, i, j, k, x0, y0, z0, out xo, out yo, out zo);
				vx += aaaa * xo;
				vy += aaaa * yo;
				vz += aaaa * zo;
			}

			float b = a;
			int i1 = i;
			int j1 = j;
			int k1 = k;
			float x1 = x0;
			float y1 = y0;
			float z1 = z0;

			if (ax0 >= ay0 && ax0 >= az0) {
				x1 += xNSign;
				b = b + ax0 + ax0;
				i1 -= xNSign * PRIME_X;
			} else if (ay0 > ax0 && ay0 >= az0) {
				y1 += yNSign;
				b = b + ay0 + ay0;
				j1 -= yNSign * PRIME_Y;
			} else {
				z1 += zNSign;
				b = b + az0 + az0;
				k1 -= zNSign * PRIME_Z;
			}

			if (b > 1) {
				b -= 1;
				float bbbb = (b * b) * (b * b);
				float xo, yo, zo;
				if (outGradOnly)
					GradCoordOut(seed, i1, j1, k1, out xo, out yo, out zo);
				else
					GradCoordDual(seed, i1, j1, k1, x1, y1, z1, out xo, out yo, out zo);
				vx += bbbb * xo;
				vy += bbbb * yo;
				vz += bbbb * zo;
			}

			if (l == 1) break;

			ax0 = 0.5f - ax0;
			ay0 = 0.5f - ay0;
			az0 = 0.5f - az0;

			x0 = xNSign * ax0;
			y0 = yNSign * ay0;
			z0 = zNSign * az0;

			a += (0.75f - ax0) - (ay0 + az0);

			i += (xNSign >> 1) & PRIME_X;
			j += (yNSign >> 1) & PRIME_Y;
			k += (zNSign >> 1) & PRIME_Z;

			xNSign = -xNSign;
			yNSign = -yNSign;
			zNSign = -zNSign;

			seed += 1293373;
		}

		xr += vx * warpAmp;
		yr += vy * warpAmp;
		zr += vz * warpAmp;
	}


	#endregion




	#region --- UTL ---


	[MethodImpl(INLINE)]
	private static float FastMin (float a, float b) => a < b ? a : b;

	[MethodImpl(INLINE)]
	private static float FastMax (float a, float b) => a > b ? a : b;

	[MethodImpl(INLINE)]
	private static float FastAbs (float f) => f < 0 ? -f : f;

	[MethodImpl(INLINE)]
	private static float FastSqrt (float f) => (float)Math.Sqrt(f);

	[MethodImpl(INLINE)]
	private static int FastFloor (float f) => f >= 0 ? (int)f : (int)f - 1;

	[MethodImpl(INLINE)]
	private static int FastRound (float f) => f >= 0 ? (int)(f + 0.5f) : (int)(f - 0.5f);

	[MethodImpl(INLINE)]
	private static float Lerp (float a, float b, float t) => a + t * (b - a);

	[MethodImpl(INLINE)]
	private static float InterpHermite (float t) => t * t * (3 - 2 * t);

	[MethodImpl(INLINE)]
	private static float InterpQuintic (float t) => t * t * t * (t * (t * 6 - 15) + 10);

	[MethodImpl(INLINE)]
	private static float CubicLerp (float a, float b, float c, float d, float t) {
		float p = (d - c) - (a - b);
		return t * t * t * p + t * t * ((a - b) - p) + t * (c - a) + b;
	}

	[MethodImpl(INLINE)]
	private static float PingPong (float t) {
		t -= (int)(t * 0.5f) * 2;
		return t < 1 ? t : 2 - t;
	}


	[MethodImpl(INLINE)]
	private static int Hash (int seed, int xPrimed, int yPrimed) {
		int hash = seed ^ xPrimed ^ yPrimed;

		hash *= 0x27d4eb2d;
		return hash;
	}

	[MethodImpl(INLINE)]
	private static int Hash (int seed, int xPrimed, int yPrimed, int zPrimed) {
		int hash = seed ^ xPrimed ^ yPrimed ^ zPrimed;

		hash *= 0x27d4eb2d;
		return hash;
	}

	[MethodImpl(INLINE)]
	private static float ValCoord (int seed, int xPrimed, int yPrimed) {
		int hash = Hash(seed, xPrimed, yPrimed);

		hash *= hash;
		hash ^= hash << 19;
		return hash * (1 / 2147483648.0f);
	}

	[MethodImpl(INLINE)]
	private static float ValCoord (int seed, int xPrimed, int yPrimed, int zPrimed) {
		int hash = Hash(seed, xPrimed, yPrimed, zPrimed);

		hash *= hash;
		hash ^= hash << 19;
		return hash * (1 / 2147483648.0f);
	}

	[MethodImpl(INLINE)]
	private static float GradCoord (int seed, int xPrimed, int yPrimed, float xd, float yd) {
		int hash = Hash(seed, xPrimed, yPrimed);
		hash ^= hash >> 15;
		hash &= 127 << 1;

		float xg = Gradients2D[hash];
		float yg = Gradients2D[hash | 1];

		return xd * xg + yd * yg;
	}

	[MethodImpl(INLINE)]
	private static float GradCoord (int seed, int xPrimed, int yPrimed, int zPrimed, float xd, float yd, float zd) {
		int hash = Hash(seed, xPrimed, yPrimed, zPrimed);
		hash ^= hash >> 15;
		hash &= 63 << 2;

		float xg = Gradients3D[hash];
		float yg = Gradients3D[hash | 1];
		float zg = Gradients3D[hash | 2];

		return xd * xg + yd * yg + zd * zg;
	}

	[MethodImpl(INLINE)]
	private static void GradCoordOut (int seed, int xPrimed, int yPrimed, out float xo, out float yo) {
		int hash = Hash(seed, xPrimed, yPrimed) & (255 << 1);

		xo = RandVecs2D[hash];
		yo = RandVecs2D[hash | 1];
	}

	[MethodImpl(INLINE)]
	private static void GradCoordOut (int seed, int xPrimed, int yPrimed, int zPrimed, out float xo, out float yo, out float zo) {
		int hash = Hash(seed, xPrimed, yPrimed, zPrimed) & (255 << 2);

		xo = RandVecs3D[hash];
		yo = RandVecs3D[hash | 1];
		zo = RandVecs3D[hash | 2];
	}

	[MethodImpl(INLINE)]
	private static void GradCoordDual (int seed, int xPrimed, int yPrimed, float xd, float yd, out float xo, out float yo) {
		int hash = Hash(seed, xPrimed, yPrimed);
		int index1 = hash & (127 << 1);
		int index2 = (hash >> 7) & (255 << 1);

		float xg = Gradients2D[index1];
		float yg = Gradients2D[index1 | 1];
		float value = xd * xg + yd * yg;

		float xgo = RandVecs2D[index2];
		float ygo = RandVecs2D[index2 | 1];

		xo = value * xgo;
		yo = value * ygo;
	}

	[MethodImpl(INLINE)]
	private static void GradCoordDual (int seed, int xPrimed, int yPrimed, int zPrimed, float xd, float yd, float zd, out float xo, out float yo, out float zo) {
		int hash = Hash(seed, xPrimed, yPrimed, zPrimed);
		int index1 = hash & (63 << 2);
		int index2 = (hash >> 6) & (255 << 2);

		float xg = Gradients3D[index1];
		float yg = Gradients3D[index1 | 1];
		float zg = Gradients3D[index1 | 2];
		float value = xd * xg + yd * yg + zd * zg;

		float xgo = RandVecs3D[index2];
		float ygo = RandVecs3D[index2 | 1];
		float zgo = RandVecs3D[index2 | 2];

		xo = value * xgo;
		yo = value * ygo;
		zo = value * zgo;
	}



	#endregion




}