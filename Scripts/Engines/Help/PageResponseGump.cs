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

namespace Server.Engines.Help
{
	public class PageResponseGump : Gump
	{
		private Mobile m_From;
		private string m_Name, m_Text;

		public PageResponseGump( Mobile from, string name, string text ) : base( 0, 0 )
		{
			m_From = from;
			m_Name = name;
			m_Text = text;

			AddBackground( 50, 25, 540, 430, 2600 );

			AddPage( 0 );

			AddHtmlLocalized( 150, 40, 360, 40, 1062610, false, false ); // <CENTER><U>Ultima Online Help Response</U></CENTER>

			AddHtml( 80, 90, 480, 290, String.Format( "{0} tells {1}: {2}", name, from.Name, text ), true, true );

			AddHtmlLocalized( 80, 390, 480, 40, 1062611, false, false ); // Clicking the OKAY button will remove the reponse you have received.
			AddButton( 400, 417, 2074, 2075, 1, GumpButtonType.Reply, 0 ); // OKAY

			AddButton( 475, 417, 2073, 2072, 0, GumpButtonType.Reply, 0 ); // CANCEL
		}

		public override void OnResponse( NetState sender, RelayInfo info )
		{
			if ( info.ButtonID != 1 )
				m_From.SendGump( new MessageSentGump( m_From, m_Name, m_Text ) );
		}
	}
}
�