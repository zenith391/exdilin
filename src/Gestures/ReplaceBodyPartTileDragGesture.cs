using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

namespace Gestures
{
	// Token: 0x02000185 RID: 389
	public class ReplaceBodyPartTileDragGesture : BaseGesture
	{
		// Token: 0x06001635 RID: 5685 RVA: 0x0009C405 File Offset: 0x0009A805
		public ReplaceBodyPartTileDragGesture(BuildPanel buildPanel)
		{
			this._buildPanel = buildPanel;
			this.touchBeginWindow = 12f;
		}

		// Token: 0x06001636 RID: 5686 RVA: 0x0009C420 File Offset: 0x0009A820
		public override void TouchesBegan(List<Touch> allTouches)
		{
			if (Blocksworld.CurrentState == State.Play || Blocksworld.InModalDialogState())
			{
				base.EnterState(GestureState.Failed);
				return;
			}
			if (base.gestureState == GestureState.Active)
			{
				return;
			}
			if (!TBox.dragBlockTween.IsFinished())
			{
				base.EnterState(GestureState.Failed);
				return;
			}
			if (CharacterEditor.Instance.InEditMode())
			{
				this._targetCharacter = CharacterEditor.Instance.CharacterBlock();
			}
			else if (Blocksworld.selectedBlock != null || Blocksworld.selectedBlock is BlockAnimatedCharacter)
			{
				this._targetCharacter = (Blocksworld.selectedBlock as BlockAnimatedCharacter);
			}
			if (this._targetCharacter == null)
			{
				base.EnterState(GestureState.Failed);
				return;
			}
			bool flag = allTouches[0].Phase == TouchPhase.Began;
			bool flag2 = (allTouches[0].Phase == TouchPhase.Moved || allTouches[0].Phase == TouchPhase.Stationary) && (float)allTouches[0].moveFrameCount < this.touchBeginWindow;
			if (!flag && !flag2)
			{
				base.EnterState(GestureState.Failed);
				return;
			}
			Blocksworld.blocksworldCamera.SetCameraStill(true);
			Vector2 position = allTouches[0].Position;
			if (this._buildPanel.Hit(position) && !Blocksworld.scriptPanel.Hit(position))
			{
				Tile tile = this._buildPanel.HitTile(position, false);
				if (tile != null && tile.gaf.Predicate.Name == "AnimCharacter.ReplaceBodyPart")
				{
					string stringArg = Util.GetStringArg(tile.gaf.Args, 0, string.Empty);
					ReplaceBodyPartTileDragGesture._bodyPartGAF = new GAF("AnimCharacter.ReplaceBodyPart", new object[]
					{
						stringArg
					});
					ReplaceBodyPartTileDragGesture._replacedBodyPartGAF = null;
					if (stringArg.StartsWith("Limb"))
					{
						this._bodyPartStrings = new string[]
						{
							stringArg + " Right",
							stringArg + " Left"
						};
						this._hasMirrorPart = true;
					}
					else
					{
						this._bodyPartStrings = new string[]
						{
							stringArg
						};
						this._hasMirrorPart = false;
					}
					int num = (!this._hasMirrorPart) ? 1 : 2;
					this._dragParts = new GameObject[num];
					for (int i = 0; i < num; i++)
					{
						this._dragParts[i] = BlocksterBody.CreateBodyPartObject(this._bodyPartStrings[i]);
						if (this._dragParts[i] == null)
						{
							BWLog.Error("Failed to load body part " + this._bodyPartStrings[i]);
							base.EnterState(GestureState.Failed);
							return;
						}
					}
					string currentHeadColor = this._targetCharacter.GetCurrentHeadColor();
					string currentBodyColor = this._targetCharacter.GetCurrentBodyColor();
					foreach (GameObject gameObject in this._dragParts)
					{
						BodyPartInfo[] componentsInChildren = gameObject.GetComponentsInChildren<BodyPartInfo>();
						foreach (BodyPartInfo bodyPartInfo in componentsInChildren)
						{
							MeshRenderer component = bodyPartInfo.gameObject.GetComponent<MeshRenderer>();
							switch (bodyPartInfo.colorGroup)
							{
							case BodyPartInfo.ColorGroup.SkinFace:
							case BodyPartInfo.ColorGroup.SkinRightArm:
							case BodyPartInfo.ColorGroup.SkinLeftArm:
							case BodyPartInfo.ColorGroup.SkinRightHand:
							case BodyPartInfo.ColorGroup.SkinLeftHand:
							case BodyPartInfo.ColorGroup.SkinRightLeg:
							case BodyPartInfo.ColorGroup.SkinLeftLeg:
								component.sharedMaterial = Materials.GetMaterial(currentHeadColor, "Plain", ShaderType.Normal);
								break;
							case BodyPartInfo.ColorGroup.Shirt:
								component.sharedMaterial = Materials.GetMaterial(currentBodyColor, "Plain", ShaderType.Normal);
								break;
							default:
								if (!string.IsNullOrEmpty(bodyPartInfo.defaultPaint))
								{
									component.sharedMaterial = Materials.GetMaterial(bodyPartInfo.defaultPaint, "Plain", ShaderType.Normal);
								}
								break;
							}
						}
					}
					this._targetParts = new BlocksterBody.BodyPart[num];
					this._targetPartBounds = new Bounds[num];
					this._originalBodyPartStrings = new string[num];
					for (int l = 0; l < num; l++)
					{
						BlocksterBody.BodyPart bodyPart = BlocksterBody.BodyPartFromString(this._bodyPartStrings[l]);
						this._targetParts[l] = bodyPart;
						List<BlocksterBody.Bone> bonesForBodyPart = BlocksterBody.GetBonesForBodyPart(bodyPart);
						Vector3 size = 0.25f * Vector3.one;
						Bounds bounds = new Bounds(this._targetCharacter.GetBoneTransform(bonesForBodyPart[0]).position, size);
						for (int m = 0; m < bonesForBodyPart.Count; m++)
						{
							Transform boneTransform = this._targetCharacter.GetBoneTransform(bonesForBodyPart[m]);
							bounds.Encapsulate(new Bounds(boneTransform.position, size));
							List<GameObject> objectsForBone = this._targetCharacter.bodyParts.GetObjectsForBone(bonesForBodyPart[m]);
							foreach (GameObject gameObject2 in objectsForBone)
							{
								MeshRenderer component2 = gameObject2.GetComponent<MeshRenderer>();
								if (component2 != null)
								{
									bounds.Encapsulate(component2.bounds);
								}
							}
						}
						this._targetPartBounds[l] = bounds;
						this._originalBodyPartStrings[l] = this._targetCharacter.CurrentlyAssignedPartVersion(bodyPart);
						this._targetCharacter.SetShaderForBodyPart(bodyPart, ShaderType.Glass);
					}
					this._dragTile = tile.Clone();
					this.Drag(position);
					Sound.PlaySound("Tile Start Drag", Sound.GetOrCreateOneShotAudioSource(), true, 0.2f, 1f, false);
					base.EnterState(GestureState.Active);
				}
				else
				{
					base.EnterState(GestureState.Possible);
				}
			}
		}

		// Token: 0x06001637 RID: 5687 RVA: 0x0009C9B4 File Offset: 0x0009ADB4
		public override void TouchesMoved(List<Touch> allTouches)
		{
			Vector2 position = allTouches[0].Position;
			this.Drag(position);
		}

		// Token: 0x06001638 RID: 5688 RVA: 0x0009C9D5 File Offset: 0x0009ADD5
		public override void TouchesEnded(List<Touch> allTouches)
		{
			this.Reset();
		}

		// Token: 0x06001639 RID: 5689 RVA: 0x0009C9DD File Offset: 0x0009ADDD
		public override void Cancel()
		{
			ReplaceBodyPartTileDragGesture._bodyPartGAF = null;
			ReplaceBodyPartTileDragGesture._replacedBodyPartGAF = null;
			base.Cancel();
		}

		// Token: 0x0600163A RID: 5690 RVA: 0x0009C9F4 File Offset: 0x0009ADF4
		public override void Reset()
		{
			if (this._targetCharacter != null && this._targetCharacter.go != null && this._targetParts != null)
			{
				foreach (BlocksterBody.BodyPart bodyPart in this._targetParts)
				{
					this._targetCharacter.SetShaderForBodyPart(bodyPart, ShaderType.Normal);
				}
			}
			if (this._dragParts != null)
			{
				foreach (GameObject gameObject in this._dragParts)
				{
					if (gameObject != null)
					{
						UnityEngine.Object.Destroy(gameObject);
					}
				}
				this._dragParts = null;
			}
			if (this._dragTile != null)
			{
				this._dragTile.Destroy();
				this._dragTile = null;
			}
			this._bodyPartStrings = null;
			if (this._partIsSnapped)
			{
				this._targetCharacter.SaveBodyPartAssignmentToTiles(this._snappedToPart);
				History.AddStateIfNecessary();
			}
			this._partIsSnapped = false;
			ReplaceBodyPartTileDragGesture._bodyPartGAF = null;
			ReplaceBodyPartTileDragGesture._replacedBodyPartGAF = null;
			base.EnterState(GestureState.Possible);
		}

		// Token: 0x0600163B RID: 5691 RVA: 0x0009CB04 File Offset: 0x0009AF04
		private void Drag(Vector2 pos)
		{
			Vector3 pos2 = pos - NormalizedScreen.pixelScale * 0.5f * 80f * Vector2.one;
			pos2.z = -0.5f;
			this._dragTile.MoveTo(pos2, false);
			Vector2 v = pos * NormalizedScreen.scale;
			Ray ray = Blocksworld.mainCamera.ScreenPointToRay(v);
			int num = 0;
			int num2 = 1;
			if (this._hasMirrorPart)
			{
				num2 = 2;
				Plane plane = new Plane(this._targetCharacter.goT.forward, this._targetCharacter.goT.position);
				float d;
				plane.Raycast(ray, out d);
				Vector3 a = ray.origin + ray.direction * d;
				float num3 = Vector3.Dot(this._targetCharacter.goT.right, a - this._targetCharacter.goT.position);
				if (num3 < 0f)
				{
					num = 1;
				}
			}
			GameObject gameObject = this._dragParts[num];
			BlocksterBody.BodyPart bodyPart = this._targetParts[num];
			Bounds bounds = this._targetPartBounds[num];
			string partStr = this._bodyPartStrings[num];
			string partStr2 = this._originalBodyPartStrings[num];
			Vector3 vector = Blocksworld.mainCamera.transform.InverseTransformPoint(bounds.center);
			float num4 = vector.z;
			Plane plane2 = new Plane(Vector3.up, this._targetCharacter.goT.position - Vector3.up);
			float a2;
			if (plane2.Raycast(ray, out a2))
			{
				num4 = Mathf.Min(a2, vector.z);
			}
			num4 -= 0.05f;
			Vector3 position = new Vector3(v.x, v.y, num4);
			Vector3 position2 = Blocksworld.mainCamera.ScreenToWorldPoint(position);
			for (int i = 0; i < num2; i++)
			{
				this._dragParts[i].transform.position = position2;
				this._dragParts[i].transform.rotation = this._targetCharacter.GetRotation();
				if (i != num)
				{
					this._dragParts[i].SetActive(false);
				}
			}
			bool flag = bounds.IntersectRay(ray);
			bool flag2 = false;
			string bodyPartStr = string.Empty;
			if (flag)
			{
				if (!this._partIsSnapped)
				{
					bodyPartStr = this._targetCharacter.CurrentlyAssignedPartVersion(bodyPart);
					this._targetCharacter.SubstituteBodyPart(bodyPart, partStr, true);
					this._partIsSnapped = true;
					this._targetCharacter.SetShaderForBodyPart(bodyPart, ShaderType.PulsateGlow);
					flag2 = true;
				}
				else if (this._snappedToPart != bodyPart)
				{
					bodyPartStr = this._targetCharacter.CurrentlyAssignedPartVersion(bodyPart);
					this._targetCharacter.SubstituteBodyPart(bodyPart, partStr, true);
					this._targetCharacter.SubstituteBodyPart(this._snappedToPart, this._originalBodyPartStrings[this._snappedToDragPartIndex], true);
					this._targetCharacter.SetShaderForBodyPart(this._snappedToPart, ShaderType.Glass);
					flag2 = true;
				}
			}
			else if (!flag && this._partIsSnapped)
			{
				bodyPartStr = this._targetCharacter.CurrentlyAssignedPartVersion(bodyPart);
				this._targetCharacter.SubstituteBodyPart(bodyPart, partStr2, true);
				this._partIsSnapped = false;
				this._targetCharacter.SetShaderForBodyPart(bodyPart, ShaderType.Glass);
				flag2 = true;
			}
			if (flag2)
			{
				ReplaceBodyPartTileDragGesture._replacedBodyPartGAF = this.GAFForBodyPartStr(bodyPartStr);
				Scarcity.inventoryScales[ReplaceBodyPartTileDragGesture._bodyPartGAF] = 1.5f;
				Scarcity.UpdateInventory(true, null);
			}
			if (this._partIsSnapped)
			{
				this._snappedToDragPartIndex = num;
				this._snappedToPart = bodyPart;
			}
			bool flag3 = this._buildPanel.Hit(pos);
			if (flag3)
			{
				gameObject.SetActive(false);
				this._dragTile.Show(!flag);
				this._dragTile.MoveTo(pos2, false);
			}
			else
			{
				gameObject.SetActive(!flag);
				this._dragTile.Show(false);
			}
		}

		// Token: 0x0600163C RID: 5692 RVA: 0x0009CEF4 File Offset: 0x0009B2F4
		private GAF GAFForBodyPartStr(string bodyPartStr)
		{
			if (bodyPartStr.EndsWith(" Left"))
			{
				bodyPartStr = bodyPartStr.Remove(bodyPartStr.Length - 5, 5);
			}
			else if (bodyPartStr.EndsWith(" Right"))
			{
				bodyPartStr = bodyPartStr.Remove(bodyPartStr.Length - 6, 6);
			}
			return new GAF("AnimCharacter.ReplaceBodyPart", new object[]
			{
				bodyPartStr
			});
		}

		// Token: 0x0600163D RID: 5693 RVA: 0x0009CF5C File Offset: 0x0009B35C
		public static HashSet<GAF> GetScarcityHighlightGafs(HashSet<GAF> result)
		{
			if (ReplaceBodyPartTileDragGesture._bodyPartGAF != null)
			{
				if (result == null)
				{
					result = new HashSet<GAF>();
				}
				result.Add(ReplaceBodyPartTileDragGesture._bodyPartGAF);
				if (ReplaceBodyPartTileDragGesture._replacedBodyPartGAF != null)
				{
					result.Add(ReplaceBodyPartTileDragGesture._replacedBodyPartGAF);
				}
			}
			return result;
		}

		// Token: 0x04001146 RID: 4422
		private BuildPanel _buildPanel;

		// Token: 0x04001147 RID: 4423
		private BlockAnimatedCharacter _targetCharacter;

		// Token: 0x04001148 RID: 4424
		private BlocksterBody.BodyPart[] _targetParts;

		// Token: 0x04001149 RID: 4425
		private BlocksterBody.BodyPart _snappedToPart;

		// Token: 0x0400114A RID: 4426
		private int _snappedToDragPartIndex;

		// Token: 0x0400114B RID: 4427
		private Bounds[] _targetPartBounds;

		// Token: 0x0400114C RID: 4428
		private GameObject[] _dragParts;

		// Token: 0x0400114D RID: 4429
		private bool _hasMirrorPart;

		// Token: 0x0400114E RID: 4430
		private Tile _dragTile;

		// Token: 0x0400114F RID: 4431
		private string[] _bodyPartStrings;

		// Token: 0x04001150 RID: 4432
		private string[] _originalBodyPartStrings;

		// Token: 0x04001151 RID: 4433
		private bool _overPanel;

		// Token: 0x04001152 RID: 4434
		private bool _partIsSnapped;

		// Token: 0x04001153 RID: 4435
		private static GAF _bodyPartGAF;

		// Token: 0x04001154 RID: 4436
		private static GAF _replacedBodyPartGAF;
	}
}
