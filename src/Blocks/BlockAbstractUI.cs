using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockAbstractUI : Block
{
	public int index;

	protected bool dirty = true;

	private bool _uiEnabled;

	private bool _uiVisible = true;

	protected bool physicalVisible = true;

	protected bool uiPaused;

	protected string text = string.Empty;

	protected List<float[]> flashDurations = new List<float[]>
	{
		new float[2] { 1f, -0.1f },
		new float[11]
		{
			0.1f, -0.1f, 0.1f, -0.1f, 0.1f, -0.1f, 0.1f, -0.1f, 0.1f, -0.1f,
			0.5f
		}
	};

	protected bool uiVisible
	{
		get
		{
			if (_uiVisible)
			{
				return _uiEnabled;
			}
			return false;
		}
	}

	public BlockAbstractUI(List<List<Tile>> tiles, int index)
		: base(tiles)
	{
		this.index = index;
	}

	public override void Play()
	{
		base.Play();
		dirty = true;
		_uiEnabled = true;
		_uiVisible = true;
		physicalVisible = true;
		uiPaused = false;
		text = string.Empty;
		ShowPhysical(v: true);
	}

	public override void Stop(bool resetBlock = true)
	{
		base.Stop(resetBlock);
		ShowPhysical(v: true);
	}

	public void EnableUI(bool enable)
	{
		_uiEnabled = enable;
	}

	public TileResultCode ShowUI(ScriptRowExecutionInfo eInfo, object[] args)
	{
		bool uIVisible = args.Length == 0 || (int)args[0] != 0;
		SetUIVisible(uIVisible);
		return TileResultCode.True;
	}

	protected virtual void SetUIVisible(bool v)
	{
		dirty = dirty || v != _uiVisible;
		_uiVisible = v;
	}

	private void ShowPhysical(bool v)
	{
		if (chunk != null && chunk.go != null)
		{
			Rigidbody rb = chunk.rb;
			if (rb != null && connections.Count == 0)
			{
				rb.isKinematic = !v;
				if (v)
				{
					rb.WakeUp();
				}
			}
		}
		go.GetComponent<Collider>().isTrigger = !v;
	}

	public TileResultCode ShowPhysical(ScriptRowExecutionInfo eInfo, object[] args)
	{
		bool flag = physicalVisible;
		physicalVisible = args.Length == 0 || (int)args[0] != 0;
		if (flag != physicalVisible)
		{
			dirty = true;
			ShowPhysical(physicalVisible);
		}
		return TileResultCode.True;
	}

	protected string ParseText(string s)
	{
		foreach (string key in Blocksworld.customIntVariables.Keys)
		{
			s = s.Replace("<" + key + ">", Blocksworld.customIntVariables[key].ToString());
		}
		s = s.Replace("/name/", WorldSession.current.config.currentUserUsername);
		s = s.Replace("/coins/", WorldSession.current.config.currentUserCoins.ToString());
		return s;
	}

	public virtual TileResultCode SetText(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string text = this.text;
		this.text = ((args.Length == 0) ? string.Empty : ParseText((string)args[0]));
		dirty = dirty || text != this.text;
		return TileResultCode.True;
	}

	public TileResultCode PauseUI(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float num = ((args.Length == 0) ? 1f : ((float)args[0]));
		if (eInfo.timer >= num)
		{
			PauseUI(p: false);
			return TileResultCode.True;
		}
		PauseUI(p: true);
		return TileResultCode.Delayed;
	}

	public virtual void PauseUI(bool p)
	{
		uiPaused = p;
	}

	public virtual TileResultCode Flash(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float num = 0f;
		float num2 = -1f;
		bool uIVisible = true;
		int num3 = Util.GetIntArg(args, 0, 0) % flashDurations.Count;
		float[] array = flashDurations[num3];
		foreach (float num4 in array)
		{
			float num5 = Mathf.Abs(num4);
			num += num5;
			if ((eInfo.timer >= num2) & (eInfo.timer < num))
			{
				uIVisible = num4 > 0f;
				break;
			}
			num2 = num;
		}
		SetUIVisible(uIVisible);
		if (!(eInfo.timer < num))
		{
			return TileResultCode.True;
		}
		return TileResultCode.Delayed;
	}
}
