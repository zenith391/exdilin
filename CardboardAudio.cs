using System;
using System.Runtime.InteropServices;
using UnityEngine;

// Token: 0x0200000B RID: 11
public static class CardboardAudio
{
	// Token: 0x17000002 RID: 2
	// (get) Token: 0x06000031 RID: 49 RVA: 0x00003D8E File Offset: 0x0000218E
	public static int SampleRate
	{
		get
		{
			return CardboardAudio.sampleRate;
		}
	}

	// Token: 0x17000003 RID: 3
	// (get) Token: 0x06000032 RID: 50 RVA: 0x00003D95 File Offset: 0x00002195
	public static int NumChannels
	{
		get
		{
			return CardboardAudio.numChannels;
		}
	}

	// Token: 0x17000004 RID: 4
	// (get) Token: 0x06000033 RID: 51 RVA: 0x00003D9C File Offset: 0x0000219C
	public static int FramesPerBuffer
	{
		get
		{
			return CardboardAudio.framesPerBuffer;
		}
	}

	// Token: 0x06000034 RID: 52 RVA: 0x00003DA4 File Offset: 0x000021A4
	public static void Initialize(CardboardAudioListener listener, CardboardAudio.Quality quality)
	{
		if (!CardboardAudio.initialized)
		{
			AudioConfiguration configuration = AudioSettings.GetConfiguration();
			CardboardAudio.sampleRate = configuration.sampleRate;
			CardboardAudio.numChannels = (int)configuration.speakerMode;
			CardboardAudio.framesPerBuffer = configuration.dspBufferSize;
			if (CardboardAudio.numChannels != 2)
			{
				Debug.LogError("Only 'Stereo' speaker mode is supported by Cardboard.");
				return;
			}
			CardboardAudio.Initialize(quality, CardboardAudio.sampleRate, CardboardAudio.numChannels, CardboardAudio.framesPerBuffer);
			CardboardAudio.listenerTransform = listener.transform;
			CardboardAudio.initialized = true;
			Debug.Log(string.Concat(new object[]
			{
				"Cardboard audio system is initialized (Quality: ",
				quality,
				", Sample Rate: ",
				CardboardAudio.sampleRate,
				", Channels: ",
				CardboardAudio.numChannels,
				", Frames Per Buffer: ",
				CardboardAudio.framesPerBuffer,
				")."
			}));
		}
		else if (listener.transform != CardboardAudio.listenerTransform)
		{
			Debug.LogError("Only one CardboardAudioListener component is allowed in the scene.");
			UnityEngine.Object.Destroy(listener);
		}
	}

	// Token: 0x06000035 RID: 53 RVA: 0x00003EB4 File Offset: 0x000022B4
	public static void Shutdown(CardboardAudioListener listener)
	{
		if (CardboardAudio.initialized && listener.transform == CardboardAudio.listenerTransform)
		{
			CardboardAudio.initialized = false;
			CardboardAudio.Shutdown();
			CardboardAudio.sampleRate = -1;
			CardboardAudio.numChannels = -1;
			CardboardAudio.framesPerBuffer = -1;
			Debug.Log("Cardboard audio system is shutdown.");
		}
	}

	// Token: 0x06000036 RID: 54 RVA: 0x00003F07 File Offset: 0x00002307
	public static void ProcessAudioListener(float[] data, int length)
	{
		if (CardboardAudio.initialized)
		{
			CardboardAudio.ProcessListener(data, length);
		}
	}

	// Token: 0x06000037 RID: 55 RVA: 0x00003F1C File Offset: 0x0000231C
	public static void UpdateAudioListener(float globalGainDb, float worldScale)
	{
		if (CardboardAudio.initialized)
		{
			CardboardAudio.worldScaleInverse = 1f / worldScale;
			float listenerGain = CardboardAudio.ConvertAmplitudeFromDb(globalGainDb);
			Vector3 position = CardboardAudio.listenerTransform.position;
			Quaternion rotation = CardboardAudio.listenerTransform.rotation;
			CardboardAudio.ConvertAudioTransformFromUnity(ref position, ref rotation);
			CardboardAudio.SetListenerGain(listenerGain);
			CardboardAudio.SetListenerTransform(position.x, position.y, position.z, rotation.x, rotation.y, rotation.z, rotation.w);
		}
	}

	// Token: 0x06000038 RID: 56 RVA: 0x00003FA4 File Offset: 0x000023A4
	public static int CreateAudioSource(bool hrtfEnabled)
	{
		int result = -1;
		if (CardboardAudio.initialized)
		{
			result = CardboardAudio.CreateSource(hrtfEnabled);
		}
		return result;
	}

	// Token: 0x06000039 RID: 57 RVA: 0x00003FC5 File Offset: 0x000023C5
	public static void DestroyAudioSource(int id)
	{
		if (CardboardAudio.initialized)
		{
			CardboardAudio.DestroySource(id);
		}
	}

	// Token: 0x0600003A RID: 58 RVA: 0x00003FD8 File Offset: 0x000023D8
	public static void UpdateAudioSource(int id, Transform transform, float gainDb, AudioRolloffMode rolloffMode, float minDistance, float maxDistance, float alpha, float sharpness, float occlusion)
	{
		if (CardboardAudio.initialized)
		{
			float gain = CardboardAudio.ConvertAmplitudeFromDb(gainDb);
			Vector3 position = transform.position;
			Quaternion rotation = transform.rotation;
			float scale = CardboardAudio.worldScaleInverse * Mathf.Max(new float[]
			{
				transform.lossyScale.x,
				transform.lossyScale.y,
				transform.lossyScale.z
			});
			CardboardAudio.ConvertAudioTransformFromUnity(ref position, ref rotation);
			CardboardAudio.SetSourceDirectivity(id, alpha, sharpness);
			CardboardAudio.SetSourceGain(id, gain);
			CardboardAudio.SetSourceOcclusionIntensity(id, occlusion);
			if (rolloffMode != AudioRolloffMode.Custom)
			{
				CardboardAudio.SetSourceDistanceAttenuationMethod(id, rolloffMode, minDistance, maxDistance);
			}
			CardboardAudio.SetSourceTransform(id, position.x, position.y, position.z, rotation.x, rotation.y, rotation.z, rotation.w, scale);
		}
	}

	// Token: 0x0600003B RID: 59 RVA: 0x000040B8 File Offset: 0x000024B8
	public static int CreateAudioRoom()
	{
		int result = -1;
		if (CardboardAudio.initialized)
		{
			result = CardboardAudio.CreateRoom();
		}
		return result;
	}

	// Token: 0x0600003C RID: 60 RVA: 0x000040D8 File Offset: 0x000024D8
	public static void DestroyAudioRoom(int id)
	{
		if (CardboardAudio.initialized)
		{
			CardboardAudio.DestroyRoom(id);
		}
	}

	// Token: 0x0600003D RID: 61 RVA: 0x000040EC File Offset: 0x000024EC
	public static void UpdateAudioRoom(int id, Transform transform, CardboardAudioRoom.SurfaceMaterial[] materials, float reflectivity, float reverbGainDb, float reverbBrightness, float reverbTime, Vector3 size)
	{
		if (CardboardAudio.initialized)
		{
			Vector3 position = transform.position;
			Quaternion rotation = transform.rotation;
			Vector3 vector = Vector3.Scale(size, transform.lossyScale);
			vector = CardboardAudio.worldScaleInverse * new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
			CardboardAudio.ConvertAudioTransformFromUnity(ref position, ref rotation);
			float reverbGain = CardboardAudio.ConvertAmplitudeFromDb(reverbGainDb);
			CardboardAudio.SetRoomProperties(id, position.x, position.y, position.z, rotation.x, rotation.y, rotation.z, rotation.w, vector.x, vector.y, vector.z, materials, reflectivity, reverbGain, reverbBrightness, reverbTime);
		}
	}

	// Token: 0x0600003E RID: 62 RVA: 0x000041B8 File Offset: 0x000025B8
	public static float ComputeOcclusion(Transform sourceTransform)
	{
		float num = 0f;
		if (CardboardAudio.initialized)
		{
			Vector3 position = CardboardAudio.listenerTransform.position;
			Vector3 direction = sourceTransform.position - position;
			RaycastHit[] array = Physics.RaycastAll(position, direction, direction.magnitude);
			foreach (RaycastHit raycastHit in array)
			{
				if (raycastHit.transform != CardboardAudio.listenerTransform && raycastHit.transform != sourceTransform)
				{
					num += 1f;
				}
			}
		}
		return num;
	}

	// Token: 0x0600003F RID: 63 RVA: 0x0000425C File Offset: 0x0000265C
	public static Vector2[] Generate2dPolarPattern(float alpha, float order, int resolution)
	{
		Vector2[] array = new Vector2[resolution];
		float num = 6.28318548f / (float)resolution;
		for (int i = 0; i < resolution; i++)
		{
			float f = (float)i * num;
			float num2 = Mathf.Pow(Mathf.Abs(1f - alpha + alpha * Mathf.Cos(f)), order);
			array[i] = new Vector2(num2 * Mathf.Sin(f), num2 * Mathf.Cos(f));
		}
		return array;
	}

	// Token: 0x06000040 RID: 64 RVA: 0x000042D1 File Offset: 0x000026D1
	private static float ConvertAmplitudeFromDb(float db)
	{
		return Mathf.Pow(10f, 0.05f * db);
	}

	// Token: 0x06000041 RID: 65 RVA: 0x000042E4 File Offset: 0x000026E4
	private static void ConvertAudioTransformFromUnity(ref Vector3 position, ref Quaternion rotation)
	{
		CardboardAudio.pose.SetRightHanded(Matrix4x4.TRS(position, rotation, Vector3.one));
		position = CardboardAudio.pose.Position * CardboardAudio.worldScaleInverse;
		rotation = CardboardAudio.pose.Orientation;
	}

	// Token: 0x06000042 RID: 66
	[DllImport("audiopluginvrunity")]
	private static extern void ProcessListener([In] [Out] float[] data, int bufferSize);

	// Token: 0x06000043 RID: 67
	[DllImport("audiopluginvrunity")]
	private static extern void SetListenerGain(float gain);

	// Token: 0x06000044 RID: 68
	[DllImport("audiopluginvrunity")]
	private static extern void SetListenerTransform(float px, float py, float pz, float qx, float qy, float qz, float qw);

	// Token: 0x06000045 RID: 69
	[DllImport("audiopluginvrunity")]
	private static extern int CreateSource(bool enableHrtf);

	// Token: 0x06000046 RID: 70
	[DllImport("audiopluginvrunity")]
	private static extern void DestroySource(int sourceId);

	// Token: 0x06000047 RID: 71
	[DllImport("audiopluginvrunity")]
	private static extern void SetSourceDirectivity(int sourceId, float alpha, float order);

	// Token: 0x06000048 RID: 72
	[DllImport("audiopluginvrunity")]
	private static extern void SetSourceDistanceAttenuationMethod(int sourceId, AudioRolloffMode rolloffMode, float minDistance, float maxDistance);

	// Token: 0x06000049 RID: 73
	[DllImport("audiopluginvrunity")]
	private static extern void SetSourceGain(int sourceId, float gain);

	// Token: 0x0600004A RID: 74
	[DllImport("audiopluginvrunity")]
	private static extern void SetSourceOcclusionIntensity(int sourceId, float intensity);

	// Token: 0x0600004B RID: 75
	[DllImport("audiopluginvrunity")]
	private static extern void SetSourceTransform(int sourceId, float px, float py, float pz, float qx, float qy, float qz, float qw, float scale);

	// Token: 0x0600004C RID: 76
	[DllImport("audiopluginvrunity")]
	private static extern int CreateRoom();

	// Token: 0x0600004D RID: 77
	[DllImport("audiopluginvrunity")]
	private static extern void DestroyRoom(int roomId);

	// Token: 0x0600004E RID: 78
	[DllImport("audiopluginvrunity")]
	private static extern void SetRoomProperties(int roomId, float px, float py, float pz, float qx, float qy, float qz, float qw, float dx, float dy, float dz, CardboardAudioRoom.SurfaceMaterial[] materialNames, float reflectionScalar, float reverbGain, float reverbBrightness, float reverbTime);

	// Token: 0x0600004F RID: 79
	[DllImport("audiopluginvrunity")]
	private static extern void Initialize(CardboardAudio.Quality quality, int sampleRate, int numChannels, int framesPerBuffer);

	// Token: 0x06000050 RID: 80
	[DllImport("audiopluginvrunity")]
	private static extern void Shutdown();

	// Token: 0x04000095 RID: 149
	private static int sampleRate = -1;

	// Token: 0x04000096 RID: 150
	private static int numChannels = -1;

	// Token: 0x04000097 RID: 151
	private static int framesPerBuffer = -1;

	// Token: 0x04000098 RID: 152
	public const float distanceEpsilon = 0.01f;

	// Token: 0x04000099 RID: 153
	public const float maxDistanceLimit = 1000000f;

	// Token: 0x0400009A RID: 154
	public const float minDistanceLimit = 990099f;

	// Token: 0x0400009B RID: 155
	public const float maxGainDb = 24f;

	// Token: 0x0400009C RID: 156
	public const float minGainDb = -24f;

	// Token: 0x0400009D RID: 157
	public const float maxWorldScale = 1000f;

	// Token: 0x0400009E RID: 158
	public const float minWorldScale = 0.001f;

	// Token: 0x0400009F RID: 159
	public const float maxReverbBrightness = 1f;

	// Token: 0x040000A0 RID: 160
	public const float minReverbBrightness = -1f;

	// Token: 0x040000A1 RID: 161
	public const float maxReverbTime = 3f;

	// Token: 0x040000A2 RID: 162
	public const float maxReflectivity = 2f;

	// Token: 0x040000A3 RID: 163
	public const float occlusionDetectionInterval = 0.2f;

	// Token: 0x040000A4 RID: 164
	public const float occlusionLerpSpeed = 6f;

	// Token: 0x040000A5 RID: 165
	public const int numRoomSurfaces = 6;

	// Token: 0x040000A6 RID: 166
	private static bool initialized = false;

	// Token: 0x040000A7 RID: 167
	private static Transform listenerTransform = null;

	// Token: 0x040000A8 RID: 168
	private static MutablePose3D pose = new MutablePose3D();

	// Token: 0x040000A9 RID: 169
	private static float worldScaleInverse = 1f;

	// Token: 0x040000AA RID: 170
	private const string pluginName = "audiopluginvrunity";

	// Token: 0x0200000C RID: 12
	public enum Quality
	{
		// Token: 0x040000AC RID: 172
		Low,
		// Token: 0x040000AD RID: 173
		Medium,
		// Token: 0x040000AE RID: 174
		High
	}
}
