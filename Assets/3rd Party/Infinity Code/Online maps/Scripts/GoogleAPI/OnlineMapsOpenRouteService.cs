/*     INFINITY CODE 2013-2016      */
/*   http://www.infinity-code.com   */

using System;
using System.Text;
using UnityEngine;

/// <summary>
/// This class is used to search for a route by coordinates using Open Route Service.\n
/// You can create a new instance using OnlineMapsOpenRouteService.Find.\n
/// http://wiki.openstreetmap.org/wiki/OpenRouteService
/// </summary>
public class OnlineMapsOpenRouteService: OnlineMapsGoogleAPIQuery
{
    /// <summary>
    /// Gets the type of query to API.
    /// </summary>
    public override OnlineMapsQueryType type
    {
        get { return OnlineMapsQueryType.ors; }
    }

    private OnlineMapsOpenRouteService(Vector2 start, Vector2 end, string lang, OnlineMapsOpenRouteServicePref pref, bool noMotorways, bool noTollways, Vector2[] via)
    {
        _status = OnlineMapsQueryStatus.downloading;

        StringBuilder url = new StringBuilder("http://openls.geog.uni-heidelberg.de/route?");
        url.Append("start=").Append(start.x).Append(",").Append(start.y);
        url.Append("&end=").Append(end.x).Append(",").Append(end.y);

        url.Append("&via=");
        if (via != null && via.Length > 0)
        {
            for (int i = 0; i < via.Length; i++)
            {
                url.Append(via[i].x).Append(",").Append(via[i].y);
                if (i < via.Length - 1) url.Append(" ");
            }
        }

        url.Append("&lang=").Append(lang);
        url.Append("&distunit=KM&routepref=").Append(Enum.GetName(typeof (OnlineMapsOpenRouteServicePref), pref));
        url.Append("&weighting=Shortest");
        url.Append("&avoidAreas=&useTMC=false&noMotorways=").Append(noMotorways);
        url.Append("&noTollways=").Append(noTollways).Append("&noUnpavedroads=false&noSteps=false&noFerries=false&instructions=true");

        Debug.Log(url.ToString());

        www = OnlineMapsUtils.GetWWW(url);
    }

    /// <summary>
    /// Creates a new request for a route search.
    /// </summary>
    /// <param name="start">Coordinates of the route begins.</param>
    /// <param name="end">Coordinates of the route ends.</param>
    /// <param name="lang">Language of intructions.</param>
    /// <param name="pref">The preference of the routing.</param>
    /// <param name="noMotorways">No Motorways.</param>
    /// <param name="noTollways">No Tollways.</param>
    /// <param name="via">Coordinates of the via positions.</param>
    /// <returns>Query instance.</returns>
    public static OnlineMapsOpenRouteService Find(Vector2 start, Vector2 end, string lang, OnlineMapsOpenRouteServicePref pref = OnlineMapsOpenRouteServicePref.Fastest, bool noMotorways = false, bool noTollways = false, Vector2[] via = null)
    {
        OnlineMapsOpenRouteService query = new OnlineMapsOpenRouteService(start, end, lang, pref, noMotorways, noTollways, via);
        OnlineMaps.instance.AddGoogleAPIQuery(query);
        return query;
    }

    /// <summary>
    /// The preference of the routing.
    /// </summary>
    public enum OnlineMapsOpenRouteServicePref
    {
        Fastest,
        Shortest,
        Pedestrian,
        Bicycle
    }
}