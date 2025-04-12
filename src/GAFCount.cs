using System;
using System.Globalization;
using System.IO;
using System.Text;
using SimpleJSON;

// Token: 0x02000165 RID: 357
public class GAFCount
{
	// Token: 0x06001570 RID: 5488 RVA: 0x000961A6 File Offset: 0x000945A6
	public GAFCount(GAF gaf, int count)
	{
		this.gaf = gaf;
		this.count = count;
		this.gafString = GAFCount.GetGAFString(gaf);
	}

	// Token: 0x06001571 RID: 5489 RVA: 0x000961C8 File Offset: 0x000945C8
	public static GAF JSONToGAF(string gafString)
	{
		GAF result = null;
		try
		{
			JObject obj = JSONDecoder.Decode(gafString);
			result = GAF.FromJSON(obj, true, true);
		}
		catch (Exception ex)
		{
			BWLog.Info(ex.Message);
		}
		return result;
	}

	// Token: 0x06001572 RID: 5490 RVA: 0x00096210 File Offset: 0x00094610
	public static string GetGAFString(GAF gaf)
	{
		StringBuilder stringBuilder = new StringBuilder(32768);
		StringWriter writer = new StringWriter(stringBuilder, CultureInfo.InvariantCulture);
		JSONStreamEncoder encoder = new JSONStreamEncoder(writer, 20);
		gaf.ToJSONCompact(encoder);
		return stringBuilder.ToString();
	}

	// Token: 0x06001573 RID: 5491 RVA: 0x0009624A File Offset: 0x0009464A
	public void SetGAF(GAF gaf, string gafString)
	{
		this.gaf = gaf;
		this.gafString = gafString;
	}

	// Token: 0x040010B0 RID: 4272
	public string gafString;

	// Token: 0x040010B1 RID: 4273
	public GAF gaf;

	// Token: 0x040010B2 RID: 4274
	public int count;

	// Token: 0x040010B3 RID: 4275
	public bool gafError;
}
