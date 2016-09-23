using UnityEngine;
using System.Collections;

public class PanAndZoomTool : DrawingToolBase
{
    #region Class members
    public float zoomSpeed = 0.5f;        // The rate of change of the orthographic size in orthographic mode.
    private bool zooming;
	private Vector2 prevPos;
    #endregion

    #region Class implementation
	override public void TouchDown(Vector2 pos)
    {
        prevPos = pos;
    }

	override public void TouchMove(Vector2 pos)
    {
    	if (zooming == true)
    		return;

        Vector3 delta = Camera.main.ScreenToWorldPoint(pos) - Camera.main.ScreenToWorldPoint(prevPos);

        DecoratorPanel.Instance.photoCamera.transform.position -= delta;
        /*Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, Mathf.Clamp(Camera.main.transform.position.y,
                                                                                                    -Camera.main.orthographicSize * 0.5f,
                                                                                                     Camera.main.orthographicSize * 0.5f) ,0);*/
        prevPos = pos;
    }

	override public void TouchUp(Vector2 pos)
	{
		zooming = false;
	}

    override public void Update()
	{
        // If there are two touches on the device...
        if (Input.touchCount == 2)
        {
         	zooming = true;
            // Store both touches.
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            // Find the position in the previous frame of each touch.
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            // Find the magnitude of the vector (the distance) between the touches in each frame.
            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            // Find the difference in the distances between each frame.
            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            // ... change the orthographic size based on the change in distance between the touches.
            // Make sure the orthographic size never drops below zero.
            DecoratorPanel.Instance.orthoSizeInterpolator.targetValue = Mathf.Clamp(DecoratorPanel.Instance.orthoSizeInterpolator.targetValue + (deltaMagnitudeDiff * zoomSpeed), 1, DecoratorPanel.Instance.GetPixelBuffer().height * 2);
        }
    }
	#endregion
}
