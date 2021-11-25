using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x0200006D RID: 109
	public class BlockAbstractTorsionSpring : Block
	{
		// Token: 0x060008C3 RID: 2243 RVA: 0x0003D6A4 File Offset: 0x0003BAA4
		public BlockAbstractTorsionSpring(List<List<Tile>> tiles, string axleName, float angleLimit) : base(tiles)
		{
			this.axle = this.goT.Find(axleName).gameObject;
			this.loopName = "Torsion Spring Loop";
			this.sfxLoopUpdateCounter = UnityEngine.Random.Range(0, 5);
			this.springLimit = angleLimit;
		}

		// Token: 0x060008C4 RID: 2244 RVA: 0x0003D74C File Offset: 0x0003BB4C
		private float GetAngleIncPerSecond(object[] args)
		{
			return (args.Length <= 0) ? 15f : ((float)args[0]);
		}

		// Token: 0x060008C5 RID: 2245 RVA: 0x0003D778 File Offset: 0x0003BB78
		private void Charge(object[] args, float arg1 = 1f)
		{
			if (this.joint == null)
			{
				return;
			}
			float num = (this.springLimit <= 0f) ? 90f : this.springLimit;
			float num2 = arg1 * Blocksworld.fixedDeltaTime * this.GetAngleIncPerSecond(args);
			this.angleOffset = Mathf.Clamp(this.angleOffset + num2, -num, num);
			this.charging = (Mathf.Abs(this.angleOffset) < 89f);
			this.mode = BlockAbstractTorsionSpring.TorsionSpringMode.Spring;
			if (this._limitedRange)
			{
				float limitedRangeRealAngle = this._limitedRangeRealAngle;
				if (this.charging)
				{
					this._limitedRangeRealAngle = Mathf.Clamp(this.angleOffset, -num, num);
				}
				else
				{
					this._limitedRangeRealAngle = Mathf.Clamp(num2 - this.GetRealAngle(), -num, num);
				}
				if (limitedRangeRealAngle != this._limitedRangeRealAngle)
				{
					this.SetSpringLimits(this._limitedRangeRealAngle - this._limitedRangeAngle, this._limitedRangeRealAngle + this._limitedRangeAngle);
				}
			}
		}

		// Token: 0x060008C6 RID: 2246 RVA: 0x0003D875 File Offset: 0x0003BC75
		public TileResultCode SetSpringStiffness(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.springStiffness = Util.GetFloatArg(args, 0, 1f);
			return TileResultCode.True;
		}

		// Token: 0x060008C7 RID: 2247 RVA: 0x0003D88C File Offset: 0x0003BC8C
		public TileResultCode SetRigidity(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.joint == null)
			{
				return TileResultCode.True;
			}
			float num = (args.Length <= 0) ? 1f : ((float)args[0]);
			float num2 = 90f;
			if (num > 0.9f)
			{
				num2 = 0.05f;
			}
			else if (num > 0.8f)
			{
				num2 = 0.25f;
			}
			else if (num > 0.7f)
			{
				num2 = 0.7f;
			}
			else if (num > 0.6f)
			{
				num2 = 1.4f;
			}
			else if (num > 0.5f)
			{
				num2 = 2.5f;
			}
			else if (num > 0.4f)
			{
				num2 = 4.75f;
			}
			else if (num > 0.3f)
			{
				num2 = 7f;
			}
			else if (num > 0.2f)
			{
				num2 = 10f;
			}
			else if (num > 0.1f)
			{
				num2 = 14f;
			}
			else if (num > 0f)
			{
				num2 = 20f;
			}
			else if (this.springLimit > 0f)
			{
				num2 = this.springLimit;
			}
			if (this._limitedRange || num > 0f)
			{
				bool flag = false;
				if (!this._limitedRange)
				{
					flag = true;
					this._limitedRange = true;
					this._limitedRangeAngle = num2;
					this._limitedRangeRealAngle = -this.GetRealAngle();
				}
				else if (this._limitedRangeAngle != num2)
				{
					flag = true;
					this._limitedRangeAngle = num2;
				}
				if (flag)
				{
					this.SetSpringLimits(this._limitedRangeRealAngle - this._limitedRangeAngle, this._limitedRangeRealAngle + this._limitedRangeAngle);
				}
			}
			else if (this.springLimit > 0f)
			{
				this.SetSpringLimits(-this.springLimit, this.springLimit);
			}
			else
			{
				this.SetSpringLimits(-90f, 90f);
			}
			if (this.springLimit <= 0f)
			{
				if (this._limitedRange)
				{
					this.joint.angularXMotion = ConfigurableJointMotion.Limited;
				}
				else
				{
					this.joint.angularXMotion = ConfigurableJointMotion.Free;
				}
			}
			return TileResultCode.True;
		}

		// Token: 0x060008C8 RID: 2248 RVA: 0x0003DAB7 File Offset: 0x0003BEB7
		public TileResultCode Charge(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.Charge(args, eInfo.floatArg);
			return TileResultCode.True;
		}

		// Token: 0x060008C9 RID: 2249 RVA: 0x0003DAC7 File Offset: 0x0003BEC7
		public TileResultCode StepCharge(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (eInfo.timer >= 0.25f)
			{
				return TileResultCode.True;
			}
			this.Charge(args, eInfo.floatArg);
			return TileResultCode.Delayed;
		}

		// Token: 0x060008CA RID: 2250 RVA: 0x0003DAEC File Offset: 0x0003BEEC
		public TileResultCode FreeSpin(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.joint == null)
			{
				return TileResultCode.True;
			}
			this.mode = BlockAbstractTorsionSpring.TorsionSpringMode.FreeSpin;
			if (this._limitedRange)
			{
				if (this.springLimit > 0f)
				{
					this.SetSpringLimits(-this.springLimit, this.springLimit);
				}
				else
				{
					this.SetSpringLimits(-90f, 90f);
				}
			}
			this._limitedRange = false;
			this._limitedRangeAngle = 0.05f;
			this._limitedRangeRealAngle = 0f;
			return TileResultCode.True;
		}

		// Token: 0x060008CB RID: 2251 RVA: 0x0003DB74 File Offset: 0x0003BF74
		public TileResultCode Release(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.joint == null)
			{
				return TileResultCode.True;
			}
			this.released = (Mathf.Abs(this.angleOffset) > 0f);
			this.angleOffset = 0f;
			this.mode = BlockAbstractTorsionSpring.TorsionSpringMode.Spring;
			if (this._limitedRange)
			{
				this.SetSpringLimits(-this._limitedRangeAngle, this._limitedRangeAngle);
				this._limitedRangeRealAngle = 0f;
			}
			return TileResultCode.True;
		}

		// Token: 0x060008CC RID: 2252 RVA: 0x0003DBE8 File Offset: 0x0003BFE8
		public TileResultCode ChargeGreaterThan(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float num = (args.Length <= 0) ? 45f : ((float)args[0]);
			bool flag = args.Length <= 1 || (bool)args[1];
			if (((!flag) ? this.angleOffset : Mathf.Abs(this.angleOffset)) >= num)
			{
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x060008CD RID: 2253 RVA: 0x0003DC4E File Offset: 0x0003C04E
		public override void Play2()
		{
			base.Play2();
			base.CreateFakeRigidbodyBetweenJoints();
		}

		// Token: 0x060008CE RID: 2254 RVA: 0x0003DC5C File Offset: 0x0003C05C
		public override void Play()
		{
			base.Play();
			this.treatAsVehicleStatus = -1;
			List<Block> list = base.ConnectionsOfType(2, true);
			this.fakeRotor = null;
			if (this.jointToJointConnection != null)
			{
				this.rotor = this.jointToJointConnection.gameObject;
			}
			else if (list.Count > 0)
			{
				Chunk chunk = list[0].chunk;
				this.rotorChunk = chunk;
				this.rotor = this.rotorChunk.go;
				ConfigurableJoint[] components = this.chunk.go.GetComponents<ConfigurableJoint>();
				if (components.Length > 0)
				{
					this.jointToJointConnection = chunk.rb;
					this.jointOffset = this.goT.localEulerAngles;
				}
			}
			else
			{
				this.fakeRotor = new GameObject(this.go.name + " Fake Rotor");
				this.fakeRotor.transform.position = this.goT.position;
				Rigidbody rigidbody = this.fakeRotor.AddComponent<Rigidbody>();
				if (Blocksworld.interpolateRigidBodies)
				{
					rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
				}
				rigidbody.mass = 1f;
				this.axle.transform.parent = this.fakeRotor.transform;
				this.rotor = this.fakeRotor;
			}
			this.rotorT = this.rotor.transform;
			this.axleAudioSource = this.axle.GetComponent<AudioSource>();
			if (this.axleAudioSource == null)
			{
				this.axleAudioSource = this.axle.AddComponent<AudioSource>();
				this.axleAudioSource.playOnAwake = false;
				this.axleAudioSource.clip = Sound.GetSfx("Torsion Spring Stretch");
				Sound.SetWorldAudioSourceParams(this.axleAudioSource, 5f, 150f, AudioRolloffMode.Logarithmic);
			}
			this.CreateJoint();
			this.targetAngle = 0f;
			this.currentAngle = 0f;
			this.lastRealAngle = 0f;
			this._limitedRange = false;
			this._limitedRangeAngle = 0.05f;
			this.CalculateMassDistributions();
			this.angleOffset = 0f;
			this.lockedAngleOffset = 0f;
			this.locked = false;
			this.wasLocked = false;
			this.halfRevolutions = 0;
			this.CalculateRotorAngleVecType();
			this.mode = BlockAbstractTorsionSpring.TorsionSpringMode.Spring;
		}

		// Token: 0x060008CF RID: 2255 RVA: 0x0003DE98 File Offset: 0x0003C298
		private void SetSpringLimits(float lowLimit, float highLimit)
		{
			SoftJointLimit softJointLimit = default(SoftJointLimit);
			softJointLimit.bounciness = 0f;
			softJointLimit.limit = lowLimit;
			this.joint.lowAngularXLimit = softJointLimit;
			softJointLimit.bounciness = 0f;
			softJointLimit.limit = highLimit;
			this.joint.highAngularXLimit = softJointLimit;
		}

		// Token: 0x060008D0 RID: 2256 RVA: 0x0003DEF0 File Offset: 0x0003C2F0
		private void CreateJoint()
		{
			if (this.chunk.go == this.rotor)
			{
				return;
			}
			this.joint = this.chunk.go.AddComponent<ConfigurableJoint>();
			this.joint.anchor = this.goT.localPosition;
			this.joint.axis = this.goT.right;
			this.joint.xMotion = ConfigurableJointMotion.Locked;
			this.joint.yMotion = ConfigurableJointMotion.Locked;
			this.joint.zMotion = ConfigurableJointMotion.Locked;
			if (this.springLimit > 0f)
			{
				this.joint.angularXMotion = ConfigurableJointMotion.Limited;
			}
			else
			{
				this.joint.angularXMotion = ConfigurableJointMotion.Free;
			}
			this.joint.angularYMotion = ConfigurableJointMotion.Locked;
			this.joint.angularZMotion = ConfigurableJointMotion.Locked;
			this.joint.connectedBody = this.rotor.GetComponent<Rigidbody>();
			if (this.springLimit > 0f)
			{
				this.SetSpringLimits(-this.springLimit, this.springLimit);
			}
		}

		// Token: 0x060008D1 RID: 2257 RVA: 0x0003E000 File Offset: 0x0003C400
		private void CalculateMassDistributions()
		{
			Chunk chunk = this.chunk;
			this.chunkMi = base.CalculateMassDistribution(chunk, this.goT.up, this.rotorChunk);
			this.chunkKinematic = base.ConnectedToKinematicChunk(chunk, this.rotorChunk);
			this.rotorChunkMi = this.chunkMi;
			if (this.rotorChunk != null)
			{
				this.rotorChunkMi = base.CalculateMassDistribution(this.rotorChunk, this.goT.up, chunk);
				this.rotorChunkKinematic = base.ConnectedToKinematicChunk(this.rotorChunk, chunk);
			}
		}

		// Token: 0x060008D2 RID: 2258 RVA: 0x0003E090 File Offset: 0x0003C490
		public override void ChunkInModelFrozen()
		{
			base.ChunkInModelFrozen();
			this.chunkKinematic = base.ConnectedToKinematicChunk(this.chunk, this.rotorChunk);
			if (this.rotorChunk != null)
			{
				this.rotorChunkKinematic = base.ConnectedToKinematicChunk(this.rotorChunk, this.chunk);
			}
		}

		// Token: 0x060008D3 RID: 2259 RVA: 0x0003E0E0 File Offset: 0x0003C4E0
		public override void ChunkInModelUnfrozen()
		{
			base.ChunkInModelUnfrozen();
			this.chunkKinematic = base.ConnectedToKinematicChunk(this.chunk, this.rotorChunk);
			if (this.rotorChunk != null)
			{
				this.rotorChunkKinematic = base.ConnectedToKinematicChunk(this.rotorChunk, this.chunk);
			}
		}

		// Token: 0x060008D4 RID: 2260 RVA: 0x0003E12E File Offset: 0x0003C52E
		private void DestroyJoint()
		{
			if (this.joint != null)
			{
				UnityEngine.Object.Destroy(this.joint);
				this.joint = null;
				base.DestroyFakeRigidbodies();
			}
		}

		// Token: 0x060008D5 RID: 2261 RVA: 0x0003E15C File Offset: 0x0003C55C
		public override void Stop(bool resetBlock = true)
		{
			this.DestroyJoint();
			if (this.fakeRotor != null)
			{
				if (this.axle.GetComponent<Collider>() != null)
				{
					UnityEngine.Object.Destroy(this.axle.GetComponent<Collider>());
				}
				this.DestroyFakeRotor();
			}
			this.jointToJointConnection = null;
			this.axle.transform.localRotation = default(Quaternion);
			this.axle.transform.localScale = Vector3.one;
			this.axle.GetComponent<Renderer>().enabled = true;
			this.PlayLoopSound(false, base.GetLoopClip(), 1f, null, 1f);
			base.Stop(resetBlock);
		}

		// Token: 0x060008D6 RID: 2262 RVA: 0x0003E214 File Offset: 0x0003C614
		public override void Pause()
		{
			if (this.fakeRotor != null)
			{
				this.pausedVelocityAxle = this.fakeRotor.GetComponent<Rigidbody>().velocity;
				this.pausedAngularVelocityAxle = this.fakeRotor.GetComponent<Rigidbody>().angularVelocity;
				this.fakeRotor.GetComponent<Rigidbody>().isKinematic = true;
			}
			this.PlayLoopSound(false, base.GetLoopClip(), 1f, null, 1f);
		}

		// Token: 0x060008D7 RID: 2263 RVA: 0x0003E288 File Offset: 0x0003C688
		public override void Resume()
		{
			if (this.fakeRotor != null)
			{
				this.fakeRotor.GetComponent<Rigidbody>().isKinematic = false;
				this.fakeRotor.GetComponent<Rigidbody>().velocity = this.pausedVelocityAxle;
				this.fakeRotor.GetComponent<Rigidbody>().angularVelocity = this.pausedAngularVelocityAxle;
			}
		}

		// Token: 0x060008D8 RID: 2264 RVA: 0x0003E2E4 File Offset: 0x0003C6E4
		public override void Break(Vector3 chunkPos, Vector3 chunkVel, Vector3 chunkAngVel)
		{
			base.Break(chunkPos, chunkVel, chunkAngVel);
			if (this.fakeRotor != null)
			{
				this.axle.AddComponent<BoxCollider>();
				Block.AddExplosiveForce(this.fakeRotor.GetComponent<Rigidbody>(), this.fakeRotor.transform.position, chunkPos, chunkVel, chunkAngVel, 1f);
			}
			this.DestroyJoint();
		}

		// Token: 0x060008D9 RID: 2265 RVA: 0x0003E348 File Offset: 0x0003C748
		public override void Update()
		{
			base.Update();
			if (Blocksworld.CurrentState == State.Play && this.fakeRotor == null && this.rotor != null && !this.vanished)
			{
				if (this.jointToJointConnection != null)
				{
					Quaternion rotation = (!(this.jointOffset == Vector3.zero)) ? (this.jointToJointConnection.rotation * Quaternion.Euler(this.jointOffset)) : Quaternion.LookRotation(this.jointToJointConnection.transform.position - this.goT.position, this.axle.transform.up);
					this.axle.transform.rotation = rotation;
				}
				else
				{
					float realAngle = this.GetRealAngle();
					this.axle.transform.localRotation = Quaternion.Euler(new Vector3(-realAngle, 0f, 0f));
				}
			}
		}

		// Token: 0x060008DA RID: 2266 RVA: 0x0003E454 File Offset: 0x0003C854
		private float GetRealAngle()
		{
			Vector3 forward = this.goT.forward;
			Vector3 planeNormal = Vector3.Cross(forward, this.goT.up);
			Vector3 vector = this.GetRotorAngleVec();
			vector = Util.ProjectOntoPlane(vector, planeNormal).normalized;
			return Util.AngleBetween(vector, forward, this.goT.right);
		}

		// Token: 0x060008DB RID: 2267 RVA: 0x0003E4A8 File Offset: 0x0003C8A8
		private void CheckAndSet(Vector3 v2, int type)
		{
			Vector3 forward = this.goT.forward;
			float f = Vector3.Dot(forward, v2);
			float num = Mathf.Sign(f);
			if (Mathf.Abs(f) > 0.5f)
			{
				this.rotorAngleVecType = type;
				this.rotorAngleVecMultiplier = num;
			}
		}

		// Token: 0x060008DC RID: 2268 RVA: 0x0003E4EE File Offset: 0x0003C8EE
		private void CalculateRotorAngleVecType()
		{
			this.CheckAndSet(this.rotorT.forward, 0);
			this.CheckAndSet(this.rotorT.up, 1);
			this.CheckAndSet(this.rotorT.right, 2);
		}

		// Token: 0x060008DD RID: 2269 RVA: 0x0003E528 File Offset: 0x0003C928
		public Vector3 GetRotorAngleVec()
		{
			if (this.rotorT == null)
			{
				BWLog.Info("rotorT null!");
				return this.rotorAngleVecMultiplier * Vector3.forward;
			}
			int num = this.rotorAngleVecType;
			if (num == 0)
			{
				return this.rotorAngleVecMultiplier * this.rotorT.forward;
			}
			if (num == 1)
			{
				return this.rotorAngleVecMultiplier * this.rotorT.up;
			}
			if (num != 2)
			{
				return this.rotorAngleVecMultiplier * this.rotorT.forward;
			}
			return this.rotorAngleVecMultiplier * this.rotorT.right;
		}

		// Token: 0x060008DE RID: 2270 RVA: 0x0003E5DC File Offset: 0x0003C9DC
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			this.UpdateTorsionSpringLoopSound();
			if (this.isTreasure)
			{
				return;
			}
			if (this.joint == null)
			{
				return;
			}
			if (this.joint.connectedBody == null)
			{
				this.DestroyJoint();
				return;
			}
			if (this.rotor == null)
			{
				this.SetMotorVelocity(0f, 10f);
				return;
			}
			float realAngle = this.GetRealAngle();
			float f = realAngle - this.lastRealAngle;
			if (Mathf.Abs(f) > 90f)
			{
				if (this.lastRealAngle < 0f)
				{
					this.halfRevolutions--;
				}
				else
				{
					this.halfRevolutions++;
				}
			}
			this.lastRealAngle = realAngle;
			this.currentAngle = realAngle + (float)this.halfRevolutions * 360f;
			if (this.locked && !this.wasLocked)
			{
				this.lockedAngleOffset = this.angleOffset;
			}
			if (!this.locked)
			{
				this.currentAngle += this.angleOffset;
				this.lockedAngleOffset = 0f;
			}
			else
			{
				this.currentAngle += this.lockedAngleOffset;
			}
			float f2 = this.targetAngle - this.currentAngle;
			if (this.released && Sound.sfxEnabled && !this.vanished)
			{
				this.axleAudioSource.pitch = 0.8f + UnityEngine.Random.value * 0.4f;
				this.axleAudioSource.PlayOneShot(Sound.GetSfx("Torsion Spring Stretch"), 0.5f);
			}
			float num = Mathf.Max(this.chunkMi, this.rotorChunkMi);
			Rigidbody rb = this.chunk.rb;
			if (rb != null && !this.chunkKinematic)
			{
				num = Mathf.Min(num, this.chunkMi);
			}
			Rigidbody component = this.rotor.GetComponent<Rigidbody>();
			if (component != null && !this.rotorChunkKinematic)
			{
				num = Mathf.Min(num, this.rotorChunkMi);
			}
			float num2 = 0.07f * Mathf.Sqrt(3f * num + 0.3f);
			float maxForce = 0f;
			float v = 0f;
			BlockAbstractTorsionSpring.TorsionSpringMode torsionSpringMode = this.mode;
			if (torsionSpringMode != BlockAbstractTorsionSpring.TorsionSpringMode.FreeSpin)
			{
				if (torsionSpringMode == BlockAbstractTorsionSpring.TorsionSpringMode.Spring)
				{
					maxForce = num2 * this.springStiffness * Mathf.Min(1200f, Mathf.Abs(f2));
					v = Mathf.Sign(f2) * 50f;
				}
			}
			else if (Mathf.Abs(f2) >= 90f)
			{
				maxForce = num2 * this.springStiffness * (Mathf.Abs(f2) - 90f);
				v = Mathf.Sign(f2) * 50f;
			}
			this.SetMotorVelocity(v, maxForce);
			this.wasLocked = this.locked;
			this.locked = false;
			this.released = false;
			this.charging = false;
		}

		// Token: 0x060008DF RID: 2271 RVA: 0x0003E8D8 File Offset: 0x0003CCD8
		private void SetMotorVelocity(float v, float maxForce = 1000f)
		{
			JointDrive angularXDrive = default(JointDrive);
			float num = maxForce * 100f * 0.5f;
			angularXDrive.maximumForce = num;
			angularXDrive.positionDamper = ((this.mode != BlockAbstractTorsionSpring.TorsionSpringMode.FreeSpin) ? num : 0f);
			this.joint.targetAngularVelocity = new Vector3(v, 0f, 0f);
			this.joint.angularXDrive = angularXDrive;
			Rigidbody rb = this.chunk.rb;
			if (rb.IsSleeping())
			{
				rb.WakeUp();
			}
			Rigidbody connectedBody = this.joint.connectedBody;
			if (connectedBody.IsSleeping())
			{
				connectedBody.WakeUp();
			}
		}

		// Token: 0x060008E0 RID: 2272 RVA: 0x0003E984 File Offset: 0x0003CD84
		private void UpdateTorsionSpringLoopSound()
		{
			float num = 0.03f;
			if (!this.charging || this.broken || this.vanished || this.isTreasure)
			{
				num *= -1f;
			}
			this.turnLoopVol = Mathf.Clamp(this.turnLoopVol + num, 0f, 0.5f);
			if (this.sfxLoopUpdateCounter % 5 == 0)
			{
				this.PlayLoopSound(this.turnLoopVol > 0.01f, base.GetLoopClip(), this.turnLoopVol, null, this.turnLoopPitch);
				base.UpdateWithinWaterLPFilter(null);
				base.UpdateWithinWaterLPFilter(this.axle);
			}
			this.sfxLoopUpdateCounter++;
		}

		// Token: 0x060008E1 RID: 2273 RVA: 0x0003EA3C File Offset: 0x0003CE3C
		protected override void Appearing(float scale)
		{
			base.Appearing(scale);
			if (this.fakeRotor != null)
			{
				this.axle.GetComponent<Renderer>().enabled = true;
				this.axle.transform.localScale = Vector3.one * scale;
			}
		}

		// Token: 0x060008E2 RID: 2274 RVA: 0x0003EA8D File Offset: 0x0003CE8D
		protected override void Vanishing(float scale)
		{
			base.Vanishing(scale);
			if (this.fakeRotor != null)
			{
				this.axle.transform.localScale = Vector3.one * scale;
			}
		}

		// Token: 0x060008E3 RID: 2275 RVA: 0x0003EAC2 File Offset: 0x0003CEC2
		public override void Vanished()
		{
			base.Vanished();
			if (this.fakeRotor != null)
			{
				this.axle.GetComponent<Renderer>().enabled = false;
			}
		}

		// Token: 0x060008E4 RID: 2276 RVA: 0x0003EAEC File Offset: 0x0003CEEC
		public override void Appeared()
		{
			base.Appeared();
			if (this.fakeRotor != null)
			{
				this.axle.GetComponent<Renderer>().enabled = true;
				this.axle.transform.localScale = Vector3.one;
			}
		}

		// Token: 0x060008E5 RID: 2277 RVA: 0x0003EB2C File Offset: 0x0003CF2C
		private void DestroyFakeRotor()
		{
			if (this.fakeRotor != null)
			{
				Util.UnparentTransformSafely(this.axle.transform);
				this.axle.transform.position = this.goT.position;
				this.axle.transform.rotation = this.goT.rotation;
				this.axle.transform.parent = this.goT;
				UnityEngine.Object.Destroy(this.fakeRotor);
				this.fakeRotor = null;
			}
		}

		// Token: 0x060008E6 RID: 2278 RVA: 0x0003EBB8 File Offset: 0x0003CFB8
		public override void BecameTreasure()
		{
			base.BecameTreasure();
			this.DestroyFakeRotor();
		}

		// Token: 0x060008E7 RID: 2279 RVA: 0x0003EBC6 File Offset: 0x0003CFC6
		public override bool TreatAsVehicleLikeBlock()
		{
			return base.TreatAsVehicleLikeBlockWithStatus(ref this.treatAsVehicleStatus);
		}

		// Token: 0x060008E8 RID: 2280 RVA: 0x0003EBD4 File Offset: 0x0003CFD4
		public override void ChunksAndJointsModified(Dictionary<Joint, Joint> oldToNew, Dictionary<Chunk, Chunk> oldToNewChunks, Dictionary<Chunk, Chunk> newToOldChunks)
		{
			if (this.broken || this.isTreasure || this.joint == null)
			{
				return;
			}
			Joint joint;
			if (oldToNew.TryGetValue(this.joint, out joint))
			{
				this.joint = (ConfigurableJoint)joint;
			}
			this.rotor = this.joint.connectedBody.gameObject;
			this.rotorT = this.rotor.transform;
			List<Block> list = base.ConnectionsOfType(2, true);
			if (list.Count > 0)
			{
				this.rotorChunk = list[0].chunk;
			}
		}

		// Token: 0x060008E9 RID: 2281 RVA: 0x0003EC76 File Offset: 0x0003D076
		public override void Deactivate()
		{
			base.Deactivate();
			if (this.fakeRotor != null)
			{
				this.fakeRotor.SetActive(false);
			}
		}

		// Token: 0x040006CE RID: 1742
		private BlockAbstractTorsionSpring.TorsionSpringMode mode = BlockAbstractTorsionSpring.TorsionSpringMode.Spring;

		// Token: 0x040006CF RID: 1743
		private int treatAsVehicleStatus = -1;

		// Token: 0x040006D0 RID: 1744
		private const float angleEpsilon = 0.05f;

		// Token: 0x040006D1 RID: 1745
		private const float anglesPerSecond = 90f;

		// Token: 0x040006D2 RID: 1746
		private float targetAngle;

		// Token: 0x040006D3 RID: 1747
		private float currentAngle;

		// Token: 0x040006D4 RID: 1748
		private float lastRealAngle;

		// Token: 0x040006D5 RID: 1749
		private float springStiffness = 1f;

		// Token: 0x040006D6 RID: 1750
		private float springLimit;

		// Token: 0x040006D7 RID: 1751
		private float angleOffset;

		// Token: 0x040006D8 RID: 1752
		private int halfRevolutions;

		// Token: 0x040006D9 RID: 1753
		private float turnLoopPitch = 1f;

		// Token: 0x040006DA RID: 1754
		private float turnLoopVol;

		// Token: 0x040006DB RID: 1755
		private float stretchSoundTime;

		// Token: 0x040006DC RID: 1756
		private Chunk rotorChunk;

		// Token: 0x040006DD RID: 1757
		private float chunkMi = 1f;

		// Token: 0x040006DE RID: 1758
		private float rotorChunkMi = 1f;

		// Token: 0x040006DF RID: 1759
		private bool chunkKinematic;

		// Token: 0x040006E0 RID: 1760
		private bool rotorChunkKinematic;

		// Token: 0x040006E1 RID: 1761
		private bool released;

		// Token: 0x040006E2 RID: 1762
		private bool charging;

		// Token: 0x040006E3 RID: 1763
		private bool _limitedRange;

		// Token: 0x040006E4 RID: 1764
		private float _limitedRangeAngle = 0.05f;

		// Token: 0x040006E5 RID: 1765
		private float _limitedRangeRealAngle;

		// Token: 0x040006E6 RID: 1766
		private GameObject rotor;

		// Token: 0x040006E7 RID: 1767
		private Transform rotorT;

		// Token: 0x040006E8 RID: 1768
		private GameObject fakeRotor;

		// Token: 0x040006E9 RID: 1769
		private GameObject axle;

		// Token: 0x040006EA RID: 1770
		public ConfigurableJoint joint;

		// Token: 0x040006EB RID: 1771
		private Vector3 pausedVelocityAxle;

		// Token: 0x040006EC RID: 1772
		private Vector3 pausedAngularVelocityAxle;

		// Token: 0x040006ED RID: 1773
		private AudioSource axleAudioSource;

		// Token: 0x040006EE RID: 1774
		private int sfxLoopUpdateCounter;

		// Token: 0x040006EF RID: 1775
		private const int SFX_LOOP_UPDATE_INTERVAL = 5;

		// Token: 0x040006F0 RID: 1776
		private bool locked;

		// Token: 0x040006F1 RID: 1777
		private bool wasLocked;

		// Token: 0x040006F2 RID: 1778
		private float lockedAngleOffset;

		// Token: 0x040006F3 RID: 1779
		private float rotorAngleVecMultiplier = 1f;

		// Token: 0x040006F4 RID: 1780
		private int rotorAngleVecType;

		// Token: 0x040006F5 RID: 1781
		public Rigidbody jointToJointConnection;

		// Token: 0x040006F6 RID: 1782
		private Vector3 jointOffset = Vector3.zero;

		// Token: 0x0200006E RID: 110
		private enum TorsionSpringMode
		{
			// Token: 0x040006F8 RID: 1784
			FreeSpin,
			// Token: 0x040006F9 RID: 1785
			Spring
		}
	}
}
