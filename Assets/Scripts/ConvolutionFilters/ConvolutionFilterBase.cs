using UnityEngine;
using System.Collections;

public abstract class ConvolutionFilterBase 
{ 
	public abstract string FilterName 
	{
		get; 
	}
	
	
	public abstract float Factor 
	{
		get; 
	} 
	
	
	public abstract float Bias 
	{ 
		get; 
	} 
	
	
	public abstract float[,] FilterMatrix 
	{
		get; 
	} 
	
	public Texture2D Apply (Texture2D sourceTexture)
	{
		Color[] pixelBuffer = sourceTexture.GetPixels();
		Color[] resultBuffer = new Color[pixelBuffer.Length];
			
		float blue  = 0.0f;
		float green = 0.0f;
		float red   = 0.0f;
		float alpha   = 0.0f;
			
		int filterWidth = FilterMatrix.GetLength (1);
		//int filterHeight = FilterMatrix.GetLength (0);
			
		int filterOffset = (filterWidth-1) / 2;
		int calcOffset = 0;
			
		int byteOffset = 0;
	
		for (int offsetY = filterOffset; offsetY < sourceTexture.height - filterOffset; offsetY++)
		{
			for (int offsetX = filterOffset; offsetX < sourceTexture.width - filterOffset; offsetX++)
			{
				blue = 0;
				green = 0;
				red = 0;
				alpha = 0;
			
				byteOffset = offsetY * sourceTexture.width + offsetX;
				
				for (int filterY = -filterOffset; filterY <= filterOffset; filterY++)
				{
					for (int filterX = -filterOffset; filterX <= filterOffset; filterX++)
					{
						calcOffset = byteOffset + filterX + (filterY * sourceTexture.width);
						
						blue  += (float)(pixelBuffer[calcOffset].r) * FilterMatrix[filterY + filterOffset, filterX + filterOffset];
						green += (float)(pixelBuffer[calcOffset].g) * FilterMatrix[filterY + filterOffset, filterX + filterOffset];
						red   += (float)(pixelBuffer[calcOffset].b) * FilterMatrix[filterY + filterOffset, filterX + filterOffset];
						alpha += (float)(pixelBuffer[calcOffset].a) * FilterMatrix[filterY + filterOffset, filterX + filterOffset];
					}
				}
				
				blue = Factor * blue + Bias;
				green = Factor * green + Bias;
				red = Factor * red + Bias;
				alpha = Factor * alpha + Bias;
				
				/*		
				resultBuffer[byteOffset] = new Color (red, green, blue, 1);
						
				resultBuffer[byteOffset] = (byte)(blue);
				resultBuffer[byteOffset + 1] = (byte)(green);
				resultBuffer[byteOffset + 2] = (byte)(red);
				resultBuffer[byteOffset + 3] = 255;*/
				
				resultBuffer[byteOffset] = new Color (red, green, blue, alpha);
			}
		}
		
		Texture2D resultTexture = new Texture2D (sourceTexture.width, sourceTexture.height); 
		resultTexture.SetPixels (resultBuffer);
		resultTexture.Apply ();
		
		return resultTexture;
	}
}