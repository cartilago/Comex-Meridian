using UnityEngine;
using System.Collections;


public static class MonoBehaviourExt {
	public static T GetComponentInChildren2<T>( this GameObject self, bool alsoInactive=false ) where T : MonoBehaviour {
		return MonoBehaviourExt.Iterate<T>( self, alsoInactive );
	}
	static T Iterate<T>( GameObject go, bool alsoInactive ) where T : MonoBehaviour {
		//Debug.LogError("Iterate");
		if( go.activeInHierarchy == false && !alsoInactive ) {
			//Debug.LogError("No inactive allowed");
			return null;
		}

		foreach( var owned_b in go.GetComponents<MonoBehaviour>() ) {
			if( owned_b.GetType()==typeof(T) ) {
				//Debug.LogError("type is " + owned_b.GetType().Name );
				return (T)owned_b;
			}
		}

		for( int i = 0; i<go.transform.childCount; i++ ) {
			var ch = go.transform.GetChild(i);
			T ret = Iterate<T>( ch.gameObject, alsoInactive );
			if( ret != null ) {
				return ret;
			}
		}

		return null;
	}
} 
