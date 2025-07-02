using UnityEngine;

public class BlockMetaData : MonoBehaviour
{
	public BlockMeshMetaData[] meshDatas;

	public bool isTerrain;

	public Vector3 blockSize;

	public Vector3 canScale;

	public float massK;

	public float massM;

	public string[] canOccupySameGrid;

	public bool keepRigidbody;

	public Vector3[] scaleConstraints;

	public string editorIconName;

	public string scaleType;

	public Vector3 defaultScale = Vector3.one;

	public Vector3 defaultOrientation = Vector3.zero;

	public bool characterEditModeUsesDefaultOrientation = true;

	public Vector3 characterEditModeOrientation = Vector3.zero;

	public Vector3 characterEditModeSnapOffset = Vector3.zero;

	public float shadowStrengthMultiplier = 1f;

	public float lightStrengthMultiplier = 1f;

	public float buoyancyMultiplier = 1f;

	public GAFInfo[] defaultGAFs;

	public bool freezeInTerrain = true;

	public bool selectableTerrain;

	public bool disableBuildModeScale;

	public bool disableBuildModeMove;

	public Vector3 allowedBuildModeRotations = Vector3.one;

	public bool scrollToScriptTileOnSelect;

	public Vector3 autoAnglesTutorial = Vector3.zero;

	public string[] noShapeCollideClasses;

	public string[] shapeCategories;

	public Vector3 scaleLimit = Vector3.zero;

	public bool isBlocksterMassless;

	public Vector3 attachOffset = Vector3.zero;

	public HandAttachmentType handUse = HandAttachmentType.None;

	public BlocksterGearType gearType;

	public bool hideInFirstPersonCamera;

	public Transform firstPersonCameraReplacement;

	public string preferredEngineSound;

	public void Reset()
	{
		if (meshDatas == null)
		{
			return;
		}
		BlockMeshMetaData[] array = meshDatas;
		foreach (BlockMeshMetaData blockMeshMetaData in array)
		{
			if (blockMeshMetaData.defaultPaint == null || blockMeshMetaData.defaultPaint == string.Empty)
			{
				blockMeshMetaData.defaultPaint = "Yellow";
			}
		}
	}
}
