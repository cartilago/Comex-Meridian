/*     INFINITY CODE 2013-2016      */
/*   http://www.infinity-code.com   */

#if !UNITY_4_3 && !UNITY_4_5

using System.Collections.Generic;
using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/uGUICustomMarkerEngineExample")]
    public class uGUICustomMarkerEngineExample : MonoBehaviour 
    {
        private static uGUICustomMarkerEngineExample _instance;
        private static List<uGUICustomMarkerExample> markers;

        public RectTransform markerContainer;
        public GameObject markerPrefab;

        private GameObject container;
        private bool needUpdateMarkers;
        private Canvas canvas;
        private OnlineMaps api;

        public static uGUICustomMarkerEngineExample instance
        {
            get { return _instance; }
        }

        private Camera worldCamera
        {
            get
            {
                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay) return null;
                return canvas.worldCamera;
            }
        }

        public static uGUICustomMarkerExample AddMarker(Vector2 position, string text)
        {
            return AddMarker(position.x, position.y, text);
        }

        public static uGUICustomMarkerExample AddMarker(double lng, double lat, string text)
        {
            GameObject markerGameObject = Instantiate(_instance.markerPrefab) as GameObject;
            (markerGameObject.transform as RectTransform).SetParent(_instance.markerContainer);
            markerGameObject.transform.localScale = Vector3.one;
            uGUICustomMarkerExample marker = markerGameObject.GetComponent<uGUICustomMarkerExample>();
            marker.text = text;
            marker.lng = lng;
            marker.lat = lat;
            markers.Add(marker);
            _instance.UpdateMarker(marker);
            return marker;
        }

        private void OnEnable()
        {
            _instance = this;
            api = OnlineMaps.instance;
            markers = new List<uGUICustomMarkerExample>();
            canvas = markerContainer.GetComponentInParent<Canvas>();
        }

        public static void RemoveAllMarkers()
        {
            foreach (uGUICustomMarkerExample marker in markers)
            {
                marker.Dispose();
                OnlineMapsUtils.DestroyImmediate(marker.gameObject);
            }
            markers.Clear();
        }

        public static void RemoveMarker(uGUICustomMarkerExample marker)
        {
            OnlineMapsUtils.DestroyImmediate(marker.gameObject);
            marker.Dispose();
            markers.Remove(marker);
        }

        public static void RemoveMarkerAt(int index)
        {
            if (index < 0 || index >= markers.Count) return;

            uGUICustomMarkerExample marker = markers[index];
            OnlineMapsUtils.DestroyImmediate(marker.gameObject);
            marker.Dispose();
            markers.RemoveAt(index);
        }

        private void Start () 
        {
            OnlineMaps.instance.OnMapUpdated += UpdateMarkers;

            if (OnlineMapsControlBase.instance is OnlineMapsControlBase3D) OnlineMapsControlBase3D.instance.OnCameraControl += UpdateMarkers;

            AddMarker(new Vector2(), "12");
        }

        public void GetPosition(double lng, double lat, double tlx, double tly, out double px, out double py)
        {
            double dx, dy, dtx, dty;
            api.projection.CoordinatesToTile(lng, lat, api.buffer.apiZoom, out dx, out dy);
            api.projection.CoordinatesToTile(tlx, tly, api.buffer.apiZoom, out dtx, out dty);
            dx -= dtx;
            dy -= dty;
            int maxX = 1 << api.zoom;
            if (dx < maxX / -2) dx += maxX;
            px = dx * OnlineMapsUtils.tileSize;
            py = dy * OnlineMapsUtils.tileSize;
        }

        public Vector2 GetScreenPosition(double lng, double lat, double tlx, double tly)
        {
            double mx, my;
            GetPosition(lng, lat, tlx, tly, out mx, out my);
            mx /= api.width;
            my /= api.height;
            Rect mapRect = OnlineMapsControlBase.instance.GetRect();
            mx = mapRect.x + mapRect.width * mx;
            my = mapRect.y + mapRect.height - mapRect.height * my;
            return new Vector2((float)mx, (float)my);
        }

        private void UpdateMarkers()
        {
            foreach (uGUICustomMarkerExample marker in markers) UpdateMarker(marker);
        }

        private void UpdateMarker(uGUICustomMarkerExample marker)
        {
            double tlx, tly, brx, bry;

            int countX = api.width / OnlineMapsUtils.tileSize;
            int countY = api.height / OnlineMapsUtils.tileSize;

            double px, py;
            api.projection.CoordinatesToTile(api.buffer.apiPosition.x, api.buffer.apiPosition.y, api.buffer.apiZoom, out px, out py);

            px -= countX / 2f;
            py -= countY / 2f;

            api.projection.TileToCoordinates(px, py, api.buffer.apiZoom, out tlx, out tly);

            px += countX;
            py += countY;

            api.projection.TileToCoordinates(px, py, api.buffer.apiZoom, out brx, out bry);

            px = marker.lng;
            py = marker.lat;

            if (px < tlx || px > brx || py < bry || py > tly)
            {
                marker.gameObject.SetActive(false);
                return;
            }

            Vector2 screenPosition = GetScreenPosition(px, py, tlx, tly);

            RectTransform markerRectTransform = marker.transform as RectTransform;

            if (!marker.gameObject.activeSelf) marker.gameObject.SetActive(true);

            screenPosition.y += markerRectTransform.rect.height / 2;

            Vector2 point;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(markerRectTransform.parent as RectTransform, screenPosition, worldCamera, out point);
            markerRectTransform.localPosition = point;
        }
    }
}

#endif