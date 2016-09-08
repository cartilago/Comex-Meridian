using UnityEngine;
using System.Collections;

public class AppSettingsManager : MonoBehaviour
{
	// Use this for initialization
	void Start ()
    {
        Application.targetFrameRate = 60;
        Screen.sleepTimeout = SleepTimeout.NeverSleep; 
	}
}
