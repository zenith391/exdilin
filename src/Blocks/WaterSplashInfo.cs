namespace Blocks;

public class WaterSplashInfo
{
	public Block block;

	public float forceSum;

	public int counter;

	public WaterSplashInfo(Block block)
	{
		this.block = block;
	}

	public void Update()
	{
		counter++;
	}

	public void EnterWater()
	{
		counter = 0;
		forceSum = 0f;
	}

	public void LeaveWater()
	{
	}
}
