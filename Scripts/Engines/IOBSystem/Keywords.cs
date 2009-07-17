/***************************************************************************
 *                               CREDITS
 *                         -------------------
 *                         : (C) 2004-2009 Luke Tomasello (AKA Adam Ant)
 *                         :   and the Angel Island Software Team
 *                         :   luke@tomasello.com
 *                         :   Official Documentation:
 *                         :   www.game-master.net, wiki.game-master.net
 *                         :   Official Source Code (SVN Repository):
 *                         :   http://game-master.net:8050/svn/angelisland
 *                         : 
 *                         : (C) May 1, 2002 The RunUO Software Team
 *                         :   info@runuo.com
 *
 *   Give credit where credit is due!
 *   Even though this is 'free software', you are encouraged to give
 *    credit to the individuals that spent many hundreds of hours
 *    developing this software.
 *   Many of the ideas you will find in this Angel Island version of 
 *   Ultima Online are unique and one-of-a-kind in the gaming industry! 
 *
 ***************************************************************************/

/***************************************************************************
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 ***************************************************************************/

/* Scripts/Engines/IOBSystem/Keywords.cs
 * CHANGELOG:
 *  05/25/09, plasma
 *		- Prevent post placment too near a door, fixed some typos.
 *		- Prevent placement unless there's 1 empty tile in every direction
 *  04/09/09, plasma
 *		- Prevent post placment too near another post, fixed a bug.
 *	04/06/09, plasma
 *		- Fixed bug with guard post placement, wasn't assigning new Map
 *  10/02/08, plasma
 *		- Consolidate sporadic speech handlers 
 *		- Add new speech handler for adding guard posts
 *		- Add copyright notice & changelog
 */

using System;
using System.Collections.Generic;
using System.Text;
using Server;
using Server.Items;
using Server.Gumps;
using Server.Mobiles;
using Server.Engines.IOBSystem.Targets;

namespace Server.Engines.IOBSystem
{
	class Keywords
	{
		public static void Initialize()
		{
			EventSink.Speech += new SpeechEventHandler(EventSink_Speech);
		}

		private static void EventSink_Speech(SpeechEventArgs e)
		{
			Mobile from = e.Mobile;
			int[] keywords = e.Keywords;

			for (int i = 0; i < keywords.Length; ++i)
			{
				switch (keywords[i])
				{
					case 0x00EC: // *showscore*
						{
							if (KinSystemSettings.PointsEnabled && from is PlayerMobile && from.Alive)
							{
								PlayerMobile pm = from as PlayerMobile;
								from.PublicOverheadMessage(Server.Network.MessageType.Regular,0x3B2, true, string.Format("Unassisted: {0:0.00}", pm.KinSoloPoints));
								from.PublicOverheadMessage(Server.Network.MessageType.Regular, 0x3B2, true, string.Format("Assisted: {0:0.00}", pm.KinTeamPoints));

								from.SendMessage("Unassisted: {0:0.00}", pm.KinSoloPoints);
								from.SendMessage("Assisted: {0:0.00}", pm.KinTeamPoints);
								from.SendMessage("Power: {0:0.00}", pm.KinPowerPoints);
							}
							break;
						}
				}
			}

			if (e.Mobile != null && e.Mobile is PlayerMobile && e.Mobile.Alive)
			{
				if (e.Speech.ToLower().IndexOf("i wish to place a guard post") > -1)
				{
					//wooo sanity!
					PlayerMobile pm = e.Mobile as PlayerMobile;
					if (pm == null || pm.IOBRealAlignment == IOBAlignment.None)
						return;

					//Check player is in a faction region
					if (!(pm.Region is KinCityRegion))
					{
						pm.SendMessage("You are not within a faction city where you are a beneficiary.");
						return;
					}

					//Check they are a beneficiary 
					KinCityData cityData = KinCityManager.GetCityData(((KinCityRegion)pm.Region).KinFactionCity);
					KinCityData.BeneficiaryData bd = cityData.GetBeneficiary(pm);
					if (bd == null)
					{
						pm.SendMessage("You are not within a faction city where you are a beneficiary.");
						return;
					}

					if (cityData.GuardOption == KinCityData.GuardOptions.LordBritish || cityData.GuardOption == KinCityData.GuardOptions.None)
					{
						pm.SendMessage("You may not place a guard post with your city's current guard policy");
						return;
					}


					IPooledEnumerable eable = pm.GetItemsInRange(5);
					if (eable != null)
					{
						try
						{
							foreach (Item i in eable)
							{
								if (i != null && i is KinGuardPost && !i.Deleted)
								{
									pm.SendMessage("You may not place a guard post this close to another guard post");
									return;
								}
							}
						}
						finally
						{
							eable.Free();
							eable = null;
						}
					}

					eable = pm.GetItemsInRange(3);
					if (eable != null)
					{
						try
						{
							foreach (Item i in eable)
							{
								if (i != null && i is BaseDoor && !i.Deleted)
								{
									pm.SendMessage("You may not place a guard post this close to a door");
									return;
								}
							}
						}
						finally
						{
							eable.Free();
							eable = null;
						}
					}

					//Check they have enough spare guard slots
					if (bd.UnassignedGuardSlots < 1)
					{
						pm.SendMessage("You do not have enough free guard slots to create a guard post");
						return;
					}

					//Test the 8 squares around the target tile to make sure there is nothing blocking there.
					for (int x = -1; x < 2; x++)
						for (int y = -1; y < 2; y++)
						{
							if (x == 0 && y == 0) continue; //ignore the tile where they are standing
							Point3D location = pm.Location;
							location.X += x;
							location.Y += y;
							if (!pm.Map.CanSpawnMobile(location))
							{
								pm.SendMessage("You must have at least one free space in every direction around you.");
								return;
							}
						}


					//Place & register guard post
					KinGuardPost kgp = new KinGuardPost(0x429, pm, KinFactionGuardTypes.FactionHenchman, ((KinCityRegion)pm.Region).KinFactionCity);
					if (bd.RegisterGuardPost(kgp))
					{
						pm.SendMessage("Successfully created guard post.");
						kgp.MoveToWorld(pm.Location, pm.Map);
						kgp.Visible = true;
					}
					else
					{
						kgp.Delete();
					}
				}
				else if (e.Speech.ToLower().IndexOf("i wish to fund my guard post") > -1)
				{
					//wooo sanity!
					PlayerMobile pm = e.Mobile as PlayerMobile;
					if (pm == null || pm.IOBRealAlignment == IOBAlignment.None)
						return;

					pm.SendMessage("Select the guard post you would like to fund");
					pm.Target = new KinGuardPostFundTarget();
				}
				else if (e.Speech.ToLower().IndexOf("i renounce my kin status") > -1)
				{
					((PlayerMobile)e.Mobile).ResetIOBRankTime();
					e.Mobile.SendMessage("You have reduced your rank.");
				}
			}
		}
	}
}
