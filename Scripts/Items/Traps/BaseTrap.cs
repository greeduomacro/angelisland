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

/* Items/Traps/BaseTrap.cs
 * CHANGELOG:
 *	2/21/06, Adam
 *		Make a check to see that something is a BaseCreature before
 *		casting it to a BaseCreature. This is *rarely* needed, but there is 
 *		at least one 'mobile' that runs around ans is not a BaseCreature.
 *		See: Wanderer.cs
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 *  01/05/05, Darva
 *  		Caused traps to only attack players, tamed mobs, and controlled mobs.
 */

using System;
using Server.Mobiles;

namespace Server.Items
{
	public abstract class BaseTrap : Item
	{
		public virtual bool PassivelyTriggered{ get{ return false; } }
		public virtual TimeSpan PassiveTriggerDelay{ get{ return TimeSpan.Zero; } }
		public virtual int PassiveTriggerRange{ get{ return -1; } }
		public virtual TimeSpan ResetDelay{ get{ return TimeSpan.Zero; } }

		private DateTime m_NextPassiveTrigger, m_NextActiveTrigger;

		public virtual void OnTrigger( Mobile from )
		{
		}

		public override bool HandlesOnMovement{ get{ return true; } } // Tell the core that we implement OnMovement

		public virtual int GetEffectHue()
		{
			int hue = this.Hue & 0x3FFF;

			if ( hue < 2 )
				return 0;

			return hue - 1;
		}

		public bool CheckRange( Point3D loc, Point3D oldLoc, int range )
		{
			return CheckRange( loc, range ) && !CheckRange( oldLoc, range );
		}

		public bool CheckRange( Point3D loc, int range )
		{
			return ( (this.Z + 8) >= loc.Z && (loc.Z + 16) > this.Z )
				&& Utility.InRange( GetWorldLocation(), loc, range );
		}

		public override void OnMovement( Mobile m, Point3D oldLocation )
		{
			base.OnMovement( m, oldLocation );

			if ( m.Location == oldLocation )
				return;

			if ( CheckRange( m.Location, oldLocation, 0 ) && DateTime.Now >= m_NextActiveTrigger )
			{
				m_NextActiveTrigger = m_NextPassiveTrigger = DateTime.Now + ResetDelay;
				if ( m is PlayerMobile  || (m is BaseCreature && ((BaseCreature)m).Controlled == true) )
					OnTrigger( m );
			}
			else if ( PassivelyTriggered && CheckRange( m.Location, oldLocation, PassiveTriggerRange ) && DateTime.Now >= m_NextPassiveTrigger )
			{
				m_NextPassiveTrigger = DateTime.Now + PassiveTriggerDelay;
				if (m is PlayerMobile || (m is BaseCreature && ((BaseCreature)m).Controlled == true) )
					OnTrigger( m );
			}
		}

		public BaseTrap( int itemID ) : base( itemID )
		{
			Movable = false;
		}

		public BaseTrap( Serial serial ) : base( serial )
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
}