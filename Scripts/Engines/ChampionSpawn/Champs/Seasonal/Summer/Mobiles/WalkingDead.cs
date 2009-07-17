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

/* Scripts\Engines\ChampionSpawn\Champs\Seasonal\Summer\Mobiles\WalkingDead.cs
 *	ChangeLog
 *	3/12/08, Adam
 *		initial creation
 *		based on Scripts/Mobiles/Monsters/Humanoid/Melee/Vampire.cs
 */

using System;
using System.Collections;
using Server.Items;
using Server.ContextMenus;
using Server.Misc;
using Server.Network;
using Server.Scripts.Commands;

namespace Server.Mobiles
{
	public class WalkingDead : Vampire
	{
		// not as strong as vampires
		public override int LifeDrain { get { return base.LifeDrain / 2; } }
		public override int StamDrain { get { return base.StamDrain / 2; } }

		[Constructable]
		public WalkingDead()
			: base()
		{
			FlyArray = FlyTiles; //assign to mobile fly array for movement code to use.
			BardImmune = true;

			SpeechHue = 0x21;
			Hue = 0;
			HueMod = 0;

			// vamp stats
			SetStr(200 / 2, 300 / 2);	// 1/2 the STR of a Vampire
			SetDex(105, 135);
			SetInt(80, 105);
			SetHits(140, 176);
			SetDamage(1, 5); // all damage is via life drain
			
			VirtualArmor = 20;

			CoreVampSkills();
			SetSkill(SkillName.Swords, 86.0, 100.0);	// for bucher knife

			Fame = 10000;
			Karma = 0;

			InitBody();
			InitOutfit();

		}

		public override void InitBody()
		{
			if (Female = Utility.RandomBool())
			{
				Body = 0x191;
				Name = NameList.RandomName("female");
			}
			else
			{
				Body = 0x190;
				Name = NameList.RandomName("male");
			}

			Title = "the walking dead";
		}
		public override void InitOutfit()
		{
			WipeLayers();

			// black backpack. we need a backpack so our walking-dead can be disarmed, and black is cool
			Shaft WoodenStake = new Shaft();
			WoodenStake.Hue = 51;
			WoodenStake.Name = "wooden stake";
			PackItem(WoodenStake);
			Backpack.Hue = 0x01;

			if (Utility.RandomBool())
				AddItem(new Cleaver());
			else
				AddItem(new ButcherKnife());

			// walking dead are naked
			if (this.Female)
			{

				Item hair = new Item(0x203C);
				if (Utility.RandomMinMax(0, 100) <= 20) //20% chance to have black hair
				{
					hair.Hue = 0x1;
				}
				else
					hair.Hue = Utility.RandomHairHue();

				hair.Layer = Layer.Hair;
				AddItem(hair);
			}
			else
			{
				Item hair2 = new Item(Utility.RandomList(0x203C, 0x203B));
				hair2.Hue = Utility.RandomHairHue();
				hair2.Layer = Layer.Hair;
				AddItem(hair2);
			}
		}
		
		public WalkingDead(Serial serial)
			: base(serial)
		{
		}


		public override void GenerateLoot()
		{
			PackGold(170 / 2, 220 / 2); //add gold if its daytime - 1/2 of vamipres
			Item blood = new BloodVial();
			blood.Name = "blood of " + this.Name;
			PackItem(blood);

		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)1); // version
			//writer.Write(BatForm); // version 1
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 1:
					// remove batform bool from serialization
					break;
				case 0:
					bool dmy = reader.ReadBool();
					break;
			}
			
		}
	}
}
