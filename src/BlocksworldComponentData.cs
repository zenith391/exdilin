using UnityEngine;

public class BlocksworldComponentData : MonoBehaviour
{
	public Camera guiCamera;

	public Camera rewardCamera;

	public Texture buttonPlus;

	public Texture buttonMinus;

	public GameObject prefabArrow;

	public ParticleSystem stars;

	public ParticleSystem starsReward;

	public Vector3 firstPersonDeadZone = new Vector3(5f, 5f, 0f);

	public float firstPersonTurnPower = 1f;

	public float firstPersonTorque = 0.5f;

	public float aimAdjustMin = 10f;

	public float aimAdjustMax = 50f;

	public float maxSpeedFoV = 35f;

	public Texture[] hudTextures;

	public bool FPCLookXFlip;

	public bool FPCLookYFlip;

	public void Awake()
	{
		Blocksworld.Bootstrap(base.gameObject);
	}
}
