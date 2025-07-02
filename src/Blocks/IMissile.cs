using System.Collections.Generic;

namespace Blocks;

public interface IMissile
{
	float GetLifetime();

	void SetLifetime(float newLifetime);

	HashSet<int> GetLabels();

	void FixedUpdate();

	void Update();

	bool IsBroken();

	void Break();

	bool CanExplode();

	void Explode(float radius);

	bool HasExploded();

	bool HasExpired();

	void SetExpired();

	void Destroy();

	void Deactivate();

	bool IsBursting();

	float GetInFlightTime();
}
