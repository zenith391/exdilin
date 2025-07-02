using UnityEngine;

public class PasteModelAnimationCommand : ModelAnimationCommand
{
	protected override Vector3 GetStartPos()
	{
		return GetUpperRightWorldPos();
	}

	protected override Vector3 GetTargetPos()
	{
		return GetBlockCenter();
	}

	protected override float GetScaleFromFraction(float fraction)
	{
		return 2f - fraction;
	}
}
