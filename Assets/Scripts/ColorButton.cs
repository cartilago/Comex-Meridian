using UnityEngine;
using UnityEngine.UI;


public class ColorButton : MonoBehaviour  
{
	#region Class members
	public int index;
	public Text addColorLabel;
	public GameObject selectColorButton;
	public Image highlight;

	private ColorWidget colorWidget;
	#endregion

	#region MonoBehaviour overrides
	public void Start()
	{
		addColorLabel.gameObject.SetActive(true);
		selectColorButton.SetActive(false);
		highlight.gameObject.SetActive(false);
		colorWidget = null;
	}

	public void SetColorWidget(ColorWidget colorWidget)
	{
		// Remove old color widget
		if (this.colorWidget != null)
        {
			DestroyImmediate(this.colorWidget.gameObject);
        }

        this.colorWidget = colorWidget;

        // Add new color
        if (this.colorWidget != null)
        {
            this.colorWidget.transform.SetParent(transform, false);
            this.colorWidget.transform.localPosition = Vector3.zero;
            this.colorWidget.transform.localScale = Vector3.one;
            this.colorWidget.GetComponent<Graphic>().raycastTarget = false;
            this.colorWidget.transform.SetAsFirstSibling();
        }
	}

    public ColorWidget GetColorWidget()
    {
        return colorWidget;
    }
	#endregion
}
