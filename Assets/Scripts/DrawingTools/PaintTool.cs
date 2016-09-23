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
            StartCoroutine(FloodFill(startCanvasPosition, maskColors[ColorsManager.Instance.GetCurrentColor()], 0.2f, 0.35f, 0.025f, picelBuffer, masksPixelBuffer));
        else
        {
            Stroke(masksPixelBuffer, ColorsManager.Instance.GetCurrentColor());
        }
		strokePoints.Clear();
    }

    private void UpdateMaskPixels()
    {
        // Clear finger painting on mask's alpha channel
        for (int a = 0; a < masksPixelBuffer.data.Length; a++)
            masksPixelBuffer.data[a].a = 0;

        // Now set the modified pixels back to the masks texture
        masksTexture.SetPixels32(masksPixelBuffer.data);
        masksTexture.Apply();

        // Finally copy the modified masks texture back to the render texture
        Graphics.Blit(masksTexture, FingerCanvas.Instance.renderTexture);

        FingerCanvas.Instance.SaveUndo();
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

	private void ProcessPixel(int x, int y, ref Color32 currentColor, Color32 startColor, Color maskColor, float hueTolerance, float saturationTolerance, float valueTolerance, Queue<Point>openNodes, ColorBuffer32 pixelBuffer, ColorBuffer32 masksBuffer, ColorBuffer32 copyBmp)
    {
		Color32 pixel = copyBmp[x , y];
	
		if (ColorTest(pixel, currentColor, startColor, 1) == true)
        {
            	currentColor = pixel;
         	copyBmp[x, y] = Color.black;
           	masksBuffer[x, y] = maskColor;
			openNodes.Enqueue(new Point(x, y, currentColor));
        }
    }

	public static bool ColorTest(Color32 c1, Color32 c2, Color32 startColor, float tol) 
	{
        //Those values you can just divide by the amount of difference saturations (255), and you will get the difference between the two.
        float diffRed   = Mathf.Abs(c1.r - c2.r) / 255f;
        float diffGreen = Mathf.Abs(c1.g - c2.g) / 255f;
        float diffBlue  = Mathf.Abs(c1.b - c2.b) / 255f;
       
        //After which you can just find the average color difference in percentage.
        float diffPercentage = (diffRed + diffGreen + diffBlue) / 3 * 100;

        if(diffPercentage >= tol) 
        {
            return false;
        } 
        else 
        { 
			// Do HSV comparsion
			HSVColor color1 = HSVColor.FromRGBA(c1.r / 255f, c1.g / 255f, c1.b / 255f, 1);
			HSVColor color2 = HSVColor.FromRGBA(startColor.r / 255f, startColor.g / 255f, startColor.b / 255f, 1);

			// Only for highly saturated colors
			if (Mathf.Abs(color1.s- color2.s) > 0.5f)
				return false;
        	
            return true;
        }
    }

    public static bool ColorTestHSV(Color32 c1, Color32 c2, Color32 startColor, float tol)
    {
        //Those values you can just divide by the amount of difference saturations (255), and you will get the difference between the two.
        HSVColor hsv1 = HSVColor.FromRGBA(c1.r / 255f, c1.g / 255f, c1.b / 255f, 1);
        HSVColor hsv2 = HSVColor.FromRGBA(c2.r / 255f, c2.g / 255f, c2.b / 255f, 1);

        float diffHue = Mathf.Abs(hsv1.h - hsv2.h);
        float diffSat = Mathf.Abs(hsv1.s - hsv2.s);
        float diffVal = Mathf.Abs(hsv1.v - hsv2.v);

        // TODO

        return true;
    }

    private void FloodFillScanLine(Vector2 startPos, Color maskColor, float hueTolerance, float saturationTolerance, float valueTolerance, ColorBuffer32 pixelBuffer, ColorBuffer32 masksBuffer)
    {
        Color32 startColor = pixelBuffer[(int)startPos.x, (int)startPos.y];
        Color32 currentColor = startColor;
        Debug.Log(HSVColor.FromColor(startColor).ToString());

        Point start = new Point((int)startPos.x, (int)startPos.y, startColor);

        ColorBuffer32 copyBmp = new ColorBuffer32(pixelBuffer.width, pixelBuffer.height, (Color32[])pixelBuffer.data.Clone());

        copyBmp[start.x, start.y] = maskColor;

        Queue<Point> openNodes = new Queue<Point>();
        openNodes.Enqueue(start);

        while (openNodes.Count > 0)
        {
            Point current = openNodes.Dequeue();
            currentColor = current.color;

            for (int x = 0; x < pixelBuffer.width; x++)
            {
                Color32 pixel = copyBmp[x, current.y];

                if (ColorTest(pixel, currentColor, startColor, 1) == false || masksBuffer[x, current.y] == maskColor)
                    break;
                
                copyBmp[x, current.y] = Color.black;
                masksBuffer[x, current.y] = maskColor;
        /*
                // down
                if (current.y + 1 < pixelBuffer.height)
                {
                    pixel = copyBmp[x, current.y+1];

                    if (ColorTest(pixel, currentColor, startColor, 1) == true && masksBuffer[x, current.y+1] != maskColor)
                        openNodes.Enqueue(new Point(x, current.y + 1));
                }
        
                if (current.y - 1 >= 0)
                {
                    pixel = copyBmp[x, current.y - 1];

                    if (ColorTest(pixel, currentColor, startColor, 1) == true && masksBuffer[x, current.y-1] != maskColor)
                        openNodes.Enqueue(new Point(x, current.y - 1));
                }*/
            }

            for (int x = current.x -1; x >= 0; x--)
            {
                Color32 pixel = copyBmp[x, current.y];

                if (ColorTest(pixel, currentColor, startColor, 1) == false || masksBuffer[x, current.y] == maskColor)
                    break;

                copyBmp[x, current.y] = Color.black;
                masksBuffer[x, current.y] = maskColor;
                /*
                // down
                if (current.y + 1 < pixelBuffer.height)
                {
                    pixel = copyBmp[x, current.y + 1];

                    if (ColorTest(pixel, currentColor, startColor, 1) == true && masksBuffer[x, current.y + 1] != maskColor)
                        openNodes.Enqueue(new Point(x, current.y + 1));
                }

                if (current.y - 1 >= 0)
                {
                    pixel = copyBmp[x, current.y - 1];

                    if (ColorTest(pixel, currentColor, startColor, 1) == true && masksBuffer[x, current.y - 1] != maskColor)
                        openNodes.Enqueue(new Point(x, current.y - 1));
                }*/
            }
        }

       
    }



    IEnumerator FloodFill(Vector2 startPos, Color maskColor, float hueTolerance, float saturationTolerance, float valueTolerance, ColorBuffer32 pixelBuffer, ColorBuffer32 masksBuffer)
    {
        DecoratorPanel.Instance.progressIndicator.SetActive(true);
        yield return new WaitForEndOfFrame();
        yield return null;
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
                yield break;

            Point current = openNodes.Dequeue();
            int x = current.x;
            int y = current.y;
            Color32 currentColor = current.color;
           
            if (x > 0 && masksBuffer[x - 1, y] != maskColor)
                ProcessPixel(x - 1, y, ref currentColor, startColor, maskColor, hueTolerance, saturationTolerance, valueTolerance, openNodes, pixelBuffer, masksBuffer, copyBmp);
              
            if (x < pixelBuffer.width - 1 && masksBuffer[x+1,y] != maskColor)
                ProcessPixel(x + 1, y, ref currentColor, startColor, maskColor, hueTolerance, saturationTolerance, valueTolerance, openNodes, pixelBuffer, masksBuffer, copyBmp);
          
            if (y > 0 && masksBuffer[x, y-1] != maskColor)
                ProcessPixel(x, y - 1, ref currentColor, startColor, maskColor, hueTolerance, saturationTolerance, valueTolerance, openNodes, pixelBuffer, masksBuffer, copyBmp);
              
            if (y < pixelBuffer.height - 1 && masksBuffer[x, y + 1] != maskColor)
                ProcessPixel(x, y + 1, ref currentColor, startColor, maskColor, hueTolerance, saturationTolerance, valueTolerance, openNodes, pixelBuffer, masksBuffer, copyBmp);
        }

        UpdateMaskPixels();
        DecoratorPanel.Instance.progressIndicator.SetActive(false);
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