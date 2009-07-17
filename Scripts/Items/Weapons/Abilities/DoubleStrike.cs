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
	/// The highly skilled warrior can use this special attack to make two quick swings in succession.
	/// Landing both blows would be devastating! 
	/// </summary>
	public class DoubleStrike : WeaponAbility
	{
		public DoubleStrike()
		{
		}

		public override int BaseMana{ get{ return 30; } }
		public override double DamageScalar{ get{ return 0.9; } }

		public override void OnHit( Mobile attacker, Mobile defender, int damage )
		{
			if ( !Validate( attacker ) || !CheckMana( attacker, true ) )
				return;

			ClearCurrentAbility( attacker );

			attacker.SendLocalizedMessage( 1060084 ); // You attack with lightning speed!
			defender.SendLocalizedMessage( 1060085 ); // Your attacker strikes with lightning speed!

			defender.PlaySound( 0x3BB );
			defender.FixedEffect( 0x37B9, 244, 25 );

			// Swing again:

			// If no combatant, wrong map, one of us is a ghost, or cannot see, or deleted, then stop combat
			if ( defender == null || defender.Deleted || attacker.Deleted || defender.Map != attacker.Map || !defender.Alive || !attacker.Alive || !attacker.CanSee( defender ) )
			{
				attacker.Combatant = null;
				return;
			}

			IWeapon weapon = attacker.Weapon;

			if ( weapon == null )
				return;

			if ( !attacker.InRange( defender, weapon.MaxRange ) )
				return;

			if ( attacker.InLOS( defender ) )
			{
				BaseWeapon.InDoubleStrike = true;
				attacker.RevealingAction();
				attacker.NextCombatTime = DateTime.Now + weapon.OnSwing( attacker, defender );
				BaseWeapon.InDoubleStrike = false;
			}
		}
	}
} 
