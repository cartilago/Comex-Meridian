using UnityEngine;
using System.Collections;

namespace Meridian.Framework.Utils
{
	/// <summary>
	/// Mono singleton.
	/// Created by Jorge L. Chavez Herrera
	/// 
	/// Gerenric singleton class
	/// Derive all singleton objects from MonoSingleton.
	/// </summary>
	public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
	{
		private static bool applicationIsQuitting = false;

		#region Class accessors
		private static T instance = null;
		public static T Instance
		{
			get
			{
				if (applicationIsQuitting) {
					Debug.LogWarning("[Singleton] Instance '"+ typeof(T) +
					                 "' already destroyed on application quit." +
					                 " Won't create again - returning null.");
					return null;
				}
				// Instance requiered for the first time, we look for it
				if( instance == null )
				{
					instance = GameObject.FindObjectOfType(typeof(T)) as T;

					if ( FindObjectsOfType(typeof(T)).Length > 1 )
					{
						Debug.LogError("[Singleton "+typeof(T)+"] Something went wrong, more than 1 instance found!");
						return instance;
					}

					// Object not found, we create a temporary one
					if( instance == null )
					{
						// Debug.LogWarning("No instance of " + typeof(T).ToString() + ", a temporary one is created.");
						instance = new GameObject("Temp Instance of " + typeof(T).ToString(), typeof(T)).GetComponent<T>();
						
						// Problem during the creation, this should not happen
						if( instance == null )
						{
							// Debug.LogError("Problem during the creation of " + typeof(T).ToString());
						}
					}
					instance.Init();
				}
				return instance;
			}
		}
		
		#endregion
		
		#region MonoBehaviour overrides
		
		// If no other monobehaviour request the instance in an awake function
		// executing before this one, no need to search the object.
		void Awake ()
		{
			if ( instance == null )
			{
				instance = this as T;
				instance.Init();
			}
		}

		// This function is called when the instance is used the first time
		// Put all the initializations you need here, as you would do in Awake
		public virtual void Init () {}

		// Removes the instance when the object is destroyed.
		public virtual void OnDestroy()
		{
			instance = null;
		}

		public virtual void OnApplicationQuit()
		{
			applicationIsQuitting = true;
		}

		#endregion
	}

	/// <summary>
	/// This class is a singleton. Only one instance of this class can exist.
	/// </summary>
	public class Singleton<T> where T : class, new()
	{
		private static T instance;
		/// <summary>
		/// The singleton instance of this class.
		/// </summary>
		public static T Instance
		{
			get
			{
				if(instance == null)
				{
					instance = new T();
				}
				return instance;
			}
		}
		/// <summary>
		/// Destroys the singleton. Important for cleaning up the static reference.
		/// </summary>
		public void Destroy ()
		{
			instance = null;
		}
	}

}