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

/* Scripts\Commands\TakeOwnership.cs
 * ChangeLog
 *  06/12/07, Adam
 *      First time checkin
 *      Takes ownership oif a house.
 *      Could be extended to take ownership of a boat as well.
 */

using System;
using System.Reflection;
using Server.Items;
using Server.Targeting;
using Server.Multis;        // HouseSign

namespace Server.Scripts.Commands
{
	public class TakeOwnership
	{
		public static void Initialize()
		{
			Server.Commands.Register( "TakeOwnership", AccessLevel.GameMaster, new CommandEventHandler( TakeOwnership_OnCommand ) );
		}

		[Usage( "TakeOwnership" )]
		[Description( "take ownership of a house." )]
		private static void TakeOwnership_OnCommand( CommandEventArgs e )
		{
            e.Mobile.Target = new TakeOwnershipTarget();
			e.Mobile.SendMessage( "What do you wish to take ownership of?" );
		}

		private class TakeOwnershipTarget : Target
		{
			public TakeOwnershipTarget( ) : base( 15, false, TargetFlags.None )
			{
			}

            protected override void OnTarget(Mobile from, object target)
			{
                if (target is HouseSign && (target as HouseSign).Owner != null)
                {
                    try
                    {
                        BaseHouse bh = (target as HouseSign).Owner as BaseHouse;
                        bh.AdminTransfer(from);
                    }
                    catch (Exception tse)
                    {
                        LogHelper.LogException(tse);
                    }
                }
                else
                {
                    from.SendMessage("That is not a house sign.");
                }
			}
		}

	}
}
