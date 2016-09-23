using UnityEngine;
using System.Collections;

public class AutoRotate : OptimizedGameObject
{
	public Vector3 speed = new Vector3( 0, 40f, 0);
	
	void FixedUpdate ()
	{
        //cachedTransform.Rotate(speed * Time.deltaTime);
        cachedTransform.localEulerAngles = new Vector3(0, 0, -Mathf.Floor(Time.time * 10) * (360 / 8));
	}
}
