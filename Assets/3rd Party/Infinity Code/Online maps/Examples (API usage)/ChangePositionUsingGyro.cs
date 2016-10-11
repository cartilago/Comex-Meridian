﻿/*     INFINITY CODE 2013-2016      */
/*   http://www.infinity-code.com   */

using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example how to change the position of the map using the gyroscope.
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/ChangePositionUsingGyro")]
    public class ChangePositionUsingGyro : MonoBehaviour
    {
        /// <summary>
        /// Speed of moving the map.
        /// </summary>
        public float speed;

        private bool allowDrag;
        private OnlineMaps map;

        private void OnGUI()
        {
            GUIStyle style = new GUIStyle(GUI.skin.button);

            // If the button is pressed, allow movement of map.
            allowDrag = GUI.RepeatButton(new Rect(5, 5, 100, 100), "Drag", style);

            // Log gyroscope rotationRate.
            GUI.Label(new Rect(5, Screen.height - 50, Screen.width - 10, 50), Input.gyro.rotationRate.ToString("F4"));
        }

        private void Start()
        {
            // Forbid the user to control the map.
            OnlineMapsControlBase.instance.allowUserControl = false;

            // Turn on the gyro.
            Input.gyro.enabled = true;
            map = OnlineMaps.instance;
        }

        private void Update()
        {
            // If the movement is not allowed to return.
            if (!allowDrag) return;

            // Gets rotationRate
            Vector3 rate = Input.gyro.rotationRate;

            // Gets map position
            double lng, lat;
            map.GetPosition(out lng, out lat);

            // Converts the geographic coordinates to the tile coordinates.
            double tx, ty;
            map.projection.CoordinatesToTile(lng, lat, map.zoom, out tx, out ty);

            // Move tile coordinates
            tx += rate.x * speed;
            ty += rate.y * speed;

            // Converts the tile coordinates to the geographic coordinates.
            map.projection.TileToCoordinates(tx, ty, map.zoom, out lng, out lat);

            // Set map position
            map.SetPosition(lng, lat);
        }
    }
}