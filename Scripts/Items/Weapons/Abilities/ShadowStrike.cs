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

namespace Server.Items
{
	/// <summary>
	/// This powerful ability requires secondary skills to activate.
	/// Successful use of Shadowstrike deals extra damage to the target � and renders the attacker invisible!
	/// Only those who are adept at the art of stealth will be able to use this ability.
	/// </summary>
	public class ShadowStrike : WeaponAbility
	{
		public ShadowStrike()
		{
		}

		public override int BaseMana{ get{ return 20; } }
		public override double DamageScalar{ get{ return 1.25; } }

		public override bool CheckSkills( Mobile from )
		{
			if ( !base.CheckSkills( from ) )
				return false;

			Skill skill = from.Skills[SkillName.Stealth];

			if ( skill != null && skill.Value >= 80.0 )
				return true;

			from.SendLocalizedMessage( 1060183 ); // You lack the required stealth to perform that attack

			return false;
		}

		public override void OnHit( Mobile attacker, Mobile defender, int damage )
		{
			if ( !Validate( attacker ) || !CheckMana( attacker, true ) )
				return;

			ClearCurrentAbility( attacker );

			attacker.SendLocalizedMessage( 1060078 ); // You strike and hide in the shadows!
			defender.SendLocalizedMessage( 1060079 ); // You are dazed by the attack and your attacker vanishes!

			Effects.SendLocationParticles( EffectItem.Create( attacker.Location, attacker.Map, EffectItem.DefaultDuration ), 0x376A, 8, 12, 9943 );
			attacker.PlaySound( 0x482 );

			defender.FixedEffect( 0x37BE, 20, 25 );

			attacker.Combatant = null;
			attacker.Warmode = false;
			attacker.Hidden = true;
		}
	}
}