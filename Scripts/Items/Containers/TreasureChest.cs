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

/* Items/Containers/TreasureChest.cs
 * ChangeLog:
 *	11/20/04, Adam
 *		Add L0TreasureChest trainers (for building initial lockpick skill)
 *	6/27/04, adam
 *		Add L2TreasureChest, L3TreasureChest, L4TreasureChest, and L5TreasureChest
 *	6/25/04, adam
 *		Add L1TreasureChest
 */

using Server;
using Server.Items;
using Server.Multis;
using Server.Network;
using System;

namespace Server.Items
{
	[FlipableAttribute(0xe43, 0xe42)]
	public class L0TreasureChest : DungeonTreasureChest
	{
		//public override int DefaultGumpID{ get{ return 0x49; } }
		//public override int DefaultDropSound{ get{ return 0x42; } }

		//public override Rectangle2D Bounds
		//{
		//	get{ return new Rectangle2D( 20, 105, 150, 180 ); }
		//}

		[Constructable]
		public L0TreasureChest() : base( 0 )
		{
		}

		public L0TreasureChest(Serial serial) : base( serial )
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0); // version 
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}
	
	[FlipableAttribute( 0xe43, 0xe42 )] 
	public class L1TreasureChest : DungeonTreasureChest 
	{ 
		//public override int DefaultGumpID{ get{ return 0x49; } }
		//public override int DefaultDropSound{ get{ return 0x42; } }

		//public override Rectangle2D Bounds
		//{
		//	get{ return new Rectangle2D( 20, 105, 150, 180 ); }
		//}

		[Constructable] 
		public L1TreasureChest() : base( 1 ) 
		{ 
		} 

		public L1TreasureChest( Serial serial ) : base( serial ) 
		{ 
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
	} 

	[FlipableAttribute( 0xe43, 0xe42 )] 
	public class L2TreasureChest : DungeonTreasureChest 
	{ 
		//public override int DefaultGumpID{ get{ return 0x49; } }
		//public override int DefaultDropSound{ get{ return 0x42; } }

		//public override Rectangle2D Bounds
		//{
		//	get{ return new Rectangle2D( 20, 105, 150, 180 ); }
		//}

		[Constructable] 
		public L2TreasureChest() : base( 2 ) 
		{ 
		} 

		public L2TreasureChest( Serial serial ) : base( serial ) 
		{ 
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
	} 

	[FlipableAttribute( 0xe43, 0xe42 )] 
	public class L3TreasureChest : DungeonTreasureChest 
	{ 
		//public override int DefaultGumpID{ get{ return 0x49; } }
		//public override int DefaultDropSound{ get{ return 0x42; } }

		//public override Rectangle2D Bounds
		//{
		//	get{ return new Rectangle2D( 20, 105, 150, 180 ); }
		//}

		[Constructable] 
		public L3TreasureChest() : base( 3 ) 
		{ 
		} 

		public L3TreasureChest( Serial serial ) : base( serial ) 
		{ 
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
	} 

	[FlipableAttribute( 0xe43, 0xe42 )] 
	public class L4TreasureChest : DungeonTreasureChest 
	{ 
		//public override int DefaultGumpID{ get{ return 0x49; } }
		//public override int DefaultDropSound{ get{ return 0x42; } }

		//public override Rectangle2D Bounds
		//{
		//	get{ return new Rectangle2D( 20, 105, 150, 180 ); }
		//}

		[Constructable] 
		public L4TreasureChest() : base( 4 ) 
		{ 
		} 

		public L4TreasureChest( Serial serial ) : base( serial ) 
		{ 
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
	} 

	[FlipableAttribute( 0xe43, 0xe42 )] 
	public class L5TreasureChest : DungeonTreasureChest 
	{ 
		//public override int DefaultGumpID{ get{ return 0x49; } }
		//public override int DefaultDropSound{ get{ return 0x42; } }

		//public override Rectangle2D Bounds
		//{
		//	get{ return new Rectangle2D( 20, 105, 150, 180 ); }
		//}

		[Constructable] 
		public L5TreasureChest() : base( 5 ) 
		{ 
		} 

		public L5TreasureChest( Serial serial ) : base( serial ) 
		{ 
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
	} 

	[FlipableAttribute( 0xe43, 0xe42 )] 
	public class WoodenTreasureChest : BaseTreasureChest 
	{ 
		public override int DefaultGumpID{ get{ return 0x49; } }
		public override int DefaultDropSound{ get{ return 0x42; } }

		public override Rectangle2D Bounds
		{
			get{ return new Rectangle2D( 20, 105, 150, 180 ); }
		}

		[Constructable] 
		public WoodenTreasureChest() : base( 0xE43 ) 
		{ 
		} 

		public WoodenTreasureChest( Serial serial ) : base( serial ) 
		{ 
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
	} 

	[FlipableAttribute( 0xe41, 0xe40 )] 
	public class MetalGoldenTreasureChest : BaseTreasureChest 
	{ 
		public override int DefaultGumpID{ get{ return 0x42; } }
		public override int DefaultDropSound{ get{ return 0x42; } }

		public override Rectangle2D Bounds
		{
			get{ return new Rectangle2D( 20, 105, 150, 180 ); }
		}

		[Constructable] 
		public MetalGoldenTreasureChest() : base( 0xE41 ) 
		{ 
		} 

		public MetalGoldenTreasureChest( Serial serial ) : base( serial ) 
		{ 
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
	} 

	[FlipableAttribute( 0x9ab, 0xe7c )] 
	public class MetalTreasureChest : BaseTreasureChest 
	{ 
		public override int DefaultGumpID{ get{ return 0x4A; } }
		public override int DefaultDropSound{ get{ return 0x42; } }

		public override Rectangle2D Bounds
		{
			get{ return new Rectangle2D( 20, 105, 150, 180 ); }
		}

		[Constructable] 
		public MetalTreasureChest() : base( 0x9AB ) 
		{ 
		} 

		public MetalTreasureChest( Serial serial ) : base( serial ) 
		{ 
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
	} 
}