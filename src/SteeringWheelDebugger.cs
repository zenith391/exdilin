using Blocks;
using UnityEngine;

public class SteeringWheelDebugger : MonoBehaviour
{
	public float awdBalanceFront = 0.4f;

	public float awdBalanceRear = 0.6f;

	public bool applyChanges;

	private float lastAwdBalanceFront = 0.4f;

	private float lastAwdBalanceRear = 0.6f;

	public void Update()
	{
		if (lastAwdBalanceFront != awdBalanceFront || lastAwdBalanceRear != awdBalanceRear || applyChanges)
		{
			if (lastAwdBalanceFront != awdBalanceFront || applyChanges)
			{
				BlockSteeringWheel.awdBalanceFront = awdBalanceFront;
			}
			if (lastAwdBalanceRear != awdBalanceRear || applyChanges)
			{
				BlockSteeringWheel.awdBalanceRear = awdBalanceRear;
			}
			if (applyChanges)
			{
				applyChanges = false;
				Blocksworld.UI.Overlay.ShowTimedOnScreenMessage("Steering wheel changes applied.", 3f);
			}
			lastAwdBalanceFront = awdBalanceFront;
			lastAwdBalanceRear = awdBalanceRear;
		}
	}
}
