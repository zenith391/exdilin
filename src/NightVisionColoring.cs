using Blocks;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class NightVisionColoring : MonoBehaviour
{
	private Block _nightVisionBlock;

	private string _lastColorName;

	public void SetSourceBlock(Block sourceBlock)
	{
		_nightVisionBlock = sourceBlock;
	}

	public void Update()
	{
		ColorCorrectionCurves component = base.gameObject.GetComponent<ColorCorrectionCurves>();
		if (!(component != null) || _nightVisionBlock == null)
		{
			return;
		}
		string paint = _nightVisionBlock.GetPaint();
		if (_lastColorName != paint)
		{
			if (Blocksworld.colorDefinitions.TryGetValue(paint, out var value))
			{
				Color color = value[0];
				float saturation = (Mathf.Abs(color.r - color.g) + Mathf.Abs(color.r - color.b) + Mathf.Abs(color.g - color.b)) / 2f;
				component.saturation = saturation;
				component.redChannel.MoveKey(1, new Keyframe(1f, value[0].r));
				component.greenChannel.MoveKey(1, new Keyframe(1f, value[0].g));
				component.blueChannel.MoveKey(1, new Keyframe(1f, value[0].b));
				component.UpdateParameters();
			}
			_lastColorName = paint;
		}
	}
}
