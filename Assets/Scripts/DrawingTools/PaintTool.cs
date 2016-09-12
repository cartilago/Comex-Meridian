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
	//ConvolutionFilterBase blurFilter = new Gaussian5x5BlurFilter();
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
		Texture2D masksTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
		masksTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
		masksTexture.Apply();
		ColorBuffer masksPixelBuffer = new ColorBuffer(masksTexture.width, masksTexture.height, masksTexture.GetPixels());

		// Do a floof fill only when the stroke is really short, (Tapping)
		if (GetStrokeRadius(strokePoints) < 10)
			FloodFill(hsvPixelBuffer, masksPixelBuffer, startCanvasPosition, maskColors[ColorsManager.Instance.GetCurrentColor()], 0.1f, 0.5f, 0.5f);
		else
			FloodFillOnAlpha(hsvPixelBuffer, masksPixelBuffer, startCanvasPosition, maskColors[ColorsManager.Instance.GetCurrentColor()], 0.1f, 0.5f, 0.5f);
			//AddAlpha(masksPixelBuffer, ColorsManager.Instance.GetCurrentColor());

		// Clear finger painting on mask's alpha channel
		for (int a = 0; a < masksPixelBuffer.data.Length; a++)
			masksPixelBuffer.data[a].a = 0;

		// Now set the modified pixels back to the masks texture
		masksTexture.SetPixels(masksPixelBuffer.data);
		masksTexture.Apply();

		//blurFilter.Apply(masksTexture);

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

	private void FloodFillOnAlpha(ColorBuffer HSVBuffer, ColorBuffer masksBuffer, Vector2 startPos, Color maskColor, float hueTolerance, float saturationTolerance, float valueTolerance)
    {
        Point start = new Point((int)startPos.x, (int)startPos.y);

        ColorBuffer copyBmp = new ColorBuffer(HSVBuffer.width, HSVBuffer.height, (Color[])HSVBuffer.data.Clone());

        Color originalColor = HSVBuffer[start.X, start.Y]; 

        Debug.Log (originalColor);

		int width =  HSVBuffer.width; 
        int height = HSVBuffer.height; 

        Debug.Log ("Original color: " + originalColor);

        // Deal with highly saturated colors
		if (originalColor.g > 0.7f)
		{
			hueTolerance = 0.1f;
			saturationTolerance = 0.1f;
        	valueTolerance = 1;
			Debug.Log("Saturated color");
		}

		// Deal with whites
		if (originalColor.g < 0.05f /*&& originalColor.b > 0.8f*/)
		{
			hueTolerance = 1;
			saturationTolerance = 0.05f;
			valueTolerance = 0.05f;
			Debug.Log("White color");
		} 


        copyBmp[start.X, start.Y] = maskColor;

        Queue<Point> openNodes = new Queue<Point>();
        openNodes.Enqueue(start);

        int i = 0;

        // TODO: remove this
        // emergency switch so it doesn't hang if something goes wrong
        int emergency = width * height;

        while (openNodes.Count > 0)
        {
            i++;

            if (i > emergency)
            {
                return;
            }

            Point current = openNodes.Dequeue();
            int x = current.X;
            int y = current.Y;
           
            if (x > 0)
            {
				Color hsvColor = copyBmp[x - 1, y];

                if (masksBuffer[x - 1, y].a != 0 &&
					Mathf.Abs(hsvColor.r - originalColor.r) <= hueTolerance && 
					Mathf.Abs(hsvColor.g - originalColor.g) <= saturationTolerance && 
					Mathf.Abs(hsvColor.b - originalColor.b) <= valueTolerance)
                {
                    copyBmp[x - 1, y] = maskColor;
					masksBuffer[x -1, y] = maskColor;
                    openNodes.Enqueue(new Point(x - 1, y));
                }
            }
            if (x < width - 1)
            {
				Color hsvColor = copyBmp[x + 1, y];

				if (masksBuffer[x + 1, y].a != 0 &&
					Mathf.Abs(hsvColor.r - originalColor.r) <= hueTolerance &&
					Mathf.Abs(hsvColor.g - originalColor.g) <= saturationTolerance &&
					Mathf.Abs(hsvColor.b - originalColor.b) <= valueTolerance)
                {
                    copyBmp[x + 1, y] = maskColor;
					masksBuffer[x + 1, y] = maskColor;
                    openNodes.Enqueue(new Point(x + 1, y));
                }
            }
            if (y > 0)
            {
				Color hsvColor = copyBmp[x, y - 1];

				if (masksBuffer[x, y - 1].a != 0 &&
					Mathf.Abs(hsvColor.r - originalColor.r) <= hueTolerance &&
					Mathf.Abs(hsvColor.g - originalColor.g) <= saturationTolerance &&
					Mathf.Abs(hsvColor.b - originalColor.b) <= valueTolerance)
                {
                    copyBmp[x, y - 1] = maskColor;
					masksBuffer[x, y - 1] = maskColor;
                    openNodes.Enqueue(new Point(x, y - 1));
                }
            }
            if (y < height - 1) 
            {
				Color hsvColor = copyBmp[x, y + 1];

				if (masksBuffer[x, y + 1].a != 0 &&
					Mathf.Abs(hsvColor.r - originalColor.r) <= hueTolerance &&
					Mathf.Abs(hsvColor.g - originalColor.g) <= saturationTolerance &&
					Mathf.Abs(hsvColor.b - originalColor.b) <= valueTolerance)
                {
                    copyBmp[x, y + 1] = maskColor;
					masksBuffer[x, y + 1] = maskColor;
                    openNodes.Enqueue(new Point(x, y + 1));
                }
            }
        }
    }

	private void FloodFill(ColorBuffer HSVBuffer, ColorBuffer masksBuffer, Vector2 startPos, Color maskColor, float hueTolerance, float saturationTolerance, float valueTolerance)
    {
        Point start = new Point((int)startPos.x, (int)startPos.y);

        ColorBuffer copyBmp = new ColorBuffer(HSVBuffer.width, HSVBuffer.height, (Color[])HSVBuffer.data.Clone());

        Color originalColor = HSVBuffer[start.X, start.Y]; 

        Debug.Log (originalColor);

		int width =  HSVBuffer.width; 
        int height = HSVBuffer.height; 

        /*
		if (originalColor == maskColor)
        {
            return;
        }*/

        Debug.Log ("Original color: " + originalColor);

        // Deal with highly saturated colors
		if (originalColor.g > 0.7f)
		{
			hueTolerance = 0.1f;
			saturationTolerance = 0.1f;
        	valueTolerance = 1;
			Debug.Log("Saturated color");
		}

		// Deal with whites
		if (originalColor.g < 0.05f /*&& originalColor.b > 0.8f*/)
		{
			hueTolerance = 1;
			saturationTolerance = 0.05f;
			valueTolerance = 0.1f;
			Debug.Log("White color");
		} 


        copyBmp[start.X, start.Y] = maskColor;

        Queue<Point> openNodes = new Queue<Point>();
        openNodes.Enqueue(start);

        int i = 0;

        // TODO: remove this
        // emergency switch so it doesn't hang if something goes wrong
        int emergency = width * height;

        while (openNodes.Count > 0)
        {
            i++;

            if (i > emergency)
            {
                return;
            }

            Point current = openNodes.Dequeue();
            int x = current.X;
            int y = current.Y;
           
            if (x > 0)
            {
				Color hsvColor = copyBmp[x - 1, y];

                if (/*masksBuffer[x - 1, y] != maskColor &&*/
					Mathf.Abs(hsvColor.r - originalColor.r) <= hueTolerance && 
					Mathf.Abs(hsvColor.g - originalColor.g) <= saturationTolerance && 
					Mathf.Abs(hsvColor.b - originalColor.b) <= valueTolerance)
                {
                    copyBmp[x - 1, y] = maskColor;
					masksBuffer[x -1, y] = maskColor;
                    openNodes.Enqueue(new Point(x - 1, y));
                }
            }
            if (x < width - 1)
            {
				Color hsvColor = copyBmp[x + 1, y];

				if (/*masksBuffer[x + 1, y] != maskColor &&*/
					Mathf.Abs(hsvColor.r - originalColor.r) <= hueTolerance &&
					Mathf.Abs(hsvColor.g - originalColor.g) <= saturationTolerance &&
					Mathf.Abs(hsvColor.b - originalColor.b) <= valueTolerance)
                {
                    copyBmp[x + 1, y] = maskColor;
					masksBuffer[x + 1, y] = maskColor;
                    openNodes.Enqueue(new Point(x + 1, y));
                }
            }
            if (y > 0)
            {
				Color hsvColor = copyBmp[x, y - 1];

				if (/*masksBuffer[x, y - 1] != maskColor &&*/
					Mathf.Abs(hsvColor.r - originalColor.r) <= hueTolerance &&
					Mathf.Abs(hsvColor.g - originalColor.g) <= saturationTolerance &&
					Mathf.Abs(hsvColor.b - originalColor.b) <= valueTolerance)
                {
                    copyBmp[x, y - 1] = maskColor;
					masksBuffer[x, y - 1] = maskColor;
                    openNodes.Enqueue(new Point(x, y - 1));
                }
            }
            if (y < height - 1) 
            {
				Color hsvColor = copyBmp[x, y + 1];

				if (/*masksBuffer[x, y + 1] != maskColor &&*/
					Mathf.Abs(hsvColor.r - originalColor.r) <= hueTolerance &&
					Mathf.Abs(hsvColor.g - originalColor.g) <= saturationTolerance &&
					Mathf.Abs(hsvColor.b - originalColor.b) <= valueTolerance)
                {
                    copyBmp[x, y + 1] = maskColor;
					masksBuffer[x, y + 1] = maskColor;
                    openNodes.Enqueue(new Point(x, y + 1));
                }
            }
        }
    }

   
    /*
    unsafe void LinearFloodFill4( byte* scan0, int x, int y,Size bmpsize, int stride, byte* startcolor)
		{
			
			//offset the pointer to the point passed in
			int* p=(int*) (scan0+(CoordsToIndex(x,y, stride)));
			
			
			//FIND LEFT EDGE OF COLOR AREA
			int LFillLoc=x; //the location to check/fill on the left
			int* ptr=p; //the pointer to the current location
			while(true)
			{
				ptr[0]=m_fillcolor; 	 //fill with the color
				PixelsChecked[LFillLoc,y]=true;
				LFillLoc--; 		 	 //de-increment counter
				ptr-=1;				 	 //de-increment pointer
				if(LFillLoc<=0 || !CheckPixel((byte*)ptr,startcolor) ||  (PixelsChecked[LFillLoc,y]))
					break;			 	 //exit loop if we're at edge of bitmap or color area
				
			}
			LFillLoc++;
			
			//FIND RIGHT EDGE OF COLOR AREA
			int RFillLoc=x; //the location to check/fill on the left
			ptr=p;
			while(true)
			{
				ptr[0]=m_fillcolor; //fill with the color
				PixelsChecked[RFillLoc,y]=true;
				RFillLoc++; 		 //increment counter
				ptr+=1;				 //increment pointer
				if(RFillLoc>=bmpsize.Width || !CheckPixel((byte*)ptr,startcolor) ||  (PixelsChecked[RFillLoc,y]))
					break;			 //exit loop if we're at edge of bitmap or color area
				
			}
			RFillLoc--;
			
			
			//START THE LOOP UPWARDS AND DOWNWARDS			
			ptr=(int*)(scan0+CoordsToIndex(LFillLoc,y,stride));
			for(int i=LFillLoc;i<=RFillLoc;i++)
			{
				//START LOOP UPWARDS
				//if we're not above the top of the bitmap and the pixel above this one is within the color tolerance
				if(y>0 && CheckPixel((byte*)(scan0+CoordsToIndex(i,y-1,stride)),startcolor) && (!(PixelsChecked[i,y-1])))
					LinearFloodFill4(scan0, i,y-1,bmpsize,stride,startcolor);
				//START LOOP DOWNWARDS
				if(y<(bmpsize.Height-1) && CheckPixel((byte*)(scan0+CoordsToIndex(i,y+1,stride)),startcolor) && (!(PixelsChecked[i,y+1])))
					LinearFloodFill4(scan0, i,y+1,bmpsize,stride,startcolor);
				ptr+=1;
			}
			
		}
    */
   

	private struct Point
    {
        public int X;
        public int Y;

        public Point(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }
    #endregion
}