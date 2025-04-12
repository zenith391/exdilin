using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x020000F3 RID: 243
	public class BlockWorldJumper : Block
	{
		// Token: 0x060011F0 RID: 4592 RVA: 0x0007ADA8 File Offset: 0x000791A8
		public BlockWorldJumper(List<List<Tile>> tiles) : base(tiles)
		{
			this.sunMesh = this.goT.Find("Orrery Sun");
			this.innerPlanetMesh = this.goT.Find("Orrery Planet In");
			this.innerPlanetArmMesh = this.goT.Find("Orrery Planet In Arm");
			this.outerPlanetMesh = this.goT.Find("Orrery Planet Out");
			this.outerPlanetArmMesh = this.goT.Find("Orrery Planet Out Arm");
			this.orreryActive = false;
			this.orreryAccelerating = false;
			WorldSession.current.LoadAvailableTeleportWorlds();
			foreach (List<Tile> list in tiles)
			{
				foreach (Tile tile in list)
				{
					if (tile.gaf.Predicate.Name == "BlockWorldJumper.Jump")
					{
						string stringArgSafe = Util.GetStringArgSafe(tile.gaf.Args, 0, string.Empty);
						if (!string.IsNullOrEmpty(stringArgSafe))
						{
							WorldSession.current.AddToAvailableTeleportWorlds(stringArgSafe);
						}
					}
				}
			}
		}

		// Token: 0x060011F1 RID: 4593 RVA: 0x0007AF34 File Offset: 0x00079334
		public new static void Register()
		{
			PredicateRegistry.Add<BlockWorldJumper>("BlockWorldJumper.Jump", null, (Block b) => new PredicateActionDelegate(((BlockWorldJumper)b).JumpToWorld), new Type[]
			{
				typeof(string)
			}, null, null);
		}

		// Token: 0x060011F2 RID: 4594 RVA: 0x0007AF74 File Offset: 0x00079374
		public override void Play()
		{
			base.Play();
			this.SetOrreryActive(true);
		}

		// Token: 0x060011F3 RID: 4595 RVA: 0x0007AF83 File Offset: 0x00079383
		public override void Stop(bool resetBlock = true)
		{
			base.Stop(resetBlock);
			this.SetOrreryActive(false);
		}

		// Token: 0x060011F4 RID: 4596 RVA: 0x0007AF94 File Offset: 0x00079394
		private void SetOrreryActive(bool active)
		{
			this.orreryActive = active;
			this.orreryAccelerating = false;
			this.orreryAccelerationFactor = 1f;
			if (this.vfxObj != null)
			{
				UnityEngine.Object.Destroy(this.vfxObj);
			}
			this.vfxObj = null;
			this.playedSFX = false;
			this.teleportState = BlockWorldJumper.TeleportState.NONE;
		}

		// Token: 0x060011F5 RID: 4597 RVA: 0x0007AFEB File Offset: 0x000793EB
		public override void Destroy()
		{
			this.SetOrreryActive(false);
			base.Destroy();
		}

		// Token: 0x060011F6 RID: 4598 RVA: 0x0007AFFC File Offset: 0x000793FC
		private void OrreryFixedUpdate()
		{
			if (this.orreryActive)
			{
				if (this.orreryAccelerating)
				{
					this.orreryAccelerationFactor *= 1.065f;
				}
				else
				{
					this.orreryAccelerationFactor = 1f;
				}
				this.sunAngle = Mathf.Repeat(this.sunAngle + 0.1f * this.orreryAccelerationFactor, 360f);
				this.innerPlanetAngle = Mathf.Repeat(this.innerPlanetAngle + 1.3f * this.orreryAccelerationFactor, 360f);
				this.outerPlanetAngle = Mathf.Repeat(this.outerPlanetAngle + 0.8f * this.orreryAccelerationFactor, 360f);
				if (this.teleportState == BlockWorldJumper.TeleportState.LOADING_WORLD)
				{
					if (WorldSession.current.WorldTeleportHasSource())
					{
						this.teleportState = BlockWorldJumper.TeleportState.POWER_UP;
						this.vfxTimer = this.vfxDelay;
						this.sfxTimer = this.sfxDelay;
						this.playedSFX = false;
					}
				}
				else if (this.teleportState == BlockWorldJumper.TeleportState.POWER_UP)
				{
					if (!this.playedSFX)
					{
						this.sfxTimer -= Time.fixedDeltaTime;
						if (this.sfxTimer <= 0f)
						{
							Sound.PlayOneShotSound("Teleport_Buildup", 1f);
							this.playedSFX = true;
						}
					}
					if (this.vfxObj == null)
					{
						this.vfxTimer -= Time.fixedDeltaTime;
						if (this.vfxTimer <= 0f)
						{
							this.vfxObj = UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>("VFX/WorldTeleportVFX"));
							this.vfxObj.transform.position = this.goT.position + Vector3.up * 0.6f;
							this.vfxObj.transform.parent = this.goT;
							this.vfxObj.GetComponent<Animator>().Play("WorldTeleport");
						}
					}
				}
			}
		}

		// Token: 0x060011F7 RID: 4599 RVA: 0x0007B1E4 File Offset: 0x000795E4
		private void OrreryUpdate()
		{
			if (this.orreryActive)
			{
				this.sunMesh.localRotation = Quaternion.AngleAxis(this.sunAngle, Vector3.up);
				this.innerPlanetArmMesh.localRotation = Quaternion.AngleAxis(this.innerPlanetAngle, Vector3.up);
				this.innerPlanetMesh.localRotation = this.innerPlanetArmMesh.localRotation;
				this.outerPlanetArmMesh.localRotation = Quaternion.AngleAxis(this.outerPlanetAngle, Vector3.up);
				this.outerPlanetMesh.localRotation = this.outerPlanetArmMesh.localRotation;
			}
		}

		// Token: 0x060011F8 RID: 4600 RVA: 0x0007B27C File Offset: 0x0007967C
		public TileResultCode JumpToWorld(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string stringArg = Util.GetStringArg(args, 0, string.Empty);
			if (WorldSession.current.availableTeleportWorlds.IsLoadingWorldList())
			{
				return TileResultCode.Delayed;
			}
			float delay = 1.8f;
			if (WorldSession.current.TriggerJumpToWorld(stringArg, delay))
			{
				this.teleportState = BlockWorldJumper.TeleportState.LOADING_WORLD;
				this.orreryAccelerating = true;
				this.playedSFX = false;
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x060011F9 RID: 4601 RVA: 0x0007B2DB File Offset: 0x000796DB
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (Blocksworld.CurrentState == State.Play)
			{
				this.OrreryFixedUpdate();
			}
		}

		// Token: 0x060011FA RID: 4602 RVA: 0x0007B2F4 File Offset: 0x000796F4
		public override void Update()
		{
			base.Update();
			if (Blocksworld.CurrentState == State.Play)
			{
				this.OrreryUpdate();
			}
		}

		// Token: 0x04000E2F RID: 3631
		private BlockWorldJumper.TeleportState teleportState;

		// Token: 0x04000E30 RID: 3632
		private Transform sunMesh;

		// Token: 0x04000E31 RID: 3633
		private Transform innerPlanetMesh;

		// Token: 0x04000E32 RID: 3634
		private Transform innerPlanetArmMesh;

		// Token: 0x04000E33 RID: 3635
		private Transform outerPlanetMesh;

		// Token: 0x04000E34 RID: 3636
		private Transform outerPlanetArmMesh;

		// Token: 0x04000E35 RID: 3637
		private float sunAngle;

		// Token: 0x04000E36 RID: 3638
		private float innerPlanetAngle;

		// Token: 0x04000E37 RID: 3639
		private float outerPlanetAngle;

		// Token: 0x04000E38 RID: 3640
		private bool orreryActive;

		// Token: 0x04000E39 RID: 3641
		private bool orreryAccelerating;

		// Token: 0x04000E3A RID: 3642
		private float orreryAccelerationFactor = 1f;

		// Token: 0x04000E3B RID: 3643
		private bool playedSFX;

		// Token: 0x04000E3C RID: 3644
		private float sfxDelay = 0.6f;

		// Token: 0x04000E3D RID: 3645
		private float sfxTimer;

		// Token: 0x04000E3E RID: 3646
		private GameObject vfxObj;

		// Token: 0x04000E3F RID: 3647
		private float vfxDelay = 0.8f;

		// Token: 0x04000E40 RID: 3648
		private float vfxTimer;

		// Token: 0x020000F4 RID: 244
		private enum TeleportState
		{
			// Token: 0x04000E43 RID: 3651
			NONE,
			// Token: 0x04000E44 RID: 3652
			LOADING_WORLD,
			// Token: 0x04000E45 RID: 3653
			POWER_UP,
			// Token: 0x04000E46 RID: 3654
			JUMPING
		}
	}
}
