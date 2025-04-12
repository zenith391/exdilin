using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x02000264 RID: 612
public class ScriptPanel : Panel
{
	// Token: 0x06001CC2 RID: 7362 RVA: 0x000CB1C4 File Offset: 0x000C95C4
	public ScriptPanel(string name) : base(name, 20)
	{
		this.width = 10000f;
		this.CreateHeader();
		this._momentum = 0f;
		this._snapBackMomentum = 0f;
		this.toggleOverlayEnabled = true;
		this.parameterOverlayEnabled = true;
		this.buttonOverlayEnabled = true;
		this.visibleTileRowStartIndex = 1;
		this.PositionReset();
		this.extraVerticalTopPadding = 34f * NormalizedScreen.pixelScale;
		ViewportWatchdog.AddListener(new ViewportWatchdog.ViewportSizeChangedAction(this.ViewportSizeDidChange));
	}

	// Token: 0x06001CC3 RID: 7363 RVA: 0x000CB245 File Offset: 0x000C9645
	public Vector3 TutorialArrowOffset()
	{
		return new Vector3(0f, this.extraVerticalTopPadding, 0f);
	}

	// Token: 0x06001CC4 RID: 7364 RVA: 0x000CB25C File Offset: 0x000C965C
	private void CreateHeader()
	{
		GameObject original = Resources.Load<GameObject>("GUI/Prefab Script Panel Header");
		this.headerGameObject = UnityEngine.Object.Instantiate<GameObject>(original);
		this.headerGameObject.transform.parent = this.go.transform;
		this.headerGameObject.transform.localScale = NormalizedScreen.pixelScale * Vector3.one;
	}

	// Token: 0x06001CC5 RID: 7365 RVA: 0x000CB2BA File Offset: 0x000C96BA
	public void Position()
	{
		if (this.pos.x < -0.5f * this.width)
		{
			this.pos.x = -this.width;
		}
		this.UpdatePosition();
	}

	// Token: 0x06001CC6 RID: 7366 RVA: 0x000CB2F0 File Offset: 0x000C96F0
	public void PositionReset()
	{
		this.pos.x = 0f;
		this.pos.y = (float)NormalizedScreen.height - this.height - 75f * NormalizedScreen.pixelScale;
		this.Position();
	}

	// Token: 0x06001CC7 RID: 7367 RVA: 0x000CB32C File Offset: 0x000C972C
	public override void UpdatePosition()
	{
		float min = -this.width + 26f * NormalizedScreen.pixelScale;
		float max = (float)NormalizedScreen.width - 26f * NormalizedScreen.pixelScale - Blocksworld.UI.TabBar.GetPixelWidth() / NormalizedScreen.scale;
		this.pos.x = Mathf.Clamp(this.pos.x, min, max);
		this.overBuildPanel = (this.pos.x + this.width > (float)NormalizedScreen.width - Blocksworld.UI.SidePanel.BuildPanelWidth());
		base.UpdatePosition();
	}

	// Token: 0x06001CC8 RID: 7368 RVA: 0x000CB3CC File Offset: 0x000C97CC
	public override void Show(bool show)
	{
		this.overlaysDirty = true;
		if (this.ignoreShow)
		{
			this.ignoreShow = false;
			return;
		}
		if (!show)
		{
			TileIconManager.Instance.ClearNewLoadLimit();
			this.overBuildPanel = false;
		}
		if (this.go.activeSelf == show)
		{
			return;
		}
		this.goParent.SetActive(show);
		this.go.SetActive(show);
		this.headerGameObject.SetActive(show);
		bool show2 = show && Blocksworld.buildPanel.IsShowing() && !Tutorial.InTutorialOrPuzzle();
		Blocksworld.tileButtonClearScript.tileObject.Show(show2);
		Blocksworld.tileButtonCopyScript.tileObject.Show(show2);
		Blocksworld.tileButtonPasteScript.tileObject.Show(show2);
		if (!show)
		{
			this.ClearTiles();
		}
	}

	// Token: 0x06001CC9 RID: 7369 RVA: 0x000CB49D File Offset: 0x000C989D
	public void SavePositionForNextLayout()
	{
		this.previousTopScreenPosY = this.pos.y + this.height;
	}

	// Token: 0x06001CCA RID: 7370 RVA: 0x000CB4B8 File Offset: 0x000C98B8
	public void ClearTiles()
	{
		this.SavePositionForNextLayout();
		base.ClearOverlays();
		if (this.tiles != null)
		{
			for (int i = 0; i < this.tiles.Count; i++)
			{
				for (int j = 0; j < this.tiles[i].Count; j++)
				{
					Tile tile = this.tiles[i][j];
					tile.SetParent(null);
					tile.Show(false);
					tile.Destroy();
				}
			}
		}
		this.tiles = new List<List<Tile>>();
		this.overlaysDirty = true;
	}

	// Token: 0x06001CCB RID: 7371 RVA: 0x000CB553 File Offset: 0x000C9953
	public void SetTilesFromBlock(Block block)
	{
		this.ClearTiles();
		this.tiles = block.tiles;
		this.AssignUnparentedTiles();
		this.overlaysDirty = true;
	}

	// Token: 0x06001CCC RID: 7372 RVA: 0x000CB574 File Offset: 0x000C9974
	public void AssignUnparentedTiles()
	{
		if (this.tiles != null)
		{
			for (int i = 0; i < this.tiles.Count; i++)
			{
				for (int j = 0; j < this.tiles[i].Count; j++)
				{
					Tile tile = this.tiles[i][j];
					tile.SetParent(this.goParent.transform);
				}
			}
		}
	}

	// Token: 0x06001CCD RID: 7373 RVA: 0x000CB5EE File Offset: 0x000C99EE
	private void ViewportSizeDidChange()
	{
		if (base.IsShowing())
		{
			this.Layout();
			this.Position();
		}
		else
		{
			this.PositionReset();
		}
	}

	// Token: 0x06001CCE RID: 7374 RVA: 0x000CB612 File Offset: 0x000C9A12
	public override void Move(Vector3 delta)
	{
		base.Move(delta);
		if (delta.sqrMagnitude > Mathf.Epsilon)
		{
			this.UpdateTileVisibility();
		}
	}

	// Token: 0x06001CCF RID: 7375 RVA: 0x000CB632 File Offset: 0x000C9A32
	public override void BeginTrackingTouch()
	{
		TileIconManager.Instance.SetNewLoadLimit(24);
		base.BeginTrackingTouch();
	}

	// Token: 0x06001CD0 RID: 7376 RVA: 0x000CB646 File Offset: 0x000C9A46
	public override void EndTrackingTouch()
	{
		TileIconManager.Instance.ClearNewLoadLimit();
		base.EndTrackingTouch();
	}

	// Token: 0x06001CD1 RID: 7377 RVA: 0x000CB658 File Offset: 0x000C9A58
	private bool IsOnScreen(Tile tile)
	{
		Vector3 vector = tile.GetLocalPosition() + this.pos;
		return vector.x > -100f && vector.x < (float)(NormalizedScreen.width + 100) && vector.y > -100f && vector.y < (float)(NormalizedScreen.height + 100);
	}

	// Token: 0x06001CD2 RID: 7378 RVA: 0x000CB6C4 File Offset: 0x000C9AC4
	private void UpdateTileVisibility()
	{
		for (int i = 1; i < this.tiles.Count; i++)
		{
			List<Tile> list = this.tiles[i];
			for (int j = 0; j < list.Count; j++)
			{
				Tile tile = list[j];
				bool flag = this.IsOnScreen(tile);
				if (flag && !tile.IsShowing())
				{
					if (tile.gaf.Predicate.EditableParameter != null)
					{
						tile.gaf.Predicate.EditableParameter.ApplyTileParameterUI(tile);
					}
					tile.Show(true);
				}
				else if (!flag && tile.IsShowing())
				{
					tile.Show(false);
				}
			}
		}
	}

	// Token: 0x06001CD3 RID: 7379 RVA: 0x000CB784 File Offset: 0x000C9B84
	public void Layout()
	{
		Vector3 position = this.goParent.transform.position;
		this.height = this.extraVerticalTopPadding + (float)((this.tiles.Count - 1) * (base.size + base.margin)) + (float)(this.tiles.Count * base.padding);
		float num = 0f;
		float num2 = (float)(base.size + base.margin);
		float num3 = this.height - this.extraVerticalTopPadding - (float)(base.size + base.margin + base.padding);
		for (int i = 1; i < this.tiles.Count; i++)
		{
			float num4 = 44f;
			List<Tile> list = this.tiles[i];
			for (int j = 0; j < list.Count; j++)
			{
				Tile tile = list[j];
				tile.LocalMoveTo(new Vector3(num4, num3, -0.1f), false);
				num4 += num2;
				if (tile.doubleWidth)
				{
					num4 += num2;
				}
				num = Mathf.Max(num, num4);
			}
			num3 -= (float)(base.size + base.margin + base.padding);
		}
		num = 80f + Mathf.Max(num - (float)(base.margin / 2), (float)(2 * base.margin + 3 * base.size));
		if (this.pos.x < -0.5f * num)
		{
			this.pos.x = -num;
			this.pos.y = (float)NormalizedScreen.height - this.height - 75f * NormalizedScreen.pixelScale;
		}
		else if (this.previousTopScreenPosY > 0f)
		{
			this.pos.y = this.previousTopScreenPosY - this.height;
			this.previousTopScreenPosY = 0f;
		}
		if (this.pos.y - this.height > (float)NormalizedScreen.height - 75f)
		{
			this.pos.y = (float)NormalizedScreen.height - this.height - 75f * NormalizedScreen.pixelScale;
		}
		this.width = num;
		this.expanded = 0f;
		this.overlaysDirty = true;
		this.UpdateTileVisibility();
		this.UpdatePosition();
		this.UpdateTitleBar();
	}

	// Token: 0x06001CD4 RID: 7380 RVA: 0x000CB9E0 File Offset: 0x000C9DE0
	private void UpdateTitleBar()
	{
		float num = this.width + this.expanded;
		this.headerGameObject.transform.localPosition = new Vector3(num * 0.35f, this.height - 5f * NormalizedScreen.pixelScale, 0f);
		this.headerGameObject.transform.localScale = NormalizedScreen.pixelScale * Vector3.one;
		if (this.tiles != null && this.tiles.Count > 1 && this.tiles[1] != null)
		{
			bool enabled = this.tiles.Count != 2 || this.tiles[1].Count != 1;
			bool enabled2 = Blocksworld.clipboard.scriptCopyPasteBuffer.Count > 0;
			float offsetY = -56f * NormalizedScreen.pixelScale;
			this.PositionTileButton(Blocksworld.tileButtonClearScript, num - 120f * NormalizedScreen.pixelScale, offsetY, enabled);
			this.PositionTileButton(Blocksworld.tileButtonCopyScript, -8f * NormalizedScreen.pixelScale, offsetY, enabled);
			this.PositionTileButton(Blocksworld.tileButtonPasteScript, num - 80f * NormalizedScreen.pixelScale, offsetY, enabled2);
		}
	}

	// Token: 0x06001CD5 RID: 7381 RVA: 0x000CBB14 File Offset: 0x000C9F14
	private void PositionTileButton(Tile tileButton, float offsetX, float offsetY, bool enabled)
	{
		if (tileButton.IsShowing())
		{
			Vector3 position = this.go.transform.position;
			tileButton.MoveTo(position.x + offsetX, position.y + this.height + offsetY, this.goParent.transform.position.z - 1f);
			tileButton.SetTileScale(NormalizedScreen.pixelScale);
			tileButton.Enable(enabled);
			tileButton.SetParent(this.goParent.transform);
		}
	}

	// Token: 0x06001CD6 RID: 7382 RVA: 0x000CBBA0 File Offset: 0x000C9FA0
	public void UpdateInner()
	{
		if (!this.trackingTouch)
		{
			this.pos += this.smoothSpeed;
		}
		this.smoothSpeed *= this._momentum;
		if (base.IsShowing())
		{
			this.UpdatePosition();
		}
	}

	// Token: 0x06001CD7 RID: 7383 RVA: 0x000CBBF7 File Offset: 0x000C9FF7
	public void Update()
	{
		this.wasWithinBounds = this.IsWithinBounds();
		base.StepTutorialIfNecessary();
		this.UpdateInner();
	}

	// Token: 0x06001CD8 RID: 7384 RVA: 0x000CBC14 File Offset: 0x000CA014
	public Vector2 SnapTile(Vector3 mouse, Tile tile)
	{
		int num = 0;
		int num2 = 0;
		float num3 = (float)(base.size + base.margin);
		if (this.Hit(mouse) || (this.expanded == 0f && this.Hit(mouse + new Vector3(-num3, 0f, 0f))))
		{
			float num4 = this.height - (float)base.padding - (mouse.y - this.pos.y);
			num = (int)Mathf.Clamp(Mathf.Floor(num4 / (float)(base.size + base.margin + base.padding)), 0f, (float)(this.tiles.Count - 2)) + 1;
			float num5 = mouse.x - this.pos.x - 40f;
			num2 = (int)Mathf.Clamp(Mathf.Floor(num5 / (float)(base.size + base.margin)), 0f, (float)Mathf.Max(1, this.tiles[num].Count));
			List<Tile> list = this.tiles[num];
			bool flag = tile.gaf.AllowTilesAfter();
			int num6 = 0;
			int num7 = -1;
			for (int i = 0; i < list.Count; i++)
			{
				GAF gaf = list[i].gaf;
				if (gaf.Predicate == Block.predicateThen)
				{
					num6 = i;
				}
				else if (!gaf.AllowTilesAfter())
				{
					num7 = i;
				}
			}
			bool flag2 = flag || num7 == -1;
			if (flag2)
			{
				if (!tile.gaf.CanBeAction())
				{
					num2 = Mathf.Min(num2, num6);
				}
				if (!tile.gaf.CanBeCondition())
				{
					num2 = Mathf.Max(num2, num6 + 1);
				}
				if (num7 >= 0)
				{
					num2 = Mathf.Min(num2, num7);
				}
				if (!flag)
				{
					num2 = list.Count;
				}
				float num8 = 44f + (float)(num2 * (base.size + base.margin));
				float f = this.height - this.extraVerticalTopPadding - (float)(num * (base.size + base.margin + base.padding));
				Vector3 localPosition = tile.GetLocalPosition();
				if (Mathf.Floor(num8) != localPosition.x || Mathf.Floor(f) != localPosition.y)
				{
					Sound.PlaySound("Move", Sound.GetOrCreateOneShotAudioSource(), true, 1f, 1f, false);
				}
				tile.SetParent(this.goParent.transform);
				tile.LocalMoveTo(Mathf.Floor(num8), Mathf.Floor(f));
				if (num8 - this.pos.x > this.width - num3)
				{
					this.expanded = num3;
				}
				else
				{
					this.expanded = 0f;
				}
			}
			else
			{
				num2 = 0;
				num = 0;
				this.expanded = 0f;
			}
			this.UpdatePosition();
		}
		else if (this.expanded != 0f)
		{
			this.expanded = 0f;
			this.UpdatePosition();
		}
		float expanded = this.expanded;
		for (int j = 1; j < this.tiles.Count; j++)
		{
			for (int k = 0; k < this.tiles[j].Count; k++)
			{
				float num9 = 0f;
				if (j == num && k >= num2)
				{
					num9 = 1f;
				}
				float num10 = this.pos.x + ((float)k + num9) * (float)(base.size + base.margin) + 40f - (float)(base.margin / 4);
				Tile tile2 = this.tiles[j][k];
				Vector3 position = tile2.GetPosition();
				float num11 = position.x + 0.5f * (num10 - position.x);
				tile2.MoveTo(num11, position.y);
				if (num11 - this.pos.x > this.width - num3)
				{
					this.expanded = num3;
				}
			}
		}
		if (Mathf.Abs(this.expanded - expanded) > Mathf.Epsilon)
		{
			this.UpdatePosition();
		}
		this.overlaysDirty = true;
		return new Vector2((float)num2, (float)num);
	}

	// Token: 0x06001CD9 RID: 7385 RVA: 0x000CC060 File Offset: 0x000CA460
	public override bool TileOnLeftSide(Tile tile)
	{
		for (int i = 0; i < this.tiles.Count; i++)
		{
			List<Tile> list = this.tiles[i];
			bool flag = false;
			for (int j = 0; j < list.Count; j++)
			{
				Tile tile2 = list[j];
				if (tile2 == tile)
				{
					return !flag;
				}
				if (tile2.gaf.Predicate == Block.predicateThen)
				{
					flag = true;
				}
			}
		}
		return false;
	}

	// Token: 0x0400178B RID: 6027
	private const int horizontalPadding = 40;

	// Token: 0x0400178C RID: 6028
	private const int verticalPadding = 20;

	// Token: 0x0400178D RID: 6029
	public bool ignoreShow;

	// Token: 0x0400178E RID: 6030
	public bool overBuildPanel;

	// Token: 0x0400178F RID: 6031
	private float extraVerticalTopPadding;

	// Token: 0x04001790 RID: 6032
	private float previousTopScreenPosY;

	// Token: 0x04001791 RID: 6033
	private GameObject headerGameObject;
}
