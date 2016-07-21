using UnityEngine;
using System.Collections;

public class AutoRotate : OptimizedGameObject
{
	public Vector3 speed = new Vector3( 0, 40f, 0);
	
	void Update ()
	{
		cachedTransform.Rotate(speed * Time.deltaTime);
	}
}
