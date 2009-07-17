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

/* /Scripts/Items/Construction/Chairs/Chairs.cs
 * ChangeLog
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */

using System;

namespace Server.Items
{
	[Furniture]
	[Flipable( 0xB4F, 0xB4E, 0xB50, 0xB51 )]
	public class FancyWoodenChairCushion : Item
	{
		[Constructable]
		public FancyWoodenChairCushion() : base(0xB4F)
		{
			Weight = 20.0;
		}

		public FancyWoodenChairCushion(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int) 0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			if ( Weight == 6.0 )
				Weight = 20.0;
		}
	}

	[Furniture]
	[Flipable( 0xB53, 0xB52, 0xB54, 0xB55 )]
	public class WoodenChairCushion : Item
	{
		[Constructable]
		public WoodenChairCushion() : base(0xB53)
		{
			Weight = 20.0;
		}

		public WoodenChairCushion(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int) 0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			if ( Weight == 6.0 )
				Weight = 20.0;
		}
	}

	[Furniture]
	[Flipable( 0xB57, 0xB56, 0xB59, 0xB58 )]
	public class WoodenChair : Item
	{
		[Constructable]
		public WoodenChair() : base(0xB57)
		{
			Weight = 20.0;
		}

		public WoodenChair(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int) 0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			if ( Weight == 6.0 )
				Weight = 20.0;
		}
	}

	[Furniture]
	[Flipable( 0xB5B, 0xB5A, 0xB5C, 0xB5D )]
	public class BambooChair : Item
	{
		[Constructable]
		public BambooChair() : base(0xB5B)
		{
			Weight = 20.0;
		}

		public BambooChair(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int) 0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			if ( Weight == 6.0 )
				Weight = 20.0;
		}
	}

	[DynamicFliping]
	[Flipable(0x1218, 0x1219, 0x121A, 0x121B)]
	public class StoneChair : Item
	{
		[Constructable]
		public StoneChair() : base(0x1218)
		{
			Weight = 20;
		}

		public StoneChair(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int) 0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}
}