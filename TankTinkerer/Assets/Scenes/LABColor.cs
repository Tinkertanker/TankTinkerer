using UnityEngine;

[System.Serializable]

public struct LABColor
{

	// This script provides a Lab color space in addition to Unity's built in Red/Green/Blue colors.
	// Lab is based on CIE XYZ and is a color-opponent space with L for lightness and a and b for the color-opponent dimensions.
	// Lab color is designed to approximate human vision and so it aspires to perceptual uniformity.
	// The L component closely matches human perception of lightness.
	// Put LABColor.cs in a 'Plugins' folder to ensure that it is accessible to other scripts.

	private float L;
	private float A;
	private float B;

	// lightness accessors
	public float l {
		get { return this.L; }
		set { this.L = value; }
	}

	// a color-opponent accessor
	public float a {
		get { return this.A; }
		set { this.A = value; }
	}

	// b color-opponent accessor
	public float b {
		get { return this.B; }
		set { this.B = value; }
	}

	// constructor - takes three floats for lightness and color-opponent dimensions
	public LABColor (float l, float a, float b) : this ()
	{
		this.l = l;
		this.a = a;
		this.b = b;
	}

	// constructor - takes a Color
	public LABColor (Color col) : this ()
	{
		LABColor temp = FromColor (col);
		l = temp.l;
		a = temp.a;
		b = temp.b;
	}

	// static function for linear interpolation between two LABColors
	public static LABColor Lerp (LABColor a, LABColor b, float t)
	{
		return new LABColor (Mathf.Lerp (a.l, b.l, t), Mathf.Lerp (a.a, b.a, t), Mathf.Lerp (a.b, b.b, t));
	}

	// static function for interpolation between two Unity Colors through normalized colorspace
	public static Color Lerp (Color a, Color b, float t)
	{
		return (LABColor.Lerp (LABColor.FromColor (a), LABColor.FromColor (b), t)).ToColor ();
	}

	// static function for returning the color difference in a normalized colorspace (Delta-E)
	public static float Distance (LABColor a, LABColor b)
	{
		return Mathf.Sqrt (Mathf.Pow ((a.l - b.l), 2f) + Mathf.Pow ((a.a - b.a), 2f) + Mathf.Pow ((a.b - b.b), 2f));
	}

	// static function for converting from Color to LABColor
	public static LABColor FromColor (Color c)
	{
		float D65x = 0.9505f;
		float D65y = 1.0f;
		float D65z = 1.0890f;
		float rLinear = c.r;
		float gLinear = c.g;
		float bLinear = c.b;
		float r = (rLinear > 0.04045f) ? Mathf.Pow ((rLinear + 0.055f) / (1f + 0.055f), 2.2f) : (rLinear / 12.92f);
		float g = (gLinear > 0.04045f) ? Mathf.Pow ((gLinear + 0.055f) / (1f + 0.055f), 2.2f) : (gLinear / 12.92f);
		float b = (bLinear > 0.04045f) ? Mathf.Pow ((bLinear + 0.055f) / (1f + 0.055f), 2.2f) : (bLinear / 12.92f);
		float x = (r * 0.4124f + g * 0.3576f + b * 0.1805f);
		float y = (r * 0.2126f + g * 0.7152f + b * 0.0722f);
		float z = (r * 0.0193f + g * 0.1192f + b * 0.9505f);
		x = (x > 0.9505f) ? 0.9505f : ((x < 0f) ? 0f : x);
		y = (y > 1.0f) ? 1.0f : ((y < 0f) ? 0f : y);
		z = (z > 1.089f) ? 1.089f : ((z < 0f) ? 0f : z);
		LABColor lab = new LABColor (0f, 0f, 0f);
		float fx = x / D65x;
		float fy = y / D65y;
		float fz = z / D65z;
		fx = ((fx > 0.008856f) ? Mathf.Pow (fx, (1.0f / 3.0f)) : (7.787f * fx + 16.0f / 116.0f));
		fy = ((fy > 0.008856f) ? Mathf.Pow (fy, (1.0f / 3.0f)) : (7.787f * fy + 16.0f / 116.0f));
		fz = ((fz > 0.008856f) ? Mathf.Pow (fz, (1.0f / 3.0f)) : (7.787f * fz + 16.0f / 116.0f));
		lab.l = 116.0f * fy - 16f;
		lab.a = 500.0f * (fx - fy);
		lab.b = 200.0f * (fy - fz);
		return lab;
	}

	// static function for converting from LABColor to Color
	public static Color ToColor (LABColor lab)
	{
		float D65x = 0.9505f;
		float D65y = 1.0f;
		float D65z = 1.0890f;
		float delta = 6.0f / 29.0f;
		float fy = (lab.l + 16f) / 116.0f;
		float fx = fy + (lab.a / 500.0f);
		float fz = fy - (lab.b / 200.0f);
		float x = (fx > delta) ? D65x * (fx * fx * fx) : (fx - 16.0f / 116.0f) * 3f * (delta * delta) * D65x;
		float y = (fy > delta) ? D65y * (fy * fy * fy) : (fy - 16.0f / 116.0f) * 3f * (delta * delta) * D65y;
		float z = (fz > delta) ? D65z * (fz * fz * fz) : (fz - 16.0f / 116.0f) * 3f * (delta * delta) * D65z;
		float r = x * 3.2410f - y * 1.5374f - z * 0.4986f;
		float g = -x * 0.9692f + y * 1.8760f - z * 0.0416f;
		float b = x * 0.0556f - y * 0.2040f + z * 1.0570f;
		r = (r <= 0.0031308f) ? 12.92f * r : (1f + 0.055f) * Mathf.Pow (r, (1.0f / 2.4f)) - 0.055f;
		g = (g <= 0.0031308f) ? 12.92f * g : (1f + 0.055f) * Mathf.Pow (g, (1.0f / 2.4f)) - 0.055f;
		b = (b <= 0.0031308f) ? 12.92f * b : (1f + 0.055f) * Mathf.Pow (b, (1.0f / 2.4f)) - 0.055f;
		r = (r < 0) ? 0 : r;
		g = (g < 0) ? 0 : g;
		b = (b < 0) ? 0 : b;
		return new Color (r, g, b);
	}

	// function for converting an instance of LABColor to Color
	public Color ToColor ()
	{
		return LABColor.ToColor (this);	
	}

	// override for string
	public override string ToString ()
	{
		return "L:" + l + " A:" + a + " B:" + b;
	}

	// are two LABColors the same?
	public override bool Equals (System.Object obj)
	{
		if (obj == null || GetType () != obj.GetType ())
			return false;
		return (this == (LABColor)obj);
	}

	// override hashcode for a LABColor
	public override int GetHashCode ()
	{
		return l.GetHashCode () ^ a.GetHashCode () ^ b.GetHashCode ();
	}

	// Equality operator
	public static bool operator == (LABColor item1, LABColor item2)
	{
		return (item1.l == item2.l && item1.a == item2.a && item1.b == item2.b);
	}

	// Inequality operator
	public static bool operator != (LABColor item1, LABColor item2)
	{
		return (item1.l != item2.l || item1.a != item2.a || item1.b != item2.b);
	}

	// Comparison
	public static float Compare (LABColor lab1, LABColor lab2)
	{

		//Set weighting factors to 1
		float k_L = 1.0f;
		float k_C = 1.0f;
		float k_H = 1.0f;

		//Calculate Cprime1, Cprime2, Cabbar
		float c_star_1_ab = Mathf.Sqrt (lab1.A * lab1.A + lab1.B * lab1.B);
		float c_star_2_ab = Mathf.Sqrt (lab2.A * lab2.A + lab2.B * lab2.B);
		float c_star_average_ab = (c_star_1_ab + c_star_2_ab) / 2;

		float c_star_average_ab_pot7 = c_star_average_ab * c_star_average_ab * c_star_average_ab;
		c_star_average_ab_pot7 *= c_star_average_ab_pot7 * c_star_average_ab;

		float G = 0.5f * (1 - Mathf.Sqrt (c_star_average_ab_pot7 / (c_star_average_ab_pot7 + 6103515625))); //25^7
		float a1_prime = (1 + G) * lab1.A;
		float a2_prime = (1 + G) * lab2.A;

		float C_prime_1 = Mathf.Sqrt (a1_prime * a1_prime + lab1.B * lab1.B);
		float C_prime_2 = Mathf.Sqrt (a2_prime * a2_prime + lab2.B * lab2.B);
		//Angles in Degree.
		float h_prime_1 = (Mathf.Rad2Deg * ((Mathf.Atan2 (lab1.B, a1_prime)) + 360) % 360f);
		float h_prime_2 = (Mathf.Rad2Deg * ((Mathf.Atan2 (lab2.B, a2_prime)) + 360) % 360f);

		float delta_L_prime = lab2.L - lab1.L;
		float delta_C_prime = C_prime_2 - C_prime_1;

		float h_bar = Mathf.Abs (h_prime_1 - h_prime_2);
		float delta_h_prime;
		if (C_prime_1 * C_prime_2 == 0)
			delta_h_prime = 0;
		else {
			if (h_bar <= 180f) {
				delta_h_prime = h_prime_2 - h_prime_1;
			} else if (h_bar > 180f && h_prime_2 <= h_prime_1) {
				delta_h_prime = h_prime_2 - h_prime_1 + 360f;
			} else {
				delta_h_prime = h_prime_2 - h_prime_1 - 360f;
			}
		}
		float delta_H_prime = 2 * Mathf.Sqrt (C_prime_1 * C_prime_2) * Mathf.Sin (Mathf.Deg2Rad * (delta_h_prime / 2));

		// Calculate CIEDE2000
		float L_prime_average = (lab1.L + lab2.L) / 2f;
		float C_prime_average = (C_prime_1 + C_prime_2) / 2f;

		//Calculate h_prime_average

		float h_prime_average;
		if (C_prime_1 * C_prime_2 == 0)
			h_prime_average = 0;
		else {
			if (h_bar <= 180f) {
				h_prime_average = (h_prime_1 + h_prime_2) / 2;
			} else if (h_bar > 180f && (h_prime_1 + h_prime_2) < 360f) {
				h_prime_average = (h_prime_1 + h_prime_2 + 360f) / 2;
			} else {
				h_prime_average = (h_prime_1 + h_prime_2 - 360f) / 2;
			}
		}
		float L_prime_average_minus_50_square = (L_prime_average - 50);
		L_prime_average_minus_50_square *= L_prime_average_minus_50_square;

		float S_L = 1 + ((.015f * L_prime_average_minus_50_square) / Mathf.Sqrt (20 + L_prime_average_minus_50_square));
		float S_C = 1 + .045f * C_prime_average;
		float T = 1
		          - .17f * Mathf.Cos (Mathf.Deg2Rad * (h_prime_average - 30))
		          + .24f * Mathf.Cos (Mathf.Deg2Rad * (h_prime_average * 2))
		          + .32f * Mathf.Cos (Mathf.Deg2Rad * (h_prime_average * 3 + 6))
		          - .2f * Mathf.Cos (Mathf.Deg2Rad * (h_prime_average * 4 - 63));
		float S_H = 1 + .015f * T * C_prime_average;
		float h_prime_average_minus_275_div_25_square = (h_prime_average - 275) / (25);
		h_prime_average_minus_275_div_25_square *= h_prime_average_minus_275_div_25_square;
		float delta_theta = 30 * Mathf.Exp (-h_prime_average_minus_275_div_25_square);

		float C_prime_average_pot_7 = C_prime_average * C_prime_average * C_prime_average;
		C_prime_average_pot_7 *= C_prime_average_pot_7 * C_prime_average;
		float R_C = 2 * Mathf.Sqrt (C_prime_average_pot_7 / (C_prime_average_pot_7 + 6103515625));

		float R_T = -Mathf.Sin (Mathf.Deg2Rad * ((2 * delta_theta))) * R_C;

		float delta_L_prime_div_k_L_S_L = delta_L_prime / (S_L * k_L);
		float delta_C_prime_div_k_C_S_C = delta_C_prime / (S_C * k_C);
		float delta_H_prime_div_k_H_S_H = delta_H_prime / (S_H * k_H);

		float CIEDE2000 = Mathf.Sqrt (
			                  delta_L_prime_div_k_L_S_L * delta_L_prime_div_k_L_S_L
			                  + delta_C_prime_div_k_C_S_C * delta_C_prime_div_k_C_S_C
			                  + delta_H_prime_div_k_H_S_H * delta_H_prime_div_k_H_S_H
			                  + R_T * delta_C_prime_div_k_C_S_C * delta_H_prime_div_k_H_S_H
		                  );

		return CIEDE2000;
	}
}
