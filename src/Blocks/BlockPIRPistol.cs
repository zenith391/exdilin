using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockPIRPistol : BlockAbstractBow
{
	private const float _minTimeBetweenShots = 0.15f;

	private readonly Vector3 _ammoPositionOffset = new Vector3(0.393f, -0.126f, -0.072f);

	private readonly Vector3 _hammerPositionOffset = new Vector3(0.27f, -0.026f, -0.1024f);

	private readonly string _ammoPrefabName = "Blocks/Prefab PIR Pistol Ammo";

	private readonly string _blockedSFX = "Arrow Blocked";

	private readonly string[] _hitSFX = new string[4] { "Meteor Impact Earth 1", "Meteor Impact Earth 2", "Meteor Impact Earth 3", "Meteor Impact Earth 4" };

	private readonly string _shootSFX = "grenade_detonation";

	private ParticleSystem _muzzleFlashParticleSystem;

	private GameObject _muzzleFlashParticleSystemGo;

	private ParticleSystem _smokeParticleSystem;

	private GameObject _smokeParticleSystemGo;

	private readonly Color32 gray = new Color32(127, 127, 127, byte.MaxValue);

	private readonly string _hammerSubmeshName = "pir pistol hammer";

	protected Transform _hammerSubmesh;

	public BlockPIRPistol(List<List<Tile>> tiles)
		: base(tiles)
	{
		_hammerSubmesh = goT.Find(_hammerSubmeshName);
		_muzzleFlashParticleSystemGo = UnityEngine.Object.Instantiate(Resources.Load("Particles/Muzzle Flash")) as GameObject;
		_muzzleFlashParticleSystem = _muzzleFlashParticleSystemGo.GetComponent<ParticleSystem>();
		_muzzleFlashParticleSystem.enableEmission = false;
		_muzzleFlashParticleSystemGo.transform.parent = goT;
		_muzzleFlashParticleSystemGo.transform.localPosition = Vector3.zero;
		_muzzleFlashParticleSystemGo.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
		_smokeParticleSystemGo = UnityEngine.Object.Instantiate(Resources.Load("Blocks/BlockLaser Smoke Particle System")) as GameObject;
		_smokeParticleSystem = _smokeParticleSystemGo.GetComponent<ParticleSystem>();
	}

	protected override float GetMinTimeBetweenShots()
	{
		return 0.15f;
	}

	protected override Vector3 GetAmmoPositionOffset()
	{
		return _ammoPositionOffset;
	}

	protected override string GetAmmoPrefabName()
	{
		return _ammoPrefabName;
	}

	protected override Vector3 GetFiringDirection()
	{
		return goT.up;
	}

	protected override string GetSFXForBlocked()
	{
		return _blockedSFX;
	}

	protected override string GetSFXForHit()
	{
		return _hitSFX[UnityEngine.Random.Range(0, 4)];
	}

	protected override string GetSFXForLoad()
	{
		return null;
	}

	protected override string GetSFXForShoot()
	{
		return _shootSFX;
	}

	protected override bool HasSFXForLoad()
	{
		return false;
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockPIRPistol>("PIRPistol.Triggered", (Block b) => b.IsFiredAsWeapon);
		Predicate pred = PredicateRegistry.Add<BlockPIRPistol>("PIRPistol.Load", (Block b) => ((BlockAbstractBow)b).IsLoaded, (Block b) => ((BlockAbstractBow)b).Load);
		PredicateRegistry.Add<BlockPIRPistol>("PIRPistol.Shoot", null, (Block b) => ((BlockAbstractBow)b).Shoot, new Type[1] { typeof(float) }, new string[1] { "Force" });
		List<List<Tile>> list = new List<List<Tile>>();
		list.Add(new List<Tile>
		{
			new Tile(Block.predicateFirstFrame),
			Block.ThenTile(),
			new Tile(pred)
		});
		list.Add(Block.EmptyTileRow());
		List<List<Tile>> value = list;
		Block.defaultExtraTiles["PIR Pistol"] = value;
	}

	protected override void BowLoaded()
	{
		_hammerSubmesh.localRotation = Quaternion.identity;
	}

	protected override void BowShot()
	{
		_hammerSubmesh.localRotation = Quaternion.AngleAxis(35f, Vector3.right);
		_muzzleFlashParticleSystemGo.transform.localPosition = _ammoPositionOffset + Vector3.up * 0.75f;
		_muzzleFlashParticleSystemGo.transform.localRotation = Quaternion.Euler(-90f, 0f, UnityEngine.Random.value * 360f);
		_muzzleFlashParticleSystem.Emit(1);
		float num = 1f + UnityEngine.Random.value;
		ParticleSystem.Particle particle = new ParticleSystem.Particle
		{
			position = goT.TransformPoint(_hammerPositionOffset),
			velocity = Vector3.zero,
			size = 0.2f + UnityEngine.Random.value * 0.3f,
			remainingLifetime = num,
			startLifetime = num,
			color = gray
		};
		_smokeParticleSystem.Emit(particle);
	}

	public override void Destroy()
	{
		UnityEngine.Object.Destroy(_smokeParticleSystemGo);
		UnityEngine.Object.Destroy(_muzzleFlashParticleSystemGo);
		base.Destroy();
	}

	public override void Pause()
	{
		base.Pause();
		_smokeParticleSystem.Pause();
	}

	public override void Play()
	{
		base.Play();
		_muzzleFlashParticleSystemGo.transform.localPosition = _ammoPositionOffset + Vector3.up * 0.75f;
	}

	public override void Resume()
	{
		base.Resume();
		_smokeParticleSystem.Play();
	}

	public override void Stop(bool resetBlock = true)
	{
		base.Stop(resetBlock);
		_smokeParticleSystem.Clear();
		_smokeParticleSystem.Play();
		_hammerSubmesh.localRotation = Quaternion.identity;
	}
}
