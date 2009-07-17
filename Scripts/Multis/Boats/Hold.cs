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

/* Multis/Boats/Hold.cs
 * ChangeLog:
 *  05/01/06 Taran Kain
 *		Added MaxItems override to differentiate between boat sizes. I also did the MaxWeight thing hella long ago,
 *		apparently I didn't document it.
 *  02/17/05, mith
 *		Changed base object from Container to BaseContainer to include fixes made there that override
 *			functionality in the core.
 */

using System;
using Server;
using Server.Multis;
using Server.Network;

namespace Server.Items
{
	public class Hold : BaseContainer
	{
		private BaseBoat m_Boat;

		public override int MaxWeight
		{ 
			get
			{ 
				if (m_Boat is SmallBoat || m_Boat is SmallDragonBoat)
					return 1200;
				if (m_Boat is MediumBoat || m_Boat is MediumDragonBoat)
					return 2400;
				if (m_Boat is LargeBoat || m_Boat is LargeDragonBoat)
					return 4800;
				return 0; // catch-all, player will definitely report this and erroneous boat will be found
			}
		}

		public override int MaxItems
		{
			get
			{
				if (m_Boat is SmallBoat || m_Boat is SmallDragonBoat)
					return 125;
				if (m_Boat is MediumBoat || m_Boat is MediumDragonBoat)
					return 250;
				if (m_Boat is LargeBoat || m_Boat is LargeDragonBoat)
					return 375;
				return 0; // catch-all, player will definitely report this and erroneous boat will be found
			}
		}
		
		public override int DefaultGumpID{ get{ return 0x4C; } }
		public override int DefaultDropSound{ get{ return 0x42; } }

		public override Rectangle2D Bounds
		{
			get{ return new Rectangle2D( 46, 74, 150, 110 ); }
		}

		public Hold( BaseBoat boat ) : base( 0x3EAE )
		{
			m_Boat = boat;
			Movable = false;
		}

		public Hold( Serial serial ) : base( serial )
		{
		}

		public void SetFacing( Direction dir )
		{
			switch ( dir )
			{
				case Direction.East:  ItemID = 0x3E65; break;
				case Direction.West:  ItemID = 0x3E93; break;
				case Direction.North: ItemID = 0x3EAE; break;
				case Direction.South: ItemID = 0x3EB9; break;
			}
		}

		public override bool OnDragDrop( Mobile from, Item item )
		{
			if ( m_Boat == null || !m_Boat.Contains( from ) || m_Boat.IsMoving )
				return false;

			return base.OnDragDrop( from, item );
		}

		public override bool OnDragDropInto( Mobile from, Item item, Point3D p )
		{
			if ( m_Boat == null || !m_Boat.Contains( from ) || m_Boat.IsMoving )
				return false;

			return base.OnDragDropInto( from, item, p );
		}

		public override bool CheckItemUse( Mobile from, Item item )
		{
			if ( m_Boat == null || !m_Boat.Contains( from ) || m_Boat.IsMoving )
				return false;

			return base.CheckItemUse( from, item );
		}

        public override bool CheckLift(Mobile from, Item item, ref LRReason reject)
		{
			if ( m_Boat == null || !m_Boat.Contains( from ) || m_Boat.IsMoving )
				return false;

			return base.CheckLift( from, item, ref reject );
		}

		public override void OnAfterDelete()
		{
			if ( m_Boat != null )
				m_Boat.Delete();
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( m_Boat == null || !m_Boat.Contains( from ) )
			{
				if ( m_Boat.TillerMan != null )
					m_Boat.TillerMan.Say( 502490 ); // You must be on the ship to open the hold.
			}
			else if ( m_Boat.IsMoving )
			{
				if ( m_Boat.TillerMan != null )
					m_Boat.TillerMan.Say( 502491 ); // I can not open the hold while the ship is moving.
			}
			else
			{
				base.OnDoubleClick( from );
			}
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 );

			writer.Write( m_Boat );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch ( version )
			{
				case 0:
				{
					m_Boat = reader.ReadItem() as BaseBoat;

					if ( m_Boat == null || Parent != null )
						Delete();

					Movable = false;

					break;
				}
			}
		}
	}
}