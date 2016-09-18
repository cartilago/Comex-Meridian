/// <summary>
/// TweenBase.
/// Created by Jorge L. CHavez Herrera
/// 
/// Defines functionality for all kinds of tweening.
/// </summary>
using UnityEngine;
using System.Collections;

public class TweenBase : OptimizedGameObject
{
    #region Class members
    public float inDuraion = 0.5f;
    public float outDuration = 0.5f;
    public float delay = 0;
    public EInterpolationType inInterpolationType = EInterpolationType.EaseInOut;
    public EInterpolationType outInterpolationType = EInterpolationType.EaseInOut;

    private enum ETweenState {Out, In}; 
    private ETweenState tweenState;
    #endregion

    #region Class implementation
    public void TweenIn()
    {
    	    if (tweenState == ETweenState.In)
    		    return;

        DoTweenIn();
        tweenState = ETweenState.In;
    }

    virtual protected void DoTweenIn() {}

    public void TweenOut()
    {
		if (tweenState == ETweenState.Out)
    		    return;

        DoTweenOut();
		tweenState = ETweenState.Out;
    }

    virtual protected void DoTweenOut() {}
    #endregion
}
