using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockVolume : Block
{
	public BlockVolume(List<List<Tile>> tiles)
		: base(tiles)
	{
		go.GetComponent<Renderer>().enabled = Options.BlockVolumeDisplay;
		if (goShadow != null)
		{
			goShadow.GetComponent<Renderer>().enabled = false;
		}
	}

	public new static void Register()
	{
	}

	public override void Play()
	{
		base.Play();
		if ((bool)goT.parent.GetComponent<Rigidbody>())
		{
			goT.parent.GetComponent<Rigidbody>().isKinematic = true;
		}
		go.GetComponent<Renderer>().enabled = false;
		go.GetComponent<Collider>().isTrigger = true;
		IgnoreRaycasts(value: true);
	}

	public override void Stop(bool resetBlock = true)
	{
		base.Stop(resetBlock);
		go.GetComponent<Renderer>().enabled = Options.BlockVolumeDisplay;
		go.GetComponent<Collider>().isTrigger = false;
		if (goShadow != null)
		{
			goShadow.GetComponent<Renderer>().enabled = false;
		}
		IgnoreRaycasts(value: false);
	}

	public override bool IsColliding(float terrainOffset = 0f, HashSet<Block> exclude = null)
	{
		return false;
	}
}
