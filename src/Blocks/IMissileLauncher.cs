namespace Blocks;

public interface IMissileLauncher
{
	IMissile GetLaunchedMissile();

	bool CanLaunch();

	void LaunchMissile(float burstMultiplier);

	bool CanReload();

	bool IsLoaded();

	void Reload();

	bool MissileGone();

	bool HasLabel(int label);

	void AddControllerTargetTag(string tagName, float lockDelay);

	void SetGlobalBurstTime(float burstTime);
}
