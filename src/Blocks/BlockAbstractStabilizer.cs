using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x0200006A RID: 106
	public abstract class BlockAbstractStabilizer : Block
	{
		// Token: 0x06000894 RID: 2196 RVA: 0x0003BC40 File Offset: 0x0003A040
		public BlockAbstractStabilizer(List<List<Tile>> tiles) : base(tiles)
		{
			this.nozzleInfos = new List<StabilizerNozzleInfo>();
			StabilizerNozzleInfo stabilizerNozzleInfo = new StabilizerNozzleInfo(this, Quaternion.Euler(new Vector3(0f, 0f, 0f)), -1);
			StabilizerNozzleInfo stabilizerNozzleInfo2 = new StabilizerNozzleInfo(this, Quaternion.Euler(new Vector3(180f, 0f, 0f)), 1);
			stabilizerNozzleInfo.otherNozzle = stabilizerNozzleInfo2;
			stabilizerNozzleInfo2.otherNozzle = stabilizerNozzleInfo;
			this.nozzleInfos.Add(stabilizerNozzleInfo);
			this.nozzleInfos.Add(stabilizerNozzleInfo2);
			this.loopName = "Stabilizer Hover Loop";
			this.sfxLoopUpdateCounter = UnityEngine.Random.Range(0, 5);
		}

		// Token: 0x06000895 RID: 2197 RVA: 0x0003BCF1 File Offset: 0x0003A0F1
		public override void SetVaryingGravity(bool vg)
		{
			this.varyingGravity = vg;
		}

		// Token: 0x06000896 RID: 2198 RVA: 0x0003BCFC File Offset: 0x0003A0FC
		public TileResultCode BoostStabilizer(ScriptRowExecutionInfo eInfo, object[] args)
		{
			for (int i = 0; i < this.nozzleInfos.Count; i++)
			{
				StabilizerNozzleInfo stabilizerNozzleInfo = this.nozzleInfos[i];
				stabilizerNozzleInfo.BoostStabilizer();
			}
			return TileResultCode.True;
		}

		// Token: 0x06000897 RID: 2199 RVA: 0x0003BD3C File Offset: 0x0003A13C
		public TileResultCode Stabilize(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float floatArg = Util.GetFloatArg(args, 0, 1f);
			for (int i = 0; i < this.nozzleInfos.Count; i++)
			{
				StabilizerNozzleInfo stabilizerNozzleInfo = this.nozzleInfos[i];
				stabilizerNozzleInfo.Stabilize(eInfo.floatArg * floatArg);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000898 RID: 2200 RVA: 0x0003BD90 File Offset: 0x0003A190
		public TileResultCode StabilizePlane(ScriptRowExecutionInfo eInfo, object[] args)
		{
			for (int i = 0; i < this.nozzleInfos.Count; i++)
			{
				StabilizerNozzleInfo stabilizerNozzleInfo = this.nozzleInfos[i];
				stabilizerNozzleInfo.StabilizePlane(eInfo.floatArg);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000899 RID: 2201 RVA: 0x0003BDD4 File Offset: 0x0003A1D4
		public TileResultCode ControlZeroAngVel(ScriptRowExecutionInfo eInfo, object[] args)
		{
			for (int i = 0; i < this.nozzleInfos.Count; i++)
			{
				StabilizerNozzleInfo stabilizerNozzleInfo = this.nozzleInfos[i];
				stabilizerNozzleInfo.ControlZeroAngVel(eInfo.floatArg);
			}
			return TileResultCode.True;
		}

		// Token: 0x0600089A RID: 2202 RVA: 0x0003BE18 File Offset: 0x0003A218
		public TileResultCode ControlPosition(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float floatArg = Util.GetFloatArg(args, 0, 1f);
			for (int i = 0; i < this.nozzleInfos.Count; i++)
			{
				StabilizerNozzleInfo stabilizerNozzleInfo = this.nozzleInfos[i];
				stabilizerNozzleInfo.ControlPosition(eInfo.floatArg, floatArg);
			}
			return TileResultCode.True;
		}

		// Token: 0x0600089B RID: 2203 RVA: 0x0003BE6C File Offset: 0x0003A26C
		private void ChangePosition(float amount)
		{
			for (int i = 0; i < this.nozzleInfos.Count; i++)
			{
				StabilizerNozzleInfo stabilizerNozzleInfo = this.nozzleInfos[i];
				stabilizerNozzleInfo.ChangePosition(amount);
			}
		}

		// Token: 0x0600089C RID: 2204 RVA: 0x0003BEA9 File Offset: 0x0003A2A9
		public TileResultCode IncreasePosition(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.ChangePosition(0.1f);
			return TileResultCode.True;
		}

		// Token: 0x0600089D RID: 2205 RVA: 0x0003BEB7 File Offset: 0x0003A2B7
		public TileResultCode DecreasePosition(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.ChangePosition(-0.1f);
			return TileResultCode.True;
		}

		// Token: 0x0600089E RID: 2206 RVA: 0x0003BEC8 File Offset: 0x0003A2C8
		public TileResultCode Burst(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float num = (float)args[0] * eInfo.floatArg;
			bool flag = num > 0f;
			this.nozzleInfos[0].IncreaseForce((!flag) ? 0f : Mathf.Abs(num));
			this.nozzleInfos[1].IncreaseForce(flag ? 0f : Mathf.Abs(num));
			if (!this.didFix && !this.broken)
			{
				Vector3 a = this.goT.up * num;
				Vector3 vector = a * 2f;
				vector = Util.ProjectOntoPlane(vector, Vector3.up);
				Blocksworld.blocksworldCamera.AddForceDirectionHint(this.chunk, vector);
				BlockAccelerations.BlockAccelerates(this, a);
			}
			return TileResultCode.True;
		}

		// Token: 0x0600089F RID: 2207 RVA: 0x0003BF98 File Offset: 0x0003A398
		public TileResultCode IsBursting(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float num = (float)args[0];
			if ((num > 0f && this._burstingRight) || (num < 0f && this._burstingLeft))
			{
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x060008A0 RID: 2208 RVA: 0x0003BFE0 File Offset: 0x0003A3E0
		public TileResultCode IsStabilizing(ScriptRowExecutionInfo eInfo, object[] args)
		{
			for (int i = 0; i < this.nozzleInfos.Count; i++)
			{
				StabilizerNozzleInfo stabilizerNozzleInfo = this.nozzleInfos[i];
				if (stabilizerNozzleInfo.lastMode == BlockStabilizerMode.STABILIZE && stabilizerNozzleInfo.stabilizing)
				{
					return TileResultCode.True;
				}
			}
			return TileResultCode.False;
		}

		// Token: 0x060008A1 RID: 2209 RVA: 0x0003C030 File Offset: 0x0003A430
		public TileResultCode IsCloseToSomething(ScriptRowExecutionInfo eInfo, object[] args)
		{
			for (int i = 0; i < this.nozzleInfos.Count; i++)
			{
				StabilizerNozzleInfo stabilizerNozzleInfo = this.nozzleInfos[i];
				if (stabilizerNozzleInfo.canHover)
				{
					if (stabilizerNozzleInfo.hovering)
					{
						return TileResultCode.True;
					}
				}
				else
				{
					Transform transform = stabilizerNozzleInfo.flame.transform;
					Vector3 direction = -transform.up;
					RaycastHit[] array = Physics.RaycastAll(transform.position, direction, stabilizerNozzleInfo.hoverHeight);
					Array.Sort<RaycastHit>(array, new RaycastDistanceComparer(transform.position));
					bool flag = false;
					int j = 0;
					while (j < array.Length)
					{
						RaycastHit raycastHit = array[j];
						Block block = BWSceneManager.FindBlock(raycastHit.collider.gameObject, false);
						if (block != null)
						{
							flag = (this.goT.parent != block.goT.parent);
						}
						if (flag)
						{
							float num = Vector3.Distance(raycastHit.point, transform.position);
							if (num < stabilizerNozzleInfo.hoverHeight)
							{
								return TileResultCode.True;
							}
							break;
						}
						else
						{
							j++;
						}
					}
				}
			}
			return TileResultCode.False;
		}

		// Token: 0x060008A2 RID: 2210 RVA: 0x0003C158 File Offset: 0x0003A558
		private void ChangeAngle(float amount)
		{
			for (int i = 0; i < this.nozzleInfos.Count; i++)
			{
				StabilizerNozzleInfo stabilizerNozzleInfo = this.nozzleInfos[i];
				stabilizerNozzleInfo.ChangeAngVel(amount);
			}
		}

		// Token: 0x060008A3 RID: 2211 RVA: 0x0003C195 File Offset: 0x0003A595
		public TileResultCode IncreaseAngle(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.ChangeAngle(0.1f);
			return TileResultCode.True;
		}

		// Token: 0x060008A4 RID: 2212 RVA: 0x0003C1A3 File Offset: 0x0003A5A3
		public TileResultCode DecreaseAngle(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.ChangeAngle(-0.1f);
			return TileResultCode.True;
		}

		// Token: 0x060008A5 RID: 2213 RVA: 0x0003C1B4 File Offset: 0x0003A5B4
		public override void Destroy()
		{
			for (int i = 0; i < this.nozzleInfos.Count; i++)
			{
				StabilizerNozzleInfo stabilizerNozzleInfo = this.nozzleInfos[i];
				stabilizerNozzleInfo.Destroy();
			}
			base.Destroy();
		}

		// Token: 0x060008A6 RID: 2214 RVA: 0x0003C1F8 File Offset: 0x0003A5F8
		public override void Play()
		{
			base.Play();
			this.treatAsVehicleStatus = -1;
			for (int i = 0; i < this.nozzleInfos.Count; i++)
			{
				StabilizerNozzleInfo stabilizerNozzleInfo = this.nozzleInfos[i];
				stabilizerNozzleInfo.Play();
			}
			this.SetVaryingGravity(false);
			this.currentLoopVolume = 0f;
		}

		// Token: 0x060008A7 RID: 2215 RVA: 0x0003C254 File Offset: 0x0003A654
		public override void Play2()
		{
			this.mass = Bunch.GetModelMass(this);
			for (int i = 0; i < this.nozzleInfos.Count; i++)
			{
				StabilizerNozzleInfo stabilizerNozzleInfo = this.nozzleInfos[i];
				stabilizerNozzleInfo.SetMass(this.mass);
			}
		}

		// Token: 0x060008A8 RID: 2216 RVA: 0x0003C2A4 File Offset: 0x0003A6A4
		public override void Stop(bool resetBlock = true)
		{
			for (int i = 0; i < this.nozzleInfos.Count; i++)
			{
				StabilizerNozzleInfo stabilizerNozzleInfo = this.nozzleInfos[i];
				stabilizerNozzleInfo.Stop();
			}
			base.Stop(resetBlock);
			this.wasBroken = false;
		}

		// Token: 0x060008A9 RID: 2217 RVA: 0x0003C2F0 File Offset: 0x0003A6F0
		public override void Pause()
		{
			for (int i = 0; i < this.nozzleInfos.Count; i++)
			{
				StabilizerNozzleInfo stabilizerNozzleInfo = this.nozzleInfos[i];
				stabilizerNozzleInfo.Pause();
			}
		}

		// Token: 0x060008AA RID: 2218 RVA: 0x0003C32C File Offset: 0x0003A72C
		public override void Resume()
		{
			for (int i = 0; i < this.nozzleInfos.Count; i++)
			{
				StabilizerNozzleInfo stabilizerNozzleInfo = this.nozzleInfos[i];
				stabilizerNozzleInfo.Resume();
			}
		}

		// Token: 0x060008AB RID: 2219 RVA: 0x0003C368 File Offset: 0x0003A768
		public override void ResetFrame()
		{
			for (int i = 0; i < this.nozzleInfos.Count; i++)
			{
				StabilizerNozzleInfo stabilizerNozzleInfo = this.nozzleInfos[i];
				stabilizerNozzleInfo.ResetFrame();
			}
			this._burstingLeft = false;
			this._burstingRight = false;
		}

		// Token: 0x060008AC RID: 2220 RVA: 0x0003C3B4 File Offset: 0x0003A7B4
		private void UpdateLoopSound()
		{
			if (Blocksworld.CurrentState == State.Play && Sound.sfxEnabled && !this.vanished)
			{
				bool flag = false;
				float num = 0f;
				for (int i = 0; i < this.nozzleInfos.Count; i++)
				{
					StabilizerNozzleInfo stabilizerNozzleInfo = this.nozzleInfos[i];
					flag = (flag || stabilizerNozzleInfo.lastMode != BlockStabilizerMode.OFF);
					if (flag)
					{
						num = Mathf.Max(num, stabilizerNozzleInfo.lastBurstAmount);
					}
				}
				if (num < 0.11f)
				{
				}
				float num2 = num - 0.15f;
				float num3 = this.currentLoopVolume;
				float num4 = 0.02f;
				if (num3 < num2 && !this.broken && !this.vanished)
				{
					num3 += num4;
				}
				else
				{
					num3 -= num4;
				}
				num3 = Mathf.Clamp(num3, 0f, 0.5f);
				if (this.sfxLoopUpdateCounter % 5 == 0)
				{
					base.UpdateWithinWaterLPFilter(null);
					AudioSource component = this.go.GetComponent<AudioSource>();
					this.PlayLoopSound(num3 > 0.01f, base.GetLoopClip(), num3, component, 1f + 0.5f * num2);
				}
				this.currentLoopVolume = num3;
				this.sfxLoopUpdateCounter++;
			}
			else
			{
				this.PlayLoopSound(false, base.GetLoopClip(), 1f, null, 1f);
			}
		}

		// Token: 0x060008AD RID: 2221 RVA: 0x0003C520 File Offset: 0x0003A920
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (this.chunk != null && this.chunk.mobileCharacter != null)
			{
				this.chunk.mobileCharacter.isHovering = true;
			}
			for (int i = 0; i < this.nozzleInfos.Count; i++)
			{
				StabilizerNozzleInfo stabilizerNozzleInfo = this.nozzleInfos[i];
				stabilizerNozzleInfo.UpdateTransform();
			}
			if (!this.wasBroken && this.broken && this.chunk.go != null)
			{
				Rigidbody rb = this.chunk.rb;
				if (rb != null)
				{
					for (int j = 0; j < this.nozzleInfos.Count; j++)
					{
						StabilizerNozzleInfo stabilizerNozzleInfo2 = this.nozzleInfos[j];
						stabilizerNozzleInfo2.SetMass(rb.mass);
					}
				}
			}
			this.wasBroken = this.broken;
			for (int k = 0; k < this.nozzleInfos.Count; k++)
			{
				StabilizerNozzleInfo stabilizerNozzleInfo3 = this.nozzleInfos[k];
				if ((double)stabilizerNozzleInfo3.FixedUpdate(this.broken, this.vanished).sqrMagnitude > 0.01)
				{
					if (stabilizerNozzleInfo3.sign == 1)
					{
						this._burstingLeft = true;
					}
					else if (stabilizerNozzleInfo3.sign == -1)
					{
						this._burstingRight = true;
					}
				}
			}
			this.UpdateLoopSound();
		}

		// Token: 0x060008AE RID: 2222 RVA: 0x0003C6A2 File Offset: 0x0003AAA2
		public override bool TreatAsVehicleLikeBlock()
		{
			return base.TreatAsVehicleLikeBlockWithStatus(ref this.treatAsVehicleStatus);
		}

		// Token: 0x04000696 RID: 1686
		private List<StabilizerNozzleInfo> nozzleInfos;

		// Token: 0x04000697 RID: 1687
		private float mass = 1f;

		// Token: 0x04000698 RID: 1688
		private bool wasBroken;

		// Token: 0x04000699 RID: 1689
		private int treatAsVehicleStatus = -1;

		// Token: 0x0400069A RID: 1690
		private bool _burstingLeft;

		// Token: 0x0400069B RID: 1691
		private bool _burstingRight;

		// Token: 0x0400069C RID: 1692
		public bool varyingGravity;

		// Token: 0x0400069D RID: 1693
		private int sfxLoopUpdateCounter;

		// Token: 0x0400069E RID: 1694
		private const int SFX_LOOP_UPDATE_INTERVAL = 5;

		// Token: 0x0400069F RID: 1695
		private float currentLoopVolume;
	}
}
