using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Meridian.Framework.Utils;


public class FingerCanvas : MonoSingleton<FingerCanvas> 
{
	#region Class members
	public Camera canvasCamera;
	public Renderer canvasRenderer;
	public SpriteRenderer brushSprite;
	public Material[] brushMaterials;
	private bool newTexture;
	private List<byte[]>pngUndoBuffer = new List<byte[]>();
	private byte[] pngCurrentUndo = null;
	#endregion

	#region Class accessors
	/// <summary>
	/// Gets the render texture for the canvas.
	/// </summary>
	/// <value>The render texture.</value>
	private RenderTexture _renderTexture;
	public RenderTexture renderTexture 
	{
		get 
		{
			if (_renderTexture == null)
			{
				_renderTexture = new RenderTexture((int)canvasRenderer.transform.localScale.x, (int)canvasRenderer.transform.localScale.y, 0);
				newTexture = true;
			}
			else
			{
				if (_renderTexture.width != (int)canvasRenderer.transform.localScale.x || _renderTexture.height != (int)canvasRenderer.transform.localScale.y)
				{
                    if (RenderTexture.active == _renderTexture)
                        RenderTexture.active = null;

					DestroyImmediate(_renderTexture);
					_renderTexture = new RenderTexture((int)canvasRenderer.transform.localScale.x, (int)canvasRenderer.transform.localScale.y, 0);
					newTexture = true;
				}
			}

			return _renderTexture;
		} 
	}
	#endregion

	#region MonoBehaviour overrides
	private void Start()
	{
		SetVisible(false);
	}
	#endregion

	#region Class implementation
	public void SetVisible(bool isVisible)
	{
		brushSprite.gameObject.SetActive(isVisible);
		canvasCamera.gameObject.SetActive(isVisible);
		canvasRenderer.gameObject.SetActive(isVisible);
	}

	/// <summary>
	/// Sets the normal brush.
	/// </summary>
	public void SetNormalBrush()
	{
		brushSprite.material = brushMaterials[0];
	}

	/// <summary>
	/// Sets the eraser brush.
	/// </summary>
	public void SetEraserBrush()
	{
		brushSprite.material = brushMaterials[1];
	}

	/// <summary>
	/// Setups the drawing canvas.
	/// </summary>
	public void SetupCanvas()
	{
		// Setup render tecture & camera
		canvasCamera.targetTexture = renderTexture;
		canvasCamera.gameObject.SetActive(true);

		if (newTexture == true)
		{
			RenderTexture.active = canvasCamera.targetTexture;
			GL.Clear(false, true, Color.clear, 0);
			newTexture = false;
		}

		canvasRenderer.material.mainTexture = canvasCamera.targetTexture;

		// Set brush size, 10% of the actual size of the image
		float area = canvasRenderer.transform.localScale.x * canvasRenderer.transform.localScale.y * (1.0f / 256.0f) * 0.0125f;
		float yFit = (canvasCamera.orthographicSize * 2) / canvasRenderer.transform.localScale.y;
		brushSprite.transform.localScale = new Vector3(area, area * yFit, 1);
		brushSprite.color = new Color(0,0,0,1); // Brush always draws on alpha so it can be cleared after each stroke

        // Set canvas texture for photo shader, tint colors encoded as r,g,b
        DecoratorPanel.Instance.photoRenderer.material.SetTexture("_TintMask", renderTexture);

        Debug.Log("Canvas set " + renderTexture.width + " x " + renderTexture.height);
	}

    public void UpdateBrushColor()
    {
        brushSprite.color = new Color(0, 0, 0, 1);

        switch (ColorsManager.Instance.GetCurrentColor())
        {
            case 0: DecoratorPanel.Instance.photoRenderer.material.SetColor("_Color4", DecoratorPanel.Instance.photoRenderer.material.GetColor("_Color1")); break;
            case 1: DecoratorPanel.Instance.photoRenderer.material.SetColor("_Color4", DecoratorPanel.Instance.photoRenderer.material.GetColor("_Color2")); break;
            case 2: DecoratorPanel.Instance.photoRenderer.material.SetColor("_Color4", DecoratorPanel.Instance.photoRenderer.material.GetColor("_Color3")); break;
        }
    }

	/// <summary>
	/// Sets the brush position in canvas.
	/// </summary>
	/// <param name="screenPos">Screen position.</param>
	public void SetBrushPosition(Vector2 screenPos)
    {
		Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
		Vector3 photoLocalPos = DecoratorPanel.Instance.photoRenderer.transform.InverseTransformPoint(worldPos);
		Vector3 canvasPosition = new Vector3(photoLocalPos.x * renderTexture.width, photoLocalPos.y * renderTexture.height, 0);

		float aspectCorrection = (canvasCamera.orthographicSize * 2) / (float)renderTexture.height;
		brushSprite.transform.position = new Vector2(canvasPosition.x, canvasPosition.y * aspectCorrection);
    }

    /// <summary>
    /// Transforms screen to canvas coordinates
    /// </summary>
    /// <returns>The canvas position.</returns>
    /// <param name="screenPos">Screen position.</param>
    public Vector2 GetCanvasPosition(Vector2 screenPos)
    {
		Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
		Vector3 photoLocalPos = DecoratorPanel.Instance.photoRenderer.transform.InverseTransformPoint(worldPos) + new Vector3(0.5f,0.5f);

		return new Vector2(photoLocalPos.x * renderTexture.width, photoLocalPos.y * renderTexture.height);
    }

    /// <summary>
    /// Clear tint masks.
    /// </summary>
    public void Clear()
   	{
		RenderTexture.active = renderTexture;
		GL.Clear(false, true, new Color(0,0,0,0));
		GL.Flush();
		Debug.Log("Cleared");
	}

	/// <summary>
	/// Sets the contents of the renderTexture.
	/// </summary>
	/// <param name="contents">Contents.</param>
	public void SetContents(Texture2D contents)
	{
		Graphics.Blit(contents, renderTexture);
		ClearUndoStack();
	}

	/// <summary>
	/// Gets a snapshot of the current rendertexture contents..
	/// </summary>
	/// <returns>The snapshot.</returns>
	public Texture2D GetSnapshot()
	{
		RenderTexture.active = renderTexture;
		Texture2D masksTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
		masksTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
		masksTexture.Apply();

		return masksTexture;
	}

	/// <summary>
	/// Saves current canvas image to undo stack, image is encoded as PNG.
	/// </summary>
	public void SaveUndo()
	{ 
		Texture2D snapshot = GetSnapshot();	

		if (pngCurrentUndo != null)
			pngUndoBuffer.Add(pngCurrentUndo);

		pngCurrentUndo = snapshot.EncodeToPNG();
		Debug.Log("Undo size: " + pngCurrentUndo.Length);
		
        // Release texture memory
		DestroyImmediate(snapshot);
		Resources.UnloadUnusedAssets(); 
		System.GC.Collect();
	} 

    /// <summary>
    /// Restores a PNG encode image from the from undo stack.
    /// </summary>
    public void RestoreFromUndoStack()
    {
		Texture2D snapshot = null;
		Texture2D saved = null;

		if (pngUndoBuffer.Count > 0)
        {
			saved = new Texture2D(2, 2);
	        saved.LoadImage(pngUndoBuffer[pngUndoBuffer.Count-1]);
			RenderTexture.active = renderTexture;
			Graphics.Blit(saved, renderTexture);
			pngUndoBuffer.RemoveAt(pngUndoBuffer.Count-1);

			// Save undo again
			snapshot = GetSnapshot();	
			pngCurrentUndo = snapshot.EncodeToPNG();

			Debug.Log("Undo restored, stack size: " + pngUndoBuffer.Count + " current is null " + (pngCurrentUndo == null));
	    }

        // Release memory
        DestroyImmediate(snapshot);
		DestroyImmediate(saved);
		Resources.UnloadUnusedAssets(); 
		System.GC.Collect();
    }

    public void ClearUndoStack()
	{
		pngCurrentUndo = null;
		// Release memory
		Resources.UnloadUnusedAssets(); 
		System.GC.Collect();
        Debug.Log("Undo Stack Cleared");
	}
    #endregion
}
