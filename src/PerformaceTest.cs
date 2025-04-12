using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

// Token: 0x02000265 RID: 613
public class PerformaceTest
{
	// Token: 0x06001CDA RID: 7386 RVA: 0x000CC0DE File Offset: 0x000CA4DE
	public PerformaceTest(string testName)
	{
		this.testName = testName;
	}

	// Token: 0x06001CDB RID: 7387 RVA: 0x000CC110 File Offset: 0x000CA510
	public static void MeasureSpeed(string testName, Action action)
	{
		PerformaceTest performaceTest = new PerformaceTest(testName);
		performaceTest.Start();
		action();
		performaceTest.Stop();
		performaceTest.DebugLogTestResults();
	}

	// Token: 0x06001CDC RID: 7388 RVA: 0x000CC13C File Offset: 0x000CA53C
	public void Start()
	{
		this.startTime = Time.realtimeSinceStartup;
	}

	// Token: 0x06001CDD RID: 7389 RVA: 0x000CC149 File Offset: 0x000CA549
	public void StartSubTest(string subTestName)
	{
		this.subTestStartTimes[subTestName] = Time.realtimeSinceStartup;
		this.activeSubTests.Add(subTestName);
	}

	// Token: 0x06001CDE RID: 7390 RVA: 0x000CC168 File Offset: 0x000CA568
	public void StopSubTest(string subTestName)
	{
		float num = Time.realtimeSinceStartup - this.subTestStartTimes[subTestName];
		this.totalSubTestDuration += num;
		this.subTestDuration[subTestName] = num;
		this.activeSubTests.Remove(subTestName);
	}

	// Token: 0x06001CDF RID: 7391 RVA: 0x000CC1B0 File Offset: 0x000CA5B0
	public void StopSubTest()
	{
		string subTestName = this.activeSubTests[this.activeSubTests.Count - 1];
		this.StopSubTest(subTestName);
	}

	// Token: 0x06001CE0 RID: 7392 RVA: 0x000CC1E0 File Offset: 0x000CA5E0
	public void Stop()
	{
		this.endTime = Time.realtimeSinceStartup;
		for (int i = 0; i < this.activeSubTests.Count; i++)
		{
			string key = this.activeSubTests[i];
			float num = this.endTime - this.subTestStartTimes[key];
			this.subTestDuration[key] = num;
			this.totalSubTestDuration += num;
		}
		this.activeSubTests.Clear();
		this.totalDuration = this.endTime - this.startTime;
	}

	// Token: 0x06001CE1 RID: 7393 RVA: 0x000CC270 File Offset: 0x000CA670
	public List<string> GetResults()
	{
		List<string> list = new List<string>();
		int num = this.testName.Length;
		foreach (string text in this.subTestDuration.Keys)
		{
			num = Mathf.Max(num, text.Length);
		}
		string item = string.Format("Total time to do task: {0} : {1} second", this.testName.PadRight(num), this.totalDuration);
		list.Add(item);
		foreach (KeyValuePair<string, float> keyValuePair in this.subTestDuration)
		{
			list.Add(string.Format("\t\t {0} : {1} seconds", keyValuePair.Key.PadRight(num), keyValuePair.Value));
		}
		list.Add(string.Format("\t\t untracked:\t {0} seconds", this.totalDuration - this.totalSubTestDuration));
		return list;
	}

	// Token: 0x06001CE2 RID: 7394 RVA: 0x000CC3A8 File Offset: 0x000CA7A8
	public void DebugLogTestResults()
	{
		List<string> results = this.GetResults();
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < results.Count; i++)
		{
			stringBuilder.Append(results[i]);
			stringBuilder.AppendLine();
		}
		BWLog.Info(stringBuilder.ToString());
	}

	// Token: 0x04001792 RID: 6034
	private float startTime;

	// Token: 0x04001793 RID: 6035
	private string testName;

	// Token: 0x04001794 RID: 6036
	private float endTime;

	// Token: 0x04001795 RID: 6037
	private float totalDuration;

	// Token: 0x04001796 RID: 6038
	private float totalSubTestDuration;

	// Token: 0x04001797 RID: 6039
	private Dictionary<string, float> subTestStartTimes = new Dictionary<string, float>();

	// Token: 0x04001798 RID: 6040
	private Dictionary<string, float> subTestDuration = new Dictionary<string, float>();

	// Token: 0x04001799 RID: 6041
	private List<string> activeSubTests = new List<string>();
}
