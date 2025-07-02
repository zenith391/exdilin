using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

public class VisualEffect
{
	protected static List<VisualEffect> vfxs;

	protected static List<VisualEffect> endedVfxs;

	public string name;

	public Block block;

	public float timeLength = 1f;

	public float currentTime;

	private bool isDestroyed;

	protected bool paused;

	public VisualEffect(string name)
	{
		this.name = name;
	}

	public bool HasEnded()
	{
		return currentTime >= timeLength;
	}

	public virtual void Begin()
	{
	}

	public virtual void End()
	{
	}

	public virtual void Destroy()
	{
		isDestroyed = true;
	}

	public virtual void Pause()
	{
		paused = true;
	}

	public virtual void Resume()
	{
		paused = false;
	}

	public virtual void Stop()
	{
	}

	public virtual void Clear()
	{
	}

	public virtual void FixedUpdate()
	{
		currentTime += Blocksworld.fixedDeltaTime;
	}

	public virtual void Update()
	{
	}

	public virtual Block GetBlock()
	{
		return null;
	}

	public static void FixedUpdateVfxs()
	{
		if (vfxs == null)
		{
			return;
		}
		for (int num = vfxs.Count - 1; num >= 0; num--)
		{
			VisualEffect visualEffect = vfxs[num];
			visualEffect.FixedUpdate();
			if (visualEffect.currentTime >= visualEffect.timeLength)
			{
				vfxs.RemoveAt(num);
				endedVfxs.Add(visualEffect);
				visualEffect.End();
			}
		}
		for (int num2 = endedVfxs.Count - 1; num2 >= 0; num2--)
		{
			VisualEffect visualEffect2 = endedVfxs[num2];
			visualEffect2.FixedUpdate();
			if (visualEffect2.isDestroyed)
			{
				endedVfxs.RemoveAt(num2);
			}
		}
	}

	public static void UpdateVfxs()
	{
		if (vfxs != null)
		{
			for (int i = 0; i < vfxs.Count; i++)
			{
				vfxs[i].Update();
			}
			for (int j = 0; j < endedVfxs.Count; j++)
			{
				endedVfxs[j].Update();
			}
		}
	}

	public static void DestroyVfxs()
	{
		if (vfxs == null)
		{
			return;
		}
		foreach (VisualEffect vfx in vfxs)
		{
			vfx.Destroy();
		}
		vfxs.Clear();
		vfxs = null;
		foreach (VisualEffect endedVfx in endedVfxs)
		{
			if (!endedVfx.isDestroyed)
			{
				endedVfx.Destroy();
			}
		}
		endedVfxs.Clear();
		endedVfxs = null;
	}

	private static void Apply(Action<VisualEffect> action)
	{
		if (vfxs == null)
		{
			return;
		}
		foreach (VisualEffect vfx in vfxs)
		{
			action(vfx);
		}
		foreach (VisualEffect endedVfx in endedVfxs)
		{
			action(endedVfx);
		}
	}

	public static void StopVfxs()
	{
		Apply(delegate(VisualEffect e)
		{
			e.Stop();
		});
		Apply(delegate(VisualEffect e)
		{
			e.Clear();
		});
		DestroyVfxs();
	}

	public static void PauseVfxs()
	{
		if (vfxs == null)
		{
			return;
		}
		foreach (VisualEffect vfx in vfxs)
		{
			vfx.Pause();
		}
		foreach (VisualEffect endedVfx in endedVfxs)
		{
			endedVfx.Pause();
		}
	}

	public static void ResumeVfxs()
	{
		if (vfxs == null)
		{
			return;
		}
		foreach (VisualEffect vfx in vfxs)
		{
			vfx.Resume();
		}
	}

	public static VisualEffect CreateEffect(Block b, string name, float lengthMult, string colorName)
	{
		if (vfxs == null)
		{
			vfxs = new List<VisualEffect>();
			endedVfxs = new List<VisualEffect>();
		}
		VisualEffect visualEffect = null;
		switch (name)
		{
		case "Sparkle":
			visualEffect = new SparkleVisualEffect(name);
			break;
		case "WindLines":
			visualEffect = new VisualEffectWind(name);
			break;
		case "Sparkle Model":
			visualEffect = new SparkleVisualEffect(name, BlockVfxRange.MODEL);
			break;
		case "Sparkle Group":
			visualEffect = new SparkleVisualEffect(name, BlockVfxRange.GROUP);
			break;
		case "Letterbox In":
			visualEffect = new LetterboxVisualEffect(name, moveIn: true);
			break;
		case "Letterbox Out":
			visualEffect = new LetterboxVisualEffect(name, moveIn: false);
			break;
		case "Space Dust":
			visualEffect = new SpaceDustVisualEffect(name);
			break;
		}
		if (colorName.Length > 0 && visualEffect is EmissionVisualEffect)
		{
			Color[] value = new Color[1] { Color.white };
			Blocksworld.colorDefinitions.TryGetValue(colorName, out value);
			(visualEffect as EmissionVisualEffect).SetColor(value[0]);
		}
		if (visualEffect != null)
		{
			visualEffect.timeLength = lengthMult * GetEffectLength(name);
			visualEffect.block = b;
			vfxs.Add(visualEffect);
		}
		else
		{
			BWLog.Info("Could not find visual effect '" + name + "'");
		}
		return visualEffect;
	}

	public static float GetEffectLength(string name)
	{
		switch (name)
		{
		case "Sparkle":
		case "Sparkle Model":
			return 1f;
		default:
			_ = name == "Letterbox Out";
			goto case "Letterbox In";
		case "Letterbox In":
			return 1f;
		case null:
			return 1f;
		}
	}
}
