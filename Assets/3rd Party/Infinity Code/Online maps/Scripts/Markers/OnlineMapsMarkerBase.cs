/*     INFINITY CODE 2013-2016      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The base class for markers.
/// </summary>
[Serializable]
public class OnlineMapsMarkerBase
{
    /// <summary>
    /// Default event caused to draw tooltip.
    /// </summary>
    [NonSerialized]
    public static Action<OnlineMapsMarkerBase> OnMarkerDrawTooltip;

    /// <summary>
    /// Events that occur when user click on the marker.
    /// </summary>
    [NonSerialized]
    public Action<OnlineMapsMarkerBase> OnClick;

    /// <summary>
    /// Events that occur when user double click on the marker.
    /// </summary>
    [NonSerialized]
    public Action<OnlineMapsMarkerBase> OnDoubleClick;

    /// <summary>
    /// Events that occur when user drag the marker.
    /// </summary>
    [NonSerialized]
    public Action<OnlineMapsMarkerBase> OnDrag;

    /// <summary>
    /// Event caused to draw tooltip.
    /// </summary>
    [NonSerialized]
    public Action<OnlineMapsMarkerBase> OnDrawTooltip;

    /// <summary>
    /// Event occurs when the marker enabled change.
    /// </summary>
    [NonSerialized]
    public Action<OnlineMapsMarkerBase> OnEnabledChange;

    /// <summary>
    /// Events that occur when user long press on the marker.
    /// </summary>
    [NonSerialized]
    public Action<OnlineMapsMarkerBase> OnLongPress;

    /// <summary>
    /// Events that occur when user press on the marker.
    /// </summary>
    [NonSerialized]
    public Action<OnlineMapsMarkerBase> OnPress;

    /// <summary>
    /// Events that occur when user release on the marker.
    /// </summary>
    [NonSerialized]
    public Action<OnlineMapsMarkerBase> OnRelease;

    /// <summary>
    /// Events that occur when user roll out marker.
    /// </summary>
    [NonSerialized]
    public Action<OnlineMapsMarkerBase> OnRollOut;

    /// <summary>
    /// Events that occur when user roll over marker.
    /// </summary>
    [NonSerialized]
    public Action<OnlineMapsMarkerBase> OnRollOver;

    /// <summary>
    /// In this variable you can put any data that you need to work with markers.
    /// </summary>
    public object customData;

    /// <summary>
    /// Marker label.
    /// </summary>
    public string label = "";

    /// <summary>
    /// Zoom range, in which the marker will be displayed.
    /// </summary>
    public OnlineMapsRange range;

    [SerializeField]
    protected bool _enabled = true;

    [SerializeField]
    protected double latitude;

    [SerializeField]
    protected double longitude;

    [SerializeField]
    protected float _scale = 1;

    /// <summary>
    /// Gets or sets marker enabled.
    /// </summary>
    /// <value>
    /// true if enabled, false if not.
    /// </value>
    public virtual bool enabled
    {
        get { return _enabled; }
        set
        {
            if (_enabled != value)
            {
                _enabled = value;
                if (OnEnabledChange != null) OnEnabledChange(this);
            }
        }
    }

    /// <summary>
    /// Marker coordinates.
    /// </summary>
    public Vector2 position
    {
        get
        {
            return new Vector2((float)longitude, (float)latitude);
        }
        set
        {
            longitude = value.x;
            latitude = value.y;
        }
    }

    /// <summary>
    /// Scale of marker.
    /// </summary>
    public virtual float scale
    {
        get { return _scale; }
        set { _scale = value; }
    }

    /// <summary>
    /// Checks to display marker in current map view.
    /// </summary>
    public virtual bool inMapView
    {
        get
        {
            if (!enabled) return false;

            OnlineMaps api = OnlineMaps.instance;

            if (!range.InRange(api.zoom)) return false;

            double tlx, tly, brx, bry;
            api.GetTopLeftPosition(out tlx, out tly);
            api.GetBottomRightPosition(out brx, out bry);

            if (longitude >= tlx && longitude <= brx && latitude >= bry && latitude <= tly) return true;
            return false;
        }
    }

    public OnlineMapsMarkerBase()
    {
        range = new OnlineMapsRange(3, 20);
    }

    /// <summary>
    /// Disposes marker
    /// </summary>
    public void Dispose()
    {
        OnClick = null;
        OnDoubleClick = null;
        OnDrag = null;
        OnDrawTooltip = null;
        OnEnabledChange = null;
        OnLongPress = null;
        OnPress = null;
        OnRelease = null;
        OnRollOut = null;
        OnRollOver = null;
    }

    /// <summary>
    /// Gets location of marker.
    /// </summary>
    /// <param name="lng">Longitude</param>
    /// <param name="lat">Latitude</param>
    public void GetPosition(out double lng, out double lat)
    {
        lng = longitude;
        lat = latitude;
    }

    /// <summary>
    /// Turns the marker in the direction specified coordinates.
    /// </summary>
    /// <param name="coordinates">
    /// The coordinates.
    /// </param>
    public virtual void LookToCoordinates(Vector2 coordinates)
    {
        
    }

    private void OnMarkerPress(OnlineMapsMarkerBase onlineMapsMarkerBase)
    {
        OnlineMapsControlBase.instance.dragMarker = this;
    }

    public virtual OnlineMapsXML Save(OnlineMapsXML parent)
    {
        OnlineMapsXML element = parent.Create("Marker");
        element.Create("Longitude", longitude);
        element.Create("Latitude", latitude);
        element.Create("Range", range);
        element.Create("Label", label);
        return element;
    }

    /// <summary>
    /// Set location of marker.
    /// </summary>
    /// <param name="lng">Longitude</param>
    /// <param name="lat">Latitude</param>
    public void SetPosition(double lng, double lat)
    {
        longitude = lng;
        latitude = lat;
    }

    /// <summary>
    /// Makes the marker dragable.
    /// </summary>
    public void SetDragable()
    {
        OnPress += OnMarkerPress;
    }

    /// <summary>
    /// Method that called when need update marker.
    /// </summary>
    /// <param name="topLeft">Coordinates of top-Left corner of map.</param>
    /// <param name="bottomRight">Coordinates of bottom-right corner of map.</param>
    /// <param name="zoom">Map zoom.</param>
    public virtual void Update(Vector2 topLeft, Vector2 bottomRight, int zoom)
    {
        
    }

    /// <summary>
    /// Method that called when need update marker.
    /// </summary>
    /// <param name="tlx">Top-left longitude.</param>
    /// <param name="tly">Top-left latutude.</param>
    /// <param name="brx">Bottom-right longitude.</param>
    /// <param name="bry">Bottom-right latitude.</param>
    /// <param name="zoom">Map zoom.</param>
    public virtual void Update(double tlx, double tly, double brx, double bry, int zoom)
    {

    }
}