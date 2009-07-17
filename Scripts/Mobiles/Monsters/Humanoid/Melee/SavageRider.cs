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

/* Scripts/Mobiles/Monsters/Humanoid/Melee/SavageRider.cs
 * ChangeLog
 *  8/16/06, Rhiannon
 *		Changed speed settings to match SpeedInfo table.
 *  7/02/06, Kit
 *		InitBody/InitOutfit additions
 *	7/26/05, erlein
 *		Automated removal of AoS resistance related function calls. 1 lines removed.
 *	4/13/05, Kit
 *		Switch to new region specific loot model
 *  03/14/05, Lego
 *          added the ability to bandage.
 *	12/15/04, Pix
 *		Removed damage mod to big pets.
 *	12/11/04, Pix
 *		Changed ControlSlots for IOBF.
 *  11/10/04, Froste
 *      Implemented new random IOB drop system and changed drop change to 12%
 *	11/05/04, Pigpen
 *		Made changes for Implementation of IOBSystem. Changes include:
 *		Removed IsEnemy and Aggressive Action Checks. These are now handled in BaseCreature.cs
 *		Set Creature IOBAlignment to Savage.
 *		Set BearMask drop to set mask to IOBAlignment Savage.
 *	8/23/04, Adam
 *		Increase gold to 125-175
 *		Add berry drop of 20%
 *		decrease the drop of the tribal spears 10%
 *		decrease bola drop to 5%
 *	7/29/04 smerX
 *		Set BearMask drop to 5%
 *	7/29/04, mith
 *		Included BearMask()
 *	7/6/04, Adam
 *		1. implement Jade's new Category Based Drop requirements
 *	6/11/04, mith
 *		Moved the equippable combat items out of OnBeforeDeath()
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */

using System;
using Server;
using Server.Misc;
using Server.Items;
using Server.Engines.IOBSystem;

namespace Server.Mobiles
{
	[CorpseName( "a savage corpse" )]
	public class SavageRider : BaseCreature
	{
		[Constructable]
		public SavageRider() : base( AIType.AI_Melee, FightMode.All | FightMode.Closest, 10, 1, 0.2, 0.4 )
		{
					
			IOBAlignment = IOBAlignment.Savage;
			ControlSlots = 3;

			SetStr( 151, 170 );
			SetDex( 92, 130 );
			SetInt( 51, 65 );

			SetDamage( 29, 34 );


			SetSkill( SkillName.Fencing, 72.5, 95.0 );
			SetSkill( SkillName.Healing, 60.3, 90.0 );
			SetSkill( SkillName.Macing, 72.5, 95.0 );
			SetSkill( SkillName.Poisoning, 60.0, 82.5 );
			SetSkill( SkillName.MagicResist, 72.5, 95.0 );
			SetSkill( SkillName.Swords, 72.5, 95.0 );
			SetSkill( SkillName.Tactics, 72.5, 95.0 );

			Fame = 1000;
			Karma = -1000;

			InitBody();
			InitOutfit();

			new SavageRidgeback().Rider = this;
		}

		public override int Meat{ get{ return 1; } }
		public override bool AlwaysMurderer{ get{ return true; } }
		public override bool ShowFameTitle{ get{ return false; } }

        public override bool CanBandage { get { return true; } }
        public override TimeSpan BandageDelay { get { return TimeSpan.FromSeconds(Utility.RandomMinMax(10, 13)); } }

		public override void InitBody()
		{
			Name = NameList.RandomName( "savage rider" );

			if ( Female = Utility.RandomBool() )
				Body = 186;
			else
				Body = 185;
		}
		public override void InitOutfit()
		{
			WipeLayers();
			AddItem( new BoneArms() );
			AddItem( new BoneLegs() );
			
			BearMask mask = new BearMask();
			mask.LootType = LootType.Newbied;
			AddItem( mask );

			TribalSpear spear = new TribalSpear();
			if ( Utility.RandomDouble() <= 0.10 )
			{
				spear.LootType = LootType.Regular;
			}
			else
			{
				spear.LootType = LootType.Newbied;
			}
			AddItem( spear );

			
		}
		public override void GenerateLoot()
		{
			PackGold( 125, 175 );
			PackItem( new Bandage( Utility.RandomMinMax( 1, 15 ) ) );

			if (Utility.RandomDouble() < 0.05)
				PackItem( new BolaBall() );

			if (Utility.RandomDouble() < 0.30)
				PackItem( new TribalBerry() );

			// Froste: 12% random IOB drop
			if (0.12 > Utility.RandomDouble())
			{
				Item iob = Loot.RandomIOB();
				PackItem(iob);
			}

			// Category 2 MID
			PackMagicItem( 1, 1, 0.05 );
			
			if (IOBRegions.GetIOBStronghold(this) == IOBAlignment)
			{
				// 30% boost to gold
				PackGold( base.GetGold()/3 );
			}
		}

		public override bool OnBeforeDeath()
		{
			IMount mount = this.Mount;

			if ( mount != null )
				mount.Rider = null;

			if ( mount is Mobile )
				((Mobile)mount).Delete();

			return base.OnBeforeDeath();
		}

		public override void AlterMeleeDamageTo( Mobile to, ref int damage )
		{
			//if ( to is Dragon || to is WhiteWyrm || to is SwampDragon || to is Drake || to is Nightmare || to is Daemon )
			//	damage *= 5;
		}

		public SavageRider( Serial serial ) : base( serial )
		{
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
