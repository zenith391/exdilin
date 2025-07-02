using UnityEngine;

namespace Blocks;

public class BlockWaterInfo
{
	public Block block;

	public float mass;

	public Vector3 scale;

	public int interval;

	public int checkCount;

	public int counterOffset;

	public bool isSimulating = true;

	public bool hasWaterSensor;

	public float maxExtent = 1f;

	public BlockWaterInfo(Block b)
	{
		block = b;
		mass = b.GetMass();
		scale = b.Scale();
		counterOffset = Random.Range(0, 10000);
		maxExtent = b.CalculateMaxExtent();
	}
}
