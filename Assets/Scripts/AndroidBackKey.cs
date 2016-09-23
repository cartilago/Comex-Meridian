using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AndroidBackKey : MonoBehaviour
{
	void Update ()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            GetComponent<Button>().onClick.Invoke();
        }
	}
}
