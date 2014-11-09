using UnityEngine;

namespace Planetary {

public class PerlinNoise {

	private static int[] permutation;

	private static int[] table = new int[] {15,131,91,90,13,95,201,194,96,7,53,233,140,30,225,69,36,103,142,6,8,99,21,
											37,240,10,23,120,190,148,0,247,94,234,75,197,26,62,203,32,252,219,57,117,35,
											11,87,33,177,149,88,237,56,20,174,136,175,125,171,168,74,68,134,48,165,71,139,
											27,77,166,231,111,146,158,83,133,229,60,122,230,211,220,41,105,55,92,102,46,245,
											244,40,143,63,54,65,25,216,161,1,73,80,169,187,209,76,132,208,89,18,188,130,200,
											196,135,116,159,86,198,100,164,3,109,173,64,186,52,226,5,217,250,124,202,123,38,
											255,147,126,118,59,85,82,212,207,206,227,16,47,58,189,28,17,182,183,42,170,223,213,
											248,152,119,44,221,2,155,154,163,70,101,153,167,43,9,172,129,39,22,110,253,108,19,98,
											113,79,232,224,185,178,112,246,218,104,251,97,228,34,242,193,238,241,210,12,144,179,
											191,162,81,107,51,145,235,249,239,14,106,49,199,192,214,31,181,84,204,157,184,127,115,
											176,121,50,4,45,150,254,138,236,222,93,205,29,114,67,141,24,72,128,243,215,195,66,78,
											137,156,61,160,180,151 };

	static PerlinNoise() {
		permutation = new int[512];
		for(int i = 0; i < 512; i++)
			permutation[i] = table[i & 255]; 
	}

	public static double Noise(double x, double y, double z) {
		int X = FastFloor(x) & 255;
		int Y = FastFloor(y) & 255;
		int Z = FastFloor(z) & 255;
		x -= FastFloor(x);
		y -= FastFloor(y);
		z -= FastFloor(z);

		int A = permutation[X] + Y;
		int AA = permutation[A] + Z;
		int AB = permutation[A + 1] + Z;
		int B = permutation[X + 1] + Y;
		int BA = permutation[B] + Z;
		int BB = permutation[B + 1] + Z;

		double u = Fade(x);
		double v = Fade(y);
		double w = Fade(z);
		double g1 = Lerp(v, Lerp(u, Gradient(permutation[AA], x, y, z),
		                        	Gradient(permutation[BA], x - 1, y, z)),
		                 	Lerp(u, Gradient(permutation[AB], x, y - 1, z),
		    						Gradient(permutation[BB], x - 1, y - 1, z)));
		double g2 = Lerp(v, Lerp(u, Gradient(permutation[AA + 1], x, y, z - 1), 
		                        	Gradient(permutation[BA + 1], x - 1, y  , z - 1 )),
		                	Lerp(u, Gradient(permutation[AB + 1], x  , y - 1, z - 1 ),
		     						Gradient(permutation[BB + 1], x - 1, y - 1, z - 1 )));
		return Lerp(w, g1, g2);
	}

	public static Vector4 dNoise(double x, double y, double z) {
		int X = FastFloor(x) & 255;
		int Y = FastFloor(y) & 255;
		int Z = FastFloor(z) & 255;
		x -= FastFloor(x);
		y -= FastFloor(y);
		z -= FastFloor(z);

		double u = x;
		double v = y;
		double w = z;

		//double u = 3*x*x - 2*x*x*x;
		//double v = 3*y*y - 2*y*y*y;
		//double w = 3*z*z - 2*z*z*z;

		double du = 30 * u*u*u*u - 60 * u*u*u + 30 * u*u;//30.0*u*u*(u*(u-2.0)+1.0);
		double dv = 30 * v*v*v*v - 60 * v*v*v + 30 * v*v;//30.0*v*v*(v*(v-2.0)+1.0);
		double dw = 30 * w*w*w*w - 60 * w*w*w + 30 * w*w;//30.0*w*w*(w*(w-2.0)+1.0);

		u = Fade(x);
		v = Fade(y);
		w = Fade(z);

		int A = permutation[X] + Y;
		int AA = permutation[A] + Z;
		int AB = permutation[A + 1] + Z;
		int B = permutation[X + 1] + Y;
		int BA = permutation[B] + Z;
		int BB = permutation[B + 1] + Z;

		double a = Gradient(permutation[AA], x, y, z);
		double b = Gradient(permutation[BA], x - 1, y, z);
		double c = Gradient(permutation[AB], x, y - 1, z);
		double d = Gradient(permutation[BB], x - 1, y - 1, z);
		double e = Gradient(permutation[AA + 1], x, y, z - 1);
		double f = Gradient(permutation[BA + 1], x - 1, y  , z - 1 );
		double g = Gradient(permutation[AB + 1], x  , y - 1, z - 1 );
		double h = Gradient(permutation[BB + 1], x - 1, y - 1, z - 1 );

		double k0 =   a;
		double k1 =   b - a;
		double k2 =   c - a;
		double k3 =   e - a;
		double k4 =   a - b - c + d;
		double k5 =   a - c - e + g;
		double k6 =   a - b - e + f;
		double k7 = - a + b + c - d + e - f - g + h;

		double noise = k0 + k1*u + k2*v + k3*w + k4*u*v + k5*v*w + k6*w*u + k7*u*v*w;

		return new Vector4((float)noise,
		                   (float)(du * (k1 + k4*v + k6*w + k7*v*w)),
		                   (float)(dv * (k2 + k5*w + k4*u + k7*w*u)),
		                   (float)(dw * (k3 + k6*u + k5*v + k7*u*v)));

		//double g1 = Lerp(v, Lerp(u, a, b), Lerp(u, c, d));
		//double g2 = Lerp(v, Lerp(u, e, f), Lerp(u, g, h));
		//return Lerp(w, g1, g2);
	}

	private static int FastFloor(double x) {
		return x > 0 ? (int)x : (int)x - 1;
	}
	private static double Lerp(double t, double a, double b) { 
		return a + t * (b - a); 
	}
	private static double Fade(double t) { 
		return t * t * t * (t * (t * 6 - 15) + 10); 
	}
	private static double Gradient(int hash, double x, double y, double z) {
		int h = hash & 15;
		double u = h < 8 ? x : y;
		double v = h < 4 ? y : h == 12 || h == 14 ? x : z;
		return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
	}
}

}