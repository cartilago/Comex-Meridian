using UnityEngine;
using System.Collections;

public class NewImagePanel : MonoBehaviour
{
    public GameObject buttonsGrid;
    public GameObject importingProgress;

    public void Show()
    {
        gameObject.SetActive(true);
        buttonsGrid.SetActive(true);
        importingProgress.SetActive(false);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void ShowProgress()
    {
        buttonsGrid.SetActive(false);
        importingProgress.SetActive(true);
    }
}
