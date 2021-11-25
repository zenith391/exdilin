using System;
using System.Collections.Generic;
using Blocks;
using Gestures;
using UnityEngine;

// Token: 0x02000263 RID: 611
public class Panel
{
	// Token: 0x06001CA0 RID: 7328 RVA: 0x000C7ED0 File Offset: 0x000C62D0
	public Panel(string name, int padding)
	{
		this.basePadding = padding;
		if (name == "Script Panel")
		{
			this.go = (UnityEngine.Object.Instantiate(Resources.Load("GUI/Prefab Script Panel")) as GameObject);
		}
		else
		{
			this.go = (UnityEngine.Object.Instantiate(Resources.Load("GUI/Prefab Panel")) as GameObject);
		}
		if (Panel.highlightTexture == null)
		{
			Panel.highlightTexture = (Resources.Load("GUI/Texture Panel Highlight") as Texture2D);
		}
		if (Panel.normalTexture == null)
		{
			Panel.normalTexture = (Texture2D)this.go.GetComponent<Renderer>().material.mainTexture;
		}
		this.go.name = name;
		this.goParent = new GameObject();
		this.goParent.name = name + " Tiles";
		this.goParent.transform.parent = this.go.transform;
		this.meshBackground = this.go.GetComponent<MeshFilter>().mesh;
		this.useMeshBackground &= (this.meshBackground != null);
	}

	// Token: 0x06001CA1 RID: 7329 RVA: 0x000C8090 File Offset: 0x000C6490
	public void ClearOverlays()
	{
		foreach (KeyValuePair<Tile, List<Tile>> keyValuePair in this.tileOverlays)
		{
			foreach (Tile tile in keyValuePair.Value)
			{
				tile.Show(false);
			}
		}
		this.tileOverlays.Clear();
		this.overlaysDirty = true;
	}

	// Token: 0x1700012F RID: 303
	// (get) Token: 0x06001CA2 RID: 7330 RVA: 0x000C8144 File Offset: 0x000C6544
	protected int size
	{
		get
		{
			return (int)((float)this.baseTileSize * NormalizedScreen.pixelScale);
		}
	}

	// Token: 0x17000130 RID: 304
	// (get) Token: 0x06001CA3 RID: 7331 RVA: 0x000C8154 File Offset: 0x000C6554
	protected int margin
	{
		get
		{
			return (int)((float)this.baseMargin * NormalizedScreen.pixelScale);
		}
	}

	// Token: 0x17000131 RID: 305
	// (get) Token: 0x06001CA4 RID: 7332 RVA: 0x000C8164 File Offset: 0x000C6564
	protected int padding
	{
		get
		{
			return (int)((float)this.basePadding * NormalizedScreen.pixelScale);
		}
	}

	// Token: 0x06001CA5 RID: 7333 RVA: 0x000C8174 File Offset: 0x000C6574
	public void UpdateAllOverlays()
	{
		if (this.tiles != null)
		{
			this.overlaysDirty = false;
			HashSet<Tile> hashSet = new HashSet<Tile>();
			for (int i = this.visibleTileRowStartIndex; i < this.tiles.Count; i++)
			{
				List<Tile> list = this.tiles[i];
				if (list.Count != 0)
				{
					for (int j = 0; j < list.Count; j++)
					{
						Tile tile = list[j];
						if (tile.gaf.Predicate.canHaveOverlay)
						{
							if (tile.IsShowing())
							{
								this.UpdateOverlay(tile);
								hashSet.Add(tile);
							}
						}
					}
				}
			}
			foreach (Tile tile2 in new List<Tile>(this.tileOverlays.Keys))
			{
				if (!hashSet.Contains(tile2))
				{
					List<Tile> list2 = this.tileOverlays[tile2];
					this.ShowTiles(list2, false);
					this.tileOverlays.Remove(tile2);
				}
			}
		}
	}

	// Token: 0x06001CA6 RID: 7334 RVA: 0x000C82B8 File Offset: 0x000C66B8
	public Tile CreateTileInPanel(GAF gaf)
	{
		Tile tile = new Tile(gaf);
		tile.AssignToPanel(this);
		return tile;
	}

	// Token: 0x06001CA7 RID: 7335 RVA: 0x000C82D4 File Offset: 0x000C66D4
	protected void ShowTiles(List<Tile> tiles, bool s)
	{
		for (int i = 0; i < tiles.Count; i++)
		{
			tiles[i].Show(s);
		}
		this.overlaysDirty = true;
	}

	// Token: 0x06001CA8 RID: 7336 RVA: 0x000C830C File Offset: 0x000C670C
	public void UpdateOverlay(Tile tile)
	{
		bool flag = this.tileOverlays.ContainsKey(tile);
		List<Tile> list = null;
		if (flag)
		{
			list = this.tileOverlays[tile];
		}
		GAF gaf = tile.gaf;
		EditableTileParameter editableParameter = gaf.Predicate.EditableParameter;
		bool flag2 = this.parameterOverlayEnabled && editableParameter != null && (!editableParameter.settings.hideOnLeftSide || !this.TileOnLeftSide(tile));
		bool flag3 = this.buttonOverlayEnabled && tile.gaf.Predicate == Block.predicateButton;
		bool flag4 = this.toggleOverlayEnabled && TileToggleChain.HasMoreThanOneUnlocked(gaf);
		bool flag5 = flag3 || flag2 || flag4;
		if (flag5)
		{
			if (tile.tileObject == null)
			{
				if (flag)
				{
					this.ShowTiles(list, false);
					this.tileOverlays.Remove(tile);
				}
			}
			else
			{
				HashSet<string> hashSet = new HashSet<string>();
				int blockItemId = tile.gaf.BlockItemId;
				int unlockedCount = TileToggleChain.GetUnlockedCount(blockItemId);
				int lastUnlockedBlockItemIdFor = TileToggleChain.GetLastUnlockedBlockItemIdFor(blockItemId);
				if (flag2 && gaf.Predicate.EditableParameter != null)
				{
					if (tile.doubleWidth && tile.subParameterCount > 1)
					{
						if (tile.subParameterIndex != tile.subParameterCount - 1)
						{
							hashSet.Add("TileOverlays/First_Toggle");
						}
						else
						{
							hashSet.Add("TileOverlays/Last_Toggle");
						}
					}
					else
					{
						hashSet.Add("TileOverlays/Icn_Parameter_Edit");
					}
				}
				else if (flag4 && unlockedCount == 2 && lastUnlockedBlockItemIdFor.Equals(blockItemId))
				{
					hashSet.Add("TileOverlays/Last_Toggle");
				}
				else if (flag4)
				{
					hashSet.Add("TileOverlays/First_Toggle");
				}
				if (!flag)
				{
					list = new List<Tile>();
					foreach (string iconName in hashSet)
					{
						Tile tile2 = new Tile(Blocksworld.tilePool.GetTileObjectForIcon(iconName, true));
						tile2.Show(true);
						tile2.SetTileScale(NormalizedScreen.pixelScale);
						list.Add(tile2);
					}
					this.tileOverlays[tile] = list;
				}
				else
				{
					foreach (Tile tile3 in new List<Tile>(list))
					{
						string item = tile3.tileObject.IconName();
						if (!hashSet.Contains(item))
						{
							tile3.Show(false);
							list.Remove(tile3);
						}
						else
						{
							tile3.Show(true);
							tile3.SetTileScale(NormalizedScreen.pixelScale);
							hashSet.Remove(item);
						}
					}
					foreach (string iconName2 in hashSet)
					{
						Tile tile4 = new Tile(Blocksworld.tilePool.GetTileObjectForIcon(iconName2, true));
						tile4.Show(true);
						list.Add(tile4);
					}
				}
				Vector3 position = tile.tileObject.GetPosition();
				foreach (Tile tile5 in list)
				{
					tile5.Show(true);
					tile5.MoveTo(position.x, position.y, position.z - 0.2f);
				}
			}
		}
		else if (flag)
		{
			this.tileOverlays.Remove(tile);
			this.ShowTiles(list, false);
		}
	}

	// Token: 0x06001CA9 RID: 7337 RVA: 0x000C8714 File Offset: 0x000C6B14
	public void Highlight(bool h)
	{
		this.go.GetComponent<Renderer>().material.mainTexture = ((!h) ? Panel.normalTexture : Panel.highlightTexture);
	}

	// Token: 0x17000132 RID: 306
	// (get) Token: 0x06001CAA RID: 7338 RVA: 0x000C8740 File Offset: 0x000C6B40
	public Vector3 position
	{
		get
		{
			return this.pos;
		}
	}

	// Token: 0x17000133 RID: 307
	// (get) Token: 0x06001CAB RID: 7339 RVA: 0x000C8748 File Offset: 0x000C6B48
	// (set) Token: 0x06001CAC RID: 7340 RVA: 0x000C876B File Offset: 0x000C6B6B
	public Vector3 upperRight
	{
		get
		{
			return this.pos + new Vector3(this.width, this.height, 0f);
		}
		set
		{
			this.SetPosition(value - new Vector3(this.width, this.height, 0f));
		}
	}

	// Token: 0x06001CAD RID: 7341 RVA: 0x000C878F File Offset: 0x000C6B8F
	public Vector3 GetSpeed()
	{
		return this.smoothSpeed;
	}

	// Token: 0x06001CAE RID: 7342 RVA: 0x000C8797 File Offset: 0x000C6B97
	public void SetPosition(Vector3 p)
	{
		this.pos = p;
		this.tutCheckInterval = 5;
	}

	// Token: 0x06001CAF RID: 7343 RVA: 0x000C87A7 File Offset: 0x000C6BA7
	public virtual Transform GetTransform()
	{
		return this.go.transform;
	}

	// Token: 0x06001CB0 RID: 7344 RVA: 0x000C87B4 File Offset: 0x000C6BB4
	protected void StepTutorialIfNecessary()
	{
		float magnitude = this.smoothSpeed.magnitude;
		if (magnitude > 0.03f && Blocksworld.mouseBlock == null && magnitude <= 25f)
		{
			if ((Tutorial.state == TutorialState.Scroll || Tutorial.state == TutorialState.Color || Tutorial.state == TutorialState.CreateBlock || Tutorial.state == TutorialState.AddTile || Tutorial.state == TutorialState.Texture) && this.tutCheckCounter % this.tutCheckInterval == 0)
			{
				Tutorial.Step();
				this.tutCheckCounter = 0;
				this.tutCheckInterval += 3;
				if (this.tutCheckInterval > 60)
				{
					this.tutCheckInterval = 60;
				}
			}
			this.tutCheckCounter++;
		}
	}

	// Token: 0x06001CB1 RID: 7345 RVA: 0x000C8878 File Offset: 0x000C6C78
	public void UpdateGestureRecognizer(GestureRecognizer recognizer)
	{
		foreach (BaseGesture gesture in this.gestures)
		{
			recognizer.RemoveGesture(gesture);
		}
		this.gestures.Clear();
	}

	// Token: 0x06001CB2 RID: 7346 RVA: 0x000C88E0 File Offset: 0x000C6CE0
	public virtual void Show(bool show)
	{
		this.overlaysDirty = true;
		if (this.go.activeSelf == show)
		{
			return;
		}
		if (this.tiles != null)
		{
			for (int i = 0; i < this.tiles.Count; i++)
			{
				for (int j = 0; j < this.tiles[i].Count; j++)
				{
					this.tiles[i][j].Show(show);
				}
			}
		}
		this.goParent.SetActive(show);
		this.go.SetActive(show);
	}

	// Token: 0x06001CB3 RID: 7347 RVA: 0x000C897F File Offset: 0x000C6D7F
	public bool IsShowing()
	{
		return this.go.activeSelf;
	}

	// Token: 0x06001CB4 RID: 7348 RVA: 0x000C898C File Offset: 0x000C6D8C
	public virtual bool Hit(Vector3 v)
	{
		return this.go.activeSelf && Mathf.Max(0f, v.x) >= this.pos.x && v.x <= this.pos.x + this.width + this.expanded && v.y >= this.pos.y && v.y <= this.pos.y + this.height;
	}

	// Token: 0x06001CB5 RID: 7349 RVA: 0x000C8A28 File Offset: 0x000C6E28
	public virtual Tile HitTile(Vector3 pos, bool allowDisabledTiles = false)
	{
		if (this.tiles != null && this.go.activeSelf)
		{
			for (int i = 0; i < this.tiles.Count; i++)
			{
				List<Tile> list = this.tiles[i];
				for (int j = list.Count - 1; j >= 0; j--)
				{
					Tile tile = list[j];
					if (tile.tileObject != null && tile.Hit(pos, allowDisabledTiles))
					{
						return tile;
					}
				}
			}
		}
		return null;
	}

	// Token: 0x06001CB6 RID: 7350 RVA: 0x000C8ABC File Offset: 0x000C6EBC
	public virtual void BeginTrackingTouch()
	{
		this.trackingTouch = true;
		this.lastSmoothSpeed2 = (this.lastSmoothSpeed1 = (this.smoothSpeed = Vector3.zero));
		this.smoothSpeedStep = 0;
	}

	// Token: 0x06001CB7 RID: 7351 RVA: 0x000C8AF4 File Offset: 0x000C6EF4
	public virtual void EndTrackingTouch()
	{
		this.trackingTouch = false;
	}

	// Token: 0x06001CB8 RID: 7352 RVA: 0x000C8AFD File Offset: 0x000C6EFD
	public virtual bool IsWithinBounds()
	{
		return true;
	}

	// Token: 0x06001CB9 RID: 7353 RVA: 0x000C8B00 File Offset: 0x000C6F00
	public virtual void Move(Vector3 delta)
	{
		this.snappingBack = false;
		this.tutCheckInterval = 5;
		this.speedDirty = true;
		if (this.trackingTouch)
		{
			this.pos += ((!this.IsWithinBounds()) ? 0.5f : 1f) * delta;
			this.smoothSpeed = (1.25f * delta + 0.75f * this.lastSmoothSpeed1 + 0.5f * this.lastSmoothSpeed2) / 2.5f;
			this.lastSmoothSpeed1 = this.smoothSpeed;
			this.lastSmoothSpeed2 = ((this.smoothSpeedStep != 0) ? this.lastSmoothSpeed1 : this.smoothSpeed);
			this.smoothSpeedStep++;
		}
		else
		{
			this.smoothSpeed = delta;
			this.lastSmoothSpeed1 = delta;
			this.lastSmoothSpeed2 = delta;
		}
	}

	// Token: 0x06001CBA RID: 7354 RVA: 0x000C8C00 File Offset: 0x000C7000
	public void ScrollToVisible(Tile tile, bool immediately = false, bool showTileAtTopOfScreen = false, bool forceScroll = false)
	{
		if (tile == null)
		{
			return;
		}
		Vector3 min = tile.GetHitBounds().min;
		float num = (!showTileAtTopOfScreen) ? 0f : ((float)Screen.height - 320f);
		if (min.y - num < 0f || forceScroll)
		{
			Vector3 v = new Vector3(this.pos.x, this.pos.y - min.y + 240f + num, this.pos.z);
			this.MoveTo(v, immediately, false);
		}
	}

	// Token: 0x06001CBB RID: 7355 RVA: 0x000C8C9B File Offset: 0x000C709B
	public float GetScrollPos()
	{
		return this.pos.y;
	}

	// Token: 0x06001CBC RID: 7356 RVA: 0x000C8CA8 File Offset: 0x000C70A8
	public void SetScrollPos(float scrollPos)
	{
		this.MoveTo(new Vector3(this.pos.x, scrollPos, this.pos.z), true, false);
	}

	// Token: 0x06001CBD RID: 7357 RVA: 0x000C8CD0 File Offset: 0x000C70D0
	public void MoveTo(Vector3 v, bool immediately = false, bool snapBack = false)
	{
		if (immediately)
		{
			this.pos = v;
			this.UpdatePosition();
			this.snappingBack = false;
			this.lastSmoothSpeed2 = (this.lastSmoothSpeed1 = (this.smoothSpeed = Vector3.zero));
		}
		else if (snapBack)
		{
			this.Move((v - this.pos) * (1f - this._snapBackMomentum));
			this.snappingBack = true;
		}
		else
		{
			this.Move((v - this.pos) * (1f - ((!this.snappingBack) ? this._momentum : this._snapBackMomentum)));
		}
	}

	// Token: 0x06001CBE RID: 7358 RVA: 0x000C8D87 File Offset: 0x000C7187
	protected virtual float PanelYOffset()
	{
		return 0f;
	}

	// Token: 0x06001CBF RID: 7359 RVA: 0x000C8D90 File Offset: 0x000C7190
	public virtual void UpdatePosition()
	{
		Vector3 position = new Vector3(Mathf.Floor(this.pos.x), Mathf.Floor(this.pos.y), this.depth);
		this.go.transform.position = position;
		if (this.useMeshBackground)
		{
			Util.SetGridMeshSize(this.meshBackground, this.width + this.expanded, this.height, 32f * NormalizedScreen.pixelScale, this.PanelYOffset());
		}
		if (position.y > (float)NormalizedScreen.height - 70f - this.PanelYOffset())
		{
			this.MoveTo(new Vector3(position.x, (float)NormalizedScreen.height - 70f - 1f - this.PanelYOffset(), position.z), true, false);
		}
		if (this.overlaysDirty || Mathf.Abs(this.lastOverlaysHeight - this.height) > 0.01f || Mathf.Abs(this.lastOverlaysWidth - this.width) > 0.01f || (this.lastOverlaysPos - this.pos).sqrMagnitude > 0.0001f)
		{
			this.lastOverlaysPos = this.pos;
			this.lastOverlaysHeight = this.height;
			this.lastOverlaysWidth = this.width;
			this.UpdateAllOverlays();
		}
	}

	// Token: 0x06001CC0 RID: 7360 RVA: 0x000C8EF8 File Offset: 0x000C72F8
	public Tile FindFirstTileWithPredicate(HashSet<Predicate> preds)
	{
		Tile result = null;
		if (this.tiles != null)
		{
			for (int i = 0; i < this.tiles.Count; i++)
			{
				List<Tile> list = this.tiles[i];
				for (int j = 0; j < list.Count; j++)
				{
					Tile tile = list[j];
					if (preds.Contains(tile.gaf.Predicate))
					{
						return tile;
					}
				}
			}
		}
		return result;
	}

	// Token: 0x06001CC1 RID: 7361 RVA: 0x000C8F76 File Offset: 0x000C7376
	public virtual bool TileOnLeftSide(Tile tile)
	{
		return false;
	}

	// Token: 0x04001765 RID: 5989
	public List<List<Tile>> tiles;

	// Token: 0x04001766 RID: 5990
	public float height;

	// Token: 0x04001767 RID: 5991
	public float width;

	// Token: 0x04001768 RID: 5992
	public float depth = 10f;

	// Token: 0x04001769 RID: 5993
	protected float _momentum = 0.94f;

	// Token: 0x0400176A RID: 5994
	protected float _snapBackMomentum = 0.85f;

	// Token: 0x0400176B RID: 5995
	protected GameObject go;

	// Token: 0x0400176C RID: 5996
	protected GameObject goParent;

	// Token: 0x0400176D RID: 5997
	protected int baseTileSize = 80;

	// Token: 0x0400176E RID: 5998
	protected int baseMargin = Blocksworld.marginTile;

	// Token: 0x0400176F RID: 5999
	protected int basePadding;

	// Token: 0x04001770 RID: 6000
	protected float expanded;

	// Token: 0x04001771 RID: 6001
	protected Vector3 pos = Vector3.zero;

	// Token: 0x04001772 RID: 6002
	private int tutCheckInterval = 5;

	// Token: 0x04001773 RID: 6003
	private int tutCheckCounter;

	// Token: 0x04001774 RID: 6004
	protected bool speedDirty;

	// Token: 0x04001775 RID: 6005
	public bool trackingTouch;

	// Token: 0x04001776 RID: 6006
	protected Vector3 smoothSpeed;

	// Token: 0x04001777 RID: 6007
	protected Vector3 lastSmoothSpeed1;

	// Token: 0x04001778 RID: 6008
	protected Vector3 lastSmoothSpeed2;

	// Token: 0x04001779 RID: 6009
	private int smoothSpeedStep;

	// Token: 0x0400177A RID: 6010
	protected bool snappingBack;

	// Token: 0x0400177B RID: 6011
	protected bool wasWithinBounds = true;

	// Token: 0x0400177C RID: 6012
	private List<BaseGesture> gestures = new List<BaseGesture>();

	// Token: 0x0400177D RID: 6013
	protected Dictionary<Tile, List<Tile>> tileOverlays = new Dictionary<Tile, List<Tile>>();

	// Token: 0x0400177E RID: 6014
	protected bool overlaysDirty = true;

	// Token: 0x0400177F RID: 6015
	protected Vector3 lastOverlaysPos = Util.nullVector3;

	// Token: 0x04001780 RID: 6016
	protected float lastOverlaysHeight = -1f;

	// Token: 0x04001781 RID: 6017
	protected float lastOverlaysWidth = -1f;

	// Token: 0x04001782 RID: 6018
	private static Texture2D highlightTexture;

	// Token: 0x04001783 RID: 6019
	private static Texture2D normalTexture;

	// Token: 0x04001784 RID: 6020
	protected bool parameterOverlayEnabled;

	// Token: 0x04001785 RID: 6021
	protected bool buttonOverlayEnabled;

	// Token: 0x04001786 RID: 6022
	protected bool toggleOverlayEnabled;

	// Token: 0x04001787 RID: 6023
	protected int visibleTileRowStartIndex;

	// Token: 0x04001788 RID: 6024
	protected Mesh meshBackground;

	// Token: 0x04001789 RID: 6025
	protected bool useMeshBackground = true;

	// Token: 0x0400178A RID: 6026
	public TilePool tileObjectPool;
}
