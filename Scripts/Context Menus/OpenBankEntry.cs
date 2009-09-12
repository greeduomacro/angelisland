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
/* ChangeLog
	*	01/03/07, plasma
 *		Remove all duel challenge system code
 *	10/15/07, Pix
 *		Fixed expired AggressorInfos from preventing banking.
 *	8/26/07, Pix
 *		Prevent duelers from accessing their bankbox.
 * 01/05/05, Darva
 * Added code to prevent players from opening their bank box for 2 minutes after
 * 	Any successful steal.
 *
*/

using System;
using Server.Items;
using Server.Mobiles;
namespace Server.ContextMenus
{
	public class OpenBankEntry : ContextMenuEntry
	{
		private Mobile m_Banker;

		public OpenBankEntry(Mobile from, Mobile banker)
			: base(6105, 12)
		{
			m_Banker = banker;
		}

		public override void OnClick()
		{
			if (!Owner.From.CheckAlive())
				return;

			if (Owner.From.Criminal)
			{
				m_Banker.Say(500378); // Thou art a criminal and cannot access thy bank box.
			}
			else
			{
				PlayerMobile pm = Owner.From as PlayerMobile;

				if (pm != null && DateTime.Now - pm.LastStoleAt < TimeSpan.FromMinutes(2))
				{
					m_Banker.Say(500378); // Thou art a criminal and cannot access thy bank box.
				}
				else if (pm != null && AggressorInFight(pm))
				{
					m_Banker.Say("You seem to be busy to bank, come back when you're not fighting.");
				}
				else
				{
					BankBox box = this.Owner.From.BankBox;

					if (box != null)
						box.Open();
				}
			}
		}

		private bool AggressorInFight(PlayerMobile pm)
		{
			if (pm == null) return false;

			//check that we're not actively involved in a fight:
			for (int i = 0; i < pm.Aggressed.Count; ++i)
			{
				AggressorInfo info = (AggressorInfo)pm.Aggressed[i];
				if (!info.Expired)
				{
					if (info.Attacker == pm && info.Defender is PlayerMobile)
					{
						return true;
					}
				}
			}

			return false;
		}
	}
}