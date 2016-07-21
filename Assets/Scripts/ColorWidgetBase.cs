using UnityEngine;
using System.Collections;

public class ColorWidgetBase : OptimizedGameObject
{
    [System.NonSerialized]
    public float waitToShow = 0;

    // Use this for initialization
    private void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(ShowAnimated(1));
    }

    // Update is called once per frame
    private IEnumerator ShowAnimated(float duration)
    {
        cachedTransform.localScale = Vector3.zero;
        yield return new WaitForSeconds(waitToShow);
        TweenManager.ScaleTo(cachedTransform, Vector3.one, 0.5f, 0, EInterpolationType.Elastic);
    }
}
