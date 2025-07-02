using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockWorldJumper : Block
{
	private enum TeleportState
	{
		NONE,
		LOADING_WORLD,
		POWER_UP,
		JUMPING
	}

	private TeleportState teleportState;

	private Transform sunMesh;

	private Transform innerPlanetMesh;

	private Transform innerPlanetArmMesh;

	private Transform outerPlanetMesh;

	private Transform outerPlanetArmMesh;

	private float sunAngle;

	private float innerPlanetAngle;

	private float outerPlanetAngle;

	private bool orreryActive;

	private bool orreryAccelerating;

	private float orreryAccelerationFactor = 1f;

	private bool playedSFX;

	private float sfxDelay = 0.6f;

	private float sfxTimer;

	private GameObject vfxObj;

	private float vfxDelay = 0.8f;

	private float vfxTimer;

	public BlockWorldJumper(List<List<Tile>> tiles)
		: base(tiles)
	{
		sunMesh = goT.Find("Orrery Sun");
		innerPlanetMesh = goT.Find("Orrery Planet In");
		innerPlanetArmMesh = goT.Find("Orrery Planet In Arm");
		outerPlanetMesh = goT.Find("Orrery Planet Out");
		outerPlanetArmMesh = goT.Find("Orrery Planet Out Arm");
		orreryActive = false;
		orreryAccelerating = false;
		WorldSession.current.LoadAvailableTeleportWorlds();
		foreach (List<Tile> tile in tiles)
		{
			foreach (Tile item in tile)
			{
				if (item.gaf.Predicate.Name == "BlockWorldJumper.Jump")
				{
					string stringArgSafe = Util.GetStringArgSafe(item.gaf.Args, 0, string.Empty);
					if (!string.IsNullOrEmpty(stringArgSafe))
					{
						WorldSession.current.AddToAvailableTeleportWorlds(stringArgSafe);
					}
				}
			}
		}
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockWorldJumper>("BlockWorldJumper.Jump", null, (Block b) => ((BlockWorldJumper)b).JumpToWorld, new Type[1] { typeof(string) });
	}

	public override void Play()
	{
		base.Play();
		SetOrreryActive(active: true);
	}

	public override void Stop(bool resetBlock = true)
	{
		base.Stop(resetBlock);
		SetOrreryActive(active: false);
	}

	private void SetOrreryActive(bool active)
	{
		orreryActive = active;
		orreryAccelerating = false;
		orreryAccelerationFactor = 1f;
		if (vfxObj != null)
		{
			UnityEngine.Object.Destroy(vfxObj);
		}
		vfxObj = null;
		playedSFX = false;
		teleportState = TeleportState.NONE;
	}

	public override void Destroy()
	{
		SetOrreryActive(active: false);
		base.Destroy();
	}

	private void OrreryFixedUpdate()
	{
		if (!orreryActive)
		{
			return;
		}
		if (orreryAccelerating)
		{
			orreryAccelerationFactor *= 1.065f;
		}
		else
		{
			orreryAccelerationFactor = 1f;
		}
		sunAngle = Mathf.Repeat(sunAngle + 0.1f * orreryAccelerationFactor, 360f);
		innerPlanetAngle = Mathf.Repeat(innerPlanetAngle + 1.3f * orreryAccelerationFactor, 360f);
		outerPlanetAngle = Mathf.Repeat(outerPlanetAngle + 0.8f * orreryAccelerationFactor, 360f);
		if (teleportState == TeleportState.LOADING_WORLD)
		{
			if (WorldSession.current.WorldTeleportHasSource())
			{
				teleportState = TeleportState.POWER_UP;
				vfxTimer = vfxDelay;
				sfxTimer = sfxDelay;
				playedSFX = false;
			}
		}
		else
		{
			if (teleportState != TeleportState.POWER_UP)
			{
				return;
			}
			if (!playedSFX)
			{
				sfxTimer -= Time.fixedDeltaTime;
				if (sfxTimer <= 0f)
				{
					Sound.PlayOneShotSound("Teleport_Buildup");
					playedSFX = true;
				}
			}
			if (vfxObj == null)
			{
				vfxTimer -= Time.fixedDeltaTime;
				if (vfxTimer <= 0f)
				{
					vfxObj = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("VFX/WorldTeleportVFX"));
					vfxObj.transform.position = goT.position + Vector3.up * 0.6f;
					vfxObj.transform.parent = goT;
					vfxObj.GetComponent<Animator>().Play("WorldTeleport");
				}
			}
		}
	}

	private void OrreryUpdate()
	{
		if (orreryActive)
		{
			sunMesh.localRotation = Quaternion.AngleAxis(sunAngle, Vector3.up);
			innerPlanetArmMesh.localRotation = Quaternion.AngleAxis(innerPlanetAngle, Vector3.up);
			innerPlanetMesh.localRotation = innerPlanetArmMesh.localRotation;
			outerPlanetArmMesh.localRotation = Quaternion.AngleAxis(outerPlanetAngle, Vector3.up);
			outerPlanetMesh.localRotation = outerPlanetArmMesh.localRotation;
		}
	}

	public TileResultCode JumpToWorld(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string stringArg = Util.GetStringArg(args, 0, string.Empty);
		if (WorldSession.current.availableTeleportWorlds.IsLoadingWorldList())
		{
			return TileResultCode.Delayed;
		}
		float delay = 1.8f;
		if (WorldSession.current.TriggerJumpToWorld(stringArg, delay))
		{
			teleportState = TeleportState.LOADING_WORLD;
			orreryAccelerating = true;
			playedSFX = false;
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (Blocksworld.CurrentState == State.Play)
		{
			OrreryFixedUpdate();
		}
	}

	public override void Update()
	{
		base.Update();
		if (Blocksworld.CurrentState == State.Play)
		{
			OrreryUpdate();
		}
	}
}
