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

/* Items/Deeds/BarkeepContract.cs
 * ChangeLog:
 *	4/2/08, Adam
 *		Because we allow the purchase of 'work permits' to have more than 2 barkeeps;
 *		Change the message from:
 *		"That action would exceed the m/aximum number of barkeeps for this house."
 *	to
 *		"You must purchase a work permit if you wish to employ more barkeepers."
 *  12/12/04, Jade
 *      Unblessed the deeds.
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */

using System;
using Server;
using Server.Mobiles;
using Server.Network;
using Server.Multis;

namespace Server.Items
{
	public class BarkeepContract : Item
	{
		[Constructable]
		public BarkeepContract() : base( 0x14F0 )
		{
			Name = "a barkeep contract";
			Weight = 1.0;
            //Jade: make these unblessed.
			LootType = LootType.Regular;
		}

		public BarkeepContract( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			
			writer.Write( (int)0 ); //version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			
			int version = reader.ReadInt();
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( !IsChildOf( from.Backpack ) )
			{
				from.SendLocalizedMessage( 1042001 ); // That must be in your pack for you to use it.
			}
			else if ( from.AccessLevel >= AccessLevel.GameMaster )
			{
				from.SendLocalizedMessage( 503248 ); // Your godly powers allow you to place this vendor whereever you wish.

				Mobile v = new PlayerBarkeeper( from );

				v.Direction = from.Direction & Direction.Mask;
				v.MoveToWorld( from.Location, from.Map );

				this.Delete();
			}
			else
			{
				BaseHouse house = BaseHouse.FindHouseAt( from );

				if ( house == null || !house.IsOwner( from ) )
				{
					from.LocalOverheadMessage( MessageType.Regular, 0x3B2, false, "You are not the full owner of this house." );
				}
				else if ( !house.CanPlaceNewBarkeep() )
				{
					//from.SendLocalizedMessage( 1062490 ); // That action would exceed the m/aximum number of barkeeps for this house.
					from.SendMessage("You must purchase a work permit if you wish to employ more barkeepers.");
				}
				else
				{
					Mobile v = new PlayerBarkeeper( from );

					v.Direction = from.Direction & Direction.Mask;
					v.MoveToWorld( from.Location, from.Map );
						
					this.Delete();
				}
			}
		}
	}
}