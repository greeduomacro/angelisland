/*
 *	This program is the CONFIDENTIAL and PROPRIETARY property
 *	of Tomasello Software LLC. Any unauthorized use, reproduction or
 *	transfer of this computer program is strictly prohibited.
 *
 *      Copyright (c) 2004 Tomasello Software LLC.
 *	This is an unpublished work, and is subject to limited distribution and
 *	restricted disclosure only. ALL RIGHTS RESERVED.
 *
 *			RESTRICTED RIGHTS LEGEND
 *	Use, duplication, or disclosure by the Government is subject to
 *	restrictions set forth in subparagraph (c)(1)(ii) of the Rights in
 * 	Technical Data and Computer Software clause at DFARS 252.227-7013.
 *
 *	Angel Island UO Shard	Version 1.0
 *			Release A
 *			March 25, 2004
 */

/* Scripts/Items/Containers/FurnitureContainer.cs
 * ChangeLog:
 *	7/30/05, erlein
 *		Fixed orientation problem.
 *	7/29/05, erlein
 *		- Removed some of the flippable attributes on FullBookcase as were
 *		preventing rotation of the bookcase when dropped.
 *		- Added FullBookcase2 and FullBookcase3.
 *	7/28/05, erlein
 *		Modified FullBookcase to be 25 stones instead of 1.
 *	5/9/05, Adam
 *		Push the Deco flag down to the Container level
 *		Pack old property from serialization routines.
 *	9/25/04, Adam
 *		Serialize new Deco attribute to enable Decorative Furniture.
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */

using System;
using System.Collections;
using Server;
using Server.Multis;
using Server.Network;

namespace Server.Items
{
	[Furniture]
	[Flipable( 0xA97, 0xA99 )]

	public class FullBookcase : BaseContainer
	{
		public override int DefaultGumpID{ get{ return 0x4D; } }
		public override int DefaultDropSound{ get{ return 0x42; } }

		public override Rectangle2D Bounds
		{
			get{ return new Rectangle2D( 80, 5, 140, 70 ); }
		}

		[Constructable]
		public FullBookcase() : base( 0xa97 )
		{
			Weight = 12.0;
		}

		public FullBookcase( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 2 );		// version
			//writer.Write( (bool) false );	// removed deco field in version 2
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
			switch ( version )
			{
				case 2:
				{
					// removed deco field (case 1)
					goto case 0;
				}
				case 1:
				{
					bool dummy = reader.ReadBool();
					goto case 0;
				}
				case 0:
				{
					break;
				}
			}
		}
	}

	[Furniture]
	[Flipable( 0xA98, 0xA9A )]

	public class FullBookcase2 : BaseContainer
	{
		public override int DefaultGumpID{ get{ return 0x4D; } }
		public override int DefaultDropSound{ get{ return 0x42; } }

		public override Rectangle2D Bounds
		{
			get{ return new Rectangle2D( 80, 5, 140, 70 ); }
		}

		[Constructable]
		public FullBookcase2() : base( 0xa98 )
		{
			Weight = 12.0;
		}

		public FullBookcase2( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 );		// version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}

	[Furniture]
	[Flipable( 0xA9B, 0xA9C )]

	public class FullBookcase3 : BaseContainer
	{
		public override int DefaultGumpID{ get{ return 0x4D; } }
		public override int DefaultDropSound{ get{ return 0x42; } }

		public override Rectangle2D Bounds
		{
			get{ return new Rectangle2D( 80, 5, 140, 70 ); }
		}

		[Constructable]
		public FullBookcase3() : base( 0xa9c )
		{
			Weight = 12.0;
		}

		public FullBookcase3( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 );		// version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}

	[Furniture]
	[Flipable( 0xa9d, 0xa9e )]
	public class EmptyBookcase : BaseContainer
	{
		public override int DefaultGumpID{ get{ return 0x4D; } }
		public override int DefaultDropSound{ get{ return 0x42; } }

		public override Rectangle2D Bounds
		{
			get{ return new Rectangle2D( 80, 5, 140, 70 ); }
		}

		[Constructable]
		public EmptyBookcase() : base( 0xA9D )
		{
			Weight = 1.0;
		}

		public EmptyBookcase( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 2 );		// version
			//writer.Write( (bool) false );	// removed deco field in version 2
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
			switch ( version )
			{
				case 2:
				{
					// removed deco field (case 1)
					goto case 0;
				}
				case 1:
				{
					bool dummy = reader.ReadBool();
					goto case 0;
				}
				case 0:
				{
					break;
				}
			}
		}
	}

	[Furniture]
	[Flipable( 0xa2c, 0xa34 )]
	public class Drawer : BaseContainer
	{
		public override int DefaultGumpID{ get{ return 0x51; } }
		public override int DefaultDropSound{ get{ return 0x42; } }

		public override Rectangle2D Bounds
		{
			get{ return new Rectangle2D( 20, 10, 150, 90 ); }
		}

		[Constructable]
		public Drawer() : base( 0xA2C )
		{
			Weight = 1.0;
		}

		public Drawer( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 2 );			// version
			//writer.Write( (bool) false );		// removed deco field in version 2
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
			switch ( version )
			{
				case 2:
				{
					// removed deco field (case 1)
					goto case 0;
				}
				case 1:
				{
					bool dummy = reader.ReadBool();
					goto case 0;
				}
				case 0:
				{
					break;
				}
			}
		}
	}

	[Furniture]
	[Flipable( 0xa30, 0xa38 )]
	public class FancyDrawer : BaseContainer
	{
		public override int DefaultGumpID{ get{ return 0x48; } }
		public override int DefaultDropSound{ get{ return 0x42; } }

		public override Rectangle2D Bounds
		{
			get{ return new Rectangle2D( 20, 10, 150, 90 ); }
		}

		[Constructable]
		public FancyDrawer() : base( 0xA30 )
		{
			Weight = 1.0;
		}

		public FancyDrawer( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 2 );		// version
			//writer.Write( (bool) false );	// removed deco field in version 2
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
			switch ( version )
			{
				case 2:
				{
					// removed deco field (case 1)
					goto case 0;
				}
				case 1:
				{
					bool dummy = reader.ReadBool();
					goto case 0;
				}
				case 0:
				{
					break;
				}
			}
		}
	}

	[Furniture]
	[Flipable( 0xa4f, 0xa53 )]
	public class Armoire : BaseContainer
	{
		public override int DefaultGumpID{ get{ return 0x4F; } }
		public override int DefaultDropSound{ get{ return 0x42; } }

		public override Rectangle2D Bounds
		{
			get{ return new Rectangle2D( 30, 30, 90, 150 ); }
		}

		[Constructable]
		public Armoire() : base( 0xA4F )
		{
			Weight = 1.0;
		}

		public override void DisplayTo( Mobile m )
		{
			if ( DynamicFurniture.Open( this, m ) )
				base.DisplayTo( m );
		}

		public Armoire( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 2 );		// version
			//writer.Write( (bool) false );	// removed deco field in version 2
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
			switch ( version )
			{
				case 2:
				{
					// removed deco field (case 1)
					goto case 0;
				}
				case 1:
				{
					bool dummy = reader.ReadBool();
					goto case 0;
				}
				case 0:
				{
					break;
				}
			}

			DynamicFurniture.Close( this );
		}
	}

	[Furniture]
	[Flipable( 0xa4d, 0xa51 )]
	public class FancyArmoire : BaseContainer
	{
		public override int DefaultGumpID{ get{ return 0x4E; } }
		public override int DefaultDropSound{ get{ return 0x42; } }

		public override Rectangle2D Bounds
		{
			get{ return new Rectangle2D( 30, 30, 90, 150 ); }
		}

		[Constructable]
		public FancyArmoire() : base( 0xA4D )
		{
			Weight = 1.0;
		}

		public override void DisplayTo( Mobile m )
		{
			if ( DynamicFurniture.Open( this, m ) )
				base.DisplayTo( m );
		}

		public FancyArmoire( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 2 );		// version
			//writer.Write( (bool) false );	// removed deco field in version 2
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
			switch ( version )
			{
				case 2:
				{
					// removed deco field (case 1)
					goto case 0;
				}
				case 1:
				{
					bool dummy = reader.ReadBool();
					goto case 0;
				}
				case 0:
				{
					break;
				}
			}

			DynamicFurniture.Close( this );
		}
	}

	public class DynamicFurniture
	{
		private static Hashtable m_Table = new Hashtable();

		public static bool Open( Container c, Mobile m )
		{
			if ( m_Table.Contains( c ) )
			{
				c.SendRemovePacket();
				Close( c );
				c.Delta( ItemDelta.Update );
				c.ProcessDelta();
				return false;
			}

			if ( c is Armoire || c is FancyArmoire )
			{
				Timer t = new FurnitureTimer( c, m );
				t.Start();
				m_Table[c] = t;

				switch ( c.ItemID )
				{
					case 0xA4D: c.ItemID = 0xA4C; break;
					case 0xA4F: c.ItemID = 0xA4E; break;
					case 0xA51: c.ItemID = 0xA50; break;
					case 0xA53: c.ItemID = 0xA52; break;
				}
			}

			return true;
		}

		public static void Close( Container c )
		{
			Timer t = (Timer)m_Table[c];

			if ( t != null )
			{
				t.Stop();
				m_Table.Remove( c );
			}

			if ( c is Armoire || c is FancyArmoire )
			{
				switch ( c.ItemID )
				{
					case 0xA4C: c.ItemID = 0xA4D; break;
					case 0xA4E: c.ItemID = 0xA4F; break;
					case 0xA50: c.ItemID = 0xA51; break;
					case 0xA52: c.ItemID = 0xA53; break;
				}
			}
		}
	}

	public class FurnitureTimer : Timer
	{
		private Container m_Container;
		private Mobile m_Mobile;

		public FurnitureTimer( Container c, Mobile m ) : base( TimeSpan.FromSeconds( 0.5 ), TimeSpan.FromSeconds( 0.5 ) )
		{
			Priority = TimerPriority.TwoFiftyMS;

			m_Container = c;
			m_Mobile = m;
		}

		protected override void OnTick()
		{
			if ( m_Mobile.Map != m_Container.Map || !m_Mobile.InRange( m_Container.GetWorldLocation(), 3 ) )
				DynamicFurniture.Close( m_Container );
		}
	}
}