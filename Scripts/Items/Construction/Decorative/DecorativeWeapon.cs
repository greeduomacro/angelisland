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
using Server.Network;

namespace Server.Items
{
	[FlipableAttribute( 0x155E, 0x155F, 0x155C, 0x155D )] 
	public class DecorativeBowWest : Item
	{
		[Constructable]
		public DecorativeBowWest() : base( Utility.Random( 0x155E, 2 ) )
		{
			Movable = false;
		}

		public DecorativeBowWest( Serial serial ) : base( serial )
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

	[FlipableAttribute( 0x155C, 0x155D, 0x155E, 0x155F )] 
	public class DecorativeBowNorth : Item
	{
		[Constructable]
		public DecorativeBowNorth() : base( Utility.Random( 0x155C, 2 ) )
		{
			Movable = false;
		}

		public DecorativeBowNorth( Serial serial ) : base( serial )
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

	[FlipableAttribute( 0x1560, 0x1561, 0x1562, 0x1563 )] 
	public class DecorativeAxeNorth : Item
	{
		[Constructable]
		public DecorativeAxeNorth() : base( Utility.Random( 0x1560, 2 ) )
		{
			Movable = false;
		}

		public DecorativeAxeNorth( Serial serial ) : base( serial )
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

	[FlipableAttribute( 0x1562, 0x1563, 0x1560, 0x1561 )] 
	public class DecorativeAxeWest : Item
	{
		[Constructable]
		public DecorativeAxeWest() : base( Utility.Random( 0x1562, 2 ) )
		{
			Movable = false;
		}

		public DecorativeAxeWest( Serial serial ) : base( serial )
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

	public class DecorativeSwordNorth : Item
	{
		private InternalItem m_Item;

		[Constructable]
		public DecorativeSwordNorth() : base( 0x1565 )
		{
			Movable = false;

			m_Item = new InternalItem( this );
		}

		public DecorativeSwordNorth( Serial serial ) : base( serial )
		{
		}

		public override void OnLocationChange( Point3D oldLocation )
		{
			if ( m_Item != null )
				m_Item.Location = new Point3D( X - 1, Y, Z );
		}

		public override void OnMapChange()
		{
			if ( m_Item != null )
				m_Item.Map = Map;
		}

		public override void OnAfterDelete()
		{
			base.OnAfterDelete();

			if ( m_Item != null )
				m_Item.Delete();
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version

			writer.Write( m_Item );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			m_Item = reader.ReadItem() as InternalItem;
		}
		private class InternalItem : Item
		{
			private DecorativeSwordNorth m_Item;

			public InternalItem( DecorativeSwordNorth item ) : base( 0x1564 )
			{
				Movable = true;

				m_Item = item;
			}

			public InternalItem( Serial serial ) : base( serial )
			{
			}

			public override void OnLocationChange( Point3D oldLocation )
			{
				if ( m_Item != null )
					m_Item.Location = new Point3D( X + 1, Y, Z );
			}

			public override void OnMapChange()
			{
				if ( m_Item != null )
					m_Item.Map = Map;
			}

			public override void OnAfterDelete()
			{
				base.OnAfterDelete();

				if ( m_Item != null )
					m_Item.Delete();
			}

			public override void Serialize( GenericWriter writer )
			{
				base.Serialize( writer );

				writer.Write( (int) 0 ); // version

				writer.Write( m_Item );
			}

			public override void Deserialize( GenericReader reader )
			{
				base.Deserialize( reader );

				int version = reader.ReadInt();

				m_Item = reader.ReadItem() as DecorativeSwordNorth;
			}
		}
	}
	public class DecorativeSwordWest : Item
	{
		private InternalItem m_Item;

		[Constructable]
		public DecorativeSwordWest() : base( 0x1566 )
		{
			Movable = false;

			m_Item = new InternalItem( this );
		}

		public DecorativeSwordWest( Serial serial ) : base( serial )
		{
		}

		public override void OnLocationChange( Point3D oldLocation )
		{
			if ( m_Item != null )
				m_Item.Location = new Point3D( X , Y - 1 , Z );
		}

		public override void OnMapChange()
		{
			if ( m_Item != null )
				m_Item.Map = Map;
		}

		public override void OnAfterDelete()
		{
			base.OnAfterDelete();

			if ( m_Item != null )
				m_Item.Delete();
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version

			writer.Write( m_Item );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			m_Item = reader.ReadItem() as InternalItem;
		}
		private class InternalItem : Item
		{
			private DecorativeSwordWest m_Item;

			public InternalItem( DecorativeSwordWest item ) : base( 0x1567 )
			{
				Movable = true;

				m_Item = item;
			}

			public InternalItem( Serial serial ) : base( serial )
			{
			}

			public override void OnLocationChange( Point3D oldLocation )
			{
				if ( m_Item != null )
					m_Item.Location = new Point3D( X , Y + 1, Z );
			}

			public override void OnMapChange()
			{
				if ( m_Item != null )
					m_Item.Map = Map;
			}

			public override void OnAfterDelete()
			{
				base.OnAfterDelete();

				if ( m_Item != null )
					m_Item.Delete();
			}

			public override void Serialize( GenericWriter writer )
			{
				base.Serialize( writer );

				writer.Write( (int) 0 ); // version

				writer.Write( m_Item );
			}

			public override void Deserialize( GenericReader reader )
			{
				base.Deserialize( reader );

				int version = reader.ReadInt();

				m_Item = reader.ReadItem() as DecorativeSwordWest;
			}
		}
	}
	public class DecorativeDAxeNorth : Item
	{
		private InternalItem m_Item;

		[Constructable]
		public DecorativeDAxeNorth() : base( 0x1569 )
		{
			Movable = false;

			m_Item = new InternalItem( this );
		}

		public DecorativeDAxeNorth( Serial serial ) : base( serial )
		{
		}

		public override void OnLocationChange( Point3D oldLocation )
		{
			if ( m_Item != null )
				m_Item.Location = new Point3D( X - 1, Y, Z );
		}

		public override void OnMapChange()
		{
			if ( m_Item != null )
				m_Item.Map = Map;
		}

		public override void OnAfterDelete()
		{
			base.OnAfterDelete();

			if ( m_Item != null )
				m_Item.Delete();
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version

			writer.Write( m_Item );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			m_Item = reader.ReadItem() as InternalItem;
		}
		private class InternalItem : Item
		{
			private DecorativeDAxeNorth m_Item;

			public InternalItem( DecorativeDAxeNorth item ) : base( 0x1568 )
			{
				Movable = true;

				m_Item = item;
			}

			public InternalItem( Serial serial ) : base( serial )
			{
			}

			public override void OnLocationChange( Point3D oldLocation )
			{
				if ( m_Item != null )
					m_Item.Location = new Point3D( X + 1, Y, Z );
			}

			public override void OnMapChange()
			{
				if ( m_Item != null )
					m_Item.Map = Map;
			}

			public override void OnAfterDelete()
			{
				base.OnAfterDelete();

				if ( m_Item != null )
					m_Item.Delete();
			}

			public override void Serialize( GenericWriter writer )
			{
				base.Serialize( writer );

				writer.Write( (int) 0 ); // version

				writer.Write( m_Item );
			}

			public override void Deserialize( GenericReader reader )
			{
				base.Deserialize( reader );

				int version = reader.ReadInt();

				m_Item = reader.ReadItem() as DecorativeDAxeNorth;
			}
		}
	}
	public class DecorativeDAxeWest : Item
	{
		private InternalItem m_Item;

		[Constructable]
		public DecorativeDAxeWest() : base( 0x156A )
		{
			Movable = false;

			m_Item = new InternalItem( this );
		}

		public DecorativeDAxeWest( Serial serial ) : base( serial )
		{
		}

		public override void OnLocationChange( Point3D oldLocation )
		{
			if ( m_Item != null )
				m_Item.Location = new Point3D( X , Y - 1 , Z );
		}

		public override void OnMapChange()
		{
			if ( m_Item != null )
				m_Item.Map = Map;
		}

		public override void OnAfterDelete()
		{
			base.OnAfterDelete();

			if ( m_Item != null )
				m_Item.Delete();
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version

			writer.Write( m_Item );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			m_Item = reader.ReadItem() as InternalItem;
		}
		private class InternalItem : Item
		{
			private DecorativeDAxeWest m_Item;

			public InternalItem( DecorativeDAxeWest item ) : base( 0x156B )
			{
				Movable = true;

				m_Item = item;
			}

			public InternalItem( Serial serial ) : base( serial )
			{
			}

			public override void OnLocationChange( Point3D oldLocation )
			{
				if ( m_Item != null )
					m_Item.Location = new Point3D( X , Y + 1, Z );
			}

			public override void OnMapChange()
			{
				if ( m_Item != null )
					m_Item.Map = Map;
			}

			public override void OnAfterDelete()
			{
				base.OnAfterDelete();

				if ( m_Item != null )
					m_Item.Delete();
			}

			public override void Serialize( GenericWriter writer )
			{
				base.Serialize( writer );

				writer.Write( (int) 0 ); // version

				writer.Write( m_Item );
			}

			public override void Deserialize( GenericReader reader )
			{
				base.Deserialize( reader );

				int version = reader.ReadInt();

				m_Item = reader.ReadItem() as DecorativeDAxeWest;
			}
		}
	}
} 
