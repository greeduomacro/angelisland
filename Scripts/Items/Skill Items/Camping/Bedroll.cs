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

/* Items/Skill Items/Camping/Bedroll.cs
 * CHANGELOG:
 *  03/27/07, plasma
 *      - Fixed bedroll from disappearing if used within any container other than the backpack.
 *	08/14/06, weaver
 *		- Fixed so bedroll doesn't disappear when accessed via a nested container (modified to 
 *		perform iterative check on backpack).
 *		- Added sanity message for players so they know bedroll must be used from the ground.
 *	10/22/04 - Pix
 *		Changed camping to not use campfireregion.
 *	9/11/04, Pixie
 *		Updates Campfire's new OwnerUsedBedroll so the only people using bedrolls logout instantly.
 *	5/10/04, Pixie
 *		Initial working revision
 */

using System;
using System.Collections;

using Server.Network;
using Server.Gumps;

namespace Server.Items
{
	[FlipableAttribute( 0xA57, 0xA58, 0xA59 )]
	public class Bedroll : Item
	{
		[Constructable]
		public Bedroll() : this( 1 )
		{
		}

		[Constructable]
		public Bedroll( int amount ) : base( 0xA57 )
		{
			Stackable = false;
			Weight = 5.0;
			Amount = amount;
		}

		public Bedroll( Serial serial ) : base( serial )
		{
		}

		public override Item Dupe( int amount )
		{
			return base.Dupe( new Bedroll(), amount );
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
			// wea: 14/Aug/2006 Modified to perform iterative check & added sanity message
			if ( IsChildOf( from.Backpack ) )
			{
				from.SendMessage("You cannot use that from within your backpack.");
				return;
			}
            // pla: 03/22/2007 Same as above for non-backpack containers.
            else if (Parent != null && Parent is Container)
            {
                from.SendMessage("You cannot use that from within a container.");
                return;
            }

			Consume();

			Point3D loc;

			if ( Parent == null )
				loc = Location;
			else
				loc = from.Location;

			new UnrolledBedroll().MoveToWorld( loc, from.Map );

			Consume();
		}
	}

	[FlipableAttribute( 0xA55, 0xA56 )]
	public class UnrolledBedroll : Item
	{
		[Constructable]
		public UnrolledBedroll() : this( 1 )
		{
		}

		[Constructable]
		public UnrolledBedroll( int amount ) : base( 0xA55 )
		{
			Stackable = false;
			Weight = 5.0;
			Amount = amount;
		}

		public UnrolledBedroll( Serial serial ) : base( serial )
		{
		}

		public override Item Dupe( int amount )
		{
			return base.Dupe( new UnrolledBedroll(), amount );
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
			if ( !from.InRange( GetWorldLocation(), 2 ) )
			{
				from.SendLocalizedMessage( 500446 ); // That is too far away.
				return;
			}

			IPooledEnumerable eable = this.Map.GetItemsInRange ( this.Location,  3 );
			foreach ( Item item in eable )
			{
				if ( item is Campfire )
				{
					Campfire campfire = item as Campfire;
					if( campfire.CampSecure && campfire.Camper == from )
					{
						campfire.OwnerUsedBedroll = true;
						from.SendGump( new CampLogoutGump( from, this ) );
						eable.Free();
						return;
					}
				}
			}
			
			eable.Free();
			
			this.Consume();
			int x = from.X, y = from.Y;

			switch ( from.Direction & Direction.Mask )
			{
				case Direction.North: --y; break;
				case Direction.South: ++y; break;
				case Direction.West:  --x; break;
				case Direction.East:  ++x; break;
				case Direction.Up:    --x; --y; break;
				case Direction.Down:  ++x; ++y; break;
				case Direction.Left:  --x; ++y; break;
				case Direction.Right: ++x; --y; break;
			}

			new Bedroll().MoveToWorld( new Point3D( x, y, from.Z ), from.Map );
		}
	}
}