using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x02000340 RID: 832
public class VisualEffect
{
	// Token: 0x0600255D RID: 9565 RVA: 0x0010F389 File Offset: 0x0010D789
	public VisualEffect(string name)
	{
		this.name = name;
	}

	// Token: 0x0600255E RID: 9566 RVA: 0x0010F3A3 File Offset: 0x0010D7A3
	public bool HasEnded()
	{
		return this.currentTime >= this.timeLength;
	}

	// Token: 0x0600255F RID: 9567 RVA: 0x0010F3B6 File Offset: 0x0010D7B6
	public virtual void Begin()
	{
	}

	// Token: 0x06002560 RID: 9568 RVA: 0x0010F3B8 File Offset: 0x0010D7B8
	public virtual void End()
	{
	}

	// Token: 0x06002561 RID: 9569 RVA: 0x0010F3BA File Offset: 0x0010D7BA
	public virtual void Destroy()
	{
		this.isDestroyed = true;
	}

	// Token: 0x06002562 RID: 9570 RVA: 0x0010F3C3 File Offset: 0x0010D7C3
	public virtual void Pause()
	{
		this.paused = true;
	}

	// Token: 0x06002563 RID: 9571 RVA: 0x0010F3CC File Offset: 0x0010D7CC
	public virtual void Resume()
	{
		this.paused = false;
	}

	// Token: 0x06002564 RID: 9572 RVA: 0x0010F3D5 File Offset: 0x0010D7D5
	public virtual void Stop()
	{
	}

	// Token: 0x06002565 RID: 9573 RVA: 0x0010F3D7 File Offset: 0x0010D7D7
	public virtual void Clear()
	{
	}

	// Token: 0x06002566 RID: 9574 RVA: 0x0010F3D9 File Offset: 0x0010D7D9
	public virtual void FixedUpdate()
	{
		this.currentTime += Blocksworld.fixedDeltaTime;
	}

	// Token: 0x06002567 RID: 9575 RVA: 0x0010F3ED File Offset: 0x0010D7ED
	public virtual void Update()
	{
	}

	// Token: 0x06002568 RID: 9576 RVA: 0x0010F3EF File Offset: 0x0010D7EF
	public virtual Block GetBlock()
	{
		return null;
	}

	// Token: 0x06002569 RID: 9577 RVA: 0x0010F3F4 File Offset: 0x0010D7F4
	public static void FixedUpdateVfxs()
	{
		if (VisualEffect.vfxs != null)
		{
			for (int i = VisualEffect.vfxs.Count - 1; i >= 0; i--)
			{
				VisualEffect visualEffect = VisualEffect.vfxs[i];
				visualEffect.FixedUpdate();
				if (visualEffect.currentTime >= visualEffect.timeLength)
				{
					VisualEffect.vfxs.RemoveAt(i);
					VisualEffect.endedVfxs.Add(visualEffect);
					visualEffect.End();
				}
			}
			for (int j = VisualEffect.endedVfxs.Count - 1; j >= 0; j--)
			{
				VisualEffect visualEffect2 = VisualEffect.endedVfxs[j];
				visualEffect2.FixedUpdate();
				if (visualEffect2.isDestroyed)
				{
					VisualEffect.endedVfxs.RemoveAt(j);
				}
			}
		}
	}

	// Token: 0x0600256A RID: 9578 RVA: 0x0010F4AC File Offset: 0x0010D8AC
	public static void UpdateVfxs()
	{
		if (VisualEffect.vfxs != null)
		{
			for (int i = 0; i < VisualEffect.vfxs.Count; i++)
			{
				VisualEffect.vfxs[i].Update();
			}
			for (int j = 0; j < VisualEffect.endedVfxs.Count; j++)
			{
				VisualEffect.endedVfxs[j].Update();
			}
		}
	}

	// Token: 0x0600256B RID: 9579 RVA: 0x0010F51C File Offset: 0x0010D91C
	public static void DestroyVfxs()
	{
		if (VisualEffect.vfxs != null)
		{
			foreach (VisualEffect visualEffect in VisualEffect.vfxs)
			{
				visualEffect.Destroy();
			}
			VisualEffect.vfxs.Clear();
			VisualEffect.vfxs = null;
			foreach (VisualEffect visualEffect2 in VisualEffect.endedVfxs)
			{
				if (!visualEffect2.isDestroyed)
				{
					visualEffect2.Destroy();
				}
			}
			VisualEffect.endedVfxs.Clear();
			VisualEffect.endedVfxs = null;
		}
	}

	// Token: 0x0600256C RID: 9580 RVA: 0x0010F5F4 File Offset: 0x0010D9F4
	private static void Apply(Action<VisualEffect> action)
	{
		if (VisualEffect.vfxs != null)
		{
			foreach (VisualEffect obj in VisualEffect.vfxs)
			{
				action(obj);
			}
			foreach (VisualEffect obj2 in VisualEffect.endedVfxs)
			{
				action(obj2);
			}
		}
	}

	// Token: 0x0600256D RID: 9581 RVA: 0x0010F6A4 File Offset: 0x0010DAA4
	public static void StopVfxs()
	{
		VisualEffect.Apply(delegate(VisualEffect e)
		{
			e.Stop();
		});
		VisualEffect.Apply(delegate(VisualEffect e)
		{
			e.Clear();
		});
		VisualEffect.DestroyVfxs();
	}

	// Token: 0x0600256E RID: 9582 RVA: 0x0010F6FC File Offset: 0x0010DAFC
	public static void PauseVfxs()
	{
		if (VisualEffect.vfxs != null)
		{
			foreach (VisualEffect visualEffect in VisualEffect.vfxs)
			{
				visualEffect.Pause();
			}
			foreach (VisualEffect visualEffect2 in VisualEffect.endedVfxs)
			{
				visualEffect2.Pause();
			}
		}
	}

	// Token: 0x0600256F RID: 9583 RVA: 0x0010F7AC File Offset: 0x0010DBAC
	public static void ResumeVfxs()
	{
		if (VisualEffect.vfxs != null)
		{
			foreach (VisualEffect visualEffect in VisualEffect.vfxs)
			{
				visualEffect.Resume();
			}
		}
	}

	// Token: 0x06002570 RID: 9584 RVA: 0x0010F810 File Offset: 0x0010DC10
	public static VisualEffect CreateEffect(Block b, string name, float lengthMult, string colorName)
	{
		if (VisualEffect.vfxs == null)
		{
			VisualEffect.vfxs = new List<VisualEffect>();
			VisualEffect.endedVfxs = new List<VisualEffect>();
		}
		VisualEffect visualEffect = null;
		switch (name)
		{
		case "Sparkle":
			visualEffect = new SparkleVisualEffect(name, BlockVfxRange.BLOCK);
			break;
		case "WindLines":
			visualEffect = new VisualEffectWind(name, BlockVfxRange.BLOCK);
			break;
		case "Sparkle Model":
			visualEffect = new SparkleVisualEffect(name, BlockVfxRange.MODEL);
			break;
		case "Sparkle Group":
			visualEffect = new SparkleVisualEffect(name, BlockVfxRange.GROUP);
			break;
		case "Letterbox In":
			visualEffect = new LetterboxVisualEffect(name, true);
			break;
		case "Letterbox Out":
			visualEffect = new LetterboxVisualEffect(name, false);
			break;
		case "Space Dust":
			visualEffect = new SpaceDustVisualEffect(name);
			break;
		}
		if (colorName.Length > 0 && visualEffect is EmissionVisualEffect)
		{
			Color[] array = new Color[]
			{
				Color.white
			};
			Blocksworld.colorDefinitions.TryGetValue(colorName, out array);
			(visualEffect as EmissionVisualEffect).SetColor(array[0]);
		}
		if (visualEffect != null)
		{
			visualEffect.timeLength = lengthMult * VisualEffect.GetEffectLength(name);
			visualEffect.block = b;
			VisualEffect.vfxs.Add(visualEffect);
		}
		else
		{
			BWLog.Info("Could not find visual effect '" + name + "'");
		}
		return visualEffect;
	}

	// Token: 0x06002571 RID: 9585 RVA: 0x0010F9D8 File Offset: 0x0010DDD8
	public static float GetEffectLength(string name)
	{
		if (name != null)
		{
			if (name == "Sparkle" || name == "Sparkle Model")
			{
				return 1f;
			}
			if (name == "Letterbox In" || name == "Letterbox Out")
			{
				return 1f;
			}
		}
		return 1f;
	}

	// Token: 0x04001FF5 RID: 8181
	protected static List<VisualEffect> vfxs;

	// Token: 0x04001FF6 RID: 8182
	protected static List<VisualEffect> endedVfxs;

	// Token: 0x04001FF7 RID: 8183
	public string name;

	// Token: 0x04001FF8 RID: 8184
	public Block block;

	// Token: 0x04001FF9 RID: 8185
	public float timeLength = 1f;

	// Token: 0x04001FFA RID: 8186
	public float currentTime;

	// Token: 0x04001FFB RID: 8187
	private bool isDestroyed;

	// Token: 0x04001FFC RID: 8188
	protected bool paused;
}
