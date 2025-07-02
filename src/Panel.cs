using System.Collections.Generic;
using Blocks;
using Gestures;
using UnityEngine;

public class Panel
{
	public List<List<Tile>> tiles;

	public float height;

	public float width;

	public float depth = 10f;

	protected float _momentum = 0.94f;

	protected float _snapBackMomentum = 0.85f;

	protected GameObject go;

	protected GameObject goParent;

	protected int baseTileSize = 80;

	protected int baseMargin = Blocksworld.marginTile;

	protected int basePadding;

	protected float expanded;

	protected Vector3 pos = Vector3.zero;

	private int tutCheckInterval = 5;

	private int tutCheckCounter;

	protected bool speedDirty;

	public bool trackingTouch;

	protected Vector3 smoothSpeed;

	protected Vector3 lastSmoothSpeed1;

	protected Vector3 lastSmoothSpeed2;

	private int smoothSpeedStep;

	protected bool snappingBack;

	protected bool wasWithinBounds = true;

	private List<BaseGesture> gestures = new List<BaseGesture>();

	protected Dictionary<Tile, List<Tile>> tileOverlays = new Dictionary<Tile, List<Tile>>();

	protected bool overlaysDirty = true;

	protected Vector3 lastOverlaysPos = Util.nullVector3;

	protected float lastOverlaysHeight = -1f;

	protected float lastOverlaysWidth = -1f;

	private static Texture2D highlightTexture;

	private static Texture2D normalTexture;

	protected bool parameterOverlayEnabled;

	protected bool buttonOverlayEnabled;

	protected bool toggleOverlayEnabled;

	protected int visibleTileRowStartIndex;

	protected Mesh meshBackground;

	protected bool useMeshBackground = true;

	public TilePool tileObjectPool;

	protected int size => (int)((float)baseTileSize * NormalizedScreen.pixelScale);

	protected int margin => (int)((float)baseMargin * NormalizedScreen.pixelScale);

	protected int padding => (int)((float)basePadding * NormalizedScreen.pixelScale);

	public Vector3 position => pos;

	public Vector3 upperRight
	{
		get
		{
			return pos + new Vector3(width, height, 0f);
		}
		set
		{
			SetPosition(value - new Vector3(width, height, 0f));
		}
	}

	public Panel(string name, int padding)
	{
		basePadding = padding;
		if (name == "Script Panel")
		{
			go = Object.Instantiate(Resources.Load("GUI/Prefab Script Panel")) as GameObject;
		}
		else
		{
			go = Object.Instantiate(Resources.Load("GUI/Prefab Panel")) as GameObject;
		}
		if (highlightTexture == null)
		{
			highlightTexture = Resources.Load("GUI/Texture Panel Highlight") as Texture2D;
		}
		if (normalTexture == null)
		{
			normalTexture = (Texture2D)go.GetComponent<Renderer>().material.mainTexture;
		}
		go.name = name;
		goParent = new GameObject();
		goParent.name = name + " Tiles";
		goParent.transform.parent = go.transform;
		meshBackground = go.GetComponent<MeshFilter>().mesh;
		useMeshBackground &= meshBackground != null;
	}

	public void ClearOverlays()
	{
		foreach (KeyValuePair<Tile, List<Tile>> tileOverlay in tileOverlays)
		{
			foreach (Tile item in tileOverlay.Value)
			{
				item.Show(show: false);
			}
		}
		tileOverlays.Clear();
		overlaysDirty = true;
	}

	public void UpdateAllOverlays()
	{
		if (tiles == null)
		{
			return;
		}
		overlaysDirty = false;
		HashSet<Tile> hashSet = new HashSet<Tile>();
		for (int i = visibleTileRowStartIndex; i < tiles.Count; i++)
		{
			List<Tile> list = tiles[i];
			if (list.Count == 0)
			{
				continue;
			}
			for (int j = 0; j < list.Count; j++)
			{
				Tile tile = list[j];
				if (tile.gaf.Predicate.canHaveOverlay && tile.IsShowing())
				{
					UpdateOverlay(tile);
					hashSet.Add(tile);
				}
			}
		}
		foreach (Tile item in new List<Tile>(tileOverlays.Keys))
		{
			if (!hashSet.Contains(item))
			{
				List<Tile> list2 = tileOverlays[item];
				ShowTiles(list2, s: false);
				tileOverlays.Remove(item);
			}
		}
	}

	public Tile CreateTileInPanel(GAF gaf)
	{
		Tile tile = new Tile(gaf);
		tile.AssignToPanel(this);
		return tile;
	}

	protected void ShowTiles(List<Tile> tiles, bool s)
	{
		for (int i = 0; i < tiles.Count; i++)
		{
			tiles[i].Show(s);
		}
		overlaysDirty = true;
	}

	public void UpdateOverlay(Tile tile)
	{
		bool flag = tileOverlays.ContainsKey(tile);
		List<Tile> list = null;
		if (flag)
		{
			list = tileOverlays[tile];
		}
		GAF gaf = tile.gaf;
		EditableTileParameter editableParameter = gaf.Predicate.EditableParameter;
		bool flag2 = parameterOverlayEnabled && editableParameter != null && (!editableParameter.settings.hideOnLeftSide || !TileOnLeftSide(tile));
		bool flag3 = buttonOverlayEnabled && tile.gaf.Predicate == Block.predicateButton;
		bool flag4 = toggleOverlayEnabled && TileToggleChain.HasMoreThanOneUnlocked(gaf);
		if (flag3 || flag2 || flag4)
		{
			if (!(tile.tileObject == null))
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
					foreach (string item2 in hashSet)
					{
						Tile tile2 = new Tile(Blocksworld.tilePool.GetTileObjectForIcon(item2, enabled: true));
						tile2.Show(show: true);
						tile2.SetTileScale(NormalizedScreen.pixelScale);
						list.Add(tile2);
					}
					tileOverlays[tile] = list;
				}
				else
				{
					foreach (Tile item3 in new List<Tile>(list))
					{
						string item = item3.tileObject.IconName();
						if (!hashSet.Contains(item))
						{
							item3.Show(show: false);
							list.Remove(item3);
						}
						else
						{
							item3.Show(show: true);
							item3.SetTileScale(NormalizedScreen.pixelScale);
							hashSet.Remove(item);
						}
					}
					foreach (string item4 in hashSet)
					{
						Tile tile3 = new Tile(Blocksworld.tilePool.GetTileObjectForIcon(item4, enabled: true));
						tile3.Show(show: true);
						list.Add(tile3);
					}
				}
				Vector3 vector = tile.tileObject.GetPosition();
				{
					foreach (Tile item5 in list)
					{
						item5.Show(show: true);
						item5.MoveTo(vector.x, vector.y, vector.z - 0.2f);
					}
					return;
				}
			}
			if (flag)
			{
				ShowTiles(list, s: false);
				tileOverlays.Remove(tile);
			}
		}
		else if (flag)
		{
			tileOverlays.Remove(tile);
			ShowTiles(list, s: false);
		}
	}

	public void Highlight(bool h)
	{
		go.GetComponent<Renderer>().material.mainTexture = ((!h) ? normalTexture : highlightTexture);
	}

	public Vector3 GetSpeed()
	{
		return smoothSpeed;
	}

	public void SetPosition(Vector3 p)
	{
		pos = p;
		tutCheckInterval = 5;
	}

	public virtual Transform GetTransform()
	{
		return go.transform;
	}

	protected void StepTutorialIfNecessary()
	{
		float magnitude = smoothSpeed.magnitude;
		if (!(magnitude > 0.03f) || Blocksworld.mouseBlock != null || !(magnitude <= 25f))
		{
			return;
		}
		if ((Tutorial.state == TutorialState.Scroll || Tutorial.state == TutorialState.Color || Tutorial.state == TutorialState.CreateBlock || Tutorial.state == TutorialState.AddTile || Tutorial.state == TutorialState.Texture) && tutCheckCounter % tutCheckInterval == 0)
		{
			Tutorial.Step();
			tutCheckCounter = 0;
			tutCheckInterval += 3;
			if (tutCheckInterval > 60)
			{
				tutCheckInterval = 60;
			}
		}
		tutCheckCounter++;
	}

	public void UpdateGestureRecognizer(GestureRecognizer recognizer)
	{
		foreach (BaseGesture gesture in gestures)
		{
			recognizer.RemoveGesture(gesture);
		}
		gestures.Clear();
	}

	public virtual void Show(bool show)
	{
		overlaysDirty = true;
		if (go.activeSelf == show)
		{
			return;
		}
		if (tiles != null)
		{
			for (int i = 0; i < tiles.Count; i++)
			{
				for (int j = 0; j < tiles[i].Count; j++)
				{
					tiles[i][j].Show(show);
				}
			}
		}
		goParent.SetActive(show);
		go.SetActive(show);
	}

	public bool IsShowing()
	{
		return go.activeSelf;
	}

	public virtual bool Hit(Vector3 v)
	{
		if (go.activeSelf && Mathf.Max(0f, v.x) >= pos.x && v.x <= pos.x + width + expanded && v.y >= pos.y)
		{
			return v.y <= pos.y + height;
		}
		return false;
	}

	public virtual Tile HitTile(Vector3 pos, bool allowDisabledTiles = false)
	{
		if (tiles != null && go.activeSelf)
		{
			for (int i = 0; i < tiles.Count; i++)
			{
				List<Tile> list = tiles[i];
				for (int num = list.Count - 1; num >= 0; num--)
				{
					Tile tile = list[num];
					if (tile.tileObject != null && tile.Hit(pos, allowDisabledTiles))
					{
						return tile;
					}
				}
			}
		}
		return null;
	}

	public virtual void BeginTrackingTouch()
	{
		trackingTouch = true;
		lastSmoothSpeed2 = (lastSmoothSpeed1 = (smoothSpeed = Vector3.zero));
		smoothSpeedStep = 0;
	}

	public virtual void EndTrackingTouch()
	{
		trackingTouch = false;
	}

	public virtual bool IsWithinBounds()
	{
		return true;
	}

	public virtual void Move(Vector3 delta)
	{
		snappingBack = false;
		tutCheckInterval = 5;
		speedDirty = true;
		if (trackingTouch)
		{
			pos += ((!IsWithinBounds()) ? 0.5f : 1f) * delta;
			smoothSpeed = (1.25f * delta + 0.75f * lastSmoothSpeed1 + 0.5f * lastSmoothSpeed2) / 2.5f;
			lastSmoothSpeed1 = smoothSpeed;
			lastSmoothSpeed2 = ((smoothSpeedStep != 0) ? lastSmoothSpeed1 : smoothSpeed);
			smoothSpeedStep++;
		}
		else
		{
			smoothSpeed = delta;
			lastSmoothSpeed1 = delta;
			lastSmoothSpeed2 = delta;
		}
	}

	public void ScrollToVisible(Tile tile, bool immediately = false, bool showTileAtTopOfScreen = false, bool forceScroll = false)
	{
		if (tile != null)
		{
			Vector3 min = tile.GetHitBounds().min;
			float num = ((!showTileAtTopOfScreen) ? 0f : ((float)Screen.height - 320f));
			if (min.y - num < 0f || forceScroll)
			{
				Vector3 v = new Vector3(pos.x, pos.y - min.y + 240f + num, pos.z);
				MoveTo(v, immediately);
			}
		}
	}

	public float GetScrollPos()
	{
		return pos.y;
	}

	public void SetScrollPos(float scrollPos)
	{
		MoveTo(new Vector3(pos.x, scrollPos, pos.z), immediately: true);
	}

	public void MoveTo(Vector3 v, bool immediately = false, bool snapBack = false)
	{
		if (immediately)
		{
			pos = v;
			UpdatePosition();
			snappingBack = false;
			lastSmoothSpeed2 = (lastSmoothSpeed1 = (smoothSpeed = Vector3.zero));
		}
		else if (snapBack)
		{
			Move((v - pos) * (1f - _snapBackMomentum));
			snappingBack = true;
		}
		else
		{
			Move((v - pos) * (1f - ((!snappingBack) ? _momentum : _snapBackMomentum)));
		}
	}

	protected virtual float PanelYOffset()
	{
		return 0f;
	}

	public virtual void UpdatePosition()
	{
		Vector3 vector = new Vector3(Mathf.Floor(pos.x), Mathf.Floor(pos.y), depth);
		go.transform.position = vector;
		if (useMeshBackground)
		{
			Util.SetGridMeshSize(meshBackground, width + expanded, height, 32f * NormalizedScreen.pixelScale, PanelYOffset());
		}
		if (vector.y > (float)NormalizedScreen.height - 70f - PanelYOffset())
		{
			MoveTo(new Vector3(vector.x, (float)NormalizedScreen.height - 70f - 1f - PanelYOffset(), vector.z), immediately: true);
		}
		if (overlaysDirty || Mathf.Abs(lastOverlaysHeight - height) > 0.01f || Mathf.Abs(lastOverlaysWidth - width) > 0.01f || (lastOverlaysPos - pos).sqrMagnitude > 0.0001f)
		{
			lastOverlaysPos = pos;
			lastOverlaysHeight = height;
			lastOverlaysWidth = width;
			UpdateAllOverlays();
		}
	}

	public Tile FindFirstTileWithPredicate(HashSet<Predicate> preds)
	{
		Tile result = null;
		if (tiles != null)
		{
			for (int i = 0; i < tiles.Count; i++)
			{
				List<Tile> list = tiles[i];
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

	public virtual bool TileOnLeftSide(Tile tile)
	{
		return false;
	}
}
