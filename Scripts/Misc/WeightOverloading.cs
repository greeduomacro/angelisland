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

/* Scripts/Misc/WeightOverloading.cs
 * ChangeLog
 *	6/28/04, Pix
 *		Tweaked to only drain low-on-stamina people when they're running.
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */

using System;
using Server;
using Server.Mobiles;

namespace Server.Misc
{
	public enum DFAlgorithm
	{
		Standard,
		PainSpike
	}

	public class WeightOverloading
	{
		public static void Initialize()
		{
			EventSink.Movement += new MovementEventHandler( EventSink_Movement );
		}

		private static DFAlgorithm m_DFA;

		public static DFAlgorithm DFA
		{
			get{ return m_DFA; }
			set{ m_DFA = value; }
		}

		public static void FatigueOnDamage( Mobile m, int damage )
		{
			double fatigue = 0.0;

			switch ( m_DFA )
			{
				case DFAlgorithm.Standard:
				{
					fatigue = (damage * (100.0 / m.Hits) * ((double)m.Stam / 100)) - 5.0;
					break;
				}
				case DFAlgorithm.PainSpike:
				{
					fatigue = (damage * ((100.0 / m.Hits) + ((50.0 + m.Stam) / 100) - 1.0)) - 5.0;
					break;
				}
			}

			if ( fatigue > 0 )
				m.Stam -= (int)fatigue;
		}

		public const int OverloadAllowance = 4; // We can be four stones overweight without getting fatigued

		public static int GetMaxWeight( Mobile m )
		{
			return 40 + (int)(3.5 * m.Str);
		}

		public static void EventSink_Movement( MovementEventArgs e )
		{
			Mobile from = e.Mobile;

			if ( !from.Player || !from.Alive || from.AccessLevel >= AccessLevel.GameMaster )
				return;

			int maxWeight = GetMaxWeight( from ) + OverloadAllowance;
			int overWeight = (Mobile.BodyWeight + from.TotalWeight) - maxWeight;

			if ( overWeight > 0 )
			{
				from.Stam -= GetStamLoss( from, overWeight, (e.Direction & Direction.Running) != 0 );

				if ( from.Stam == 0 )
				{
					from.SendLocalizedMessage( 500109 ); // You are too fatigued to move, because you are carrying too much weight!
					e.Blocked = true;
					return;
				}
			}

			if( (e.Direction & Direction.Running) != 0 ) //if you're running
			{
				if ( ((from.Stam * 100) / Math.Max( from.StamMax, 1 )) < 10 )
					--from.Stam;
			}

			if ( from.Stam == 0 )
			{
				from.SendLocalizedMessage( 500110 ); // You are too fatigued to move.
				e.Blocked = true;
				return;
			}

			if ( from is PlayerMobile )
			{
				int amt = ( from.Mounted ? 48 : 16 );
				PlayerMobile pm = (PlayerMobile)from;

				if ( (++pm.StepsTaken % amt) == 0 )
					--from.Stam;
			}
		}

		public static int GetStamLoss( Mobile from, int overWeight, bool running )
		{
			int loss = 5 + (overWeight / 25);

			if ( from.Mounted )
				loss /= 3;

			if ( running )
				loss *= 2;

			return loss;
		}

		public static bool IsOverloaded( Mobile m )
		{
			if ( !m.Player || !m.Alive || m.AccessLevel >= AccessLevel.GameMaster )
				return false;

			return ( (Mobile.BodyWeight + m.TotalWeight) > (GetMaxWeight( m ) + OverloadAllowance) );
		}
	}
}