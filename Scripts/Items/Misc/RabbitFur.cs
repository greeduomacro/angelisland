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

/* Items/RabbitFur.cs
 * ChangeLog:
 *	3/26/05, Adam
 *		First time checkin
 */

using System;

namespace Server.Items
{
	public class RabbitFur1 : Item
	{
		[Constructable]
		public RabbitFur1() : base(0x11F4)
		{
			Name = "rabbit fur";
			Weight = 1.0;
		}

		public RabbitFur1(Serial serial) : base(serial)
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

	public class RabbitFur2 : Item
	{
		[Constructable]
		public RabbitFur2() : base(0x11F5)
		{
			Name = "rabbit fur";
			Weight = 1.0;
		}

		public RabbitFur2(Serial serial) : base(serial)
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

	public class RabbitFur3 : Item
	{
		[Constructable]
		public RabbitFur3() : base(0x11F6)
		{
			Name = "rabbit fur";
			Weight = 1.0;
		}

		public RabbitFur3(Serial serial) : base(serial)
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

	public class RabbitFur4 : Item
	{
		[Constructable]
		public RabbitFur4() : base(0x11F7)
		{
			Name = "rabbit fur";
			Weight = 1.0;
		}

		public RabbitFur4(Serial serial) : base(serial)
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

	public class RabbitFur5 : Item
	{
		[Constructable]
		public RabbitFur5() : base(0x11F8)
		{
			Name = "rabbit fur";
			Weight = 1.0;
		}

		public RabbitFur5(Serial serial) : base(serial)
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

	public class RabbitFur6 : Item
	{
		[Constructable]
		public RabbitFur6() : base(0x11F9)
		{
			Name = "rabbit fur";
			Weight = 1.0;
		}

		public RabbitFur6(Serial serial) : base(serial)
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

	public class RabbitFur7 : Item
	{
		[Constructable]
		public RabbitFur7() : base(0x11FA)
		{
			Name = "rabbit fur";
			Weight = 1.0;
		}

		public RabbitFur7(Serial serial) : base(serial)
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

	public class RabbitFur8 : Item
	{
		[Constructable]
		public RabbitFur8() : base(0x11FB)
		{
			Name = "rabbit fur";
			Weight = 1.0;
		}

		public RabbitFur8(Serial serial) : base(serial)
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