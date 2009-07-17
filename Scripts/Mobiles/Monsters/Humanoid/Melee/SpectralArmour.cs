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

/* Scripts/Mobiles/Monsters/Humanoid/Melee/SpectralArmor.cs
 * ChangeLog
 *  8/16/06, Rhiannon
 *		Changed speed settings to match SpeedInfo table.
 *	7/6/04, Adam
 *		1. implement Jade's new Category Based Drop requirements
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */

using System; 
using System.Collections; 
using Server.Items; 
using Server.Targeting; 

namespace Server.Mobiles 
{ 
    
	public class SpectralArmour : BaseCreature 
	{ 
		[Constructable] 
		public SpectralArmour() : base( AIType.AI_Melee, FightMode.All | FightMode.Closest, 10, 1, 0.25, 0.5 ) 
		{ 
			Body = 637; 
			Hue = 32; 
			Name = "spectral armour"; 
			BaseSoundID = 451; 

			SetStr( 309, 333 ); 
			SetDex( 99, 106 ); 
			SetInt( 101, 110 ); 
			SetSkill( SkillName.Wrestling, 78.1, 95.5 ); 
			SetSkill( SkillName.Tactics, 91.1, 99.7 ); 
			SetSkill( SkillName.MagicResist, 92.4, 79 ); 
			SetSkill( SkillName.Swords, 78.1, 97.4); 

			VirtualArmor = 40; 
			SetFameLevel( 3 ); 
			SetKarmaLevel( 3 ); 
		} 

		public override void GenerateLoot()
		{
			//AddLoot( LootPack.Rich );
		}

		public override Poison PoisonImmune{ get{ return Poison.Regular; } }

		[CommandProperty( AccessLevel.GameMaster )] 
		public override int HitsMax { get { return 323; } } 

		public SpectralArmour( Serial serial ) : base( serial ) 
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
		public override bool OnBeforeDeath() 
		{ 
			Scimitar weapon = new Scimitar(); 

			weapon.DamageLevel = (WeaponDamageLevel)Utility.Random( 0, 5 ); 
			weapon.DurabilityLevel = (WeaponDurabilityLevel)Utility.Random( 0, 5 ); 
			weapon.AccuracyLevel = (WeaponAccuracyLevel)Utility.Random( 0, 5 ); 

			weapon.MoveToWorld( this.Location, this.Map );

			// TODO: need to handle this Category 2 MID
			// Category 2 MID
			// PackMagicItem( 1, 1, 0.05 );
         
			this.Delete(); 
			return false; 
		} 
	} 
}