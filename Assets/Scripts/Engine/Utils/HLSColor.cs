using UnityEngine;

[System.Serializable]
public struct HLSColor
{
    public float h;
    public float s;
    public float l;
    public float a;
	
	//_________________________________________________________________________
    public HLSColor(float h, float l, float s, float a){
        this.h = h;
        this.l = l;
        this.s = s;
        this.a = a;
    }
   
   //_________________________________________________________________________
    public HLSColor(float h, float l, float s){
        this.h = h;
        this.l = l;
        this.s = s;
        this.a = 1f;
    }
	

    
    //_________________________________________________________________________
    private static float Value(float n1, float n2, float hue)
    {
    	float aux;
    	
    	 if (hue > 360)
   			 hue-=360;
   			 
  		if (hue < 0)
    		hue+=360;
    	
    	if (hue < 60)
    		aux=n1+(n2-n1) * hue / 60.0f;
    	else
    		if (hue < 180)
      			aux=n2;
  		else
    		if (hue < 240 )
      			aux=n1+(n2-n1)*(240 -hue) / 60.0f;
      		else
      			aux=n1;
    	
    	return aux; 
    }
	
	//_________________________________________________________________________
	public static Color ToColor(float h,float l, float s, float a)
	{
 		float r=0,g=0,b=0,m1,m2;
  
  		if (l <= 0.5f)
    		m2= l*(1+s);
  		else
    		m2= l+s - l * s;
    
  		m1=2 * l - m2;
		
    	r = Value(m1,m2,h+120);
    	g = Value(m1,m2,h);
    	b = Value(m1,m2,h-120);
  		
  		return new Color(r,g,b,a);       
	}
	
	//_________________________________________________________________________
	 public static HLSColor FromColor(Color color){
        float min = Mathf.Min(Mathf.Min(color.r, color.g), color.b);
        float max = Mathf.Max(Mathf.Max(color.r, color.g), color.b);
        float delta = max - min;

        float h = 0;
        float s = 0;
        float l = (float)((max + min) / 2.0f);

        if (delta != 0){
			
            if (l < 0.5f){
                s = (float)(delta / (max + min));
            }
            else{
                s = (float)(delta / (2.0f - max - min));
            }


            if (color.r == max)
            {
                h = (color.g - color.b) / delta;
            }
            else if (color.g == max)
            {
                h = 2f + (color.b - color.r) / delta;
            }
            else if (color.b == max)
            {
                h = 4f + (color.r - color.g) / delta;
            }
        }
		
		h = Mathf.Repeat(h * 60,360f);
		
        return new HLSColor(h, l, s, color.a);
    }
	
	//_________________________________________________________________________
	 public static HLSColor FromRGBA(float r, float g, float b, float a){
        float min = Mathf.Min(Mathf.Min(r, g), b);
        float max = Mathf.Max(Mathf.Max(r, g), b);
        float delta = max - min;

        float h = 0;
        float s = 0;
        float l = (float)((max + min) / 2.0f);

        if (delta != 0){
			
            if (l < 0.5f){
                s = (float)(delta / (max + min));
            }
            else{
                s = (float)(delta / (2.0f - max - min));
            }


            if (r == max)
            {
                h = (g - b) / delta;
            }
            else if (g == max)
            {
                h = 2f + (b - r) / delta;
            }
            else if (b == max)
            {
                h = 4f + (r - g) / delta;
            }
        }
		
		h = Mathf.Repeat(h * 60,360f);
		
        return new HLSColor(h, l, s, a);
    }


}
