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

/* Scripts/Commands/Resynch.cs
 * ChangeLog
 *	9/25/04 - Pix.
 *		Added 2 minute time period between uses of the command.
 *	9/16/04 - Pixie
 *		Resurrected and re-structured this command.
 *		Attempting to see if sending a MobileUpdate packet works.
 */


using System;
using Server;
using Server.Mobiles;
using Server.Network;

namespace Server.Scripts.Commands
{
	public class ResynchCommand
	{
		public static void Initialize()
		{
			Server.Commands.Register("Resynch", AccessLevel.Player, new CommandEventHandler(Resynch_OnCommand));
		}

		public static void Resynch_OnCommand(CommandEventArgs e)
		{
			Mobile m = e.Mobile;

			if (m is PlayerMobile)
			{
				if (((PlayerMobile)m).m_LastResynchTime < (DateTime.Now - TimeSpan.FromMinutes(2.0))
					|| (m.AccessLevel > AccessLevel.Player))
				{
					m.SendMessage("Resynchronizing server and client.");
					m.Send(new MobileUpdate(m));
					((PlayerMobile)m).m_LastResynchTime = DateTime.Now;
				}
				else
				{
					m.SendMessage("You must wait to use that command.");
				}
			}

		}

	}
}
