/*     INFINITY CODE 2013-2016      */
/*   http://www.infinity-code.com   */

using UnityEngine;

namespace InfinityCode.OnlineMapsDemos
{
    [AddComponentMenu("Infinity Code/Online Maps/Demos/Demo")]
    public class Demo : MonoBehaviour
    {
        public Transform camera2D;
        public Transform camera3D;

        public Shader tileShader;

        public float CameraChangeTime = 1;

        private GUIStyle activeRowStyle;
        private float animValue;
        private OnlineMaps api;
        private OnlineMapsTileSetControl control;
        private bool is2D = true;
        private bool isCameraModeChange;
        private GUIStyle rowStyle;
        private string search = "";
        private OnlineMapsMarker searchMarker;

        private Transform fromTransform;
        private Transform toTransform;

        private void ChangeMode()
        {
            is2D = !is2D;

            animValue = 0;
            isCameraModeChange = true;

            Camera c = Camera.main;
            fromTransform = is2D ? camera3D : camera2D;
            toTransform = is2D ? camera2D : camera3D;

            c.orthographic = false;
            if (!is2D) c.fieldOfView = 28;
        }

        private void OnGUI()
        {
            if (api == null) api = OnlineMaps.instance;
            int labelFontSize = GUI.skin.label.fontSize;
            int buttonFontSize = GUI.skin.button.fontSize;
            int toggleFontSize = GUI.skin.toggle.fontSize;
            int textFieldFontSize = GUI.skin.textField.fontSize;
            GUI.skin.label.fontSize = 20;
            GUI.skin.button.fontSize = 20;
            GUI.skin.toggle.fontSize = 20;
            GUI.skin.toggle.contentOffset = new Vector2(5, -5);
            GUI.skin.textField.fontSize = 20;

            if (GUI.Button(new Rect(5, 5, 50, 50), is2D ? "3D" : "2D") && !isCameraModeChange)
            {
                ChangeMode();
            }

            if (rowStyle == null)
            {
                rowStyle = new GUIStyle(GUI.skin.button);
                RectOffset margin = rowStyle.margin;
                rowStyle.margin = new RectOffset(margin.left, margin.right, 1, 1);
            }

            if (activeRowStyle == null)
            {
                activeRowStyle = new GUIStyle(GUI.skin.button);
                activeRowStyle.normal.background = activeRowStyle.hover.background;
                RectOffset margin = activeRowStyle.margin;
                activeRowStyle.margin = new RectOffset(margin.left, margin.right, 1, 1);
            }

            if (GUI.Button(new Rect(5, 60, 50, 50), "+")) api.zoom++;

            Color defBackgroundColor = GUI.backgroundColor;

            for (int i = 20; i > 2; i--)
            {
                if (api.zoom == i) GUI.backgroundColor = Color.green;
                if (GUI.Button(new Rect(5, 115 + (20 - i) * 15, 50, 10), "")) api.zoom = i;
                GUI.backgroundColor = defBackgroundColor;
            }

            if (GUI.Button(new Rect(5, 390, 50, 50), "-")) api.zoom--;

            GUI.Box(new Rect(65, 5, Screen.width - 70, 75), "");

            GUI.Label(new Rect(75, 10, 150, 50), "Find place:");
            search = GUI.TextField(new Rect(200, 10, Screen.width - 320, 30), search);
            if (Event.current.type == EventType.KeyUp && (Event.current.keyCode == KeyCode.KeypadEnter || Event.current.keyCode == KeyCode.Return)) OnlineMapsFindLocation.Find(search).OnComplete += OnFindLocationComplete;
            if (GUI.Button(new Rect(Screen.width - 110, 10, 100, 30), "Search")) OnlineMapsFindLocation.Find(search).OnComplete += OnFindLocationComplete;

            GUI.Label(new Rect(75, 45, 100, 30), "Show:");

            api.labels = GUI.Toggle(new Rect(200, 50, 100, 30), api.labels, "Labels");
            api.traffic = GUI.Toggle(new Rect(300, 50, 100, 30), api.traffic, "Traffic");
            control.useElevation = !is2D && GUI.Toggle(new Rect(400, 50, 110, 30), control.useElevation, "Elevation");

            GUI.skin.label.fontSize = labelFontSize;
            GUI.skin.button.fontSize = buttonFontSize;
            GUI.skin.toggle.fontSize = toggleFontSize;
            GUI.skin.toggle.contentOffset = Vector2.zero;
            GUI.skin.textField.fontSize = textFieldFontSize;
        }

        private void OnFindLocationComplete(string result)
        {
            Vector2 position = OnlineMapsFindLocation.GetCoordinatesFromResult(result);

            if (position == Vector2.zero) return;

            if (searchMarker == null) searchMarker = api.AddMarker(position, search);
            else
            {
                searchMarker.position = position;
                searchMarker.label = search;
            }

            if (api.zoom < 13) api.zoom = 13;

            api.position = position;
            api.Redraw();
        }

        private void Start()
        {
            control = (OnlineMapsTileSetControl) OnlineMapsControlBase.instance;
            api = OnlineMaps.instance;
        }

        private void Update()
        {
            if (!isCameraModeChange) return;

            animValue += Time.deltaTime / CameraChangeTime;

            if (animValue > 1)
            {
                animValue = 1;
                isCameraModeChange = false;
            }

            Camera c = Camera.main;

            c.transform.position = Vector3.Lerp(fromTransform.position, toTransform.position, animValue);
            c.transform.rotation = Quaternion.Lerp(fromTransform.rotation, toTransform.rotation, animValue);

            float fromFOV = is2D ? 60 : 28;
            float toFOV = is2D ? 28 : 60;

            c.fieldOfView = Mathf.Lerp(fromFOV, toFOV, animValue);

            if (!isCameraModeChange && is2D) c.orthographic = true;
        }
    }
}