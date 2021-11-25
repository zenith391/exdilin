using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x020000BB RID: 187
	public class BlockPIRPistol : BlockAbstractBow
	{
		// Token: 0x06000E6C RID: 3692 RVA: 0x00061C48 File Offset: 0x00060048
		public BlockPIRPistol(List<List<Tile>> tiles) : base(tiles)
		{
			this._hammerSubmesh = this.goT.Find(this._hammerSubmeshName);
			this._muzzleFlashParticleSystemGo = (UnityEngine.Object.Instantiate(Resources.Load("Particles/Muzzle Flash")) as GameObject);
			this._muzzleFlashParticleSystem = this._muzzleFlashParticleSystemGo.GetComponent<ParticleSystem>();
			this._muzzleFlashParticleSystem.enableEmission = false;
			this._muzzleFlashParticleSystemGo.transform.parent = this.goT;
			this._muzzleFlashParticleSystemGo.transform.localPosition = Vector3.zero;
			this._muzzleFlashParticleSystemGo.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
			this._smokeParticleSystemGo = (UnityEngine.Object.Instantiate(Resources.Load("Blocks/BlockLaser Smoke Particle System")) as GameObject);
			this._smokeParticleSystem = this._smokeParticleSystemGo.GetComponent<ParticleSystem>();
		}

		// Token: 0x06000E6D RID: 3693 RVA: 0x00061DC6 File Offset: 0x000601C6
		protected override float GetMinTimeBetweenShots()
		{
			return 0.15f;
		}

		// Token: 0x06000E6E RID: 3694 RVA: 0x00061DCD File Offset: 0x000601CD
		protected override Vector3 GetAmmoPositionOffset()
		{
			return this._ammoPositionOffset;
		}

		// Token: 0x06000E6F RID: 3695 RVA: 0x00061DD5 File Offset: 0x000601D5
		protected override string GetAmmoPrefabName()
		{
			return this._ammoPrefabName;
		}

		// Token: 0x06000E70 RID: 3696 RVA: 0x00061DDD File Offset: 0x000601DD
		protected override Vector3 GetFiringDirection()
		{
			return this.goT.up;
		}

		// Token: 0x06000E71 RID: 3697 RVA: 0x00061DEA File Offset: 0x000601EA
		protected override string GetSFXForBlocked()
		{
			return this._blockedSFX;
		}

		// Token: 0x06000E72 RID: 3698 RVA: 0x00061DF2 File Offset: 0x000601F2
		protected override string GetSFXForHit()
		{
			return this._hitSFX[UnityEngine.Random.Range(0, 4)];
		}

		// Token: 0x06000E73 RID: 3699 RVA: 0x00061E02 File Offset: 0x00060202
		protected override string GetSFXForLoad()
		{
			return null;
		}

		// Token: 0x06000E74 RID: 3700 RVA: 0x00061E05 File Offset: 0x00060205
		protected override string GetSFXForShoot()
		{
			return this._shootSFX;
		}

		// Token: 0x06000E75 RID: 3701 RVA: 0x00061E0D File Offset: 0x0006020D
		protected override bool HasSFXForLoad()
		{
			return false;
		}

		// Token: 0x06000E76 RID: 3702 RVA: 0x00061E10 File Offset: 0x00060210
		public new static void Register()
		{
			PredicateRegistry.Add<BlockPIRPistol>("PIRPistol.Triggered", (Block b) => new PredicateSensorDelegate(b.IsFiredAsWeapon), null, null, null, null);
			Predicate pred = PredicateRegistry.Add<BlockPIRPistol>("PIRPistol.Load", (Block b) => new PredicateSensorDelegate(((BlockAbstractBow)b).IsLoaded), (Block b) => new PredicateActionDelegate(((BlockAbstractBow)b).Load), null, null, null);
			PredicateRegistry.Add<BlockPIRPistol>("PIRPistol.Shoot", null, (Block b) => new PredicateActionDelegate(((BlockAbstractBow)b).Shoot), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Force"
			}, null);
			List<List<Tile>> value = new List<List<Tile>>
			{
				new List<Tile>
				{
					new Tile(Block.predicateFirstFrame, new object[0]),
					Block.ThenTile(),
					new Tile(pred, new object[0])
				},
				Block.EmptyTileRow()
			};
			Block.defaultExtraTiles["PIR Pistol"] = value;
		}

		// Token: 0x06000E77 RID: 3703 RVA: 0x00061F3F File Offset: 0x0006033F
		protected override void BowLoaded()
		{
			this._hammerSubmesh.localRotation = Quaternion.identity;
		}

		// Token: 0x06000E78 RID: 3704 RVA: 0x00061F54 File Offset: 0x00060354
		protected override void BowShot()
		{
			this._hammerSubmesh.localRotation = Quaternion.AngleAxis(35f, Vector3.right);
			this._muzzleFlashParticleSystemGo.transform.localPosition = this._ammoPositionOffset + Vector3.up * 0.75f;
			this._muzzleFlashParticleSystemGo.transform.localRotation = Quaternion.Euler(-90f, 0f, UnityEngine.Random.value * 360f);
			this._muzzleFlashParticleSystem.Emit(1);
			float num = 1f + UnityEngine.Random.value;
			ParticleSystem.Particle particle = default(ParticleSystem.Particle);
			particle.position = this.goT.TransformPoint(this._hammerPositionOffset);
			particle.velocity = Vector3.zero;
			particle.size = 0.2f + UnityEngine.Random.value * 0.3f;
			particle.remainingLifetime = num;
			particle.startLifetime = num;
			particle.color = this.gray;
			this._smokeParticleSystem.Emit(particle);
		}

		// Token: 0x06000E79 RID: 3705 RVA: 0x00062054 File Offset: 0x00060454
		public override void Destroy()
		{
			UnityEngine.Object.Destroy(this._smokeParticleSystemGo);
			UnityEngine.Object.Destroy(this._muzzleFlashParticleSystemGo);
			base.Destroy();
		}

		// Token: 0x06000E7A RID: 3706 RVA: 0x00062072 File Offset: 0x00060472
		public override void Pause()
		{
			base.Pause();
			this._smokeParticleSystem.Pause();
		}

		// Token: 0x06000E7B RID: 3707 RVA: 0x00062085 File Offset: 0x00060485
		public override void Play()
		{
			base.Play();
			this._muzzleFlashParticleSystemGo.transform.localPosition = this._ammoPositionOffset + Vector3.up * 0.75f;
		}

		// Token: 0x06000E7C RID: 3708 RVA: 0x000620B7 File Offset: 0x000604B7
		public override void Resume()
		{
			base.Resume();
			this._smokeParticleSystem.Play();
		}

		// Token: 0x06000E7D RID: 3709 RVA: 0x000620CA File Offset: 0x000604CA
		public override void Stop(bool resetBlock = true)
		{
			base.Stop(resetBlock);
			this._smokeParticleSystem.Clear();
			this._smokeParticleSystem.Play();
			this._hammerSubmesh.localRotation = Quaternion.identity;
		}

		// Token: 0x04000B33 RID: 2867
		private const float _minTimeBetweenShots = 0.15f;

		// Token: 0x04000B34 RID: 2868
		private readonly Vector3 _ammoPositionOffset = new Vector3(0.393f, -0.126f, -0.072f);

		// Token: 0x04000B35 RID: 2869
		private readonly Vector3 _hammerPositionOffset = new Vector3(0.27f, -0.026f, -0.1024f);

		// Token: 0x04000B36 RID: 2870
		private readonly string _ammoPrefabName = "Blocks/Prefab PIR Pistol Ammo";

		// Token: 0x04000B37 RID: 2871
		private readonly string _blockedSFX = "Arrow Blocked";

		// Token: 0x04000B38 RID: 2872
		private readonly string[] _hitSFX = new string[]
		{
			"Meteor Impact Earth 1",
			"Meteor Impact Earth 2",
			"Meteor Impact Earth 3",
			"Meteor Impact Earth 4"
		};

		// Token: 0x04000B39 RID: 2873
		private readonly string _shootSFX = "grenade_detonation";

		// Token: 0x04000B3A RID: 2874
		private ParticleSystem _muzzleFlashParticleSystem;

		// Token: 0x04000B3B RID: 2875
		private GameObject _muzzleFlashParticleSystemGo;

		// Token: 0x04000B3C RID: 2876
		private ParticleSystem _smokeParticleSystem;

		// Token: 0x04000B3D RID: 2877
		private GameObject _smokeParticleSystemGo;

		// Token: 0x04000B3E RID: 2878
		private readonly Color32 gray = new Color32(127, 127, 127, byte.MaxValue);

		// Token: 0x04000B3F RID: 2879
		private readonly string _hammerSubmeshName = "pir pistol hammer";

		// Token: 0x04000B40 RID: 2880
		protected Transform _hammerSubmesh;
	}
}
