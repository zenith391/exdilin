using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class PerformaceTest
{
	private float startTime;

	private string testName;

	private float endTime;

	private float totalDuration;

	private float totalSubTestDuration;

	private Dictionary<string, float> subTestStartTimes = new Dictionary<string, float>();

	private Dictionary<string, float> subTestDuration = new Dictionary<string, float>();

	private List<string> activeSubTests = new List<string>();

	public PerformaceTest(string testName)
	{
		this.testName = testName;
	}

	public static void MeasureSpeed(string testName, Action action)
	{
		PerformaceTest performaceTest = new PerformaceTest(testName);
		performaceTest.Start();
		action();
		performaceTest.Stop();
		performaceTest.DebugLogTestResults();
	}

	public void Start()
	{
		startTime = Time.realtimeSinceStartup;
	}

	public void StartSubTest(string subTestName)
	{
		subTestStartTimes[subTestName] = Time.realtimeSinceStartup;
		activeSubTests.Add(subTestName);
	}

	public void StopSubTest(string subTestName)
	{
		float num = Time.realtimeSinceStartup - subTestStartTimes[subTestName];
		totalSubTestDuration += num;
		subTestDuration[subTestName] = num;
		activeSubTests.Remove(subTestName);
	}

	public void StopSubTest()
	{
		string subTestName = activeSubTests[activeSubTests.Count - 1];
		StopSubTest(subTestName);
	}

	public void Stop()
	{
		endTime = Time.realtimeSinceStartup;
		for (int i = 0; i < activeSubTests.Count; i++)
		{
			string key = activeSubTests[i];
			float num = endTime - subTestStartTimes[key];
			subTestDuration[key] = num;
			totalSubTestDuration += num;
		}
		activeSubTests.Clear();
		totalDuration = endTime - startTime;
	}

	public List<string> GetResults()
	{
		List<string> list = new List<string>();
		int num = testName.Length;
		foreach (string key in subTestDuration.Keys)
		{
			num = Mathf.Max(num, key.Length);
		}
		string item = $"Total time to do task: {testName.PadRight(num)} : {totalDuration} second";
		list.Add(item);
		foreach (KeyValuePair<string, float> item2 in subTestDuration)
		{
			list.Add($"\t\t {item2.Key.PadRight(num)} : {item2.Value} seconds");
		}
		list.Add($"\t\t untracked:\t {totalDuration - totalSubTestDuration} seconds");
		return list;
	}

	public void DebugLogTestResults()
	{
		List<string> results = GetResults();
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < results.Count; i++)
		{
			stringBuilder.Append(results[i]);
			stringBuilder.AppendLine();
		}
		BWLog.Info(stringBuilder.ToString());
	}
}
