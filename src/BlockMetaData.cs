using System;
using UnityEngine;

// Token: 0x020001ED RID: 493
public class BlockMetaData : MonoBehaviour
{
	// Token: 0x060018D5 RID: 6357 RVA: 0x000AF708 File Offset: 0x000ADB08
	public void Reset()
	{
		if (this.meshDatas != null)
		{
			foreach (BlockMeshMetaData blockMeshMetaData in this.meshDatas)
			{
				if (blockMeshMetaData.defaultPaint == null || blockMeshMetaData.defaultPaint == string.Empty)
				{
					blockMeshMetaData.defaultPaint = "Yellow";
				}
			}
		}
	}

	// Token: 0x040013FA RID: 5114
	public BlockMeshMetaData[] meshDatas;

	// Token: 0x040013FB RID: 5115
	public bool isTerrain;

	// Token: 0x040013FC RID: 5116
	public Vector3 blockSize;

	// Token: 0x040013FD RID: 5117
	public Vector3 canScale;

	// Token: 0x040013FE RID: 5118
	public float massK;

	// Token: 0x040013FF RID: 5119
	public float massM;

	// Token: 0x04001400 RID: 5120
	public string[] canOccupySameGrid;

	// Token: 0x04001401 RID: 5121
	public bool keepRigidbody;

	// Token: 0x04001402 RID: 5122
	public Vector3[] scaleConstraints;

	// Token: 0x04001403 RID: 5123
	public string editorIconName;

	// Token: 0x04001404 RID: 5124
	public string scaleType;

	// Token: 0x04001405 RID: 5125
	public Vector3 defaultScale = Vector3.one;

	// Token: 0x04001406 RID: 5126
	public Vector3 defaultOrientation = Vector3.zero;

	// Token: 0x04001407 RID: 5127
	public bool characterEditModeUsesDefaultOrientation = true;

	// Token: 0x04001408 RID: 5128
	public Vector3 characterEditModeOrientation = Vector3.zero;

	// Token: 0x04001409 RID: 5129
	public Vector3 characterEditModeSnapOffset = Vector3.zero;

	// Token: 0x0400140A RID: 5130
	public float shadowStrengthMultiplier = 1f;

	// Token: 0x0400140B RID: 5131
	public float lightStrengthMultiplier = 1f;

	// Token: 0x0400140C RID: 5132
	public float buoyancyMultiplier = 1f;

	// Token: 0x0400140D RID: 5133
	public GAFInfo[] defaultGAFs;

	// Token: 0x0400140E RID: 5134
	public bool freezeInTerrain = true;

	// Token: 0x0400140F RID: 5135
	public bool selectableTerrain;

	// Token: 0x04001410 RID: 5136
	public bool disableBuildModeScale;

	// Token: 0x04001411 RID: 5137
	public bool disableBuildModeMove;

	// Token: 0x04001412 RID: 5138
	public Vector3 allowedBuildModeRotations = Vector3.one;

	// Token: 0x04001413 RID: 5139
	public bool scrollToScriptTileOnSelect;

	// Token: 0x04001414 RID: 5140
	public Vector3 autoAnglesTutorial = Vector3.zero;

	// Token: 0x04001415 RID: 5141
	public string[] noShapeCollideClasses;

	// Token: 0x04001416 RID: 5142
	public string[] shapeCategories;

	// Token: 0x04001417 RID: 5143
	public Vector3 scaleLimit = Vector3.zero;

	// Token: 0x04001418 RID: 5144
	public bool isBlocksterMassless;

	// Token: 0x04001419 RID: 5145
	public Vector3 attachOffset = Vector3.zero;

	// Token: 0x0400141A RID: 5146
	public HandAttachmentType handUse = HandAttachmentType.None;

	// Token: 0x0400141B RID: 5147
	public BlocksterGearType gearType;

	// Token: 0x0400141C RID: 5148
	public bool hideInFirstPersonCamera;

	// Token: 0x0400141D RID: 5149
	public Transform firstPersonCameraReplacement;

	// Token: 0x0400141E RID: 5150
	public string preferredEngineSound;
}
