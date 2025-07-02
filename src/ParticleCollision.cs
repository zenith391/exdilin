using System.Collections.Generic;
using Blocks;
using UnityEngine;

public class ParticleCollision : MonoBehaviour
{
	public int particleType;

	private static HashSet<Predicate> emitterPreds;

	private ParticleSystem part;

	private ParticleCollisionEvent[] collisionEvents;

	private void Start()
	{
		part = GetComponent<ParticleSystem>();
		collisionEvents = new ParticleCollisionEvent[16];
	}

	private void OnParticleCollision(GameObject chunkGO)
	{
		int safeCollisionEventSize = part.GetSafeCollisionEventSize();
		if (collisionEvents.Length < safeCollisionEventSize)
		{
			collisionEvents = new ParticleCollisionEvent[safeCollisionEventSize];
		}
		int num = part.GetCollisionEvents(chunkGO, collisionEvents);
		for (int i = 0; i < num; i++)
		{
			GameObject gameObject = collisionEvents[i].colliderComponent.gameObject;
			if (gameObject != null)
			{
				Block block = BWSceneManager.FindBlock(gameObject, checkChildGos: true);
				if (block != null)
				{
					CollisionManager.ForwardParticleCollision(block, particleType);
				}
			}
		}
	}
}
