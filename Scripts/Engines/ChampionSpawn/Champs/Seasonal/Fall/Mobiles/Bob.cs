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

/* Scripts\Engines\ChampionSpawn\Champs\Seasonal\Fall\Mobiles\Bob.cs
 * ChangeLog
 *  9/29/07, Adam
 *		Create from Brigand.
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
	public class Bob : BaseCreature
	{
		public override bool ClickTitle{ get{ return false; } }

		[Constructable]
		public Bob() : base( AIType.AI_Melee, FightMode.All | FightMode.Weakest, 10, 1, 0.25, 0.5 )
		{
			SpeechHue = Utility.RandomDyedHue();
			Hue = 33770;
            BardImmune = true;
            CanRun = true;
			
			SetStr( 96, 115 );
			SetDex( 86, 105 );
			SetInt( 51, 65 );

			SetDamage( 23, 27 );

			SetSkill( SkillName.Macing, 100 );
			SetSkill( SkillName.MagicResist, 100 );
			SetSkill( SkillName.Tactics, 100 );

			InitBody();
			InitOutfit();

			Fame = 1000;
			Karma = -1000;

			PackItem( new Bandage( Utility.RandomMinMax( 1, 15 ) ) );
			
		}

		public override bool AlwaysMurderer{ get{ return true; } }
		public override bool ShowFameTitle{ get{ return false; } }
		public override bool CanRummageCorpses{ get{ return true; } }

		public override bool CanBandage{ get{ return true; } }
		public override TimeSpan BandageDelay{ get{ return TimeSpan.FromSeconds( Utility.RandomMinMax( 10, 13 ) ); } }

		public Bob( Serial serial ) : base( serial )
		{
		}

		public override void InitBody()
		{
            this.Female = false;
			Body = 0x190;
			Name = NameList.RandomName( "Bob" );
		}
		public override void InitOutfit()
		{
			WipeLayers();

            Robe robe = new Robe(23);
            AddItem( robe );

            AddItem( new Club() ); 
		}
		public override void GenerateLoot()
		{
			PackGold( 100, 150 );
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
}
