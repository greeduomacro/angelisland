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
using System.Collections;
using Server;
using Server.Guilds;
using Server.Network;

namespace Server.Gumps
{
	public class GuildWarAdminGump : Gump
	{
		private Mobile m_Mobile;
		private Guild m_Guild;

		public GuildWarAdminGump( Mobile from, Guild guild ) : base( 20, 30 )
		{
			m_Mobile = from;
			m_Guild = guild;

			Dragable = false;

			AddPage( 0 );
			AddBackground( 0, 0, 550, 440, 5054 );
			AddBackground( 10, 10, 530, 420, 3000 );

			AddHtmlLocalized( 20, 10, 510, 35, 1011105, false, false ); // <center>WAR FUNCTIONS</center>

			AddButton( 20, 40, 4005, 4007, 1, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 55, 40, 400, 30, 1011099, false, false ); // Declare war through guild name search.

			int count = 0;

			if ( guild.Enemies.Count > 0 )
			{
				AddButton( 20, 160 + (count * 30), 4005, 4007, 2, GumpButtonType.Reply, 0 );
				AddHtmlLocalized( 55, 160 + (count++ * 30), 400, 30, 1011103, false, false ); // Declare peace.
			}
			else
			{
				AddHtmlLocalized( 20, 160 + (count++ * 30), 400, 30, 1013033, false, false ); // No current wars
			}

			if ( guild.WarInvitations.Count > 0 )
			{
				AddButton( 20, 160 + (count * 30), 4005, 4007, 3, GumpButtonType.Reply, 0 );
				AddHtmlLocalized( 55, 160 + (count++ * 30), 400, 30, 1011100, false, false ); // Accept war invitations.

				AddButton( 20, 160 + (count * 30), 4005, 4007, 4, GumpButtonType.Reply, 0 );
				AddHtmlLocalized( 55, 160 + (count++ * 30), 400, 30, 1011101, false, false ); // Reject war invitations.
			}
			else
			{
				AddHtmlLocalized( 20, 160 + (count++ * 30), 400, 30, 1018012, false, false ); // No current invitations received for war.
			}

			if ( guild.WarDeclarations.Count > 0 )
			{
				AddButton( 20, 160 + (count * 30), 4005, 4007, 5, GumpButtonType.Reply, 0 );
				AddHtmlLocalized( 55, 160 + (count++ * 30), 400, 30, 1011102, false, false ); // Rescind your war declarations.
			}
			else
			{
				AddHtmlLocalized( 20, 160 + (count++ * 30), 400, 30, 1013055, false, false ); // No current war declarations
			}

			AddButton( 20, 400, 4005, 4007, 6, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 55, 400, 400, 35, 1011104, false, false ); // Return to the previous menu.
		}

		public override void OnResponse( NetState state, RelayInfo info )
		{
			if ( GuildGump.BadLeader( m_Mobile, m_Guild ) )
				return;

			switch ( info.ButtonID )
			{
				case 1: // Declare war
				{
					m_Mobile.SendLocalizedMessage( 1018001 ); // Declare war through search - Enter Guild Name:  
					m_Mobile.Prompt = new GuildDeclareWarPrompt( m_Mobile, m_Guild );

					break;
				}
				case 2: // Declare peace
				{
					GuildGump.EnsureClosed( m_Mobile );
					m_Mobile.SendGump( new GuildDeclarePeaceGump( m_Mobile, m_Guild ) );

					break;
				}
				case 3: // Accept war
				{
					GuildGump.EnsureClosed( m_Mobile );
					m_Mobile.SendGump( new GuildAcceptWarGump( m_Mobile, m_Guild ) );

					break;
				}
				case 4: // Reject war
				{
					GuildGump.EnsureClosed( m_Mobile );
					m_Mobile.SendGump( new GuildRejectWarGump( m_Mobile, m_Guild ) );

					break;
				}
				case 5: // Rescind declarations
				{
					GuildGump.EnsureClosed( m_Mobile );
					m_Mobile.SendGump( new GuildRescindDeclarationGump( m_Mobile, m_Guild ) );

					break;
				}
				case 6: // Return
				{
					GuildGump.EnsureClosed( m_Mobile );
					m_Mobile.SendGump( new GuildmasterGump( m_Mobile, m_Guild ) );

					break;
				}
			}
		}
	}
} 
