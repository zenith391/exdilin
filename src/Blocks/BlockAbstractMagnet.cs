using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x0200005F RID: 95
	public class BlockAbstractMagnet : Block
	{
		// Token: 0x060007CE RID: 1998 RVA: 0x000368DC File Offset: 0x00034CDC
		public BlockAbstractMagnet(List<List<Tile>> tiles) : base(tiles)
		{
		}

		// Token: 0x060007CF RID: 1999 RVA: 0x00036954 File Offset: 0x00034D54
		public static bool PulledByMagnet(Block b)
		{
			foreach (BlockAbstractMagnet blockAbstractMagnet in BlockAbstractMagnet.activeMagnets)
			{
				if (blockAbstractMagnet.pulledBlocks.Contains(b))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060007D0 RID: 2000 RVA: 0x000369C4 File Offset: 0x00034DC4
		public static bool PulledByMagnetModel(Block mb)
		{
			foreach (BlockAbstractMagnet blockAbstractMagnet in BlockAbstractMagnet.activeMagnets)
			{
				if (blockAbstractMagnet.pulledModelBlocks.Contains(mb))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060007D1 RID: 2001 RVA: 0x00036A34 File Offset: 0x00034E34
		public static bool PushedByMagnet(Block b)
		{
			foreach (BlockAbstractMagnet blockAbstractMagnet in BlockAbstractMagnet.activeMagnets)
			{
				if (blockAbstractMagnet.pushedBlocks.Contains(b))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060007D2 RID: 2002 RVA: 0x00036AA4 File Offset: 0x00034EA4
		public static bool PushedByMagnetModel(Block mb)
		{
			foreach (BlockAbstractMagnet blockAbstractMagnet in BlockAbstractMagnet.activeMagnets)
			{
				if (blockAbstractMagnet.pushedModelBlocks.Contains(mb))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060007D3 RID: 2003 RVA: 0x00036B14 File Offset: 0x00034F14
		public override void ResetFrame()
		{
			base.ResetFrame();
			this.centerOfMagneticSurface = this.goT.position - 0.5f * this.goT.up;
		}

		// Token: 0x060007D4 RID: 2004 RVA: 0x00036B48 File Offset: 0x00034F48
		public override TileResultCode PaintTo(string paint, bool permanent, int meshIndex)
		{
			TileResultCode result = base.PaintTo(paint, permanent, meshIndex);
			if (meshIndex == 0)
			{
				this.magnetPaint = this.GetPaint(0);
			}
			return result;
		}

		// Token: 0x060007D5 RID: 2005 RVA: 0x00036B74 File Offset: 0x00034F74
		public override TileResultCode TextureTo(string texture, Vector3 normal, bool permanent, int meshIndex, bool force)
		{
			TileResultCode result = base.TextureTo(texture, normal, permanent, meshIndex, force);
			if (meshIndex == 0)
			{
				this.magnetTexture = base.GetTexture(0);
			}
			return result;
		}

		// Token: 0x060007D6 RID: 2006 RVA: 0x00036BA4 File Offset: 0x00034FA4
		public TileResultCode SetForceMode(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.forceMode = Util.GetIntArg(args, 0, 0);
			return TileResultCode.True;
		}

		// Token: 0x060007D7 RID: 2007 RVA: 0x00036BB5 File Offset: 0x00034FB5
		public int GetForceMode()
		{
			return this.forceMode;
		}

		// Token: 0x060007D8 RID: 2008 RVA: 0x00036BBD File Offset: 0x00034FBD
		public TileResultCode SetDistance(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.farDistance = Util.GetFloatArg(args, 0, 50f);
			this.farDistance = Mathf.Max(1f, this.farDistance);
			return TileResultCode.True;
		}

		// Token: 0x060007D9 RID: 2009 RVA: 0x00036BE8 File Offset: 0x00034FE8
		public TileResultCode SetDistanceNear(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.nearDistance = Util.GetFloatArg(args, 0, 10f);
			return TileResultCode.True;
		}

		// Token: 0x060007DA RID: 2010 RVA: 0x00036BFD File Offset: 0x00034FFD
		public float GetDistanceNear()
		{
			return this.nearDistance;
		}

		// Token: 0x060007DB RID: 2011 RVA: 0x00036C08 File Offset: 0x00035008
		public override void Play()
		{
			base.Play();
			BlockAbstractMagnet.activeMagnets.Add(this);
			this.farDistance = 50f;
			this.nearDistance = 0f;
			this.magnetPaint = this.GetPaint(0);
			this.magnetTexture = base.GetTexture(0);
			this.chunkForces.Clear();
		}

		// Token: 0x060007DC RID: 2012 RVA: 0x00036C64 File Offset: 0x00035064
		public override void Stop(bool resetBlock)
		{
			base.Stop(resetBlock);
			BlockAbstractMagnet.activeMagnets.Remove(this);
			this.chunkForces.Clear();
			this.pulledBlocks.Clear();
			this.pulledModelBlocks.Clear();
			this.pushedBlocks.Clear();
			this.pushedModelBlocks.Clear();
		}

		// Token: 0x060007DD RID: 2013 RVA: 0x00036CBC File Offset: 0x000350BC
		private BlockAbstractMagnet.ChunkForces GetOrCreateChunkForcesForChunk(Chunk ch)
		{
			BlockAbstractMagnet.ChunkForces chunkForces;
			if (!this.chunkForces.TryGetValue(ch, out chunkForces))
			{
				chunkForces = new BlockAbstractMagnet.ChunkForces();
				this.chunkForces[ch] = chunkForces;
			}
			return chunkForces;
		}

		// Token: 0x060007DE RID: 2014 RVA: 0x00036CF0 File Offset: 0x000350F0
		private void AddChunkInfluenceAtPosition(Block b, float blockMass, Chunk ch, float influence, Vector3 toBlock, Vector3 position)
		{
			if (ch.GetMass() == 0f)
			{
				ch.ForceMass(0.1f);
			}
			float mass = base.GetMass();
			Vector3 normalized = toBlock.normalized;
			Vector3 vector = normalized * 4f * influence * mass * blockMass;
			Vector3 b2 = (!(ch.rb != null)) ? ch.go.transform.position : ch.rb.worldCenterOfMass;
			BlockAbstractMagnet.ChunkForces orCreateChunkForcesForChunk = this.GetOrCreateChunkForcesForChunk(ch);
			orCreateChunkForcesForChunk.forceSumCm += vector;
			orCreateChunkForcesForChunk.torqueSum += Vector3.Cross(position - b2, vector);
			orCreateChunkForcesForChunk.pulled |= (influence < 0f);
			orCreateChunkForcesForChunk.pushed |= (influence > 0f);
			orCreateChunkForcesForChunk.blocks.Add(b);
		}

		// Token: 0x060007DF RID: 2015 RVA: 0x00036DF0 File Offset: 0x000351F0
		private void AddChunkInfluence(Block b, float influence, Vector3 toBlock, Vector3 blockPos)
		{
			float num = b.GetMass();
			Chunk chunk = b.chunk;
			BlockAbstractWheel blockAbstractWheel = b as BlockAbstractWheel;
			if (blockAbstractWheel != null)
			{
				GameObject realChassis = blockAbstractWheel.GetRealChassis();
				if (realChassis == null)
				{
					num += blockAbstractWheel.chunk.rb.mass / chunk.GetTotalBlockMass();
				}
				else
				{
					num += blockAbstractWheel.GetHelperObjectMass();
					Block block = BWSceneManager.FindBlock(realChassis.transform.GetChild(0).gameObject, true);
					chunk = block.chunk;
				}
			}
			else if (chunk.rb != null)
			{
				num *= chunk.rb.mass / chunk.GetTotalBlockMass();
			}
			this.AddChunkInfluenceAtPosition(b, num, chunk, influence, toBlock, blockPos);
		}

		// Token: 0x060007E0 RID: 2016 RVA: 0x00036EAC File Offset: 0x000352AC
		private void AddBlockInfluence(Block b, float influence)
		{
			if (b == null || b.go == null || b.chunk.rb == null || b.modelBlock == this.modelBlock || b.chunk == null || b.isTerrain)
			{
				return;
			}
			Vector3 vector = b.GetCenter();
			if (b is BlockAbstractMovingPlatform)
			{
				vector = b.chunk.go.transform.position;
			}
			Vector3 toBlock = vector - this.centerOfMagneticSurface;
			float sqrMagnitude = toBlock.sqrMagnitude;
			if (sqrMagnitude > this.farDistance * this.farDistance)
			{
				vector = b.go.GetComponent<Renderer>().bounds.ClosestPoint(this.centerOfMagneticSurface);
				toBlock = vector - this.centerOfMagneticSurface;
				float sqrMagnitude2 = toBlock.sqrMagnitude;
				if (sqrMagnitude2 > this.farDistance * this.farDistance)
				{
					return;
				}
			}
			float num = Mathf.Max(0.01f, 1f - toBlock.magnitude / this.farDistance);
			this.AddChunkInfluence(b, influence * num * num, toBlock, vector);
		}

		// Token: 0x060007E1 RID: 2017 RVA: 0x00036FD8 File Offset: 0x000353D8
		public TileResultCode InfluenceTag(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string stringArg = Util.GetStringArg(args, 0, string.Empty);
			float floatArg = Util.GetFloatArg(args, 1, 1f);
			List<Block> blocksWithTag = TagManager.GetBlocksWithTag(stringArg);
			for (int i = 0; i < blocksWithTag.Count; i++)
			{
				Block b = blocksWithTag[i];
				this.AddBlockInfluence(b, floatArg);
			}
			return TileResultCode.True;
		}

		// Token: 0x060007E2 RID: 2018 RVA: 0x00037030 File Offset: 0x00035430
		public TileResultCode InfluencePaint(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float floatArg = Util.GetFloatArg(args, 0, 1f);
			foreach (Block b in TextureAndPaintBlockRegistry.GetBlocksWithPaint(this.magnetPaint))
			{
				this.AddBlockInfluence(b, floatArg);
			}
			return TileResultCode.True;
		}

		// Token: 0x060007E3 RID: 2019 RVA: 0x000370A0 File Offset: 0x000354A0
		public TileResultCode InfluenceTexture(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float floatArg = Util.GetFloatArg(args, 0, 1f);
			foreach (Block b in TextureAndPaintBlockRegistry.GetBlocksWithTexture(this.magnetTexture))
			{
				this.AddBlockInfluence(b, floatArg);
			}
			return TileResultCode.True;
		}

		// Token: 0x060007E4 RID: 2020 RVA: 0x00037110 File Offset: 0x00035510
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			this.pulledBlocks.Clear();
			this.pulledModelBlocks.Clear();
			this.pushedBlocks.Clear();
			this.pushedModelBlocks.Clear();
			if (this.chunkForces.Count > 0)
			{
				Vector3 a = Vector3.zero;
				foreach (KeyValuePair<Chunk, BlockAbstractMagnet.ChunkForces> keyValuePair in this.chunkForces)
				{
					Chunk key = keyValuePair.Key;
					BlockAbstractMagnet.ChunkForces value = keyValuePair.Value;
					if (!float.IsNaN(value.forceSumCm.x))
					{
						Rigidbody rb = key.rb;
						List<Block> blocks = value.blocks;
						int count = blocks.Count;
						if (count == 0)
						{
							this.keysToRemove.Add(key);
						}
						else
						{
							if (rb != null)
							{
								rb.AddForce(value.forceSumCm);
								rb.AddTorque(value.torqueSum);
							}
							a += value.forceSumCm;
							value.forceSumCm.Set(0f, 0f, 0f);
							value.torqueSum.Set(0f, 0f, 0f);
						}
						if (value.pulled)
						{
							for (int i = 0; i < count; i++)
							{
								Block block = blocks[i];
								this.pulledBlocks.Add(block);
								this.pulledModelBlocks.Add(block.modelBlock);
							}
						}
						if (value.pushed)
						{
							for (int j = 0; j < count; j++)
							{
								Block block2 = blocks[j];
								this.pushedBlocks.Add(block2);
								this.pushedModelBlocks.Add(block2.modelBlock);
							}
						}
						blocks.Clear();
					}
				}
				if (this.chunk != null && this.chunk.rb != null && !this.chunk.rb.isKinematic)
				{
					if (this.chunk.GetMass() == 0f)
					{
						this.chunk.ForceMass(0.1f);
					}
					this.chunk.rb.AddForceAtPosition(-a, this.centerOfMagneticSurface);
				}
				if (this.keysToRemove.Count > 0)
				{
					for (int k = 0; k < this.keysToRemove.Count; k++)
					{
						this.chunkForces.Remove(this.keysToRemove[k]);
					}
					this.keysToRemove.Clear();
				}
			}
		}

		// Token: 0x040005F6 RID: 1526
		private int forceMode;

		// Token: 0x040005F7 RID: 1527
		private const float DEFAULT_MAGNET_FAR_DISTANCE = 50f;

		// Token: 0x040005F8 RID: 1528
		private float nearDistance;

		// Token: 0x040005F9 RID: 1529
		private float farDistance = 50f;

		// Token: 0x040005FA RID: 1530
		private Vector3 centerOfMagneticSurface;

		// Token: 0x040005FB RID: 1531
		private string magnetPaint = "Yellow";

		// Token: 0x040005FC RID: 1532
		private string magnetTexture = "Plain";

		// Token: 0x040005FD RID: 1533
		private Dictionary<Chunk, BlockAbstractMagnet.ChunkForces> chunkForces = new Dictionary<Chunk, BlockAbstractMagnet.ChunkForces>();

		// Token: 0x040005FE RID: 1534
		private HashSet<Block> pulledBlocks = new HashSet<Block>();

		// Token: 0x040005FF RID: 1535
		private HashSet<Block> pushedModelBlocks = new HashSet<Block>();

		// Token: 0x04000600 RID: 1536
		private HashSet<Block> pushedBlocks = new HashSet<Block>();

		// Token: 0x04000601 RID: 1537
		private HashSet<Block> pulledModelBlocks = new HashSet<Block>();

		// Token: 0x04000602 RID: 1538
		private static List<BlockAbstractMagnet> activeMagnets = new List<BlockAbstractMagnet>();

		// Token: 0x04000603 RID: 1539
		private const float FORCE_SCALE = 4f;

		// Token: 0x04000604 RID: 1540
		private List<Chunk> keysToRemove = new List<Chunk>();

		// Token: 0x02000060 RID: 96
		private class ChunkForces
		{
			// Token: 0x04000605 RID: 1541
			public Vector3 forceSumCm = Vector3.zero;

			// Token: 0x04000606 RID: 1542
			public Vector3 torqueSum = Vector3.zero;

			// Token: 0x04000607 RID: 1543
			public List<Block> blocks = new List<Block>();

			// Token: 0x04000608 RID: 1544
			public bool pulled;

			// Token: 0x04000609 RID: 1545
			public bool pushed;
		}
	}
}
