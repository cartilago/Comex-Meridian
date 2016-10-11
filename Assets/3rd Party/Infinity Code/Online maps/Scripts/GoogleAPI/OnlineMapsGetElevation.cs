/*     INFINITY CODE 2013-2016      */
/*   http://www.infinity-code.com   */

using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// The Elevation API provides elevation data for all locations on the surface of the earth, including depth locations on the ocean floor (which return negative values). \n
/// In those cases where Google does not possess exact elevation measurements at the precise location you request, the service will interpolate and return an averaged value using the four nearest locations.\n
/// With the Elevation API, you can develop hiking and biking applications, mobile positioning applications, or low resolution surveying applications. \n
/// https://developers.google.com/maps/documentation/elevation/
/// </summary>
public class OnlineMapsGetElevation:OnlineMapsGoogleAPIQuery
{
    public override OnlineMapsQueryType type
    {
        get { return OnlineMapsQueryType.elevation; }
    }

    private OnlineMapsGetElevation(Vector2 location, string key, string client, string signature)
    {
        _status = OnlineMapsQueryStatus.downloading;
        StringBuilder url = new StringBuilder("https://maps.googleapis.com/maps/api/elevation/xml?sensor=false&locations=").Append(location.y).Append(",").Append(location.x);
        if (!string.IsNullOrEmpty(key)) url.Append("&key=").Append(key);
        if (!string.IsNullOrEmpty(client)) url.Append("&client=").Append(client);
        if (!string.IsNullOrEmpty(signature)) url.Append("&signature=").Append(signature);
        www = OnlineMapsUtils.GetWWW(url);
    }

    private OnlineMapsGetElevation(Vector2[] locations, string key, string client, string signature)
    {
        _status = OnlineMapsQueryStatus.downloading;
        StringBuilder url = new StringBuilder("https://maps.googleapis.com/maps/api/elevation/xml?sensor=false&locations=");

        for (int i = 0; i < locations.Length; i++)
        {
            url.Append(locations[i].y).Append(",").Append(locations[i].x);
            if (i < locations.Length - 1) url.Append("|");
        }

        if (!string.IsNullOrEmpty(key)) url.Append("&key=").Append(key);
        if (!string.IsNullOrEmpty(client)) url.Append("&client=").Append(client);
        if (!string.IsNullOrEmpty(signature)) url.Append("&signature=").Append(signature);
        www = OnlineMapsUtils.GetWWW(url);
    }

    private OnlineMapsGetElevation(Vector2[] path, int samples, string key, string client, string signature)
    {
        _status = OnlineMapsQueryStatus.downloading;
        StringBuilder url = new StringBuilder();
        url.Append("https://maps.googleapis.com/maps/api/elevation/xml?sensor=false&path=");

        for (int i = 0; i < path.Length; i++)
        {
            url.Append(path[i].y).Append(",").Append(path[i].x);
            if (i < path.Length - 1) url.Append("|");
        }

        url.Append("&samples=").Append(samples);

        if (!string.IsNullOrEmpty(key)) url.Append("&key=").Append(key);
        if (!string.IsNullOrEmpty(client)) url.Append("&client=").Append(client);
        if (!string.IsNullOrEmpty(signature)) url.Append("&signature=").Append(signature);
        www = OnlineMapsUtils.GetWWW(url);
    }

    /// <summary>
    /// Get elevation value for single location.
    /// </summary>
    /// <param name="location">
    /// Location on the earth from which to return elevation data.
    /// </param>
    /// <param name="key">
    /// Your application's API key. \n
    /// This key identifies your application for purposes of quota management.
    /// </param>
    /// <param name="client">
    /// Client ID identifies you as a Maps API for Work customer and enables support and purchased quota for your application. \n
    /// Requests made without a client ID are not eligible for Maps API for Work benefits.
    /// </param>
    /// <param name="signature">
    /// Your welcome letter includes a cryptographic signing key, which you must use to generate unique signatures for your web service requests.
    /// </param>
    /// <returns>Query instance to the Google API.</returns>
    public static OnlineMapsGoogleAPIQuery Find(Vector2 location, string key = null, string client = null, string signature = null)
    {
        OnlineMapsGetElevation query = new OnlineMapsGetElevation(location, key, client, signature);
        OnlineMaps.instance.AddGoogleAPIQuery(query);
        return query;
    }

    /// <summary>
    /// Get elevation values for several locations.
    /// </summary>
    /// <param name="locations">
    /// Locations on the earth from which to return elevation data.
    /// </param>
    /// <param name="key">
    /// Your application's API key.\n
    /// This key identifies your application for purposes of quota management.
    /// </param>
    /// <param name="client">
    /// Client ID identifies you as a Maps API for Work customer and enables support and purchased quota for your application.\n
    /// Requests made without a client ID are not eligible for Maps API for Work benefits.
    /// </param>
    /// <param name="signature">
    /// Your welcome letter includes a cryptographic signing key, which you must use to generate unique signatures for your web service requests.
    /// </param>
    /// <returns>Query instance to the Google API.</returns>
    public static OnlineMapsGoogleAPIQuery Find(Vector2[] locations, string key = null, string client = null, string signature = null)
    {
        OnlineMapsGetElevation query = new OnlineMapsGetElevation(locations, key, client, signature);
        OnlineMaps.instance.AddGoogleAPIQuery(query);
        return query;
    }

    /// <summary>
    /// Get elevation values for path.
    /// </summary>
    /// <param name="path">Path on the earth for which to return elevation data. </param>
    /// <param name="samples">
    /// Specifies the number of sample points along a path for which to return elevation data. \n
    /// The samples parameter divides the given path into an ordered set of equidistant points along the path.
    /// </param>
    /// <param name="key">
    /// Your application's API key. \n
    /// This key identifies your application for purposes of quota management.
    /// </param>
    /// <param name="client">
    /// Client ID identifies you as a Maps API for Work customer and enables support and purchased quota for your application. \n
    /// Requests made without a client ID are not eligible for Maps API for Work benefits.
    /// </param>
    /// <param name="signature">
    /// Your welcome letter includes a cryptographic signing key, which you must use to generate unique signatures for your web service requests.
    /// </param>
    /// <returns>Query instance to the Google API.</returns>
    public static OnlineMapsGoogleAPIQuery Find(Vector2[] path, int samples, string key = null, string client = null, string signature = null)
    {
        OnlineMapsGetElevation query = new OnlineMapsGetElevation(path, samples, key, client, signature);
        OnlineMaps.instance.AddGoogleAPIQuery(query);
        return query;
    }

    /// <summary>
    /// Converts response into an array of results.
    /// </summary>
    /// <param name="response">Response of Google API.</param>
    /// <returns>Array of result.</returns>
    public static OnlineMapsGetElevationResult[] GetResults(string response)
    {
        OnlineMapsXML xml = OnlineMapsXML.Load(response);
        if (xml.isNull || xml.Get<string>("status") != "OK") return null;

        List<OnlineMapsGetElevationResult> rList = new List<OnlineMapsGetElevationResult>();
        foreach (OnlineMapsXML node in xml.FindAll("result")) rList.Add(new OnlineMapsGetElevationResult(node));

        return rList.ToArray();
    }
}