using UnityEngine;
using Meridian.Framework.Managers;

public class ZoomAndPan : MonoBehaviour
{
    #region Class members
    private float orthoZoomSpeed = 0.000005f; // The rate of change of the orthographic size in orthographic mode.
    private float zoomFactor = 1;
    #endregion

    #region MonoBehaviour overrides
    private void Update()
    {
        // Pan
        if (Input.touchCount == 1)
        {
            Touch touchZero = Input.GetTouch(0);
            Camera.main.transform.position -= new Vector3(touchZero.deltaPosition.x, touchZero.deltaPosition.y, 0) * zoomFactor * 2;
        }

        // Zoom
        if (Input.touchCount == 2)
        {
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

            // Change the orthographic size based on the change in distance between the touches.
            zoomFactor += deltaMagnitudeDiff * orthoZoomSpeed * Screen.dpi;
            // Make sure the zoomFactor never drops below 10% pixel.
            zoomFactor = Mathf.Max(zoomFactor, 0.1f);

            Camera.main.orthographicSize = Decorator.Instance.GetBaseOrthographicSize() * zoomFactor;
        }
    }
    #endregion
}