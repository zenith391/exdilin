using System.Collections.Generic;
using UnityEngine;

public class CopyScriptAnimationCommand : Command
{
	private int step = int.MaxValue;

	private int animationSteps = 40;

	private GameObject parent;

	private List<Tile> tiles = new List<Tile>();

	private Vector3 targetPos = new Vector3(800f, 100f, -0.5f);

	private Vector3 startPos;

	public DelegateCommand endCommand;

	public override void Execute()
	{
		step++;
		if (step >= animationSteps)
		{
			parent.transform.localScale = Vector3.one;
			parent.transform.DetachChildren();
			foreach (Tile tile in tiles)
			{
				tile.Show(show: false);
			}
			tiles.Clear();
			done = true;
			step = int.MaxValue;
			Sound.PlayOneShotSound("Copy Script Done");
			if (endCommand != null)
			{
				Blocksworld.AddFixedUpdateCommand(endCommand);
			}
		}
		if (step == (int)Mathf.Round((float)animationSteps / 3f))
		{
			Sound.PlayOneShotSound("Copy Script Transfer");
		}
		float num = Mathf.Clamp((float)step / (float)animationSteps, 0f, 1f);
		Vector3 position = (1f - num) * startPos + num * targetPos;
		parent.transform.position = position;
		parent.transform.localScale = Vector3.one * Mathf.Max(0.01f, 1f - num);
	}

	public bool Animating()
	{
		return step <= animationSteps;
	}

	public void SetTiles(List<List<Tile>> tiles)
	{
		step = 0;
		endCommand = null;
		this.tiles.Clear();
		if (parent == null)
		{
			parent = new GameObject("Script copy animation object");
		}
		parent.layer = 8;
		parent.transform.localScale = Vector3.one;
		startPos = Vector3.zero;
		int num = 0;
		foreach (List<Tile> tile2 in tiles)
		{
			foreach (Tile item in tile2)
			{
				if (item.tileObject != null)
				{
					Tile tile = item.Clone();
					tile.Show(show: true);
					tile.Enable(enabled: true);
					Vector3 position = tile.tileObject.GetPosition();
					startPos += position;
					tile.MoveTo(position);
					this.tiles.Add(tile);
					num++;
				}
			}
		}
		startPos /= (float)num;
		startPos.z = -0.5f;
		parent.transform.position = startPos;
		foreach (Tile tile3 in this.tiles)
		{
			if (!(tile3.tileObject == null))
			{
				tile3.SetParent(parent.transform);
				Vector3 localPosition = tile3.tileObject.GetLocalPosition();
				localPosition.z = 0f;
				tile3.tileObject.SetLocalPosition(localPosition);
			}
		}
		UIQuickSelect quickSelect = Blocksworld.UI.QuickSelect;
		if (quickSelect != null)
		{
			Vector2 vector = quickSelect.ScriptRectCenter();
			targetPos.x = vector.x;
			targetPos.y = vector.y;
			float magnitude = (targetPos - startPos).magnitude;
			animationSteps = (int)Mathf.Max(2f, magnitude / 50f);
		}
	}
}
