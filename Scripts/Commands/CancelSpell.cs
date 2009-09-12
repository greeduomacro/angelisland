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

/* /Scripts/Commands/CancelSpell.cs
*	ChangeLog:
*	4/17/04 Creation by smerX
*		Created to provide easier access to certain game features
*/
using System;
using System.Collections;
using System.Reflection;
using Server;
using Server.Misc;
using Server.Gumps;
using Server.Multis;
using Server.Network;
using Server.Spells;

namespace Server.Scripts.Commands
{
	public class CancelSpell
	{

		public static void Initialize()
		{
			Server.Commands.Register("CancelSpell", AccessLevel.Player, new CommandEventHandler(CancelSpell_OnCommand));
		}

		[Usage("CancelSpell")]
		[Description("Cancels the spell currently being cast.")]
		private static void CancelSpell_OnCommand(CommandEventArgs e)
		{
			Mobile m = e.Mobile;
			ISpell i = m.Spell;

			if (i != null && i.IsCasting)
			{
				Spell s = (Spell)i;
				s.Disturb(DisturbType.EquipRequest, true, false);
				m.SendMessage("You snap yourself out of concentration.");
				m.FixedEffect(0x3735, 6, 30);
				return;
			}

			else
			{
				m.SendMessage("You must be casting a spell to use this feature.");
				return;
			}

		}
	}
}