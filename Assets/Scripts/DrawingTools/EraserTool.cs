using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EraserTool : DrawingToolBase 
{
	#region Class members
	public Camera canvasCamera;
	public Renderer canvasRenderer;
	public SpriteRenderer brushSprite;
	#endregion

	#region DrawingToolBase overrides
	override public void TouchDown(Vector2 screenPos)
    {
		FingerCanvas.Instance.SetupCanvas(); 
    	FingerCanvas.Instance.SetVisible(true);
		FingerCanvas.Instance.SaveUndo();
		FingerCanvas.Instance.SetEraserBrush(); 
		FingerCanvas.Instance.SetBrushPosition(screenPos);
		brushSprite.color = Color.black;
    }

	override public void TouchMove(Vector2 screenPos)
    {
		FingerCanvas.Instance.SetBrushPosition(screenPos);
    }

	override public void TouchUp(Vector2 pos)
    {
		FingerCanvas.Instance.SetVisible(false);
		/*
		// Get the photo hsv pixel buffer
		ColorBuffer hsvPixelBuffer = DecoratorPanel.Instance.GetHSVPixelBuffer();

		// Grab render texture pixels
		RenderTexture renderTexture = FingerCanvas.Instance.renderTexture;

		RenderTexture.active = renderTexture;
		Texture2D masksTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
		masksTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
		masksTexture.Apply();
		ColorBuffer masksPixelBuffer = new ColorBuffer(masksTexture.width, masksTexture.height, masksTexture.GetPixels());



		// Clear finger painting on mask's alpha channel
		for (int a = 0; a < masksPixelBuffer.data.Length; a++)
		{
			//if (Color 
			//masksPixelBuffer.data[a] = new Color(1 - a;
			masksPixelBuffer.data[a].a = 0;
		}

		// Now set the modified pixels back to the masks texture
		masksTexture.SetPixels(masksPixelBuffer.data);
		masksTexture.Apply();
		// Finally copy the modified masks texture back to the render texture
		Graphics.Blit(masksTexture, renderTexture);*/
	}
    #endregion
}