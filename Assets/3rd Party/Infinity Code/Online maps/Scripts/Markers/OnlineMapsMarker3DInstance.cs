/*     INFINITY CODE 2013-2016      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if !UNITY_4_3 && !UNITY_4_5
using UnityEngine.EventSystems;
#endif

/// <summary>
/// 3D marker instance class.
/// </summary>
[AddComponentMenu("")]
public class OnlineMapsMarker3DInstance : OnlineMapsMarkerInstanceBase
{
    private double _longitude;
    private double _latitude;
    //private Vector2 _position;
    private float _scale;

    private int lastTouchCount;
    private bool isPressed;

    private long[] lastClickTimes = { 0, 0 };
    private IEnumerator longPressEnumenator;
    private Vector2 pressPoint;

    public override OnlineMapsMarkerBase marker
    {
        get { return _marker; }
        set { _marker = value as OnlineMapsMarker3D; }
    }

    [SerializeField]
    private OnlineMapsMarker3D _marker;

    private void Awake()
    {
#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
        Collider cl = collider;
#else
        Collider cl = GetComponent<Collider>();
#endif

        if (cl == null) cl  = gameObject.AddComponent<BoxCollider>();
        cl.isTrigger = true;
    }

    private void OnPress()
    {
#if !UNITY_ANDROID
        OnlineMapsControlBase.instance.InvokeBasePress();
#endif

        isPressed = true;
        if (marker.OnPress != null) marker.OnPress(marker);

        lastClickTimes[0] = lastClickTimes[1];
        lastClickTimes[1] = DateTime.Now.Ticks;

        pressPoint = OnlineMapsControlBase.instance.GetInputPosition();

        if (Input.GetKey(KeyCode.LeftControl))
        {
            OnlineMapsControlBase.instance.dragMarker = marker;
            OnlineMapsControlBase.instance.isMapDrag = false;
        }

        longPressEnumenator = WaitLongPress();
        StartCoroutine("WaitLongPress");
        OnlineMapsControlBase.instance.OnMapDrag += StopWaitLongPress;
    }

    private void OnRelease()
    {
#if !UNITY_ANDROID
        OnlineMapsControlBase.instance.InvokeBaseRelease();
#endif

        if (marker.OnRelease != null) marker.OnRelease(marker);

        StopWaitLongPress();

        OnlineMapsControlBase.instance.dragMarker = null;
        bool isClick = (pressPoint - OnlineMapsControlBase.instance.GetInputPosition()).sqrMagnitude < 400;

        if (!isPressed || !isClick) return;

        if (DateTime.Now.Ticks - lastClickTimes[0] < 5000000)
        {
            if (marker.OnDoubleClick != null) marker.OnDoubleClick(marker);
            lastClickTimes[0] = 0;
            lastClickTimes[1] = 0;
        }
        else if (marker.OnClick != null) marker.OnClick(marker);
    }

    private void Start()
    {
        marker.GetPosition(out _longitude, out _latitude);
        _scale = marker.scale;
        transform.localScale = new Vector3(_scale, _scale, _scale);
    }

    private void StopWaitLongPress()
    {
        OnlineMapsControlBase.instance.OnMapDrag -= StopWaitLongPress;
        if (longPressEnumenator != null) StopCoroutine("WaitLongPress");
        longPressEnumenator = null;
    }

    private void Update()
    {
        if ((marker as OnlineMapsMarker3D) == null) 
        {
            OnlineMapsUtils.DestroyImmediate(gameObject);
            return;
        }

        UpdateBaseProps();
        UpdateDefaultMarkerEvens();
    }

    private void UpdateBaseProps()
    {
        double mx, my;
        marker.GetPosition(out mx, out my);

        if (_longitude != mx || _latitude != my)
        {
            _longitude = mx;
            _latitude = my;

            OnlineMaps map = OnlineMaps.instance;

            double tlx, tly, brx, bry;
            map.GetTopLeftPosition(out tlx, out tly);
            map.GetBottomRightPosition(out brx, out bry);

            marker.Update(tlx, tly, brx, bry, map.zoom);
        }

        if (_scale != marker.scale)
        {
            _scale = marker.scale;
            transform.localScale = new Vector3(_scale, _scale, _scale);
        }
    }

    private void UpdateDefaultMarkerEvens()
    {
        if (!(marker as OnlineMapsMarker3D).allowDefaultMarkerEvents) return;

#if !IGUI && ((!UNITY_ANDROID && !UNITY_IOS) || UNITY_EDITOR)
        if (OnlineMaps.instance.notInteractUnderGUI && GUIUtility.hotControl != 0) return;
#endif

        Vector2 inputPosition = OnlineMapsControlBase.instance.GetInputPosition();

#if !UNITY_4_3 && !UNITY_4_5
        if (EventSystem.current != null)
        {
            PointerEventData pe = new PointerEventData(EventSystem.current);
            pe.position = inputPosition;
            List<RaycastResult> uiHits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pe, uiHits);
            if (uiHits.Count > 0 && uiHits[0].gameObject != gameObject) return;
        }
#endif

        int touchCount = 0;

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        touchCount = Input.touchCount;
#else
        touchCount = Input.GetMouseButton(0) ? 1 : 0;
#endif
        if (lastTouchCount == touchCount) return;

        RaycastHit[] hits = Physics.RaycastAll(OnlineMapsControlBase3D.instance.activeCamera.ScreenPointToRay(inputPosition));
        bool hit = false;

        foreach (RaycastHit h in hits)
        {
            if (h.collider.gameObject == gameObject)
            {
                hit = true;
                break;
            }
            if (h.collider.GetComponent<OnlineMapsMarkerInstanceBase>() != null)
            {
                break;
            }
        }

        if (touchCount == 0)
        {
            if (hit) OnRelease();
            isPressed = false;
        }
        else if (hit)
        {
            OnPress();
        }

        lastTouchCount = touchCount;
    }

    private IEnumerator WaitLongPress()
    {
        yield return new WaitForSeconds(OnlineMapsControlBase.longPressDelay);

        OnlineMapsControlBase.instance.OnMapDrag -= StopWaitLongPress;

        if (marker.OnLongPress != null) marker.OnLongPress(marker);
        longPressEnumenator = null;
    }
}