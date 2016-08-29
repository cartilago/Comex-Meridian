using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class ColorButton : MonoBehaviour  
{
	#region Class members
	public int index;
	public Text addColorLabel;
	public GameObject selectColorButton;
	public Image highlight;
	public ColorWidget colorWidget;
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

		// Add new color
		this.colorWidget = colorWidget;
		this.colorWidget.transform.SetParent(transform, false);
		this.colorWidget.transform.localPosition = Vector3.zero;
		this.colorWidget.transform.localScale = Vector3.one;
		this.colorWidget.GetComponent<Graphic>().raycastTarget = false;
		this.colorWidget.transform.SetAsFirstSibling();
	}
	#endregion
}
