using System.Collections.Generic;
using Blocks;
using UnityEngine;

public class ScriptPanel : Panel
{
	private const int horizontalPadding = 40;

	private const int verticalPadding = 20;

	public bool ignoreShow;

	public bool overBuildPanel;

	private float extraVerticalTopPadding;

	private float previousTopScreenPosY;

	private GameObject headerGameObject;

	public ScriptPanel(string name)
		: base(name, 20)
	{
		width = 10000f;
		CreateHeader();
		_momentum = 0f;
		_snapBackMomentum = 0f;
		toggleOverlayEnabled = true;
		parameterOverlayEnabled = true;
		buttonOverlayEnabled = true;
		visibleTileRowStartIndex = 1;
		PositionReset();
		extraVerticalTopPadding = 34f * NormalizedScreen.pixelScale;
		ViewportWatchdog.AddListener(ViewportSizeDidChange);
	}

	public Vector3 TutorialArrowOffset()
	{
		return new Vector3(0f, extraVerticalTopPadding, 0f);
	}

	private void CreateHeader()
	{
		GameObject original = Resources.Load<GameObject>("GUI/Prefab Script Panel Header");
		headerGameObject = Object.Instantiate(original);
		headerGameObject.transform.parent = go.transform;
		headerGameObject.transform.localScale = NormalizedScreen.pixelScale * Vector3.one;
	}

	public void Position()
	{
		if (pos.x < -0.5f * width)
		{
			pos.x = 0f - width;
		}
		UpdatePosition();
	}

	public void PositionReset()
	{
		pos.x = 0f;
		pos.y = (float)NormalizedScreen.height - height - 75f * NormalizedScreen.pixelScale;
		Position();
	}

	public override void UpdatePosition()
	{
		float min = 0f - width + 26f * NormalizedScreen.pixelScale;
		float max = (float)NormalizedScreen.width - 26f * NormalizedScreen.pixelScale - Blocksworld.UI.TabBar.GetPixelWidth() / NormalizedScreen.scale;
		pos.x = Mathf.Clamp(pos.x, min, max);
		overBuildPanel = pos.x + width > (float)NormalizedScreen.width - Blocksworld.UI.SidePanel.BuildPanelWidth();
		base.UpdatePosition();
	}

	public override void Show(bool show)
	{
		overlaysDirty = true;
		if (ignoreShow)
		{
			ignoreShow = false;
			return;
		}
		if (!show)
		{
			TileIconManager.Instance.ClearNewLoadLimit();
			overBuildPanel = false;
		}
		if (go.activeSelf != show)
		{
			goParent.SetActive(show);
			go.SetActive(show);
			headerGameObject.SetActive(show);
			bool show2 = show && Blocksworld.buildPanel.IsShowing() && !Tutorial.InTutorialOrPuzzle();
			Blocksworld.tileButtonClearScript.tileObject.Show(show2);
			Blocksworld.tileButtonCopyScript.tileObject.Show(show2);
			Blocksworld.tileButtonPasteScript.tileObject.Show(show2);
			if (!show)
			{
				ClearTiles();
			}
		}
	}

	public void SavePositionForNextLayout()
	{
		previousTopScreenPosY = pos.y + height;
	}

	public void ClearTiles()
	{
		SavePositionForNextLayout();
		ClearOverlays();
		if (tiles != null)
		{
			for (int i = 0; i < tiles.Count; i++)
			{
				for (int j = 0; j < tiles[i].Count; j++)
				{
					Tile tile = tiles[i][j];
					tile.SetParent(null);
					tile.Show(show: false);
					tile.Destroy();
				}
			}
		}
		tiles = new List<List<Tile>>();
		overlaysDirty = true;
	}

	public void SetTilesFromBlock(Block block)
	{
		ClearTiles();
		tiles = block.tiles;
		AssignUnparentedTiles();
		overlaysDirty = true;
	}

	public void AssignUnparentedTiles()
	{
		if (tiles == null)
		{
			return;
		}
		for (int i = 0; i < tiles.Count; i++)
		{
			for (int j = 0; j < tiles[i].Count; j++)
			{
				Tile tile = tiles[i][j];
				tile.SetParent(goParent.transform);
			}
		}
	}

	private void ViewportSizeDidChange()
	{
		if (IsShowing())
		{
			Layout();
			Position();
		}
		else
		{
			PositionReset();
		}
	}

	public override void Move(Vector3 delta)
	{
		base.Move(delta);
		if (delta.sqrMagnitude > Mathf.Epsilon)
		{
			UpdateTileVisibility();
		}
	}

	public override void BeginTrackingTouch()
	{
		TileIconManager.Instance.SetNewLoadLimit(24);
		base.BeginTrackingTouch();
	}

	public override void EndTrackingTouch()
	{
		TileIconManager.Instance.ClearNewLoadLimit();
		base.EndTrackingTouch();
	}

	private bool IsOnScreen(Tile tile)
	{
		Vector3 vector = tile.GetLocalPosition() + pos;
		if (vector.x > -100f && vector.x < (float)(NormalizedScreen.width + 100) && vector.y > -100f)
		{
			return vector.y < (float)(NormalizedScreen.height + 100);
		}
		return false;
	}

	private void UpdateTileVisibility()
	{
		for (int i = 1; i < tiles.Count; i++)
		{
			List<Tile> list = tiles[i];
			for (int j = 0; j < list.Count; j++)
			{
				Tile tile = list[j];
				bool flag = IsOnScreen(tile);
				if (flag && !tile.IsShowing())
				{
					if (tile.gaf.Predicate.EditableParameter != null)
					{
						tile.gaf.Predicate.EditableParameter.ApplyTileParameterUI(tile);
					}
					tile.Show(show: true);
				}
				else if (!flag && tile.IsShowing())
				{
					tile.Show(show: false);
				}
			}
		}
	}

	public void Layout()
	{
		Vector3 vector = goParent.transform.position;
		height = extraVerticalTopPadding + (float)((tiles.Count - 1) * (base.size + base.margin)) + (float)(tiles.Count * base.padding);
		float num = 0f;
		float num2 = base.size + base.margin;
		float num3 = height - extraVerticalTopPadding - (float)(base.size + base.margin + base.padding);
		for (int i = 1; i < tiles.Count; i++)
		{
			float num4 = 44f;
			List<Tile> list = tiles[i];
			for (int j = 0; j < list.Count; j++)
			{
				Tile tile = list[j];
				tile.LocalMoveTo(new Vector3(num4, num3, -0.1f));
				num4 += num2;
				if (tile.doubleWidth)
				{
					num4 += num2;
				}
				num = Mathf.Max(num, num4);
			}
			num3 -= (float)(base.size + base.margin + base.padding);
		}
		num = 80f + Mathf.Max(num - (float)(base.margin / 2), 2 * base.margin + 3 * base.size);
		if (pos.x < -0.5f * num)
		{
			pos.x = 0f - num;
			pos.y = (float)NormalizedScreen.height - height - 75f * NormalizedScreen.pixelScale;
		}
		else if (previousTopScreenPosY > 0f)
		{
			pos.y = previousTopScreenPosY - height;
			previousTopScreenPosY = 0f;
		}
		if (pos.y - height > (float)NormalizedScreen.height - 75f)
		{
			pos.y = (float)NormalizedScreen.height - height - 75f * NormalizedScreen.pixelScale;
		}
		width = num;
		expanded = 0f;
		overlaysDirty = true;
		UpdateTileVisibility();
		UpdatePosition();
		UpdateTitleBar();
	}

	private void UpdateTitleBar()
	{
		float num = width + expanded;
		headerGameObject.transform.localPosition = new Vector3(num * 0.35f, height - 5f * NormalizedScreen.pixelScale, 0f);
		headerGameObject.transform.localScale = NormalizedScreen.pixelScale * Vector3.one;
		if (tiles != null && tiles.Count > 1 && tiles[1] != null)
		{
			bool enabled = tiles.Count != 2 || tiles[1].Count != 1;
			bool enabled2 = Blocksworld.clipboard.scriptCopyPasteBuffer.Count > 0;
			float offsetY = -56f * NormalizedScreen.pixelScale;
			PositionTileButton(Blocksworld.tileButtonClearScript, num - 120f * NormalizedScreen.pixelScale, offsetY, enabled);
			PositionTileButton(Blocksworld.tileButtonCopyScript, -8f * NormalizedScreen.pixelScale, offsetY, enabled);
			PositionTileButton(Blocksworld.tileButtonPasteScript, num - 80f * NormalizedScreen.pixelScale, offsetY, enabled2);
		}
	}

	private void PositionTileButton(Tile tileButton, float offsetX, float offsetY, bool enabled)
	{
		if (tileButton.IsShowing())
		{
			Vector3 vector = go.transform.position;
			tileButton.MoveTo(vector.x + offsetX, vector.y + height + offsetY, goParent.transform.position.z - 1f);
			tileButton.SetTileScale(NormalizedScreen.pixelScale);
			tileButton.Enable(enabled);
			tileButton.SetParent(goParent.transform);
		}
	}

	public void UpdateInner()
	{
		if (!trackingTouch)
		{
			pos += smoothSpeed;
		}
		smoothSpeed *= _momentum;
		if (IsShowing())
		{
			UpdatePosition();
		}
	}

	public void Update()
	{
		wasWithinBounds = IsWithinBounds();
		StepTutorialIfNecessary();
		UpdateInner();
	}

	public Vector2 SnapTile(Vector3 mouse, Tile tile)
	{
		int num = 0;
		int num2 = 0;
		float num3 = base.size + base.margin;
		if (Hit(mouse) || (expanded == 0f && Hit(mouse + new Vector3(0f - num3, 0f, 0f))))
		{
			float num4 = height - (float)base.padding - (mouse.y - pos.y);
			num = (int)Mathf.Clamp(Mathf.Floor(num4 / (float)(base.size + base.margin + base.padding)), 0f, tiles.Count - 2) + 1;
			float num5 = mouse.x - pos.x - 40f;
			num2 = (int)Mathf.Clamp(Mathf.Floor(num5 / (float)(base.size + base.margin)), 0f, Mathf.Max(1, tiles[num].Count));
			List<Tile> list = tiles[num];
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
			if (flag || num7 == -1)
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
				float f = height - extraVerticalTopPadding - (float)(num * (base.size + base.margin + base.padding));
				Vector3 localPosition = tile.GetLocalPosition();
				if (Mathf.Floor(num8) != localPosition.x || Mathf.Floor(f) != localPosition.y)
				{
					Sound.PlaySound("Move", Sound.GetOrCreateOneShotAudioSource(), oneShot: true);
				}
				tile.SetParent(goParent.transform);
				tile.LocalMoveTo(Mathf.Floor(num8), Mathf.Floor(f));
				if (num8 - pos.x > width - num3)
				{
					expanded = num3;
				}
				else
				{
					expanded = 0f;
				}
			}
			else
			{
				num2 = 0;
				num = 0;
				expanded = 0f;
			}
			UpdatePosition();
		}
		else if (expanded != 0f)
		{
			expanded = 0f;
			UpdatePosition();
		}
		float num9 = expanded;
		for (int j = 1; j < tiles.Count; j++)
		{
			for (int k = 0; k < tiles[j].Count; k++)
			{
				float num10 = 0f;
				if (j == num && k >= num2)
				{
					num10 = 1f;
				}
				float num11 = pos.x + ((float)k + num10) * (float)(base.size + base.margin) + 40f - (float)(base.margin / 4);
				Tile tile2 = tiles[j][k];
				Vector3 vector = tile2.GetPosition();
				float num12 = vector.x + 0.5f * (num11 - vector.x);
				tile2.MoveTo(num12, vector.y);
				if (num12 - pos.x > width - num3)
				{
					expanded = num3;
				}
			}
		}
		if (Mathf.Abs(expanded - num9) > Mathf.Epsilon)
		{
			UpdatePosition();
		}
		overlaysDirty = true;
		return new Vector2(num2, num);
	}

	public override bool TileOnLeftSide(Tile tile)
	{
		for (int i = 0; i < tiles.Count; i++)
		{
			List<Tile> list = tiles[i];
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
}
