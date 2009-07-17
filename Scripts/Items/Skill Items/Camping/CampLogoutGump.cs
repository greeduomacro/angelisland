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

/* Items/Skill Items/Camping/CampLogoutGump.cs
 * CHANGELOG:
 *	8/14/06, weaver
 *		Added null check when handling the bedroll passed (for tent cases).
 *	10/28/04 - Pix
 *		Logout confirmation times out now after 20 seconds.
 *		Moving from the location where you doubleclicked the open bedroll now stops
 *		you from instaloggging.
 *	10/22/04 - Pix
 *		Changed camping to not use campfireregion.
 *	5/10/04, Pixie
 *		Initial working revision
 */

using System;
using Server;
using Server.Gumps;
using Server.Network;
using Server.Items;

namespace Server.Gumps
{
	public class CampLogoutGump : Gump
	{
		private Mobile m_From;
		private UnrolledBedroll m_Bedroll;
		private Point3D m_Location;

		public CampLogoutGump( Mobile from, UnrolledBedroll bedroll ) : base( 150, 200 )
		{
			m_From = from;
			m_Bedroll = bedroll;
			m_Location = from.Location;

			m_From.CloseGump( typeof( CampLogoutGump ) );

			AddPage( 0 );

			AddBackground( 0, 0, 400, 200, 5054 );

			AddHtml( 130, 10, 200, 25, "Logging Out Via Camping", false, false );

			AddHtmlLocalized( 40, 40, 320, 100, 1011016, 0,  true, true);

			AddHtmlLocalized( 70, 175, 140, 25, 1011011, false, false ); // CONTINUE
			AddButton( 40, 175, 4005, 4007, 2, GumpButtonType.Reply, 0 );

			AddHtmlLocalized( 160, 175, 140, 25, 1011012, false, false ); // CANCEL
			AddButton( 130, 175, 4005, 4007, 1, GumpButtonType.Reply, 0 );

			new CampLogoutGumpTimeoutTimer(from).Start();
		}

		public override void OnResponse( NetState state, RelayInfo info )
		{
			if ( info.ButtonID == 2 )
			{
				// wea: 14/Aug/2006 added null check to bedroll (for instances when we log out in tents)
				if( m_Bedroll != null )
				{
					m_Bedroll.Delete();
					state.Mobile.AddToBackpack( new Bedroll() );
				}

				if( m_Location != m_From.Location )
				{
					m_From.SendMessage("Moving from the location of the bedroll prohibits logging out via camping.");
				}
				else if( m_From.Criminal )
				{
					m_From.SendMessage("You are criminal, so you cannot logout via camping.");
				}
				else
				{
					//state.Dispose();
					state.Send( new LogoutAck() );

					//manually logout: set logout location and logout map and move to internal map
					m_From.LogoutLocation = m_From.Location;
					m_From.LogoutMap = m_From.Map;
					m_From.Map = Map.Internal;
				}
			}
		}

		private class CampLogoutGumpTimeoutTimer : Timer
		{
			private Mobile m_Player;

			public CampLogoutGumpTimeoutTimer( Mobile m ) : base( TimeSpan.FromSeconds( 20.0 ) )
			{
				m_Player = m;
			}

			protected override void OnTick()
			{
				m_Player.CloseGump( typeof(CampLogoutGump) );
				Stop();
			}
		}

	}
}