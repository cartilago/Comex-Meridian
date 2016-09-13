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

		// Get the photo hsv pixel buffer
		ColorBuffer hsvPixelBuffer = DecoratorPanel.Instance.GetHSVPixelBuffer();
        ColorBuffer32 rgbPixelBuffer = DecoratorPanel.Instance.GetRGBPixelBuffer();

		// Grab render texture pixels
		RenderTexture renderTexture = FingerCanvas.Instance.renderTexture;

		RenderTexture.active = renderTexture;
		Texture2D masksTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
		masksTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
		masksTexture.Apply();
		ColorBuffer masksPixelBuffer = new ColorBuffer(masksTexture.width, masksTexture.height, masksTexture.GetPixels());

       
        // Do a floof fill only when the stroke is really short, (Tapping)
        if (GetStrokeRadius(strokePoints) < 10)
        //FloodFill(hsvPixelBuffer, masksPixelBuffer, startCanvasPosition, maskColors[ColorsManager.Instance.GetCurrentColor()], 0.1f, 0.1f, 0.025f);
            FloodFill(startCanvasPosition, maskColors[ColorsManager.Instance.GetCurrentColor()], 0.2f, 0.35f, 0.025f, rgbPixelBuffer, hsvPixelBuffer, masksPixelBuffer);
         else
            FloodFillOnAlpha(hsvPixelBuffer, masksPixelBuffer, startCanvasPosition, maskColors[ColorsManager.Instance.GetCurrentColor()], 0.2f, 0.35f, 0.025f);
        //AddAlpha(masksPixelBuffer, ColorsManager.Instance.GetCurrentColor());

        // Clear finger painting on mask's alpha channel
        for (int a = 0; a < masksPixelBuffer.data.Length; a++)
            masksPixelBuffer.data[a].a = 0;

        // Now set the modified pixels back to the masks texture
        masksTexture.SetPixels(masksPixelBuffer.data);

        masksTexture.Apply();

        // Finally copy the modified masks texture back to the render texture
        Graphics.Blit(masksTexture, renderTexture);

        FingerCanvas.Instance.SaveUndo();


        strokePoints.Clear();
    }
    #endregion

    #region Class implementation
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

	private void AddAlpha(ColorBuffer masksBuffer, int currentColor)
    {
    	    switch (currentColor)
    	    {
		    case 0: for (int i = 0; i < masksBuffer.data.Length; i++)
			    masksBuffer.data[i].r = Mathf.Clamp01(masksBuffer.data[i].r + masksBuffer.data[i].a);
			    break;

		    case 1: for (int i = 0; i < masksBuffer.data.Length; i++)
			    masksBuffer.data[i].g = Mathf.Clamp01(masksBuffer.data[i].g + masksBuffer.data[i].a);
			    break;

		    case 2: for (int i = 0; i < masksBuffer.data.Length; i++)
			    masksBuffer.data[i].b = Mathf.Clamp01(masksBuffer.data[i].b + masksBuffer.data[i].a);
			    break;
    	    }
    }

    private void ProcessPixel(int x, int y, ref Color color, Color startColor, Color maskColor, float hueTolerance, float saturationTolerance, float valueTolerance, Queue<Point>openNodes, ColorBuffer32 RGBBuffer, ColorBuffer HSVBuffer, ColorBuffer masksBuffer, ColorBuffer copyBmp)
    {
        Color32 rgbColor = RGBBuffer[x, y];
        Color hsvColor = copyBmp[x , y];
        Color origColor = HSVBuffer[x, y];
        Color startColDifference = new Color(Mathf.Abs(origColor.r - startColor.r), Mathf.Abs(origColor.g - startColor.g), Mathf.Abs(origColor.b - startColor.b));

        if (Mathf.Abs(hsvColor.r - color.r) <= hueTolerance &&
            Mathf.Abs(hsvColor.g - color.g) <= saturationTolerance &&
            Mathf.Abs(hsvColor.b - color.b) <= valueTolerance &&
            startColDifference.g < 0.5f)
        {
            copyBmp[x, y] = Color.black; // maskColor;
            masksBuffer[x, y] = maskColor;
            openNodes.Enqueue(new Point(x, y, color));

            // Adaptiveness limit for highly saturated colors 
            if (startColor.g > 0.5f)
                color.g = Mathf.Clamp(hsvColor.g, startColor.g - 0.001f, 1);
            else
                // Adaptiveness limit for low saturated colors
                color.g = Mathf.Clamp(hsvColor.g, 0, startColor.g + 0.001f);

            color.b = hsvColor.b;
        }
    }


	private void FloodFill(Vector2 startPos, Color maskColor, float hueTolerance, float saturationTolerance, float valueTolerance, ColorBuffer32 RGBBuffer, ColorBuffer HSVBuffer, ColorBuffer masksBuffer)
    {
        Color startColor = HSVBuffer[(int)startPos.x, (int)startPos.y];
        Point start = new Point((int)startPos.x, (int)startPos.y, startColor);

        ColorBuffer copyBmp = new ColorBuffer(HSVBuffer.width, HSVBuffer.height, (Color[])HSVBuffer.data.Clone());

		int width =  HSVBuffer.width; 
        int height = HSVBuffer.height; 

        copyBmp[start.x, start.y] = maskColor;

        Queue<Point> openNodes = new Queue<Point>();
        openNodes.Enqueue(start);

        int i = 0;
        int emergency = width * height;

        while (openNodes.Count > 0)
        {
            i++;

            if (i > emergency)
                return;

            Point current = openNodes.Dequeue();
            int x = current.x;
            int y = current.y;
            Color color = current.color;
           
            if (x > 0)
                ProcessPixel(x - 1, y, ref color, startColor, maskColor, hueTolerance, saturationTolerance, valueTolerance, openNodes, RGBBuffer, HSVBuffer, masksBuffer, copyBmp);
              
            if (x < width - 1)
                ProcessPixel(x + 1, y, ref color, startColor, maskColor, hueTolerance, saturationTolerance, valueTolerance, openNodes, RGBBuffer, HSVBuffer, masksBuffer, copyBmp);
          
            if (y > 0)
                ProcessPixel(x, y - 1, ref color, startColor, maskColor, hueTolerance, saturationTolerance, valueTolerance, openNodes, RGBBuffer, HSVBuffer, masksBuffer, copyBmp);
              
            if (y < height - 1) 
                ProcessPixel(x, y + 1, ref color, startColor, maskColor, hueTolerance, saturationTolerance, valueTolerance, openNodes, RGBBuffer, HSVBuffer, masksBuffer, copyBmp);
        }
    }

    private void FloodFillOnAlpha(ColorBuffer HSVBuffer, ColorBuffer masksBuffer, Vector2 startPos, Color maskColor, float hueTolerance, float saturationTolerance, float valueTolerance)
    {
        Color startColor = HSVBuffer[(int)startPos.x, (int)startPos.y];
        Point start = new Point((int)startPos.x, (int)startPos.y, startColor);

        ColorBuffer copyBmp = new ColorBuffer(HSVBuffer.width, HSVBuffer.height, (Color[])HSVBuffer.data.Clone());

        int width = HSVBuffer.width;
        int height = HSVBuffer.height;

        copyBmp[start.x, start.y] = maskColor;

        Queue<Point> openNodes = new Queue<Point>();
        openNodes.Enqueue(start);

        int i = 0;
        int emergency = width * height;

        while (openNodes.Count > 0)
        {
            i++;

            if (i > emergency)
            {
                return;
            }

            Point current = openNodes.Dequeue();
            int x = current.x;
            int y = current.y;
            Color color = current.color;

            if (x > 0)
            {
                Color hsvColor = copyBmp[x - 1, y];

                Color origColor = HSVBuffer[x - 1, y];
                Color startColDifference = new Color(Mathf.Abs(origColor.r - startColor.r), Mathf.Abs(origColor.g - startColor.g), Mathf.Abs(origColor.b - startColor.b));

                if (masksBuffer[x-1, y].a != 0 &&
                    Mathf.Abs(hsvColor.r - color.r) <= hueTolerance &&
                    Mathf.Abs(hsvColor.g - color.g) <= saturationTolerance &&
                    Mathf.Abs(hsvColor.b - color.b) <= valueTolerance &&
                    startColDifference.g < 0.5f)
                {
                    copyBmp[x - 1, y] = Color.black; // maskColor;
                    masksBuffer[x - 1, y] = maskColor;
                    openNodes.Enqueue(new Point(x - 1, y, color));

                    // Adaptiveness limit for highly saturated colors 
                    if (startColor.g > 0.5f)
                        color.g = Mathf.Clamp(hsvColor.g, startColor.g - 0.001f, 1);
                    else
                        // Adaptiveness limit for low saturated colors
                        color.g = Mathf.Clamp(hsvColor.g, 0, startColor.g + 0.001f);

                    color.b = hsvColor.b;
                }
            }
            if (x < width - 1)
            {
                Color hsvColor = copyBmp[x + 1, y];

                Color origColor = HSVBuffer[x + 1, y];
                Color startColDifference = new Color(Mathf.Abs(origColor.r - startColor.r), Mathf.Abs(origColor.g - startColor.g), Mathf.Abs(origColor.b - startColor.b));

                if (masksBuffer[x + 1, y].a != 0 &&
                    Mathf.Abs(hsvColor.r - color.r) <= hueTolerance &&
                    Mathf.Abs(hsvColor.g - color.g) <= saturationTolerance &&
                    Mathf.Abs(hsvColor.b - color.b) <= valueTolerance &&
                    startColDifference.g < 0.5f)
                {
                    copyBmp[x + 1, y] = Color.black; // maskColor;
                    masksBuffer[x + 1, y] = maskColor;
                    openNodes.Enqueue(new Point(x + 1, y, color));

                    // Adaptiveness limit for highly saturated colors 
                    if (startColor.g > 0.5f)
                        color.g = Mathf.Clamp(hsvColor.g, startColor.g - 0.001f, 1);
                    else
                        // Adaptiveness limit for low saturated colors
                        color.g = Mathf.Clamp(hsvColor.g, 0, startColor.g + 0.001f);

                    color.b = hsvColor.b;
                }
            }
            if (y > 0)
            {
                Color hsvColor = copyBmp[x, y - 1];

                Color origColor = HSVBuffer[x, y - 1];
                Color startColDifference = new Color(Mathf.Abs(origColor.r - startColor.r), Mathf.Abs(origColor.g - startColor.g), Mathf.Abs(origColor.b - startColor.b));

                if (masksBuffer[x, y - 1].a != 0 &&
                    Mathf.Abs(hsvColor.r - color.r) <= hueTolerance &&
                    Mathf.Abs(hsvColor.g - color.g) <= saturationTolerance &&
                    Mathf.Abs(hsvColor.b - color.b) <= valueTolerance &&
                    startColDifference.g < 0.5f)
                {
                    copyBmp[x, y - 1] = Color.black; // maskColor;
                    masksBuffer[x, y - 1] = maskColor;
                    openNodes.Enqueue(new Point(x, y - 1, color));

                    // Adaptiveness limit for highly saturated colors 
                    if (startColor.g > 0.5f)
                        color.g = Mathf.Clamp(hsvColor.g, startColor.g - 0.001f, 1);
                    else
                        // Adaptiveness limit for low saturated colors
                        color.g = Mathf.Clamp(hsvColor.g, 0, startColor.g + 0.001f);

                    color.b = hsvColor.b;
                }
            }
            if (y < height - 1)
            {
                Color hsvColor = copyBmp[x, y + 1];

                Color origColor = HSVBuffer[x, y + 1];
                Color startColDifference = new Color(Mathf.Abs(origColor.r - startColor.r), Mathf.Abs(origColor.g - startColor.g), Mathf.Abs(origColor.b - startColor.b));

                if (masksBuffer[x, y + 1].a != 0 &&
                    Mathf.Abs(hsvColor.r - color.r) <= hueTolerance &&
                    Mathf.Abs(hsvColor.g - color.g) <= saturationTolerance &&
                    Mathf.Abs(hsvColor.b - color.b) <= valueTolerance &&
                    startColDifference.g < 0.5f)
                {
                    copyBmp[x, y + 1] = Color.black; // maskColor;
                    masksBuffer[x, y + 1] = maskColor;
                    openNodes.Enqueue(new Point(x, y + 1, color));

                    // Adaptiveness limit for highly saturated colors 
                    if (startColor.g > 0.5f)
                        color.g = Mathf.Clamp(hsvColor.g, startColor.g - 0.001f, 1);
                    else
                        // Adaptiveness limit for low saturated colors
                        color.g = Mathf.Clamp(hsvColor.g, 0, startColor.g + 0.001f);

                    color.b = hsvColor.b;
                }
            }
        }
    }

    private struct Point
    {
        public int x;
        public int y;
        public Color color;

        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
            this.color = Color.clear;
        }

        public Point(int x, int y, Color color)
        {
            this.x = x;
            this.y = y;
            this.color = color;
        }
    }
    #endregion
}