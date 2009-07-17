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

/* Scripts/Engines/Help/PageEnableCommand.cs
 * CHANGELOG:
 *	3/5/05: Pix
 *		Initial Version.
 */

using System;

namespace Server.Scripts.Commands
{
	public class PageEnableCommand
	{
		public static bool Enabled;

		public static void Initialize()
		{
			PageEnableCommand.Enabled = true;
			Server.Commands.Register( "PageEnable", AccessLevel.Administrator, new CommandEventHandler( PageEnable_OnCommand ) );
		}

		public static void PageEnable_OnCommand( CommandEventArgs e )
		{
			if( e.Arguments.Length > 0 )
			{
				if( e.Arguments[0].ToLower() == "on" )
				{
					PageEnableCommand.Enabled = true;
				}
				else if( e.Arguments[0].ToLower() == "off" )
				{
					PageEnableCommand.Enabled = false;
				}
				else
				{
					e.Mobile.SendMessage( "[pageenable takes either 'on' or 'off' as a parameter." );
				}
			}

			e.Mobile.SendMessage( "PageEnable is {0}.", PageEnableCommand.Enabled ? "ON" : "OFF" );
		}
	}
}
