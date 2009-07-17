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
/* /Scripts/Gumps/ForensicKillerGump.cs
 *
 *	ChangeLog:
 *	5/17/04 created by smerX
 */



using System;
using System.Collections;
using Server.Network;
using Server.Items;
using Server.Mobiles;

namespace Server.Gumps
{
	public class ForensicKillerGump : Gump
	{
		private Mobile m_Owner;
		private Mobile m_Killer;
		private BountyLedger m_Book;

		public ForensicKillerGump( Mobile owner, Mobile killer, BountyLedger book ) : base( 30, 30 )
		{
			owner.CloseGump( typeof( ForensicKillerGump ) );

			m_Owner = owner;
			m_Killer = killer;
			m_Book = book;

			BuildKillerGump();
		}

		public void BuildKillerGump()
		{
			int borderwidth = 8;
			int bothY = 90;
			int firstbuttonX = 40;
			int secondbuttonX = 230;

			AddPage( 0 );

			AddBackground( 0, 0, 350, 140, PropsConfig.BackGumpID );
			AddImageTiled( borderwidth - 1, borderwidth, 350 - (borderwidth * 2), 140 - (borderwidth * 2), PropsConfig.HeaderGumpID );

			AddLabel( 20, 20, 0x47e, "They were killed by " + m_Killer.Name + "!");
			AddLabel( 20, 40, 0x47e, "Would you like to add them to your bounty ledger?" );

			AddLabel( firstbuttonX + 35, bothY, 0x47e, "Cancel" );
			AddButton( firstbuttonX, bothY, 0xFA5, 0xFA7, 1, GumpButtonType.Reply, 0 );

			AddLabel( secondbuttonX + 35, bothY, 0x47e, "Add" );
			AddButton( secondbuttonX, bothY, 0xFA5, 0xFA7, 2, GumpButtonType.Reply, 0 );
		}

		public override void OnResponse( NetState state, RelayInfo info )
		{
			Mobile from = state.Mobile;

			switch ( info.ButtonID )
			{
				case 0: // Closed
				{
					from.SendMessage( "You decide not to persue that person." );
					break;
				}
				case 1: // Cancel
				{
					from.SendMessage( "You decide not to persue that person." );
					break;
				}
				default: // Add
				{

					if ( m_Killer is PlayerMobile )
					{
						Mobile m = m_Killer;
	
						if ( m.Deleted )
						{
							from.SendMessage( "That player has deleted their character." );
							from.SendGump( new ForensicKillerGump( from, m_Killer, m_Book ) );
						}
						else
						{
							LedgerEntry e = new LedgerEntry( m_Killer, 0, false );
							m_Book.AddEntry( from, e, m_Book.Entries.Count + 1 );
						}
					}
					else
					{
						from.SendMessage( "You have specified an invalid target." );
						return;
					}
				}
				break;
			}
		}
	}
}