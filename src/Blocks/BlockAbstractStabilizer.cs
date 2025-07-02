using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public abstract class BlockAbstractStabilizer : Block
{
	private List<StabilizerNozzleInfo> nozzleInfos;

	private float mass = 1f;

	private bool wasBroken;

	private int treatAsVehicleStatus = -1;

	private bool _burstingLeft;

	private bool _burstingRight;

	public bool varyingGravity;

	private int sfxLoopUpdateCounter;

	private const int SFX_LOOP_UPDATE_INTERVAL = 5;

	private float currentLoopVolume;

	public BlockAbstractStabilizer(List<List<Tile>> tiles)
		: base(tiles)
	{
		nozzleInfos = new List<StabilizerNozzleInfo>();
		StabilizerNozzleInfo stabilizerNozzleInfo = new StabilizerNozzleInfo(this, Quaternion.Euler(new Vector3(0f, 0f, 0f)), -1);
		StabilizerNozzleInfo stabilizerNozzleInfo2 = (stabilizerNozzleInfo.otherNozzle = new StabilizerNozzleInfo(this, Quaternion.Euler(new Vector3(180f, 0f, 0f))));
		stabilizerNozzleInfo2.otherNozzle = stabilizerNozzleInfo;
		nozzleInfos.Add(stabilizerNozzleInfo);
		nozzleInfos.Add(stabilizerNozzleInfo2);
		loopName = "Stabilizer Hover Loop";
		sfxLoopUpdateCounter = UnityEngine.Random.Range(0, 5);
	}

	public override void SetVaryingGravity(bool vg)
	{
		varyingGravity = vg;
	}

	public TileResultCode BoostStabilizer(ScriptRowExecutionInfo eInfo, object[] args)
	{
		for (int i = 0; i < nozzleInfos.Count; i++)
		{
			StabilizerNozzleInfo stabilizerNozzleInfo = nozzleInfos[i];
			stabilizerNozzleInfo.BoostStabilizer();
		}
		return TileResultCode.True;
	}

	public TileResultCode Stabilize(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float floatArg = Util.GetFloatArg(args, 0, 1f);
		for (int i = 0; i < nozzleInfos.Count; i++)
		{
			StabilizerNozzleInfo stabilizerNozzleInfo = nozzleInfos[i];
			stabilizerNozzleInfo.Stabilize(eInfo.floatArg * floatArg);
		}
		return TileResultCode.True;
	}

	public TileResultCode StabilizePlane(ScriptRowExecutionInfo eInfo, object[] args)
	{
		for (int i = 0; i < nozzleInfos.Count; i++)
		{
			StabilizerNozzleInfo stabilizerNozzleInfo = nozzleInfos[i];
			stabilizerNozzleInfo.StabilizePlane(eInfo.floatArg);
		}
		return TileResultCode.True;
	}

	public TileResultCode ControlZeroAngVel(ScriptRowExecutionInfo eInfo, object[] args)
	{
		for (int i = 0; i < nozzleInfos.Count; i++)
		{
			StabilizerNozzleInfo stabilizerNozzleInfo = nozzleInfos[i];
			stabilizerNozzleInfo.ControlZeroAngVel(eInfo.floatArg);
		}
		return TileResultCode.True;
	}

	public TileResultCode ControlPosition(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float floatArg = Util.GetFloatArg(args, 0, 1f);
		for (int i = 0; i < nozzleInfos.Count; i++)
		{
			StabilizerNozzleInfo stabilizerNozzleInfo = nozzleInfos[i];
			stabilizerNozzleInfo.ControlPosition(eInfo.floatArg, floatArg);
		}
		return TileResultCode.True;
	}

	private void ChangePosition(float amount)
	{
		for (int i = 0; i < nozzleInfos.Count; i++)
		{
			StabilizerNozzleInfo stabilizerNozzleInfo = nozzleInfos[i];
			stabilizerNozzleInfo.ChangePosition(amount);
		}
	}

	public TileResultCode IncreasePosition(ScriptRowExecutionInfo eInfo, object[] args)
	{
		ChangePosition(0.1f);
		return TileResultCode.True;
	}

	public TileResultCode DecreasePosition(ScriptRowExecutionInfo eInfo, object[] args)
	{
		ChangePosition(-0.1f);
		return TileResultCode.True;
	}

	public TileResultCode Burst(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float num = (float)args[0] * eInfo.floatArg;
		bool flag = num > 0f;
		nozzleInfos[0].IncreaseForce((!flag) ? 0f : Mathf.Abs(num));
		nozzleInfos[1].IncreaseForce(flag ? 0f : Mathf.Abs(num));
		if (!didFix && !broken)
		{
			Vector3 vector = goT.up * num;
			Vector3 vec = vector * 2f;
			vec = Util.ProjectOntoPlane(vec, Vector3.up);
			Blocksworld.blocksworldCamera.AddForceDirectionHint(chunk, vec);
			BlockAccelerations.BlockAccelerates(this, vector);
		}
		return TileResultCode.True;
	}

	public TileResultCode IsBursting(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float num = (float)args[0];
		if ((num > 0f && _burstingRight) || (num < 0f && _burstingLeft))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsStabilizing(ScriptRowExecutionInfo eInfo, object[] args)
	{
		for (int i = 0; i < nozzleInfos.Count; i++)
		{
			StabilizerNozzleInfo stabilizerNozzleInfo = nozzleInfos[i];
			if (stabilizerNozzleInfo.lastMode == BlockStabilizerMode.STABILIZE && stabilizerNozzleInfo.stabilizing)
			{
				return TileResultCode.True;
			}
		}
		return TileResultCode.False;
	}

	public TileResultCode IsCloseToSomething(ScriptRowExecutionInfo eInfo, object[] args)
	{
		for (int i = 0; i < nozzleInfos.Count; i++)
		{
			StabilizerNozzleInfo stabilizerNozzleInfo = nozzleInfos[i];
			if (stabilizerNozzleInfo.canHover)
			{
				if (stabilizerNozzleInfo.hovering)
				{
					return TileResultCode.True;
				}
				continue;
			}
			Transform transform = stabilizerNozzleInfo.flame.transform;
			Vector3 direction = -transform.up;
			RaycastHit[] array = Physics.RaycastAll(transform.position, direction, stabilizerNozzleInfo.hoverHeight);
			Array.Sort(array, new RaycastDistanceComparer(transform.position));
			bool flag = false;
			for (int j = 0; j < array.Length; j++)
			{
				RaycastHit raycastHit = array[j];
				Block block = BWSceneManager.FindBlock(raycastHit.collider.gameObject);
				if (block != null)
				{
					flag = goT.parent != block.goT.parent;
				}
				if (flag)
				{
					float num = Vector3.Distance(raycastHit.point, transform.position);
					if (!(num < stabilizerNozzleInfo.hoverHeight))
					{
						break;
					}
					return TileResultCode.True;
				}
			}
		}
		return TileResultCode.False;
	}

	private void ChangeAngle(float amount)
	{
		for (int i = 0; i < nozzleInfos.Count; i++)
		{
			StabilizerNozzleInfo stabilizerNozzleInfo = nozzleInfos[i];
			stabilizerNozzleInfo.ChangeAngVel(amount);
		}
	}

	public TileResultCode IncreaseAngle(ScriptRowExecutionInfo eInfo, object[] args)
	{
		ChangeAngle(0.1f);
		return TileResultCode.True;
	}

	public TileResultCode DecreaseAngle(ScriptRowExecutionInfo eInfo, object[] args)
	{
		ChangeAngle(-0.1f);
		return TileResultCode.True;
	}

	public override void Destroy()
	{
		for (int i = 0; i < nozzleInfos.Count; i++)
		{
			StabilizerNozzleInfo stabilizerNozzleInfo = nozzleInfos[i];
			stabilizerNozzleInfo.Destroy();
		}
		base.Destroy();
	}

	public override void Play()
	{
		base.Play();
		treatAsVehicleStatus = -1;
		for (int i = 0; i < nozzleInfos.Count; i++)
		{
			StabilizerNozzleInfo stabilizerNozzleInfo = nozzleInfos[i];
			stabilizerNozzleInfo.Play();
		}
		SetVaryingGravity(vg: false);
		currentLoopVolume = 0f;
	}

	public override void Play2()
	{
		mass = Bunch.GetModelMass(this);
		for (int i = 0; i < nozzleInfos.Count; i++)
		{
			StabilizerNozzleInfo stabilizerNozzleInfo = nozzleInfos[i];
			stabilizerNozzleInfo.SetMass(mass);
		}
	}

	public override void Stop(bool resetBlock = true)
	{
		for (int i = 0; i < nozzleInfos.Count; i++)
		{
			StabilizerNozzleInfo stabilizerNozzleInfo = nozzleInfos[i];
			stabilizerNozzleInfo.Stop();
		}
		base.Stop(resetBlock);
		wasBroken = false;
	}

	public override void Pause()
	{
		for (int i = 0; i < nozzleInfos.Count; i++)
		{
			StabilizerNozzleInfo stabilizerNozzleInfo = nozzleInfos[i];
			stabilizerNozzleInfo.Pause();
		}
	}

	public override void Resume()
	{
		for (int i = 0; i < nozzleInfos.Count; i++)
		{
			StabilizerNozzleInfo stabilizerNozzleInfo = nozzleInfos[i];
			stabilizerNozzleInfo.Resume();
		}
	}

	public override void ResetFrame()
	{
		for (int i = 0; i < nozzleInfos.Count; i++)
		{
			StabilizerNozzleInfo stabilizerNozzleInfo = nozzleInfos[i];
			stabilizerNozzleInfo.ResetFrame();
		}
		_burstingLeft = false;
		_burstingRight = false;
	}

	private void UpdateLoopSound()
	{
		if (Blocksworld.CurrentState == State.Play && Sound.sfxEnabled && !vanished)
		{
			bool flag = false;
			float num = 0f;
			for (int i = 0; i < nozzleInfos.Count; i++)
			{
				StabilizerNozzleInfo stabilizerNozzleInfo = nozzleInfos[i];
				flag = flag || stabilizerNozzleInfo.lastMode != BlockStabilizerMode.OFF;
				if (flag)
				{
					num = Mathf.Max(num, stabilizerNozzleInfo.lastBurstAmount);
				}
			}
			_ = 0.11f;
			float num2 = num - 0.15f;
			float num3 = currentLoopVolume;
			float num4 = 0.02f;
			num3 = ((!(num3 < num2) || broken || vanished) ? (num3 - num4) : (num3 + num4));
			num3 = Mathf.Clamp(num3, 0f, 0.5f);
			if (sfxLoopUpdateCounter % 5 == 0)
			{
				UpdateWithinWaterLPFilter();
				AudioSource component = go.GetComponent<AudioSource>();
				PlayLoopSound(num3 > 0.01f, GetLoopClip(), num3, component, 1f + 0.5f * num2);
			}
			currentLoopVolume = num3;
			sfxLoopUpdateCounter++;
		}
		else
		{
			PlayLoopSound(play: false, GetLoopClip());
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (chunk != null && chunk.mobileCharacter != null)
		{
			chunk.mobileCharacter.isHovering = true;
		}
		for (int i = 0; i < nozzleInfos.Count; i++)
		{
			StabilizerNozzleInfo stabilizerNozzleInfo = nozzleInfos[i];
			stabilizerNozzleInfo.UpdateTransform();
		}
		if (!wasBroken && broken && chunk.go != null)
		{
			Rigidbody rb = chunk.rb;
			if (rb != null)
			{
				for (int j = 0; j < nozzleInfos.Count; j++)
				{
					StabilizerNozzleInfo stabilizerNozzleInfo2 = nozzleInfos[j];
					stabilizerNozzleInfo2.SetMass(rb.mass);
				}
			}
		}
		wasBroken = broken;
		for (int k = 0; k < nozzleInfos.Count; k++)
		{
			StabilizerNozzleInfo stabilizerNozzleInfo3 = nozzleInfos[k];
			if ((double)stabilizerNozzleInfo3.FixedUpdate(broken, vanished).sqrMagnitude > 0.01)
			{
				if (stabilizerNozzleInfo3.sign == 1)
				{
					_burstingLeft = true;
				}
				else if (stabilizerNozzleInfo3.sign == -1)
				{
					_burstingRight = true;
				}
			}
		}
		UpdateLoopSound();
	}

	public override bool TreatAsVehicleLikeBlock()
	{
		return TreatAsVehicleLikeBlockWithStatus(ref treatAsVehicleStatus);
	}
}
