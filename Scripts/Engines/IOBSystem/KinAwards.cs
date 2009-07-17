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

/* Scripts\Engines\IOBSystem\KinAwards.cs
 * CHANGELOG:
 *	06/30/09, plasma
 *		Prevent kin pets dropping silver
 *		Prevent silver drop for blue interferers
 *	12/1/08, Adam
 *		Initial Version
 */

using System;
using Server;
using Server.Regions;
using Server.Items;
using System.Collections;
using Server.Mobiles;

namespace Server.Engines.IOBSystem
{
	public class KinAwards
	{
		public static int DeletePackGold(Mobile m)
		{
			Container pack = m.Backpack;
			int iAmountDeleted = 0;
			if (pack != null)
			{
				// how much gold is on the creature?
				Item[] golds = pack.FindItemsByType(typeof(Gold), true);
				foreach (Item g in golds)
				{
					iAmountDeleted += g.Amount;
					g.Delete();
				}

				return iAmountDeleted;
			}

			return 0;
		}

		public static void PackItem(Mobile m, Item item)
		{
			Container pack = m.Backpack;

			if (pack == null)
			{	// add a pack
				pack = new Backpack();
				pack.Movable = false;
				m.AddItem(pack);
			}

			if (!item.Stackable || !pack.TryDropItem(m, item, false)) // try stack
				pack.DropItem(item); // failed, drop it anyway
		}

		public static void AdjustLootForKinAward(Mobile m, int silver, int gold)
		{
			// sanity
			if (m == null) return;
			
			// first delete old gold
			DeletePackGold(m);

			// drop the gold
			PackItem(m, new Gold(gold));

			// drop the silver
			PackItem(m, new Silver(silver));
		}

		public static bool CalcAwardInSilver(Mobile m, out int silver, out int gold)
		{
			// new award amounts
			silver=gold=0;

			if (m == null || m is BaseCreature == false)
				return false;

			BaseCreature bc = m as BaseCreature;

			// creature must be IOB aligned
			if (IOBSystem.IsIOBAligned(bc) == false)
				return false;

			//creature must not be controlled 
			if (bc.ControlMaster != null)
				return false;

			// first find out how much gold this creature is dropping as that will be the gauge for the silver drop
			int MobGold = bc.GetGold();

			// meh, random I know
			gold = MobGold / 2;		// cut the gold in half
			silver = MobGold / 10;	// and give him 10% in silver

			// now calc the damagers.
			bool fail = false;
			ArrayList list = BaseCreature.GetLootingRights(bc.DamageEntries);
			IOBAlignment IOBBase = IOBAlignment.None;
			for (int i = 0; i < list.Count; ++i)
			{
				DamageStore ds = (DamageStore)list[i];
				
				if (!ds.m_HasRight)
					continue;

				if (ds.m_Mobile != null)
				{	
					// initialize the required IOBAlignment (one time)
					if (IOBBase == IOBAlignment.None)
						IOBBase = IOBSystem.GetIOBAlignment(ds.m_Mobile);
					
					// ds.m_Mobile may be a basecreature or a playermobile
					// 1. if the damager is not an ememy of the creature it killed, then no silver awards
					// 2. if all damagers are not of the same alignment, then no silver awards
					// 3. if the top damager was an interferer, no silver awards
					if (IOBSystem.IsEnemy(ds.m_Mobile, m) == false || IOBBase != IOBSystem.GetIOBAlignment(ds.m_Mobile) || IOBBase == IOBAlignment.Healer)
					{	// no silver awards
						fail = true;
						break;
					}
				}
			}

			// see if there were any non same-kin damagers.
			//	we won't reward silver if there was outside help
			if (fail == true)
				return false;	


			// okay, we have new amounts
			return true;
		}
	}
}
