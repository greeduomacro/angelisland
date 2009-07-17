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
	public class Animations
	{
		public static void Initialize()
		{
			EventSink.AnimateRequest += new AnimateRequestEventHandler( EventSink_AnimateRequest );
		}

		private static void EventSink_AnimateRequest( AnimateRequestEventArgs e )
		{
			Mobile from = e.Mobile;

			int action;

			switch ( e.Action )
			{
				case "bow": action = 32; break;
				case "salute": action = 33; break;
				default: return;
			}

			if ( from.Alive && !from.Mounted && from.Body.IsHuman )
				from.Animate( action, 5, 1, true, false, 0 );
		}
	}
}�