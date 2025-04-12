using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x020000D6 RID: 214
	public class BlockSteeringWheel : Block
	{
		// Token: 0x06000FCB RID: 4043 RVA: 0x00069C00 File Offset: 0x00068000
		public BlockSteeringWheel(List<List<Tile>> tiles) : base(tiles)
		{
			Vector3 vector = new Vector3(0f, 0f, -1f);
			this.localSteeringAxle = vector.normalized;
			this.meshesToTurn = new List<Transform>();
			this.wheelsBounds = default(Bounds);
			this.meshesToTurn.Add(this.goT.Find("DrivingWheel SW"));
			this.meshesToTurn.Add(this.goT.Find("CarHorn SW"));
			this.meshesToTurn.Add(this.goT.Find("SteeringWheelConnect SW"));
			this.engineSoundDefinition = Blocksworld.engineSoundDefinitions.GetEngineSoundDefinition("Default");
			this.loopName = this.engineSoundDefinition.loopSFXName;
		}

		// Token: 0x06000FCC RID: 4044 RVA: 0x00069E78 File Offset: 0x00068278
		public override void OnCreate()
		{
			this.metaData = this.go.GetComponent<SteeringWheelMetaData>();
		}

		// Token: 0x06000FCD RID: 4045 RVA: 0x00069E8C File Offset: 0x0006828C
		public static void AddVehicleDefintionToggleChain()
		{
			BlockSteeringWheel.LoadVehicleDefinitions();
			List<GAF> list = new List<GAF>();
			foreach (VehicleDefinition vehicleDefinition in BlockSteeringWheel.vehicleDefinitions.definitions)
			{
				list.Add(new GAF(BlockSteeringWheel.predicateSetVehicleType, new object[]
				{
					vehicleDefinition.name
				}));
			}
			TileToggleChain.AddChain(list);
		}

		// Token: 0x06000FCE RID: 4046 RVA: 0x00069EEC File Offset: 0x000682EC
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (this.broken || this.vanished || this.isTreasure)
			{
				return;
			}
			this.UpdateEngineSound();
			this.speedometerOn = this.turnOnSpeedometer;
			this.turnOnSpeedometer = false;
			if (this.hadSetControlType)
			{
				if (this.moveAlongMaxForce > 0f)
				{
					if (this.moveAlongDirection.sqrMagnitude > 1f)
					{
						this.moveAlongDirection.Normalize();
					}
					this.MoveAlongDirection(this.moveAlongDirection, this.moveAlongMaxAngle, this.moveAlongMaxForce);
					this.moveAlongDirection.Set(0f, 0f, 0f);
					this.moveAlongMaxAngle = 45f;
					this.moveAlongMaxForce = 0f;
				}
				int num = 0;
				bool flag = this.jumpCounter < 5 && this.jumpHeight > 0.01f;
				float jumpYOffset = 0f;
				if (flag)
				{
					float jumpForcePerFrame = Util.GetJumpForcePerFrame(this.jumpHeight, this.chunk.rb.mass, 5);
					float num2 = jumpForcePerFrame / this.chunk.rb.mass;
					jumpYOffset = num2 * Blocksworld.fixedDeltaTime * 0.5f;
				}
				this.jumpCounter++;
				this.jumpHeight = 0f;
				for (int i = 0; i < this.allControlledWheels.Count; i++)
				{
					BlockAbstractWheel blockAbstractWheel = this.allControlledWheels[i];
					if (blockAbstractWheel.externalControlBlock == this)
					{
						blockAbstractWheel.FixedUpdateDriveAndTurn(jumpYOffset);
					}
					if (blockAbstractWheel.onGround > 0.17f)
					{
						num++;
					}
				}
				for (int j = 0; j < this.allControlledTankWheels.Count; j++)
				{
					BlockTankTreadsWheel blockTankTreadsWheel = this.allControlledTankWheels[j];
					if (blockTankTreadsWheel.externalControlBlock == this)
					{
						blockTankTreadsWheel.FixedUpdateDriveAndTurn();
					}
					if (blockTankTreadsWheel.onGround > 0.17f)
					{
						num++;
					}
				}
				int num3 = this.allControlledWheels.Count + this.allControlledTankWheels.Count;
				Rigidbody rigidbody = null;
				if (num3 >= 3)
				{
					this.onGroundFraction = Mathf.Lerp(this.onGroundFraction, (float)num / (float)num3, 0.2f);
					if (this.chunk.go != null)
					{
						rigidbody = this.chunk.rb;
						if (rigidbody != null && !rigidbody.isKinematic)
						{
							float num4 = 1f / Mathf.Max(1f, 4f * rigidbody.angularVelocity.magnitude);
						}
					}
					Vector3 up = this.goT.up;
					if (this.onGroundFraction > 0.75f)
					{
						this.alignUp = (0.95f * this.alignUp + 0.05f * up).normalized;
					}
					else if (this.onGroundFraction > 0.25f || this.flipping > 0f)
					{
						if (rigidbody != null)
						{
							float num5 = Mathf.Clamp(Vector3.Angle(this.alignUp, up), -45f, 45f);
							if (rigidbody != null && !rigidbody.isKinematic)
							{
								float num4 = 1f / Mathf.Max(1f, 4f * rigidbody.angularVelocity.magnitude);
								Vector3 a = 5f * -num5 * num4 * rigidbody.mass * Vector3.Cross(this.alignUp, up).normalized;
								if (this.flipping > 0f)
								{
									a *= this.flipping;
								}
								this.alignUp = (0.99f * this.alignUp + 0.01f * Vector3.up).normalized;
							}
						}
						this.flipping = 0f;
					}
					else if (this.onGroundFraction < 0.05f && num == 0)
					{
						this.alignUp = (0.98f * this.alignUp + 0.02f * Vector3.up).normalized;
					}
				}
				float num6 = (!(this.vehicleType == "Utility")) ? 1f : -1f;
				this.visualAngleTarget = this.turnAngle * num6;
				this.visualAngle = 0.1f * this.visualAngleTarget + 0.9f * this.visualAngle;
				this.turnAngle = 0f;
				this.helpForceTurnAngle = 0f;
				if (this.beingDriven)
				{
					this.beingDriven = false;
				}
				else
				{
					this.canChangeDriveState = true;
					if (rigidbody != null && Mathf.Abs(rigidbody.velocity.sqrMagnitude) < 3f)
					{
						this.driveState = BlockSteeringWheel.DriveStates.IDLE;
					}
				}
			}
			if (this.hadSetControlType != this.hasSetControlType)
			{
				BlockSteeringWheel blockSteeringWheel = (!this.hasSetControlType) ? null : this;
				foreach (BlockAbstractWheel blockAbstractWheel2 in this.allControlledWheels)
				{
					blockAbstractWheel2.AssignExternalControl(blockSteeringWheel, this.allControlledWheels);
					blockAbstractWheel2.maxSpeedInc = ((!this.hasSetControlType) ? 99999f : this.maxSpeedInc);
				}
				foreach (BlockTankTreadsWheel blockTankTreadsWheel2 in this.allControlledTankWheels)
				{
					blockTankTreadsWheel2.externalControlBlock = blockSteeringWheel;
				}
				this.ResetHandling();
				if (this.hasSetControlType)
				{
					this.ImproveHandling();
				}
				this.hadSetControlType = this.hasSetControlType;
			}
			this.hasSetControlType = false;
			float b = 0f;
			if (!this.setEngineSoundRPM)
			{
				this.engineSoundRPM = Mathf.Lerp(this.engineSoundRPM, 0f, this.engineSoundDefinition.RPMDecaySpeed * Time.fixedDeltaTime);
				foreach (BlockAbstractWheel blockAbstractWheel3 in this.allControlledWheels)
				{
					float magnitude = blockAbstractWheel3.chunk.rb.angularVelocity.magnitude;
					b = Mathf.Max(magnitude, b);
				}
			}
			this.setEngineSoundRPM = false;
			this.engineSoundWheelSpinSpeed = Mathf.Lerp(this.engineSoundWheelSpinSpeed, b, Time.fixedDeltaTime);
		}

		// Token: 0x06000FCF RID: 4047 RVA: 0x0006A5F0 File Offset: 0x000689F0
		private void UpdateEngineSound()
		{
			if (!this.engineSoundOn || Blocksworld.CurrentState != State.Play)
			{
				this.PlayLoopSound(false, base.GetLoopClip(), 1f, null, 1f);
				return;
			}
			float num = this.engineSoundDefinition.baseVolume + this.engineSoundRPM * this.engineSoundDefinition.RPMVolumeMod;
			float num2 = this.engineSoundDefinition.basePitch + this.engineSoundRPM * this.engineSoundDefinition.RPMPitchMod;
			float num3 = Mathf.Clamp(this.engineSoundWheelSpinSpeed - this.engineSoundDefinition.wheelSpinThreshold, 0f, this.engineSoundDefinition.wheelSpinMax);
			float b = this.engineSoundDefinition.baseVolume + num3 * this.engineSoundDefinition.wheelSpinVolumeMod;
			float b2 = this.engineSoundDefinition.basePitch + num3 * this.engineSoundDefinition.wheelSpinPitchMod;
			num = Mathf.Max(num, b);
			num2 = Mathf.Max(num2, b2);
			this.PlayLoopSound(true, base.GetLoopClip(), num, null, num2);
		}

		// Token: 0x06000FD0 RID: 4048 RVA: 0x0006A6E8 File Offset: 0x00068AE8
		public override void Update()
		{
			base.Update();
			if (Blocksworld.CurrentState == State.Play)
			{
				if (this.broken || this.vanished || this.isTreasure)
				{
					if (this.speedometer != null)
					{
						this.speedometer.Hide();
					}
					return;
				}
				for (int i = 0; i < this.meshesToTurn.Count; i++)
				{
					Transform transform = this.meshesToTurn[i];
					transform.localRotation = Quaternion.AngleAxis(-this.visualAngle, this.localSteeringAxle);
				}
				if (this.speedometerOn && this.chunk.rb != null)
				{
					if (this.speedometer == null)
					{
						this.speedometer = Blocksworld.UI.Overlay.CreateSpeedometer();
					}
					this.speedometer.SetSpeed(Mathf.Max(this.chunk.rb.velocity.magnitude - 0.5f, 0f));
					this.speedometer.Show();
				}
				else if (this.speedometer != null)
				{
					this.speedometer.Hide();
				}
			}
		}

		// Token: 0x06000FD1 RID: 4049 RVA: 0x0006A82C File Offset: 0x00068C2C
		public new static void Register()
		{
			PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.SetTurnMode", null, (Block b) => new PredicateActionDelegate(((BlockSteeringWheel)b).SetTurnMode), new Type[]
			{
				typeof(int)
			}, new string[]
			{
				"Turn Mode"
			}, null);
			PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.SetDriveMode", null, (Block b) => new PredicateActionDelegate(((BlockSteeringWheel)b).SetDriveMode), new Type[]
			{
				typeof(int)
			}, new string[]
			{
				"Drive Mode"
			}, null);
			BlockSteeringWheel.predicateSteeringWheelMoverSteer = PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.MoverSteer", null, (Block b) => new PredicateActionDelegate(((BlockSteeringWheel)b).MoverSteer), new Type[]
			{
				typeof(string),
				typeof(float)
			}, new string[]
			{
				"Stick Name",
				"Max Angle"
			}, null);
			BlockSteeringWheel.predicateSteeringWheelTiltSteer = PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.TiltSteer", null, (Block b) => new PredicateActionDelegate(((BlockSteeringWheel)b).TiltSteer), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Max Angle"
			}, null);
			BlockSteeringWheel.predicateSteeringWheelTiltDrive = PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.TiltDrive", null, (Block b) => new PredicateActionDelegate(((BlockSteeringWheel)b).TiltDrive), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Speed"
			}, null);
			PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.Flip", null, (Block b) => new PredicateActionDelegate(((BlockSteeringWheel)b).Flip), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Strength"
			}, null);
			BlockSteeringWheel.predicateSteeringWheelMoveLocalAlongMover = PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.MoveLocalAlongMover", null, (Block b) => new PredicateActionDelegate(((BlockSteeringWheel)b).MoveLocalAlongMover), new Type[]
			{
				typeof(string),
				typeof(float)
			}, new string[]
			{
				"Stick Name",
				"Max Force"
			}, null);
			BlockSteeringWheel.predicateSteeringWheelMoveAlongMover = PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.MoveAlongMover", null, (Block b) => new PredicateActionDelegate(((BlockSteeringWheel)b).MoveAlongMover), new Type[]
			{
				typeof(string),
				typeof(float),
				typeof(float)
			}, new string[]
			{
				"Stick Name",
				"Max Angle",
				"Max Force"
			}, null);
			BlockSteeringWheel.predicateSteeringWheelMoveLocalAlongTilt = PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.MoveLocalAlongTilt", null, (Block b) => new PredicateActionDelegate(((BlockSteeringWheel)b).MoveLocalAlongTilt), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Max Force"
			}, null);
			PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.DriveThroughTag", null, (Block b) => new PredicateActionDelegate(((BlockSteeringWheel)b).DriveThroughTag), new Type[]
			{
				typeof(string),
				typeof(float),
				typeof(float)
			}, new string[]
			{
				"Tag Name",
				"Max Angle",
				"Max Force"
			}, null);
			PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.AvoidTag", null, (Block b) => new PredicateActionDelegate(((BlockSteeringWheel)b).AvoidTag), new Type[]
			{
				typeof(string),
				typeof(float),
				typeof(float),
				typeof(float),
				typeof(int)
			}, new string[]
			{
				"Tag Name",
				"Distance",
				"Max Angle",
				"Max Force"
			}, null);
			PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.JumpHeight", null, (Block b) => new PredicateActionDelegate(((BlockSteeringWheel)b).Jump), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Height"
			}, null);
			PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.TurnAlongMover", null, (Block b) => new PredicateActionDelegate(((BlockSteeringWheel)b).TurnAlongMover), new Type[]
			{
				typeof(string),
				typeof(float),
				typeof(int)
			}, new string[]
			{
				"Stick Name",
				"Max Angle"
			}, null);
			PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.DriveAlongMover", null, (Block b) => new PredicateActionDelegate(((BlockSteeringWheel)b).DriveAlongMover), new Type[]
			{
				typeof(string),
				typeof(float),
				typeof(int)
			}, new string[]
			{
				"Stick Name",
				"Max Force"
			}, null);
			BlockSteeringWheel.predicateTurn = PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.Turn", null, (Block b) => new PredicateActionDelegate(((BlockSteeringWheel)b).Turn), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Angle"
			}, null);
			BlockSteeringWheel.predicateDrive = PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.Drive", null, (Block b) => new PredicateActionDelegate(((BlockSteeringWheel)b).Drive), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Force"
			}, null);
			PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.BrakeEffectiveness", (Block b) => new PredicateSensorDelegate(((BlockSteeringWheel)b).IsBrakeEffectiveness), (Block b) => new PredicateActionDelegate(((BlockSteeringWheel)b).SetBrakeEffectiveness), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Effectiveness"
			}, null);
			PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.Brake", (Block b) => new PredicateSensorDelegate(((BlockSteeringWheel)b).IsBraking), null, null, null, null);
			PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.Driving", (Block b) => new PredicateSensorDelegate(((BlockSteeringWheel)b).IsDriving), null, null, null, null);
			PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.Idling", (Block b) => new PredicateSensorDelegate(((BlockSteeringWheel)b).IsIdling), null, null, null, null);
			PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.Reversing", (Block b) => new PredicateSensorDelegate(((BlockSteeringWheel)b).IsReversing), null, null, null, null);
			PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.Turning", (Block b) => new PredicateSensorDelegate(((BlockSteeringWheel)b).IsTurning), null, new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.Turning", (Block b) => new PredicateSensorDelegate(((BlockSteeringWheel)b).IsTurning), null, new Type[]
			{
				typeof(float)
			}, null, null);
			BlockSteeringWheel.predicateSetupTurn = PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.SetupTurn", null, (Block b) => new PredicateActionDelegate(((BlockSteeringWheel)b).SetupTurn), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Angle"
			}, null);
			BlockSteeringWheel.predicateSetupDrive = PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.SetupDriver", null, (Block b) => new PredicateActionDelegate(((BlockSteeringWheel)b).SetupDriver), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Force"
			}, null);
			BlockSteeringWheel.predicateDriveControl = PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.DriveControlType", null, (Block b) => new PredicateActionDelegate(((BlockSteeringWheel)b).SetDriveControl), new Type[]
			{
				typeof(string)
			}, new string[]
			{
				"Mover",
				"Buttons",
				"Combined",
				"Tilt",
				"TiltCombined"
			}, null);
			BlockSteeringWheel.predicateSetVehicleType = PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.SetVehicleType", null, (Block b) => new PredicateActionDelegate(((BlockSteeringWheel)b).SetVehicleType), new Type[]
			{
				typeof(string)
			}, new string[]
			{
				"Vehicle type"
			}, null);
			PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.SetBallastFraction", null, (Block b) => new PredicateActionDelegate(((BlockSteeringWheel)b).SetBallastFraction), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Fraction"
			}, null);
			PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.ShowSpeedometer", null, (Block b) => new PredicateActionDelegate(((BlockSteeringWheel)b).ShowSpeedometer), null, null, null);
			PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.SuspensionDamper", null, (Block b) => new PredicateActionDelegate(((BlockSteeringWheel)b).SetSuspensionDamper), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.SuspensionHeight", null, (Block b) => new PredicateActionDelegate(((BlockSteeringWheel)b).SetSuspensionHeight), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.SuspensionLength", null, (Block b) => new PredicateActionDelegate(((BlockSteeringWheel)b).SetSuspensionLength), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.SuspensionSpring", null, (Block b) => new PredicateActionDelegate(((BlockSteeringWheel)b).SetSuspensionSpring), new Type[]
			{
				typeof(float)
			}, null, null);
			Block.AddSimpleDefaultTiles(new List<GAF>
			{
				new GAF(BlockSteeringWheel.predicateDriveControl, new object[]
				{
					"Combined"
				})
			}, new string[]
			{
				"Steering Wheel"
			});
		}

		// Token: 0x06000FD2 RID: 4050 RVA: 0x0006B2B0 File Offset: 0x000696B0
		private void TurnAlongDirection(Vector3 direction, float maxAngle)
		{
			Transform goT = this.goT;
			float value = Util.AngleBetween(Util.ProjectOntoPlane(goT.forward, Vector3.up), direction, goT.up);
			float num = Mathf.Clamp(value, -maxAngle, maxAngle);
			this.TurnStandard(-num);
		}

		// Token: 0x06000FD3 RID: 4051 RVA: 0x0006B2F4 File Offset: 0x000696F4
		private void DriveAlongDirection(Vector3 direction, float maxForce)
		{
			Transform goT = this.goT;
			float num = Vector3.Dot(goT.forward, direction);
			if (num >= 0f)
			{
				num = Mathf.Max(0.25f, num);
			}
			else
			{
				num = Mathf.Min(-0.25f, num);
			}
			float force = maxForce * num;
			this.Drive(force);
		}

		// Token: 0x06000FD4 RID: 4052 RVA: 0x0006B348 File Offset: 0x00069748
		private void MoveAlongDirection(Vector3 direction, float maxAngle, float maxForce)
		{
			Transform goT = this.goT;
			Vector3 forward = goT.forward;
			float num = 0.75f;
			float num2 = Vector3.Dot(forward, direction.normalized);
			if (num2 > -num)
			{
				num2 = (num2 + num) / (1f + num);
			}
			else if (num2 < -num)
			{
				num2 = (num2 + num) / (1f - num);
			}
			float magnitude = direction.magnitude;
			float num3 = magnitude * num2;
			float num4 = magnitude * Mathf.Sign(num2);
			Vector3 vector = Vector3.zero;
			Rigidbody rb = this.chunk.rb;
			if (rb != null)
			{
				vector = rb.velocity;
				float num5 = Vector3.Dot(rb.angularVelocity, goT.up);
			}
			float num6 = maxForce * num3;
			this.Drive(-num6);
		}

		// Token: 0x06000FD5 RID: 4053 RVA: 0x0006B420 File Offset: 0x00069820
		private void UpdateDriveState(float force)
		{
			if (this.canChangeDriveState || this.controlType == "Mover")
			{
				switch (this.driveState)
				{
				case BlockSteeringWheel.DriveStates.IDLE:
				case BlockSteeringWheel.DriveStates.FORWARD_BRAKING:
				case BlockSteeringWheel.DriveStates.REVERSE_BRAKING:
					this.driveState = ((force >= 0f) ? ((force <= 0f) ? BlockSteeringWheel.DriveStates.IDLE : BlockSteeringWheel.DriveStates.REVERSE) : BlockSteeringWheel.DriveStates.FORWARD);
					break;
				case BlockSteeringWheel.DriveStates.FORWARD:
					if (force > 0f)
					{
						this.driveState = BlockSteeringWheel.DriveStates.FORWARD_BRAKING;
					}
					break;
				case BlockSteeringWheel.DriveStates.REVERSE:
					if (force < 0f)
					{
						this.driveState = BlockSteeringWheel.DriveStates.REVERSE_BRAKING;
					}
					break;
				}
			}
			else if (force < 0f)
			{
				if (this.driveState == BlockSteeringWheel.DriveStates.FORWARD_BRAKING)
				{
					this.driveState = BlockSteeringWheel.DriveStates.FORWARD;
				}
				else if (this.driveState == BlockSteeringWheel.DriveStates.REVERSE)
				{
					this.driveState = BlockSteeringWheel.DriveStates.REVERSE_BRAKING;
				}
			}
			else if (force > 0f)
			{
				if (this.driveState == BlockSteeringWheel.DriveStates.REVERSE_BRAKING)
				{
					this.driveState = BlockSteeringWheel.DriveStates.REVERSE;
				}
				else if (this.driveState == BlockSteeringWheel.DriveStates.FORWARD)
				{
					this.driveState = BlockSteeringWheel.DriveStates.FORWARD_BRAKING;
				}
			}
			this.canChangeDriveState = false;
		}

		// Token: 0x06000FD6 RID: 4054 RVA: 0x0006B54C File Offset: 0x0006994C
		private void Drive(float force)
		{
			this.beingDriven = true;
			this.UpdateDriveState(force);
			if (this.driveState == BlockSteeringWheel.DriveStates.FORWARD_BRAKING || this.driveState == BlockSteeringWheel.DriveStates.REVERSE_BRAKING)
			{
				float f = this.brakeEffectiveness * 0.5f;
				this.BrakeAll(this.driveWheels, f);
				this.BrakeAll(this.inverseDriveWheels, f);
			}
			else
			{
				this.engineSoundRPM = Mathf.Lerp(this.engineSoundRPM, this.engineSoundDefinition.maximumRPM, this.engineSoundDefinition.RPMIncreaseSpeed * Time.fixedDeltaTime);
				this.setEngineSoundRPM = true;
				float num = 2f / Mathf.Max(1f, (float)(this.frontDriveWheels.Count + this.frontInverseDriveWheels.Count));
				float num2 = 2f / Mathf.Max(1f, (float)(this.backDriveWheels.Count + this.backInverseDriveWheels.Count));
				VehicleDriveMode vehicleDriveMode = this.driveMode;
				if (vehicleDriveMode != VehicleDriveMode.ALL)
				{
					if (vehicleDriveMode != VehicleDriveMode.FRONT)
					{
						if (vehicleDriveMode == VehicleDriveMode.BACK)
						{
							this.DriveAll(this.backDriveWheels, force * num2, false);
							this.DriveAll(this.backInverseDriveWheels, -force * num2, false);
						}
					}
					else
					{
						this.DriveAll(this.frontDriveWheels, force * num, false);
						this.DriveAll(this.frontInverseDriveWheels, -force * num, false);
					}
				}
				else
				{
					this.DriveAll(this.frontDriveWheels, force * BlockSteeringWheel.awdBalanceFront * num, false);
					this.DriveAll(this.frontInverseDriveWheels, -force * BlockSteeringWheel.awdBalanceFront * num, false);
					this.DriveAll(this.backDriveWheels, force * BlockSteeringWheel.awdBalanceRear * num2, false);
					this.DriveAll(this.backInverseDriveWheels, -force * BlockSteeringWheel.awdBalanceRear * num2, false);
				}
			}
			this.DriveAll(this.drive_TankWheels, force);
			this.DriveAll(this.inverseDrive_TankWheels, -force * 0.25f);
		}

		// Token: 0x06000FD7 RID: 4055 RVA: 0x0006B720 File Offset: 0x00069B20
		private void SetDefinitionForAll(WheelDefinition def, HashSet<BlockAbstractWheel> done, params List<BlockAbstractWheel>[] wheelLists)
		{
			foreach (List<BlockAbstractWheel> list in wheelLists)
			{
				for (int j = 0; j < list.Count; j++)
				{
					BlockAbstractWheel blockAbstractWheel = list[j];
					if (!done.Contains(blockAbstractWheel))
					{
						blockAbstractWheel.SetWheelDefinitionData(def);
						done.Add(blockAbstractWheel);
					}
				}
			}
		}

		// Token: 0x06000FD8 RID: 4056 RVA: 0x0006B780 File Offset: 0x00069B80
		public static void LoadVehicleDefinitions()
		{
			if (BlockSteeringWheel.vehicleDefinitions != null)
			{
				return;
			}
			BlockSteeringWheel.vehicleDefinitions = Resources.Load<VehicleDefinitions>("VehicleDefinitions");
			BlockSteeringWheel.vehicleDefsDict = new Dictionary<string, VehicleDefinition>();
			foreach (VehicleDefinition vehicleDefinition in BlockSteeringWheel.vehicleDefinitions.definitions)
			{
				BlockSteeringWheel.vehicleDefsDict[vehicleDefinition.name] = vehicleDefinition;
			}
		}

		// Token: 0x06000FD9 RID: 4057 RVA: 0x0006B7EC File Offset: 0x00069BEC
		public static VehicleDefinition FindVehicleDefinition(string name)
		{
			BlockSteeringWheel.LoadVehicleDefinitions();
			VehicleDefinition result;
			if (BlockSteeringWheel.vehicleDefsDict.TryGetValue(name, out result))
			{
				return result;
			}
			return BlockSteeringWheel.vehicleDefinitions.definitions[0];
		}

		// Token: 0x06000FDA RID: 4058 RVA: 0x0006B820 File Offset: 0x00069C20
		private void UpdateVehicleTypeValues()
		{
			VehicleDefinition vehicleDefinition = BlockSteeringWheel.FindVehicleDefinition(this.vehicleType);
			this.turnMode = vehicleDefinition.turnMode;
			this.driveMode = vehicleDefinition.driveMode;
			WheelDefinition wheelDefinition = BlockAbstractWheel.FindWheelDefinition(vehicleDefinition.wheelDefinitionName);
			WheelDefinition def = wheelDefinition;
			WheelDefinition def2 = wheelDefinition;
			if (!string.IsNullOrEmpty(vehicleDefinition.backWheelDefinitionName))
			{
				def = BlockAbstractWheel.FindWheelDefinition(vehicleDefinition.backWheelDefinitionName);
			}
			if (!string.IsNullOrEmpty(vehicleDefinition.backWheelDefinitionName))
			{
				def2 = BlockAbstractWheel.FindWheelDefinition(vehicleDefinition.frontWheelDefinitionName);
			}
			HashSet<BlockAbstractWheel> done = new HashSet<BlockAbstractWheel>();
			this.SetDefinitionForAll(def2, done, new List<BlockAbstractWheel>[]
			{
				this.frontWheels
			});
			this.SetDefinitionForAll(def, done, new List<BlockAbstractWheel>[]
			{
				this.backWheels
			});
			this.SetBallastFraction(vehicleDefinition.ballastFraction, false);
		}

		// Token: 0x06000FDB RID: 4059 RVA: 0x0006B8E0 File Offset: 0x00069CE0
		public TileResultCode SetVehicleType(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string b = this.vehicleType;
			this.vehicleType = Util.GetStringArg(args, 0, string.Empty);
			if (this.vehicleType != b)
			{
				this.UpdateVehicleTypeValues();
			}
			return TileResultCode.True;
		}

		// Token: 0x06000FDC RID: 4060 RVA: 0x0006B91E File Offset: 0x00069D1E
		public TileResultCode SetDriveControl(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.controlType = Util.GetStringArg(args, 0, "Mover");
			this.hasSetControlType = true;
			return TileResultCode.True;
		}

		// Token: 0x06000FDD RID: 4061 RVA: 0x0006B93C File Offset: 0x00069D3C
		public TileResultCode SetBallastFraction(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float floatArg = Util.GetFloatArg(args, 0, 0.5f);
			this.SetBallastFraction(floatArg, false);
			return TileResultCode.True;
		}

		// Token: 0x06000FDE RID: 4062 RVA: 0x0006B960 File Offset: 0x00069D60
		public TileResultCode SetSuspensionHeight(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float externalSuspensionHeight = Util.GetFloatArg(args, 0, 0.25f) * eInfo.floatArg;
			for (int i = 0; i < this.allWheels.Count; i++)
			{
				this.allWheels[i].SetExternalSuspensionHeight(externalSuspensionHeight);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000FDF RID: 4063 RVA: 0x0006B9B0 File Offset: 0x00069DB0
		public TileResultCode SetSuspensionLength(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float floatArg = Util.GetFloatArg(args, 0, 0.5f);
			for (int i = 0; i < this.allWheels.Count; i++)
			{
				this.allWheels[i].SetExternalSuspensionLength(floatArg);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000FE0 RID: 4064 RVA: 0x0006B9FC File Offset: 0x00069DFC
		public TileResultCode SetSuspensionDamper(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float floatArg = Util.GetFloatArg(args, 0, 2f);
			for (int i = 0; i < this.allWheels.Count; i++)
			{
				this.allWheels[i].SetExternalSuspensionDamper(floatArg);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000FE1 RID: 4065 RVA: 0x0006BA48 File Offset: 0x00069E48
		public TileResultCode SetSuspensionSpring(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float floatArg = Util.GetFloatArg(args, 0, 5f);
			for (int i = 0; i < this.allWheels.Count; i++)
			{
				this.allWheels[i].SetExternalSuspensionSpring(floatArg);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000FE2 RID: 4066 RVA: 0x0006BA91 File Offset: 0x00069E91
		public TileResultCode SetTurnMode(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.turnMode = (VehicleTurnMode)Util.GetIntArg(args, 0, 0);
			return TileResultCode.True;
		}

		// Token: 0x06000FE3 RID: 4067 RVA: 0x0006BAA2 File Offset: 0x00069EA2
		public TileResultCode SetDriveMode(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.driveMode = (VehicleDriveMode)Util.GetIntArg(args, 0, 0);
			return TileResultCode.True;
		}

		// Token: 0x06000FE4 RID: 4068 RVA: 0x0006BAB4 File Offset: 0x00069EB4
		public TileResultCode MoveAlongMover(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string stringArg = Util.GetStringArg(args, 0, "L");
			Blocksworld.UI.Controls.EnableDPad(stringArg, MoverDirectionMask.ALL);
			Vector3 worldDPadOffset = Blocksworld.UI.Controls.GetWorldDPadOffset(stringArg);
			if (worldDPadOffset.sqrMagnitude > 0.01f)
			{
				float floatArg = Util.GetFloatArg(args, 1, 45f);
				float floatArg2 = Util.GetFloatArg(args, 2, 20f);
				this.moveAlongDirection += worldDPadOffset;
				this.moveAlongMaxAngle = Mathf.Max(this.moveAlongMaxAngle, floatArg);
				this.moveAlongMaxForce = Mathf.Max(this.moveAlongMaxForce, floatArg2);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000FE5 RID: 4069 RVA: 0x0006BB53 File Offset: 0x00069F53
		public TileResultCode Flip(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.flipping += Util.GetFloatArg(args, 0, 1f);
			return TileResultCode.True;
		}

		// Token: 0x06000FE6 RID: 4070 RVA: 0x0006BB70 File Offset: 0x00069F70
		public TileResultCode MoverSteer(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string stringArg = Util.GetStringArg(args, 0, "L");
			Blocksworld.UI.Controls.EnableDPad(stringArg, MoverDirectionMask.LEFT | MoverDirectionMask.RIGHT);
			Vector2 normalizedDPadOffset = Blocksworld.UI.Controls.GetNormalizedDPadOffset(stringArg);
			if (normalizedDPadOffset.x * normalizedDPadOffset.x > 0.01f)
			{
				float floatArg = Util.GetFloatArg(args, 1, 45f);
				float angle = -normalizedDPadOffset.x * floatArg;
				this.Turn(angle);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000FE7 RID: 4071 RVA: 0x0006BBE8 File Offset: 0x00069FE8
		public TileResultCode TiltSteer(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (!TiltManager.Instance.IsMonitoring())
			{
				return TileResultCode.Delayed;
			}
			float tiltTwist = TiltManager.Instance.GetTiltTwist();
			float x = TiltManager.Instance.GetRelativeGravityVector().x;
			float f = (Mathf.Abs(tiltTwist) <= Mathf.Abs(x)) ? (-x) : (-tiltTwist);
			float num = Mathf.Asin(f) * 57.29578f;
			float floatArg = Util.GetFloatArg(args, 1, 45f);
			num = Mathf.Clamp(num, -floatArg, floatArg);
			Blocksworld.UI.Controls.UpdateTiltPrompt();
			this.Turn(num);
			return TileResultCode.True;
		}

		// Token: 0x06000FE8 RID: 4072 RVA: 0x0006BC84 File Offset: 0x0006A084
		public TileResultCode TiltDrive(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (!TiltManager.Instance.IsMonitoring())
			{
				return TileResultCode.Delayed;
			}
			float num = -TiltManager.Instance.GetRelativeGravityVector().y;
			if (num < 0f)
			{
				num *= 0.75f;
			}
			float num2 = 3f * Util.GetFloatArg(args, 0, 1f);
			this.Drive(num2 * num);
			return TileResultCode.True;
		}

		// Token: 0x06000FE9 RID: 4073 RVA: 0x0006BCE8 File Offset: 0x0006A0E8
		public TileResultCode MoveLocalAlongMover(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string stringArg = Util.GetStringArg(args, 0, "L");
			if (this.go == null)
			{
				return TileResultCode.True;
			}
			Blocksworld.UI.Controls.EnableDPad(stringArg, MoverDirectionMask.ALL);
			Vector2 a = Blocksworld.UI.Controls.GetNormalizedDPadOffset(stringArg);
			if (a.y < 0f)
			{
				a *= 0.75f;
			}
			Vector3 b = this.goT.forward * a.y;
			if (b.sqrMagnitude > 0.01f)
			{
				float b2 = this.storeForce;
				this.moveAlongDirection += b;
				this.moveAlongMaxAngle = 45f;
				this.moveAlongMaxForce = Mathf.Max(this.moveAlongMaxForce, b2);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000FEA RID: 4074 RVA: 0x0006BDB8 File Offset: 0x0006A1B8
		public TileResultCode MoveLocalAlongTilt(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.go == null)
			{
				return TileResultCode.True;
			}
			if (!TiltManager.Instance.IsMonitoring())
			{
				return TileResultCode.Delayed;
			}
			float num = TiltManager.Instance.GetRelativeGravityVector().y;
			if (num < 0f)
			{
				num *= 0.75f;
			}
			Vector3 b = this.goT.forward * num;
			if (b.sqrMagnitude > 0.01f)
			{
				float b2 = this.storeForce;
				this.moveAlongDirection += b;
				this.moveAlongMaxAngle = 45f;
				this.moveAlongMaxForce = Mathf.Max(this.moveAlongMaxForce, b2);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000FEB RID: 4075 RVA: 0x0006BE6C File Offset: 0x0006A26C
		public TileResultCode DriveThroughTag(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string stringArg = Util.GetStringArg(args, 0, string.Empty);
			Vector3 position = this.goT.position;
			Block block;
			if (TagManager.TryGetClosestBlockWithTag(stringArg, position, out block, this.chunkBlocks))
			{
				Vector3 vector = block.goT.position - position;
				if (vector.sqrMagnitude > 0.01f)
				{
					float floatArg = Util.GetFloatArg(args, 1, 45f);
					float floatArg2 = Util.GetFloatArg(args, 2, 20f);
					this.moveAlongDirection += vector.normalized;
					this.moveAlongMaxAngle = Mathf.Max(this.moveAlongMaxAngle, floatArg);
					this.moveAlongMaxForce = Mathf.Max(this.moveAlongMaxForce, floatArg2);
				}
			}
			return TileResultCode.True;
		}

		// Token: 0x06000FEC RID: 4076 RVA: 0x0006BF24 File Offset: 0x0006A324
		public TileResultCode AvoidTag(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string stringArg = Util.GetStringArg(args, 0, string.Empty);
			Vector3 position = this.goT.position;
			Block block;
			if (TagManager.TryGetClosestBlockWithTag(stringArg, position, out block, this.chunkBlocks))
			{
				Vector3 a;
				if (block.size.sqrMagnitude > 4f)
				{
					a = block.go.GetComponent<Collider>().ClosestPointOnBounds(position);
				}
				else
				{
					a = block.goT.position;
				}
				Vector3 a2 = a - position;
				float magnitude = a2.magnitude;
				float floatArg = Util.GetFloatArg(args, 1, 10f);
				if (magnitude > 0.01f && magnitude < floatArg)
				{
					float floatArg2 = Util.GetFloatArg(args, 2, 45f);
					float floatArg3 = Util.GetFloatArg(args, 3, 10f);
					Vector3 a3 = a2 / magnitude;
					float d = 2f * Mathf.Clamp(1f - magnitude / floatArg, 0f, 1f);
					this.moveAlongDirection -= d * a3;
					this.moveAlongMaxAngle = Mathf.Max(this.moveAlongMaxAngle, floatArg2);
					this.moveAlongMaxForce = Mathf.Max(this.moveAlongMaxForce, floatArg3);
				}
			}
			return TileResultCode.True;
		}

		// Token: 0x06000FED RID: 4077 RVA: 0x0006C058 File Offset: 0x0006A458
		public TileResultCode TurnAlongMover(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string stringArg = Util.GetStringArg(args, 0, "L");
			Blocksworld.UI.Controls.EnableDPad(stringArg, MoverDirectionMask.ALL);
			Vector3 worldDPadOffset = Blocksworld.UI.Controls.GetWorldDPadOffset(stringArg);
			if (worldDPadOffset.x * worldDPadOffset.x > 0.01f)
			{
				float floatArg = Util.GetFloatArg(args, 1, 20f);
				this.TurnAlongDirection(worldDPadOffset, floatArg);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000FEE RID: 4078 RVA: 0x0006C0C4 File Offset: 0x0006A4C4
		public TileResultCode DriveAlongMover(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string stringArg = Util.GetStringArg(args, 0, "L");
			Blocksworld.UI.Controls.EnableDPad(stringArg, MoverDirectionMask.ALL);
			Vector3 worldDPadOffset = Blocksworld.UI.Controls.GetWorldDPadOffset(stringArg);
			if (worldDPadOffset.y * worldDPadOffset.y > 0.01f)
			{
				float floatArg = Util.GetFloatArg(args, 1, 20f);
				this.DriveAlongDirection(worldDPadOffset, -floatArg);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000FEF RID: 4079 RVA: 0x0006C134 File Offset: 0x0006A534
		public TileResultCode Drive(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float num = eInfo.floatArg * Util.GetFloatArg(args, 0, 20f);
			float force = Mathf.Sign(-num) * this.storeForce;
			this.Drive(force);
			return TileResultCode.True;
		}

		// Token: 0x06000FF0 RID: 4080 RVA: 0x0006C16C File Offset: 0x0006A56C
		public TileResultCode IsBrakeEffectiveness(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float num = Util.GetFloatArg(args, 0, 1f) * 0.01f;
			return (num != this.brakeEffectiveness) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000FF1 RID: 4081 RVA: 0x0006C19F File Offset: 0x0006A59F
		public TileResultCode SetBrakeEffectiveness(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.brakeEffectiveness = Util.GetFloatArg(args, 0, 1f) * eInfo.floatArg * 0.01f;
			return TileResultCode.True;
		}

		// Token: 0x06000FF2 RID: 4082 RVA: 0x0006C1C4 File Offset: 0x0006A5C4
		public TileResultCode IsBraking(ScriptRowExecutionInfo eInfo, object[] args)
		{
			bool flag = this.driveState == BlockSteeringWheel.DriveStates.FORWARD_BRAKING || this.driveState == BlockSteeringWheel.DriveStates.REVERSE_BRAKING;
			return (!flag) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000FF3 RID: 4083 RVA: 0x0006C1F8 File Offset: 0x0006A5F8
		public TileResultCode IsDriving(ScriptRowExecutionInfo eInfo, object[] args)
		{
			bool flag = this.driveState == BlockSteeringWheel.DriveStates.FORWARD || this.driveState == BlockSteeringWheel.DriveStates.FORWARD_BRAKING;
			return (!flag) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000FF4 RID: 4084 RVA: 0x0006C22C File Offset: 0x0006A62C
		public TileResultCode IsIdling(ScriptRowExecutionInfo eInfo, object[] args)
		{
			bool flag = this.driveState == BlockSteeringWheel.DriveStates.IDLE;
			return (!flag) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000FF5 RID: 4085 RVA: 0x0006C250 File Offset: 0x0006A650
		public TileResultCode IsReversing(ScriptRowExecutionInfo eInfo, object[] args)
		{
			bool flag = this.driveState == BlockSteeringWheel.DriveStates.REVERSE || this.driveState == BlockSteeringWheel.DriveStates.REVERSE_BRAKING;
			return (!flag) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000FF6 RID: 4086 RVA: 0x0006C284 File Offset: 0x0006A684
		public TileResultCode IsTurning(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float floatArg = Util.GetFloatArg(args, 0, 0f);
			bool flag = false;
			if (floatArg < 0f)
			{
				flag = (this.visualAngle > -floatArg);
			}
			else if (floatArg > 0f)
			{
				flag = (this.visualAngle < -floatArg);
			}
			return (!flag) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000FF7 RID: 4087 RVA: 0x0006C2DE File Offset: 0x0006A6DE
		public TileResultCode ShowSpeedometer(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.turnOnSpeedometer = true;
			return TileResultCode.True;
		}

		// Token: 0x06000FF8 RID: 4088 RVA: 0x0006C2E8 File Offset: 0x0006A6E8
		private void TurnWheel(BlockAbstractWheel w, float angle)
		{
			if (!w.isSpareTire)
			{
				w.Turn(angle);
			}
		}

		// Token: 0x06000FF9 RID: 4089 RVA: 0x0006C2FC File Offset: 0x0006A6FC
		private void TurnStandard(float angle)
		{
			for (int i = 0; i < this.turnWheels.Count; i++)
			{
				BlockAbstractWheel w = this.turnWheels[i];
				this.TurnWheel(w, -angle);
			}
			this.turnAngle += angle;
		}

		// Token: 0x06000FFA RID: 4090 RVA: 0x0006C34C File Offset: 0x0006A74C
		private void TurnAll(float angle)
		{
			for (int i = 0; i < this.turnWheels.Count; i++)
			{
				BlockAbstractWheel w = this.turnWheels[i];
				this.TurnWheel(w, -angle);
			}
			for (int j = 0; j < this.inverseTurnWheels.Count; j++)
			{
				BlockAbstractWheel w2 = this.inverseTurnWheels[j];
				this.TurnWheel(w2, angle);
			}
			this.turnAngle += angle;
		}

		// Token: 0x06000FFB RID: 4091 RVA: 0x0006C3CC File Offset: 0x0006A7CC
		private void TurnBack(float angle)
		{
			for (int i = 0; i < this.inverseTurnWheels.Count; i++)
			{
				BlockAbstractWheel w = this.inverseTurnWheels[i];
				this.TurnWheel(w, -angle);
			}
			this.turnAngle += angle;
		}

		// Token: 0x06000FFC RID: 4092 RVA: 0x0006C41C File Offset: 0x0006A81C
		private void TankTurnAll(float f)
		{
			this.DriveAll(this.leftTankTurn_Wheels, f, true);
			this.DriveAll(this.rightTankTurn_Wheels, -f, true);
			this.DriveAll(this.inverseLeftTankTurn_Wheels, f, true);
			this.DriveAll(this.inverseRightTankTurn_Wheels, -f, true);
			this.turnAngle += f * 3f;
		}

		// Token: 0x06000FFD RID: 4093 RVA: 0x0006C477 File Offset: 0x0006A877
		private void Turn_TankWheels(float f)
		{
			this.DriveAll(this.leftTurn_TankWheels, f);
			this.DriveAll(this.rightTurn_TankWheels, -f);
			this.DriveAll(this.inverseLeftTurn_TankWheels, f);
			this.DriveAll(this.inverseRightTurn_TankWheels, -f);
		}

		// Token: 0x06000FFE RID: 4094 RVA: 0x0006C4B0 File Offset: 0x0006A8B0
		private void Turn(float angle)
		{
			float num = angle * 0.5f * this.storeTurn / 45f;
			float num2 = 1f;
			if (this.chunk.rb != null)
			{
				num2 = 1f / Mathf.Clamp(this.chunk.rb.velocity.magnitude / 50f, 1.5f, 3f);
			}
			angle = Mathf.Clamp(angle, -this.storeTurn * num2, this.storeTurn * num2);
			switch (this.turnMode)
			{
			case VehicleTurnMode.FRONT:
				this.TurnStandard(angle);
				break;
			case VehicleTurnMode.BACK:
				this.TurnBack(-angle);
				break;
			case VehicleTurnMode.FRONT_AND_BACK:
				this.TurnAll(angle);
				break;
			case VehicleTurnMode.TANK:
				this.TankTurnAll(num);
				break;
			}
			this.Turn_TankWheels(num * 0.25f);
			this.helpForceTurnAngle += angle;
		}

		// Token: 0x06000FFF RID: 4095 RVA: 0x0006C5AC File Offset: 0x0006A9AC
		public TileResultCode Turn(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float num = Mathf.Min(1f, 2f * eInfo.floatArg) * Util.GetFloatArg(args, 0, 45f);
			float angle = -num;
			this.Turn(angle);
			return TileResultCode.True;
		}

		// Token: 0x06001000 RID: 4096 RVA: 0x0006C5E8 File Offset: 0x0006A9E8
		public TileResultCode SetupTurn(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.storeTurn = Mathf.Min(1f, 2f * eInfo.floatArg) * Util.GetFloatArg(args, 0, 45f);
			return TileResultCode.True;
		}

		// Token: 0x06001001 RID: 4097 RVA: 0x0006C614 File Offset: 0x0006AA14
		public TileResultCode SetupDriver(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.storeForce = eInfo.floatArg * Util.GetFloatArg(args, 0, 20f);
			this.storeForce = 10f + 1.5f * this.storeForce;
			return TileResultCode.True;
		}

		// Token: 0x06001002 RID: 4098 RVA: 0x0006C648 File Offset: 0x0006AA48
		private void BrakeAll(List<BlockAbstractWheel> wheels, float f)
		{
			for (int i = 0; i < wheels.Count; i++)
			{
				BlockAbstractWheel blockAbstractWheel = wheels[i];
				if (!blockAbstractWheel.isSpareTire)
				{
					blockAbstractWheel.Brake(f);
				}
			}
		}

		// Token: 0x06001003 RID: 4099 RVA: 0x0006C688 File Offset: 0x0006AA88
		private void DriveAll(List<BlockAbstractWheel> wheels, float f, bool drivenToTurn = false)
		{
			for (int i = 0; i < wheels.Count; i++)
			{
				BlockAbstractWheel blockAbstractWheel = wheels[i];
				if (!blockAbstractWheel.isSpareTire)
				{
					blockAbstractWheel.Drive(f, drivenToTurn);
				}
			}
		}

		// Token: 0x06001004 RID: 4100 RVA: 0x0006C6C8 File Offset: 0x0006AAC8
		private void DriveAll(List<BlockTankTreadsWheel> wheels, float f)
		{
			for (int i = 0; i < wheels.Count; i++)
			{
				BlockTankTreadsWheel blockTankTreadsWheel = wheels[i];
				blockTankTreadsWheel.Drive(f);
			}
		}

		// Token: 0x06001005 RID: 4101 RVA: 0x0006C6FC File Offset: 0x0006AAFC
		private void SpinAll(List<BlockAbstractWheel> wheels)
		{
			for (int i = 0; i < wheels.Count; i++)
			{
				BlockAbstractWheel blockAbstractWheel = wheels[i];
				if (!blockAbstractWheel.isSpareTire)
				{
					blockAbstractWheel.KeepSpinning();
				}
			}
		}

		// Token: 0x06001006 RID: 4102 RVA: 0x0006C739 File Offset: 0x0006AB39
		public TileResultCode Jump(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.jumpHeight += Util.GetFloatArg(args, 0, 0f) * eInfo.floatArg;
			if (this.jumpCounter - 5 > 40)
			{
				this.jumpCounter = 0;
			}
			return TileResultCode.True;
		}

		// Token: 0x06001007 RID: 4103 RVA: 0x0006C774 File Offset: 0x0006AB74
		private void ClearFunctionalWheels()
		{
			this.allControlledWheels.Clear();
			this.allControlledTankWheels.Clear();
			this.turnWheels.Clear();
			this.inverseTurnWheels.Clear();
			this.driveWheels.Clear();
			this.inverseDriveWheels.Clear();
			this.leftTankTurn_Wheels.Clear();
			this.rightTankTurn_Wheels.Clear();
			this.inverseLeftTankTurn_Wheels.Clear();
			this.inverseRightTankTurn_Wheels.Clear();
			this.frontDriveWheels.Clear();
			this.backDriveWheels.Clear();
			this.frontInverseDriveWheels.Clear();
			this.backInverseDriveWheels.Clear();
			this.drive_TankWheels.Clear();
			this.inverseDrive_TankWheels.Clear();
			this.leftTurn_TankWheels.Clear();
			this.rightTurn_TankWheels.Clear();
			this.inverseLeftTurn_TankWheels.Clear();
			this.inverseRightTurn_TankWheels.Clear();
			this.frontWheels.Clear();
			this.backWheels.Clear();
		}

		// Token: 0x06001008 RID: 4104 RVA: 0x0006C874 File Offset: 0x0006AC74
		private void GatherWheelFunctions()
		{
			this.ClearFunctionalWheels();
			Transform goT = this.goT;
			Vector3 right = goT.right;
			foreach (BlockTankTreadsWheel blockTankTreadsWheel in this.allMainTankWheels)
			{
				Transform goT2 = blockTankTreadsWheel.goT;
				Vector3 position = goT2.position;
				Vector3 right2 = goT2.right;
				Vector3 v = position - this.vehicleCenter;
				Vector3 vector = goT.worldToLocalMatrix.MultiplyVector(v);
				float num = Vector3.Dot(right2, right);
				if (Mathf.Abs(num) > 0.1f)
				{
					if (num < 0f)
					{
						this.drive_TankWheels.Add(blockTankTreadsWheel);
						if (vector.x < -0.1f)
						{
							this.leftTurn_TankWheels.Add(blockTankTreadsWheel);
						}
						else if (vector.x > 0.1f)
						{
							this.rightTurn_TankWheels.Add(blockTankTreadsWheel);
						}
					}
					else
					{
						this.inverseDrive_TankWheels.Add(blockTankTreadsWheel);
						if (vector.x > 0.1f)
						{
							this.inverseLeftTurn_TankWheels.Add(blockTankTreadsWheel);
						}
						else if (vector.x < -0.1f)
						{
							this.inverseRightTurn_TankWheels.Add(blockTankTreadsWheel);
						}
					}
					this.allControlledTankWheels.Add(blockTankTreadsWheel);
				}
			}
			this.averageTurnWheelRadius = 0f;
			foreach (BlockAbstractWheel blockAbstractWheel in this.allWheels)
			{
				Transform goT3 = blockAbstractWheel.goT;
				Vector3 position2 = goT3.position;
				Vector3 right3 = goT3.right;
				Vector3 v2 = position2 - this.vehicleCenter;
				Vector3 vector2 = goT.worldToLocalMatrix.MultiplyVector(v2);
				bool flag = false;
				if (vector2.z < -0.4f)
				{
					this.inverseTurnWheels.Add(blockAbstractWheel);
					flag = true;
				}
				else if (vector2.z > 0.4f)
				{
					this.turnWheels.Add(blockAbstractWheel);
					flag = true;
					this.averageTurnWheelRadius += blockAbstractWheel.GetRadius();
				}
				bool flag2 = vector2.z >= 0f;
				float num2 = Vector3.Dot(right3, right);
				if (Mathf.Abs(num2) > 0.1f)
				{
					if (num2 < 0f)
					{
						this.driveWheels.Add(blockAbstractWheel);
						if (flag2)
						{
							this.frontDriveWheels.Add(blockAbstractWheel);
						}
						if (!flag2)
						{
							this.backDriveWheels.Add(blockAbstractWheel);
						}
						flag = true;
						if (vector2.x < -0.1f)
						{
							this.leftTankTurn_Wheels.Add(blockAbstractWheel);
						}
						else if (vector2.x > 0.1f)
						{
							this.rightTankTurn_Wheels.Add(blockAbstractWheel);
						}
					}
					else
					{
						this.inverseDriveWheels.Add(blockAbstractWheel);
						if (flag2)
						{
							this.frontInverseDriveWheels.Add(blockAbstractWheel);
						}
						if (!flag2)
						{
							this.backInverseDriveWheels.Add(blockAbstractWheel);
						}
						flag = true;
						if (vector2.x > 0.1f)
						{
							this.inverseLeftTankTurn_Wheels.Add(blockAbstractWheel);
						}
						else if (vector2.x < -0.1f)
						{
							this.inverseRightTankTurn_Wheels.Add(blockAbstractWheel);
						}
					}
				}
				if (flag)
				{
					this.allControlledWheels.Add(blockAbstractWheel);
				}
				if (flag2)
				{
					this.frontWheels.Add(blockAbstractWheel);
				}
				else
				{
					this.backWheels.Add(blockAbstractWheel);
				}
			}
			if (this.turnWheels.Count > 0)
			{
				this.averageTurnWheelRadius /= (float)this.turnWheels.Count;
			}
			else
			{
				this.averageTurnWheelRadius = 1f;
			}
		}

		// Token: 0x06001009 RID: 4105 RVA: 0x0006CC90 File Offset: 0x0006B090
		private void GetEngineSoundFromWheels()
		{
			Dictionary<string, int> dictionary = new Dictionary<string, int>();
			foreach (BlockAbstractWheel blockAbstractWheel in this.allWheels)
			{
				string text = blockAbstractWheel.GetBlockMetaData().preferredEngineSound;
				if (string.IsNullOrEmpty(text))
				{
					text = "Default";
				}
				int num = (!this.driveWheels.Contains(blockAbstractWheel)) ? 1 : 2;
				if (!dictionary.ContainsKey(text))
				{
					dictionary[text] = 0;
				}
				dictionary[text] += num;
			}
			int num2 = -1;
			List<string> list = new List<string>
			{
				"Default"
			};
			foreach (KeyValuePair<string, int> keyValuePair in dictionary)
			{
				if (keyValuePair.Value > num2)
				{
					num2 = keyValuePair.Value;
					list.Insert(0, keyValuePair.Key);
				}
			}
			foreach (string name in list)
			{
				EngineSoundDefinition engineSoundDefinition = Blocksworld.engineSoundDefinitions.GetEngineSoundDefinition(name);
				if (engineSoundDefinition != null)
				{
					this.engineSoundDefinition = engineSoundDefinition;
					this.loopName = this.engineSoundDefinition.loopSFXName;
					break;
				}
			}
		}

		// Token: 0x0600100A RID: 4106 RVA: 0x0006CE40 File Offset: 0x0006B240
		private static List<List<Tile>> GetDriveControlTiles()
		{
			return new List<List<Tile>>
			{
				new List<Tile>
				{
					new Tile(Block.predicateSendCustomSignalModel, new object[]
					{
						"BSW_Mover"
					}),
					Block.ThenTile(),
					new Tile(BlockSteeringWheel.predicateSteeringWheelMoveLocalAlongMover, new object[]
					{
						"L",
						90f
					}),
					new Tile(BlockSteeringWheel.predicateSteeringWheelMoverSteer, new object[]
					{
						"L",
						90f
					})
				},
				new List<Tile>
				{
					new Tile(Block.predicateSendCustomSignalModel, new object[]
					{
						"BSW_Buttons"
					}),
					new Tile(Block.predicateButton, new object[]
					{
						"Up"
					}),
					Block.ThenTile(),
					new Tile(BlockSteeringWheel.predicateDrive, new object[]
					{
						1f
					})
				},
				new List<Tile>
				{
					new Tile(Block.predicateSendCustomSignalModel, new object[]
					{
						"BSW_Buttons"
					}),
					new Tile(Block.predicateButton, new object[]
					{
						"Down"
					}),
					Block.ThenTile(),
					new Tile(BlockSteeringWheel.predicateDrive, new object[]
					{
						-0.75f
					})
				},
				new List<Tile>
				{
					new Tile(Block.predicateSendCustomSignalModel, new object[]
					{
						"BSW_Buttons"
					}),
					new Tile(Block.predicateButton, new object[]
					{
						"Left"
					}),
					Block.ThenTile(),
					new Tile(BlockSteeringWheel.predicateTurn, new object[]
					{
						-90f
					})
				},
				new List<Tile>
				{
					new Tile(Block.predicateSendCustomSignalModel, new object[]
					{
						"BSW_Buttons"
					}),
					new Tile(Block.predicateButton, new object[]
					{
						"Right"
					}),
					Block.ThenTile(),
					new Tile(BlockSteeringWheel.predicateTurn, new object[]
					{
						90f
					})
				},
				new List<Tile>
				{
					new Tile(Block.predicateSendCustomSignalModel, new object[]
					{
						"BSW_Combined"
					}),
					new Tile(Block.predicateButton, new object[]
					{
						"Up"
					}),
					Block.ThenTile(),
					new Tile(BlockSteeringWheel.predicateDrive, new object[]
					{
						1f
					})
				},
				new List<Tile>
				{
					new Tile(Block.predicateSendCustomSignalModel, new object[]
					{
						"BSW_Combined"
					}),
					new Tile(Block.predicateButton, new object[]
					{
						"Down"
					}),
					Block.ThenTile(),
					new Tile(BlockSteeringWheel.predicateDrive, new object[]
					{
						-0.75f
					})
				},
				new List<Tile>
				{
					new Tile(Block.predicateSendCustomSignalModel, new object[]
					{
						"BSW_Combined"
					}),
					Block.ThenTile(),
					new Tile(BlockSteeringWheel.predicateSteeringWheelMoverSteer, new object[]
					{
						"L",
						90f
					})
				},
				new List<Tile>
				{
					new Tile(Block.predicateSendCustomSignalModel, new object[]
					{
						"BSW_Tilt"
					}),
					Block.ThenTile(),
					new Tile(BlockSteeringWheel.predicateSteeringWheelMoveLocalAlongTilt, new object[]
					{
						90f
					}),
					new Tile(BlockSteeringWheel.predicateSteeringWheelTiltSteer, new object[]
					{
						90f
					})
				},
				new List<Tile>
				{
					new Tile(Block.predicateSendCustomSignalModel, new object[]
					{
						"BSW_TiltCombined"
					}),
					Block.ThenTile(),
					new Tile(BlockSteeringWheel.predicateSteeringWheelTiltSteer, new object[]
					{
						90f
					})
				},
				new List<Tile>
				{
					new Tile(Block.predicateSendCustomSignalModel, new object[]
					{
						"BSW_TiltCombined"
					}),
					new Tile(Block.predicateButton, new object[]
					{
						"Up"
					}),
					Block.ThenTile(),
					new Tile(BlockSteeringWheel.predicateDrive, new object[]
					{
						1f
					})
				},
				new List<Tile>
				{
					new Tile(Block.predicateSendCustomSignalModel, new object[]
					{
						"BSW_TiltCombined"
					}),
					new Tile(Block.predicateButton, new object[]
					{
						"Down"
					}),
					Block.ThenTile(),
					new Tile(BlockSteeringWheel.predicateDrive, new object[]
					{
						-0.75f
					})
				},
				Block.EmptyTileRow()
			};
		}

		// Token: 0x0600100B RID: 4107 RVA: 0x0006D3DC File Offset: 0x0006B7DC
		private void AddDriveControlEquivalent(List<Tile> row, Tile t)
		{
			row.Add(t);
			if (BlockSteeringWheel.predicateDriveControl == t.gaf.Predicate)
			{
				string text = "BSW_" + Util.GetStringArg(t.gaf.Args, 0, "Mover");
				row.Add(new Tile(new GAF(Block.predicateSendCustomSignalModel, new object[]
				{
					text
				})));
			}
		}

		// Token: 0x0600100C RID: 4108 RVA: 0x0006D448 File Offset: 0x0006B848
		private void SetupDriveControlTiles()
		{
			this.steeringWheelTiles = new List<List<Tile>>();
			for (int i = 0; i < this.tiles.Count; i++)
			{
				List<Tile> list = new List<Tile>();
				for (int j = 0; j < this.tiles[i].Count; j++)
				{
					this.AddDriveControlEquivalent(list, this.tiles[i][j]);
				}
				this.steeringWheelTiles.Add(list);
			}
			this.steeringWheelTiles.AddRange(BlockSteeringWheel.GetDriveControlTiles());
			base.CreateFlattenTiles();
		}

		// Token: 0x0600100D RID: 4109 RVA: 0x0006D4E0 File Offset: 0x0006B8E0
		public override void Play()
		{
			base.Play();
			this.engineSoundOn = true;
			this.engineSoundRPM = 0f;
			this.loopClip = null;
			this.SetupDriveControlTiles();
			this.hasSetControlType = false;
			this.hadSetControlType = false;
			this.vehicleType = "Regular";
			this.controlType = string.Empty;
			this.storeForce = 17.6f;
			this.storeTurn = 45f;
			this.brakeEffectiveness = 1f;
			this.onGroundFraction = 0f;
			this.alignUp = this.goT.up;
			this.turnMode = VehicleTurnMode.FRONT;
			this.driveMode = VehicleDriveMode.ALL;
			this.jumpCounter = 45;
			this.jumpHeight = 0f;
			this.treatAsVehicleStatus = -1;
			this.allWheels.Clear();
			this.allMainTankWheels.Clear();
			this.jumpBlocks.Clear();
			this.chunkBlocks.Clear();
			this.chunkBlocks.UnionWith(this.chunk.blocks);
			this.wheelBlocks = new List<Block>();
			List<Block> list = Block.connectedCache[this];
			foreach (Block block in list)
			{
				BlockAbstractWheel blockAbstractWheel = block as BlockAbstractWheel;
				BlockTankTreadsWheel blockTankTreadsWheel = block as BlockTankTreadsWheel;
				if (blockAbstractWheel != null || (blockTankTreadsWheel != null && blockTankTreadsWheel.IsMainBlockInGroup()))
				{
					if (blockAbstractWheel != null)
					{
						this.allWheels.Add(blockAbstractWheel);
						this.jumpBlocks.Add(blockAbstractWheel);
						this.wheelBlocks.Add(blockAbstractWheel);
					}
					if (blockTankTreadsWheel != null)
					{
						this.allMainTankWheels.Add(blockTankTreadsWheel);
						foreach (Block item in blockTankTreadsWheel.group.GetBlocks())
						{
							this.wheelBlocks.Add(item);
							this.jumpBlocks.Add(item);
						}
					}
				}
			}
			this.jumpBlocks.Add(this);
			this.wheelsBounds = Util.ComputeBounds(this.wheelBlocks);
			this.vehicleCenter = this.wheelsBounds.center;
			this.GatherWheelFunctions();
			this.GetEngineSoundFromWheels();
			this.UpdateVehicleTypeValues();
		}

		// Token: 0x0600100E RID: 4110 RVA: 0x0006D728 File Offset: 0x0006BB28
		public override List<List<Tile>> GetRuntimeTiles()
		{
			return (this.steeringWheelTiles.Count <= 0) ? base.GetRuntimeTiles() : this.steeringWheelTiles;
		}

		// Token: 0x0600100F RID: 4111 RVA: 0x0006D74C File Offset: 0x0006BB4C
		public override void RemovedPlayBlock(Block b)
		{
			base.RemovedPlayBlock(b);
			bool flag = false;
			BlockAbstractWheel blockAbstractWheel = b as BlockAbstractWheel;
			if (blockAbstractWheel != null)
			{
				flag = true;
				this.allWheels.Remove(blockAbstractWheel);
			}
			BlockTankTreadsWheel blockTankTreadsWheel = b as BlockTankTreadsWheel;
			if (blockTankTreadsWheel != null)
			{
				flag = true;
				this.allMainTankWheels.Remove(blockTankTreadsWheel);
			}
			if (flag)
			{
				this.GatherWheelFunctions();
			}
		}

		// Token: 0x06001010 RID: 4112 RVA: 0x0006D7A8 File Offset: 0x0006BBA8
		private void SetBallastFraction(float fraction, bool force = false)
		{
			if (this.chunk.rb != null)
			{
				Vector3 vector = (1f - fraction) * this.origLocalCM + fraction * this.origLocalVehicleCenter;
				if (force || (vector - this.chunk.rb.centerOfMass).sqrMagnitude > 0.0001f)
				{
					this.chunk.rb.centerOfMass = vector;
				}
			}
		}

		// Token: 0x06001011 RID: 4113 RVA: 0x0006D830 File Offset: 0x0006BC30
		private void ResetHandling()
		{
			if (this.chunk.rb != null)
			{
				this.chunk.rb.ResetCenterOfMass();
				this.chunk.rb.ResetInertiaTensor();
			}
			this.chunk.UpdateCenterOfMass(true);
			this.wheelsBounds = Util.ComputeBounds(this.wheelBlocks);
			this.vehicleCenter = this.wheelsBounds.center;
			if (this.chunk.rb != null)
			{
				this.origLocalCM = this.chunk.rb.centerOfMass;
			}
			this.origLocalVehicleCenter = this.chunk.go.transform.InverseTransformPoint(this.vehicleCenter);
		}

		// Token: 0x06001012 RID: 4114 RVA: 0x0006D8F0 File Offset: 0x0006BCF0
		private void ImproveHandling()
		{
			if (this.allControlledWheels.Count + this.allControlledTankWheels.Count == 0)
			{
				return;
			}
			Rigidbody rb = this.chunk.rb;
			if (rb != null)
			{
				Vector3 inertiaTensor = rb.inertiaTensor;
				Vector3 vector = 5f * this.goT.forward * Vector3.Dot(inertiaTensor, this.goT.forward);
				vector = Util.Abs(vector);
				try
				{
					rb.inertiaTensor = inertiaTensor + vector;
				}
				catch
				{
					BWLog.Info("Unable to set inertia tensor, possibly due to the use of rigidbody constraints in the world.");
				}
			}
			this.origLocalCM = rb.centerOfMass;
			this.origLocalVehicleCenter = this.chunk.go.transform.InverseTransformPoint(this.vehicleCenter);
			this.SetBallastFraction(this.metaData.ballastFraction, true);
		}

		// Token: 0x06001013 RID: 4115 RVA: 0x0006D9E0 File Offset: 0x0006BDE0
		public override void Stop(bool resetBlock)
		{
			this.steeringWheelTiles.Clear();
			base.Stop(resetBlock);
			this.engineSoundOn = false;
			this.UpdateEngineSound();
			foreach (BlockAbstractWheel blockAbstractWheel in this.allWheels)
			{
				blockAbstractWheel.maxSpeedInc = 99999f;
			}
			this.allWheels.Clear();
			this.allMainTankWheels.Clear();
			this.jumpBlocks.Clear();
			this.ClearFunctionalWheels();
			this.chunkBlocks.Clear();
			foreach (Transform transform in this.meshesToTurn)
			{
				transform.localRotation = Quaternion.identity;
			}
			if (this.speedometer != null)
			{
				Blocksworld.UI.Overlay.RemoveSpeedometer(this.speedometer);
			}
			this.speedometer = null;
		}

		// Token: 0x06001014 RID: 4116 RVA: 0x0006DB10 File Offset: 0x0006BF10
		public override bool TreatAsVehicleLikeBlock()
		{
			return base.TreatAsVehicleLikeBlockWithStatus(ref this.treatAsVehicleStatus);
		}

		// Token: 0x04000C52 RID: 3154
		public static Predicate predicateSteeringWheelMoveAlongMover;

		// Token: 0x04000C53 RID: 3155
		public static Predicate predicateSteeringWheelMoveLocalAlongMover;

		// Token: 0x04000C54 RID: 3156
		public static Predicate predicateSteeringWheelMoveLocalAlongTilt;

		// Token: 0x04000C55 RID: 3157
		public static Predicate predicateSteeringWheelMoverSteer;

		// Token: 0x04000C56 RID: 3158
		public static Predicate predicateSteeringWheelTiltSteer;

		// Token: 0x04000C57 RID: 3159
		public static Predicate predicateSteeringWheelTiltDrive;

		// Token: 0x04000C58 RID: 3160
		public static Predicate predicateSetVehicleType;

		// Token: 0x04000C59 RID: 3161
		public static Predicate predicateDrive;

		// Token: 0x04000C5A RID: 3162
		public static Predicate predicateTurn;

		// Token: 0x04000C5B RID: 3163
		public static Predicate predicateSetupTurn;

		// Token: 0x04000C5C RID: 3164
		public static Predicate predicateSetupDrive;

		// Token: 0x04000C5D RID: 3165
		public static Predicate predicateDriveControl;

		// Token: 0x04000C5E RID: 3166
		private SteeringWheelMetaData metaData;

		// Token: 0x04000C5F RID: 3167
		private const float DEFAULT_BRAKE_FORCE = 0.5f;

		// Token: 0x04000C60 RID: 3168
		private const float DEFAULT_DRIVE_FORCE = 17.6f;

		// Token: 0x04000C61 RID: 3169
		private const float DEFAULT_TURN_ANGLE = 45f;

		// Token: 0x04000C62 RID: 3170
		private string vehicleType = "Regular";

		// Token: 0x04000C63 RID: 3171
		private bool hasSetControlType;

		// Token: 0x04000C64 RID: 3172
		private bool hadSetControlType;

		// Token: 0x04000C65 RID: 3173
		private string controlType = string.Empty;

		// Token: 0x04000C66 RID: 3174
		private float brakeEffectiveness = 1f;

		// Token: 0x04000C67 RID: 3175
		private float storeForce = 17.6f;

		// Token: 0x04000C68 RID: 3176
		private float storeTurn = 45f;

		// Token: 0x04000C69 RID: 3177
		private Vector3 origLocalCM;

		// Token: 0x04000C6A RID: 3178
		private Vector3 origLocalVehicleCenter;

		// Token: 0x04000C6B RID: 3179
		private bool canChangeDriveState = true;

		// Token: 0x04000C6C RID: 3180
		private bool beingDriven;

		// Token: 0x04000C6D RID: 3181
		private bool turnOnSpeedometer;

		// Token: 0x04000C6E RID: 3182
		private bool speedometerOn;

		// Token: 0x04000C6F RID: 3183
		private BlockSteeringWheel.DriveStates driveState;

		// Token: 0x04000C70 RID: 3184
		public static VehicleDefinitions vehicleDefinitions;

		// Token: 0x04000C71 RID: 3185
		private static Dictionary<string, VehicleDefinition> vehicleDefsDict;

		// Token: 0x04000C72 RID: 3186
		private HashSet<Block> chunkBlocks = new HashSet<Block>();

		// Token: 0x04000C73 RID: 3187
		private List<BlockAbstractWheel> allWheels = new List<BlockAbstractWheel>();

		// Token: 0x04000C74 RID: 3188
		private List<Block> wheelBlocks = new List<Block>();

		// Token: 0x04000C75 RID: 3189
		private List<BlockTankTreadsWheel> allMainTankWheels = new List<BlockTankTreadsWheel>();

		// Token: 0x04000C76 RID: 3190
		private List<Block> jumpBlocks = new List<Block>();

		// Token: 0x04000C77 RID: 3191
		private List<BlockAbstractWheel> allControlledWheels = new List<BlockAbstractWheel>();

		// Token: 0x04000C78 RID: 3192
		private List<BlockTankTreadsWheel> allControlledTankWheels = new List<BlockTankTreadsWheel>();

		// Token: 0x04000C79 RID: 3193
		private List<BlockAbstractWheel> backWheels = new List<BlockAbstractWheel>();

		// Token: 0x04000C7A RID: 3194
		private List<BlockAbstractWheel> frontWheels = new List<BlockAbstractWheel>();

		// Token: 0x04000C7B RID: 3195
		private List<BlockAbstractWheel> turnWheels = new List<BlockAbstractWheel>();

		// Token: 0x04000C7C RID: 3196
		private List<BlockAbstractWheel> inverseTurnWheels = new List<BlockAbstractWheel>();

		// Token: 0x04000C7D RID: 3197
		private List<BlockAbstractWheel> driveWheels = new List<BlockAbstractWheel>();

		// Token: 0x04000C7E RID: 3198
		private List<BlockAbstractWheel> inverseDriveWheels = new List<BlockAbstractWheel>();

		// Token: 0x04000C7F RID: 3199
		private List<BlockAbstractWheel> frontDriveWheels = new List<BlockAbstractWheel>();

		// Token: 0x04000C80 RID: 3200
		private List<BlockAbstractWheel> backDriveWheels = new List<BlockAbstractWheel>();

		// Token: 0x04000C81 RID: 3201
		private List<BlockAbstractWheel> frontInverseDriveWheels = new List<BlockAbstractWheel>();

		// Token: 0x04000C82 RID: 3202
		private List<BlockAbstractWheel> backInverseDriveWheels = new List<BlockAbstractWheel>();

		// Token: 0x04000C83 RID: 3203
		private List<BlockAbstractWheel> leftTankTurn_Wheels = new List<BlockAbstractWheel>();

		// Token: 0x04000C84 RID: 3204
		private List<BlockAbstractWheel> rightTankTurn_Wheels = new List<BlockAbstractWheel>();

		// Token: 0x04000C85 RID: 3205
		private List<BlockAbstractWheel> inverseLeftTankTurn_Wheels = new List<BlockAbstractWheel>();

		// Token: 0x04000C86 RID: 3206
		private List<BlockAbstractWheel> inverseRightTankTurn_Wheels = new List<BlockAbstractWheel>();

		// Token: 0x04000C87 RID: 3207
		private List<BlockTankTreadsWheel> drive_TankWheels = new List<BlockTankTreadsWheel>();

		// Token: 0x04000C88 RID: 3208
		private List<BlockTankTreadsWheel> inverseDrive_TankWheels = new List<BlockTankTreadsWheel>();

		// Token: 0x04000C89 RID: 3209
		private List<BlockTankTreadsWheel> leftTurn_TankWheels = new List<BlockTankTreadsWheel>();

		// Token: 0x04000C8A RID: 3210
		private List<BlockTankTreadsWheel> rightTurn_TankWheels = new List<BlockTankTreadsWheel>();

		// Token: 0x04000C8B RID: 3211
		private List<BlockTankTreadsWheel> inverseLeftTurn_TankWheels = new List<BlockTankTreadsWheel>();

		// Token: 0x04000C8C RID: 3212
		private List<BlockTankTreadsWheel> inverseRightTurn_TankWheels = new List<BlockTankTreadsWheel>();

		// Token: 0x04000C8D RID: 3213
		private List<List<Tile>> steeringWheelTiles = new List<List<Tile>>();

		// Token: 0x04000C8E RID: 3214
		private Vector3 alignUp = Vector3.up;

		// Token: 0x04000C8F RID: 3215
		private float onGroundFraction;

		// Token: 0x04000C90 RID: 3216
		private float maxSpeedInc = 0.3f;

		// Token: 0x04000C91 RID: 3217
		private float averageTurnWheelRadius = 1f;

		// Token: 0x04000C92 RID: 3218
		private float flipping;

		// Token: 0x04000C93 RID: 3219
		private int treatAsVehicleStatus = -1;

		// Token: 0x04000C94 RID: 3220
		private Vector3 moveAlongDirection = Vector3.zero;

		// Token: 0x04000C95 RID: 3221
		private float moveAlongMaxAngle = 45f;

		// Token: 0x04000C96 RID: 3222
		private float moveAlongMaxForce;

		// Token: 0x04000C97 RID: 3223
		private VehicleTurnMode turnMode;

		// Token: 0x04000C98 RID: 3224
		private VehicleDriveMode driveMode;

		// Token: 0x04000C99 RID: 3225
		private float helpForceTurnAngle;

		// Token: 0x04000C9A RID: 3226
		private float turnAngle;

		// Token: 0x04000C9B RID: 3227
		private float visualAngleTarget;

		// Token: 0x04000C9C RID: 3228
		private float visualAngle;

		// Token: 0x04000C9D RID: 3229
		private Vector3 localSteeringAxle;

		// Token: 0x04000C9E RID: 3230
		private float jumpHeight;

		// Token: 0x04000C9F RID: 3231
		private int jumpCounter;

		// Token: 0x04000CA0 RID: 3232
		private const int MAX_JUMP_FRAMES = 5;

		// Token: 0x04000CA1 RID: 3233
		private const int JUMP_RELOAD_FRAMES = 40;

		// Token: 0x04000CA2 RID: 3234
		private List<Transform> meshesToTurn;

		// Token: 0x04000CA3 RID: 3235
		private Bounds wheelsBounds;

		// Token: 0x04000CA4 RID: 3236
		private Vector3 vehicleCenter;

		// Token: 0x04000CA5 RID: 3237
		private bool engineSoundOn;

		// Token: 0x04000CA6 RID: 3238
		private float engineSoundRPM;

		// Token: 0x04000CA7 RID: 3239
		private bool setEngineSoundRPM;

		// Token: 0x04000CA8 RID: 3240
		private float engineSoundWheelSpinSpeed;

		// Token: 0x04000CA9 RID: 3241
		private EngineSoundDefinition engineSoundDefinition;

		// Token: 0x04000CAA RID: 3242
		private UISpeedometer speedometer;

		// Token: 0x04000CAB RID: 3243
		public static float awdBalanceFront = 0.4f;

		// Token: 0x04000CAC RID: 3244
		public static float awdBalanceRear = 0.6f;

		// Token: 0x04000CAD RID: 3245
		private const float steeringWheelBackwardSpeedMod = 0.75f;

		// Token: 0x020000D7 RID: 215
		private enum DriveStates
		{
			// Token: 0x04000CD1 RID: 3281
			IDLE,
			// Token: 0x04000CD2 RID: 3282
			FORWARD,
			// Token: 0x04000CD3 RID: 3283
			REVERSE,
			// Token: 0x04000CD4 RID: 3284
			FORWARD_BRAKING,
			// Token: 0x04000CD5 RID: 3285
			REVERSE_BRAKING
		}
	}
}
