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
	/// <param name='h'>
	/// Hue.
	/// </param>
	/// <param name='s'>
	/// Ssaturation.
	/// </param>
	/// <param name='v'>
	/// Value.
	/// </param>
	public HSVColor(float h, float s, float v){
        this.h = h;
        this.s = s;
        this.v = v;
        this.a = 1f;
    }
	
	/// <summary>
	/// Initializes a new instance of the <see cref="HSVColor"/> struct.
	/// </summary>
	/// <param name='h'>
	/// Hue.
	/// </param>
	/// <param name='s'>
	/// Saturation.
	/// </param>
	/// <param name='v'>
	/// Value.
	/// </param>
	/// <param name='a'>
	/// Aalpha.
	/// </param>
    public HSVColor(float h, float s, float v, float a){
        this.h = h;
        this.s = s;
		this.v = v;
        this.a = a;
    }
   
  
 
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
}
