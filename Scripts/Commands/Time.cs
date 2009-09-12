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

/* Scripts/Commands/Time.cs
 * ChangeLog
 *	3/25/08 - Pix
 *		Changed to use new AdjustedDateTime utility class.
 *	12/06/05 - Pigpen
 *		Created.
 *		Time command works as follows. '[time' Displays Date then time.
 *	3/10/07 - Cyrun
 *		Edited message displayed to include "PST".
 */


using System;
using Server;
using Server.Mobiles;
using Server.Network;

namespace Server.Scripts.Commands
{
	public class TimeCommand
	{
		public static void Initialize()
		{
			Server.Commands.Register("Time", AccessLevel.Player, new CommandEventHandler(Time_OnCommand));
		}

		public static void Time_OnCommand(CommandEventArgs e)
		{
			Mobile m = e.Mobile;

			if (m is PlayerMobile)
			{
				//m.SendMessage("Server time is: {0} PST.", DateTime.Now);

				AdjustedDateTime ddt = new AdjustedDateTime(DateTime.Now);
				m.SendMessage("Server time is: {0} {1}.", ddt.Value.ToShortTimeString(), ddt.TZName);
			}
		}

	}
}
