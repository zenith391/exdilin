using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x020000DF RID: 223
	public class BlockTeleportVolumeBlock : BlockGrouped
	{
		// Token: 0x060010BC RID: 4284 RVA: 0x00074C50 File Offset: 0x00073050
		public BlockTeleportVolumeBlock(List<List<Tile>> tiles) : base(tiles)
		{
		}

		// Token: 0x060010BD RID: 4285 RVA: 0x00074C64 File Offset: 0x00073064
		public new static void Register()
		{
			PredicateRegistry.Add<BlockTeleportVolumeBlock>("TeleportVolume.Outlet", null, (Block b) => new PredicateActionDelegate(((BlockTeleportVolumeBlock)b).SetOutlet), new Type[]
			{
				typeof(int)
			}, null, null);
			PredicateRegistry.Add<BlockTeleportVolumeBlock>("TeleportVolume.Teleport", null, (Block b) => new PredicateActionDelegate(((BlockTeleportVolumeBlock)b).DoTeleport), null, null, null);
			PredicateRegistry.Add<BlockTeleportVolumeBlock>("TeleportVolume.TeleportTag", null, (Block b) => new PredicateActionDelegate(((BlockTeleportVolumeBlock)b).DoTeleportTag), new Type[]
			{
				typeof(string)
			}, null, null);
		}

		// Token: 0x060010BE RID: 4286 RVA: 0x00074D19 File Offset: 0x00073119
		public override void SetBlockGroup(BlockGroup blockGroup)
		{
			base.SetBlockGroup(blockGroup);
			if (blockGroup is TeleportVolumeBlockGroup)
			{
				this.group = blockGroup;
			}
		}

		// Token: 0x060010BF RID: 4287 RVA: 0x00074D34 File Offset: 0x00073134
		public override bool BlockUsesDefaultPaintsAndTextures()
		{
			return false;
		}

		// Token: 0x060010C0 RID: 4288 RVA: 0x00074D37 File Offset: 0x00073137
		public override bool GroupHasIndividualSripting()
		{
			return true;
		}

		// Token: 0x060010C1 RID: 4289 RVA: 0x00074D3A File Offset: 0x0007313A
		public override bool GroupRotateMainBlockOnPlacement()
		{
			return false;
		}

		// Token: 0x060010C2 RID: 4290 RVA: 0x00074D3D File Offset: 0x0007313D
		public override TileResultCode PaintTo(string paint, bool permanent, int meshIndex = 0)
		{
			if (Blocksworld.CurrentState == State.Play || Blocksworld.resetting)
			{
				return base.PaintTo(paint, permanent, meshIndex);
			}
			return TileResultCode.True;
		}

		// Token: 0x060010C3 RID: 4291 RVA: 0x00074D60 File Offset: 0x00073160
		public override void Play()
		{
			base.Play();
			this._isOutlet = false;
			this._teleportActive = false;
			this._teleportActiveTags.Clear();
			Block[] blocks = this.group.GetBlocks();
			for (int i = 0; i < blocks.Length; i++)
			{
				Block block = blocks[i];
				if (this != block)
				{
					this._otherVolume = (BlockTeleportVolumeBlock)blocks[i];
				}
			}
			this.HideAndMakeTrigger();
			this.IgnoreRaycasts(true);
		}

		// Token: 0x060010C4 RID: 4292 RVA: 0x00074DD2 File Offset: 0x000731D2
		public override void Pause()
		{
		}

		// Token: 0x060010C5 RID: 4293 RVA: 0x00074DD4 File Offset: 0x000731D4
		public override void Resume()
		{
		}

		// Token: 0x060010C6 RID: 4294 RVA: 0x00074DD6 File Offset: 0x000731D6
		public override void ResetFrame()
		{
		}

		// Token: 0x060010C7 RID: 4295 RVA: 0x00074DD8 File Offset: 0x000731D8
		public override void Stop(bool resetBlock = true)
		{
			base.Stop(resetBlock);
			this.UpdateStopVisibility();
			this._isOutlet = false;
			this._teleportActive = false;
			this._teleportActiveTags.Clear();
			this._otherVolume = null;
		}

		// Token: 0x060010C8 RID: 4296 RVA: 0x00074E07 File Offset: 0x00073207
		public override void OnCreate()
		{
			base.OnCreate();
			this.UpdateStopVisibility();
			this.TextureTo("Volume", base.GetTextureNormal(), true, 0, true);
		}

		// Token: 0x060010C9 RID: 4297 RVA: 0x00074E2A File Offset: 0x0007322A
		public TileResultCode SetOutlet(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this._isOutlet = (Util.GetIntArg(args, 0, 1) != 0);
			return TileResultCode.True;
		}

		// Token: 0x060010CA RID: 4298 RVA: 0x00074E41 File Offset: 0x00073241
		public TileResultCode DoTeleport(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this._teleportActive = true;
			return TileResultCode.True;
		}

		// Token: 0x060010CB RID: 4299 RVA: 0x00074E4B File Offset: 0x0007324B
		public TileResultCode DoTeleportTag(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this._teleportActiveTags.Add(Util.GetStringArg(args, 0, string.Empty));
			return TileResultCode.True;
		}

		// Token: 0x060010CC RID: 4300 RVA: 0x00074E68 File Offset: 0x00073268
		private static Vector3 ComputePositionFromNormalizedPosition(BoxCollider collider, Vector3 normalizedPosition)
		{
			Vector3 vector = Vector3.Scale(Vector3.Scale(collider.transform.localScale, collider.size), normalizedPosition);
			vector = collider.transform.rotation * vector;
			return collider.transform.position + vector;
		}

		// Token: 0x060010CD RID: 4301 RVA: 0x00074EB8 File Offset: 0x000732B8
		private static Vector3 GetNormalizedPositionFromVolume(BoxCollider collider, Vector3 worldPosition)
		{
			Vector3 vector = worldPosition - collider.transform.position;
			vector = Quaternion.Inverse(collider.transform.rotation) * vector;
			Vector3 vector2 = Vector3.Scale(collider.transform.localScale, collider.size);
			return Vector3.Scale(vector, new Vector3(1f / vector2.x, 1f / vector2.y, 1f / vector2.z));
		}

		// Token: 0x060010CE RID: 4302 RVA: 0x00074F3C File Offset: 0x0007333C
		private static Quaternion GetRotationToOtherVolume(BoxCollider fromCollider, BoxCollider toCollider)
		{
			Quaternion identity = Quaternion.identity;
			identity.SetFromToRotation(-fromCollider.transform.forward, toCollider.transform.forward);
			return identity;
		}

		// Token: 0x060010CF RID: 4303 RVA: 0x00074F74 File Offset: 0x00073374
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			bool flag = this._teleportActive || this._teleportActiveTags.Count > 0;
			if (flag && this._otherVolume._isOutlet)
			{
				HashSet<GameObject> triggeringBlocks = CollisionManager.GetTriggeringBlocks(this);
				if (triggeringBlocks != null)
				{
					Collider component = this.go.GetComponent<Collider>();
					Bounds bounds = component.bounds;
					HashSet<GameObject> hashSet = new HashSet<GameObject>();
					foreach (GameObject gameObject in triggeringBlocks)
					{
						if (!(gameObject == null))
						{
							if (!this._teleportActive)
							{
								bool flag2 = false;
								for (int i = 0; i < gameObject.transform.childCount; i++)
								{
									Block block = BWSceneManager.FindBlock(gameObject.transform.GetChild(i).gameObject, false);
									if (block != null && TagManager.blockTags.ContainsKey(block))
									{
										flag2 |= this._teleportActiveTags.Overlaps(TagManager.blockTags[block]);
									}
								}
								if (!flag2)
								{
									continue;
								}
							}
							Rigidbody component2 = gameObject.GetComponent<Rigidbody>();
							if (gameObject.transform.childCount > 0)
							{
								Block block2 = BWSceneManager.FindBlock(gameObject.transform.GetChild(0).gameObject, false);
								if (block2 != null)
								{
									BlockMissile blockMissile = block2 as BlockMissile;
									if (blockMissile == null || blockMissile.GetLaunchedMissile() == null)
									{
										if (Block.connectedChunks[block2].Count > 1)
										{
											continue;
										}
									}
								}
							}
							BoxCollider boxCollider = component as BoxCollider;
							BoxCollider component3 = this._otherVolume.go.GetComponent<BoxCollider>();
							Quaternion rotationToOtherVolume = BlockTeleportVolumeBlock.GetRotationToOtherVolume(boxCollider, component3);
							Vector3 normalizedPositionFromVolume = BlockTeleportVolumeBlock.GetNormalizedPositionFromVolume(boxCollider, component2.worldCenterOfMass);
							Vector3 position = BlockTeleportVolumeBlock.ComputePositionFromNormalizedPosition(component3, normalizedPositionFromVolume);
							component2.position = position;
							component2.rotation = rotationToOtherVolume * component2.rotation;
							component2.velocity = rotationToOtherVolume * component2.velocity;
							component2.angularVelocity = rotationToOtherVolume * component2.angularVelocity;
							hashSet.Add(gameObject);
							this._otherVolume.PlayPositionedSound("Teleport_Quick", 1f, 1f);
						}
					}
					foreach (GameObject gameObject2 in hashSet)
					{
						triggeringBlocks.Remove(gameObject2);
						for (int j = 0; j < gameObject2.transform.childCount; j++)
						{
							Block block3 = BWSceneManager.FindBlock(gameObject2.transform.GetChild(j).gameObject, false);
							if (block3 != null)
							{
								block3.Teleported(false, false, false);
								if (Blocksworld.worldOceanBlock != null && !Blocksworld.worldOceanBlock.SimulatesBlock(block3))
								{
									Blocksworld.worldOceanBlock.AddBlockToSimulation(block3);
								}
								Blocksworld.blocksworldCamera.HandleTeleport(block3);
							}
						}
					}
				}
			}
			this._teleportActive = false;
			this._teleportActiveTags.Clear();
		}

		// Token: 0x060010D0 RID: 4304 RVA: 0x000752DC File Offset: 0x000736DC
		public void HideAndMakeTrigger()
		{
			if (this.go.GetComponent<Collider>() != null)
			{
				this.go.GetComponent<Collider>().isTrigger = true;
				this.go.GetComponent<Collider>().enabled = true;
			}
			this.go.GetComponent<Renderer>().enabled = false;
			if (this.goShadow != null)
			{
				this.goShadow.GetComponent<Renderer>().enabled = false;
			}
		}

		// Token: 0x060010D1 RID: 4305 RVA: 0x00075354 File Offset: 0x00073754
		public void ShowAndRemoveIsTrigger()
		{
			if (this.go.GetComponent<Collider>() != null)
			{
				this.go.GetComponent<Collider>().isTrigger = false;
			}
			if (this.go.GetComponent<Renderer>() != null)
			{
				this.go.GetComponent<Renderer>().enabled = true;
			}
			if (this.goShadow != null)
			{
				this.goShadow.GetComponent<Renderer>().enabled = true;
			}
		}

		// Token: 0x060010D2 RID: 4306 RVA: 0x000753D1 File Offset: 0x000737D1
		public override TileResultCode TextureTo(string texture, Vector3 normal, bool permanent, int meshIndex = 0, bool force = false)
		{
			if (Blocksworld.CurrentState != State.Play)
			{
				texture = "Volume";
			}
			return base.TextureTo(texture, normal, permanent, meshIndex, true);
		}

		// Token: 0x060010D3 RID: 4307 RVA: 0x000753F1 File Offset: 0x000737F1
		public void UpdateStopVisibility()
		{
			if (Tutorial.state == TutorialState.None)
			{
				this.ShowAndRemoveIsTrigger();
				this.IgnoreRaycasts(false);
			}
			else
			{
				this.HideAndMakeTrigger();
			}
		}

		// Token: 0x060010D4 RID: 4308 RVA: 0x00075415 File Offset: 0x00073815
		public override bool VisibleInPlayMode()
		{
			return false;
		}

		// Token: 0x060010D5 RID: 4309 RVA: 0x00075418 File Offset: 0x00073818
		public override bool ColliderIsTriggerInPlayMode()
		{
			return true;
		}

		// Token: 0x060010D6 RID: 4310 RVA: 0x0007541B File Offset: 0x0007381B
		protected override void UpdateBlockPropertiesForTextureAssignment(int meshIndex, bool forceEnabled)
		{
		}

		// Token: 0x060010D7 RID: 4311 RVA: 0x0007541D File Offset: 0x0007381D
		public override bool IsRuntimeInvisible()
		{
			return true;
		}

		// Token: 0x04000D3E RID: 3390
		private BlockTeleportVolumeBlock _otherVolume;

		// Token: 0x04000D3F RID: 3391
		private bool _teleportActive;

		// Token: 0x04000D40 RID: 3392
		private bool _isOutlet;

		// Token: 0x04000D41 RID: 3393
		private HashSet<string> _teleportActiveTags = new HashSet<string>();
	}
}
