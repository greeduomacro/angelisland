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

/* Scripts/Commands/Listen.cs
 * CHANGELOG:
 *	7/11/05 - Pix
 *		Initial Version
 */

using System;
using Server;
using Server.Targeting;

namespace Server.Scripts.Commands
{
	/// <summary>
	/// Summary description for Listen.
	/// </summary>
	public class Listen
	{
		public static void Initialize()
		{
			Server.Commands.Register( "Listen", AccessLevel.GameMaster, new CommandEventHandler( Listen_OnCommand ) );
		}

		public static void Listen_OnCommand( CommandEventArgs e )
		{
			e.Mobile.BeginTarget( -1, false, TargetFlags.None, new TargetCallback( Listen_OnTarget ) );
			e.Mobile.SendMessage( "Target a player." );
		}

		public static void Listen_OnTarget( Mobile from, object obj )
		{
			if ( obj is Mobile )
			{
				Server.Engines.PartySystem.Party.ListenToParty_OnTarget( from, obj );
				Server.Guilds.Guild.ListenToGuild_OnTarget( from, obj );
			}
		}

	}
}
