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

/* /Scripts/Gumps/Guilds/MoveGuildstoneGump.cs
 * ChangeLog
 *	3/10/05, mith
 *		Script Created. Called from Guildstone.OnDoubleClick()
 */

using System;
using Server;
using Server.Gumps;
using Server.Items;
using Server.Multis;
using Server.Network;

namespace Server.Gumps
{
	public class MoveGuildstoneGump : Gump
	{
		private Guildstone m_Stone;

		public MoveGuildstoneGump( Mobile from, Guildstone stone ) : base( 50, 50 )
		{
			m_Stone = stone;

			AddPage( 0 );

			AddBackground( 10, 10, 190, 140, 0x242C );

			AddHtml( 30, 30, 150, 75, String.Format( "<div align=CENTER>{0}</div>", "Are you sure you want to re-deed this guildstone?" ), false, false );

			AddButton( 40, 105, 0x81A, 0x81B, 0x1, GumpButtonType.Reply, 0 ); // Okay
			AddButton( 110, 105, 0x819, 0x818, 0x2, GumpButtonType.Reply, 0 ); // Cancel
		}

		public override void OnResponse( NetState state, RelayInfo info )
		{
			if ( m_Stone.Deleted )
				return;

			Mobile from = state.Mobile;

			if ( info.ButtonID == 1 )
				m_Stone.OnPrepareMove( from );
		}
	}
}