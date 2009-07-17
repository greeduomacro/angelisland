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

/* Scripts/Gumps/HouseDemolishGump.cs
 * ChangeLog
 * 7/20/08, Pix
 *		De-coupled township stones from houses.
 *  7/6/08, Adam
 *      Fix text typo
 *  8/07/06, Rhiannon
 *		Changed warning gump to reflect how demolising a house works on AI.
 *	2/20/06, Adam
 *		Add check m_House.FindPlayer() to find players in a house.
 *		We no longer allow a house to be deeded when players are inside (on roof) 
 *		This was used as an exploit.
 */

using System;
using Server;
using Server.Items;
using Server.Multis;
using Server.Multis.Deeds;
using Server.Network;
using Server.Mobiles;

namespace Server.Gumps
{
	public class HouseDemolishGump : Gump
	{
		private Mobile m_Mobile;
		private BaseHouse m_House;

		public HouseDemolishGump( Mobile mobile, BaseHouse house ) : base( 110, 100 )
		{
			m_Mobile = mobile;
			m_House = house;

			mobile.CloseGump( typeof( HouseDemolishGump ) );

			Closable = false;

			AddPage( 0 );

			AddBackground( 0, 0, 420, 280, 5054 );

			AddImageTiled( 10, 10, 400, 20, 2624 );
			AddAlphaRegion( 10, 10, 400, 20 );

			AddHtmlLocalized( 10, 10, 400, 20, 1060635, 30720, false, false ); // <CENTER>WARNING</CENTER>

			AddImageTiled( 10, 40, 400, 200, 2624 );
			AddAlphaRegion( 10, 40, 400, 200 );

			// The following warning is more appropriate for AI than the localized warning.
			String WarningString = "You are about to demolish your house. A deed to the house will be placed in your bank box. All items in the house will remain behind and can be freely picked up by anyone. Once the house is demolished, anyone can attempt to place a new house on the vacant land. Are you sure you wish to continue?";
			AddHtml( 10, 40, 400, 200, WarningString, false, true );

//			AddHtmlLocalized( 10, 40, 400, 200, 1061795, 32512, false, true ); /* You are about to demolish your house.
//																				* You will be refunded the house's value directly to your bank box.
//																				* All items in the house will remain behind and can be freely picked up by anyone.
//																				* Once the house is demolished, anyone can attempt to place a new house on the vacant land.
//																				* This action will not un-condemn any other houses on your account, nor will it end your 7-day waiting period (if it applies to you).
//																				* Are you sure you wish to continue?
//																				*/

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
			{
				if ( m_House.IsOwner( m_Mobile ) )
				{
					if ( m_House.FindGuildstone() != null )
					{
						m_Mobile.SendLocalizedMessage( 501389 ); // You cannot redeed a house with a guildstone inside.
						return;
					}
//Pix: 7/13/2008 - Removing the requirement of a townshipstone to be in a house.
//					else if (m_House.FindTownshipStone() != null)
//					{
//						m_Mobile.SendMessage("You can't demolish a house which holds a Township stone.");
//						return;
//					}
					//It was decided that we should just auto-dismiss the township NPC if the house is demolished
					//else if (m_House.FindTownshipNPC() != null)
					//{
					//	m_Mobile.SendMessage("You need to dismiss your Township NPC before moving.");
					//	return;
					//}
					else if (m_House.FindPlayerVendor() != null)
					{
						m_Mobile.SendLocalizedMessage(503236); // You need to collect your vendor's belongings before moving.
						return;
					}
					else if (m_House.FindPlayer() != null)
					{
						m_Mobile.SendMessage("It is not safe to demolish this house with someone still inside."); // You need to collect your vendor's belongings before moving.
						//Tell staff that an exploit is in progress
						//Server.Scripts.Commands.CommandHandlers.BroadcastMessage( AccessLevel.Counselor, 
						//0x482, 
						//String.Format( "Exploit in progress at {0}. Stay hidden, Jail involved players, get acct name, ban.", m_House.Location.ToString() ) );
						return;
					}

					Item toGive = null;
                    Item Refund = null;     // for various home upgrades

					if ( m_House.IsAosRules )
					{
						if ( m_House.Price > 0 )
							toGive = new BankCheck( m_House.Price );
						else
							toGive = m_House.GetDeed();
					}
					else
					{
						toGive = m_House.GetDeed();

						if ( toGive == null && m_House.Price > 0 )
							toGive = new BankCheck( m_House.Price );

                        if (m_House.UpgradeCosts > 0)
                            Refund = new BankCheck((int)m_House.UpgradeCosts);
					}

					if ( toGive != null )
					{
						BankBox box = m_Mobile.BankBox;

                        // Adam: TryDropItem() fails if trhe bank is full, and this isn't the time to be 
                        //  failing .. just overload their bank.
						if ( box != null /*&& box.TryDropItem( m_Mobile, toGive, false )*/ )
						{
                            box.AddItem(toGive);
							if ( toGive is BankCheck )
								m_Mobile.SendLocalizedMessage( 1060397, ((BankCheck)toGive).Worth.ToString() ); // ~1_AMOUNT~ gold has been deposited into your bank box.

                            if (Refund != null)
                            {
                                box.AddItem(Refund);
                                if (Refund is BankCheck)
                                    m_Mobile.SendLocalizedMessage(1060397, ((BankCheck)Refund).Worth.ToString()); // ~1_AMOUNT~ gold has been deposited into your bank box.
                            }

							m_House.RemoveKeys( m_Mobile );
							m_House.Delete();
						}
						else
						{
							toGive.Delete();
							m_Mobile.SendLocalizedMessage( 500390 ); // Your bank box is full.
						}
					}
					else
					{
						m_Mobile.SendMessage( "Unable to refund house." );
					}
				}
				else
				{
					m_Mobile.SendLocalizedMessage( 501320 ); // Only the house owner may do this.
				}
			}
		}
	}
}
