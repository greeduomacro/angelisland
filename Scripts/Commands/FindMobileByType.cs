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

/* Scripts/Commands/FindMobileByType.cs
 * Changelog : 
 *	6/18/08, Adam
 *      first time checkin
 */

using System;
using System.Collections;
using Server;
using Server.Mobiles;
using Server.Multis;

namespace Server.Scripts.Commands
{
	public class FindMobileByType
	{
		public static void Initialize()
		{
			Server.Commands.Register( "FindMobileByType", AccessLevel.Administrator, new CommandEventHandler( FindMobileByType_OnCommand ) );
		}

		[Usage( "FindMobileByType <type>" )]
		[Description( "Finds a mobile by type." )]
		public static void FindMobileByType_OnCommand( CommandEventArgs e )
		{
			try
			{
				if ( e.Length == 1 )
				{
					LogHelper Logger = new LogHelper("FindMobileByType.log", e.Mobile, false);
				
					string name = e.GetString( 0 );

                    foreach (Mobile mob  in World.Mobiles.Values)
					{
						if (mob != null && mob.GetType().ToString().ToLower().IndexOf(name.ToLower()) >= 0)
						{
							Logger.Log(LogType.Mobile, mob);
						}
					}
					Logger.Finish();
				}
				else
				{
					e.Mobile.SendMessage( "Format: FindMobileByType <type>" );
				}
			}
			catch (Exception ex) { EventSink.InvokeLogException(new LogExceptionEventArgs(ex)); }
		}
	}
}