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

/* Scripts/Commands/CheckLOS.cs
 * ChangeLog
 *	4/28/08, Adam
 *		First time checkin
 */


using System;
using Server;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;
using Server.Misc;						// TestCenter

namespace Server.Scripts.Commands
{
	public class CheckLOSCommand
	{
		public static void Initialize()
		{
			Server.Commands.Register("CheckLOS", AccessLevel.Player, new CommandEventHandler(CheckLOS_OnCommand));
		}

		public static void CheckLOS_OnCommand(CommandEventArgs e)
		{
			if (e.Mobile.AccessLevel == AccessLevel.Player && TestCenter.Enabled == false)
			{	// Players can only test this on Test Center
				e.Mobile.SendMessage("Not available here.");
				return;
			}

			if (e.Mobile.AccessLevel > AccessLevel.Player)
			{	// you will not get good results if you test this with AccessLevel > Player
				e.Mobile.SendMessage("You should test this with AccessLevel.Player.");
				return;
			}

			try
			{
				e.Mobile.Target = new LOSTarget();
				e.Mobile.SendMessage("Check LOS to which object?");
			}
			catch (Exception ex)
			{
				LogHelper.LogException(ex);
			}

		}

		private class LOSTarget : Target
		{
			public LOSTarget()
				: base(15, false, TargetFlags.None)
			{
			}

			protected override void OnTarget(Mobile from, object targ)
			{
				from.SendMessage("You {0} see that.", from.Map.LineOfSight(from, targ) ? "can" : "cannot");
				return;
			}
		}

	}
}
