using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockPosition : Block
{
	private bool visibleBlock;

	private bool hiddenInTutorial;

	private bool hideAfterTutorial;

	public BlockPosition(List<List<Tile>> tiles, bool isVisible = false)
		: base(tiles)
	{
		visibleBlock = isVisible;
	}

	public override void Play()
	{
		base.Play();
		HideAndMakeTrigger();
		IgnoreRaycasts(value: true);
	}

	public bool IsHiddenAndTrigger()
	{
		if (go.GetComponent<Collider>().isTrigger)
		{
			return !go.GetComponent<Renderer>().enabled;
		}
		return false;
	}

	public void HideAndMakeTrigger()
	{
		if (go.GetComponent<Collider>() != null)
		{
			go.GetComponent<Collider>().isTrigger = true;
			go.GetComponent<Collider>().enabled = true;
		}
		if (!visibleBlock)
		{
			go.GetComponent<Renderer>().enabled = false;
			hiddenInTutorial = true;
		}
		else
		{
			if (Tutorial.state == TutorialState.None)
			{
				go.GetComponent<Renderer>().enabled = false;
			}
			hideAfterTutorial = true;
		}
		if (goShadow != null)
		{
			goShadow.GetComponent<Renderer>().enabled = false;
		}
	}

	public void ShowAndRemoveIsTrigger()
	{
		if (go.GetComponent<Collider>() != null)
		{
			go.GetComponent<Collider>().isTrigger = false;
		}
		if (go.GetComponent<Renderer>() != null)
		{
			go.GetComponent<Renderer>().enabled = true;
		}
		if (goShadow != null)
		{
			goShadow.GetComponent<Renderer>().enabled = true;
		}
		hiddenInTutorial = false;
	}

	public override void Stop(bool resetBlock = true)
	{
		base.Stop(resetBlock);
		UpdateStopVisibility();
	}

	public override TileResultCode TextureTo(string texture, Vector3 normal, bool permanent, int meshIndex = 0, bool force = false)
	{
		if (Blocksworld.CurrentState != State.Play)
		{
			string text = BlockType();
			texture = ((!text.Contains("Position")) ? "Volume" : "Glass");
		}
		return base.TextureTo(texture, normal, permanent, meshIndex, force: true);
	}

	public void UpdateStopVisibility()
	{
		if (Tutorial.state == TutorialState.None)
		{
			ShowAndRemoveIsTrigger();
			IgnoreRaycasts(value: false);
		}
		else
		{
			HideAndMakeTrigger();
		}
		if (BlockType() == "Position Camera Hint")
		{
			if (Options.BlockCameraHintDisplay)
			{
				ShowAndRemoveIsTrigger();
				IgnoreRaycasts(value: false);
			}
			else
			{
				HideAndMakeTrigger();
				IgnoreRaycasts(value: true);
			}
		}
	}

	public override void OnCreate()
	{
		base.OnCreate();
		UpdateStopVisibility();
		string text = BlockType();
		if (text.Contains("Position"))
		{
			TextureTo("Glass", GetTextureNormal(), permanent: true, 0, force: true);
		}
		else
		{
			TextureTo("Volume", GetTextureNormal(), permanent: true, 0, force: true);
		}
	}

	public override void Pause()
	{
	}

	public override void Resume()
	{
	}

	public override void ResetFrame()
	{
	}

	public override void Update()
	{
		base.Update();
		if (!hiddenInTutorial && (Tutorial.mode == TutorialMode.Puzzle || Tutorial.state != TutorialState.None))
		{
			HideAndMakeTrigger();
			if (!visibleBlock)
			{
				IgnoreRaycasts(value: true);
			}
		}
		if (Blocksworld.CurrentState == State.Play && hideAfterTutorial)
		{
			go.GetComponent<Renderer>().enabled = false;
			hideAfterTutorial = false;
		}
	}

	public override bool VisibleInPlayMode()
	{
		return visibleBlock;
	}

	public override bool ColliderIsTriggerInPlayMode()
	{
		return true;
	}

	protected override void UpdateBlockPropertiesForTextureAssignment(int meshIndex, bool forceEnabled)
	{
	}

	public override bool IsRuntimeInvisible()
	{
		return true;
	}
}
