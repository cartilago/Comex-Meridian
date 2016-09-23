/// <summary>
/// Smooth Damp Interpolator
/// Defines Float, Angle, Vector2 & Vector3 value interpolators encapsulatiing all other variables needed for the computation.
/// Values will upddate only on a new execution cycle as a basic optimization.
/// 
/// Create by Jorge Luis Chavez Herrera.
/// </summary>
using UnityEngine;
using System.Collections;

#region SDInterpolatorBase
/// <summary>
/// SD Interpolator Base.
/// Base class for Smooth Damp interpolators
/// </summary>
public class SDInterpolatorBase
{
	static public float time;
		
	public float smoothTime;
}
#endregion
	
#region FloatSDInterpolator
/// <summary>
/// Float Smooth Damp interpolator.
///  Interpolates a float value unsing smooth damping.
/// </summary>
public class FloatSDInterpolator : SDInterpolatorBase
{
	public float targetValue;
		
	private float _value;
	private float velocity;
		
	public float instantValue
	{
		set 
		{
			_value = targetValue = value;
		}
	}
		
	public float value 
	{
		get 
		{
			_value = Mathf.SmoothDamp (_value, targetValue, ref velocity, smoothTime);
			return _value;
		}
		set
		{
			_value = value;
		}
	}
	
	public FloatSDInterpolator (float smoothTime)
	{
		this.smoothTime = smoothTime;
	}
}
#endregion
	
#region AngleSDInterpolator
/// <summary>
/// Angle Smooth Damp interpolator.
/// Interpolates a float angle value unsing smooth damping.
/// </summary>
public class AngleSDInterpolator : SDInterpolatorBase
{
	public float targetValue;
		
	private float _value;
	private float velocity;
		
	public float instantValue
	{
		set 
		{
			_value = targetValue = value;;
		}
	}
		
	public float value 
	{
		get 
		{	
			_value = Mathf.SmoothDampAngle (_value, targetValue, ref velocity, smoothTime);
			return _value;
		}
		set
		{
			_value = value;
		}
	}
	
	public AngleSDInterpolator (float smoothTime)
	{
		this.smoothTime = smoothTime;
	}
}
#endregion

#region EulerAnglesInterpolator
public class EulerAnglesSDInterpolator : SDInterpolatorBase
{
	public Vector3 targetValue;
		
	private Vector3 _value;
	private Vector3 velocity;
		
	public Vector3 instantValue
	{
		set 
		{
			_value = targetValue = value;
		}
	}
		
	public Vector3 value 
	{
		get 
		{
			// Update value only when needed
			_value = new Vector3 (Mathf.SmoothDampAngle (_value.x, targetValue.x, ref velocity.x, smoothTime),
				                    Mathf.SmoothDampAngle (_value.y, targetValue.y, ref velocity.y, smoothTime),
				                    Mathf.SmoothDampAngle (_value.z, targetValue.z, ref velocity.z, smoothTime));
			return _value;
		}
		set
		{
			_value = value;
		}
	}
		
	public EulerAnglesSDInterpolator (float smoothTime)
	{
		this.smoothTime = smoothTime;
	}
}
#endregion
	
#region Vector2SDInterpolator
/// <summary>
/// Vector2 Smooth Damp interpolator.
/// Interpolates a Vector2 value unsing smooth damping.
/// </summary>
public class Vector2SDInterpolator : SDInterpolatorBase
{
	public Vector2 targetValue;
		
	private Vector2 _value;
	private Vector2 velocity;
		
	public Vector2 instantValue
	{
		set 
		{
			_value = targetValue = value;
		}
	}
		
	public Vector2 value 
	{
		get 
		{
			_value = Vector2.SmoothDamp (_value, targetValue, ref velocity, smoothTime);
				
			return _value;
		}
		set 
		{
			_value = value;
		}
	}
		
	public Vector2SDInterpolator (float smoothTime)
	{
		this.smoothTime = smoothTime;
	}
}
#endregion
	
#region Vector3SDInterpolator
/// <summary>
/// Vector3 Smooth Damp interpolator.
/// Interpolates a Vector3 value unsing smooth damping.
/// </summary>
public class Vector3SDInterpolator : SDInterpolatorBase
{
	public Vector3 targetValue;
		
	private Vector3 _value;
	private Vector3 velocity;
		
	public Vector3 instantValue
	{
		set 
		{
			_value = targetValue = value;
		}
	}
		
	public Vector3 value 
	{
		get 
		{	
			_value = Vector3.SmoothDamp (_value, targetValue, ref velocity, smoothTime);
			return _value;
		}
		set 
		{
			_value = value;
		}
	}
	
	public Vector3SDInterpolator (float smoothTime)
	{
		this.smoothTime = smoothTime;
	}
}
#endregion
