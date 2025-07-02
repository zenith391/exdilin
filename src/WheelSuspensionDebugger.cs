using System.Collections.Generic;
using Blocks;
using UnityEngine;

public class WheelSuspensionDebugger : MonoBehaviour
{
	public float damper = 1000f;

	public float spring = 1000f;

	public float length = 1f;

	public float height = 1f;

	public bool applyChanges;

	private float lastDamper = 1000f;

	private float lastSpring = 1000f;

	private float lastLength = 1f;

	private float lastHeight = 1f;

	public void Update()
	{
		if (lastDamper == damper && lastSpring == spring && lastLength == length && lastHeight == height && !applyChanges)
		{
			return;
		}
		List<Block> list = BWSceneManager.AllBlocks();
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] is BlockAbstractWheel blockAbstractWheel)
			{
				if (lastDamper != damper || applyChanges)
				{
					blockAbstractWheel.suspensionDamperOverride = damper;
					blockAbstractWheel.suspensionDamperOverrideActive = true;
				}
				if (lastSpring != spring || applyChanges)
				{
					blockAbstractWheel.suspensionSpringOverride = spring;
					blockAbstractWheel.suspensionSpringOverrideActive = true;
				}
				if (lastLength != length || applyChanges)
				{
					blockAbstractWheel.suspensionLengthOverride = length;
				}
				if (lastHeight != height || applyChanges)
				{
					blockAbstractWheel.suspensionHeightOverride = height;
				}
				blockAbstractWheel.jointsAdjusted = true;
			}
		}
		if (applyChanges)
		{
			applyChanges = false;
			Blocksworld.UI.Overlay.ShowTimedOnScreenMessage("Wheel suspension changes applied.", 3f);
		}
		lastDamper = damper;
		lastSpring = spring;
		lastLength = length;
		lastHeight = height;
	}
}
