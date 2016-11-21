using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System;

public class ColorWidget : ColorWidgetBase, IPointerEnterHandler, IPointerExitHandler
{
    #region Class members;
    public Image image;
    public Text label;
    private Vector2 pointerDownPos;
    #endregion

    #region Class accessors
    public Color color
    {
        get
        {
            return image.color;
        }
        set
        {
            image.color = value;
        }
    }

    public string colorName
    {
        get
        {
            return label.text;
        }
        set
        {
            label.text = value;
        }
    }
    #endregion

    #region IPointerDown interface implementation
    public void OnPointerEnter(PointerEventData eventData)
    {
       pointerDownPos = eventData.position;
    }
    #endregion

    #region IPointerDown interface implementation
    public void OnPointerExit(PointerEventData eventData)
    {
        if (Vector2.Distance(pointerDownPos, eventData.position) < 10)
            ColorsManager.Instance.PickColorForCurrentColorButton(this.gameObject);
    }
    #endregion
}
