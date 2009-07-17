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

/* Items/Construction/Misc/Stautes.cs
 * ChangeLog:
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */

using System; 

namespace Server.Items 
{ 
	public class StatueSouth : Item 
	{ 
		[Constructable] 
		public StatueSouth() : base(0x139A) 
		{ 
			Weight = 10; 
		} 

		public StatueSouth(Serial serial) : base(serial) 
		{ 
		} 

		public override bool ForceShowProperties{ get{ return ObjectPropertyList.Enabled; } } 

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

	public class StatueSouth2 : Item 
	{ 
		[Constructable] 
		public StatueSouth2() : base(0x1227) 
		{ 
			Weight = 10; 
		} 

		public StatueSouth2(Serial serial) : base(serial) 
		{ 
		} 

		public override bool ForceShowProperties{ get{ return ObjectPropertyList.Enabled; } } 

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

	public class StatueNorth : Item 
	{ 
		[Constructable] 
		public StatueNorth() : base(0x139B) 
		{ 
			Weight = 10; 
		} 

		public StatueNorth(Serial serial) : base(serial) 
		{ 
		} 

		public override bool ForceShowProperties{ get{ return ObjectPropertyList.Enabled; } } 

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

	public class StatueWest : Item 
	{ 
		[Constructable] 
		public StatueWest() : base(0x1226) 
		{ 
			Weight = 10; 
		} 

		public StatueWest(Serial serial) : base(serial) 
		{ 
		} 

		public override bool ForceShowProperties{ get{ return ObjectPropertyList.Enabled; } } 

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

	public class StatueEast : Item 
	{ 
		[Constructable] 
		public StatueEast() : base(0x139C) 
		{ 
			Weight = 10; 
		} 

		public StatueEast(Serial serial) : base(serial) 
		{ 
		} 

		public override bool ForceShowProperties{ get{ return ObjectPropertyList.Enabled; } } 

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

	public class StatueEast2 : Item 
	{ 
		[Constructable] 
		public StatueEast2() : base(0x1224) 
		{ 
			Weight = 10; 
		} 

		public StatueEast2(Serial serial) : base(serial) 
		{ 
		} 

		public override bool ForceShowProperties{ get{ return ObjectPropertyList.Enabled; } } 

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

	public class StatueSouthEast : Item 
	{ 
		[Constructable] 
		public StatueSouthEast() : base(0x1225) 
		{ 
			Weight = 10; 
		} 

		public StatueSouthEast(Serial serial) : base(serial) 
		{ 
		} 

		public override bool ForceShowProperties{ get{ return ObjectPropertyList.Enabled; } } 

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

	public class BustSouth : Item 
	{ 
		[Constructable] 
		public BustSouth() : base(0x12CB) 
		{ 
			Weight = 10; 
		} 

		public BustSouth(Serial serial) : base(serial) 
		{ 
		} 

		public override bool ForceShowProperties{ get{ return ObjectPropertyList.Enabled; } } 

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

	public class BustEast : Item 
	{ 
		[Constructable] 
		public BustEast() : base(0x12CA) 
		{ 
			Weight = 10; 
		} 

		public BustEast(Serial serial) : base(serial) 
		{ 
		} 

		public override bool ForceShowProperties{ get{ return ObjectPropertyList.Enabled; } } 

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

	public class StatuePegasus : Item 
	{ 
		[Constructable] 
		public StatuePegasus() : base(0x139D) 
		{ 
			Weight = 10; 
		} 

		public StatuePegasus(Serial serial) : base(serial) 
		{ 
		} 

		public override bool ForceShowProperties{ get{ return ObjectPropertyList.Enabled; } } 

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

	public class StatuePegasus2 : Item 
	{ 
		[Constructable] 
		public StatuePegasus2() : base(0x1228) 
		{ 
			Weight = 10; 
		} 

		public StatuePegasus2(Serial serial) : base(serial) 
		{ 
		} 

		public override bool ForceShowProperties{ get{ return ObjectPropertyList.Enabled; } } 

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