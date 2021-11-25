using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x02000125 RID: 293
public class ModelAnimationCommand : Command
{
	// Token: 0x060013FD RID: 5117 RVA: 0x0008B7A8 File Offset: 0x00089BA8
	protected Vector3 GetBlockCenter()
	{
		Vector3 vector = Vector3.zero;
		int num = 0;
		foreach (Block block in this.blocks)
		{
			vector += block.GetPosition();
			num++;
		}
		if (num > 0)
		{
			vector /= (float)num;
		}
		return vector;
	}

	// Token: 0x060013FE RID: 5118 RVA: 0x0008B828 File Offset: 0x00089C28
	protected virtual Vector3 GetStartPos()
	{
		return this.GetBlockCenter();
	}

	// Token: 0x060013FF RID: 5119 RVA: 0x0008B830 File Offset: 0x00089C30
	protected Vector3 GetRayHitPos(Ray ray)
	{
		float d = 10f;
		RaycastHit raycastHit;
		if (Physics.Raycast(ray, out raycastHit, 10f))
		{
			d = Mathf.Max(2f, raycastHit.distance);
		}
		return ray.origin + ray.direction * d;
	}

	// Token: 0x06001400 RID: 5120 RVA: 0x0008B880 File Offset: 0x00089C80
	protected Vector3 GetModelRectWorldPos()
	{
		Vector3 result = Vector3.zero;
		UIQuickSelect quickSelect = Blocksworld.UI.QuickSelect;
		if (quickSelect != null)
		{
			Vector2 vector = quickSelect.ModelRectCenter();
			Ray ray = Blocksworld.mainCamera.ScreenPointToRay(new Vector3(vector.x, vector.y, 0f) * NormalizedScreen.scale);
			result = this.GetRayHitPos(ray);
		}
		return result;
	}

	// Token: 0x06001401 RID: 5121 RVA: 0x0008B8E8 File Offset: 0x00089CE8
	protected Vector3 GetUpperRightWorldPos()
	{
		Ray ray = Blocksworld.mainCamera.ScreenPointToRay(new Vector3((float)Screen.width, (float)Screen.height, 0f));
		return this.GetRayHitPos(ray);
	}

	// Token: 0x06001402 RID: 5122 RVA: 0x0008B91D File Offset: 0x00089D1D
	protected virtual Vector3 GetTargetPos()
	{
		return this.GetModelRectWorldPos();
	}

	// Token: 0x06001403 RID: 5123 RVA: 0x0008B925 File Offset: 0x00089D25
	protected virtual float GetScaleFromFraction(float fraction)
	{
		return 2f * Mathf.Max(0.01f, 1f - fraction);
	}

	// Token: 0x06001404 RID: 5124 RVA: 0x0008B940 File Offset: 0x00089D40
	public override void Execute()
	{
		this.step++;
		if (this.step >= this.animationSteps)
		{
			this.parent.transform.localScale = Vector3.one;
			this.parent.transform.DetachChildren();
			foreach (Block block in this.blocks)
			{
				block.Destroy();
				if (block is BlockTerrain)
				{
					BWSceneManager.RemoveTerrainBlock((BlockTerrain)block);
				}
			}
			this.blocks.Clear();
			this.done = true;
			this.step = int.MaxValue;
			if (!string.IsNullOrEmpty(this.doneSfx))
			{
				Sound.PlayOneShotSound(this.doneSfx, 1f);
			}
			if (this.endCommand != null)
			{
				Blocksworld.AddFixedUpdateCommand(this.endCommand);
			}
			Blocksworld.clipboard.copiedModelIconUpToDate = false;
			Blocksworld.UI.QuickSelect.UpdateModelIcon();
		}
		if (this.step == (int)Mathf.Round((float)this.animationSteps / 3f) && !string.IsNullOrEmpty(this.halfwaySfx))
		{
			Sound.PlayOneShotSound(this.halfwaySfx, 1f);
		}
		float num = Mathf.Clamp((float)this.step / (float)this.animationSteps, 0f, 1f);
		Vector3 position = (1f - num) * this.startPos + num * this.targetPos;
		this.parent.transform.position = position;
		this.parent.transform.localScale = Vector3.one * this.GetScaleFromFraction(num);
	}

	// Token: 0x06001405 RID: 5125 RVA: 0x0008BB18 File Offset: 0x00089F18
	public bool Animating()
	{
		return this.step <= this.animationSteps;
	}

	// Token: 0x06001406 RID: 5126 RVA: 0x0008BB2C File Offset: 0x00089F2C
	public void SetBlocks(List<Block> blocks)
	{
		this.step = 0;
		this.endCommand = null;
		this.blocks.Clear();
		if (this.parent == null)
		{
			this.parent = new GameObject("Model animation object");
		}
		this.parent.layer = 8;
		this.parent.transform.localScale = Vector3.one;
		List<Block> list = new List<Block>(blocks);
		int num = 20;
		if (blocks.Count > num)
		{
			Vector3 camPos = Blocksworld.cameraTransform.position;
			HashSet<Block> blockVisible = new HashSet<Block>();
			for (int i = 0; i < blocks.Count; i++)
			{
				Block block = blocks[i];
				if (Util.CameraVisibilityCheck(camPos, block.goT.position, new HashSet<Block>
				{
					block
				}, true, null))
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
		foreach (Block block2 in list)
		{
			Block block3 = Block.NewBlock(Blocksworld.CloneBlockTiles(block2.tiles, null, false, false), false, false);
			if (block3 is BlockAnimatedCharacter)
			{
				(block3 as BlockAnimatedCharacter).PrepareForModelIconRender(Layer.Default);
			}
			this.blocks.Add(block3);
		}
		BlockGroups.GatherBlockGroups(this.blocks);
		foreach (Block block4 in this.blocks)
		{
			BlockTankTreadsWheel blockTankTreadsWheel = block4 as BlockTankTreadsWheel;
			if (blockTankTreadsWheel != null && blockTankTreadsWheel.IsMainBlockInGroup())
			{
				blockTankTreadsWheel.CreateTreads(false, true);
			}
		}
		foreach (Block block5 in this.blocks)
		{
			UnityEngine.Object.Destroy(block5.go.GetComponent<Collider>());
		}
		this.parent.transform.position = this.GetBlockCenter();
		foreach (Block block6 in this.blocks)
		{
			Transform goT = block6.goT;
			goT.parent = this.parent.transform;
		}
		this.startPos = this.GetStartPos();
		this.parent.transform.position = this.startPos;
		this.targetPos = this.GetTargetPos();
		this.animationSteps = 12;
	}

	// Token: 0x04000FA0 RID: 4000
	private int step = int.MaxValue;

	// Token: 0x04000FA1 RID: 4001
	private int animationSteps = 40;

	// Token: 0x04000FA2 RID: 4002
	protected string doneSfx = "Copy Model Done";

	// Token: 0x04000FA3 RID: 4003
	protected string halfwaySfx = "Copy Model Transfer";

	// Token: 0x04000FA4 RID: 4004
	private GameObject parent;

	// Token: 0x04000FA5 RID: 4005
	private List<Block> blocks = new List<Block>();

	// Token: 0x04000FA6 RID: 4006
	private Vector3 targetPos = new Vector3(800f, 100f, -0.5f);

	// Token: 0x04000FA7 RID: 4007
	private Vector3 startPos = default(Vector3);

	// Token: 0x04000FA8 RID: 4008
	public DelegateCommand endCommand;
}
