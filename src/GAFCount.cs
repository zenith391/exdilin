using System;
using System.Globalization;
using System.IO;
using System.Text;
using SimpleJSON;

public class GAFCount
{
	public string gafString;

	public GAF gaf;

	public int count;

	public bool gafError;

	public GAFCount(GAF gaf, int count)
	{
		this.gaf = gaf;
		this.count = count;
		gafString = GetGAFString(gaf);
	}

	public static GAF JSONToGAF(string gafString)
	{
		GAF result = null;
		try
		{
			JObject obj = JSONDecoder.Decode(gafString);
			result = GAF.FromJSON(obj, nullOnFailure: true);
		}
		catch (Exception ex)
		{
			BWLog.Info(ex.Message);
		}
		return result;
	}

	public static string GetGAFString(GAF gaf)
	{
		StringBuilder stringBuilder = new StringBuilder(32768);
		StringWriter writer = new StringWriter(stringBuilder, CultureInfo.InvariantCulture);
		JSONStreamEncoder encoder = new JSONStreamEncoder(writer);
		gaf.ToJSONCompact(encoder);
		return stringBuilder.ToString();
	}

	public void SetGAF(GAF gaf, string gafString)
	{
		this.gaf = gaf;
		this.gafString = gafString;
	}
}
