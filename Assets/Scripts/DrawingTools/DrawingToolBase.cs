using UnityEngine;
using System.Collections;

/// <summary>
/// DrawingToolBase.
/// Base class for all drawing tools.
/// 
/// By Jorge L. Chavez Herrera.
/// </summary>
public class DrawingToolBase : MonoBehaviour
{
    public GameObject drawingActionPrefab;

    #region Class implementation
    virtual public void TouchDown(Vector2 pos) { }

    virtual public void TouchMove(Vector2 pos) { }

    virtual public void TouchUp(Vector2 pos) { }
    #endregion
}
