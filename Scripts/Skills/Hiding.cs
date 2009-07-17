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


/* /sandbox/ai/Scripts/Skills/Hiding.cs
 *	ChangeLog :
 *	07/23/08, weaver
 *		Automated IPooledEnumerable optimizations. 1 loops updated.
 */


using System;
using Server.Targeting;
using Server.Items;
using Server.Network;
using Server.Mobiles;
using Server.Multis;

namespace Server.SkillHandlers
{
	public class Hiding
	{
		public static void Initialize()
		{
			SkillInfo.Table[21].Callback = new SkillUseCallback( OnUse );
		}

		public static TimeSpan OnUse( Mobile m )
		{
			if ( m.Target != null || m.Spell != null )
			{
				m.SendLocalizedMessage( 501238 ); // You are busy doing something else and cannot hide.
				return TimeSpan.FromSeconds( 1.0 );
			}


			double bonus = 0.0;

			BaseHouse house = BaseHouse.FindHouseAt( m );

			if ( house != null && house.IsFriend( m ) )
			{
				bonus = 100.0;
			}
			else if ( !Core.AOS )
			{
				if ( house == null )
					house = BaseHouse.FindHouseAt( new Point3D( m.X - 1, m.Y, 127 ), m.Map, 16 );

				if ( house == null )
					house = BaseHouse.FindHouseAt( new Point3D( m.X + 1, m.Y, 127 ), m.Map, 16 );

				if ( house == null )
					house = BaseHouse.FindHouseAt( new Point3D( m.X, m.Y - 1, 127 ), m.Map, 16 );

				if ( house == null )
					house = BaseHouse.FindHouseAt( new Point3D( m.X, m.Y + 1, 127 ), m.Map, 16 );

				if ( house != null )
					bonus = 50.0;
			}

			int range = 18 - (int)(m.Skills[SkillName.Hiding].Value / 10);

			bool badCombat = ( m.Combatant != null && m.InRange( m.Combatant.Location, range ) && m.Combatant.InLOS( m ) );
			bool ok = ( !badCombat /*&& m.CheckSkill( SkillName.Hiding, 0.0 - bonus, 100.0 - bonus )*/ );

			if ( ok )
			{
				IPooledEnumerable eable = m.GetMobilesInRange( range );
				foreach ( Mobile check in eable)
				{
					if ( check.InLOS( m ) && check.Combatant == m )
					{
						badCombat = true;
						ok = false;
						break;
					}
				}
				eable.Free();

				ok = ( !badCombat && m.CheckSkill( SkillName.Hiding, 0.0 - bonus, 100.0 - bonus ) );
			}

			if ( badCombat )
			{
				m.RevealingAction();

				m.LocalOverheadMessage( MessageType.Regular, 0x22, 501237 ); // You can't seem to hide right now.

				return TimeSpan.FromSeconds( 1.0 );
			}
			else 
			{
				if ( ok )
				{
					m.Hidden = true;

					m.LocalOverheadMessage( MessageType.Regular, 0x1F4, 501240 ); // You have hidden yourself well.
				}
				else
				{
					m.RevealingAction();

					m.LocalOverheadMessage( MessageType.Regular, 0x22, 501241 ); // You can't seem to hide here.
				}

				return TimeSpan.FromSeconds( 10.0 );
			}
		}
	}
}
