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

namespace Server.Mobiles
{
	public class Wanderer : Mobile
	{
		private Timer m_Timer;

		[Constructable]
		public Wanderer()
		{
			this.Name = "Me";
			this.Body = 0x1;
			this.AccessLevel = AccessLevel.GameMaster;

			m_Timer = new InternalTimer( this );
			m_Timer.Start();
		}

		public Wanderer( Serial serial ) : base( serial )
		{
			m_Timer = new InternalTimer( this );
			m_Timer.Start();
		}

		public override void OnDelete()
		{
			m_Timer.Stop();

			base.OnDelete();
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
			private Wanderer m_Owner;
			private int m_Count = 0;

			public InternalTimer( Wanderer owner ) : base( TimeSpan.FromSeconds( 0.1 ), TimeSpan.FromSeconds( 0.1 ) )
			{
				m_Owner = owner;
			}

			protected override void OnTick()
			{
				if ( (m_Count++ & 0x3) == 0 )
				{
					m_Owner.Direction = (Direction)(Utility.Random( 8 ) | 0x80);
				}

				m_Owner.Move( m_Owner.Direction );
			}
		}
	}
} 
