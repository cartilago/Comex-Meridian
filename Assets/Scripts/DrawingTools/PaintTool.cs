using UnityEngine;
using System.Collections;

public class PaintTool : DrawingToolBase 
{
	#region Class members
	public Camera canvasCamera;
	public Renderer canvasRenderer;
	public SpriteRenderer _brushSprite;

	private PaintDrawingAction currentPaintDrawingAction;
	static public SpriteRenderer brushSprite;
	#endregion

	#region Class accessors
	private RenderTexture _renderTexture;
	private RenderTexture renderTexture 
	{
		get 
		{
			if (_renderTexture == null)
			{
				_renderTexture = new RenderTexture((int)canvasRenderer.transform.localScale.x, (int)canvasRenderer.transform.localScale.y, 0);
			}
			else
			{
				if (_renderTexture.width != (int)canvasRenderer.transform.localScale.x || _renderTexture.height != (int)canvasRenderer.transform.localScale.y)
					_renderTexture = new RenderTexture((int)canvasRenderer.transform.localScale.x, (int)canvasRenderer.transform.localScale.y, 0);
			}

			return _renderTexture;
		} 
	}
	#endregion

	#region MonoBehaviour overrides
	private void Awake()
	{
		brushSprite = _brushSprite;
		brushSprite.gameObject.SetActive(false);
		canvasCamera.gameObject.SetActive(false);
	}

	private void SetupCanvas()
	{
		// Setup render tecture & camera
		canvasCamera.targetTexture = renderTexture;
		canvasCamera.gameObject.SetActive(true);
		RenderTexture.active = canvasCamera.targetTexture;
		GL.Clear(false, true, Color.clear, 0);
		canvasRenderer.material.mainTexture = canvasCamera.targetTexture;

		// Set brush size, 10% of the actual size of the image
		float area = canvasRenderer.transform.localScale.x * canvasRenderer.transform.localScale.y * (1.0f / 256.0f) * 0.025f;

		float yFit = (canvasCamera.orthographicSize * 2) / canvasRenderer.transform.localScale.y;

		brushSprite.transform.localScale = new Vector3(area, area * yFit, 1);
	}
	#endregion
	 

	#region DrawingToolBase overrides
	override public void TouchDown(Vector2 pos)
    {
    	SetupCanvas();
    	canvasRenderer.gameObject.SetActive(true);
		brushSprite.gameObject.SetActive(true);

		currentPaintDrawingAction = GameObject.Instantiate(drawingActionPrefab).GetComponent<PaintDrawingAction>();
		currentPaintDrawingAction.cachedTransform.position = Camera.main.ScreenToWorldPoint(pos);
		DecoratorPanel.Instance.GetCurrentProject().AddDrawingAction(currentPaintDrawingAction);

		SetBrushPosition(pos);
    }

	override public void TouchMove(Vector2 pos)
    {
		SetBrushPosition(pos);
    }

	override public void TouchUp(Vector2 pos)
    {
		brushSprite.gameObject.SetActive(false);
		canvasCamera.gameObject.SetActive(false);
		canvasRenderer.gameObject.SetActive(false);

		// Grab render texture pixels
		RenderTexture.active = renderTexture;
		Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
		texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
		texture.Apply();

		DecoratorPanel.Instance.photoRenderer.material.SetTexture("_TintMask", texture);
		//Color32[] colors = texture.GetPixels32();
	}
    #endregion

    #region Class implementation
    private void SetBrushPosition(Vector2 screenPos)
    {
		Ray ray = Camera.main.ScreenPointToRay(screenPos);

		RaycastHit hit;

		if (Physics.Raycast(ray, out hit))
		{
			brushSprite.transform.position = new Vector3( (hit.textureCoord.x - 0.5f) * canvasCamera.orthographicSize, 
				(hit.textureCoord.y - 0.5f) * canvasCamera.orthographicSize / canvasCamera.aspect, 0); 
		}
    }
    #endregion
}

/*
vec3 rgb2hsv(vec3 c)
{
    vec4 K = vec4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
	vec4 p = c.g < c.b ? vec4(c.bg, K.wz) : vec4(c.gb, K.xy);
    vec4 q = c.r < p.x ? vec4(p.xyw, c.r) : vec4(c.r, p.yzx);

    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    return vec3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

vec3 hsv2rgb(vec3 c)
{
    vec4 K = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}
*/