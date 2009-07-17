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

/* Scripts/Mobiles/Healers/PricedHealer.cs 
 * Changelog
 *	06/28/06, Adam
 *		Logic cleanup
 */

using System;
using Server;
using Server.Gumps;

namespace Server.Mobiles
{
	public class PricedHealer : BaseHealer
	{
		private int m_Price;

		[CommandProperty( AccessLevel.GameMaster )]
		public int Price
		{
			get{ return m_Price; }
			set{ m_Price = value; }
		}

		[Constructable]
		public PricedHealer() : this( 5000 )
		{
		}

		[Constructable]
		public PricedHealer( int price )
		{
			m_Price = price;
			if ( !Core.AOS )
				NameHue = 0x35;
		}

		
		public override void InitSBInfo()
		{
		}

		public override void OfferResurrection( Mobile m )
		{
			Direction = GetDirectionTo( m );

			m.PlaySound( 0x214 );
			m.FixedEffect( 0x376A, 10, 16 );

			m.CloseGump( typeof( ResurrectGump ) );
			m.SendGump( new ResurrectGump( m, this, m_Price ) );
		}

		public override bool CheckResurrect( Mobile m )
		{
			return true;
		}

		public PricedHealer( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version

			writer.Write( (int) m_Price );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch ( version )
			{
				case 0:
				{
					m_Price = reader.ReadInt();
					break;
				}
			}
		}
	}
}