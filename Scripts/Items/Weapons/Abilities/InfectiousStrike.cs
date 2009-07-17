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
	/// This special move represents a significant change to the use of poisons in Age of Shadows.
	/// Now, only certain weapon types � those that have Infectious Strike as an available special move � will be able to be poisoned.
	/// Targets will no longer be poisoned at random when hit by poisoned weapons.
	/// Instead, the wielder must use this ability to deliver the venom.
	/// While no skill in Poisoning is directly required to use this ability, being knowledgeable in the application and use of toxins
	/// will allow a character to use Infectious Strike at reduced mana cost and with a chance to inflict more deadly poison on his victim.
	/// With this change, weapons will no longer be corroded by poison.
	/// Level 5 poison will be possible when using this special move.
	/// </summary>
	public class InfectiousStrike : WeaponAbility
	{
		public InfectiousStrike()
		{
		}

		public override int BaseMana{ get{ return 15; } }

		public override void OnHit( Mobile attacker, Mobile defender, int damage )
		{
			if ( !Validate( attacker ) )
				return;

			ClearCurrentAbility( attacker );

			BaseWeapon weapon = attacker.Weapon as BaseWeapon;

			if ( weapon == null )
				return;

			Poison p = weapon.Poison;

			if ( p == null || weapon.PoisonCharges <= 0 )
			{
				attacker.SendLocalizedMessage( 1061141 ); // Your weapon must have a dose of poison to perform an infectious strike!
				return;
			}

			if ( !CheckMana( attacker, true ) )
				return;

			--weapon.PoisonCharges;

			if ( (attacker.Skills[SkillName.Poisoning].Value / 100.0) > Utility.RandomDouble() )
			{
				int level = p.Level + 1;
				Poison newPoison = Poison.GetPoison( level );

				if ( newPoison != null )
				{
					p = newPoison;

					attacker.SendLocalizedMessage( 1060080 ); // Your precise strike has increased the level of the poison by 1
					defender.SendLocalizedMessage( 1060081 ); // The poison seems extra effective!
				}
			}

			defender.PlaySound( 0xDD );
			defender.FixedParticles( 0x3728, 244, 25, 9941, 1266, 0, EffectLayer.Waist );

			if ( defender.ApplyPoison( attacker, p ) != ApplyPoisonResult.Immune )
			{
				attacker.SendLocalizedMessage( 1008096, true, defender.Name ); // You have poisoned your target : 
				defender.SendLocalizedMessage( 1008097, false, attacker.Name ); //  : poisoned you!
			}
		}
	}
}