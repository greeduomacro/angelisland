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

namespace Server.Multis
{
	public class MediumDragonBoat : BaseBoat
	{
		public override int NorthID{ get{ return 0x400C; } }
		public override int  EastID{ get{ return 0x400D; } }
		public override int SouthID{ get{ return 0x400E; } }
		public override int  WestID{ get{ return 0x400F; } }

		public override int HoldDistance{ get{ return 4; } }
		public override int TillerManDistance{ get{ return -5; } }

		public override Point2D StarboardOffset{ get{ return new Point2D(  2, 0 ); } }
		public override Point2D      PortOffset{ get{ return new Point2D( -2, 0 ); } }

		public override Point3D MarkOffset{ get{ return new Point3D( 0, 1, 3 ); } }

		public override BaseDockedBoat DockedBoat{ get{ return new MediumDockedDragonBoat( this ); } }

		[Constructable]
		public MediumDragonBoat()
		{
		}

		public MediumDragonBoat( Serial serial ) : base( serial )
		{
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int)0 );
		}
	}

	public class MediumDragonBoatDeed : BaseBoatDeed
	{
		public override int LabelNumber{ get{ return 1041208; } } // medium dragon ship deed
		public override BaseBoat Boat{ get{ return new MediumDragonBoat(); } }

		[Constructable]
		public MediumDragonBoatDeed() : base( 0x400C, Point3D.Zero )
		{
		}

		public MediumDragonBoatDeed( Serial serial ) : base( serial )
		{
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int)0 );
		}
	}

	public class MediumDockedDragonBoat : BaseDockedBoat
	{
		public override BaseBoat Boat{ get{ return new MediumDragonBoat(); } }

		public MediumDockedDragonBoat( BaseBoat boat ) : base( 0x400C, Point3D.Zero, boat )
		{
		}

		public MediumDockedDragonBoat( Serial serial ) : base( serial )
		{
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int)0 );
		}
	}
} 
