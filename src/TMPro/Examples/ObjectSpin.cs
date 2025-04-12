using System;
using UnityEngine;

namespace TMPro.Examples
{
	// Token: 0x02000379 RID: 889
	public class ObjectSpin : MonoBehaviour
	{
		// Token: 0x060027A9 RID: 10153 RVA: 0x00122F30 File Offset: 0x00121330
		private void Awake()
		{
			this.m_transform = base.transform;
			this.m_initial_Rotation = this.m_transform.rotation.eulerAngles;
			this.m_initial_Position = this.m_transform.position;
			Light component = base.GetComponent<Light>();
			this.m_lightColor = ((!(component != null)) ? Color.black : component.color);
		}

		// Token: 0x060027AA RID: 10154 RVA: 0x00122FA4 File Offset: 0x001213A4
		private void Update()
		{
			if (this.Motion == ObjectSpin.MotionType.Rotation)
			{
				this.m_transform.Rotate(0f, this.SpinSpeed * Time.deltaTime, 0f);
			}
			else if (this.Motion == ObjectSpin.MotionType.BackAndForth)
			{
				this.m_time += this.SpinSpeed * Time.deltaTime;
				this.m_transform.rotation = Quaternion.Euler(this.m_initial_Rotation.x, Mathf.Sin(this.m_time) * (float)this.RotationRange + this.m_initial_Rotation.y, this.m_initial_Rotation.z);
			}
			else
			{
				this.m_time += this.SpinSpeed * Time.deltaTime;
				float x = 15f * Mathf.Cos(this.m_time * 0.95f);
				float z = 10f;
				float y = 0f;
				this.m_transform.position = this.m_initial_Position + new Vector3(x, y, z);
				this.m_prevPOS = this.m_transform.position;
				this.frames++;
			}
		}

		// Token: 0x0400229A RID: 8858
		public float SpinSpeed = 5f;

		// Token: 0x0400229B RID: 8859
		public int RotationRange = 15;

		// Token: 0x0400229C RID: 8860
		private Transform m_transform;

		// Token: 0x0400229D RID: 8861
		private float m_time;

		// Token: 0x0400229E RID: 8862
		private Vector3 m_prevPOS;

		// Token: 0x0400229F RID: 8863
		private Vector3 m_initial_Rotation;

		// Token: 0x040022A0 RID: 8864
		private Vector3 m_initial_Position;

		// Token: 0x040022A1 RID: 8865
		private Color32 m_lightColor;

		// Token: 0x040022A2 RID: 8866
		private int frames;

		// Token: 0x040022A3 RID: 8867
		public ObjectSpin.MotionType Motion;

		// Token: 0x0200037A RID: 890
		public enum MotionType
		{
			// Token: 0x040022A5 RID: 8869
			Rotation,
			// Token: 0x040022A6 RID: 8870
			BackAndForth,
			// Token: 0x040022A7 RID: 8871
			Translation
		}
	}
}
