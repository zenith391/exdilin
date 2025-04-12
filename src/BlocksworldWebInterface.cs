using System;
using UnityEngine;

// Token: 0x0200035C RID: 860
public class BlocksworldWebInterface : MonoBehaviour
{
	// Token: 0x06002636 RID: 9782 RVA: 0x0011A8D3 File Offset: 0x00118CD3
	private void Awake()
	{
		this.OnLauncherAwake();
	}

	// Token: 0x17000177 RID: 375
	// (get) Token: 0x06002637 RID: 9783 RVA: 0x0011A8DB File Offset: 0x00118CDB
	public static BlocksworldWebInterface Instance
	{
		get
		{
			if (BlocksworldWebInterface._instance == null)
			{
				BlocksworldWebInterface.Create();
			}
			return BlocksworldWebInterface._instance;
		}
	}

	// Token: 0x06002638 RID: 9784 RVA: 0x0011A8F8 File Offset: 0x00118CF8
	public static void Create()
	{
		if (BlocksworldWebInterface._instance != null)
		{
			return;
		}
		GameObject gameObject = new GameObject("BlocksworldWebInterface");
		BlocksworldWebInterface._instance = gameObject.AddComponent<BlocksworldWebInterface>();
		UnityEngine.Object.DontDestroyOnLoad(gameObject);
	}

	// Token: 0x06002639 RID: 9785 RVA: 0x0011A932 File Offset: 0x00118D32
	public void EnableInputCapture()
	{
	}

	// Token: 0x0600263A RID: 9786 RVA: 0x0011A934 File Offset: 0x00118D34
	public void DisableInputCapture()
	{
	}

	// Token: 0x0600263B RID: 9787 RVA: 0x0011A936 File Offset: 0x00118D36
	public bool IsBlocksworldStarted()
	{
		return this._sceneLoaded;
	}

	// Token: 0x0600263C RID: 9788 RVA: 0x0011A93E File Offset: 0x00118D3E
	public void UnpauseWorld()
	{
		this.EnableInputCapture();
		Blocksworld.bw.ButtonPlayTapped();
	}

	// Token: 0x0600263D RID: 9789 RVA: 0x0011A950 File Offset: 0x00118D50
	public void PauseWorld()
	{
		this.DisableInputCapture();
		Blocksworld.bw.ButtonPauseTapped();
	}

	// Token: 0x0600263E RID: 9790 RVA: 0x0011A962 File Offset: 0x00118D62
	public void PlayWorld()
	{
		this.EnableInputCapture();
		Blocksworld.bw.ButtonRestartTapped();
	}

	// Token: 0x0600263F RID: 9791 RVA: 0x0011A974 File Offset: 0x00118D74
	public void Mute()
	{
		AudioListener.volume = 0f;
	}

	// Token: 0x06002640 RID: 9792 RVA: 0x0011A980 File Offset: 0x00118D80
	public void Unmute()
	{
		AudioListener.volume = 1f;
	}

	// Token: 0x06002641 RID: 9793 RVA: 0x0011A98C File Offset: 0x00118D8C
	public void SetVolume(float volume)
	{
		AudioListener.volume = volume;
	}

	// Token: 0x06002642 RID: 9794 RVA: 0x0011A994 File Offset: 0x00118D94
	public bool LaunchWorldWithJson(string worldSourceJsonStr)
	{
		Debug.Log(string.Format("Launching world from definition", new object[0]));
		this.LaunchWorldWithAction(delegate
		{
			WorldSession.StartForWebGLWithWorldSourceJsonStr(worldSourceJsonStr);
		});
		return true;
	}

	// Token: 0x06002643 RID: 9795 RVA: 0x0011A9D8 File Offset: 0x00118DD8
	public void LaunchModelPreviewWorldLand(string modelSourceJsonStr)
	{
		this.LaunchWorldWithAction(delegate
		{
			WorldSession.StartForWebGLWithModelSourceJsonStr("land", modelSourceJsonStr);
		});
	}

	// Token: 0x06002644 RID: 9796 RVA: 0x0011AA04 File Offset: 0x00118E04
	public void LaunchModelPreviewWorldSky(string modelSourceJsonStr)
	{
		this.LaunchWorldWithAction(delegate
		{
			WorldSession.StartForWebGLWithModelSourceJsonStr("sky", modelSourceJsonStr);
		});
	}

	// Token: 0x06002645 RID: 9797 RVA: 0x0011AA30 File Offset: 0x00118E30
	public void LaunchModelPreviewWorldWater(string modelSourceJsonStr)
	{
		this.LaunchWorldWithAction(delegate
		{
			WorldSession.StartForWebGLWithModelSourceJsonStr("water", modelSourceJsonStr);
		});
	}

	// Token: 0x06002646 RID: 9798 RVA: 0x0011AA5C File Offset: 0x00118E5C
	public void LaunchStarterIslandBuildModeDemo()
	{
		this.LaunchWorldWithAction(delegate
		{
			WorldSession.StartForWebGLBuildModeDemo();
		});
	}

	// Token: 0x06002647 RID: 9799 RVA: 0x0011AA84 File Offset: 0x00118E84
	private void LaunchWorldWithAction(Action loadWorldAction = null)
	{
		if (!this.IsBlocksworldStarted())
		{
			this._startupAction = loadWorldAction;
			Application.LoadLevel("Scene");
		}
		else if (WorldSession.current != null)
		{
			this._worldDidQuitAction = loadWorldAction;
			WorldSession.Quit();
		}
		else
		{
			loadWorldAction();
		}
	}

	// Token: 0x06002648 RID: 9800 RVA: 0x0011AAD3 File Offset: 0x00118ED3
	private void OnLauncherAwake()
	{
		Application.ExternalCall("PlayLoader.onLauncherAwake", new object[0]);
	}

	// Token: 0x06002649 RID: 9801 RVA: 0x0011AAE5 File Offset: 0x00118EE5
	public void OnEnginePause()
	{
		Application.ExternalCall("PlayLoader.onEnginePause", new object[0]);
	}

	// Token: 0x0600264A RID: 9802 RVA: 0x0011AAF7 File Offset: 0x00118EF7
	public void OnEngineLoaded()
	{
		this._sceneLoaded = true;
		Application.ExternalCall("PlayLoader.onEngineLoaded", new object[0]);
		if (this._startupAction != null)
		{
			this._startupAction();
		}
		this._startupAction = null;
	}

	// Token: 0x0600264B RID: 9803 RVA: 0x0011AB2D File Offset: 0x00118F2D
	public void OnBeforeWorldLoad()
	{
		Application.ExternalCall("PlayLoader.onBeforeWorldLoad", new object[0]);
	}

	// Token: 0x0600264C RID: 9804 RVA: 0x0011AB3F File Offset: 0x00118F3F
	public void OnWorldReady()
	{
		Application.ExternalCall("PlayLoader.onWorldReady", new object[0]);
	}

	// Token: 0x0600264D RID: 9805 RVA: 0x0011AB51 File Offset: 0x00118F51
	public void OnWorldQuit()
	{
		if (this._worldDidQuitAction != null)
		{
			this._worldDidQuitAction();
		}
		this._worldDidQuitAction = null;
	}

	// Token: 0x04002175 RID: 8565
	private bool _sceneLoaded;

	// Token: 0x04002176 RID: 8566
	private static BlocksworldWebInterface _instance;

	// Token: 0x04002177 RID: 8567
	private Action _startupAction;

	// Token: 0x04002178 RID: 8568
	private Action _worldDidQuitAction;
}
