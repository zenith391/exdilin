using System.Collections.Generic;
using Blocks;
using UnityEngine;

public class ModelAnimationCommand : Command
{
	private int step = int.MaxValue;

	private int animationSteps = 40;

	protected string doneSfx = "Copy Model Done";

	protected string halfwaySfx = "Copy Model Transfer";

	private GameObject parent;

	private List<Block> blocks = new List<Block>();

	private Vector3 targetPos = new Vector3(800f, 100f, -0.5f);

	private Vector3 startPos;

	public DelegateCommand endCommand;

	protected Vector3 GetBlockCenter()
	{
		Vector3 zero = Vector3.zero;
		int num = 0;
		foreach (Block block in blocks)
		{
			zero += block.GetPosition();
			num++;
		}
		if (num > 0)
		{
			zero /= (float)num;
		}
		return zero;
	}

	protected virtual Vector3 GetStartPos()
	{
		return GetBlockCenter();
	}

	protected Vector3 GetRayHitPos(Ray ray)
	{
		float num = 10f;
		if (Physics.Raycast(ray, out var hitInfo, 10f))
		{
			num = Mathf.Max(2f, hitInfo.distance);
		}
		return ray.origin + ray.direction * num;
	}

	protected Vector3 GetModelRectWorldPos()
	{
		Vector3 result = Vector3.zero;
		UIQuickSelect quickSelect = Blocksworld.UI.QuickSelect;
		if (quickSelect != null)
		{
			Vector2 vector = quickSelect.ModelRectCenter();
			Ray ray = Blocksworld.mainCamera.ScreenPointToRay(new Vector3(vector.x, vector.y, 0f) * NormalizedScreen.scale);
			result = GetRayHitPos(ray);
		}
		return result;
	}

	protected Vector3 GetUpperRightWorldPos()
	{
		Ray ray = Blocksworld.mainCamera.ScreenPointToRay(new Vector3(Screen.width, Screen.height, 0f));
		return GetRayHitPos(ray);
	}

	protected virtual Vector3 GetTargetPos()
	{
		return GetModelRectWorldPos();
	}

	protected virtual float GetScaleFromFraction(float fraction)
	{
		return 2f * Mathf.Max(0.01f, 1f - fraction);
	}

	public override void Execute()
	{
		step++;
		if (step >= animationSteps)
		{
			parent.transform.localScale = Vector3.one;
			parent.transform.DetachChildren();
			foreach (Block block in blocks)
			{
				block.Destroy();
				if (block is BlockTerrain)
				{
					BWSceneManager.RemoveTerrainBlock((BlockTerrain)block);
				}
			}
			blocks.Clear();
			done = true;
			step = int.MaxValue;
			if (!string.IsNullOrEmpty(doneSfx))
			{
				Sound.PlayOneShotSound(doneSfx);
			}
			if (endCommand != null)
			{
				Blocksworld.AddFixedUpdateCommand(endCommand);
			}
			Blocksworld.clipboard.copiedModelIconUpToDate = false;
			Blocksworld.UI.QuickSelect.UpdateModelIcon();
		}
		if (step == (int)Mathf.Round((float)animationSteps / 3f) && !string.IsNullOrEmpty(halfwaySfx))
		{
			Sound.PlayOneShotSound(halfwaySfx);
		}
		float num = Mathf.Clamp((float)step / (float)animationSteps, 0f, 1f);
		Vector3 position = (1f - num) * startPos + num * targetPos;
		parent.transform.position = position;
		parent.transform.localScale = Vector3.one * GetScaleFromFraction(num);
	}

	public bool Animating()
	{
		return step <= animationSteps;
	}

	public void SetBlocks(List<Block> blocks)
	{
		step = 0;
		endCommand = null;
		this.blocks.Clear();
		if (parent == null)
		{
			parent = new GameObject("Model animation object");
		}
		parent.layer = 8;
		parent.transform.localScale = Vector3.one;
		List<Block> list = new List<Block>(blocks);
		int num = 20;
		if (blocks.Count > num)
		{
			Vector3 camPos = Blocksworld.cameraTransform.position;
			HashSet<Block> blockVisible = new HashSet<Block>();
			for (int i = 0; i < blocks.Count; i++)
			{
				Block block = blocks[i];
				if (Util.CameraVisibilityCheck(camPos, block.goT.position, new HashSet<Block> { block }))
				{
					blockVisible.Add(block);
				}
			}
			list.Sort(delegate(Block b1, Block b2)
			{
				Vector3 position = b1.goT.position;
				Vector3 position2 = b2.goT.position;
				float num2 = (position - camPos).sqrMagnitude;
				float num3 = (position2 - camPos).sqrMagnitude;
				if (!blockVisible.Contains(b1))
				{
					num2 += 10f;
				}
				if (!blockVisible.Contains(b2))
				{
					num3 += 10f;
				}
				return (int)(num2 - num3);
			});
			list.RemoveRange(num, list.Count - num);
		}
		foreach (Block item in list)
		{
			Block block2 = Block.NewBlock(Blocksworld.CloneBlockTiles(item.tiles));
			if (block2 is BlockAnimatedCharacter)
			{
				(block2 as BlockAnimatedCharacter).PrepareForModelIconRender(Layer.Default);
			}
			this.blocks.Add(block2);
		}
		BlockGroups.GatherBlockGroups(this.blocks);
		foreach (Block block3 in this.blocks)
		{
			if (block3 is BlockTankTreadsWheel blockTankTreadsWheel && blockTankTreadsWheel.IsMainBlockInGroup())
			{
				blockTankTreadsWheel.CreateTreads(shapeOnly: false, parentIsBlock: true);
			}
		}
		foreach (Block block4 in this.blocks)
		{
			Object.Destroy(block4.go.GetComponent<Collider>());
		}
		parent.transform.position = GetBlockCenter();
		foreach (Block block5 in this.blocks)
		{
			Transform goT = block5.goT;
			goT.parent = parent.transform;
		}
		startPos = GetStartPos();
		parent.transform.position = startPos;
		targetPos = GetTargetPos();
		animationSteps = 12;
	}
}
