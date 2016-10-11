/*     INFINITY CODE 2013-2016      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// This class is used to search for a route with full props by address or coordinates.\n
/// You can create a new instance using OnlineMapsFindDirectionAdvanced.Find.\n
/// https://developers.google.com/maps/documentation/directions/intro
/// </summary>
public class OnlineMapsFindDirectionAdvanced:OnlineMapsGoogleAPIQuery
{
    public override OnlineMapsQueryType type
    {
        get { return OnlineMapsQueryType.direction; }
    }

    private OnlineMapsFindDirectionAdvanced(Params p)
    {
        _status = OnlineMapsQueryStatus.downloading;

        StringBuilder url = new StringBuilder();
        url.Append("https://maps.googleapis.com/maps/api/directions/xml?sensor=false");
        url.Append("&origin=");

        if (p.origin is string) url.Append(OnlineMapsWWW.EscapeURL(p.origin as string));
        else if (p.origin is Vector2)
        {
            Vector2 o = (Vector2)p.origin;
            url.Append(o.y).Append(",").Append(o.x);
        }
        else throw new Exception("Origin must be string or Vector2.");

        url.Append("&destination=");

        if (p.destination is string) url.Append(OnlineMapsWWW.EscapeURL(p.destination as string));
        else if (p.destination is Vector2)
        {
            Vector2 d = (Vector2)p.destination;
            url.Append(d.y).Append(",").Append(d.x);
        }
        else throw new Exception("Destination must be string or Vector2.");

        if (p.mode.HasValue && p.mode.Value != OnlineMapsFindDirectionMode.driving) url.Append("&mode=").Append(Enum.GetName(typeof(OnlineMapsFindDirectionMode), p.mode.Value));
        if (p.waypoints != null)
        {
            StringBuilder waypointStr = new StringBuilder();
            bool isFirst = true;
            int countWaypoints = 0;
            foreach (object w in p.waypoints)
            {
                if (countWaypoints >= 8)
                {
                    Debug.LogWarning("The maximum number of waypoints is 8.");
                    break;
                }

                if (!isFirst) waypointStr = waypointStr.Append("|");

                if (w is string) waypointStr.Append(OnlineMapsWWW.EscapeURL(w as string));
                else if (w is Vector2)
                {
                    Vector2 v = (Vector2)w;
                    waypointStr.Append(v.y).Append(",").Append(v.x);
                }
                else throw new Exception("Waypoints must be string or Vector2.");

                countWaypoints++;

                isFirst = false;
            }

            if (countWaypoints > 0) url.Append("&waypoints=optimize:true|").Append(waypointStr);
        }
        if (p.alternatives) url.Append("&alternatives=true");
        if (p.avoid.HasValue && p.avoid.Value != OnlineMapsFindDirectionAvoid.none) url.Append("&avoid=").Append(Enum.GetName(typeof(OnlineMapsFindDirectionAvoid), p.avoid.Value));
        if (p.units.HasValue && p.units.Value != OnlineMapsFindDirectionUnits.metric) url.Append("&units=").Append(Enum.GetName(typeof(OnlineMapsFindDirectionUnits), p.units.Value));
        if (!string.IsNullOrEmpty(p.region)) url.Append("&region=").Append(p.region);
        if (p.departure_time != null) url.Append("&departure_time=").Append(p.departure_time);
        if (p.arrival_time.HasValue && p.arrival_time.Value > 0) url.Append("&arrival_time=").Append(p.arrival_time.Value);
        if (!string.IsNullOrEmpty(p.language)) url.Append("&language=").Append(p.language);
        if (!string.IsNullOrEmpty(p.key)) url.Append("&key=").Append(p.key);
        if (p.traffic_model.HasValue && p.traffic_model.Value != TrafficModel.bestGuess) url.Append("&traffic_model=").Append(Enum.GetName(typeof(TrafficModel), p.traffic_model.Value));
        if (p.transit_mode.HasValue) GetValuesFromEnum(url, "transit_mode", typeof(TransitMode), (int)p.transit_mode.Value);
        if (p.transit_routing_preference.HasValue) url.Append("&transit_routing_preference=").Append(Enum.GetName(typeof(TransitRoutingPreference), p.transit_routing_preference.Value));

        www = OnlineMapsUtils.GetWWW(url);
    }

    /// <summary>
    /// Calculates directions between locations.\n
    /// You can search for directions for several modes of transportation, include transit, driving, walking or cycling. \n
    /// Directions may specify origins, destinations as latitude/longitude coordinates.
    /// </summary>
    /// <param name="origin">Latitude/longitude value from which you wish to calculate directions.</param>
    /// <param name="destination">Latitude/longitude value from which you wish to calculate directions.</param>
    /// <param name="mode">Specifies the mode of transport to use when calculating directions.</param>
    /// <param name="waypoints">
    /// Specifies an array of waypoints. \n
    /// Waypoints alter a route by routing it through the specified location(s). \n
    /// A waypoint is specified as either a latitude/longitude coordinate or as an address which will be geocoded. \n
    /// Waypoints are only supported for driving, walking and bicycling directions.
    /// </param>
    /// <param name="alternatives">
    /// If set to true, specifies that the Directions service may provide more than one route alternative in the response. \n
    /// Note that providing route alternatives may increase the response time from the server.
    /// </param>
    /// <param name="avoid">Indicates that the calculated route(s) should avoid the indicated features.</param>
    /// <param name="units">
    /// Specifies the unit system to use when displaying results.\n
    /// Note: this unit system setting only affects the text displayed within distance fields. The distance fields also contain values which are always expressed in meters.
    /// </param>
    /// <param name="region">Specifies the region code, specified as a ccTLD ("top-level domain") two-character value.</param>
    /// <param name="departure_time">
    /// Specifies the desired time of departure. \n
    /// You can specify the time as an integer in seconds since midnight, January 1, 1970 UTC. \n
    /// Alternatively, you can specify a value of now, which sets the departure time to the current time (correct to the nearest second).\n
    /// The departure time may be specified in two cases:\n
    /// For transit directions: You can optionally specify one of departure_time or arrival_time. If neither time is specified, the departure_time defaults to now (that is, the departure time defaults to the current time).\n
    /// For driving directions: Google Maps API for Work customers can specify the departure_time to receive trip duration considering current traffic conditions. The departure_time must be set to within a few minutes of the current time.
    /// </param>
    /// <param name="arrival_time">
    /// Specifies the desired time of arrival for transit directions, in seconds since midnight, January 1, 1970 UTC. \n
    /// You can specify either departure_time or arrival_time, but not both. \n
    /// Note that arrival_time must be specified as an integer.
    /// </param>
    /// <param name="language">
    /// Specifies the language in which to return results.\n
    /// Note that we often update supported languages so this list may not be exhaustive.\n
    /// If language is not supplied, the service will attempt to use the native language of the domain from which the request is sent.
    /// </param>
    /// <returns>Instance of the search query.</returns>
    public static OnlineMapsFindDirectionAdvanced Find(
        Vector2 origin, 
        Vector2 destination, 
        OnlineMapsFindDirectionMode mode = OnlineMapsFindDirectionMode.driving, 
        string[] waypoints = null, 
        bool alternatives = false, 
        OnlineMapsFindDirectionAvoid avoid = OnlineMapsFindDirectionAvoid.none, 
        OnlineMapsFindDirectionUnits units = OnlineMapsFindDirectionUnits.metric, 
        string region = null, 
        long departure_time = -1, 
        long arrival_time = -1,
        string language = null)
    {
        return Find(origin.y + "," + origin.x, destination.y + "," + destination.x, mode, waypoints, alternatives, avoid, units, region, departure_time, arrival_time, language);
    }

    /// <summary>
    /// Calculates directions between locations.\n
    /// You can search for directions for several modes of transportation, include transit, driving, walking or cycling. \n
    /// Directions may specify origins, destinations as latitude/longitude coordinates.
    /// </summary>
    /// <param name="origin">Latitude/longitude value from which you wish to calculate directions.</param>
    /// <param name="destination">The address from which you wish to calculate directions.</param>
    /// <param name="mode">Specifies the mode of transport to use when calculating directions.</param>
    /// <param name="waypoints">
    /// Specifies an array of waypoints. \n
    /// Waypoints alter a route by routing it through the specified location(s). \n
    /// A waypoint is specified as either a latitude/longitude coordinate or as an address which will be geocoded. \n
    /// Waypoints are only supported for driving, walking and bicycling directions.
    /// </param>
    /// <param name="alternatives">
    /// If set to true, specifies that the Directions service may provide more than one route alternative in the response. \n
    /// Note that providing route alternatives may increase the response time from the server.
    /// </param>
    /// <param name="avoid">Indicates that the calculated route(s) should avoid the indicated features.</param>
    /// <param name="units">
    /// Specifies the unit system to use when displaying results.\n
    /// Note: this unit system setting only affects the text displayed within distance fields. The distance fields also contain values which are always expressed in meters.
    /// </param>
    /// <param name="region">Specifies the region code, specified as a ccTLD ("top-level domain") two-character value.</param>
    /// <param name="departure_time">
    /// Specifies the desired time of departure. \n
    /// You can specify the time as an integer in seconds since midnight, January 1, 1970 UTC. \n
    /// Alternatively, you can specify a value of now, which sets the departure time to the current time (correct to the nearest second).\n
    /// The departure time may be specified in two cases:\n
    /// For transit directions: You can optionally specify one of departure_time or arrival_time. If neither time is specified, the departure_time defaults to now (that is, the departure time defaults to the current time).\n
    /// For driving directions: Google Maps API for Work customers can specify the departure_time to receive trip duration considering current traffic conditions. The departure_time must be set to within a few minutes of the current time.
    /// </param>
    /// <param name="arrival_time">
    /// Specifies the desired time of arrival for transit directions, in seconds since midnight, January 1, 1970 UTC. \n
    /// You can specify either departure_time or arrival_time, but not both. \n
    /// Note that arrival_time must be specified as an integer.
    /// </param>
    /// <param name="language">
    /// Specifies the language in which to return results.\n
    /// Note that we often update supported languages so this list may not be exhaustive.\n
    /// If language is not supplied, the service will attempt to use the native language of the domain from which the request is sent.
    /// </param>
    /// <returns>Instance of the search query.</returns>
    public static OnlineMapsFindDirectionAdvanced Find(
        Vector2 origin,
        string destination,
        OnlineMapsFindDirectionMode mode = OnlineMapsFindDirectionMode.driving,
        string[] waypoints = null,
        bool alternatives = false,
        OnlineMapsFindDirectionAvoid avoid = OnlineMapsFindDirectionAvoid.none,
        OnlineMapsFindDirectionUnits units = OnlineMapsFindDirectionUnits.metric,
        string region = null,
        long departure_time = -1,
        long arrival_time = -1,
        string language = null)
    {
        return Find(origin.y + "," + origin.x, destination, mode, waypoints, alternatives, avoid, units, region, departure_time, arrival_time, language);
    }

    /// <summary>
    /// Calculates directions between locations.\n
    /// You can search for directions for several modes of transportation, include transit, driving, walking or cycling. \n
    /// Directions may specify origins, destinations as latitude/longitude coordinates.
    /// </summary>
    /// <param name="origin">The address from which you wish to calculate directions.</param>
    /// <param name="destination">Latitude/longitude value from which you wish to calculate directions.</param>
    /// <param name="mode">Specifies the mode of transport to use when calculating directions.</param>
    /// <param name="waypoints">
    /// Specifies an array of waypoints. \n
    /// Waypoints alter a route by routing it through the specified location(s). \n
    /// A waypoint is specified as either a latitude/longitude coordinate or as an address which will be geocoded. \n
    /// Waypoints are only supported for driving, walking and bicycling directions.
    /// </param>
    /// <param name="alternatives">
    /// If set to true, specifies that the Directions service may provide more than one route alternative in the response. \n
    /// Note that providing route alternatives may increase the response time from the server.
    /// </param>
    /// <param name="avoid">Indicates that the calculated route(s) should avoid the indicated features.</param>
    /// <param name="units">
    /// Specifies the unit system to use when displaying results.\n
    /// Note: this unit system setting only affects the text displayed within distance fields. The distance fields also contain values which are always expressed in meters.
    /// </param>
    /// <param name="region">Specifies the region code, specified as a ccTLD ("top-level domain") two-character value.</param>
    /// <param name="departure_time">
    /// Specifies the desired time of departure. \n
    /// You can specify the time as an integer in seconds since midnight, January 1, 1970 UTC. \n
    /// Alternatively, you can specify a value of now, which sets the departure time to the current time (correct to the nearest second).\n
    /// The departure time may be specified in two cases:\n
    /// For transit directions: You can optionally specify one of departure_time or arrival_time. If neither time is specified, the departure_time defaults to now (that is, the departure time defaults to the current time).\n
    /// For driving directions: Google Maps API for Work customers can specify the departure_time to receive trip duration considering current traffic conditions. The departure_time must be set to within a few minutes of the current time.
    /// </param>
    /// <param name="arrival_time">
    /// Specifies the desired time of arrival for transit directions, in seconds since midnight, January 1, 1970 UTC. \n
    /// You can specify either departure_time or arrival_time, but not both. \n
    /// Note that arrival_time must be specified as an integer.
    /// </param>
    /// <param name="language">
    /// Specifies the language in which to return results.\n
    /// Note that we often update supported languages so this list may not be exhaustive.\n
    /// If language is not supplied, the service will attempt to use the native language of the domain from which the request is sent.
    /// </param>
    /// <returns>Instance of the search query.</returns>
    public static OnlineMapsFindDirectionAdvanced Find(
        string origin,
        Vector2 destination,
        OnlineMapsFindDirectionMode mode = OnlineMapsFindDirectionMode.driving,
        string[] waypoints = null,
        bool alternatives = false,
        OnlineMapsFindDirectionAvoid avoid = OnlineMapsFindDirectionAvoid.none,
        OnlineMapsFindDirectionUnits units = OnlineMapsFindDirectionUnits.metric,
        string region = null,
        long departure_time = -1,
        long arrival_time = -1,
        string language = null)
    {
        return Find(origin, destination.y + "," + destination.x, mode, waypoints, alternatives, avoid, units, region, departure_time, arrival_time, language);
    }

    /// <summary>
    /// Calculates directions between locations.\n
    /// You can search for directions for several modes of transportation, include transit, driving, walking or cycling. \n
    /// Directions may specify origins, destinations as latitude/longitude coordinates.
    /// </summary>
    /// <param name="origin">The address from which you wish to calculate directions.</param>
    /// <param name="destination">The address from which you wish to calculate directions.</param>
    /// <param name="mode">Specifies the mode of transport to use when calculating directions.</param>
    /// <param name="waypoints">
    /// Specifies an array of waypoints. \n
    /// Waypoints alter a route by routing it through the specified location(s). \n
    /// A waypoint is specified as either a latitude/longitude coordinate or as an address which will be geocoded. \n
    /// Waypoints are only supported for driving, walking and bicycling directions.
    /// </param>
    /// <param name="alternatives">
    /// If set to true, specifies that the Directions service may provide more than one route alternative in the response. \n
    /// Note that providing route alternatives may increase the response time from the server.
    /// </param>
    /// <param name="avoid">Indicates that the calculated route(s) should avoid the indicated features.</param>
    /// <param name="units">
    /// Specifies the unit system to use when displaying results.\n
    /// Note: this unit system setting only affects the text displayed within distance fields. The distance fields also contain values which are always expressed in meters.
    /// </param>
    /// <param name="region">Specifies the region code, specified as a ccTLD ("top-level domain") two-character value.</param>
    /// <param name="departure_time">
    /// Specifies the desired time of departure. \n
    /// You can specify the time as an integer in seconds since midnight, January 1, 1970 UTC. \n
    /// Alternatively, you can specify a value of now, which sets the departure time to the current time (correct to the nearest second).\n
    /// The departure time may be specified in two cases:\n
    /// For transit directions: You can optionally specify one of departure_time or arrival_time. If neither time is specified, the departure_time defaults to now (that is, the departure time defaults to the current time).\n
    /// For driving directions: Google Maps API for Work customers can specify the departure_time to receive trip duration considering current traffic conditions. The departure_time must be set to within a few minutes of the current time.
    /// </param>
    /// <param name="arrival_time">
    /// Specifies the desired time of arrival for transit directions, in seconds since midnight, January 1, 1970 UTC. \n
    /// You can specify either departure_time or arrival_time, but not both. \n
    /// Note that arrival_time must be specified as an integer.
    /// </param>
    /// <param name="language">
    /// Specifies the language in which to return results.\n
    /// Note that we often update supported languages so this list may not be exhaustive.\n
    /// If language is not supplied, the service will attempt to use the native language of the domain from which the request is sent.
    /// </param>
    /// <returns>Instance of the search query.</returns>
    public static OnlineMapsFindDirectionAdvanced Find(
        string origin,
        string destination,
        OnlineMapsFindDirectionMode mode = OnlineMapsFindDirectionMode.driving,
        string[] waypoints = null,
        bool alternatives = false,
        OnlineMapsFindDirectionAvoid avoid = OnlineMapsFindDirectionAvoid.none,
        OnlineMapsFindDirectionUnits units = OnlineMapsFindDirectionUnits.metric,
        string region = null,
        long departure_time = -1,
        long arrival_time = -1,
        string language = null)
    {
        OnlineMapsFindDirectionAdvanced query = new OnlineMapsFindDirectionAdvanced(new Params(origin, destination)
        {
            mode = mode,
            waypoints = waypoints != null? waypoints.Cast<object>().ToArray(): null,
            alternatives = alternatives,
            avoid = avoid,
            units = units,
            region = region,
            departure_time = departure_time > 0? (object)departure_time: null,
            arrival_time = arrival_time > 0? (long?)arrival_time: null,
            language = language
        });
        OnlineMaps.instance.AddGoogleAPIQuery(query);
        return query;
    }

    public static OnlineMapsFindDirectionAdvanced Find(Params p)
    {
        OnlineMapsFindDirectionAdvanced query = new OnlineMapsFindDirectionAdvanced(p);
        OnlineMaps.instance.AddGoogleAPIQuery(query);
        return query;
    }

    public static OnlineMapsFindDirectionResult GetResult(string response)
    {
        try
        {
            OnlineMapsXML xml = OnlineMapsXML.Load(response);
            return new OnlineMapsFindDirectionResult(xml);
        }
        catch { }

        return null;
    }

    private void GetValuesFromEnum(StringBuilder builder, string key, Type type, int value)
    {
        builder.Append("&").Append(key).Append("=");
        Array values = Enum.GetValues(type);

        bool addSeparator = false;
        for (int i = 0; i < values.Length; i++)
        {
            int v = (int)values.GetValue(i);
            if ((value & v) == v)
            {
                if (addSeparator) builder.Append(",");
                builder.Append(Enum.GetName(type, v));
                addSeparator = true;
            }
        }
    }

    /// <summary>
    /// Request parameters.
    /// </summary>
    public class Params
    {
        /// <summary>
        /// The address (string), coordinates (Vector2), or place ID (string prefixed with place_id:) from which you wish to calculate directions.
        /// </summary>
        public object origin;

        /// <summary>
        /// The address (string), coordinates (Vector2), or place ID (string prefixed with place_id:) to which you wish to calculate directions.
        /// </summary>
        public object destination;

        /// <summary>
        /// Specifies the mode of transport to use when calculating directions. Default: driving.
        /// </summary>
        public OnlineMapsFindDirectionMode? mode;

        /// <summary>
        /// Specifies an IEnumerate of waypoints. Waypoints alter a route by routing it through the specified location(s). \n
        /// The maximum number of waypoints is 8. \n
        /// Each waypoint can be specified as a coordinates (Vector2), an encoded polyline (string prefixed with enc:), a place ID (string prefixed with place_id:), or an address which will be geocoded. 
        /// </summary>
        public IEnumerable waypoints;

        /// <summary>
        /// If set to true, specifies that the Directions service may provide more than one route alternative in the response. \n
        /// Note that providing route alternatives may increase the response time from the server.
        /// </summary>
        public bool alternatives;

        /// <summary>
        /// Indicates that the calculated route(s) should avoid the indicated features.
        /// </summary>
        public OnlineMapsFindDirectionAvoid? avoid;

        /// <summary>
        /// Specifies the unit system to use when displaying results.
        /// </summary>
        public OnlineMapsFindDirectionUnits? units;

        /// <summary>
        /// Specifies the region code, specified as a ccTLD ("top-level domain") two-character value. 
        /// </summary>
        public string region;

        /// <summary>
        /// Specifies the desired time of arrival for transit directions, in seconds since midnight, January 1, 1970 UTC. \n
        /// You can specify either departure_time or arrival_time, but not both. 
        /// </summary>
        public long? arrival_time;

        /// <summary>
        /// Specifies the language in which to return results.
        /// </summary>
        public string language;

        /// <summary>
        /// Your application's API key. This key identifies your application for purposes of quota management.
        /// </summary>
        public string key;

        /// <summary>
        /// Specifies the assumptions to use when calculating time in traffic.
        /// </summary>
        public TrafficModel? traffic_model;

        /// <summary>
        /// Specifies one or more preferred modes of transit. This parameter may only be specified for transit directions.
        /// </summary>
        public TransitMode? transit_mode;

        /// <summary>
        /// Specifies preferences for transit routes. Using this parameter, you can bias the options returned, rather than accepting the default best route chosen by the API. \n
        /// This parameter may only be specified for transit directions.
        /// </summary>
        public TransitRoutingPreference? transit_routing_preference;

        private object _departure_time;

        /// <summary>
        /// Specifies the desired time of departure. You can specify the time as an integer in seconds since midnight, January 1, 1970 UTC. \n
        /// Alternatively, you can specify a value of now, which sets the departure time to the current time.
        /// </summary>
        public object departure_time
        {
            get { return _departure_time; }
            set { _departure_time = value; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="origin">The address (string), coordinates (Vector2), or place ID (string prefixed with place_id:) from which you wish to calculate directions.</param>
        /// <param name="destination">The address (string), coordinates (Vector2), or place ID (string prefixed with place_id:) to which you wish to calculate directions.</param>
        public Params(object origin, object destination)
        {
            if (!(origin is string || origin is Vector2)) throw new Exception("Origin must be string or Vector2.");
            if (!(destination is string || destination is Vector2)) throw new Exception("Destination must be string or Vector2.");

            this.origin = origin;
            this.destination = destination;
        }   
    }

    /// <summary>
    /// Specifies the assumptions to use when calculating time in traffic.
    /// </summary>
    public enum TrafficModel
    {
        /// <summary>
        /// Indicates that the returned duration_in_traffic should be the best estimate of travel time given what is known about both historical traffic conditions and live traffic.
        /// </summary>
        bestGuess,

        /// <summary>
        /// Indicates that the returned duration_in_traffic should be longer than the actual travel time on most days, though occasional days with particularly bad traffic conditions may exceed this value. 
        /// </summary>
        pessimistic,

        /// <summary>
        /// Indicates that the returned duration_in_traffic should be shorter than the actual travel time on most days, though occasional days with particularly good traffic conditions may be faster than this value. 
        /// </summary>
        optimistic
    }

    /// <summary>
    /// Specifies one or more preferred modes of transit.
    /// </summary>
    [Flags]
    public enum TransitMode
    {
        /// <summary>
        /// Indicates that the calculated route should prefer travel by bus.
        /// </summary>
        bus = 1,

        /// <summary>
        /// Indicates that the calculated route should prefer travel by subway.
        /// </summary>
        subway = 2,

        /// <summary>
        /// Indicates that the calculated route should prefer travel by train.
        /// </summary>
        train = 4,

        /// <summary>
        /// Indicates that the calculated route should prefer travel by tram and light rail.
        /// </summary>
        tram = 8,

        /// <summary>
        /// Indicates that the calculated route should prefer travel by train, tram, light rail, and subway. This is equivalent to train|tram|subway.
        /// </summary>
        rail = 16
    }

    /// <summary>
    /// Specifies preferences for transit routes.
    /// </summary>
    public enum TransitRoutingPreference
    {
        /// <summary>
        /// Indicates that the calculated route should prefer limited amounts of walking.
        /// </summary>
        lessWalking,

        /// <summary>
        /// Indicates that the calculated route should prefer a limited number of transfers.
        /// </summary>
        fewerTransfers
    }
}

/// <summary>
/// Mode of transport to use when calculating directions.
/// </summary>
public enum OnlineMapsFindDirectionMode
{
    /// <summary>
    /// Indicates standard driving directions using the road network.
    /// </summary>
    driving,

    /// <summary>
    /// Requests walking directions via pedestrian paths & sidewalks (where available).
    /// </summary>
    walking,

    /// <summary>
    /// Requests bicycling directions via bicycle paths & preferred streets (where available).
    /// </summary>
    bicycling,

    /// <summary>
    /// Requests directions via public transit routes (where available). \n
    /// If you set the mode to transit, you can optionally specify either a departure_time or an arrival_time. \n
    /// If neither time is specified, the departure_time defaults to now (that is, the departure time defaults to the current time). 
    /// </summary>
    transit
}

/// <summary>
/// Indicates that the calculated route(s) should avoid the indicated features.
/// </summary>
public enum OnlineMapsFindDirectionAvoid
{
    /// <summary>
    /// None avoid.
    /// </summary>
    none,

    /// <summary>
    /// Indicates that the calculated route should avoid toll roads/bridges.
    /// </summary>
    tolls,

    /// <summary>
    /// Indicates that the calculated route should avoid highways.
    /// </summary>
    highways,

    /// <summary>
    /// Indicates that the calculated route should avoid ferries.
    /// </summary>
    ferries
}

/// <summary>
/// Specifies the unit system to use when displaying results. 
/// </summary>
public enum OnlineMapsFindDirectionUnits
{
    /// <summary>
    /// Specifies usage of the metric system. Textual distances are returned using kilometers and meters.
    /// </summary>
    metric,

    /// <summary>
    /// Specifies usage of the Imperial (English) system. Textual distances are returned using miles and feet.
    /// </summary>
    imperial
}