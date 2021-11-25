using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x02000072 RID: 114
	public class BlockAbstractWheel : Block, INoCollisionSound
	{
		// Token: 0x06000921 RID: 2337 RVA: 0x0003F7B8 File Offset: 0x0003DBB8
		public BlockAbstractWheel(List<List<Tile>> tiles, string axleName = "", string wheelDefinitionName = "") : base(tiles)
		{
			if (!string.IsNullOrEmpty(axleName))
			{
				Transform transform = this.goT.Find(axleName);
				if (transform)
				{
					this.axle = transform.gameObject;
				}
			}
			this.loopName = "Wheel Engine Loop Default";
			this.defaultWheelDefinition = wheelDefinitionName;
			this.SetWheelDefinitionData(this.defaultWheelDefinition);
		}

		// Token: 0x06000922 RID: 2338 RVA: 0x0003FA64 File Offset: 0x0003DE64
		private static GameObject CreateNewHelperObject(string goName, string helperName)
		{
			if (BlockAbstractWheel.helper_object_parent == null)
			{
				GameObject gameObject = new GameObject("Wheel Helper Objects");
				BlockAbstractWheel.helper_object_parent = gameObject.transform;
			}
			return new GameObject(goName + " " + helperName)
			{
				transform = 
				{
					parent = BlockAbstractWheel.helper_object_parent
				}
			};
		}

		// Token: 0x06000923 RID: 2339 RVA: 0x0003FABC File Offset: 0x0003DEBC
		public new static void Register()
		{
			BlockAbstractWheel.predicateWheelDrive = PredicateRegistry.Add<BlockAbstractWheel>("Wheel.Drive", (Block b) => new PredicateSensorDelegate(((BlockAbstractWheel)b).IsDrivingSensor), (Block b) => new PredicateActionDelegate(((BlockAbstractWheel)b).Drive), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Force"
			}, null);
			BlockAbstractWheel.predicateWheelTurn = PredicateRegistry.Add<BlockAbstractWheel>("Wheel.Turn", (Block b) => new PredicateSensorDelegate(((BlockAbstractWheel)b).IsTurning), (Block b) => new PredicateActionDelegate(((BlockAbstractWheel)b).Turn), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Angle"
			}, null);
			PredicateRegistry.Add<BlockAbstractWheel>("Wheel.SetAsSpareTire", null, (Block b) => new PredicateActionDelegate(((BlockAbstractWheel)b).SetAsSpareTire), null, null, null);
			PredicateRegistry.Add<BlockAbstractWheel>("Wheel.Brake", (Block b) => new PredicateSensorDelegate(((BlockAbstractWheel)b).IsBraking), (Block b) => new PredicateActionDelegate(((BlockAbstractWheel)b).Brake), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockAbstractWheel>("Wheel.BrakeEffectiveness", (Block b) => new PredicateSensorDelegate(((BlockAbstractWheel)b).IsBrakeEffectiveness), (Block b) => new PredicateActionDelegate(((BlockAbstractWheel)b).SetBrakeEffectiveness), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockAbstractWheel>("Wheel.DriveEffectiveness", (Block b) => new PredicateSensorDelegate(((BlockAbstractWheel)b).IsDriveEffectiveness), (Block b) => new PredicateActionDelegate(((BlockAbstractWheel)b).SetDriveEffectiveness), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockAbstractWheel>("Wheel.JumpEffectiveness", (Block b) => new PredicateSensorDelegate(((BlockAbstractWheel)b).IsJumpEffectiveness), (Block b) => new PredicateActionDelegate(((BlockAbstractWheel)b).SetJumpEffectiveness), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockAbstractWheel>("Wheel.SteerEffectiveness", (Block b) => new PredicateSensorDelegate(((BlockAbstractWheel)b).IsSteerEffectiveness), (Block b) => new PredicateActionDelegate(((BlockAbstractWheel)b).SetSteerEffectiveness), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockAbstractWheel>("Wheel.SuspensionDamper", (Block b) => new PredicateSensorDelegate(((BlockAbstractWheel)b).IsSuspensionDamper), (Block b) => new PredicateActionDelegate(((BlockAbstractWheel)b).SetSuspensionDamper), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockAbstractWheel>("Wheel.SuspensionHeight", (Block b) => new PredicateSensorDelegate(((BlockAbstractWheel)b).IsSuspensionHeight), (Block b) => new PredicateActionDelegate(((BlockAbstractWheel)b).SetSuspensionHeight), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockAbstractWheel>("Wheel.SuspensionLength", (Block b) => new PredicateSensorDelegate(((BlockAbstractWheel)b).IsSuspensionLength), (Block b) => new PredicateActionDelegate(((BlockAbstractWheel)b).SetSuspensionLength), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockAbstractWheel>("Wheel.SuspensionSpring", (Block b) => new PredicateSensorDelegate(((BlockAbstractWheel)b).IsSuspensionSpring), (Block b) => new PredicateActionDelegate(((BlockAbstractWheel)b).SetSuspensionSpring), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockAbstractWheel>("Wheel.TurnTowardsTag", null, (Block b) => new PredicateActionDelegate(((BlockAbstractWheel)b).TurnTowardsTag), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockAbstractWheel>("Wheel.DriveTowardsTag", null, (Block b) => new PredicateActionDelegate(((BlockAbstractWheel)b).DriveTowardsTag), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockAbstractWheel>("Wheel.DriveTowardsTagRaw", null, (Block b) => new PredicateActionDelegate(((BlockAbstractWheel)b).DriveTowardsTagRaw), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockAbstractWheel>("Wheel.IsWheelTowardsTag", (Block b) => new PredicateSensorDelegate(((BlockAbstractWheel)b).IsWheelTowardsTag), null, new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockAbstractWheel>("Wheel.IsDPadAlongWheel", (Block b) => new PredicateSensorDelegate(((BlockAbstractWheel)b).IsDPadAlongWheel), null, new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockAbstractWheel>("Wheel.TurnAlongDPad", null, (Block b) => new PredicateActionDelegate(((BlockAbstractWheel)b).TurnAlongDPad), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockAbstractWheel>("Wheel.DriveAlongDPad", null, (Block b) => new PredicateActionDelegate(((BlockAbstractWheel)b).DriveAlongDPad), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockAbstractWheel>("Wheel.DriveAlongDPadRaw", null, (Block b) => new PredicateActionDelegate(((BlockAbstractWheel)b).DriveAlongDPadRaw), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			Block.AddSimpleDefaultTiles(new GAF("Wheel.Drive", new object[]
			{
				12.5f
			}), new string[]
			{
				"Wheel",
				"RAR Moon Rover Wheel",
				"Bulky Wheel",
				"Spoked Wheel",
				"Golden Wheel"
			});
		}

		// Token: 0x06000924 RID: 2340 RVA: 0x000401A0 File Offset: 0x0003E5A0
		private static void LoadWheelDefinitions()
		{
			if (BlockAbstractWheel.wheelDefs != null)
			{
				return;
			}
			BlockAbstractWheel.wheelDefs = Resources.Load<WheelDefinitions>("WheelDefinitions");
			BlockAbstractWheel.wheelDefsDict = new Dictionary<string, WheelDefinition>();
			foreach (WheelDefinition wheelDefinition in BlockAbstractWheel.wheelDefs.definitions)
			{
				BlockAbstractWheel.wheelDefsDict[wheelDefinition.name] = wheelDefinition;
			}
		}

		// Token: 0x06000925 RID: 2341 RVA: 0x0004020C File Offset: 0x0003E60C
		public static WheelDefinition FindWheelDefinition(string name)
		{
			BlockAbstractWheel.LoadWheelDefinitions();
			WheelDefinition result;
			if (BlockAbstractWheel.wheelDefsDict.TryGetValue(name, out result))
			{
				return result;
			}
			return BlockAbstractWheel.wheelDefs.definitions[0];
		}

		// Token: 0x06000926 RID: 2342 RVA: 0x0004023E File Offset: 0x0003E63E
		public void SetWheelDefinitionData(WheelDefinition def)
		{
			this.wheelDefinition = def;
			BlockAbstractWheel.wheelFriction.staticFriction = this.wheelDefinition.staticFriction;
			BlockAbstractWheel.wheelFriction.dynamicFriction = this.wheelDefinition.dynamicFriction;
		}

		// Token: 0x06000927 RID: 2343 RVA: 0x00040271 File Offset: 0x0003E671
		public void SetWheelDefinitionData(string name)
		{
			BlockAbstractWheel.LoadWheelDefinitions();
			if (BlockAbstractWheel.wheelDefs.definitions.Length > 0)
			{
				this.wheelDefinition = BlockAbstractWheel.FindWheelDefinition(name);
				this.SetWheelDefinitionData(this.wheelDefinition);
			}
		}

		// Token: 0x06000928 RID: 2344 RVA: 0x000402A2 File Offset: 0x0003E6A2
		public static void ClearLoopSoundSources()
		{
			BlockAbstractWheel.loopSoundSources.Clear();
		}

		// Token: 0x06000929 RID: 2345 RVA: 0x000402B0 File Offset: 0x0003E6B0
		public override void Play()
		{
			base.Play();
			this.SetWheelDefinitionData(this.defaultWheelDefinition);
			this.isSpareTire = false;
			this.jointsAdjusted = true;
			this.externalControlBlock = null;
			this.treatAsVehicleStatus = -1;
			this.volScale = Mathf.Clamp(Util.MaxComponent(base.Scale()), 1f, 3f);
			this.pitchScale = Mathf.Clamp(1f / (this.volScale * 0.6f), 0.6f, 1f);
			this.scaleSpeed = 1f / this.GetRadius();
			this.speedTarget = 0f;
			this.lastSpeedTarget = 0f;
			this.speed = 0f;
			this.angle = 0f;
			this.onGround = 0.2f;
			this.modelMass = -1f;
			this.wheelMass = -1f;
			this.brakeEffectiveness = 1f;
			this.driveEffectiveness = 1f;
			this.jumpEffectiveness = 1f;
			this.steerEffectiveness = 1f;
			this.suspensionHeightOverride = -1f;
			this.suspensionLengthOverride = -1f;
			this.suspensionDamperOverrideActive = false;
			this.suspensionSpringOverrideActive = false;
			this.suspensionDamperOverride = 10f;
			this.suspensionSpringOverride = 50000f;
			this.externalSuspensionHeightOverride = -1f;
			this.externalSuspensionLengthOverride = -1f;
			this.externalSuspensionDamperOverrideActive = false;
			this.externalSuspensionSpringOverrideActive = false;
			this.externalSuspensionDamperOverride = 10f;
			this.externalSuspensionSpringOverride = 50000f;
			this.lastSuspensionHeight = -1234f;
			this.lastSuspensionLength = -1234f;
			this.lastSuspensionClicks = -1234f;
			this.lastSuspensionStiffness = -1234f;
			this.lastExternalSuspensionHeight = -1234f;
			this.lastExternalSuspensionLength = -1234f;
			this.lastExternalSuspensionClicks = -1234f;
			this.lastExternalSuspensionStiffness = -1234f;
			if (this.goT.GetComponent<Collider>().sharedMaterial == null)
			{
				this.goT.GetComponent<Collider>().sharedMaterial = BlockAbstractWheel.wheelFriction;
			}
			BlockAbstractWheel.ClearLoopSoundSources();
			this.loopSoundSourceInfo = new LoopSoundSourceInfo(this.goT.position, this);
			BlockAbstractWheel.loopSoundSources.Add(this.loopSoundSourceInfo);
		}

		// Token: 0x0600092A RID: 2346 RVA: 0x000404E8 File Offset: 0x0003E8E8
		private void SetupJoints()
		{
			if (this.chassis == null)
			{
				return;
			}
			Rigidbody component = this.chassis.GetComponent<Rigidbody>();
			component.mass = Mathf.Max(component.mass, 2f);
			this.suspensionDrive.positionSpring = this.GetSuspensionSpring();
			this.suspensionDrive.positionDamper = this.GetSuspensionDamper();
			SoftJointLimit linearLimit = new SoftJointLimit
			{
				limit = this.GetSuspensionLength()
			};
			foreach (ConfigurableJoint configurableJoint in this.turnJoints)
			{
				configurableJoint.xDrive = this.suspensionDrive;
				configurableJoint.linearLimit = linearLimit;
			}
		}

		// Token: 0x0600092B RID: 2347 RVA: 0x000405C0 File Offset: 0x0003E9C0
		public override void Play2()
		{
			Rigidbody rb = this.chunk.rb;
			Rigidbody x = (!(this.goT.parent != null)) ? null : this.goT.parent.GetComponent<Rigidbody>();
			if (rb == null || x == null)
			{
				BWLog.Info("Missing rigidbody when parenting wheel, parented to character joint?");
				return;
			}
			rb.maxAngularVelocity = float.PositiveInfinity;
			rb.angularDrag = 0.025f;
			List<Block> list = base.ConnectionsOfType(2, true);
			HashSet<Transform> hashSet = new HashSet<Transform>();
			foreach (Block block in list)
			{
				hashSet.Add(block.goT.parent);
			}
			this.ignoreColliders = null;
			if (hashSet.Count == 0)
			{
				if (this.axle != null)
				{
					this.axle.GetComponent<Renderer>().enabled = false;
				}
			}
			else
			{
				foreach (Transform transform in hashSet)
				{
					Rigidbody component = transform.GetComponent<Rigidbody>();
					if (component == null)
					{
						BWLog.Info("Missing rigidbody when parenting wheel, parented to character joint?");
					}
					else
					{
						this.CreateJoints(transform.gameObject);
					}
				}
			}
			float num = Bunch.GetModelMass(this);
			this.brakeMultiplier = num * this.wheelDefinition.brakeHelperMultiplier;
			this.turnMultiplier = num * this.wheelDefinition.turnHelperMultiplier;
			float num2 = num * 0.5f + num * num * 0.001f;
			this.speedMultiplier = num2 * this.wheelDefinition.speedHelperMultiplier;
			this.blocksInModelSet = null;
			this.initialRBToLocalRotation = this.goT.localRotation;
			this.initialRBToLocalRotationInverse = Quaternion.Inverse(this.initialRBToLocalRotation);
		}

		// Token: 0x0600092C RID: 2348 RVA: 0x000407D8 File Offset: 0x0003EBD8
		public override void Update()
		{
			base.Update();
			if (this.isTreasure || this.broken || this.vanished || this.isSpareTire || this.goT.parent == null)
			{
				return;
			}
			bool flag = this.externalControlBlock != null && !this.externalControlBlock.broken;
			flag &= (this.turnObjects.Count > 0);
			flag &= (Mathf.Abs(this.speedTarget) > 0f || (this.chunk != null && this.chunk.rb != null && this.chunk.rb.velocity.sqrMagnitude > 25f));
			if (flag)
			{
				Vector3 right = this.turnObjects[0].transform.right;
				Vector3 normalized = (this.goT.forward - Vector3.Dot(this.goT.forward, right) * right).normalized;
				Vector3 normalized2 = (this.goT.up - Vector3.Dot(this.goT.up, right) * right).normalized;
				this.goT.parent.rotation = Quaternion.LookRotation(normalized, normalized2) * this.initialRBToLocalRotationInverse;
			}
		}

		// Token: 0x0600092D RID: 2349 RVA: 0x00040960 File Offset: 0x0003ED60
		public float GetRadius()
		{
			Vector3 scale = base.GetScale();
			return 0.25f * (scale.y + scale.z);
		}

		// Token: 0x0600092E RID: 2350 RVA: 0x0004098B File Offset: 0x0003ED8B
		public void DestroyJoint(ConfigurableJoint joint)
		{
			this.turnJoints.Remove(joint);
			UnityEngine.Object.Destroy(joint);
		}

		// Token: 0x0600092F RID: 2351 RVA: 0x000409A0 File Offset: 0x0003EDA0
		private float GetSuspensionDamper()
		{
			if (this.chassis == null)
			{
				return 0f;
			}
			float suspensionDampen;
			if (this.suspensionDamperOverrideActive)
			{
				suspensionDampen = this.suspensionDamperOverride;
			}
			else if (this.externalSuspensionDamperOverrideActive)
			{
				suspensionDampen = this.externalSuspensionDamperOverride;
			}
			else
			{
				suspensionDampen = this.wheelDefinition.suspensionDampen;
			}
			return suspensionDampen * this.modelMass;
		}

		// Token: 0x06000930 RID: 2352 RVA: 0x00040A08 File Offset: 0x0003EE08
		private float GetSuspensionSpring()
		{
			if (this.chassis == null)
			{
				return 0f;
			}
			float suspensionSpring;
			if (this.suspensionSpringOverrideActive)
			{
				suspensionSpring = this.suspensionSpringOverride;
			}
			else if (this.externalSuspensionSpringOverrideActive)
			{
				suspensionSpring = this.externalSuspensionSpringOverride;
			}
			else
			{
				suspensionSpring = this.wheelDefinition.suspensionSpring;
			}
			return suspensionSpring * this.modelMass;
		}

		// Token: 0x06000931 RID: 2353 RVA: 0x00040A70 File Offset: 0x0003EE70
		private float GetSuspensionHeight()
		{
			float num = Mathf.Max(1f, base.GetScale().y);
			float suspensionHeight;
			if (this.suspensionHeightOverride >= 0f)
			{
				suspensionHeight = this.suspensionHeightOverride;
			}
			else if (this.externalSuspensionHeightOverride >= 0f)
			{
				suspensionHeight = this.externalSuspensionHeightOverride;
			}
			else
			{
				suspensionHeight = this.wheelDefinition.suspensionHeight;
			}
			return suspensionHeight * num;
		}

		// Token: 0x06000932 RID: 2354 RVA: 0x00040AE0 File Offset: 0x0003EEE0
		private float GetSuspensionLength()
		{
			float num = Mathf.Max(1f, base.GetScale().y);
			float suspensionLength;
			if (this.suspensionLengthOverride >= 0f)
			{
				suspensionLength = this.suspensionLengthOverride;
			}
			else if (this.externalSuspensionLengthOverride >= 0f)
			{
				suspensionLength = this.externalSuspensionLengthOverride;
			}
			else
			{
				suspensionLength = this.wheelDefinition.suspensionLength;
			}
			return suspensionLength * num;
		}

		// Token: 0x06000933 RID: 2355 RVA: 0x00040B50 File Offset: 0x0003EF50
		public ConfigurableJoint CreateJoints(GameObject chassis)
		{
			ConfigurableJoint configurableJoint = chassis.AddComponent<ConfigurableJoint>();
			configurableJoint.autoConfigureConnectedAnchor = false;
			configurableJoint.enablePreprocessing = true;
			configurableJoint.enableCollision = false;
			GameObject gameObject = BlockAbstractWheel.CreateNewHelperObject(this.go.name, "Turn Helper");
			Vector3 position = this.goT.parent.position;
			Vector3 anchor = position - chassis.transform.position;
			gameObject.transform.position = position;
			configurableJoint.anchor = anchor;
			configurableJoint.connectedAnchor = Vector3.zero;
			gameObject.transform.rotation = this.goT.rotation;
			Rigidbody component = this.goT.parent.GetComponent<Rigidbody>();
			Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
			BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
			boxCollider.size = base.GetScale() * 0.75f;
			boxCollider.sharedMaterial = this.goT.GetComponent<Collider>().sharedMaterial;
			float mass = component.mass / 2f;
			rigidbody.mass = mass;
			component.mass = mass;
			rigidbody.detectCollisions = false;
			configurableJoint.connectedBody = rigidbody;
			configurableJoint.axis = this.goT.up;
			configurableJoint.xMotion = ConfigurableJointMotion.Limited;
			configurableJoint.yMotion = ConfigurableJointMotion.Locked;
			configurableJoint.zMotion = ConfigurableJointMotion.Locked;
			configurableJoint.angularXMotion = ConfigurableJointMotion.Limited;
			configurableJoint.angularYMotion = ConfigurableJointMotion.Locked;
			configurableJoint.angularZMotion = ConfigurableJointMotion.Locked;
			configurableJoint.linearLimit = new SoftJointLimit
			{
				limit = this.GetSuspensionLength()
			};
			this.driveYZ.maximumForce = float.PositiveInfinity;
			this.driveYZ.positionSpring = 1E+07f;
			this.driveYZ.positionDamper = 10f;
			configurableJoint.angularXDrive = this.driveYZ;
			float y = -0.1f;
			configurableJoint.targetPosition = this.goT.TransformDirection(new Vector3(0f, y, 0f));
			this.suspensionDrive.positionSpring = 1000f;
			this.suspensionDrive.positionDamper = 10f;
			this.suspensionDrive.maximumForce = float.PositiveInfinity;
			configurableJoint.xDrive = this.suspensionDrive;
			ConfigurableJoint configurableJoint2 = gameObject.AddComponent<ConfigurableJoint>();
			configurableJoint2.enablePreprocessing = true;
			configurableJoint2.enableCollision = false;
			configurableJoint2.anchor = component.transform.position - gameObject.transform.position;
			configurableJoint2.axis = Vector3.zero;
			configurableJoint2.xMotion = ConfigurableJointMotion.Locked;
			configurableJoint2.yMotion = ConfigurableJointMotion.Locked;
			configurableJoint2.zMotion = ConfigurableJointMotion.Locked;
			configurableJoint2.angularXMotion = ConfigurableJointMotion.Free;
			configurableJoint2.angularYMotion = ConfigurableJointMotion.Locked;
			configurableJoint2.angularZMotion = ConfigurableJointMotion.Locked;
			configurableJoint2.connectedBody = component;
			configurableJoint2.projectionMode = JointProjectionMode.PositionAndRotation;
			configurableJoint2.projectionDistance = 0.005f;
			this.turnObjects.Add(gameObject);
			this.turnJoints.Add(configurableJoint);
			this.spinJoints.Add(configurableJoint2);
			this.chassis = chassis;
			Collider component2 = this.goT.GetComponent<Collider>();
			this.ignoreColliders = chassis.GetComponentsInChildren<Collider>();
			for (int i = 0; i < this.ignoreColliders.Length; i++)
			{
				if (this.ignoreColliders[i].enabled)
				{
					if (this.ignoreColliders[i].gameObject.activeInHierarchy)
					{
						Physics.IgnoreCollision(this.ignoreColliders[i], component2, true);
						Physics.IgnoreCollision(this.ignoreColliders[i], boxCollider, true);
					}
				}
			}
			return configurableJoint;
		}

		// Token: 0x06000934 RID: 2356 RVA: 0x00040EB0 File Offset: 0x0003F2B0
		internal void AssignExternalControl(BlockSteeringWheel controlBlock, List<BlockAbstractWheel> controlledWheels)
		{
			if (this.externalControlBlock != controlBlock)
			{
				this.externalControlBlock = controlBlock;
				Collider component = this.goT.GetComponent<Collider>();
				HashSet<Collider> hashSet = new HashSet<Collider>();
				for (int i = 0; i < controlledWheels.Count; i++)
				{
					BlockAbstractWheel blockAbstractWheel = controlledWheels[i];
					if (blockAbstractWheel != this)
					{
						hashSet.Add(blockAbstractWheel.goT.GetComponent<Collider>());
					}
				}
				if (hashSet.Count > 0)
				{
					foreach (Collider collider in hashSet)
					{
						Physics.IgnoreCollision(collider, component, true);
					}
					if (this.ignoreColliders != null)
					{
						HashSet<Collider> other = new HashSet<Collider>(this.ignoreColliders);
						hashSet.UnionWith(other);
					}
					this.ignoreColliders = new Collider[hashSet.Count];
					hashSet.CopyTo(this.ignoreColliders);
				}
			}
		}

		// Token: 0x06000935 RID: 2357 RVA: 0x00040FB4 File Offset: 0x0003F3B4
		private void DestroyJointsAndHelperObjects()
		{
			foreach (ConfigurableJoint obj in this.spinJoints)
			{
				UnityEngine.Object.Destroy(obj);
			}
			this.spinJoints.Clear();
			foreach (ConfigurableJoint obj2 in this.turnJoints)
			{
				UnityEngine.Object.Destroy(obj2);
			}
			this.turnJoints.Clear();
			for (int i = 0; i < this.turnObjects.Count; i++)
			{
				UnityEngine.Object.Destroy(this.turnObjects[i]);
			}
			this.turnObjects.Clear();
		}

		// Token: 0x06000936 RID: 2358 RVA: 0x000410AC File Offset: 0x0003F4AC
		public override void Stop(bool resetBlock = true)
		{
			this.ResetIgnoreCollision();
			this.DestroyJointsAndHelperObjects();
			if (this.chassis == null && this.axle != null && this.axle.GetComponent<Collider>() != null)
			{
				UnityEngine.Object.Destroy(this.axle.GetComponent<Collider>());
			}
			if (this.axle != null)
			{
				this.axle.GetComponent<Renderer>().enabled = true;
				this.axle.transform.localScale = Vector3.one;
			}
			this.engineLoopOn = false;
			this.PlayLoopSound(false, base.GetLoopClip(), 1f, null, 1f);
			base.Stop(resetBlock);
		}

		// Token: 0x06000937 RID: 2359 RVA: 0x0004116A File Offset: 0x0003F56A
		public override void Pause()
		{
			this.engineLoopOn = false;
			this.PlayLoopSound(false, base.GetLoopClip(), 1f, null, 1f);
		}

		// Token: 0x06000938 RID: 2360 RVA: 0x0004118C File Offset: 0x0003F58C
		public override void ResetFrame()
		{
			if (this.speedTarget != 0f)
			{
				this.lastSpeedTarget = this.speedTarget;
			}
			else
			{
				this.lastSpeedTarget = Mathf.Lerp(this.lastSpeedTarget, 0f, 0.01f);
			}
			this.speedTarget = 0f;
			this.angleTarget = 0f;
			this.turnTowardsMax = 0f;
			this.driveTowardsMax = 0f;
			this.braking = false;
			this.brakeForce = 0f;
			this.drivenToTurn = false;
		}

		// Token: 0x06000939 RID: 2361 RVA: 0x0004121A File Offset: 0x0003F61A
		private void UpdateBlocksInModelSetIfNecessary()
		{
			if (this.blocksInModelSet == null)
			{
				base.UpdateConnectedCache();
				this.blocksInModelSet = new HashSet<Block>(Block.connectedCache[this]);
			}
		}

		// Token: 0x0600093A RID: 2362 RVA: 0x00041244 File Offset: 0x0003F644
		public void GlueToChassis()
		{
			if (this.chassis == null)
			{
				return;
			}
			foreach (ConfigurableJoint obj in this.spinJoints)
			{
				UnityEngine.Object.Destroy(obj);
			}
			this.spinJoints.Clear();
			foreach (ConfigurableJoint obj2 in this.turnJoints)
			{
				UnityEngine.Object.Destroy(obj2);
			}
			this.turnJoints.Clear();
			this.chunk.Destroy(false);
			Blocksworld.chunks.Remove(this.chunk);
		}

		// Token: 0x0600093B RID: 2363 RVA: 0x00041330 File Offset: 0x0003F730
		private void DriveRelativePoint(Vector3 targetPos, float howMuch, bool towards = true)
		{
			if (this.chassis == null)
			{
				return;
			}
			Vector3 position = this.goT.position;
			Transform transform = this.chassis.transform;
			Vector3 vector = targetPos - position;
			if (!towards)
			{
				vector *= -1f;
			}
			vector = Util.ProjectOntoPlane(vector, transform.up);
			if (vector.sqrMagnitude > 0.0001f)
			{
				Vector3 normalized = vector.normalized;
				float f = Util.AngleBetween(normalized, transform.forward, transform.up);
				float num = Mathf.Abs(f);
				float num2 = num / 180f;
				float num3 = 1f - num2;
				num3 = Mathf.Clamp(num3 * num3, 0.25f, 1f);
				this.speedTarget += num3 * howMuch;
			}
		}

		// Token: 0x0600093C RID: 2364 RVA: 0x00041400 File Offset: 0x0003F800
		private void DriveOffsetRaw(Vector3 offset, float howMuch)
		{
			Vector3 rhs = Quaternion.Euler(0f, -90f, 0f) * this.goT.right;
			float num = Vector3.Dot(offset, rhs);
			this.speedTarget += howMuch * num;
		}

		// Token: 0x0600093D RID: 2365 RVA: 0x0004144C File Offset: 0x0003F84C
		private void TurnRelativePoint(Vector3 targetPos, float turnTowardsMax, bool towards = true)
		{
			if (this.broken || this.chassis == null)
			{
				return;
			}
			Vector3 position = this.goT.position;
			Transform transform = this.chassis.transform;
			Vector3 vector = targetPos - position;
			if (!towards)
			{
				vector *= -1f;
			}
			vector = Util.ProjectOntoPlane(vector, transform.up);
			Vector3 v = Quaternion.Euler(0f, -90f, 0f) * this.goT.right;
			if (vector.sqrMagnitude > 0.0001f)
			{
				Vector3 normalized = vector.normalized;
				float num = Util.AngleBetween(normalized, v, transform.up);
				this.angleTarget = Mathf.Clamp(this.angleTarget - num, -turnTowardsMax, turnTowardsMax);
			}
		}

		// Token: 0x0600093E RID: 2366 RVA: 0x0004151C File Offset: 0x0003F91C
		public TileResultCode IsBrakeEffectiveness(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float num = Util.GetFloatArg(args, 0, 1f) * 0.01f;
			return (num != this.brakeEffectiveness) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x0600093F RID: 2367 RVA: 0x00041550 File Offset: 0x0003F950
		public TileResultCode IsDriveEffectiveness(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float num = Util.GetFloatArg(args, 0, 1f) * 0.01f;
			return (num != this.driveEffectiveness) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000940 RID: 2368 RVA: 0x00041584 File Offset: 0x0003F984
		public TileResultCode IsJumpEffectiveness(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float num = Util.GetFloatArg(args, 0, 1f) * 0.01f;
			return (num != this.jumpEffectiveness) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000941 RID: 2369 RVA: 0x000415B8 File Offset: 0x0003F9B8
		public TileResultCode IsSteerEffectiveness(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float num = Util.GetFloatArg(args, 0, 1f) * 0.01f;
			return (num != this.steerEffectiveness) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000942 RID: 2370 RVA: 0x000415EB File Offset: 0x0003F9EB
		public TileResultCode SetBrakeEffectiveness(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.brakeEffectiveness = Util.GetFloatArg(args, 0, 1f) * eInfo.floatArg * 0.01f;
			return TileResultCode.True;
		}

		// Token: 0x06000943 RID: 2371 RVA: 0x0004160D File Offset: 0x0003FA0D
		public TileResultCode SetDriveEffectiveness(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.driveEffectiveness = Util.GetFloatArg(args, 0, 1f) * eInfo.floatArg * 0.01f;
			return TileResultCode.True;
		}

		// Token: 0x06000944 RID: 2372 RVA: 0x0004162F File Offset: 0x0003FA2F
		public TileResultCode SetJumpEffectiveness(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.jumpEffectiveness = Util.GetFloatArg(args, 0, 1f) * eInfo.floatArg * 0.01f;
			return TileResultCode.True;
		}

		// Token: 0x06000945 RID: 2373 RVA: 0x00041651 File Offset: 0x0003FA51
		public TileResultCode SetSteerEffectiveness(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.steerEffectiveness = Util.GetFloatArg(args, 0, 1f) * eInfo.floatArg * 0.01f;
			return TileResultCode.True;
		}

		// Token: 0x06000946 RID: 2374 RVA: 0x00041674 File Offset: 0x0003FA74
		public TileResultCode SetAsSpareTire(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (!this.isSpareTire)
			{
				this.isSpareTire = true;
				this.suspensionHeightOverride = 0f;
				this.suspensionLengthOverride = 0f;
				this.SetupJoints();
				for (int i = 0; i < this.turnJoints.Count; i++)
				{
					this.turnJoints[i].angularXMotion = ConfigurableJointMotion.Locked;
				}
				for (int j = 0; j < this.spinJoints.Count; j++)
				{
					this.spinJoints[j].angularXMotion = ConfigurableJointMotion.Locked;
				}
			}
			return TileResultCode.True;
		}

		// Token: 0x06000947 RID: 2375 RVA: 0x0004170C File Offset: 0x0003FB0C
		public TileResultCode SetSuspensionHeight(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.suspensionHeightOverride = Util.GetFloatArg(args, 0, 0.25f) * eInfo.floatArg;
			this.jointsAdjusted |= (this.lastSuspensionHeight != this.suspensionHeightOverride);
			this.lastSuspensionHeight = this.suspensionHeightOverride;
			return TileResultCode.True;
		}

		// Token: 0x06000948 RID: 2376 RVA: 0x00041760 File Offset: 0x0003FB60
		public TileResultCode IsSuspensionHeight(ScriptRowExecutionInfo eInfo, object[] args)
		{
			bool flag = this.lastSuspensionHeight == Util.GetFloatArg(args, 0, 0.25f);
			return (!flag) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000949 RID: 2377 RVA: 0x0004178F File Offset: 0x0003FB8F
		public void SetExternalSuspensionHeight(float height)
		{
			this.externalSuspensionHeightOverride = height;
			this.jointsAdjusted |= (this.lastExternalSuspensionHeight != height && this.suspensionHeightOverride < 0f);
			this.lastExternalSuspensionHeight = this.externalSuspensionHeightOverride;
		}

		// Token: 0x0600094A RID: 2378 RVA: 0x000417CD File Offset: 0x0003FBCD
		public TileResultCode SetSuspensionLength(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.suspensionLengthOverride = Util.GetFloatArg(args, 0, 0.5f);
			this.jointsAdjusted |= (this.lastSuspensionLength != this.suspensionLengthOverride);
			this.lastSuspensionLength = this.suspensionLengthOverride;
			return TileResultCode.True;
		}

		// Token: 0x0600094B RID: 2379 RVA: 0x0004180C File Offset: 0x0003FC0C
		public TileResultCode IsSuspensionLength(ScriptRowExecutionInfo eInfo, object[] args)
		{
			bool flag = this.lastSuspensionLength == Util.GetFloatArg(args, 0, 0.5f);
			return (!flag) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x0600094C RID: 2380 RVA: 0x0004183B File Offset: 0x0003FC3B
		public void SetExternalSuspensionLength(float length)
		{
			this.externalSuspensionLengthOverride = length;
			this.jointsAdjusted |= (this.lastExternalSuspensionLength != length && this.suspensionLengthOverride < 0f);
			this.lastExternalSuspensionLength = this.externalSuspensionLengthOverride;
		}

		// Token: 0x0600094D RID: 2381 RVA: 0x0004187C File Offset: 0x0003FC7C
		public TileResultCode SetSuspensionDamper(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.suspensionDamperOverrideActive = true;
			float floatArg = Util.GetFloatArg(args, 0, 2f);
			this.suspensionDamperOverride = floatArg * floatArg * 0.2f;
			this.jointsAdjusted |= (this.lastSuspensionClicks != floatArg);
			this.lastSuspensionClicks = floatArg;
			return TileResultCode.True;
		}

		// Token: 0x0600094E RID: 2382 RVA: 0x000418D0 File Offset: 0x0003FCD0
		public TileResultCode IsSuspensionDamper(ScriptRowExecutionInfo eInfo, object[] args)
		{
			bool flag = this.lastSuspensionClicks == Util.GetFloatArg(args, 0, 2f);
			return (!flag) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x0600094F RID: 2383 RVA: 0x000418FF File Offset: 0x0003FCFF
		public void SetExternalSuspensionDamper(float clicks)
		{
			this.externalSuspensionDamperOverrideActive = true;
			this.externalSuspensionDamperOverride = clicks * clicks * 0.2f;
			this.jointsAdjusted |= (this.lastExternalSuspensionClicks != clicks);
			this.lastExternalSuspensionClicks = clicks;
		}

		// Token: 0x06000950 RID: 2384 RVA: 0x00041938 File Offset: 0x0003FD38
		public TileResultCode SetSuspensionSpring(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.suspensionSpringOverrideActive = true;
			float floatArg = Util.GetFloatArg(args, 0, 5f);
			this.suspensionSpringOverride = floatArg * floatArg * 2f + 5f;
			this.jointsAdjusted |= (this.lastSuspensionStiffness != floatArg);
			this.lastSuspensionStiffness = floatArg;
			return TileResultCode.True;
		}

		// Token: 0x06000951 RID: 2385 RVA: 0x00041990 File Offset: 0x0003FD90
		public TileResultCode IsSuspensionSpring(ScriptRowExecutionInfo eInfo, object[] args)
		{
			bool flag = this.lastSuspensionStiffness == Util.GetFloatArg(args, 0, 5f);
			return (!flag) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000952 RID: 2386 RVA: 0x000419BF File Offset: 0x0003FDBF
		public void SetExternalSuspensionSpring(float stiffness)
		{
			this.externalSuspensionSpringOverrideActive = true;
			this.externalSuspensionSpringOverride = stiffness * stiffness * 2f + 5f;
			this.jointsAdjusted |= (this.lastExternalSuspensionStiffness != stiffness);
			this.lastExternalSuspensionStiffness = stiffness;
		}

		// Token: 0x06000953 RID: 2387 RVA: 0x00041A00 File Offset: 0x0003FE00
		public TileResultCode DriveTowardsTag(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string tagName = (args.Length <= 0) ? string.Empty : ((string)args[0]);
			float num = (args.Length <= 1) ? 20f : ((float)args[1]);
			this.driveTowardsMax += num * eInfo.floatArg;
			this.UpdateBlocksInModelSetIfNecessary();
			Block block;
			if (TagManager.TryGetClosestBlockWithTag(tagName, this.goT.position, out block, this.blocksInModelSet))
			{
				Transform goT = block.goT;
				Vector3 position = goT.position;
				this.DriveRelativePoint(position, Mathf.Abs(num), num > 0f);
				return TileResultCode.True;
			}
			return TileResultCode.True;
		}

		// Token: 0x06000954 RID: 2388 RVA: 0x00041AA8 File Offset: 0x0003FEA8
		public TileResultCode DriveTowardsTagRaw(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string tagName = (args.Length <= 0) ? string.Empty : ((string)args[0]);
			float num = (args.Length <= 1) ? 20f : ((float)args[1]);
			Vector3 position = this.goT.position;
			this.UpdateBlocksInModelSetIfNecessary();
			Block block;
			if (TagManager.TryGetClosestBlockWithTag(tagName, position, out block, this.blocksInModelSet))
			{
				Transform goT = block.goT;
				Vector3 position2 = goT.position;
				Vector3 vector = position2 - position;
				if (vector.sqrMagnitude > 0.01f)
				{
					this.DriveOffsetRaw(vector.normalized, num * eInfo.floatArg);
				}
				return TileResultCode.True;
			}
			return TileResultCode.True;
		}

		// Token: 0x06000955 RID: 2389 RVA: 0x00041B58 File Offset: 0x0003FF58
		public TileResultCode DriveAlongDPad(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string key = (args.Length <= 0) ? string.Empty : ((string)args[0]);
			float num = (args.Length <= 1) ? 20f : ((float)args[1]);
			Blocksworld.UI.Controls.EnableDPad(key, MoverDirectionMask.ALL);
			this.driveTowardsMax += num * eInfo.floatArg;
			Vector3 worldDPadOffset = Blocksworld.UI.Controls.GetWorldDPadOffset(key);
			Vector3 position = this.goT.position;
			Vector3 targetPos = position + worldDPadOffset * 1000f;
			this.DriveRelativePoint(targetPos, num, num > 0f);
			return TileResultCode.True;
		}

		// Token: 0x06000956 RID: 2390 RVA: 0x00041C08 File Offset: 0x00040008
		public TileResultCode DriveAlongDPadRaw(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string key = (args.Length <= 0) ? string.Empty : ((string)args[0]);
			float num = (args.Length <= 1) ? 20f : ((float)args[1]);
			Blocksworld.UI.Controls.EnableDPad(key, MoverDirectionMask.ALL);
			Vector3 worldDPadOffset = Blocksworld.UI.Controls.GetWorldDPadOffset(key);
			this.DriveOffsetRaw(worldDPadOffset, num * eInfo.floatArg);
			return TileResultCode.True;
		}

		// Token: 0x06000957 RID: 2391 RVA: 0x00041C80 File Offset: 0x00040080
		public void Brake(float f)
		{
			this.braking = true;
			this.brakeForce = f * f;
			this.engineLoopOn = false;
			this.idleEngineCounter = 0;
			this.engineIncreasingPitch = false;
		}

		// Token: 0x06000958 RID: 2392 RVA: 0x00041CA7 File Offset: 0x000400A7
		public TileResultCode Brake(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.externalControlBlock == null || this.isSpareTire)
			{
				this.Brake(eInfo.floatArg);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000959 RID: 2393 RVA: 0x00041CCC File Offset: 0x000400CC
		public TileResultCode IsBraking(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!this.braking) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x0600095A RID: 2394 RVA: 0x00041CE0 File Offset: 0x000400E0
		public void Drive(float f, bool driveTurning)
		{
			this.speedTarget += f;
			this.drivenToTurn = driveTurning;
			this.engineLoopOn = !this.broken;
			this.idleEngineCounter = 0;
			this.engineIncreasingPitch = true;
		}

		// Token: 0x0600095B RID: 2395 RVA: 0x00041D14 File Offset: 0x00040114
		public TileResultCode Drive(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.externalControlBlock == null || this.isSpareTire)
			{
				float f = (float)args[0] * eInfo.floatArg;
				this.Drive(f, false);
			}
			return TileResultCode.True;
		}

		// Token: 0x0600095C RID: 2396 RVA: 0x00041D50 File Offset: 0x00040150
		public void KeepSpinning()
		{
			this.SetMotorTorqueNoChassis(this.lastSpeedTarget);
		}

		// Token: 0x0600095D RID: 2397 RVA: 0x00041D60 File Offset: 0x00040160
		private bool IsDirectionAlongWheel(Vector3 direction, float sign, ScriptRowExecutionInfo eInfo, bool analog)
		{
			if (direction.sqrMagnitude > 0.001f)
			{
				Vector3 lhs = Quaternion.Euler(0f, -90f, 0f) * this.goT.right;
				float num = sign * Vector3.Dot(lhs, direction.normalized);
				if (num > 0f)
				{
					if (analog)
					{
						eInfo.floatArg = Mathf.Min(eInfo.floatArg, num);
					}
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600095E RID: 2398 RVA: 0x00041DDC File Offset: 0x000401DC
		public TileResultCode IsDPadAlongWheel(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string key = (args.Length <= 0) ? string.Empty : ((string)args[0]);
			float sign = (args.Length <= 1) ? 1f : ((float)args[1]);
			Blocksworld.UI.Controls.EnableDPad(key, MoverDirectionMask.ALL);
			Vector3 worldDPadOffset = Blocksworld.UI.Controls.GetWorldDPadOffset(key);
			if (this.IsDirectionAlongWheel(worldDPadOffset, sign, eInfo, false))
			{
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x0600095F RID: 2399 RVA: 0x00041E58 File Offset: 0x00040258
		public TileResultCode IsWheelTowardsTag(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string tagName = (args.Length <= 0) ? string.Empty : ((string)args[0]);
			float sign = (args.Length <= 1) ? 1f : ((float)args[1]);
			Vector3 position = this.goT.position;
			Block block;
			if (TagManager.TryGetClosestBlockWithTag(tagName, position, out block, null))
			{
				Vector3 direction = block.goT.position - position;
				if (this.IsDirectionAlongWheel(direction, sign, eInfo, false))
				{
					return TileResultCode.True;
				}
			}
			return TileResultCode.False;
		}

		// Token: 0x06000960 RID: 2400 RVA: 0x00041EE0 File Offset: 0x000402E0
		public TileResultCode TurnAlongDPad(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string key = (args.Length <= 0) ? string.Empty : ((string)args[0]);
			float num = (args.Length <= 1) ? 30f : ((float)args[1]);
			Blocksworld.UI.Controls.EnableDPad(key, MoverDirectionMask.ALL);
			Vector3 worldDPadOffset = Blocksworld.UI.Controls.GetWorldDPadOffset(key);
			this.turnTowardsMax += num * eInfo.floatArg;
			Vector3 targetPos = this.goT.position + worldDPadOffset * 1000f;
			this.TurnRelativePoint(targetPos, this.turnTowardsMax, this.turnTowardsMax > 0f);
			return TileResultCode.True;
		}

		// Token: 0x06000961 RID: 2401 RVA: 0x00041F94 File Offset: 0x00040394
		public TileResultCode TurnTowardsTag(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string tagName = (args.Length <= 0) ? string.Empty : ((string)args[0]);
			float num = (args.Length <= 1) ? 30f : ((float)args[1]);
			this.turnTowardsMax += num * eInfo.floatArg;
			this.UpdateBlocksInModelSetIfNecessary();
			Block block;
			if (TagManager.TryGetClosestBlockWithTag(tagName, this.goT.position, out block, this.blocksInModelSet))
			{
				Transform goT = block.goT;
				Vector3 position = goT.position;
				this.TurnRelativePoint(position, Mathf.Abs(this.turnTowardsMax), this.turnTowardsMax > 0f);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000962 RID: 2402 RVA: 0x00042042 File Offset: 0x00040442
		public void Turn(float angleOffset)
		{
			this.angleTarget += angleOffset;
		}

		// Token: 0x06000963 RID: 2403 RVA: 0x00042054 File Offset: 0x00040454
		public TileResultCode Turn(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.externalControlBlock == null || this.isSpareTire)
			{
				float num = (float)args[0] * Mathf.Min(1f, 2f * eInfo.floatArg);
				this.angleTarget += num;
			}
			return TileResultCode.True;
		}

		// Token: 0x06000964 RID: 2404 RVA: 0x000420A6 File Offset: 0x000404A6
		public TileResultCode IsDrivingSensor(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return ((float)args[0] <= 0f) ? this.IsBreaking() : this.IsDriving();
		}

		// Token: 0x06000965 RID: 2405 RVA: 0x000420CC File Offset: 0x000404CC
		public TileResultCode IsDriving()
		{
			Vector3 rhs = Quaternion.Euler(0f, -90f, 0f) * this.goT.right;
			return (this.goT.parent.GetComponent<Rigidbody>().velocity.sqrMagnitude <= 0.25f || Vector3.Dot(this.goT.parent.GetComponent<Rigidbody>().velocity, rhs) <= 0f) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000966 RID: 2406 RVA: 0x00042154 File Offset: 0x00040554
		public TileResultCode IsBreaking()
		{
			Vector3 rhs = Quaternion.Euler(0f, -90f, 0f) * this.goT.right;
			return (this.goT.parent.GetComponent<Rigidbody>().velocity.sqrMagnitude <= 0.25f || Vector3.Dot(this.goT.parent.GetComponent<Rigidbody>().velocity, rhs) >= 0f) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000967 RID: 2407 RVA: 0x000421D9 File Offset: 0x000405D9
		public TileResultCode IsTurning(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return ((float)args[0] >= 0f) ? this.IsTurningRight() : this.IsTurningLeft();
		}

		// Token: 0x06000968 RID: 2408 RVA: 0x000421FE File Offset: 0x000405FE
		public TileResultCode IsTurningLeft()
		{
			return (this.angleTarget >= 0f) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000969 RID: 2409 RVA: 0x00042217 File Offset: 0x00040617
		public TileResultCode IsTurningRight()
		{
			return (this.angleTarget <= 0f) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x0600096A RID: 2410 RVA: 0x00042230 File Offset: 0x00040630
		private void SetMotor(float targetSpeed)
		{
			float new_x = targetSpeed * this.scaleSpeed;
			float num = Mathf.Clamp(this.wheelMass / (this.scaleSpeed * this.scaleSpeed), 20f, 100000f);
			float num2 = num * this.wheelDefinition.wheelSpinMultiplier * this.modelMass;
			for (int i = 0; i < this.spinJoints.Count; i++)
			{
				ConfigurableJoint configurableJoint = this.spinJoints[i];
				if (configurableJoint != null && configurableJoint.connectedBody != null)
				{
					Rigidbody connectedBody = configurableJoint.connectedBody;
					if (connectedBody.IsSleeping())
					{
						connectedBody.WakeUp();
					}
					this._targetAngularVelocity.Set(new_x, 0f, 0f);
					configurableJoint.targetAngularVelocity = this._targetAngularVelocity;
					if (this.braking)
					{
						this.driveX.positionDamper = this.suspensionDrive.positionDamper * num2 * this.brakeEffectiveness;
					}
					else if (targetSpeed != 0f)
					{
						this.driveX.positionDamper = this.suspensionDrive.positionDamper * num2;
					}
					else
					{
						this.driveX.positionDamper = 0f;
					}
					this.driveX.positionSpring = 0f;
					this.driveX.maximumForce = num2;
					configurableJoint.angularXDrive = this.driveX;
				}
			}
		}

		// Token: 0x0600096B RID: 2411 RVA: 0x00042398 File Offset: 0x00040798
		private void SetMotorTorqueNoChassis(float targetSpeed)
		{
			if (this.chunk == null || this.chunk.rb == null)
			{
				return;
			}
			float d = targetSpeed * this.scaleSpeed;
			Vector3 vector = Vector3.right * d;
			Vector3 vector2 = this.initialRBToLocalRotationInverse * this.chunk.rb.angularVelocity;
			if (vector.x > 0f && vector2.x < vector.x)
			{
				float num = Mathf.Clamp(this.wheelMass / (this.scaleSpeed * this.scaleSpeed), 5f, 100000f);
				float d2 = num * this.wheelDefinition.wheelSpinMultiplier * this.wheelMass;
				this.chunk.rb.AddRelativeTorque(this.initialRBToLocalRotation * Vector3.right * d2);
			}
			else if (vector.x < 0f && vector2.x > vector.x)
			{
				float num2 = -1f * Mathf.Clamp(this.wheelMass / (this.scaleSpeed * this.scaleSpeed), 5f, 100000f);
				float d3 = num2 * this.wheelDefinition.wheelSpinMultiplier * this.wheelMass;
				this.chunk.rb.AddRelativeTorque(this.initialRBToLocalRotation * Vector3.right * d3);
			}
		}

		// Token: 0x0600096C RID: 2412 RVA: 0x00042510 File Offset: 0x00040910
		private void SetAngle(float angle)
		{
			for (int i = 0; i < this.turnJoints.Count; i++)
			{
				ConfigurableJoint configurableJoint = this.turnJoints[i];
				if (configurableJoint != null && configurableJoint.connectedBody != null)
				{
					SoftJointLimit softJointLimit = new SoftJointLimit
					{
						limit = angle
					};
					if (angle > 0f)
					{
						configurableJoint.highAngularXLimit = softJointLimit;
					}
					else if (angle == 0f)
					{
						configurableJoint.highAngularXLimit = softJointLimit;
						configurableJoint.lowAngularXLimit = softJointLimit;
					}
					else
					{
						configurableJoint.lowAngularXLimit = softJointLimit;
					}
					configurableJoint.targetRotation = Quaternion.Euler(angle, 0f, angle);
					Rigidbody connectedBody = configurableJoint.connectedBody;
					if (connectedBody.IsSleeping())
					{
						connectedBody.WakeUp();
					}
				}
			}
		}

		// Token: 0x0600096D RID: 2413 RVA: 0x000425E0 File Offset: 0x000409E0
		private void UpdateEngineSound()
		{
			if (!Sound.sfxEnabled || this.isTreasure || this.vanished || this.externalControlBlock != null)
			{
				this.PlayLoopSound(false, base.GetLoopClip(), 1f, null, 1f);
				return;
			}
			Transform goT = this.goT;
			if (goT.parent == null)
			{
				return;
			}
			Rigidbody component = goT.parent.GetComponent<Rigidbody>();
			if (component == null)
			{
				return;
			}
			int num = 4;
			bool flag = this.engineIntervalCounter % num == 0;
			this.engineIntervalCounter++;
			float num2 = Mathf.Abs(Vector3.Dot(component.angularVelocity, goT.right));
			float num3 = num2 / 15f;
			float max = 3f * this.pitchScale;
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
			Vector3 position = goT.position;
			this.loopSoundSourceInfo.Update(position, this.engineLoopPitch, this.engineLoopOn);
			float num9 = 1f;
			for (int i = 0; i < BlockAbstractWheel.loopSoundSources.Count; i++)
			{
				LoopSoundSourceInfo loopSoundSourceInfo = BlockAbstractWheel.loopSoundSources[i];
				if (loopSoundSourceInfo.block != this && loopSoundSourceInfo.playing)
				{
					float magnitude = (position - loopSoundSourceInfo.pos).magnitude;
					float num10 = Mathf.Abs(loopSoundSourceInfo.pitch - this.loopSoundSourceInfo.pitch);
					num9 = Mathf.Min(num9 + num10, Mathf.Clamp(magnitude / 7f - 0.75f, 0f, 1f));
				}
			}
			float num11 = (!this.engineLoopOn) ? -0.01f : 0.1f;
			num11 *= this.volScale;
			this.engineLoopVolume = Mathf.Clamp(this.engineLoopVolume + num11, 0f, 0.2f * this.volScale);
			if (this.engineLoopOn && num8 < 0f)
			{
				this.idleEngineCounter++;
				if (this.idleEngineCounter > 100)
				{
					this.engineLoopOn = false;
				}
			}
			float num12 = num9 * this.engineLoopVolume;
			if (flag)
			{
				if (Sound.BlockIsMuted(this))
				{
					num12 = 0f;
				}
				float num13 = 1f;
				if (num12 > 0.01f)
				{
					float num14 = 0.1f;
					num13 = num14 * 2f * (Mathf.PerlinNoise(Time.time, 0f) - 0.5f) + 1f;
				}
				this.PlayLoopSound(num12 > 0.01f, base.GetLoopClip(), num12, null, num13 * 0.7f * this.engineLoopPitch);
			}
			base.UpdateWithinWaterLPFilter(null);
			this.engineIncreasingPitch = false;
		}

		// Token: 0x0600096E RID: 2414 RVA: 0x00042914 File Offset: 0x00040D14
		private void ResetIgnoreCollision()
		{
			if (this.ignoreColliders != null)
			{
				Collider component = this.goT.GetComponent<Collider>();
				for (int i = 0; i < this.ignoreColliders.Length; i++)
				{
					if (this.ignoreColliders[i] != null && this.ignoreColliders[i].enabled && this.ignoreColliders[i].gameObject.activeInHierarchy)
					{
						Physics.IgnoreCollision(this.ignoreColliders[i], component, false);
					}
				}
				this.ignoreColliders = null;
			}
		}

		// Token: 0x0600096F RID: 2415 RVA: 0x000429A3 File Offset: 0x00040DA3
		private void DisconnectFromStuff()
		{
			this.ResetIgnoreCollision();
			this.chassis = null;
			this.externalControlBlock = null;
			this.DestroyJointsAndHelperObjects();
			this.engineLoopOn = false;
			this.PlayLoopSound(false, base.GetLoopClip(), 1f, null, 1f);
		}

		// Token: 0x06000970 RID: 2416 RVA: 0x000429DE File Offset: 0x00040DDE
		public override void Break(Vector3 chunkPos, Vector3 chunkVel, Vector3 chunkAngVel)
		{
			if (this.broken)
			{
				return;
			}
			this.DisconnectFromStuff();
			base.Break(chunkPos, chunkVel, chunkAngVel);
		}

		// Token: 0x06000971 RID: 2417 RVA: 0x000429FB File Offset: 0x00040DFB
		public override void RemovedPlayBlock(Block b)
		{
			base.RemovedPlayBlock(b);
			if (this.chassis == b.go || this.externalControlBlock == b)
			{
				this.DisconnectFromStuff();
			}
		}

		// Token: 0x06000972 RID: 2418 RVA: 0x00042A2C File Offset: 0x00040E2C
		public void FixedUpdateDriveAndTurn(float jumpYOffset)
		{
			this.UpdateEngineSound();
			if (this.isTreasure || this.broken || this.vanished || this.isSpareTire)
			{
				return;
			}
			if (this.modelMass < 0f)
			{
				this.modelMass = Bunch.GetModelMassPerType<BlockAbstractWheel>(this);
				this.wheelMass = this.goT.parent.GetComponent<Rigidbody>().mass;
			}
			if (this.jointsAdjusted)
			{
				this.jointsAdjusted = false;
				this.SetupJoints();
			}
			this.speedTarget *= ((!this.drivenToTurn) ? this.driveEffectiveness : this.steerEffectiveness);
			this.angleTarget *= this.steerEffectiveness;
			this.SetAngle(this.angle);
			if (this.chassis == null)
			{
				this.angleTarget = Mathf.Clamp(this.angleTarget, -20f, 20f);
				this.SetMotorTorqueNoChassis(this.speedTarget);
				return;
			}
			this.SetMotor(-this.speed);
			this.angle += (this.angleTarget - this.angle) / this.wheelDefinition.rampTurn;
			Vector3 forward;
			Vector3 a;
			Vector3 right;
			if (this.externalControlBlock != null)
			{
				forward = this.externalControlBlock.goT.forward;
				a = -this.externalControlBlock.goT.up;
				right = this.externalControlBlock.goT.right;
			}
			else
			{
				forward = this.turnObjects[0].transform.forward;
				a = -this.turnObjects[0].transform.up;
				right = this.turnObjects[0].transform.right;
			}
			if (this.goT.parent == null)
			{
				return;
			}
			Rigidbody component = this.goT.parent.GetComponent<Rigidbody>();
			float num;
			if (this.onGround <= 0f)
			{
				num = this.speed;
			}
			else
			{
				num = Vector3.Dot(forward, component.velocity);
			}
			if (this.braking)
			{
				if (this.brakeEffectiveness >= 1f && num < 25f)
				{
					this.speed = 0f;
				}
				else
				{
					this.speed = Mathf.Lerp(num, 0f, 0.1f * this.brakeEffectiveness);
				}
			}
			else
			{
				this.speed = Mathf.Lerp(num, this.speedTarget, 0.1f * ((!this.drivenToTurn) ? this.driveEffectiveness : this.steerEffectiveness));
			}
			if (this.onGround > 0f)
			{
				Vector3 position = this.goT.position;
				Vector3 vector = Vector3.zero;
				if (Mathf.Abs(this.angle) > 10f)
				{
					vector += this.modelMass * 0.00125f * right * num * Mathf.Sin(this.angle * 0.0174532924f) * this.turnMultiplier;
					vector += a * 0.35f * Mathf.Abs(num) * this.speedMultiplier;
				}
				if (this.braking)
				{
					vector -= forward * num * this.brakeMultiplier * Mathf.Min(1f, this.brakeForce * this.brakeEffectiveness);
				}
				else
				{
					vector += forward * this.speedTarget * this.speedMultiplier;
				}
				vector += a * 0.35f * Mathf.Abs(num) * this.speedMultiplier;
				component.AddForceAtPosition(vector, position);
			}
			bool flag = true;
			for (int i = 0; i < this.turnJoints.Count; i++)
			{
				ConfigurableJoint configurableJoint = this.turnJoints[i];
				if (configurableJoint != null)
				{
					float y = configurableJoint.connectedAnchor.y;
					float num2 = Mathf.Lerp(y, this.GetSuspensionHeight() + jumpYOffset * this.jumpEffectiveness, 0.85f);
					float num3 = num2 - y;
					if (num3 > 0.01f)
					{
						configurableJoint.connectedAnchor += Vector3.up * num3;
					}
					else if (num3 < -0.01f)
					{
						configurableJoint.connectedAnchor += Vector3.up * num3;
					}
					Rigidbody connectedBody = configurableJoint.connectedBody;
					Rigidbody component2 = configurableJoint.GetComponent<Rigidbody>();
					if (connectedBody == null || connectedBody.isKinematic || component2 == null || component2.isKinematic)
					{
						flag = false;
						break;
					}
				}
			}
			if (flag)
			{
				Vector3 vector2 = forward * 0.25f * Mathf.Clamp(this.speed, -60f, 60f);
				vector2 = Util.ProjectOntoPlane(vector2, Vector3.up);
				Blocksworld.blocksworldCamera.AddForceDirectionHint(this.chunk, vector2);
			}
		}

		// Token: 0x06000973 RID: 2419 RVA: 0x00042F98 File Offset: 0x00041398
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (this.goT.parent != null && CollisionManager.IsImpactingBlock(this))
			{
				this.onGround = Blocksworld.fixedDeltaTime * 10f;
			}
			else if (this.onGround > 0f)
			{
				this.onGround = Mathf.Max(0f, this.onGround - Blocksworld.fixedDeltaTime);
			}
			if (this.externalControlBlock == null)
			{
				this.FixedUpdateDriveAndTurn(0f);
			}
		}

		// Token: 0x06000974 RID: 2420 RVA: 0x00043024 File Offset: 0x00041424
		public GameObject GetRealChassis()
		{
			return this.chassis;
		}

		// Token: 0x06000975 RID: 2421 RVA: 0x0004302C File Offset: 0x0004142C
		public float GetHelperObjectMass()
		{
			float num = 0f;
			for (int i = 0; i < this.turnObjects.Count; i++)
			{
				num += this.turnObjects[i].GetComponent<Rigidbody>().mass;
			}
			return num;
		}

		// Token: 0x06000976 RID: 2422 RVA: 0x00043075 File Offset: 0x00041475
		public override bool TreatAsVehicleLikeBlock()
		{
			return base.TreatAsVehicleLikeBlockWithStatus(ref this.treatAsVehicleStatus);
		}

		// Token: 0x06000977 RID: 2423 RVA: 0x00043084 File Offset: 0x00041484
		public override void ChunksAndJointsModified(Dictionary<Joint, Joint> oldToNew, Dictionary<Chunk, Chunk> oldToNewChunks, Dictionary<Chunk, Chunk> newToOldChunks)
		{
			if (this.broken || this.isTreasure)
			{
				return;
			}
			for (int i = 0; i < this.turnJoints.Count; i++)
			{
				ConfigurableJoint configurableJoint = this.turnJoints[i];
				Joint joint;
				if (configurableJoint != null && oldToNew.TryGetValue(configurableJoint, out joint))
				{
					configurableJoint = (ConfigurableJoint)joint;
					this.turnJoints[i] = configurableJoint;
					this.chassis = configurableJoint.gameObject;
				}
			}
			for (int j = 0; j < this.spinJoints.Count; j++)
			{
				ConfigurableJoint configurableJoint2 = this.spinJoints[j];
				Joint joint2;
				if (configurableJoint2 != null && oldToNew.TryGetValue(configurableJoint2, out joint2))
				{
					configurableJoint2 = (ConfigurableJoint)joint2;
					this.spinJoints[j] = configurableJoint2;
				}
			}
		}

		// Token: 0x0400072D RID: 1837
		public static Predicate predicateWheelDrive;

		// Token: 0x0400072E RID: 1838
		public static Predicate predicateWheelTurn;

		// Token: 0x0400072F RID: 1839
		private GameObject chassis;

		// Token: 0x04000730 RID: 1840
		public List<GameObject> turnObjects = new List<GameObject>();

		// Token: 0x04000731 RID: 1841
		public List<ConfigurableJoint> turnJoints = new List<ConfigurableJoint>();

		// Token: 0x04000732 RID: 1842
		public List<ConfigurableJoint> spinJoints = new List<ConfigurableJoint>();

		// Token: 0x04000733 RID: 1843
		private Collider[] ignoreColliders;

		// Token: 0x04000734 RID: 1844
		private JointDrive driveX = default(JointDrive);

		// Token: 0x04000735 RID: 1845
		private JointDrive driveYZ = default(JointDrive);

		// Token: 0x04000736 RID: 1846
		private JointDrive suspensionDrive = default(JointDrive);

		// Token: 0x04000737 RID: 1847
		private Quaternion initialRBToLocalRotation = Quaternion.identity;

		// Token: 0x04000738 RID: 1848
		private Quaternion initialRBToLocalRotationInverse = Quaternion.identity;

		// Token: 0x04000739 RID: 1849
		internal BlockSteeringWheel externalControlBlock;

		// Token: 0x0400073A RID: 1850
		public bool isSpareTire;

		// Token: 0x0400073B RID: 1851
		private int treatAsVehicleStatus = -1;

		// Token: 0x0400073C RID: 1852
		private float speed;

		// Token: 0x0400073D RID: 1853
		private float angle;

		// Token: 0x0400073E RID: 1854
		private float speedTarget;

		// Token: 0x0400073F RID: 1855
		private float angleTarget;

		// Token: 0x04000740 RID: 1856
		private float lastSpeedTarget;

		// Token: 0x04000741 RID: 1857
		public GameObject axle;

		// Token: 0x04000742 RID: 1858
		private float scaleSpeed;

		// Token: 0x04000743 RID: 1859
		public float maxSpeedInc = 99999f;

		// Token: 0x04000744 RID: 1860
		private bool braking;

		// Token: 0x04000745 RID: 1861
		private float brakeForce;

		// Token: 0x04000746 RID: 1862
		private bool drivenToTurn;

		// Token: 0x04000747 RID: 1863
		private float modelMass = -1f;

		// Token: 0x04000748 RID: 1864
		private float wheelMass = -1f;

		// Token: 0x04000749 RID: 1865
		private float brakeMultiplier = 1f;

		// Token: 0x0400074A RID: 1866
		private float speedMultiplier = 1f;

		// Token: 0x0400074B RID: 1867
		private float turnMultiplier = 1f;

		// Token: 0x0400074C RID: 1868
		private float brakeEffectiveness = 1f;

		// Token: 0x0400074D RID: 1869
		private float driveEffectiveness = 1f;

		// Token: 0x0400074E RID: 1870
		private float jumpEffectiveness = 1f;

		// Token: 0x0400074F RID: 1871
		private float steerEffectiveness = 1f;

		// Token: 0x04000750 RID: 1872
		private static PhysicMaterial wheelFriction = new PhysicMaterial();

		// Token: 0x04000751 RID: 1873
		public float onGround = 0.2f;

		// Token: 0x04000752 RID: 1874
		private Vector3 pausedVelocityAxle;

		// Token: 0x04000753 RID: 1875
		private Vector3 pausedAngularVelocityAxle;

		// Token: 0x04000754 RID: 1876
		private bool engineLoopOn;

		// Token: 0x04000755 RID: 1877
		private float engineLoopPitch = 1f;

		// Token: 0x04000756 RID: 1878
		private float engineLoopVolume;

		// Token: 0x04000757 RID: 1879
		private bool engineIncreasingPitch;

		// Token: 0x04000758 RID: 1880
		private int idleEngineCounter;

		// Token: 0x04000759 RID: 1881
		private LoopSoundSourceInfo loopSoundSourceInfo;

		// Token: 0x0400075A RID: 1882
		private float turnTowardsMax;

		// Token: 0x0400075B RID: 1883
		private float driveTowardsMax;

		// Token: 0x0400075C RID: 1884
		private float volScale = 1f;

		// Token: 0x0400075D RID: 1885
		private float pitchScale = 1f;

		// Token: 0x0400075E RID: 1886
		internal bool jointsAdjusted;

		// Token: 0x0400075F RID: 1887
		internal float suspensionHeightOverride = -1f;

		// Token: 0x04000760 RID: 1888
		internal float suspensionLengthOverride = -1f;

		// Token: 0x04000761 RID: 1889
		internal bool suspensionDamperOverrideActive;

		// Token: 0x04000762 RID: 1890
		internal bool suspensionSpringOverrideActive;

		// Token: 0x04000763 RID: 1891
		internal float suspensionDamperOverride = 10f;

		// Token: 0x04000764 RID: 1892
		internal float suspensionSpringOverride = 50000f;

		// Token: 0x04000765 RID: 1893
		private float externalSuspensionHeightOverride = -1f;

		// Token: 0x04000766 RID: 1894
		private float externalSuspensionLengthOverride = -1f;

		// Token: 0x04000767 RID: 1895
		private bool externalSuspensionDamperOverrideActive;

		// Token: 0x04000768 RID: 1896
		private bool externalSuspensionSpringOverrideActive;

		// Token: 0x04000769 RID: 1897
		private float externalSuspensionDamperOverride = 10f;

		// Token: 0x0400076A RID: 1898
		private float externalSuspensionSpringOverride = 50000f;

		// Token: 0x0400076B RID: 1899
		private float lastSuspensionHeight = -1234f;

		// Token: 0x0400076C RID: 1900
		private float lastSuspensionLength = -1234f;

		// Token: 0x0400076D RID: 1901
		private float lastSuspensionClicks = -1234f;

		// Token: 0x0400076E RID: 1902
		private float lastSuspensionStiffness = -1234f;

		// Token: 0x0400076F RID: 1903
		private float lastExternalSuspensionHeight = -1234f;

		// Token: 0x04000770 RID: 1904
		private float lastExternalSuspensionLength = -1234f;

		// Token: 0x04000771 RID: 1905
		private float lastExternalSuspensionClicks = -1234f;

		// Token: 0x04000772 RID: 1906
		private float lastExternalSuspensionStiffness = -1234f;

		// Token: 0x04000773 RID: 1907
		private HashSet<Block> blocksInModelSet;

		// Token: 0x04000774 RID: 1908
		private static List<LoopSoundSourceInfo> loopSoundSources = new List<LoopSoundSourceInfo>();

		// Token: 0x04000775 RID: 1909
		public string defaultWheelDefinition = "Generic";

		// Token: 0x04000776 RID: 1910
		private static WheelDefinitions wheelDefs;

		// Token: 0x04000777 RID: 1911
		private static Dictionary<string, WheelDefinition> wheelDefsDict;

		// Token: 0x04000778 RID: 1912
		private WheelDefinition wheelDefinition = new WheelDefinition
		{
			suspensionSpring = 10f,
			suspensionDampen = 1.25f,
			suspensionLength = 0.5f,
			suspensionHeight = 0.25f,
			speedHelperMultiplier = 0.1f,
			wheelSpinMultiplier = 1.15f,
			turnHelperMultiplier = 0.45f,
			rampTurn = 22f,
			staticFriction = 0.5f,
			dynamicFriction = 0.6f
		};

		// Token: 0x04000779 RID: 1913
		private static Transform helper_object_parent = null;

		// Token: 0x0400077A RID: 1914
		private Vector3 _targetAngularVelocity = Vector3.zero;

		// Token: 0x0400077B RID: 1915
		private int engineIntervalCounter;
	}
}
