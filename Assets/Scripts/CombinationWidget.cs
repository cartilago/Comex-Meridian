using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CombinationWidget : ColorWidgetBase
{
    #region Class members
    public Graphic buttonGraphic;
    public Graphic graphic1;
    public Graphic graphic2;
    public Graphic graphic3;
    #endregion

    #region Class accessors
    public Color color1
    {
        get { return graphic1.color; }
        set { graphic1.color = value; }
    }

    public Color color2
    {
        get { return graphic2.color; }
        set { graphic2.color = value; }
    }

    public Color color3
    {
        get { return graphic3.color; }
        set { graphic3.color = value; }
    }
    #endregion
}
