using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x0200021F RID: 543
public class ParticleCollision : MonoBehaviour
{
	// Token: 0x06001A88 RID: 6792 RVA: 0x000C2D7B File Offset: 0x000C117B
	private void Start()
	{
		this.part = base.GetComponent<ParticleSystem>();
		this.collisionEvents = new ParticleCollisionEvent[16];
	}

	// Token: 0x06001A89 RID: 6793 RVA: 0x000C2D98 File Offset: 0x000C1198
	private void OnParticleCollision(GameObject chunkGO)
	{
		int safeCollisionEventSize = this.part.GetSafeCollisionEventSize();
		if (this.collisionEvents.Length < safeCollisionEventSize)
		{
			this.collisionEvents = new ParticleCollisionEvent[safeCollisionEventSize];
		}
		int num = this.part.GetCollisionEvents(chunkGO, this.collisionEvents);
		for (int i = 0; i < num; i++)
		{
			GameObject gameObject = this.collisionEvents[i].colliderComponent.gameObject;
			if (gameObject != null)
			{
				Block block = BWSceneManager.FindBlock(gameObject, true);
				if (block != null)
				{
					CollisionManager.ForwardParticleCollision(block, this.particleType);
				}
			}
		}
	}

	// Token: 0x04001627 RID: 5671
	public int particleType;

	// Token: 0x04001628 RID: 5672
	private static HashSet<Predicate> emitterPreds;

	// Token: 0x04001629 RID: 5673
	private ParticleSystem part;

	// Token: 0x0400162A RID: 5674
	private ParticleCollisionEvent[] collisionEvents;
}
