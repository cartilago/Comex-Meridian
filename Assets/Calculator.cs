using UnityEngine;
using UnityEngine.UI;

using System.Collections;

public class Calculator : MonoBehaviour
{
    public InputField height;
    public InputField width;
    public InputField windowCount;

    public GameObject resultPanel;
    public Text resultText;

    public void GetTotal()
    {
        float h = 0;
        float.TryParse(height.text, out h);

        float w = 0;
        float.TryParse(width.text, out w);

        int wc = 0;
        int.TryParse(windowCount.text, out wc);

        resultText.text = string.Format("Necesitarás {0:0.00} lts", ((h * w * .0909f) - (wc * .02f)).ToString());

        resultPanel.SetActive(true);
    }
}
