using System;
using UnityEngine;

namespace AmplifyOcclusion;

[Serializable]
public class VersionInfo
{
	public const byte Major = 1;

	public const byte Minor = 2;

	public const byte Release = 2;

	private static string StageSuffix = "_dev001";

	[SerializeField]
	private int m_major;

	[SerializeField]
	private int m_minor;

	[SerializeField]
	private int m_release;

	public int Number => m_major * 100 + m_minor * 10 + m_release;

	private VersionInfo()
	{
		m_major = 1;
		m_minor = 2;
		m_release = 2;
	}

	private VersionInfo(byte major, byte minor, byte release)
	{
		m_major = major;
		m_minor = minor;
		m_release = release;
	}

	public static string StaticToString()
	{
		return $"{1}.{2}.{2}" + StageSuffix;
	}

	public override string ToString()
	{
		return $"{m_major}.{m_minor}.{m_release}" + StageSuffix;
	}

	public static VersionInfo Current()
	{
		return new VersionInfo(1, 2, 2);
	}

	public static bool Matches(VersionInfo version)
	{
		if (version.m_major == 1 && version.m_minor == 2)
		{
			return 2 == version.m_release;
		}
		return false;
	}
}
