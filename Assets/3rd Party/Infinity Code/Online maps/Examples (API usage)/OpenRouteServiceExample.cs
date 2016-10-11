/*     INFINITY CODE 2013-2016      */
/*   http://www.infinity-code.com   */

using System.Collections.Generic;
using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example of a request to Open Route Service.
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/OpenRouteServiceExample")]
    public class OpenRouteServiceExample : MonoBehaviour
    {
        private void Start()
        {
            // Looking for pedestrian route between the coordinates.
            OnlineMapsOpenRouteService.Find(new Vector2(8.6817521f, 49.4173462f), new Vector2(8.6828883f, 49.4067577f), "ru", OnlineMapsOpenRouteService.OnlineMapsOpenRouteServicePref.Pedestrian).OnComplete += OnRequestComplete;
        }

        /// <summary>
        /// This method is called when a response is received.
        /// </summary>
        /// <param name="response">Response string</param>
        private void OnRequestComplete(string response)
        {
            Debug.Log(response);

            // Get the route steps.
            List<OnlineMapsDirectionStep> steps = OnlineMapsDirectionStep.TryParseORS(response);

            // Get the route points.
            List<Vector2> points = OnlineMapsDirectionStep.GetPoints(steps);

            // Draw the route.
            OnlineMaps.instance.AddDrawingElement(new OnlineMapsDrawingLine(points, Color.red));

            // Set the map position to the first point of route.
            OnlineMaps.instance.position = points[0];
        }
    }
}