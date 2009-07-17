/*
*	This program is the CONFIDENTIAL and PROPRIETARY property 
*	of Tomasello Software LLC. Any unauthorized use, reproduction or
*	transfer of this computer program is strictly prohibited.
*
*      Copyright (c) 2004 Tomasello Software LLC.
*	This is an unpublished work, and is subject to limited distribution and
*	restricted disclosure only. ALL RIGHTS RESERVED.
*
*			RESTRICTED RIGHTS LEGEND
*	Use, duplication, or disclosure by the Government is subject to
*	restrictions set forth in subparagraph (c)(1)(ii) of the Rights in
* 	Technical Data and Computer Software clause at DFARS 252.227-701.
*
*	Angel Island UO Shard	Version 1.0
*			Release A
*			March 25, 2004
*/

/* Scripts/Commands/IntMapOrphan.cs
 * CHANGELOG:
 *	10/21/08, Comment out console messages
 *	10/17/08, Adam
 *		Find and destroy orphaned items on the internal map specifically used the in SpawnerTempMob, SpawnerTempItem, and IsIntMapStorage storage models.
 *		Initial Version
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Gumps;
using Server.Prompts;
using Server.Targeting;
using Server.Misc;
using Server.Multis;
using Server.Regions;
using Server.SMTP;
using Server.Accounting;
using Server.Network;
using Server.Scripts.Commands;

namespace Server.Scripts.Commands
{

	public class IntMapOrphan
	{
		public static void Initialize()
		{
			Server.Commands.Register("IntMapOrphan", AccessLevel.Owner, new CommandEventHandler(IntMapOrphan_OnCommand));
		}

		[Usage("IntMapOrphan List | Mark(for cleanup)")]
		[Description("Find and destroy orphaned items on the internal map.")]
		public static void IntMapOrphan_OnCommand(CommandEventArgs e)
		{
			try
			{
				DoCommand(e);
			}
			catch (Exception ex)
			{
				LogHelper.LogException(ex);
			}
		}

		private static void DoCommand(CommandEventArgs e)
		{
			Dictionary<Serial, Serial> dict = new Dictionary<Serial, Serial>();
			string logname = "IntMapOrphan.log";

			if (e.ArgString.ToLower() == "list")
			{
				LoadDict(dict);
				
				LogHelper Logger = new LogHelper(logname, e.Mobile, true);
				int orphans=0;
				foreach (Serial serial in dict.Keys)
				{
					Item item = World.FindItem(serial);
					Mobile mob = World.FindMobile(serial);
					if (item != null)
						Logger.Log(LogType.Item, item, String.Format("Managed by Spawner {0}", dict[serial]));
					if (mob != null)
						Logger.Log(LogType.Mobile, mob, String.Format("Managed by Spawner {0}", dict[serial]));
					if (dict[serial] == Serial.MinusOne)
						orphans++;
				}

				e.Mobile.SendMessage(String.Format("{0} template objects found with {1} orphans.", dict.Count, orphans));
				e.Mobile.SendMessage(String.Format("Please see {0} for a list of template/spawner pairs. ", logname));

				Logger.Finish();
			}
			else if (e.ArgString.ToLower() == "mark")
			{
				LoadDict(dict);
				LogHelper Logger = new LogHelper(logname, e.Mobile, true);

				int orphans = 0;
				foreach (Serial serial in dict.Keys)
				{
					Item item = World.FindItem(serial);
					Mobile mob = World.FindMobile(serial);
					if (item != null && dict[serial] == Serial.MinusOne)
					{
						item.IsIntMapStorage = false;
						item.SpawnerTempItem = false;
						Logger.Log(LogType.Item, item, "Set to decay.");
					}
					if (mob != null && dict[serial] == Serial.MinusOne)
					{
						mob.SpawnerTempMob = false;
						Logger.Log(LogType.Mobile, mob, "Set to decay.");
					}
					if (dict[serial] == Serial.MinusOne)
						orphans++;
				}

				e.Mobile.SendMessage(String.Format("{0} template objects found with {1} orphans.", dict.Count, orphans));
				e.Mobile.SendMessage(String.Format("Please see {0} for a list cleared templates. ", logname));

				Logger.Finish();
			}
			else
				e.Mobile.SendMessage("Usage: IntMapOrphan List | Mark(for cleanup)");
		}

		private static void LoadDict(Dictionary<Serial, Serial> dict)
		{
			// find spawner template ITEMS and MOBILES
			//Console.WriteLine("Starting pass I");
			foreach (Item i in World.Items.Values)
			{
				if (i != null && i.Deleted == false)
				{
					if (i.IsIntMapStorage || i.SpawnerTempItem)
						dict[i.Serial] = Serial.MinusOne;
				}
			}

			// find spawner LOOT PACKS (items)
			//Console.WriteLine("Starting pass II");
			foreach (Mobile m in World.Mobiles.Values)
			{
				if (m != null && m.Deleted == false)
					if (m.SpawnerTempMob)
						dict[m.Serial] = Serial.MinusOne;
			}

			// associate all found items with parent spawners. Any that don't have a parent spawner is an ORPHAN
			//Console.WriteLine("Starting pass III");
			foreach (Item i in World.Items.Values)
			{
				if (i != null && i.Deleted == false && i is Spawner)
				{
					Spawner spawner = i as Spawner;
					// template ITEM
					if (spawner.TemplateItem != null && dict.ContainsKey(spawner.TemplateItem.Serial))
						dict[spawner.TemplateItem.Serial] = spawner.Serial;
					// template MOBILE
					if (spawner.TemplateMobile != null && dict.ContainsKey(spawner.TemplateMobile.Serial))
						dict[spawner.TemplateMobile.Serial] = spawner.Serial;
					// template LOOT
					if (spawner.LootPack != null && dict.ContainsKey(spawner.LootPack.Serial))
						dict[spawner.LootPack.Serial] = spawner.Serial;
				}
			}
		}

	}
}

