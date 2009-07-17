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
using Server.Gumps;
using Server.Network;

namespace Server.Engines.VeteranRewards
{
	public class RewardNoticeGump : Gump
	{
		private Mobile m_From;

		public RewardNoticeGump( Mobile from ) : base( 0, 0 )
		{
			m_From = from;

			from.CloseGump( typeof( RewardNoticeGump ) );

			AddPage( 0 );

			AddBackground( 10, 10, 500, 135, 2600 );

			/* You have reward items available.
			 * Click 'ok' below to get the selection menu or 'cancel' to be prompted upon your next login.
			 */
			AddHtmlLocalized( 52, 35, 420, 55, 1006046, true, true );

			AddButton( 60, 95, 4005, 4007, 1, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 95, 96, 150, 35, 1006044, false, false ); // Ok

			AddButton( 285, 95, 4017, 4019, 0, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 320, 96, 150, 35, 1006045, false, false ); // Cancel
		}

		public override void OnResponse( NetState sender, RelayInfo info )
		{
			if ( info.ButtonID == 1 )
				m_From.SendGump( new RewardChoiceGump( m_From ) );
		}
	}
} 
