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
        FingerCanvas.Instance.SaveUndo();
        FingerCanvas.Instance.SetVisible(true);
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
	}
    #endregion
}