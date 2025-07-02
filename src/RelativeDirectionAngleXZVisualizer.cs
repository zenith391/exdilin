using Blocks;
using UnityEngine;

public class RelativeDirectionAngleXZVisualizer : DirectionAngleXZVisualizer
{
	protected override Quaternion GetRotation(float angle)
	{
		Block selectedScriptBlock = Blocksworld.GetSelectedScriptBlock();
		if (selectedScriptBlock != null)
		{
			Vector3 lookTowardAngleDirection = BlocksworldCamera.GetLookTowardAngleDirection(selectedScriptBlock, angle);
			return Quaternion.LookRotation(lookTowardAngleDirection, Vector3.up);
		}
		return Quaternion.identity;
	}
}
