using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x02000080 RID: 128
	public class BlockCharacter : BlockAbstractLegs, IBlockster
	{
		// Token: 0x06000AF3 RID: 2803 RVA: 0x0004E1F4 File Offset: 0x0004C5F4
		public BlockCharacter(List<List<Tile>> tiles, Dictionary<string, string> partNames, bool hasLegMesh, float footHeight = -1.35f) : base(tiles, partNames, 1, null, null, 0f, false, 1f, 0.25f)
		{
			this.maxStepHeight = 1.5f;
			this.maxStepLength = 0.75f;
			this.onGroundHeight = -footHeight + 0.15f;
			this.replaceWithCapsuleCollider = true;
			if (base.BlockType().EndsWith("Headless"))
			{
				this.capsuleColliderOffset = 0.4f * Vector3.down;
				this.capsuleColliderHeight = 0.9f;
				this.capsuleColliderRadius = 0.6f;
			}
			else if (footHeight != -1.35f)
			{
				this.capsuleColliderOffset = 0.125f * Vector3.up;
				this.capsuleColliderHeight = 1.75f;
				this.capsuleColliderRadius = 0.7f;
			}
			else
			{
				this.capsuleColliderOffset = Vector3.zero;
				this.capsuleColliderHeight = 2f;
				this.capsuleColliderRadius = 0.7f;
			}
			this.hasLegMesh = hasLegMesh;
			this.footHeight = footHeight;
			this.moveCMOffsetFeetCenter = 0.75f;
			this.moveCMMaxDistance = 1f;
			this.moveCM = true;
			if (!BlockCharacter.gotGlueIndices)
			{
				BlockCharacter.idxBottom = CollisionTest.IndexOfGlueCollisionMesh("Character", "Bottom");
				BlockCharacter.idxFront = CollisionTest.IndexOfGlueCollisionMesh("Character", "Front");
				BlockCharacter.idxLeft = CollisionTest.IndexOfGlueCollisionMesh("Character", "Left");
				BlockCharacter.idxRight = CollisionTest.IndexOfGlueCollisionMesh("Character", "Right");
				BlockCharacter.idxBack = CollisionTest.IndexOfGlueCollisionMesh("Character", "Back");
				BlockCharacter.gotGlueIndices = true;
			}
		}

		// Token: 0x06000AF4 RID: 2804 RVA: 0x0004E410 File Offset: 0x0004C810
		public void IBlockster_FindAttachments()
		{
			this.FindAttachements();
		}

		// Token: 0x06000AF5 RID: 2805 RVA: 0x0004E418 File Offset: 0x0004C818
		public Block IBlockster_BottomAttachment()
		{
			return this.bottom;
		}

		// Token: 0x06000AF6 RID: 2806 RVA: 0x0004E420 File Offset: 0x0004C820
		public List<Block> IBlockster_HeadAttachments()
		{
			return this.headAttachments;
		}

		// Token: 0x06000AF7 RID: 2807 RVA: 0x0004E428 File Offset: 0x0004C828
		public Block IBlockster_FrontAttachment()
		{
			return this.front;
		}

		// Token: 0x06000AF8 RID: 2808 RVA: 0x0004E430 File Offset: 0x0004C830
		public Block IBlockster_BackAttachment()
		{
			return this.back;
		}

		// Token: 0x06000AF9 RID: 2809 RVA: 0x0004E438 File Offset: 0x0004C838
		public Block IBlockster_RightHandAttachment()
		{
			return this.right;
		}

		// Token: 0x06000AFA RID: 2810 RVA: 0x0004E440 File Offset: 0x0004C840
		public Block IBlockster_LeftHandAttachement()
		{
			return this.left;
		}

		// Token: 0x06000AFB RID: 2811 RVA: 0x0004E448 File Offset: 0x0004C848
		public new static void Register()
		{
			BlockCharacter.predicateCharacterJump = PredicateRegistry.Add<BlockCharacter>("Character.Jump", (Block b) => new PredicateSensorDelegate(((BlockAbstractLegs)b).IsJumping), (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).Jump), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Force"
			}, null);
			BlockCharacter.predicateCharacterGotoTag = PredicateRegistry.Add<BlockCharacter>("Character.GotoTag", null, (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).GotoTag), new Type[]
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
			BlockCharacter.predicateCharacterChaseTag = PredicateRegistry.Add<BlockCharacter>("Character.ChaseTag", null, (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).ChaseTag), new Type[]
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
			PredicateRegistry.Add<BlockCharacter>("Character.GotoTap", null, (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).GotoTap), new Type[]
			{
				typeof(float),
				typeof(float)
			}, new string[]
			{
				"Speed",
				"Wackiness"
			}, null);
			BlockCharacter.predicateCharacterMover = PredicateRegistry.Add<BlockCharacter>("Character.DPadControl", null, (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).DPadControl), new Type[]
			{
				typeof(string),
				typeof(float),
				typeof(float)
			}, null, null);
			BlockCharacter.predicateCharacterTiltMover = PredicateRegistry.Add<BlockCharacter>("Character.TiltMover", null, (Block b) => new PredicateActionDelegate(b.TiltMoverControl), new Type[]
			{
				typeof(float),
				typeof(int)
			}, null, null);
			PredicateRegistry.Add<BlockCharacter>("Character.Translate", null, (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).Translate), new Type[]
			{
				typeof(Vector3),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockCharacter>("Character.WackyMode", null, (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).WackyMode), null, null, null);
			PredicateRegistry.Add<BlockCharacter>("Character.Turn", null, (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).Turn), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockCharacter>("Character.TurnTowardsTag", null, (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).TurnTowardsTag), new Type[]
			{
				typeof(string)
			}, null, null);
			PredicateRegistry.Add<BlockCharacter>("Character.TurnTowardsTap", null, (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).TurnTowardsTap), null, null, null);
			PredicateRegistry.Add<BlockCharacter>("Character.TurnAlongCam", null, (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).TurnAlongCam), null, null, null);
			BlockCharacter.predicateCharacterAvoidTag = PredicateRegistry.Add<BlockCharacter>("Character.AvoidTag", null, (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).AvoidTag), new Type[]
			{
				typeof(string),
				typeof(float),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockCharacter>("Character.Idle", null, (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).Idle), null, null, null);
			PredicateRegistry.Add<BlockCharacter>("Character.FreezeRotation", null, (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).FreezeRotation), null, null, null);
		}

		// Token: 0x06000AFC RID: 2812 RVA: 0x0004E8BC File Offset: 0x0004CCBC
		public static void StripNonCompatibleTiles(List<List<Tile>> tiles)
		{
			HashSet<Predicate> hashSet = new HashSet<Predicate>();
			hashSet.UnionWith(PredicateRegistry.ForType(typeof(BlockCharacter), true));
			hashSet.UnionWith(PredicateRegistry.ForType(typeof(BlockAbstractLegs), true));
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

		// Token: 0x06000AFD RID: 2813 RVA: 0x0004E998 File Offset: 0x0004CD98
		public override void OnCreate()
		{
			this.lockPaintAndTexture = this.isAvatar;
		}

		// Token: 0x06000AFE RID: 2814 RVA: 0x0004E9A8 File Offset: 0x0004CDA8
		public override void Destroy()
		{
			if (this.hands != null)
			{
				for (int i = 0; i < this.hands.Length; i++)
				{
					if (this.hands[i].gameObject != null)
					{
						UnityEngine.Object.Destroy(this.hands[i].gameObject);
					}
				}
			}
			if (this.middle != null)
			{
				UnityEngine.Object.Destroy(this.middle);
			}
			base.Destroy();
		}

		// Token: 0x06000AFF RID: 2815 RVA: 0x0004EA28 File Offset: 0x0004CE28
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

		// Token: 0x06000B00 RID: 2816 RVA: 0x0004EAA8 File Offset: 0x0004CEA8
		public override void FindFeet()
		{
			string name = (!this.partNames.ContainsKey("Foot Right")) ? "Character Foot Right" : this.partNames["Foot Right"];
			string name2 = (!this.partNames.ContainsKey("Foot Left")) ? "Character Foot Left" : this.partNames["Foot Left"];
			string name3 = (!this.partNames.ContainsKey("Hand Right")) ? "Character Hand Right" : this.partNames["Hand Right"];
			string name4 = (!this.partNames.ContainsKey("Hand Left")) ? "Character Hand Left" : this.partNames["Hand Left"];
			string name5 = (!this.partNames.ContainsKey("Body")) ? "Character Body" : this.partNames["Body"];
			this.feet = new FootInfo[2 * this.legPairCount];
			for (int i = 0; i < this.feet.Length; i++)
			{
				this.feet[i] = new FootInfo();
			}
			this.feet[0].go = this.goT.Find(name).gameObject;
			this.feet[1].go = this.goT.Find(name2).gameObject;
			if (this.feet[0] != null)
			{
				this.footPos = this.feet[0].go.transform.localPosition;
				this.footRot = this.feet[0].go.transform.localEulerAngles;
			}
			this.hands[0] = this.goT.Find(name3).gameObject;
			this.hands[1] = this.goT.Find(name4).gameObject;
			this.middle = this.goT.Find(name5).gameObject;
			if (this.hands[0] != null && this.middle != null)
			{
				this.handPos = this.hands[0].transform.localPosition;
				this.handsOutXMod = this.middle.transform.localScale.x * 0.5f;
				this.handsOutYMod -= -(0.6f + this.hands[0].transform.localPosition.y);
			}
			this.collider = this.go.GetComponent<BoxCollider>();
			this.colliderSize = this.collider.size;
			this.colliderCenter = this.collider.center;
			this.middleLocalPosition = this.middle.transform.localPosition;
		}

		// Token: 0x06000B01 RID: 2817 RVA: 0x0004ED8C File Offset: 0x0004D18C
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

		// Token: 0x06000B02 RID: 2818 RVA: 0x0004EEA8 File Offset: 0x0004D2A8
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

		// Token: 0x06000B03 RID: 2819 RVA: 0x0004EFC4 File Offset: 0x0004D3C4
		public override void Break(Vector3 chunkPos, Vector3 chunkVel, Vector3 chunkAngVel)
		{
			base.Break(chunkPos, chunkVel, chunkAngVel);
			this.BreakCollider();
			Blocksworld.AddFixedUpdateCommand(new DelayedBreakCharacterCommand(this, chunkPos, chunkVel, chunkAngVel));
		}

		// Token: 0x06000B04 RID: 2820 RVA: 0x0004EFE4 File Offset: 0x0004D3E4
		public override void BeforePlay()
		{
			this.FindAttachements();
			bool flag = this.left == null || this.right == null;
			this.replaceWithCapsuleCollider = (this.front == null && this.bottom == null && flag);
			this.goLayerAssignment = Layer.Default;
			this.go.SetLayer(this.goLayerAssignment, true);
		}

		// Token: 0x06000B05 RID: 2821 RVA: 0x0004F048 File Offset: 0x0004D448
		public void FindAttachements()
		{
			this.bottom = null;
			this.front = null;
			this.left = null;
			this.right = null;
			this.back = null;
			this.headAttachments = new List<Block>();
			Matrix4x4 worldToLocalMatrix = this.goT.worldToLocalMatrix;
			for (int i = 0; i < this.connections.Count; i++)
			{
				Block block = this.connections[i];
				if (!block.isTerrain)
				{
					Vector3 vector = worldToLocalMatrix.MultiplyPoint(block.goT.position);
					bool flag = false;
					if (this.bottom == null && vector.y < 0.5f && (CollisionTest.MultiMeshMeshTest2(this.glueMeshes[BlockCharacter.idxBottom], block.glueMeshes, false) || CollisionTest.MultiMeshMeshTest2(this.glueMeshes[BlockCharacter.idxBottom], block.jointMeshes, false)))
					{
						this.bottom = block;
						flag = true;
					}
					if (this.front == null && vector.z > -0.5f && (CollisionTest.MultiMeshMeshTest2(this.glueMeshes[BlockCharacter.idxFront], block.glueMeshes, false) || CollisionTest.MultiMeshMeshTest2(this.glueMeshes[BlockCharacter.idxFront], block.jointMeshes, false)))
					{
						this.front = block;
						flag = true;
					}
					if (this.left == null && vector.x < 0.5f && (CollisionTest.MultiMeshMeshTest2(this.glueMeshes[BlockCharacter.idxLeft], block.glueMeshes, false) || CollisionTest.MultiMeshMeshTest2(this.glueMeshes[BlockCharacter.idxLeft], block.jointMeshes, false)))
					{
						this.left = block;
						flag = true;
					}
					if (this.right == null && vector.x > -0.5f && (CollisionTest.MultiMeshMeshTest2(this.glueMeshes[BlockCharacter.idxRight], block.glueMeshes, false) || CollisionTest.MultiMeshMeshTest2(this.glueMeshes[BlockCharacter.idxRight], block.jointMeshes, false)))
					{
						this.right = block;
						flag = true;
					}
					if (this.back == null && vector.z < 0.5f && (CollisionTest.MultiMeshMeshTest2(this.glueMeshes[BlockCharacter.idxBack], block.glueMeshes, false) || CollisionTest.MultiMeshMeshTest2(this.glueMeshes[BlockCharacter.idxBack], block.jointMeshes, false)))
					{
						this.back = block;
						flag = true;
					}
					if (!flag)
					{
						this.headAttachments.Add(block);
					}
				}
			}
		}

		// Token: 0x06000B06 RID: 2822 RVA: 0x0004F2D4 File Offset: 0x0004D6D4
		public override void BecameTreasure()
		{
			base.BecameTreasure();
			for (int i = 0; i < 2; i++)
			{
				FootInfo footInfo = this.feet[i];
				footInfo.go.transform.localPosition = new Vector3((i != 0) ? (-this.footPos.x) : this.footPos.x, this.footPos.y, this.footPos.z);
				footInfo.go.transform.localEulerAngles = this.footRot;
			}
		}

		// Token: 0x06000B07 RID: 2823 RVA: 0x0004F368 File Offset: 0x0004D768
		public override void Play()
		{
			if (this.front != null)
			{
				Vector3 localPosition = this.handPos;
				localPosition.z += 0.3f;
				this.hands[0].transform.localPosition = localPosition;
				localPosition.x *= -1f;
				this.hands[1].transform.localPosition = localPosition;
			}
			this.keepCollider = true;
			base.Play();
			this.middle.GetComponent<Collider>().enabled = true;
			this.hands[0].GetComponent<Collider>().enabled = true;
			this.hands[1].GetComponent<Collider>().enabled = true;
			this.unmoving = (this.unmoving || this.bottom != null || this.front != null || base.IsFixed());
			base.UpdateConnectedCache();
			List<Block> list = Block.connectedCache[this];
			this.canUseCharacterShadow = !Blocksworld.renderingShadows;
			foreach (Block block in list)
			{
				if (block is BlockAbstractLegs && block != this)
				{
					this.canUseCharacterShadow = false;
				}
			}
			if (this.canUseCharacterShadow)
			{
				HashSet<Predicate> manyPreds = new HashSet<Predicate>
				{
					BlockCharacter.predicateCharacterMover
				};
				this.canUseCharacterShadow = base.ContainsTileWithAnyPredicateInPlayMode2(manyPreds);
			}
			if (this.canUseCharacterShadow)
			{
				if (BlockCharacter.prefabCharacterShadow == null)
				{
					BlockCharacter.prefabCharacterShadow = (UnityEngine.Object.Instantiate(Resources.Load("Blocks/Character Shadow")) as GameObject);
					BlockCharacter.prefabCharacterShadow.SetActive(false);
					Material material = BlockCharacter.prefabCharacterShadow.GetComponent<Renderer>().material;
					if (material == null)
					{
						BWLog.Info("Character shadow material was null");
					}
				}
				if (this.goShadow != null)
				{
					UnityEngine.Object.Destroy(this.goShadow);
				}
				base.InstantiateShadow(BlockCharacter.prefabCharacterShadow);
				this.goShadow.GetComponent<Renderer>().enabled = true;
			}
		}

		// Token: 0x06000B08 RID: 2824 RVA: 0x0004F594 File Offset: 0x0004D994
		public override void PlayLegs1()
		{
			if (this.unmoving)
			{
				return;
			}
			for (int i = 0; i < 2; i++)
			{
				FootInfo footInfo = this.feet[i];
				Transform transform = footInfo.go.transform;
				transform.localPosition = new Vector3((i != 0) ? (this.footPos.x * -1.25f) : (this.footPos.x * 1.25f), this.footHeight, 0f);
				transform.transform.localEulerAngles = Vector3.zero;
				footInfo.position = transform.position;
				FootInfo footInfo2 = footInfo;
				footInfo2.position.y = footInfo2.position.y + 0.5f;
				if (i != 0 || this.right == null)
				{
					if (i != 1 || this.left == null)
					{
						this.hands[i].transform.localPosition = new Vector3((i != 0) ? (-this.handsOutXMod) : this.handsOutXMod, -0.6f, 0f);
						this.hands[i].transform.localEulerAngles = new Vector3(0f, 0f, (i != 0) ? -25f : 25f);
					}
				}
			}
			if (this.back == null)
			{
				this.middle.transform.localPosition = this.middleLocalPosition + Vector3.forward * 0.1f;
			}
			base.PlayLegs1();
		}

		// Token: 0x06000B09 RID: 2825 RVA: 0x0004F724 File Offset: 0x0004DB24
		public override void PlayLegs2()
		{
			if (this.unmoving)
			{
				return;
			}
			base.PlayLegs2();
			for (int i = 0; i < 2; i++)
			{
				BoxCollider component = this.feet[i].go.GetComponent<BoxCollider>();
				component.size = new Vector3(0.125f, 0.25f, 0.25f);
			}
			if (this.canUseCharacterShadow)
			{
				this.walkController.climbOn = true;
			}
		}

		// Token: 0x06000B0A RID: 2826 RVA: 0x0004F799 File Offset: 0x0004DB99
		public override bool FeetPartOfGo()
		{
			return this.unmoving && !this.broken;
		}

		// Token: 0x06000B0B RID: 2827 RVA: 0x0004F7B2 File Offset: 0x0004DBB2
		public override bool CanMergeShadow()
		{
			return false;
		}

		// Token: 0x06000B0C RID: 2828 RVA: 0x0004F7B5 File Offset: 0x0004DBB5
		protected override int GetPrimaryMeshIndex()
		{
			return (!base.BlockType().EndsWith("Headless")) ? 0 : 1;
		}

		// Token: 0x06000B0D RID: 2829 RVA: 0x0004F7D4 File Offset: 0x0004DBD4
		public override void Stop(bool resetBlock = true)
		{
			base.Stop(resetBlock);
			this.RestoreCollider();
			this.middle.GetComponent<Collider>().enabled = false;
			this.hands[0].GetComponent<Collider>().enabled = false;
			this.hands[1].GetComponent<Collider>().enabled = false;
			for (int i = 0; i < 2; i++)
			{
				if (this.hands[i].transform.parent == null)
				{
					UnityEngine.Object.Destroy(this.hands[i].GetComponent<Rigidbody>());
					this.hands[i].transform.parent = this.goT;
				}
				this.hands[i].transform.localPosition = new Vector3((i != 0) ? (-this.handPos.x) : this.handPos.x, this.handPos.y, this.handPos.z);
				this.hands[i].transform.localEulerAngles = Vector3.zero;
				FootInfo footInfo = this.feet[i];
				Transform transform = footInfo.go.transform;
				if (transform.parent == null)
				{
					UnityEngine.Object.Destroy(footInfo.go.GetComponent<Rigidbody>());
					transform.parent = this.goT;
				}
				transform.localPosition = new Vector3((i != 0) ? (-this.footPos.x) : this.footPos.x, this.footPos.y, this.footPos.z);
				transform.localEulerAngles = this.footRot;
			}
			if (this.middle.transform.parent == null)
			{
				UnityEngine.Object.Destroy(this.middle.GetComponent<Rigidbody>());
				this.middle.transform.parent = this.goT;
				this.collider.size = this.colliderSize;
				this.collider.center = this.colliderCenter;
			}
			this.middle.transform.localPosition = this.middleLocalPosition;
			this.middle.transform.localEulerAngles = Vector3.zero;
		}

		// Token: 0x06000B0E RID: 2830 RVA: 0x0004FA03 File Offset: 0x0004DE03
		public override void PositionAnkle(int i)
		{
			if (this.hasLegMesh)
			{
				base.PositionAnkle(i);
			}
		}

		// Token: 0x06000B0F RID: 2831 RVA: 0x0004FA18 File Offset: 0x0004DE18
		public override void IgnoreRaycasts(bool value)
		{
			base.IgnoreRaycasts(value);
			int layer = (int)((!value) ? this.goLayerAssignment : Layer.IgnoreRaycast);
			this.middle.layer = layer;
			this.hands[0].layer = layer;
			this.hands[1].layer = layer;
		}

		// Token: 0x06000B10 RID: 2832 RVA: 0x0004FA68 File Offset: 0x0004DE68
		public override TileResultCode PaintTo(string paint, bool permanent, int meshIndex = 0)
		{
			if (this.lockPaintAndTexture)
			{
				return TileResultCode.False;
			}
			if (permanent && meshIndex == 1 && base.GetTexture(1) == "Plain" && Block.skinPaints.Contains(paint) && base.GetDefaultTexture(1) != "Plain")
			{
				this.TextureTo("Clothing Underwear", Vector3.forward, permanent, 1, false);
			}
			return base.PaintTo(paint, permanent, meshIndex);
		}

		// Token: 0x06000B11 RID: 2833 RVA: 0x0004FAE8 File Offset: 0x0004DEE8
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

		// Token: 0x06000B12 RID: 2834 RVA: 0x0004FB28 File Offset: 0x0004DF28
		public override TileResultCode TextureToAction(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string text = (string)args[0];
			Vector3 newNormal = (Vector3)args[1];
			int num = 1;
			if (args.Length > 2)
			{
				num = (int)args[2];
			}
			else if (base.IsCharacterFaceTexture(text))
			{
				num = 0;
				if (!base.BlockType().EndsWith("Skeleton"))
				{
					int meshIndex = 6;
					if (base.IsCharacterFaceWrapAroundTexture(text))
					{
						base.TextureToAction(text, newNormal, meshIndex);
					}
					else
					{
						base.TextureToAction("Plain", newNormal, meshIndex);
					}
				}
			}
			else if (Materials.IsMaterialShaderTexture(text))
			{
				num = ((!base.BlockType().EndsWith("Headless")) ? 0 : 1);
				for (int i = num + 1; i <= this.subMeshGameObjects.Count; i++)
				{
					base.TextureToAction(text, newNormal, i);
				}
			}
			return base.TextureToAction(text, newNormal, num);
		}

		// Token: 0x06000B13 RID: 2835 RVA: 0x0004FC14 File Offset: 0x0004E014
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
			return base.TextureTo(texture, normal, permanent, meshIndex, force);
		}

		// Token: 0x06000B14 RID: 2836 RVA: 0x0004FC8C File Offset: 0x0004E08C
		public void SetLayer(Layer layerEnum)
		{
			this.feet[0].go.layer = (int)layerEnum;
			this.feet[1].go.layer = (int)layerEnum;
			this.hands[0].layer = (int)layerEnum;
			this.hands[1].layer = (int)layerEnum;
			this.middle.layer = (int)layerEnum;
			this.go.layer = (int)layerEnum;
			this.goLayerAssignment = layerEnum;
		}

		// Token: 0x06000B15 RID: 2837 RVA: 0x0004FCFC File Offset: 0x0004E0FC
		protected void UpdateHands()
		{
			Rigidbody component = this.body.GetComponent<Rigidbody>();
			Vector3 velocity = component.velocity;
			float h = 1f;
			float num = base.Ipm(velocity.z, h);
			float num2 = base.Ipm(velocity.x, h);
			for (int i = 0; i < 2; i++)
			{
				if (i != 0 || this.right == null)
				{
					if (i != 1 || this.left == null)
					{
						float num3 = 0.6f;
						GameObject gameObject = this.hands[i];
						if (this.controllerWasActive)
						{
							this.walkController.SetHandPosition(i, gameObject, this.handsOutXMod, this.handsOutYMod);
						}
						else
						{
							Vector3 a = new Vector3(((i != 0) ? (-num3) : num3) - num2, -0.6f, -num);
							Vector3 a2 = a - gameObject.transform.localPosition;
							Vector3 localPosition = gameObject.transform.localPosition + 0.1f * a2;
							localPosition.x = ((i != 0) ? Mathf.Clamp(localPosition.x, -1f, -num3) : Mathf.Clamp(localPosition.x, num3, 1f));
							if (localPosition.sqrMagnitude < 1f)
							{
								gameObject.transform.localPosition = localPosition;
							}
							gameObject.transform.LookAt(this.goT.position + this.goT.up);
							gameObject.transform.Rotate(0f, 0f, 90f);
						}
					}
				}
			}
		}

		// Token: 0x06000B16 RID: 2838 RVA: 0x0004FEB4 File Offset: 0x0004E2B4
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (this.broken || this.unmoving)
			{
				return;
			}
			if (this.chunk.IsFrozen())
			{
				return;
			}
			this.UpdateHands();
			Rigidbody component = this.body.GetComponent<Rigidbody>();
			if (this.back == null)
			{
				if (!this.controllerWasActive && !this.hasLegMesh)
				{
					this.middle.transform.LookAt(this.goT.position + this.goT.rotation * (0.5f * this.hands[0].transform.localPosition + 0.5f * this.hands[1].transform.localPosition + new Vector3(0f, 0f, 1f)), this.goT.up + 0.1f * Vector3.up);
				}
			}
			if (this.controllerWasActive && this.hasMovedCM)
			{
				this.walkController.torqueMultiplier = 2f;
				float num = 5f;
				this.chunk.SetAngularDragMultiplier(num);
				component.angularDrag = 2f * num;
				bool enabled = this.walkController.IsOnGround();
				for (int i = 0; i < this.feet.Length; i++)
				{
					this.feet[i].go.GetComponent<Collider>().enabled = enabled;
				}
			}
			else
			{
				this.walkController.torqueMultiplier = 1f;
				this.chunk.SetAngularDragMultiplier(1f);
			}
		}

		// Token: 0x06000B17 RID: 2839 RVA: 0x00050074 File Offset: 0x0004E474
		protected override void ReplaceCollider()
		{
			BoxCollider[] componentsInChildren = this.go.GetComponentsInChildren<BoxCollider>();
			foreach (BoxCollider boxCollider in componentsInChildren)
			{
				GameObject gameObject = boxCollider.gameObject;
				this.boxColliderData[gameObject] = new BoxColliderData(boxCollider);
			}
			for (int j = 0; j < this.hands.Length; j++)
			{
				GameObject gameObject2 = this.hands[j];
				if (!this.boxColliderData.ContainsKey(gameObject2))
				{
					BoxCollider boxCollider2 = (BoxCollider)gameObject2.GetComponent<Collider>();
					if (boxCollider2 != null)
					{
						this.boxColliderData[gameObject2] = new BoxColliderData(boxCollider2);
					}
				}
			}
			base.ReplaceCollider();
		}

		// Token: 0x06000B18 RID: 2840 RVA: 0x00050134 File Offset: 0x0004E534
		protected BoxCollider RestoreBoxCollider(GameObject o)
		{
			BoxCollider boxCollider;
			if (o.GetComponent<BoxCollider>() != null)
			{
				boxCollider = o.GetComponent<BoxCollider>();
			}
			else
			{
				boxCollider = o.AddComponent<BoxCollider>();
			}
			if (this.boxColliderData.ContainsKey(o))
			{
				BoxColliderData boxColliderData = this.boxColliderData[o];
				boxCollider.center = boxColliderData.center;
				boxCollider.size = boxColliderData.size;
			}
			return boxCollider;
		}

		// Token: 0x06000B19 RID: 2841 RVA: 0x0005019C File Offset: 0x0004E59C
		protected override void RestoreCollider()
		{
			bool flag = false;
			if (this.go.GetComponent<Collider>() is CapsuleCollider)
			{
				UnityEngine.Object.Destroy(this.go.GetComponent<CapsuleCollider>());
				flag = true;
			}
			if (this.go.GetComponent<Collider>() == null || flag)
			{
				this.collider = this.RestoreBoxCollider(this.go);
			}
			if (this.middle.GetComponent<Collider>() == null)
			{
				this.RestoreBoxCollider(this.middle);
			}
			for (int i = 0; i < this.hands.Length; i++)
			{
				GameObject gameObject = this.hands[i];
				if (gameObject.GetComponent<Collider>() == null)
				{
					this.RestoreBoxCollider(gameObject);
				}
			}
			this.boxColliderData.Clear();
		}

		// Token: 0x06000B1A RID: 2842 RVA: 0x0005026C File Offset: 0x0004E66C
		private void BreakCollider()
		{
			bool flag = false;
			if (this.go.GetComponent<Collider>() is CapsuleCollider)
			{
				UnityEngine.Object.Destroy(this.go.GetComponent<CapsuleCollider>());
				flag = true;
			}
			if (base.BlockType().EndsWith("Headless"))
			{
				Blocksworld.blocksworldCamera.Unfollow(this.chunk);
				Blocksworld.chunks.Remove(this.chunk);
				this.chunk.Destroy(true);
				this.go.SetActive(false);
			}
			else if (this.go.GetComponent<Collider>() == null || flag)
			{
				this.collider = this.RestoreBoxCollider(this.go);
			}
			if (this.middle.GetComponent<Collider>() == null)
			{
				this.RestoreBoxCollider(this.middle);
			}
			for (int i = 0; i < this.hands.Length; i++)
			{
				GameObject gameObject = this.hands[i];
				if (gameObject.GetComponent<Collider>() == null)
				{
					this.RestoreBoxCollider(gameObject);
				}
			}
		}

		// Token: 0x06000B1B RID: 2843 RVA: 0x00050384 File Offset: 0x0004E784
		protected override void UnVanishFeet()
		{
			base.UnVanishFeet();
			Shader invisibleShader = this.InvisibleShader;
			if (this.middle.GetComponent<Renderer>().sharedMaterial.shader != invisibleShader)
			{
				this.middle.GetComponent<Renderer>().enabled = true;
				this.middle.SetActive(true);
			}
			for (int i = 0; i < this.hands.Length; i++)
			{
				if (this.hands[i].GetComponent<Renderer>().sharedMaterial.shader != invisibleShader)
				{
					this.hands[i].GetComponent<Renderer>().enabled = true;
					this.hands[i].SetActive(true);
				}
			}
		}

		// Token: 0x06000B1C RID: 2844 RVA: 0x00050438 File Offset: 0x0004E838
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

		// Token: 0x06000B1D RID: 2845 RVA: 0x000504F6 File Offset: 0x0004E8F6
		public override void Vanished()
		{
			base.Vanished();
			if (this.broken)
			{
				this.SetHandsAndBodyVisibleState(false);
			}
		}

		// Token: 0x06000B1E RID: 2846 RVA: 0x00050510 File Offset: 0x0004E910
		public override void Appeared()
		{
			base.Appeared();
			if (this.broken)
			{
				this.SetHandsAndBodyVisibleState(true);
			}
		}

		// Token: 0x06000B1F RID: 2847 RVA: 0x0005052C File Offset: 0x0004E92C
		public override List<GameObject> GetIgnoreRaycastGOs()
		{
			List<GameObject> ignoreRaycastGOs = base.GetIgnoreRaycastGOs();
			ignoreRaycastGOs.Add(this.middle);
			return ignoreRaycastGOs;
		}

		// Token: 0x06000B20 RID: 2848 RVA: 0x00050550 File Offset: 0x0004E950
		protected override void UpdateShadow()
		{
			if (!this.canUseCharacterShadow)
			{
				base.UpdateShadow();
				return;
			}
			Transform goT = this.goT;
			Vector3 position = goT.position;
			Vector3 b = 2.2f * Vector3.up;
			position.y -= this.footHeight;
			RaycastHit raycastHit = default(RaycastHit);
			float num = 50f;
			for (int i = 0; i < this.feet.Length; i++)
			{
				this.feet[i].go.layer = 2;
			}
			if (Physics.Raycast(position - b, -Vector3.up, out raycastHit, num, 4113))
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
			}
			else
			{
				base.SetShadowAlpha(0f);
			}
			for (int j = 0; j < this.feet.Length; j++)
			{
				this.feet[j].go.layer = 0;
			}
		}

		// Token: 0x06000B21 RID: 2849 RVA: 0x0005071F File Offset: 0x0004EB1F
		protected override Vector3 GetFeetCenter()
		{
			return this.goT.position - Vector3.up * 1.35f;
		}

		// Token: 0x06000B22 RID: 2850 RVA: 0x00050740 File Offset: 0x0004EB40
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
			return base.Explode(eInfo, args);
		}

		// Token: 0x06000B23 RID: 2851 RVA: 0x000507B8 File Offset: 0x0004EBB8
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

		// Token: 0x06000B24 RID: 2852 RVA: 0x000507F4 File Offset: 0x0004EBF4
		public override List<Collider> GetColliders()
		{
			List<Collider> colliders = base.GetColliders();
			foreach (FootInfo footInfo in this.feet)
			{
				colliders.Add(footInfo.go.GetComponent<Collider>());
			}
			foreach (GameObject gameObject in this.hands)
			{
				colliders.Add(gameObject.GetComponent<Collider>());
			}
			colliders.Add(this.middle.GetComponent<Collider>());
			return colliders;
		}

		// Token: 0x06000B25 RID: 2853 RVA: 0x00050880 File Offset: 0x0004EC80
		public override void Deactivate()
		{
			base.Deactivate();
			for (int i = 0; i < this.hands.Length; i++)
			{
				this.hands[i].SetActive(false);
			}
		}

		// Token: 0x040008A4 RID: 2212
		public GameObject[] hands = new GameObject[2];

		// Token: 0x040008A5 RID: 2213
		public GameObject middle;

		// Token: 0x040008A6 RID: 2214
		private ConfigurableJoint middleJoint;

		// Token: 0x040008A7 RID: 2215
		public BoxCollider collider;

		// Token: 0x040008A8 RID: 2216
		private Vector3 colliderSize;

		// Token: 0x040008A9 RID: 2217
		private Vector3 colliderCenter;

		// Token: 0x040008AA RID: 2218
		public Block bottom;

		// Token: 0x040008AB RID: 2219
		public Block front;

		// Token: 0x040008AC RID: 2220
		public Block left;

		// Token: 0x040008AD RID: 2221
		public Block right;

		// Token: 0x040008AE RID: 2222
		public Block back;

		// Token: 0x040008AF RID: 2223
		public List<Block> headAttachments;

		// Token: 0x040008B0 RID: 2224
		private float footHeight = -1.35f;

		// Token: 0x040008B1 RID: 2225
		private Vector3 handPos = new Vector3(0.5f, -0.5f, 0.4f);

		// Token: 0x040008B2 RID: 2226
		private float handsOutXMod = 0.7f;

		// Token: 0x040008B3 RID: 2227
		private float handsOutYMod = -0.1f;

		// Token: 0x040008B4 RID: 2228
		private Vector3 footPos = new Vector3(0.25f, -0.8f, 0.4f);

		// Token: 0x040008B5 RID: 2229
		private Vector3 footRot = new Vector3(270f, 0f, 0f);

		// Token: 0x040008B6 RID: 2230
		private Vector3 pausedVelocityHands0;

		// Token: 0x040008B7 RID: 2231
		private Vector3 pausedAngularVelocityHands0;

		// Token: 0x040008B8 RID: 2232
		private Vector3 pausedVelocityHands1;

		// Token: 0x040008B9 RID: 2233
		private Vector3 pausedAngularVelocityHands1;

		// Token: 0x040008BA RID: 2234
		private Vector3 pausedVelocityMiddle;

		// Token: 0x040008BB RID: 2235
		private Vector3 pausedAngularVelocityMiddle;

		// Token: 0x040008BC RID: 2236
		private Vector3 middleLocalPosition;

		// Token: 0x040008BD RID: 2237
		private bool hasLegMesh;

		// Token: 0x040008BE RID: 2238
		private bool canUseCharacterShadow;

		// Token: 0x040008BF RID: 2239
		public bool isAvatar;

		// Token: 0x040008C0 RID: 2240
		private bool lockPaintAndTexture;

		// Token: 0x040008C1 RID: 2241
		public static Predicate predicateCharacterMover;

		// Token: 0x040008C2 RID: 2242
		public static Predicate predicateCharacterTiltMover;

		// Token: 0x040008C3 RID: 2243
		public static Predicate predicateCharacterJump;

		// Token: 0x040008C4 RID: 2244
		public static Predicate predicateCharacterGotoTag;

		// Token: 0x040008C5 RID: 2245
		public static Predicate predicateCharacterChaseTag;

		// Token: 0x040008C6 RID: 2246
		public static Predicate predicateCharacterAvoidTag;

		// Token: 0x040008C7 RID: 2247
		private static int idxBottom;

		// Token: 0x040008C8 RID: 2248
		private static int idxFront;

		// Token: 0x040008C9 RID: 2249
		private static int idxLeft;

		// Token: 0x040008CA RID: 2250
		private static int idxRight;

		// Token: 0x040008CB RID: 2251
		private static int idxBack;

		// Token: 0x040008CC RID: 2252
		private static bool gotGlueIndices;

		// Token: 0x040008CD RID: 2253
		private static GameObject prefabCharacterShadow;

		// Token: 0x040008CE RID: 2254
		private Dictionary<GameObject, BoxColliderData> boxColliderData = new Dictionary<GameObject, BoxColliderData>();
	}
}
