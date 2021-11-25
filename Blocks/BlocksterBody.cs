using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x020000F7 RID: 247
	public class BlocksterBody
	{
		// Token: 0x06001214 RID: 4628 RVA: 0x0007BE82 File Offset: 0x0007A282
		public BlocksterBody(BlockAnimatedCharacter character)
		{
			this._character = character;
		}

		// Token: 0x06001215 RID: 4629 RVA: 0x0007BEA7 File Offset: 0x0007A2A7
		public List<GameObject> GetObjectsForBone(BlocksterBody.Bone bone)
		{
			if (this.currentBoneChildObjects.ContainsKey(bone))
			{
				return this.currentBoneChildObjects[bone];
			}
			return null;
		}

		// Token: 0x06001216 RID: 4630 RVA: 0x0007BEC8 File Offset: 0x0007A2C8
		public List<GameObject> GetObjectsForBodyPart(BlocksterBody.BodyPart bodyPart)
		{
			List<GameObject> list = new List<GameObject>();
			List<BlocksterBody.Bone> list2 = BlocksterBody.GetBonesForBodyPart(bodyPart);
			foreach (BlocksterBody.Bone bone in list2)
			{
				list.AddRange(this.GetObjectsForBone(bone));
			}
			return list;
		}

		// Token: 0x06001217 RID: 4631 RVA: 0x0007BF34 File Offset: 0x0007A334
		public void SubstitutePart(BlocksterBody.BodyPart part, string bodyPartVersionStr)
		{
			if (this.currentBodyPartVersions.ContainsKey(part) && this.currentBodyPartVersions[part] == bodyPartVersionStr)
			{
				return;
			}
			this.currentBodyPartVersions[part] = bodyPartVersionStr;
			GameObject gameObject = BlocksterBody.CreateBodyPartObject(bodyPartVersionStr);
			foreach (BlocksterBody.Bone key in BlocksterBody.GetBonesForBodyPart(part))
			{
				List<GameObject> list;
				if (this.currentBoneChildObjects.TryGetValue(key, out list))
				{
					foreach (GameObject obj in list)
					{
						UnityEngine.Object.Destroy(obj);
					}
					list.Clear();
				}
				else
				{
					list = new List<GameObject>();
					this.currentBoneChildObjects[key] = list;
				}
			}
			List<Transform> list2 = new List<Transform>();
			list2.Add(gameObject.transform);
			IEnumerator enumerator3 = gameObject.transform.GetEnumerator();
			try
			{
				while (enumerator3.MoveNext())
				{
					object obj2 = enumerator3.Current;
					Transform item = (Transform)obj2;
					list2.Add(item);
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator3 as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
			HashSet<GameObject> hashSet = new HashSet<GameObject>();
			foreach (Transform transform in list2)
			{
				BodyPartInfo component = transform.GetComponent<BodyPartInfo>();
				if (component == null)
				{
					hashSet.Add(transform.gameObject);
				}
				else
				{
					transform.localScale *= this._character.blocksterRigScale;
					Transform boneTransform = this._character.GetBoneTransform(component.bone);
					transform.parent = boneTransform;
					transform.localPosition = component.offsetFromBone;
					transform.localEulerAngles = this.GetBoneRotationOffset(component.bone);
					this.currentBoneChildObjects[component.bone].Add(transform.gameObject);
				}
			}
			foreach (GameObject obj3 in hashSet)
			{
				UnityEngine.Object.Destroy(obj3);
			}
			HashSet<string> animatorParametersForBodyParts = new HashSet<string>(this.currentBodyPartVersions.Values);
			this._character.stateHandler.SetAnimatorParametersForBodyParts(animatorParametersForBodyParts);
		}

		// Token: 0x06001218 RID: 4632 RVA: 0x0007C20C File Offset: 0x0007A60C
		public static BlocksterBody.BodyPart BodyPartFromString(string bodyPartStr)
		{
			BlocksterBody.BodyPart result = BlocksterBody.BodyPart.LeftArm;
			if (bodyPartStr.Contains("Arm Left"))
			{
				result = BlocksterBody.BodyPart.LeftArm;
			}
			else if (bodyPartStr.Contains("Arm Right"))
			{
				result = BlocksterBody.BodyPart.RightArm;
			}
			else if (bodyPartStr.Contains("Leg Left"))
			{
				result = BlocksterBody.BodyPart.LeftLeg;
			}
			else if (bodyPartStr.Contains("Leg Right"))
			{
				result = BlocksterBody.BodyPart.RightLeg;
			}
			return result;
		}

		// Token: 0x06001219 RID: 4633 RVA: 0x0007C274 File Offset: 0x0007A674
		public static List<BlocksterBody.Bone> GetBonesForBodyPart(BlocksterBody.BodyPart bodyPart)
		{
			if (BlocksterBody.bonesForBodyPart == null)
			{
				BlocksterBody.bonesForBodyPart = new Dictionary<BlocksterBody.BodyPart, List<BlocksterBody.Bone>>();
				BlocksterBody.bonesForBodyPart.Add(BlocksterBody.BodyPart.LeftArm, new List<BlocksterBody.Bone>
				{
					BlocksterBody.Bone.LeftHand,
					BlocksterBody.Bone.LeftLowerArm,
					BlocksterBody.Bone.LeftUpperArm
				});
				BlocksterBody.bonesForBodyPart.Add(BlocksterBody.BodyPart.RightArm, new List<BlocksterBody.Bone>
				{
					BlocksterBody.Bone.RightHand,
					BlocksterBody.Bone.RightLowerArm,
					BlocksterBody.Bone.RightUpperArm
				});
				BlocksterBody.bonesForBodyPart.Add(BlocksterBody.BodyPart.LeftLeg, new List<BlocksterBody.Bone>
				{
					BlocksterBody.Bone.LeftFoot,
					BlocksterBody.Bone.LeftLowerLeg,
					BlocksterBody.Bone.LeftUpperLeg
				});
				BlocksterBody.bonesForBodyPart.Add(BlocksterBody.BodyPart.RightLeg, new List<BlocksterBody.Bone>
				{
					BlocksterBody.Bone.RightFoot,
					BlocksterBody.Bone.RightLowerLeg,
					BlocksterBody.Bone.RightUpperLeg
				});
			}
			return BlocksterBody.bonesForBodyPart[bodyPart];
		}

		// Token: 0x0600121A RID: 4634 RVA: 0x0007C344 File Offset: 0x0007A744
		public static GameObject CreateBodyPartObject(string bodyPartStr)
		{
			GameObject gameObject = Resources.Load<GameObject>("BodyParts/" + bodyPartStr);
			if (gameObject == null)
			{
				BWLog.Error("Failed to find prefab for body part: " + bodyPartStr);
				return null;
			}
			return UnityEngine.Object.Instantiate<GameObject>(gameObject);
		}

		// Token: 0x0600121B RID: 4635 RVA: 0x0007C388 File Offset: 0x0007A788
		public void ApplyDefaultPaints(BlocksterBody.BodyPart bodyPart)
		{
			foreach (BlocksterBody.Bone bone in BlocksterBody.GetBonesForBodyPart(bodyPart))
			{
				foreach (GameObject gameObject in this.GetObjectsForBone(bone))
				{
					int subMeshIndex = this._character.GetSubMeshIndex(gameObject);
					if (subMeshIndex >= 0)
					{
						string text = null;
						if (this._character.characterType == CharacterType.Avatar)
						{
							text = "Dark Blue";
						}
						else
						{
							BodyPartInfo component = gameObject.GetComponent<BodyPartInfo>();
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
									text = this._character.GetCurrentHeadColor();
									break;
								case BodyPartInfo.ColorGroup.Shirt:
									text = this._character.GetCurrentBodyColor();
									break;
								default:
									text = component.defaultPaint;
									break;
								}
							}
						}
						if (text == null)
						{
							BWLog.Info(string.Concat(new object[]
							{
								"Failed to get default color for object ",
								gameObject,
								" on bone ",
								bone
							}));
							text = "Tan";
						}
						this._character.PaintTo(text, true, subMeshIndex);
					}
					else
					{
						BWLog.Info(string.Concat(new object[]
						{
							"object ",
							gameObject.name,
							"on bone ",
							bone,
							" has not been added to character submeshes"
						}));
					}
				}
			}
		}

		// Token: 0x0600121C RID: 4636 RVA: 0x0007C57C File Offset: 0x0007A97C
		public void ReapplyPaints(BlocksterBody.BodyPart bodyPart)
		{
			foreach (BlocksterBody.Bone bone in BlocksterBody.GetBonesForBodyPart(bodyPart))
			{
				foreach (GameObject obj in this.GetObjectsForBone(bone))
				{
					int subMeshIndex = this._character.GetSubMeshIndex(obj);
					if (subMeshIndex >= 0)
					{
						string paint = this._character.GetPaint(subMeshIndex);
						this._character.PaintTo(paint, true, subMeshIndex);
					}
				}
			}
		}

		// Token: 0x0600121D RID: 4637 RVA: 0x0007C64C File Offset: 0x0007AA4C
		public void PaintSkinColor(string paint, bool permanent)
		{
			this.PaintColorGroup(BodyPartInfo.ColorGroup.SkinFace, paint, permanent);
			this.PaintColorGroup(BodyPartInfo.ColorGroup.SkinRightArm, paint, permanent);
			this.PaintColorGroup(BodyPartInfo.ColorGroup.SkinLeftArm, paint, permanent);
			this.PaintColorGroup(BodyPartInfo.ColorGroup.SkinRightLeg, paint, permanent);
			this.PaintColorGroup(BodyPartInfo.ColorGroup.SkinLeftLeg, paint, permanent);
			this.PaintColorGroup(BodyPartInfo.ColorGroup.SkinRightHand, paint, permanent);
			this.PaintColorGroup(BodyPartInfo.ColorGroup.SkinLeftHand, paint, permanent);
		}

		// Token: 0x0600121E RID: 4638 RVA: 0x0007C698 File Offset: 0x0007AA98
		public void PaintShirtColor(string paint, bool permanent)
		{
			this.PaintColorGroup(BodyPartInfo.ColorGroup.Shirt, paint, permanent);
		}

		// Token: 0x0600121F RID: 4639 RVA: 0x0007C6A3 File Offset: 0x0007AAA3
		public void PaintPantsColor(string paint, bool permanent)
		{
			this.PaintColorGroup(BodyPartInfo.ColorGroup.Pant, paint, permanent);
		}

		// Token: 0x06001220 RID: 4640 RVA: 0x0007C6B0 File Offset: 0x0007AAB0
		public void PaintColorGroup(BodyPartInfo.ColorGroup group, string paint, bool permanent)
		{
			List<GameObject> list = new List<GameObject>();
			foreach (BlocksterBody.Bone bone in BlocksterBody.GetBonesForBodyPart(BlocksterBody.BodyPart.LeftArm))
			{
				list.AddRange(this.GetObjectsForBone(bone));
			}
			foreach (BlocksterBody.Bone bone2 in BlocksterBody.GetBonesForBodyPart(BlocksterBody.BodyPart.RightArm))
			{
				list.AddRange(this.GetObjectsForBone(bone2));
			}
			foreach (BlocksterBody.Bone bone3 in BlocksterBody.GetBonesForBodyPart(BlocksterBody.BodyPart.LeftLeg))
			{
				list.AddRange(this.GetObjectsForBone(bone3));
			}
			foreach (BlocksterBody.Bone bone4 in BlocksterBody.GetBonesForBodyPart(BlocksterBody.BodyPart.RightLeg))
			{
				list.AddRange(this.GetObjectsForBone(bone4));
			}
			foreach (GameObject gameObject in list)
			{
				int subMeshIndex = this._character.GetSubMeshIndex(gameObject);
				if (subMeshIndex >= 0)
				{
					BodyPartInfo component = gameObject.GetComponent<BodyPartInfo>();
					if (component != null && component.colorGroup == group && this._character.GetPaint(subMeshIndex) != paint)
					{
						this._character.PaintTo(paint, permanent, subMeshIndex);
					}
				}
			}
		}

		// Token: 0x06001221 RID: 4641 RVA: 0x0007C8B8 File Offset: 0x0007ACB8
		public void TextureColorGroup(BodyPartInfo.ColorGroup group, string texture, Vector3 normal, bool permanent)
		{
			List<GameObject> list = new List<GameObject>();
			foreach (BlocksterBody.Bone bone in BlocksterBody.GetBonesForBodyPart(BlocksterBody.BodyPart.LeftArm))
			{
				list.AddRange(this.GetObjectsForBone(bone));
			}
			foreach (BlocksterBody.Bone bone2 in BlocksterBody.GetBonesForBodyPart(BlocksterBody.BodyPart.RightArm))
			{
				list.AddRange(this.GetObjectsForBone(bone2));
			}
			foreach (BlocksterBody.Bone bone3 in BlocksterBody.GetBonesForBodyPart(BlocksterBody.BodyPart.LeftLeg))
			{
				list.AddRange(this.GetObjectsForBone(bone3));
			}
			foreach (BlocksterBody.Bone bone4 in BlocksterBody.GetBonesForBodyPart(BlocksterBody.BodyPart.RightLeg))
			{
				list.AddRange(this.GetObjectsForBone(bone4));
			}
			foreach (GameObject gameObject in list)
			{
				int subMeshIndex = this._character.GetSubMeshIndex(gameObject);
				if (subMeshIndex >= 0)
				{
					BodyPartInfo component = gameObject.GetComponent<BodyPartInfo>();
					if (component != null && component.colorGroup == group && this._character.GetTexture(subMeshIndex) != texture)
					{
						this._character.TextureTo(texture, normal, permanent, subMeshIndex, false);
					}
				}
			}
		}

		// Token: 0x06001222 RID: 4642 RVA: 0x0007CAC0 File Offset: 0x0007AEC0
		public void SetShaderForBodyPart(BlocksterBody.BodyPart bodyPart, ShaderType shader)
		{
			foreach (GameObject gameObject in this.GetObjectsForBodyPart(bodyPart))
			{
				BodyPartInfo component = gameObject.GetComponent<BodyPartInfo>();
				int subMeshIndex = this._character.GetSubMeshIndex(gameObject);
				string subMeshPaint = this._character.GetSubMeshPaint(subMeshIndex);
				string subMeshTexture = this._character.GetSubMeshTexture(subMeshIndex);
				MeshRenderer component2 = gameObject.GetComponent<MeshRenderer>();
				component2.sharedMaterial = Materials.GetMaterial(subMeshPaint, subMeshTexture, shader);
			}
		}

		// Token: 0x06001223 RID: 4643 RVA: 0x0007CB60 File Offset: 0x0007AF60
		public void ClearBodyPartObjectLists()
		{
			this.currentBoneChildObjects.Clear();
		}

		// Token: 0x06001224 RID: 4644 RVA: 0x0007CB70 File Offset: 0x0007AF70
		private Vector3 GetBoneRotationOffset(BlocksterBody.Bone bone)
		{
			Vector3 zero = Vector3.zero;
			switch (bone)
			{
			case BlocksterBody.Bone.LeftHand:
				zero = new Vector3(0f, 0f, 0f);
				break;
			case BlocksterBody.Bone.RightUpperArm:
			case BlocksterBody.Bone.RightLowerArm:
			case BlocksterBody.Bone.RightHand:
				zero = new Vector3(180f, 0f, 0f);
				break;
			case BlocksterBody.Bone.LeftFoot:
				zero = new Vector3(0f, 0f, 0f);
				break;
			case BlocksterBody.Bone.RightUpperLeg:
			case BlocksterBody.Bone.RightLowerLeg:
			case BlocksterBody.Bone.RightFoot:
				zero = new Vector3(180f, 0f, 0f);
				break;
			}
			return zero;
		}

		// Token: 0x04000E4F RID: 3663
		private BlockAnimatedCharacter _character;

		// Token: 0x04000E50 RID: 3664
		public static List<BlocksterBody.BodyPart> AllLimbs = new List<BlocksterBody.BodyPart>
		{
			BlocksterBody.BodyPart.LeftArm,
			BlocksterBody.BodyPart.RightArm,
			BlocksterBody.BodyPart.LeftLeg,
			BlocksterBody.BodyPart.RightLeg
		};

		// Token: 0x04000E51 RID: 3665
		public Dictionary<BlocksterBody.BodyPart, string> currentBodyPartVersions = new Dictionary<BlocksterBody.BodyPart, string>();

		// Token: 0x04000E52 RID: 3666
		private Dictionary<BlocksterBody.Bone, List<GameObject>> currentBoneChildObjects = new Dictionary<BlocksterBody.Bone, List<GameObject>>();

		// Token: 0x04000E53 RID: 3667
		private static Dictionary<BlocksterBody.BodyPart, List<BlocksterBody.Bone>> bonesForBodyPart;

		// Token: 0x020000F8 RID: 248
		public enum BodyPart
		{
			// Token: 0x04000E55 RID: 3669
			LeftArm,
			// Token: 0x04000E56 RID: 3670
			RightArm,
			// Token: 0x04000E57 RID: 3671
			LeftLeg,
			// Token: 0x04000E58 RID: 3672
			RightLeg
		}

		// Token: 0x020000F9 RID: 249
		public enum Bone
		{
			// Token: 0x04000E5A RID: 3674
			Root,
			// Token: 0x04000E5B RID: 3675
			Spine,
			// Token: 0x04000E5C RID: 3676
			Head,
			// Token: 0x04000E5D RID: 3677
			LeftUpperArm,
			// Token: 0x04000E5E RID: 3678
			LeftLowerArm,
			// Token: 0x04000E5F RID: 3679
			LeftHand,
			// Token: 0x04000E60 RID: 3680
			RightUpperArm,
			// Token: 0x04000E61 RID: 3681
			RightLowerArm,
			// Token: 0x04000E62 RID: 3682
			RightHand,
			// Token: 0x04000E63 RID: 3683
			LeftUpperLeg,
			// Token: 0x04000E64 RID: 3684
			LeftLowerLeg,
			// Token: 0x04000E65 RID: 3685
			LeftFoot,
			// Token: 0x04000E66 RID: 3686
			RightUpperLeg,
			// Token: 0x04000E67 RID: 3687
			RightLowerLeg,
			// Token: 0x04000E68 RID: 3688
			RightFoot
		}
	}
}
