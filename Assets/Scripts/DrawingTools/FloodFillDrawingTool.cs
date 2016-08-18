using UnityEngine;
using System.Collections;

/// <summary>
/// FloodFillDrawingTool.
/// Base class for all drawing tools.
/// 
/// By Jorge L. Chavez Herrera.
/// </summary>
public class FloodFillDrawingTool : DrawingToolBase
{
    private FloodFillDrawingAction currentFloodFillDrawingAction;
    private Vector3 startPosition;

    #region Class implementation
    override public void TouchDown(Vector2 pos)
    {
        currentFloodFillDrawingAction = GameObject.Instantiate(drawingActionPrefab).GetComponent<FloodFillDrawingAction>();
        currentFloodFillDrawingAction.cachedTransform.position = Camera.main.ScreenToWorldPoint(pos);
        DecoratorPanel.Instance.GetCurrentProject().AddDrawingAction(currentFloodFillDrawingAction);
    }

    override public void TouchMove(Vector2 pos)
    {
        currentFloodFillDrawingAction.cachedTransform.position = Camera.main.ScreenToWorldPoint(pos);
    }

    override public void TouchUp(Vector2 pos)
    {
        currentFloodFillDrawingAction.cachedTransform.position = Camera.main.ScreenToWorldPoint(pos);
        currentFloodFillDrawingAction.Apply();
        currentFloodFillDrawingAction = null;
        toggle.isOn = false;
        DecoratorPanel.Instance.SetCurrentTool(DecoratorPanel.Instance.tools[0]);
    }
    #endregion
}
