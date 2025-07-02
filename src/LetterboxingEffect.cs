using UnityEngine;

[RequireComponent(typeof(Camera))]
public class LetterboxingEffect : ScreenPostEffect
{
	protected override string GetShaderName()
	{
		return "Blocksworld/LetterboxingEffect";
	}

	public void SetFraction(float fraction)
	{
		if (shaderRGB != null)
		{
			base.material.SetFloat("_Fraction", fraction);
		}
	}
}
