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

using System;
using Server;

namespace Server.Misc
{
	public class RenameRequests
	{
		public static void Initialize()
		{
			EventSink.RenameRequest += new RenameRequestEventHandler( EventSink_RenameRequest );
		}

		private static void EventSink_RenameRequest( RenameRequestEventArgs e )
		{
			Mobile from = e.From;
			Mobile targ = e.Target;
			string name = e.Name;

			if ( from.CanSee( targ ) && from.InRange( targ, 12 ) && targ.CanBeRenamedBy( from ) )
			{
				name = name.Trim();

				if ( NameVerification.Validate( name, 1, 16, true, false, true, 0, NameVerification.Empty ) )
					targ.Name = name;
				else
					from.SendMessage( "That name is unacceptable." );
			}
		}
	}
}�