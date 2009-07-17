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
/*         changelog.
 *      9/24/04 Lego Eater
 *              changed delay of how long preveiw stays up from 20 to 5
 *
 */

using System;
using System.Collections;
using Server;
using Server.Items;

namespace Server.Multis
{
	public class PreviewHouse : BaseMulti
	{
		private ArrayList m_Components;
		private Timer m_Timer;

		public PreviewHouse( int multiID ) : base( multiID | 0x4000 )
		{
			m_Components = new ArrayList();

			MultiComponentList mcl = this.Components;

			for ( int i = 1; i < mcl.List.Length; ++i )
			{
				MultiTileEntry entry = mcl.List[i];

				if ( entry.m_Flags == 0 )
				{
					Item item = new Static( entry.m_ItemID & 0x3FFF );

					item.MoveToWorld( new Point3D( X + entry.m_OffsetX, Y + entry.m_OffsetY, Z + entry.m_OffsetZ ), Map );

					m_Components.Add( item );
				}
			}

			m_Timer = new DecayTimer( this );
			m_Timer.Start();
		}

		public override void OnLocationChange( Point3D oldLocation )
		{
			base.OnLocationChange( oldLocation );

			if ( m_Components == null )
				return;

			int xOffset = X - oldLocation.X;
			int yOffset = Y - oldLocation.Y;
			int zOffset = Z - oldLocation.Z;

			for ( int i = 0; i < m_Components.Count; ++i )
			{
				Item item = (Item)m_Components[i];

				item.MoveToWorld( new Point3D( item.X + xOffset, item.Y + yOffset, item.Z + zOffset ), this.Map );
			}
		}

		public override void OnMapChange()
		{
			base.OnMapChange();

			if ( m_Components == null )
				return;

			for ( int i = 0; i < m_Components.Count; ++i )
			{
				Item item = (Item)m_Components[i];

				item.Map = this.Map;
			}
		}

		public override void OnDelete()
		{
			base.OnDelete();

			if ( m_Components == null )
				return;

			for ( int i = 0; i < m_Components.Count; ++i )
			{
				Item item = (Item)m_Components[i];

				item.Delete();
			}
		}

		public override void OnAfterDelete()
		{
			if ( m_Timer != null )
				m_Timer.Stop();

			m_Timer = null;

			base.OnAfterDelete();
		}

		public PreviewHouse( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version

			writer.WriteItemList( m_Components );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch ( version )
			{
				case 0:
				{
					m_Components = reader.ReadItemList();

					break;
				}
			}

			Delete();
		}

		private class DecayTimer : Timer
		{
			private Item m_Item;

			public DecayTimer( Item item ) : base( TimeSpan.FromSeconds( 5.0 ) )
			{
				m_Item = item;
				Priority = TimerPriority.OneSecond;
			}

			protected override void OnTick()
			{
				m_Item.Delete();
			}
		}
	}
}�