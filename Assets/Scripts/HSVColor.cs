using UnityEngine;

[System.Serializable]
public struct HSVColor
{
    public float h;
    public float s;
    public float v;
    public float a;
	
	/// <summary>
	/// Initializes a new instance of the <see cref="HSVColor"/> struct.
	/// </summary>
	/// <param name="h">The height.</param>
	/// <param name="s">S.</param>
	/// <param name="v">V.</param>
	public HSVColor(float h, float s, float v)
	{
        this.h = h;
        this.s = s;
        this.v = v;
        this.a = 1f;
    }
	
	/// <summary>
	/// Initializes a new instance of the <see cref="HSVColor"/> struct.
	/// </summary>
	/// <param name="h">The height.</param>
	/// <param name="s">S.</param>
	/// <param name="v">V.</param>
	/// <param name="a">The alpha component.</param>
    public HSVColor(float h, float s, float v, float a)
    {
        this.h = h;
        this.s = s;
		this.v = v;
        this.a = a;
    }

    /// <summary>
    /// Returns an instance of HSVColor from RGBA
    /// </summary>
    /// <returns>The RGB.</returns>
    /// <param name="r">The red component.</param>
    /// <param name="g">The green component.</param>
    /// <param name="b">The blue component.</param>
    /// <param name="a">The alpha component.</param>
	static public HSVColor FromRGBA(float r, float g, float b, float a)
	{
		Vector4 K = new Vector4(0.0f, -1.0f / 3.0f, 2.0f / 3.0f, -1.0f);
		Vector4 p = g < b ? new Vector4(b, g, K.w, K.z) : new Vector4(g, b, K.x, K.y);
		Vector4 q = r < p.x ? new Vector4(p.x, p.y, p.w, r) : new Vector4(r, p.y, p.z, p.x);

    	float d = q.x - Mathf.Min(q.w, q.y);
    	float e = 1.0e-10f;
    	return new HSVColor(Mathf.Abs(q.z + (q.w - q.y) / (6.0f * d + e)), d / (q.x + e), q.x);
	}

	/// <summary>
	/// Returns an instance of HSVColor from Color
	/// </summary>
	/// <returns>The color.</returns>
	/// <param name="c">C.</param>
	static public HSVColor FromColor(Color c)
	{
		Vector4 K = new Vector4(0.0f, -1.0f / 3.0f, 2.0f / 3.0f, -1.0f);
		Vector4 p = c.g < c.b ? new Vector4(c.b, c.g, K.w, K.z) : new Vector4(c.g, c.b, K.x, K.y);
		Vector4 q = c.r < p.x ? new Vector4(p.x, p.y, p.w, c.r) : new Vector4(c.r, p.y, p.z, p.x);

    	float d = q.x - Mathf.Min(q.w, q.y);
    	float e = 1.0e-10f;
    	return new HSVColor(Mathf.Abs(q.z + (q.w - q.y) / (6.0f * d + e)), d / (q.x + e), q.x);
	}


	static private Vector3 Frac(Vector3 value)
	{
		return new Vector3 (value.x - Mathf.Floor(value.x), value.y - Mathf.Floor(value.y), value.z - Mathf.Floor(value.z));
	}

	static private Vector3 Clamp(Vector3 value, float min, float max)
	{
		return new Vector3(Mathf.Clamp(value.x, min, max), Mathf.Clamp(value.y, min, max), Mathf.Clamp(value.z, min, max));
	}

	static private Vector3 Abs(Vector3 value)
	{
		return new Vector3(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z));
	}
}

public class HSVColorBuffer
{
	public HSVColor[] data;
	private readonly int height;
	private readonly int width;

	public HSVColorBuffer(int width, int height, Color[] colorData)
	{
		this.width = width;
		this.height = height;

		this.data = new HSVColor[colorData.Length];

		for (int i = 0; i < colorData.Length; i++)
			this.data[i] = HSVColor.FromColor(colorData[i]);
	}

	public HSVColor this[int x, int y]
	{
		get
		{
			return data[(y * width)+height];
		}
		set
		{
			data[(y * width) + x] = value;
		}
	}
}

	/*

	static public Color ToColor(Vector3 c)
	{
    	Vector4 K = new Vector4(1.0f, 2.0f / 3.0f, 1.0f / 3.0f, 3.0f);
    	Vector4 p = Abs(Frac(new Vector3(c.x, c.x, c.x) + new Vector3(K.x, K.y, K.z)) * 6.0f - new Vector3(K.w, K.w, K.w));
    	return new Color(c.z * Vector3.Lerp(new Vector3(K.x, K.x, K.x), Clamp(new Vector3(p.x, p.y, p.z) - new Vector3(K.x, K.x, K.x), 0.0f, 1.0f), c.y));
	}*/


/*
vec3 rgb2hsv(vec3 c)
{
    vec4 K = vec4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
	vec4 p = c.g < c.b ? vec4(c.bg, K.wz) : vec4(c.gb, K.xy);
    vec4 q = c.r < p.x ? vec4(p.xyw, c.r) : vec4(c.r, p.yzx);

    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    return vec3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

vec3 hsv2rgb(vec3 c)
{
    vec4 K = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * normalize(mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y));
}
*/

    /*
 
	/// <summary>
	/// RGG HSV Converstion
	/// </summary>
	/// <returns>
	/// The color.
	/// </returns>
	/// <param name='r'>
	/// R.
	/// </param>
	/// <param name='g'>
	/// G.
	/// </param>
	/// <param name='b'>
	/// B.
	/// </param>
	/// <param name='a'>
	/// A.
	/// </param>
	static public HSVColor FromRGBA(float r, float g, float b, float a)
	{
		float K = 0.0f;
		
		if (g < b)
		{
			float tmp = g; g = b; b = tmp;
			K = -1.0f;
		}
		
		if (r < g)
		{
			float tmp = r; r = g; g = tmp;
			K = -2.0f / 6.0f - K;
		}
		
		if (g < b)
		{
			float tmp = g; g = b; b = tmp;
			K = -K;
		}
	
		float chroma = r - b;
		float h = Mathf.Abs(K + (g - b) / (6.0f * chroma + 1e-20f));
		float s = chroma / (r + 1e-20f);
		float v = r;
		
		return new HSVColor(h,s,v,a);
	}


	
	/// <summary>
	/// Converts to an RGB Color
	/// </summary>
	/// <returns>
	/// The color.
	/// </returns>
	public Color ToColor() {
		int hi = (int)Mathf.Floor(h / 60.0f) % 6;
		float f = (h / 60.0f) - Mathf.Floor(h / 60.0f);
		
		float p = v * (1.0f - s);
		float q = v * (1.0f - (f * s));
		float t = v * (1.0f - ((1.0f - f) * s));
		
		Color ret;
		
		switch (hi)
		{
			case 0:
				ret = new Color(v, t, p);
			break;
			case 1:
				ret = new Color(q, v, p);
			break;
			case 2:
				ret = new Color(p, v, t);
			break;
			case 3:
				ret = new Color(p, q, v);
			break;
			case 4:
				ret = new Color(t, p, v);
			break;
			case 5:
				ret = new Color(v, p, q);
			break;
			default:
				ret = new Color(0, 0, 0, 1);
			break;
		}
		return ret;
	}
}*/
