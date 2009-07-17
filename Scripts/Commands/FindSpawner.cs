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
 * 	Technical Data and Computer Software clause at DFARS 252.227-7013.
 *
 *	Angel Island UO Shard	Version 1.0
 *			Release A
 *			March 25, 2004
 */

/* /Scripts/Commands/FindSpawner.cs
 * ChangeLog
 *	03/28/05, erlein
 *		Altered so utilises generic call to Log(LogType.Item, etc.) in LogHelper class.
 *	03/25/05, erlein
 *		Integrated with LogHelper class.
 *		Normalized output.
 *  02/27/05, erlein
 *		Altered so Admin priviledges required for [findspawner <item-type>.
 *		Added ChestItemSpawners to matching process.
 *	02/25/05, erlein
 *		Altered output format to fit in single journal line and
 *		reflect running status of the spawner in question.
 *	02/24/05, erlein
 *		Added Z co-ordinate to spawner location display and changed
 *		distance approximation to display in tiles as opposed to differential
 *		co-ordinates.
 *	02/24/05, erlein
 *		Initial creation - designed to retrieve list of spawners
 *		spawning mobile type passed as parameter.
*/
using System;
using System.Collections;
using Server;
using Server.Mobiles;
using Server.Items;
using Server.Network;
using System.Text;
using System.Text.RegularExpressions;

namespace Server.Scripts.Commands
{
	public class FindSpawner
	{

		// Spawner search match holder (sortable)

		public struct SpawnerMatch : IComparable
		{
			public string Status;
			public string Matched;
			public int Distance;
			public bool Item;
			public object Sp;

			public Int32 CompareTo(Object obj)
			{
				SpawnerMatch tmpObj = (SpawnerMatch)obj;
				return (-this.Distance.CompareTo(tmpObj.Distance));
			}
		}

		public static void Initialize()
		{
			Server.Commands.Register("FindSpawner", AccessLevel.Counselor, new CommandEventHandler(FindSpawner_OnCommand));
		}

		[Usage("FindSpawner <mobile type name> (<tile range>)")]
		[Description("Finds locations of all spawners spawning specified mobile type.")]
		private static void FindSpawner_OnCommand(CommandEventArgs arg)
		{
			Mobile from = arg.Mobile;

			int X = from.Location.X;
			int Y = from.Location.Y;

			string SearchText = arg.GetString(0);
			int TileRange = arg.GetInt32(1);

			// Validate parameters

			if (SearchText == "")
			{
				from.SendMessage("To use : [findspawner <type> (<range>)");
				return;
			}

			Regex InvalidPatt = new Regex("[^-a-zA-Z0-9' ]");

			if (InvalidPatt.IsMatch(SearchText))
			{
				from.SendMessage("Invalid characters used in type or range specification.");
				return;
			}


			// Perform search and retrieve list of spawner matches

			LogHelper Logger = new LogHelper("findspawner.log", from, true);

			ArrayList SpawnerList = FindMobSpawners(SearchText, X, Y, TileRange);

			if (SpawnerList.Count > 0)
			{

				// Have results so sort, loop and display message for each match

				SpawnerList.Sort();

				foreach (SpawnerMatch ms in SpawnerList)
				{
					if (ms.Item == false || from.AccessLevel == Server.AccessLevel.Administrator)
						Logger.Log(LogType.Item, ms.Sp, string.Format("{0}:{1}:{2}",
												ms.Matched,
												ms.Distance,
												ms.Status));
				}

			}

			Logger.Finish();

		}

		// Searches for spawners and returns struct list containing
		// relevant detail

		public static ArrayList FindMobSpawners(string searchtext, int x, int y, int tilerange)
		{
			ArrayList SpawnerList = new ArrayList();
			Regex SearchPattern = new Regex(searchtext.ToLower());

			// Loop through mobiles and check for Spawners

			foreach (Item item in World.Items.Values)
			{

				if (item is Server.Mobiles.Spawner)
				{

					Spawner sp = (Spawner)item;

					// Now check range / ignore range accordingly

					int spX = sp.Location.X - x;
					int spY = sp.Location.Y - y;

					if ((tilerange == 0) || (
						(sqr(sp.Location.X - x) <= sqr(tilerange) &&
						 sqr(sp.Location.Y - y) <= sqr(tilerange))
										))
					{

						// Loop through spawners' creature list and match
						// against search text

						foreach (string CreatureName in sp.CreaturesName)
						{

							if (SearchPattern.IsMatch(CreatureName.ToLower()))
							{

								SpawnerMatch ms = new SpawnerMatch();

								ms.Item = false;
								ms.Sp = sp;

								// Check if item type

								Type TestType = SpawnerType.GetType(CreatureName);
								string strTest = TestType.ToString();

								Regex InvalidPatt = new Regex("^Server.Item");

								if (InvalidPatt.IsMatch(strTest))
								{
									ms.Item = true;
								}
								// We have a match! Create new match struct
								// and add to return reference list

								if (sp.Running == true)
									ms.Status = "on";
								else
									ms.Status = "off";

								ms.Matched = CreatureName;
								ms.Distance = (int)Math.Sqrt(sqr(spX) + sqr(spY));

								SpawnerList.Add(ms);

							}

						}

					}

				}

				if (item is Server.Items.ChestItemSpawner)
				{

					ChestItemSpawner sp = (ChestItemSpawner)item;

					// Now check range / ignore range accordingly

					int spX = sp.Location.X - x;
					int spY = sp.Location.Y - y;

					if ((tilerange == 0) || (
						(sqr(sp.Location.X - x) <= sqr(tilerange) &&
						 sqr(sp.Location.Y - y) <= sqr(tilerange))
										))
					{

						// Loop through spawners' creature list and match
						// against search text

						foreach (string ItemName in sp.ItemsName)
						{

							if (SearchPattern.IsMatch(ItemName.ToLower()))
							{

								SpawnerMatch ms = new SpawnerMatch();

								ms.Item = false;
								ms.Sp = sp;

								// Check if item type

								Type TestType = SpawnerType.GetType(ItemName);
								string strTest = TestType.ToString();

								Regex InvalidPatt = new Regex("^Server.Item");

								if (InvalidPatt.IsMatch(strTest))
								{
									ms.Item = true;
								}
								// We have a match! Create new match struct
								// and add to return reference list

								if (sp.Running == true)
									ms.Status = "on";
								else
									ms.Status = "off";

								ms.Matched = ItemName;
								ms.Distance = (int)Math.Sqrt(sqr(spX) + sqr(spY));

								SpawnerList.Add(ms);

							}

						}

					}

				}

			}

			return (SpawnerList);
		}

		private static int sqr(int num)
		{
			return ((num * num));
		}

	}

}
