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

/* Items/Skill Items/Camping/Campfire.cs
 * CHANGELOG:
 *  11/21/06, Plasma
 *      Changed ctor to not start secure timer if in a no camp zone
 *	11/6/06, Adam
 *		Remove debug output: Console.WriteLine("Region is a house region");
 *		*shakes fist at weaver*
 *	08/14/06, weaver
 *		Changed camping to assume you have a bedroll handy if you're in a tent.
 *	10/22/04, Pix
 *		Changed camping to not use campfireregion.
 *	9/11/04, Pixie
 *		Added OwnerUsedBedroll so that we can ensure the owner used the bedroll and log him/her
 *		out properly.
 *	7/25/04, Pixie
 *		Made sure that the region passed into new CampfireRegion isn't another CampfireRegion.
 *	7/15/04, Pixie
 *		Made it so spells and lightlevels use behavior for the region that the campfire is created in
 *	5/10/04, Pixie
 *		Initial working revision
 */

using System;
using System.Collections;
using Server;
using Server.Items;
using Server.Gumps;
using Server.Mobiles;
using Server.Multis;
using Server.Regions;
using Server.Network;
using Server.Targeting;
using Server.Accounting;

namespace Server.Items
{
	public class Campfire : Item
	{
		private Mobile m_Owner;
		private bool m_campSecure;
		private Timer m_campSecureTimer;
		private bool m_firstMsgSent;

		public bool OwnerUsedBedroll = false;

		private Timer m_Timer;

		[Constructable]
		public Campfire( Mobile owner ) : base ( 0xDE3 )
		{
			Movable = false;
			Light = LightType.Circle300;
			m_Timer = new DecayTimer( this );
			m_Timer.Start();

			m_Owner = owner;
			m_Owner.Location = new Point3D( m_Owner.X, m_Owner.Y, m_Owner.Z );

			CampSecure = false;
			FirstMessageSent = true;

            //pla : if within a rectricted camping zone then don't start the timer
            if (CampHelper.InRestrictedArea(owner))
            {
                owner.SendMessage("You do not consider it safe to secure a camp here");
            }
            else
            {
                m_campSecureTimer = new SecureTimer(m_Owner, this);
                m_campSecureTimer.Start();
            }
		}

		public override void OnDelete()
		{
			base.OnDelete();
		}

		public Campfire( Serial serial ) : base( serial )
		{
			m_Timer = new DecayTimer( this );
			m_Timer.Start();
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

		private class DecayTimer : Timer
		{
			private Campfire m_Owner;

			public DecayTimer( Campfire owner ) : base( TimeSpan.FromMinutes( 2.0 ) )
			{
				Priority = TimerPriority.FiveSeconds;

				m_Owner = owner;
			}

			protected override void OnTick()
			{
				m_Owner.Delete();
			}
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public Mobile Owner
		{
			get
			{
				return m_Owner;
			}
			set
			{
				m_Owner = value;
			}
		}

		public bool CampSecure
		{
			get { return m_campSecure; }
			set { m_campSecure = value; }
		}

		public bool FirstMessageSent
		{
			get { return m_firstMsgSent; }
			set { m_firstMsgSent = value; }
		}

		public Mobile Camper
		{
			get { return m_Owner; }
		}

		public void RestartSecureTimer()
		{
			if( !FirstMessageSent )
			{
				FirstMessageSent = true;
				Camper.SendLocalizedMessage( 500620 );
				m_campSecureTimer.Start();
			}
		}
		public void StopSecureTimer()
		{
			FirstMessageSent = false;
			m_campSecureTimer.Stop();
		}

		private class SecureTimer : Timer
		{
			private Mobile m_Owner;
			private Campfire m_campfire;

			public SecureTimer( Mobile owner , Campfire campfire) : base( TimeSpan.FromSeconds( 30.0 ) )
			{
				Priority = TimerPriority.FiveSeconds;
				m_campfire = campfire;
				m_Owner = owner;
			}

			protected override void OnTick()
			{
				m_Owner.SendLocalizedMessage( 500621 );
				m_campfire.CampSecure = true;

				// wea: 14/Aug/2006 if we're in a tent, we also have a bedroll handy!!
				if( m_Owner.Region != null )
				{
					if( m_Owner.Region is HouseRegion )
					{
						//Console.WriteLine("Region is a house region");
						if( ((HouseRegion) m_Owner.Region).House is Tent ||
							((HouseRegion) m_Owner.Region).House is SiegeTent )
						{
							m_campfire.OwnerUsedBedroll = true;
							m_Owner.SendGump( new CampLogoutGump( m_Owner, null ) );
						}
					}
				}
			}
		}
	}
}
