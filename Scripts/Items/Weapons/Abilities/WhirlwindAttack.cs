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

/* /sandbox/ai/Scripts/Items/Weapons/Abilities/WhirlwindAttack.cs
 *	ChangeLog :
 *	07/23/08, weaver
 *		Automated IPooledEnumerable optimizations. 1 loops updated.
 */



using System;
using System.Collections;
using Server;
using Server.Spells;
using Server.Engines.PartySystem;

namespace Server.Items
{
	/// <summary>
	/// A godsend to a warrior surrounded, the Whirlwind Attack allows the fighter to strike at all nearby targets in one mighty spinning swing.
	/// </summary>
	public class WhirlwindAttack : WeaponAbility
	{
		public WhirlwindAttack()
		{
		}

		public override int BaseMana{ get{ return 15; } }

		public override void OnHit( Mobile attacker, Mobile defender, int damage )
		{
			if ( !Validate( attacker )  )
				return;

			ClearCurrentAbility( attacker );

			Map map = attacker.Map;

			if ( map == null )
				return;

			BaseWeapon weapon = attacker.Weapon as BaseWeapon;

			if ( weapon == null )
				return;

			if ( !CheckMana( attacker, true ) )
				return;

			attacker.FixedEffect( 0x3728, 10, 15 );
			attacker.PlaySound( 0x2A1 );

			ArrayList list = new ArrayList();

			IPooledEnumerable eable = attacker.GetMobilesInRange( 1 );
			foreach ( Mobile m in eable)
				list.Add( m );
			eable.Free();

			Party p = Party.Get( attacker );

			for ( int i = 0; i < list.Count; ++i )
			{
				Mobile m = (Mobile)list[i];

				if ( m != defender && m != attacker && SpellHelper.ValidIndirectTarget( attacker, m ) && (p == null || !p.Contains( m )) )
				{
					if ( m == null || m.Deleted || attacker.Deleted || m.Map != attacker.Map || !m.Alive || !attacker.Alive || !attacker.CanSee( m ) )
						continue;

					if ( !attacker.InRange( m, weapon.MaxRange ) )
						continue;

					if ( attacker.InLOS( m ) )
					{
						attacker.RevealingAction();

						attacker.SendLocalizedMessage( 1060161 ); // The whirling attack strikes a target!
						m.SendLocalizedMessage( 1060162 ); // You are struck by the whirling attack and take damage!

						weapon.OnHit( attacker, m );
					}
				}
			}
		}
	}
}
