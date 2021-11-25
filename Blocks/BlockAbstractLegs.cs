using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x0200005C RID: 92
	public abstract class BlockAbstractLegs : Block
	{
		// Token: 0x06000755 RID: 1877 RVA: 0x00032838 File Offset: 0x00030C38
		public BlockAbstractLegs(List<List<Tile>> tiles, Dictionary<string, string> partNames = null, int legPairCount = 1, float[] legPairOffsets = null, int[][] legPairIndices = null, float ankleYSeparation = 0f, bool oneAnkleMeshPerFoot = false, float torqueMultiplier = 1f, float footWidth = 0.25f) : base(tiles)
		{
			if (partNames == null)
			{
				partNames = new Dictionary<string, string>();
			}
			this.partNames = partNames;
			this.legPairCount = legPairCount;
			this.ankleYSeparation = ankleYSeparation;
			this.oneAnkleMeshPerFoot = oneAnkleMeshPerFoot;
			this.torqueMult = torqueMultiplier;
			this.xWidth = footWidth;
			this.FindFeet();
			this.StoreDefaultFootScale();
			if (legPairOffsets == null)
			{
				legPairOffsets = new float[1];
			}
			this.legPairOffsets = legPairOffsets;
			if (legPairIndices == null)
			{
				legPairIndices = new int[][]
				{
					new int[]
					{
						0,
						1
					}
				};
			}
			this.legPairIndices = legPairIndices;
			Materials.GetMaterial("White", "Plain", Materials.shaders["Plain"]);
			this.InvisibleShader = Shader.Find("Blocksworld/Invisible");
			this.unmoving = false;
		}

		// Token: 0x06000756 RID: 1878 RVA: 0x00032A94 File Offset: 0x00030E94
		public List<string> GetPossibleSfxs(string semantic, bool warn = true)
		{
			List<string> result;
			if (this.semanticSfxs.TryGetValue(semantic, out result))
			{
				return result;
			}
			if (this.defaultSemanticSfxs.TryGetValue(semantic, out result))
			{
				return result;
			}
			if (warn)
			{
				BWLog.Warning("Could not find any sfxs for " + semantic);
			}
			return new List<string>();
		}

		// Token: 0x06000757 RID: 1879 RVA: 0x00032AE8 File Offset: 0x00030EE8
		public string SampleSfx(string semantic, bool warn = true)
		{
			List<string> possibleSfxs = this.GetPossibleSfxs(semantic, warn);
			List<string> list = new List<string>();
			foreach (string text in possibleSfxs)
			{
				if (!this.forbiddenCounters.ContainsKey(text))
				{
					list.Add(text);
				}
			}
			if (list.Count > 0)
			{
				return list[UnityEngine.Random.Range(0, list.Count)];
			}
			return string.Empty;
		}

		// Token: 0x06000758 RID: 1880 RVA: 0x00032B84 File Offset: 0x00030F84
		public static void InitPlay()
		{
			BlockAbstractLegs.numLegsOnChunk.Clear();
		}

		// Token: 0x06000759 RID: 1881 RVA: 0x00032B90 File Offset: 0x00030F90
		public void SetSfxs(string texture, int meshIndex)
		{
			if (meshIndex == 0)
			{
				Dictionary<string, List<string>> dictionary;
				if (this.textureSemanticSfxs.TryGetValue(texture, out dictionary))
				{
					this.semanticSfxs = dictionary;
				}
				else
				{
					this.semanticSfxs = this.defaultSemanticSfxs;
				}
				if (this.connectedBlockTypeSemanticSfxs.Count > 0)
				{
					this.semanticSfxs = this.connectedBlockTypeSemanticSfxs;
				}
				this.hasWheeSound = (this.GetPossibleSfxs("Whee", false).Count > 0);
				this.hasOuchSound = (this.GetPossibleSfxs("Ouch", false).Count > 0);
				this.hasOhSound = (this.GetPossibleSfxs("Oh", false).Count > 0);
			}
		}

		// Token: 0x0600075A RID: 1882 RVA: 0x00032C3C File Offset: 0x0003103C
		public override void OnCreate()
		{
			base.OnCreate();
			if (this.resetFeetPositionsOnCreate)
			{
				this.ResetFeetPositions();
			}
			else
			{
				for (int i = 0; i < this.legPairCount; i++)
				{
					this.PositionAnkle(i * 2);
					this.PositionAnkle(1 + i * 2);
				}
			}
		}

		// Token: 0x0600075B RID: 1883 RVA: 0x00032C90 File Offset: 0x00031090
		public virtual void FindFeet()
		{
			this.feet = new FootInfo[this.legPairCount * 2];
			for (int i = 0; i < this.legPairCount; i++)
			{
				string str = (i != 0) ? "Back" : "Front";
				string text = (this.legPairCount != 1) ? ("Foot Right " + str) : "Foot Right";
				string text2 = (this.legPairCount != 1) ? ("Foot Left " + str) : "Foot Left";
				string text3 = (!this.partNames.ContainsKey(text)) ? text : this.partNames[text];
				string text4 = (!this.partNames.ContainsKey(text2)) ? text2 : this.partNames[text2];
				Transform transform = this.goT.Find(text3);
				Transform transform2 = this.goT.Find(text4);
				string text5 = "Bone ";
				Transform transform3 = this.goT.Find(text5 + text3);
				Transform transform4 = this.goT.Find(text5 + text4);
				if (transform != null)
				{
					this.feet[i * 2] = new FootInfo
					{
						go = transform.gameObject
					};
					if (transform3 != null)
					{
						this.feet[i * 2].bone = transform3.transform;
						this.feet[i * 2].boneYOffset = transform3.transform.localPosition.y + this.ankleYSeparation;
					}
					else
					{
						BWLog.Info("Could not find bone with name '" + text5 + text3 + "'");
					}
				}
				else
				{
					BWLog.Info("Could not find foot with name '" + text3 + "'");
				}
				if (transform2 != null)
				{
					this.feet[1 + i * 2] = new FootInfo
					{
						go = transform2.gameObject
					};
					if (transform4 != null)
					{
						this.feet[1 + i * 2].bone = transform4.transform;
						this.feet[1 + i * 2].boneYOffset = transform4.transform.localPosition.y + this.ankleYSeparation;
					}
					else
					{
						BWLog.Info("Could not find bone with name '" + text5 + text4 + "'");
					}
				}
				else
				{
					BWLog.Info("Could not find foot with name '" + text4 + "'");
				}
			}
		}

		// Token: 0x0600075C RID: 1884 RVA: 0x00032F2C File Offset: 0x0003132C
		protected virtual void PauseAnkles()
		{
			for (int i = 0; i < this.legPairCount; i++)
			{
				for (int j = 0; j < 2; j++)
				{
					this.PositionAnkle(j + i * 2);
				}
			}
		}

		// Token: 0x0600075D RID: 1885 RVA: 0x00032F70 File Offset: 0x00031370
		public override void Pause()
		{
			if (this.voxAudioSource != null)
			{
				this.voxAudioSource.Stop();
			}
			for (int i = 0; i < this.feet.Length; i++)
			{
				this.feet[i].go.name = this.feet[i].oldName;
			}
			if (!this.broken && !this.unmoving)
			{
				this.OrientFeetWithGround(!this.controllerWasActive || this.walkController.IsOnGround());
				this.PauseAnkles();
			}
			for (int j = 0; j < 2 * this.legPairCount; j++)
			{
				FootInfo footInfo = this.feet[j];
				Rigidbody rb = footInfo.rb;
				if (rb != null)
				{
					footInfo.pausedVelocity = rb.velocity;
					footInfo.pausedAngularVelocity = rb.angularVelocity;
					rb.isKinematic = true;
				}
			}
		}

		// Token: 0x0600075E RID: 1886 RVA: 0x00033060 File Offset: 0x00031460
		public override void Resume()
		{
			for (int i = 0; i < 2 * this.legPairCount; i++)
			{
				FootInfo footInfo = this.feet[i];
				Rigidbody rb = footInfo.rb;
				if (rb != null)
				{
					rb.isKinematic = false;
					rb.velocity = footInfo.pausedVelocity;
					rb.angularVelocity = footInfo.pausedAngularVelocity;
				}
			}
		}

		// Token: 0x0600075F RID: 1887 RVA: 0x000330C4 File Offset: 0x000314C4
		public override void Break(Vector3 chunkPos, Vector3 chunkVel, Vector3 chunkAngVel)
		{
			base.Break(chunkPos, chunkVel, chunkAngVel);
			for (int i = 0; i < 2 * this.legPairCount; i++)
			{
				FootInfo footInfo = this.feet[i];
				UnityEngine.Object.Destroy(footInfo.joint);
				Rigidbody rb = footInfo.rb;
				if (rb != null)
				{
					Block.AddExplosiveForce(rb, footInfo.go.transform.position, chunkPos, chunkVel, chunkAngVel, 1f);
				}
			}
			this.go.GetComponent<Collider>().enabled = true;
		}

		// Token: 0x06000760 RID: 1888 RVA: 0x0003314C File Offset: 0x0003154C
		private void CreateVocalAudioSource()
		{
			if (this.voxAudioSource == null)
			{
				this.voxGameObject = new GameObject(this.go.name + " Vox Object");
				this.voxGameObject.transform.parent = this.goT;
				this.voxAudioSource = this.voxGameObject.AddComponent<AudioSource>();
				this.voxAudioSource.playOnAwake = false;
				this.voxAudioSource.loop = false;
				this.voxAudioSource.dopplerLevel = 0.5f;
				this.voxAudioSource.enabled = true;
				this.voxAudioSource.spatialBlend = 0.5f;
				this.voxGameObject.transform.localPosition = Vector3.zero;
				Sound.SetWorldAudioSourceParams(this.voxAudioSource, 5f, 150f, AudioRolloffMode.Logarithmic);
			}
		}

		// Token: 0x06000761 RID: 1889 RVA: 0x00033220 File Offset: 0x00031620
		public override void Play()
		{
			base.Play();
			this.treatAsVehicleStatus = -1;
			this.lpFilterUpdateCounter = UnityEngine.Random.Range(1, 5);
			this.unmoving = false;
			this.legParameters = this.GetLegParameters();
			if (this.legParameters != null)
			{
				this.maxSurfaceWalkAngle = this.legParameters.maxSurfaceWalkAngle;
			}
			this.maxSurfaceWalkAngleDot = Mathf.Cos(0.0174532924f * this.maxSurfaceWalkAngle);
			this.CreateVocalAudioSource();
			this.ignoreRaycastGOs = null;
			this.forbiddenCounters.Clear();
			this.body = this.goT.parent.gameObject;
			this.keepCollider |= (this.body.transform.childCount <= 1);
			if (!this.keepCollider)
			{
				this.go.GetComponent<Collider>().enabled = false;
			}
			this.hasMovedCM = false;
			this.hasChangedInertia = false;
			this.onGround = true;
			this.upright = true;
			this.StoreDefaultFootScale();
			this.UnVanishFeet();
			for (int i = 0; i < 2 * this.legPairCount; i++)
			{
				FootInfo footInfo = this.feet[i];
				if (footInfo == null)
				{
					BWLog.Info("foot was null");
				}
				footInfo.position = footInfo.go.transform.position;
				footInfo.oldName = footInfo.go.name;
				footInfo.go.name = this.go.name;
				footInfo.normal = Vector3.up;
			}
			this.stepTimer = 0f;
			this.vpe = Vector3.zero;
			this.vi = Vector3.zero;
			this.vd = Vector3.zero;
			this.wasBroken = false;
			this.startJumpCountdown = (int)Mathf.Round(1f / Blocksworld.fixedDeltaTime);
			this.walkController = new WalkController(this);
			this.controllerWasActive = false;
			this.modelMass = Bunch.GetModelMass(this);
			this.ignoreRotation = (this.ChunkContainsJoint() && this.modelMass > 10f);
			this.GetCharacterSfxs();
			this.SetSfxs(base.GetTexture(0), 0);
			if (this.legParameters != null)
			{
				this.walkController.SetDefaultWackiness(this.legParameters.wackiness);
				this.ohPlayProbability = this.legParameters.ohPlayProbability;
				this.wheePlayProbability = this.legParameters.wheePlayProbability;
				this.ouchPlayProbability = this.legParameters.ouchPlayProbability;
				this.walkController.DPadControl("L", 2f, 0.25f);
			}
			this.walkController.SetChunk();
			this.playCallouts = true;
			if (this.unmoving)
			{
				this.go.GetComponent<Collider>().enabled = true;
			}
			if (this.torqueMult != 1f)
			{
				this.walkController.SetAddedTorqueMultiplier = this.torqueMult;
			}
		}

		// Token: 0x06000762 RID: 1890 RVA: 0x00033503 File Offset: 0x00031903
		protected void StoreDefaultFootScale()
		{
			this.oldLocalScale = this.feet[0].go.transform.localScale;
		}

		// Token: 0x06000763 RID: 1891 RVA: 0x00033524 File Offset: 0x00031924
		public LegParameters GetLegParameters()
		{
			LegParameters component = this.go.GetComponent<LegParameters>();
			if (component == null)
			{
				BWLog.Info("No parameters found for legs");
			}
			return component;
		}

		// Token: 0x06000764 RID: 1892 RVA: 0x00033554 File Offset: 0x00031954
		public void GetSemanticSfxRule(ObjectSfxItem[] items, Dictionary<string, List<string>> dict)
		{
			foreach (ObjectSfxItem objectSfxItem in items)
			{
				string semanticName = objectSfxItem.semanticName;
				string sfxName = objectSfxItem.sfxName;
				List<string> list;
				if (!dict.TryGetValue(semanticName, out list))
				{
					list = new List<string>();
					dict[semanticName] = list;
				}
				this.canInterrupts[sfxName] = objectSfxItem.canInterrupt;
				list.Add(sfxName);
			}
		}

		// Token: 0x06000765 RID: 1893 RVA: 0x000335C0 File Offset: 0x000319C0
		public void GetCharacterSfxs()
		{
			ObjectSfxRules component = this.go.GetComponent<ObjectSfxRules>();
			if (component == null)
			{
				component = Blocksworld.blocksworldDataContainer.GetComponent<ObjectSfxRules>();
				if (component == null)
				{
					BWLog.Info("Could not find any object sfx rules " + base.BlockType());
				}
			}
			if (component != null)
			{
				this.connectedBlockTypeSemanticSfxs.Clear();
				for (int i = 0; i < component.textureRules.Length; i++)
				{
					ObjectTextureSfxRule objectTextureSfxRule = component.textureRules[i];
					string texture = objectTextureSfxRule.texture;
					string key = texture;
					Dictionary<string, List<string>> dictionary;
					if (!this.textureSemanticSfxs.ContainsKey(key))
					{
						dictionary = new Dictionary<string, List<string>>();
						this.textureSemanticSfxs[key] = dictionary;
					}
					else
					{
						dictionary = this.textureSemanticSfxs[key];
					}
					this.GetSemanticSfxRule(objectTextureSfxRule.items, dictionary);
				}
				HashSet<string> hashSet = new HashSet<string>();
				foreach (Block block in this.connections)
				{
					hashSet.Add(block.BlockType());
				}
				foreach (ObjectBlockTypeSfxRule objectBlockTypeSfxRule in component.connectedBlockTypeRules)
				{
					string blockType = objectBlockTypeSfxRule.blockType;
					if (hashSet.Contains(blockType))
					{
						this.GetSemanticSfxRule(objectBlockTypeSfxRule.items, this.connectedBlockTypeSemanticSfxs);
					}
				}
			}
			this.GetSemanticSfxRule(component.defaultTextureRule.items, this.defaultSemanticSfxs);
		}

		// Token: 0x06000766 RID: 1894 RVA: 0x00033768 File Offset: 0x00031B68
		public virtual void PlayLegs1()
		{
			if (this.unmoving)
			{
				return;
			}
			for (int i = 0; i < 2 * this.legPairCount; i++)
			{
				Util.UnparentTransformSafely(this.feet[i].go.transform);
			}
			this.legIndex = ((!BlockAbstractLegs.numLegsOnChunk.ContainsKey(this.body.name)) ? 1 : (BlockAbstractLegs.numLegsOnChunk[this.body.name] + 1));
			BlockAbstractLegs.numLegsOnChunk[this.body.name] = this.legIndex;
			this.currentLeg = this.legIndex % 2;
		}

		// Token: 0x06000767 RID: 1895 RVA: 0x00033818 File Offset: 0x00031C18
		public virtual void PlayLegs2()
		{
			if (this.unmoving)
			{
				return;
			}
			for (int i = 0; i < 2 * this.legPairCount; i++)
			{
				FootInfo footInfo = this.feet[i];
				Rigidbody rigidbody = footInfo.rb;
				if (rigidbody == null)
				{
					rigidbody = footInfo.go.AddComponent<Rigidbody>();
					footInfo.rb = rigidbody;
					rigidbody.mass = this.footWeight;
					if (Blocksworld.interpolateRigidBodies)
					{
						rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
					}
					footInfo.collider = footInfo.go.GetComponent<Collider>();
					footInfo.collider.enabled = true;
					footInfo.joint = this.CreateJoint(this.body, footInfo.go);
					footInfo.go.AddComponent<ForwardEvents>();
				}
				rigidbody.isKinematic = false;
			}
			this.body.GetComponent<Rigidbody>().mass = Mathf.Max(1f, this.body.GetComponent<Rigidbody>().mass - 1f);
			this.multiLegged = (BlockAbstractLegs.numLegsOnChunk[this.body.name] > 1);
		}

		// Token: 0x06000768 RID: 1896 RVA: 0x0003392C File Offset: 0x00031D2C
		protected virtual void ResetFeetPositions()
		{
			for (int i = 0; i < this.legPairCount; i++)
			{
				this.feet[i * 2].go.transform.position = this.goT.position + this.goT.rotation * new Vector3(this.xWidth, this.footOffsetY - this.ankleOffset, this.legPairOffsets[i]);
				this.feet[i * 2].go.transform.rotation = this.goT.rotation;
				this.feet[i * 2 + 1].go.transform.position = this.goT.position + this.goT.rotation * new Vector3(-this.xWidth, this.footOffsetY - this.ankleOffset, this.legPairOffsets[i]);
				this.feet[i * 2 + 1].go.transform.rotation = this.goT.rotation;
			}
			for (int j = 0; j < this.feet.Length; j++)
			{
				this.feet[j].position = this.feet[j].go.transform.position;
			}
			for (int k = 0; k < this.legPairCount; k++)
			{
				this.PositionAnkle(k * 2);
				this.PositionAnkle(1 + k * 2);
			}
		}

		// Token: 0x06000769 RID: 1897 RVA: 0x00033AB8 File Offset: 0x00031EB8
		private void MakeUnmoving()
		{
			for (int i = 0; i < 2 * this.legPairCount; i++)
			{
				if (this.feet != null)
				{
					FootInfo footInfo = this.feet[i];
					if (!(footInfo.go == null) && !(footInfo.collider == null))
					{
						footInfo.collider.enabled = false;
						if (footInfo.joint != null)
						{
							UnityEngine.Object.Destroy(footInfo.joint);
						}
						UnityEngine.Object.Destroy(footInfo.rb);
						UnityEngine.Object.Destroy(footInfo.go.GetComponent<ForwardEvents>());
						footInfo.go.transform.parent = this.goT;
						footInfo.go.name = footInfo.oldName;
					}
				}
			}
			this.ResetFeetPositions();
			this.RestoreCollider();
			this.unmoving = true;
			if (this.chunk != null)
			{
				this.chunk.UpdateCenterOfMass(true);
			}
		}

		// Token: 0x0600076A RID: 1898 RVA: 0x00033BB2 File Offset: 0x00031FB2
		private void MakeMoving()
		{
			if (!this.unmoving)
			{
				return;
			}
			this.unmoving = false;
			this.PlayLegs1();
			this.PlayLegs2();
			this.ResetFeetPositions();
			if (this.chunk != null)
			{
				this.chunk.UpdateCenterOfMass(true);
			}
		}

		// Token: 0x0600076B RID: 1899 RVA: 0x00033BF0 File Offset: 0x00031FF0
		public override void Stop(bool resetBlock = true)
		{
			base.Stop(resetBlock);
			if (this.voxAudioSource != null)
			{
				this.voxAudioSource.Stop();
			}
			this.UnVanishFeet();
			this.ignoreRaycastGOs = null;
			this.MakeUnmoving();
			this.keepCollider = false;
			this.body = null;
			this.wasBroken = false;
			this.controllerWasActive = false;
			this.groundGO = null;
		}

		// Token: 0x0600076C RID: 1900 RVA: 0x00033C58 File Offset: 0x00032058
		public SpringJoint CreateJoint(GameObject parent, GameObject child)
		{
			SpringJoint springJoint = child.AddComponent<SpringJoint>();
			springJoint.enablePreprocessing = false;
			springJoint.connectedBody = parent.GetComponent<Rigidbody>();
			springJoint.anchor = Vector3.zero;
			springJoint.axis = this.body.transform.up;
			this.SetSpring(springJoint);
			return springJoint;
		}

		// Token: 0x0600076D RID: 1901 RVA: 0x00033CA8 File Offset: 0x000320A8
		private void SetSpring(SpringJoint joint)
		{
			this.SetSpring(joint, 10f + 4f * this.modelMass, 0.5f);
		}

		// Token: 0x0600076E RID: 1902 RVA: 0x00033CC8 File Offset: 0x000320C8
		public void SetSpring(SpringJoint joint, float spring, float damper)
		{
			float wackiness = this.walkController.GetWackiness();
			float spring2 = Mathf.Max(0.01f, 5f * wackiness * spring);
			float damper2 = Mathf.Max(0.1f, 5f * wackiness * damper);
			joint.spring = spring2;
			joint.damper = damper2;
		}

		// Token: 0x0600076F RID: 1903 RVA: 0x00033D18 File Offset: 0x00032118
		private void InspectSoftLimitSpring(string name, SoftJointLimitSpring ls)
		{
			BWLog.Info(string.Concat(new object[]
			{
				name,
				": ",
				ls.damper,
				" ",
				ls.spring
			}));
		}

		// Token: 0x06000770 RID: 1904 RVA: 0x00033D68 File Offset: 0x00032168
		public TileResultCode FreezeRotation(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.broken || this.unmoving)
			{
				return TileResultCode.False;
			}
			Rigidbody component = this.body.GetComponent<Rigidbody>();
			component.freezeRotation = true;
			return TileResultCode.True;
		}

		// Token: 0x06000771 RID: 1905 RVA: 0x00033DA4 File Offset: 0x000321A4
		public TileResultCode Translate(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.broken || this.unmoving)
			{
				return TileResultCode.True;
			}
			Vector3 dir = (args.Length <= 0) ? Vector3.forward : ((Vector3)args[0]);
			float num = (args.Length <= 1) ? this.walkController.defaultMaxSpeed : ((float)args[1]);
			this.walkController.Translate(dir, eInfo.floatArg * num);
			return TileResultCode.True;
		}

		// Token: 0x06000772 RID: 1906 RVA: 0x00033E1C File Offset: 0x0003221C
		public TileResultCode Walk(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.broken || this.unmoving)
			{
				return TileResultCode.True;
			}
			float num = (float)args[0] * eInfo.floatArg;
			if ((num > 0f && this.IsWalking() != TileResultCode.True) || (num < 0f && this.IsBacking() != TileResultCode.True))
			{
				this.body.GetComponent<Rigidbody>().AddForce(num * this.goT.forward, ForceMode.Impulse);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000773 RID: 1907 RVA: 0x00033EA4 File Offset: 0x000322A4
		public TileResultCode Turn(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.broken || this.unmoving)
			{
				return TileResultCode.True;
			}
			float speed = (float)args[0] * eInfo.floatArg;
			this.walkController.Turn(speed);
			return TileResultCode.True;
		}

		// Token: 0x06000774 RID: 1908 RVA: 0x00033EE8 File Offset: 0x000322E8
		public TileResultCode AvoidTag(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.broken || this.unmoving || this.vanished)
			{
				return TileResultCode.True;
			}
			string tagName = (string)args[0];
			float avoidDistance = (args.Length <= 1) ? this.walkController.defaultAvoidDistance : ((float)args[1]);
			float maxSpeed = (args.Length <= 2) ? this.walkController.defaultMaxSpeed : ((float)args[2]);
			this.walkController.AvoidTag(tagName, avoidDistance, maxSpeed, eInfo.floatArg);
			return TileResultCode.True;
		}

		// Token: 0x06000775 RID: 1909 RVA: 0x00033F7C File Offset: 0x0003237C
		public TileResultCode TurnTowardsTag(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.broken || this.unmoving || this.vanished)
			{
				return TileResultCode.True;
			}
			string tagName = (string)args[0];
			this.walkController.TurnTowardsTag(tagName);
			return TileResultCode.True;
		}

		// Token: 0x06000776 RID: 1910 RVA: 0x00033FC2 File Offset: 0x000323C2
		public TileResultCode TurnTowardsTap(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.broken || this.unmoving || this.vanished)
			{
				return TileResultCode.True;
			}
			this.walkController.TurnTowardsTap();
			return TileResultCode.True;
		}

		// Token: 0x06000777 RID: 1911 RVA: 0x00033FF3 File Offset: 0x000323F3
		public TileResultCode TurnAlongCam(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.broken || this.unmoving || this.vanished)
			{
				return TileResultCode.True;
			}
			this.walkController.TurnAlongCamera();
			return TileResultCode.True;
		}

		// Token: 0x06000778 RID: 1912 RVA: 0x00034024 File Offset: 0x00032424
		public TileResultCode Jump(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (!this.broken && !this.unmoving && !this.vanished)
			{
				float num = (float)args[0] * eInfo.floatArg;
				if (this.controllerWasActive || this.walkController.IsActive())
				{
					this.walkController.Jump(num);
				}
				else if (this.IsOnGround(this.onGroundHeight))
				{
					if (this.upright)
					{
						num *= 2f;
					}
					this.body.GetComponent<Rigidbody>().AddForce(num * Vector3.up, ForceMode.Impulse);
					this.jumpCountdown = this.startJumpCountdown;
				}
			}
			return TileResultCode.True;
		}

		// Token: 0x06000779 RID: 1913 RVA: 0x000340DB File Offset: 0x000324DB
		public TileResultCode Idle(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.idle = true;
			return TileResultCode.True;
		}

		// Token: 0x0600077A RID: 1914 RVA: 0x000340E5 File Offset: 0x000324E5
		public TileResultCode Stand(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.MakeMoving();
			this.idle = true;
			return TileResultCode.True;
		}

		// Token: 0x0600077B RID: 1915 RVA: 0x000340F5 File Offset: 0x000324F5
		public TileResultCode Sit(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.idle = true;
			this.MakeUnmoving();
			return TileResultCode.True;
		}

		// Token: 0x0600077C RID: 1916 RVA: 0x00034108 File Offset: 0x00032508
		public TileResultCode DPadControl(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.walkController != null && !this.vanished)
			{
				string key = (args.Length <= 0) ? "L" : ((string)args[0]);
				float maxSpeed = (args.Length <= 1) ? this.walkController.defaultDPadMaxSpeed : ((float)args[1]);
				float wackiness = (args.Length <= 2) ? this.walkController.defaultWackiness : ((float)args[2]);
				Blocksworld.UI.Controls.EnableDPad(key, MoverDirectionMask.ALL);
				Blocksworld.worldSessionHadBlocksterMover = true;
				this.walkController.DPadControl(key, maxSpeed, wackiness);
			}
			return TileResultCode.True;
		}

		// Token: 0x0600077D RID: 1917 RVA: 0x000341B2 File Offset: 0x000325B2
		protected override void HandleTiltMover(float xTilt, float yTilt, float zTilt)
		{
		}

		// Token: 0x0600077E RID: 1918 RVA: 0x000341B4 File Offset: 0x000325B4
		public TileResultCode GotoTap(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.walkController != null && !this.vanished)
			{
				float maxSpeed = (args.Length <= 0) ? this.walkController.defaultMaxSpeed : ((float)args[0]);
				float wackiness = (args.Length <= 1) ? this.walkController.defaultWackiness : ((float)args[1]);
				this.walkController.GotoTap(maxSpeed, wackiness, true);
			}
			return TileResultCode.True;
		}

		// Token: 0x0600077F RID: 1919 RVA: 0x0003422C File Offset: 0x0003262C
		public TileResultCode GotoTag(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.walkController != null && !this.vanished)
			{
				string tagName = (args.Length <= 0) ? string.Empty : ((string)args[0]);
				float maxSpeed = (args.Length <= 1) ? this.walkController.defaultMaxSpeed : ((float)args[1]);
				float wackiness = (args.Length <= 2) ? this.walkController.defaultWackiness : ((float)args[2]);
				this.walkController.GotoTag(tagName, maxSpeed, wackiness, eInfo.floatArg, true);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000780 RID: 1920 RVA: 0x000342C8 File Offset: 0x000326C8
		public TileResultCode ChaseTag(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.walkController != null && !this.vanished)
			{
				string tagName = (args.Length <= 0) ? string.Empty : ((string)args[0]);
				float maxSpeed = (args.Length <= 1) ? this.walkController.defaultMaxSpeed : ((float)args[1]);
				float wackiness = (args.Length <= 2) ? this.walkController.defaultWackiness : ((float)args[2]);
				this.walkController.GotoTag(tagName, maxSpeed, wackiness, eInfo.floatArg, false);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000781 RID: 1921 RVA: 0x00034361 File Offset: 0x00032761
		public TileResultCode IsWalkingSensor(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return ((float)args[0] >= 0f) ? this.IsWalking() : this.IsBacking();
		}

		// Token: 0x06000782 RID: 1922 RVA: 0x00034388 File Offset: 0x00032788
		public TileResultCode IsWalking()
		{
			return (this.broken || this.unmoving || Vector3.Dot(this.body.GetComponent<Rigidbody>().velocity, this.goT.forward) < 0f) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000783 RID: 1923 RVA: 0x000343DC File Offset: 0x000327DC
		public TileResultCode IsBacking()
		{
			return (this.broken || this.unmoving || Vector3.Dot(this.body.GetComponent<Rigidbody>().velocity, this.goT.forward) >= 0f) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000784 RID: 1924 RVA: 0x00034430 File Offset: 0x00032830
		public TileResultCode IsTurning(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return ((float)args[0] >= 0f) ? this.IsTurningRight() : this.IsTurningLeft();
		}

		// Token: 0x06000785 RID: 1925 RVA: 0x00034458 File Offset: 0x00032858
		public TileResultCode IsTurningLeft()
		{
			return (this.broken || this.unmoving || this.body.GetComponent<Rigidbody>().angularVelocity.y >= 0f) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000786 RID: 1926 RVA: 0x000344A4 File Offset: 0x000328A4
		public TileResultCode IsTurningRight()
		{
			return (this.broken || this.unmoving || this.body.GetComponent<Rigidbody>().angularVelocity.y <= 0f) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000787 RID: 1927 RVA: 0x000344F0 File Offset: 0x000328F0
		public TileResultCode IsJumping(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return this.IsOnGround(this.onGroundHeight) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000788 RID: 1928 RVA: 0x0003450C File Offset: 0x0003290C
		public bool IsOnGround(float onGroundHeight)
		{
			this.IgnoreRaycasts(true);
			RaycastHit raycastHit;
			bool flag = Physics.Raycast(this.goT.position, -Vector3.up, out raycastHit, onGroundHeight, BlockAbstractLegs.raycastMask);
			if (flag && raycastHit.collider != null)
			{
				flag = !raycastHit.collider.isTrigger;
			}
			this.IgnoreRaycasts(false);
			return flag;
		}

		// Token: 0x06000789 RID: 1929 RVA: 0x00034573 File Offset: 0x00032973
		public bool GetThinksOnGround()
		{
			return this.onGround;
		}

		// Token: 0x0600078A RID: 1930 RVA: 0x0003457C File Offset: 0x0003297C
		public override TileResultCode PlaySound(string sfxName, string location, string soundType, float volume, float pitch, bool durational = false, float timer = 0f)
		{
			if (durational && Sound.sfxEnabled && !Sound.BlockIsMuted(this))
			{
				if (timer == 0f)
				{
					this.voxAudioSource.Stop();
					this.voxAudioSource.clip = Sound.GetSfx(sfxName);
					this.voxAudioSource.Play();
				}
				return base.UpdateDurationalSoundSource(sfxName, timer);
			}
			return base.PlaySound(sfxName, location, soundType, volume, pitch, durational, timer);
		}

		// Token: 0x0600078B RID: 1931 RVA: 0x000345F3 File Offset: 0x000329F3
		private void PlayOhSound(float vol)
		{
			this.PlaySemanticSound("Oh", vol, this.ohPlayProbability);
		}

		// Token: 0x0600078C RID: 1932 RVA: 0x00034608 File Offset: 0x00032A08
		private void PlayWheeSound(float vol)
		{
			this.PlaySemanticSound("Whee", vol, this.wheePlayProbability);
		}

		// Token: 0x0600078D RID: 1933 RVA: 0x0003461D File Offset: 0x00032A1D
		private void PlayOuchSound(float vol, bool always = false)
		{
			if (this.PlaySemanticSound("Ouch", vol, (!always) ? this.ouchPlayProbability : 1f))
			{
				this.speakOuchSoundTime = Time.time;
			}
		}

		// Token: 0x0600078E RID: 1934 RVA: 0x00034654 File Offset: 0x00032A54
		private bool PlaySemanticSound(string name, float vol, float prob)
		{
			if (UnityEngine.Random.value < prob && !Sound.durationalSoundBlockIDs.Contains(this.go.GetInstanceID()))
			{
				string text = this.SampleSfx(name, false);
				if (text.Length > 0 && Sound.sfxEnabled && !Sound.BlockIsMuted(this) && (!this.voxAudioSource.isPlaying || (this.canInterrupts.ContainsKey(text) && this.canInterrupts[text])))
				{
					foreach (string key in new List<string>(this.forbiddenCounters.Keys))
					{
						int num = this.forbiddenCounters[key] - 1;
						this.forbiddenCounters[key] = num;
						if (num <= 0)
						{
							this.forbiddenCounters.Remove(key);
						}
					}
					this.forbiddenCounters[text] = 1;
					this.voxAudioSource.Stop();
					this.voxAudioSource.clip = Sound.GetSfx(text);
					this.voxAudioSource.Play();
				}
				this.speakSoundTime = Time.time;
				return true;
			}
			return false;
		}

		// Token: 0x0600078F RID: 1935 RVA: 0x000347A8 File Offset: 0x00032BA8
		public override void Mute()
		{
			base.Mute();
			if (this.voxAudioSource.isPlaying)
			{
				this.voxAudioSource.Stop();
			}
		}

		// Token: 0x06000790 RID: 1936 RVA: 0x000347CC File Offset: 0x00032BCC
		public void Collided(Vector3 relativeVelocity)
		{
			if (this.broken)
			{
				return;
			}
			this.noCollisionCounter = 0;
			if (this.playCallouts && this.hasOuchSound && Time.time - this.speakOuchSoundTime > 1f)
			{
				Vector3 vector = relativeVelocity;
				float magnitude = vector.magnitude;
				if (magnitude > 6f || this.broken)
				{
					float vol = 0.2f * (magnitude - 5f);
					this.PlayOuchSound(vol, true);
				}
			}
		}

		// Token: 0x06000791 RID: 1937 RVA: 0x00034850 File Offset: 0x00032C50
		public void UpdateSounds()
		{
			if (this.playCallouts && (this.hasWheeSound || this.hasOhSound || this.hasOuchSound) && this.body != null)
			{
				Rigidbody component = this.body.GetComponent<Rigidbody>();
				if (component != null)
				{
					float num = (component.velocity - this.oldVel).magnitude / Blocksworld.fixedDeltaTime;
					this.oldVel = component.velocity;
					this.wheeness = this.wheeness * 0.95f + num * 0.05f;
					float time = Time.time;
					if (num > 400f && time - this.speakOuchSoundTime > 1f)
					{
						this.PlayOuchSound(1f, false);
						this.wheeness = 0f;
					}
					if (!this.footHitGround && time - this.speakSoundTime > 1.5f && this.wheeness > 25f)
					{
						float num2 = 0.5f * (1f - Mathf.Max((float)this.jumpCountdown, 0f) / (float)this.startJumpCountdown) + 0.5f;
						if (UnityEngine.Random.value < num2)
						{
							this.PlayWheeSound(1f);
							this.wheeness = 0f;
						}
					}
					float num3 = 0f;
					if (this.walkController != null)
					{
						num3 = Mathf.Sqrt(this.walkController.GetWantedSpeedSqr());
					}
					if (!this.unmoving && this.footHitGround && time - this.speakSoundTime > 3f)
					{
						float num4 = Mathf.Abs(component.velocity.magnitude - num3);
						float num5 = 1f / Mathf.Max(num3, 1f);
						num4 *= num5;
						float num6 = component.angularVelocity.magnitude * num5;
						if (num4 > 5f || num6 > 5f)
						{
							this.PlayOhSound(1f);
							this.speakSoundTime = time;
						}
					}
				}
			}
		}

		// Token: 0x06000792 RID: 1938 RVA: 0x00034A6C File Offset: 0x00032E6C
		protected virtual void ReplaceCollider()
		{
			if (this.replaceWithCapsuleCollider)
			{
				if (BlockAbstractLegs.capsuleColliderMaterial == null)
				{
					BlockAbstractLegs.capsuleColliderMaterial = new PhysicMaterial();
					BlockAbstractLegs.capsuleColliderMaterial.dynamicFriction = 0.3f;
					BlockAbstractLegs.capsuleColliderMaterial.staticFriction = 0.3f;
					BlockAbstractLegs.capsuleColliderMaterial.frictionCombine = PhysicMaterialCombine.Minimum;
				}
				BoxCollider[] componentsInChildren = this.go.GetComponentsInChildren<BoxCollider>();
				foreach (BoxCollider obj in componentsInChildren)
				{
					UnityEngine.Object.Destroy(obj);
				}
				CapsuleCollider capsuleCollider = this.go.AddComponent<CapsuleCollider>();
				capsuleCollider.material = BlockAbstractLegs.capsuleColliderMaterial;
				capsuleCollider.height = this.capsuleColliderHeight;
				capsuleCollider.center = this.capsuleColliderOffset;
				capsuleCollider.radius = this.capsuleColliderRadius;
				capsuleCollider.enabled = false;
				capsuleCollider.enabled = true;
				this.walkController.AddIgnoreCollider(capsuleCollider);
			}
		}

		// Token: 0x06000793 RID: 1939 RVA: 0x00034B50 File Offset: 0x00032F50
		protected virtual void RestoreCollider()
		{
		}

		// Token: 0x06000794 RID: 1940 RVA: 0x00034B54 File Offset: 0x00032F54
		protected virtual void PositionAnkles()
		{
			for (int i = 0; i < this.legPairCount; i++)
			{
				for (int j = 0; j < 2; j++)
				{
					this.PositionAnkle(j + i * 2);
				}
			}
		}

		// Token: 0x06000795 RID: 1941 RVA: 0x00034B98 File Offset: 0x00032F98
		public override void Update()
		{
			base.Update();
			if (this.broken)
			{
				return;
			}
			if (this.unmoving)
			{
				return;
			}
			bool flag = Blocksworld.CurrentState == State.Play;
			if (flag)
			{
				this.UpdateBodyIfNecessary();
				this.upright = (this.goT.up.y > 0.5f);
				this.PositionAnkles();
			}
		}

		// Token: 0x06000796 RID: 1942 RVA: 0x00034C00 File Offset: 0x00033000
		public float Ipm(float v, float h)
		{
			float value = v * Mathf.Sqrt(h / 9.82f + v * v / 385.729584f);
			return Mathf.Clamp(value, -this.maxStepLength, this.maxStepLength);
		}

		// Token: 0x06000797 RID: 1943 RVA: 0x00034C3C File Offset: 0x0003303C
		protected virtual Vector3 GetFeetCenter()
		{
			Vector3 vector = Vector3.zero;
			foreach (FootInfo footInfo in this.feet)
			{
				vector += footInfo.go.transform.position / (float)this.feet.Length;
			}
			return vector;
		}

		// Token: 0x06000798 RID: 1944 RVA: 0x00034C94 File Offset: 0x00033094
		public void ChangeInertia()
		{
			Rigidbody rigidBody = this.GetRigidBody();
			if (rigidBody != null && !rigidBody.isKinematic && !this.ChunkContainsOtherLegs())
			{
				Vector3 inertiaTensor = rigidBody.inertiaTensor;
				float num = (!(this.legParameters != null)) ? 1f : this.legParameters.inertiaThreshold;
				float num2 = (!(this.legParameters != null)) ? 2f : this.legParameters.inertiaScaler;
				bool flag = false;
				for (int i = 0; i < 3; i++)
				{
					float num3 = inertiaTensor[i];
					if (num3 > num)
					{
						num3 = num + (num3 - num) / num2;
						inertiaTensor[i] = num3;
						flag = true;
					}
				}
				if (flag)
				{
					try
					{
						rigidBody.inertiaTensor = inertiaTensor;
					}
					catch
					{
						BWLog.Info("Unable to set inertia tensor, possibly due to the use of rigidbody constraints in the world.");
					}
				}
			}
		}

		// Token: 0x06000799 RID: 1945 RVA: 0x00034D98 File Offset: 0x00033198
		protected bool ChunkContainsOtherLegs()
		{
			bool result = false;
			foreach (Block block in this.chunk.blocks)
			{
				if (block is BlockAbstractLegs && block != this)
				{
					result = true;
					break;
				}
			}
			return result;
		}

		// Token: 0x0600079A RID: 1946 RVA: 0x00034E10 File Offset: 0x00033210
		private bool ChunkContainsJoint()
		{
			List<Block> blocks = this.chunk.blocks;
			for (int i = 0; i < blocks.Count; i++)
			{
				Block block = blocks[i];
				if (block.connectionTypes.Contains(2) || block.connectionTypes.Contains(-2))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600079B RID: 1947 RVA: 0x00034E70 File Offset: 0x00033270
		protected void MoveCenterOfMass()
		{
			Rigidbody rigidBody = this.GetRigidBody();
			if (rigidBody != null && !rigidBody.isKinematic && !this.ChunkContainsOtherLegs())
			{
				Vector3 feetCenter = this.GetFeetCenter();
				Vector3 vector = feetCenter + this.goT.up * this.moveCMOffsetFeetCenter - rigidBody.worldCenterOfMass;
				if (vector.magnitude > this.moveCMMaxDistance)
				{
					vector = vector.normalized * this.moveCMMaxDistance;
				}
				if (rigidBody.mass > 3f)
				{
					float d = rigidBody.mass - 2f;
					vector /= d;
				}
				rigidBody.centerOfMass += vector;
			}
		}

		// Token: 0x0600079C RID: 1948 RVA: 0x00034F38 File Offset: 0x00033338
		protected void UpdateColliderAndCM(bool controllerActive)
		{
			bool flag = false;
			if (controllerActive && !this.controllerWasActive)
			{
				this.ReplaceCollider();
				flag = true;
			}
			else if (!controllerActive && this.controllerWasActive)
			{
				this.RestoreCollider();
				flag = true;
			}
			if (controllerActive)
			{
				if (this.moveCM && (!this.hasMovedCM || flag))
				{
					this.hasMovedCM = true;
				}
				if (!this.hasChangedInertia)
				{
					this.ChangeInertia();
					this.hasChangedInertia = true;
				}
			}
		}

		// Token: 0x0600079D RID: 1949 RVA: 0x00034FBF File Offset: 0x000333BF
		private void UpdateBodyIfNecessary()
		{
			if (this.body == null)
			{
				this.body = this.chunk.go;
			}
		}

		// Token: 0x0600079E RID: 1950 RVA: 0x00034FE4 File Offset: 0x000333E4
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			this.noCollisionCounter++;
			if (this.broken && !this.wasBroken && this.playCallouts)
			{
				this.PlayOuchSound(1f, false);
			}
			this.wasBroken = this.broken;
			if (this.broken || this.vanished)
			{
				return;
			}
			bool flag = Blocksworld.CurrentState == State.Play;
			if (flag)
			{
				this.UpdateSounds();
			}
			if (this.unmoving)
			{
				return;
			}
			if (this.chunk.IsFrozen())
			{
				return;
			}
			Transform goT = this.goT;
			Vector3 up = goT.up;
			this.upright = (up.y > 0.5f);
			this.UpdateBodyIfNecessary();
			bool flag2 = this.walkController != null && this.walkController.IsActive() && !this.didFix;
			this.OrientFeetWithGround(!flag2 || this.walkController.IsOnGround());
			if (!flag2)
			{
				if (this.wacky)
				{
					this.wacky = false;
				}
				else
				{
					flag2 = true;
					this.walkController.Translate(Vector3.forward, 0f);
				}
			}
			this.UpdateColliderAndCM(flag2);
			this.stepTimer += Blocksworld.fixedDeltaTime;
			Rigidbody component = this.body.GetComponent<Rigidbody>();
			float sqrMagnitude = Util.ProjectOntoPlane(component.velocity, Vector3.up).sqrMagnitude;
			float num = sqrMagnitude;
			float num2 = this.stepTime * this.stepTimeMultiplier;
			bool flag3 = false;
			if (flag2)
			{
				num2 = this.stepTimeMultiplier * this.stepTime * (1f - this.walkController.GetAndResetHighSpeedFraction());
				this.buoyancyMultiplier = 2.5f;
				float sqrMagnitude2 = this.walkController.GetRigidBodyBelowVelocity().sqrMagnitude;
				num = 2f * Mathf.Max(sqrMagnitude - sqrMagnitude2, this.walkController.GetWantedSpeedSqr() - sqrMagnitude2);
				if (up.y < 0.955f && this.onGround)
				{
					flag3 = true;
				}
			}
			else
			{
				this.buoyancyMultiplier = 1f;
			}
			if (this.stepTimer > num2 && ((num > this.stepSpeedTrigger && !this.idle) || flag3))
			{
				this.groundGO = null;
				this.footHitGround = false;
				this.IgnoreRaycasts(true);
				Transform transform = this.body.transform;
				Vector3 position = transform.position;
				Vector3 vector = position;
				if (this.multiLegged)
				{
					vector += Util.ProjectOntoPlane(goT.position - vector, up);
				}
				RaycastHit raycastHit;
				this.onGround = (Physics.Raycast(goT.position + up * 0.5f, -Vector3.up, out raycastHit, 5f, BlockAbstractLegs.raycastMask) && !raycastHit.collider.isTrigger);
				if (this.upright && this.onGround)
				{
					Vector3 velocity = component.velocity;
					velocity.y = 0f;
					Vector3 lhs = Vector3.zero;
					Vector3 a = Vector3.zero;
					float num3 = 1f;
					if (flag2)
					{
						num3 = this.walkController.GetWackiness();
						a = this.walkController.GetTotalForce();
						lhs = this.walkController.GetTotalCmTorque();
					}
					if (num3 != this.previousWackiness)
					{
						if (this.feet[0].joint != null)
						{
							for (int i = 0; i < 2 * this.legPairCount; i++)
							{
								this.SetSpring(this.feet[i].joint);
							}
						}
						this.previousWackiness = num3;
					}
					this.vi += velocity * Blocksworld.fixedDeltaTime;
					if (this.vi.sqrMagnitude > 1f)
					{
						this.vi.Normalize();
					}
					this.vd = (velocity - this.vpe) / Blocksworld.fixedDeltaTime;
					Vector3 vector2 = 1f * velocity + 2f * this.vi + 0.03f * this.vd;
					this.vpe = velocity;
					float num4 = this.Ipm(vector2.z, position.y - raycastHit.point.y);
					float num5 = this.Ipm(vector2.x, position.y - raycastHit.point.y);
					Vector3 a2 = raycastHit.point - (this.footOffsetY - 0.125f) * Vector3.up;
					float num6 = 1f;
					if (flag2)
					{
						float magnitude = velocity.magnitude;
						num6 = Mathf.Clamp(magnitude / 8f, 0.1f, 1f);
					}
					float num7 = num2 / (this.stepTime * this.stepTimeMultiplier);
					num5 *= num6 * this.stepLengthMultiplier * num7;
					num4 *= num6 * this.stepLengthMultiplier * num7;
					Vector3 vector3 = a2 + this.footOffsetY * Vector3.up + num4 * Vector3.forward + num5 * Vector3.right;
					if (flag2)
					{
						Vector3 a3 = default(Vector3) + a * 0.03f;
						float magnitude2 = lhs.magnitude;
						a3 += Vector3.Cross(lhs, -Vector3.up).normalized * magnitude2 * 0.05f;
						a3.y = 0f;
						float sqrMagnitude3 = a3.sqrMagnitude;
						float num8 = 0.5f;
						if (sqrMagnitude3 > num8 * num8)
						{
							a3.Normalize();
							a3 *= num8;
						}
						vector3 += a3 * this.stepLengthMultiplier;
					}
					this.groundGO = raycastHit.collider.gameObject;
					for (int j = 0; j < this.legPairIndices.Length; j++)
					{
						int[] array = this.legPairIndices[j];
						int num9 = array[this.currentLeg];
						int num10 = array[(this.currentLeg + 1) % 2];
						float d = this.legPairOffsets[j];
						Vector3 forward = goT.forward;
						forward.y = 0f;
						vector3 += forward * d;
						FootInfo footInfo = this.feet[num9];
						FootInfo footInfo2 = this.feet[num10];
						footInfo.normal = raycastHit.normal;
						Vector3 a4 = Quaternion.Euler(0f, goT.rotation.eulerAngles.y, 0f) * Vector3.right;
						Vector3 vector4 = footInfo2.go.transform.position;
						int num11 = num9 % 2;
						vector4 += (float)((num11 != 0) ? -1 : 1) * (0.75f - this.xWidth) * a4;
						Vector3 vector5 = (float)((num11 != 0) ? -1 : 1) * a4;
						Plane plane = new Plane(vector5, vector4);
						if (!plane.GetSide(vector3))
						{
							vector3 -= plane.GetDistanceToPoint(vector3) * vector5;
						}
						if (Mathf.Abs(vector3.y - goT.position.y) < this.maxStepHeight && Vector3.Dot(footInfo.normal, Vector3.up) > this.maxSurfaceWalkAngleDot)
						{
							this.footHitGround = true;
							Vector3 position2 = vector3 - this.walkController.GetRigidBodyBelowVelocity() * Blocksworld.fixedDeltaTime * this.xWidth;
							footInfo.go.transform.position = position2;
							footInfo.position = position2;
							List<string> possibleSfxs = this.GetPossibleSfxs("Legs Step", true);
							if (possibleSfxs.Count > 0)
							{
								string sfxName = possibleSfxs[this.currentLeg % possibleSfxs.Count];
								base.PlayPositionedSound(sfxName, 0.2f, 1f);
							}
							this.stepTimer = 0f;
						}
					}
					this.currentLeg = (this.currentLeg + 1) % 2;
				}
				this.IgnoreRaycasts(false);
			}
			Vector3 a5 = Vector3.zero;
			if (flag2)
			{
				this.walkController.FixedUpdate();
				a5 = this.walkController.GetRigidBodyBelowVelocity();
			}
			this.controllerWasActive = flag2;
			if (this.lpFilterUpdateCounter % 5 == 0)
			{
				base.UpdateWithinWaterLPFilter(null);
				base.UpdateWithinWaterLPFilter(this.voxGameObject);
			}
			this.lpFilterUpdateCounter++;
			for (int k = 0; k < this.feet.Length; k++)
			{
				FootInfo footInfo3 = this.feet[k];
				Rigidbody rb = footInfo3.rb;
				if (this.jumpCountdown <= 0 && this.upright && this.stepTimer <= num2 && !this.idle && Mathf.Abs(footInfo3.position.y - this.goT.position.y) < this.maxStepHeight && !Util.IsNullVector3(footInfo3.position) && Vector3.Dot(footInfo3.normal, Vector3.up) > this.maxSurfaceWalkAngleDot)
				{
					footInfo3.go.transform.position = footInfo3.position;
					rb.velocity = Vector3.zero;
					if (flag2)
					{
						footInfo3.position += a5 * Blocksworld.fixedDeltaTime;
					}
					if (this.groundGO != null)
					{
						CollisionManager.ForwardCollisionEnter(this.go, this.groundGO, null);
					}
				}
				else
				{
					footInfo3.position = Util.nullVector3;
				}
				if (rb.IsSleeping())
				{
					rb.WakeUp();
				}
			}
			this.jumpCountdown--;
			this.idle = false;
		}

		// Token: 0x0600079F RID: 1951 RVA: 0x00035A2C File Offset: 0x00033E2C
		public virtual void PositionAnkle(int i)
		{
			FootInfo footInfo = this.feet[i];
			if (footInfo.bone != null)
			{
				footInfo.bone.position = footInfo.go.transform.position - footInfo.go.transform.TransformDirection(Vector3.up) * footInfo.boneYOffset;
				footInfo.bone.rotation = footInfo.go.transform.rotation;
			}
		}

		// Token: 0x060007A0 RID: 1952 RVA: 0x00035AB0 File Offset: 0x00033EB0
		public virtual void OrientFeetWithGround(bool v = true)
		{
			for (int i = 0; i < 2 * this.legPairCount; i++)
			{
				this.feet[i].go.transform.rotation = ((!this.upright || !v) ? this.goT.rotation : Quaternion.Euler(0f, this.goT.eulerAngles.y, 0f));
			}
		}

		// Token: 0x060007A1 RID: 1953 RVA: 0x00035B30 File Offset: 0x00033F30
		public override List<GameObject> GetIgnoreRaycastGOs()
		{
			List<GameObject> list = base.GetIgnoreRaycastGOs();
			for (int i = 0; i < this.feet.Length; i++)
			{
				list.Add(this.feet[i].go);
			}
			return list;
		}

		// Token: 0x060007A2 RID: 1954 RVA: 0x00035B74 File Offset: 0x00033F74
		public override void IgnoreRaycasts(bool value)
		{
			if (this.ignoreRaycastGOs == null)
			{
				this.ignoreRaycastGOs = new List<GameObject>();
				this.ignoreRaycastGOs.Add(this.go);
				for (int i = 0; i < 2 * this.legPairCount; i++)
				{
					this.ignoreRaycastGOs.Add(this.feet[i].go);
				}
				Vector3 position = this.goT.position;
				if (this.body != null)
				{
					this.ignoreRaycastGOs.Add(this.body);
					foreach (Block block in this.chunk.blocks)
					{
						if (block != this)
						{
							foreach (GameObject gameObject in block.GetIgnoreRaycastGOs())
							{
								Vector3 position2 = gameObject.transform.position;
								if ((position2 - position).magnitude < 2f)
								{
									this.ignoreRaycastGOs.Add(gameObject);
								}
							}
						}
					}
				}
			}
			int layer = (int)((!value) ? this.goLayerAssignment : Layer.IgnoreRaycast);
			for (int j = 0; j < this.ignoreRaycastGOs.Count; j++)
			{
				GameObject gameObject2 = this.ignoreRaycastGOs[j];
				if (gameObject2 != null)
				{
					gameObject2.layer = layer;
				}
			}
		}

		// Token: 0x060007A3 RID: 1955 RVA: 0x00035D30 File Offset: 0x00034130
		public override bool ScaleTo(Vector3 scale, bool recalculateCollider = true, bool forceRescale = false)
		{
			return true;
		}

		// Token: 0x060007A4 RID: 1956 RVA: 0x00035D34 File Offset: 0x00034134
		public override TileResultCode Vanish(ScriptRowExecutionInfo eInfo, object[] args)
		{
			TileResultCode tileResultCode = base.Vanish(eInfo, args);
			if (tileResultCode == TileResultCode.True)
			{
				for (int i = 0; i < this.feet.Length; i++)
				{
					this.feet[i].go.GetComponent<Renderer>().enabled = false;
					this.feet[i].go.SetActive(false);
				}
			}
			else if (tileResultCode == TileResultCode.Delayed)
			{
				for (int j = 0; j < this.feet.Length; j++)
				{
					this.feet[j].go.transform.localScale = this.goT.localScale;
				}
			}
			return tileResultCode;
		}

		// Token: 0x060007A5 RID: 1957 RVA: 0x00035DDD File Offset: 0x000341DD
		public TileResultCode WackyMode(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.wacky = true;
			return TileResultCode.True;
		}

		// Token: 0x060007A6 RID: 1958 RVA: 0x00035DE8 File Offset: 0x000341E8
		protected virtual void UnVanishFeet()
		{
			for (int i = 0; i < this.feet.Length; i++)
			{
				GameObject go = this.feet[i].go;
				if (go.GetComponent<Renderer>().sharedMaterial.shader != this.InvisibleShader)
				{
					go.GetComponent<Renderer>().enabled = true;
					go.SetActive(true);
				}
				go.transform.localScale = this.oldLocalScale;
			}
		}

		// Token: 0x060007A7 RID: 1959 RVA: 0x00035E60 File Offset: 0x00034260
		public Rigidbody GetRigidBody()
		{
			this.UpdateBodyIfNecessary();
			return (!(this.body != null)) ? null : this.body.GetComponent<Rigidbody>();
		}

		// Token: 0x060007A8 RID: 1960 RVA: 0x00035E8A File Offset: 0x0003428A
		public virtual bool FeetPartOfGo()
		{
			return false;
		}

		// Token: 0x060007A9 RID: 1961 RVA: 0x00035E90 File Offset: 0x00034290
		public override void Appeared()
		{
			base.Appeared();
			if (!this.FeetPartOfGo())
			{
				this.ResetFeetPositions();
				for (int i = 0; i < this.feet.Length; i++)
				{
					FootInfo footInfo = this.feet[i];
					Rigidbody rb = footInfo.rb;
					if (rb != null && !rb.isKinematic)
					{
						rb.velocity = Vector3.zero;
					}
					if (footInfo.go.GetComponent<Renderer>().sharedMaterial.shader != this.GetInvisibleShader())
					{
						footInfo.go.GetComponent<Renderer>().enabled = true;
					}
					footInfo.go.GetComponent<Collider>().enabled = true;
				}
			}
		}

		// Token: 0x060007AA RID: 1962 RVA: 0x00035F46 File Offset: 0x00034346
		public Shader GetInvisibleShader()
		{
			BlockAbstractLegs.invisibleShader = ((!(BlockAbstractLegs.invisibleShader != null)) ? Shader.Find("Blocksworld/Invisible") : BlockAbstractLegs.invisibleShader);
			return BlockAbstractLegs.invisibleShader;
		}

		// Token: 0x060007AB RID: 1963 RVA: 0x00035F78 File Offset: 0x00034378
		public override void Vanished()
		{
			base.Vanished();
			if (!this.FeetPartOfGo())
			{
				for (int i = 0; i < this.feet.Length; i++)
				{
					FootInfo footInfo = this.feet[i];
					Renderer component = footInfo.go.GetComponent<Renderer>();
					Collider component2 = footInfo.go.GetComponent<Collider>();
					if (component != null)
					{
						footInfo.go.GetComponent<Renderer>().enabled = false;
					}
					if (component2 != null)
					{
						component2.enabled = false;
					}
				}
			}
		}

		// Token: 0x060007AC RID: 1964 RVA: 0x00036000 File Offset: 0x00034400
		public override void Teleported(bool resetAngle = false, bool resetVel = false, bool resetAngVel = false)
		{
			base.Teleported(resetAngle, resetVel, resetAngVel);
			if (!this.FeetPartOfGo())
			{
				this.ResetFeetPositions();
				for (int i = 0; i < this.feet.Length; i++)
				{
					if (resetVel || resetAngVel)
					{
						Rigidbody rb = this.feet[i].rb;
						if (rb != null)
						{
							if (resetVel)
							{
								rb.velocity = Vector3.zero;
							}
							if (resetAngVel)
							{
								rb.angularVelocity = Vector3.zero;
							}
						}
					}
				}
			}
		}

		// Token: 0x060007AD RID: 1965 RVA: 0x00036090 File Offset: 0x00034490
		public override void Destroy()
		{
			for (int i = 0; i < this.feet.Length; i++)
			{
				Mesh ankleMesh = this.feet[i].ankleMesh;
				if (ankleMesh != this.mesh)
				{
					UnityEngine.Object.Destroy(ankleMesh);
				}
				if (this.feet[i].go != null)
				{
					UnityEngine.Object.Destroy(this.feet[i].go);
				}
			}
			base.Destroy();
		}

		// Token: 0x060007AE RID: 1966 RVA: 0x0003610C File Offset: 0x0003450C
		public override void RemoveBlockMaps()
		{
			if (this.feet != null)
			{
				for (int i = 0; i < this.feet.Length; i++)
				{
					if (this.feet[i].go != null)
					{
						BWSceneManager.RemoveChildBlockInstanceID(this.feet[i].go);
					}
				}
			}
			base.RemoveBlockMaps();
		}

		// Token: 0x060007AF RID: 1967 RVA: 0x00036170 File Offset: 0x00034570
		public override TileResultCode TextureTo(string texture, Vector3 normal, bool permanent, int meshIndex = 0, bool force = false)
		{
			TileResultCode tileResultCode = base.TextureTo(texture, normal, permanent, meshIndex, force);
			if (tileResultCode == TileResultCode.True && meshIndex == 0)
			{
				this.SetSfxs(texture, meshIndex);
			}
			return tileResultCode;
		}

		// Token: 0x060007B0 RID: 1968 RVA: 0x000361A4 File Offset: 0x000345A4
		public override float GetCurrentMassChange()
		{
			if (this.FeetPartOfGo())
			{
				return 0f;
			}
			float num = 0f;
			if (this.feet != null)
			{
				foreach (FootInfo footInfo in this.feet)
				{
					if (footInfo != null)
					{
						Rigidbody rb = footInfo.rb;
						if (rb != null)
						{
							num += rb.mass;
						}
					}
				}
			}
			return num;
		}

		// Token: 0x060007B1 RID: 1969 RVA: 0x00036218 File Offset: 0x00034618
		public override bool CanChangeMass()
		{
			return true;
		}

		// Token: 0x060007B2 RID: 1970 RVA: 0x0003621B File Offset: 0x0003461B
		public override void BecameTreasure()
		{
			base.BecameTreasure();
			this.MakeUnmoving();
		}

		// Token: 0x060007B3 RID: 1971 RVA: 0x00036229 File Offset: 0x00034629
		protected override void RestoreMeshColliderInfo()
		{
			base.RestoreMeshColliderInfo();
			Blocksworld.AddFixedUpdateCommand(new DelayedDelegateCommand(delegate()
			{
				if (this.walkController != null)
				{
					this.walkController.ClearIgnoreColliders();
				}
			}, 1));
		}

		// Token: 0x060007B4 RID: 1972 RVA: 0x00036248 File Offset: 0x00034648
		public override bool TreatAsVehicleLikeBlock()
		{
			return base.TreatAsVehicleLikeBlockWithStatus(ref this.treatAsVehicleStatus);
		}

		// Token: 0x060007B5 RID: 1973 RVA: 0x00036258 File Offset: 0x00034658
		public override void ChunksAndJointsModified(Dictionary<Joint, Joint> oldToNew, Dictionary<Chunk, Chunk> oldToNewChunks, Dictionary<Chunk, Chunk> newToOldChunks)
		{
			if (this.broken || this.unmoving || this.isTreasure)
			{
				return;
			}
			Chunk chunk;
			if (newToOldChunks.TryGetValue(this.chunk, out chunk))
			{
				this.body = this.chunk.go;
				Rigidbody component = this.body.GetComponent<Rigidbody>();
				for (int i = 0; i < this.feet.Length; i++)
				{
					FootInfo footInfo = this.feet[i];
					Vector3 position = chunk.go.transform.TransformPoint(footInfo.joint.connectedAnchor);
					footInfo.joint.connectedBody = component;
					footInfo.joint.connectedAnchor = this.body.transform.InverseTransformPoint(position);
					this.SetSpring(footInfo.joint);
				}
				if (this.walkController != null)
				{
					this.walkController.SetChunk();
				}
			}
		}

		// Token: 0x060007B6 RID: 1974 RVA: 0x00036344 File Offset: 0x00034744
		public override void Deactivate()
		{
			base.Deactivate();
			for (int i = 0; i < this.feet.Length; i++)
			{
				this.feet[i].go.SetActive(false);
			}
		}

		// Token: 0x060007B7 RID: 1975 RVA: 0x00036384 File Offset: 0x00034784
		public void UpdateSpring()
		{
			this.modelMass = Bunch.GetModelMass(this);
			if (this.feet[0].joint != null)
			{
				for (int i = 0; i < 2 * this.legPairCount; i++)
				{
					this.SetSpring(this.feet[i].joint);
				}
			}
		}

		// Token: 0x0400058A RID: 1418
		public static ObjectSfxRules globalSfxRules;

		// Token: 0x0400058B RID: 1419
		protected AudioSource voxAudioSource;

		// Token: 0x0400058C RID: 1420
		protected GameObject voxGameObject;

		// Token: 0x0400058D RID: 1421
		public Dictionary<string, Dictionary<string, List<string>>> textureSemanticSfxs = new Dictionary<string, Dictionary<string, List<string>>>();

		// Token: 0x0400058E RID: 1422
		public Dictionary<string, List<string>> connectedBlockTypeSemanticSfxs = new Dictionary<string, List<string>>();

		// Token: 0x0400058F RID: 1423
		public Dictionary<string, List<string>> semanticSfxs = new Dictionary<string, List<string>>();

		// Token: 0x04000590 RID: 1424
		public Dictionary<string, bool> canInterrupts = new Dictionary<string, bool>();

		// Token: 0x04000591 RID: 1425
		public Dictionary<string, int> forbiddenCounters = new Dictionary<string, int>();

		// Token: 0x04000592 RID: 1426
		public Dictionary<string, List<string>> defaultSemanticSfxs = new Dictionary<string, List<string>>();

		// Token: 0x04000593 RID: 1427
		protected float maxSurfaceWalkAngle = 45f;

		// Token: 0x04000594 RID: 1428
		protected float maxSurfaceWalkAngleDot = 0.7f;

		// Token: 0x04000595 RID: 1429
		private int treatAsVehicleStatus = -1;

		// Token: 0x04000596 RID: 1430
		protected bool wacky;

		// Token: 0x04000597 RID: 1431
		protected bool replaceWithCapsuleCollider;

		// Token: 0x04000598 RID: 1432
		protected Vector3 capsuleColliderOffset = Vector3.zero;

		// Token: 0x04000599 RID: 1433
		protected float capsuleColliderHeight = 1f;

		// Token: 0x0400059A RID: 1434
		protected float capsuleColliderRadius = 0.5f;

		// Token: 0x0400059B RID: 1435
		private static PhysicMaterial capsuleColliderMaterial;

		// Token: 0x0400059C RID: 1436
		private bool hasOuchSound;

		// Token: 0x0400059D RID: 1437
		private bool hasWheeSound;

		// Token: 0x0400059E RID: 1438
		private bool hasOhSound;

		// Token: 0x0400059F RID: 1439
		private float ouchPlayProbability = 1f;

		// Token: 0x040005A0 RID: 1440
		private float wheePlayProbability = 0.7f;

		// Token: 0x040005A1 RID: 1441
		private float ohPlayProbability = 0.4f;

		// Token: 0x040005A2 RID: 1442
		public WalkController walkController;

		// Token: 0x040005A3 RID: 1443
		public bool controllerWasActive;

		// Token: 0x040005A4 RID: 1444
		private float previousWackiness;

		// Token: 0x040005A5 RID: 1445
		public float modelMass = 1f;

		// Token: 0x040005A6 RID: 1446
		public float footOffsetY;

		// Token: 0x040005A7 RID: 1447
		protected bool moveCM;

		// Token: 0x040005A8 RID: 1448
		protected float moveCMOffsetFeetCenter = 1f;

		// Token: 0x040005A9 RID: 1449
		protected bool hasMovedCM;

		// Token: 0x040005AA RID: 1450
		protected float moveCMMaxDistance = 1f;

		// Token: 0x040005AB RID: 1451
		protected bool hasChangedInertia;

		// Token: 0x040005AC RID: 1452
		public int legPairCount = 1;

		// Token: 0x040005AD RID: 1453
		public float[] legPairOffsets;

		// Token: 0x040005AE RID: 1454
		public int[][] legPairIndices;

		// Token: 0x040005AF RID: 1455
		public float ankleYSeparation;

		// Token: 0x040005B0 RID: 1456
		public float stepLengthMultiplier = 1f;

		// Token: 0x040005B1 RID: 1457
		public float stepTimeMultiplier = 1f;

		// Token: 0x040005B2 RID: 1458
		protected bool oneAnkleMeshPerFoot;

		// Token: 0x040005B3 RID: 1459
		private bool playCallouts = true;

		// Token: 0x040005B4 RID: 1460
		private LegParameters legParameters;

		// Token: 0x040005B5 RID: 1461
		private static Dictionary<string, int> numLegsOnChunk = new Dictionary<string, int>();

		// Token: 0x040005B6 RID: 1462
		private List<GameObject> ignoreRaycastGOs;

		// Token: 0x040005B7 RID: 1463
		protected float stepSpeedTrigger = 0.4f;

		// Token: 0x040005B8 RID: 1464
		private float stepTime = 0.125f;

		// Token: 0x040005B9 RID: 1465
		private float footWeight = 0.5f;

		// Token: 0x040005BA RID: 1466
		private float ankleOffset = 0.125f;

		// Token: 0x040005BB RID: 1467
		public float maxStepLength = 1f;

		// Token: 0x040005BC RID: 1468
		public float maxStepHeight = 1f;

		// Token: 0x040005BD RID: 1469
		public float onGroundHeight = 0.5f;

		// Token: 0x040005BE RID: 1470
		public GameObject body;

		// Token: 0x040005BF RID: 1471
		public FootInfo[] feet;

		// Token: 0x040005C0 RID: 1472
		protected bool resetFeetPositionsOnCreate;

		// Token: 0x040005C1 RID: 1473
		protected Dictionary<string, string> partNames;

		// Token: 0x040005C2 RID: 1474
		private Vector3 oldLocalScale = Vector3.one;

		// Token: 0x040005C3 RID: 1475
		public float stepTimer;

		// Token: 0x040005C4 RID: 1476
		private int currentLeg;

		// Token: 0x040005C5 RID: 1477
		public bool upright;

		// Token: 0x040005C6 RID: 1478
		public bool unmoving;

		// Token: 0x040005C7 RID: 1479
		public bool footHitGround;

		// Token: 0x040005C8 RID: 1480
		private GameObject groundGO;

		// Token: 0x040005C9 RID: 1481
		public int jumpCountdown;

		// Token: 0x040005CA RID: 1482
		public int startJumpCountdown = 50;

		// Token: 0x040005CB RID: 1483
		private bool wasBroken;

		// Token: 0x040005CC RID: 1484
		private bool onGround = true;

		// Token: 0x040005CD RID: 1485
		private bool idle;

		// Token: 0x040005CE RID: 1486
		public bool ignoreRotation;

		// Token: 0x040005CF RID: 1487
		public bool keepCollider;

		// Token: 0x040005D0 RID: 1488
		private bool multiLegged;

		// Token: 0x040005D1 RID: 1489
		private int legIndex;

		// Token: 0x040005D2 RID: 1490
		private Vector3 vpe = Vector3.zero;

		// Token: 0x040005D3 RID: 1491
		private Vector3 vi = Vector3.zero;

		// Token: 0x040005D4 RID: 1492
		private Vector3 vd = Vector3.zero;

		// Token: 0x040005D5 RID: 1493
		public static int raycastMask = 539;

		// Token: 0x040005D6 RID: 1494
		private Vector3 oldVel = Vector3.zero;

		// Token: 0x040005D7 RID: 1495
		private float wheeness;

		// Token: 0x040005D8 RID: 1496
		private int lpFilterUpdateCounter;

		// Token: 0x040005D9 RID: 1497
		private int noCollisionCounter;

		// Token: 0x040005DA RID: 1498
		private float speakSoundTime;

		// Token: 0x040005DB RID: 1499
		private float speakOuchSoundTime;

		// Token: 0x040005DC RID: 1500
		private float torqueMult = 1f;

		// Token: 0x040005DD RID: 1501
		private float xWidth = 0.25f;

		// Token: 0x040005DE RID: 1502
		public Shader InvisibleShader;

		// Token: 0x040005DF RID: 1503
		private const float IPM_G = 9.82f;

		// Token: 0x040005E0 RID: 1504
		private const float IPM_VEL_FACTOR = 385.729584f;

		// Token: 0x040005E1 RID: 1505
		private const float UPRIGHT_CONTROL_P = 1f;

		// Token: 0x040005E2 RID: 1506
		private const float UPRIGHT_CONTROL_I = 2f;

		// Token: 0x040005E3 RID: 1507
		private const float UPRIGHT_CONTROL_D = 0.03f;

		// Token: 0x040005E4 RID: 1508
		private static Shader invisibleShader;
	}
}
