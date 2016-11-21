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
    public GameObject colorPageButtonPrefab;

    public RectTransform trendsRoot;
    public RectTransform topColorRoot;
    public RectTransform combinationsRoot;
    public RectTransform allColorsRoot;
    public RectTransform colorPageButtonsRoot;

    public List<ColorButton> colorButtons;
    public List<GameObject> colorButtonHighlights;

    private int colorWidgetCount = 36;
    private int currentColor = 0;

    private List<GameObject> colorWidgets = new List<GameObject>();
    #endregion

    #region MonoBehaviour overrides
    private void Start ()
    {
        CreateGetColorWidgets(allColorsRoot, colorWidgetPrefab);
       // CreateGetColorWidgets(topColorRoot, colorWidgetPrefab);
        CreateCombinationWidgets(combinationsRoot, combinationWidgetPrefab);

       //ColorWidget random = colorButtons[Random.Range(0, colorButtons.Count)].GetComponentInChildren<ColorWidget>(true);
       // SetColorForButton(0, random.color, random.name);
        SetColorForButton(0, new Color(.55f, .6f, .55f), "MK5-13\nOzono");
        gameObject.SetActive(false);
    }
    #endregion

    #region Class implementation
    private void CreateGetColorWidgetsRandom(RectTransform t, GameObject prefab)
    {
        // Read CMYK colors from text file;
        string[] lines = colorCSVFile.text.Split("\n"[0]);

        int maxlines = 75;

        for (int i = 0; i < /*lines.Length*/ maxlines; i++)
        {
            string[] rows = lines[Random.Range(0,lines.Length-1)].Split(',');

            GameObject go = GameObject.Instantiate(prefab);
            go.transform.SetParent(t, false);    
            colorWidgets.Add(go);
            go.GetComponent<Button>().onClick.AddListener(() =>  PickColorForCurrentColorButton(go));
            go.GetComponent<ColorWidget>().waitToShow = i * 0.05f;
            go.GetComponent<ColorWidget>().label.text = rows[0] + '\n' + rows[1];
            go.GetComponent<Graphic>().color = CMYKToColor32(float.Parse(rows[2]), float.Parse(rows[3]), float.Parse(rows[4]), float.Parse(rows[5]));
        }
    }

    private void CreateGetColorWidgets(RectTransform t, GameObject prefab)
    {
        // Read CMYK colors from text file;
        string[] lines = colorCSVFile.text.Split("\n"[0]);
        List<string> pages = new List<string>();
        GameObject currentPageRoot = null;
        Toggle currentToggle = null;
        int colorIndex = 0;

        for (int i = 0; i < lines.Length; i++)
        {
            string[] rows = lines[i].Split(',');
            string page = rows[0].Split('-')[0].Remove(2);

            // Create pages
            if (pages.Contains(page) == false)
            {
                pages.Add(page);
                GameObject pageRoot = currentPageRoot = new GameObject(page);
                RectTransform rt = currentPageRoot.AddComponent<RectTransform>();
                rt.SetParent(trendsRoot, false);
                /*
                rt.anchorMin = new Vector2(0, 0);
                rt.anchorMax = new Vector2(1, 1);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;*/
                currentPageRoot.SetActive(false);

                // Add grid component
                GridLayoutGroup glg = currentPageRoot.AddComponent<GridLayoutGroup>();
                int margin = 0;
                glg.cellSize = new Vector2(210-margin, 210-margin);
                glg.spacing = new Vector2(20 + margin, 20 + margin);
                glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                glg.constraintCount = 3;
                glg.childAlignment = TextAnchor.UpperCenter;

                ContentSizeFitter csf = currentPageRoot.AddComponent<ContentSizeFitter>();
                csf.horizontalFit = ContentSizeFitter.FitMode.MinSize;
                csf.verticalFit = ContentSizeFitter.FitMode.MinSize;


                // CreateButton
                GameObject pageButton = GameObject.Instantiate(colorPageButtonPrefab);
                pageButton.transform.SetParent(colorPageButtonsRoot, false);
                pageButton.name = pageButton.GetComponentInChildren<Text>().text = page;
                Toggle toggle = currentToggle = pageButton.GetComponent<Toggle>();
                toggle.group = colorPageButtonsRoot.GetComponent<ToggleGroup>();
                toggle.onValueChanged.AddListener((value) => { pageRoot.SetActive(value); });

                colorIndex = 0;
            }

            // Create button
            GameObject go = GameObject.Instantiate(prefab);
            go.transform.SetParent(currentPageRoot.transform, false);
            colorWidgets.Add(go);
            //go.GetComponent<Button>().onClick.AddListener(() => PickColorForCurrentColorButton(go));

            go.GetComponent<ColorWidget>().label.text = rows[0] + '\n' + rows[1];
            go.GetComponent<Graphic>().color = CMYKToColor32(float.Parse(rows[2]), float.Parse(rows[3]), float.Parse(rows[4]), float.Parse(rows[5]));
            if (colorIndex == 11)
            {
                ColorBlock cb = currentToggle.colors;
                cb.normalColor = go.GetComponent<Graphic>().color;
                currentToggle.colors = cb;
            }

            colorIndex++;
        }

        colorPageButtonsRoot.GetChild(0).GetComponent<Toggle>().isOn = true;

       

        /*

        for (int i = 0; i < 70; i++)
        {
            //string[] rows = lines[Random.Range(0, lines.Length - 1)].Split(',');
            string[] rows = lines[i].Split(',');
            GameObject go = GameObject.Instantiate(prefab);
            go.transform.SetParent(t, false);
            colorWidgets.Add(go);
            go.GetComponent<Button>().onClick.AddListener(() => PickColorForCurrentColorButton(go));
            go.GetComponent<ColorWidget>().waitToShow = 0; // i * 0.05f;
            go.GetComponent<ColorWidget>().label.text = rows[0] + '\n' + rows[1];
            go.GetComponent<Graphic>().color = CMYKToColor32(float.Parse(rows[2]), float.Parse(rows[3]), float.Parse(rows[4]), float.Parse(rows[5]));
            Debug.Log(rows[0].Split('-')[0].Remove(2));
        }*/
    }

    private void SetPageActive(GameObject page, bool value)
    {
        Debug.Log(page.name);
    }

    public static Color32 CMYKToColor32(float C, float M, float Y, float K)
    {
    	C*= .01f; M*= .01f; Y*=.01f; K*=.01f;
        byte r = (byte)(255 * (1 - C) * (1 - K));
        byte g = (byte)(255 * (1 - M) * (1 - K));
        byte b = (byte)(255 * (1 - Y) * (1 - K));

        return new Color32(r, g, b, 255);
    }

    private void CreateCombinationWidgets(RectTransform t, GameObject prefab)
    {
		// Read CMYK colors from text file;
        string[] lines = colorCSVFile.text.Split("\n"[0]);

        for (int i = 0; i < colorWidgetCount; i++)
        {
            GameObject go = GameObject.Instantiate(prefab);
            go.transform.SetParent(t, false);
			colorWidgets.Add(go);

			string[] rows = lines[Random.Range(0,lines.Length-1)].Split(',');
			go.GetComponent<CombinationWidget>().colorNames[0] = rows[0] + '\n' + rows[1];
			go.GetComponent<CombinationWidget>().color1 = CMYKToColor32(float.Parse(rows[2]), float.Parse(rows[3]), float.Parse(rows[4]), float.Parse(rows[5]));
			rows = lines[Random.Range(0,lines.Length-1)].Split(',');
			go.GetComponent<CombinationWidget>().colorNames[1] = rows[0] + '\n' + rows[1];
			go.GetComponent<CombinationWidget>().color2 = CMYKToColor32(float.Parse(rows[2]), float.Parse(rows[3]), float.Parse(rows[4]), float.Parse(rows[5]));
			rows = lines[Random.Range(0,lines.Length-1)].Split(',');
			go.GetComponent<CombinationWidget>().colorNames[2] = rows[0] + '\n' + rows[1];
			go.GetComponent<CombinationWidget>().color3 = CMYKToColor32(float.Parse(rows[2]), float.Parse(rows[3]), float.Parse(rows[4]), float.Parse(rows[5]));
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

        FingerCanvas.Instance.UpdateBrushColor();
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
            DecoratorPanel.Instance.photoRenderer.material.SetColor("_Color" + (index + 1).ToString(), newColorWidget.color);
        }
        else
        {
            colorButtons[index].addColorLabel.gameObject.SetActive(true);
            colorButtons[index].selectColorButton.gameObject.SetActive(false);
            colorButtons[index].SetColorWidget(null);

            // Pass the color to the shader
            DecoratorPanel.Instance.photoRenderer.material.SetColor("_Color" + (index + 1).ToString(), Color.black);
        }
    }

    public void PickColorForCurrentColorButton(GameObject colorWidget)
    {
        gameObject.SetActive(false);

        colorButtons[currentColor].addColorLabel.gameObject.SetActive(false);
        colorButtons[currentColor].selectColorButton.gameObject.SetActive(true);

        ColorWidget newColorWidget = Instantiate(colorWidget.gameObject).GetComponent<ColorWidget>();
        colorButtons[currentColor].SetColorWidget(newColorWidget);

        // Pass color information to shader
        switch (currentColor)
        {
            case 0: DecoratorPanel.Instance.photoRenderer.material.SetColor("_Color1", newColorWidget.GetComponent<Graphic>().color); break;
            case 1: DecoratorPanel.Instance.photoRenderer.material.SetColor("_Color2", newColorWidget.GetComponent<Graphic>().color); break;
            case 2: DecoratorPanel.Instance.photoRenderer.material.SetColor("_Color3", newColorWidget.GetComponent<Graphic>().color); break;
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
			newColorWidget.colorName = combinationWidget.GetComponent<CombinationWidget>().colorNames[currentColor];
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
