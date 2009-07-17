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
/* /Scripts/items/sheilds/baseshield.cs
 *  04/30/05, Kit
 *	Added meditation allowance none to allow passive mana regen with sheilds equiped
 */

using System;
using System.Collections;
using Server;
using Server.Network;

namespace Server.Items
{
	public class BaseShield : BaseArmor
	{
		public override ArmorMaterialType MaterialType{ get{ return ArmorMaterialType.Plate; } }
		public override ArmorMeditationAllowance DefMedAllowance{ get{ return ArmorMeditationAllowance.All; } } 

		public BaseShield( int itemID ) : base( itemID )
		{
		}

		public BaseShield( Serial serial ) : base(serial)
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int)0 );//version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}

		public override double ArmorRating
		{
			get
			{
				Mobile m = this.Parent as Mobile;
				double ar = base.ArmorRating;

				if ( m != null )
					return ( ( m.Skills[SkillName.Parry].Value * ar ) / 200.0 ) + 1.0;
				else
					return ar;
			}
		}

		public override int OnHit( BaseWeapon weapon, int damage )
		{
			Mobile owner = this.Parent as Mobile;
			if ( owner == null )
				return damage;
			
			double ar = this.ArmorRating;
			double chance = ( owner.Skills[SkillName.Parry].Value - ( ar * 2.0 ) ) / 100.0;

			if ( chance < 0.01 )
				chance = 0.01;
			/*
			FORMULA: Displayed AR = ((Parrying Skill * Base AR of Shield) � 200) + 1 

			FORMULA: % Chance of Blocking = parry skill - (shieldAR * 2)

			FORMULA: Melee Damage Absorbed = (AR of Shield) / 2 | Archery Damage Absorbed = AR of Shield 
			*/
			if ( owner.CheckSkill( SkillName.Parry, chance ) )
			{
				if ( weapon.Skill == SkillName.Archery )
					damage -= (int)ar;
				else 
					damage -= (int)(ar / 2.0);

				if ( damage < 0 )
					damage = 0;

				owner.FixedEffect( 0x37B9, 10, 16 );

				if ( Utility.Random( 3 ) == 0 )
					HitPoints -= Utility.Random( 2 );
			}

			return damage;
		}
	}
}