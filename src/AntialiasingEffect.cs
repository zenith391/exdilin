using UnityEngine;

[RequireComponent(typeof(Camera))]
public class AntialiasingEffect : ScreenPostEffect
{
	protected override string GetShaderName()
	{
		return "Blocksworld/AntialiasingEffect";
	}
}
