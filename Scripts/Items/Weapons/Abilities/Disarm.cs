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

/* Items/Weapons/Abilties/Disarm.cs
 * CHANGELOG:
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */

using System;

namespace Server.Items
{
	/// <summary>
	/// This attack allows you to disarm your foe.
	/// Now in Age of Shadows, a successful Disarm leaves the victim unable to re-arm another weapon for several seconds.
	/// </summary>
	public class Disarm : WeaponAbility
	{
		public Disarm()
		{
		}

		public override int BaseMana{ get{ return 20; } }

		// No longer active in pub21:
		//BUT WE want it on Angel Island
		public override bool CheckSkills( Mobile from )
		{
			if ( !base.CheckSkills( from ) )
				return false;

			if ( !(from.Weapon is Fists) )
				return true;

			Skill skill = from.Skills[SkillName.ArmsLore];

			if ( skill != null && skill.Base >= 80.0 )
				return true;

			from.SendLocalizedMessage( 1061812 ); // You lack the required skill in armslore to perform that attack!

			return false;
		}

		public static readonly TimeSpan BlockEquipDuration = TimeSpan.FromSeconds( 5.0 );

		public override void OnHit( Mobile attacker, Mobile defender, int damage )
		{
			if ( !Validate( attacker ) )
				return;

			ClearCurrentAbility( attacker );

			Item toDisarm = defender.FindItemOnLayer( Layer.OneHanded );

			if ( toDisarm == null || !toDisarm.Movable )
				toDisarm = defender.FindItemOnLayer( Layer.TwoHanded );

			Container pack = defender.Backpack;

			if ( pack == null || (toDisarm != null && !toDisarm.Movable) )
			{
				attacker.SendLocalizedMessage( 1004001 ); // You cannot disarm your opponent.
			}
			else if ( toDisarm == null || toDisarm is BaseShield )
			{
				attacker.SendLocalizedMessage( 1060849 ); // Your target is already unarmed!
			}
			else if ( CheckMana( attacker, true ) )
			{
				attacker.SendLocalizedMessage( 1060092 ); // You disarm their weapon!
				defender.SendLocalizedMessage( 1060093 ); // Your weapon has been disarmed!

				defender.PlaySound( 0x3B9 );
				defender.FixedParticles( 0x37BE, 232, 25, 9948, EffectLayer.LeftHand );

				pack.DropItem( toDisarm );

				BaseWeapon.BlockEquip( defender, BlockEquipDuration );
			}
		}
	}
}