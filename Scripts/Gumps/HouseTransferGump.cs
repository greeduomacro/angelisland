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
using Server.Items;
using Server.Multis;
using Server.Multis.Deeds;
using Server.Network;

namespace Server.Gumps
{
	public class HouseTransferGump : Gump
	{
		private Mobile m_From, m_To;
		private BaseHouse m_House;

		public HouseTransferGump( Mobile from, Mobile to, BaseHouse house ) : base( 110, 100 )
		{
			m_From = from;
			m_To = to;
			m_House = house;

			Closable = false;

			AddPage( 0 );

			AddBackground( 0, 0, 420, 280, 5054 );

			AddImageTiled( 10, 10, 400, 20, 2624 );
			AddAlphaRegion( 10, 10, 400, 20 );

			AddHtmlLocalized( 10, 10, 400, 20, 1060635, 30720, false, false ); // <CENTER>WARNING</CENTER>

			AddImageTiled( 10, 40, 400, 200, 2624 );
			AddAlphaRegion( 10, 40, 400, 200 );

			/* Another player is attempting to initiate a house trade with you.
			 * In order for you to see this window, both you and the other person are standing within two paces of the house to be traded.
			 * If you click OKAY below, a house trade scroll will appear in your trade window and you can complete the transaction.
			 * This scroll is a distinctive blue color and will show the name of the house, the name of the owner of that house, and the sextant coordinates of the center of the house when you hover your mouse over it.
			 * In order for the transaction to be successful, you both must accept the trade and you both must remain within two paces of the house sign.
			 * <BR><BR>Accepting this house in trade will <a href = "?ForceTopic97">condemn</a> any and all of your other houses that you may have.
			 * All of your houses on <U>all shards</U> will be affected.
			 * <BR><BR>In addition, you will not be able to place another house or have one transferred to you for one (1) real-life week.<BR><BR>
			 * Once you accept these terms, these effects cannot be reversed.
			 * Re-deeding or transferring your new house will <U>not</U> uncondemn your other house(s) nor will the one week timer be removed.<BR><BR>
			 * If you are absolutely certain you wish to proceed, click the button next to OKAY below.
			 * If you do not wish to trade for this house, click CANCEL.
			 */
			AddHtmlLocalized( 10, 40, 400, 200, 1062086, 32512, false, true );

			AddImageTiled( 10, 250, 400, 20, 2624 );
			AddAlphaRegion( 10, 250, 400, 20 );

			AddButton( 10, 250, 4005, 4007, 1, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 40, 250, 170, 20, 1011036, 32767, false, false ); // OKAY

			AddButton( 210, 250, 4005, 4007, 0, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 240, 250, 170, 20, 1011012, 32767, false, false ); // CANCEL
		}

		public override void OnResponse( NetState state, RelayInfo info )
		{
			if ( info.ButtonID == 1 && !m_House.Deleted )
				m_House.EndConfirmTransfer( m_From, m_To );
		}
	}
} 
