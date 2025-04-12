using System;
using System.Text;
using Steamworks;
using UnityEngine;

// Token: 0x02000463 RID: 1123
[DisallowMultipleComponent]
public class SteamManager : MonoBehaviour
{
	// Token: 0x17000250 RID: 592
	// (get) Token: 0x06002F72 RID: 12146 RVA: 0x0014F65E File Offset: 0x0014DA5E
	private static SteamManager Instance
	{
		get
		{
			if (SteamManager.s_instance == null)
			{
				return new GameObject("SteamManager").AddComponent<SteamManager>();
			}
			return SteamManager.s_instance;
		}
	}

	// Token: 0x17000251 RID: 593
	// (get) Token: 0x06002F73 RID: 12147 RVA: 0x0014F685 File Offset: 0x0014DA85
	public static bool Initialized
	{
		get
		{
			return SteamManager.Instance.m_bInitialized;
		}
	}

	// Token: 0x06002F74 RID: 12148 RVA: 0x0014F691 File Offset: 0x0014DA91
	private static void SteamAPIDebugTextHook(int nSeverity, StringBuilder pchDebugText)
	{
		Debug.LogWarning(pchDebugText);
	}

	// Token: 0x06002F75 RID: 12149 RVA: 0x0014F69C File Offset: 0x0014DA9C
	private void Awake()
	{
		if (SteamManager.s_instance != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		BWLog.Info("Creating SteamManager..");
		SteamManager.s_instance = this;
		if (SteamManager.s_EverInialized)
		{
			throw new Exception("Tried to Initialize the SteamAPI twice in one session!");
		}
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		if (!Packsize.Test())
		{
			Debug.LogError("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.", this);
		}
		if (!DllCheck.Test())
		{
			Debug.LogError("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.", this);
		}
		try
		{
			if (SteamAPI.RestartAppIfNecessary(AppId_t.Invalid))
			{
				Application.Quit();
				return;
			}
		}
		catch (DllNotFoundException arg)
		{
			Debug.LogError("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + arg, this);
			Application.Quit();
			return;
		}
		BWLog.Info("Init Steam API..");
		this.m_bInitialized = SteamAPI.Init();
		if (!this.m_bInitialized)
		{
			Debug.LogError("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.", this);
			return;
		}
		SteamManager.s_EverInialized = true;
		BWLog.Info("Done!");
	}

	// Token: 0x06002F76 RID: 12150 RVA: 0x0014F788 File Offset: 0x0014DB88
	private void OnEnable()
	{
		if (SteamManager.s_instance == null)
		{
			SteamManager.s_instance = this;
		}
		if (!this.m_bInitialized)
		{
			return;
		}
		if (this.m_SteamAPIWarningMessageHook == null)
		{
			this.m_SteamAPIWarningMessageHook = new SteamAPIWarningMessageHook_t(SteamManager.SteamAPIDebugTextHook);
			SteamClient.SetWarningMessageHook(this.m_SteamAPIWarningMessageHook);
		}
	}

	// Token: 0x06002F77 RID: 12151 RVA: 0x0014F7DF File Offset: 0x0014DBDF
	private void OnDestroy()
	{
		if (SteamManager.s_instance != this)
		{
			return;
		}
		SteamManager.s_instance = null;
		if (!this.m_bInitialized)
		{
			return;
		}
		SteamAPI.Shutdown();
	}

	// Token: 0x06002F78 RID: 12152 RVA: 0x0014F809 File Offset: 0x0014DC09
	private void Update()
	{
		if (!this.m_bInitialized)
		{
			return;
		}
		SteamAPI.RunCallbacks();
	}

	// Token: 0x040027C7 RID: 10183
	private static SteamManager s_instance;

	// Token: 0x040027C8 RID: 10184
	private static bool s_EverInialized;

	// Token: 0x040027C9 RID: 10185
	private bool m_bInitialized;

	// Token: 0x040027CA RID: 10186
	private SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;
}
