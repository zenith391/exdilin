using System;
using Blocks;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

// Token: 0x02000258 RID: 600
public class NightVisionColoring : MonoBehaviour
{
	// Token: 0x06001B40 RID: 6976 RVA: 0x000C66E7 File Offset: 0x000C4AE7
	public void SetSourceBlock(Block sourceBlock)
	{
		this._nightVisionBlock = sourceBlock;
	}

	// Token: 0x06001B41 RID: 6977 RVA: 0x000C66F0 File Offset: 0x000C4AF0
	public void Update()
	{
		ColorCorrectionCurves component = base.gameObject.GetComponent<ColorCorrectionCurves>();
		if (component != null && this._nightVisionBlock != null)
		{
			string paint = this._nightVisionBlock.GetPaint(0);
			if (this._lastColorName != paint)
			{
				Color[] array;
				if (Blocksworld.colorDefinitions.TryGetValue(paint, out array))
				{
					Color color = array[0];
					float saturation = (Mathf.Abs(color.r - color.g) + Mathf.Abs(color.r - color.b) + Mathf.Abs(color.g - color.b)) / 2f;
					component.saturation = saturation;
					component.redChannel.MoveKey(1, new Keyframe(1f, array[0].r));
					component.greenChannel.MoveKey(1, new Keyframe(1f, array[0].g));
					component.blueChannel.MoveKey(1, new Keyframe(1f, array[0].b));
					component.UpdateParameters();
				}
				this._lastColorName = paint;
			}
		}
	}

	// Token: 0x0400172D RID: 5933
	private Block _nightVisionBlock;

	// Token: 0x0400172E RID: 5934
	private string _lastColorName;
}
