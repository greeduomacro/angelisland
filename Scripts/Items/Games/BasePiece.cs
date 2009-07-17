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

namespace Server.Items
{
	public class BasePiece : Item
	{
		private BaseBoard m_Board;

		public BaseBoard Board
		{
			get { return m_Board; }
			set { m_Board = value; }
		}

		public override bool IsVirtualItem{ get{ return true; } }

		public BasePiece( int itemID, string name, BaseBoard board ) : base( itemID )
		{
			m_Board = board;
			Name = name;
		}

		public BasePiece( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 );
			writer.Write( m_Board );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch ( version )
			{
				case 0:
				{
					m_Board = (BaseBoard)reader.ReadItem();

					if ( m_Board == null || Parent == null )
						Delete();

					break;
				}
			}
		}

		public override void OnSingleClick( Mobile from )
		{
			if ( m_Board == null || m_Board.Deleted )
				Delete();
			else if ( !IsChildOf( m_Board ) )
				m_Board.DropItem( this );
			else
				base.OnSingleClick( from );
		}

		public override bool OnDragLift( Mobile from )
		{
			if ( m_Board == null || m_Board.Deleted )
			{
				Delete();
				return false;
			}
			else if ( !IsChildOf( m_Board ) )
			{
				m_Board.DropItem( this );
				return false;
			}
			else
			{
				return true;
			}
		}

		public override bool CanTarget{ get{ return false; } }

		public override bool DropToMobile( Mobile from, Mobile target, Point3D p )
		{
			return false;
		}

		public override bool DropToItem( Mobile from, Item target, Point3D p )
		{
			return ( target == m_Board && p.X != -1 && p.Y != -1 && base.DropToItem( from, target, p ) );
		}

		public override bool DropToWorld( Mobile from, Point3D p )
		{
			return false;
		}

		public override int GetLiftSound( Mobile from )
		{
			return -1;
		}
	}
}�