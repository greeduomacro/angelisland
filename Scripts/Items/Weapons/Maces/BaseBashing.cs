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

/*
	ChangeLog:
	2/17/05 - Pix
		Fixed another cast!
	2/16/05 - Pix
		Fixed BaseArmor cast.
	4/22/04 Changes by smerX
		Added Armor.HitPoints damage
*/


using System;
using Server;
using Server.Items;

namespace Server.Items
{
	public abstract class BaseBashing : BaseMeleeWeapon
	{
		public override int DefHitSound{ get{ return 0x233; } }
		public override int DefMissSound{ get{ return 0x239; } }

		public override SkillName DefSkill{ get{ return SkillName.Macing; } }
		public override WeaponType DefType{ get{ return WeaponType.Bashing; } }
		public override WeaponAnimation DefAnimation{ get{ return WeaponAnimation.Bash1H; } }

		public BaseBashing( int itemID ) : base( itemID )
		{
		}

		public BaseBashing( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}

		public override void OnHit( Mobile attacker, Mobile defender )
		{
			base.OnHit( attacker, defender );

			defender.Stam -= Utility.Random( 2, 4 ); // 2-4 points of stamina loss

			BaseArmor shield = defender.ShieldArmor as BaseArmor;

			if ( shield != null )
			{
				DamageArmor( attacker, defender, shield, Utility.Random( 2, 4 ) );
			}
			else
			{
				BaseArmor armor = SelectArmor( defender );

				if ( armor != null )
					DamageArmor( attacker, defender, armor, Utility.Random( 2, 4 ) );
			}
		}


		private BaseArmor SelectArmor( Mobile m )
		{
			BaseArmor arms = m.ArmsArmor as BaseArmor;
			BaseArmor chest = m.ChestArmor as BaseArmor;
			BaseArmor legs = m.LegsArmor as BaseArmor;
			BaseArmor neck = m.NeckArmor as BaseArmor;
			BaseArmor head = m.HeadArmor as BaseArmor;
			BaseArmor hands = m.HandArmor as BaseArmor;
			
			int w = Utility.Random( 1, 7 );
			
			if ( w == 1 )
				return arms;
			else if ( w == 2 || w == 3 )
				return chest;
			else if ( w == 4 )
				return legs;
			else if ( w == 5 )
				return neck;
			else if ( w == 6 )
				return head;
			else if ( w == 7 )
				return hands;
				
			return null;			
		
		}



		private void DamageArmor( Mobile attacker, Mobile defender, BaseArmor ar, int amount )
		{
		
		
			if ( ar != null ) //&& !!!Utility.RandomBool() )
			{
				if ( ar.HitPoints >= amount )
				{
					ar.HitPoints -= amount;
				}
				else
				{
					if ( defender.Player )
						defender.SendMessage( "{0} has destroyed a piece of your armor!", attacker.Name );
						
					ar.HitPoints = 0;
				}
		
			}
			
		}


/*		public override double GetBaseDamage( Mobile attacker )
		{
			double damage = base.GetBaseDamage( attacker );

			if ( !Core.AOS && (attacker.Player || attacker.Body.IsHuman) && Layer == Layer.TwoHanded && (attacker.Skills[SkillName.Anatomy].Value / 400.0) >= Utility.RandomDouble() )
			{
				damage *= 1.5;

				attacker.SendMessage( "You deliver a crushing blow!" ); // Is this not localized?
				attacker.PlaySound( 0x11C );
			}

			return damage;
		}
*/	}
}