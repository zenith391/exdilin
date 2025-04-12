using System;
using Steamworks;
using UnityEngine;

// Token: 0x020003BF RID: 959
public class BWSteamworksOverlay : MonoBehaviour
{
	// Token: 0x060029D9 RID: 10713 RVA: 0x001333EC File Offset: 0x001317EC
	private void OnEnable()
	{
		BWLog.Info("Check for Steamworks overlay..");
		if (SteamManager.Initialized)
		{
			this._gameOverlayActivated = Callback<GameOverlayActivated_t>.Create(new Callback<GameOverlayActivated_t>.DispatchDelegate(this.OnGameOverlayActivated));
		}
	}

	// Token: 0x060029DA RID: 10714 RVA: 0x0013340F File Offset: 0x0013180F
	private void OnGameOverlayActivated(GameOverlayActivated_t pCallback)
	{
		if (pCallback.m_bActive != 0)
		{
			BWLog.Info("Steam Overlay activated");
			Blocksworld.bw.OnApplicationPause(true);
		}
		else
		{
			BWLog.Info("Steam Overlay closed");
			Blocksworld.bw.OnApplicationPause(false);
		}
	}

	// Token: 0x04002406 RID: 9222
	protected Callback<GameOverlayActivated_t> _gameOverlayActivated;
}
