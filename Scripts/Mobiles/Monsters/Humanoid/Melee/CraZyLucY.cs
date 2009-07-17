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

/* Scripts/Mobiles/Monsters/Humanoid/Melee/CraZyLucY.cs
 *
 *	ChangeLog:
 *  03/09/07, plasma    
 *      Removed cannedevil namespace reference
 *  1/11/07, Adam
 *      Select 'None' Skull type for this special champ
 *  01/05/07, plasma
 *      Changed CannedEvil namespace to ChampionSpawn for cleanup!
 *  8/16/06, Rhiannon
 *		Moved speed settings into constructor
 *  7/02/06, Kit
 *		InitBody/InitOutfit additions
 *	7/26/05, erlein
 *		Automated removal of AoS resistance related function calls. 6 lines removed.
 *	12/7/05, Adam
 *		non-champ AI
 *		minor tweaks to damage
 *	12/6/05, Adam
 *		first time checkin
 */

using System;
using Server;
using Server.Items;
using Server.Engines.ChampionSpawn;

namespace Server.Mobiles
{
	[CorpseName( "the corpse of CraZy LucY" )]
	public class CraZyLucY : BaseChampion
	{
		public override ChampionSkullType SkullType{ get{ return ChampionSkullType.None; } }

		[Constructable]
		public CraZyLucY() : base( AIType.AI_Melee, FightMode.Aggressor, 0.2, 0.4 )
		{
			// non-champ AI
			RangePerception = 10;
			RangeFight = 1;
			
			Name = "CraZy LucY";
			Title = "The Annihilator";
			BaseSoundID = 1200;
			Female = true;

			SetStr( 505, 1000 );
			SetDex( 102, 300 );
			SetInt( 402, 600 );

			SetHits( 3000 );
			SetStam( 105, 600 );

			SetDamage( 15, 30 );

			SetSkill( SkillName.Anatomy, 99.5 );
			SetSkill( SkillName.Poisoning, 60.0, 82.5 );
			SetSkill( SkillName.MagicResist, 83.5, 92.5 );
			SetSkill( SkillName.Swords, 99.5 );
			SetSkill( SkillName.Tactics, 99.5 );
			SetSkill( SkillName.Lumberjacking, 99.5 );

			Fame = 22500;
			Karma = -22500;

			InitBody();
			InitOutfit();
			VirtualArmor = 60;

			Hue = Utility.RandomSkinHue();

			Container pack = new Backpack();
			pack.Movable = false;

			pack.DropItem( new Gold( 10, 25 ) );

			// add bandages, can't steal
			Bandage bandage = new Bandage( Utility.RandomMinMax( 1, 15 ) );
			bandage.LootType = LootType.Newbied;
			pack.DropItem( bandage );
		
			AddItem( pack );

		}
		
		public override void InitBody()
		{
			Body = 0x191;
		}
		public override void InitOutfit()
		{
			WipeLayers();
			switch( Utility.Random( 2 ) )
			{
				case 0: AddItem( new LeatherSkirt() ); break;
				case 1: AddItem( new LeatherShorts() ); break;
			}

			switch( Utility.Random( 5 ) )
			{
				case 0: AddItem( new FemaleLeatherChest() ); break;
				case 1: AddItem( new FemaleStuddedChest() ); break;
				case 2: AddItem( new LeatherBustierArms() ); break;
				case 3: AddItem( new StuddedBustierArms() ); break;
				case 4: AddItem( new FemalePlateChest() ); break;
			}

			Item hair = new Item( Utility.RandomList( 0x203B, 0x203C, 0x203D, 0x2044, 0x2045, 0x2047, 0x2049, 0x204A ) );
			hair.Hue = Utility.RandomHairHue();
			hair.Layer = Layer.Hair;
			hair.Movable = false;
			AddItem( hair );

			ExecutionersAxe weapon = new ExecutionersAxe();

			weapon.Movable = false;
			weapon.Crafter = this;
			weapon.Quality = WeaponQuality.Exceptional;
			AddItem( weapon );
		}


		public override bool OnBeforeDeath()
		{
			PlaySound( 0x1FE );

			int gems = Utility.RandomMinMax( 1, 5 );

			for ( int i = 0; i < gems; ++i )
				PackGem();

			PackGold( 900, 1200 );
			PackMagicEquipment(2, 3, 0.60, 0.60);
			PackMagicEquipment(2, 3, 0.25, 0.25);

			// Category 4 MID
			PackMagicItem( 2, 3, 0.10 );
			PackMagicItem( 2, 3, 0.05 );
			PackMagicItem( 2, 3, 0.02 );

			// set this so we don't generate all that crazy champ loot
			NoKillAwards = true;

			return base.OnBeforeDeath();
		}

		public override bool AlwaysAttackable{ get{ return true; } }
		public override Poison PoisonImmune{ get{ return Poison.Deadly; } }
		public override bool CanBandage{ get{ return true; } }
		public override TimeSpan BandageDelay{ get{ return TimeSpan.FromSeconds( Utility.RandomMinMax( 20, 40 ) ); } }
		public override bool ShowFameTitle{ get{ return false; } }
		public override bool ClickTitle { get { return false; } }

		public override void Damage( int amount, Mobile from )
		{
			base.Damage( amount, from );
			
			Mobile m = VerifyValidMobile( from, 9 );
			
			if ( m != null )
			{
				ShootWeb( m, 9 );
				new WarpTimer( this, m, 8, TimeSpan.FromSeconds( 2.0 ) ).Start();
			}
		}
		
		public Mobile VerifyValidMobile( Mobile m, int tileRange )
		{
			if ( m != null && m is PlayerMobile && m.AccessLevel == AccessLevel.Player ||  m != null && m is BaseCreature && ((BaseCreature)m).Controlled )
			{
				if ( m != null && m.Map == this.Map && m.InRange( this, tileRange ) && m.Alive )
					return m;
			}
		
			return null;
		}
		
		private void ShootWeb( Mobile target, int range )
		{
			Mobile m = VerifyValidMobile( target, range );
			
			if ( m != null && !(m is BaseCreature) )
			{
				//this.MovingParticles( m, 0x379F, 7, 0, false, true, 3043, 4043, 0x211 );
				this.MovingParticles( m, 0x10d4, 7, 0, false, false, 3043, 4043, 0x211 );
				m.Freeze( TimeSpan.FromSeconds( 4.0 ) );
				Fishnets w = new Fishnets( TimeSpan.FromSeconds( 15.0 ), TimeSpan.FromSeconds( 4.0 ) );
				w.MoveToWorld( new Point3D(m.X, m.Y, m.Z), m.Map );
			}
		}

		private class WarpTimer : Timer
		{
			private Mobile m_From;
			private Mobile m_To;
			private int m_Range;

			public WarpTimer( Mobile from, Mobile to, int range, TimeSpan delay ) : base( delay )
			{
				m_From = from;
				m_To = to;
				m_Range = range;
			}

			protected override void OnTick()
			{
				if ( m_From != null && m_To != null && !(m_To.Hidden) && m_To.InRange( m_From, m_Range ) )
					m_From.Location =  new Point3D(m_To.X, m_To.Y, m_To.Z);
				
				this.Stop();
			}
		}
		
		public CraZyLucY( Serial serial ) : base( serial )
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
	}
	
	public class Fishnets : Item
	{
		private TimeSpan m_ParaTime;

		[Constructable]
		public Fishnets( TimeSpan delay, TimeSpan paraTime ) : base( 0x10d4 )
		{
			m_ParaTime = paraTime;
			
			this.Movable = false;
			
			new DeletionTimer( delay, this ).Start();
		}

		public override bool OnMoveOver( Mobile m )
		{
			if ( m != null && m is PlayerMobile && m.AccessLevel == AccessLevel.Player ||  m != null && m is BaseCreature && ((BaseCreature)m).Controlled )
			{
				if ( m != null && m.Map == this.Map && m.Alive )
				{
					m.Freeze( m_ParaTime );
					m.SendMessage( "You become entangled in CraZy LucY's Fishnets!" );
				}
			}
			
			return base.OnMoveOver( m );
		}
		
		private class DeletionTimer : Timer
		{
			private Item m_ToDelete;
			
			public DeletionTimer( TimeSpan delay, Item todelete ) : base( delay )
			{
				m_ToDelete = todelete;
				Priority = TimerPriority.TwentyFiveMS;
			}
			
			protected override void OnTick()
			{
				m_ToDelete.Delete();				
				this.Stop();
			}
		}

		public Fishnets( Serial serial ) : base( serial )
		{
		}
	
		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			
			writer.Write( (int) 0 ); //vers
		}
		
		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			
			int version = reader.ReadInt();
		}	
	}
}
