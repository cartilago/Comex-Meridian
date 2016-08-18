/// <summary>
/// TweenTransform.
/// Created by Jorge L. CHavez Herrera
/// 
/// Defines functionality for tweening the transform of a game object.
/// </summary>
using UnityEngine;
using System.Collections;

public class TweenTransform : OptimizedGameObject
{
    #region Class members
    public float inDuraion = 0.5f;
    public float outDuration = 0.5f;
    public float delay = 0;
    public EInterpolationType inInterpolationType = EInterpolationType.EaseInOut;
    public EInterpolationType outInterpolationType = EInterpolationType.EaseInOut;

    public Vector3 startPosition;
    public Vector3 endPosition;
    public Vector3 startRotation;
    public Vector3 endRotation;
    public Vector3 startScale = Vector3.one;
    public Vector3 endScale = Vector3.one;

    private enum ETweenState {Out, In}; 
    private ETweenState tweenState;
    #endregion

    #region Class implementation
    public void TweenIn()
    {
    	if (tweenState == ETweenState.In)
    		return;

        cachedRectTransform.anchoredPosition3D = startPosition;
        cachedTransform.localEulerAngles = startRotation;
        cachedRectTransform.localScale = startScale;

        TweenManager.AnchoredPositionTo(cachedRectTransform, endPosition, inDuraion, delay, inInterpolationType);
        TweenManager.RotateTo(cachedRectTransform, endRotation, inDuraion, delay, inInterpolationType);
        TweenManager.ScaleTo(cachedRectTransform, endScale, inDuraion, delay, inInterpolationType);

        tweenState = ETweenState.In;
    }

    public void TweenOut()
    {
		if (tweenState == ETweenState.Out)
    		return;

        cachedRectTransform.anchoredPosition3D = endPosition;
        cachedTransform.localEulerAngles = endRotation;
        cachedRectTransform.localScale = endScale;

        TweenManager.AnchoredPositionTo(cachedRectTransform, startPosition, inDuraion, delay, inInterpolationType);
        TweenManager.RotateTo(cachedRectTransform, startRotation, inDuraion, delay, inInterpolationType);
        TweenManager.ScaleTo(cachedRectTransform, startScale, inDuraion, delay, inInterpolationType);

		tweenState = ETweenState.Out;
    }
    #endregion
}
