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
using System.Collections;
using Server;

namespace Server.Items
{
	public class EffectItem : Item
	{
		private static ArrayList m_Free = new ArrayList(); // List of available EffectItems

		public static readonly TimeSpan DefaultDuration = TimeSpan.FromSeconds( 5.0 );

		public static EffectItem Create( Point3D p, Map map, TimeSpan duration )
		{
			EffectItem item = null;

			for ( int i = m_Free.Count - 1; item == null && i >= 0; --i ) // We reuse new entries first so decay works better
			{
				EffectItem free = (EffectItem)m_Free[i];

				m_Free.RemoveAt( i );

				if ( !free.Deleted && free.Map == Map.Internal )
					item = free;
			}

			if ( item == null )
				item = new EffectItem();
			else
				item.ItemID = 1;

			item.MoveToWorld( p, map );
			item.BeginFree( duration );

			return item;
		}

		private EffectItem() : base( 1 ) // nodraw
		{
			Movable = false;
		}

		public void BeginFree( TimeSpan duration )
		{
			new FreeTimer( this, duration ).Start();
		}

		public override bool Decays
		{
			get
			{
				return true;
			}
		}

		public EffectItem( Serial serial ) : base( serial )
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

			Delete();
		}

		private class FreeTimer : Timer
		{
			private Item m_Item;

			public FreeTimer( Item item, TimeSpan delay ) : base( delay )
			{
				m_Item = item;
				Priority = TimerPriority.OneSecond;
			}

			protected override void OnTick()
			{
				m_Item.Internalize();

				m_Free.Add( m_Item );
			}
		}
	}
}�