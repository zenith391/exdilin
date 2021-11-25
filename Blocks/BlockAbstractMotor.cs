using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x02000062 RID: 98
	public class BlockAbstractMotor : Block
	{
		// Token: 0x06000800 RID: 2048 RVA: 0x00037DF4 File Offset: 0x000361F4
		public BlockAbstractMotor(List<List<Tile>> tiles, string axleName) : base(tiles)
		{
			this.axle = this.goT.Find(axleName).gameObject;
			this.loopName = "Motor Turn";
			this.sfxLoopUpdateCounter = UnityEngine.Random.Range(0, 5);
		}

		// Token: 0x06000801 RID: 2049 RVA: 0x00037E8B File Offset: 0x0003628B
		public override void Play2()
		{
			base.Play2();
			base.CreateFakeRigidbodyBetweenJoints();
		}

		// Token: 0x06000802 RID: 2050 RVA: 0x00037E9C File Offset: 0x0003629C
		public override void Play()
		{
			base.Play();
			this.hasNegativeAngleLimit = false;
			this.hasPositiveAngleLimit = false;
			this.treatAsVehicleStatus = -1;
			this.motorTargetVelocity = 0f;
			List<Block> list = base.ConnectionsOfType(2, true);
			if (list.Count > 0)
			{
				this.rotorChunk = list[0].chunk;
				this.rotor = this.rotorChunk.go;
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
				rigidbody.mass = 0.0001f;
				this.axle.transform.parent = this.fakeRotor.transform;
				this.rotor = this.fakeRotor;
			}
			this.CreateJoint();
			this.state = BlockAbstractMotor.MotorState.Hold;
			this.cumulativeTargetAngle = 0f;
			this.targetAngle = 0f;
			this.stepTargetAngle = 0f;
			this.currentAngle = 0f;
			this.lastAngle = 0f;
			this.turnSpeed = 0f;
			this.CalculateMassDistributions();
		}

		// Token: 0x06000803 RID: 2051 RVA: 0x00037FEC File Offset: 0x000363EC
		private void CreateJoint()
		{
			if (this.chunk.go != this.rotor)
			{
				this.joint = this.chunk.go.AddComponent<ConfigurableJoint>();
				this.joint.anchor = this.goT.localPosition;
				this.joint.axis = this.goT.up;
				this.joint.xMotion = ConfigurableJointMotion.Locked;
				this.joint.yMotion = ConfigurableJointMotion.Locked;
				this.joint.zMotion = ConfigurableJointMotion.Locked;
				this.joint.angularXMotion = ConfigurableJointMotion.Free;
				this.joint.angularYMotion = ConfigurableJointMotion.Locked;
				this.joint.angularZMotion = ConfigurableJointMotion.Locked;
				this.joint.connectedBody = this.rotor.GetComponent<Rigidbody>();
			}
		}

		// Token: 0x06000804 RID: 2052 RVA: 0x000380B4 File Offset: 0x000364B4
		private void DestroyJoint()
		{
			if (this.joint != null)
			{
				UnityEngine.Object.Destroy(this.joint);
				this.joint = null;
				base.DestroyFakeRigidbodies();
			}
		}

		// Token: 0x06000805 RID: 2053 RVA: 0x000380E0 File Offset: 0x000364E0
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
			this.axle.transform.localScale = Vector3.one;
			this.axle.transform.localRotation = Quaternion.identity;
			this.axle.GetComponent<Renderer>().enabled = true;
			this.PlayLoopSound(false, base.GetLoopClip(), 1f, null, 1f);
			base.Stop(resetBlock);
		}

		// Token: 0x06000806 RID: 2054 RVA: 0x0003818C File Offset: 0x0003658C
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

		// Token: 0x06000807 RID: 2055 RVA: 0x00038218 File Offset: 0x00036618
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

		// Token: 0x06000808 RID: 2056 RVA: 0x0003828C File Offset: 0x0003668C
		public override void Resume()
		{
			if (this.fakeRotor != null)
			{
				this.fakeRotor.GetComponent<Rigidbody>().isKinematic = false;
				this.fakeRotor.GetComponent<Rigidbody>().velocity = this.pausedVelocityAxle;
				this.fakeRotor.GetComponent<Rigidbody>().angularVelocity = this.pausedAngularVelocityAxle;
			}
		}

		// Token: 0x06000809 RID: 2057 RVA: 0x000382E8 File Offset: 0x000366E8
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

		// Token: 0x0600080A RID: 2058 RVA: 0x00038349 File Offset: 0x00036749
		public TileResultCode SetPositiveAngleLimit(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.positiveAngleLimit = Util.GetFloatArg(args, 0, float.MaxValue);
			this.hasPositiveAngleLimit = true;
			return TileResultCode.True;
		}

		// Token: 0x0600080B RID: 2059 RVA: 0x00038365 File Offset: 0x00036765
		public TileResultCode SetNegativeAngleLimit(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.negativeAngleLimit = Util.GetFloatArg(args, 0, float.MinValue);
			this.hasNegativeAngleLimit = true;
			return TileResultCode.True;
		}

		// Token: 0x0600080C RID: 2060 RVA: 0x00038384 File Offset: 0x00036784
		public TileResultCode Turn(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float num = (float)args[0] * eInfo.floatArg;
			this.cumulativeTurn += num;
			this.state = BlockAbstractMotor.MotorState.Turn;
			return TileResultCode.True;
		}

		// Token: 0x0600080D RID: 2061 RVA: 0x000383B8 File Offset: 0x000367B8
		private void ExecuteTurn()
		{
			if (Mathf.Abs(this.cumulativeTurn) > 0.0001f)
			{
				float num = Mathf.Max(Mathf.Abs(this.motorTargetVelocity), Mathf.Abs(this.cumulativeTurn) * this.stepSize / 0.25f);
				this.turnSpeed = Mathf.Clamp(this.turnSpeed + this.cumulativeTurn * 180f * Blocksworld.fixedDeltaTime, -num, num);
				float num2 = Mathf.Abs(this.motorTargetVelocity);
				this.turnDecceleration += num2;
			}
			this.cumulativeTurn = 0f;
		}

		// Token: 0x0600080E RID: 2062 RVA: 0x00038450 File Offset: 0x00036850
		public TileResultCode Return(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float num = eInfo.floatArg * Util.GetFloatArg(args, 0, 1f);
			float a = num * this.stepSize / 0.25f;
			float num2 = num;
			if (this.targetAngle > 0f)
			{
				num2 *= -1f;
			}
			float num3 = Mathf.Min(a, Mathf.Max(Mathf.Abs(this.targetAngle * 10f), 1f));
			this.turnSpeed = Mathf.Clamp(this.turnSpeed + num2 * 180f * Blocksworld.fixedDeltaTime, -num3, num3);
			this.turnDecceleration += Mathf.Abs(this.motorTargetVelocity);
			this.state = BlockAbstractMotor.MotorState.Turn;
			return TileResultCode.True;
		}

		// Token: 0x0600080F RID: 2063 RVA: 0x000384FE File Offset: 0x000368FE
		public TileResultCode FreeSpin(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.state = BlockAbstractMotor.MotorState.Spin;
			return TileResultCode.True;
		}

		// Token: 0x06000810 RID: 2064 RVA: 0x00038508 File Offset: 0x00036908
		public TileResultCode Step(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float num = (float)args[0] * eInfo.floatArg;
			if (Blocksworld.CurrentState == State.Play && eInfo.timer == Blocksworld.fixedDeltaTime && !this.broken && !this.vanished)
			{
				this.stepSoundCounter = 3;
			}
			float num2 = 0.25f;
			if (eInfo.timer == 0f)
			{
				float num3 = this.stepSize * num;
				this.stepTargetAngle += num3;
				this.cumulativeTargetAngle += num3;
			}
			this.state = BlockAbstractMotor.MotorState.Step;
			float num4 = Blocksworld.fixedDeltaTime / num2 * this.stepSize * num;
			this.targetAngle += num4;
			if (eInfo.timer >= num2)
			{
				this.state = BlockAbstractMotor.MotorState.Hold;
				this.targetAngle = this.stepTargetAngle;
				return TileResultCode.True;
			}
			return TileResultCode.Delayed;
		}

		// Token: 0x06000811 RID: 2065 RVA: 0x000385E0 File Offset: 0x000369E0
		public TileResultCode TargetAngle(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float num = (float)args[0] * eInfo.floatArg;
			float num2 = 180f * Blocksworld.fixedDeltaTime;
			if (num < this.targetAngle)
			{
				this.targetAngle = Mathf.Max(num, this.targetAngle - num2);
			}
			else if (num > this.targetAngle)
			{
				this.targetAngle = Mathf.Min(num, this.targetAngle + num2);
			}
			this.state = BlockAbstractMotor.MotorState.TargetAngle;
			return TileResultCode.True;
		}

		// Token: 0x06000812 RID: 2066 RVA: 0x00038658 File Offset: 0x00036A58
		private void UpdateMotorLoopSound()
		{
			if (!Sound.sfxEnabled || this.vanished || this.isTreasure)
			{
				this.PlayLoopSound(false, base.GetLoopClip(), 0f, null, 1f);
				return;
			}
			float num = 0f;
			if (this.state != BlockAbstractMotor.MotorState.Spin && this.state != BlockAbstractMotor.MotorState.Hold && this.joint != null && this.rotor != null)
			{
				Rigidbody rb = this.chunk.rb;
				if (rb != null)
				{
					Vector3 up = this.goT.up;
					float num2 = Vector3.Dot(this.rotor.GetComponent<Rigidbody>().angularVelocity, up);
					float num3 = Vector3.Dot(rb.angularVelocity, up);
					num = Mathf.Abs(num2 - num3);
					float num4 = num / 1.5f;
					float num5 = 0.05f;
					if (this.turnLoopPitch > num4)
					{
						num5 *= -2f;
					}
					this.turnLoopPitch = Mathf.Clamp(this.turnLoopPitch + num5, 0.5f, 3f);
				}
			}
			float num6 = 0.03f;
			if (this.stepSoundCounter > 0 && !this.broken && !this.broken)
			{
				num6 = 0.2f;
				this.stepSoundCounter--;
			}
			if (num < 0.2f || this.broken || this.vanished)
			{
				num6 *= -1f;
			}
			this.turnLoopVol = Mathf.Clamp(this.turnLoopVol + num6, 0f, 0.5f);
			if (this.sfxLoopUpdateCounter % 5 == 0)
			{
				float num7 = 1f;
				if (this.turnLoopVol > 0.01f)
				{
					float num8 = 0.18f;
					num7 = num8 * 2f * (Mathf.PerlinNoise(0.972f * Time.time, 5f) - 0.5f) + 1f;
				}
				this.PlayLoopSound(this.turnLoopVol > 0.01f, base.GetLoopClip(), this.turnLoopVol, null, num7 * this.turnLoopPitch);
				base.UpdateWithinWaterLPFilter(null);
			}
			this.sfxLoopUpdateCounter++;
		}

		// Token: 0x06000813 RID: 2067 RVA: 0x00038894 File Offset: 0x00036C94
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			this.ExecuteTurn();
			this.UpdateMotorLoopSound();
			if (this.isTreasure)
			{
				return;
			}
			if (this.joint == null || this.rotor == null)
			{
				return;
			}
			if (this.joint.connectedBody == null)
			{
				this.DestroyJoint();
				return;
			}
			this.lastAngle = this.currentAngle;
			this.currentAngle = Util.AngleBetween(this.rotor.transform.rotation * this.goT.localRotation * Vector3.forward, this.goT.forward, this.goT.up);
			if (this.fakeRotor == null)
			{
				this.axle.transform.localRotation = Quaternion.Euler(0f, -this.currentAngle, 0f);
			}
			if (this.turnSpeed != 0f)
			{
				float num = Blocksworld.fixedDeltaTime * this.turnSpeed;
				this.targetAngle += num;
				this.cumulativeTargetAngle += num;
			}
			float num2 = this.targetAngle;
			if (this.hasPositiveAngleLimit)
			{
				this.targetAngle = Mathf.Min(this.positiveAngleLimit, this.targetAngle);
			}
			if (this.hasNegativeAngleLimit)
			{
				this.targetAngle = Mathf.Max(this.negativeAngleLimit, this.targetAngle);
			}
			bool flag = Mathf.Abs(this.targetAngle - num2) > 0.01f;
			if (this.targetAngle > 180f)
			{
				this.targetAngle -= 360f;
			}
			else if (this.targetAngle < -180f)
			{
				this.targetAngle += 360f;
			}
			float num3 = this.targetAngle - this.currentAngle;
			if (num3 > 0f)
			{
				float num4 = num3 - 360f;
				if (Mathf.Abs(num4) < Mathf.Abs(num3))
				{
					num3 = num4;
				}
			}
			else
			{
				float num5 = num3 + 360f;
				if (Mathf.Abs(num5) < Mathf.Abs(num3))
				{
					num3 = num5;
				}
			}
			float num6 = Mathf.Abs(num3);
			if (this.state == BlockAbstractMotor.MotorState.Turn && num6 > 120f)
			{
				this.targetAngle = this.currentAngle;
				this.turnSpeed = 0f;
			}
			if (this.state == BlockAbstractMotor.MotorState.Spin && !flag)
			{
				this.targetAngle = this.currentAngle;
				this.cumulativeTargetAngle = this.currentAngle;
				this.SetMotorVelocity(0f, 0f);
			}
			else
			{
				if (this.state == BlockAbstractMotor.MotorState.Spin)
				{
					this.targetAngle = 0.98f * this.targetAngle;
				}
				float num7 = Mathf.Max(this.chunkMi, this.rotorChunkMi);
				Rigidbody rb = this.chunk.rb;
				if (rb != null && !this.chunkKinematic)
				{
					num7 = Mathf.Min(num7, this.chunkMi);
				}
				Rigidbody component = this.rotor.GetComponent<Rigidbody>();
				if (component != null && !this.rotorChunkKinematic)
				{
					num7 = Mathf.Min(num7, this.rotorChunkMi);
				}
				float v = Mathf.Clamp(num3, -25f, 25f);
				float force = Mathf.Max(num7 * (100f + Mathf.Abs(this.turnSpeed * 0.2f)), 20f) * 100f;
				this.SetMotorVelocity(v, force);
			}
			if (this.state != BlockAbstractMotor.MotorState.Turn)
			{
				this.turnDecceleration = Mathf.Max(90f, Mathf.Abs(3f * this.turnSpeed));
			}
			if (this.turnSpeed > 0f)
			{
				this.turnSpeed = Mathf.Max(0f, this.turnSpeed - this.turnDecceleration * Blocksworld.fixedDeltaTime);
			}
			else if (this.turnSpeed < 0f)
			{
				this.turnSpeed = Mathf.Min(0f, this.turnSpeed + this.turnDecceleration * Blocksworld.fixedDeltaTime);
			}
			if (this.state != BlockAbstractMotor.MotorState.Step)
			{
				this.stepTargetAngle = this.targetAngle;
			}
			this.turnDecceleration = 0f;
			this.state = BlockAbstractMotor.MotorState.Hold;
		}

		// Token: 0x06000814 RID: 2068 RVA: 0x00038CE4 File Offset: 0x000370E4
		private void SetMotorVelocity(float v, float force = 1000f)
		{
			this.motorTargetVelocity = v;
			JointDrive angularXDrive = default(JointDrive);
			angularXDrive.maximumForce = force;
			angularXDrive.positionDamper = force * 0.0035f;
			if (this.fakeRotor != null)
			{
				angularXDrive.positionDamper *= 0.0035f;
			}
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

		// Token: 0x06000815 RID: 2069 RVA: 0x00038D9B File Offset: 0x0003719B
		public TileResultCode IsStepping(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return ((float)args[0] >= 0f) ? this.IsSteppingRight() : this.IsSteppingLeft();
		}

		// Token: 0x06000816 RID: 2070 RVA: 0x00038DC0 File Offset: 0x000371C0
		public TileResultCode IsSteppingLeft()
		{
			return (this.state != BlockAbstractMotor.MotorState.Step) ? TileResultCode.False : this.IsTurning();
		}

		// Token: 0x06000817 RID: 2071 RVA: 0x00038DDA File Offset: 0x000371DA
		public TileResultCode IsSteppingRight()
		{
			return (this.state != BlockAbstractMotor.MotorState.Step) ? TileResultCode.False : this.IsReversing();
		}

		// Token: 0x06000818 RID: 2072 RVA: 0x00038DF4 File Offset: 0x000371F4
		public TileResultCode IsTurningSensor(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return ((float)args[0] >= 0f) ? this.IsReversing() : this.IsTurning();
		}

		// Token: 0x06000819 RID: 2073 RVA: 0x00038E1C File Offset: 0x0003721C
		public TileResultCode IsTurning()
		{
			if (Mathf.Abs(this.currentAngle - this.lastAngle) < 180f)
			{
				return (this.lastAngle <= this.currentAngle) ? TileResultCode.False : TileResultCode.True;
			}
			return (this.lastAngle >= 0f || this.currentAngle <= 0f) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x0600081A RID: 2074 RVA: 0x00038E88 File Offset: 0x00037288
		public TileResultCode IsReversing()
		{
			if (Mathf.Abs(this.currentAngle - this.lastAngle) < 180f)
			{
				return (this.lastAngle >= this.currentAngle) ? TileResultCode.False : TileResultCode.True;
			}
			return (this.lastAngle <= 0f || this.currentAngle >= 0f) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x0600081B RID: 2075 RVA: 0x00038EF1 File Offset: 0x000372F1
		public TileResultCode IsFreeSpinning(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (this.state != BlockAbstractMotor.MotorState.Spin) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x0600081C RID: 2076 RVA: 0x00038F08 File Offset: 0x00037308
		public void CalculateMassDistributions()
		{
			Chunk chunk = this.chunk;
			HashSet<Chunk> hashSet = new HashSet<Chunk>();
			this.chunkMi = base.CalculateMassDistribution(chunk, this.goT.up, this.rotorChunk);
			this.chunkKinematic = base.ConnectedToKinematicChunk(chunk, this.rotorChunk);
			this.rotorChunkMi = this.chunkMi;
			if (this.rotorChunk != null)
			{
				this.rotorChunkMi = base.CalculateMassDistribution(this.rotorChunk, this.goT.up, chunk);
				this.rotorChunkKinematic = base.ConnectedToKinematicChunk(this.rotorChunk, chunk);
			}
		}

		// Token: 0x0600081D RID: 2077 RVA: 0x00038F9C File Offset: 0x0003739C
		public override void ChunkInModelFrozen()
		{
			base.ChunkInModelFrozen();
			this.chunkKinematic = base.ConnectedToKinematicChunk(this.chunk, this.rotorChunk);
			if (this.rotorChunk != null)
			{
				this.rotorChunkKinematic = base.ConnectedToKinematicChunk(this.rotorChunk, this.chunk);
			}
		}

		// Token: 0x0600081E RID: 2078 RVA: 0x00038FEC File Offset: 0x000373EC
		public override void ChunkInModelUnfrozen()
		{
			base.ChunkInModelUnfrozen();
			this.chunkKinematic = base.ConnectedToKinematicChunk(this.chunk, this.rotorChunk);
			if (this.rotorChunk != null)
			{
				this.rotorChunkKinematic = base.ConnectedToKinematicChunk(this.rotorChunk, this.chunk);
			}
		}

		// Token: 0x0600081F RID: 2079 RVA: 0x0003903C File Offset: 0x0003743C
		protected override void Appearing(float scale)
		{
			base.Appearing(scale);
			if (this.fakeRotor != null)
			{
				this.axle.GetComponent<Renderer>().enabled = true;
				this.axle.transform.localScale = Vector3.one * scale;
			}
		}

		// Token: 0x06000820 RID: 2080 RVA: 0x0003908D File Offset: 0x0003748D
		protected override void Vanishing(float scale)
		{
			base.Vanishing(scale);
			if (this.fakeRotor != null)
			{
				this.axle.transform.localScale = Vector3.one * scale;
			}
		}

		// Token: 0x06000821 RID: 2081 RVA: 0x000390C2 File Offset: 0x000374C2
		public override void Vanished()
		{
			base.Vanished();
			if (this.fakeRotor != null)
			{
				this.axle.GetComponent<Renderer>().enabled = false;
			}
		}

		// Token: 0x06000822 RID: 2082 RVA: 0x000390EC File Offset: 0x000374EC
		public override void Appeared()
		{
			base.Appeared();
			if (this.fakeRotor != null)
			{
				this.axle.GetComponent<Renderer>().enabled = true;
				this.axle.transform.localScale = Vector3.one;
			}
		}

		// Token: 0x06000823 RID: 2083 RVA: 0x0003912B File Offset: 0x0003752B
		public override void BecameTreasure()
		{
			base.BecameTreasure();
			this.DestroyFakeRotor();
		}

		// Token: 0x06000824 RID: 2084 RVA: 0x00039139 File Offset: 0x00037539
		public override bool TreatAsVehicleLikeBlock()
		{
			return base.TreatAsVehicleLikeBlockWithStatus(ref this.treatAsVehicleStatus);
		}

		// Token: 0x06000825 RID: 2085 RVA: 0x00039148 File Offset: 0x00037548
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
			List<Block> list = base.ConnectionsOfType(2, true);
			if (list.Count > 0)
			{
				this.rotorChunk = list[0].chunk;
			}
		}

		// Token: 0x06000826 RID: 2086 RVA: 0x000391D9 File Offset: 0x000375D9
		public override void Deactivate()
		{
			base.Deactivate();
			if (this.fakeRotor != null)
			{
				this.fakeRotor.SetActive(false);
			}
		}

		// Token: 0x06000827 RID: 2087 RVA: 0x000391FE File Offset: 0x000375FE
		public override bool IsRuntimeInvisible()
		{
			return false;
		}

		// Token: 0x0400061A RID: 1562
		private const float anglesPerSecond = 90f;

		// Token: 0x0400061B RID: 1563
		private float stepSize = 22.5f;

		// Token: 0x0400061C RID: 1564
		private BlockAbstractMotor.MotorState state;

		// Token: 0x0400061D RID: 1565
		private int treatAsVehicleStatus = -1;

		// Token: 0x0400061E RID: 1566
		private float motorTargetVelocity;

		// Token: 0x0400061F RID: 1567
		private bool hasPositiveAngleLimit;

		// Token: 0x04000620 RID: 1568
		private float positiveAngleLimit = 45f;

		// Token: 0x04000621 RID: 1569
		private bool hasNegativeAngleLimit;

		// Token: 0x04000622 RID: 1570
		private float negativeAngleLimit = -45f;

		// Token: 0x04000623 RID: 1571
		private float cumulativeTargetAngle;

		// Token: 0x04000624 RID: 1572
		private float stepTargetAngle;

		// Token: 0x04000625 RID: 1573
		private float targetAngle;

		// Token: 0x04000626 RID: 1574
		private float currentAngle;

		// Token: 0x04000627 RID: 1575
		private float lastAngle;

		// Token: 0x04000628 RID: 1576
		private const float turnAcceleration = 180f;

		// Token: 0x04000629 RID: 1577
		private float turnDecceleration = 90f;

		// Token: 0x0400062A RID: 1578
		private float turnSpeed;

		// Token: 0x0400062B RID: 1579
		private float cumulativeTurn;

		// Token: 0x0400062C RID: 1580
		private float turnLoopPitch = 1f;

		// Token: 0x0400062D RID: 1581
		private float turnLoopVol;

		// Token: 0x0400062E RID: 1582
		private int stepSoundCounter;

		// Token: 0x0400062F RID: 1583
		private int sfxLoopUpdateCounter;

		// Token: 0x04000630 RID: 1584
		private const int SFX_LOOP_UPDATE_INTERVAL = 5;

		// Token: 0x04000631 RID: 1585
		private GameObject rotor;

		// Token: 0x04000632 RID: 1586
		private GameObject fakeRotor;

		// Token: 0x04000633 RID: 1587
		private GameObject axle;

		// Token: 0x04000634 RID: 1588
		private ConfigurableJoint joint;

		// Token: 0x04000635 RID: 1589
		private JointMotor m;

		// Token: 0x04000636 RID: 1590
		private Vector3 pausedVelocityAxle;

		// Token: 0x04000637 RID: 1591
		private Vector3 pausedAngularVelocityAxle;

		// Token: 0x04000638 RID: 1592
		private float chunkMi = 1f;

		// Token: 0x04000639 RID: 1593
		private float rotorChunkMi = 1f;

		// Token: 0x0400063A RID: 1594
		private bool chunkKinematic;

		// Token: 0x0400063B RID: 1595
		private bool rotorChunkKinematic;

		// Token: 0x0400063C RID: 1596
		private Chunk rotorChunk;

		// Token: 0x02000063 RID: 99
		private enum MotorState
		{
			// Token: 0x0400063E RID: 1598
			Hold,
			// Token: 0x0400063F RID: 1599
			Step,
			// Token: 0x04000640 RID: 1600
			Turn,
			// Token: 0x04000641 RID: 1601
			Spin,
			// Token: 0x04000642 RID: 1602
			TargetAngle
		}
	}
}
