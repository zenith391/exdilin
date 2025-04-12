using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x020000EA RID: 234
	public class BlockVolumeBlock : BlockPosition
	{
		// Token: 0x06001141 RID: 4417 RVA: 0x000771FA File Offset: 0x000755FA
		public BlockVolumeBlock(List<List<Tile>> tiles) : base(tiles, true)
		{
		}

		// Token: 0x06001142 RID: 4418 RVA: 0x00077204 File Offset: 0x00075604
		public new static void Register()
		{
			PredicateRegistry.Add<BlockVolumeBlock>("Volume.DestroyTouchingBlock", null, (Block b) => new PredicateActionDelegate(((BlockVolumeBlock)b).DestroyTouchingBlock), null, null, null);
			PredicateRegistry.Add<BlockVolumeBlock>("Volume.DestroyTouchingChunk", null, (Block b) => new PredicateActionDelegate(((BlockVolumeBlock)b).DestroyTouchingChunk), null, null, null);
			List<List<Tile>> value = new List<List<Tile>>
			{
				new List<Tile>
				{
					Block.ThenTile(),
					new Tile(new GAF("Block.Fixed", new object[0]))
				},
				Block.EmptyTileRow()
			};
			Block.defaultExtraTiles["Volume Block"] = value;
			Block.defaultExtraTiles["Volume Block No Glue"] = value;
			Block.defaultExtraTiles["Volume Block Slab"] = value;
			Block.defaultExtraTiles["Volume Block Slab No Glue"] = value;
		}

		// Token: 0x06001143 RID: 4419 RVA: 0x000772EF File Offset: 0x000756EF
		public override void Play()
		{
			base.Play();
			this._destroyBlocksTouching = false;
			this._destroyChunkTouching = false;
		}

		// Token: 0x06001144 RID: 4420 RVA: 0x00077305 File Offset: 0x00075705
		public override void Stop(bool resetBlock = true)
		{
			base.Stop(resetBlock);
			this._destroyBlocksTouching = false;
			this._destroyChunkTouching = false;
		}

		// Token: 0x06001145 RID: 4421 RVA: 0x0007731C File Offset: 0x0007571C
		public TileResultCode DestroyTouchingBlock(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this._destroyBlocksTouching = true;
			return TileResultCode.True;
		}

		// Token: 0x06001146 RID: 4422 RVA: 0x00077326 File Offset: 0x00075726
		public TileResultCode DestroyTouchingChunk(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this._destroyChunkTouching = true;
			return TileResultCode.True;
		}

		// Token: 0x06001147 RID: 4423 RVA: 0x00077330 File Offset: 0x00075730
		private bool IsBlockForRemoval(Block b)
		{
			bool result = false;
			if (b != null)
			{
				BlockGrouped blockGrouped = b as BlockGrouped;
				if (blockGrouped != null)
				{
					result = !Invincibility.IsInvincible(blockGrouped.GetMainBlockInGroup());
				}
				else
				{
					result = !Invincibility.IsInvincible(b);
				}
			}
			return result;
		}

		// Token: 0x06001148 RID: 4424 RVA: 0x00077374 File Offset: 0x00075774
		private void RemovePlayBlockFromScene(Block b)
		{
			BlockMissile blockMissile = b as BlockMissile;
			if (blockMissile != null)
			{
				blockMissile.RemoveMissile();
			}
			else
			{
				Blocksworld.AddFixedUpdateCommand(new SplitChunkCommand(b));
				BWSceneManager.RemovePlayBlock(b);
			}
		}

		// Token: 0x06001149 RID: 4425 RVA: 0x000773AC File Offset: 0x000757AC
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (this._destroyBlocksTouching || this._destroyChunkTouching)
			{
				HashSet<GameObject> triggeringBlocks = CollisionManager.GetTriggeringBlocks(this);
				if (triggeringBlocks != null)
				{
					Collider component = this.go.GetComponent<Collider>();
					Bounds bounds = component.bounds;
					HashSet<GameObject> hashSet = new HashSet<GameObject>();
					foreach (GameObject gameObject in triggeringBlocks)
					{
						if (!(gameObject == null))
						{
							if (this._destroyChunkTouching)
							{
								Vector3 worldCenterOfMass = gameObject.GetComponent<Rigidbody>().worldCenterOfMass;
								if (bounds.Contains(worldCenterOfMass))
								{
									IEnumerator enumerator2 = gameObject.transform.GetEnumerator();
									try
									{
										while (enumerator2.MoveNext())
										{
											object obj = enumerator2.Current;
											Transform transform = (Transform)obj;
											Block b = BWSceneManager.FindBlock(transform.gameObject, false);
											if (this.IsBlockForRemoval(b))
											{
												this.RemovePlayBlockFromScene(b);
												hashSet.Add(gameObject);
											}
										}
									}
									finally
									{
										IDisposable disposable;
										if ((disposable = (enumerator2 as IDisposable)) != null)
										{
											disposable.Dispose();
										}
									}
								}
							}
							else
							{
								IEnumerator enumerator3 = gameObject.transform.GetEnumerator();
								try
								{
									while (enumerator3.MoveNext())
									{
										object obj2 = enumerator3.Current;
										Transform transform2 = (Transform)obj2;
										Block b2 = BWSceneManager.FindBlock(transform2.gameObject, false);
										if (this.IsBlockForRemoval(b2) && bounds.Intersects(transform2.gameObject.GetComponent<Collider>().bounds))
										{
											this.RemovePlayBlockFromScene(b2);
											hashSet.Add(gameObject);
										}
									}
								}
								finally
								{
									IDisposable disposable2;
									if ((disposable2 = (enumerator3 as IDisposable)) != null)
									{
										disposable2.Dispose();
									}
								}
							}
						}
					}
					foreach (GameObject item in hashSet)
					{
						triggeringBlocks.Remove(item);
					}
				}
			}
			this._destroyBlocksTouching = false;
			this._destroyChunkTouching = false;
		}

		// Token: 0x04000D89 RID: 3465
		private bool _destroyBlocksTouching;

		// Token: 0x04000D8A RID: 3466
		private bool _destroyChunkTouching;
	}
}
