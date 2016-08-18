using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour 
{
	#region Class members
	public Panel[] panels;
	public GameObject[] menuHighlights;
	#endregion

	#region MonoBehaviour overrides
	private void Start()
	{
		ShowPanel(0);
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

		GetComponentInChildren<TweenTransform>().TweenOut();
	}
	#endregion
}
