/// <summary>
/// TweenTransform.
/// Created by Jorge L. CHavez Herrera
/// 
/// Defines functionality for tweening the color of a maskeable graphic.
/// </summary>
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TweenColor : TweenBase
{
    #region Class members
    public MaskableGraphic targetGraphic;
    public Color inColor;
    public Color outColor;
    #endregion

    #region MonoBehaviour overrides
    private void Start()
    {
        if (targetGraphic == null)
            targetGraphic = GetComponent<MaskableGraphic>();
    }
    #endregion

    #region Class implementation
    override protected void DoTweenIn()
    {
        TweenManager.FadeColorTo(targetGraphic, inColor, inDuraion, delay);
    }

    override protected void DoTweenOut()
    {
        TweenManager.FadeColorTo(targetGraphic, outColor, outDuration, delay);
    }
    #endregion
}
