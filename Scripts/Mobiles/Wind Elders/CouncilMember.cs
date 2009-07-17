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

/* Scripts/Mobiles/Wind Elders/CouncilMember.cs
 * ChangeLog
 * 12/03/06 Taran Kain
 *      Set Female to false - no tranny council members!
 *	7/26/05, erlein
 *		Automated removal of AoS resistance related function calls. 8 lines removed.
 * 6/05/05, Kit
 *		Switched to use new CouncilMemberAI based off of HumanMageAI
 *	4/13/05, Kit
 *		Switch to new region specific loot model
 *	1/16/05, Adam
 *		level 5 mob balance
 *			Up Wrestling from 100.0, 110.0 to 160.0, 170.0
 *			This change allows the Council Member to avoid excessive interruptions
 *	12/22/04, Adam
 *		Region specific rares... 
 *			Because IOBs can be used to take your kin away to be farmed, it is best to restrict
 *			at least the rares drop the the kin's stronghold.
 *	12/11/04, Pix
 *		Changed ControlSlots for IOBF.
 *  11/16/04, Froste
 *      Changed IOBAlignment to Council
 *  11/10/04, Froste
 *      Implemented new random IOB drop system and changed drop change to 12%
 *	11/05/04, Pigpen
 *		Made changes for Implementation of IOBSystem. Changes include:
 *		Removed IsEnemy and Aggressive Action Checks. These are now handled in BaseCreature.cs
 *		Set Creature IOBAlignment to Undead.
 *	9/26/04, Adam
 *		Add 5% IOB drop (BloodDrenchedBandana)
 *	8/3/04, Adam
 *		Update Stats, Skills, and Damage/Resist values to be more consistent.
 *	7/21/04, mith
 *		IsEnemy() and AggressiveAction() code added to support Brethren property of BloodDrenchedBandana.
 *	7/13/04 smerX
 *		Changed AIType to AIType.AI_Council (formerly AIType.AI_Mage)
 *	7/13/04, adam
 *		1. reduce chance to drop robes and sandies from 7% to 5%// adam: reduce chance to 5% from 7% drop
 *	7/13/04 smerX
 *		Upgraded AI
 *		Changed FightMode to Strongest (formerly Closest)
 *	7/12/04 smerX
 *		Fixxed issue with shrouds being set LootType.Blessed by default
 *	7/6/04, Adam
 *		1. implement Jade's new Category Based Drop requirements
 * Created 5/5/04 by mith
 */

using System; 
using System.Collections; 
using Server.Items; 
using Server.ContextMenus; 
using Server.Misc; 
using Server.Network; 
using Server.Engines.IOBSystem;

namespace Server.Mobiles
{
	public class CouncilMember : BaseCreature
	{
		private static int m_ID;

		[Constructable]
		public CouncilMember() : base( AIType.AI_CouncilMember, FightMode.All | FightMode.Strongest, 10, 1, 0.2, 0.4 )
		{
			if ( m_ID >= 8 )
				m_ID = 0;
			++m_ID;

			switch ( m_ID )
			{
				case 1: Name = "Etheorious Moori"; break;
				case 2: Name = "Luscious Moori"; break;
				case 3: Name = "Broderick Sway"; break;
				case 4: Name = "Keras Moiras"; break;
				case 5: Name = "Erinyes Furiae"; break;
				case 6: Name = "Heremod Furiae"; break;
				case 7: Name = "Hrothgar Wolfson"; break;
				case 8: Name = "Belk Baranow"; break;
			}

			Title = " of the Mystic Council";
            Female = false;
			Body = 0x190;
			IOBAlignment = IOBAlignment.Council;
			ControlSlots = 5;
			BardImmune = true;

			HoodedShroudOfShadows shroud = new HoodedShroudOfShadows();
			shroud.Hue = 0x455;
			shroud.Name = "Tattered Council Robe";

			// adam: reduce chance to 5% from 7% drop
			if ( Utility.RandomDouble() <= 0.95 )
			{
				shroud.LootType = LootType.Newbied;
			}
			else
			{
				shroud.LootType = LootType.Regular;
			}
			
			AddItem( shroud );

			Sandals sandals = new Sandals();
			double chance = Utility.RandomDouble();
			if ( chance <= 0.25 )
				sandals.Hue =  0x15C;
			else if ( chance <= 0.50 )
				sandals.Hue = 0x592;
			else if ( chance <= 0.75 )
				sandals.Hue = 0x4D3;
			else
				sandals.Hue = 0x653;

			// adam: reduce chance to 5% from 7% drop
			if ( Utility.RandomDouble() <= 0.95 )
				sandals.LootType = LootType.Newbied;

			AddItem( sandals );

			SetStr( 416, 505 );
			SetDex( 146, 165 );
			SetInt( 566, 655 );

			SetHits( 250, 303 );
			SetDamage( 11, 13 );



			SetSkill( SkillName.EvalInt, 90.1, 100.0 );
			SetSkill( SkillName.Magery, 90.1, 100.0 );
			SetSkill( SkillName.MagicResist, 150.5, 200.0 );
			SetSkill( SkillName.Meditation, 100.0, 110.0 );
			SetSkill( SkillName.Tactics, 50.1, 70.0 );
			SetSkill( SkillName.Wrestling, 160.0, 170.0 );
			SetSkill( SkillName.Poisoning, 100.1, 101.0 );

			Fame = 10000;
			Karma = -10000;

			VirtualArmor = 50;
		}

		public override bool AlwaysMurderer{ get{ return true; } }
		public override bool CanRummageCorpses{ get{ return true; } }
		public override Poison PoisonImmune{ get{ return Poison.Lethal; } }
		public override int TreasureMapLevel{ get{ return 5; } }

		public override bool Uncalmable
		{
			get
			{
				if ( Hits > 1 )
					Say( "Peace, is it? I'll give you peace!" );
				
				return BardImmune;
			}
		}

		public CouncilMember( Serial serial ) : base( serial )
		{
		}

		public override void GenerateLoot()
		{
			PackItem( new GnarledStaff() );
			PackGold( 600, 800 );
			PackScroll( 3, 7 );
			PackScroll( 3, 7 );
			PackMagicEquipment( 2, 3, 0.60, 0.60 );
			PackMagicEquipment( 2, 3, 0.25, 0.25 );
			PackReg( 10, 20 );

			// Froste: 12% random IOB drop
			if (0.12 > Utility.RandomDouble())
			{
				Item iob = Loot.RandomIOB();
				PackItem(iob);
			}

			if (IOBRegions.GetIOBStronghold(this) == IOBAlignment)
			{
				// 30% boost to gold
				PackGold( base.GetGold()/3 );
			}

			//if not in region
			if (IOBRegions.GetIOBStronghold(this) != IOBAlignment)
			{
			
				// Adam: Remove interesting colors if not in stronghold
				Item shoes = this.FindItemOnLayer( Layer.Shoes );
				if ( shoes is Sandals )
					shoes.Hue = 0;

				// make sure the robe does not drop
				Item shroud = FindItemOnLayer( Layer.OuterTorso );
				if (shroud != null)
					shroud.LootType = LootType.Newbied;
			}
				
		}
		
		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 );

			writer.Write( (int) m_ID );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();

			int m_ID = reader.ReadInt();
		}
	}
}
