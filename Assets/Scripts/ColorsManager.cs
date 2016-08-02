using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class ColorsManager : MonoBehaviour
{
    #region Class members
    public GameObject colorWidgetPrefab;
    public GameObject combinationWidgetPrefab;

    public RectTransform trendsRoot;
    public RectTransform topColorRoot;
    public RectTransform combinationsRoot;

    public List<GameObject> colorButtons;
    public List<GameObject> colorButtonHighlights;
    private GameObject currentColorButton;

    private int colorWidgetCount = 36;

    private List<GameObject> colorWidgets = new List<GameObject>();
    #endregion

    #region MonoBehaviour overrides
    private void Start ()
    {
        CreateGetColorWidgets(trendsRoot, colorWidgetPrefab);
        CreateGetColorWidgets(topColorRoot, colorWidgetPrefab);
        CreateCombinationWidgets(combinationsRoot, combinationWidgetPrefab);
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
            go.GetComponent<Button>().onClick.AddListener(() =>  PickColor(go));
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

    public void AddColor(GameObject button)
    {
        currentColorButton = button;
        this.gameObject.SetActive(true);
    }

    public void SetCurrentColor(int index)
    {
        for (int i = 0; i < colorButtonHighlights.Count; i++)
        {
            colorButtonHighlights[i].SetActive(i == index);
        }
    }

    public void PickColor(GameObject colorWidget)
    {
        gameObject.SetActive(false);

        // Remove old color
        if (currentColorButton.GetComponentInChildren<ColorWidget>() != null)
        {
            GameObject goToRemove = currentColorButton.GetComponentInChildren<ColorWidget>().gameObject;
            DestroyImmediate(goToRemove);
        }

        // Add new color
        GameObject go = GameObject.Instantiate(colorWidget);
        go.transform.SetParent(currentColorButton.transform, false);
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one;
        go.GetComponent<Graphic>().raycastTarget = false;
        go.transform.SetAsFirstSibling();

        // Pass color information to shader
        int index = colorButtons.IndexOf(currentColorButton);

        switch (index)
        {
            case 0: Decorator.Instance.photoRnderer.material.SetColor("_Color1", go.GetComponent<Graphic>().color); break;
            case 1: Decorator.Instance.photoRnderer.material.SetColor("_Color2", go.GetComponent<Graphic>().color); break;
            case 2: Decorator.Instance.photoRnderer.material.SetColor("_Color3", go.GetComponent<Graphic>().color); break;
        }
    }

    public void PickCombination(GameObject combinationWidget)
    {
        gameObject.SetActive(false);

        // Remove old colors from buttons
        for (int i = 0; i < colorButtons.Count; i++)
        {
            ColorWidget cg = colorButtons[i].GetComponentInChildren<ColorWidget>();

            if (cg != null)
                DestroyImmediate(cg.gameObject);
        }

        // Add combination colors to buttons
        for (int i = 0; i < colorButtons.Count; i++)
        {
            // Add new color
            GameObject go = GameObject.Instantiate(colorWidgetPrefab);
            go.transform.SetParent(colorButtons[i].transform, false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
            go.GetComponent<Graphic>().color = combinationWidget.GetComponent<CombinationWidget>().graphics[i].color;
            go.GetComponent<Graphic>().raycastTarget = false;
            go.transform.SetAsFirstSibling();

            switch (i)
            {
                case 0: Decorator.Instance.photoRnderer.material.SetColor("_Color1", go.GetComponent<Graphic>().color); break;
                case 1: Decorator.Instance.photoRnderer.material.SetColor("_Color2", go.GetComponent<Graphic>().color); break;
                case 2: Decorator.Instance.photoRnderer.material.SetColor("_Color3", go.GetComponent<Graphic>().color); break;
            }
        }
    }
    #endregion
}
