/*     INFINITY CODE 2013-2016      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// Controls map using GPS.\n
/// Online Maps Location Service is a wrapper for Unity Location Service.\n
/// http://docs.unity3d.com/ScriptReference/LocationService.html
/// </summary>
[Serializable]
[AddComponentMenu("Infinity Code/Online Maps/Plugins/Location Service")]
public class OnlineMapsLocationService : MonoBehaviour
{
    private static OnlineMapsLocationService _instance;

    /// <summary>
    /// This event is called when the user rotates the device.
    /// </summary>
    public Action<float> OnCompassChanged;

    /// <summary>
    /// This event is called when changed your GPS location.
    /// </summary>
    public Action<Vector2> OnLocationChanged;

    /// <summary>
    /// This event is called when the GPS is initialized (the first value is received).
    /// </summary>
    public Action OnLocationInited;

    /// <summary>
    /// Update stop position when user input.
    /// </summary>
    public bool autoStopUpdateOnInput = true;

    /// <summary>
    /// Threshold of compass.
    /// </summary>
    public float compassThreshold = 8;

    /// <summary>
    /// Specifies the need to create a marker that indicates the current GPS coordinates.
    /// </summary>
    public bool createMarkerInUserPosition = false;

    /// <summary>
    /// Desired service accuracy in meters. 
    /// </summary>
    public float desiredAccuracy = 10;

    /// <summary>
    /// Emulated compass trueHeading.\n
    /// Do not use.\n
    /// Use OnlineMapsLocationService.trueHeading.
    /// </summary>
    public float emulatorCompass;

    /// <summary>
    /// Emulated GPS position.\n
    /// Do not use.\n
    /// Use OnlineMapsLocationService.position.
    /// </summary>
    public Vector2 emulatorPosition;

    /// <summary>
    /// Specifies whether to search for a location by IP.
    /// </summary>
    public bool findLocationByIP = true;

    /// <summary>
    /// Tooltip of the marker.
    /// </summary>
    public string markerTooltip;

    /// <summary>
    /// Type of the marker.
    /// </summary>
    public OnlineMapsLocationServiceMarkerType markerType = OnlineMapsLocationServiceMarkerType.twoD;

    /// <summary>
    /// Align of the 2D marker.
    /// </summary>
    public OnlineMapsAlign marker2DAlign = OnlineMapsAlign.Center;

    /// <summary>
    /// Texture of 2D marker.
    /// </summary>
    public Texture2D marker2DTexture;

    /// <summary>
    /// Prefab of 3D marker.
    /// </summary>
    public GameObject marker3DPrefab;

    /// <summary>
    /// The maximum number of stored positions./n
    /// It is used to calculate the speed.
    /// </summary>
    public int maxPositionCount = 3;

    /// <summary>
    /// Current GPS coordinates.\n
    /// <strong>Important: position not available Start, because GPS is not already initialized. \n
    /// Use OnLocationChanged event, to determine the initialization of GPS.</strong>
    /// </summary>
    public Vector2 position = Vector2.zero;

    /// <summary>
    /// Use the GPS coordinates after seconds of inactivity.
    /// </summary>
    public int restoreAfter = 10;

    /// <summary>
    /// The heading in degrees relative to the geographic North Pole.\n
    /// <strong>Important: position not available Start, because compass is not already initialized. \n
    /// Use OnCompassChanged event, to determine the initialization of compass.</strong>
    /// </summary>
    public float trueHeading = 0;

    /// <summary>
    ///  The minimum distance (measured in meters) a device must move laterally before location is updated.
    /// </summary>
    public float updateDistance = 10;

    /// <summary>
    /// Specifies whether the script will automatically update the location.
    /// </summary>
    public bool updatePosition = true;

    /// <summary>
    /// Specifies the need for marker rotation.
    /// </summary>
    public bool useCompassForMarker = false;

    /// <summary>
    /// Specifies GPS emulator usage. \n
    /// Works only in Unity Editor.
    /// </summary>
    public bool useGPSEmulator = false;

    private OnlineMaps api;

    private bool _allowUpdatePosition = true; 
    private long lastPositionChangedTime;
    private bool lockDisable;
    private bool isPositionInited = false;
    
    private OnlineMapsMarkerBase _marker; 
    private List<LastPositionItem> lastPositions;
    private double lastLocationInfoTimestamp;
    private float _speed = 0;
    private bool started = false;

    //private string errorMessage = "";
    private OnlineMapsWWW findByIPRequest;

    /// <summary>
    /// Instance of LocationService.
    /// </summary>
    public static OnlineMapsLocationService instance
    {
        get { return _instance; }
    }

    /// <summary>
    /// Instance of marker.
    /// </summary>
    public static OnlineMapsMarkerBase marker
    {
        get { return _instance._marker; }
        set { _instance._marker = value; }
    }

    public bool allowUpdatePosition
    {
        get { return _allowUpdatePosition; }
        set
        {
            _allowUpdatePosition = true;
            UpdatePosition();
        }
    }

    /// <summary>
    /// Speed km/h.
    /// Note: in Unity Editor will always be zero.
    /// </summary>
    public float speed
    {
        get { return _speed; }
    }

    private void OnChangePosition()
    {
        if (lockDisable) return;

        lastPositionChangedTime = DateTime.Now.Ticks;
        if (autoStopUpdateOnInput) _allowUpdatePosition = false;
    }

    private void OnEnable()
    {
        _instance = this;
        if (api != null) api.OnChangePosition += OnChangePosition;
    }

    /*private void OnGUI()
    {
        if (!string.IsNullOrEmpty(errorMessage)) GUI.Label(new Rect(5, 5, Screen.width, 300), errorMessage);
    }*/

    public OnlineMapsXML Save(OnlineMapsXML parent)
    {
        OnlineMapsXML element = parent.Create("LocationService");
        element.Create("DesiredAccuracy", desiredAccuracy);
        element.Create("UpdatePosition", updatePosition);
        element.Create("AutoStopUpdateOnInput", autoStopUpdateOnInput);
        element.Create("RestoreAfter", restoreAfter);

        element.Create("CreateMarkerInUserPosition", createMarkerInUserPosition);

        if (createMarkerInUserPosition)
        {
            element.Create("MarkerType", (int)markerType);
            
            if (markerType == OnlineMapsLocationServiceMarkerType.twoD)
            {
                element.Create("Marker2DAlign", (int) marker2DAlign);
                element.Create("Marker2DTexture", marker2DTexture);
            }
            else element.Create("Marker3DPrefab", marker3DPrefab);

            element.Create("MarkerTooltip", markerTooltip);
            element.Create("UseCompassForMarker", useCompassForMarker);
        }

        element.Create("UseGPSEmulator", useGPSEmulator);
        if (useGPSEmulator)
        {
            element.Create("EmulatorPosition", emulatorPosition);
            element.Create("EmulatorCompass", emulatorCompass);
        }

        return element;
    }

    private void Start()
    {
        api = OnlineMaps.instance;
        api.OnChangePosition += OnChangePosition;

        if (findLocationByIP)
        {
#if UNITY_EDITOR || !UNITY_WEBGL
            findByIPRequest = OnlineMapsUtils.GetWWW("http://www.geoplugin.net/php.gp");
#else
            findByIPRequest = OnlineMapsUtils.GetWWW("http://service.infinity-code.com/getlocation.php");
#endif
        }
    }

    /// <summary>
    /// Starts location service updates. Last location coordinates could be.
    /// </summary>
    /// <param name="desiredAccuracyInMeters">
    /// Desired service accuracy in meters. \n
    /// Using higher value like 500 usually does not require to turn GPS chip on and thus saves battery power. \n
    /// Values like 5-10 could be used for getting best accuracy. Default value is 10 meters.</param>
    /// <param name="updateDistanceInMeters">
    /// The minimum distance (measured in meters) a device must move laterally before Input.location property is updated. \n
    /// Higher values like 500 imply less overhead.
    /// </param>
    public void StartLocationService(float? desiredAccuracyInMeters = null, float? updateDistanceInMeters = null)
    {
        if (!desiredAccuracyInMeters.HasValue) desiredAccuracyInMeters = desiredAccuracy;
        if (!updateDistanceInMeters.HasValue) updateDistanceInMeters = updateDistance;

        Input.location.Start(desiredAccuracyInMeters.Value, updateDistanceInMeters.Value);
    }

    /// <summary>
    /// Stops location service updates. This could be useful for saving battery life.
    /// </summary>
    public void StopLocationService()
    {
        Input.location.Stop();
    }

    private void Update () 
    {
	    if (api == null)
	    {
            api = OnlineMaps.instance;
            if (api == null) return; 
        }

	    try
	    { 
            if (!started)
            {
#if !UNITY_EDITOR
                if (!Input.location.isEnabledByUser) return;

                Input.compass.enabled = true;
                Input.location.Start(desiredAccuracy, updateDistance);
#endif
                started = true;
            }

#if !UNITY_EDITOR
            useGPSEmulator = false;
#endif
            bool positionChanged = false;

            if (findByIPRequest != null && findByIPRequest.isDone)
            {
                if (string.IsNullOrEmpty(findByIPRequest.error))
                {
                    string response = findByIPRequest.text;
                    Match latMath = Regex.Match(response, "geoplugin_latitude\";.*?\"(\\d*\\.\\d*)\"");
                    Match lngMath = Regex.Match(response, "geoplugin_longitude\";.*?\"(\\d*\\.\\d*)\"");

                    if (latMath.Success && lngMath.Success)
                    {
                        float lng = float.Parse(lngMath.Groups[1].Value);
                        float lat = float.Parse(latMath.Groups[1].Value);

                        if (useGPSEmulator) emulatorPosition = new Vector2(lng, lat);
                        else if (position == Vector2.zero)
                        {
                            position = new Vector2(lng, lat);
                            positionChanged = true;
                        }
                    }
                }
                findByIPRequest = null;
            }

            if (createMarkerInUserPosition && _marker == null && (useGPSEmulator || position != Vector2.zero)) UpdateMarker();

            if (!useGPSEmulator && Input.location.status != LocationServiceStatus.Running) return;

            bool compassChanged = false;

#if !UNITY_EDITOR
	        UpdateCompassFromInput(ref compassChanged);
#else
            if (useGPSEmulator) UpdateCompassFromEmulator(ref compassChanged);
            else UpdateCompassFromInput(ref compassChanged);
#endif

#if !UNITY_EDITOR
            UpdateSpeed();
            UpdatePositionFromInput(ref positionChanged);
#else
            if (useGPSEmulator) UpdatePositionFromEmulator(ref positionChanged);
            else UpdatePositionFromInput(ref positionChanged);
#endif

            if (positionChanged)
            {
                if (!isPositionInited)
                {
                    isPositionInited = true;
                    if (OnLocationInited != null) OnLocationInited();
                }
                if (OnLocationChanged != null) OnLocationChanged(position);
            }

	        if (createMarkerInUserPosition && (positionChanged || compassChanged)) UpdateMarker();

            if (updatePosition)
            {
                if (_allowUpdatePosition)
                {
                    UpdatePosition();
                }
                else if (restoreAfter > 0 && DateTime.Now.Ticks > lastPositionChangedTime + OnlineMapsUtils.second * restoreAfter)
                {
                    _allowUpdatePosition = true;
                    UpdatePosition();
                }
            } 
	    }
        catch /*(Exception exception)*/
	    {
	        //errorMessage = exception.Message + "\n" + exception.StackTrace;
	    }
    }

    private void UpdateCompassFromEmulator(ref bool compassChanged)
    {
        if (trueHeading != emulatorCompass)
        {
            compassChanged = true;
            trueHeading = emulatorCompass;
            if (OnCompassChanged != null) OnCompassChanged(trueHeading / 360);
        }
    }

    private void UpdateCompassFromInput(ref bool compassChanged)
    {
        float heading = Input.compass.trueHeading;
        float offset = trueHeading - heading;

        if (offset > 360) offset -= 360;
        else if (offset < -360) offset += 360;

        if (Mathf.Abs(offset) > compassThreshold)
        {
            compassChanged = true;
            trueHeading = heading;
            if (OnCompassChanged != null) OnCompassChanged(trueHeading / 360);
        }
    }

    private void UpdateMarker()
    {
        if (_marker == null)
        {
            if (markerType == OnlineMapsLocationServiceMarkerType.twoD)
            {
                _marker = OnlineMaps.instance.AddMarker(position, marker2DTexture, markerTooltip);
                (_marker as OnlineMapsMarker).align = marker2DAlign;
            }
            else
            {
                OnlineMapsControlBase3D control = OnlineMapsControlBase3D.instance;
                if (control == null)
                {
                    Debug.LogError("You must use the 3D control (Texture or Tileset).");
                    createMarkerInUserPosition = false;
                    return;
                }
                _marker = control.AddMarker3D(position, marker3DPrefab);
                _marker.label = markerTooltip;
            }
        }
        else
        {
            _marker.position = position;
        }

        if (useCompassForMarker)
        {
            if (markerType == OnlineMapsLocationServiceMarkerType.twoD) (_marker as OnlineMapsMarker).rotation = trueHeading / 360;
            else (_marker as OnlineMapsMarker3D).rotation = Quaternion.Euler(0, trueHeading, 0);
        }

        api.Redraw();
    }

    /// <summary>
    /// Sets map position using GPS coordinates.
    /// </summary>
    public void UpdatePosition()
    {  
        if (!useGPSEmulator && position == Vector2.zero) return;
        if (api == null) return;

        lockDisable = true;

        Vector2 p = api.position;
        bool changed = false;

        if (p.x != position.x)
        {
            p.x = position.x;
            changed = true;
        }
        if (p.y != position.y)
        {
            p.y = position.y;
            changed = true;
        }
        if (changed)
        {
            api.position = p;
            api.Redraw();
        }

        lockDisable = false;
    }

    private void UpdatePositionFromEmulator(ref bool positionChanged)
    {
        if (position.x != emulatorPosition.x)
        {
            position.x = emulatorPosition.x;
            positionChanged = true;
        }
        if (position.y != emulatorPosition.y)
        {
            position.y = emulatorPosition.y;
            positionChanged = true;
        }
    }

    private void UpdatePositionFromInput(ref bool positionChanged)
    {
        if (position.x != Input.location.lastData.longitude)
        {
            position.x = Input.location.lastData.longitude;
            positionChanged = true;
        }
        if (position.y != Input.location.lastData.latitude)
        {
            position.y = Input.location.lastData.latitude;
            positionChanged = true;
        }
    }

    private void UpdateSpeed()
    {
        LocationInfo lastData = Input.location.lastData;
        if (lastLocationInfoTimestamp == lastData.timestamp) return;

        lastLocationInfoTimestamp = lastData.timestamp;

        if (lastPositions == null) lastPositions = new List<LastPositionItem>();

        lastPositions.Add(new LastPositionItem(lastData.longitude, lastData.latitude, lastData.timestamp));
        while (lastPositions.Count > maxPositionCount) lastPositions.RemoveAt(0);

        if (lastPositions.Count < 2)
        {
            _speed = 0;
            return;
        }

        LastPositionItem p1 = lastPositions[0];
        LastPositionItem p2 = lastPositions[lastPositions.Count - 1];

        double dx, dy;
        OnlineMapsUtils.DistanceBetweenPoints(p1.lng, p1.lat, p2.lng, p2.lat, out dx, out dy);
        double distance = Math.Sqrt(dx * dx + dy * dy);
        double time = (p2.timestamp - p1.timestamp) / 3600;
        _speed = Mathf.Abs((float) (distance / time));
    }

    internal struct LastPositionItem
    {
        public float lat;
        public float lng;
        public double timestamp;

        public LastPositionItem(float longitude, float latitude, double timestamp)
        {
            lng = longitude;
            lat = latitude;
            this.timestamp = timestamp;
        }
    }
}