/*     INFINITY CODE 2013-2016      */
/*   http://www.infinity-code.com   */

#if !UNITY_4_3 && !UNITY_4_5

using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace InfinityCode.OnlineMapsExamples
{
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/uGUISmoothZoomExample")]
    public class uGUISmoothZoomExample : MonoBehaviour
    {
        public Action OnSmoothZoomBegin;
        public Action OnSmoothZoomFinish;
        public Action OnSmoothZoomProcess;

        private int touchCount;
        private bool smoothZoomStarted = false;
        private Graphic graphic;
        private RectTransform rectTransform;
        private Vector2 defPivot;
        private Vector2 defSize;
        private Vector2 defPosition;
        private Vector2 initialPosition;
        private Rect mapRect;
        private bool needRestore;
        private Vector2[] positions;

#if !UNITY_EDITOR
        private float initialDistance;
#endif

        private Camera worldCamera
        {
            get
            {
                if (graphic.canvas.renderMode == RenderMode.ScreenSpaceOverlay) return null;
                return graphic.canvas.worldCamera;
            }
        }

        private void LateUpdate()
        {
            int currentTouchCount = Input.touchCount;
            positions = Input.touches.Select(t => t.position).ToArray();

#if UNITY_EDITOR
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButton(0))
            {
                currentTouchCount = 2;

                if (initialPosition == Vector2.zero) initialPosition = Input.mousePosition;

                positions = new Vector2[2];
                positions[0] = Input.mousePosition;
                positions[1] = initialPosition - ((Vector2) Input.mousePosition - initialPosition);
            }
#endif

            if (currentTouchCount != touchCount)
            {
                touchCount = currentTouchCount;
                if (touchCount == 2) StartSmoothZoom(positions);
                else if (smoothZoomStarted) StopSmoothZoom();
            }

            UpdateMapCanvas();
        }

        private void RestoreSize()
        {
            rectTransform.pivot = defPivot;
            rectTransform.sizeDelta = defSize;
            rectTransform.anchoredPosition = defPosition;
            rectTransform.localScale = Vector3.one;

            if (needRestore)
            {
                needRestore = false;
                OnlineMaps.instance.OnMapUpdated -= RestoreSize;
            }
        }

        private void StopSmoothZoom()
        {
            int offset =
                Mathf.RoundToInt(rectTransform.localScale.x > 1
                    ? rectTransform.localScale.x - 1
                    : -1 / rectTransform.localScale.x + 1);

            smoothZoomStarted = false;

            if (offset != 0) OnlineMapsControlBase.instance.ZoomOnPoint(offset, initialPosition);
            if (OnSmoothZoomFinish != null) OnSmoothZoomFinish();

            needRestore = true;
            OnlineMaps.instance.OnMapUpdated += RestoreSize;

            initialPosition = Vector2.zero;
        }

        private void Start()
        {
            graphic = GetComponent<Graphic>();
            rectTransform = transform as RectTransform;
            OnlineMapsControlBase.instance.allowZoom = false;
        }

        private void StartSmoothZoom(Vector2[] positions)
        {
            smoothZoomStarted = true;

            if (needRestore) RestoreSize();

            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition,
                worldCamera, out localPoint);

            mapRect = rectTransform.rect;

            defPivot = rectTransform.pivot;
            defSize = rectTransform.sizeDelta;
            defPosition = rectTransform.anchoredPosition;

            initialPosition = (positions[0] + positions[1]) / 2;

            Vector2 pivot = new Vector2();
            float ox = localPoint.x + mapRect.width * defPivot.x;
            float oy = localPoint.y + mapRect.height * defPivot.y;
            pivot.x = ox / mapRect.width;
            pivot.y = oy / mapRect.height;

#if !UNITY_EDITOR
            initialDistance = (positions[0] - positions[1]).magnitude;
#endif

            Vector2 offsetMax = rectTransform.offsetMax;
            Vector2 offsetMin = rectTransform.offsetMin;
            rectTransform.pivot = pivot;
            rectTransform.offsetMax = offsetMax;
            rectTransform.offsetMin = offsetMin;

            OnlineMapsControlBase.instance.isMapDrag = false;
            if (OnSmoothZoomBegin != null) OnSmoothZoomBegin();
        }

        private void UpdateMapCanvas()
        {
            if (!smoothZoomStarted) return;

#if UNITY_EDITOR
            float a = (Input.mousePosition.x - initialPosition.x) / 100 + 1;
            if (a < 0.1f) a = 0.1f;
            rectTransform.localScale = new Vector3(a, a, 1);
#else
            float distance = (positions[0] - positions[1]).magnitude;
            float a = distance / initialDistance;
            rectTransform.localScale = new Vector3(a, a, 1);
#endif
            if (OnSmoothZoomProcess != null) OnSmoothZoomProcess();
        }
    }
}

#endif