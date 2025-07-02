using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockLauncher : Block
{
	private ConfigurableJoint joint;

	private bool isLaunched;

	private bool wasLaunched;

	private float launchForce;

	public BlockLauncher(List<List<Tile>> tiles)
		: base(tiles)
	{
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockLauncher>("Launcher.Launch", (Block b) => ((BlockLauncher)b).IsLaunched, (Block b) => ((BlockLauncher)b).Launch, new Type[1] { typeof(float) });
	}

	public override void Play2()
	{
		base.Play2();
		CreateFakeRigidbodyBetweenJoints();
	}

	public override void Play()
	{
		base.Play();
		List<Block> list = ConnectionsOfType(2, directed: true);
		joint = null;
		if (list.Count > 0)
		{
			BWLog.Info("Created joint");
			GameObject gameObject = list[0].chunk.go;
			GameObject gameObject2 = new GameObject(go.name + " Middle");
			gameObject2.transform.position = go.transform.position + go.transform.up;
			Rigidbody rigidbody = gameObject2.AddComponent<Rigidbody>();
			rigidbody.mass = 1f;
			if (Blocksworld.interpolateRigidBodies)
			{
				rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
			}
			joint = chunk.go.AddComponent<ConfigurableJoint>();
			joint.anchor = go.transform.localPosition;
			joint.axis = go.transform.up;
			joint.xMotion = ConfigurableJointMotion.Locked;
			joint.yMotion = ConfigurableJointMotion.Locked;
			joint.zMotion = ConfigurableJointMotion.Locked;
			joint.angularXMotion = ConfigurableJointMotion.Locked;
			joint.angularYMotion = ConfigurableJointMotion.Locked;
			joint.angularZMotion = ConfigurableJointMotion.Locked;
			joint.connectedBody = rigidbody;
			ConfigurableJoint configurableJoint = gameObject2.AddComponent<ConfigurableJoint>();
			configurableJoint.anchor = go.transform.localPosition;
			configurableJoint.axis = go.transform.up;
			configurableJoint.xMotion = ConfigurableJointMotion.Locked;
			configurableJoint.yMotion = ConfigurableJointMotion.Locked;
			configurableJoint.zMotion = ConfigurableJointMotion.Locked;
			configurableJoint.angularXMotion = ConfigurableJointMotion.Locked;
			configurableJoint.angularYMotion = ConfigurableJointMotion.Locked;
			configurableJoint.angularZMotion = ConfigurableJointMotion.Locked;
			configurableJoint.connectedBody = gameObject.GetComponent<Rigidbody>();
		}
		isLaunched = false;
		wasLaunched = false;
		launchForce = 0f;
	}

	public override void Stop(bool resetBlock = true)
	{
		if (joint != null)
		{
			UnityEngine.Object.Destroy(joint);
			joint = null;
			DestroyFakeRigidbodies();
		}
		base.Stop(resetBlock);
	}

	public TileResultCode Launch(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float num = (float)args[0];
		if (joint != null && !wasLaunched)
		{
			joint.xMotion = ConfigurableJointMotion.Free;
			joint.yMotion = ConfigurableJointMotion.Free;
			joint.zMotion = ConfigurableJointMotion.Free;
			joint.angularXMotion = ConfigurableJointMotion.Free;
			joint.angularYMotion = ConfigurableJointMotion.Free;
			joint.angularZMotion = ConfigurableJointMotion.Free;
			launchForce += num;
			isLaunched = true;
		}
		return TileResultCode.True;
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (isLaunched && !wasLaunched)
		{
			Vector3 vector = go.transform.up * launchForce;
			Vector3 position = go.transform.position + go.transform.up * 0.5f;
			joint.connectedBody.AddForceAtPosition(vector, position, ForceMode.Force);
			go.transform.parent.gameObject.GetComponent<Rigidbody>().AddForceAtPosition(-vector, position, ForceMode.Force);
			if (vector.magnitude > 0.5f)
			{
				PlayPositionedSound("Explode");
			}
		}
		wasLaunched = isLaunched;
	}

	public TileResultCode IsLaunched(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (isLaunched)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}
}
