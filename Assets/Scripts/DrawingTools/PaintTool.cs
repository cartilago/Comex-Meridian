using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ImageConvolutionFilters;

public class PaintTool : DrawingToolBase 
{
	#region Class members
	private Vector2 startCanvasPosition;

	static private Color[] maskColors = new Color[]{Color.red, Color.green, Color.blue};
	private List<Vector2> strokePoints = new List<Vector2>();  
	static private Texture2D masksTexture;
	static private ColorBuffer32 masksPixelBuffer;

	#endregion

	#region DrawingToolBase overrides
	override public void TouchDown(Vector2 screenPos)
    { 
        FingerCanvas.Instance.UpdateBrushColor();
     	FingerCanvas.Instance.SetVisible(true);
		FingerCanvas.Instance.SetNormalBrush(); 
		startCanvasPosition = FingerCanvas.Instance.GetCanvasPosition(screenPos);
		FingerCanvas.Instance.SetBrushPosition(screenPos);
		strokePoints.Add(screenPos);
    }

	override public void TouchMove(Vector2 screenPos)
    {
		FingerCanvas.Instance.SetBrushPosition(screenPos);
		strokePoints.Add(screenPos);
    }

	override public void TouchUp(Vector2 pos)
    {
		FingerCanvas.Instance.SetVisible(false);

		// Get the photo pixel buffer
		ColorBuffer32 picelBuffer = DecoratorPanel.Instance.GetPixelBuffer();

		// Grab render texture pixels
		RenderTexture renderTexture = FingerCanvas.Instance.renderTexture;
		RenderTexture.active = renderTexture;

		if (masksTexture == null)
			masksTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);

		masksTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
		masksTexture.Apply();

		if (masksPixelBuffer == null)
			masksPixelBuffer = new ColorBuffer32(masksTexture.width, masksTexture.height, masksTexture.GetPixels32());
		else
			masksPixelBuffer.data = masksTexture.GetPixels32();

       
        // Do a flood fill only when the stroke is really short, (Tapping)
        if (GetStrokeRadius(strokePoints) < 10)
            FloodFill(startCanvasPosition, maskColors[ColorsManager.Instance.GetCurrentColor()], 0.2f, 0.35f, 0.025f, picelBuffer, masksPixelBuffer);
        else
			Stroke(masksPixelBuffer, ColorsManager.Instance.GetCurrentColor());

        // Clear finger painting on mask's alpha channel
        for (int a = 0; a < masksPixelBuffer.data.Length; a++)
            masksPixelBuffer.data[a].a = 0;

        // Now set the modified pixels back to the masks texture
        masksTexture.SetPixels32(masksPixelBuffer.data);
        masksTexture.Apply();

        // Finally copy the modified masks texture back to the render texture
        Graphics.Blit(masksTexture, renderTexture);
 
        FingerCanvas.Instance.SaveUndo();

		strokePoints.Clear();
    }
    #endregion

    #region Class implementation
    static public void ReleaseMemory()
    {
    	DestroyImmediate(masksTexture);
		DestroyImmediate(masksPixelBuffer);
		Resources.UnloadUnusedAssets(); 
		System.GC.Collect();
    }

    private float GetStrokeRadius(List<Vector2> points)
    {
    	Vector2 average = Vector2.zero;
    	Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
    	Vector2 max = new Vector2(float.MinValue, float .MinValue);

    	for (int i = 0;  i < points.Count; i++)
    	{
    		Vector2 point = points[i];

    		min = new Vector2(Mathf.Min(point.x, min.x), Mathf.Min(point.y, min.y));
    		max = new Vector2(Mathf.Max(point.x, max.x), Mathf.Max(point.y, max.y));
    		average+= point;
    	}

    	average/= points.Count;

    	Vector2 size = new Vector2(max.x - min.x, max.y - min.y);

    	return Mathf.Max(size.x, size.y);
    }

	private void Stroke(ColorBuffer32 masksBuffer, int currentColor)
    {
    	    switch (currentColor)
    	    {
			    case 0: for (int i = 0; i < masksBuffer.data.Length; i++)
				    masksBuffer.data[i].r = (byte)Mathf.Clamp(masksBuffer.data[i].r + masksBuffer.data[i].a, 0, 255);
			    break;

			    case 1: for (int i = 0; i < masksBuffer.data.Length; i++)
					masksBuffer.data[i].g = (byte)Mathf.Clamp(masksBuffer.data[i].g + masksBuffer.data[i].a, 0, 255);
			    break;

			    case 2: for (int i = 0; i < masksBuffer.data.Length; i++)
					masksBuffer.data[i].b = (byte)Mathf.Clamp(masksBuffer.data[i].b + masksBuffer.data[i].a, 0, 255);
			    break;
    	    }
    }

    private void ProcessPixelHSV(int x, int y, ref Color32 currentColor, Color32 startColor, Color maskColor, float hueTolerance, float saturationTolerance, float valueTolerance, Queue<Point>openNodes, ColorBuffer32 pixelBuffer, ColorBuffer32 masksBuffer, ColorBuffer32 copyBmp)
    {
		Color32 c = copyBmp[x , y];
    	HSVColor currentHSV = HSVColor.FromRGBA(c.r / 255f, c.g / 255f, c.b / 255f, 1);
		HSVColor startHSV = HSVColor.FromRGBA(startColor.r / 255f, startColor.g / 255f, startColor.b / 255f, 1);
		HSVColor hsvDifference = new HSVColor(Mathf.Abs(currentHSV.h - startHSV.h), Mathf.Abs(currentHSV.s - startHSV.s), Mathf.Abs(currentHSV.v - startHSV.v));
  
		hueTolerance = .1f;
		saturationTolerance = .1f;//0.095f;
		valueTolerance = 1;
  
		if (hsvDifference.h <= hueTolerance && hsvDifference.s <= saturationTolerance && hsvDifference.v <= valueTolerance)
        {
         	copyBmp[x, y] = Color.black;
           	masksBuffer[x, y] = maskColor;
			openNodes.Enqueue(new Point(x, y, currentColor));
        }
    }
   

	private void ProcessPixelRGB(int x, int y, ref Color32 currentColor, Color32 startColor, Color maskColor, float hueTolerance, float saturationTolerance, float valueTolerance, Queue<Point>openNodes, ColorBuffer32 pixelBuffer, ColorBuffer32 masksBuffer, ColorBuffer32 copyBmp)
    {
		Color32 c = copyBmp[x , y];
		Color32 rgbDifference = new Color32((byte)Mathf.Abs(c.r - startColor.r), (byte)Mathf.Abs(c.g - startColor.g), (byte)Mathf.Abs(c.b - startColor.b), 255);
  
		float tolerance = 64;
  
		if (rgbDifference.r <= tolerance && rgbDifference.g <= tolerance && rgbDifference.b <= tolerance)
        {
         	copyBmp[x, y] = Color.black;
           	masksBuffer[x, y] = maskColor;
			openNodes.Enqueue(new Point(x, y, currentColor));
        }
    }

	private void ProcessPixel(int x, int y, ref Color32 currentColor, Color32 startColor, Color maskColor, float hueTolerance, float saturationTolerance, float valueTolerance, Queue<Point>openNodes, ColorBuffer32 pixelBuffer, ColorBuffer32 masksBuffer, ColorBuffer32 copyBmp)
    {
		Color32 c = copyBmp[x , y];
	
		if (ColorTest(c, currentColor, 1) == true)
        {
        	currentColor = c;
         	copyBmp[x, y] = Color.black;
           	masksBuffer[x, y] = maskColor;
			openNodes.Enqueue(new Point(x, y, currentColor));
        }
    }

	public static bool ColorTest(Color32 c1, Color32 c2, float tol) 
	{
        float diffRed   = Mathf.Abs(c1.r - c2.r);
        float diffGreen = Mathf.Abs(c1.g - c2.g);
        float diffBlue  = Mathf.Abs(c1.b - c2.b);
        //Those values you can just divide by the amount of difference saturations (255), and you will get the difference between the two.

        float pctDiffRed   = (float)diffRed   / 255;
        float pctDiffGreen = (float)diffGreen / 255;
        float pctDiffBlue  = (float)diffBlue  / 255;

        //After which you can just find the average color difference in percentage.
        float diffPercentage = (pctDiffRed + pctDiffGreen + pctDiffBlue) / 3 * 100;

        if(diffPercentage >= tol) 
        {
            return false;
        } 
        else 
        { 
			// Do HSV comparsion
			HSVColor color1 = HSVColor.FromRGBA(c1.r / 255f, c1.g / 255f, c1.b / 255f, 1);
			HSVColor color2 = HSVColor.FromRGBA(c2.r / 255f, c2.g / 255f, c2.b / 255f, 1);

			// Only for highly saturated colors
			if (Mathf.Abs(color1.s- color2.s) > 0.25f)
				return false;
        	
            return true;
        }
    }

	private void FloodFill(Vector2 startPos, Color maskColor, float hueTolerance, float saturationTolerance, float valueTolerance, ColorBuffer32 pixelBuffer, ColorBuffer32 masksBuffer)
    {
        Color32 startColor = pixelBuffer[(int)startPos.x, (int)startPos.y];

        Debug.Log(HSVColor.FromColor(startColor).ToString());

        Point start = new Point((int)startPos.x, (int)startPos.y, startColor);

        ColorBuffer32 copyBmp = new ColorBuffer32(pixelBuffer.width, pixelBuffer.height, (Color32[])pixelBuffer.data.Clone());

        copyBmp[start.x, start.y] = maskColor;

        Queue<Point> openNodes = new Queue<Point>();
        openNodes.Enqueue(start);

        int i = 0;
        int emergency = pixelBuffer.width * pixelBuffer.height;

        while (openNodes.Count > 0)
        {
            i++;

            if (i > emergency)
                return;

            Point current = openNodes.Dequeue();
            int x = current.x;
            int y = current.y;
            Color32 currentColor = current.color;
           
            if (x > 0)
                ProcessPixel(x - 1, y, ref currentColor, startColor, maskColor, hueTolerance, saturationTolerance, valueTolerance, openNodes, pixelBuffer, masksBuffer, copyBmp);
              
            if (x < pixelBuffer.width - 1)
                ProcessPixel(x + 1, y, ref currentColor, startColor, maskColor, hueTolerance, saturationTolerance, valueTolerance, openNodes, pixelBuffer, masksBuffer, copyBmp);
          
            if (y > 0)
                ProcessPixel(x, y - 1, ref currentColor, startColor, maskColor, hueTolerance, saturationTolerance, valueTolerance, openNodes, pixelBuffer, masksBuffer, copyBmp);
              
            if (y < pixelBuffer.height - 1) 
                ProcessPixel(x, y + 1, ref currentColor, startColor, maskColor, hueTolerance, saturationTolerance, valueTolerance, openNodes, pixelBuffer, masksBuffer, copyBmp);
        }
    }

    private struct Point
    {
        public int x;
        public int y;
        public Color32 color;

        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
            this.color = new Color32();
        }

        public Point(int x, int y, Color32 color)
        {
            this.x = x;
            this.y = y;
            this.color = color;
        }
    }
    #endregion
}