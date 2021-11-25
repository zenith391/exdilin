using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x0200035F RID: 863
public class WheelSuspensionDebugger : MonoBehaviour
{
	// Token: 0x06002652 RID: 9810 RVA: 0x0011AC58 File Offset: 0x00119058
	public void Update()
	{
		if (this.lastDamper != this.damper || this.lastSpring != this.spring || this.lastLength != this.length || this.lastHeight != this.height || this.applyChanges)
		{
			List<Block> list = BWSceneManager.AllBlocks();
			for (int i = 0; i < list.Count; i++)
			{
				BlockAbstractWheel blockAbstractWheel = list[i] as BlockAbstractWheel;
				if (blockAbstractWheel != null)
				{
					if (this.lastDamper != this.damper || this.applyChanges)
					{
						blockAbstractWheel.suspensionDamperOverride = this.damper;
						blockAbstractWheel.suspensionDamperOverrideActive = true;
					}
					if (this.lastSpring != this.spring || this.applyChanges)
					{
						blockAbstractWheel.suspensionSpringOverride = this.spring;
						blockAbstractWheel.suspensionSpringOverrideActive = true;
					}
					if (this.lastLength != this.length || this.applyChanges)
					{
						blockAbstractWheel.suspensionLengthOverride = this.length;
					}
					if (this.lastHeight != this.height || this.applyChanges)
					{
						blockAbstractWheel.suspensionHeightOverride = this.height;
					}
					blockAbstractWheel.jointsAdjusted = true;
				}
			}
			if (this.applyChanges)
			{
				this.applyChanges = false;
				Blocksworld.UI.Overlay.ShowTimedOnScreenMessage("Wheel suspension changes applied.", 3f);
			}
			this.lastDamper = this.damper;
			this.lastSpring = this.spring;
			this.lastLength = this.length;
			this.lastHeight = this.height;
		}
	}

	// Token: 0x04002187 RID: 8583
	public float damper = 1000f;

	// Token: 0x04002188 RID: 8584
	public float spring = 1000f;

	// Token: 0x04002189 RID: 8585
	public float length = 1f;

	// Token: 0x0400218A RID: 8586
	public float height = 1f;

	// Token: 0x0400218B RID: 8587
	public bool applyChanges;

	// Token: 0x0400218C RID: 8588
	private float lastDamper = 1000f;

	// Token: 0x0400218D RID: 8589
	private float lastSpring = 1000f;

	// Token: 0x0400218E RID: 8590
	private float lastLength = 1f;

	// Token: 0x0400218F RID: 8591
	private float lastHeight = 1f;
}
