/*     INFINITY CODE 2013-2016      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// This class is used to search for a location by address.\n
/// You can create a new instance using OnlineMaps.FindLocation.\n
/// https://developers.google.com/maps/documentation/geocoding/intro
/// </summary>
public class OnlineMapsFindLocation : OnlineMapsGoogleAPIQuery
{
    /// <summary>
    /// Gets the type of query to Google API.
    /// </summary>
    /// <value>
    /// OnlineMapsQueryType.location
    /// </value>
    public override OnlineMapsQueryType type
    {
        get { return OnlineMapsQueryType.location; }
    }

    /// <summary>
    /// Constructor. \n
    /// <strong>Please do not use. </strong>\n
    /// Use OnlineMapsFindLocation.Find.
    /// </summary>
    /// <param name="address">Location title</param>
    /// <param name="latlng">Location coordinates (latitude,longitude). Example: 40.714224,-73.961452.</param>
    /// <param name="lang">Language of result</param>
    private OnlineMapsFindLocation(string address = null, string latlng = null, string lang = null)
    {
        _status = OnlineMapsQueryStatus.downloading;

        StringBuilder url = new StringBuilder("https://maps.googleapis.com/maps/api/geocode/xml?sensor=false");
        if (!string.IsNullOrEmpty(address)) url.Append("&address=").Append(OnlineMapsWWW.EscapeURL(address));
        if (!string.IsNullOrEmpty(latlng)) url.Append("&latlng=").Append(latlng.Replace(" ", ""));
        if (!string.IsNullOrEmpty(lang)) url.Append("&language=").Append(lang);
        www = OnlineMapsUtils.GetWWW(url);
    }

    /// <summary>
    /// Creates a new request for a location search.\n
    /// This method is used for Reverse Geocoding.\n
    /// https://developers.google.com/maps/documentation/geocoding/intro#Geocoding
    /// </summary>
    /// <param name="address">Location title</param>
    /// <param name="latlng">Location coordinates (latitude,longitude). Example: 40.714224,-73.961452.</param>
    /// <param name="lang">Language of result</param>
    /// <returns>Instance of the search query.</returns>
    public static OnlineMapsGoogleAPIQuery Find(string address = null, string latlng = null, string lang = null)
    {
        OnlineMapsFindLocation query = new OnlineMapsFindLocation(address, latlng, lang);
        OnlineMaps.instance.AddGoogleAPIQuery(query);
        return query;
    }

    /// <summary>
    /// Creates a new request for a location search.\n
    /// This method is used for Reverse Geocoding.\n
    /// https://developers.google.com/maps/documentation/geocoding/intro#ReverseGeocoding
    /// </summary>
    /// <param name="lnglat">Location coordinates</param>
    /// <param name="lang">Language of result</param>
    /// <returns>Instance of the search query.</returns>
    public static OnlineMapsGoogleAPIQuery Find(Vector2 lnglat, string lang = null)
    {
        OnlineMapsFindLocation query = new OnlineMapsFindLocation(null, string.Format("{0},{1}", lnglat.y, lnglat.x), lang);
        OnlineMaps.instance.AddGoogleAPIQuery(query);
        return query;
    }

    /// <summary>
    /// Gets the coordinates of the first results from OnlineMapsFindLocation result.
    /// </summary>
    /// <param name="response">XML string. The result of the search location.</param>
    /// <returns>Coordinates - if successful, Vector2.zero - if failed.</returns>
    public static Vector2 GetCoordinatesFromResult(string response)
    {
        try
        {
            OnlineMapsXML xml = OnlineMapsXML.Load(response);

            OnlineMapsXML location = xml.Find("//geometry/location");
            if (location.isNull) return Vector2.zero;

            return GetVector2FromNode(location);
        }
        catch { }
        return Vector2.zero;
    }

    /// <summary>
    /// Converts response into an array of results.
    /// </summary>
    /// <param name="response">Response of Google API.</param>
    /// <returns>Array of result.</returns>
    public static OnlineMapsFindLocationResult[] GetResults(string response)
    {
        try
        {
            OnlineMapsXML xml = OnlineMapsXML.Load(response);
            string status = xml.Find<string>("//status");
            if (status != "OK") return null;

            List<OnlineMapsFindLocationResult> results = new List<OnlineMapsFindLocationResult>();

            OnlineMapsXMLList resNodes = xml.FindAll("//result");

            foreach (OnlineMapsXML node in resNodes)
            {
                results.Add(new OnlineMapsFindLocationResult(node));
            }

            return results.ToArray();
        }
        catch (Exception exception)
        {
            Debug.Log(exception.Message + "\n" + exception.StackTrace);
        }

        return null;
    }

    /// <summary>
    /// Centers the map on the result of the search location.
    /// </summary>
    /// <param name="response">XML string. The result of the search location.</param>
    public static void MovePositionToResult(string response)
    {
        Vector2 position = GetCoordinatesFromResult(response);
        if (position != Vector2.zero) OnlineMaps.instance.position = position;
    }
}