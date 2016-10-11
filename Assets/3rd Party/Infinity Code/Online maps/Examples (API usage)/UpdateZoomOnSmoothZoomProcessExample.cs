/*     INFINITY CODE 2013-2016      */
/*   http://www.infinity-code.com   */

using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example of how to make a map update zoom, when smooth zoom is processed.
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/UpdateZoomOnSmoothZoomProcessExample")]
    public class UpdateZoomOnSmoothZoomProcessExample : MonoBehaviour
    {
        private Vector3 originalPosition;

        private void Start()
        {
            // Subscribe to smooth zoom events
            OnlineMapsTileSetControl.instance.OnSmoothZoomBegin += OnSmoothZoomBegin;
            OnlineMapsTileSetControl.instance.OnSmoothZoomProcess += OnSmoothZoomProcess;
        }

        private void OnSmoothZoomBegin()
        {
            // Store original position
            originalPosition = OnlineMapsTileSetControl.instance.transform.position;
        }

        private void OnSmoothZoomProcess()
        {
            Transform t = OnlineMapsTileSetControl.instance.transform;

            Vector2 p1 = Input.GetTouch(0).position;
            Vector2 p2 = Input.GetTouch(1).position;

            Vector2 zoomPoint = Vector2.Lerp(p1, p2, 0.5f);

            while (t.localScale.x > 2 || t.localScale.x < 0.5)
            {
                // If localScale > 2, zoom in
                if (t.localScale.x > 2)
                {
                    // Zoom in
                    OnlineMapsControlBase.instance.ZoomOnPoint(1, zoomPoint);

                    // Update GameObject position and scale
                    t.position = (t.position - originalPosition) / 2 + originalPosition;
                    t.localScale /= 2;
                }
                // If localScale < 0.5, zoom out
                else if (t.localScale.x < 0.5)
                {
                    // Zoom out
                    OnlineMapsControlBase.instance.ZoomOnPoint(-1, zoomPoint);

                    // Update GameObject position and scale
                    t.position = (originalPosition - t.position) * 2 + t.position;
                    t.localScale *= 2;
                }
            }

        }
    }
}