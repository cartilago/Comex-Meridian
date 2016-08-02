using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CombinationWidget : ColorWidgetBase
{
    #region Class members
    public Graphic buttonGraphic;
    public Graphic[] graphics;
    #endregion

    #region Class accessors
    public Color color1
    {
        get { return graphics[0].color; }
        set { graphics[0].color = value; }
    }

    public Color color2
    {
        get { return graphics[1].color; }
        set { graphics[1].color = value; }
    }

    public Color color3
    {
        get { return graphics[2].color; }
        set { graphics[2].color = value; }
    }
    #endregion
}
