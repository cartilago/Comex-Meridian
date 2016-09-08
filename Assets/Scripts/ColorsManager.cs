using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using Meridian.Framework.Utils;

public class ColorsManager : MonoSingleton<ColorsManager>
{
    #region Class members
    public TextAsset colorCSVFile;
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
        // Read CMYK colors from text file;
        string[] lines = colorCSVFile.text.Split("\n"[0]);

        int maxlines = 75;

        for (int i = 0; i < /*lines.Length*/ maxlines; i++)
        {
            string[] rows = lines[i].Split(',');

            GameObject go = GameObject.Instantiate(prefab);
            go.transform.SetParent(t, false);    
            colorWidgets.Add(go);
            go.GetComponent<Button>().onClick.AddListener(() =>  PickColorForCurrentColorButton(go));
            go.GetComponent<ColorWidget>().waitToShow = i * 0.05f;
            go.GetComponent<ColorWidget>().label.text = rows[0] + '\n' + rows[1];
            //go.GetComponent<ColorWidget>().label.gameObject.SetActive(false);
            go.GetComponent<Graphic>().color = CMYKToColor(float.Parse(rows[2]) * .01f,
                                                           float.Parse(rows[3]) * .01f,
                                                           float.Parse(rows[4]) * .01f, 
                                                           float.Parse(rows[5]) * .01f);
        }
    }

    public static Color32 CMYKToColor(float C, float M, float Y, float K)
    {
        byte r = (byte)(255 * (1 - C) * (1 - K));
        byte g = (byte)(255 * (1 - M) * (1 - K));
        byte b = (byte)(255 * (1 - Y) * (1 - K));

        return new Color32(r, g, b, 255);
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
        if (colorButton.GetColorWidget() == null)
       	{
       		gameObject.SetActive(true);
       	}
    }

	public void ChangeColor(ColorButton colorButton)
	{
        SetCurrentColor(colorButton);
        //Turn on color picker
        gameObject.SetActive(true);	 
	}

    public int GetCurrentColor()
    {
        	return currentColor;
    }

    private void SetColorForButton(int index, Color color, string colorName)
    {
        if (string.IsNullOrEmpty(colorName) == false)
        {
            colorButtons[index].addColorLabel.gameObject.SetActive(false);
            colorButtons[index].selectColorButton.gameObject.SetActive(true);

            ColorWidget newColorWidget = Instantiate(colorWidgetPrefab).GetComponent<ColorWidget>();
            newColorWidget.color = color;
            newColorWidget.colorName = colorName;
            colorButtons[index].SetColorWidget(newColorWidget);

            // Pass the color to the shader
            DecoratorPanel.Instance.photoRenderer.material.SetColor("_Color" + (index + 1).ToString(), Color32Utils.ConvertToHSV(newColorWidget.color));
        }
        else
        {
            colorButtons[index].addColorLabel.gameObject.SetActive(true);
            colorButtons[index].selectColorButton.gameObject.SetActive(false);
            colorButtons[index].SetColorWidget(null);

            // Pass the color to the shader
            DecoratorPanel.Instance.photoRenderer.material.SetColor("_Color" + (index + 1).ToString(), Color32Utils.ConvertToHSV(Color.black));
        }
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

    /// <summary>
    /// Gets the colorwidgets attached to the UI color buttons
    /// </summary>
    /// <returns></returns>
    public ColorWidget[] GetButtonColorWidgets()
    {
        ColorWidget[] ret = new ColorWidget[3];

        for (int i = 0; i < 3; i++)
        {
            ret[i] = colorButtons[i].GetColorWidget();
        }

        return ret;
    }

    public void SetColorsForButtons(Color[] colors, string[] colorNames)
    {
        for (int i = 0; i < 3; i++)
        {
            SetColorForButton(i, colors[i], colorNames[i]);
        }
    }
    #endregion
}
