using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlocksterBody
{
	public enum BodyPart
	{
		LeftArm,
		RightArm,
		LeftLeg,
		RightLeg
	}

	public enum Bone
	{
		Root,
		Spine,
		Head,
		LeftUpperArm,
		LeftLowerArm,
		LeftHand,
		RightUpperArm,
		RightLowerArm,
		RightHand,
		LeftUpperLeg,
		LeftLowerLeg,
		LeftFoot,
		RightUpperLeg,
		RightLowerLeg,
		RightFoot
	}

	private BlockAnimatedCharacter _character;

	public static List<BodyPart> AllLimbs = new List<BodyPart>
	{
		BodyPart.LeftArm,
		BodyPart.RightArm,
		BodyPart.LeftLeg,
		BodyPart.RightLeg
	};

	public Dictionary<BodyPart, string> currentBodyPartVersions = new Dictionary<BodyPart, string>();

	private Dictionary<Bone, List<GameObject>> currentBoneChildObjects = new Dictionary<Bone, List<GameObject>>();

	private static Dictionary<BodyPart, List<Bone>> bonesForBodyPart;

	public BlocksterBody(BlockAnimatedCharacter character)
	{
		_character = character;
	}

	public List<GameObject> GetObjectsForBone(Bone bone)
	{
		if (currentBoneChildObjects.ContainsKey(bone))
		{
			return currentBoneChildObjects[bone];
		}
		return null;
	}

	public List<GameObject> GetObjectsForBodyPart(BodyPart bodyPart)
	{
		List<GameObject> list = new List<GameObject>();
		List<Bone> list2 = GetBonesForBodyPart(bodyPart);
		foreach (Bone item in list2)
		{
			list.AddRange(GetObjectsForBone(item));
		}
		return list;
	}

	public void SubstitutePart(BodyPart part, string bodyPartVersionStr)
	{
		if (currentBodyPartVersions.ContainsKey(part) && currentBodyPartVersions[part] == bodyPartVersionStr)
		{
			return;
		}
		currentBodyPartVersions[part] = bodyPartVersionStr;
		GameObject gameObject = CreateBodyPartObject(bodyPartVersionStr);
		foreach (Bone item2 in GetBonesForBodyPart(part))
		{
			if (currentBoneChildObjects.TryGetValue(item2, out var value))
			{
				foreach (GameObject item3 in value)
				{
					Object.Destroy(item3);
				}
				value.Clear();
			}
			else
			{
				value = new List<GameObject>();
				currentBoneChildObjects[item2] = value;
			}
		}
		List<Transform> list = new List<Transform>();
		list.Add(gameObject.transform);
		foreach (object item4 in gameObject.transform)
		{
			Transform item = (Transform)item4;
			list.Add(item);
		}
		HashSet<GameObject> hashSet = new HashSet<GameObject>();
		foreach (Transform item5 in list)
		{
			BodyPartInfo component = item5.GetComponent<BodyPartInfo>();
			if (component == null)
			{
				hashSet.Add(item5.gameObject);
				continue;
			}
			item5.localScale *= _character.blocksterRigScale;
			Transform boneTransform = _character.GetBoneTransform(component.bone);
			item5.parent = boneTransform;
			item5.localPosition = component.offsetFromBone;
			item5.localEulerAngles = GetBoneRotationOffset(component.bone);
			currentBoneChildObjects[component.bone].Add(item5.gameObject);
		}
		foreach (GameObject item6 in hashSet)
		{
			Object.Destroy(item6);
		}
		HashSet<string> animatorParametersForBodyParts = new HashSet<string>(currentBodyPartVersions.Values);
		_character.stateHandler.SetAnimatorParametersForBodyParts(animatorParametersForBodyParts);
	}

	public static BodyPart BodyPartFromString(string bodyPartStr)
	{
		BodyPart result = BodyPart.LeftArm;
		if (bodyPartStr.Contains("Arm Left"))
		{
			result = BodyPart.LeftArm;
		}
		else if (bodyPartStr.Contains("Arm Right"))
		{
			result = BodyPart.RightArm;
		}
		else if (bodyPartStr.Contains("Leg Left"))
		{
			result = BodyPart.LeftLeg;
		}
		else if (bodyPartStr.Contains("Leg Right"))
		{
			result = BodyPart.RightLeg;
		}
		return result;
	}

	public static List<Bone> GetBonesForBodyPart(BodyPart bodyPart)
	{
		if (bonesForBodyPart == null)
		{
			bonesForBodyPart = new Dictionary<BodyPart, List<Bone>>();
			bonesForBodyPart.Add(BodyPart.LeftArm, new List<Bone>
			{
				Bone.LeftHand,
				Bone.LeftLowerArm,
				Bone.LeftUpperArm
			});
			bonesForBodyPart.Add(BodyPart.RightArm, new List<Bone>
			{
				Bone.RightHand,
				Bone.RightLowerArm,
				Bone.RightUpperArm
			});
			bonesForBodyPart.Add(BodyPart.LeftLeg, new List<Bone>
			{
				Bone.LeftFoot,
				Bone.LeftLowerLeg,
				Bone.LeftUpperLeg
			});
			bonesForBodyPart.Add(BodyPart.RightLeg, new List<Bone>
			{
				Bone.RightFoot,
				Bone.RightLowerLeg,
				Bone.RightUpperLeg
			});
		}
		return bonesForBodyPart[bodyPart];
	}

	public static GameObject CreateBodyPartObject(string bodyPartStr)
	{
		GameObject gameObject = Resources.Load<GameObject>("BodyParts/" + bodyPartStr);
		if (gameObject == null)
		{
			BWLog.Error("Failed to find prefab for body part: " + bodyPartStr);
			return null;
		}
		return Object.Instantiate(gameObject);
	}

	public void ApplyDefaultPaints(BodyPart bodyPart)
	{
		foreach (Bone item in GetBonesForBodyPart(bodyPart))
		{
			foreach (GameObject item2 in GetObjectsForBone(item))
			{
				int subMeshIndex = _character.GetSubMeshIndex(item2);
				if (subMeshIndex >= 0)
				{
					string text = null;
					if (_character.characterType == CharacterType.Avatar)
					{
						text = "Dark Blue";
					}
					else
					{
						BodyPartInfo component = item2.GetComponent<BodyPartInfo>();
						if (component != null)
						{
							switch (component.colorGroup)
							{
							case BodyPartInfo.ColorGroup.SkinFace:
							case BodyPartInfo.ColorGroup.SkinRightArm:
							case BodyPartInfo.ColorGroup.SkinLeftArm:
							case BodyPartInfo.ColorGroup.SkinRightHand:
							case BodyPartInfo.ColorGroup.SkinLeftHand:
							case BodyPartInfo.ColorGroup.SkinRightLeg:
							case BodyPartInfo.ColorGroup.SkinLeftLeg:
								text = _character.GetCurrentHeadColor();
								break;
							case BodyPartInfo.ColorGroup.Shirt:
								text = _character.GetCurrentBodyColor();
								break;
							default:
								text = component.defaultPaint;
								break;
							}
						}
					}
					if (text == null)
					{
						BWLog.Info(string.Concat("Failed to get default color for object ", item2, " on bone ", item));
						text = "Tan";
					}
					_character.PaintTo(text, permanent: true, subMeshIndex);
				}
				else
				{
					BWLog.Info(string.Concat("object ", item2.name, "on bone ", item, " has not been added to character submeshes"));
				}
			}
		}
	}

	public void ReapplyPaints(BodyPart bodyPart)
	{
		foreach (Bone item in GetBonesForBodyPart(bodyPart))
		{
			foreach (GameObject item2 in GetObjectsForBone(item))
			{
				int subMeshIndex = _character.GetSubMeshIndex(item2);
				if (subMeshIndex >= 0)
				{
					string paint = _character.GetPaint(subMeshIndex);
					_character.PaintTo(paint, permanent: true, subMeshIndex);
				}
			}
		}
	}

	public void PaintSkinColor(string paint, bool permanent)
	{
		PaintColorGroup(BodyPartInfo.ColorGroup.SkinFace, paint, permanent);
		PaintColorGroup(BodyPartInfo.ColorGroup.SkinRightArm, paint, permanent);
		PaintColorGroup(BodyPartInfo.ColorGroup.SkinLeftArm, paint, permanent);
		PaintColorGroup(BodyPartInfo.ColorGroup.SkinRightLeg, paint, permanent);
		PaintColorGroup(BodyPartInfo.ColorGroup.SkinLeftLeg, paint, permanent);
		PaintColorGroup(BodyPartInfo.ColorGroup.SkinRightHand, paint, permanent);
		PaintColorGroup(BodyPartInfo.ColorGroup.SkinLeftHand, paint, permanent);
	}

	public void PaintShirtColor(string paint, bool permanent)
	{
		PaintColorGroup(BodyPartInfo.ColorGroup.Shirt, paint, permanent);
	}

	public void PaintPantsColor(string paint, bool permanent)
	{
		PaintColorGroup(BodyPartInfo.ColorGroup.Pant, paint, permanent);
	}

	public void PaintColorGroup(BodyPartInfo.ColorGroup group, string paint, bool permanent)
	{
		List<GameObject> list = new List<GameObject>();
		foreach (Bone item in GetBonesForBodyPart(BodyPart.LeftArm))
		{
			list.AddRange(GetObjectsForBone(item));
		}
		foreach (Bone item2 in GetBonesForBodyPart(BodyPart.RightArm))
		{
			list.AddRange(GetObjectsForBone(item2));
		}
		foreach (Bone item3 in GetBonesForBodyPart(BodyPart.LeftLeg))
		{
			list.AddRange(GetObjectsForBone(item3));
		}
		foreach (Bone item4 in GetBonesForBodyPart(BodyPart.RightLeg))
		{
			list.AddRange(GetObjectsForBone(item4));
		}
		foreach (GameObject item5 in list)
		{
			int subMeshIndex = _character.GetSubMeshIndex(item5);
			if (subMeshIndex >= 0)
			{
				BodyPartInfo component = item5.GetComponent<BodyPartInfo>();
				if (component != null && component.colorGroup == group && _character.GetPaint(subMeshIndex) != paint)
				{
					_character.PaintTo(paint, permanent, subMeshIndex);
				}
			}
		}
	}

	public void TextureColorGroup(BodyPartInfo.ColorGroup group, string texture, Vector3 normal, bool permanent)
	{
		List<GameObject> list = new List<GameObject>();
		foreach (Bone item in GetBonesForBodyPart(BodyPart.LeftArm))
		{
			list.AddRange(GetObjectsForBone(item));
		}
		foreach (Bone item2 in GetBonesForBodyPart(BodyPart.RightArm))
		{
			list.AddRange(GetObjectsForBone(item2));
		}
		foreach (Bone item3 in GetBonesForBodyPart(BodyPart.LeftLeg))
		{
			list.AddRange(GetObjectsForBone(item3));
		}
		foreach (Bone item4 in GetBonesForBodyPart(BodyPart.RightLeg))
		{
			list.AddRange(GetObjectsForBone(item4));
		}
		foreach (GameObject item5 in list)
		{
			int subMeshIndex = _character.GetSubMeshIndex(item5);
			if (subMeshIndex >= 0)
			{
				BodyPartInfo component = item5.GetComponent<BodyPartInfo>();
				if (component != null && component.colorGroup == group && _character.GetTexture(subMeshIndex) != texture)
				{
					_character.TextureTo(texture, normal, permanent, subMeshIndex);
				}
			}
		}
	}

	public void SetShaderForBodyPart(BodyPart bodyPart, ShaderType shader)
	{
		foreach (GameObject item in GetObjectsForBodyPart(bodyPart))
		{
			BodyPartInfo component = item.GetComponent<BodyPartInfo>();
			int subMeshIndex = _character.GetSubMeshIndex(item);
			string subMeshPaint = _character.GetSubMeshPaint(subMeshIndex);
			string subMeshTexture = _character.GetSubMeshTexture(subMeshIndex);
			MeshRenderer component2 = item.GetComponent<MeshRenderer>();
			component2.sharedMaterial = Materials.GetMaterial(subMeshPaint, subMeshTexture, shader);
		}
	}

	public void ClearBodyPartObjectLists()
	{
		currentBoneChildObjects.Clear();
	}

	private Vector3 GetBoneRotationOffset(Bone bone)
	{
		Vector3 result = Vector3.zero;
		switch (bone)
		{
		case Bone.LeftHand:
			result = new Vector3(0f, 0f, 0f);
			break;
		case Bone.RightUpperArm:
		case Bone.RightLowerArm:
		case Bone.RightHand:
			result = new Vector3(180f, 0f, 0f);
			break;
		case Bone.LeftFoot:
			result = new Vector3(0f, 0f, 0f);
			break;
		case Bone.RightUpperLeg:
		case Bone.RightLowerLeg:
		case Bone.RightFoot:
			result = new Vector3(180f, 0f, 0f);
			break;
		}
		return result;
	}
}
