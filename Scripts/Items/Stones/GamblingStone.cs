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
using Server.Items;

namespace Server.Items
{
	public class GamblingStone : Item
	{
		private int m_GamblePot = 2500;

		[CommandProperty( AccessLevel.GameMaster )]
		public int GamblePot
		{
			get
			{
				return m_GamblePot;
			}
			set
			{
				m_GamblePot = value;
				InvalidateProperties();
			}
		}

		[Constructable]
		public GamblingStone() : base( 0xED4 )
		{
			Movable = false;
			Hue = 0x56;
			Name = "a gambling stone";
		}

		public override void GetProperties( ObjectPropertyList list )
		{
			base.GetProperties( list );

			list.Add( "Jackpot: {0}gp", m_GamblePot );
		}

		public override void OnSingleClick( Mobile from )
		{
			base.OnSingleClick( from );
			base.LabelTo( from, "Jackpot: {0}gp", m_GamblePot );
		}

		public override void OnDoubleClick( Mobile from )
		{
			Container pack = from.Backpack;

			if ( pack != null && pack.ConsumeTotal( typeof( Gold ), 250 ) )
			{
				m_GamblePot += 150;
				InvalidateProperties();

				int roll = Utility.Random( 1200 );

				if ( roll == 0 ) // Jackpot
				{
					from.SendMessage( 0x35, "You win the {0}gp jackpot!", m_GamblePot );
					from.AddToBackpack( new BankCheck( m_GamblePot ) );

					m_GamblePot = 2500;
				}
				else if ( roll <= 20 ) // Chance for a regbag
				{
					from.SendMessage( 0x35, "You win a bag of reagents!" );
					from.AddToBackpack( new BagOfReagents( 50 ) );
				}
				else if ( roll <= 40 ) // Chance for gold
				{
					from.SendMessage( 0x35, "You win 1500gp!" );
					from.AddToBackpack( new BankCheck( 1500 ) );
				}
				else if ( roll <= 100 ) // Another chance for gold
				{
					from.SendMessage( 0x35, "You win 1000gp!" );
					from.AddToBackpack( new BankCheck( 1000 ) );
				}
				else // Loser!
				{
					from.SendMessage( 0x22, "You lose!" );
				}
			}
			else
			{
				from.SendMessage( 0x22, "You need at least 250gp in your backpack to use this." );
			}
		}
    
		public GamblingStone( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version

			writer.Write( (int) m_GamblePot );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch ( version )
			{
				case 0:
				{
					m_GamblePot = reader.ReadInt();

					break;
				}
			}
		}
	}
}�