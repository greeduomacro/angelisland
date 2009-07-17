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
	public class GuildCharterGump : Gump
	{
		private Mobile m_Mobile;
		private Guild m_Guild;

		private const string DefaultWebsite = "http://www.runuo.com/";

		public GuildCharterGump( Mobile from, Guild guild ) : base( 20, 30 )
		{
			m_Mobile = from;
			m_Guild = guild;

			Dragable = false;

			AddPage( 0 );
			AddBackground( 0, 0, 550, 400, 5054 );
			AddBackground( 10, 10, 530, 380, 3000 );

			AddButton( 20, 360, 4005, 4007, 1, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 55, 360, 300, 35, 1011120, false, false ); // Return to the main menu.

			string charter;

			if ( (charter = guild.Charter) == null || (charter = charter.Trim()).Length <= 0 )
				AddHtmlLocalized( 20, 20, 400, 35, 1013032, false, false ); // No charter has been defined.
			else
				AddHtml( 20, 20, 510, 75, charter, true, true );

			AddButton( 20, 200, 4005, 4007, 2, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 55, 200, 300, 20, 1011122, false, false ); // Visit the guild website : 

			string website;

			if ( (website = guild.Website) == null || (website = website.Trim()).Length <= 0 )
				website = DefaultWebsite;

			AddHtml( 55, 220, 300, 20, website, false, false );
		}

		public override void OnResponse( NetState state, RelayInfo info )
		{
			if ( GuildGump.BadMember( m_Mobile, m_Guild ) )
				return;

			switch ( info.ButtonID )
			{
				case 0: return; // Close
				case 1: break; // Return to main menu
				case 2:
				{
					string website;

					if ( (website = m_Guild.Website) == null || (website = website.Trim()).Length <= 0 )
						website = DefaultWebsite;

					m_Mobile.LaunchBrowser( website );
					break;
				}
			}

			GuildGump.EnsureClosed( m_Mobile );
			m_Mobile.SendGump( new GuildGump( m_Mobile, m_Guild ) );
		}
	}
}�