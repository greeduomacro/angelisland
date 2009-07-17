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

/* Skills/RemoveTrap.cs
 * CHANGELOG:
 *  11/23/06, Plasma
 *       Modified to only apply skill delay OnTarget()
 *  8/8/04, Pixie
 *		Added functionality for tripping the trap if you fail to disarm it.
 *  7/10/04, Old Salty
 * 		Removed debug message for remove trap unless you are GM or higher accesslevel.
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 *	5/20/2004, Pixie
 *		Changed so that someone with removetrap+detecthidden > 150 
 *		can disarm a level 5 treasure map.
 */

using System;
using Server.Targeting;
using Server.Items;
using Server.Network;

namespace Server.SkillHandlers
{
	public class RemoveTrap
	{
		public static void Initialize()
		{
			SkillInfo.Table[(int)SkillName.RemoveTrap].Callback = new SkillUseCallback( OnUse );
		}

		public static TimeSpan OnUse( Mobile m )
		{
			if ( m.Skills[SkillName.Lockpicking].Value < 50 )
			{
				m.SendLocalizedMessage( 502366 ); // You do not know enough about locks.  Become better at picking locks.
			}
			else if ( m.Skills[SkillName.DetectHidden].Value < 50 )
			{
				m.SendLocalizedMessage( 502367 ); // You are not perceptive enough.  Become better at detect hidden.
			}
			else
			{
				m.Target = new InternalTarget();

				m.SendLocalizedMessage( 502368 ); // Wich trap will you attempt to disarm?
			}

			return TimeSpan.FromSeconds( 1.0 ); // pla: changed this to 1 second.  10 seconds applied OnTarget()
		}

		private class InternalTarget : Target
		{
			public InternalTarget() :  base ( 2, false, TargetFlags.None )
			{
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				if ( targeted is Mobile )
				{
					from.SendLocalizedMessage( 502816 ); // You feel that such an action would be inappropriate
				}
				else if ( targeted is TrapableContainer )
				{
					TrapableContainer targ = (TrapableContainer)targeted;

					from.Direction = from.GetDirectionTo( targ );

					if ( targ.TrapType == TrapType.None )
					{
						from.SendLocalizedMessage( 502373 ); // That doesn't appear to be trapped
						return;
					}

					from.PlaySound( 0x241 );

					double minskill = targ.TrapPower;
					//modify minskill with detect hidden skill
					if( from.Skills[SkillName.DetectHidden].Base > 50.0 )
					{
						minskill -= (from.Skills[SkillName.DetectHidden].Base - 50.0);
					}
					double maxskill = minskill + 50;

					//What this means is that with a trappower of 125 (level 5 tmap) and
					//GM DH, min RT skill to unlock is 75 and max is 100, OR, rather,
					//DH and RT need to be above a combined 175 to have a chance at disarming
					//the trap.
					//Level 4 power (100), they need to have above 150 total to have a chance.
					
					if ( from.AccessLevel >= AccessLevel.GameMaster )    //This line added by Old Salty
					from.SendMessage( string.Format("minskill is {0}, maxskill is {1}", minskill, maxskill) );
					
                    if ( from.CheckTargetSkill( 
							SkillName.RemoveTrap, 
							targ, 
							minskill, 
							maxskill ) )
					{
						targ.TrapPower = 0;
						targ.TrapType = TrapType.None;
						from.SendLocalizedMessage( 502377 ); // You successfully render the trap harmless
					}
					else
					{
						//chance to set off trap...
						if( targ.OnFailDisarm(from) )
						{
						}
						else
						{
							from.SendLocalizedMessage( 502372 ); // You fail to disarm the trap... but you don't set it off
						}
					}

                    //pla: set 10 second delay before next skill use
                    from.NextSkillTime = DateTime.Now + TimeSpan.FromSeconds(10.0);
                     
				}
				else
				{
					from.SendLocalizedMessage( 502373 ); // That does'nt appear to be trapped                    
				}
			}
		}
	}
}
