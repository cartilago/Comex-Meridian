using UnityEngine;
using UnityEngine.UI;


public class ColorWidget : OptimizedGameObject
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
}
