using System;
using System.Runtime.InteropServices;
using UnityEngine;

public static class CardboardAudio
{
	public enum Quality
	{
		Low,
		Medium,
		High
	}

	private static int sampleRate = -1;

	private static int numChannels = -1;

	private static int framesPerBuffer = -1;

	public const float distanceEpsilon = 0.01f;

	public const float maxDistanceLimit = 1000000f;

	public const float minDistanceLimit = 990099f;

	public const float maxGainDb = 24f;

	public const float minGainDb = -24f;

	public const float maxWorldScale = 1000f;

	public const float minWorldScale = 0.001f;

	public const float maxReverbBrightness = 1f;

	public const float minReverbBrightness = -1f;

	public const float maxReverbTime = 3f;

	public const float maxReflectivity = 2f;

	public const float occlusionDetectionInterval = 0.2f;

	public const float occlusionLerpSpeed = 6f;

	public const int numRoomSurfaces = 6;

	private static bool initialized = false;

	private static Transform listenerTransform = null;

	private static MutablePose3D pose = new MutablePose3D();

	private static float worldScaleInverse = 1f;

	private const string pluginName = "audiopluginvrunity";

	public static int SampleRate => sampleRate;

	public static int NumChannels => numChannels;

	public static int FramesPerBuffer => framesPerBuffer;

	public static void Initialize(CardboardAudioListener listener, Quality quality)
	{
		if (!initialized)
		{
			AudioConfiguration configuration = AudioSettings.GetConfiguration();
			sampleRate = configuration.sampleRate;
			numChannels = (int)configuration.speakerMode;
			framesPerBuffer = configuration.dspBufferSize;
			if (numChannels != 2)
			{
				Debug.LogError("Only 'Stereo' speaker mode is supported by Cardboard.");
				return;
			}
			Initialize(quality, sampleRate, numChannels, framesPerBuffer);
			listenerTransform = listener.transform;
			initialized = true;
			Debug.Log(string.Concat("Cardboard audio system is initialized (Quality: ", quality, ", Sample Rate: ", sampleRate, ", Channels: ", numChannels, ", Frames Per Buffer: ", framesPerBuffer, ")."));
		}
		else if (listener.transform != listenerTransform)
		{
			Debug.LogError("Only one CardboardAudioListener component is allowed in the scene.");
			UnityEngine.Object.Destroy(listener);
		}
	}

	public static void Shutdown(CardboardAudioListener listener)
	{
		if (initialized && listener.transform == listenerTransform)
		{
			initialized = false;
			Shutdown();
			sampleRate = -1;
			numChannels = -1;
			framesPerBuffer = -1;
			Debug.Log("Cardboard audio system is shutdown.");
		}
	}

	public static void ProcessAudioListener(float[] data, int length)
	{
		if (initialized)
		{
			ProcessListener(data, length);
		}
	}

	public static void UpdateAudioListener(float globalGainDb, float worldScale)
	{
		if (initialized)
		{
			worldScaleInverse = 1f / worldScale;
			float listenerGain = ConvertAmplitudeFromDb(globalGainDb);
			Vector3 position = listenerTransform.position;
			Quaternion rotation = listenerTransform.rotation;
			ConvertAudioTransformFromUnity(ref position, ref rotation);
			SetListenerGain(listenerGain);
			SetListenerTransform(position.x, position.y, position.z, rotation.x, rotation.y, rotation.z, rotation.w);
		}
	}

	public static int CreateAudioSource(bool hrtfEnabled)
	{
		int result = -1;
		if (initialized)
		{
			result = CreateSource(hrtfEnabled);
		}
		return result;
	}

	public static void DestroyAudioSource(int id)
	{
		if (initialized)
		{
			DestroySource(id);
		}
	}

	public static void UpdateAudioSource(int id, Transform transform, float gainDb, AudioRolloffMode rolloffMode, float minDistance, float maxDistance, float alpha, float sharpness, float occlusion)
	{
		if (initialized)
		{
			float gain = ConvertAmplitudeFromDb(gainDb);
			Vector3 position = transform.position;
			Quaternion rotation = transform.rotation;
			float scale = worldScaleInverse * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
			ConvertAudioTransformFromUnity(ref position, ref rotation);
			SetSourceDirectivity(id, alpha, sharpness);
			SetSourceGain(id, gain);
			SetSourceOcclusionIntensity(id, occlusion);
			if (rolloffMode != AudioRolloffMode.Custom)
			{
				SetSourceDistanceAttenuationMethod(id, rolloffMode, minDistance, maxDistance);
			}
			SetSourceTransform(id, position.x, position.y, position.z, rotation.x, rotation.y, rotation.z, rotation.w, scale);
		}
	}

	public static int CreateAudioRoom()
	{
		int result = -1;
		if (initialized)
		{
			result = CreateRoom();
		}
		return result;
	}

	public static void DestroyAudioRoom(int id)
	{
		if (initialized)
		{
			DestroyRoom(id);
		}
	}

	public static void UpdateAudioRoom(int id, Transform transform, CardboardAudioRoom.SurfaceMaterial[] materials, float reflectivity, float reverbGainDb, float reverbBrightness, float reverbTime, Vector3 size)
	{
		if (initialized)
		{
			Vector3 position = transform.position;
			Quaternion rotation = transform.rotation;
			Vector3 vector = Vector3.Scale(size, transform.lossyScale);
			vector = worldScaleInverse * new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
			ConvertAudioTransformFromUnity(ref position, ref rotation);
			float reverbGain = ConvertAmplitudeFromDb(reverbGainDb);
			SetRoomProperties(id, position.x, position.y, position.z, rotation.x, rotation.y, rotation.z, rotation.w, vector.x, vector.y, vector.z, materials, reflectivity, reverbGain, reverbBrightness, reverbTime);
		}
	}

	public static float ComputeOcclusion(Transform sourceTransform)
	{
		float num = 0f;
		if (initialized)
		{
			Vector3 position = listenerTransform.position;
			Vector3 direction = sourceTransform.position - position;
			RaycastHit[] array = Physics.RaycastAll(position, direction, direction.magnitude);
			RaycastHit[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				RaycastHit raycastHit = array2[i];
				if (raycastHit.transform != listenerTransform && raycastHit.transform != sourceTransform)
				{
					num += 1f;
				}
			}
		}
		return num;
	}

	public static Vector2[] Generate2dPolarPattern(float alpha, float order, int resolution)
	{
		Vector2[] array = new Vector2[resolution];
		float num = (float)Math.PI * 2f / (float)resolution;
		for (int i = 0; i < resolution; i++)
		{
			float f = (float)i * num;
			float num2 = Mathf.Pow(Mathf.Abs(1f - alpha + alpha * Mathf.Cos(f)), order);
			array[i] = new Vector2(num2 * Mathf.Sin(f), num2 * Mathf.Cos(f));
		}
		return array;
	}

	private static float ConvertAmplitudeFromDb(float db)
	{
		return Mathf.Pow(10f, 0.05f * db);
	}

	private static void ConvertAudioTransformFromUnity(ref Vector3 position, ref Quaternion rotation)
	{
		pose.SetRightHanded(Matrix4x4.TRS(position, rotation, Vector3.one));
		position = pose.Position * worldScaleInverse;
		rotation = pose.Orientation;
	}

	[DllImport("audiopluginvrunity")]
	private static extern void ProcessListener([In][Out] float[] data, int bufferSize);

	[DllImport("audiopluginvrunity")]
	private static extern void SetListenerGain(float gain);

	[DllImport("audiopluginvrunity")]
	private static extern void SetListenerTransform(float px, float py, float pz, float qx, float qy, float qz, float qw);

	[DllImport("audiopluginvrunity")]
	private static extern int CreateSource(bool enableHrtf);

	[DllImport("audiopluginvrunity")]
	private static extern void DestroySource(int sourceId);

	[DllImport("audiopluginvrunity")]
	private static extern void SetSourceDirectivity(int sourceId, float alpha, float order);

	[DllImport("audiopluginvrunity")]
	private static extern void SetSourceDistanceAttenuationMethod(int sourceId, AudioRolloffMode rolloffMode, float minDistance, float maxDistance);

	[DllImport("audiopluginvrunity")]
	private static extern void SetSourceGain(int sourceId, float gain);

	[DllImport("audiopluginvrunity")]
	private static extern void SetSourceOcclusionIntensity(int sourceId, float intensity);

	[DllImport("audiopluginvrunity")]
	private static extern void SetSourceTransform(int sourceId, float px, float py, float pz, float qx, float qy, float qz, float qw, float scale);

	[DllImport("audiopluginvrunity")]
	private static extern int CreateRoom();

	[DllImport("audiopluginvrunity")]
	private static extern void DestroyRoom(int roomId);

	[DllImport("audiopluginvrunity")]
	private static extern void SetRoomProperties(int roomId, float px, float py, float pz, float qx, float qy, float qz, float qw, float dx, float dy, float dz, CardboardAudioRoom.SurfaceMaterial[] materialNames, float reflectionScalar, float reverbGain, float reverbBrightness, float reverbTime);

	[DllImport("audiopluginvrunity")]
	private static extern void Initialize(Quality quality, int sampleRate, int numChannels, int framesPerBuffer);

	[DllImport("audiopluginvrunity")]
	private static extern void Shutdown();
}
