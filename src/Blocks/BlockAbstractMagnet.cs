using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockAbstractMagnet : Block
{
	private class ChunkForces
	{
		public Vector3 forceSumCm = Vector3.zero;

		public Vector3 torqueSum = Vector3.zero;

		public List<Block> blocks = new List<Block>();

		public bool pulled;

		public bool pushed;
	}

	private int forceMode;

	private const float DEFAULT_MAGNET_FAR_DISTANCE = 50f;

	private float nearDistance;

	private float farDistance = 50f;

	private Vector3 centerOfMagneticSurface;

	private string magnetPaint = "Yellow";

	private string magnetTexture = "Plain";

	private Dictionary<Chunk, ChunkForces> chunkForces = new Dictionary<Chunk, ChunkForces>();

	private HashSet<Block> pulledBlocks = new HashSet<Block>();

	private HashSet<Block> pushedModelBlocks = new HashSet<Block>();

	private HashSet<Block> pushedBlocks = new HashSet<Block>();

	private HashSet<Block> pulledModelBlocks = new HashSet<Block>();

	private static List<BlockAbstractMagnet> activeMagnets;

	private const float FORCE_SCALE = 4f;

	private List<Chunk> keysToRemove = new List<Chunk>();

	public BlockAbstractMagnet(List<List<Tile>> tiles)
		: base(tiles)
	{
	}

	public static bool PulledByMagnet(Block b)
	{
		foreach (BlockAbstractMagnet activeMagnet in activeMagnets)
		{
			if (activeMagnet.pulledBlocks.Contains(b))
			{
				return true;
			}
		}
		return false;
	}

	public static bool PulledByMagnetModel(Block mb)
	{
		foreach (BlockAbstractMagnet activeMagnet in activeMagnets)
		{
			if (activeMagnet.pulledModelBlocks.Contains(mb))
			{
				return true;
			}
		}
		return false;
	}

	public static bool PushedByMagnet(Block b)
	{
		foreach (BlockAbstractMagnet activeMagnet in activeMagnets)
		{
			if (activeMagnet.pushedBlocks.Contains(b))
			{
				return true;
			}
		}
		return false;
	}

	public static bool PushedByMagnetModel(Block mb)
	{
		foreach (BlockAbstractMagnet activeMagnet in activeMagnets)
		{
			if (activeMagnet.pushedModelBlocks.Contains(mb))
			{
				return true;
			}
		}
		return false;
	}

	public override void ResetFrame()
	{
		base.ResetFrame();
		centerOfMagneticSurface = goT.position - 0.5f * goT.up;
	}

	public override TileResultCode PaintTo(string paint, bool permanent, int meshIndex)
	{
		TileResultCode result = base.PaintTo(paint, permanent, meshIndex);
		if (meshIndex == 0)
		{
			magnetPaint = GetPaint();
		}
		return result;
	}

	public override TileResultCode TextureTo(string texture, Vector3 normal, bool permanent, int meshIndex, bool force)
	{
		TileResultCode result = base.TextureTo(texture, normal, permanent, meshIndex, force);
		if (meshIndex == 0)
		{
			magnetTexture = GetTexture();
		}
		return result;
	}

	public TileResultCode SetForceMode(ScriptRowExecutionInfo eInfo, object[] args)
	{
		forceMode = Util.GetIntArg(args, 0, 0);
		return TileResultCode.True;
	}

	public int GetForceMode()
	{
		return forceMode;
	}

	public TileResultCode SetDistance(ScriptRowExecutionInfo eInfo, object[] args)
	{
		farDistance = Util.GetFloatArg(args, 0, 50f);
		farDistance = Mathf.Max(1f, farDistance);
		return TileResultCode.True;
	}

	public TileResultCode SetDistanceNear(ScriptRowExecutionInfo eInfo, object[] args)
	{
		nearDistance = Util.GetFloatArg(args, 0, 10f);
		return TileResultCode.True;
	}

	public float GetDistanceNear()
	{
		return nearDistance;
	}

	public override void Play()
	{
		base.Play();
		activeMagnets.Add(this);
		farDistance = 50f;
		nearDistance = 0f;
		magnetPaint = GetPaint();
		magnetTexture = GetTexture();
		chunkForces.Clear();
	}

	public override void Stop(bool resetBlock)
	{
		base.Stop(resetBlock);
		activeMagnets.Remove(this);
		chunkForces.Clear();
		pulledBlocks.Clear();
		pulledModelBlocks.Clear();
		pushedBlocks.Clear();
		pushedModelBlocks.Clear();
	}

	private ChunkForces GetOrCreateChunkForcesForChunk(Chunk ch)
	{
		if (!chunkForces.TryGetValue(ch, out var value))
		{
			value = new ChunkForces();
			chunkForces[ch] = value;
		}
		return value;
	}

	private void AddChunkInfluenceAtPosition(Block b, float blockMass, Chunk ch, float influence, Vector3 toBlock, Vector3 position)
	{
		if (ch.GetMass() == 0f)
		{
			ch.ForceMass(0.1f);
		}
		float mass = GetMass();
		Vector3 normalized = toBlock.normalized;
		Vector3 vector = normalized * 4f * influence * mass * blockMass;
		Vector3 vector2 = ((!(ch.rb != null)) ? ch.go.transform.position : ch.rb.worldCenterOfMass);
		ChunkForces orCreateChunkForcesForChunk = GetOrCreateChunkForcesForChunk(ch);
		orCreateChunkForcesForChunk.forceSumCm += vector;
		orCreateChunkForcesForChunk.torqueSum += Vector3.Cross(position - vector2, vector);
		orCreateChunkForcesForChunk.pulled |= influence < 0f;
		orCreateChunkForcesForChunk.pushed |= influence > 0f;
		orCreateChunkForcesForChunk.blocks.Add(b);
	}

	private void AddChunkInfluence(Block b, float influence, Vector3 toBlock, Vector3 blockPos)
	{
		float num = b.GetMass();
		Chunk chunk = b.chunk;
		if (b is BlockAbstractWheel blockAbstractWheel)
		{
			GameObject realChassis = blockAbstractWheel.GetRealChassis();
			if (realChassis == null)
			{
				num += blockAbstractWheel.chunk.rb.mass / chunk.GetTotalBlockMass();
			}
			else
			{
				num += blockAbstractWheel.GetHelperObjectMass();
				Block block = BWSceneManager.FindBlock(realChassis.transform.GetChild(0).gameObject, checkChildGos: true);
				chunk = block.chunk;
			}
		}
		else if (chunk.rb != null)
		{
			num *= chunk.rb.mass / chunk.GetTotalBlockMass();
		}
		AddChunkInfluenceAtPosition(b, num, chunk, influence, toBlock, blockPos);
	}

	private void AddBlockInfluence(Block b, float influence)
	{
		if (b == null || b.go == null || b.chunk == null || b.chunk.rb == null || b.modelBlock == null || b.isTerrain)
		{
			return;
		}
		Renderer component = b.go.GetComponent<Renderer>();
		if (component == null)
		{
			return;
		}
		Vector3 vector = b.GetCenter();
		if (b is BlockAbstractMovingPlatform)
		{
			vector = b.chunk.go.transform.position;
		}
		_ = centerOfMagneticSurface;
		Vector3 toBlock = vector - centerOfMagneticSurface;
		if (toBlock.sqrMagnitude > farDistance * farDistance)
		{
			vector = component.bounds.ClosestPoint(centerOfMagneticSurface);
			toBlock = vector - centerOfMagneticSurface;
			if (toBlock.sqrMagnitude > farDistance * farDistance)
			{
				return;
			}
		}
		float num = Mathf.Max(0.01f, 1f - toBlock.magnitude / farDistance);
		AddChunkInfluence(b, influence * num * num, toBlock, vector);
	}

	public TileResultCode InfluenceTag(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string stringArg = Util.GetStringArg(args, 0, string.Empty);
		float floatArg = Util.GetFloatArg(args, 1, 1f);
		List<Block> blocksWithTag = TagManager.GetBlocksWithTag(stringArg);
		for (int i = 0; i < blocksWithTag.Count; i++)
		{
			Block b = blocksWithTag[i];
			AddBlockInfluence(b, floatArg);
		}
		return TileResultCode.True;
	}

	public TileResultCode InfluencePaint(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float floatArg = Util.GetFloatArg(args, 0, 1f);
		foreach (Block item in TextureAndPaintBlockRegistry.GetBlocksWithPaint(magnetPaint))
		{
			AddBlockInfluence(item, floatArg);
		}
		return TileResultCode.True;
	}

	public TileResultCode InfluenceTexture(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float floatArg = Util.GetFloatArg(args, 0, 1f);
		foreach (Block item in TextureAndPaintBlockRegistry.GetBlocksWithTexture(magnetTexture))
		{
			AddBlockInfluence(item, floatArg);
		}
		return TileResultCode.True;
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		pulledBlocks.Clear();
		pulledModelBlocks.Clear();
		pushedBlocks.Clear();
		pushedModelBlocks.Clear();
		if (chunkForces.Count <= 0)
		{
			return;
		}
		Vector3 zero = Vector3.zero;
		foreach (KeyValuePair<Chunk, ChunkForces> chunkForce in chunkForces)
		{
			Chunk key = chunkForce.Key;
			ChunkForces value = chunkForce.Value;
			if (float.IsNaN(value.forceSumCm.x))
			{
				continue;
			}
			Rigidbody rb = key.rb;
			List<Block> blocks = value.blocks;
			int count = blocks.Count;
			if (count == 0)
			{
				keysToRemove.Add(key);
			}
			else
			{
				if (rb != null)
				{
					rb.AddForce(value.forceSumCm);
					rb.AddTorque(value.torqueSum);
				}
				zero += value.forceSumCm;
				value.forceSumCm.Set(0f, 0f, 0f);
				value.torqueSum.Set(0f, 0f, 0f);
			}
			if (value.pulled)
			{
				for (int i = 0; i < count; i++)
				{
					Block block = blocks[i];
					pulledBlocks.Add(block);
					pulledModelBlocks.Add(block.modelBlock);
				}
			}
			if (value.pushed)
			{
				for (int j = 0; j < count; j++)
				{
					Block block2 = blocks[j];
					pushedBlocks.Add(block2);
					pushedModelBlocks.Add(block2.modelBlock);
				}
			}
			blocks.Clear();
		}
		if (chunk != null && chunk.rb != null && !chunk.rb.isKinematic)
		{
			if (chunk.GetMass() == 0f)
			{
				chunk.ForceMass(0.1f);
			}
			chunk.rb.AddForceAtPosition(-zero, centerOfMagneticSurface);
		}
		if (keysToRemove.Count > 0)
		{
			for (int k = 0; k < keysToRemove.Count; k++)
			{
				chunkForces.Remove(keysToRemove[k]);
			}
			keysToRemove.Clear();
		}
	}

	static BlockAbstractMagnet()
	{
		activeMagnets = new List<BlockAbstractMagnet>();
	}
}
