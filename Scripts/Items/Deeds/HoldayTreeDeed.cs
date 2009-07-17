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

/* Items/Deeds/HolidayTreeDeed.cs
 * ChangeLog:
 *  08/8/06, Kit
 *		Make holiday tree be added to houses addon list so they poof when a house is deleted.
 *	12/8/05, erlein
 *		Altered so label displays "a christmas tree deed" instead of "a holiday tree deed".
 *  12/19/04, Jade
 *      Change hue to a christmas green.
 *  12/12/04, Jade
 *      Unblessed the deeds.
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */

using System;
using Server;
using Server.Items;
using Server.Gumps;
using Server.Multis;
using Server.Network;
using Server.Targeting;

namespace Server.Items
{
	public class HolidayTreeDeed : Item
	{
		public override int LabelNumber{ get{ return 1041116; } } // a deed for a holiday tree

		[Constructable]
		public HolidayTreeDeed() : base( 0x14F0 )
		{
            //Jade: change hue to a christmassy green
			Hue = 0xAC;
			Weight = 1.0;
            
			//Jade: make these unblessed.
			LootType = LootType.Regular;

			Name = "a christmas tree deed";
		}

		public HolidayTreeDeed( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 1 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			// erl: removed - LootType = LootType.Blessed;

			switch( version )
			{
				case 1:
				{
					// Post tree renaming
					goto case 0;
				}
				case 0:
				{
					if( version < 1 )
						Name = "a christmas tree deed";

					break;
				}
			}
		}

		public bool ValidatePlacement( Mobile from, Point3D loc )
		{
			if ( from.AccessLevel >= AccessLevel.GameMaster )
				return true;

			if ( !from.InRange( this.GetWorldLocation(), 1 ) )
			{
				from.SendLocalizedMessage( 500446 ); // That is too far away.
				return false;
			}

			if ( DateTime.Now.Month != 12 )
			{
				from.SendLocalizedMessage( 1005700 ); // You will have to wait till next December to put your tree back up for display.
				return false;
			}

			Map map = from.Map;

			if ( map == null )
				return false;

			BaseHouse house = BaseHouse.FindHouseAt( loc, map, 20 );

			if ( house == null || !house.IsFriend( from ) )
			{
				from.SendLocalizedMessage( 1005701 ); // The holiday tree can only be placed in your house.
				return false;
			}

			if ( !map.CanFit( loc, 20 ) )
			{
				from.SendLocalizedMessage( 500269 ); // You cannot build that there.
				return false;
			}

			return true;
		}

		public void BeginPlace( Mobile from, HolidayTreeType type )
		{
			from.BeginTarget( -1, true, TargetFlags.None, new TargetStateCallback( Placement_OnTarget ), type );
		}

		public void Placement_OnTarget( Mobile from, object targeted, object state )
		{
			IPoint3D p = targeted as IPoint3D;

			if ( p == null )
				return;

			Point3D loc = new Point3D( p );

			if ( p is StaticTarget )
				loc.Z -= TileData.ItemTable[((StaticTarget)p).ItemID & 0x3FFF].CalcHeight; /* NOTE: OSI does not properly normalize Z positioning here.
																							* A side affect is that you can only place on floors (due to the CanFit call).
																							* That functionality may be desired. And so, it's included in this script.
																							*/

			if ( ValidatePlacement( from, loc ) )
				EndPlace( from, (HolidayTreeType) state, loc );
		}

		public void EndPlace( Mobile from, HolidayTreeType type, Point3D loc )
		{
			this.Delete();
			HolidayTree temp = new HolidayTree(from,type,loc);

			BaseHouse house = BaseHouse.FindHouseAt(loc, from.Map, 20);
			if(house != null)
				house.Addons.Add(temp);

		}

		public override void OnDoubleClick( Mobile from )
		{
			from.CloseGump( typeof( HolidayTreeChoiceGump ) );
			from.SendGump( new HolidayTreeChoiceGump( from, this ) );
		}
	}

	public class HolidayTreeChoiceGump : Gump
	{
		private Mobile m_From;
		private HolidayTreeDeed m_Deed;

		public HolidayTreeChoiceGump( Mobile from, HolidayTreeDeed deed ) : base( 200, 200 )
		{
			m_From = from;
			m_Deed = deed;

			AddPage( 0 );

			AddBackground( 0, 0, 220, 120, 5054 );
			AddBackground( 10, 10, 200, 100, 3000 );

			AddButton( 20, 35, 4005, 4007, 1, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 55, 35, 145, 25, 1018322, false, false ); // Classic

			AddButton( 20, 65, 4005, 4007, 2, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 55, 65, 145, 25, 1018321, false, false ); // Modern
		}

		public override void OnResponse( NetState sender, RelayInfo info )
		{
			if ( m_Deed.Deleted )
				return;

			switch ( info.ButtonID )
			{
				case 1:
				{
					m_Deed.BeginPlace( m_From, HolidayTreeType.Classic );
					break;
				}
				case 2:
				{
					m_Deed.BeginPlace( m_From, HolidayTreeType.Modern );
					break;
				}
			}
		}
	}
}