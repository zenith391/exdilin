using System;
using UnityEngine;

namespace Blocks
{
	// Token: 0x020000CC RID: 204
	public class SphereLightChanger : LightChanger
	{
		// Token: 0x06000F7A RID: 3962 RVA: 0x000681DC File Offset: 0x000665DC
		public override Color GetLightTint(Vector3 pos)
		{
			Color result = Color.white;
			float magnitude = (pos - this.position).magnitude;
			if (magnitude < this.radius)
			{
				float num = 1f / (this.radius - this.innerRadius);
				float num2 = Mathf.Clamp(num * (magnitude - this.innerRadius), 0f, 1f);
				result = Color.white * num2 + this.color * (1f - num2);
			}
			return result;
		}

		// Token: 0x04000BFE RID: 3070
		public Color color = Color.white;

		// Token: 0x04000BFF RID: 3071
		public Vector3 position = default(Vector3);

		// Token: 0x04000C00 RID: 3072
		public float radius = 50f;

		// Token: 0x04000C01 RID: 3073
		public float innerRadius = 20f;
	}
}
