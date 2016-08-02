using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System;

public class FloodFillDrawingAction : DrawingActionBase, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    #region Class members
    #endregion
    #region DrawingToolBase overrides
    public FloodFillDrawingAction (Vector2 pos)
    {
        cachedTransform.position = pos;
        Debug.Log("New Floodfill Action " + pos);
    }

    public override void Apply()
    {
        Texture2D texture = Decorator.Instance.photoRnderer.material.mainTexture as Texture2D;
        Vector2 p = new Vector2((cachedTransform.position.x) + (texture.width * 0.5f), (texture.height - cachedTransform.position.y) - (texture.height * 0.5f));
        Texture2D tx = FloodFill.HSVFill(texture,(int) p.x, (int) p.y, Color.blue, .03f, 0.9f, 1);
        tx.Apply();
        Decorator.Instance.photoRnderer.material.SetTexture("_TintMask", tx);
        Debug.Log("Flood fill applied Width: " + texture.width + " Height " + texture.height + " pos " + p);
    }
    #endregion

    #region IBeginDragHandler
    public void OnBeginDrag(PointerEventData eventData)
    {
        cachedTransform.position = Camera.main.ScreenToWorldPoint(eventData.position);
    }
    #endregion

    #region IDragHandler
    public void OnDrag(PointerEventData eventData)
    {
        cachedTransform.position = Camera.main.ScreenToWorldPoint(eventData.position);
    }
    #endregion

    #region IDragHandler
    public void OnEndDrag(PointerEventData eventData)
    {
        cachedTransform.position = Camera.main.ScreenToWorldPoint(eventData.position);
        Apply();
    }
    #endregion

}
