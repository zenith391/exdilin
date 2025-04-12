using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x02000055 RID: 85
	public class BlockAbstractLaser : Block
	{
		// Token: 0x060006FD RID: 1789 RVA: 0x0002F64C File Offset: 0x0002DA4C
		public BlockAbstractLaser(List<List<Tile>> tiles) : base(tiles)
		{
			this.smokeParticleSystemGo = (UnityEngine.Object.Instantiate(Resources.Load("Blocks/BlockLaser Smoke Particle System")) as GameObject);
			this.smokeParticleSystem = this.smokeParticleSystemGo.GetComponent<ParticleSystem>();
			this.muzzleFlashParticleSystemGo = (UnityEngine.Object.Instantiate(Resources.Load("Particles/Muzzle Flash")) as GameObject);
			this.muzzleFlashParticleSystem = this.muzzleFlashParticleSystemGo.GetComponent<ParticleSystem>();
			this.muzzleFlashParticleSystem.enableEmission = false;
			if (BlockAbstractLaser.projectileHitParticleSystem == null)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load("Particles/Projectile Hit Particle System")) as GameObject;
				BlockAbstractLaser.projectileHitParticleSystem = gameObject.GetComponent<ParticleSystem>();
				BlockAbstractLaser.projectileHitParticleSystem.enableEmission = false;
			}
			this.muzzleFlashParticleSystemGo.transform.parent = this.goT;
			this.muzzleFlashParticleSystemGo.transform.localPosition = Vector3.zero;
			this.muzzleFlashParticleSystemGo.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
			this.pulseCycleTime = 0.25f / (float)this.pulsesPerCycle;
			this.projectileCycleTime = 0.25f / (float)this.projectilesPerCycle;
			if (BlockAbstractLaser.laserColors == null)
			{
				BlockAbstractLaser.laserColors = new Dictionary<string, Color>();
			}
			this.loopName = "Laser Beam Loop";
		}

		// Token: 0x060006FE RID: 1790 RVA: 0x0002F8A7 File Offset: 0x0002DCA7
		public virtual bool CanFireProjectiles()
		{
			return false;
		}

		// Token: 0x060006FF RID: 1791 RVA: 0x0002F8AA File Offset: 0x0002DCAA
		public virtual bool CanFireLaser()
		{
			return true;
		}

		// Token: 0x06000700 RID: 1792 RVA: 0x0002F8AD File Offset: 0x0002DCAD
		public static bool IsHitByPulseOrBeam(Block block)
		{
			return BlockAbstractLaser.anyLaserBeamOrPulseAvailable && BlockAbstractLaser.Hits.Contains(block);
		}

		// Token: 0x06000701 RID: 1793 RVA: 0x0002F8C8 File Offset: 0x0002DCC8
		public static bool IsHitByTaggedPulseOrBeam(Block block, string tagName)
		{
			HashSet<Block> hashSet;
			return BlockAbstractLaser.TagHits.TryGetValue(tagName, out hashSet) && hashSet.Contains(block);
		}

		// Token: 0x06000702 RID: 1794 RVA: 0x0002F8F6 File Offset: 0x0002DCF6
		public static bool IsHitByProjectile(Block block)
		{
			return BlockAbstractLaser.anyProjectileAvailable && BlockAbstractLaser.ProjectileHits.Contains(block);
		}

		// Token: 0x06000703 RID: 1795 RVA: 0x0002F90F File Offset: 0x0002DD0F
		public static bool IsHitByProjectileModel(Block modelBlock)
		{
			return BlockAbstractLaser.anyProjectileAvailable && BlockAbstractLaser.ModelProjectileHits.Contains(modelBlock);
		}

		// Token: 0x06000704 RID: 1796 RVA: 0x0002F928 File Offset: 0x0002DD28
		public static bool IsHitByProjectile(List<Block> list)
		{
			for (int i = 0; i < list.Count; i++)
			{
				Block item = list[i];
				if (BlockAbstractLaser.ProjectileHits.Contains(item))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000705 RID: 1797 RVA: 0x0002F968 File Offset: 0x0002DD68
		public static bool IsHitByTaggedProjectile(Block block, string tagName)
		{
			HashSet<Block> hashSet;
			return BlockAbstractLaser.TagProjectileHits.TryGetValue(tagName, out hashSet) && hashSet.Contains(block);
		}

		// Token: 0x06000706 RID: 1798 RVA: 0x0002F996 File Offset: 0x0002DD96
		public static bool IsHitByLaser(Block b)
		{
			return BlockAbstractLaser.Hits.Contains(b);
		}

		// Token: 0x06000707 RID: 1799 RVA: 0x0002F9A3 File Offset: 0x0002DDA3
		public static bool IsHitByLaserModel(Block modelBlock)
		{
			return BlockAbstractLaser.ModelHits.Contains(modelBlock);
		}

		// Token: 0x06000708 RID: 1800 RVA: 0x0002F9B0 File Offset: 0x0002DDB0
		public static bool IsHitByLaser(List<Block> list)
		{
			for (int i = 0; i < list.Count; i++)
			{
				Block item = list[i];
				if (BlockAbstractLaser.Hits.Contains(item))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000709 RID: 1801 RVA: 0x0002F9F0 File Offset: 0x0002DDF0
		public static bool IsHitByTaggedLaser(List<Block> list, string tagName)
		{
			for (int i = 0; i < list.Count; i++)
			{
				Block item = list[i];
				if (BlockAbstractLaser.TagHits.ContainsKey(tagName) && BlockAbstractLaser.TagHits[tagName].Contains(item))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600070A RID: 1802 RVA: 0x0002FA45 File Offset: 0x0002DE45
		public static bool IsHitByTaggedLaserModel(Block modelBlock, string tagName)
		{
			return BlockAbstractLaser.ModelTagHits.ContainsKey(tagName) && BlockAbstractLaser.ModelTagHits[tagName].Contains(modelBlock);
		}

		// Token: 0x0600070B RID: 1803 RVA: 0x0002FA70 File Offset: 0x0002DE70
		public static bool IsHitByTaggedProjectile(List<Block> list, string tagName)
		{
			for (int i = 0; i < list.Count; i++)
			{
				Block item = list[i];
				if (BlockAbstractLaser.TagProjectileHits.ContainsKey(tagName) && BlockAbstractLaser.TagProjectileHits[tagName].Contains(item))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600070C RID: 1804 RVA: 0x0002FAC5 File Offset: 0x0002DEC5
		public static bool IsHitByTaggedProjectileModel(Block modelBlock, string tagName)
		{
			return BlockAbstractLaser.ModelTagProjectileHits.ContainsKey(tagName) && BlockAbstractLaser.ModelTagProjectileHits[tagName].Contains(modelBlock);
		}

		// Token: 0x0600070D RID: 1805 RVA: 0x0002FAF0 File Offset: 0x0002DEF0
		public override void Pause()
		{
			this.smokeParticleSystem.Pause();
			this._paused = true;
		}

		// Token: 0x0600070E RID: 1806 RVA: 0x0002FB04 File Offset: 0x0002DF04
		public override void Resume()
		{
			this.smokeParticleSystem.Play();
			this._paused = false;
		}

		// Token: 0x0600070F RID: 1807 RVA: 0x0002FB18 File Offset: 0x0002DF18
		public override void Play()
		{
			base.Play();
			this.laserMeta = this.go.GetComponent<LaserMetaData>();
			if (this.laserMeta != null)
			{
				this.loopName = this.laserMeta.loopSfx;
				float num = 0.85f + 0.15f * Mathf.Min(this.size.x, this.size.z);
				this.pulseSizeMultiplier = num * this.laserMeta.pulseSizeMultiplier;
				this.beamSizeMultiplier = num * this.laserMeta.beamSizeMultiplier;
				this.projectileSizeMultiplier = num * this.laserMeta.projectileSizeMultiplier;
				this.laserExitOffset = this.laserMeta.exitOffset;
				this.laserExitOffset.Scale(this.size);
				this.projectileExitOffset = this.laserMeta.projectileExitOffset;
				this.projectileExitOffset.Scale(this.size);
				this.pulseLengthMultiplier = this.laserMeta.pulseLengthMultiplier;
				this.pulseFrequencyMultiplier = this.laserMeta.pulseFrequencyMultiplier;
				this.fixLaserColor = this.laserMeta.fixLaserColor;
				this.laserColor = this.laserMeta.laserColor;
			}
			else
			{
				BWLog.Info("Could not find laser meta data component");
			}
			this.muzzleFlashParticleSystemGo.transform.localPosition = this.laserMeta.exitOffset + Vector3.up * 0.8f;
		}

		// Token: 0x06000710 RID: 1808 RVA: 0x0002FC88 File Offset: 0x0002E088
		public static void ClearHits()
		{
			BlockAbstractLaser.anyLaserBeamOrPulseAvailable = false;
			BlockAbstractLaser.anyProjectileAvailable = false;
			BlockAbstractLaser.Hits.Clear();
			BlockAbstractLaser.ModelHits.Clear();
			BlockAbstractLaser.ClearSets(BlockAbstractLaser.TagHits);
			BlockAbstractLaser.ClearSets(BlockAbstractLaser.ModelTagHits);
			BlockAbstractLaser.ProjectileHits.Clear();
			BlockAbstractLaser.ModelProjectileHits.Clear();
			BlockAbstractLaser.ClearSets(BlockAbstractLaser.TagProjectileHits);
			BlockAbstractLaser.ClearSets(BlockAbstractLaser.ModelTagProjectileHits);
		}

		// Token: 0x06000711 RID: 1809 RVA: 0x0002FCF4 File Offset: 0x0002E0F4
		private static void ClearSets(Dictionary<string, HashSet<Block>> dict)
		{
			if (dict.Count > 0)
			{
				bool flag = true;
				foreach (KeyValuePair<string, HashSet<Block>> keyValuePair in dict)
				{
					HashSet<Block> value = keyValuePair.Value;
					if (value.Count > 0)
					{
						flag = false;
					}
					keyValuePair.Value.Clear();
				}
				if (flag)
				{
					dict.Clear();
				}
			}
		}

		// Token: 0x06000712 RID: 1810 RVA: 0x0002FD80 File Offset: 0x0002E180
		public override void PlaceInCharacterHand(BlockAnimatedCharacter character)
		{
			this.animatedCharacter = character;
		}

		// Token: 0x06000713 RID: 1811 RVA: 0x0002FD89 File Offset: 0x0002E189
		public override void RemoveFromCharacterHand()
		{
			this.animatedCharacter = null;
		}

		// Token: 0x06000714 RID: 1812 RVA: 0x0002FD92 File Offset: 0x0002E192
		public TileResultCode IsBeaming(ScriptRowExecutionInfo eInfo, object[] args)
		{
			eInfo.floatArg = Mathf.Min(eInfo.floatArg, this.beamStrength);
			return (!this._beaming) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000715 RID: 1813 RVA: 0x0002FDBD File Offset: 0x0002E1BD
		public TileResultCode IsPulsing(ScriptRowExecutionInfo eInfo, object[] args)
		{
			eInfo.floatArg = Mathf.Min(eInfo.floatArg, this.pulseFrequencyFraction);
			return (!this._pulsing) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000716 RID: 1814 RVA: 0x0002FDE8 File Offset: 0x0002E1E8
		public TileResultCode IsFiringProjectile(ScriptRowExecutionInfo eInfo, object[] args)
		{
			eInfo.floatArg = Mathf.Min(eInfo.floatArg, this.projectileFrequencyFraction);
			return (!this._firingProjectile) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000717 RID: 1815 RVA: 0x0002FE13 File Offset: 0x0002E213
		public static bool IsReflective(string paint)
		{
			return paint == "White";
		}

		// Token: 0x06000718 RID: 1816 RVA: 0x0002FE20 File Offset: 0x0002E220
		public static bool IsTransparent(string texture)
		{
			return BlockAbstractLaser.transparentTexturesForLaser.Contains(texture);
		}

		// Token: 0x06000719 RID: 1817 RVA: 0x0002FE2D File Offset: 0x0002E22D
		public override void Destroy()
		{
			UnityEngine.Object.Destroy(this.smokeParticleSystemGo);
			UnityEngine.Object.Destroy(this.muzzleFlashParticleSystemGo);
			this.ClearBeams();
			base.Destroy();
		}

		// Token: 0x0600071A RID: 1818 RVA: 0x0002FE51 File Offset: 0x0002E251
		public override void Exploded()
		{
			base.Exploded();
			this.ClearBeams();
		}

		// Token: 0x0600071B RID: 1819 RVA: 0x0002FE5F File Offset: 0x0002E25F
		public override TileResultCode ExplodeLocal(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.ClearBeams();
			return base.ExplodeLocal(eInfo, args);
		}

		// Token: 0x0600071C RID: 1820 RVA: 0x0002FE70 File Offset: 0x0002E270
		private void ClearBeams()
		{
			foreach (AbstractProjectile abstractProjectile in this.projectiles)
			{
				LaserPulse laserPulse = (LaserPulse)abstractProjectile;
				laserPulse.Destroy();
			}
			this.projectiles.Clear();
			if (this._beam != null)
			{
				this._beam.Destroy();
				this._beam = null;
			}
		}

		// Token: 0x0600071D RID: 1821 RVA: 0x0002FEF8 File Offset: 0x0002E2F8
		public override void ResetFrame()
		{
			base.ResetFrame();
			if (!this._beaming && this._beam != null)
			{
				this._beam.Destroy();
				this._beam = null;
			}
			this._beaming = false;
			this._pulsing = false;
			this._firingProjectile = false;
		}

		// Token: 0x0600071E RID: 1822 RVA: 0x0002FF48 File Offset: 0x0002E348
		public override void Update()
		{
			base.Update();
			if (this._beam != null)
			{
				this._beam.Update(this._paused);
			}
			AbstractProjectile abstractProjectile = null;
			for (int i = 0; i < this.projectiles.Count; i++)
			{
				AbstractProjectile abstractProjectile2 = this.projectiles[i];
				abstractProjectile2.Update();
				if (abstractProjectile2.ShouldBeDestroyed())
				{
					abstractProjectile = abstractProjectile2;
				}
			}
			if (abstractProjectile != null)
			{
				if (this._currentPulse == abstractProjectile)
				{
					this._currentPulse = null;
				}
				this.projectiles.Remove(abstractProjectile);
				abstractProjectile.Destroy();
			}
		}

		// Token: 0x0600071F RID: 1823 RVA: 0x0002FFE4 File Offset: 0x0002E3E4
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (this._beam != null)
			{
				this._beam.FixedUpdate(false, this._paused);
			}
			BlockAbstractLaser.anyLaserBeamOrPulseAvailable = (BlockAbstractLaser.anyLaserBeamOrPulseAvailable || this._beam != null || this.projectiles.Count > 0);
			BlockAbstractLaser.anyProjectileAvailable = (this.projectiles.Count > 0);
			for (int i = 0; i < this.projectiles.Count; i++)
			{
				AbstractProjectile abstractProjectile = this.projectiles[i];
				abstractProjectile.FixedUpdate();
			}
			if (Sound.sfxEnabled && !this.vanished && (this._beaming || this._pulsing))
			{
				if (this.ourSource == null)
				{
					this.ourSource = this.go.GetComponent<AudioSource>();
				}
				if (this.ourSource != null && this.ourSource.isPlaying)
				{
					return;
				}
				this.PlayLoopSound(this._beaming, base.GetLoopClip(), 0.1f + 0.4f * this.beamStrength, null, 0.8f + 0.2f * this.beamStrength);
				base.UpdateWithinWaterLPFilter(null);
			}
			else
			{
				this.PlayLoopSound(false, base.GetLoopClip(), 1f, null, 1f);
			}
		}

		// Token: 0x06000720 RID: 1824 RVA: 0x0003014C File Offset: 0x0002E54C
		public override void Stop(bool resetBlock = true)
		{
			base.Stop(resetBlock);
			if (this._beam != null)
			{
				this._beam.Destroy();
				this._beam = null;
			}
			foreach (AbstractProjectile abstractProjectile in this.projectiles)
			{
				abstractProjectile.Destroy();
			}
			this.projectiles.Clear();
			this.smokeParticleSystem.Clear();
			this.smokeParticleSystem.Play();
			this.PlayLoopSound(false, base.GetLoopClip(), 1f, null, 1f);
			this._paused = false;
		}

		// Token: 0x06000721 RID: 1825 RVA: 0x0003020C File Offset: 0x0002E60C
		public Color GetLaserColor()
		{
			if (this.fixLaserColor)
			{
				return this.laserColor;
			}
			string paint = this.GetPaint(0);
			if (BlockAbstractLaser.laserColors.ContainsKey(paint))
			{
				return BlockAbstractLaser.laserColors[paint];
			}
			return this.go.GetComponent<Renderer>().sharedMaterial.GetColor("_Color");
		}

		// Token: 0x06000722 RID: 1826 RVA: 0x00030269 File Offset: 0x0002E669
		public Vector3 GetFireDirectionUp()
		{
			if (this.animatedCharacter == null)
			{
				return this.goT.forward;
			}
			return this.animatedCharacter.goT.up;
		}

		// Token: 0x06000723 RID: 1827 RVA: 0x00030292 File Offset: 0x0002E692
		public Vector3 GetFireDirectionForward()
		{
			if (this.animatedCharacter == null)
			{
				return this.goT.up;
			}
			return this.animatedCharacter.goT.forward;
		}

		// Token: 0x06000724 RID: 1828 RVA: 0x000302BC File Offset: 0x0002E6BC
		private void UpdatePulseVariables(float power)
		{
			this.pulseSpeedMultiplier = power;
			float num = 50f;
			float t = power / num;
			this.pulseLengthFraction = Mathf.Lerp(0.43f, 0.1f, t);
			this.segmentTime = Mathf.Lerp(100f, 400f, t);
		}

		// Token: 0x06000725 RID: 1829 RVA: 0x00030308 File Offset: 0x0002E708
		public TileResultCode Pulse(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float num = (args.Length <= 0) ? 4f : ((float)args[0]);
			if (this.pulseSpeedMultiplier != num)
			{
				this.UpdatePulseVariables(num);
			}
			if (this.broken || this.vanished)
			{
				if (this._currentPulse != null)
				{
					this._currentPulse.StopReceivingEnergy();
				}
				return TileResultCode.True;
			}
			this.pulseFrequencyFraction = eInfo.floatArg;
			if (this._currentPulse == null)
			{
				this._currentPulse = new LaserPulse(this);
				this.projectiles.Add(this._currentPulse);
				if (eInfo.timer == 0f)
				{
					string sfxName = (!(this.laserMeta == null)) ? this.laserMeta.pulseSfx : "Laser Pulse";
					base.PlayPositionedSound(sfxName, 0.5f, 1f);
				}
			}
			this._pulsing = true;
			float num2 = 6f - 5f * eInfo.floatArg;
			if (eInfo.timer >= this.pulseCycleTime * num2 / this.pulseFrequencyMultiplier)
			{
				if (this._currentPulse != null)
				{
					this._currentPulse.StopReceivingEnergy();
				}
				this._currentPulse = null;
				return TileResultCode.True;
			}
			if (eInfo.timer >= this.pulseCycleTime * this.pulseLengthFraction * this.pulseLengthMultiplier / this.pulseFrequencyMultiplier)
			{
				this._currentPulse.StopReceivingEnergy();
				return TileResultCode.Delayed;
			}
			this._currentPulse.EnergyFraction = 1f;
			return TileResultCode.Delayed;
		}

		// Token: 0x06000726 RID: 1830 RVA: 0x00030488 File Offset: 0x0002E888
		public TileResultCode FireProjectile(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.broken || this.vanished)
			{
				if (this._currentProjectile != null)
				{
					this._currentProjectile.StopReceivingEnergy();
				}
				return TileResultCode.True;
			}
			this.projectileFrequencyFraction = eInfo.floatArg;
			if (this._currentProjectile == null)
			{
				this._currentProjectile = new Projectile(this);
				this.projectiles.Add(this._currentProjectile);
				if (eInfo.timer == 0f)
				{
					string sfxName = (!(this.laserMeta == null)) ? this.laserMeta.projectileSfx : "Fire";
					base.PlayPositionedSound(sfxName, 0.5f, 1f);
					Rigidbody rb = this.chunk.rb;
					if (rb != null && !rb.isKinematic)
					{
						float num = this.laserMeta.projectileRecoilForce;
						float mass = rb.mass;
						float num2 = num / mass;
						if (num2 > this.laserMeta.projectileMaxRecoilPerMass)
						{
							num *= this.laserMeta.projectileMaxRecoilPerMass / num2;
						}
						Vector3 force = -this.goT.up * num;
						rb.AddForce(force, ForceMode.Impulse);
					}
					this.muzzleFlashParticleSystemGo.transform.localPosition = this.laserMeta.exitOffset + Vector3.up * 0.5f;
					this.muzzleFlashParticleSystemGo.transform.localRotation = Quaternion.Euler(-90f, 0f, UnityEngine.Random.value * 360f);
					this.muzzleFlashParticleSystem.Emit(1);
				}
			}
			this._firingProjectile = true;
			float num3 = 6f - 5f * eInfo.floatArg;
			if (eInfo.timer >= this.projectileCycleTime * num3 / this.projectileFrequencyMultiplier)
			{
				if (this._currentProjectile != null)
				{
					this._currentProjectile.StopReceivingEnergy();
				}
				this._currentProjectile = null;
				return TileResultCode.True;
			}
			if (eInfo.timer >= this.projectileCycleTime * this.projectileLengthFraction * this.projectileLengthMultiplier / this.projectileFrequencyMultiplier)
			{
				this._currentProjectile.StopReceivingEnergy();
				return TileResultCode.Delayed;
			}
			this._currentProjectile.EnergyFraction = eInfo.timer / (this.projectileCycleTime * this.projectileLengthFraction * this.projectileLengthMultiplier / this.projectileFrequencyMultiplier);
			return TileResultCode.Delayed;
		}

		// Token: 0x06000727 RID: 1831 RVA: 0x000306DC File Offset: 0x0002EADC
		public TileResultCode Beam(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.broken || this.vanished)
			{
				return TileResultCode.True;
			}
			if (this._beam == null)
			{
				this._beam = new LaserBeam(this);
			}
			this._beaming = true;
			this.beamStrength = eInfo.floatArg;
			return TileResultCode.True;
		}

		// Token: 0x06000728 RID: 1832 RVA: 0x0003072C File Offset: 0x0002EB2C
		public void UpdateLaserHitParticles(Vector3 pos, Vector3 normal, Vector3 inDir, bool emitSparks = true, bool emitSmoke = true)
		{
			if (!this._paused && UnityEngine.Random.value < 0.5f)
			{
				Vector3 normalized = Vector3.Cross(inDir, normal).normalized;
				Vector3 a = Vector3.Cross(normalized, inDir);
				float num = 0.02f + UnityEngine.Random.value * 0.08f;
				float d = 2f + UnityEngine.Random.value * 8f;
				float num2 = -this.smokeRndOffset * 0.5f;
				float num3 = this.smokeRndOffset;
				Vector3 a2 = -inDir + normalized * (num2 + UnityEngine.Random.value * num3) + a * (num2 + UnityEngine.Random.value * num3);
				a2.Normalize();
				if (emitSmoke && UnityEngine.Random.value < 0.5f)
				{
					d = 0.1f + UnityEngine.Random.value * 0.4f;
					num = 1f + UnityEngine.Random.value * 0.5f;
					ParticleSystem.Particle particle = default(ParticleSystem.Particle);
					particle.position = pos;
					particle.velocity = a2 * d;
					particle.size = 0.2f + UnityEngine.Random.value * 0.3f;
					particle.remainingLifetime = num;
					particle.startLifetime = num;
					particle.color = this.gray;
					this.smokeParticleSystem.Emit(particle);
				}
			}
		}

		// Token: 0x06000729 RID: 1833 RVA: 0x00030880 File Offset: 0x0002EC80
		public static void DrawLaserLine(Vector3 from, Vector3 to, Vector3 up, List<Vector3> vertices, List<Vector2> uvs, List<int> triangles, float sizeMult = 1f)
		{
			Vector3 normalized = (to - from).normalized;
			Vector3 normalized2 = Vector3.Cross(normalized, up).normalized;
			float d = 0.25f * sizeMult;
			BlockAbstractLaser.upArr[0] = up;
			BlockAbstractLaser.upArr[1] = normalized2;
			for (int i = 0; i < BlockAbstractLaser.upArr.Length; i++)
			{
				Vector3 a = BlockAbstractLaser.upArr[i];
				int count = vertices.Count;
				vertices.Add(from - a * d);
				uvs.Add(new Vector2(0f, 0f));
				vertices.Add(from + a * d);
				uvs.Add(new Vector2(0f, 1f));
				vertices.Add(to - a * d);
				uvs.Add(new Vector2(1f, 0f));
				vertices.Add(to + a * d);
				uvs.Add(new Vector2(1f, 1f));
				for (int j = 0; j < BlockAbstractLaser.triangleOffsets.Length; j++)
				{
					triangles.Add(count + BlockAbstractLaser.triangleOffsets[j]);
				}
			}
		}

		// Token: 0x0600072A RID: 1834 RVA: 0x000309EC File Offset: 0x0002EDEC
		public static void DrawProjectileLine(Vector3 from, Vector3 to, Vector3 up, List<Vector3> vertices, List<Vector2> uvs, List<int> triangles, float sizeMult = 1f, float startFraction = 0f)
		{
			float num = 0.5f * sizeMult;
			float d = 0.5f * num;
			Transform cameraTransform = Blocksworld.cameraTransform;
			Vector3 position = cameraTransform.position;
			Vector3 rhs = from - position;
			Vector3 lhs = to - from;
			Vector3 normalized = Vector3.Cross(lhs, rhs).normalized;
			int count = vertices.Count;
			vertices.Add(from - normalized * d);
			uvs.Add(new Vector2(startFraction, 0f));
			vertices.Add(from + normalized * d);
			uvs.Add(new Vector2(startFraction, 1f));
			vertices.Add(to - normalized * d);
			uvs.Add(new Vector2(1f, 0f));
			vertices.Add(to + normalized * d);
			uvs.Add(new Vector2(1f, 1f));
			for (int i = 0; i < BlockAbstractLaser.triangleOffsets.Length; i++)
			{
				triangles.Add(count + BlockAbstractLaser.triangleOffsets[i]);
			}
		}

		// Token: 0x0600072B RID: 1835 RVA: 0x00030B18 File Offset: 0x0002EF18
		public static void AddHit(BlockAbstractLaser lb, Block block)
		{
			BlockAbstractLaser.Hits.Add(block);
			bool flag = block.CanTriggerBlockListSensor();
			if (flag)
			{
				BlockAbstractLaser.ModelHits.Add(block.modelBlock);
			}
			List<string> blockTags = TagManager.GetBlockTags(lb);
			for (int i = 0; i < blockTags.Count; i++)
			{
				string key = blockTags[i];
				HashSet<Block> hashSet;
				HashSet<Block> hashSet2;
				if (BlockAbstractLaser.TagHits.ContainsKey(key))
				{
					hashSet = BlockAbstractLaser.TagHits[key];
					hashSet2 = BlockAbstractLaser.ModelTagHits[key];
				}
				else
				{
					hashSet = new HashSet<Block>();
					hashSet2 = new HashSet<Block>();
					BlockAbstractLaser.TagHits.Add(key, hashSet);
					BlockAbstractLaser.ModelTagHits.Add(key, hashSet2);
				}
				hashSet.Add(block);
				if (flag)
				{
					hashSet2.Add(block.modelBlock);
				}
			}
		}

		// Token: 0x0600072C RID: 1836 RVA: 0x00030BF0 File Offset: 0x0002EFF0
		public static void AddProjectileHit(BlockAbstractLaser lb, Block block)
		{
			BlockAbstractLaser.ProjectileHits.Add(block);
			bool flag = block.CanTriggerBlockListSensor();
			if (flag)
			{
				BlockAbstractLaser.ModelProjectileHits.Add(block.modelBlock);
			}
			List<string> blockTags = TagManager.GetBlockTags(lb);
			for (int i = 0; i < blockTags.Count; i++)
			{
				string key = blockTags[i];
				HashSet<Block> hashSet;
				HashSet<Block> hashSet2;
				if (BlockAbstractLaser.TagProjectileHits.ContainsKey(key))
				{
					hashSet = BlockAbstractLaser.TagProjectileHits[key];
					hashSet2 = BlockAbstractLaser.ModelTagProjectileHits[key];
				}
				else
				{
					hashSet = new HashSet<Block>();
					hashSet2 = new HashSet<Block>();
					BlockAbstractLaser.TagProjectileHits.Add(key, hashSet);
					BlockAbstractLaser.ModelTagProjectileHits.Add(key, hashSet2);
				}
				hashSet.Add(block);
				if (flag)
				{
					hashSet2.Add(block.modelBlock);
				}
			}
		}

		// Token: 0x04000526 RID: 1318
		private LaserBeam _beam;

		// Token: 0x04000527 RID: 1319
		private List<AbstractProjectile> projectiles = new List<AbstractProjectile>();

		// Token: 0x04000528 RID: 1320
		private bool _beaming;

		// Token: 0x04000529 RID: 1321
		public float beamStrength = 1f;

		// Token: 0x0400052A RID: 1322
		private bool _pulsing;

		// Token: 0x0400052B RID: 1323
		private bool _firingProjectile;

		// Token: 0x0400052C RID: 1324
		private ParticleSystem smokeParticleSystem;

		// Token: 0x0400052D RID: 1325
		private GameObject smokeParticleSystemGo;

		// Token: 0x0400052E RID: 1326
		private AudioSource ourSource;

		// Token: 0x0400052F RID: 1327
		public static ParticleSystem projectileHitParticleSystem;

		// Token: 0x04000530 RID: 1328
		private ParticleSystem muzzleFlashParticleSystem;

		// Token: 0x04000531 RID: 1329
		private GameObject muzzleFlashParticleSystemGo;

		// Token: 0x04000532 RID: 1330
		private Color32 gray = new Color32(127, 127, 127, byte.MaxValue);

		// Token: 0x04000533 RID: 1331
		public float segmentTime = 100f;

		// Token: 0x04000534 RID: 1332
		private float smokeRndOffset = 2f;

		// Token: 0x04000535 RID: 1333
		public float pulseSpeedMultiplier = 4f;

		// Token: 0x04000536 RID: 1334
		public float pulseFrequencyFraction = 1f;

		// Token: 0x04000537 RID: 1335
		public float projectileSpeedMultiplier = 4f;

		// Token: 0x04000538 RID: 1336
		public float projectileFrequencyFraction = 1f;

		// Token: 0x04000539 RID: 1337
		public int pulsesPerCycle = 2;

		// Token: 0x0400053A RID: 1338
		private float pulseLengthFraction = 0.4f;

		// Token: 0x0400053B RID: 1339
		private float pulseCycleTime = 0.25f;

		// Token: 0x0400053C RID: 1340
		public int projectilesPerCycle = 2;

		// Token: 0x0400053D RID: 1341
		private float projectileLengthFraction = 0.4f;

		// Token: 0x0400053E RID: 1342
		private float projectileCycleTime = 0.25f;

		// Token: 0x0400053F RID: 1343
		public LaserMetaData laserMeta;

		// Token: 0x04000540 RID: 1344
		public float beamSizeMultiplier = 1f;

		// Token: 0x04000541 RID: 1345
		public float pulseSizeMultiplier = 1f;

		// Token: 0x04000542 RID: 1346
		public float projectileSizeMultiplier = 1f;

		// Token: 0x04000543 RID: 1347
		public Vector3 laserExitOffset = Vector3.zero;

		// Token: 0x04000544 RID: 1348
		public Vector3 projectileExitOffset = Vector3.zero;

		// Token: 0x04000545 RID: 1349
		public float pulseLengthMultiplier = 1f;

		// Token: 0x04000546 RID: 1350
		public float pulseFrequencyMultiplier = 1f;

		// Token: 0x04000547 RID: 1351
		public float projectileLengthMultiplier = 1f;

		// Token: 0x04000548 RID: 1352
		public float projectileFrequencyMultiplier = 1f;

		// Token: 0x04000549 RID: 1353
		public bool fixLaserColor;

		// Token: 0x0400054A RID: 1354
		public Color laserColor = Color.white;

		// Token: 0x0400054B RID: 1355
		public BlockAnimatedCharacter animatedCharacter;

		// Token: 0x0400054C RID: 1356
		private LaserPulse _currentPulse;

		// Token: 0x0400054D RID: 1357
		public static HashSet<Block> Hits = new HashSet<Block>();

		// Token: 0x0400054E RID: 1358
		public static HashSet<Block> ModelHits = new HashSet<Block>();

		// Token: 0x0400054F RID: 1359
		private Projectile _currentProjectile;

		// Token: 0x04000550 RID: 1360
		public static HashSet<Block> ProjectileHits = new HashSet<Block>();

		// Token: 0x04000551 RID: 1361
		public static HashSet<Block> ModelProjectileHits = new HashSet<Block>();

		// Token: 0x04000552 RID: 1362
		public static Dictionary<string, HashSet<Block>> TagHits = new Dictionary<string, HashSet<Block>>();

		// Token: 0x04000553 RID: 1363
		public static Dictionary<string, HashSet<Block>> ModelTagHits = new Dictionary<string, HashSet<Block>>();

		// Token: 0x04000554 RID: 1364
		public static Dictionary<string, HashSet<Block>> TagProjectileHits = new Dictionary<string, HashSet<Block>>();

		// Token: 0x04000555 RID: 1365
		public static Dictionary<string, HashSet<Block>> ModelTagProjectileHits = new Dictionary<string, HashSet<Block>>();

		// Token: 0x04000556 RID: 1366
		private bool _paused;

		// Token: 0x04000557 RID: 1367
		private static Dictionary<string, Color> laserColors = null;

		// Token: 0x04000558 RID: 1368
		public static bool anyLaserBeamOrPulseAvailable = false;

		// Token: 0x04000559 RID: 1369
		public static bool anyProjectileAvailable = false;

		// Token: 0x0400055A RID: 1370
		private static HashSet<string> transparentTexturesForLaser = new HashSet<string>
		{
			"Glass",
			"Transparent",
			"Texture Soccer Net",
			"Water",
			"Ice Material",
			"Ice Material_Terrain",
			"Invisible"
		};

		// Token: 0x0400055B RID: 1371
		private static Vector3[] upArr = new Vector3[]
		{
			Vector3.up,
			Vector3.right
		};

		// Token: 0x0400055C RID: 1372
		private static int[] triangleOffsets = new int[]
		{
			0,
			1,
			2,
			2,
			1,
			3,
			2,
			1,
			0,
			3,
			1,
			2
		};
	}
}
