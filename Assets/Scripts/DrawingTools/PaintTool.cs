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

		// Get the photo hsv pixel buffer
		ColorBuffer hsvPixelBuffer = DecoratorPanel.Instance.GetHSVPixelBuffer();

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
            FloodFill(startCanvasPosition, maskColors[ColorsManager.Instance.GetCurrentColor()], 0.2f, 0.35f, 0.025f, hsvPixelBuffer, masksPixelBuffer);
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
    	Destroy(masksTexture);
    	Destroy(masksPixelBuffer);
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

    private void ProcessPixel(int x, int y, ref Color currentHSV, Color startHSV, Color maskColor, float hueTolerance, float saturationTolerance, float valueTolerance, Queue<Point>openNodes, ColorBuffer HSVBuffer, ColorBuffer32 masksBuffer, ColorBuffer copyBmp)
    {

        Color hsvColor = copyBmp[x , y];
        Color prevHSVDifference = new Color(Mathf.Abs(hsvColor.r - currentHSV.r), Mathf.Abs(hsvColor.g - currentHSV.g), Mathf.Abs(hsvColor.b - currentHSV.b));

        valueTolerance = .015f;

        if (prevHSVDifference.r <= hueTolerance && prevHSVDifference.g <= saturationTolerance && prevHSVDifference.b <= valueTolerance)
        {
            // Luminance adapting
            currentHSV.b = hsvColor.b;

         	copyBmp[x, y] = Color.black;
           	masksBuffer[x, y] = maskColor;
           	openNodes.Enqueue(new Point(x, y, currentHSV));
        }
    }

	private void FloodFill(Vector2 startPos, Color maskColor, float hueTolerance, float saturationTolerance, float valueTolerance, ColorBuffer HSVBuffer, ColorBuffer32 masksBuffer)
    {
        Color startHSV = HSVBuffer[(int)startPos.x, (int)startPos.y];
        Point start = new Point((int)startPos.x, (int)startPos.y, startHSV);

        ColorBuffer copyBmp = new ColorBuffer(HSVBuffer.width, HSVBuffer.height, (Color[])HSVBuffer.data.Clone());

        copyBmp[start.x, start.y] = maskColor;

        Queue<Point> openNodes = new Queue<Point>();
        openNodes.Enqueue(start);

        int i = 0;
        int emergency = HSVBuffer.width * HSVBuffer.height;

        while (openNodes.Count > 0)
        {
            i++;

            if (i > emergency)
                return;

            Point current = openNodes.Dequeue();
            int x = current.x;
            int y = current.y;
            Color currentHSV = current.color;
           
            if (x > 0)
                ProcessPixel(x - 1, y, ref currentHSV, startHSV, maskColor, hueTolerance, saturationTolerance, valueTolerance, openNodes, HSVBuffer, masksBuffer, copyBmp);
              
            if (x < HSVBuffer.width - 1)
                ProcessPixel(x + 1, y, ref currentHSV, startHSV, maskColor, hueTolerance, saturationTolerance, valueTolerance, openNodes, HSVBuffer, masksBuffer, copyBmp);
          
            if (y > 0)
                ProcessPixel(x, y - 1, ref currentHSV, startHSV, maskColor, hueTolerance, saturationTolerance, valueTolerance, openNodes, HSVBuffer, masksBuffer, copyBmp);
              
            if (y < HSVBuffer.height - 1) 
                ProcessPixel(x, y + 1, ref currentHSV, startHSV, maskColor, hueTolerance, saturationTolerance, valueTolerance, openNodes, HSVBuffer, masksBuffer, copyBmp);
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