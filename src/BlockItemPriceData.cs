using System;
using SimpleJSON;

// Token: 0x0200039D RID: 925
public class BlockItemPriceData
{
	// Token: 0x06002856 RID: 10326 RVA: 0x00129D68 File Offset: 0x00128168
	public BlockItemPriceData(JObject json)
	{
		this.blockItemID = BWJsonHelpers.PropertyIfExists(this.blockItemID, "block_item_id", json);
		this.stackPrice = BWJsonHelpers.PropertyIfExists(this.stackPrice, "a_la_carte_stack_price", json);
		this.stackSize = BWJsonHelpers.PropertyIfExists(this.stackSize, "a_la_carte_stack_size", json);
		this.goldPennies = BWJsonHelpers.PropertyIfExists(this.goldPennies, "gold_pennies", json);
		this.isCoinsValueFlatRate = BWJsonHelpers.PropertyIfExists(this.isCoinsValueFlatRate, "is_coins_value_flat_rate", json);
	}

	// Token: 0x17000198 RID: 408
	// (get) Token: 0x06002857 RID: 10327 RVA: 0x00129DEE File Offset: 0x001281EE
	// (set) Token: 0x06002858 RID: 10328 RVA: 0x00129DF6 File Offset: 0x001281F6
	public int blockItemID { get; private set; }

	// Token: 0x17000199 RID: 409
	// (get) Token: 0x06002859 RID: 10329 RVA: 0x00129DFF File Offset: 0x001281FF
	// (set) Token: 0x0600285A RID: 10330 RVA: 0x00129E07 File Offset: 0x00128207
	public int stackPrice { get; private set; }

	// Token: 0x1700019A RID: 410
	// (get) Token: 0x0600285B RID: 10331 RVA: 0x00129E10 File Offset: 0x00128210
	// (set) Token: 0x0600285C RID: 10332 RVA: 0x00129E18 File Offset: 0x00128218
	public int stackSize { get; private set; }

	// Token: 0x1700019B RID: 411
	// (get) Token: 0x0600285D RID: 10333 RVA: 0x00129E21 File Offset: 0x00128221
	// (set) Token: 0x0600285E RID: 10334 RVA: 0x00129E29 File Offset: 0x00128229
	public int goldPennies { get; private set; }

	// Token: 0x1700019C RID: 412
	// (get) Token: 0x0600285F RID: 10335 RVA: 0x00129E32 File Offset: 0x00128232
	// (set) Token: 0x06002860 RID: 10336 RVA: 0x00129E3A File Offset: 0x0012823A
	public bool isCoinsValueFlatRate { get; private set; }
}
