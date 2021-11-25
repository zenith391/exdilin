using System;
using Blocks;
using UnityEngine;

// Token: 0x02000238 RID: 568
public class RelativeDirectionAngleXZVisualizer : DirectionAngleXZVisualizer
{
	// Token: 0x06001AEC RID: 6892 RVA: 0x000C5378 File Offset: 0x000C3778
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
