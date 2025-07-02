using Blocks;
using UnityEngine;

public class BodyPartInfo : MonoBehaviour
{
	public enum ColorGroup
	{
		None,
		SkinFace,
		SkinRightArm,
		SkinLeftArm,
		SkinRightHand,
		SkinLeftHand,
		SkinRightLeg,
		SkinLeftLeg,
		Shirt,
		Pant
	}

	public BlocksterBody.Bone bone;

	public Vector3 offsetFromBone;

	public ColorGroup colorGroup;

	public string defaultPaint;

	public bool canBeTextured;

	public bool canBeMaterialTextured = true;

	[HideInInspector]
	public string currentPaint;

	[HideInInspector]
	public string currentTexture;
}
