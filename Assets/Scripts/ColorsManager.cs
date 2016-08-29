using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using Meridian.Framework.Utils;

public class ColorsManager : MonoSingleton<ColorsManager>
{
    #region Class members
    public GameObject colorWidgetPrefab;
    public GameObject combinationWidgetPrefab;

    public RectTransform trendsRoot;
    public RectTransform topColorRoot;
    public RectTransform combinationsRoot;

    public List<ColorButton> colorButtons;
    public List<GameObject> colorButtonHighlights;

    private int colorWidgetCount = 36;
    private int currentColor = 0;

    private List<GameObject> colorWidgets = new List<GameObject>();
    #endregion

    #region MonoBehaviour overrides
    private void Start ()
    {
        CreateGetColorWidgets(trendsRoot, colorWidgetPrefab);
        CreateGetColorWidgets(topColorRoot, colorWidgetPrefab);
        CreateCombinationWidgets(combinationsRoot, combinationWidgetPrefab);
        gameObject.SetActive(false);
    }
    #endregion

    #region Class implementation
    private void CreateGetColorWidgets(RectTransform t, GameObject prefab)
    {
        for (int i = 0; i < colorWidgetCount; i++)
        {
            GameObject go = GameObject.Instantiate(prefab);
            go.transform.SetParent(t, false);
            go.GetComponent<Graphic>().color = new Color(Random.value, Random.value, Random.value);
            colorWidgets.Add(go);
            go.GetComponent<Button>().onClick.AddListener(() =>  PickColorForCurrentColorButton(go));
            go.GetComponent<ColorWidgetBase>().waitToShow = i * 0.05f;
        }
    }

    private void CreateCombinationWidgets(RectTransform t, GameObject prefab)
    {
        for (int i = 0; i < colorWidgetCount; i++)
        {
            GameObject go = GameObject.Instantiate(prefab);
            go.transform.SetParent(t, false);
            go.GetComponent<CombinationWidget>().color1 = new Color(Random.value, Random.value, Random.value);
            go.GetComponent<CombinationWidget>().color2 = new Color(Random.value, Random.value, Random.value);
            go.GetComponent<CombinationWidget>().color3 = new Color(Random.value, Random.value, Random.value);
            colorWidgets.Add(go);
            go.GetComponent<Button>().onClick.AddListener(() => PickCombination(go));
            go.GetComponent<ColorWidgetBase>().waitToShow = i * 0.05f;
        }
    }

    public void SetCurrentColor(ColorButton colorButton)
    {
        for (int i = 0; i < colorButtonHighlights.Count; i++)
        {
            colorButtonHighlights[i].SetActive(i == colorButton.index);
        }

        currentColor = colorButton.index;

        // Pick new color if needed
        if (colorButton.colorWidget == null)
       	{
       		gameObject.SetActive(true);
       	}
    }

	public void ChangeColor(ColorButton colorButton)
	{
	 	//Turn on color picker
		gameObject.SetActive(true);	 
	}

    public int GetCurrentColor()
    {
    	return currentColor;
    }

    private void PickColorForCurrentColorButton(GameObject colorWidget)
    {
        gameObject.SetActive(false);

		colorButtons[currentColor].addColorLabel.gameObject.SetActive(false);
		colorButtons[currentColor].selectColorButton.gameObject.SetActive(true);
	
		ColorWidget newColorWidget = Instantiate(colorWidget.gameObject).GetComponent<ColorWidget>();
		colorButtons[currentColor].SetColorWidget(newColorWidget);

        // Pass color information to shader
        switch (currentColor)
        {
			case 0: DecoratorPanel.Instance.photoRenderer.material.SetColor("_Color1", Color32Utils.ConvertToHSV(newColorWidget.GetComponent<Graphic>().color)); break;
			case 1: DecoratorPanel.Instance.photoRenderer.material.SetColor("_Color2", Color32Utils.ConvertToHSV(newColorWidget.GetComponent<Graphic>().color)); break;
			case 2: DecoratorPanel.Instance.photoRenderer.material.SetColor("_Color3", Color32Utils.ConvertToHSV(newColorWidget.GetComponent<Graphic>().color)); break;
        }
    }

    public void PickCombination(GameObject combinationWidget)
    {
        gameObject.SetActive(false);

		colorButtons[currentColor].addColorLabel.gameObject.SetActive(false);
		colorButtons[currentColor].selectColorButton.gameObject.SetActive(true);

		int prevCurrentColor = currentColor;

		for (currentColor = 0; currentColor < colorButtons.Count; currentColor++)
        {
			ColorWidget newColorWidget = Instantiate(colorWidgetPrefab).GetComponent<ColorWidget>();
			newColorWidget.GetComponent<Graphic>().color = combinationWidget.GetComponent<CombinationWidget>().graphics[currentColor].color;
			PickColorForCurrentColorButton(newColorWidget.gameObject);
       	}

		currentColor = prevCurrentColor;
    }
    #endregion
}
