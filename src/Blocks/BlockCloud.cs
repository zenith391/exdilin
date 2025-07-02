using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockCloud : BlockTerrain
{
	private Color lightTint = Color.white;

	private bool camWithinCloud;

	private bool camWasWithinCloud;

	private bool dontKnowWhetherCamWasWithinCloudOrNot = true;

	private bool intangible;

	private float insideDist;

	public BlockCloud(List<List<Tile>> tiles)
		: base(tiles)
	{
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockCloud>("Cloud.Intangible", null, (Block b) => ((BlockCloud)b).SetTrigger);
		List<List<Tile>> list = new List<List<Tile>>();
		list.Add(new List<Tile>
		{
			Block.ThenTile(),
			new Tile(new GAF("Cloud.Intangible"))
		});
		list.Add(Block.EmptyTileRow());
		List<List<Tile>> value = list;
		Block.defaultExtraTiles["Cloud 1"] = value;
		Block.defaultExtraTiles["Cloud 2"] = value;
	}

	public override TileResultCode SetTrigger(ScriptRowExecutionInfo eInfo, object[] args)
	{
		return base.SetTrigger(eInfo, args);
	}

	public override void Play()
	{
		base.Play();
	}

	public override void Update()
	{
		base.Update();
		Vector3 cameraPosition = Blocksworld.cameraPosition;
		Collider component = go.GetComponent<Collider>();
		if (component != null)
		{
			intangible = component.isTrigger;
			Vector3 cameraForward = Blocksworld.cameraForward;
			float num = Util.MaxComponent(size);
			Vector3 vector = cameraPosition + cameraForward;
			float num2 = insideDist;
			if (vector + cameraForward * num != -cameraForward)
			{
				if (component.Raycast(new Ray(vector + cameraForward * num, -cameraForward), out var hitInfo, num) && component.Raycast(new Ray(vector - cameraForward * num, cameraForward), out var _, num))
				{
					insideDist = (vector - hitInfo.point).magnitude;
					camWithinCloud = true;
				}
				else
				{
					insideDist = 0f;
					camWithinCloud = false;
				}
			}
			bool flag = camWithinCloud != camWasWithinCloud || dontKnowWhetherCamWasWithinCloudOrNot;
			dontKnowWhetherCamWasWithinCloudOrNot = false;
			if (flag || Mathf.Abs(num2 - insideDist) > 0.02f)
			{
				float insideFraction = GetInsideFraction();
				Blocksworld.mainCamera.backgroundColor = lightTint * (insideFraction * lightTint + (1f - insideFraction) * Color.white);
				Blocksworld.worldSky.go.GetComponent<Renderer>().enabled = (!camWithinCloud || !intangible) && !Blocksworld.renderingSkybox;
				Blocksworld.UpdateDynamicalLights();
			}
			if (flag)
			{
				go.GetComponent<Renderer>().enabled = !camWithinCloud;
			}
		}
		camWasWithinCloud = camWithinCloud;
	}

	public override void Destroy()
	{
		base.Destroy();
		if (Blocksworld.worldSky != null && Blocksworld.worldSky.go != null)
		{
			Blocksworld.worldSky.go.GetComponent<Renderer>().enabled = !Blocksworld.renderingSkybox;
			Blocksworld.UpdateDynamicalLights();
		}
	}

	public override bool HasDynamicalLight()
	{
		return true;
	}

	public override Color GetDynamicalLightTint()
	{
		if (camWithinCloud && intangible)
		{
			float insideFraction = GetInsideFraction();
			return insideFraction * lightTint + (1f - insideFraction) * Color.white;
		}
		return Color.white;
	}

	public override Color GetFogColorOverride()
	{
		if (camWithinCloud && intangible)
		{
			float insideFraction = GetInsideFraction();
			return insideFraction * lightTint + (1f - insideFraction) * BlockSky.GetFogColor();
		}
		return Color.white;
	}

	private float GetInsideFraction()
	{
		return Mathf.Min(1f, insideDist * 0.25f);
	}

	public override float GetFogMultiplier()
	{
		if (camWithinCloud && intangible)
		{
			return 1f - GetInsideFraction() * 0.85f;
		}
		return 1f;
	}

	public override TileResultCode PaintTo(string paint, bool permanent, int meshIndex = 0)
	{
		TileResultCode result = base.PaintTo(paint, permanent, meshIndex);
		Vector4 vector = lightTint;
		lightTint = go.GetComponent<Renderer>().sharedMaterial.color;
		lightTint = 0.5f * lightTint + 0.5f * Color.white;
		Vector4 vector2 = lightTint;
		if (camWithinCloud && (vector - vector2).sqrMagnitude > 0.001f)
		{
			Blocksworld.UpdateDynamicalLights();
		}
		return result;
	}

	public override bool IsSolidTerrain()
	{
		if (Blocksworld.playFixedUpdateCounter != 0)
		{
			return !intangible;
		}
		return false;
	}
}
