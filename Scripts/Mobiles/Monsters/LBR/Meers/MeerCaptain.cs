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

/* Scripts/Mobiles/Monsters/LBR/Meers/MeerCaptain.cs
 * ChangeLog
 *	07/23/08, weaver
 *		Automated IPooledEnumerable optimizations. 1 loops updated.
 *  8/16/06, Rhiannon
 *		Changed speed settings to match SpeedInfo table.
 *	7/26/05, erlein
 *		Automated removal of AoS resistance related function calls. 6 lines removed.
 *	6/11/04, mith
 *		Moved the equippable items out of OnBeforeDeath()
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */

using System;
using System.Collections;
using Server;
using Server.Misc;
using Server.Items;
using Server.Spells;

namespace Server.Mobiles
{
	[CorpseName( "a meer corpse" )]
	public class MeerCaptain : BaseCreature
	{
		[Constructable]
		public MeerCaptain() : base( AIType.AI_Archer, FightMode.Aggressor | FightMode.Evil, 10, 1, 0.25, 0.5 )
		{
			Name = "a meer captain";
			Body = 773;
			BardImmune = true;

			SetStr( 96, 110 );
			SetDex( 186, 200 );
			SetInt( 96, 110 );

			SetHits( 58, 66 );

			SetDamage( 5, 15 );



			SetSkill( SkillName.Archery, 90.1, 100.0 );
			SetSkill( SkillName.MagicResist, 91.0, 100.0 );
			SetSkill( SkillName.Swords, 90.1, 100.0 );
			SetSkill( SkillName.Tactics, 91.0, 100.0 );
			SetSkill( SkillName.Wrestling, 80.9, 89.9 );

			Fame = 2000;
			Karma = 5000;

			VirtualArmor = 28;

			AddItem( new Crossbow() );

			m_NextAbilityTime = DateTime.Now + TimeSpan.FromSeconds( Utility.RandomMinMax( 2, 5 ) );
		}

		public override bool CanRummageCorpses{ get{ return true; } }
		public override bool InitialInnocent{ get{ return true; } }

		public override int GetHurtSound()
		{
			return 0x14D;
		}

		public override int GetDeathSound()
		{
			return 0x314;
		}

		public override int GetAttackSound()
		{
			return 0x75;
		}

		private DateTime m_NextAbilityTime;

		public override void OnThink()
		{
			if ( Combatant != null && this.MagicDamageAbsorb < 1 )
			{
				this.MagicDamageAbsorb = Utility.RandomMinMax( 5, 7 );
				this.FixedParticles( 0x375A, 10, 15, 5037, EffectLayer.Waist );
				this.PlaySound( 0x1E9 );
			}

			if ( DateTime.Now >= m_NextAbilityTime )
			{
				m_NextAbilityTime = DateTime.Now + TimeSpan.FromSeconds( Utility.RandomMinMax( 10, 15 ) );

				ArrayList list = new ArrayList();

				IPooledEnumerable eable = this.GetMobilesInRange( 8 );
				foreach ( Mobile m in eable)
				{
					if ( m is MeerWarrior && IsFriend( m ) && CanBeBeneficial( m ) && m.Hits < m.HitsMax && !m.Poisoned && !MortalStrike.IsWounded( m ) )
						list.Add( m );
				}
				eable.Free();

				for ( int i = 0; i < list.Count; ++i )
				{
					Mobile m = (Mobile)list[i];

					DoBeneficial( m );

					int toHeal = Utility.RandomMinMax( 20, 30 );

					SpellHelper.Turn( this, m );

					m.Heal( toHeal );

					m.FixedParticles( 0x376A, 9, 32, 5030, EffectLayer.Waist );
					m.PlaySound( 0x202 );
				}
			}

			base.OnThink();
		}

		public MeerCaptain( Serial serial ) : base( serial )
		{
		}

		public override void GenerateLoot()
		{
			Container pack = new Backpack();

			pack.DropItem( new Bolt( Utility.RandomMinMax( 10, 20 ) ) );
			pack.DropItem( new Bolt( Utility.RandomMinMax( 10, 20 ) ) );

			switch ( Utility.Random( 6 ) )
			{
				case 0: pack.DropItem( new Broadsword() ); break;
				case 1: pack.DropItem( new Cutlass() ); break;
				case 2: pack.DropItem( new Katana() ); break;
				case 3: pack.DropItem( new Longsword() ); break;
				case 4: pack.DropItem( new Scimitar() ); break;
				case 5: pack.DropItem( new VikingSword() ); break;
			}

			Container bag = new Bag();

			int count = Utility.RandomMinMax( 10, 20 );

			for ( int i = 0; i < count; ++i )
			{
				Item item = Loot.RandomReagent();

				if ( item == null )
					continue;

				if ( !bag.TryDropItem( this, item, false ) )
					item.Delete();
			}

			pack.DropItem( bag );

			PackGold( 25, 50 );
			PackItem( pack );
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}
