using UnityEngine;
using System.Collections;

public class PanAndZoomTool : DrawingToolBase
{
    private Vector2 prevPos;
    private Vector3 halfScreen = new Vector3(Screen.width / 2, Screen.height /2, 0);

    #region Class implementation
    override public void TouchDown(Vector2 pos)
    {
        prevPos = pos;
    }

    override public void TouchMove(Vector2 pos)
    {
        Vector3 delta = Camera.main.ScreenToWorldPoint(pos) - Camera.main.ScreenToWorldPoint(prevPos);
        Camera.main.transform.position -= delta;
        Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, Mathf.Clamp(Camera.main.transform.position.y,
                                                                                                    -Camera.main.orthographicSize * 0.5f,
                                                                                                     Camera.main.orthographicSize * 0.5f) ,0);
        prevPos = pos;
    }
    #endregion
}
