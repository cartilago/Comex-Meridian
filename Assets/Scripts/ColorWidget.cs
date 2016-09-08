using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ColorWidget : ColorWidgetBase
{
    #region Class members;
    public Image image;
    public Text label;
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
}
