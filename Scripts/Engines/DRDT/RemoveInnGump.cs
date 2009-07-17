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

/* /Scripts/Engines/DRDT/RemoveInnGump.cs
 *  05/03/05, Kit
 *	 Initial Creation
 */
using System;
using Server;
using Server.Gumps;
using Server.Items;
using System.Collections;
using Server.Network;

namespace Server.Gumps
{
	public class RemoveInnGump : Gump
	{
		RegionControl m_Control;

		public RemoveInnGump( RegionControl r )	: base( 25, 300 )
		{
			m_Control = r;


			Closable=true;
			Dragable=true;
			Resizable=false;

			AddPage(0);
			AddBackground(23, 32, 412, 256, 9270);
			AddAlphaRegion(19, 29, 418, 263);

			AddLabel(186, 45, 1152, "Remove Inn");


			//+25 between 'em.

			int itemsThisPage = 0;
			int nextPageNumber = 1;


			for( int i = 0; i < r.InnBounds.Count; i++ )
			{
				Rectangle2D rect;

				if( r.InnBounds[i] is Rectangle2D )
					rect = (Rectangle2D)r.InnBounds[i];
				else
					continue;



				if( itemsThisPage >= 8 || itemsThisPage == 0)
				{
					itemsThisPage = 0;

					if( nextPageNumber != 1)
					{
						AddButton(393, 45, 4007, 4009, 0, GumpButtonType.Page, nextPageNumber);
						//Forward button -> #0
					}

					AddPage( nextPageNumber++ );

					if( nextPageNumber != 2)
					{
						AddButton(35, 45, 4014, 4016, 1, GumpButtonType.Page, nextPageNumber-2 );
						//Back Button -> #1
					}
				}

					AddButton(70, 75 + 25 * itemsThisPage, 4017, 4019, 100 + i, GumpButtonType.Reply, 0);
					//Button is 100 + i

					//AddLabel(116, 77 + 25*i, 0, "(1234, 5678)");
					AddLabel(116, 77 + 25 * itemsThisPage, 1152, String.Format("({0}, {1})", rect.X, rect.Y ) );


					AddLabel(232, 78 + 25 * itemsThisPage, 1152, "<-->");

					//AddLabel(294, 77 + 25*i, 0, "(9876, 5432)");
					AddLabel(294, 77 + 25 * itemsThisPage, 1152, String.Format("({0}, {1})", rect.X + rect.Width, rect.Y + rect.Height ));

				itemsThisPage++;

			}

		}

		public override void OnResponse( NetState sender, RelayInfo info )
		{
			if( info.ButtonID >= 100 )
			{
				m_Control.InnBounds.RemoveAt( info.ButtonID - 100 );

				sender.Mobile.CloseGump( typeof( RemoveInnGump ) );

				sender.Mobile.SendGump( new RemoveInnGump( m_Control ) );
			}
		}
		
	}
}