/// <summary>
/// ButtonColor.
/// Created by: Jorge L. Chavez Herrera.
///
/// Sets the color of a button based on its different states.
/// </summary>
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class ButtonColor : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    #region Class members
    public MaskableGraphic targetGraphic;
    public ColorBlock colorBlock;
    #endregion

    #region IPointerDownHandler implementation
    // This fucntion is triggered by the Unity's event system
    public void OnPointerDown(PointerEventData eventData)
    {
        Selectable selectable = GetComponent<Selectable>();

        if (selectable != null && selectable.interactable == true)
        {
            StopAllCoroutines();
            StartCoroutine(ColorTo(colorBlock.pressedColor, colorBlock.fadeDuration));
        }
    }
    #endregion

    #region IPointerUpHandler implementation
    // This fucntion is triggered by the Unity's event system
    public void OnPointerUp(PointerEventData eventData)
    {
        Selectable selectable = GetComponent<Selectable>();

        if (selectable != null && selectable.interactable == true)
        {
            StopAllCoroutines();
            StartCoroutine(ColorTo (colorBlock.normalColor, colorBlock.fadeDuration));
        }
    }
    #endregion

    #region Class implementation
    private IEnumerator ColorTo (Color color, float duration)
    {
        Color startColor = targetGraphic.color;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            targetGraphic.color = Color.Lerp(startColor, color, t / duration);
            yield return null;
        }

        targetGraphic.color = color;
    }
    #endregion
}
