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

/* Scripts/Gumps/Guilds/GuildMobileListGump.cs
 * CHANGELOG:
 *	6/16/06, Pix
 *		Added guildmember's title to listing.
 */

using System;
using System.Collections;
using Server;
using Server.Guilds;
using Server.Scripts.Commands;

namespace Server.Gumps
{
	public abstract class GuildMobileListGump : Gump
	{
		protected Mobile m_Mobile;
		protected Guild m_Guild;
		protected ArrayList m_List;

		public GuildMobileListGump( Mobile from, Guild guild, bool radio, ArrayList list ) : base( 20, 30 )
		{
			m_Mobile = from;
			m_Guild = guild;

			Dragable = false;

			AddPage( 0 );
			AddBackground( 0, 0, 550, 440, 5054 );
			AddBackground( 10, 10, 530, 420, 3000 );

			Design();

			m_List = new ArrayList( list );

			for ( int i = 0; i < m_List.Count; ++i )
			{
				if ( (i % 11) == 0 )
				{
					if ( i != 0 )
					{
						AddButton( 300, 370, 4005, 4007, 0, GumpButtonType.Page, (i / 11) + 1 );
						AddHtmlLocalized( 335, 370, 300, 35, 1011066, false, false ); // Next page
					}

					AddPage( (i / 11) + 1 );

					if ( i != 0 )
					{
						AddButton( 20, 370, 4014, 4016, 0, GumpButtonType.Page, (i / 11) );
						AddHtmlLocalized( 55, 370, 300, 35, 1011067, false, false ); // Previous page
					}
				}

				if ( radio )
					AddRadio( 20, 35 + ((i % 11) * 30), 208, 209, false, i );

				Mobile m = (Mobile)m_List[i];

				string name;

				if ( (name = m.Name) != null && (name = name.Trim()).Length <= 0 )
					name = "(empty)";

				string title = "(no title)";
				try
				{
					if( m.GuildTitle != null && m.GuildTitle.Trim().Length > 0 )
					{
						title = m.GuildTitle.Trim();
					}
				}
				catch(Exception ex)
				{
					LogHelper.LogException(ex);
					Console.WriteLine("Send the following exception to Pixie:\n{0}\n{1}\n{2}\ntitle={3}",
						"Exception in GuildMobileListGump",
						ex.Message,
						ex.StackTrace.ToString(),
						m.GuildTitle);

					title = "(error)";
				}

				AddLabel( (radio ? 55 : 20), 35 + ((i % 11) * 30), 0, name + ", " + title );
			}
		}

		protected virtual void Design()
		{
		}
	}
}