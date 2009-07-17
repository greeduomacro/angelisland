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

/* Scripts/Items/Skill Items/Fishing/Misc/MessageInABottle.cs
 * CHANGELOG:
 *	6/29/04 - Pix
 *		Temporary fix for Mibs/SOSs set for Trammel
 */

using System;
using Server.Network;
using Server.Items;

namespace Server.Items
{
	public class MessageInABottle : Item
	{
		public override int LabelNumber{ get{ return 1041080; } } // a message in a bottle

		private Map m_TargetMap;

		[CommandProperty( AccessLevel.GameMaster )]
		public Map TargetMap
		{
			get{ return m_TargetMap; }
			set{ m_TargetMap = value; }
		}

		[Constructable]
		public MessageInABottle() : this( Map.Trammel )
		{
		}

		[Constructable]
		public MessageInABottle( Map map ) : base( 0x099F )
		{
			Weight = 1.0;
			m_TargetMap = map;
		}

		public MessageInABottle( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 1 ); // version

			writer.Write( m_TargetMap );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch ( version )
			{
				case 1:
				{
					m_TargetMap = reader.ReadMap();
					break;
				}
				case 0:
				{
					m_TargetMap = Map.Trammel;
					break;
				}
			}

			//Angel Island flub fix - can remove at a later date
			if( m_TargetMap == Map.Trammel ) m_TargetMap = Map.Felucca;
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( IsChildOf( from.Backpack ) )
			{
				Consume();
				from.AddToBackpack( new SOS( m_TargetMap ) );
				from.LocalOverheadMessage( Network.MessageType.Regular, 0x3B2, 501891 ); // You extract the message from the bottle.
			}
			else
			{
				from.SendLocalizedMessage( 1042001 ); // That must be in your pack for you to use it.
			}
		}
	}
}