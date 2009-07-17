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

/* Items/Special/Holiday/Christmas/HolidayTree.cs
 * CHANGELOG:
 *  08/8/06, Kit
 *		Added check to remove tree from houses addon list when redeeded.
 *	12/8/05, erlein
 *		Altered so label displays "a christmas tree" instead of "a holiday tree".
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */

using System;
using System.Collections;
using Server.Network;
using Server.Items;
using Server.Multis;

namespace Server.Items
{
	public enum HolidayTreeType
	{
		Classic,
		Modern
	}

	public class HolidayTree : Item
	{
		private ArrayList m_Components;
		private Mobile m_Placer;

		[CommandProperty( AccessLevel.GameMaster )]
		public Mobile Placer
		{
			get{ return m_Placer; }
			set{ m_Placer = value; }
		}

		private class Ornament : Item
		{
			public override int LabelNumber{ get{ return 1041118; } } // a tree ornament

			public Ornament( int itemID ) : base( itemID )
			{
				Movable = false;
			}

			public Ornament( Serial serial ) : base( serial )
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

		private class TreeTrunk : Item
		{
			private HolidayTree m_Tree;

			public override int LabelNumber{ get{ return 1041117; } } // a tree for the holidays

			public TreeTrunk( HolidayTree tree, int itemID ) : base( itemID )
			{
				Movable = false;
				MoveToWorld( tree.Location, tree.Map );

				m_Tree = tree;
			}

			public TreeTrunk( Serial serial ) : base( serial )
			{
			}

			public override void OnDoubleClick( Mobile from )
			{
				if ( m_Tree != null && !m_Tree.Deleted )
					m_Tree.OnDoubleClick( from );
			}

			public override void Serialize( GenericWriter writer )
			{
				base.Serialize( writer );

				writer.Write( (int) 0 ); // version

				writer.Write( m_Tree );
			}

			public override void Deserialize( GenericReader reader )
			{
				base.Deserialize( reader );

				int version = reader.ReadInt();

				switch ( version )
				{
					case 0:
					{
						m_Tree = reader.ReadItem() as HolidayTree;

						if ( m_Tree == null )
							Delete();

						break;
					}
				}
			}
		}

		public override int LabelNumber{ get{ return 1041117; } } // a tree for the holidays

		public HolidayTree( Mobile from, HolidayTreeType type, Point3D loc ) : base( 1 )
		{
			Movable = false;
			MoveToWorld( loc, from.Map );

			Name = "a christmas tree";

			m_Placer = from;
			m_Components = new ArrayList();

			switch ( type )
			{
				case HolidayTreeType.Classic:
				{
					ItemID = 0xCD7;

					AddItem( 0, 0, 0, new TreeTrunk( this, 0xCD6 ) );

					AddOrnament( 0, 0,  2, 0xF22 );
					AddOrnament( 0, 0,  9, 0xF18 );
					AddOrnament( 0, 0, 15, 0xF20 );
					AddOrnament( 0, 0, 19, 0xF17 );
					AddOrnament( 0, 0, 20, 0xF24 );
					AddOrnament( 0, 0, 20, 0xF1F );
					AddOrnament( 0, 0, 20, 0xF19 );
					AddOrnament( 0, 0, 21, 0xF1B );
					AddOrnament( 0, 0, 28, 0xF2F );
					AddOrnament( 0, 0, 30, 0xF23 );
					AddOrnament( 0, 0, 32, 0xF2A );
					AddOrnament( 0, 0, 33, 0xF30 );
					AddOrnament( 0, 0, 34, 0xF29 );
					AddOrnament( 0, 1,  7, 0xF16 );
					AddOrnament( 0, 1,  7, 0xF1E );
					AddOrnament( 0, 1, 12, 0xF0F );
					AddOrnament( 0, 1, 13, 0xF13 );
					AddOrnament( 0, 1, 18, 0xF12 );
					AddOrnament( 0, 1, 19, 0xF15 );
					AddOrnament( 0, 1, 25, 0xF28 );
					AddOrnament( 0, 1, 29, 0xF1A );
					AddOrnament( 0, 1, 37, 0xF2B );
					AddOrnament( 1, 0, 13, 0xF10 );
					AddOrnament( 1, 0, 14, 0xF1C );
					AddOrnament( 1, 0, 16, 0xF14 );
					AddOrnament( 1, 0, 17, 0xF26 );
					AddOrnament( 1, 0, 22, 0xF27 );

					break;
				}
				case HolidayTreeType.Modern:
				{
					ItemID = 0x1B7E;

					AddOrnament( 0, 0,  2, 0xF2F );
					AddOrnament( 0, 0,  2, 0xF20 );
					AddOrnament( 0, 0,  2, 0xF22 );
					AddOrnament( 0, 0,  5, 0xF30 );
					AddOrnament( 0, 0,  5, 0xF15 );
					AddOrnament( 0, 0,  5, 0xF1F );
					AddOrnament( 0, 0,  5, 0xF2B );
					AddOrnament( 0, 0,  6, 0xF0F );
					AddOrnament( 0, 0,  7, 0xF1E );
					AddOrnament( 0, 0,  7, 0xF24 );
					AddOrnament( 0, 0,  8, 0xF29 );
					AddOrnament( 0, 0,  9, 0xF18 );
					AddOrnament( 0, 0, 14, 0xF1C );
					AddOrnament( 0, 0, 15, 0xF13 );
					AddOrnament( 0, 0, 15, 0xF20 );
					AddOrnament( 0, 0, 16, 0xF26 );
					AddOrnament( 0, 0, 17, 0xF12 );
					AddOrnament( 0, 0, 18, 0xF17 );
					AddOrnament( 0, 0, 20, 0xF1B );
					AddOrnament( 0, 0, 23, 0xF28 );
					AddOrnament( 0, 0, 25, 0xF18 );
					AddOrnament( 0, 0, 25, 0xF2A );
					AddOrnament( 0, 1,  7, 0xF16 );

					break;
				}
			}
		}

		public override void OnAfterDelete()
		{
			for ( int i = 0; i < m_Components.Count; ++i )
				((Item)m_Components[i]).Delete();
		}

		private void AddOrnament( int x, int y, int z, int itemID )
		{
			AddItem( x + 1, y + 1, z + 11, new Ornament( itemID ) );
		}

		private void AddItem( int x, int y, int z, Item item )
		{
			item.MoveToWorld( new Point3D( this.Location.X + x, this.Location.Y + y, this.Location.Z + z ), this.Map );

			m_Components.Add( item );
		}

		public HolidayTree( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 2 ); // version

			writer.Write( m_Placer );

			writer.Write( (int) m_Components.Count );

			for ( int i = 0; i < m_Components.Count; ++i )
				writer.Write( (Item)m_Components[i] );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch ( version )
			{
				case 2:
				{
					// Post tree renaming
					goto case 1;
				}
				case 1:
				{
					m_Placer = reader.ReadMobile();

					if( version < 2 )
						Name = "a christmas tree";

					goto case 0;
				}
				case 0:
				{
					int count = reader.ReadInt();

					m_Components = new ArrayList( count );

					for ( int i = 0; i < count; ++i )
					{
						Item item = reader.ReadItem();

						if ( item != null )
							m_Components.Add( item );
					}

					break;
				}
			}
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( from.InRange( this.GetWorldLocation(), 1 ) )
			{
				if ( m_Placer == null || from == m_Placer || from.AccessLevel >= AccessLevel.GameMaster )
				{
					BaseHouse house = BaseHouse.FindHouseAt(this.Location, from.Map, 20);
					if(house != null)
						house.Addons.Remove(this);

					from.AddToBackpack( new HolidayTreeDeed() );

					this.Delete();

					from.SendLocalizedMessage( 503393 ); // A deed for the tree has been placed in your backpack.
				}
				else
				{
					from.SendLocalizedMessage( 503396 ); // You cannot take this tree down.
				}
			}
			else
			{
				from.SendLocalizedMessage( 500446 ); // That is too far away.
			}
		}
	}
}