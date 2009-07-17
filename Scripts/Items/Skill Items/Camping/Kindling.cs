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

/* Items/Skill Items/Camping/Kindling.cs
 * CHANGELOG:
 *  11/21/06, Plasma
 *      Prevent secure message if in a no camp zone
 *	5/10/06, Adam
 *		remove assumption players region is non-null
 *	5/03/06, weaver
 *		Made camping work 100% of the time in tents.
 * 	5/12/04, Pixie
 *		Reversed the previous change - can camp in dungeons now.
 *	5/10/04, Pixie
 *		Made it so you can't camp in a dungeon
 *	5/10/04, Pixie
 *		Initial working revision
 */

using System;
using Server.Network;
using Server.Regions;
using Server.Multis;
using Server.Spells;

namespace Server.Items
{
	public class Kindling : Item
	{
		[Constructable]
		public Kindling() : this( 1 )
		{
		}

		[Constructable]
		public Kindling( int amount ) : base( 0xDE1 )
		{
			Stackable = true;
			Weight = 5.0;
			Amount = amount;
		}

		public Kindling( Serial serial ) : base( serial )
		{
		}

		public override Item Dupe( int amount )
		{
			return base.Dupe( new Kindling(), amount );
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
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
            
            
			// Adam: remove assumption players region is non-null
			HouseRegion hr = from.Region as HouseRegion;
            
			// wea: are you in a tent? if so, 100% success
			if ( from.CheckSkill( SkillName.Camping, 0.0, 100.0 ) || hr != null && hr.House is Tent )
			{
				Point3D loc;

				if ( Parent == null )
					loc = Location;
				else
					loc = from.Location;

				Consume();

                // Pla: Don't show message if in non camping zone
                if (!CampHelper.InRestrictedArea(from))
                {
                    from.SendLocalizedMessage(500620); // You feel it would take a few moments to secure your camp.
                }

				new Campfire( from ).MoveToWorld( loc, from.Map );
			}
			else
			{
				from.SendLocalizedMessage( 501696 ); // You fail to ignite the campfire.
			}
		}
	}
}