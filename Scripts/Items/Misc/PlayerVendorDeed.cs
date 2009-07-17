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

/* Items/Misc/PlayerVendorDeed.cs
 * CHANGELOG:
 *  1/14/08, Adam
 *      Add support for Commission based PricingModel
 *	08/03/06, weaver
 *		Allowed placement within siege tents.
 *	05/15/06, weaver
 *		Added check for null region.
 *	05/03/06, weaver
 *		Allowed placement within tents.
 * 	04/15/05, Kitaras
 *		Updated calls to PlayerVendor() to pass a house as well as mobile.
 *	12/12/04, Jade
 *		Unblessed the deeds.
 *	6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */

using System;
using Server;
using Server.Mobiles;
using Server.Multis;
using Server.Regions;

namespace Server.Items
{
	public class ContractOfEmployment : Item
	{
		public override int LabelNumber{ get{ return 1041243; } } // a contract of employment

		[Constructable]
		public ContractOfEmployment() : base( 0x14F0 )
		{
			Weight = 1.0;
            //Jade: make these unblessed
			LootType = LootType.Regular;
		}

		public ContractOfEmployment( Serial serial ) : base( serial )
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
				from.SendLocalizedMessage( 503248 );//Your godly powers allow you to place this vendor whereever you wish.
				
				Mobile v = new PlayerVendor( from, BaseHouse.FindHouseAt( from ) );
				v.Direction = from.Direction & Direction.Mask;
				v.MoveToWorld( from.Location, from.Map );

				v.SayTo( from, 503246 ); // Ah! it feels good to be working again.
		
				this.Delete();
			}
			else
			{	
				//find the house there at
				BaseHouse house = BaseHouse.FindHouseAt( from );

				// wea: allow placement within tents
				if( house == null )
				{
					if( from.Region != null )
					{
						// is there a tent belonging to the person's account here though?
						if( from.Region is HouseRegion )
						{
							HouseRegion hr = (HouseRegion) from.Region;

							if( (hr.House is Tent || hr.House is SiegeTent) && hr.House.Owner.Account == from.Account )
								house = ((HouseRegion) from.Region).House;
								
						}
					}
				}

				if ( house == null )
				{
					from.SendLocalizedMessage( 503240 );//Vendors can only be placed in houses.	
				}
				else if ( !house.IsFriend( from ) )
				{
					from.SendLocalizedMessage( 503242 ); //You must ask the owner of this house to make you a friend in order to place this vendor here,
				}
				else if ( !house.Public ) 
				{
					from.SendLocalizedMessage( 503241 );//You cannot place this vendor.  Make sure the building is public and you have not reached your vendor limit.
				}
				else if ( !house.CanPlaceNewVendor() )
				{
					from.SendLocalizedMessage( 503241 ); // You cannot place this vendor or barkeep.  Make sure the house is public or a shop and has sufficient storage available.
				}
				//else if (house.FindTownshipNPC() != null)
				else if( house.CanPlacePlayerVendorInThisTownshipHouse() == false )
				{
					from.SendMessage("You cannot place a vendor in a house with the township NPCs present.");
				}
				else
				{
					Mobile v = new PlayerVendor(from, house);
					v.Direction = from.Direction & Direction.Mask;
					v.MoveToWorld(from.Location, from.Map);

					v.SayTo(from, 503246); // Ah! it feels good to be working again.

					this.Delete();
				}
			}
		}
	}
}