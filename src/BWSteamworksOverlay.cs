using Steamworks;
using UnityEngine;

public class BWSteamworksOverlay : MonoBehaviour
{
	protected Callback<GameOverlayActivated_t> _gameOverlayActivated;

	private void OnEnable()
	{
		BWLog.Info("Check for Steamworks overlay..");
		if (SteamManager.Initialized)
		{
			_gameOverlayActivated = Callback<GameOverlayActivated_t>.Create(OnGameOverlayActivated);
		}
	}

	private void OnGameOverlayActivated(GameOverlayActivated_t pCallback)
	{
		if (pCallback.m_bActive != 0)
		{
			BWLog.Info("Steam Overlay activated");
			Blocksworld.bw.OnApplicationPause(pauseStatus: true);
		}
		else
		{
			BWLog.Info("Steam Overlay closed");
			Blocksworld.bw.OnApplicationPause(pauseStatus: false);
		}
	}
}
