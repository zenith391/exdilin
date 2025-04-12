using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x0200004F RID: 79
	public abstract class BlockAbstractBow : Block
	{
		// Token: 0x0600069D RID: 1693 RVA: 0x0002D56D File Offset: 0x0002B96D
		public BlockAbstractBow(List<List<Tile>> tiles) : base(tiles)
		{
		}

		// Token: 0x0600069E RID: 1694
		protected abstract Vector3 GetAmmoPositionOffset();

		// Token: 0x0600069F RID: 1695
		protected abstract string GetAmmoPrefabName();

		// Token: 0x060006A0 RID: 1696 RVA: 0x0002D58C File Offset: 0x0002B98C
		protected virtual int GetAmmoSubmeshColorMapping(int i)
		{
			return -1;
		}

		// Token: 0x060006A1 RID: 1697 RVA: 0x0002D58F File Offset: 0x0002B98F
		protected virtual Vector3 GetFiringDirection()
		{
			return -this.goT.right;
		}

		// Token: 0x060006A2 RID: 1698
		protected abstract float GetMinTimeBetweenShots();

		// Token: 0x060006A3 RID: 1699 RVA: 0x0002D5A1 File Offset: 0x0002B9A1
		protected virtual bool ShotsObeyGravity()
		{
			return true;
		}

		// Token: 0x060006A4 RID: 1700
		protected abstract string GetSFXForBlocked();

		// Token: 0x060006A5 RID: 1701
		protected abstract string GetSFXForHit();

		// Token: 0x060006A6 RID: 1702 RVA: 0x0002D5A4 File Offset: 0x0002B9A4
		protected virtual string GetSFXForHitWater()
		{
			return "Water Splash Medium " + UnityEngine.Random.Range(1, 4);
		}

		// Token: 0x060006A7 RID: 1703
		protected abstract string GetSFXForLoad();

		// Token: 0x060006A8 RID: 1704
		protected abstract string GetSFXForShoot();

		// Token: 0x060006A9 RID: 1705 RVA: 0x0002D5BC File Offset: 0x0002B9BC
		protected virtual bool HasSFXForLoad()
		{
			return true;
		}

		// Token: 0x060006AA RID: 1706 RVA: 0x0002D5BF File Offset: 0x0002B9BF
		protected virtual void BowLoaded()
		{
		}

		// Token: 0x060006AB RID: 1707 RVA: 0x0002D5C1 File Offset: 0x0002B9C1
		protected virtual void BowShot()
		{
		}

		// Token: 0x060006AC RID: 1708 RVA: 0x0002D5C4 File Offset: 0x0002B9C4
		private void DestroyArrows()
		{
			for (int i = 0; i < this._arrows.Count; i++)
			{
				this._arrows[i].Destroy();
			}
			this._arrows.Clear();
		}

		// Token: 0x060006AD RID: 1709 RVA: 0x0002D609 File Offset: 0x0002BA09
		public override void Destroy()
		{
			this.DestroyArrows();
			base.Destroy();
		}

		// Token: 0x060006AE RID: 1710 RVA: 0x0002D618 File Offset: 0x0002BA18
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			for (int i = 0; i < this._arrows.Count; i++)
			{
				this._arrows[i].FixedUpdate();
			}
			if (this._timeBetweenShots > 0f)
			{
				this._timeBetweenShots -= Time.fixedDeltaTime;
			}
			if (this._shotSignaled && this._timeBetweenShots <= 0f)
			{
				Vector3 direction = this.GetFiringDirection();
				BlockAnimatedCharacter blockAnimatedCharacter = BlockAnimatedCharacter.FindBlockOwner(this);
				if (blockAnimatedCharacter != null && Vector3.Dot(blockAnimatedCharacter.goT.up, Vector3.up) > 0.95f)
				{
					direction.y = 0f;
					Vector3 b = (!this.ShotsObeyGravity()) ? Vector3.zero : (-0.005f * Physics.gravity);
					direction = (direction.normalized + b).normalized;
				}
				this._arrows[0].Shoot(direction, this._shotForce);
				this._shotSignaled = false;
				this._timeBetweenShots = this.GetMinTimeBetweenShots();
				base.PlayPositionedSound(this.GetSFXForShoot(), 0.4f, 1f);
				this.BowShot();
			}
			for (int j = this._arrows.Count - 1; j >= 0; j--)
			{
				if (this._arrows[j].IsReadyForCleanup())
				{
					this._arrows[j].Destroy();
					this._arrows.RemoveAt(j);
					break;
				}
			}
		}

		// Token: 0x060006AF RID: 1711 RVA: 0x0002D7B4 File Offset: 0x0002BBB4
		public override void Stop(bool resetBlock = true)
		{
			base.Stop(resetBlock);
			this.DestroyArrows();
		}

		// Token: 0x060006B0 RID: 1712 RVA: 0x0002D7C3 File Offset: 0x0002BBC3
		public TileResultCode IsLoaded(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this._arrows.Count > 0 && this._arrows[0].IsReadyToShoot())
			{
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x060006B1 RID: 1713 RVA: 0x0002D7F0 File Offset: 0x0002BBF0
		public TileResultCode Load(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this._timeBetweenShots > 0f)
			{
				return TileResultCode.Delayed;
			}
			if (this._arrows.Count == 0 || !this._arrows[0].IsReadyToShoot())
			{
				this._arrows.Insert(0, new BlockAbstractBow.BlockBowArrow(this));
				if (this.HasSFXForLoad())
				{
					base.PlayPositionedSound(this.GetSFXForLoad(), 0.4f, 1f);
				}
				this.BowLoaded();
			}
			return TileResultCode.True;
		}

		// Token: 0x060006B2 RID: 1714 RVA: 0x0002D870 File Offset: 0x0002BC70
		public TileResultCode Shoot(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this._timeBetweenShots > 0f)
			{
				return TileResultCode.True;
			}
			if (this.broken || this.vanished || this._arrows.Count == 0 || !this._arrows[0].IsReadyToShoot())
			{
				return TileResultCode.False;
			}
			this._shotForce = Util.GetFloatArg(args, 0, 50f);
			this._shotSignaled = true;
			return TileResultCode.Delayed;
		}

		// Token: 0x060006B3 RID: 1715 RVA: 0x0002D8E7 File Offset: 0x0002BCE7
		internal Vector3 LoadingArrowPosition()
		{
			return this.goT.position;
		}

		// Token: 0x060006B4 RID: 1716 RVA: 0x0002D8F4 File Offset: 0x0002BCF4
		internal Quaternion LoadingArrowRotation()
		{
			return this.goT.rotation;
		}

		// Token: 0x060006B5 RID: 1717 RVA: 0x0002D901 File Offset: 0x0002BD01
		internal static void ClearHits()
		{
			BlockAbstractBow.Hits.Clear();
			BlockAbstractBow.ModelHits.Clear();
			BlockAbstractBow.ClearSets(BlockAbstractBow.TagHits);
			BlockAbstractBow.ClearSets(BlockAbstractBow.ModelTagHits);
		}

		// Token: 0x060006B6 RID: 1718 RVA: 0x0002D92C File Offset: 0x0002BD2C
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

		// Token: 0x060006B7 RID: 1719 RVA: 0x0002D9B8 File Offset: 0x0002BDB8
		internal bool ArrowHit(Block block, Vector3 position, Vector3 direction)
		{
			if (block != null)
			{
				BlockAbstractBow.Hits.Add(block);
				bool flag = block.CanTriggerBlockListSensor();
				if (flag)
				{
					BlockAbstractBow.ModelHits.Add(block.modelBlock);
				}
				List<string> blockTags = TagManager.GetBlockTags(this);
				for (int i = 0; i < blockTags.Count; i++)
				{
					string key = blockTags[i];
					HashSet<Block> hashSet;
					HashSet<Block> hashSet2;
					if (BlockAbstractBow.TagHits.ContainsKey(key))
					{
						hashSet = BlockAbstractBow.TagHits[key];
						hashSet2 = BlockAbstractBow.ModelTagHits[key];
					}
					else
					{
						hashSet = new HashSet<Block>();
						hashSet2 = new HashSet<Block>();
						BlockAbstractBow.TagHits.Add(key, hashSet);
						BlockAbstractBow.ModelTagHits.Add(key, hashSet2);
					}
					hashSet.Add(block);
					if (flag)
					{
						hashSet2.Add(block.modelBlock);
					}
				}
				if (block.chunk.rb != null)
				{
					block.chunk.rb.AddForceAtPosition(0.1f * this._shotForce * direction, position, ForceMode.Impulse);
				}
				bool flag2 = Invincibility.IsInvincible(block);
				bool flag3 = block is BlockAbstractWater;
				if (flag2)
				{
					block.PlayPositionedSound(this.GetSFXForBlocked(), 0.2f, 1f);
				}
				else if (flag3)
				{
					block.PlayPositionedSound(this.GetSFXForHitWater(), 0.2f, 1f);
				}
				else
				{
					block.PlayPositionedSound(this.GetSFXForHit(), 0.2f, 1f);
				}
				return !flag2;
			}
			return false;
		}

		// Token: 0x060006B8 RID: 1720 RVA: 0x0002DB43 File Offset: 0x0002BF43
		internal static bool IsHitByArrow(Block block)
		{
			return BlockAbstractBow.Hits.Contains(block);
		}

		// Token: 0x060006B9 RID: 1721 RVA: 0x0002DB50 File Offset: 0x0002BF50
		internal static bool IsHitByArrow(List<Block> blocks)
		{
			for (int i = 0; i < blocks.Count; i++)
			{
				if (BlockAbstractBow.IsHitByArrow(blocks[i]))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060006BA RID: 1722 RVA: 0x0002DB88 File Offset: 0x0002BF88
		internal static bool IsHitByArrowModel(Block modelBlock)
		{
			return BlockAbstractBow.ModelHits.Contains(modelBlock);
		}

		// Token: 0x060006BB RID: 1723 RVA: 0x0002DB98 File Offset: 0x0002BF98
		internal static bool IsHitByTaggedArrow(Block block, string tag)
		{
			HashSet<Block> hashSet;
			return BlockAbstractBow.TagHits.TryGetValue(tag, out hashSet) && hashSet.Contains(block);
		}

		// Token: 0x060006BC RID: 1724 RVA: 0x0002DBC0 File Offset: 0x0002BFC0
		internal static bool IsHitByTaggedArrow(List<Block> blocks, string tag)
		{
			HashSet<Block> hashSet;
			if (BlockAbstractBow.TagHits.TryGetValue(tag, out hashSet))
			{
				for (int i = 0; i < blocks.Count; i++)
				{
					if (hashSet.Contains(blocks[i]))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x060006BD RID: 1725 RVA: 0x0002DC0C File Offset: 0x0002C00C
		internal static bool IsHitByTaggedArrowModel(Block modelBlock, string tag)
		{
			HashSet<Block> hashSet;
			return BlockAbstractBow.ModelTagHits.TryGetValue(tag, out hashSet) && hashSet.Contains(modelBlock);
		}

		// Token: 0x040004D7 RID: 1239
		public static HashSet<Block> Hits = new HashSet<Block>();

		// Token: 0x040004D8 RID: 1240
		public static HashSet<Block> ModelHits = new HashSet<Block>();

		// Token: 0x040004D9 RID: 1241
		public static Dictionary<string, HashSet<Block>> TagHits = new Dictionary<string, HashSet<Block>>();

		// Token: 0x040004DA RID: 1242
		public static Dictionary<string, HashSet<Block>> ModelTagHits = new Dictionary<string, HashSet<Block>>();

		// Token: 0x040004DB RID: 1243
		private List<BlockAbstractBow.BlockBowArrow> _arrows = new List<BlockAbstractBow.BlockBowArrow>();

		// Token: 0x040004DC RID: 1244
		private const float DEFAULT_SHOT_FORCE = 50f;

		// Token: 0x040004DD RID: 1245
		private float _timeBetweenShots;

		// Token: 0x040004DE RID: 1246
		private bool _shotSignaled;

		// Token: 0x040004DF RID: 1247
		private float _shotForce = 50f;

		// Token: 0x02000050 RID: 80
		private class BlockBowArrow
		{
			// Token: 0x060006BF RID: 1727 RVA: 0x0002DC60 File Offset: 0x0002C060
			public BlockBowArrow(BlockAbstractBow source)
			{
				this._state = BlockAbstractBow.BlockBowArrow.ArrowState.LOADING;
				this._sourceBow = source;
				this._go = (UnityEngine.Object.Instantiate(Resources.Load(this._sourceBow.GetAmmoPrefabName())) as GameObject);
				this._collider = this._go.GetComponent<Collider>();
				this._go.transform.position = this._sourceBow.goT.position + this._sourceBow.goT.rotation * this._sourceBow.GetAmmoPositionOffset();
				this._go.transform.rotation = this._sourceBow.goT.rotation * this._arrowRotation;
				this._go.transform.parent = this._sourceBow.goT.parent;
				MeshFilter[] componentsInChildren = this._go.GetComponentsInChildren<MeshFilter>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					GameObject gameObject = componentsInChildren[i].gameObject;
					int ammoSubmeshColorMapping = this._sourceBow.GetAmmoSubmeshColorMapping(i);
					if (ammoSubmeshColorMapping >= 0)
					{
						Materials.SetMaterial(gameObject, componentsInChildren[i].mesh, gameObject.name, this._sourceBow.GetPaint(ammoSubmeshColorMapping), this._sourceBow.GetTexture(ammoSubmeshColorMapping), Vector3.one, this._sourceBow.GetScale(), string.Empty);
					}
				}
			}

			// Token: 0x060006C0 RID: 1728 RVA: 0x0002DE03 File Offset: 0x0002C203
			private void AlignToDirection(Vector3 direction)
			{
				this._go.transform.up = -direction;
			}

			// Token: 0x060006C1 RID: 1729 RVA: 0x0002DE1B File Offset: 0x0002C21B
			public void Destroy()
			{
				UnityEngine.Object.Destroy(this._go);
				this._go = null;
				this._rb = null;
				this._collider = null;
				this._sourceBow = null;
				this._hitBlock = null;
			}

			// Token: 0x060006C2 RID: 1730 RVA: 0x0002DE4B File Offset: 0x0002C24B
			public bool IsReadyForCleanup()
			{
				return this._state == BlockAbstractBow.BlockBowArrow.ArrowState.NONE;
			}

			// Token: 0x060006C3 RID: 1731 RVA: 0x0002DE56 File Offset: 0x0002C256
			public bool IsReadyToShoot()
			{
				return this._state == BlockAbstractBow.BlockBowArrow.ArrowState.LOADING || this._state == BlockAbstractBow.BlockBowArrow.ArrowState.DRAWING;
			}

			// Token: 0x060006C4 RID: 1732 RVA: 0x0002DE70 File Offset: 0x0002C270
			public void Shoot(Vector3 direction, float force)
			{
				this._rb = this._go.AddComponent<Rigidbody>();
				this._rb.mass = 0.25f;
				this._rb.useGravity = this._sourceBow.ShotsObeyGravity();
				this._rb.AddForce(force * direction, ForceMode.VelocityChange);
				this._go.transform.parent = null;
				this._state = BlockAbstractBow.BlockBowArrow.ArrowState.FLYING;
			}

			// Token: 0x060006C5 RID: 1733 RVA: 0x0002DEE0 File Offset: 0x0002C2E0
			public void FixedUpdate()
			{
				switch (this._state)
				{
				case BlockAbstractBow.BlockBowArrow.ArrowState.FLYING:
					this._lifetimeInFlight -= Time.fixedDeltaTime;
					if (this._lifetimeInFlight <= 0f)
					{
						this._state = BlockAbstractBow.BlockBowArrow.ArrowState.NONE;
					}
					else
					{
						Vector3 normalized = this._rb.velocity.normalized;
						float magnitude = this._rb.velocity.magnitude;
						float num = magnitude * Time.fixedDeltaTime;
						float maxDistance = num * 1.75f;
						Vector3 origin = this._go.transform.position - normalized * num * 0.5f;
						this.AlignToDirection(normalized);
						if (Physics.Raycast(origin, normalized, out this._hit, maxDistance))
						{
							this._hitBlock = BWSceneManager.FindBlock(this._hit.collider.gameObject, false);
							bool flag = this._hitBlock is BlockAnimatedCharacter || this._hitBlock is BlockCharacter;
							if (this._sourceBow.ArrowHit(this._hitBlock, this._hit.point, normalized) && !flag)
							{
								this._state = BlockAbstractBow.BlockBowArrow.ArrowState.EMBEDDED;
								this._go.transform.parent = this._hit.collider.transform;
								UnityEngine.Object.DestroyImmediate(this._rb);
								this._rb = null;
								this._go.transform.position = this._hit.point - normalized * UnityEngine.Random.value * 0.1f;
								if (this._hitBlock is BlockAbstractWater)
								{
									this._lifetimeAfterHit = 0f;
								}
							}
							else if (this._hitBlock != null && this._hitBlock.BlockType().EndsWith("Character Skeleton"))
							{
								this._rb.velocity = magnitude * 0.2f * UnityEngine.Random.onUnitSphere;
							}
							else
							{
								this._state = BlockAbstractBow.BlockBowArrow.ArrowState.STOPPED;
								this._go.transform.position = this._hit.point;
								this._rb.velocity = Vector3.zero;
								this._collider.enabled = true;
							}
						}
					}
					break;
				case BlockAbstractBow.BlockBowArrow.ArrowState.EMBEDDED:
				case BlockAbstractBow.BlockBowArrow.ArrowState.STOPPED:
					this._lifetimeAfterHit -= Time.fixedDeltaTime;
					if (this._lifetimeAfterHit <= 0f)
					{
						this._state = BlockAbstractBow.BlockBowArrow.ArrowState.NONE;
					}
					break;
				}
			}

			// Token: 0x040004E0 RID: 1248
			private GameObject _go;

			// Token: 0x040004E1 RID: 1249
			private Rigidbody _rb;

			// Token: 0x040004E2 RID: 1250
			private Collider _collider;

			// Token: 0x040004E3 RID: 1251
			private Quaternion _arrowRotation = Quaternion.Euler(-180f, -180f, 90f);

			// Token: 0x040004E4 RID: 1252
			private RaycastHit _hit = default(RaycastHit);

			// Token: 0x040004E5 RID: 1253
			private BlockAbstractBow _sourceBow;

			// Token: 0x040004E6 RID: 1254
			private Block _hitBlock;

			// Token: 0x040004E7 RID: 1255
			private float _lifetimeInFlight = 20f;

			// Token: 0x040004E8 RID: 1256
			private float _lifetimeAfterHit = 5f;

			// Token: 0x040004E9 RID: 1257
			private BlockAbstractBow.BlockBowArrow.ArrowState _state;

			// Token: 0x02000051 RID: 81
			private enum ArrowState
			{
				// Token: 0x040004EB RID: 1259
				NONE,
				// Token: 0x040004EC RID: 1260
				LOADING,
				// Token: 0x040004ED RID: 1261
				DRAWING,
				// Token: 0x040004EE RID: 1262
				FLYING,
				// Token: 0x040004EF RID: 1263
				EMBEDDED,
				// Token: 0x040004F0 RID: 1264
				STOPPED
			}
		}
	}
}
