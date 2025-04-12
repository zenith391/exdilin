using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x020000EC RID: 236
	public abstract class BlockWalkable : Block
	{
		// Token: 0x06001156 RID: 4438 RVA: 0x0004340C File Offset: 0x0004180C
		public BlockWalkable(List<List<Tile>> tiles, int legPairCount = 1, int[][] legPairIndices = null, float ankleYSeparation = 0f) : base(tiles)
		{
			this.InvisibleShader = Shader.Find("Blocksworld/Invisible");
			this.legPairCount = legPairCount;
			this.ankleYSeparation = ankleYSeparation;
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
			this.unmoving = true;
		}

		// Token: 0x06001157 RID: 4439 RVA: 0x000435E0 File Offset: 0x000419E0
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

		// Token: 0x06001158 RID: 4440 RVA: 0x00043634 File Offset: 0x00041A34
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

		// Token: 0x06001159 RID: 4441 RVA: 0x000436D0 File Offset: 0x00041AD0
		private void PlayOhSound(float vol)
		{
			this.PlaySemanticSound("Oh", vol, this.ohPlayProbability);
		}

		// Token: 0x0600115A RID: 4442 RVA: 0x000436E5 File Offset: 0x00041AE5
		private void PlayWheeSound(float vol)
		{
			this.PlaySemanticSound("Whee", vol, this.wheePlayProbability);
		}

		// Token: 0x0600115B RID: 4443 RVA: 0x000436FA File Offset: 0x00041AFA
		public void PlayOuchSound(float vol, bool always = false)
		{
			if (this.PlaySemanticSound("Ouch", vol, (!always) ? this.ouchPlayProbability : 1f))
			{
				this.speakOuchSoundTime = Time.time;
			}
		}

		// Token: 0x0600115C RID: 4444 RVA: 0x00043730 File Offset: 0x00041B30
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

		// Token: 0x0600115D RID: 4445 RVA: 0x00043884 File Offset: 0x00041C84
		public override void Mute()
		{
			base.Mute();
			if (this.voxAudioSource.isPlaying)
			{
				this.voxAudioSource.Stop();
			}
		}

		// Token: 0x0600115E RID: 4446 RVA: 0x000438A8 File Offset: 0x00041CA8
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
					if (time - this.speakSoundTime > 1.5f && this.wheeness > 25f)
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
					if (!this.unmoving && time - this.speakSoundTime > 3f)
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

		// Token: 0x0600115F RID: 4447 RVA: 0x00043AB0 File Offset: 0x00041EB0
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

		// Token: 0x06001160 RID: 4448 RVA: 0x00043B5C File Offset: 0x00041F5C
		public LegParameters GetLegParameters()
		{
			LegParameters component = this.go.GetComponent<LegParameters>();
			if (component == null)
			{
				BWLog.Info("No parameters found for legs");
			}
			return component;
		}

		// Token: 0x06001161 RID: 4449 RVA: 0x00043B8C File Offset: 0x00041F8C
		public virtual GameObject GetHandAttach(int hand)
		{
			if (hand < 0 || hand > 1)
			{
				return null;
			}
			return this.hands[hand];
		}

		// Token: 0x06001162 RID: 4450 RVA: 0x00043BA8 File Offset: 0x00041FA8
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

		// Token: 0x06001163 RID: 4451 RVA: 0x00043C7C File Offset: 0x0004207C
		public override void Play()
		{
			base.Play();
			this.body = this.goT.parent.gameObject;
			this.onGround = true;
			this.upright = true;
			this.walkController = new WalkControllerAnimated(this);
			this.controllerWasActive = false;
			this.modelMass = Bunch.GetModelMass(this);
			this.ignoreRotation = (this.ChunkContainsJoint() && this.modelMass > 10f);
			this.walkController.SetChunk();
			this.legParameters = this.GetLegParameters();
			if (this.legParameters != null)
			{
				this.maxSurfaceWalkAngle = this.legParameters.maxSurfaceWalkAngle;
			}
			this.maxSurfaceWalkAngleDot = Mathf.Cos(0.0174532924f * this.maxSurfaceWalkAngle);
			this.CreateVocalAudioSource();
			if (this.legParameters != null)
			{
				this.ohPlayProbability = this.legParameters.ohPlayProbability;
				this.wheePlayProbability = this.legParameters.wheePlayProbability;
				this.ouchPlayProbability = this.legParameters.ouchPlayProbability;
			}
			if (!this.unmoving)
			{
				this.ReplaceColliderWithCapsule();
			}
			this.GetCharacterSfxs();
			this.SetSfxs(base.GetTexture(0), 0);
		}

		// Token: 0x06001164 RID: 4452 RVA: 0x00043DB4 File Offset: 0x000421B4
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

		// Token: 0x06001165 RID: 4453 RVA: 0x00043E20 File Offset: 0x00042220
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

		// Token: 0x06001166 RID: 4454 RVA: 0x00043FC8 File Offset: 0x000423C8
		public override void Break(Vector3 chunkPos, Vector3 chunkVel, Vector3 chunkAngVel)
		{
			base.Break(chunkPos, chunkVel, chunkAngVel);
			if (!this.playCallouts)
			{
				this.PlayOuchSound(1f, true);
			}
			this.MakeUnmoving();
		}

		// Token: 0x06001167 RID: 4455 RVA: 0x00043FF0 File Offset: 0x000423F0
		public override void Stop(bool resetBlock = true)
		{
			base.Stop(resetBlock);
			if (this.voxAudioSource != null)
			{
				this.voxAudioSource.Stop();
			}
			this.MakeUnmoving();
			this.body = null;
			this.controllerWasActive = false;
			this.isHovering = false;
			this.groundGO = null;
		}

		// Token: 0x06001168 RID: 4456 RVA: 0x00044044 File Offset: 0x00042444
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

		// Token: 0x06001169 RID: 4457 RVA: 0x000440BC File Offset: 0x000424BC
		protected bool ChunkContainsJoint()
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

		// Token: 0x0600116A RID: 4458 RVA: 0x0004411C File Offset: 0x0004251C
		public virtual void GatherIgnoreColliders(HashSet<Collider> ignoreColliders)
		{
			for (int i = 0; i < 2 * this.legPairCount; i++)
			{
				if (this.feet[i].collider != null)
				{
					ignoreColliders.Add(this.feet[i].collider);
				}
			}
			if (this.body != null)
			{
				foreach (Transform transform in this.body.GetComponentsInChildren<Transform>())
				{
					Collider component = transform.gameObject.GetComponent<Collider>();
					if (component != null)
					{
						ignoreColliders.Add(component);
					}
				}
			}
		}

		// Token: 0x0600116B RID: 4459 RVA: 0x000441C6 File Offset: 0x000425C6
		public static void InitPlay()
		{
		}

		// Token: 0x0600116C RID: 4460 RVA: 0x000441C8 File Offset: 0x000425C8
		protected virtual Vector3 GetFeetCenter()
		{
			Vector3 vector = Vector3.zero;
			foreach (FootInfo footInfo in this.feet)
			{
				if (footInfo.go != null)
				{
					vector += footInfo.go.transform.position / (float)this.feet.Length;
				}
			}
			return vector;
		}

		// Token: 0x0600116D RID: 4461 RVA: 0x00044234 File Offset: 0x00042634
		protected virtual void UpdateBodyIfNecessary()
		{
			if (this.chunk == null)
			{
				return;
			}
			if (this.body == null)
			{
				this.body = this.chunk.go;
				Vector3 centerOfMass = this.body.GetComponent<Rigidbody>().centerOfMass;
				centerOfMass.y -= 0.5f;
				this.body.GetComponent<Rigidbody>().centerOfMass = centerOfMass;
			}
		}

		// Token: 0x0600116E RID: 4462 RVA: 0x000442A4 File Offset: 0x000426A4
		public Rigidbody GetRigidBody()
		{
			this.UpdateBodyIfNecessary();
			return (!(this.body != null)) ? null : this.body.GetComponent<Rigidbody>();
		}

		// Token: 0x0600116F RID: 4463 RVA: 0x000442D0 File Offset: 0x000426D0
		public bool IsOnGround()
		{
			if (this.walkController != null)
			{
				return this.walkController.IsOnGround();
			}
			Collider component = this.goT.GetComponent<Collider>();
			Vector3 origin = component.bounds.center + Vector3.down * component.bounds.extents.y * 0.9f;
			bool flag = this.go.IsLayer(Layer.IgnoreRaycast);
			this.IgnoreRaycasts(true);
			RaycastHit raycastHit;
			bool flag2 = Physics.Raycast(origin, Vector3.down, out raycastHit, component.bounds.extents.y * 0.2f, BlockWalkable.raycastMask);
			if (flag2 && raycastHit.collider != null)
			{
				flag2 = !raycastHit.collider.isTrigger;
			}
			if (!flag)
			{
				this.IgnoreRaycasts(false);
			}
			return flag2;
		}

		// Token: 0x06001170 RID: 4464 RVA: 0x000443C0 File Offset: 0x000427C0
		public float NearGround(float maxDist = 1f)
		{
			if (this.walkController == null)
			{
				return -1f;
			}
			if (this.walkController.OnGroundHeight() <= maxDist)
			{
				return this.walkController.OnGroundHeight();
			}
			return -1f;
		}

		// Token: 0x06001171 RID: 4465 RVA: 0x000443F5 File Offset: 0x000427F5
		public bool OnMovingObject()
		{
			return this.walkController.onMovingObject;
		}

		// Token: 0x06001172 RID: 4466 RVA: 0x00044402 File Offset: 0x00042802
		public bool GetThinksOnGround()
		{
			return this.onGround;
		}

		// Token: 0x06001173 RID: 4467 RVA: 0x0004440A File Offset: 0x0004280A
		private void MakeMoving()
		{
			if (!this.unmoving)
			{
				return;
			}
			this.ReplaceColliderWithCapsule();
			this.unmoving = false;
			if (this.chunk != null)
			{
				this.chunk.UpdateCenterOfMass(true);
			}
		}

		// Token: 0x06001174 RID: 4468 RVA: 0x0004443C File Offset: 0x0004283C
		protected virtual void MakeUnmoving()
		{
			this.RestoreOrigBoxCollider();
			this.unmoving = true;
			if (this.chunk != null)
			{
				this.chunk.UpdateCenterOfMass(true);
			}
		}

		// Token: 0x06001175 RID: 4469 RVA: 0x00044464 File Offset: 0x00042864
		protected void ReplaceColliderWithCapsule()
		{
			if (BlockWalkable.capsuleColliderMaterial == null)
			{
				BlockWalkable.capsuleColliderMaterial = new PhysicMaterial();
				BlockWalkable.capsuleColliderMaterial.dynamicFriction = 0.3f;
				BlockWalkable.capsuleColliderMaterial.staticFriction = 0.3f;
				BlockWalkable.capsuleColliderMaterial.frictionCombine = PhysicMaterialCombine.Minimum;
			}
			BoxCollider component = this.go.GetComponent<BoxCollider>();
			if (component != null)
			{
				if (this.origBoxColliderData == null)
				{
					this.origBoxColliderData = new BoxColliderData(component);
				}
				UnityEngine.Object.Destroy(component);
			}
			CapsuleCollider component2 = this.go.GetComponent<CapsuleCollider>();
			if (component2 == null)
			{
				CapsuleCollider capsuleCollider = this.go.AddComponent<CapsuleCollider>();
				capsuleCollider.material = BlockWalkable.capsuleColliderMaterial;
				capsuleCollider.height = this.capsuleColliderHeight;
				capsuleCollider.center = this.capsuleColliderOffset;
				capsuleCollider.radius = this.capsuleColliderRadius;
				capsuleCollider.enabled = false;
				capsuleCollider.enabled = true;
				this.walkController.SetCapsuleCollider(capsuleCollider);
				this.walkController.AddIgnoreCollider(capsuleCollider);
			}
			else
			{
				component2.enabled = true;
			}
		}

		// Token: 0x06001176 RID: 4470 RVA: 0x00044570 File Offset: 0x00042970
		protected void RestoreOrigBoxCollider()
		{
			if (this.origBoxColliderData != null)
			{
				CapsuleCollider component = this.go.GetComponent<CapsuleCollider>();
				if (component != null)
				{
					UnityEngine.Object.Destroy(component);
				}
				BoxCollider boxCollider = this.go.GetComponent<BoxCollider>();
				if (boxCollider == null)
				{
					boxCollider = this.go.AddComponent<BoxCollider>();
				}
				boxCollider.center = this.origBoxColliderData.center;
				boxCollider.size = this.origBoxColliderData.size;
			}
		}

		// Token: 0x06001177 RID: 4471 RVA: 0x000445EC File Offset: 0x000429EC
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
			if (Blocksworld.CurrentState == State.Play)
			{
				this.UpdateSounds();
			}
			if (this.unmoving)
			{
				return;
			}
			this.UpdateBodyIfNecessary();
			this.upright = (this.goT.up.y > 0.5f);
			Rigidbody component = this.body.GetComponent<Rigidbody>();
			bool flag = this.walkController != null && this.walkController.IsActive() && !this.didFix;
			if (!flag)
			{
				flag = true;
				this.walkController.Translate(Vector3.forward, 0f);
			}
			Vector3 vector = Vector3.zero;
			if (flag)
			{
				this.walkController.FixedUpdate();
				vector = this.walkController.GetRigidBodyBelowVelocity();
			}
			this.controllerWasActive = flag;
			if (this.lpFilterUpdateCounter % 5 == 0)
			{
				base.UpdateWithinWaterLPFilter(null);
				base.UpdateWithinWaterLPFilter(this.voxGameObject);
			}
			this.lpFilterUpdateCounter++;
			this.jumpCountdown--;
			this.idle = false;
		}

		// Token: 0x06001178 RID: 4472 RVA: 0x0004475E File Offset: 0x00042B5E
		public Shader GetInvisibleShader()
		{
			BlockWalkable.invisibleShader = ((!(BlockWalkable.invisibleShader != null)) ? Shader.Find("Blocksworld/Invisible") : BlockWalkable.invisibleShader);
			return BlockWalkable.invisibleShader;
		}

		// Token: 0x06001179 RID: 4473 RVA: 0x0004478E File Offset: 0x00042B8E
		public TileResultCode IsWalkingSensor(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return ((float)args[0] >= 0f) ? this.IsWalking() : this.IsBacking();
		}

		// Token: 0x0600117A RID: 4474 RVA: 0x000447B4 File Offset: 0x00042BB4
		public TileResultCode IsWalking()
		{
			return (this.broken || this.unmoving || Vector3.Dot(this.body.GetComponent<Rigidbody>().velocity, this.goT.forward) < 0f) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x0600117B RID: 4475 RVA: 0x00044808 File Offset: 0x00042C08
		public TileResultCode IsBacking()
		{
			return (this.broken || this.unmoving || Vector3.Dot(this.body.GetComponent<Rigidbody>().velocity, this.goT.forward) >= 0f) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x0600117C RID: 4476 RVA: 0x0004485C File Offset: 0x00042C5C
		public TileResultCode IsTurning(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return ((float)args[0] >= 0f) ? this.IsTurningRight() : this.IsTurningLeft();
		}

		// Token: 0x0600117D RID: 4477 RVA: 0x00044884 File Offset: 0x00042C84
		public TileResultCode IsTurningLeft()
		{
			return (this.broken || this.unmoving || this.body.GetComponent<Rigidbody>().angularVelocity.y >= 0f) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x0600117E RID: 4478 RVA: 0x000448D0 File Offset: 0x00042CD0
		public TileResultCode IsTurningRight()
		{
			return (this.broken || this.unmoving || this.body.GetComponent<Rigidbody>().angularVelocity.y <= 0f) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x0600117F RID: 4479 RVA: 0x0004491C File Offset: 0x00042D1C
		public TileResultCode IsJumping(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!this.IsOnGround()) ? TileResultCode.True : TileResultCode.False;
		}

		// Token: 0x06001180 RID: 4480 RVA: 0x00044930 File Offset: 0x00042D30
		public TileResultCode TurnTowardsTap(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.broken || this.unmoving || this.vanished)
			{
				return TileResultCode.True;
			}
			this.walkController.TurnTowardsTap();
			return TileResultCode.True;
		}

		// Token: 0x06001181 RID: 4481 RVA: 0x00044961 File Offset: 0x00042D61
		public TileResultCode TurnAlongCam(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.broken || this.unmoving || this.vanished)
			{
				return TileResultCode.True;
			}
			this.walkController.TurnAlongCamera();
			return TileResultCode.True;
		}

		// Token: 0x06001182 RID: 4482 RVA: 0x00044994 File Offset: 0x00042D94
		public TileResultCode Jump(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (!this.broken && !this.unmoving && !this.vanished)
			{
				float force = (float)args[0] * eInfo.floatArg;
				this.walkController.Jump(force);
			}
			return TileResultCode.True;
		}

		// Token: 0x06001183 RID: 4483 RVA: 0x000449E0 File Offset: 0x00042DE0
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

		// Token: 0x06001184 RID: 4484 RVA: 0x00044A1C File Offset: 0x00042E1C
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

		// Token: 0x06001185 RID: 4485 RVA: 0x00044A94 File Offset: 0x00042E94
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

		// Token: 0x06001186 RID: 4486 RVA: 0x00044B1C File Offset: 0x00042F1C
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

		// Token: 0x06001187 RID: 4487 RVA: 0x00044B60 File Offset: 0x00042F60
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

		// Token: 0x06001188 RID: 4488 RVA: 0x00044BF4 File Offset: 0x00042FF4
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

		// Token: 0x06001189 RID: 4489 RVA: 0x00044C3A File Offset: 0x0004303A
		public TileResultCode Idle(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.idle = true;
			return TileResultCode.True;
		}

		// Token: 0x0600118A RID: 4490 RVA: 0x00044C44 File Offset: 0x00043044
		public TileResultCode DPadControl(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.walkController != null && !this.vanished)
			{
				BlockAnimatedCharacter blockAnimatedCharacter = this as BlockAnimatedCharacter;
				if (!blockAnimatedCharacter.stateHandler.IsImmobile() && !blockAnimatedCharacter.didFix)
				{
					string key = (args.Length <= 0) ? "L" : ((string)args[0]);
					float maxSpeed = (args.Length <= 1) ? this.walkController.defaultDPadMaxSpeed : ((float)args[1]);
					Blocksworld.UI.Controls.EnableDPad(key, MoverDirectionMask.ALL);
					Blocksworld.worldSessionHadBlocksterMover = true;
					this.walkController.DPadControl(key, maxSpeed);
				}
			}
			return TileResultCode.True;
		}

		// Token: 0x0600118B RID: 4491 RVA: 0x00044CED File Offset: 0x000430ED
		protected override void HandleTiltMover(float xTilt, float yTilt, float zTilt)
		{
			if (this.walkController != null && !this.vanished)
			{
				this.walkController.TiltMoverControl(new Vector2(xTilt, yTilt));
			}
		}

		// Token: 0x0600118C RID: 4492 RVA: 0x00044D18 File Offset: 0x00043118
		public TileResultCode GotoTap(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.walkController != null && !this.vanished)
			{
				float maxSpeed = (args.Length <= 0) ? this.walkController.defaultMaxSpeed : ((float)args[0]);
				this.walkController.GotoTap(maxSpeed);
			}
			return TileResultCode.True;
		}

		// Token: 0x0600118D RID: 4493 RVA: 0x00044D6C File Offset: 0x0004316C
		public TileResultCode GotoTag(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.walkController == null || this.vanished)
			{
				return TileResultCode.True;
			}
			string tagName = (args.Length <= 0) ? string.Empty : ((string)args[0]);
			float maxSpeed = (args.Length <= 1) ? this.walkController.defaultMaxSpeed : ((float)args[1]);
			bool flag = this.walkController.GotoTag(tagName, maxSpeed);
			if (flag)
			{
				return TileResultCode.Delayed;
			}
			return TileResultCode.True;
		}

		// Token: 0x0600118E RID: 4494 RVA: 0x00044DE8 File Offset: 0x000431E8
		public TileResultCode ChaseTag(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.walkController != null && !this.vanished)
			{
				string tagName = (args.Length <= 0) ? string.Empty : ((string)args[0]);
				float maxSpeed = (args.Length <= 1) ? this.walkController.defaultMaxSpeed : ((float)args[1]);
				this.walkController.ChaseTag(tagName, maxSpeed);
			}
			return TileResultCode.True;
		}

		// Token: 0x0600118F RID: 4495 RVA: 0x00044E58 File Offset: 0x00043258
		public float Ipm(float v, float h)
		{
			float value = v * Mathf.Sqrt(h / 9.82f + v * v / 385.729584f);
			return Mathf.Clamp(value, -this.maxStepLength, this.maxStepLength);
		}

		// Token: 0x04000D95 RID: 3477
		public WalkControllerAnimated walkController;

		// Token: 0x04000D96 RID: 3478
		public bool controllerWasActive;

		// Token: 0x04000D97 RID: 3479
		public bool isHovering;

		// Token: 0x04000D98 RID: 3480
		private int noCollisionCounter;

		// Token: 0x04000D99 RID: 3481
		private float speakSoundTime;

		// Token: 0x04000D9A RID: 3482
		private float speakOuchSoundTime;

		// Token: 0x04000D9B RID: 3483
		protected Vector3 handPos = new Vector3(0.5f, -0.5f, 0.4f);

		// Token: 0x04000D9C RID: 3484
		protected float handsOutXMod = 0.7f;

		// Token: 0x04000D9D RID: 3485
		protected float handsOutYMod = -0.1f;

		// Token: 0x04000D9E RID: 3486
		public GameObject[] hands = new GameObject[2];

		// Token: 0x04000D9F RID: 3487
		public GameObject middle;

		// Token: 0x04000DA0 RID: 3488
		public GameObject head;

		// Token: 0x04000DA1 RID: 3489
		protected ConfigurableJoint middleJoint;

		// Token: 0x04000DA2 RID: 3490
		protected Vector3 middleLocalPosition;

		// Token: 0x04000DA3 RID: 3491
		public GameObject body;

		// Token: 0x04000DA4 RID: 3492
		protected GameObject groundGO;

		// Token: 0x04000DA5 RID: 3493
		public float modelMass = 1f;

		// Token: 0x04000DA6 RID: 3494
		private bool wasBroken;

		// Token: 0x04000DA7 RID: 3495
		public bool playCallouts;

		// Token: 0x04000DA8 RID: 3496
		public FootInfo[] feet;

		// Token: 0x04000DA9 RID: 3497
		public float[] legPairOffsets;

		// Token: 0x04000DAA RID: 3498
		public int[][] legPairIndices;

		// Token: 0x04000DAB RID: 3499
		public float ankleYSeparation;

		// Token: 0x04000DAC RID: 3500
		protected float stepSpeedTrigger = 0.4f;

		// Token: 0x04000DAD RID: 3501
		protected float stepTime = 0.125f;

		// Token: 0x04000DAE RID: 3502
		protected float footWeight = 0.5f;

		// Token: 0x04000DAF RID: 3503
		protected float ankleOffset = 0.125f;

		// Token: 0x04000DB0 RID: 3504
		public float maxStepLength = 1f;

		// Token: 0x04000DB1 RID: 3505
		public float maxStepHeight = 1f;

		// Token: 0x04000DB2 RID: 3506
		private Vector3 oldVel = Vector3.zero;

		// Token: 0x04000DB3 RID: 3507
		private float wheeness;

		// Token: 0x04000DB4 RID: 3508
		private int lpFilterUpdateCounter;

		// Token: 0x04000DB5 RID: 3509
		protected Vector3 oldLocalScale = Vector3.one;

		// Token: 0x04000DB6 RID: 3510
		protected bool resetFeetPositionsOnCreate;

		// Token: 0x04000DB7 RID: 3511
		public int legPairCount = 1;

		// Token: 0x04000DB8 RID: 3512
		public int jumpCountdown;

		// Token: 0x04000DB9 RID: 3513
		public int startJumpCountdown = 50;

		// Token: 0x04000DBA RID: 3514
		public float onGroundHeight = 1f;

		// Token: 0x04000DBB RID: 3515
		public bool ignoreRotation;

		// Token: 0x04000DBC RID: 3516
		protected bool idle;

		// Token: 0x04000DBD RID: 3517
		public bool upright;

		// Token: 0x04000DBE RID: 3518
		public bool unmoving;

		// Token: 0x04000DBF RID: 3519
		protected bool onGround = true;

		// Token: 0x04000DC0 RID: 3520
		protected float maxSurfaceWalkAngle = 45f;

		// Token: 0x04000DC1 RID: 3521
		protected float maxSurfaceWalkAngleDot = 0.7f;

		// Token: 0x04000DC2 RID: 3522
		protected int treatAsVehicleStatus = -1;

		// Token: 0x04000DC3 RID: 3523
		protected bool hasOuchSound;

		// Token: 0x04000DC4 RID: 3524
		protected bool hasWheeSound;

		// Token: 0x04000DC5 RID: 3525
		protected bool hasOhSound;

		// Token: 0x04000DC6 RID: 3526
		protected float ouchPlayProbability = 1f;

		// Token: 0x04000DC7 RID: 3527
		protected float wheePlayProbability = 0.7f;

		// Token: 0x04000DC8 RID: 3528
		protected float ohPlayProbability = 0.4f;

		// Token: 0x04000DC9 RID: 3529
		protected LegParameters legParameters;

		// Token: 0x04000DCA RID: 3530
		protected List<GameObject> ignoreRaycastGOs;

		// Token: 0x04000DCB RID: 3531
		protected bool moveCM;

		// Token: 0x04000DCC RID: 3532
		protected float moveCMOffsetFeetCenter = 1f;

		// Token: 0x04000DCD RID: 3533
		protected float moveCMMaxDistance = 1f;

		// Token: 0x04000DCE RID: 3534
		protected Vector3 capsuleColliderOffset = Vector3.zero;

		// Token: 0x04000DCF RID: 3535
		protected float capsuleColliderHeight = 1f;

		// Token: 0x04000DD0 RID: 3536
		protected float capsuleColliderRadius = 0.5f;

		// Token: 0x04000DD1 RID: 3537
		private static PhysicMaterial capsuleColliderMaterial;

		// Token: 0x04000DD2 RID: 3538
		public Shader InvisibleShader;

		// Token: 0x04000DD3 RID: 3539
		public static int raycastMask = 539;

		// Token: 0x04000DD4 RID: 3540
		protected AudioSource voxAudioSource;

		// Token: 0x04000DD5 RID: 3541
		protected GameObject voxGameObject;

		// Token: 0x04000DD6 RID: 3542
		public Dictionary<string, Dictionary<string, List<string>>> textureSemanticSfxs = new Dictionary<string, Dictionary<string, List<string>>>();

		// Token: 0x04000DD7 RID: 3543
		public Dictionary<string, List<string>> connectedBlockTypeSemanticSfxs = new Dictionary<string, List<string>>();

		// Token: 0x04000DD8 RID: 3544
		public Dictionary<string, List<string>> semanticSfxs = new Dictionary<string, List<string>>();

		// Token: 0x04000DD9 RID: 3545
		public Dictionary<string, bool> canInterrupts = new Dictionary<string, bool>();

		// Token: 0x04000DDA RID: 3546
		public Dictionary<string, int> forbiddenCounters = new Dictionary<string, int>();

		// Token: 0x04000DDB RID: 3547
		public Dictionary<string, List<string>> defaultSemanticSfxs = new Dictionary<string, List<string>>();

		// Token: 0x04000DDC RID: 3548
		private BoxColliderData origBoxColliderData;

		// Token: 0x04000DDD RID: 3549
		protected static Shader invisibleShader;

		// Token: 0x04000DDE RID: 3550
		private const float IPM_G = 9.82f;

		// Token: 0x04000DDF RID: 3551
		private const float IPM_VEL_FACTOR = 385.729584f;
	}
}
