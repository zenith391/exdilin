using System;
using UnityEngine;

public class BlocksworldWebInterface : MonoBehaviour
{
	private bool _sceneLoaded;

	private static BlocksworldWebInterface _instance;

	private Action _startupAction;

	private Action _worldDidQuitAction;

	public static BlocksworldWebInterface Instance
	{
		get
		{
			if (_instance == null)
			{
				Create();
			}
			return _instance;
		}
	}

	private void Awake()
	{
		OnLauncherAwake();
	}

	public static void Create()
	{
		if (!(_instance != null))
		{
			GameObject gameObject = new GameObject("BlocksworldWebInterface");
			_instance = gameObject.AddComponent<BlocksworldWebInterface>();
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
		}
	}

	public void EnableInputCapture()
	{
	}

	public void DisableInputCapture()
	{
	}

	public bool IsBlocksworldStarted()
	{
		return _sceneLoaded;
	}

	public void UnpauseWorld()
	{
		EnableInputCapture();
		Blocksworld.bw.ButtonPlayTapped();
	}

	public void PauseWorld()
	{
		DisableInputCapture();
		Blocksworld.bw.ButtonPauseTapped();
	}

	public void PlayWorld()
	{
		EnableInputCapture();
		Blocksworld.bw.ButtonRestartTapped();
	}

	public void Mute()
	{
		AudioListener.volume = 0f;
	}

	public void Unmute()
	{
		AudioListener.volume = 1f;
	}

	public void SetVolume(float volume)
	{
		AudioListener.volume = volume;
	}

	public bool LaunchWorldWithJson(string worldSourceJsonStr)
	{
		Debug.Log($"Launching world from definition");
		LaunchWorldWithAction(delegate
		{
			WorldSession.StartForWebGLWithWorldSourceJsonStr(worldSourceJsonStr);
		});
		return true;
	}

	public void LaunchModelPreviewWorldLand(string modelSourceJsonStr)
	{
		LaunchWorldWithAction(delegate
		{
			WorldSession.StartForWebGLWithModelSourceJsonStr("land", modelSourceJsonStr);
		});
	}

	public void LaunchModelPreviewWorldSky(string modelSourceJsonStr)
	{
		LaunchWorldWithAction(delegate
		{
			WorldSession.StartForWebGLWithModelSourceJsonStr("sky", modelSourceJsonStr);
		});
	}

	public void LaunchModelPreviewWorldWater(string modelSourceJsonStr)
	{
		LaunchWorldWithAction(delegate
		{
			WorldSession.StartForWebGLWithModelSourceJsonStr("water", modelSourceJsonStr);
		});
	}

	public void LaunchStarterIslandBuildModeDemo()
	{
		LaunchWorldWithAction(delegate
		{
			WorldSession.StartForWebGLBuildModeDemo();
		});
	}

	private void LaunchWorldWithAction(Action loadWorldAction = null)
	{
		if (!IsBlocksworldStarted())
		{
			_startupAction = loadWorldAction;
			Application.LoadLevel("Scene");
		}
		else if (WorldSession.current != null)
		{
			_worldDidQuitAction = loadWorldAction;
			WorldSession.Quit();
		}
		else
		{
			loadWorldAction();
		}
	}

	private void OnLauncherAwake()
	{
		Application.ExternalCall("PlayLoader.onLauncherAwake");
	}

	public void OnEnginePause()
	{
		Application.ExternalCall("PlayLoader.onEnginePause");
	}

	public void OnEngineLoaded()
	{
		_sceneLoaded = true;
		Application.ExternalCall("PlayLoader.onEngineLoaded");
		if (_startupAction != null)
		{
			_startupAction();
		}
		_startupAction = null;
	}

	public void OnBeforeWorldLoad()
	{
		Application.ExternalCall("PlayLoader.onBeforeWorldLoad");
	}

	public void OnWorldReady()
	{
		Application.ExternalCall("PlayLoader.onWorldReady");
	}

	public void OnWorldQuit()
	{
		if (_worldDidQuitAction != null)
		{
			_worldDidQuitAction();
		}
		_worldDidQuitAction = null;
	}
}
