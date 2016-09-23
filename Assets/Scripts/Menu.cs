using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Menu : MonoBehaviour 
{
	#region Class members
	public Panel[] panels;
	public GameObject[] menuHighlights;
    public GameObject fader;
    public GameObject slideMenuRoot;
	#endregion

	#region MonoBehaviour overrides
	private void Start()
	{
		ShowPanel(2);
	}

    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            Hide();
        }
    }
    #endregion

    #region Class implementation
    public void ShowPanel(int activeIndex)
	{
		for (int i = 0; i < panels.Length; i++)
		{
			if (panels[i] != null) panels[i].gameObject.SetActive(i == activeIndex);
			if (menuHighlights[i] != null) menuHighlights[i].gameObject.SetActive(i == activeIndex);
		}

        Hide();
	}

    public void Show()
    {
        slideMenuRoot.GetComponentInChildren<TweenTransform>(true).TweenIn();
        fader.GetComponentInChildren<TweenColor>(true).TweenIn();
        fader.GetComponentInChildren<MaskableGraphic>(true).raycastTarget = true;
    }

    public void Hide()
    {
        slideMenuRoot.GetComponentInChildren<TweenTransform>(true).TweenOut();
        fader.GetComponentInChildren<TweenColor>(true).TweenOut();
        fader.GetComponentInChildren<MaskableGraphic>(true).raycastTarget = false;
    }

    public void Quit()
    {
        Application.Quit();
    }
	#endregion
}
