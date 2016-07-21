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

    private int colorWidgetCount = 36;

    private List<GameObject> colorWidgets = new List<GameObject>();
    #endregion

    #region MonoBehaviour overrides
    private void Start ()
    {
        GetColorWidgets(trendsRoot, colorWidgetPrefab);
        GetColorWidgets(topColorRoot, colorWidgetPrefab);
        GetCombinationWidgets(combinationsRoot, combinationWidgetPrefab);
    }
    #endregion

    #region Class implementation
    private void GetColorWidgets(RectTransform t, GameObject prefab)
    {
        for (int i = 0; i < colorWidgetCount; i++)
        {
            GameObject go = GameObject.Instantiate(prefab);
            go.transform.SetParent(t, false);
            go.GetComponent<Graphic>().color = new Color(Random.value, Random.value, Random.value);
            colorWidgets.Add(go);
            go.GetComponent<Button>().onClick.AddListener(() => PickColor(go));
            go.GetComponent<ColorWidgetBase>().waitToShow = i * 0.05f;
        }
    }

    private void GetCombinationWidgets(RectTransform t, GameObject prefab)
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
    private GameObject buttonToAddColorTo;

    public void AddColor(GameObject button)
    {
        buttonToAddColorTo = button;
        this.gameObject.SetActive(true);
    }

    public void PickColor(GameObject colorWidget)
    {
        gameObject.SetActive(false);

        // Remove old
        if (buttonToAddColorTo.GetComponentInChildren<ColorWidget>() != null)
        {
            GameObject goToRemove = buttonToAddColorTo.GetComponentInChildren<ColorWidget>().gameObject;
            DestroyImmediate(goToRemove);
        }

        if (buttonToAddColorTo.GetComponentInChildren<CombinationWidget>() != null)
        {
            GameObject goToRemove = buttonToAddColorTo.GetComponentInChildren<CombinationWidget>().gameObject;
            DestroyImmediate(goToRemove);
        }

        GameObject go = GameObject.Instantiate(colorWidget);
        go.transform.SetParent(buttonToAddColorTo.transform, false);
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one;
        go.GetComponent<Graphic>().raycastTarget = false;
        go.transform.SetAsFirstSibling();

    }

    public void PickCombination(GameObject combinationWidget)
    {
        gameObject.SetActive(false);

        // Remove old
        if (buttonToAddColorTo.GetComponentInChildren<ColorWidget>() != null)
        {
            GameObject goToRemove = buttonToAddColorTo.GetComponentInChildren<ColorWidget>().gameObject;
            DestroyImmediate(goToRemove);
        }

        if (buttonToAddColorTo.GetComponentInChildren<CombinationWidget>() != null)
        {
            GameObject goToRemove = buttonToAddColorTo.GetComponentInChildren<CombinationWidget>().gameObject;
            DestroyImmediate(goToRemove);
        }

        GameObject go = GameObject.Instantiate(combinationWidget);
        go.transform.SetParent(buttonToAddColorTo.transform, false);
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one;
        go.GetComponent<CombinationWidget>().buttonGraphic.raycastTarget = false;
        go.transform.SetAsFirstSibling();

    }
    #endregion
}
