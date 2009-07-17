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

/* CHANGELOG
 * 08/27/05, Taran Kain
 *		Added new constructor to specify the lifespan of the blood object.
 */

using System;
using Server;

namespace Server.Items
{
	public class Blood : Item
	{
		[Constructable]
		public Blood() : this( 0x1645 )
		{
		}

		[Constructable]
		public Blood( int itemID ) : this ( itemID, 3.0 + (Utility.RandomDouble() * 3.0 ) )
		{
		}

		[Constructable]
		public Blood( int itemID, double lifespan ) : base( itemID )
		{
			Movable = false;

			new InternalTimer( this, TimeSpan.FromSeconds(lifespan) ).Start();
		}

		public Blood( Serial serial ) : base( serial )
		{
			new InternalTimer( this, TimeSpan.FromSeconds( 3.0 + (Utility.RandomDouble() * 3.0 ) ) ).Start();
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

		private class InternalTimer : Timer
		{
			private Item m_Blood;

			public InternalTimer( Item blood, TimeSpan lifespan ) : base( lifespan )
			{
				Priority = TimerPriority.FiftyMS;

				m_Blood = blood;
			}

			protected override void OnTick()
			{
				m_Blood.Delete();
			}
		}
	}
}