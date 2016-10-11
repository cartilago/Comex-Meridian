#pragma warning disable 414
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;

namespace Tastybits.NativeGallery {


	/**
	 * Canvas Helper
	 */
	public class CanvasHelper {

		public static UnityEngine.Canvas GetCanvas( bool createIfNotExists=true ) {
			UnityEngine.Canvas canvas=null;
			var canvascomponents = UnityEngine.Component.FindObjectsOfType<UnityEngine.Canvas>();	

			// First try and find the object with the canvas on it that has the name Canvas and si active in hierarchy.
			foreach( var component in canvascomponents ) {
				// CONSIDER: also looking into the gameobject returned to see if the object has a set of Cavas components usually 
				// auto created by unity.
				if( component.enabled==true && component.name == "Canvas" && component.gameObject.activeInHierarchy == true ) {
					canvas=component;
					break;
				}
			}

			// Consider looking though hidden objects as well.

			// If the canvas is sstill null and there are other canvas components
			// avialable return one of them which is active.
			if (canvas == null) {
				foreach (var component in canvascomponents) {
					if (component.enabled == true ) {
						canvas = component;
						break;
					}
				}
			}

			// if the canvas is still null we will generate a canvas for our use.
			if( canvas == null && createIfNotExists ) {
				Debug.Log("MobyShop: Canvas not found - creating one.");
				var goCanvas = new GameObject("Canvas");
				canvas = goCanvas.AddComponent<UnityEngine.Canvas>();
				canvas.pixelPerfect = false;
				canvas.sortingOrder = 0;
				canvas.renderMode = RenderMode.ScreenSpaceOverlay;
				var cs = goCanvas.AddComponent<UnityEngine.UI.CanvasScaler>();
				cs.scaleFactor = 1f;
				cs.referencePixelsPerUnit = 100;
				cs.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ConstantPixelSize;
				var rc = goCanvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();
				rc.ignoreReversedGraphics = true;
				rc.blockingObjects = UnityEngine.UI.GraphicRaycaster.BlockingObjects.None;
				/*var rt = */goCanvas.AddComponent<RectTransform>();
				if( Component.FindObjectOfType<UnityEngine.EventSystems.EventSystem>()==null ) {
					var goEventSystem = new GameObject("EventSystem");
					/*var es = */goEventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
					/*var im = */goEventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
				}
			}
			return canvas;
		}

	}





	public class Job : MonoBehaviour {
		static Job instance = null;


		Job():base() {
			instance = this;
			initialized = false;  
		}


		public void EditCmdFindDuplicates() {
			var comps = Component.FindObjectsOfType(typeof(Job));
			Debug.Log("comps : " + comps.Length );
		}

		public bool manualUpdate=false;

		public bool releaseManualUpdate = false;


		public void CmdReleaseManualUpdate() {
			releaseManualUpdate=true;
		}

		bool initialized = false;



		[System.Serializable]
		public class Action {
			public Action() {
				#if UNITY_EDITOR
				// Track where on the stack we are in the Unity editor...
				stack = StackTraceUtility.ExtractStackTrace().Split( new char[] { '\n' } , System.StringSplitOptions.RemoveEmptyEntries );
				#endif 
			}
			public string[] stack;
			public System.Action cb;
			public System.Action<float,float> cb2;
			public float timeCount = 0f;
			//public DefaultInputAction<List<object>> intermediateCallback = null;
			public int type = 99;
			public bool kill=false;
			public float delay = 0;
			public int frames = 0;
			public int state = 0;
			public void Invoke() {
				if( !kill && cb != null  ){
					cb();
					cb = null;
					kill = true;
				}
			}
			public void InvokeNoKill() {
				if( !kill && cb != null  ){
					cb();
				}
			}
			public void InvokeNoKillForWaitWithStep( float absTime, float dt ) {
				if( !kill && cb2 != null  ){
					cb2.Invoke( absTime, dt );
				}
			}
			public void Die() {
				kill = true;
				cb = null;
				cb2 = null;
				//intermediateCallback = null;
			}
		}
		System.Collections.Generic.List<Action> actions = new System.Collections.Generic.List<Action>();
		System.Collections.Generic.List<Action> toBeAdded = new System.Collections.Generic.List<Action>();
		System.Collections.Generic.List<Action> toBeRemoved = new System.Collections.Generic.List<Action>();
		public bool processing=false;
		bool killCurrentStep=false;

		// Use this for initialization
		void Awake () {
			this.hideFlags |= HideFlags.HideInHierarchy;
			//Debug.LogError("Worker initliazed");
			if( instance != null && instance != this ) {
				Debug.LogError("Error there is only supposed to be one instance of this..." );
			}
			instance = this;
			initialized = true;
			//actions.Clear();
			toBeAdded.Clear();
			toBeRemoved.Clear();
			processing=false;
			killCurrentStep=false;
		}


		// Update is called once per frame
		void Update () {
			if( manualUpdate ) {
				if( releaseManualUpdate ) {
					releaseManualUpdate = false;
					UpdateImpl();
				}
				return;
			}
			UpdateImpl();
		}

		void UpdateImpl() {
			if( toBeAdded.Count > 0 ) {
				foreach( var item in toBeAdded ) {
					if( verbose ) Debug.Log ("Adding action : " + item.type );
					var tmp = new Action();
					tmp = item;
					tmp.kill = false;
					actions.Add( tmp );
				}
				toBeAdded.Clear();
			}

			processing = true;
			float dt = Time.deltaTime;
			if( dt <= 0f && Application.isPlaying == false ) {
				dt = 0.5f;
			}
			foreach( Action a in actions ) {
				killCurrentStep=false;

				if( !a.kill ) {
					if( a.type == 0 ) {
						a.delay -= dt;
						if( a.delay <= 0 ) {
							//Debug.Log ("updating process");
							a.Invoke();
							//Debug.Log( "action flagged as kill" );
							a.kill = true;
						}
					} 
					else if( a.type == 1 ) {
						if( a.frames - Time.frameCount <= 0 ) {
							//Debug.Log ("updating process");
							a.Invoke();
							//Debug.Log( "action flagged as kill" );
							a.kill = true;
							//Debug.LogError( "finished wait for frames at frame number : " + Time.frameCount );
						}
					}
					else if( a.type == 2 ) {
						//Debug.Log ("updating process");
						killCurrentStep=false;
						a.kill = false;
						a.InvokeNoKill(); // call every frame.... until KillCurrentIsCalled
						if( killCurrentStep ) {
							a.kill = true;
						} 
					}
					else if( a.type == 22 ) {
						//Debug.Log ("updating process");
						a.delay -= dt;
						killCurrentStep=false;
						a.kill = false;
						if( a.delay <= 0f ) { 
							dt -= Mathf.Abs(a.delay);
							a.delay = 0f;
						}
						a.timeCount += dt;
						a.InvokeNoKillForWaitWithStep( a.timeCount, dt ); // call every frame.... until KillCurrentIsCalled
						if( killCurrentStep || a.delay <= 0 ) {
							if(a.cb!=null) a.cb();
							a.cb = null;
							a.kill = true;
						} 
					}
					else if( a.type == 3 ) {
						a.delay -= dt;
						if( a.delay <= 0 ) {
							//Debug.Log ("updating process");
							a.Invoke();
							a.state = -1;
							//Debug.Log( "action flagged as kill" );
							a.kill = true;
						}
					}
				} else {
					if( verbose ) Debug.Log ("we are interating on a objet with was flagged for killed");
				}
				if( a.kill == true ) {
					toBeRemoved.Add( a );
				}
			}
			processing = false;

			foreach( Action a in toBeRemoved ) {
				if( verbose ) Debug.Log ("Removing action : " + a.type );
				actions.Remove( a );
			}
			toBeRemoved.Clear();
		}


		public bool verbose = false;


		private static void EnsureCreated() {
			if( instance == null ) {
				var inst = Component.FindObjectOfType<Job>();
				if( inst != null ) {
					instance = inst;
					if( instance.name != "Job" ) {
						Debug.LogError("Error the name of gameobject was : " + inst.name + " but was expected to be 'Worker'" );
					}
					return;
				}

				//Debug.Log("Creating job gameobject");
				GameObject go = null;
				if( Application.isPlaying==false ) {
					go = GameObject.Find( "Job" );
				}
				if( go == null ) {
					go = new GameObject( "Job" );
				}
				if( go.GetComponent<Job>() != null ) {
					instance = go.GetComponent<Job>() as Job;
				} else {
					instance = go.AddComponent<Job>() as Job;
				}
			}
		}

		public static Action WaitForSeconds( float t, System.Action a ) {
			EnsureCreated();
			return AddAction( 0, t, 0, a );
		}




		public static Action WaitForSecondsOrClick( float t, System.Action a ) {
			EnsureCreated();
			return AddAction( 0, t, 0, a );
		}


		public static void WaitForFrames( int frames, System.Action a ) {
			EnsureCreated();
			//Debug.LogError ("WaitForFrames");
			AddAction( 1, 0, frames, a );
		}


		static IEnumerator WaitForEndOfFrameImpl( ) {
			yield return new WaitForEndOfFrame();
		}


		public static void WaitForEndOfFrame( System.Action callback ) {
			EnsureCreated ();
			StartAndWaitforCoroutine( WaitForEndOfFrameImpl(), callback );
		}	


		public static void Step ( System.Action a ) {
			EnsureCreated();
			AddAction( 2, 0, 0, a );
		}



		public static Action StepWaitForSeconds( float time, System.Action<float,float> c2, System.Action c ) {
			EnsureCreated();
			Action a = AddAction( 22, time, 0, c );
			a.cb2 = c2;
			return a;
		}


		public static Action AnimateValue( float duration, float startVal, float endVal, System.Action<float> setter, System.Action done ) {
			return Job.StepWaitForSeconds( duration, ( float absTime, float dt )=>{
				//Debug.Log("Step wait... : " + absTime + " dt : " + dt );
				float value = Mathf.SmoothStep( startVal, endVal, absTime  / duration );
				setter( value );
			}, done );
		}


		public static void OnUpdate( System.Action a ) {
			EnsureCreated();
			AddAction( 2, 0, 0, a );
		}


		// TODO: In the future we can experiement with adding this type of 
		// timeout functionality to the code... an action that is supported to call a callback
		// might fail or might not call it right for some reason. This can be handled with a 
		// timedout callback which creates an extra callback and ticks a timer down 
		public static System.Action TimedOutCallback (System.Action cb ) {
			return cb;
		}


		public static System.Action<bool> TimedOutCallback (System.Action<bool> cb ) {
			return cb;
		}



		// KRM: 
		// This is kind of experimental.
		// 
		// this is a really nifty technique of doing a manual timeout overwrite to
		// all kinds of requst be it networking , waiting for animation or some other logic to trigger.
		// one areas of use is to apply it to things that will caurse errors, becourse seomthing doesnt return
		// maybe some callback of some library that you dont control... doesnt seem to callback within reasonable timeframe, anyways you want to be in control of 
		// your code so you dont want to run some logic in a unreliable matter later on hwen the library wants to do it. 
		// another case is that a callback is NEVER invoked for some unknown internal reason. Then this is a nifty way of handling that in a relaible way.
		/*public static DefaultInputAction<List<Challenge>>  CreateTimedOutCallback2( System.Action< List<Challenge> > realCallbackImpl, System.Action< System.Action< List<Challenge> > > timedOutCallbackImpl ) {
			float standardTimeOutCfg = 30.0f; // after 5 seconds of trying we will timeout...
			Action a = null;
			a = WorkerAction.WaitForSeconds( standardTimeOutCfg, ()=>{
				if( a.state == 1 ) {
					//Debug.LogError( "did callback did callback so no need to time out" );
				} else if( a.state == 0 ) {
					Debug.LogWarning( "Warning request did timeout after : " + standardTimeOutCfg );
					a.state = -1;
					timedOutCallbackImpl( realCallbackImpl );
				} else {
					Debug.LogError( "unknown state : " + a.state );
				}
			});
			a.state = 0;
			a.type = 3;
			DefaultInputAction<List<Challenge>> intermediateCallback = ( List<Challenge> challList )=> {
				if( a.state == -1 ) {
					Debug.LogWarning( "Warning request was timed out!" );
				} else if( a.state == 0 ) {
					//Debug.LogError( "intermediate callback called" );
					a.state = 1;
					realCallbackImpl( challList );
				} else {
					Debug.LogError( "unknown state : " + a.state );
				}
			};
			a.intermediateCallback = intermediateCallback;
			return intermediateCallback;
		}*/


		/*public static Action  CreateTimedOutCallback2_RetAction( System.Action< List<Challenge> > realCallbackImpl, System.Action< System.Action< List<Challenge> > > timedOutCallbackImpl ) {
		float standardTimeOutCfg = 30.0f; // after 5 seconds of trying we will timeout...
		Action a = null;
		a = WorkerAction.WaitForSeconds( standardTimeOutCfg, ()=>{
			if( a.state == 1 ) {
				//Debug.LogError( "did callback did callback so no need to time out" );
			} else if( a.state == 0 ) {
				Debug.LogWarning( "Warning request did timeout after : " + standardTimeOutCfg );
				a.state = -1;
				timedOutCallbackImpl( realCallbackImpl );
			} else {
				Debug.LogError( "unknown state : " + a.state );
			}
		});
		a.state = 0;
		a.type = 3;
		DefaultInputAction<List<Challenge>> intermediateCallback = ( List<Challenge> challList )=> {
			if( a.state == -1 ) {
				Debug.LogWarning( "Warning request was timed out!" );
			} else if( a.state == 0 ) {
				//Debug.LogError( "intermediate callback called" );
				a.state = 1;
				realCallbackImpl( challList );
			} else {
				Debug.LogError( "unknown state : " + a.state );
			}
		};
		a.intermediateCallback = intermediateCallback;
		return a;
	}*/


	public static void KillCurrentStep() {
		if( instance.verbose ) Debug.Log ("KillCurrentStep called");
		instance.killCurrentStep = true;
	}


	public static void StopListen() {
		KillCurrentStep();
	}


	static System.Action tmpdeleg;
	static System.Action deleg1;
	static System.Action deleg2;
	static System.Action deleg3;
	static System.Action deleg4;
	static System.Action deleg5;
	static System.Action deleg6;
	static System.Action deleg7;
	static System.Action deleg8;
	static System.Action deleg9;
	static System.Action deleg10;


	public static Coroutine StartCo( IEnumerator ienum ) {
		EnsureCreated();
		return instance.StartCoroutine( ienum );
	}


	public static bool StartAndWaitforCoroutine( IEnumerator ienum, System.Action deleg ) {
		if( deleg1 == null ) {
			deleg1 = deleg;
			instance.StartCoroutine( instance.StartCoroutine1( ienum ) );
			return true;
		}
		else if( deleg2 == null ) {
			deleg2 = deleg;
			instance.StartCoroutine( instance.StartCoroutine2( ienum ) );
			return true;
		}
		else if( deleg3 == null ) {
			deleg3 = deleg;
			instance.StartCoroutine( instance.StartCoroutine3( ienum ) );
			return true;
		}
		else if( deleg4 == null ) {
			deleg4 = deleg;
			instance.StartCoroutine( instance.StartCoroutine4( ienum ) );
			return true;
		}
		else if( deleg5 == null ) {
			deleg5 = deleg;
			instance.StartCoroutine( instance.StartCoroutine5( ienum ) );
			return true;
		}
		else if( deleg6 == null ) {
			deleg6 = deleg;
			instance.StartCoroutine( instance.StartCoroutine6( ienum ) );
			return true;
		}
		else if( deleg7 == null ) {
			deleg7 = deleg;
			instance.StartCoroutine( instance.StartCoroutine7( ienum ) );
			return true;
		}
		else if( deleg8 == null ) {
			deleg8 = deleg;
			instance.StartCoroutine( instance.StartCoroutine8( ienum ) );
			return true;
		}
		else if( deleg9 == null ) {
			deleg9 = deleg;
			instance.StartCoroutine( instance.StartCoroutine9( ienum ) );
			return true;
		}
		else if( deleg10 == null ) {
			deleg10 = deleg;
			instance.StartCoroutine( instance.StartCoroutine10( ienum ) );
			return true;
		}
		Debug.LogError("Error: there is no slot available for the deledate to call");
		return false;
	}
	IEnumerator StartCoroutine1( IEnumerator ienum )  { yield return StartCoroutine( ienum ); tmpdeleg = deleg1;  deleg1 =null; if(tmpdeleg!=null) {tmpdeleg();} else {Debug.LogError("Error callback is null LOOK INTO THIS!!!"); } }
	IEnumerator StartCoroutine2( IEnumerator ienum )  { yield return StartCoroutine( ienum ); tmpdeleg = deleg2;  deleg2 =null; if(tmpdeleg!=null) {tmpdeleg();} else {Debug.LogError("Error callback is null LOOK INTO THIS!!!"); } }
	IEnumerator StartCoroutine3( IEnumerator ienum )  { yield return StartCoroutine( ienum ); tmpdeleg = deleg3;  deleg3 =null; if(tmpdeleg!=null) {tmpdeleg();} else {Debug.LogError("Error callback is null LOOK INTO THIS!!!"); } }
	IEnumerator StartCoroutine4( IEnumerator ienum )  { yield return StartCoroutine( ienum ); tmpdeleg = deleg4;  deleg4 =null; if(tmpdeleg!=null) {tmpdeleg();} else {Debug.LogError("Error callback is null LOOK INTO THIS!!!"); } }
	IEnumerator StartCoroutine5( IEnumerator ienum )  { yield return StartCoroutine( ienum ); tmpdeleg = deleg5;  deleg5 =null; if(tmpdeleg!=null) {tmpdeleg();} else {Debug.LogError("Error callback is null LOOK INTO THIS!!!"); } }
	IEnumerator StartCoroutine6( IEnumerator ienum )  { yield return StartCoroutine( ienum ); tmpdeleg = deleg6;  deleg6 =null; if(tmpdeleg!=null) {tmpdeleg();} else {Debug.LogError("Error callback is null LOOK INTO THIS!!!"); } }
	IEnumerator StartCoroutine7( IEnumerator ienum )  { yield return StartCoroutine( ienum ); tmpdeleg = deleg7;  deleg7 =null; if(tmpdeleg!=null) {tmpdeleg();} else {Debug.LogError("Error callback is null LOOK INTO THIS!!!"); } }
	IEnumerator StartCoroutine8( IEnumerator ienum )  { yield return StartCoroutine( ienum ); tmpdeleg = deleg8;  deleg8 =null; if(tmpdeleg!=null) {tmpdeleg();} else {Debug.LogError("Error callback is null LOOK INTO THIS!!!"); } }
	IEnumerator StartCoroutine9( IEnumerator ienum )  { yield return StartCoroutine( ienum ); tmpdeleg = deleg9;  deleg9 =null; if(tmpdeleg!=null) {tmpdeleg();} else {Debug.LogError("Error callback is null LOOK INTO THIS!!!"); } }
	IEnumerator StartCoroutine10( IEnumerator ienum ) { yield return StartCoroutine( ienum ); tmpdeleg = deleg10; deleg10 =null; if(tmpdeleg!=null) {tmpdeleg();} else {Debug.LogError("Error callback is null LOOK INTO THIS!!!"); } }


	static Action AddAction( int type, float t, int frames, System.Action a ) {
		Action _a = new Action();
		_a.cb = a;
		_a.delay = t;
		_a.type = type;
		_a.frames = Time.frameCount + frames;
		_a.kill = false;
		if( frames > 0 ) {
			//Debug.LogError( "added wait for frames at frame number : " + Time.frameCount );
		}
		if( instance.processing==false ) {
			if( instance.verbose ) Debug.Log ("adding action : " + _a.type + "  ( now )" );
			instance.actions.Add( _a );
		} else {
			if( instance.verbose ) Debug.Log ("adding action : " + _a.type + "  ( add later )" );
			instance.toBeAdded.Add( _a );
		}
		return _a;
	}


}


#if UNITY_EDITOR


public static class EditorJob {
	private class JobEntry {
		public double timeCnt=0;
		public JobEntry(Func<bool> _work, Action continueWith) {
			Work = _work;
			Work2 = null;
			Done = continueWith;
		}
		public JobEntry(Action _work, Action continueWith) {
			Work2 = _work;
			Work = null;
			Done = continueWith;
		}
		public Action Work2 { get; private set; }
		public Func<bool> Work { get; private set; }
		public Action Done { get; private set; }
	}
	private static readonly List<JobEntry> jobs = new List<JobEntry>();
	public static void Schedule( Action work ) {
		if (!jobs.Any()) {
			UnityEditor.EditorApplication.update += Update;
			//UnityEditor.EditorApplication.LockReloadAssemblies();
		}
		jobs.Add(new JobEntry(work, null));
	}
	public static void Schedule(Func<bool> work, Action onDone=null) {
		if (!jobs.Any()) {
			UnityEditor.EditorApplication.update += Update;
			//UnityEditor.EditorApplication.LockReloadAssemblies();
		}
		jobs.Add(new JobEntry(work, onDone));
	}
	private static void Update() {
		for (int i = 0; i >= 0; --i) {
			var jobIt = jobs[i];
			if( jobIt.Work2!=null ) {
				jobIt.Work2();
				if(jobIt.Done!=null)jobIt.Done();
				jobs.RemoveAt(i);
			}
			if( jobIt.Work!=null && jobIt.Work() ) {
				if(jobIt.Done!=null)jobIt.Done();
				jobs.RemoveAt(i);
			}
		}
		if (!jobs.Any()) {
			UnityEditor.EditorApplication.update -= Update;
			//UnityEditor.EditorApplication.UnlockReloadAssemblies();
		}
	}
}

#endif

	public class WWWUtil : MonoBehaviour {
		static WWWUtil instance = null;
		public WWWUtil() : base() {
			instance = null;
		}
		public WWW target;
		System.Action<WWW,bool> callback;
		bool running = false;
		public static void Wait( WWW www, System.Action<WWW,bool> _callback ) {
			if( instance == null ) {
				GameObject go = GameObject.Find( "Job");
				if( go == null ){
					go = new GameObject( "Job" );
				}
				WWWUtil comp = go.GetComponent<WWWUtil> () as WWWUtil;
				if( comp==null ) {
					comp = go.AddComponent<WWWUtil>() as WWWUtil;
				}
				instance = comp;
			}
			if( instance.running ) {
				_callback(null,false);
				return;
			}
			if( www == null ) { Debug.LogError( "Error WWW is null" ); }
			instance.running = true;
			instance.target = www;
			instance.callback = _callback;
			instance.StartCoroutine( instance.tmp() );
		}
		IEnumerator tmp () {
			yield return target;
			callback( target, string.IsNullOrEmpty(target.error) );
			target = null;
			running = false;
		}
	}



	public class NativeIOSDelegate : MonoBehaviour {
		public string randomId;
		bool invoked = false;
		float timeout=-1337f;
		System.Action< System.Collections.Hashtable > deleg;

		public static NativeIOSDelegate CreateNativeIOSDelegate( System.Action< System.Collections.Hashtable > methodToCall ) {
			int rndId = UnityEngine.Random.Range( 100000, 999999 );
			int iter = 0;
			while( GameObject.Find( "NativeDelegate_" + rndId ) != null && iter++ < 100 ) {
				rndId = UnityEngine.Random.Range( 100000, 999999 );
			}

			if( GameObject.Find( "NativeDelegate_" + rndId ) != null ) {
				Debug.LogError( "Error there is allready a callback existing" );
				return null;
			}

			GameObject delegRoot = GameObject.Find( "NativeDelegates" );
			if( delegRoot == null ) {
				delegRoot = new GameObject( "NativeDelegates");
			}


			GameObject goDeleg = new GameObject( "NativeDelegate_" + rndId );
			goDeleg.transform.parent = delegRoot.transform;
			NativeIOSDelegate test = goDeleg.AddComponent<NativeIOSDelegate>() as NativeIOSDelegate;
			test.deleg = methodToCall;

			return test;
		}


		void Update() {
			if( !(timeout <= -1337 ) ) {
				if( timeout > 0f ) {
					timeout -= Time.fixedTime;
					if( timeout <= 0f ) {
						OnTimeout();
					}
				}
			}
		}


		void OnTimeout() {

		}


		public void CallDelegateFromNative( string strData ) {
			if( invoked ) {
				Debug.LogError( "Error the delegate is allready invoked" );
				return;
			}


			System.Collections.Hashtable retParams = JSON.Deserialize( strData ) as System.Collections.Hashtable;

			/*string strTmp = "{ \n"+
				"\"succeeded\" : \"false\", \n"+
				"\"path\" : \"\", \n" +
				"\"cancelled\" : \"true\" \n"+
				"}\n";*/
		
			if( retParams == null ) {
				Debug.LogError( "Error parsing ret" );
				return;
			}

			Debug.Log ( "Str= " + retParams + " str data = " + strData );

			deleg( retParams );
			this.enabled = false;
			this.gameObject.SetActive( false );
			GameObject.DestroyObject( this.gameObject ); // <- destroy this game object. to be sure we dont call it anymore
		}



	}




	public static class JSON {
		/// <summary>
		/// Parses the string json into a value
		/// </summary>
		/// <param name="json">A JSON string.</param>
		/// <returns>An ArrayList, a Hashtable, a double, an integer,a string, null, true, or false</returns>
		public static object Deserialize(string json) {
			// save the string for debug information
			if (json == null) {
				return null;
			}

			return Parser.Parse(json);
		}

		sealed class Parser : IDisposable {
			const string WORD_BREAK = "{}[],:\"";

			public static bool IsWordBreak(char c) {
				return Char.IsWhiteSpace(c) || WORD_BREAK.IndexOf(c) != -1;
			}

			enum TOKEN {
				NONE,
				CURLY_OPEN,
				CURLY_CLOSE,
				SQUARED_OPEN,
				SQUARED_CLOSE,
				COLON,
				COMMA,
				STRING,
				NUMBER,
				TRUE,
				FALSE,
				NULL
			};

			StringReader json;

			Parser(string jsonString) {
				json = new StringReader(jsonString);
			}

			public static object Parse(string jsonString) {
				using (var instance = new Parser(jsonString)) {
					return instance.ParseValue();
				}
			}

			public void Dispose() {
				json.Dispose();
				json = null;
			}

			Hashtable ParseObject() {
				Hashtable table = new Hashtable();

				// ditch opening brace
				json.Read();

				// {
				while (true) {
					switch (NextToken) {
					case TOKEN.NONE:
						return null;
					case TOKEN.COMMA:
						continue;
					case TOKEN.CURLY_CLOSE:
						return table;
					default:
						// name
						string name = ParseString();
						if (name == null) {
							return null;
						}

						// :
						if (NextToken != TOKEN.COLON) {
							return null;
						}
						// ditch the colon
						json.Read();

						// value
						table[name] = ParseValue();
						break;
					}
				}
			}

			ArrayList ParseArray() {
				ArrayList array = new ArrayList();

				// ditch opening bracket
				json.Read();

				// [
				var parsing = true;
				while (parsing) {
					TOKEN nextToken = NextToken;

					switch (nextToken) {
					case TOKEN.NONE:
						return null;
					case TOKEN.COMMA:
						continue;
					case TOKEN.SQUARED_CLOSE:
						parsing = false;
						break;
					default:
						object nextValue = ParseByToken(nextToken);

						array.Add(nextValue);
						break;
					}
				}

				return array;
			}

			object ParseValue() {
				TOKEN nextToken = NextToken;
				return ParseByToken(nextToken);
			}

			object ParseByToken(TOKEN token) {
				switch (token) {
				case TOKEN.STRING:
					return ParseString();
				case TOKEN.NUMBER:
					return ParseNumber();
				case TOKEN.CURLY_OPEN:
					return ParseObject();
				case TOKEN.SQUARED_OPEN:
					return ParseArray();
				case TOKEN.TRUE:
					return true;
				case TOKEN.FALSE:
					return false;
				case TOKEN.NULL:
					return null;
				default:
					return null;
				}
			}

			string ParseString() {
				StringBuilder s = new StringBuilder();
				char c;

				// ditch opening quote
				json.Read();

				bool parsing = true;
				while (parsing) {

					if (json.Peek() == -1) {
						parsing = false;
						break;
					}

					c = NextChar;
					switch (c) {
					case '"':
						parsing = false;
						break;
					case '\\':
						if (json.Peek() == -1) {
							parsing = false;
							break;
						}

						c = NextChar;
						switch (c) {
						case '"':
						case '\\':
						case '/':
							s.Append(c);
							break;
						case 'b':
							s.Append('\b');
							break;
						case 'f':
							s.Append('\f');
							break;
						case 'n':
							s.Append('\n');
							break;
						case 'r':
							s.Append('\r');
							break;
						case 't':
							s.Append('\t');
							break;
						case 'u':
							var hex = new char[4];

							for (int i=0; i< 4; i++) {
								hex[i] = NextChar;
							}

							s.Append((char) Convert.ToInt32(new string(hex), 16));
							break;
						}
						break;
					default:
						s.Append(c);
						break;
					}
				}

				return s.ToString();
			}

			object ParseNumber() {
				string number = NextWord;

				if (number.IndexOf('.') == -1) {
					long parsedInt;
					Int64.TryParse(number, out parsedInt);
					return parsedInt;
				}

				double parsedDouble;
				Double.TryParse(number, out parsedDouble);
				return parsedDouble;
			}

			void EatWhitespace() {
				while (Char.IsWhiteSpace(PeekChar)) {
					json.Read();

					if (json.Peek() == -1) {
						break;
					}
				}
			}

			char PeekChar {
				get {
					return Convert.ToChar(json.Peek());
				}
			}

			char NextChar {
				get {
					return Convert.ToChar(json.Read());
				}
			}

			string NextWord {
				get {
					StringBuilder word = new StringBuilder();

					while (!IsWordBreak(PeekChar)) {
						word.Append(NextChar);

						if (json.Peek() == -1) {
							break;
						}
					}

					return word.ToString();
				}
			}

			TOKEN NextToken {
				get {
					EatWhitespace();

					if (json.Peek() == -1) {
						return TOKEN.NONE;
					}

					switch (PeekChar) {
					case '{':
						return TOKEN.CURLY_OPEN;
					case '}':
						json.Read();
						return TOKEN.CURLY_CLOSE;
					case '[':
						return TOKEN.SQUARED_OPEN;
					case ']':
						json.Read();
						return TOKEN.SQUARED_CLOSE;
					case ',':
						json.Read();
						return TOKEN.COMMA;
					case '"':
						return TOKEN.STRING;
					case ':':
						return TOKEN.COLON;
					case '0':
					case '1':
					case '2':
					case '3':
					case '4':
					case '5':
					case '6':
					case '7':
					case '8':
					case '9':
					case '-':
						return TOKEN.NUMBER;
					}

					switch (NextWord) {
					case "false":
						return TOKEN.FALSE;
					case "true":
						return TOKEN.TRUE;
					case "null":
						return TOKEN.NULL;
					}

					return TOKEN.NONE;
				}
			}
		}

		/// <summary>
		/// Converts a Hashtable / ArrayList object or a simple type (string, int, etc.) into a JSON string
		/// </summary>
		/// <param name="json">A Hashtable / Arraylist</param>
		/// <returns>A JSON encoded string, or null if object 'json' is not serializable</returns>
		public static string Serialize(object obj) {
			return Serializer.Serialize(obj);
		}

		sealed class Serializer {
			StringBuilder builder;

			Serializer() {
				builder = new StringBuilder();
			}

			public static string Serialize(object obj) {
				var instance = new Serializer();

				instance.SerializeValue(obj);

				return instance.builder.ToString();
			}

			void SerializeValue(object val) {
				ArrayList asList;
				Hashtable asDict;
				string asStr;

				if (val == null) {
					builder.Append("null");
				} else if ((asStr = val as string) != null) {
					SerializeString(asStr);
				} else if (val is bool) {
					builder.Append((bool) val ? "true" : "false");
				} else if ((asList = val as ArrayList) != null) {
					SerializeArray(asList);
				} else if ((asDict = val as Hashtable) != null) {
					SerializeObject(asDict);
				} else if (val is char) {
					SerializeString(new string((char) val, 1));
				} else {
					SerializeOther(val);
				}
			}

			void SerializeObject(Hashtable obj) {
				bool first = true;

				builder.Append('{');

				foreach (DictionaryEntry e in obj) {
					if (!first) {
						builder.Append(',');
					}

					SerializeString(e.Key.ToString());
					builder.Append(':');

					SerializeValue(e.Value);

					first = false;
				}

				builder.Append('}');
			}

			void SerializeArray(ArrayList anArray) {
				builder.Append('[');

				bool first = true;

				for (int i = 0; i < anArray.Count; i++) {
					object obj = anArray[i];
					if (!first) {
						builder.Append(',');
					}

					SerializeValue(obj);

					first = false;
				}

				builder.Append(']');
			}

			void SerializeString(string str) {
				builder.Append('\"');

				char[] charArray = str.ToCharArray();
				for (int i = 0; i < charArray.Length; i++) {
					char c = charArray[i];
					switch (c) {
					case '"':
						builder.Append("\\\"");
						break;
					case '\\':
						builder.Append("\\\\");
						break;
					case '\b':
						builder.Append("\\b");
						break;
					case '\f':
						builder.Append("\\f");
						break;
					case '\n':
						builder.Append("\\n");
						break;
					case '\r':
						builder.Append("\\r");
						break;
					case '\t':
						builder.Append("\\t");
						break;
					default:
						int codepoint = Convert.ToInt32(c);
						if ((codepoint >= 32) && (codepoint <= 126)) {
							builder.Append(c);
						} else {
							builder.Append("\\u");
							builder.Append(codepoint.ToString("x4"));
						}
						break;
					}
				}

				builder.Append('\"');
			}

			void SerializeOther(object val) {
				// NOTE: decimals lose precision during serialization.
				// They always have, I'm just letting you know.
				// Previously floats and doubles lost precision too.
				if (val is float) {
					builder.Append(((float) val).ToString("R"));
				} else if (val is int
					|| val is uint
					|| val is long
					|| val is sbyte
					|| val is byte
					|| val is short
					|| val is ushort
					|| val is ulong) {
					builder.Append(val);
				} else if (val is double
					|| val is decimal) {
					builder.Append(Convert.ToDouble(val).ToString("R"));
				} else {
					SerializeString(val.ToString());
				}
			}
		}
	}



	public class Documents {
		static string _documentDirectoryString=null;

		static string GetDocumentsDirectoryOniOS() {
			if( Application.platform != RuntimePlatform.IPhonePlayer ) {
				
				return Application.persistentDataPath;
			}
			if( string.IsNullOrEmpty(_documentDirectoryString) ) {
				_documentDirectoryString = ImagePicker_GetDocumentDirectory();
			}
			return _documentDirectoryString;
		}

		[DllImport ("__Internal")]
		private static extern string ImagePicker_GetDocumentDirectory();


		public static bool LoadTextureFromDocuments( string fname, out Texture2D retval ) {
			string path = System.IO.Path.Combine(DocumentsPath, fname);
			if( !System.IO.File.Exists(path) ) {
				Debug.Log("Documents: TryLoadFromDocuments file at address does not exist : " + path);
				retval = null;

				return false;
			}

			byte[] bytes = null;
#if UNITY_IPHONE || UNITY_ANDROID || ENABLE_DOTNET
			bytes = System.IO.File.ReadAllBytes( path );
			if( bytes==null ) {
				Debug.LogError("Documents: Error calling ReadAllBytes at Path : " + path );
				retval = null;

				return false;
			}
#else
			throw new System.Exception("Native Gallery: Platform unsupported : " + Application.platform );
#endif

			var tex = new Texture2D(2,2);
			tex.LoadImage( bytes );
			retval = tex;
		
			return true;
		}


		public static void SaveTextureToDocuments( string fname, Texture2D texture ) {
			var bytes = texture.EncodeToPNG();
			string path = System.IO.Path.Combine(DocumentsPath, fname);
#if UNITY_IPHONE || UNITY_ANDROID || ENABLE_DOTNET
			System.IO.File.WriteAllBytes( path , bytes );
#else
			throw new System.Exception("Native Gallery: Platform unsupported : " + Application.platform );
#endif
		}


		public static string DocumentsPath {
			get {
				return GetDocumentsPath();
			}
		}


		static string GetDocumentsPath()  { 
			// Your game has read+write access to /var/mobile/Applications/XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX/Documents 
			// Application.dataPath returns              
			// /var/mobile/Applications/XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX/myappname.app/Data 
			// Strip "/Data" from path 
			if( Application.platform == RuntimePlatform.IPhonePlayer ) {
				string testDocDir = GetDocumentsDirectoryOniOS();
				//MiscNativStuff.Break( "Tell me this : " +  testDocDir );
				if( !string.IsNullOrEmpty(testDocDir) ) {
					return testDocDir;
				}
				Debug.LogError("Error getting documents path via native method.. retrying with Unity method");
				if( Application.dataPath.EndsWith( "/Data" ) ) {
					string path = Application.dataPath.Substring (0, Application.dataPath.Length - 5); 
					// Strip application name 
					path = path.Substring(0, path.LastIndexOf('/'));   // strip app name from path,,,
					path =  path +  "/Documents"; // add documnets to path.
					//Debug.Log ("Resulting documents path : " + path );
					return path; 
				} 
				Debug.LogError("Error datapath did not end with  /Data : " + Application.dataPath );

				Debug.LogError("Error generating document path ");
				return "";
			}
			else if ( Application.isEditor ) {
				if( Application.dataPath.EndsWith( "/Assets" )==false ) {
					Debug.LogError("Error datapath does not end with Assets");
				}
				string path = Application.dataPath.Substring( 0, Application.dataPath.Length - 6 ); 
				string ret = path + "/Documents";
				if( System.IO.Directory.Exists( ret ) == false ) {
					System.IO.Directory.CreateDirectory( ret );
				}
				return ret;
			}
			return Application.persistentDataPath;
		}


	}



}