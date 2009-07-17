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

namespace Server.Multis
{
	public class ConfirmDryDockGump : Gump
	{
		private Mobile m_From;
		private BaseBoat m_Boat;

		public ConfirmDryDockGump( Mobile from, BaseBoat boat ) : base( 150, 200 )
		{
			m_From = from;
			m_Boat = boat;

			m_From.CloseGump( typeof( ConfirmDryDockGump ) );

			AddPage( 0 );

			AddBackground( 0, 0, 220, 170, 5054 );
			AddBackground( 10, 10, 200, 150, 3000 );

			AddHtmlLocalized( 20, 20, 180, 80, 1018319, true, false ); // Do you wish to dry dock this boat?

			AddHtmlLocalized( 55, 100, 140, 25, 1011011, false, false ); // CONTINUE
			AddButton( 20, 100, 4005, 4007, 2, GumpButtonType.Reply, 0 );

			AddHtmlLocalized( 55, 125, 140, 25, 1011012, false, false ); // CANCEL
			AddButton( 20, 125, 4005, 4007, 1, GumpButtonType.Reply, 0 );
		}

		public override void OnResponse( NetState state, RelayInfo info )
		{
			if ( info.ButtonID == 2 )
				m_Boat.EndDryDock( m_From );
		}
	}
} 
