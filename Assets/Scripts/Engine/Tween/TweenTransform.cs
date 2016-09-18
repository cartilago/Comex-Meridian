/// <summary>
/// TweenTransform.
/// Created by Jorge L. CHavez Herrera
/// 
/// Defines functionality for tweening the transform of a game object.
/// </summary>
using UnityEngine;
using System.Collections;

public class TweenTransform : TweenBase
{
    #region Class members
    public RectTransform targetTransform;
    public Vector3 startPosition;
    public Vector3 endPosition;
    public Vector3 startRotation;
    public Vector3 endRotation;
    public Vector3 startScale = Vector3.one;
    public Vector3 endScale = Vector3.one;
    #endregion

    #region MonoBehaviour overrides
    private void Start()
    {
        if (targetTransform == null)
            targetTransform = GetComponent<RectTransform>();
    }
    #endregion

    #region Class implementation
    override protected void DoTweenIn()
    {
        cachedRectTransform.anchoredPosition3D = startPosition;
        cachedTransform.localEulerAngles = startRotation;
        cachedRectTransform.localScale = startScale;

        TweenManager.AnchoredPositionTo(cachedRectTransform, endPosition, inDuraion, delay, inInterpolationType);
        TweenManager.RotateTo(cachedRectTransform, endRotation, inDuraion, delay, inInterpolationType);
        TweenManager.ScaleTo(cachedRectTransform, endScale, inDuraion, delay, inInterpolationType);
    }

    override protected void DoTweenOut()
    {
        cachedRectTransform.anchoredPosition3D = endPosition;
        cachedTransform.localEulerAngles = endRotation;
        cachedRectTransform.localScale = endScale;

        TweenManager.AnchoredPositionTo(cachedRectTransform, startPosition, inDuraion, delay, inInterpolationType);
        TweenManager.RotateTo(cachedRectTransform, startRotation, inDuraion, delay, inInterpolationType);
        TweenManager.ScaleTo(cachedRectTransform, startScale, inDuraion, delay, inInterpolationType);
    }
    #endregion
}
