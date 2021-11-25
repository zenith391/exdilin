﻿using System;
using System.Collections.Generic;
using SimpleJSON;

// Token: 0x020003C3 RID: 963
public class BWUser
{
	// Token: 0x06002A27 RID: 10791 RVA: 0x001342A9 File Offset: 0x001326A9
	internal BWUser()
	{
		this.Reset();
	}

	// Token: 0x170001E6 RID: 486
	// (get) Token: 0x06002A28 RID: 10792 RVA: 0x001342B7 File Offset: 0x001326B7
	// (set) Token: 0x06002A29 RID: 10793 RVA: 0x001342BF File Offset: 0x001326BF
	internal string authToken { get; private set; }

	// Token: 0x170001E7 RID: 487
	// (get) Token: 0x06002A2A RID: 10794 RVA: 0x001342C8 File Offset: 0x001326C8
	// (set) Token: 0x06002A2B RID: 10795 RVA: 0x001342D0 File Offset: 0x001326D0
	internal BlocksInventory blocksInventory { get; private set; }

	// Token: 0x170001E8 RID: 488
	// (get) Token: 0x06002A2C RID: 10796 RVA: 0x001342D9 File Offset: 0x001326D9
	// (set) Token: 0x06002A2D RID: 10797 RVA: 0x001342E1 File Offset: 0x001326E1
	internal int coins { get; private set; }

	// Token: 0x170001E9 RID: 489
	// (get) Token: 0x06002A2E RID: 10798 RVA: 0x001342EA File Offset: 0x001326EA
	// (set) Token: 0x06002A2F RID: 10799 RVA: 0x001342F2 File Offset: 0x001326F2
	internal bool iosAccountLinkAvailable { get; private set; }

	// Token: 0x170001EA RID: 490
	// (get) Token: 0x06002A30 RID: 10800 RVA: 0x001342FB File Offset: 0x001326FB
	// (set) Token: 0x06002A31 RID: 10801 RVA: 0x00134303 File Offset: 0x00132703
	internal bool iosAccountLinkInitiated { get; private set; }

	// Token: 0x170001EB RID: 491
	// (get) Token: 0x06002A32 RID: 10802 RVA: 0x0013430C File Offset: 0x0013270C
	// (set) Token: 0x06002A33 RID: 10803 RVA: 0x00134314 File Offset: 0x00132714
	internal bool isAnonymous { get; private set; }

	// Token: 0x170001EC RID: 492
	// (get) Token: 0x06002A34 RID: 10804 RVA: 0x0013431D File Offset: 0x0013271D
	// (set) Token: 0x06002A35 RID: 10805 RVA: 0x00134325 File Offset: 0x00132725
	internal bool isUsernameBlocked { get; private set; }

	// Token: 0x170001ED RID: 493
	// (get) Token: 0x06002A36 RID: 10806 RVA: 0x0013432E File Offset: 0x0013272E
	// (set) Token: 0x06002A37 RID: 10807 RVA: 0x00134336 File Offset: 0x00132736
	internal string profileImageURL { get; private set; }

	// Token: 0x170001EE RID: 494
	// (get) Token: 0x06002A38 RID: 10808 RVA: 0x0013433F File Offset: 0x0013273F
	// (set) Token: 0x06002A39 RID: 10809 RVA: 0x00134347 File Offset: 0x00132747
	internal int userID { get; private set; }

	// Token: 0x170001EF RID: 495
	// (get) Token: 0x06002A3A RID: 10810 RVA: 0x00134350 File Offset: 0x00132750
	// (set) Token: 0x06002A3B RID: 10811 RVA: 0x00134358 File Offset: 0x00132758
	internal string username { get; private set; }

	// Token: 0x170001F0 RID: 496
	// (get) Token: 0x06002A3C RID: 10812 RVA: 0x00134361 File Offset: 0x00132761
	// (set) Token: 0x06002A3D RID: 10813 RVA: 0x00134369 File Offset: 0x00132769
	internal int userStatus { get; private set; }

	// Token: 0x170001F1 RID: 497
	// (get) Token: 0x06002A3E RID: 10814 RVA: 0x00134372 File Offset: 0x00132772
	// (set) Token: 0x06002A3F RID: 10815 RVA: 0x0013437A File Offset: 0x0013277A
	internal List<BWWorld> worlds { get; private set; }

	// Token: 0x170001F2 RID: 498
	// (get) Token: 0x06002A40 RID: 10816 RVA: 0x00134383 File Offset: 0x00132783
	// (set) Token: 0x06002A41 RID: 10817 RVA: 0x0013438B File Offset: 0x0013278B
	internal List<BWWorldTemplate> worldTemplates { get; private set; }

	// Token: 0x170001F3 RID: 499
	// (get) Token: 0x06002A42 RID: 10818 RVA: 0x00134394 File Offset: 0x00132794
	// (set) Token: 0x06002A43 RID: 10819 RVA: 0x0013439C File Offset: 0x0013279C
	internal List<string> purchasedU2UModelIds { get; private set; }

	// Token: 0x06002A44 RID: 10820 RVA: 0x001343A8 File Offset: 0x001327A8
	internal void Reset()
	{
		this.authToken = string.Empty;
		this.blocksInventory = null;
		this.coins = 0;
		this.iosAccountLinkAvailable = false;
		this.iosAccountLinkInitiated = false;
		this.isAnonymous = false;
		this.isUsernameBlocked = false;
		this.profileImageURL = string.Empty;
		this.userID = 0;
		this.username = string.Empty;
		this.userStatus = 0;
		this.worlds = new List<BWWorld>();
		this.worldTemplates = new List<BWWorldTemplate>();
	}

    public void UnlimitedBlockInventory()
    {
        this.blocksInventory = BlocksInventory.CreateUnlimited();
    }

    // Token: 0x06002A45 RID: 10821 RVA: 0x00134424 File Offset: 0x00132824
    internal void LoadFromJson(JObject userJson)
	{
		this.Reset();
		this.UpdateFromJson(userJson);
	}

	// Token: 0x06002A46 RID: 10822 RVA: 0x00134434 File Offset: 0x00132834
	internal void UpdateFromJson(JObject userJson)
	{
		this.authToken = BWJsonHelpers.PropertyIfExists(this.authToken, "auth_token", userJson);
		this.blocksInventory = BWJsonHelpers.PropertyIfExists(this.blocksInventory, "blocks_inventory_str", userJson);
		this.coins = BWJsonHelpers.PropertyIfExists(this.coins, "coins", userJson);
		this.iosAccountLinkAvailable = BWJsonHelpers.PropertyIfExists(this.iosAccountLinkAvailable, "ios_link_available", userJson);
		this.iosAccountLinkInitiated = BWJsonHelpers.PropertyIfExists(this.iosAccountLinkInitiated, "ios_link_initiated", userJson);
		this.isUsernameBlocked = BWJsonHelpers.PropertyIfExists(this.isUsernameBlocked, "is_username_blocked", userJson);
		this.profileImageURL = BWJsonHelpers.PropertyIfExists(this.profileImageURL, "profile_image_url", userJson);
		this.userID = BWJsonHelpers.PropertyIfExists(this.userID, "id", userJson);
		this.username = BWJsonHelpers.PropertyIfExists(this.username, "username", userJson);
		this.userStatus = BWJsonHelpers.PropertyIfExists(this.userStatus, "user_status", userJson);
		this.purchasedU2UModelIds = BWJsonHelpers.PropertyIfExists(this.purchasedU2UModelIds, "purchased_u2u_model_ids", userJson);
		BWJsonHelpers.AddForEachInArray<BWWorld>(this.worlds, "worlds", userJson, (JObject json) => new BWWorld(json));
		BWJsonHelpers.AddForEachInArray<BWWorldTemplate>(this.worldTemplates, "world_templates", userJson, (JObject json) => new BWWorldTemplate(json));
		BWWorldPublishCooldown.UpdateFromUserData(userJson);
		BWModelPublishCooldown.UpdateFromJson(userJson);
	}

	// Token: 0x06002A47 RID: 10823 RVA: 0x001345A8 File Offset: 0x001329A8
	internal void RemoteWorldUpdated(string worldID, JObject worldJson)
	{
		foreach (BWWorld bwworld in this.worlds)
		{
			if (bwworld.worldID == worldID)
			{
				bwworld.UpdateFromJson(worldJson);
				return;
			}
		}
		BWWorld item = new BWWorld(worldJson);
		this.worlds.Add(item);
	}

	// Token: 0x06002A48 RID: 10824 RVA: 0x00134630 File Offset: 0x00132A30
	internal Dictionary<string, string> AttributesForUserProfileUI()
	{
		return new Dictionary<string, string>
		{
			{
				"id",
				(!this.IsMrBlocksworld()) ? this.userID.ToString() : string.Empty
			},
			{
				"coins",
				this.coins.ToString()
			},
			{
				"username",
				this.username
			},
			{
				"profileImageUrl",
				this.profileImageURL
			},
			{
				"is_steam_user",
				this.IsSteamUser().ToString()
			},
			{
				"is_ios_user",
				this.IsIOSUser().ToString()
			}
		};
	}

	// Token: 0x06002A49 RID: 10825 RVA: 0x001346F8 File Offset: 0x00132AF8
	public bool IsPremiumUser()
	{
		return Util.IsPremiumUserStatus(this.userStatus) || this.IsMrBlocksworld();
	}

	// Token: 0x06002A4A RID: 10826 RVA: 0x00134713 File Offset: 0x00132B13
	public int PremiumMembershipTier()
	{
		return Util.PremiumMembershipTier(this.userStatus);
	}

	// Token: 0x06002A4B RID: 10827 RVA: 0x00134720 File Offset: 0x00132B20
	public bool IsSteamUser()
	{
		return Util.IsSteamUserStatus(this.userStatus) && !this.IsMrBlocksworld();
	}

	// Token: 0x06002A4C RID: 10828 RVA: 0x0013473E File Offset: 0x00132B3E
	public bool IsIOSUser()
	{
		return Util.IsIOSUserStatus(this.userStatus) && !Util.IsSteamUserStatus(this.userStatus) && !this.IsMrBlocksworld();
	}

	// Token: 0x06002A4D RID: 10829 RVA: 0x0013476C File Offset: 0x00132B6C
	public bool IsMrBlocksworld()
	{
		return Util.IsBlocksworldOfficialUser(this.userID);
	}

	// Token: 0x06002A4E RID: 10830 RVA: 0x00134779 File Offset: 0x00132B79
	internal static void LoadCurrentUser(JObject userJson)
	{
		BWUser.currentUser = new BWUser();
		BWUser.currentUser.LoadFromJson(userJson);
	}

	// Token: 0x06002A4F RID: 10831 RVA: 0x00134790 File Offset: 0x00132B90
	internal static void UpdateCurrentUser(JObject userJson)
	{
		if (BWUser.currentUser == null)
		{
			BWLog.Info("No current user to update, creating one...");
			BWUser.currentUser = new BWUser();
		}
		BWUser.currentUser.UpdateFromJson(userJson);
	}

	// Token: 0x06002A50 RID: 10832 RVA: 0x001347BB File Offset: 0x00132BBB
	public static void UpdateCurrentUserAndNotifyListeners(JObject userJson)
	{
		if (BWUser.currentUser == null)
		{
			BWLog.Error("Current user not loaded");
			return;
		}
		BWUser.currentUser.UpdateFromJson(userJson);
		BWUserDataManager.Instance.NotifyListeners();
	}

	// Token: 0x06002A51 RID: 10833 RVA: 0x001347E7 File Offset: 0x00132BE7
	internal static void SetCurrentUserCoinCount(int coins)
	{
		BWUser.currentUser.coins = coins;
		BWUserDataManager.Instance.NotifyListeners();
	}

	// Token: 0x06002A52 RID: 10834 RVA: 0x00134800 File Offset: 0x00132C00
	internal static void SetupDemoCurrentUser()
	{
		BWUser.currentUser = new BWUser();
		BWUser.currentUser.isAnonymous = true;
		BWUser.currentUser.blocksInventory = BlocksInventory.FromString(BWUser.DEMO_BLOCKS_INVENTORY_STR, false);
		BWUser.currentUser.userID = -1;
		BWUser.currentUser.username = "Guest Blockster";
	}

	// Token: 0x0400242D RID: 9261
	internal static BWUser currentUser;

	// Token: 0x0400243C RID: 9276
	public static List<string> expectedImageUrlsForUI = new List<string>
	{
		"profileImageUrl"
	};

	// Token: 0x0400243D RID: 9277
	public static List<string> expectedDataKeysForUI = new List<string>
	{
		"id",
		"coins",
		"username",
		"is_steam_user",
		"is_ios_user"
	};

	// Token: 0x0400243E RID: 9278
	private static readonly string DEMO_BLOCKS_INVENTORY_STR = "38:1|112:6|45:2|89:4|61:2|177:1|1496:1|1422:3|2183:2|19:15|84:12|710:8|20:6|304:4|763:4|384:4|178:4|186:1|187:1|485:2|191:1|234:2|426:1|308:1|236:1|173:2|172:2|175:3|989:3|56:;1|390:;1|399:;1|363:;1|165:1|167:1|166:1|168:1|215:1|216:1|217:1|218:1|155:1|201:1|53:2|37:1|75:1|176:1|446:1|405:1|129:4|132:2|134:2|25:6|91:6|8:6|74:4|693:4|622:4|1127:4|606:4|694:4|199:2|169:4|227:2|156:2|194:1|717:1|643:2|2194:6|6:;1|27:;1|90:;1|258:;1|59:;1|2166:;1|1204:;1|52:;1|2164:;1|1129:;1|92:;1|2160:;1|2165:;1|2167:;1|2154:;1|1239:;1|26:;1|392:;1|2168:;1|2156:;1|2162:;1|1115:;1|7:;1|658:;1|1202:;1|57:;1|2155:;1|1252:;1|47:;1|2163:;1|2157:;1|2158:;1|2153:;1|5:;1|1253:;1|9:;1|2152:;1|389:;1|2169:;1|604:;1|2159:;1|2161:;1|479:;1|492:;1|451:;1|259:;1|369:;1|476:;1|482:;1|2611:;1|472:;1|641:;1|381:;1|676:;1|1439:;1|1444:;1|1481:;1|625:;1|686:;1|687:;1|628:;1|656:;1|11:;1|773:;1|770:;1|771:;1|768:;1|769:;1|766:;1|767:;1|774:;1|775:;1|1756:;1|1972:;1|1153:;1|1124:;1|1117:;1|1179:;1|1168:;1|1152:;1|1120:;1|1113:;1|1190:;1|1180:;1|1806:;1|1986:;1|10:;1|1123:;1|12:;1|1728:;1|203:;1|497:;1|411:;1|4187:;1|3088:;1|434:;1|432:;1|433:;1|437:;1|438:;1|435:;1|436:;1|439:;1|440:;1|1791:;1|1870:;1|543:;1|512:;1|513:;1|516:;1|517:;1|514:;1|515:;1|510:;1|511:;1|1729:;1|1753:;1|533:;1|532:;1|501:;1|536:;1|503:;1|535:;1|534:;1|531:;1|530:;1|1979:;1|1755:;1|1144:;1|1142:;1|1143:;1|1147:;1|1148:;1|1145:;1|1146:;1|1149:;1|1135:;1|1805:;1|1838:;1|1498:;1|1442:;1|1505:;1|1443:;1|1446:;1|1445:;1|1490:;1|1447:;1|1450:;1|1495:;1|2003:;1|1877:;1|21:;1|1868:;1|1879:;1|24:;1|2186:;1|4:;1|1792:;1|1726:;1|1473:;1|1466:;1|29:;1|1903:;1|1874:;1|1819:;1|1829:;1|1826:;1|2023:;1|1998:;1|1875:;1|1945:;1|1994:;1|35:;1|1992:;1|1876:;1|1884:;1|1888:;1|1837:;1|2011:;1|1953:;1|1738:;1|1944:;1|1995:;1|33:;1|1997:;1|1885:;1|1915:;1|1987:;1|1813:;1|1859:;1|1862:;1|1990:;1|1931:;1|1899:;1|34:;1|1831:;1|1963:;1|1832:;1|1841:;1|2044:;1|1788:;1|1943:;1|1906:;1|1894:;1|1982:;1|31:;1|1761:;1|1821:;1|1905:;1|1866:;1|2001:;1|1765:;1|1863:;1|1961:;1|1836:;1|1878:;1|32:;1|1954:;1|1957:;1|1993:;1|1977:;1|1981:;1|1731:;1|1912:;1|1793:;1|1965:;1|1925:;1|496:;1|475:;1|488:;1|407:;1|749:;1|96:;1|95:;1|715:;1|840:;1|836:;1|813:;1|1174:;1|1163:;1|1122:;1|1203:;1|1409:;1|1410:;1|1396:;1|1397:;1|94:;1|1137:;1|465:;1|464:;1|463:;1|462:;1|461:;1|460:;1|459:;1|467:;1|362:;1|1830:;1|1881:;1|1125:;1|1133:;1|1177:;1|1191:;1|1175:;1|1112:;1|1116:;1|1172:;1|1150:;1|1854:;1|1732:;1|1956:;1|2025:;1|2035:;1|1857:;1|1890:;1|1937:;1|2009:;1|2042:;1|2039:;1|1989:;1|2031:;1|2033:;1|2032:;1|2029:;1|2028:;1|2030:;1|1659:;1|1660:;1|1661:;1|1662:;1|1713:;1|1712:;1|1711:;1|1740:;1|1716:;1|1715:;1|1714:;1|1720:;1|1719:;1|1893:;1|1883:;1|1723:;1|1975:;1|1966:;1|1853:;1|1851:;1|1852:;1|1849:;1|1850:;1|1847:;1|1835:;1|1855:;1|1856:;1|1918:;1|2006:;1|1973:;1|1790:;1|722:;1|831:;1|772:;1|816:;1|842:;1|713:;1|707:;1|739:;1|787:;1|737:;1|828:;1|759:;1|1702:;1|1701:;1|1699:;1|4185:;1|4186:;1|42:;1|364:;1|498:;1|454:;1|453:;1|452:;1|457:;1|456:;1|455:;1|487:;1|450:;1|449:;1|1939:;1|2012:;1|572:;1|574:;1|573:;1|569:;1|500:;1|571:;1|570:;1|576:;1|575:;1|1803:;1|1739:;1|562:;1|563:;1|504:;1|558:;1|559:;1|560:;1|561:;1|564:;1|565:;1|2045:;1|1708:;1|1187:;1|1189:;1|1188:;1|1184:;1|1183:;1|1186:;1|1185:;1|1182:;1|1181:;1|1980:;1|1808:;1|1480:;1|1423:;1|1425:;1|1424:;1|1427:;1|1426:;1|1429:;1|1428:;1|1475:;1|1430:;1|1934:;1|2008:;1|1491:;1|1476:;1|1477:;1|1462:;1|1485:;1|1479:;1|1483:;1|1474:;1|1435:;1|1463:;1|1452:;1|1431:;1|1448:;1|1502:;1|509:;1|600:;1|566:;1|591:;1|589:;1|590:;1|587:;1|588:;1|585:;1|586:;1|594:;1|595:;1|1933:;1|1882:;1|551:;1|549:;1|550:;1|547:;1|548:;1|545:;1|546:;1|552:;1|544:;1|1896:;1|1916:;1|526:;1|525:;1|524:;1|523:;1|522:;1|521:;1|520:;1|519:;1|518:;1|1822:;1|2046:;1|1199:;1|1197:;1|1198:;1|1195:;1|1196:;1|1193:;1|1194:;1|1200:;1|1201:;1|2020:;1|1707:;1|1646:;1|1609:;1|1625:;1|1642:;1|1655:;1|1627:;1|1613:;1|1651:;1|1600:;1|1568:;1|1639:;1|1630:;1|1589:;1|1631:;1|1567:;1|1603:;1|1534:;1|1524:;1|1521:;1|1508:;1|1523:;1|1532:;1|1509:;1|1528:;1|4191:;1|4192:;1|1288:;1|1313:;1|1265:;1|1344:;1|1342:;1|1343:;1|1340:;1|1341:;1|1338:;1|1339:;1|1345:;1|1346:;1|1880:;1|1904:;1|1364:;1|1324:;1|1363:;1|1367:;1|1368:;1|1365:;1|1366:;1|1360:;1|1361:;1|1991:;1|2026:;1|1307:;1|1306:;1|1319:;1|1312:;1|1311:;1|1323:;1|1309:;1|1334:;1|1303:;1|2014:;1|2021:;1|1308:;1|1305:;1|1321:;1|1326:;1|1329:;1|1322:;1|1310:;1|1317:;1|1304:;1|2027:;1|1940:;1|2483:;1|48:;1|49:;1|3:;1|2:;1|23:;1|1506:;1|2728:;1|2727:;1|2729:;1|2730:;1|2230:;1|2296:;1|2282:;1|2233:;1|2292:;1|2287:;1|2228:;1|2271:;1|2731:;1|2732:;1|2047:;1|1727:;1|1741:;1|1872:;1|2022:;1|1839:;1|2734:;1|2733:;1|2735:;1|2736:;1|2225:;1|2249:;1|2285:;1|2235:;1|2261:;1|2300:;1|2252:;1|2268:;1|2737:;1|2738:;1|1486:;1|1467:;1|1488:;1|1492:;1|1458:;1|1461:;1|1460:;1|1455:;1|1454:;1|1457:;1|1456:;1|1464:;1|1438:;1|2015:;1|1844:;1|1517:;1|1518:;1|1519:;1|1520:;1|1533:;1|1531:;1|1525:;1|1526:;1|1522:;1|1529:;1|1511:;1|1513:;1|1514:;1|1530:;1|1510:;1|1512:;1|1515:;1|416:;1|368:;1|417:;1|413:;1|412:;1|415:;1|414:;1|420:;1|419:;1|1816:;1|1709:;1|1470:;1|1437:;1|60:;1|71:;1|790:;1|780:;1|806:;1|810:;1|2725:;1|62:;1|63:;1|64:;1|65:;1|66:;1|67:;1|68:;1|69:;1|70:;1|2085:;1|2087:;1|2086:;1|2089:;1|2088:;1|2091:;1|2090:;1|2084:;1|2083:;1|1494:;1|73:;1|118:;1|143:;1|124:;1|142:;1|819:;1|809:;1|798:;1|834:;1|1700:;1|1703:;1|1704:;1|1754:;1|2024:;1|1764:;1|1747:;1|1817:;1|1718:;1|1825:;1|1985:;1|1710:;1|2048:;1|1887:;1|1735:;1|4193:;1|1705:;1|1773:;1|1771:;1|1772:;1|1769:;1|1770:;1|1767:;1|1768:;1|1775:;1|1776:;1|1908:;1|2036:;1|1922:;1|1921:;1|1920:;1|1926:;1|1988:;1|1924:;1|1923:;1|1928:;1|1927:;1|1955:;1|1907:;1|1949:;1|1978:;1|830:;1|814:;1|802:;1|821:;1|1811:;1|1762:;1|1867:;1|1744:;1|1717:;1|1745:;1|1748:;1|1964:;1|2037:;1|1932:;1|4196:;1|719:;1|732:;1|731:;1|730:;1|729:;1|728:;1|727:;1|735:;1|734:;1|2002:;1|1724:;1|1796:;1|1794:;1|1795:;1|1864:;1|1800:;1|1797:;1|1798:;1|1801:;1|1802:;1|2040:;1|2017:;1|1942:;1|1722:;1|777:;1|2739:;1|784:;1|833:;1|788:;1|2013:;1|1967:;1|1860:;1|1846:;1|1996:;1|1914:;1|1807:;1|1812:;1|1969:;1|2000:;1|799:;1|746:;1|747:;1|742:;1|743:;1|744:;1|745:;1|740:;1|741:;1|1809:;1|1897:;1|1784:;1|1786:;1|1785:;1|1781:;1|1780:;1|1783:;1|1782:;1|1779:;1|1778:;1|2019:;1|1930:;1|854:;1|900:;1|930:;1|873:;1|944:;1|943:;1|942:;1|909:;1|845:;1|938:;1|945:;1|911:;1|910:;1|1737:;1|1891:;1|2463:;1|914:;1|939:;1|861:;1|860:;1|862:;1|863:;1|864:;1|865:;1|870:;1|866:;1|867:;1|868:;1|871:;1|1734:;1|1730:;1|2474:;1|925:;1|847:;1|890:;1|882:;1|853:;1|916:;1|915:;1|918:;1|917:;1|919:;1|856:;1|913:;1|912:;1|1845:;1|1789:;1|2477:;1|846:;1|851:;1|869:;1|921:;1|908:;1|907:;1|906:;1|905:;1|904:;1|937:;1|936:;1|858:;1|903:;1|1777:;1|1824:;1|2475:;1|881:;1|940:;1|855:;1|877:;1|927:;1|934:;1|926:;1|902:;1|924:;1|933:;1|932:;1|920:;1|931:;1|1886:;1|1974:;1|2505:;1|872:;1|884:;1|852:;1|887:;1|889:;1|894:;1|888:;1|897:;1|898:;1|895:;1|896:;1|893:;1|886:;1|1751:;1|1873:;1|2495:;1|1261:;1|1286:;1|1269:;1|1333:;1|1331:;1|1337:;1|1255:;1|1347:;1|1260:;1|1284:;1|1357:;1|1335:;1|1336:;1|1827:;1|1721:;1|1503:;1|77:;1|1136:;1|76:;1|78:;1|1970:;1|1958:;1|1999:;1|79:;1|80:;1|2843:;1|2833:;1|2831:;1|359:;1|360:;1|357:;1|358:;1|354:;1|356:;1|355:;1|2211:;1|2218:;1|2213:;1|2204:;1|2197:;1|2209:;1|3374:;1|2289:;1|2288:;1|2266:;1|2290:;1|2291:;1|2304:;1|3373:;1|1128:;1|1138:;1|1164:;1|1173:;1|1139:;1|1160:;1|1140:;1|1141:;1|1130:;1|1861:;1|1869:;1|1169:;1|1170:;1|1154:;1|1156:;1|1155:;1|1126:;1|1157:;1|1121:;1|1171:;1|1910:;1|2034:;1|1680:;1|1681:;1|1682:;1|1683:;1|1684:;1|1685:;1|1686:;1|1678:;1|1679:;1|1951:;1|1743:;1|1693:;1|1694:;1|1695:;1|1689:;1|1690:;1|1691:;1|1692:;1|1687:;1|1688:;1|1865:;1|1960:;1|1205:;1|2207:;1|1584:;1|1581:;1|81:;1|2216:;1|85:;1|86:;1|83:;1|87:;1|88:;1|54:;1|55:;1|114:;1|113:;1|508:;1|507:;1|506:;1|601:;1|1499:;1|2182:;1|2180:;1|2181:;1|2178:;1|2179:;1|2176:;1|2177:;1|2174:;1|2175:;1|2184:;1|2191:;1|181:;1|1131:;1|1558:;1|1556:;1|1562:;1|1561:;1|1572:;1|1559:;1|1560:;1|1564:;1|1565:;1|2629:;1|2630:;1|1645:;1|1618:;1|1617:;1|1616:;1|1615:;1|1614:;1|1638:;1|1612:;1|1611:;1|2631:;1|2632:;1|2193:;1|2187:;1|2173:;1|2189:;1|2195:;1|2172:;1|2170:;1|2188:;1|2196:;1|2185:;1|2171:;1|2192:;1|2409:;1|2450:;1|2455:;1|2407:;1|2399:;1|2368:;1|2374:;1|2386:;1|2370:;1|2372:;1|2404:;1|2445:;1|2392:;1|2431:;1|2408:;1|2606:;1|2582:;1|2551:;1|2623:;1|2566:;1|2618:;1|2617:;1|2616:;1|2602:;1|2614:;1|2613:;1|2612:;1|2597:;1|2622:;1|2598:;1|2547:;1|2573:;1|2544:;1|2601:;1|2570:;1|2569:;1|2575:;1|2567:;1|2579:;1|2578:;1|2603:;1|2552:;1|2574:;1|2581:;1|2607:;1|2548:;1|2682:;1|2723:;1|2724:;1|2683:;1|2684:;1|2685:;1|2686:;1|2687:;1|2688:;1|2689:;1|2690:;1|2691:;1|2692:;1|2693:;1|2694:;1|2695:;1|2696:;1|2697:;1|2698:;1|2699:;1|2700:;1|2701:;1|2702:;1|2703:;1|2704:;1|2705:;1|2706:;1|2707:;1|2708:;1|2709:;1|2710:;1|2711:;1|2712:;1|2713:;1|2714:;1|2715:;1|2716:;1|2717:;1|2718:;1|2719:;1|2720:;1|2721:;1|2722:;1|2853:;1|2877:;1|2860:;1|2852:;1|2863:;1|3186:;1|3187:;1|3188:;1|3189:;1|3190:;1|3191:;1|3192:;1|3193:;1|3194:;1|3195:;1|3196:;1|2867:;1|2856:;1|2864:;1|2865:;1|2862:;1|2858:;1|2873:;1|2869:;1|2876:;1|2871:;1|2872:;1|2752:;1|2753:;1|2754:;1|2762:;1|2761:;1|2755:;1|2756:;1|2757:;1|2758:;1|2759:;1|2760:;1|2764:;1|2765:;1|2766:;1|2767:;1|2770:;1|223:;1|224:;1|226:;1|225:;1|2741:;1|2857:;1|2763:;1|4155:;1|4157:;1|4158:;1|4159:;1|4156:;1|2875:;1|2768:;1|2769:;1|28:;1|17:;1|100:;1|101:;1|102:;1|103:;1|104:;1|105:;1|106:;1|3214:;1|4188:;1|3218:;1|3228:;1|3264:;1|3053:;1|3059:;1|3269:;1|3271:;1|3216:;1|3268:;1|3267:;1|3270:;1|3217:;1|3273:;1|3272:;1|3282:;1|4182:;1|3283:;1|3284:;1|3301:;1|3302:;1|3303:;1|3304:;1|3305:;1|3365:;1|3318:;1|3366:;1|3319:;1|3320:;1|3299:;1|3300:;1|3316:;1|3315:;1|3330:;1|3331:;1|4026:;1|4027:;1|4028:;1|3332:;1|3333:;1|4184:;1|4183:;1|3337:;1|3338:;1|3334:;1|3999:;1|4000:;1|4001:;1|4102:;1|4103:;1|4104:;1|3364:;1|3363:;1|4030:;1|4029:;1|3317:;1|3341:;1|3343:;1|3345:;1|3347:;1|3349:;1|3351:;1|3353:;1|3355:;1|3357:;1|3359:;1|3361:;1|3340:;1|3342:;1|3344:;1|3346:;1|3348:;1|3350:;1|3352:;1|3354:;1|3356:;1|3358:;1|3360:;1|3362:;1|4002:;1|4004:;1|4006:;1|4008:;1|4010:;1|4012:;1|4014:;1|4016:;1|4018:;1|4020:;1|4022:;1|4024:;1|4003:;1|4005:;1|4007:;1|4009:;1|4011:;1|4013:;1|4015:;1|4017:;1|4019:;1|4021:;1|4023:;1|4025:;1|3321:;1|3322:;1|3329:;1|3259:;1|3262:;1|3221:;1|3219:;1|3220:;1|3224:;1|3225:;1|3222:;1|3223:;1|3226:;1|3227:;1|3256:;1|3257:;1|3246:;1|3233:;1|3234:;1|3237:;1|3238:;1|3235:;1|3236:;1|3231:;1|3232:;1|3263:;1|3258:;1|3242:;1|3241:;1|3229:;1|5245:;1|3230:;1|5344:;1|5343:;1|3240:;1|3239:;1|3260:;1|3261:;1|3250:;1|3248:;1|3249:;1|3253:;1|3254:;1|3251:;1|3252:;1|3255:;1|3247:;1|2565:;1|3274:;1|3175:;1|3275:;1|3335:;1|3336:;1|13:;1|15:;1|14:;1|16:;1|4153:;1|43:;1|44:;1|4035:;1|4036:;1|4037:;1|4038:;1|4031:;1|4032:;1|4033:;1|4034:;1|3116:;1|3140:;1|3164:;1|3172:;1|2886:;1|3119:;1|3115:;1|3120:;1|3162:;1|3123:;1|3138:;1|3110:;1|3143:;1|3149:;1|3160:;1|3105:;1";
}
