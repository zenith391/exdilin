using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x02000075 RID: 117
	public class BlockAnimatedCharacter : BlockWalkable, IBlockster
	{
		// Token: 0x0600099A RID: 2458 RVA: 0x00044EA0 File Offset: 0x000432A0
		public BlockAnimatedCharacter(List<List<Tile>> tiles, float standingCharacterHeight, CharacterType charType) : base(tiles, 1, null, 0f)
		{
			this.characterType = charType;
			this.maxStepHeight = 1.5f;
			this.maxStepLength = 0.75f;
			this.capsuleColliderOffset = Vector3.up * 0.15f;
			this.capsuleColliderHeight = standingCharacterHeight;
			this.capsuleColliderRadius = 0.7f;
			this.moveCMOffsetFeetCenter = 0.75f;
			this.moveCMMaxDistance = 1f;
			this.moveCM = true;
			if (!BlockAnimatedCharacter.gotGlueIndices)
			{
				BlockAnimatedCharacter.idxBottom = CollisionTest.IndexOfGlueCollisionMesh("Character", "Bottom");
				BlockAnimatedCharacter.idxFront = CollisionTest.IndexOfGlueCollisionMesh("Character", "Front");
				BlockAnimatedCharacter.idxLeft = CollisionTest.IndexOfGlueCollisionMesh("Character", "Left");
				BlockAnimatedCharacter.idxRight = CollisionTest.IndexOfGlueCollisionMesh("Character", "Right");
				BlockAnimatedCharacter.idxBack = CollisionTest.IndexOfGlueCollisionMesh("Character", "Back");
				BlockAnimatedCharacter.gotGlueIndices = true;
			}
			this.stateHandler = new CharacterStateHandler();
			if (this.stateHandler == null)
			{
				BWLog.Error("Out of memory!");
			}
			else
			{
				GameObject gameObject = null;
				UnityEngine.Object @object = Resources.Load("Animation/Character/Blockster");
				if (null == @object)
				{
					BWLog.Error("Unable to load character rig");
				}
				else
				{
					gameObject = (UnityEngine.Object.Instantiate(@object) as GameObject);
					gameObject.name = "BlocksterRig";
				}
				if (gameObject != null)
				{
					this.stateHandler.SetTarget(this, gameObject);
					this.FindBones();
					string name = "Character Body";
					this.middle = this.goT.Find(name).gameObject;
					this.head = this.go;
					this.ParentToSkeleton();
					this.SizeMini();
					this.stateHandler.SetRole(this.DefaultRoleForCharacterType(this.characterType));
					this.stateHandler.ForceSit();
					BlockAnimatedCharacter.stateControllers[this] = this.stateHandler;
					gameObject.transform.localScale = this.blocksterRigScale * Vector3.one;
					this.bodyParts = new BlocksterBody(this);
					this.ResetBodyParts();
				}
			}
		}

		// Token: 0x0600099B RID: 2459 RVA: 0x00045135 File Offset: 0x00043535
		public void IBlockster_FindAttachments()
		{
			this.DetermineAttachments();
		}

		// Token: 0x0600099C RID: 2460 RVA: 0x0004513D File Offset: 0x0004353D
		public Block IBlockster_BottomAttachment()
		{
			return this.attachedBottomBlock;
		}

		// Token: 0x0600099D RID: 2461 RVA: 0x00045145 File Offset: 0x00043545
		public List<Block> IBlockster_HeadAttachments()
		{
			return this.attachedHeadBlocks;
		}

		// Token: 0x0600099E RID: 2462 RVA: 0x0004514D File Offset: 0x0004354D
		public Block IBlockster_FrontAttachment()
		{
			return this.attachedFrontBlock;
		}

		// Token: 0x0600099F RID: 2463 RVA: 0x00045155 File Offset: 0x00043555
		public Block IBlockster_BackAttachment()
		{
			return (this.attachedBackBlocks != null && this.attachedBackBlocks.Count != 0) ? this.attachedBackBlocks[0] : null;
		}

		// Token: 0x060009A0 RID: 2464 RVA: 0x00045184 File Offset: 0x00043584
		public Block IBlockster_RightHandAttachment()
		{
			return this.attachedRightBlock;
		}

		// Token: 0x060009A1 RID: 2465 RVA: 0x0004518C File Offset: 0x0004358C
		public Block IBlockster_LeftHandAttachement()
		{
			return this.attachedLeftBlock;
		}

		// Token: 0x060009A2 RID: 2466 RVA: 0x00045194 File Offset: 0x00043594
		public new static void Register()
		{
			BlockAnimatedCharacter.predicateCharacterJump = PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.Jump", (Block b) => new PredicateSensorDelegate(((BlockWalkable)b).IsJumping), (Block b) => new PredicateActionDelegate(((BlockWalkable)b).Jump), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Force"
			}, null);
			BlockAnimatedCharacter.predicateCharacterGotoTag = PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.GotoTag", null, (Block b) => new PredicateActionDelegate(((BlockWalkable)b).GotoTag), new Type[]
			{
				typeof(string),
				typeof(float),
				typeof(float)
			}, new string[]
			{
				string.Empty,
				"Speed",
				string.Empty
			}, null);
			BlockAnimatedCharacter.predicateCharacterChaseTag = PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.ChaseTag", null, (Block b) => new PredicateActionDelegate(((BlockWalkable)b).ChaseTag), new Type[]
			{
				typeof(string),
				typeof(float),
				typeof(float)
			}, new string[]
			{
				string.Empty,
				"Speed",
				string.Empty
			}, null);
			PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.GotoTap", null, (Block b) => new PredicateActionDelegate(((BlockWalkable)b).GotoTap), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Speed"
			}, null);
			BlockAnimatedCharacter.predicateCharacterMover = PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.DPadControl", null, (Block b) => new PredicateActionDelegate(((BlockWalkable)b).DPadControl), new Type[]
			{
				typeof(string),
				typeof(float),
				typeof(float)
			}, null, null);
			BlockAnimatedCharacter.predicateChracterTiltMover = PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.TiltMover", null, (Block b) => new PredicateActionDelegate(b.TiltMoverControl), new Type[]
			{
				typeof(float),
				typeof(int)
			}, null, null);
			PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.Translate", null, (Block b) => new PredicateActionDelegate(((BlockWalkable)b).Translate), new Type[]
			{
				typeof(Vector3),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.Turn", null, (Block b) => new PredicateActionDelegate(((BlockWalkable)b).Turn), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.TurnTowardsTag", null, (Block b) => new PredicateActionDelegate(((BlockWalkable)b).TurnTowardsTag), new Type[]
			{
				typeof(string)
			}, null, null);
			PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.TurnTowardsTap", null, (Block b) => new PredicateActionDelegate(((BlockWalkable)b).TurnTowardsTap), null, null, null);
			PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.TurnAlongCam", null, (Block b) => new PredicateActionDelegate(((BlockWalkable)b).TurnAlongCam), null, null, null);
			BlockAnimatedCharacter.predicateCharacterAvoidTag = PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.AvoidTag", null, (Block b) => new PredicateActionDelegate(((BlockWalkable)b).AvoidTag), new Type[]
			{
				typeof(string),
				typeof(float),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockAnimatedCharacter>("Block.StartAnimFirstPersonCamera", (Block b) => new PredicateSensorDelegate(b.IsFirstPersonBlock), (Block b) => new PredicateActionDelegate(b.FirstPersonCamera), new Type[]
			{
				typeof(int)
			}, null, null);
			PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.Idle", null, (Block b) => new PredicateActionDelegate(((BlockWalkable)b).Idle), null, null, null);
			PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.FreezeRotation", null, (Block b) => new PredicateActionDelegate(((BlockWalkable)b).FreezeRotation), null, null, null);
			PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.Stand", null, (Block b) => new PredicateActionDelegate(((BlockAnimatedCharacter)b).Stand), null, null, null);
			PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.Sit", null, (Block b) => new PredicateActionDelegate(((BlockAnimatedCharacter)b).Sit), null, null, null);
			PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.Collapse", (Block b) => new PredicateSensorDelegate(((BlockAnimatedCharacter)b).IsCollapsed), (Block b) => new PredicateActionDelegate(((BlockAnimatedCharacter)b).Collapse), null, null, null);
			PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.Recover", (Block b) => new PredicateSensorDelegate(((BlockAnimatedCharacter)b).IsNotCollapsed), (Block b) => new PredicateActionDelegate(((BlockAnimatedCharacter)b).Recover), null, null, null);
			PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.QueueState", null, (Block b) => new PredicateActionDelegate(((BlockAnimatedCharacter)b).QueueState), new Type[]
			{
				typeof(string)
			}, null, null);
			PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.InterruptState", null, (Block b) => new PredicateActionDelegate(((BlockAnimatedCharacter)b).InterruptState), new Type[]
			{
				typeof(string)
			}, null, null);
			PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.PlayAnim", null, (Block b) => new PredicateActionDelegate(((BlockAnimatedCharacter)b).PlayAnim), new Type[]
			{
				typeof(string)
			}, null, null);
			PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.DebugPlayAnim", null, (Block b) => new PredicateActionDelegate(((BlockAnimatedCharacter)b).DebugPlayAnim), new Type[]
			{
				typeof(string)
			}, null, null);
			PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.ToggleCrawl", null, (Block b) => new PredicateActionDelegate(((BlockAnimatedCharacter)b).ToggleCrawl), new Type[]
			{
				typeof(string)
			}, null, null);
			PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.IsJumping", (Block b) => new PredicateSensorDelegate(((BlockAnimatedCharacter)b).IsJumping), null, null, null, null);
			PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.IsSwimming", (Block b) => new PredicateSensorDelegate(((BlockAnimatedCharacter)b).IsSwimming), null, null, null, null);
			PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.IsCrawling", (Block b) => new PredicateSensorDelegate(((BlockAnimatedCharacter)b).IsCrawling), null, null, null, null);
			PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.IsWalking", (Block b) => new PredicateSensorDelegate(((BlockAnimatedCharacter)b).IsWalking), null, null, null, null);
			PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.IsProne", (Block b) => new PredicateSensorDelegate(((BlockAnimatedCharacter)b).IsProne), null, null, null, null);
			PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.DoubleJump", (Block b) => new PredicateSensorDelegate(((BlockAnimatedCharacter)b).CanDoubleJump), (Block b) => new PredicateActionDelegate(((BlockAnimatedCharacter)b).DoubleJump), null, null, null);
			PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.ApplyDiveForce", null, (Block b) => new PredicateActionDelegate(((BlockAnimatedCharacter)b).ApplyDiveForce), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.SetRole", (Block b) => new PredicateSensorDelegate(((BlockAnimatedCharacter)b).IsRole), (Block b) => new PredicateActionDelegate(((BlockAnimatedCharacter)b).SetRole), new Type[]
			{
				typeof(string)
			}, null, null);
			PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.ResetRole", null, (Block b) => new PredicateActionDelegate(((BlockAnimatedCharacter)b).ResetRole), null, null, null);
			PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.Attack", null, (Block b) => new PredicateActionDelegate(((BlockAnimatedCharacter)b).Attack), new Type[]
			{
				typeof(string)
			}, null, null);
			PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.StandingAttack", null, (Block b) => new PredicateActionDelegate(((BlockAnimatedCharacter)b).StandingAttack), new Type[]
			{
				typeof(string)
			}, null, null);
			PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.Shield", null, (Block b) => new PredicateActionDelegate(((BlockAnimatedCharacter)b).Shield), new Type[]
			{
				typeof(string)
			}, null, null);
			PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.Dodge", null, (Block b) => new PredicateActionDelegate(((BlockAnimatedCharacter)b).Dodge), new Type[]
			{
				typeof(string)
			}, null, null);
			BlockAnimatedCharacter.predicateReplaceLimb = PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.ReplaceBodyPart", null, (Block b) => new PredicateActionDelegate(((BlockAnimatedCharacter)b).ReplaceBodyPart), new Type[]
			{
				typeof(string),
				typeof(string)
			}, null, null);
		}

		// Token: 0x060009A3 RID: 2467 RVA: 0x00045B70 File Offset: 0x00043F70
		public static void StripNonCompatibleTiles(List<List<Tile>> tiles)
		{
			HashSet<Predicate> hashSet = new HashSet<Predicate>();
			hashSet.UnionWith(PredicateRegistry.ForType(typeof(BlockAnimatedCharacter), true));
			hashSet.UnionWith(PredicateRegistry.ForType(typeof(BlockWalkable), true));
			foreach (List<Tile> list in tiles)
			{
				for (int i = list.Count - 1; i >= 0; i--)
				{
					if (!hashSet.Contains(list[i].gaf.Predicate))
					{
						BWLog.Info("Removing non-compatible tile " + list[i].gaf);
						list.RemoveAt(i);
					}
				}
			}
		}

		// Token: 0x060009A4 RID: 2468 RVA: 0x00045C4C File Offset: 0x0004404C
		public void RebuildSubMeshes()
		{
			this.subMeshes = null;
			this.FindSubMeshes();
		}

		// Token: 0x060009A5 RID: 2469 RVA: 0x00045C5C File Offset: 0x0004405C
		public void RebuildSubMeshPaintTiles()
		{
			List<Tile> list = this.tiles[0];
			for (int i = list.Count - 1; i >= 0; i--)
			{
				Tile tile = list[i];
				string name = tile.gaf.Predicate.Name;
				if (name == "Block.PaintTo" && tile.gaf.Args.Length > 1)
				{
					int num = (int)tile.gaf.Args[1];
					if (num > 0)
					{
						list.RemoveAt(i);
					}
				}
			}
			for (int j = 0; j < this.subMeshPaints.Count; j++)
			{
				list.Add(new Tile(new GAF("Block.PaintTo", new object[]
				{
					this.subMeshPaints[j],
					j + 1
				})));
			}
		}

		// Token: 0x060009A6 RID: 2470 RVA: 0x00045D48 File Offset: 0x00044148
		private void ResetBodyParts()
		{
			string str = "Limb Old";
			if (this.characterType == CharacterType.Skeleton)
			{
				str = "Limb Skeleton";
			}
			this.bodyParts.currentBodyPartVersions.Clear();
			HashSet<BlocksterBody.BodyPart> hashSet = new HashSet<BlocksterBody.BodyPart>
			{
				BlocksterBody.BodyPart.LeftArm,
				BlocksterBody.BodyPart.LeftLeg,
				BlocksterBody.BodyPart.RightArm,
				BlocksterBody.BodyPart.RightLeg
			};
			HashSet<BlocksterBody.BodyPart> hashSet2 = new HashSet<BlocksterBody.BodyPart>();
			List<Tile> list = this.tiles[0];
			foreach (Tile tile in list)
			{
				if (tile.gaf.Predicate.Name == "AnimCharacter.ReplaceBodyPart" && tile.gaf.Args.Length > 1)
				{
					string value = (string)tile.gaf.Args[1];
					string bodyPartVersionStr = (string)tile.gaf.Args[0];
					BlocksterBody.BodyPart bodyPart = (BlocksterBody.BodyPart)Enum.Parse(typeof(BlocksterBody.BodyPart), value);
					this.bodyParts.SubstitutePart(bodyPart, bodyPartVersionStr);
					hashSet.Remove(bodyPart);
					hashSet2.Add(bodyPart);
				}
			}
			foreach (BlocksterBody.BodyPart bodyPart2 in hashSet)
			{
				if (bodyPart2 == BlocksterBody.BodyPart.LeftArm)
				{
					this.bodyParts.SubstitutePart(BlocksterBody.BodyPart.LeftArm, str + " Arm Left");
				}
				else if (bodyPart2 == BlocksterBody.BodyPart.RightArm)
				{
					this.bodyParts.SubstitutePart(BlocksterBody.BodyPart.RightArm, str + " Arm Right");
				}
				else if (bodyPart2 == BlocksterBody.BodyPart.LeftLeg)
				{
					this.bodyParts.SubstitutePart(BlocksterBody.BodyPart.LeftLeg, str + " Leg Left");
				}
				else if (bodyPart2 == BlocksterBody.BodyPart.RightLeg)
				{
					this.bodyParts.SubstitutePart(BlocksterBody.BodyPart.RightLeg, str + " Leg Right");
				}
			}
			this.UpdateCachedBodyParts();
			this.RebuildSubMeshes();
			this.ReapplyInitPaintAndTexures();
			foreach (BlocksterBody.BodyPart bodyPart3 in hashSet)
			{
				this.bodyParts.ApplyDefaultPaints(bodyPart3);
				this.SaveBodyPartAssignmentToTiles(bodyPart3);
			}
		}

		// Token: 0x060009A7 RID: 2471 RVA: 0x00045FCC File Offset: 0x000443CC
		public void ReapplyInitPaintAndTexures()
		{
			if (this.characterType == CharacterType.Avatar)
			{
				bool flag = this.lockPaintAndTexture;
				this.lockPaintAndTexture = false;
				this.PaintTo("Dark Blue", true, 0);
				for (int i = 0; i < this.subMeshGameObjects.Count; i++)
				{
					this.PaintTo("Dark Blue", true, i);
				}
				this.lockPaintAndTexture = flag;
				return;
			}
			Dictionary<int, string> dictionary = new Dictionary<int, string>();
			Dictionary<int, string> dictionary2 = new Dictionary<int, string>();
			Dictionary<int, Vector3> dictionary3 = new Dictionary<int, Vector3>();
			List<Tile> list = this.tiles[0];
			foreach (Tile tile in list)
			{
				Predicate predicate = tile.gaf.Predicate;
				object[] args = tile.gaf.Args;
				if (predicate == Block.predicatePaintTo && args.Length > 0)
				{
					int key = (args.Length <= 1) ? 0 : ((int)tile.gaf.Args[1]);
					string value = (string)tile.gaf.Args[0];
					dictionary[key] = value;
				}
				if (predicate == Block.predicateTextureTo && args.Length > 1)
				{
					int key2 = (tile.gaf.Args.Length <= 2) ? 0 : ((int)tile.gaf.Args[2]);
					string value2 = (string)tile.gaf.Args[0];
					Vector3 value3 = (Vector3)tile.gaf.Args[1];
					dictionary2[key2] = value2;
					dictionary3[key2] = value3;
				}
			}
			foreach (KeyValuePair<int, string> keyValuePair in dictionary)
			{
				this.PaintTo(keyValuePair.Value, true, keyValuePair.Key);
			}
			foreach (KeyValuePair<int, string> keyValuePair2 in dictionary2)
			{
				this.TextureTo(keyValuePair2.Value, dictionary3[keyValuePair2.Key], true, keyValuePair2.Key, false);
			}
		}

		// Token: 0x060009A8 RID: 2472 RVA: 0x00046278 File Offset: 0x00044678
		protected override void FindSubMeshes()
		{
			if (this.subMeshes == null)
			{
				if (this.bodyParts == null)
				{
					base.FindSubMeshes();
					return;
				}
				string text = "Yellow";
				string text2 = "Plain";
				if (this.subMeshPaints != null && this.subMeshPaints.Count > 0)
				{
					text = this.subMeshPaints[0];
				}
				if (this.subMeshTextures != null && this.subMeshTextures.Count > 0)
				{
					text2 = this.subMeshTextures[0];
				}
				string text3 = "Brown";
				if (this.hair != null)
				{
					int subMeshIndex = base.GetSubMeshIndex(this.hair.gameObject);
					if (subMeshIndex > 0 && subMeshIndex <= this.subMeshPaints.Count)
					{
						text3 = this.subMeshPaints[subMeshIndex - 1];
					}
				}
				this.subMeshes = new List<CollisionMesh>();
				this.subMeshGameObjects = new List<GameObject>();
				this.subMeshPaints = new List<string>();
				this.subMeshTextures = new List<string>();
				this.subMeshTextureNormals = new List<Vector3>();
				List<GameObject> list = new List<GameObject>();
				list.Add(this.head);
				list.Add(this.middle);
				list.AddRange(this.bodyParts.GetObjectsForBone(BlocksterBody.Bone.RightFoot));
				list.AddRange(this.bodyParts.GetObjectsForBone(BlocksterBody.Bone.LeftFoot));
				list.AddRange(this.bodyParts.GetObjectsForBone(BlocksterBody.Bone.RightHand));
				list.AddRange(this.bodyParts.GetObjectsForBone(BlocksterBody.Bone.LeftHand));
				if (this.hair != null)
				{
					list.Add(this.hair.gameObject);
				}
				list.AddRange(this.bodyParts.GetObjectsForBone(BlocksterBody.Bone.RightLowerLeg));
				list.AddRange(this.bodyParts.GetObjectsForBone(BlocksterBody.Bone.RightUpperLeg));
				list.AddRange(this.bodyParts.GetObjectsForBone(BlocksterBody.Bone.LeftLowerLeg));
				list.AddRange(this.bodyParts.GetObjectsForBone(BlocksterBody.Bone.LeftUpperLeg));
				list.AddRange(this.bodyParts.GetObjectsForBone(BlocksterBody.Bone.RightLowerArm));
				list.AddRange(this.bodyParts.GetObjectsForBone(BlocksterBody.Bone.RightUpperArm));
				list.AddRange(this.bodyParts.GetObjectsForBone(BlocksterBody.Bone.LeftLowerArm));
				list.AddRange(this.bodyParts.GetObjectsForBone(BlocksterBody.Bone.LeftUpperArm));
				this.canBeTextured = new bool[list.Count];
				this.canBeMaterialTextured = new bool[list.Count];
				this.canBeTextured[0] = true;
				this.canBeMaterialTextured[0] = true;
				this.childMeshes = new Dictionary<string, Mesh>();
				for (int i = 1; i < list.Count; i++)
				{
					GameObject gameObject = list[i];
					this.subMeshes.Add(null);
					this.subMeshGameObjects.Add(gameObject);
					bool flag = false;
					bool flag2 = true;
					string item = "Black";
					string item2 = "Plain";
					if (i == 1)
					{
						item = text;
						item2 = text2;
						flag = true;
					}
					else if (this.hair != null && gameObject == this.hair.gameObject)
					{
						item = text3;
						flag = true;
					}
					else
					{
						BodyPartInfo component = gameObject.GetComponent<BodyPartInfo>();
						if (component != null)
						{
							if (!string.IsNullOrEmpty(component.currentPaint))
							{
								item = component.currentPaint;
							}
							if (!string.IsNullOrEmpty(component.currentTexture))
							{
								item2 = component.currentTexture;
							}
							flag = component.canBeTextured;
							flag2 = component.canBeMaterialTextured;
						}
					}
					this.subMeshPaints.Add(item);
					this.subMeshTextures.Add(item2);
					this.subMeshTextureNormals.Add(Vector3.up);
					this.canBeTextured[i] = flag;
					this.canBeMaterialTextured[i] = flag2;
					MeshFilter component2 = gameObject.GetComponent<MeshFilter>();
					if (component2 != null)
					{
						this.childMeshes[gameObject.name] = component2.mesh;
					}
				}
			}
		}

		// Token: 0x060009A9 RID: 2473 RVA: 0x00046660 File Offset: 0x00044A60
		public override int GetMeshIndexForRay(Ray ray, bool refresh, out Vector3 point, out Vector3 normal)
		{
			int meshIndexForRay = base.GetMeshIndexForRay(ray, refresh, out point, out normal);
			if (meshIndexForRay > 0)
			{
				return meshIndexForRay;
			}
			MeshRenderer meshRenderer = this.GetCloneHead() as MeshRenderer;
			Bounds bounds = meshRenderer.bounds;
			List<Bounds> list = new List<Bounds>();
			for (int i = 0; i < this.subMeshGameObjects.Count; i++)
			{
				GameObject gameObject = this.subMeshGameObjects[i];
				MeshRenderer component = gameObject.GetComponent<MeshRenderer>();
				list.Add(component.bounds);
			}
			float magnitude = (bounds.center - ray.origin).magnitude;
			Vector3 vector = ray.origin + ray.direction * magnitude;
			Vector3 a = bounds.ClosestPoint(vector);
			float num = (a - vector).sqrMagnitude;
			int result = 0;
			for (int j = 0; j < list.Count; j++)
			{
				float magnitude2 = (list[j].center - ray.origin).magnitude;
				Vector3 vector2 = ray.origin + ray.direction * magnitude2;
				Vector3 a2 = list[j].ClosestPoint(vector2);
				float sqrMagnitude = (a2 - vector2).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					result = j + 1;
				}
			}
			return result;
		}

		// Token: 0x060009AA RID: 2474 RVA: 0x000467E4 File Offset: 0x00044BE4
		private CharacterRole DefaultRoleForCharacterType(CharacterType type)
		{
			CharacterRole result = CharacterRole.Male;
			switch (type)
			{
			case CharacterType.Female:
				result = CharacterRole.Female;
				break;
			case CharacterType.Skeleton:
				result = CharacterRole.Skeleton;
				break;
			case CharacterType.MiniMale:
				result = CharacterRole.Mini;
				break;
			case CharacterType.MiniFemale:
				result = CharacterRole.MiniFemale;
				break;
			}
			return result;
		}

		// Token: 0x060009AB RID: 2475 RVA: 0x00046834 File Offset: 0x00044C34
		private void SizeMini()
		{
			if (this.stateHandler != null && (this.characterType == CharacterType.MiniMale || this.characterType == CharacterType.MiniFemale))
			{
				this.stateHandler.targetRig.transform.localScale = Vector3.one * 0.8f;
				Transform transform = this.FindRecursive("bJoint_head_scale", this.stateHandler.targetRig.transform);
				transform.localScale = Vector3.one * 1.25f;
			}
		}

		// Token: 0x060009AC RID: 2476 RVA: 0x000468BC File Offset: 0x00044CBC
		private Renderer GetCloneHead()
		{
			if (this.headClone == null)
			{
				GameObject gameObject = new GameObject("Character Head");
				MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
				MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
				meshFilter.sharedMesh = this.go.GetComponent<MeshFilter>().sharedMesh;
				meshRenderer.sharedMaterial = this.go.GetComponent<MeshRenderer>().sharedMaterial;
				this.headClone = gameObject.GetComponent<Renderer>();
				this.headClone.transform.parent = this.goT;
			}
			return this.headClone;
		}

		// Token: 0x060009AD RID: 2477 RVA: 0x00046948 File Offset: 0x00044D48
		private Transform FindRecursive(string name, Transform parent)
		{
			if (parent.name == name)
			{
				return parent;
			}
			for (int i = 0; i < parent.childCount; i++)
			{
				Transform child = parent.GetChild(i);
				if (child.name == name)
				{
					return child;
				}
				Transform transform = this.FindRecursive(name, child);
				if (transform != null)
				{
					return transform;
				}
			}
			return null;
		}

		// Token: 0x060009AE RID: 2478 RVA: 0x000469B4 File Offset: 0x00044DB4
		private void ParentToSkeleton()
		{
			this.GetCloneHead();
			foreach (KeyValuePair<string, string> keyValuePair in this.blocksterToSkeleton)
			{
				string key = keyValuePair.Key;
				Transform transform = this.goT.FindChild(key);
				if (transform != null)
				{
					transform.gameObject.SetActive(true);
					if (transform.GetComponent<MeshRenderer>() != null)
					{
						transform.GetComponent<MeshRenderer>().enabled = true;
					}
					string value = keyValuePair.Value;
					Transform transform2 = this.FindRecursive(value, this.stateHandler.targetRig.transform);
					if (transform2 != null)
					{
						if (transform.GetComponent<Rigidbody>() != null)
						{
							UnityEngine.Object.Destroy(transform.GetComponent<Rigidbody>());
							if (transform.GetComponent<BoxCollider>() != null)
							{
								transform.GetComponent<BoxCollider>().enabled = false;
							}
						}
						transform.parent = transform2;
						transform.localScale = this.GetRelativeMeshToBoneScale(key);
						transform.localEulerAngles = Vector3.zero;
						transform.localPosition = this.GetRelativeMeshToBonePos(key);
						if (key == "Character Short Hair")
						{
							this.hair = transform;
						}
						if (!this.characterPieces.Contains(transform))
						{
							this.characterPieces.Add(transform);
						}
					}
					else
					{
						BWLog.Error("Failed to find Bone " + value + " for " + transform.name);
					}
				}
			}
		}

		// Token: 0x060009AF RID: 2479 RVA: 0x00046B54 File Offset: 0x00044F54
		public void PrepareForModelIconRender(Layer screenshotLayer)
		{
			this.goLayerAssignment = screenshotLayer;
			this.stateHandler.ForceSit();
			this.stateHandler.preventLook = true;
			this.stateHandler.targetRig.SetLayer(screenshotLayer, true);
		}

		// Token: 0x060009B0 RID: 2480 RVA: 0x00046B88 File Offset: 0x00044F88
		private Vector3 GetRelativeMeshToBonePos(string ourName)
		{
			Vector3 zero = Vector3.zero;
			if (ourName != null)
			{
				if (!(ourName == "Character Body"))
				{
					if (ourName == "Character Short Hair" || ourName == "Character Head")
					{
						zero = new Vector3(0f, -0.082f, 0.06f);
					}
				}
				else
				{
					zero = new Vector3(0f, 0.27f, 0.051f);
				}
			}
			return zero;
		}

		// Token: 0x060009B1 RID: 2481 RVA: 0x00046C10 File Offset: 0x00045010
		private Vector3 GetRelativeMeshToBoneScale(string ourName)
		{
			Vector3 one = Vector3.one;
			if (ourName != null)
			{
				if (!(ourName == "Character Body"))
				{
					if (ourName == "Character Short Hair" || ourName == "Character Head")
					{
						one = new Vector3(0.8f, 0.8f, 0.8f);
					}
				}
				else
				{
					one = new Vector3(0.7f, 0.68f, 0.44f);
				}
			}
			return one;
		}

		// Token: 0x060009B2 RID: 2482 RVA: 0x00046C98 File Offset: 0x00045098
		public override GameObject GetHandAttach(int hand)
		{
			if (hand == 0)
			{
				string name = "bJoint_handRight_attach";
				return this.FindRecursive(name, this.stateHandler.targetRig.transform).gameObject;
			}
			if (hand != 1)
			{
				return base.GetHandAttach(hand);
			}
			string name2 = "bJoint_handLeft_attach";
			return this.FindRecursive(name2, this.stateHandler.targetRig.transform).gameObject;
		}

		// Token: 0x060009B3 RID: 2483 RVA: 0x00046D04 File Offset: 0x00045104
		public Transform GetLeftFootBoneTransform()
		{
			return this.FindRecursive("bJoint_footLeft", this.stateHandler.targetRig.transform);
		}

		// Token: 0x060009B4 RID: 2484 RVA: 0x00046D21 File Offset: 0x00045121
		public Transform GetRightFootBoneTransform()
		{
			return this.FindRecursive("bJoint_footRight", this.stateHandler.targetRig.transform);
		}

		// Token: 0x060009B5 RID: 2485 RVA: 0x00046D40 File Offset: 0x00045140
		public GameObject GetHeadAttach()
		{
			string name = "bJoint_head_attach";
			GameObject gameObject = this.FindRecursive(name, this.stateHandler.targetRig.transform).gameObject;
			return (!(gameObject != null)) ? null : gameObject.gameObject;
		}

		// Token: 0x060009B6 RID: 2486 RVA: 0x00046D88 File Offset: 0x00045188
		public GameObject GetBodyAttach()
		{
			string name = "bJoint_hips";
			return this.FindRecursive(name, this.stateHandler.targetRig.transform).gameObject;
		}

		// Token: 0x060009B7 RID: 2487 RVA: 0x00046DB7 File Offset: 0x000451B7
		public override void OnCreate()
		{
			this.HideSourceBlockster();
			if (this.characterType == CharacterType.Avatar)
			{
				this.lockPaintAndTexture = true;
			}
		}

		// Token: 0x060009B8 RID: 2488 RVA: 0x00046DD2 File Offset: 0x000451D2
		public string GetCurrentHeadColor()
		{
			return this.GetPaint(0);
		}

		// Token: 0x060009B9 RID: 2489 RVA: 0x00046DDB File Offset: 0x000451DB
		public string GetCurrentBodyColor()
		{
			return this.GetPaint(1);
		}

		// Token: 0x060009BA RID: 2490 RVA: 0x00046DE4 File Offset: 0x000451E4
		public override void OnReconstructed()
		{
			this.HideSourceBlockster();
		}

		// Token: 0x060009BB RID: 2491 RVA: 0x00046DEC File Offset: 0x000451EC
		private void HideSourceBlockster()
		{
			this.go.GetComponent<MeshRenderer>().enabled = false;
		}

		// Token: 0x060009BC RID: 2492 RVA: 0x00046E00 File Offset: 0x00045200
		public override void Destroy()
		{
			BlockAnimatedCharacter.stateControllers.Remove(this);
			this.stateHandler = null;
			if (this.hands != null)
			{
				for (int i = 0; i < this.hands.Length; i++)
				{
					if (this.hands[i] != null && this.hands[i].gameObject != null)
					{
						UnityEngine.Object.Destroy(this.hands[i].gameObject);
					}
				}
			}
			if (this.middle != null)
			{
				UnityEngine.Object.Destroy(this.middle);
			}
			this.bodyParts.ClearBodyPartObjectLists();
			base.Destroy();
		}

		// Token: 0x060009BD RID: 2493 RVA: 0x00046EB0 File Offset: 0x000452B0
		public override void RemoveBlockMaps()
		{
			if (this.hands != null)
			{
				for (int i = 0; i < this.hands.Length; i++)
				{
					if (this.hands[i].gameObject != null)
					{
						BWSceneManager.RemoveChildBlockInstanceID(this.hands[i].gameObject);
					}
				}
			}
			if (this.middle != null)
			{
				BWSceneManager.RemoveChildBlockInstanceID(this.middle);
			}
			base.RemoveBlockMaps();
		}

		// Token: 0x060009BE RID: 2494 RVA: 0x00046F30 File Offset: 0x00045330
		private void FindBones()
		{
			this.boneLookup = new Dictionary<BlocksterBody.Bone, Transform>();
			Transform transform = this.stateHandler.targetRig.transform;
			this.boneLookup[BlocksterBody.Bone.Root] = this.FindRecursive("bJoint_hips", transform);
			this.boneLookup[BlocksterBody.Bone.Spine] = this.FindRecursive("bJoint_spine", transform);
			this.boneLookup[BlocksterBody.Bone.Head] = this.FindRecursive("bJoint_head", transform);
			this.boneLookup[BlocksterBody.Bone.LeftUpperArm] = this.FindRecursive("bJoint_upperArmLeft", transform);
			this.boneLookup[BlocksterBody.Bone.LeftLowerArm] = this.FindRecursive("bJoint_lowerArmLeft", transform);
			this.boneLookup[BlocksterBody.Bone.LeftHand] = this.FindRecursive("bJoint_handLeft", transform);
			this.boneLookup[BlocksterBody.Bone.RightUpperArm] = this.FindRecursive("bJoint_upperArmRight", transform);
			this.boneLookup[BlocksterBody.Bone.RightLowerArm] = this.FindRecursive("bJoint_lowerArmRight", transform);
			this.boneLookup[BlocksterBody.Bone.RightHand] = this.FindRecursive("bJoint_handRight", transform);
			this.boneLookup[BlocksterBody.Bone.LeftUpperLeg] = this.FindRecursive("bJoint_upperLegLeft", transform);
			this.boneLookup[BlocksterBody.Bone.LeftLowerLeg] = this.FindRecursive("bJoint_lowerLegLeft", transform);
			this.boneLookup[BlocksterBody.Bone.LeftFoot] = this.FindRecursive("bJoint_footLeft", transform);
			this.boneLookup[BlocksterBody.Bone.RightUpperLeg] = this.FindRecursive("bJoint_upperLegRight", transform);
			this.boneLookup[BlocksterBody.Bone.RightLowerLeg] = this.FindRecursive("bJoint_lowerLegRight", transform);
			this.boneLookup[BlocksterBody.Bone.RightFoot] = this.FindRecursive("bJoint_footRight", transform);
		}

		// Token: 0x060009BF RID: 2495 RVA: 0x000470C8 File Offset: 0x000454C8
		public Transform GetBoneTransform(BlocksterBody.Bone bone)
		{
			Transform result;
			if (!this.boneLookup.TryGetValue(bone, out result))
			{
				BWLog.Error("No Transform assigned to bone: " + bone);
			}
			return result;
		}

		// Token: 0x060009C0 RID: 2496 RVA: 0x00047100 File Offset: 0x00045500
		public void UpdateCachedBodyParts()
		{
			this.feet = new FootInfo[2 * this.legPairCount];
			for (int i = 0; i < this.feet.Length; i++)
			{
				this.feet[i] = new FootInfo();
			}
			this.footRt = null;
			this.footLt = null;
			List<GameObject> objectsForBone = this.bodyParts.GetObjectsForBone(BlocksterBody.Bone.RightFoot);
			List<GameObject> objectsForBone2 = this.bodyParts.GetObjectsForBone(BlocksterBody.Bone.LeftFoot);
			if (objectsForBone != null && objectsForBone.Count > 0)
			{
				this.feet[0].go = objectsForBone[0];
				this.footRt = objectsForBone[0].transform;
			}
			if (objectsForBone2 != null && objectsForBone2.Count > 0)
			{
				this.feet[1].go = objectsForBone2[0];
				this.footLt = objectsForBone2[0].transform;
			}
			this.hands[0] = this.bodyParts.GetObjectsForBone(BlocksterBody.Bone.RightHand)[0];
			this.hands[1] = this.bodyParts.GetObjectsForBone(BlocksterBody.Bone.LeftHand)[0];
			if (this.hands[0] != null && this.middle != null)
			{
				this.handPos = this.hands[0].transform.localPosition;
				this.handsOutXMod = this.middle.transform.localScale.x * 0.5f;
				this.handsOutYMod -= -(0.6f + this.hands[0].transform.localPosition.y);
			}
		}

		// Token: 0x060009C1 RID: 2497 RVA: 0x000472A4 File Offset: 0x000456A4
		public override void Pause()
		{
			base.Pause();
			if (this.hands[0].GetComponent<Rigidbody>() != null)
			{
				this.pausedVelocityHands0 = this.hands[0].GetComponent<Rigidbody>().velocity;
				this.pausedAngularVelocityHands0 = this.hands[0].GetComponent<Rigidbody>().angularVelocity;
				this.hands[0].GetComponent<Rigidbody>().isKinematic = true;
			}
			if (this.hands[1].GetComponent<Rigidbody>() != null)
			{
				this.pausedVelocityHands1 = this.hands[1].GetComponent<Rigidbody>().velocity;
				this.pausedAngularVelocityHands1 = this.hands[1].GetComponent<Rigidbody>().angularVelocity;
				this.hands[1].GetComponent<Rigidbody>().isKinematic = true;
			}
			if (this.middle.GetComponent<Rigidbody>() != null)
			{
				this.pausedVelocityMiddle = this.middle.GetComponent<Rigidbody>().velocity;
				this.pausedAngularVelocityMiddle = this.middle.GetComponent<Rigidbody>().angularVelocity;
				this.middle.GetComponent<Rigidbody>().isKinematic = true;
			}
		}

		// Token: 0x060009C2 RID: 2498 RVA: 0x000473C0 File Offset: 0x000457C0
		public override void Resume()
		{
			base.Resume();
			if (this.hands[0].GetComponent<Rigidbody>() != null)
			{
				this.hands[0].GetComponent<Rigidbody>().isKinematic = false;
				this.hands[0].GetComponent<Rigidbody>().velocity = this.pausedVelocityHands0;
				this.hands[0].GetComponent<Rigidbody>().angularVelocity = this.pausedAngularVelocityHands0;
			}
			if (this.hands[1].GetComponent<Rigidbody>() != null)
			{
				this.hands[1].GetComponent<Rigidbody>().isKinematic = false;
				this.hands[1].GetComponent<Rigidbody>().velocity = this.pausedVelocityHands1;
				this.hands[1].GetComponent<Rigidbody>().angularVelocity = this.pausedAngularVelocityHands1;
			}
			if (this.middle.GetComponent<Rigidbody>() != null)
			{
				this.middle.GetComponent<Rigidbody>().isKinematic = false;
				this.middle.GetComponent<Rigidbody>().velocity = this.pausedVelocityMiddle;
				this.middle.GetComponent<Rigidbody>().angularVelocity = this.pausedAngularVelocityMiddle;
			}
		}

		// Token: 0x060009C3 RID: 2499 RVA: 0x000474DC File Offset: 0x000458DC
		public override void Break(Vector3 chunkPos, Vector3 chunkVel, Vector3 chunkAngVel)
		{
			base.Break(chunkPos, chunkVel, chunkAngVel);
			this.RemoveMeshFromSkeleton();
			Blocksworld.AddFixedUpdateCommand(new DelayedBreakCharacterCommand(this, chunkPos, chunkVel, chunkAngVel));
		}

		// Token: 0x060009C4 RID: 2500 RVA: 0x000474FC File Offset: 0x000458FC
		public override void ReassignedToChunk(Chunk c)
		{
			this.StopPull();
			this.stateHandler.rb = c.rb;
			foreach (Block block in c.blocks)
			{
				if (block == this.attachedLeftBlock || this.attachedLeftHandBlocks.Contains(block))
				{
					block.goT.SetParent(this.GetHandAttach(1).transform);
				}
				else if (block == this.attachedRightBlock || this.attachedRightHandBlocks.Contains(block))
				{
					block.goT.SetParent(this.GetHandAttach(0).transform);
				}
				else if (this.attachedHeadBlocks.Contains(block))
				{
					block.goT.SetParent(this.GetHeadAttach().transform);
				}
				else if (this.attachedBackBlocks.Contains(block))
				{
					block.goT.SetParent(this.GetBodyAttach().transform);
				}
			}
		}

		// Token: 0x060009C5 RID: 2501 RVA: 0x00047630 File Offset: 0x00045A30
		public bool IsAttachment(Block b)
		{
			if (!this.haveDeterminedAttachments)
			{
				this.DetermineAttachments();
			}
			return this.attachedLeftBlock == b || this.attachedRightBlock == b || this.attachedBottomBlock == b || this.attachedFrontBlock == b || this.attachedBackBlocks.Contains(b) || this.attachedHeadBlocks.Contains(b);
		}

		// Token: 0x060009C6 RID: 2502 RVA: 0x000476A0 File Offset: 0x00045AA0
		private bool IsUnsupportedAttachmentType(Block b)
		{
			return b != this && (b is BlockWalkable || b is BlockCharacter || b is BlockAbstractLegs || b is BlockAbstractWheel || b is BlockAbstractMotor || b is BlockAbstractTorsionSpring || b is BlockTankTreadsWheel || b is BlockMissile || b is BlockAbstractPlatform || b is BlockPiston);
		}

		// Token: 0x060009C7 RID: 2503 RVA: 0x00047728 File Offset: 0x00045B28
		public void DetermineAttachments()
		{
			this.attachedBottomBlock = null;
			this.attachedFrontBlock = null;
			this.attachedLeftBlock = null;
			this.attachedRightBlock = null;
			this.attachedBackBlocks.Clear();
			this.attachedHeadBlocks.Clear();
			this.attachedLeftHandBlocks.Clear();
			this.attachedRightHandBlocks.Clear();
			this.attachmentsPreventLookAnim = false;
			this.isConnectedToUnsupportedActionBlock = false;
			HashSet<Block> hashSet = new HashSet<Block>();
			Matrix4x4 worldToLocalMatrix = this.goT.worldToLocalMatrix;
			for (int i = 0; i < this.connections.Count; i++)
			{
				Block block = this.connections[i];
				if (!block.isTerrain)
				{
					if (this.IsUnsupportedAttachmentType(block))
					{
						this.isConnectedToUnsupportedActionBlock = true;
					}
					else
					{
						Vector3 vector = worldToLocalMatrix.MultiplyPoint(block.goT.position) + block.GetBlockMetaData().attachOffset;
						if (this.attachedBottomBlock == null && vector.y < 1.1f && this.CheckBottomGlueForSlopes(this.glueMeshes[BlockAnimatedCharacter.idxBottom], block.glueMeshes, block.jointMeshes))
						{
							this.attachedBottomBlock = block;
						}
						else if (this.attachedFrontBlock == null && vector.z > -1.1f && (CollisionTest.MultiMeshMeshTest2(this.glueMeshes[BlockAnimatedCharacter.idxFront], block.glueMeshes, false) || CollisionTest.MultiMeshMeshTest2(this.glueMeshes[BlockAnimatedCharacter.idxFront], block.jointMeshes, false)))
						{
							if (!CharacterEditor.IsGear(block))
							{
								this.attachmentsPreventLookAnim = true;
							}
							this.attachedFrontBlock = block;
						}
						else if (vector.z < 1.1f && this.IsBlockGluedToMesh(block, this.glueMeshes[BlockAnimatedCharacter.idxBack]))
						{
							this.attachedBackBlocks.Add(block);
						}
						else if (this.attachedLeftBlock == null && this.IsBlockGluedToMesh(block, this.glueMeshes[BlockAnimatedCharacter.idxLeft]))
						{
							this.attachedLeftBlock = block;
						}
						else if (this.attachedRightBlock == null && this.IsBlockGluedToMesh(block, this.glueMeshes[BlockAnimatedCharacter.idxRight]))
						{
							this.attachedRightBlock = block;
						}
						else
						{
							if (!CharacterEditor.IsGear(block))
							{
								this.attachmentsPreventLookAnim = true;
							}
							this.attachedHeadBlocks.Add(block);
						}
						hashSet.Add(block);
					}
				}
			}
			for (int j = 0; j < this.attachedBackBlocks.Count; j++)
			{
				Block block2 = this.attachedBackBlocks[j];
				for (int k = 0; k < block2.connections.Count; k++)
				{
					Block block3 = block2.connections[k];
					if (!hashSet.Contains(block3))
					{
						if (this.IsUnsupportedAttachmentType(block3))
						{
							this.isConnectedToUnsupportedActionBlock = true;
						}
						else if (!(block3 is BlockAnimatedCharacter))
						{
							this.attachedBackBlocks.Add(block3);
						}
						hashSet.Add(block3);
					}
				}
			}
			for (int l = 0; l < this.attachedHeadBlocks.Count; l++)
			{
				Block block4 = this.attachedHeadBlocks[l];
				for (int m = 0; m < block4.connections.Count; m++)
				{
					Block block5 = block4.connections[m];
					if (!hashSet.Contains(block5))
					{
						if (this.IsUnsupportedAttachmentType(block5))
						{
							this.isConnectedToUnsupportedActionBlock = true;
						}
						else if (!(block5 is BlockAnimatedCharacter))
						{
							this.attachedHeadBlocks.Add(block5);
						}
						hashSet.Add(block5);
					}
				}
			}
			if (this.attachedLeftBlock != null)
			{
				this.attachedLeftHandBlocks.Add(this.attachedLeftBlock);
				for (int n = 0; n < this.attachedLeftHandBlocks.Count; n++)
				{
					Block block6 = this.attachedLeftHandBlocks[n];
					for (int num = 0; num < block6.connections.Count; num++)
					{
						Block block7 = block6.connections[num];
						if (!hashSet.Contains(block7))
						{
							if (this.IsUnsupportedAttachmentType(block7))
							{
								this.isConnectedToUnsupportedActionBlock = true;
							}
							else if (!(block7 is BlockAnimatedCharacter))
							{
								this.attachedLeftHandBlocks.Add(block7);
							}
							hashSet.Add(block7);
						}
					}
				}
				this.attachedLeftHandBlocks.Remove(this.attachedLeftBlock);
			}
			if (this.attachedRightBlock != null)
			{
				this.attachedRightHandBlocks.Add(this.attachedRightBlock);
				for (int num2 = 0; num2 < this.attachedRightHandBlocks.Count; num2++)
				{
					Block block8 = this.attachedRightHandBlocks[num2];
					for (int num3 = 0; num3 < block8.connections.Count; num3++)
					{
						Block block9 = block8.connections[num3];
						if (!hashSet.Contains(block9))
						{
							if (this.IsUnsupportedAttachmentType(block9))
							{
								this.isConnectedToUnsupportedActionBlock = true;
							}
							else if (!(block9 is BlockAnimatedCharacter))
							{
								this.attachedRightHandBlocks.Add(block9);
							}
							hashSet.Add(block9);
						}
					}
				}
				this.attachedRightHandBlocks.Remove(this.attachedRightBlock);
			}
			this.haveDeterminedAttachments = true;
		}

		// Token: 0x060009C8 RID: 2504 RVA: 0x00047C94 File Offset: 0x00046094
		public Vector3 GetRightHandAttachOffset()
		{
			if (this.attachedRightBlock == null)
			{
				return Vector3.zero;
			}
			Vector3 a = this.goT.InverseTransformPoint(this.attachedRightBlock.GetPosition());
			a -= new Vector3(1f, -0.5f, 0f);
			Vector3 a2 = new Vector3(-a.y, a.x, a.z);
			return a2 + BlockAnimatedCharacter.rightHandPlacementOffset + this.attachedRightBlock.GetBlockMetaData().attachOffset;
		}

		// Token: 0x060009C9 RID: 2505 RVA: 0x00047D24 File Offset: 0x00046124
		public Vector3 GetLeftHandAttachOffset()
		{
			if (this.attachedLeftBlock == null)
			{
				return Vector3.zero;
			}
			Vector3 a = this.goT.InverseTransformPoint(this.attachedLeftBlock.GetPosition());
			a -= new Vector3(-1f, -0.5f, 0f);
			Vector3 a2 = new Vector3(a.y, a.x, a.z);
			return a2 + BlockAnimatedCharacter.leftHandPlacementOffset;
		}

		// Token: 0x060009CA RID: 2506 RVA: 0x00047D9B File Offset: 0x0004619B
		public override void BeforePlay()
		{
			this.DetermineAttachments();
			this.goLayerAssignment = Layer.Default;
			this.go.SetLayer(this.goLayerAssignment, true);
		}

		// Token: 0x060009CB RID: 2507 RVA: 0x00047DBC File Offset: 0x000461BC
		public override void ConnectionsChanged()
		{
			base.ConnectionsChanged();
			this.haveDeterminedAttachments = false;
			this.DetermineAttachments();
		}

		// Token: 0x060009CC RID: 2508 RVA: 0x00047DD1 File Offset: 0x000461D1
		private bool IsBlockGluedToMesh(Block b, CollisionMesh characterGlueMesh)
		{
			return CollisionTest.MultiMeshMeshTest2(characterGlueMesh, b.glueMeshes, false) || CollisionTest.MultiMeshMeshTest2(characterGlueMesh, b.jointMeshes, false);
		}

		// Token: 0x060009CD RID: 2509 RVA: 0x00047DF8 File Offset: 0x000461F8
		private bool CheckBottomGlueForSlopes(CollisionMesh bottomGlue, CollisionMesh[] glueBeneathCharacter, CollisionMesh[] jointsBeneathCharacter)
		{
			for (int i = 0; i < glueBeneathCharacter.Length; i++)
			{
				if (CollisionTest.MeshMeshTest(bottomGlue, glueBeneathCharacter[i], false))
				{
					this.gotSlopedGlue = (this.gotSlopedGlue ? this.gotSlopedGlue : this.CheckForAngledMesh(glueBeneathCharacter[i].localRot));
					return true;
				}
			}
			for (int j = 0; j < jointsBeneathCharacter.Length; j++)
			{
				if (CollisionTest.MeshMeshTest(bottomGlue, jointsBeneathCharacter[j], false))
				{
					this.gotSlopedGlue = (this.gotSlopedGlue ? this.gotSlopedGlue : this.CheckForAngledMesh(jointsBeneathCharacter[j].localRot));
					return true;
				}
			}
			return false;
		}

		// Token: 0x060009CE RID: 2510 RVA: 0x00047EA4 File Offset: 0x000462A4
		private bool CheckForAngledMesh(Vector3 ourRot)
		{
			return !Util.HasNoAngle(ourRot.x) || !Util.HasNoAngle(ourRot.y) || !Util.HasNoAngle(ourRot.z);
		}

		// Token: 0x060009CF RID: 2511 RVA: 0x00047EDA File Offset: 0x000462DA
		public override void BecameTreasure()
		{
			base.BecameTreasure();
			this.stateHandler.InterruptQueue(CharacterState.Sitting);
		}

		// Token: 0x060009D0 RID: 2512 RVA: 0x00047EF0 File Offset: 0x000462F0
		private bool NotOnlyBlocksterInChunk()
		{
			if (this.chunk != null)
			{
				bool flag = false;
				for (int i = 0; i < this.chunk.blocks.Count; i++)
				{
					if (this.chunk.blocks[i] is BlockAnimatedCharacter)
					{
						if (flag)
						{
							return true;
						}
						flag = true;
					}
				}
			}
			return false;
		}

		// Token: 0x060009D1 RID: 2513 RVA: 0x00047F54 File Offset: 0x00046354
		public override void Play()
		{
			this.unmoving = (this.attachedBottomBlock != null || this.attachedFrontBlock != null);
			this.unmoving |= (base.IsFixed() || this.gotSlopedGlue || this.NotOnlyBlocksterInChunk());
			this.unmoving |= this.isConnectedToUnsupportedActionBlock;
			base.UpdateConnectedCache();
			List<Block> list = Block.connectedCache[this];
			this.RemoveMeshFromSkeleton();
			this.ParentToSkeleton();
			base.Play();
			this.canUseCharacterShadow = !Blocksworld.renderingShadows;
			foreach (Block block in list)
			{
				if (block is BlockWalkable && block != this)
				{
					this.canUseCharacterShadow = false;
				}
			}
			if (this.canUseCharacterShadow)
			{
				HashSet<Predicate> manyPreds = new HashSet<Predicate>
				{
					BlockAnimatedCharacter.predicateCharacterMover
				};
				this.canUseCharacterShadow = base.ContainsTileWithAnyPredicateInPlayMode2(manyPreds);
			}
			if (this.canUseCharacterShadow)
			{
				if (BlockAnimatedCharacter.prefabCharacterShadow == null)
				{
					BlockAnimatedCharacter.prefabCharacterShadow = (UnityEngine.Object.Instantiate(Resources.Load("Blocks/Character Shadow")) as GameObject);
					BlockAnimatedCharacter.prefabCharacterShadow.SetActive(false);
					Material material = BlockAnimatedCharacter.prefabCharacterShadow.GetComponent<Renderer>().material;
					if (material == null)
					{
						BWLog.Info("Character shadow material was null");
					}
				}
				if (this.goShadow != null)
				{
					UnityEngine.Object.Destroy(this.goShadow);
				}
				base.InstantiateShadow(BlockAnimatedCharacter.prefabCharacterShadow);
				this.goShadow.GetComponent<Renderer>().enabled = true;
			}
			if (this.stateHandler != null)
			{
				this.buoyancyMultiplier = 25f;
				this.stateHandler.Play();
				this.HideSourceBlockster();
			}
			this._playingAttack = false;
		}

		// Token: 0x060009D2 RID: 2514 RVA: 0x0004814C File Offset: 0x0004654C
		public override void Play2()
		{
			base.Play2();
			BoxCollider component = this.go.GetComponent<BoxCollider>();
			PhysicMaterial physicMaterialTexture = MaterialTexture.GetPhysicMaterialTexture("Blockster");
			if (component != null && physicMaterialTexture != null)
			{
				component.material = physicMaterialTexture;
			}
		}

		// Token: 0x060009D3 RID: 2515 RVA: 0x00048195 File Offset: 0x00046595
		public override bool CanMergeShadow()
		{
			return false;
		}

		// Token: 0x060009D4 RID: 2516 RVA: 0x00048198 File Offset: 0x00046598
		public override void Stop(bool resetBlock = true)
		{
			base.Stop(resetBlock);
			this.Activate();
			this.gotSlopedGlue = false;
			if (this.stateHandler != null)
			{
				this.stateHandler.Stop();
				this.stateHandler.SetRole(this.DefaultRoleForCharacterType(this.characterType));
				this.HideSourceBlockster();
				this.RemoveMeshFromSkeleton();
				if (this.headClone != null)
				{
					this.headClone.enabled = true;
					this.HideSourceBlockster();
				}
				this.ParentToSkeleton();
				this.SizeMini();
			}
			this.GetCloneHead().enabled = true;
			if (this.walkController != null)
			{
				this.walkController.CancelPull();
			}
			this.ResetBodyParts();
		}

		// Token: 0x060009D5 RID: 2517 RVA: 0x0004824C File Offset: 0x0004664C
		private void RemoveMeshFromSkeleton()
		{
			if (this.characterPieces != null)
			{
				for (int i = 0; i < this.characterPieces.Count; i++)
				{
					this.characterPieces[i].parent = this.goT;
					if (this.characterPieces[i].name == "Character Short Hair" || this.characterPieces[i].name == "Character Head")
					{
						this.characterPieces[i].localPosition = Vector3.zero;
						this.characterPieces[i].localEulerAngles = Vector3.zero;
					}
					Collider component = this.characterPieces[i].GetComponent<Collider>();
					if (component != null)
					{
						component.enabled = false;
					}
				}
			}
		}

		// Token: 0x060009D6 RID: 2518 RVA: 0x00048328 File Offset: 0x00046728
		public override void IgnoreRaycasts(bool value)
		{
			base.IgnoreRaycasts(value);
			int layer = (int)((!value) ? this.goLayerAssignment : Layer.IgnoreRaycast);
			this.middle.layer = layer;
			if (this.hands[0] != null)
			{
				this.hands[0].layer = layer;
			}
			if (this.hands[1] != null)
			{
				this.hands[1].layer = layer;
			}
		}

		// Token: 0x060009D7 RID: 2519 RVA: 0x000483A0 File Offset: 0x000467A0
		public override void SetFPCGearVisible(bool visible)
		{
			if (this.headClone != null)
			{
				this.headClone.enabled = visible;
			}
			if (this.hair != null)
			{
				this.hair.gameObject.GetComponent<MeshRenderer>().enabled = visible;
			}
			base.SetFPCGearVisible(visible);
		}

		// Token: 0x060009D8 RID: 2520 RVA: 0x000483F8 File Offset: 0x000467F8
		public override TileResultCode PaintTo(string paint, bool permanent, int meshIndex = 0)
		{
			if (this.lockPaintAndTexture)
			{
				return TileResultCode.False;
			}
			if (this.bodyParts == null)
			{
				return TileResultCode.False;
			}
			if (permanent && meshIndex == 1 && base.GetTexture(1) == "Plain" && Block.skinPaints.Contains(paint) && base.GetDefaultTexture(1) != "Plain")
			{
				this.TextureTo("Clothing Underwear", Vector3.forward, permanent, 1, false);
			}
			TileResultCode tileResultCode = base.PaintTo(paint, permanent, meshIndex);
			if (TileResultCode.True != tileResultCode)
			{
				return tileResultCode;
			}
			if (meshIndex == 0 && this.headClone != null)
			{
				this.CopySourceHeadToHeadClone();
				this.HideSourceBlockster();
			}
			if (meshIndex == 0)
			{
				this.bodyParts.PaintSkinColor(paint, permanent);
			}
			else if (meshIndex == 1)
			{
				this.bodyParts.PaintShirtColor(paint, permanent);
			}
			else if (meshIndex <= this.subMeshGameObjects.Count)
			{
				GameObject subMeshGameObject = this.GetSubMeshGameObject(meshIndex);
				BodyPartInfo component = subMeshGameObject.GetComponent<BodyPartInfo>();
				if (component != null)
				{
					component.currentPaint = paint;
					if (component.colorGroup != BodyPartInfo.ColorGroup.None)
					{
						this.bodyParts.PaintColorGroup(component.colorGroup, paint, permanent);
					}
				}
			}
			return tileResultCode;
		}

		// Token: 0x060009D9 RID: 2521 RVA: 0x0004853C File Offset: 0x0004693C
		private void CopySourceHeadToHeadClone()
		{
			Renderer component = this.go.GetComponent<MeshRenderer>();
			MeshFilter component2 = this.go.GetComponent<MeshFilter>();
			Mesh mesh = null;
			if (component2 != null)
			{
				mesh = component2.sharedMesh;
			}
			Renderer cloneHead = this.GetCloneHead();
			Mesh mesh2 = (!(cloneHead.GetComponent<MeshFilter>() != null)) ? null : cloneHead.GetComponent<MeshFilter>().mesh;
			if (mesh != null && mesh2 != null)
			{
				if (mesh.vertexCount == mesh2.vertexCount)
				{
					mesh2.uv = mesh.uv;
				}
				else
				{
					BWLog.Info(string.Concat(new object[]
					{
						"Mesh vert count mismatch, source verts: ",
						mesh.vertexCount,
						" targetVerts ",
						mesh2.vertexCount
					}));
				}
			}
			if (component != null && cloneHead != null)
			{
				cloneHead.sharedMaterial = component.sharedMaterial;
				return;
			}
		}

		// Token: 0x060009DA RID: 2522 RVA: 0x00048640 File Offset: 0x00046A40
		public override TileResultCode IsTexturedTo(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int meshIndex = 1;
			string text = (string)args[0];
			if (base.IsCharacterFaceTexture(text))
			{
				meshIndex = 0;
			}
			return (!(base.GetTexture(meshIndex) == text)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x060009DB RID: 2523 RVA: 0x00048680 File Offset: 0x00046A80
		public override TileResultCode TextureToAction(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string text = (string)args[0];
			Vector3 newNormal = (Vector3)args[1];
			int meshIndex = 1;
			if (args.Length > 2)
			{
				meshIndex = (int)args[2];
			}
			else if (base.IsCharacterFaceTexture(text))
			{
				meshIndex = 0;
				if (!base.BlockType().EndsWith("Skeleton"))
				{
					int meshIndex2 = 6;
					if (base.IsCharacterFaceWrapAroundTexture(text))
					{
						base.TextureToAction(text, newNormal, meshIndex2);
					}
					else
					{
						base.TextureToAction("Plain", newNormal, meshIndex2);
					}
				}
			}
			else if (Materials.IsMaterialShaderTexture(text))
			{
				meshIndex = 0;
				for (int i = 1; i <= this.subMeshGameObjects.Count; i++)
				{
					base.TextureToAction(text, newNormal, i);
				}
			}
			return base.TextureToAction(text, newNormal, meshIndex);
		}

		// Token: 0x060009DC RID: 2524 RVA: 0x0004874C File Offset: 0x00046B4C
		public override TileResultCode TextureTo(string texture, Vector3 normal, bool permanent, int meshIndex = 0, bool force = false)
		{
			if (this.lockPaintAndTexture)
			{
				return TileResultCode.False;
			}
			if (permanent && meshIndex == 1 && texture == "Plain" && Block.skinPaints.Contains(this.GetPaint(1)) && base.GetDefaultTexture(1) != "Plain")
			{
				texture = "Clothing Underwear";
			}
			if (meshIndex > 0 && meshIndex <= this.subMeshGameObjects.Count)
			{
				GameObject subMeshGameObject = this.GetSubMeshGameObject(meshIndex);
				BodyPartInfo component = subMeshGameObject.GetComponent<BodyPartInfo>();
				if (component != null)
				{
					bool flag = Materials.IsMaterialShaderTexture(texture);
					if (flag)
					{
						if (!component.canBeMaterialTextured)
						{
							return TileResultCode.False;
						}
					}
					else if (component.canBeTextured)
					{
						return TileResultCode.False;
					}
				}
			}
			TileResultCode tileResultCode = base.TextureTo(texture, normal, permanent, meshIndex, force);
			if (tileResultCode != TileResultCode.True)
			{
				Debug.Log(string.Concat(new object[]
				{
					"Texture Failed ",
					texture,
					" to mesh index ",
					meshIndex,
					" result ",
					tileResultCode
				}));
				return tileResultCode;
			}
			if (meshIndex == 0)
			{
				if (null != this.headClone)
				{
					this.CopySourceHeadToHeadClone();
					this.HideSourceBlockster();
				}
			}
			else if (meshIndex <= this.subMeshGameObjects.Count)
			{
				GameObject subMeshGameObject2 = this.GetSubMeshGameObject(meshIndex);
				BodyPartInfo component2 = subMeshGameObject2.GetComponent<BodyPartInfo>();
				if (component2 != null)
				{
					component2.currentTexture = texture;
					if (component2.colorGroup != BodyPartInfo.ColorGroup.None)
					{
						this.bodyParts.TextureColorGroup(component2.colorGroup, texture, normal, permanent);
					}
				}
			}
			return tileResultCode;
		}

		// Token: 0x060009DD RID: 2525 RVA: 0x000488F4 File Offset: 0x00046CF4
		protected override void UpdateBlockPropertiesForTextureAssignment(int meshIndex, bool forceEnabled)
		{
			Renderer component = this.go.GetComponent<Renderer>();
			bool enabled = component.enabled;
			component.enabled = true;
			base.UpdateBlockPropertiesForTextureAssignment(meshIndex, forceEnabled);
			this.GetCloneHead().enabled = component.enabled;
			component.enabled = enabled;
		}

		// Token: 0x060009DE RID: 2526 RVA: 0x0004893B File Offset: 0x00046D3B
		public TileResultCode Stand(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.stateHandler.InterruptState(CharacterState.StandUp, true);
			return TileResultCode.True;
		}

		// Token: 0x060009DF RID: 2527 RVA: 0x00048950 File Offset: 0x00046D50
		public TileResultCode Sit(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.stateHandler.currentState == CharacterState.SitDown)
			{
				if (this.stateHandler.playAnimFinished)
				{
					return TileResultCode.True;
				}
				return TileResultCode.Delayed;
			}
			else
			{
				this.stateHandler.InterruptState(CharacterState.SitDown, true);
				if (this.stateHandler.currentState == CharacterState.SitDown)
				{
					return TileResultCode.Delayed;
				}
				return TileResultCode.True;
			}
		}

		// Token: 0x060009E0 RID: 2528 RVA: 0x000489A8 File Offset: 0x00046DA8
		public TileResultCode IsCollapsed(ScriptRowExecutionInfo eInfo, object[] args)
		{
			bool flag = this.stateHandler.currentState == CharacterState.Collapsed;
			return (!flag) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x060009E1 RID: 2529 RVA: 0x000489D4 File Offset: 0x00046DD4
		public TileResultCode IsNotCollapsed(ScriptRowExecutionInfo eInfo, object[] args)
		{
			bool flag = this.stateHandler.currentState == CharacterState.Collapsed;
			flag |= (this.stateHandler.currentState == CharacterState.Collapse);
			flag |= (this.stateHandler.currentState == CharacterState.Recover);
			return (!flag) ? TileResultCode.True : TileResultCode.False;
		}

		// Token: 0x060009E2 RID: 2530 RVA: 0x00048A24 File Offset: 0x00046E24
		public TileResultCode Collapse(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.stateHandler.currentState == CharacterState.Collapse)
			{
				if (this.stateHandler.playAnimFinished)
				{
					return TileResultCode.True;
				}
				return TileResultCode.Delayed;
			}
			else
			{
				if (this.stateHandler.currentState == CharacterState.Collapsed)
				{
					return TileResultCode.True;
				}
				this.stateHandler.InterruptState(CharacterState.Collapse, true);
				if (this.stateHandler.currentState == CharacterState.Collapse)
				{
					return TileResultCode.Delayed;
				}
				return TileResultCode.True;
			}
		}

		// Token: 0x060009E3 RID: 2531 RVA: 0x00048A90 File Offset: 0x00046E90
		public TileResultCode Recover(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.stateHandler.currentState == CharacterState.Recover)
			{
				if (this.stateHandler.playAnimFinished)
				{
					return TileResultCode.True;
				}
				return TileResultCode.Delayed;
			}
			else
			{
				this.stateHandler.InterruptState(CharacterState.Recover, true);
				if (this.stateHandler.currentState == CharacterState.Recover)
				{
					return TileResultCode.Delayed;
				}
				return TileResultCode.True;
			}
		}

		// Token: 0x060009E4 RID: 2532 RVA: 0x00048AE8 File Offset: 0x00046EE8
		public void SetLayer(Layer layerEnum)
		{
			foreach (GameObject gameObject in this.GetAllBodyPartObjects())
			{
				gameObject.layer = (int)layerEnum;
			}
			this.middle.layer = (int)layerEnum;
			this.go.layer = (int)layerEnum;
			this.goLayerAssignment = layerEnum;
		}

		// Token: 0x060009E5 RID: 2533 RVA: 0x00048B68 File Offset: 0x00046F68
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (this.broken || this.unmoving)
			{
				return;
			}
			this.walkController.torqueMultiplier = 1f;
			this.chunk.SetAngularDragMultiplier(1f);
		}

		// Token: 0x060009E6 RID: 2534 RVA: 0x00048BA8 File Offset: 0x00046FA8
		private void SetHandsAndBodyVisibleState(bool v)
		{
			Shader invisibleShader = this.InvisibleShader;
			for (int i = 0; i < this.hands.Length; i++)
			{
				GameObject gameObject = this.hands[i];
				if (gameObject.GetComponent<Renderer>().sharedMaterial.shader != invisibleShader)
				{
					gameObject.GetComponent<Renderer>().enabled = v;
					gameObject.SetActive(v);
				}
				gameObject.GetComponent<Collider>().enabled = v;
			}
			if (this.middle.GetComponent<Renderer>().sharedMaterial.shader != invisibleShader)
			{
				this.middle.GetComponent<Renderer>().enabled = v;
				this.middle.SetActive(v);
			}
			this.middle.GetComponent<Collider>().enabled = v;
		}

		// Token: 0x060009E7 RID: 2535 RVA: 0x00048C66 File Offset: 0x00047066
		public override void Vanished()
		{
			this.stateHandler.SaveAnimatorState();
			this.Deactivate();
			if (this.broken)
			{
				this.SetHandsAndBodyVisibleState(false);
			}
		}

		// Token: 0x060009E8 RID: 2536 RVA: 0x00048C8B File Offset: 0x0004708B
		public override void Appeared()
		{
			this.Activate();
			base.Appeared();
			if (this.broken)
			{
				this.SetHandsAndBodyVisibleState(true);
			}
			this.stateHandler.RestoreAnimatorState();
		}

		// Token: 0x060009E9 RID: 2537 RVA: 0x00048CB8 File Offset: 0x000470B8
		public override List<GameObject> GetIgnoreRaycastGOs()
		{
			List<GameObject> ignoreRaycastGOs = base.GetIgnoreRaycastGOs();
			ignoreRaycastGOs.Add(this.middle);
			return ignoreRaycastGOs;
		}

		// Token: 0x060009EA RID: 2538 RVA: 0x00048CDC File Offset: 0x000470DC
		protected override void UpdateShadow()
		{
			if (!this.canUseCharacterShadow)
			{
				base.UpdateShadow();
				return;
			}
			Collider component = this.goT.GetComponent<Collider>();
			Vector3 origin = component.bounds.center + Vector3.down * component.bounds.extents.y * 0.9f;
			RaycastHit raycastHit = default(RaycastHit);
			float num = 50f;
			for (int i = 0; i < this.feet.Length; i++)
			{
				if (this.feet[i].go != null)
				{
					this.feet[i].go.layer = 2;
				}
			}
			if (Physics.Raycast(origin, Vector3.down, out raycastHit, num, 4113))
			{
				float num2 = Mathf.Clamp(0.25f - raycastHit.distance * (0.25f / num), 0f, 1f);
				num2 *= Mathf.Clamp(raycastHit.normal.y, 0f, 1f);
				base.SetShadowAlpha(num2);
				Transform transform = this.goShadow.transform;
				Vector3 point = raycastHit.point;
				transform.position = point + Vector3.up * 0.02f;
				float num3 = 0.6f;
				Vector3 normalized = (num3 * this.oldShadowHitNormal + (1f - num3) * raycastHit.normal).normalized;
				this.oldShadowHitNormal = normalized;
				transform.rotation = Quaternion.FromToRotation(Vector3.up, normalized);
				Vector3 vector = Vector3.Cross(raycastHit.normal, -this.goT.right);
				if (this.broken || !(this.footLt != null) || !(this.footRt != null) || this.walkController == null || Blocksworld.CurrentState != State.Play || BlockAnimatedCharacter.stateControllers[this].GetState() == CharacterState.Idle)
				{
				}
			}
			else
			{
				base.SetShadowAlpha(0f);
			}
		}

		// Token: 0x060009EB RID: 2539 RVA: 0x00048F20 File Offset: 0x00047320
		public override TileResultCode Explode(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.broken)
			{
				return TileResultCode.True;
			}
			this.SetLayer(Layer.ChunkedBlock);
			for (int i = 0; i < this.connections.Count; i++)
			{
				Block block = this.connections[i];
				int num = this.connectionTypes[i];
				if (num == 1)
				{
					block.go.SetLayer(Layer.ChunkedBlock, false);
				}
			}
			this.RemoveMeshFromSkeleton();
			return base.Explode(eInfo, args);
		}

		// Token: 0x060009EC RID: 2540 RVA: 0x00048F9C File Offset: 0x0004739C
		public override TileResultCode SmashLocal(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.broken)
			{
				return TileResultCode.True;
			}
			TileResultCode result = base.SmashLocal(eInfo, args);
			this.Break(Vector3.zero, Vector3.zero, Vector3.zero);
			return result;
		}

		// Token: 0x060009ED RID: 2541 RVA: 0x00048FD8 File Offset: 0x000473D8
		public override List<Collider> GetColliders()
		{
			List<Collider> colliders = base.GetColliders();
			foreach (GameObject gameObject in this.GetAllBodyPartObjects())
			{
				Collider component = gameObject.GetComponent<Collider>();
				if (component != null)
				{
					colliders.Add(component);
				}
			}
			Collider component2 = this.middle.GetComponent<Collider>();
			if (component2 != null)
			{
				colliders.Add(component2);
			}
			return colliders;
		}

		// Token: 0x060009EE RID: 2542 RVA: 0x00049074 File Offset: 0x00047474
		public override void Activate()
		{
			if (this.goShadow != null)
			{
				this.goShadow.SetActive(true);
			}
			if (this.go == null)
			{
				return;
			}
			Collider component = this.go.GetComponent<Collider>();
			if (component != null)
			{
				component.enabled = true;
			}
			IEnumerator enumerator = this.stateHandler.targetRig.transform.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					Transform transform = (Transform)obj;
					transform.gameObject.SetActive(true);
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
		}

		// Token: 0x060009EF RID: 2543 RVA: 0x00049138 File Offset: 0x00047538
		public override void Deactivate()
		{
			if (this.goShadow != null)
			{
				this.goShadow.SetActive(false);
			}
			Collider component = this.go.GetComponent<Collider>();
			if (component != null)
			{
				component.enabled = false;
			}
			GameObject targetRig = this.stateHandler.targetRig;
			if (targetRig == null)
			{
				return;
			}
			IEnumerator enumerator = targetRig.transform.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					Transform transform = (Transform)obj;
					transform.gameObject.SetActive(false);
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
		}

		// Token: 0x060009F0 RID: 2544 RVA: 0x000491FC File Offset: 0x000475FC
		public override TileResultCode Freeze(bool informModelBlocks)
		{
			this.stateHandler.Freeze();
			return base.Freeze(informModelBlocks);
		}

		// Token: 0x060009F1 RID: 2545 RVA: 0x00049210 File Offset: 0x00047610
		public override void Unfreeze()
		{
			base.Unfreeze();
			this.stateHandler.Unfreeze();
		}

		// Token: 0x060009F2 RID: 2546 RVA: 0x00049223 File Offset: 0x00047623
		public override void ChunkInModelFrozen()
		{
			base.ChunkInModelFrozen();
			this.stateHandler.Freeze();
		}

		// Token: 0x060009F3 RID: 2547 RVA: 0x00049236 File Offset: 0x00047636
		public override void ChunkInModelUnfrozen()
		{
			base.ChunkInModelUnfrozen();
			this.stateHandler.Unfreeze();
		}

		// Token: 0x060009F4 RID: 2548 RVA: 0x0004924C File Offset: 0x0004764C
		public TileResultCode StandingAttack(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.canceledAnimOnRow == eInfo.rowIndex)
			{
				this.canceledAnimOnRow = -1;
				return TileResultCode.False;
			}
			string stringArgSafe = Util.GetStringArgSafe(args, 0, string.Empty);
			CharacterState characterState = (CharacterState)Enum.Parse(typeof(CharacterState), stringArgSafe, true);
			if (this.stateHandler.InAttack())
			{
				bool flag = this.stateHandler.CanStartNewStandingAttack();
				if (!flag)
				{
					return TileResultCode.Delayed;
				}
				if (this._playingAttack)
				{
					this._playingAttack = false;
					if (this.currentAnimRow != eInfo.rowIndex)
					{
						this.canceledAnimOnRow = this.currentAnimRow;
					}
					return TileResultCode.True;
				}
				this.stateHandler.StandingAttack(characterState);
				this.currentAnimRow = eInfo.rowIndex;
				this._playingAttack = true;
				return TileResultCode.Delayed;
			}
			else
			{
				if (this._playingAttack)
				{
					this.currentAnimRow = -1;
					this._playingAttack = false;
					return TileResultCode.True;
				}
				this.stateHandler.StandingAttack(characterState);
				if (this.stateHandler.GetState() == characterState)
				{
					this.currentAnimRow = eInfo.rowIndex;
					this._playingAttack = true;
					return TileResultCode.Delayed;
				}
				return TileResultCode.True;
			}
		}

		// Token: 0x060009F5 RID: 2549 RVA: 0x0004935C File Offset: 0x0004775C
		public TileResultCode Attack(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.canceledAnimOnRow == eInfo.rowIndex)
			{
				this.canceledAnimOnRow = -1;
				return TileResultCode.False;
			}
			string stringArgSafe = Util.GetStringArgSafe(args, 0, string.Empty);
			UpperBodyState upperBodyState = (UpperBodyState)Enum.Parse(typeof(UpperBodyState), stringArgSafe, true);
			if (this.stateHandler.InAttack())
			{
				bool flag = this.stateHandler.CanStartNewAttack();
				if (!flag)
				{
					return TileResultCode.Delayed;
				}
				if (this._playingAttack)
				{
					this._playingAttack = false;
					if (this.currentAnimRow != eInfo.rowIndex)
					{
						this.canceledAnimOnRow = this.currentAnimRow;
					}
					return TileResultCode.True;
				}
				this.stateHandler.Attack(upperBodyState);
				this._playingAttack = true;
				this.currentAnimRow = eInfo.rowIndex;
				return TileResultCode.Delayed;
			}
			else
			{
				if (this._playingAttack)
				{
					this.currentAnimRow = -1;
					this._playingAttack = false;
					return TileResultCode.True;
				}
				this.stateHandler.Attack(upperBodyState);
				if (this.stateHandler.upperBody.GetState() == upperBodyState)
				{
					this.currentAnimRow = eInfo.rowIndex;
					this._playingAttack = true;
					return TileResultCode.Delayed;
				}
				return TileResultCode.True;
			}
		}

		// Token: 0x060009F6 RID: 2550 RVA: 0x00049470 File Offset: 0x00047870
		public TileResultCode Shield(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.stateHandler == null)
			{
				return TileResultCode.False;
			}
			string stringArgSafe = Util.GetStringArgSafe(args, 0, "ShieldBlockLeft");
			UpperBodyState shieldState = (UpperBodyState)Enum.Parse(typeof(UpperBodyState), stringArgSafe, true);
			this.stateHandler.Shield(shieldState);
			return TileResultCode.True;
		}

		// Token: 0x060009F7 RID: 2551 RVA: 0x000494BC File Offset: 0x000478BC
		public TileResultCode Dodge(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string stringArgSafe = Util.GetStringArgSafe(args, 0, "Left");
			CharacterState characterState = (!(stringArgSafe == "Right")) ? CharacterState.DodgeLeft : CharacterState.DodgeRight;
			float dodgeSpeed = this.stateHandler.dodgeSpeed;
			Vector3 dir = (characterState != CharacterState.DodgeLeft) ? Vector3.left : Vector3.right;
			if (this.stateHandler.currentState == characterState)
			{
				this.walkController.Translate(dir, dodgeSpeed);
				return TileResultCode.Delayed;
			}
			bool flag = this.stateHandler.currentState == CharacterState.DodgeLeft || this.stateHandler.currentState == CharacterState.DodgeRight;
			if (flag)
			{
				return TileResultCode.False;
			}
			if (this._playingDodge && this.currentAnimRow == eInfo.rowIndex)
			{
				this._playingDodge = false;
				return TileResultCode.True;
			}
			this.stateHandler.InterruptState(characterState, true);
			if (this.stateHandler.currentState == characterState)
			{
				this.currentAnimRow = eInfo.rowIndex;
				this._playingDodge = true;
				return TileResultCode.Delayed;
			}
			return TileResultCode.True;
		}

		// Token: 0x060009F8 RID: 2552 RVA: 0x000495C0 File Offset: 0x000479C0
		public static bool HitByHandAttachment(Block block)
		{
			if (block is BlockAnimatedCharacter)
			{
				return BlockAnimatedCharacter.HitModelByHandAttachment(block);
			}
			bool flag = false;
			BlockAnimatedCharacter blockAnimatedCharacter = BlockAnimatedCharacter.FindBlockOwner(block);
			foreach (CharacterStateHandler characterStateHandler in BlockAnimatedCharacter.stateControllers.Values)
			{
				BlockAnimatedCharacter targetObject = characterStateHandler.targetObject;
				if (targetObject != blockAnimatedCharacter)
				{
					flag |= characterStateHandler.HitByHandAttachment(block);
				}
			}
			return flag;
		}

		// Token: 0x060009F9 RID: 2553 RVA: 0x00049654 File Offset: 0x00047A54
		public static bool HitByFoot(Block block)
		{
			if (block is BlockAnimatedCharacter)
			{
				return BlockAnimatedCharacter.HitModelByFoot(block);
			}
			bool flag = false;
			BlockAnimatedCharacter blockAnimatedCharacter = BlockAnimatedCharacter.FindBlockOwner(block);
			foreach (CharacterStateHandler characterStateHandler in BlockAnimatedCharacter.stateControllers.Values)
			{
				BlockAnimatedCharacter targetObject = characterStateHandler.targetObject;
				if (targetObject != blockAnimatedCharacter)
				{
					flag |= characterStateHandler.HitByFoot(block);
				}
			}
			return flag;
		}

		// Token: 0x060009FA RID: 2554 RVA: 0x000496E8 File Offset: 0x00047AE8
		public static bool HitModelByFoot(Block block)
		{
			bool flag = false;
			BlockAnimatedCharacter blockAnimatedCharacter = BlockAnimatedCharacter.FindBlockOwner(block);
			foreach (CharacterStateHandler characterStateHandler in BlockAnimatedCharacter.stateControllers.Values)
			{
				BlockAnimatedCharacter targetObject = characterStateHandler.targetObject;
				if (targetObject != blockAnimatedCharacter)
				{
					flag |= characterStateHandler.HitByFoot(block);
					flag |= characterStateHandler.HitModelByFoot(block.modelBlock);
				}
			}
			return flag;
		}

		// Token: 0x060009FB RID: 2555 RVA: 0x0004977C File Offset: 0x00047B7C
		public static bool HitByTaggedHandAttachment(Block block, string tag)
		{
			bool flag = false;
			BlockAnimatedCharacter blockAnimatedCharacter = BlockAnimatedCharacter.FindBlockOwner(block);
			foreach (CharacterStateHandler characterStateHandler in BlockAnimatedCharacter.stateControllers.Values)
			{
				BlockAnimatedCharacter targetObject = characterStateHandler.targetObject;
				if (targetObject != blockAnimatedCharacter)
				{
					flag |= characterStateHandler.HitByTaggedHandAttachment(block, tag);
				}
			}
			return flag;
		}

		// Token: 0x060009FC RID: 2556 RVA: 0x00049800 File Offset: 0x00047C00
		public static bool HitModelByHandAttachment(Block block)
		{
			bool flag = false;
			BlockAnimatedCharacter blockAnimatedCharacter = BlockAnimatedCharacter.FindBlockOwner(block);
			foreach (CharacterStateHandler characterStateHandler in BlockAnimatedCharacter.stateControllers.Values)
			{
				BlockAnimatedCharacter targetObject = characterStateHandler.targetObject;
				if (targetObject != blockAnimatedCharacter)
				{
					flag |= characterStateHandler.HitByHandAttachment(block);
					flag |= characterStateHandler.HitModelByHandAttachment(block.modelBlock);
				}
			}
			return flag;
		}

		// Token: 0x060009FD RID: 2557 RVA: 0x00049894 File Offset: 0x00047C94
		public static bool HitModelByTaggedHandAttachment(Block block, string tag)
		{
			bool flag = false;
			BlockAnimatedCharacter blockAnimatedCharacter = BlockAnimatedCharacter.FindBlockOwner(block);
			foreach (CharacterStateHandler characterStateHandler in BlockAnimatedCharacter.stateControllers.Values)
			{
				BlockAnimatedCharacter targetObject = characterStateHandler.targetObject;
				if (targetObject != blockAnimatedCharacter)
				{
					flag |= characterStateHandler.HitModelByTaggedHandAttachment(block.modelBlock, tag);
				}
			}
			return flag;
		}

		// Token: 0x060009FE RID: 2558 RVA: 0x0004991C File Offset: 0x00047D1C
		public static BlockAnimatedCharacter FindAttackingPropHolder(Block block)
		{
			foreach (KeyValuePair<BlockAnimatedCharacter, CharacterStateHandler> keyValuePair in BlockAnimatedCharacter.stateControllers)
			{
				BlockAnimatedCharacter key = keyValuePair.Key;
				CharacterStateHandler value = keyValuePair.Value;
				Block rightHandAttachment = value.combatController.GetRightHandAttachment();
				if (value.combatController.rightHandAttachmentIsAttacking && rightHandAttachment == block)
				{
					return key;
				}
				Block leftHandAttachment = value.combatController.GetLeftHandAttachment();
				if (value.combatController.leftHandAttachmentIsAttacking && leftHandAttachment == block)
				{
					return key;
				}
			}
			return null;
		}

		// Token: 0x060009FF RID: 2559 RVA: 0x000499E0 File Offset: 0x00047DE0
		public static BlockAnimatedCharacter FindBlockingPropHolder(Block block)
		{
			foreach (KeyValuePair<BlockAnimatedCharacter, CharacterStateHandler> keyValuePair in BlockAnimatedCharacter.stateControllers)
			{
				BlockAnimatedCharacter key = keyValuePair.Key;
				CharacterStateHandler value = keyValuePair.Value;
				Block rightHandAttachment = value.combatController.GetRightHandAttachment();
				if (value.combatController.rightHandAttachmentIsShielding && rightHandAttachment == block)
				{
					return key;
				}
				Block leftHandAttachment = value.combatController.GetLeftHandAttachment();
				if (value.combatController.leftHandAttachmentIsShielding && leftHandAttachment == block)
				{
					return key;
				}
			}
			return null;
		}

		// Token: 0x06000A00 RID: 2560 RVA: 0x00049AA4 File Offset: 0x00047EA4
		public static BlockAnimatedCharacter FindBlockOwner(Block block)
		{
			if (block is BlockAnimatedCharacter)
			{
				return block as BlockAnimatedCharacter;
			}
			return BlockAnimatedCharacter.FindPropHolder(block);
		}

		// Token: 0x06000A01 RID: 2561 RVA: 0x00049AC0 File Offset: 0x00047EC0
		public static BlockAnimatedCharacter FindPropHolder(Block block)
		{
			foreach (KeyValuePair<BlockAnimatedCharacter, CharacterStateHandler> keyValuePair in BlockAnimatedCharacter.stateControllers)
			{
				BlockAnimatedCharacter key = keyValuePair.Key;
				if (key.IsAttachment(block))
				{
					return key;
				}
			}
			return null;
		}

		// Token: 0x06000A02 RID: 2562 RVA: 0x00049B34 File Offset: 0x00047F34
		public void ShieldHitReact(Block shieldBlock)
		{
			Block rightHandAttachment = this.stateHandler.combatController.GetRightHandAttachment();
			Block leftHandAttachment = this.stateHandler.combatController.GetLeftHandAttachment();
			if (shieldBlock == rightHandAttachment)
			{
				this.stateHandler.combatController.HitRightShield();
			}
			else if (shieldBlock == leftHandAttachment)
			{
				this.stateHandler.combatController.HitLeftShield();
			}
		}

		// Token: 0x06000A03 RID: 2563 RVA: 0x00049B96 File Offset: 0x00047F96
		public override bool CanRepelAttack(Vector3 attackPosition, Vector3 attackDirection)
		{
			return base.CanRepelAttack(attackPosition, attackDirection) || this.IsShieldingFromAttack(attackPosition, attackDirection);
		}

		// Token: 0x06000A04 RID: 2564 RVA: 0x00049BB0 File Offset: 0x00047FB0
		public override void OnAttacked(Vector3 attackPosition, Vector3 attackDirection)
		{
			this.stateHandler.QueueHitReact(attackDirection);
		}

		// Token: 0x06000A05 RID: 2565 RVA: 0x00049BC0 File Offset: 0x00047FC0
		public bool IsShieldingFromAttack(Vector3 attackPosition, Vector3 attackDirection)
		{
			Block leftHandAttachment = this.stateHandler.combatController.GetLeftHandAttachment();
			Block rightHandAttachment = this.stateHandler.combatController.GetRightHandAttachment();
			bool flag = leftHandAttachment != null && Invincibility.IsInvincible(leftHandAttachment);
			bool flag2 = rightHandAttachment != null && Invincibility.IsInvincible(rightHandAttachment);
			bool flag3 = false;
			if (flag)
			{
				Vector3 forward = leftHandAttachment.goT.forward;
				bool flag4 = Vector3.Dot(attackDirection, forward) < -0.12f;
				bool flag5 = (leftHandAttachment.GetPosition() - base.GetPosition()).sqrMagnitude < (attackPosition - base.GetPosition()).sqrMagnitude;
				flag3 |= (flag4 && flag5);
			}
			if (flag2)
			{
				Vector3 forward2 = rightHandAttachment.goT.forward;
				bool flag6 = Vector3.Dot(attackDirection, forward2) < -0.12f;
				bool flag7 = (rightHandAttachment.GetPosition() - base.GetPosition()).sqrMagnitude < (attackPosition - base.GetPosition()).sqrMagnitude;
				flag3 |= (flag6 && flag7);
			}
			return flag3;
		}

		// Token: 0x06000A06 RID: 2566 RVA: 0x00049CE4 File Offset: 0x000480E4
		public static bool FiredAsWeapon(Block block)
		{
			bool flag = false;
			foreach (CharacterStateHandler characterStateHandler in BlockAnimatedCharacter.stateControllers.Values)
			{
				flag |= characterStateHandler.FiredAsWeapon(block);
			}
			return flag;
		}

		// Token: 0x06000A07 RID: 2567 RVA: 0x00049D4C File Offset: 0x0004814C
		public static void ClearAttackFlags()
		{
			foreach (CharacterStateHandler characterStateHandler in BlockAnimatedCharacter.stateControllers.Values)
			{
				characterStateHandler.ClearAttackFlags();
			}
		}

		// Token: 0x06000A08 RID: 2568 RVA: 0x00049DAC File Offset: 0x000481AC
		public TileResultCode QueueState(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.stateHandler == null)
			{
				return TileResultCode.True;
			}
			this.stateHandler.QueueState((CharacterState)Enum.Parse(typeof(CharacterState), Util.GetStringArg(args, 0, "Idle")));
			return TileResultCode.True;
		}

		// Token: 0x06000A09 RID: 2569 RVA: 0x00049DE8 File Offset: 0x000481E8
		public TileResultCode InterruptState(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.stateHandler == null)
			{
				return TileResultCode.True;
			}
			this.stateHandler.InterruptState((CharacterState)Enum.Parse(typeof(CharacterState), Util.GetStringArg(args, 0, "Idle")), true);
			return TileResultCode.True;
		}

		// Token: 0x06000A0A RID: 2570 RVA: 0x00049E28 File Offset: 0x00048228
		public TileResultCode PlayAnim(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.stateHandler == null)
			{
				return TileResultCode.True;
			}
			this.stateHandler.requestingPlayAnim = true;
			string stringArg = Util.GetStringArg(args, 0, string.Empty);
			if (string.IsNullOrEmpty(stringArg))
			{
				return TileResultCode.True;
			}
			if (this.canceledAnimOnRow == eInfo.rowIndex)
			{
				this.canceledAnimOnRow = -1;
				return TileResultCode.False;
			}
			if (this.stateHandler.targetController.IsInTransition(0))
			{
				return TileResultCode.Delayed;
			}
			if (this.stateHandler.currentState == CharacterState.PlayAnim)
			{
				if (!this.stateHandler.playAnimFinished)
				{
					if (eInfo.rowIndex != this.currentAnimRow)
					{
						this.stateHandler.PlayAnim(stringArg);
						this.canceledAnimOnRow = this.currentAnimRow;
						this.currentAnimRow = eInfo.rowIndex;
						return TileResultCode.Delayed;
					}
					return TileResultCode.Delayed;
				}
				else
				{
					if (this.stateHandler.playAnimCurrent != null)
					{
						this.stateHandler.ClearAnimation();
						return TileResultCode.True;
					}
					this.stateHandler.PlayAnim(stringArg);
					this.currentAnimRow = eInfo.rowIndex;
					return TileResultCode.Delayed;
				}
			}
			else
			{
				if (this.stateHandler.currentState != CharacterState.PlayAnim && this.stateHandler.currentState != CharacterState.Idle && this.stateHandler.playAnimCurrent != null)
				{
					this.stateHandler.ClearAnimation();
					return TileResultCode.False;
				}
				this.stateHandler.PlayAnim(stringArg);
				this.currentAnimRow = eInfo.rowIndex;
				return TileResultCode.Delayed;
			}
		}

		// Token: 0x06000A0B RID: 2571 RVA: 0x00049F85 File Offset: 0x00048385
		public TileResultCode DebugPlayAnim(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.stateHandler == null)
			{
				return TileResultCode.True;
			}
			this.stateHandler.DebugPlayAnim(Util.GetStringArg(args, 0, "Idle"));
			return TileResultCode.True;
		}

		// Token: 0x06000A0C RID: 2572 RVA: 0x00049FAC File Offset: 0x000483AC
		public TileResultCode ToggleCrawl(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.stateHandler == null)
			{
				return TileResultCode.True;
			}
			if (this.stateHandler.IsCrawling())
			{
				this.stateHandler.InterruptState(CharacterState.CrawlExit, true);
			}
			else
			{
				this.stateHandler.InterruptState(CharacterState.CrawlEnter, true);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000A0D RID: 2573 RVA: 0x00049FF9 File Offset: 0x000483F9
		public new TileResultCode IsJumping(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!this.stateHandler.IsJumping()) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000A0E RID: 2574 RVA: 0x0004A012 File Offset: 0x00048412
		public TileResultCode CanDoubleJump(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!this.stateHandler.CanDoubleJump()) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000A0F RID: 2575 RVA: 0x0004A02B File Offset: 0x0004842B
		public TileResultCode IsSwimming(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!this.stateHandler.IsSwimming()) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000A10 RID: 2576 RVA: 0x0004A044 File Offset: 0x00048444
		public TileResultCode IsCrawling(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!this.stateHandler.IsCrawling()) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000A11 RID: 2577 RVA: 0x0004A05D File Offset: 0x0004845D
		public TileResultCode IsDodging(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!this.stateHandler.IsDodging()) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000A12 RID: 2578 RVA: 0x0004A076 File Offset: 0x00048476
		public TileResultCode IsWalking(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!this.stateHandler.IsWalking()) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000A13 RID: 2579 RVA: 0x0004A08F File Offset: 0x0004848F
		public TileResultCode IsProne(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!this.stateHandler.IsProne()) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000A14 RID: 2580 RVA: 0x0004A0A8 File Offset: 0x000484A8
		public TileResultCode DoubleJump(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float power = (float)args[0] * eInfo.floatArg;
			return (!this.stateHandler.DoubleJump(power)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000A15 RID: 2581 RVA: 0x0004A0E0 File Offset: 0x000484E0
		public TileResultCode ApplyDiveForce(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float force = (float)args[0] * eInfo.floatArg;
			return (!this.stateHandler.ApplyDiveForce(force)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000A16 RID: 2582 RVA: 0x0004A118 File Offset: 0x00048518
		public TileResultCode IsRole(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string stringArgSafe = Util.GetStringArgSafe(args, 0, "Male");
			CharacterRole characterRole = (CharacterRole)Enum.Parse(typeof(CharacterRole), stringArgSafe);
			return (characterRole != this.stateHandler.currentRole) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000A17 RID: 2583 RVA: 0x0004A160 File Offset: 0x00048560
		public TileResultCode SetRole(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string stringArgSafe = Util.GetStringArgSafe(args, 0, "Male");
			CharacterRole characterRole = (CharacterRole)Enum.Parse(typeof(CharacterRole), stringArgSafe);
			CharacterRole characterRole2 = this.DefaultRoleForCharacterType(this.characterType);
			bool flag = characterRole2 == CharacterRole.Mini || characterRole2 == CharacterRole.MiniFemale;
			if (flag && characterRole == CharacterRole.Male)
			{
				characterRole = CharacterRole.Mini;
			}
			if (flag && characterRole == CharacterRole.Female)
			{
				characterRole = CharacterRole.MiniFemale;
			}
			if (characterRole != this.stateHandler.currentRole)
			{
				this.stateHandler.SetRole(characterRole);
				this.stateHandler.InterruptState(this.stateHandler.currentState, true);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000A18 RID: 2584 RVA: 0x0004A200 File Offset: 0x00048600
		public TileResultCode ResetRole(ScriptRowExecutionInfo eInfo, object[] args)
		{
			CharacterRole characterRole = this.DefaultRoleForCharacterType(this.characterType);
			if (characterRole != this.stateHandler.currentRole)
			{
				this.stateHandler.SetRole(characterRole);
				this.stateHandler.InterruptState(this.stateHandler.currentState, true);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000A19 RID: 2585 RVA: 0x0004A250 File Offset: 0x00048650
		public TileResultCode ReplaceBodyPart(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.bodyParts == null)
			{
				return TileResultCode.False;
			}
			string stringArgSafe = Util.GetStringArgSafe(args, 0, string.Empty);
			string stringArgSafe2 = Util.GetStringArgSafe(args, 1, string.Empty);
			if (string.IsNullOrEmpty(stringArgSafe) || string.IsNullOrEmpty(stringArgSafe2))
			{
				return TileResultCode.False;
			}
			BlocksterBody.BodyPart bodyPart = (BlocksterBody.BodyPart)Enum.Parse(typeof(BlocksterBody.BodyPart), stringArgSafe2);
			this.SubstituteBodyPart(bodyPart, stringArgSafe, false);
			return TileResultCode.True;
		}

		// Token: 0x06000A1A RID: 2586 RVA: 0x0004A2BC File Offset: 0x000486BC
		public void SubstituteBodyPart(BlocksterBody.BodyPart bodyPart, string partStr, bool applyDefaultPaints)
		{
			this.bodyParts.SubstitutePart(bodyPart, partStr);
			this.RebuildSubMeshes();
			this.UpdateCachedBodyParts();
			if (applyDefaultPaints)
			{
				this.bodyParts.ApplyDefaultPaints(bodyPart);
				this.RebuildSubMeshPaintTiles();
			}
		}

		// Token: 0x06000A1B RID: 2587 RVA: 0x0004A2F0 File Offset: 0x000486F0
		public void SetLimbsToDefaults()
		{
			string str = "Limb Default";
			if (this.characterType == CharacterType.Skeleton)
			{
				str = "Limb Skeleton";
			}
			bool flag = this.lockPaintAndTexture;
			this.lockPaintAndTexture = false;
			this.SubstituteBodyPart(BlocksterBody.BodyPart.LeftArm, str + " Arm Left", true);
			this.SubstituteBodyPart(BlocksterBody.BodyPart.RightArm, str + " Arm Right", true);
			this.SubstituteBodyPart(BlocksterBody.BodyPart.LeftLeg, str + " Leg Left", true);
			this.SubstituteBodyPart(BlocksterBody.BodyPart.RightLeg, str + " Leg Right", true);
			this.SaveBodyPartAssignmentToTiles(BlocksterBody.BodyPart.LeftArm);
			this.SaveBodyPartAssignmentToTiles(BlocksterBody.BodyPart.RightArm);
			this.SaveBodyPartAssignmentToTiles(BlocksterBody.BodyPart.LeftLeg);
			this.SaveBodyPartAssignmentToTiles(BlocksterBody.BodyPart.RightLeg);
			this.lockPaintAndTexture = flag;
		}

		// Token: 0x06000A1C RID: 2588 RVA: 0x0004A394 File Offset: 0x00048794
		public void SaveBodyPartAssignmentToTiles(BlocksterBody.BodyPart bodyPart)
		{
			List<Tile> list = this.tiles[0];
			int num = 0;
			string text = bodyPart.ToString();
			string text2 = this.bodyParts.currentBodyPartVersions[bodyPart];
			foreach (Tile tile in list)
			{
				string name = tile.gaf.Predicate.Name;
				if (name == "AnimCharacter.ReplaceBodyPart" && tile.gaf.Args.Length > 1 && (string)tile.gaf.Args[1] == text)
				{
					break;
				}
				num++;
			}
			if (num < list.Count)
			{
				Tile tile2 = list[num];
				tile2.gaf.Args[0] = text2;
			}
			else
			{
				list.Add(new Tile(new GAF("AnimCharacter.ReplaceBodyPart", new object[]
				{
					text2,
					text
				})));
			}
		}

		// Token: 0x06000A1D RID: 2589 RVA: 0x0004A4C0 File Offset: 0x000488C0
		public string CurrentlyAssignedPartVersion(BlocksterBody.BodyPart bodyPart)
		{
			return this.bodyParts.currentBodyPartVersions[bodyPart];
		}

		// Token: 0x06000A1E RID: 2590 RVA: 0x0004A4D3 File Offset: 0x000488D3
		public void SetShaderForBodyPart(BlocksterBody.BodyPart bodyPart, ShaderType shader)
		{
			this.bodyParts.SetShaderForBodyPart(bodyPart, shader);
		}

		// Token: 0x06000A1F RID: 2591 RVA: 0x0004A4E4 File Offset: 0x000488E4
		public List<GameObject> GetAllBodyPartObjects()
		{
			List<GameObject> list = new List<GameObject>();
			list.AddRange(this.bodyParts.GetObjectsForBodyPart(BlocksterBody.BodyPart.RightArm));
			list.AddRange(this.bodyParts.GetObjectsForBodyPart(BlocksterBody.BodyPart.LeftArm));
			list.AddRange(this.bodyParts.GetObjectsForBodyPart(BlocksterBody.BodyPart.RightLeg));
			list.AddRange(this.bodyParts.GetObjectsForBodyPart(BlocksterBody.BodyPart.LeftLeg));
			return list;
		}

		// Token: 0x06000A20 RID: 2592 RVA: 0x0004A540 File Offset: 0x00048940
		public void CalculateBodyPartsGAFUsage(Dictionary<GAF, int> gafUsage)
		{
			foreach (string text in this.bodyParts.currentBodyPartVersions.Values)
			{
				string text2 = text;
				if (text.EndsWith(" Left"))
				{
					text2 = text.Remove(text.Length - 5, 5);
				}
				else if (text.EndsWith(" Right"))
				{
					text2 = text.Remove(text.Length - 6, 6);
				}
				GAF key = new GAF("AnimCharacter.ReplaceBodyPart", new object[]
				{
					text2
				});
				if (gafUsage.ContainsKey(key))
				{
					gafUsage[key]++;
				}
				else
				{
					gafUsage[key] = 1;
				}
			}
		}

		// Token: 0x06000A21 RID: 2593 RVA: 0x0004A624 File Offset: 0x00048A24
		public bool CanBounce(out float bounciness)
		{
			bool flag = base.NearGround(0.4f) > 0f;
			bounciness = this.GetGroundBounciness();
			float num = Vector3.Dot(-this.walkController.legsRb.velocity, this.walkController.groundNormal);
			return flag && bounciness > 0f && num > 2f;
		}

		// Token: 0x06000A22 RID: 2594 RVA: 0x0004A68F File Offset: 0x00048A8F
		private float GetGroundBounciness()
		{
			return this.walkController.groundBounciness;
		}

		// Token: 0x06000A23 RID: 2595 RVA: 0x0004A69C File Offset: 0x00048A9C
		public static void PlayQueuedHitReacts()
		{
			foreach (CharacterStateHandler characterStateHandler in BlockAnimatedCharacter.stateControllers.Values)
			{
				characterStateHandler.PlayQueuedHitReact();
			}
		}

		// Token: 0x040007A7 RID: 1959
		public static Dictionary<BlockAnimatedCharacter, CharacterStateHandler> stateControllers = new Dictionary<BlockAnimatedCharacter, CharacterStateHandler>();

		// Token: 0x040007A8 RID: 1960
		public static Vector3 rightHandPlacementOffset = new Vector3(0.12f, 0.2f, 0f);

		// Token: 0x040007A9 RID: 1961
		public static Vector3 leftHandPlacementOffset = new Vector3(-0.12f, 0.2f, 0f);

		// Token: 0x040007AA RID: 1962
		public CharacterStateHandler stateHandler;

		// Token: 0x040007AB RID: 1963
		public Block attachedBottomBlock;

		// Token: 0x040007AC RID: 1964
		public Block attachedFrontBlock;

		// Token: 0x040007AD RID: 1965
		public Block attachedLeftBlock;

		// Token: 0x040007AE RID: 1966
		public Block attachedRightBlock;

		// Token: 0x040007AF RID: 1967
		public List<Block> attachedBackBlocks = new List<Block>();

		// Token: 0x040007B0 RID: 1968
		public List<Block> attachedHeadBlocks = new List<Block>();

		// Token: 0x040007B1 RID: 1969
		public List<Block> attachedLeftHandBlocks = new List<Block>();

		// Token: 0x040007B2 RID: 1970
		public List<Block> attachedRightHandBlocks = new List<Block>();

		// Token: 0x040007B3 RID: 1971
		public bool attachmentsPreventLookAnim;

		// Token: 0x040007B4 RID: 1972
		public bool isConnectedToUnsupportedActionBlock;

		// Token: 0x040007B5 RID: 1973
		private bool haveDeterminedAttachments;

		// Token: 0x040007B6 RID: 1974
		private Vector3 pausedVelocityHands0;

		// Token: 0x040007B7 RID: 1975
		private Vector3 pausedAngularVelocityHands0;

		// Token: 0x040007B8 RID: 1976
		private Vector3 pausedVelocityHands1;

		// Token: 0x040007B9 RID: 1977
		private Vector3 pausedAngularVelocityHands1;

		// Token: 0x040007BA RID: 1978
		private Vector3 pausedVelocityMiddle;

		// Token: 0x040007BB RID: 1979
		private Vector3 pausedAngularVelocityMiddle;

		// Token: 0x040007BC RID: 1980
		private bool canUseCharacterShadow;

		// Token: 0x040007BD RID: 1981
		private Transform footLt;

		// Token: 0x040007BE RID: 1982
		private Transform footRt;

		// Token: 0x040007BF RID: 1983
		private Transform hair;

		// Token: 0x040007C0 RID: 1984
		public static Predicate predicateCharacterMover;

		// Token: 0x040007C1 RID: 1985
		public static Predicate predicateChracterTiltMover;

		// Token: 0x040007C2 RID: 1986
		public static Predicate predicateCharacterJump;

		// Token: 0x040007C3 RID: 1987
		public static Predicate predicateCharacterGotoTag;

		// Token: 0x040007C4 RID: 1988
		public static Predicate predicateCharacterChaseTag;

		// Token: 0x040007C5 RID: 1989
		public static Predicate predicateCharacterAvoidTag;

		// Token: 0x040007C6 RID: 1990
		public static Predicate predicateReplaceLimb;

		// Token: 0x040007C7 RID: 1991
		private static GameObject prefabCharacterShadow;

		// Token: 0x040007C8 RID: 1992
		private List<Transform> characterPieces = new List<Transform>();

		// Token: 0x040007C9 RID: 1993
		private Renderer headClone;

		// Token: 0x040007CA RID: 1994
		private bool _playingAttack;

		// Token: 0x040007CB RID: 1995
		private bool _playingDodge;

		// Token: 0x040007CC RID: 1996
		private int canceledAnimOnRow = -1;

		// Token: 0x040007CD RID: 1997
		private int currentAnimRow = -1;

		// Token: 0x040007CE RID: 1998
		private bool lockPaintAndTexture;

		// Token: 0x040007CF RID: 1999
		public float blocksterRigScale = 1.25f;

		// Token: 0x040007D0 RID: 2000
		private Dictionary<string, string> blocksterToSkeleton = new Dictionary<string, string>
		{
			{
				"Character Body",
				"bJoint_hips"
			},
			{
				"Character Short Hair",
				"bJoint_head"
			},
			{
				"Character Head",
				"bJoint_head"
			}
		};

		// Token: 0x040007D1 RID: 2001
		private Dictionary<BlocksterBody.Bone, Transform> boneLookup;

		// Token: 0x040007D2 RID: 2002
		public CharacterType characterType;

		// Token: 0x040007D3 RID: 2003
		public static int idxBottom;

		// Token: 0x040007D4 RID: 2004
		public static int idxFront;

		// Token: 0x040007D5 RID: 2005
		public static int idxLeft;

		// Token: 0x040007D6 RID: 2006
		public static int idxRight;

		// Token: 0x040007D7 RID: 2007
		public static int idxBack;

		// Token: 0x040007D8 RID: 2008
		private static bool gotGlueIndices = false;

		// Token: 0x040007D9 RID: 2009
		private const float miniBodyScale = 0.8f;

		// Token: 0x040007DA RID: 2010
		private bool gotSlopedGlue;

		// Token: 0x040007DB RID: 2011
		public BlocksterBody bodyParts;
	}
}
