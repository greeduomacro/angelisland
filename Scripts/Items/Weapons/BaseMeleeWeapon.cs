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
using Server;
using Server.Items;
using Server.Engines.Harvest;

namespace Server.Items
{
	public abstract class BaseMeleeWeapon : BaseWeapon
	{
		public BaseMeleeWeapon( int itemID ) : base( itemID )
		{
		}

		public BaseMeleeWeapon( Serial serial ) : base( serial )
		{
		}

		public override int AbsorbDamage( Mobile attacker, Mobile defender, int damage )
		{
			damage = base.AbsorbDamage( attacker, defender, damage );

			if ( Core.AOS )
				return damage;

			int absorb = defender.MeleeDamageAbsorb;

			if ( absorb > 0 )
			{
				if ( absorb > damage )
				{
					int react = damage / 5;

					if ( react <= 0 )
						react = 1;

					defender.MeleeDamageAbsorb -= damage;
					damage = 0;

					attacker.Damage( react, defender );

					attacker.PlaySound( 0x1F1 );
					attacker.FixedEffect( 0x374A, 10, 16 );
				}
				else
				{
					defender.MeleeDamageAbsorb = 0;
					defender.SendLocalizedMessage( 1005556 ); // Your reactive armor spell has been nullified.
					DefensiveSpell.Nullify( defender );
				}
			}

			return damage;
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
		}
	}
}�