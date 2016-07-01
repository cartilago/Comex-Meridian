/// <summary>
/// TweenManager.
/// Created by Jorge L. Chavez Herrera.
/// 
/// Provides functionality for tweening values useful for UI effects.
/// </summary>
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Meridian.Framework.Utils;


public enum EInterpolationType {Linear, EaseIn, EaseOut, EaseInOut, Elastic};

public class TweenManager : MonoSingleton<TweenManager>
{
    #region Class members
    static private AnimationCurve linearCurve = new AnimationCurve(new Keyframe[] {new Keyframe (0,0,1,1), new Keyframe (1, 1, 1, 1)});

    static private AnimationCurve easeInCurve = new AnimationCurve(new Keyframe[] {new Keyframe (0, 0),new Keyframe (1, 1, 1 , 1)});

    static private AnimationCurve easeOutCurve = new AnimationCurve(new Keyframe[] {new Keyframe (0,0,-2, -2), new Keyframe (1, 1)});

    static private AnimationCurve easeInOutCurve = new AnimationCurve(new Keyframe[] {new Keyframe (0,0), new Keyframe (1, 1)});

    static private AnimationCurve elasticCurve = new AnimationCurve(new Keyframe[] {new Keyframe (0,0), new Keyframe (0.25f, 1.25f), new Keyframe (0.5f, 1 - 0.125f),
            new Keyframe (0.75f, 1.0625f), new Keyframe (1, 1)});

    static private AnimationCurve[] curves = new AnimationCurve[] { linearCurve, easeInCurve, easeOutCurve, easeInOutCurve, elasticCurve };
    #endregion

    #region Class implementation
    /// <summary>
    /// Fades the current color of a MaskableGraphic to the destination color
    /// </summary>
    /// <param name="graphic"></param>
    /// <param name="destColor"></param>
    /// <param name="duration"></param>
    /// <param name="delay"></param>
    static public void FadeColorTo(MaskableGraphic graphic, Color destColor, float duration, float delay = 0)
    {
        Instance.StartCoroutine(Instance.FadeColorToCoroutine(graphic, destColor, duration, delay));
    }

    private IEnumerator FadeColorToCoroutine(MaskableGraphic graphic, Color destColor, float duration, float delay)
    {
        Color sourceColor = graphic.color;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float nt = t / duration;
            graphic.color = Color.Lerp(sourceColor, destColor, nt);
            yield return null;
        }

        graphic.color = destColor;
    }

    static public void FadeAlphaTo(CanvasGroup canvasGroup, float destAlpha, float duration, float delay = 0)
    {
        Instance.StartCoroutine(Instance.FadeAlphaToCoroutine(canvasGroup, destAlpha, duration, delay));
    }
    
    private IEnumerator FadeAlphaToCoroutine(CanvasGroup canvasGroup, float destAlpha, float duration, float delay)
    {
        float sourceAlpha = canvasGroup.alpha;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float nt = t / duration;
            canvasGroup.alpha = Mathf.Lerp(sourceAlpha, destAlpha, nt);
            yield return null;
        }

        canvasGroup.alpha = destAlpha;
    }

    /// <summary>
    /// Animates the anchored position of a rect transform to the destination value
    /// </summary>
    /// <param name="rectTransform"></param>
    /// <param name="anchoredPosition"></param>
    /// <param name="duration"></param>
    /// <param name="delay"></param>
    static public void AnchoredPositionTo(RectTransform rectTransform, Vector3 anchoredPosition, float duration, float delay, EInterpolationType interpolation)
    {
        Instance.StartCoroutine(Instance.AnchoredPositionToCoroutine(rectTransform, anchoredPosition, duration, delay, interpolation));
    }

    private IEnumerator AnchoredPositionToCoroutine(RectTransform rectTransform, Vector3 anchoredPosition, float duration, float delay, EInterpolationType interpolation)
    {
        Vector3 startAnchoredPosition = rectTransform.anchoredPosition;

        AnimationCurve animationCurve = curves[(int)interpolation];

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float nt = t / duration;
            rectTransform.anchoredPosition = Vector3.LerpUnclamped(startAnchoredPosition, anchoredPosition, animationCurve.Evaluate(nt));
            yield return null;
        }

        rectTransform.anchoredPosition = anchoredPosition;
    }

    /// <summary>
    /// Animates the rotation of a transform to the destination value
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="eulerAngles"></param>
    /// <param name="duration"></param>
    /// <param name="delay"></param>
    static public void RotateTo(Transform transform, Vector3 eulerAngles, float duration, float delay, EInterpolationType interpolation)
    {
        Instance.StartCoroutine(Instance.RotateToCoroutine(transform, eulerAngles, duration, delay, interpolation));
    }

    private IEnumerator RotateToCoroutine(Transform transform, Vector3 eulerAngles, float duration, float delay, EInterpolationType interpolation)
    {
        Vector3 startEulerAngles = transform.localEulerAngles;

        AnimationCurve animationCurve = curves[(int)interpolation];

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float nt = t / duration;
            transform.localEulerAngles = Vector3.Slerp(startEulerAngles, eulerAngles, animationCurve.Evaluate(nt));
            yield return null;
        }

        transform.localEulerAngles = eulerAngles;
    }

    /// <summary>
    /// Animates the sacale of a transform to the destination value
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="scale"></param>
    /// <param name="duration"></param>
    /// <param name="delay"></param>
    static public void ScaleTo(Transform transform, Vector3 scale, float duration, float delay, EInterpolationType interpolation)
    {
        Instance.StartCoroutine(Instance.ScaleToCoroutine(transform, scale, duration, delay, interpolation));
    }

    private IEnumerator ScaleToCoroutine(Transform transform, Vector3 scale, float duration, float delay, EInterpolationType interpolation)
    {
        Vector3 startLocalScale = transform.localScale;

        AnimationCurve animationCurve = curves[(int)interpolation];

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float nt = t / duration;
            transform.localScale = Vector3.LerpUnclamped(startLocalScale, scale, animationCurve.Evaluate(nt));
            yield return null;
        }

        transform.localScale = scale;
    }
    #endregion
}
