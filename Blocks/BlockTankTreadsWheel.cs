using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Blocks
{
	// Token: 0x020000D9 RID: 217
	public class BlockTankTreadsWheel : BlockGrouped, INoCollisionSound, IHelpForceGiver
	{
		// Token: 0x0600103B RID: 4155 RVA: 0x0006DDBC File Offset: 0x0006C1BC
		public BlockTankTreadsWheel(List<List<Tile>> tiles) : base(tiles)
		{
			this.loopName = "Tank Tread Run Loop";
			this.go.GetComponent<Renderer>().enabled = false;
			this.hideAxleN = this.goT.Find("Treads X N").gameObject;
			this.hideAxleP = this.goT.Find("Treads X P").gameObject;
			this.hideAxleN.GetComponent<Renderer>().enabled = false;
			this.hideAxleP.GetComponent<Renderer>().enabled = false;
		}

		// Token: 0x0600103C RID: 4156 RVA: 0x0006DED8 File Offset: 0x0006C2D8
		public new static void Register()
		{
			BlockTankTreadsWheel.predicateTankTreadsDrive = PredicateRegistry.Add<BlockTankTreadsWheel>("TankTreads.Drive", (Block b) => new PredicateSensorDelegate(((BlockTankTreadsWheel)b).IsDrivingSensor), (Block b) => new PredicateActionDelegate(((BlockTankTreadsWheel)b).Drive), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Force"
			}, null);
			BlockTankTreadsWheel.predicateTankTreadsTurnAlongAnalogStick = PredicateRegistry.Add<BlockTankTreadsWheel>("TankTreads.TurnAlongAnalogStick", null, (Block b) => new PredicateActionDelegate(((BlockTankTreadsWheel)b).TurnAlongAnalogStick), new Type[]
			{
				typeof(string),
				typeof(float)
			}, new string[]
			{
				"Stick name",
				"Force"
			}, null);
			BlockTankTreadsWheel.predicateTankTreadsDriveAlongAnalogStick = PredicateRegistry.Add<BlockTankTreadsWheel>("TankTreads.DriveAlongAnalogStick", null, (Block b) => new PredicateActionDelegate(((BlockTankTreadsWheel)b).DriveAlongAnalogStick), new Type[]
			{
				typeof(string),
				typeof(float)
			}, new string[]
			{
				"Stick name",
				"Force"
			}, null);
			BlockTankTreadsWheel.predicateTankTreadsAnalogStickControl = PredicateRegistry.Add<BlockTankTreadsWheel>("TankTreads.AnalogStickControl", null, (Block b) => new PredicateActionDelegate(((BlockTankTreadsWheel)b).AnalogStickControl), new Type[]
			{
				typeof(string),
				typeof(float)
			}, new string[]
			{
				"Stick name",
				"Force"
			}, null);
		}

		// Token: 0x0600103D RID: 4157 RVA: 0x0006E07C File Offset: 0x0006C47C
		public override bool HasDefaultScript(List<List<Tile>> tilesToUse = null)
		{
			bool flag = base.HasDefaultScript(tilesToUse);
			if (!flag)
			{
				if (tilesToUse == null)
				{
					tilesToUse = this.tiles;
				}
				return tilesToUse.Count == 2 && tilesToUse[1].Count == 1;
			}
			return flag;
		}

		// Token: 0x0600103E RID: 4158 RVA: 0x0006E0C8 File Offset: 0x0006C4C8
		private void SetTreadsVisible(bool v)
		{
			for (int i = 0; i < this.treadsInfo.links.Count; i++)
			{
				BlockTankTreadsWheel.TreadLink treadLink = this.treadsInfo.links[i];
				if (treadLink.visualGo != null)
				{
					treadLink.visualGo.GetComponent<Renderer>().enabled = v;
				}
				if (treadLink.collisionGo != null)
				{
					treadLink.collisionGo.GetComponent<Collider>().isTrigger = !v;
				}
			}
			foreach (BlockTankTreadsWheel.TankTreadsSupportWheel tankTreadsSupportWheel in this.supportWheels)
			{
				if (tankTreadsSupportWheel.go != null)
				{
					Collider component = tankTreadsSupportWheel.go.GetComponent<Collider>();
					if (component != null)
					{
						component.isTrigger = !v;
					}
				}
			}
		}

		// Token: 0x0600103F RID: 4159 RVA: 0x0006E1D0 File Offset: 0x0006C5D0
		public override void Appeared()
		{
			base.Appeared();
			if (base.IsMainBlockInGroup())
			{
				this.SetTreadsVisible(true);
			}
			BlockTankTreadsWheel blockTankTreadsWheel = base.GetMainBlockInGroup() as BlockTankTreadsWheel;
			if (blockTankTreadsWheel != null)
			{
				blockTankTreadsWheel.ignoreCollisionsDirty = true;
			}
		}

		// Token: 0x06001040 RID: 4160 RVA: 0x0006E20E File Offset: 0x0006C60E
		public override void Vanished()
		{
			base.Vanished();
			if (base.IsMainBlockInGroup())
			{
				this.SetTreadsVisible(false);
			}
		}

		// Token: 0x06001041 RID: 4161 RVA: 0x0006E228 File Offset: 0x0006C628
		public override Vector3 GetWaterForce(float fractionWithin, Vector3 relativeVelocity, BlockAbstractWater water)
		{
			if (this.treadsInfo.mode == BlockTankTreadsWheel.TreadsMode.Drive && Mathf.Abs(this.treadsInfo.speed) > 0.05f && this.treadsInfo.appliedWaterForceTime < Time.fixedTime)
			{
				Bounds waterBounds = water.GetWaterBounds();
				Vector3 vector = Vector3.zero;
				for (int i = 0; i < this.treadsInfo.physicalLinks.Count; i++)
				{
					BlockTankTreadsWheel.TreadLink treadLink = this.treadsInfo.physicalLinks[i];
					Vector3 normalized = (treadLink.toTransform.position - treadLink.fromTransform.position).normalized;
					Vector3 a = this.treadsInfo.speed * normalized;
					Vector3 point = 0.5f * (treadLink.fromTransform.position + treadLink.toTransform.position);
					if (waterBounds.Contains(point))
					{
						vector -= (a + relativeVelocity) * treadLink.volume;
					}
				}
				this.treadsInfo.appliedWaterForceTime = Time.fixedTime;
				return vector;
			}
			return Vector3.zero;
		}

		// Token: 0x06001042 RID: 4162 RVA: 0x0006E354 File Offset: 0x0006C754
		public TileResultCode AnalogStickControl(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.externalControlBlock == null)
			{
				string stringArg = Util.GetStringArg(args, 0, "L");
				float maxSpeed = eInfo.floatArg * Util.GetFloatArg(args, 1, 5f);
				this.AnalogStickControl(stringArg, maxSpeed);
			}
			return TileResultCode.True;
		}

		// Token: 0x06001043 RID: 4163 RVA: 0x0006E398 File Offset: 0x0006C798
		public void DriveAlongAnalogStick(string stickName, float maxSpeed)
		{
			if (this.broken || this.chassis == null)
			{
				return;
			}
			Blocksworld.UI.Controls.EnableDPad(stickName, MoverDirectionMask.ALL);
			Vector3 vector = Blocksworld.UI.Controls.GetWorldDPadOffset(stickName);
			vector = Util.ProjectOntoPlane(vector, Vector3.up);
			float magnitude = vector.magnitude;
			if (magnitude < 0.01f)
			{
				return;
			}
			this.engineLoopOn = !this.broken;
			this.idleEngineCounter = 0;
			this.engineIncreasingPitch = true;
			Vector3 up = this.chassis.transform.up;
			Vector3 lhs = Vector3.Cross(this.goT.right, up);
			float num = maxSpeed * Vector3.Dot(lhs, vector);
			this.treadsInfo.driveSpeedTarget += num;
			this.treadsInfo.mode = BlockTankTreadsWheel.TreadsMode.Drive;
		}

		// Token: 0x06001044 RID: 4164 RVA: 0x0006E470 File Offset: 0x0006C870
		public void AnalogStickControl(string stickName, float maxSpeed)
		{
			if (this.broken || this.chassis == null)
			{
				return;
			}
			Blocksworld.UI.Controls.EnableDPad(stickName, MoverDirectionMask.ALL);
			Vector3 vector = Blocksworld.UI.Controls.GetWorldDPadOffset(stickName);
			vector = Util.ProjectOntoPlane(vector, Vector3.up);
			float magnitude = vector.magnitude;
			if (magnitude < 0.01f)
			{
				return;
			}
			this.engineLoopOn = !this.broken;
			this.idleEngineCounter = 0;
			this.engineIncreasingPitch = true;
			Transform goT = this.goT;
			Vector3 position = goT.position;
			Vector3 up = this.chassis.transform.up;
			Vector3 vector2 = Vector3.Cross(goT.right, up);
			this.camHintDir += vector;
			float num = Mathf.Clamp(1f - Vector3.Dot(vector2, vector), 0f, 1f);
			float num2 = magnitude * Mathf.Min(15f * num, maxSpeed);
			base.UpdateConnectedCache();
			HashSet<Chunk> hashSet = Block.connectedChunks[this];
			Vector3 vector3 = Vector3.zero;
			int num3 = 0;
			foreach (Chunk chunk in hashSet)
			{
				if (chunk.go != null)
				{
					vector3 += chunk.go.transform.position;
					num3++;
				}
			}
			if (num3 > 0)
			{
				float num4 = num2 * Mathf.Sign(Vector3.Dot(up, Vector3.Cross(vector2, vector)));
				vector3 /= (float)num3;
				Vector3 lhs = position - vector3;
				float num5 = Mathf.Sign(Vector3.Dot(Vector3.up, Vector3.Cross(lhs, vector2)));
				this.treadsInfo.driveSpeedTarget += num5 * num4;
			}
			float num6 = maxSpeed * Vector3.Dot(vector2, vector);
			this.treadsInfo.driveSpeedTarget += num6;
			this.treadsInfo.mode = BlockTankTreadsWheel.TreadsMode.Drive;
		}

		// Token: 0x06001045 RID: 4165 RVA: 0x0006E698 File Offset: 0x0006CA98
		public void TurnAlongAnalogStick(string stickName, float maxSpeed)
		{
			if (this.broken || this.chassis == null)
			{
				return;
			}
			Blocksworld.UI.Controls.EnableDPad(stickName, MoverDirectionMask.ALL);
			Vector3 vector = Blocksworld.UI.Controls.GetWorldDPadOffset(stickName);
			float magnitude = vector.magnitude;
			vector = Util.ProjectOntoPlane(vector, Vector3.up);
			if (vector.magnitude < 0.01f)
			{
				return;
			}
			this.engineLoopOn = !this.broken;
			this.idleEngineCounter = 0;
			this.engineIncreasingPitch = true;
			Vector3 up = this.chassis.transform.up;
			Vector3 vector2 = Vector3.Cross(this.goT.right, up);
			float num = Mathf.Clamp(1f - Vector3.Dot(vector2, vector), 0f, 1f);
			float num2 = magnitude * Mathf.Min(15f * num, maxSpeed);
			float num3 = num2 * Mathf.Sign(Vector3.Dot(up, Vector3.Cross(vector2, vector)));
			Transform goT = this.goT;
			Vector3 position = goT.position;
			base.UpdateConnectedCache();
			HashSet<Chunk> hashSet = Block.connectedChunks[this];
			Vector3 vector3 = Vector3.zero;
			int num4 = 0;
			foreach (Chunk chunk in hashSet)
			{
				if (chunk.go != null)
				{
					vector3 += chunk.go.transform.position;
					num4++;
				}
			}
			if (num4 == 0)
			{
				return;
			}
			vector3 /= (float)num4;
			Vector3 vector4 = position - vector3;
			float num5 = Mathf.Sign(Vector3.Dot(Vector3.up, Vector3.Cross(vector4.normalized, vector2)));
			this.treadsInfo.driveSpeedTarget += num5 * num3;
			this.treadsInfo.mode = BlockTankTreadsWheel.TreadsMode.Drive;
		}

		// Token: 0x06001046 RID: 4166 RVA: 0x0006E89C File Offset: 0x0006CC9C
		public TileResultCode DriveAlongAnalogStick(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.externalControlBlock == null)
			{
				string stringArg = Util.GetStringArg(args, 0, "L");
				float maxSpeed = eInfo.floatArg * Util.GetFloatArg(args, 1, 5f);
				this.DriveAlongAnalogStick(stringArg, maxSpeed);
			}
			return TileResultCode.True;
		}

		// Token: 0x06001047 RID: 4167 RVA: 0x0006E8E0 File Offset: 0x0006CCE0
		public override void ChunksAndJointsModified(Dictionary<Joint, Joint> oldToNew, Dictionary<Chunk, Chunk> oldToNewChunks, Dictionary<Chunk, Chunk> newToOldChunks)
		{
			if (base.IsMainBlockInGroup())
			{
				if (this.broken || this.isTreasure)
				{
					return;
				}
				foreach (BlockTankTreadsWheel blockTankTreadsWheel in this.groupBlocks)
				{
					for (int i = 0; i < blockTankTreadsWheel.joints.Count; i++)
					{
						ConfigurableJoint configurableJoint = blockTankTreadsWheel.joints[i];
						Joint joint;
						if (configurableJoint != null && oldToNew.TryGetValue(configurableJoint, out joint))
						{
							configurableJoint = (ConfigurableJoint)joint;
							blockTankTreadsWheel.joints[i] = configurableJoint;
							blockTankTreadsWheel.chassis = configurableJoint.gameObject;
						}
					}
				}
			}
		}

		// Token: 0x06001048 RID: 4168 RVA: 0x0006E9BC File Offset: 0x0006CDBC
		public TileResultCode TurnAlongAnalogStick(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.externalControlBlock == null)
			{
				string stringArg = Util.GetStringArg(args, 0, "L");
				float maxSpeed = eInfo.floatArg * Util.GetFloatArg(args, 1, 5f);
				this.TurnAlongAnalogStick(stringArg, maxSpeed);
			}
			return TileResultCode.True;
		}

		// Token: 0x06001049 RID: 4169 RVA: 0x0006EA00 File Offset: 0x0006CE00
		public override TileResultCode IsTapHoldingBlock(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (Block.goTouched != null && base.IsMainBlockInGroup())
			{
				foreach (BlockTankTreadsWheel blockTankTreadsWheel in this.groupBlocks)
				{
					if (Block.goTouched == blockTankTreadsWheel.go)
					{
						Blocksworld.worldSessionHadBlockTap = true;
						return TileResultCode.True;
					}
				}
				for (int i = 0; i < this.treadsInfo.links.Count; i++)
				{
					BlockTankTreadsWheel.TreadLink treadLink = this.treadsInfo.links[i];
					if (treadLink.collisionGo == Block.goTouched)
					{
						Blocksworld.worldSessionHadBlockTap = true;
						return TileResultCode.True;
					}
				}
				return TileResultCode.False;
			}
			return TileResultCode.False;
		}

		// Token: 0x0600104A RID: 4170 RVA: 0x0006EAE8 File Offset: 0x0006CEE8
		public override void RemoveBlockMaps()
		{
			base.RemoveBlockMaps();
			if (base.IsMainBlockInGroup())
			{
				this.DestroyTreads();
			}
		}

		// Token: 0x0600104B RID: 4171 RVA: 0x0006EB04 File Offset: 0x0006CF04
		private void RemoveTreadsBlockMaps()
		{
			foreach (BlockTankTreadsWheel.TankTreadsSupportWheel tankTreadsSupportWheel in this.supportWheels)
			{
				if (tankTreadsSupportWheel.go != null)
				{
					BWSceneManager.RemoveChildBlockInstanceID(tankTreadsSupportWheel.go);
				}
			}
			foreach (BlockTankTreadsWheel.TreadLink treadLink in this.treadsInfo.physicalLinks)
			{
				BWSceneManager.RemoveChildBlockInstanceID(treadLink.collisionGo);
			}
		}

		// Token: 0x0600104C RID: 4172 RVA: 0x0006EBCC File Offset: 0x0006CFCC
		public override void IgnoreRaycasts(bool value)
		{
			Layer layer = (!value) ? Layer.Default : Layer.IgnoreRaycast;
			if (base.IsMainBlockInGroup())
			{
				foreach (BlockTankTreadsWheel.TreadLink treadLink in this.treadsInfo.physicalLinks)
				{
					treadLink.collisionGo.SetLayer(layer, true);
				}
				foreach (BlockTankTreadsWheel.TankTreadsSupportWheel tankTreadsSupportWheel in this.supportWheels)
				{
					if (tankTreadsSupportWheel.go != null)
					{
						tankTreadsSupportWheel.go.SetLayer(layer, true);
					}
				}
				if (this.buildModeColliderGo != null)
				{
					this.buildModeColliderGo.SetLayer(layer, true);
				}
			}
			if (this.group != null)
			{
				foreach (Block block in this.group.GetBlocks())
				{
					block.go.SetLayer(layer, true);
				}
			}
			else
			{
				base.IgnoreRaycasts(value);
			}
		}

		// Token: 0x0600104D RID: 4173 RVA: 0x0006ED20 File Offset: 0x0006D120
		private void SetTreadMaterial()
		{
			if (this.treadMaterial != null)
			{
				UnityEngine.Object.Destroy(this.treadMaterial);
			}
			Material sharedMaterial = this.go.GetComponent<Renderer>().sharedMaterial;
			this.treadMaterial = new Material(sharedMaterial);
			bool flag = false;
			Mapping mapping = Materials.GetMapping(base.GetTexture(0));
			float num = 1f;
			if (mapping == Mapping.AllSidesTo4x1)
			{
				num = 0.25f;
			}
			if (this.treadsInfo.uValue != num)
			{
				flag = true;
				this.treadsInfo.uValue = num;
			}
			for (int i = 0; i < this.treadsInfo.links.Count; i++)
			{
				BlockTankTreadsWheel.TreadLink treadLink = this.treadsInfo.links[i];
				GameObject visualGo = treadLink.visualGo;
				if (visualGo != null)
				{
					visualGo.GetComponent<Renderer>().sharedMaterial = this.treadMaterial;
					if (flag)
					{
						MeshFilter component = visualGo.GetComponent<MeshFilter>();
						Mesh mesh = component.mesh;
						Vector2[] uv = mesh.uv;
						for (int j = 0; j < uv.Length; j++)
						{
							float num2 = uv[j][0];
							if (num2 > 0.01f)
							{
								uv[j][0] = num;
							}
						}
						mesh.uv = uv;
					}
				}
			}
		}

		// Token: 0x0600104E RID: 4174 RVA: 0x0006EE78 File Offset: 0x0006D278
		public override bool ScaleTo(Vector3 scale, bool recalculateCollider = true, bool forceRescale = false)
		{
			bool result = base.ScaleTo(scale, recalculateCollider, forceRescale);
			this.go.GetComponent<Renderer>().enabled = false;
			this.axlesDirty = true;
			return result;
		}

		// Token: 0x0600104F RID: 4175 RVA: 0x0006EEA8 File Offset: 0x0006D2A8
		public void TextureToBase(string texture, Vector3 normal, bool permanent, int meshIndex = 0, bool force = false)
		{
			base.TextureTo(texture, normal, permanent, meshIndex, force);
			if (meshIndex == 0)
			{
				this.isInvisible = (texture == "Invisible");
				this.go.GetComponent<Renderer>().enabled = false;
			}
		}

		// Token: 0x06001050 RID: 4176 RVA: 0x0006EEE4 File Offset: 0x0006D2E4
		public override TileResultCode TextureTo(string texture, Vector3 normal, bool permanent, int meshIndex = 0, bool force = false)
		{
			if (this.group == null)
			{
				TileResultCode result = base.TextureTo(texture, normal, permanent, meshIndex, force);
				this.go.GetComponent<Renderer>().enabled = false;
				return result;
			}
			foreach (Block block in this.group.GetBlocks())
			{
				BlockTankTreadsWheel blockTankTreadsWheel = block as BlockTankTreadsWheel;
				if (blockTankTreadsWheel != null)
				{
					blockTankTreadsWheel.TextureToBase(texture, normal, permanent, meshIndex, force);
				}
			}
			if (meshIndex == 0)
			{
				BlockTankTreadsWheel blockTankTreadsWheel2 = base.GetMainBlockInGroup() as BlockTankTreadsWheel;
				this.isInvisible = (texture == "Invisible");
				this.ToggleTreadVisibility(false);
				blockTankTreadsWheel2.SetTreadMaterial();
				blockTankTreadsWheel2.go.GetComponent<Renderer>().enabled = false;
				this.go.GetComponent<Renderer>().enabled = false;
			}
			return TileResultCode.True;
		}

		// Token: 0x06001051 RID: 4177 RVA: 0x0006EFB8 File Offset: 0x0006D3B8
		private void ToggleTreadVisibility(bool skipPlay = false)
		{
			if (this.groupBlocks == null || (Blocksworld.CurrentState != State.Play && !skipPlay))
			{
				return;
			}
			for (int i = 0; i < this.groupBlocks.Count; i++)
			{
				BlockTankTreadsWheel blockTankTreadsWheel = this.groupBlocks[i];
				List<Renderer> treadRenders = blockTankTreadsWheel.treadsInfo.TreadRenders;
				for (int j = 0; j < treadRenders.Count; j++)
				{
					treadRenders[j].enabled = !this.isInvisible;
				}
			}
		}

		// Token: 0x06001052 RID: 4178 RVA: 0x0006F043 File Offset: 0x0006D443
		public void PaintToBase(string paint, bool permanent, int meshIndex = 0)
		{
			base.PaintTo(paint, permanent, meshIndex);
			if (meshIndex == 0)
			{
				this.go.GetComponent<Renderer>().enabled = false;
			}
		}

		// Token: 0x06001053 RID: 4179 RVA: 0x0006F068 File Offset: 0x0006D468
		public override TileResultCode PaintTo(string paint, bool permanent, int meshIndex = 0)
		{
			if (this.group == null)
			{
				TileResultCode result = base.PaintTo(paint, permanent, meshIndex);
				this.go.GetComponent<Renderer>().enabled = false;
				return result;
			}
			foreach (Block block in this.group.GetBlocks())
			{
				BlockTankTreadsWheel blockTankTreadsWheel = block as BlockTankTreadsWheel;
				if (blockTankTreadsWheel != null)
				{
					blockTankTreadsWheel.PaintToBase(paint, permanent, meshIndex);
				}
			}
			if (meshIndex == 0)
			{
				BlockTankTreadsWheel blockTankTreadsWheel2 = base.GetMainBlockInGroup() as BlockTankTreadsWheel;
				blockTankTreadsWheel2.SetTreadMaterial();
				blockTankTreadsWheel2.go.GetComponent<Renderer>().enabled = false;
				this.go.GetComponent<Renderer>().enabled = false;
			}
			return TileResultCode.True;
		}

		// Token: 0x06001054 RID: 4180 RVA: 0x0006F118 File Offset: 0x0006D518
		public override void SetBlockGroup(BlockGroup group)
		{
			base.SetBlockGroup(group);
			if (!(group is TankTreadsBlockGroup))
			{
				return;
			}
			this.groupBlocks = new List<BlockTankTreadsWheel>();
			foreach (Block block in group.GetBlocks())
			{
				this.groupBlocks.Add((BlockTankTreadsWheel)block);
			}
			if (base.IsMainBlockInGroup())
			{
				this.treadsInfo = new BlockTankTreadsWheel.TreadsInfo();
				for (int j = 0; j < this.groupBlocks.Count; j++)
				{
					BlockTankTreadsWheel blockTankTreadsWheel = this.groupBlocks[j];
					blockTankTreadsWheel.treadsInfo = this.treadsInfo;
				}
			}
			else if (this.tiles.Count > 2)
			{
				BWLog.Info("Too many tiles on non-main tank treads wheel");
				for (int k = this.tiles.Count - 1; k >= 2; k--)
				{
					this.tiles.RemoveAt(k);
				}
				this.tiles[1] = Block.EmptyTileRow();
			}
		}

		// Token: 0x06001055 RID: 4181 RVA: 0x0006F220 File Offset: 0x0006D620
		public override void Destroy()
		{
			base.Destroy();
			this.DestroyTreads();
		}

		// Token: 0x06001056 RID: 4182 RVA: 0x0006F230 File Offset: 0x0006D630
		private void DestroyBuildModeCollider()
		{
			if (this.buildModeColliderGo != null)
			{
				BWSceneManager.RemoveChildBlockInstanceID(this.buildModeColliderGo);
				MeshCollider component = this.buildModeColliderGo.GetComponent<MeshCollider>();
				UnityEngine.Object.Destroy(component.sharedMesh);
				UnityEngine.Object.Destroy(this.buildModeColliderGo);
				this.buildModeColliderGo = null;
			}
		}

		// Token: 0x06001057 RID: 4183 RVA: 0x0006F284 File Offset: 0x0006D684
		private void DestroyTreads()
		{
			if (base.IsMainBlockInGroup())
			{
				this.RemoveTreadsBlockMaps();
				foreach (BlockTankTreadsWheel.TankTreadsSupportWheel tankTreadsSupportWheel in this.supportWheels)
				{
					tankTreadsSupportWheel.Destroy();
				}
				foreach (BlockTankTreadsWheel.TreadLink treadLink in this.treadsInfo.links)
				{
					treadLink.Destroy();
				}
				this.supportWheels.Clear();
				this.treadsInfo.links.Clear();
				this.treadsInfo.physicalLinks.Clear();
				this.treadsInfo.gameObjectToTreadLink.Clear();
				this.treadsInfo.TreadRenders.Clear();
				this.treadsInfo.simplifiedHullPoints.Clear();
				this.DestroyBuildModeCollider();
				this.treadsDirty = true;
			}
		}

		// Token: 0x06001058 RID: 4184 RVA: 0x0006F3A8 File Offset: 0x0006D7A8
		public override void Play()
		{
			base.Play();
			this.externalControlBlock = null;
			this.inPlayOrFrameCaptureMode = true;
			this.DestroyBuildModeCollider();
			this.treatAsVehicleStatus = -1;
			this.go.GetComponent<Renderer>().enabled = false;
			this.originalMaxDepenetrationVelocity = this.chunk.rb.maxDepenetrationVelocity;
			this.chunk.rb.maxDepenetrationVelocity = 1f;
			if (this.groupBlocks != null && this.groupBlocks.Count > 0 && base.IsMainBlockInGroup())
			{
				this.treadsInfo.position = 0f;
				List<Block> list = base.ConnectionsOfType(2, true);
				HashSet<Transform> hashSet = new HashSet<Transform>();
				foreach (Block block in list)
				{
					BlockTankTreadsWheel blockTankTreadsWheel = block as BlockTankTreadsWheel;
					if (blockTankTreadsWheel == null || !this.groupBlocks.Contains(blockTankTreadsWheel))
					{
						hashSet.Add(block.goT.parent);
					}
				}
				this.CreateSupportRigidBodies();
				this.treadsReferenceMass = 0.5f;
				this.treadsReferenceInertia = Vector3.one;
				Vector3 position = this.goT.position;
				foreach (Block block2 in this.group.GetBlocks())
				{
					Chunk chunk = block2.chunk;
					Rigidbody rb = chunk.rb;
					if (rb != null)
					{
						this.treadsReferenceMass += rb.mass;
						if (block2 == this)
						{
							this.treadsReferenceInertia += rb.inertiaTensor;
						}
						else
						{
							float magnitude = (position - block2.goT.position).magnitude;
							this.treadsReferenceInertia += Vector3.one * rb.mass * magnitude * magnitude;
						}
					}
				}
				if (hashSet.Count == 0)
				{
					this.fakeChassis = new GameObject(this.go.name + " Fake Chassis");
					this.fakeChassis.transform.position = this.goT.position;
					Rigidbody rigidbody = this.fakeChassis.AddComponent<Rigidbody>();
					rigidbody.angularDrag = 2f;
					rigidbody.drag = 0.2f;
					if (Blocksworld.interpolateRigidBodies)
					{
						rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
					}
					rigidbody.mass = this.treadsReferenceMass;
					for (int j = 0; j < this.groupBlocks.Count; j++)
					{
						BlockTankTreadsWheel blockTankTreadsWheel2 = this.groupBlocks[j];
						blockTankTreadsWheel2.fakeChassis = this.fakeChassis;
						blockTankTreadsWheel2.CreateJoint(this.fakeChassis);
					}
					foreach (BlockTankTreadsWheel.TankTreadsSupportWheel wheel in this.supportWheels)
					{
						this.CreateSupportWheelJoint(this.fakeChassis, wheel);
					}
				}
				else
				{
					foreach (Transform transform in hashSet)
					{
						for (int k = 0; k < this.groupBlocks.Count; k++)
						{
							BlockTankTreadsWheel blockTankTreadsWheel3 = this.groupBlocks[k];
							blockTankTreadsWheel3.CreateJoint(transform.gameObject);
						}
						foreach (BlockTankTreadsWheel.TankTreadsSupportWheel wheel2 in this.supportWheels)
						{
							this.CreateSupportWheelJoint(transform.gameObject, wheel2);
						}
					}
				}
				foreach (BlockTankTreadsWheel.TankTreadsSupportWheel tankTreadsSupportWheel in this.supportWheels)
				{
					if (tankTreadsSupportWheel.go != null)
					{
						tankTreadsSupportWheel.go.SetLayer(Layer.Default, true);
					}
				}
				foreach (BlockTankTreadsWheel.TreadLink treadLink in this.treadsInfo.physicalLinks)
				{
					this.CreateSupportLinkJoints(treadLink);
					treadLink.collisionGo.SetLayer(Layer.Default, true);
				}
				for (int l = 0; l < this.groupBlocks.Count; l++)
				{
					BlockTankTreadsWheel blockTankTreadsWheel4 = this.groupBlocks[l];
					Chunk chunk2 = blockTankTreadsWheel4.chunk;
					SupportWheelHelpForceBehaviour supportWheelHelpForceBehaviour = chunk2.go.AddComponent<SupportWheelHelpForceBehaviour>();
					supportWheelHelpForceBehaviour.giver = blockTankTreadsWheel4;
					blockTankTreadsWheel4.helpForceBehaviour = supportWheelHelpForceBehaviour;
				}
			}
		}

		// Token: 0x06001059 RID: 4185 RVA: 0x0006F8F0 File Offset: 0x0006DCF0
		public override void RestoredMeshCollider()
		{
			base.RestoredMeshCollider();
			BlockTankTreadsWheel blockTankTreadsWheel = base.GetMainBlockInGroup() as BlockTankTreadsWheel;
			if (blockTankTreadsWheel != null)
			{
				blockTankTreadsWheel.ignoreCollisionsDirty = true;
			}
		}

		// Token: 0x0600105A RID: 4186 RVA: 0x0006F91C File Offset: 0x0006DD1C
		private void TurnOffInternalCollision()
		{
			base.UpdateConnectedCache();
			List<Block> list = Block.connectedCache[this];
			for (int i = 0; i < this.groupBlocks.Count; i++)
			{
				BlockTankTreadsWheel blockTankTreadsWheel = this.groupBlocks[i];
				Collider component = blockTankTreadsWheel.go.GetComponent<Collider>();
				foreach (Block block in list)
				{
					if (blockTankTreadsWheel != block)
					{
						this.IgnoreCollision(block, component);
					}
				}
			}
			foreach (BlockTankTreadsWheel.TankTreadsSupportWheel tankTreadsSupportWheel in this.supportWheels)
			{
				if (tankTreadsSupportWheel.go != null)
				{
					Collider component2 = tankTreadsSupportWheel.go.GetComponent<Collider>();
					foreach (BlockTankTreadsWheel.TankTreadsSupportWheel tankTreadsSupportWheel2 in this.supportWheels)
					{
						if (tankTreadsSupportWheel2 != tankTreadsSupportWheel && tankTreadsSupportWheel2.go != null)
						{
							Collider component3 = tankTreadsSupportWheel2.go.GetComponent<Collider>();
							Physics.IgnoreCollision(component2, component3);
						}
					}
					foreach (BlockTankTreadsWheel.TreadLink treadLink in this.treadsInfo.physicalLinks)
					{
						Collider component4 = treadLink.collisionGo.GetComponent<Collider>();
						Physics.IgnoreCollision(component2, component4);
					}
					for (int j = 0; j < this.groupBlocks.Count; j++)
					{
						BlockTankTreadsWheel blockTankTreadsWheel2 = this.groupBlocks[j];
						Collider component5 = blockTankTreadsWheel2.go.GetComponent<Collider>();
						Physics.IgnoreCollision(component2, component5);
					}
					foreach (Block b in list)
					{
						this.IgnoreCollision(b, component2);
					}
				}
			}
			foreach (BlockTankTreadsWheel.TreadLink treadLink2 in this.treadsInfo.physicalLinks)
			{
				Collider component6 = treadLink2.collisionGo.GetComponent<Collider>();
				foreach (BlockTankTreadsWheel.TreadLink treadLink3 in this.treadsInfo.physicalLinks)
				{
					if (treadLink3 != treadLink2)
					{
						Collider component7 = treadLink3.collisionGo.GetComponent<Collider>();
						Physics.IgnoreCollision(component6, component7);
					}
				}
				for (int k = 0; k < this.groupBlocks.Count; k++)
				{
					BlockTankTreadsWheel blockTankTreadsWheel3 = this.groupBlocks[k];
					Collider component8 = blockTankTreadsWheel3.go.GetComponent<Collider>();
					Physics.IgnoreCollision(component6, component8);
				}
				foreach (Block b2 in list)
				{
					this.IgnoreCollision(b2, component6);
				}
			}
		}

		// Token: 0x0600105B RID: 4187 RVA: 0x0006FD50 File Offset: 0x0006E150
		private void IgnoreCollision(Block b, Collider collider)
		{
			Collider component = b.go.GetComponent<Collider>();
			if (component.enabled)
			{
				Physics.IgnoreCollision(collider, component);
			}
			else
			{
				Collider[] componentsInChildren = b.go.GetComponentsInChildren<Collider>();
				foreach (Collider collider2 in componentsInChildren)
				{
					if (collider2.enabled)
					{
						Physics.IgnoreCollision(collider, collider2);
					}
				}
			}
		}

		// Token: 0x0600105C RID: 4188 RVA: 0x0006FDC0 File Offset: 0x0006E1C0
		public override void Play2()
		{
			this.wheelMass = this.goT.parent.GetComponent<Rigidbody>().mass;
			float d = this.wheelMass;
			Block[] blocks = this.group.GetBlocks();
			for (int i = 0; i < this.joints.Count; i++)
			{
				ConfigurableJoint joint = this.joints[i];
				float num = 5f;
				float num2 = 0.5f;
				foreach (Block block in blocks)
				{
					num2 += block.GetMass();
				}
				BlockTankTreadsWheel blockTankTreadsWheel = base.GetMainBlockInGroup() as BlockTankTreadsWheel;
				foreach (BlockTankTreadsWheel.TankTreadsSupportWheel tankTreadsSupportWheel in blockTankTreadsWheel.supportWheels)
				{
					num2 += tankTreadsSupportWheel.go.GetComponent<Rigidbody>().mass;
				}
				num2 += this.chassis.GetComponent<Rigidbody>().mass;
				num += num2;
				num *= 20f;
				this.SetSpring(joint, num, d);
			}
			Rigidbody component = this.chassis.GetComponent<Rigidbody>();
			if (component != null)
			{
				float num3 = this.treadsReferenceMass / component.mass;
				if (num3 > 1f)
				{
					float num4 = Mathf.Clamp(num3, 1f, 20f);
					component.mass *= num4;
				}
				float num5 = this.treadsReferenceInertia.magnitude / component.inertiaTensor.magnitude;
				if (num5 > 1f)
				{
					Vector3 inertiaTensor = component.inertiaTensor;
					float num6 = 1f / num5;
					try
					{
						component.inertiaTensor = num6 * inertiaTensor + (1f - num6) * this.treadsReferenceInertia;
					}
					catch
					{
						BWLog.Info("Unable to set inertia tensor, possibly due to the use of rigidbody constraints in the world.");
					}
				}
			}
			this.scaleSpeed = 1f / this.GetRadius();
			if (base.IsMainBlockInGroup())
			{
				this.TurnOffInternalCollision();
			}
			if (this.treadMaterial != null && this.treadMaterial.shader == Shader.Find("Blocksworld/Invisible"))
			{
				this.isInvisible = true;
				this.ToggleTreadVisibility(true);
			}
		}

		// Token: 0x0600105D RID: 4189 RVA: 0x0007003C File Offset: 0x0006E43C
		public float GetRadius()
		{
			return 0.5f * base.GetScale().y;
		}

		// Token: 0x0600105E RID: 4190 RVA: 0x00070060 File Offset: 0x0006E460
		private void CreateSupportRigidBodies()
		{
			foreach (BlockTankTreadsWheel.TankTreadsSupportWheel tankTreadsSupportWheel in this.supportWheels)
			{
				GameObject go = tankTreadsSupportWheel.go;
				if (!(go.GetComponent<Rigidbody>() != null))
				{
					CapsuleCollider component = go.GetComponent<CapsuleCollider>();
					component.material = this.go.GetComponent<Collider>().material;
					Rigidbody rigidbody = go.AddComponent<Rigidbody>();
					if (Blocksworld.interpolateRigidBodies)
					{
						rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
					}
					rigidbody.mass = 0.3f * component.height;
					rigidbody.useGravity = false;
					rigidbody.drag = 0.2f;
					rigidbody.angularDrag = 2f;
				}
			}
			float x = base.Scale().x;
			foreach (BlockTankTreadsWheel.TreadLink treadLink in this.treadsInfo.physicalLinks)
			{
				float magnitude = (treadLink.toTransform.position - treadLink.fromTransform.position).magnitude;
				Rigidbody rigidbody2 = treadLink.collisionGo.AddComponent<Rigidbody>();
				rigidbody2.mass = 0.2f * x * magnitude;
				rigidbody2.useGravity = false;
				rigidbody2.drag = 0.2f;
				rigidbody2.angularDrag = 2f;
			}
		}

		// Token: 0x0600105F RID: 4191 RVA: 0x00070200 File Offset: 0x0006E600
		public Vector3 GetHelpForceAt(Rigidbody thisRb, Rigidbody otherRb, Vector3 pos, Vector3 relVel, bool fresh)
		{
			Vector3 position = this.goT.position;
			Vector3 lhs = pos - position;
			Vector3 normalized = Vector3.Cross(lhs, this.goT.right).normalized;
			Vector3 b = (!(otherRb == null)) ? otherRb.velocity : Vector3.zero;
			Vector3 lhs2 = this.treadsInfo.speed * normalized - thisRb.velocity + b;
			Vector3 vector = Vector3.Dot(lhs2, normalized) * normalized;
			float num = Mathf.Abs(this.treadsInfo.speed);
			if (vector.sqrMagnitude > num * num)
			{
				vector = vector.normalized * num;
			}
			if (!fresh)
			{
				float d = 1f + 0.1f * this.helpForceErrorSum.magnitude;
				vector /= d;
				this.helpForceErrorSum += vector;
				this.helpForceErrorSum *= 0.95f;
			}
			else
			{
				this.helpForceErrorSum = Vector3.zero;
			}
			return 0.3f * thisRb.mass * vector;
		}

		// Token: 0x06001060 RID: 4192 RVA: 0x0007033C File Offset: 0x0006E73C
		public Vector3 GetForcePoint(Vector3 contactPoint)
		{
			return this.goT.position;
		}

		// Token: 0x06001061 RID: 4193 RVA: 0x00070349 File Offset: 0x0006E749
		public bool IsHelpForceActive()
		{
			return !this.broken && this.treadsInfo.mode == BlockTankTreadsWheel.TreadsMode.Drive && Mathf.Abs(this.treadsInfo.speed) > 0.01f;
		}

		// Token: 0x06001062 RID: 4194 RVA: 0x00070380 File Offset: 0x0006E780
		public void DestroyJoint(ConfigurableJoint joint)
		{
			this.joints.Remove(joint);
			UnityEngine.Object.Destroy(joint);
		}

		// Token: 0x06001063 RID: 4195 RVA: 0x00070398 File Offset: 0x0006E798
		private void CreateSupportLinkJoints(BlockTankTreadsWheel.TreadLink link)
		{
			GameObject collisionGo = link.collisionGo;
			ConfigurableJoint configurableJoint = collisionGo.AddComponent<ConfigurableJoint>();
			ConfigurableJoint configurableJoint2 = collisionGo.AddComponent<ConfigurableJoint>();
			link.joint1 = configurableJoint;
			link.joint2 = configurableJoint2;
			Rigidbody component = link.fromTransform.gameObject.GetComponent<Rigidbody>();
			if (component == null)
			{
				component = this.chassis.GetComponent<Rigidbody>();
			}
			Rigidbody component2 = link.toTransform.gameObject.GetComponent<Rigidbody>();
			if (component2 == null)
			{
				component2 = this.chassis.GetComponent<Rigidbody>();
			}
			if (component != null && component2 != null)
			{
				configurableJoint.connectedBody = component;
				configurableJoint2.connectedBody = component2;
				Transform transform = collisionGo.transform;
				Vector3 forward = transform.forward;
				ConfigurableJoint[] array = new ConfigurableJoint[]
				{
					configurableJoint,
					configurableJoint2
				};
				for (int i = 0; i < array.Length; i++)
				{
					ConfigurableJoint configurableJoint3 = array[i];
					int num = (i != 0) ? 1 : -1;
					Vector3 anchor = transform.worldToLocalMatrix.MultiplyPoint(transform.position + (float)num * forward * link.length * 0.5f);
					configurableJoint3.anchor = anchor;
					configurableJoint3.axis = Vector3.right;
					configurableJoint3.xMotion = ConfigurableJointMotion.Locked;
					configurableJoint3.yMotion = ConfigurableJointMotion.Locked;
					configurableJoint3.zMotion = ConfigurableJointMotion.Locked;
					configurableJoint3.angularXMotion = ConfigurableJointMotion.Free;
					configurableJoint3.angularYMotion = ConfigurableJointMotion.Locked;
					configurableJoint3.angularZMotion = ConfigurableJointMotion.Locked;
					configurableJoint3.angularYZLimitSpring = new SoftJointLimitSpring
					{
						spring = float.PositiveInfinity,
						damper = float.PositiveInfinity
					};
				}
			}
		}

		// Token: 0x06001064 RID: 4196 RVA: 0x00070544 File Offset: 0x0006E944
		private void CreateSupportWheelJoint(GameObject chassis, BlockTankTreadsWheel.TankTreadsSupportWheel wheel)
		{
			ConfigurableJoint configurableJoint = chassis.AddComponent<ConfigurableJoint>();
			wheel.joint = configurableJoint;
			GameObject go = wheel.go;
			Rigidbody component = go.GetComponent<Rigidbody>();
			configurableJoint.connectedBody = component;
			Transform transform = go.transform;
			configurableJoint.anchor = transform.position - chassis.transform.position;
			configurableJoint.axis = this.goT.right;
			configurableJoint.xMotion = ConfigurableJointMotion.Limited;
			configurableJoint.yMotion = ConfigurableJointMotion.Limited;
			configurableJoint.zMotion = ConfigurableJointMotion.Limited;
			configurableJoint.linearLimit = new SoftJointLimit
			{
				limit = 0f,
				bounciness = 0f
			};
			configurableJoint.linearLimitSpring = new SoftJointLimitSpring
			{
				spring = 30f * component.mass,
				damper = 2f * component.mass
			};
			configurableJoint.angularXMotion = ConfigurableJointMotion.Free;
			configurableJoint.angularYMotion = ConfigurableJointMotion.Locked;
			configurableJoint.angularZMotion = ConfigurableJointMotion.Locked;
			configurableJoint.angularYZLimitSpring = new SoftJointLimitSpring
			{
				spring = float.PositiveInfinity,
				damper = float.PositiveInfinity
			};
		}

		// Token: 0x06001065 RID: 4197 RVA: 0x00070658 File Offset: 0x0006EA58
		private bool ShouldBumpSuspensionJoint()
		{
			float num = float.PositiveInfinity;
			foreach (Block block in this.group.GetBlocks())
			{
				BlockTankTreadsWheel blockTankTreadsWheel = block as BlockTankTreadsWheel;
				if (blockTankTreadsWheel != null)
				{
					Transform goT = blockTankTreadsWheel.goT;
					num = Mathf.Min(Vector3.Dot(goT.up, goT.position + goT.up * this.GetRadius()), num);
				}
			}
			return Vector3.Dot(this.goT.up, this.goT.position) < num;
		}

		// Token: 0x06001066 RID: 4198 RVA: 0x000706F8 File Offset: 0x0006EAF8
		public ConfigurableJoint CreateJoint(GameObject chassis)
		{
			ConfigurableJoint configurableJoint = chassis.AddComponent<ConfigurableJoint>();
			configurableJoint.connectedBody = this.goT.parent.GetComponent<Rigidbody>();
			bool flag = this.ShouldBumpSuspensionJoint();
			Vector3 up = this.goT.up;
			if (flag)
			{
				this.goT.parent.Translate(-0.075f * up);
			}
			configurableJoint.anchor = this.goT.position - chassis.transform.position;
			if (flag)
			{
				this.goT.parent.Translate(0.075f * up);
			}
			configurableJoint.axis = this.goT.right;
			configurableJoint.xMotion = ConfigurableJointMotion.Locked;
			configurableJoint.yMotion = ConfigurableJointMotion.Limited;
			configurableJoint.zMotion = ConfigurableJointMotion.Locked;
			configurableJoint.angularXMotion = ConfigurableJointMotion.Free;
			configurableJoint.angularYMotion = ConfigurableJointMotion.Locked;
			configurableJoint.angularZMotion = ConfigurableJointMotion.Locked;
			this.joints.Add(configurableJoint);
			this.chassis = chassis;
			return configurableJoint;
		}

		// Token: 0x06001067 RID: 4199 RVA: 0x000707E8 File Offset: 0x0006EBE8
		private void SetSpring(ConfigurableJoint joint, float force, float d)
		{
			if (joint != null)
			{
				joint.linearLimitSpring = new SoftJointLimitSpring
				{
					spring = Mathf.Max(1f, force),
					damper = d
				};
			}
		}

		// Token: 0x06001068 RID: 4200 RVA: 0x0007082C File Offset: 0x0006EC2C
		public override void Stop(bool resetBlock = true)
		{
			this.go.SetLayer(Layer.Default, true);
			this.inPlayOrFrameCaptureMode = false;
			this.helpForceBehaviour = null;
			foreach (ConfigurableJoint obj in this.joints)
			{
				UnityEngine.Object.Destroy(obj);
			}
			this.joints.Clear();
			if (this.fakeChassis != null)
			{
				this.DestroyFakeChassis();
			}
			if (base.IsMainBlockInGroup())
			{
				this.DestroyTreads();
				this.treadsInfo.position = 0f;
			}
			this.PlayLoopSound(false, base.GetLoopClip(), 1f, null, 1f);
			base.Stop(resetBlock);
			this.go.GetComponent<Renderer>().enabled = false;
		}

		// Token: 0x06001069 RID: 4201 RVA: 0x00070918 File Offset: 0x0006ED18
		public override void Pause()
		{
			if (this.fakeChassis != null)
			{
				this.pausedVelocityAxle = this.fakeChassis.GetComponent<Rigidbody>().velocity;
				this.pausedAngularVelocityAxle = this.fakeChassis.GetComponent<Rigidbody>().angularVelocity;
				this.fakeChassis.GetComponent<Rigidbody>().isKinematic = true;
				if (base.IsMainBlockInGroup())
				{
					foreach (BlockTankTreadsWheel.TankTreadsSupportWheel tankTreadsSupportWheel in this.supportWheels)
					{
						Rigidbody component = tankTreadsSupportWheel.go.GetComponent<Rigidbody>();
						if (component != null)
						{
							tankTreadsSupportWheel.pausedVelocity = component.velocity;
							tankTreadsSupportWheel.pausedAngularVelocity = component.angularVelocity;
						}
					}
					foreach (BlockTankTreadsWheel.TreadLink treadLink in this.treadsInfo.physicalLinks)
					{
						Rigidbody component2 = treadLink.collisionGo.GetComponent<Rigidbody>();
						if (component2 != null)
						{
							treadLink.pausedVelocity = component2.velocity;
							treadLink.pausedAngularVelocity = component2.angularVelocity;
						}
					}
				}
			}
			this.PlayLoopSound(false, base.GetLoopClip(), 1f, null, 1f);
		}

		// Token: 0x0600106A RID: 4202 RVA: 0x00070A8C File Offset: 0x0006EE8C
		public override void Resume()
		{
			if (this.fakeChassis != null)
			{
				this.fakeChassis.GetComponent<Rigidbody>().isKinematic = false;
				this.fakeChassis.GetComponent<Rigidbody>().velocity = this.pausedVelocityAxle;
				this.fakeChassis.GetComponent<Rigidbody>().angularVelocity = this.pausedAngularVelocityAxle;
				if (base.IsMainBlockInGroup())
				{
					foreach (BlockTankTreadsWheel.TankTreadsSupportWheel tankTreadsSupportWheel in this.supportWheels)
					{
						Rigidbody component = tankTreadsSupportWheel.go.GetComponent<Rigidbody>();
						if (component != null)
						{
							component.isKinematic = false;
							component.velocity = tankTreadsSupportWheel.pausedVelocity;
							component.angularVelocity = tankTreadsSupportWheel.pausedAngularVelocity;
						}
					}
					foreach (BlockTankTreadsWheel.TreadLink treadLink in this.treadsInfo.physicalLinks)
					{
						Rigidbody component2 = treadLink.collisionGo.GetComponent<Rigidbody>();
						if (component2 != null)
						{
							component2.isKinematic = false;
							component2.velocity = treadLink.pausedVelocity;
							component2.angularVelocity = treadLink.pausedAngularVelocity;
						}
					}
				}
			}
		}

		// Token: 0x0600106B RID: 4203 RVA: 0x00070BF8 File Offset: 0x0006EFF8
		public override void ResetFrame()
		{
			if (base.IsMainBlockInGroup())
			{
				float driveSpeedTarget = this.treadsInfo.driveSpeedTarget;
				this.treadsInfo.driveSpeedTarget = 0f;
				if (driveSpeedTarget == 0f)
				{
					float num = 0f;
					float f = 0f;
					float num2 = 0f;
					bool flag = false;
					for (int i = 0; i < this.groupBlocks.Count; i++)
					{
						BlockTankTreadsWheel blockTankTreadsWheel = this.groupBlocks[i];
						float num3 = blockTankTreadsWheel.GetAngularVelocity() / blockTankTreadsWheel.scaleSpeed;
						num2 += num3 / (float)this.groupBlocks.Count;
						if (blockTankTreadsWheel.onGround > 0f)
						{
							flag = true;
							if (Mathf.Abs(num) < Mathf.Abs(num3))
							{
								num = num3;
							}
						}
						else if (Mathf.Abs(f) < Mathf.Abs(num3))
						{
							f = num3;
						}
					}
					if (flag)
					{
						this.treadsInfo.rollSpeedTarget = num;
						this.treadsInfo.mode = BlockTankTreadsWheel.TreadsMode.Rolling;
					}
					else
					{
						this.treadsInfo.spinSpeedTarget = 0.99f * num2;
						this.treadsInfo.mode = BlockTankTreadsWheel.TreadsMode.Spinning;
					}
				}
			}
		}

		// Token: 0x0600106C RID: 4204 RVA: 0x00070D23 File Offset: 0x0006F123
		private void UpdateBlocksInModelSetIfNecessary()
		{
			if (this.blocksInModelSet == null)
			{
				base.UpdateConnectedCache();
				this.blocksInModelSet = new HashSet<Block>(Block.connectedCache[this]);
			}
		}

		// Token: 0x0600106D RID: 4205 RVA: 0x00070D4D File Offset: 0x0006F14D
		public void Drive(float f)
		{
			this.treadsInfo.driveSpeedTarget += f;
			this.treadsInfo.mode = BlockTankTreadsWheel.TreadsMode.Drive;
			this.engineLoopOn = !this.broken;
			this.idleEngineCounter = 0;
			this.engineIncreasingPitch = true;
		}

		// Token: 0x0600106E RID: 4206 RVA: 0x00070D8C File Offset: 0x0006F18C
		public TileResultCode Drive(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.externalControlBlock == null)
			{
				float f = (float)args[0] * eInfo.floatArg;
				this.Drive(f);
			}
			return TileResultCode.True;
		}

		// Token: 0x0600106F RID: 4207 RVA: 0x00070DBC File Offset: 0x0006F1BC
		public TileResultCode IsDrivingSensor(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return ((float)args[0] <= 0f) ? this.IsBreaking() : this.IsDriving();
		}

		// Token: 0x06001070 RID: 4208 RVA: 0x00070DE4 File Offset: 0x0006F1E4
		public TileResultCode IsDriving()
		{
			Vector3 rhs = Quaternion.Euler(0f, -90f, 0f) * this.goT.right;
			return (this.goT.parent.GetComponent<Rigidbody>().velocity.sqrMagnitude <= 0.25f || Vector3.Dot(this.goT.parent.GetComponent<Rigidbody>().velocity, rhs) <= 0f) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06001071 RID: 4209 RVA: 0x00070E6C File Offset: 0x0006F26C
		public TileResultCode IsBreaking()
		{
			Vector3 rhs = Quaternion.Euler(0f, -90f, 0f) * this.goT.right;
			return (this.goT.parent.GetComponent<Rigidbody>().velocity.sqrMagnitude <= 0.25f || Vector3.Dot(this.goT.parent.GetComponent<Rigidbody>().velocity, rhs) >= 0f) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06001072 RID: 4210 RVA: 0x00070EF4 File Offset: 0x0006F2F4
		private void SetMotor(float speed)
		{
			if (this.isTreasure)
			{
				return;
			}
			float x = speed * this.scaleSpeed;
			bool flag = this.treadsInfo.mode == BlockTankTreadsWheel.TreadsMode.Drive || this.onGround <= 0f;
			float num = Mathf.Clamp(this.wheelMass * 20f / (this.scaleSpeed * this.scaleSpeed), 20f, 1000f);
			bool flag2 = false;
			for (int i = 0; i < this.joints.Count; i++)
			{
				ConfigurableJoint configurableJoint = this.joints[i];
				if (configurableJoint != null && configurableJoint.connectedBody != null)
				{
					configurableJoint.targetAngularVelocity = new Vector3(x, 0f, 0f);
					this.drive.positionDamper = ((!flag) ? 0f : num);
					this.drive.maximumForce = num;
					configurableJoint.angularXDrive = this.drive;
					Rigidbody connectedBody = configurableJoint.connectedBody;
					if (connectedBody.IsSleeping())
					{
						connectedBody.WakeUp();
					}
				}
				else
				{
					flag2 = true;
				}
			}
			if (flag2 && base.IsMainBlockInGroup())
			{
				Blocksworld.AddFixedUpdateCommand(new DelegateCommand(delegate(DelegateCommand a)
				{
					this.Break(this.goT.position, Vector3.zero, Vector3.zero);
				}));
			}
		}

		// Token: 0x06001073 RID: 4211 RVA: 0x0007104C File Offset: 0x0006F44C
		private void UpdateEngineSound()
		{
			if (!Sound.sfxEnabled || this.isTreasure || this.vanished)
			{
				this.PlayLoopSound(false, base.GetLoopClip(), 1f, null, 1f);
				return;
			}
			if (this.goT.parent == null)
			{
				return;
			}
			Rigidbody component = this.goT.parent.GetComponent<Rigidbody>();
			if (component == null)
			{
				return;
			}
			int num = 4;
			bool flag = this.engineIntervalCounter % num == 0;
			this.engineIntervalCounter++;
			float num2 = Mathf.Abs(Vector3.Dot(component.angularVelocity, this.goT.right));
			float num3 = num2 / 10f;
			float max = this.pitchScale;
			float num4 = 0.1f;
			float num5 = 0.6f * this.pitchScale;
			float num6 = num5 + num3;
			float num7 = 0.02f;
			if (num6 < this.engineLoopPitch)
			{
				num7 *= -1f;
			}
			float num8 = (!this.engineIncreasingPitch) ? -0.07f : num7;
			this.engineLoopPitch = Mathf.Clamp(this.engineLoopPitch + num8, (!this.engineLoopOn) ? num4 : num5, max);
			float num9 = (!this.engineLoopOn) ? -0.01f : 0.1f;
			num9 *= this.volScale;
			this.engineLoopVolume = Mathf.Clamp(this.engineLoopVolume + num9, 0f, 0.2f * this.volScale);
			if (this.engineLoopOn && num8 < 0f)
			{
				this.idleEngineCounter++;
				if (this.idleEngineCounter > 100)
				{
					this.engineLoopOn = false;
				}
			}
			float num10 = this.engineLoopVolume;
			if (flag)
			{
				if (Sound.BlockIsMuted(this))
				{
					num10 = 0f;
				}
				float num11 = 1f;
				if (num10 > 0.01f)
				{
					float num12 = 0.1f;
					num11 = num12 * 2f * (Mathf.PerlinNoise(Time.time, 0f) - 0.5f) + 1f;
				}
				float pitch = num11 * this.engineLoopPitch;
				this.PlayLoopSound(num10 > 0.01f, base.GetLoopClip(), num10, null, pitch);
			}
			base.UpdateWithinWaterLPFilter(null);
		}

		// Token: 0x06001074 RID: 4212 RVA: 0x000712A0 File Offset: 0x0006F6A0
		public override void Update()
		{
			base.Update();
			if (base.IsMainBlockInGroup())
			{
				if (Blocksworld.CurrentState == State.Build && this.treadsInfo != null && this.treadsDirty)
				{
					this.CreateTreads(false, false);
					this.treadsDirty = false;
				}
				if (this.ignoreCollisionsDirty)
				{
					this.TurnOffInternalCollision();
					this.ignoreCollisionsDirty = false;
				}
				this.UpdateTreads();
			}
			if (this.axlesDirty)
			{
				BlockTankTreadsWheel blockTankTreadsWheel = base.GetMainBlockInGroup() as BlockTankTreadsWheel;
				Vector3 position = this.goT.position;
				bool flag = false;
				bool flag2 = false;
				foreach (Block block in blockTankTreadsWheel.connections)
				{
					if (!(block.go == null))
					{
						Vector3 position2 = block.goT.position;
						Vector3 lhs = position2 - position;
						float num = Vector3.Dot(lhs, this.goT.right);
						flag = (flag || num < 0f);
						flag2 = (flag2 || num > 0f);
					}
				}
				this.hideAxleN.GetComponent<Renderer>().enabled = flag;
				this.hideAxleP.GetComponent<Renderer>().enabled = flag2;
				this.axlesDirty = false;
			}
		}

		// Token: 0x06001075 RID: 4213 RVA: 0x00071410 File Offset: 0x0006F810
		public float GetAngularVelocity()
		{
			Rigidbody rb = this.chunk.rb;
			if (rb != null)
			{
				return Vector3.Dot(rb.angularVelocity, this.goT.right);
			}
			return 0f;
		}

		// Token: 0x06001076 RID: 4214 RVA: 0x00071454 File Offset: 0x0006F854
		public override TileResultCode Freeze(bool informModelBlocks)
		{
			if (!this.didFix && base.IsMainBlockInGroup())
			{
				bool flag = base.ContainsTileWithPredicateInPlayMode(Block.predicateUnfreeze);
				this.SetMotor(0f);
				foreach (BlockTankTreadsWheel.TreadLink treadLink in this.treadsInfo.physicalLinks)
				{
					Rigidbody component = treadLink.collisionGo.GetComponent<Rigidbody>();
					if (component != null)
					{
						component.isKinematic = true;
					}
					if (!flag)
					{
						UnityEngine.Object.Destroy(treadLink.helpBehaviour);
					}
				}
				if (!flag)
				{
					foreach (BlockTankTreadsWheel blockTankTreadsWheel in this.groupBlocks)
					{
						UnityEngine.Object.Destroy(blockTankTreadsWheel.helpForceBehaviour);
					}
				}
				foreach (BlockTankTreadsWheel.TankTreadsSupportWheel tankTreadsSupportWheel in this.supportWheels)
				{
					if (tankTreadsSupportWheel.go != null)
					{
						Rigidbody component2 = tankTreadsSupportWheel.go.GetComponent<Rigidbody>();
						if (component2 != null)
						{
							component2.isKinematic = true;
						}
					}
				}
			}
			return base.Freeze(informModelBlocks);
		}

		// Token: 0x06001077 RID: 4215 RVA: 0x000715E8 File Offset: 0x0006F9E8
		public override void Unfreeze()
		{
			if (this.didFix && base.IsMainBlockInGroup())
			{
				foreach (BlockTankTreadsWheel.TreadLink treadLink in this.treadsInfo.physicalLinks)
				{
					Rigidbody component = treadLink.collisionGo.GetComponent<Rigidbody>();
					if (component != null)
					{
						component.isKinematic = false;
						component.WakeUp();
					}
				}
				foreach (BlockTankTreadsWheel.TankTreadsSupportWheel tankTreadsSupportWheel in this.supportWheels)
				{
					if (tankTreadsSupportWheel.go != null)
					{
						Rigidbody component2 = tankTreadsSupportWheel.go.GetComponent<Rigidbody>();
						if (component2 != null)
						{
							component2.isKinematic = false;
							component2.WakeUp();
						}
					}
				}
			}
			base.Unfreeze();
		}

		// Token: 0x06001078 RID: 4216 RVA: 0x00071704 File Offset: 0x0006FB04
		public void FixedUpdateDriveAndTurn()
		{
			bool flag = base.IsMainBlockInGroup();
			if (flag)
			{
				this.UpdateEngineSound();
			}
			if (this.isTreasure)
			{
				return;
			}
			if (this.broken || this.vanished || this.didFix)
			{
				return;
			}
			if (CollisionManager.bumping.Contains(this.goT.parent.gameObject))
			{
				this.onGround = 0.3f;
			}
			else
			{
				this.onGround -= Blocksworld.fixedDeltaTime;
			}
			if (this.joints.Count == 0)
			{
				return;
			}
			Rigidbody rb = this.chunk.rb;
			this.SetMotor(-this.treadsInfo.speed);
			if (flag)
			{
				BlockTankTreadsWheel.TreadsMode mode = this.treadsInfo.mode;
				if (mode != BlockTankTreadsWheel.TreadsMode.Drive)
				{
					if (mode != BlockTankTreadsWheel.TreadsMode.Rolling)
					{
						if (mode == BlockTankTreadsWheel.TreadsMode.Spinning)
						{
							this.treadsInfo.speed += (this.treadsInfo.spinSpeedTarget - this.treadsInfo.speed) / 40f;
							this.treadsInfo.speed *= 0.97f;
						}
					}
					else
					{
						this.treadsInfo.speed += (this.treadsInfo.rollSpeedTarget - this.treadsInfo.speed) / 10f;
					}
				}
				else
				{
					this.treadsInfo.speed += (this.treadsInfo.driveSpeedTarget - this.treadsInfo.speed) / 40f;
				}
				this.treadsInfo.position = this.treadsInfo.position + this.treadsInfo.speed * Blocksworld.fixedDeltaTime;
			}
			if (flag && rb != null && !rb.isKinematic && this.onGround > 0f && this.treadsInfo.mode == BlockTankTreadsWheel.TreadsMode.Drive && this.fakeChassis == null)
			{
				bool flag2 = true;
				for (int i = 0; i < this.joints.Count; i++)
				{
					ConfigurableJoint configurableJoint = this.joints[i];
					if (configurableJoint != null)
					{
						Rigidbody connectedBody = configurableJoint.connectedBody;
						Rigidbody component = configurableJoint.GetComponent<Rigidbody>();
						if (connectedBody == null || connectedBody.isKinematic || component == null || component.isKinematic)
						{
							flag2 = false;
							break;
						}
					}
				}
				if (flag2)
				{
					float num = 5f;
					float num2 = this.treadsInfo.speed * num;
					Vector3 vector;
					if (this.camHintDir.sqrMagnitude > 0.1f)
					{
						vector = this.camHintDir * Mathf.Min(10f * num, Mathf.Abs(num2));
					}
					else
					{
						vector = Quaternion.Euler(0f, -90f, 0f) * this.goT.right * Mathf.Clamp(num2, -10f * num, 10f * num);
					}
					vector = Util.ProjectOntoPlane(vector, Vector3.up);
					Blocksworld.blocksworldCamera.AddForceDirectionHint(this.chunk, vector);
					this.camHintDir = Vector3.zero;
				}
			}
		}

		// Token: 0x06001079 RID: 4217 RVA: 0x00071A64 File Offset: 0x0006FE64
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (Blocksworld.playFixedUpdateCounter == 0 && this.chassis != null)
			{
				Rigidbody component = this.chassis.GetComponent<Rigidbody>();
				if (component != null)
				{
					component.mass = Mathf.Max(2f, component.mass);
				}
			}
			if (this.externalControlBlock == null)
			{
				this.FixedUpdateDriveAndTurn();
			}
			if (Blocksworld.playFixedUpdateCounter == 10)
			{
				this.chunk.rb.maxDepenetrationVelocity = this.originalMaxDepenetrationVelocity;
			}
		}

		// Token: 0x0600107A RID: 4218 RVA: 0x00071AF4 File Offset: 0x0006FEF4
		private void UpdateTreads()
		{
			if (this.didFix || this.broken)
			{
				return;
			}
			foreach (BlockTankTreadsWheel blockTankTreadsWheel in this.groupBlocks)
			{
				if (blockTankTreadsWheel.broken)
				{
					this.broken = true;
					return;
				}
			}
			Vector3 right = this.goT.right;
			Vector3 vector = this.treadsUp;
			if (this.inPlayOrFrameCaptureMode && this.chassis != null)
			{
				vector = this.chassis.transform.TransformDirection(vector);
			}
			Vector3 forward = Vector3.Cross(right, vector);
			List<BlockTankTreadsWheel.TreadLink> links = this.treadsInfo.links;
			int count = links.Count;
			if (count > 0)
			{
				Vector3 vector2 = links[0].FromToWorld(right, vector, forward);
				for (int i = 0; i < count; i++)
				{
					BlockTankTreadsWheel.TreadLink treadLink = links[i];
					Vector3 vector3 = treadLink.ToToWorld(right, vector, forward);
					Transform visualGoT = treadLink.visualGoT;
					visualGoT.position = 0.5f * (vector2 + vector3);
					Vector3 a = vector3 - vector2;
					float magnitude = a.magnitude;
					Vector3 vector4 = a / magnitude;
					Vector3 upwards = Vector3.Cross(vector4, right);
					visualGoT.rotation = Quaternion.LookRotation(vector4, upwards);
					float z = magnitude / treadLink.length;
					visualGoT.localScale = new Vector3(1f, 1f, z);
					vector2 = vector3;
				}
			}
			if (this.treadMaterial != null)
			{
				float num = -this.treadsInfo.position * this.uvPerUnit;
				float num2 = Mathf.Sign(num);
				num = num2 * Mathf.Repeat(num * num2, 2f);
				string propertyName = "_VOffset";
				if (this.treadMaterial.HasProperty(propertyName))
				{
					this.treadMaterial.SetFloat(propertyName, num);
				}
			}
		}

		// Token: 0x0600107B RID: 4219 RVA: 0x00071D10 File Offset: 0x00070110
		private void DestroyFakeChassis()
		{
			if (this.fakeChassis != null)
			{
				UnityEngine.Object.Destroy(this.fakeChassis);
				this.fakeChassis = null;
			}
		}

		// Token: 0x0600107C RID: 4220 RVA: 0x00071D38 File Offset: 0x00070138
		public override void BecameTreasure()
		{
			base.BecameTreasure();
			this.DestroyFakeChassis();
			if (base.IsMainBlockInGroup())
			{
				foreach (BlockTankTreadsWheel.TankTreadsSupportWheel tankTreadsSupportWheel in this.supportWheels)
				{
					tankTreadsSupportWheel.go.transform.parent = this.goT;
					UnityEngine.Object.Destroy(tankTreadsSupportWheel.joint);
					UnityEngine.Object.Destroy(tankTreadsSupportWheel.go.GetComponent<Rigidbody>());
					tankTreadsSupportWheel.go.GetComponent<Collider>().isTrigger = true;
				}
				foreach (BlockTankTreadsWheel.TreadLink treadLink in this.treadsInfo.physicalLinks)
				{
					treadLink.collisionGo.transform.parent = this.goT;
					UnityEngine.Object.Destroy(treadLink.joint1);
					UnityEngine.Object.Destroy(treadLink.joint2);
					UnityEngine.Object.Destroy(treadLink.collisionGo.GetComponent<Rigidbody>());
					treadLink.collisionGo.GetComponent<Collider>().isTrigger = true;
				}
			}
		}

		// Token: 0x0600107D RID: 4221 RVA: 0x00071E7C File Offset: 0x0007027C
		public override bool TreatAsVehicleLikeBlock()
		{
			return base.TreatAsVehicleLikeBlockWithStatus(ref this.treatAsVehicleStatus);
		}

		// Token: 0x0600107E RID: 4222 RVA: 0x00071E8A File Offset: 0x0007028A
		public override bool CanScaleUpwards()
		{
			return false;
		}

		// Token: 0x0600107F RID: 4223 RVA: 0x00071E90 File Offset: 0x00070290
		public override bool TBoxScaleTo(Vector3 scale, bool recalculateCollider = true, bool forceRescale = false)
		{
			Vector3 b = base.Scale();
			scale = Util.Round(scale);
			if (Mathf.Abs(scale.y - scale.z) > 0.01f)
			{
				float num = Mathf.Abs(scale.y);
				scale.Set(Mathf.Abs(scale.x), num, num);
			}
			Vector3 vector = scale - b;
			if (vector.magnitude > 0.01f)
			{
				if (Mathf.Abs(vector.x) > 0.01f && this.groupBlocks != null)
				{
					foreach (BlockTankTreadsWheel blockTankTreadsWheel in this.groupBlocks)
					{
						Vector3 scale2 = blockTankTreadsWheel.Scale();
						scale2.x += vector.x;
						blockTankTreadsWheel.ScaleTo(scale2, true, false);
					}
				}
				this.ScaleTo(scale, recalculateCollider, forceRescale);
				BlockTankTreadsWheel blockTankTreadsWheel2 = base.GetMainBlockInGroup() as BlockTankTreadsWheel;
				if (blockTankTreadsWheel2 != null)
				{
					blockTankTreadsWheel2.CreateTreads(true, false);
					blockTankTreadsWheel2.treadsDirty = true;
				}
			}
			return true;
		}

		// Token: 0x06001080 RID: 4224 RVA: 0x00071FC8 File Offset: 0x000703C8
		public override bool MoveTo(Vector3 pos)
		{
			this.axlesDirty = true;
			return base.MoveTo(pos);
		}

		// Token: 0x06001081 RID: 4225 RVA: 0x00071FD8 File Offset: 0x000703D8
		public override bool TBoxMoveTo(Vector3 pos, bool forced = false)
		{
			Vector3 position = base.GetPosition();
			Vector3 rhs = pos - position;
			Vector3 vector = this.goT.right;
			if (forced && rhs.sqrMagnitude > 0.01f)
			{
				vector = rhs.normalized;
			}
			float num = Vector3.Dot(vector, rhs);
			if (Mathf.Abs(num) > 0.01f && this.groupBlocks != null)
			{
				foreach (BlockTankTreadsWheel blockTankTreadsWheel in this.groupBlocks)
				{
					if (blockTankTreadsWheel != this)
					{
						Vector3 vector2 = blockTankTreadsWheel.GetPosition();
						vector2 += num * vector;
						blockTankTreadsWheel.MoveTo(vector2);
					}
				}
			}
			bool result = base.MoveTo(pos);
			BlockTankTreadsWheel blockTankTreadsWheel2 = base.GetMainBlockInGroup() as BlockTankTreadsWheel;
			if (blockTankTreadsWheel2 != null)
			{
				blockTankTreadsWheel2.CreateTreads(true, false);
				blockTankTreadsWheel2.treadsDirty = true;
			}
			return result;
		}

		// Token: 0x06001082 RID: 4226 RVA: 0x000720E8 File Offset: 0x000704E8
		public override void OnBlockGroupReconstructed()
		{
			base.OnBlockGroupReconstructed();
			if (base.IsMainBlockInGroup())
			{
				this.CreateTreads(false, false);
				this.treadsDirty = true;
			}
		}

		// Token: 0x06001083 RID: 4227 RVA: 0x0007210C File Offset: 0x0007050C
		public override void BunchMoved()
		{
			BlockTankTreadsWheel blockTankTreadsWheel = base.GetMainBlockInGroup() as BlockTankTreadsWheel;
			if (blockTankTreadsWheel != null)
			{
				blockTankTreadsWheel.CreateTreads(true, false);
				blockTankTreadsWheel.treadsDirty = true;
			}
		}

		// Token: 0x06001084 RID: 4228 RVA: 0x0007213C File Offset: 0x0007053C
		public override void BunchRotated()
		{
			BlockTankTreadsWheel blockTankTreadsWheel = base.GetMainBlockInGroup() as BlockTankTreadsWheel;
			if (blockTankTreadsWheel != null)
			{
				blockTankTreadsWheel.CreateTreads(true, false);
				blockTankTreadsWheel.treadsDirty = true;
			}
		}

		// Token: 0x06001085 RID: 4229 RVA: 0x0007216A File Offset: 0x0007056A
		public override bool RotateTo(Quaternion rot)
		{
			this.axlesDirty = true;
			return base.RotateTo(rot);
		}

		// Token: 0x06001086 RID: 4230 RVA: 0x0007217C File Offset: 0x0007057C
		public override bool TBoxRotateTo(Quaternion rot)
		{
			Quaternion rotation = this.GetRotation();
			float f = Quaternion.Angle(rot, rotation);
			bool flag = Mathf.Abs(f) > 5f;
			if (flag)
			{
				Quaternion lhs = rot * Quaternion.Inverse(rotation);
				foreach (Block block in this.group.GetBlocks())
				{
					if (block != this)
					{
						block.RotateTo(lhs * block.GetRotation());
					}
				}
			}
			bool result = this.RotateTo(rot);
			if (flag)
			{
				this.RestoreLocalCoords(this.localCoords, true);
			}
			return result;
		}

		// Token: 0x06001087 RID: 4231 RVA: 0x00072224 File Offset: 0x00070624
		public void CreateTreads(bool shapeOnly = false, bool parentIsBlock = false)
		{
			string text = null;
			if (shapeOnly && this.TryReuseSATVolumes(ref text))
			{
				return;
			}
			if (this.cachedTreadsMeshes != null && this.treadsInfo.links.Count > 0)
			{
				if (text == null)
				{
					text = BlockTankTreadsWheel.TankTreadsSATMeshesInfo.GetKey(this);
				}
				if (text == this.cachedTreadsKey)
				{
					Vector3 b = this.goT.position - this.cachedTreadsMeshes.position;
					if (b.magnitude < 0.0001f)
					{
						return;
					}
					bool flag = false;
					foreach (BlockTankTreadsWheel.TreadLink treadLink in this.treadsInfo.physicalLinks)
					{
						Vector3 a = treadLink.collisionGo.transform.position + b;
						if (Mathf.Abs(Vector3.Dot(a - this.goT.position, this.goT.right)) > 0.25f)
						{
							flag = true;
							break;
						}
					}
					foreach (BlockTankTreadsWheel.TankTreadsSupportWheel tankTreadsSupportWheel in this.supportWheels)
					{
						Vector3 a2 = tankTreadsSupportWheel.go.transform.position + b;
						if (Mathf.Abs(Vector3.Dot(a2 - this.goT.position, this.goT.right)) > 0.25f)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						foreach (BlockTankTreadsWheel.TreadLink treadLink2 in this.treadsInfo.physicalLinks)
						{
							treadLink2.collisionGo.transform.position += b;
						}
						foreach (BlockTankTreadsWheel.TankTreadsSupportWheel tankTreadsSupportWheel2 in this.supportWheels)
						{
							tankTreadsSupportWheel2.go.transform.position += b;
						}
						this.cachedTreadsMeshes.position = this.goT.position;
						return;
					}
				}
			}
			Vector3 forward = this.goT.forward;
			Vector3 up = this.goT.up;
			List<Vector2> list = new List<Vector2>();
			List<Vector3> list2 = new List<Vector3>();
			List<Transform> list3 = new List<Transform>();
			List<BlockTankTreadsWheel> list4 = new List<BlockTankTreadsWheel>();
			Vector2 a3 = Vector2.zero;
			Vector2 vector = new Vector2(float.MaxValue, float.MaxValue);
			Vector2 vector2 = new Vector2(float.MinValue, float.MinValue);
			foreach (BlockTankTreadsWheel blockTankTreadsWheel in this.groupBlocks)
			{
				Vector3 position = blockTankTreadsWheel.goT.position;
				Vector2 a4 = new Vector2(Vector3.Dot(forward, position), Vector3.Dot(up, position));
				a3 += a4 / (float)this.groupBlocks.Count;
				vector.x = Mathf.Min(a4.x - 0.5f, vector.x);
				vector.y = Mathf.Min(a4.y - 0.5f, vector.y);
				vector2.x = Mathf.Max(a4.x + 0.5f, vector2.x);
				vector2.y = Mathf.Max(a4.y + 0.5f, vector2.y);
			}
			float num = vector2.x - vector.x;
			float num2 = vector2.y - vector.y;
			float num3 = 0.03f;
			for (int i = 0; i < this.groupBlocks.Count; i++)
			{
				BlockTankTreadsWheel blockTankTreadsWheel2 = this.groupBlocks[i];
				Vector3 position2 = blockTankTreadsWheel2.GetPosition();
				float num4 = blockTankTreadsWheel2.Scale().y * 0.5f;
				int num5 = Mathf.Clamp(Mathf.RoundToInt(num4 * 12f), 8, 20);
				float num6 = 6.28318548f / (float)num5;
				Transform goT = this.groupBlocks[i].goT;
				for (int j = 0; j < num5; j++)
				{
					Vector2 item = default(Vector2);
					float f = num6 * (float)j;
					Vector3 vector3 = position2 + num4 * forward * Mathf.Sin(f) + num4 * up * Mathf.Cos(f);
					item.x = Vector3.Dot(forward, vector3);
					item.y = Vector3.Dot(up, vector3);
					Vector3 vector4 = Vector3.Cross(forward, up);
					float d = Vector3.Dot(vector4, vector3);
					float num7 = num3 * (1f - Mathf.Abs(item.y - a3.y) / (num2 * 0.5f));
					float num8 = num3 * (1f - Mathf.Abs(item.x - a3.x) / (num * 0.5f));
					float num9 = Mathf.Sign(item.x - a3.x);
					float num10 = Mathf.Sign(item.y - a3.y);
					item.x += num9 * num7;
					item.y += num10 * num8;
					vector3 = item.x * forward + item.y * up + d * vector4;
					list2.Add(vector3);
					list.Add(item);
					list3.Add(goT);
					list4.Add(blockTankTreadsWheel2);
				}
			}
			this.DestroyTreads();
			this.ComputeSimplifiedHullPoints();
			if (!shapeOnly)
			{
				List<int> indices = BlockTankTreadsWheel.ComputeConvexHull(list);
				this.uvPerUnit = 1f / base.Scale().x;
				this.AddTreadLinks(indices, list2, list3, list4);
				this.SetTreadMaterial();
			}
			this.CreateBuildModeCollider();
			this.UpdateSATVolumes();
			if (!shapeOnly)
			{
				this.cachedTreadsKey = BlockTankTreadsWheel.TankTreadsSATMeshesInfo.GetKey(this);
				this.cachedTreadsMeshes = new BlockTankTreadsWheel.TankTreadsSATMeshesInfo
				{
					position = this.goT.position
				};
			}
			if (parentIsBlock)
			{
				this.UpdateTreads();
				Transform goT2 = this.goT;
				foreach (BlockTankTreadsWheel.TankTreadsSupportWheel tankTreadsSupportWheel3 in this.supportWheels)
				{
					tankTreadsSupportWheel3.go.transform.parent = goT2;
				}
				foreach (BlockTankTreadsWheel.TreadLink treadLink3 in this.treadsInfo.links)
				{
					treadLink3.visualGoT.parent = goT2;
					if (treadLink3.collisionGo != null)
					{
						treadLink3.collisionGo.transform.parent = goT2;
					}
				}
			}
		}

		// Token: 0x06001088 RID: 4232 RVA: 0x000729EC File Offset: 0x00070DEC
		private void ComputeSimplifiedHullPoints()
		{
			Vector3 forward = this.goT.forward;
			Vector3 up = this.goT.up;
			List<Vector3> list = new List<Vector3>();
			List<Vector2> list2 = new List<Vector2>();
			for (int i = 0; i < this.groupBlocks.Count; i++)
			{
				BlockTankTreadsWheel blockTankTreadsWheel = this.groupBlocks[i];
				Vector3 position = blockTankTreadsWheel.GetPosition();
				float num = blockTankTreadsWheel.Scale().y * 0.5f;
				for (int j = -1; j < 2; j += 2)
				{
					for (int k = -1; k < 2; k += 2)
					{
						Vector2 item = default(Vector2);
						Vector3 vector = position + (num - 0.1f) * forward * (float)j + (num - 0.1f) * up * (float)k;
						list.Add(vector);
						item.x = Vector3.Dot(forward, vector);
						item.y = Vector3.Dot(up, vector);
						list2.Add(item);
					}
				}
			}
			List<int> list3 = BlockTankTreadsWheel.ComputeConvexHull(list2);
			for (int l = 0; l < list3.Count; l++)
			{
				int index = list3[l];
				this.treadsInfo.simplifiedHullPoints.Add(list[index]);
			}
		}

		// Token: 0x06001089 RID: 4233 RVA: 0x00072B50 File Offset: 0x00070F50
		private Vector3 GetTreadsUp(int fromIndex, int toIndex, List<Vector3> worldPoints)
		{
			Vector3 b = worldPoints[fromIndex];
			Vector3 a = worldPoints[toIndex];
			Vector3 normalized = (a - b).normalized;
			return Vector3.Cross(normalized, this.goT.right);
		}

		// Token: 0x0600108A RID: 4234 RVA: 0x00072B94 File Offset: 0x00070F94
		private Vector3 BumpPointIfNecessary(Vector3 point, BlockTankTreadsWheel wheel)
		{
			if (wheel.ShouldBumpSuspensionJoint())
			{
				Transform goT = wheel.goT;
				Vector3 b = -goT.up * 0.075f;
				float magnitude = (goT.position - point).magnitude;
				Vector3 a = wheel.goT.position + b;
				float magnitude2 = (a - point).magnitude;
				if (magnitude2 > magnitude)
				{
					point += b;
				}
			}
			return point;
		}

		// Token: 0x0600108B RID: 4235 RVA: 0x00072C18 File Offset: 0x00071018
		private void AddTreadLinks(List<int> indices, List<Vector3> worldPoints, List<Transform> transforms, List<BlockTankTreadsWheel> wheels)
		{
			float num = 0f;
			for (int i = 0; i < indices.Count; i++)
			{
				int index = indices[i];
				int index2 = indices[(i + 1) % indices.Count];
				num += (worldPoints[index] - worldPoints[index2]).magnitude;
			}
			float num2 = Mathf.Round(num * this.uvPerUnit);
			float uvFactor = 1f;
			if (num2 > 1f)
			{
				uvFactor = num2 / (num * this.uvPerUnit);
			}
			float num3 = 0f;
			Vector3 right = this.goT.right;
			Vector3 vector = Vector3.up;
			float f = Vector3.Dot(right, Vector3.up);
			if (Mathf.Abs(f) > 0.1f)
			{
				vector = Vector3.right;
			}
			this.treadsUp = vector;
			Vector3 vector2 = Vector3.Cross(right, vector);
			for (int j = 0; j < indices.Count; j++)
			{
				int num4 = indices[j];
				int num5 = indices[(j + 1) % indices.Count];
				int toIndex = indices[(j + 2) % indices.Count];
				int fromIndex = indices[(j + indices.Count - 1) % indices.Count];
				Vector3 vector3 = worldPoints[num4];
				Vector3 vector4 = worldPoints[num5];
				Transform transform = transforms[num4];
				Transform transform2 = transforms[num5];
				Vector3 vector5 = vector4 - vector3;
				float magnitude = vector5.magnitude;
				Vector3 normalized = vector5.normalized;
				Vector3 treadUp = Vector3.Cross(normalized, this.goT.right);
				Vector3 prevTreadUp = this.GetTreadsUp(fromIndex, num4, worldPoints);
				Vector3 nextTreadUp = this.GetTreadsUp(num5, toIndex, worldPoints);
				bool flag = false;
				float num6 = 0f;
				int num7 = Mathf.FloorToInt(Mathf.Clamp(magnitude / 4.5f, 0f, 2f));
				if (num7 > 0 && transform != transform2)
				{
					magnitude = (vector4 - vector3).magnitude;
					float num8 = magnitude / (float)(num7 + 1);
					float num9 = num8;
					Vector3 a = vector3;
					for (int k = 0; k < num7; k++)
					{
						Vector3 fromPoint = a + normalized * (num9 - num8);
						if (k == 0)
						{
							fromPoint = vector3;
						}
						Vector3 vector6 = a + normalized * num9;
						BlockTankTreadsWheel.TankTreadsSupportWheel tankTreadsSupportWheel = this.CreateSupportWheel(vector6, normalized, j, k);
						this.supportWheels.Add(tankTreadsSupportWheel);
						Transform fromTransform = transform;
						if (k > 0)
						{
							fromTransform = this.supportWheels[this.supportWheels.Count - 2].go.transform;
						}
						Transform transform3 = tankTreadsSupportWheel.go.transform;
						this.treadsInfo.links.Add(this.CreateTreadLink(fromPoint, vector6, fromTransform, transform3, right, vector, vector2, treadUp, prevTreadUp, nextTreadUp, num3 + num6, uvFactor));
						num9 += num8;
						flag = true;
						num6 += num8;
					}
				}
				if (flag)
				{
					BlockTankTreadsWheel.TreadLink treadLink = this.treadsInfo.links[this.treadsInfo.links.Count - 1];
					this.treadsInfo.links.Add(this.CreateTreadLink(treadLink.ToToWorld(right, vector, vector2), vector4, treadLink.toTransform, transform2, right, vector, vector2, treadUp, prevTreadUp, nextTreadUp, num3 + num6, uvFactor));
				}
				else
				{
					this.treadsInfo.links.Add(this.CreateTreadLink(vector3, vector4, transform, transform2, right, vector, vector2, treadUp, prevTreadUp, nextTreadUp, num3, uvFactor));
				}
				num3 += magnitude;
			}
			foreach (BlockTankTreadsWheel.TreadLink treadLink2 in this.treadsInfo.links)
			{
				if (treadLink2.collisionGo != null)
				{
					this.treadsInfo.physicalLinks.Add(treadLink2);
				}
			}
		}

		// Token: 0x0600108C RID: 4236 RVA: 0x00073038 File Offset: 0x00071438
		private BlockTankTreadsWheel.TreadLink CreateTreadLink(Vector3 fromPoint, Vector3 toPoint, Transform fromTransform, Transform toTransform, Vector3 coordRight, Vector3 coordUp, Vector3 coordForward, Vector3 treadUp, Vector3 prevTreadUp, Vector3 nextTreadUp, float distanceSoFar, float uvFactor)
		{
			Vector3 fromCoord = this.WorldToTreadsLocal(fromPoint, fromTransform, coordRight, coordUp, coordForward);
			Vector3 toCoord = this.WorldToTreadsLocal(toPoint, toTransform, coordRight, coordUp, coordForward);
			Vector3 vector = toPoint - fromPoint;
			float magnitude = vector.magnitude;
			Vector3 normalized = vector.normalized;
			Vector3 a = 0.5f * (fromPoint + toPoint);
			Vector3 vector2 = base.Scale();
			float volume = vector2.x * 0.25f * 2f * magnitude;
			GameObject gameObject = null;
			if (fromTransform != toTransform)
			{
				gameObject = new GameObject(this.go.name + " support link");
				Transform transform = gameObject.transform;
				BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
				boxCollider.size = new Vector3(vector2.x - 0.2f, 0.475f, magnitude);
				Vector3 b = -treadUp * 0.25f;
				transform.position = a + b;
				transform.rotation = Quaternion.LookRotation(normalized, treadUp);
				Util.SetLayerRaw(gameObject, this.go.layer, true);
			}
			GameObject gameObject2 = new GameObject("tread part");
			Transform transform2 = gameObject2.transform;
			Vector3 b2 = treadUp * 0.2f;
			transform2.position = a + b2;
			transform2.rotation = Quaternion.LookRotation(normalized, treadUp);
			MeshFilter meshFilter = gameObject2.AddComponent<MeshFilter>();
			float topExpandNext = 0.1f * Mathf.Tan(0.008726646f * Vector3.Angle(treadUp, nextTreadUp));
			float topExpandPrev = 0.1f * Mathf.Tan(0.008726646f * Vector3.Angle(treadUp, prevTreadUp));
			meshFilter.mesh = this.CreateTreadMesh(new Vector3(vector2.x, 0.2f, magnitude), topExpandPrev, topExpandNext, distanceSoFar, uvFactor);
			Renderer item = gameObject2.AddComponent<MeshRenderer>();
			this.treadsInfo.TreadRenders.Add(item);
			Util.SetLayerRaw(gameObject2, this.go.layer, true);
			BlockTankTreadsWheel.TreadLink treadLink = new BlockTankTreadsWheel.TreadLink
			{
				fromCoord = fromCoord,
				toCoord = toCoord,
				fromTransform = fromTransform,
				toTransform = toTransform,
				collisionGo = gameObject,
				visualGo = gameObject2,
				visualGoT = gameObject2.transform,
				length = (toPoint - fromPoint).magnitude,
				volume = volume
			};
			if (gameObject != null)
			{
				SupportWheelHelpForceBehaviour supportWheelHelpForceBehaviour = gameObject.AddComponent<SupportWheelHelpForceBehaviour>();
				supportWheelHelpForceBehaviour.giver = treadLink;
				treadLink.helpBehaviour = supportWheelHelpForceBehaviour;
				supportWheelHelpForceBehaviour.forwardEvents = true;
				treadLink.treadsInfo = this.treadsInfo;
				this.treadsInfo.gameObjectToTreadLink[gameObject] = treadLink;
				BWSceneManager.AddChildBlockInstanceID(gameObject, this);
			}
			return treadLink;
		}

		// Token: 0x0600108D RID: 4237 RVA: 0x000732F4 File Offset: 0x000716F4
		private void CreateRectangularCuboidPlane(Vector3 cuboidSize, Vector3 up, Vector3 forward, List<Vector3> vertices, List<Vector3> normals, List<int> triangles, List<Vector2> uv, float expandPrev, float expandNext, float distanceSoFar, float uvFactor)
		{
			Vector3 vector = Vector3.Cross(up, forward);
			int count = vertices.Count;
			Vector3 a = 0.5f * Mathf.Abs(Vector3.Dot(cuboidSize, up)) * up;
			float num = Mathf.Abs(Vector3.Dot(cuboidSize, forward));
			Vector3 b = 0.5f * (num + 2f * expandNext) * forward;
			Vector3 b2 = -0.5f * (num + 2f * expandPrev) * forward;
			Vector3 b3 = 0.5f * Mathf.Abs(Vector3.Dot(cuboidSize, vector)) * vector;
			vertices.AddRange(new Vector3[]
			{
				a + b2 - b3,
				a + b2 + b3,
				a + b + b3,
				a + b - b3
			});
			triangles.AddRange(new int[]
			{
				count,
				count + 2,
				count + 1,
				count,
				count + 3,
				count + 2
			});
			normals.AddRange(new Vector3[]
			{
				up,
				up,
				up,
				up
			});
			float x = 0f;
			float uValue = this.treadsInfo.uValue;
			float y = this.uvPerUnit * uvFactor * distanceSoFar;
			float y2 = this.uvPerUnit * uvFactor * (distanceSoFar + num);
			uv.AddRange(new Vector2[]
			{
				new Vector2(x, y),
				new Vector2(uValue, y),
				new Vector2(uValue, y2),
				new Vector2(x, y2)
			});
		}

		// Token: 0x0600108E RID: 4238 RVA: 0x000734FC File Offset: 0x000718FC
		private Mesh CreateTreadMesh(Vector3 meshSize, float topExpandPrev, float topExpandNext, float distanceSoFar, float uvFactor)
		{
			Mesh mesh = new Mesh();
			List<Vector3> list = new List<Vector3>();
			List<Vector3> list2 = new List<Vector3>();
			List<int> list3 = new List<int>();
			List<Vector2> list4 = new List<Vector2>();
			List<Color> list5 = new List<Color>();
			Side[] array = new Side[]
			{
				Side.Top,
				Side.Bottom,
				Side.Right,
				Side.Left,
				Side.Front,
				Side.Back
			};
			foreach (Side side in array)
			{
				Vector3 up = Materials.SideToNormal(side);
				Vector3 forward = Materials.SideToForward(side);
				float expandPrev = 0f;
				float expandNext = 0f;
				if (side == Side.Top)
				{
					expandPrev = topExpandPrev;
					expandNext = topExpandNext;
				}
				this.CreateRectangularCuboidPlane(meshSize, up, forward, list, list2, list3, list4, expandPrev, expandNext, distanceSoFar, uvFactor);
				list5.AddRange(new Color[]
				{
					Color.white,
					Color.white,
					Color.white,
					Color.white
				});
			}
			mesh.vertices = list.ToArray();
			mesh.normals = list2.ToArray();
			mesh.triangles = list3.ToArray();
			mesh.uv = list4.ToArray();
			mesh.colors = list5.ToArray();
			return mesh;
		}

		// Token: 0x0600108F RID: 4239 RVA: 0x0007363C File Offset: 0x00071A3C
		private Vector3 WorldToTreadsLocal(Vector3 point, Transform t, Vector3 right, Vector3 up, Vector3 forward)
		{
			Vector3 lhs = point - t.position;
			Vector3 result = new Vector3(Vector3.Dot(lhs, right), Vector3.Dot(lhs, up), Vector3.Dot(lhs, forward));
			return result;
		}

		// Token: 0x06001090 RID: 4240 RVA: 0x00073678 File Offset: 0x00071A78
		private BlockTankTreadsWheel.TankTreadsSupportWheel CreateSupportWheel(Vector3 treadPos, Vector3 treadDir, int majorIndex, int minorIndex)
		{
			Vector3 right = this.goT.right;
			Vector3 a = Vector3.Cross(treadDir, right);
			GameObject gameObject = new GameObject(this.go.name + " support wheel");
			Transform transform = gameObject.transform;
			Vector3 vector = base.Scale();
			transform.rotation = this.goT.rotation;
			transform.position = treadPos - a * 0.25f;
			CapsuleCollider capsuleCollider = gameObject.AddComponent<CapsuleCollider>();
			capsuleCollider.height = vector.x - 0.2f;
			capsuleCollider.radius = 0.25f;
			capsuleCollider.direction = 0;
			Util.SetLayerRaw(gameObject, this.go.layer, true);
			BWSceneManager.AddChildBlockInstanceID(gameObject, this);
			return new BlockTankTreadsWheel.TankTreadsSupportWheel
			{
				go = gameObject
			};
		}

		// Token: 0x06001091 RID: 4241 RVA: 0x00073748 File Offset: 0x00071B48
		public static List<int> ComputeConvexHull(List<Vector2> pointList)
		{
			List<int> list = new List<int>();
			for (int i = 0; i < pointList.Count; i++)
			{
				list.Add(i);
			}
			list.Sort(delegate(int a, int b)
			{
				Vector2 vector = pointList[a];
				Vector2 vector2 = pointList[b];
				return (vector.x != vector2.x) ? ((vector.x <= vector2.x) ? -1 : 1) : vector.y.CompareTo(vector2.y);
			});
			List<int> list2 = new List<int>();
			int num = 0;
			int num2 = 0;
			for (int j = list.Count - 1; j >= 0; j--)
			{
				Vector2 a2 = pointList[list[j]];
				Vector2 b2;
				while (num >= 2 && BlockTankTreadsWheel.Vector2Cross((b2 = pointList[list2[list2.Count - 1]]) - pointList[list2[list2.Count - 2]], a2 - b2) >= 0f)
				{
					list2.RemoveAt(list2.Count - 1);
					num--;
				}
				list2.Add(list[j]);
				num++;
				while (num2 >= 2 && BlockTankTreadsWheel.Vector2Cross((b2 = pointList[list2[0]]) - pointList[list2[1]], a2 - b2) <= 0f)
				{
					list2.RemoveAt(0);
					num2--;
				}
				if (num2 != 0)
				{
					list2.Insert(0, list[j]);
				}
				num2++;
			}
			list2.RemoveAt(list2.Count - 1);
			return list2;
		}

		// Token: 0x06001092 RID: 4242 RVA: 0x000738EB File Offset: 0x00071CEB
		private static float Vector2Cross(Vector2 a, Vector2 b)
		{
			return a.x * b.y - a.y * b.x;
		}

		// Token: 0x06001093 RID: 4243 RVA: 0x0007390C File Offset: 0x00071D0C
		public override GAF GetIconGaf()
		{
			int num = this.group.GetBlocks().Length;
			return new GAF(Block.predicateCreate, new object[]
			{
				"Tank Treads Wheel x" + num
			});
		}

		// Token: 0x06001094 RID: 4244 RVA: 0x0007394C File Offset: 0x00071D4C
		public override void Break(Vector3 chunkPos, Vector3 chunkVel, Vector3 chunkAngVel)
		{
			if (this.group == null)
			{
				base.Break(chunkPos, chunkVel, chunkAngVel);
				return;
			}
			if (!this.broken)
			{
				foreach (BlockTankTreadsWheel.TreadLink treadLink in this.treadsInfo.links)
				{
					treadLink.DestroyJoints();
					treadLink.Break(chunkPos, chunkVel, chunkAngVel);
				}
				foreach (BlockTankTreadsWheel.TankTreadsSupportWheel tankTreadsSupportWheel in this.supportWheels)
				{
					tankTreadsSupportWheel.DestroyJoint();
				}
				foreach (BlockTankTreadsWheel blockTankTreadsWheel in this.groupBlocks)
				{
					foreach (ConfigurableJoint joint in new List<ConfigurableJoint>(blockTankTreadsWheel.joints))
					{
						blockTankTreadsWheel.DestroyJoint(joint);
					}
					blockTankTreadsWheel.broken = true;
				}
				this.broken = true;
			}
		}

		// Token: 0x06001095 RID: 4245 RVA: 0x00073ACC File Offset: 0x00071ECC
		public override void RemovedPlayBlock(Block b)
		{
			base.RemovedPlayBlock(b);
			if (b.go == this.chassis)
			{
				this.chassis = null;
			}
			else if (this.externalControlBlock == b)
			{
				this.externalControlBlock = null;
			}
			else if (this == b)
			{
				this.SetTreadsVisible(false);
			}
		}

		// Token: 0x06001096 RID: 4246 RVA: 0x00073B28 File Offset: 0x00071F28
		private void CreateBuildModeCollider()
		{
			this.DestroyBuildModeCollider();
			this.buildModeColliderGo = new GameObject(this.go.name + " build mode collider");
			Util.SetLayerRaw(this.buildModeColliderGo, this.go.layer, true);
			BWSceneManager.AddChildBlockInstanceID(this.buildModeColliderGo, this);
			Mesh mesh = new Mesh();
			List<Vector3> simplifiedHullPoints = this.treadsInfo.simplifiedHullPoints;
			List<Vector3> list = new List<Vector3>();
			List<int> list2 = new List<int>();
			Vector3 b = this.goT.right * base.Scale().x * 0.5f;
			int count = simplifiedHullPoints.Count;
			for (int i = 0; i < simplifiedHullPoints.Count; i++)
			{
				int num = i;
				int num2 = (num + 1) % count;
				Vector3 a = simplifiedHullPoints[i];
				list.Add(a + b);
				list.Add(a - b);
				list2.AddRange(new int[]
				{
					0,
					num * 2,
					num2 * 2,
					1,
					num * 2 + 1,
					num2 * 2 + 1
				});
			}
			mesh.vertices = list.ToArray();
			mesh.triangles = list2.ToArray();
			MeshCollider meshCollider = this.buildModeColliderGo.AddComponent<MeshCollider>();
			meshCollider.sharedMesh = mesh;
			meshCollider.convex = true;
		}

		// Token: 0x06001097 RID: 4247 RVA: 0x00073C84 File Offset: 0x00072084
		private bool TryReuseSATVolumes(ref string currentKey)
		{
			currentKey = BlockTankTreadsWheel.TankTreadsSATMeshesInfo.GetKey(this);
			if (currentKey == this.cachedSatMeshesKey)
			{
				this.shapeMeshes[0] = this.cachedSatMeshes.shape;
				this.jointMeshes[0] = this.cachedSatMeshes.joint1;
				this.jointMeshes[1] = this.cachedSatMeshes.joint2;
				Vector3 offset = this.goT.position - this.cachedSatMeshes.position;
				CollisionVolumes.TranslateMeshes(this.shapeMeshes, offset);
				CollisionVolumes.TranslateMeshes(this.jointMeshes, offset);
				this.cachedSatMeshes.position = this.goT.position;
				foreach (BlockTankTreadsWheel blockTankTreadsWheel in this.groupBlocks)
				{
					blockTankTreadsWheel.shapeMeshes = this.shapeMeshes;
				}
				return true;
			}
			return false;
		}

		// Token: 0x06001098 RID: 4248 RVA: 0x00073D88 File Offset: 0x00072188
		public override void UpdateSATVolumes()
		{
			if (!base.IsMainBlockInGroup())
			{
				this.glueMeshes = new CollisionMesh[0];
				this.shapeMeshes = new CollisionMesh[0];
				this.jointMeshes = new CollisionMesh[0];
				return;
			}
			if (this.treadsInfo.simplifiedHullPoints.Count == 0)
			{
				this.ComputeSimplifiedHullPoints();
			}
			if (this.glueMeshes == null)
			{
				this.glueMeshes = new CollisionMesh[0];
			}
			if (this.shapeMeshes == null || this.shapeMeshes.Length != 1)
			{
				this.shapeMeshes = new CollisionMesh[1];
			}
			this.shapeMeshes[0] = this.CreateConvexSATMesh(base.Scale().x - 0.2f, 0f);
			foreach (BlockTankTreadsWheel blockTankTreadsWheel in this.groupBlocks)
			{
				blockTankTreadsWheel.shapeMeshes = this.shapeMeshes;
			}
			if (this.jointMeshes == null || this.jointMeshes.Length != 2)
			{
				this.jointMeshes = new CollisionMesh[2];
			}
			this.jointMeshes[0] = this.CreateConvexSATMesh(0.01f, base.Scale().x * 0.5f);
			this.jointMeshes[1] = this.CreateConvexSATMesh(0.01f, -base.Scale().x * 0.5f);
			this.cachedSatMeshesKey = BlockTankTreadsWheel.TankTreadsSATMeshesInfo.GetKey(this);
			this.cachedSatMeshes = new BlockTankTreadsWheel.TankTreadsSATMeshesInfo
			{
				joint1 = this.jointMeshes[0],
				joint2 = this.jointMeshes[1],
				shape = this.shapeMeshes[0],
				key = this.cachedSatMeshesKey,
				position = this.goT.position
			};
		}

		// Token: 0x06001099 RID: 4249 RVA: 0x00073F74 File Offset: 0x00072374
		private CollisionMesh CreateConvexSATMesh(float thickness, float offset)
		{
			Vector3 position = this.goT.position;
			if (this.treadsInfo.simplifiedHullPoints.Count == 0)
			{
				Triangle[] triangles = new Triangle[]
				{
					new Triangle(position, position + Vector3.up, position + Vector3.right)
				};
				return new CollisionMesh(triangles);
			}
			List<Triangle> list = new List<Triangle>();
			List<Vector3> simplifiedHullPoints = this.treadsInfo.simplifiedHullPoints;
			Vector3 right = this.goT.right;
			Vector3 v = simplifiedHullPoints[0] + (offset + thickness * 0.5f) * right;
			Vector3 v2 = simplifiedHullPoints[0] + (offset - thickness * 0.5f) * right;
			for (int i = 0; i < simplifiedHullPoints.Count - 1; i++)
			{
				Vector3 vector = simplifiedHullPoints[i] + (offset + thickness * 0.5f) * right;
				Vector3 v3 = simplifiedHullPoints[(i + 1) % simplifiedHullPoints.Count] + (offset + thickness * 0.5f) * right;
				Vector3 v4 = simplifiedHullPoints[i] + (offset - thickness * 0.5f) * right;
				Vector3 vector2 = simplifiedHullPoints[(i + 1) % simplifiedHullPoints.Count] + (offset - thickness * 0.5f) * right;
				if (i != 0)
				{
					list.Add(new Triangle(v, vector, v3));
					list.Add(new Triangle(v2, v4, vector2));
				}
				list.Add(new Triangle(vector, v4, vector2));
				list.Add(new Triangle(vector, vector2, v3));
			}
			return new CollisionMesh(list.ToArray());
		}

		// Token: 0x0600109A RID: 4250 RVA: 0x00074134 File Offset: 0x00072534
		private Bounds GetSimplifiedHullBounds()
		{
			Bounds result = new Bounds(this.goT.position, Vector3.one);
			if (this.treadsInfo == null)
			{
				return result;
			}
			Vector3 b = this.goT.right * base.Scale().x * 0.5f;
			foreach (Vector3 a in this.treadsInfo.simplifiedHullPoints)
			{
				result.Encapsulate(a + b);
				result.Encapsulate(a - b);
			}
			return result;
		}

		// Token: 0x0600109B RID: 4251 RVA: 0x000741FC File Offset: 0x000725FC
		public override Bounds GetShapeCollisionBounds()
		{
			return this.GetSimplifiedHullBounds();
		}

		// Token: 0x0600109C RID: 4252 RVA: 0x00074204 File Offset: 0x00072604
		public override bool ShapeMeshCanCollideWith(Block b)
		{
			BlockTankTreadsWheel blockTankTreadsWheel = b as BlockTankTreadsWheel;
			if (blockTankTreadsWheel == null || blockTankTreadsWheel.group == null || this.group == null || blockTankTreadsWheel.group.groupId != this.group.groupId)
			{
				return true;
			}
			if (b != this && !Tutorial.InTutorialOrPuzzle())
			{
				Bounds bounds = b.go.GetComponent<Collider>().bounds;
				Bounds bounds2 = this.go.GetComponent<Collider>().bounds;
				bounds.size *= 0.9f;
				bounds2.size *= 0.9f;
				return bounds.Intersects(bounds2);
			}
			return false;
		}

		// Token: 0x0600109D RID: 4253 RVA: 0x000742C0 File Offset: 0x000726C0
		public override void TBoxStartRotate()
		{
			this.localCoords = this.ComputeLocalCoords(false);
		}

		// Token: 0x0600109E RID: 4254 RVA: 0x000742D0 File Offset: 0x000726D0
		private Dictionary<Block, Vector3> ComputeLocalCoords(bool snap = false)
		{
			Dictionary<Block, Vector3> dictionary = new Dictionary<Block, Vector3>();
			Transform goT = this.goT;
			Vector3 position = goT.position;
			foreach (Block block in this.group.GetBlocks())
			{
				if (block != this)
				{
					Vector3 rhs = block.GetPosition() - position;
					Vector3 vector = new Vector3(Vector3.Dot(goT.right, rhs), Vector3.Dot(goT.up, rhs), Vector3.Dot(goT.forward, rhs));
					if (Mathf.Abs(vector[0]) > 0.001f)
					{
						vector[0] = 0f;
					}
					dictionary[block] = ((!snap) ? vector : Util.Round2(vector));
				}
			}
			return dictionary;
		}

		// Token: 0x0600109F RID: 4255 RVA: 0x000743A0 File Offset: 0x000727A0
		public override void TBoxStopScale()
		{
			Dictionary<Block, Vector3> coords = this.ComputeLocalCoords(false);
			this.RestoreLocalCoords(coords, false);
		}

		// Token: 0x060010A0 RID: 4256 RVA: 0x000743BD File Offset: 0x000727BD
		public override void TBoxStopRotate()
		{
			if (this.localCoords == null)
			{
				BWLog.Info("TBoxStopRotate() called without calling TBoxStartRotate() first");
				return;
			}
			this.RestoreLocalCoords(this.localCoords, true);
			this.localCoords = null;
		}

		// Token: 0x060010A1 RID: 4257 RVA: 0x000743EC File Offset: 0x000727EC
		private void RestoreLocalCoords(Dictionary<Block, Vector3> coords, bool updateTreads = true)
		{
			if (coords == null)
			{
				return;
			}
			Transform goT = this.goT;
			Vector3 position = goT.position;
			foreach (KeyValuePair<Block, Vector3> keyValuePair in coords)
			{
				Vector3 b = goT.TransformDirection(keyValuePair.Value);
				keyValuePair.Key.MoveTo(position + b);
			}
			if (updateTreads)
			{
				BlockTankTreadsWheel blockTankTreadsWheel = base.GetMainBlockInGroup() as BlockTankTreadsWheel;
				if (blockTankTreadsWheel != null)
				{
					blockTankTreadsWheel.CreateTreads(true, false);
					blockTankTreadsWheel.treadsDirty = true;
				}
			}
		}

		// Token: 0x060010A2 RID: 4258 RVA: 0x000744A0 File Offset: 0x000728A0
		public override bool IgnorePaintToIndexInTutorial(int meshIndex)
		{
			return meshIndex == 3 || meshIndex == 4;
		}

		// Token: 0x060010A3 RID: 4259 RVA: 0x000744B0 File Offset: 0x000728B0
		public override bool IgnoreTextureToIndexInTutorial(int meshIndex)
		{
			return meshIndex == 3 || meshIndex == 4;
		}

		// Token: 0x060010A4 RID: 4260 RVA: 0x000744C0 File Offset: 0x000728C0
		public override void ConnectionsChanged()
		{
			if (this.groupBlocks != null)
			{
				foreach (BlockTankTreadsWheel blockTankTreadsWheel in this.groupBlocks)
				{
					blockTankTreadsWheel.axlesDirty = true;
				}
			}
			base.ConnectionsChanged();
		}

		// Token: 0x060010A5 RID: 4261 RVA: 0x00074530 File Offset: 0x00072930
		protected override void TranslateSATVolumes(Vector3 offset)
		{
		}

		// Token: 0x04000CD6 RID: 3286
		public static Predicate predicateTankTreadsAnalogStickControl;

		// Token: 0x04000CD7 RID: 3287
		public static Predicate predicateTankTreadsDrive;

		// Token: 0x04000CD8 RID: 3288
		public static Predicate predicateTankTreadsDriveAlongAnalogStick;

		// Token: 0x04000CD9 RID: 3289
		public static Predicate predicateTankTreadsTurnAlongAnalogStick;

		// Token: 0x04000CDA RID: 3290
		private int treatAsVehicleStatus = -1;

		// Token: 0x04000CDB RID: 3291
		public Block externalControlBlock;

		// Token: 0x04000CDC RID: 3292
		private bool inPlayOrFrameCaptureMode;

		// Token: 0x04000CDD RID: 3293
		private float originalMaxDepenetrationVelocity;

		// Token: 0x04000CDE RID: 3294
		private GameObject chassis;

		// Token: 0x04000CDF RID: 3295
		private GameObject fakeChassis;

		// Token: 0x04000CE0 RID: 3296
		private GameObject hideAxleN;

		// Token: 0x04000CE1 RID: 3297
		private GameObject hideAxleP;

		// Token: 0x04000CE2 RID: 3298
		private bool axlesDirty = true;

		// Token: 0x04000CE3 RID: 3299
		private Material treadMaterial;

		// Token: 0x04000CE4 RID: 3300
		public SupportWheelHelpForceBehaviour helpForceBehaviour;

		// Token: 0x04000CE5 RID: 3301
		private float treadsReferenceMass;

		// Token: 0x04000CE6 RID: 3302
		private Vector3 treadsReferenceInertia;

		// Token: 0x04000CE7 RID: 3303
		private Vector3 treadsUp = Vector3.up;

		// Token: 0x04000CE8 RID: 3304
		private Vector3 camHintDir = Vector3.zero;

		// Token: 0x04000CE9 RID: 3305
		private List<ConfigurableJoint> joints = new List<ConfigurableJoint>();

		// Token: 0x04000CEA RID: 3306
		private JointDrive drive = default(JointDrive);

		// Token: 0x04000CEB RID: 3307
		private float scaleSpeed;

		// Token: 0x04000CEC RID: 3308
		private float wheelMass = 1f;

		// Token: 0x04000CED RID: 3309
		private Vector3 helpForceErrorSum = Vector3.zero;

		// Token: 0x04000CEE RID: 3310
		private GameObject buildModeColliderGo;

		// Token: 0x04000CEF RID: 3311
		private bool treadsDirty = true;

		// Token: 0x04000CF0 RID: 3312
		private bool ignoreCollisionsDirty;

		// Token: 0x04000CF1 RID: 3313
		public float onGround;

		// Token: 0x04000CF2 RID: 3314
		private const float MIN_LINK_LENGTH = 0.25f;

		// Token: 0x04000CF3 RID: 3315
		private const float SUPPORT_WHEEL_RADIUS = 0.25f;

		// Token: 0x04000CF4 RID: 3316
		private const float SUPPORT_WHEEL_MAX_DISTANCE = 4.5f;

		// Token: 0x04000CF5 RID: 3317
		private const int SUPPORT_WHEEL_MAX_COUNT = 2;

		// Token: 0x04000CF6 RID: 3318
		private const float SUSPENSION_BUMP_HEIGHT = 0.075f;

		// Token: 0x04000CF7 RID: 3319
		private const float TREADS_WIDTH = 0.2f;

		// Token: 0x04000CF8 RID: 3320
		private const float DEFAULT_UV_PER_UNIT = 0.5f;

		// Token: 0x04000CF9 RID: 3321
		private float uvPerUnit = 0.5f;

		// Token: 0x04000CFA RID: 3322
		private Vector3 pausedVelocityAxle;

		// Token: 0x04000CFB RID: 3323
		private Vector3 pausedAngularVelocityAxle;

		// Token: 0x04000CFC RID: 3324
		private HashSet<Block> blocksInModelSet;

		// Token: 0x04000CFD RID: 3325
		private bool isInvisible;

		// Token: 0x04000CFE RID: 3326
		private bool engineLoopOn;

		// Token: 0x04000CFF RID: 3327
		private float engineLoopPitch = 1f;

		// Token: 0x04000D00 RID: 3328
		private float engineLoopVolume;

		// Token: 0x04000D01 RID: 3329
		private bool engineIncreasingPitch;

		// Token: 0x04000D02 RID: 3330
		private int idleEngineCounter;

		// Token: 0x04000D03 RID: 3331
		private float volScale = 1f;

		// Token: 0x04000D04 RID: 3332
		private float pitchScale = 1f;

		// Token: 0x04000D05 RID: 3333
		public List<BlockTankTreadsWheel> groupBlocks;

		// Token: 0x04000D06 RID: 3334
		private List<BlockTankTreadsWheel.TankTreadsSupportWheel> supportWheels = new List<BlockTankTreadsWheel.TankTreadsSupportWheel>();

		// Token: 0x04000D07 RID: 3335
		public BlockTankTreadsWheel.TreadsInfo treadsInfo;

		// Token: 0x04000D08 RID: 3336
		private int engineIntervalCounter;

		// Token: 0x04000D09 RID: 3337
		private Dictionary<Block, Vector3> localCoords;

		// Token: 0x04000D0A RID: 3338
		private string cachedSatMeshesKey;

		// Token: 0x04000D0B RID: 3339
		private string cachedTreadsKey;

		// Token: 0x04000D0C RID: 3340
		private BlockTankTreadsWheel.TankTreadsSATMeshesInfo cachedSatMeshes;

		// Token: 0x04000D0D RID: 3341
		private BlockTankTreadsWheel.TankTreadsSATMeshesInfo cachedTreadsMeshes;

		// Token: 0x020000DA RID: 218
		public class TankTreadsSupportWheel
		{
			// Token: 0x060010AD RID: 4269 RVA: 0x000745B6 File Offset: 0x000729B6
			public void DestroyJoint()
			{
				if (this.joint != null)
				{
					UnityEngine.Object.Destroy(this.joint);
					this.joint = null;
				}
			}

			// Token: 0x060010AE RID: 4270 RVA: 0x000745DB File Offset: 0x000729DB
			public void Destroy()
			{
				if (this.go != null)
				{
					UnityEngine.Object.Destroy(this.go);
					this.go = null;
				}
				this.DestroyJoint();
			}

			// Token: 0x04000D13 RID: 3347
			public GameObject go;

			// Token: 0x04000D14 RID: 3348
			public ConfigurableJoint joint;

			// Token: 0x04000D15 RID: 3349
			public Vector3 pausedVelocity;

			// Token: 0x04000D16 RID: 3350
			public Vector3 pausedAngularVelocity;

			// Token: 0x04000D17 RID: 3351
			public SupportWheelHelpForceBehaviour helpBehaviour;
		}

		// Token: 0x020000DB RID: 219
		public class TreadLink : IHelpForceGiver
		{
			// Token: 0x060010B0 RID: 4272 RVA: 0x00074630 File Offset: 0x00072A30
			public Vector3 FromToWorld(Vector3 right, Vector3 up, Vector3 forward)
			{
				Vector3 position = this.fromTransform.position;
				float x = this.fromCoord.x;
				float y = this.fromCoord.y;
				float z = this.fromCoord.z;
				return new Vector3(position.x + x * right.x + y * up.x + z * forward.x, position.y + x * right.y + y * up.y + z * forward.y, position.z + x * right.z + y * up.z + z * forward.z);
			}

			// Token: 0x060010B1 RID: 4273 RVA: 0x000746E4 File Offset: 0x00072AE4
			public Vector3 ToToWorld(Vector3 right, Vector3 up, Vector3 forward)
			{
				Vector3 position = this.toTransform.position;
				float x = this.toCoord.x;
				float y = this.toCoord.y;
				float z = this.toCoord.z;
				return new Vector3(position.x + x * right.x + y * up.x + z * forward.x, position.y + x * right.y + y * up.y + z * forward.y, position.z + x * right.z + y * up.z + z * forward.z);
			}

			// Token: 0x060010B2 RID: 4274 RVA: 0x00074798 File Offset: 0x00072B98
			public Vector3 GetTreadVelocity()
			{
				Vector3 normalized = (this.toTransform.position - this.fromTransform.position).normalized;
				return this.treadsInfo.speed * normalized;
			}

			// Token: 0x060010B3 RID: 4275 RVA: 0x000747DC File Offset: 0x00072BDC
			public Vector3 GetHelpForceAt(Rigidbody thisRb, Rigidbody otherRb, Vector3 pos, Vector3 relVel, bool fresh)
			{
				Vector3 normalized = (this.toTransform.position - this.fromTransform.position).normalized;
				Vector3 b = this.treadsInfo.speed * normalized;
				Vector3 lhs = relVel - b;
				Vector3 vector = Vector3.Dot(lhs, normalized) * normalized;
				float num = Mathf.Abs(this.treadsInfo.speed);
				if (vector.sqrMagnitude > num * num)
				{
					vector = vector.normalized * num;
				}
				if (!fresh)
				{
					float d = 1f + 0.1f * this.helpForceErrorSum.magnitude;
					vector /= d;
					this.helpForceErrorSum += vector;
					this.helpForceErrorSum *= 0.95f;
				}
				else
				{
					this.helpForceErrorSum = Vector3.zero;
				}
				return 3f * vector * thisRb.mass;
			}

			// Token: 0x060010B4 RID: 4276 RVA: 0x000748E9 File Offset: 0x00072CE9
			public Vector3 GetForcePoint(Vector3 contactPoint)
			{
				return contactPoint;
			}

			// Token: 0x060010B5 RID: 4277 RVA: 0x000748EC File Offset: 0x00072CEC
			public bool IsHelpForceActive()
			{
				return Mathf.Abs(this.treadsInfo.speed) > 0.01f;
			}

			// Token: 0x060010B6 RID: 4278 RVA: 0x00074908 File Offset: 0x00072D08
			public void Break(Vector3 chunkPos, Vector3 chunkVel, Vector3 chunkAngVel)
			{
				if (this.collisionGo != null)
				{
					UnityEngine.Object.Destroy(this.collisionGo);
				}
				Rigidbody rigidbody = this.visualGo.AddComponent<Rigidbody>();
				rigidbody.drag = 0.2f;
				rigidbody.angularDrag = 2f;
				this.visualGo.AddComponent<BoxCollider>();
				Block.AddExplosiveForce(rigidbody, this.visualGoT.position, chunkPos, chunkVel, chunkAngVel, 1f);
			}

			// Token: 0x060010B7 RID: 4279 RVA: 0x00074978 File Offset: 0x00072D78
			public void DestroyJoints()
			{
				if (this.joint1 != null)
				{
					UnityEngine.Object.Destroy(this.joint1);
					this.joint1 = null;
				}
				if (this.joint2 != null)
				{
					UnityEngine.Object.Destroy(this.joint2);
					this.joint2 = null;
				}
			}

			// Token: 0x060010B8 RID: 4280 RVA: 0x000749CC File Offset: 0x00072DCC
			public void Destroy()
			{
				this.DestroyJoints();
				if (this.collisionGo != null)
				{
					UnityEngine.Object.Destroy(this.collisionGo);
					this.collisionGo = null;
				}
				if (this.visualGo != null)
				{
					MeshFilter component = this.visualGo.GetComponent<MeshFilter>();
					UnityEngine.Object.Destroy(component.sharedMesh);
					UnityEngine.Object.Destroy(component.mesh);
					UnityEngine.Object.Destroy(this.visualGo);
					this.visualGo = null;
				}
			}

			// Token: 0x04000D18 RID: 3352
			public Vector3 fromCoord;

			// Token: 0x04000D19 RID: 3353
			public Vector3 toCoord;

			// Token: 0x04000D1A RID: 3354
			public Transform fromTransform;

			// Token: 0x04000D1B RID: 3355
			public Transform toTransform;

			// Token: 0x04000D1C RID: 3356
			public GameObject visualGo;

			// Token: 0x04000D1D RID: 3357
			public Transform visualGoT;

			// Token: 0x04000D1E RID: 3358
			public GameObject collisionGo;

			// Token: 0x04000D1F RID: 3359
			public ConfigurableJoint joint1;

			// Token: 0x04000D20 RID: 3360
			public ConfigurableJoint joint2;

			// Token: 0x04000D21 RID: 3361
			public BlockTankTreadsWheel.TreadsInfo treadsInfo;

			// Token: 0x04000D22 RID: 3362
			public float length = 1f;

			// Token: 0x04000D23 RID: 3363
			public float volume = 1f;

			// Token: 0x04000D24 RID: 3364
			public Vector3 pausedVelocity;

			// Token: 0x04000D25 RID: 3365
			public Vector3 pausedAngularVelocity;

			// Token: 0x04000D26 RID: 3366
			private Vector3 helpForceErrorSum = Vector3.zero;

			// Token: 0x04000D27 RID: 3367
			public SupportWheelHelpForceBehaviour helpBehaviour;
		}

		// Token: 0x020000DC RID: 220
		public enum TreadsMode
		{
			// Token: 0x04000D29 RID: 3369
			Drive,
			// Token: 0x04000D2A RID: 3370
			Rolling,
			// Token: 0x04000D2B RID: 3371
			Spinning
		}

		// Token: 0x020000DD RID: 221
		public class TreadsInfo
		{
			// Token: 0x04000D2C RID: 3372
			public BlockTankTreadsWheel.TreadsMode mode = BlockTankTreadsWheel.TreadsMode.Rolling;

			// Token: 0x04000D2D RID: 3373
			public float driveSpeedTarget;

			// Token: 0x04000D2E RID: 3374
			public float spinSpeedTarget;

			// Token: 0x04000D2F RID: 3375
			public float rollSpeedTarget;

			// Token: 0x04000D30 RID: 3376
			public float speed;

			// Token: 0x04000D31 RID: 3377
			public float position;

			// Token: 0x04000D32 RID: 3378
			public List<BlockTankTreadsWheel.TreadLink> links = new List<BlockTankTreadsWheel.TreadLink>();

			// Token: 0x04000D33 RID: 3379
			public List<BlockTankTreadsWheel.TreadLink> physicalLinks = new List<BlockTankTreadsWheel.TreadLink>();

			// Token: 0x04000D34 RID: 3380
			public List<Vector3> simplifiedHullPoints = new List<Vector3>();

			// Token: 0x04000D35 RID: 3381
			public Dictionary<GameObject, BlockTankTreadsWheel.TreadLink> gameObjectToTreadLink = new Dictionary<GameObject, BlockTankTreadsWheel.TreadLink>();

			// Token: 0x04000D36 RID: 3382
			public List<Renderer> TreadRenders = new List<Renderer>();

			// Token: 0x04000D37 RID: 3383
			public float appliedWaterForceTime = -1f;

			// Token: 0x04000D38 RID: 3384
			public float uValue = 1f;
		}

		// Token: 0x020000DE RID: 222
		private class TankTreadsSATMeshesInfo
		{
			// Token: 0x060010BB RID: 4283 RVA: 0x00074AB8 File Offset: 0x00072EB8
			public static string GetKey(BlockTankTreadsWheel wheel)
			{
				StringBuilder stringBuilder = new StringBuilder();
				Dictionary<Block, Vector3> dictionary = wheel.ComputeLocalCoords(true);
				foreach (Block block in wheel.group.GetBlocks())
				{
					stringBuilder.Append("W");
					if (block != wheel)
					{
						Vector3 vector = dictionary[block];
						for (int j = 0; j < 3; j++)
						{
							stringBuilder.Append(Mathf.RoundToInt(vector[j] * 2f));
						}
					}
					stringBuilder.Append("s");
					Vector3 vector2 = block.Scale();
					for (int k = 0; k < 3; k++)
					{
						stringBuilder.Append(Mathf.RoundToInt(vector2[k]));
					}
					stringBuilder.Append("r");
					Vector3 vector3 = Util.Round(block.GetRotation().eulerAngles);
					for (int l = 0; l < 3; l++)
					{
						stringBuilder.Append(Mathf.RoundToInt(vector3[l]));
					}
				}
				return stringBuilder.ToString();
			}

			// Token: 0x04000D39 RID: 3385
			public CollisionMesh shape;

			// Token: 0x04000D3A RID: 3386
			public CollisionMesh joint1;

			// Token: 0x04000D3B RID: 3387
			public CollisionMesh joint2;

			// Token: 0x04000D3C RID: 3388
			public Vector3 position;

			// Token: 0x04000D3D RID: 3389
			public string key;
		}
	}
}
