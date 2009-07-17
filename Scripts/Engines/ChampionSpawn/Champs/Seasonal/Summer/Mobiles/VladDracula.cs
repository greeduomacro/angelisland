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

/* Scripts\Engines\ChampionSpawn\Champs\Seasonal\Summer\Mobiles\VladDracula.cs
 *	ChangeLog
 *	07/23/08, weaver
 *		Automated IPooledEnumerable optimizations. 1 loops updated.
 *	6/26/08, Adam
 *		OnGotMeleeAttack: spawn bats
 *		OnDamagedBySpell: polymorph target
 *	6/23/08, Adam
 *		Change call to ImbueWeaponOrArmor to allow the huing of the Scepter as well.
 *		Add new and better loots
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
using Server.Spells;
using Server.Spells.Seventh;
using Server.Spells.Fifth;
using Server.Scripts.Commands;
using Server.Engines.ChampionSpawn;

namespace Server.Mobiles
{
	public class VladDracula : Vampire
	{
		[Constructable]
		public VladDracula()
			: base()
		{
			FlyArray = FlyTiles; //assign to mobile fly array for movement code to use.
			BardImmune = true;

			SpeechHue = 0x21;
			Hue = 0;
			HueMod = 0;

			// stats of Barracoon + lifedrain, and scary, and hypnotize
			SetStr(305, 425);
			SetDex(72, 150);
			SetInt(505, 750);
			SetHits(4200);
			SetDamage(1, 5);	// all damage is via life drain
			SetStam(102, 300);

			VirtualArmor = 70;

			CoreVampSkills();
			SetSkill(SkillName.Macing, 82.5, 100);	// for Scepter

			Fame = 22500;
			Karma = 0;

			InitBody();
			InitOutfit();
		}

		public override bool AutoDispel { get { return true; } }

		public override void InitBody()
		{
			Body = 0x190;
			Female = false;
			Name = "Vlad Dracula";
			Title = "the impaler";
		}
		public override void InitOutfit()
		{
			WipeLayers();

			// add a "wooden stake" to our loot
			Shaft WoodenStake = new Shaft();
			WoodenStake.Hue = 51;
			WoodenStake.Name = "wooden stake";
			PackItem(WoodenStake);

			// black backpack. we need a backpack so our walking-dead can be disarmed, and black is cool
			Backpack.Hue = 0x01;

			// add Scepter
			Scepter weapon = new Scepter();		// can disarm, but can't steal
			weapon.LootType = LootType.Newbied;	// can't steal (will drop on death)
			AddItem(weapon);

			Item hair = new LongHair(1); 
			Item pants = new LongPants(0x1);
			Item shirt = new FancyShirt();
			hair.Layer = Layer.Hair;
			AddItem(hair);
			AddItem(pants);
			AddItem(shirt);

			Item necklace = new GoldNecklace();
			AddItem(necklace);
			Item ring = new GoldRing();
			AddItem(ring);
			Item bracelet = new GoldBracelet();
			AddItem(bracelet);

			Item boots = new Sandals(0x1);
			AddItem(boots);
		}

		#region Do Special Ability
		public void Polymorph(Mobile m)
		{
			if (!m.CanBeginAction(typeof(PolymorphSpell)) || !m.CanBeginAction(typeof(IncognitoSpell)) || m.IsBodyMod)
				return;

			IMount mount = m.Mount;

			if (mount != null)
				mount.Rider = null;

			if (m.Mounted)
				return;

			if (m.BeginAction(typeof(PolymorphSpell)))
			{
				Item disarm = m.FindItemOnLayer(Layer.OneHanded);

				if (disarm != null && disarm.Movable)
					m.AddToBackpack(disarm);

				disarm = m.FindItemOnLayer(Layer.TwoHanded);

				if (disarm != null && disarm.Movable)
					m.AddToBackpack(disarm);

				m.BodyMod = 317;
				m.HueMod = 0;

				new ExpirePolymorphTimer(m).Start();
			}
		}

		private class ExpirePolymorphTimer : Timer
		{
			private Mobile m_Owner;

			public ExpirePolymorphTimer(Mobile owner)
				: base(TimeSpan.FromMinutes(3.0))
			{
				m_Owner = owner;

				Priority = TimerPriority.OneSecond;
			}

			protected override void OnTick()
			{
				if (!m_Owner.CanBeginAction(typeof(PolymorphSpell)))
				{
					m_Owner.BodyMod = 0;
					m_Owner.HueMod = -1;
					m_Owner.EndAction(typeof(PolymorphSpell));
				}
			}
		}

		public void SpawnBats(Mobile target)
		{
			Map map = this.Map;

			if (map == null)
				return;

			int bats = 0;

			IPooledEnumerable eable = this.GetMobilesInRange(10);
			foreach (Mobile m in eable)
			{
				if (m is VampireBat)
					++bats;
			}
			eable.Free();

			if (bats < 16)
			{
				int newRats = Utility.RandomMinMax(3, 6);

				try
				{
					for (int i = 0; i < newRats; ++i)
					{
						BaseCreature bat;

						bat = new VampireBat();
						bat.Team = this.Team;

						bool validLocation = false;
						Point3D loc = this.Location;

						for (int j = 0; !validLocation && j < 10; ++j)
						{
							int x = target.X + Utility.Random(3) - 1;
							int y = target.Y + Utility.Random(3) - 1;
							int z = map.GetAverageZ(x, y);

							if (validLocation = map.CanFit(x, y, target.Z, 16, CanFitFlags.requireSurface))
								loc = new Point3D(x, y, Z);
							else if (validLocation = map.CanFit(x, y, z, 16, CanFitFlags.requireSurface))
								loc = new Point3D(x, y, z);
						}

						bat.MoveToWorld(loc, map);

						bat.Combatant = target;
					}
				}
				catch (Exception e)
				{
					LogHelper.LogException(e);
					Console.WriteLine("Exception (non-fatal) caught at VladDracula.Damage: " + e.Message);
				}
			}
		}

		public override void OnDamagedBySpell(Mobile caster)
		{	// don't use magic
			base.OnDamagedBySpell(caster);

			if (caster == this)
				return;

			if (caster is PlayerMobile)
			{
				if (Utility.RandomChance(30))		// 30% chance to polymorph attacker into a vampire bat
					Polymorph(caster);
			}
			else if (caster is BaseCreature && (caster as BaseCreature).Controlled == true)
			{	// don't use pets
				if (Utility.RandomChance(60))		// 60% chance to polymorph attacker into a vampire bat
					if (Utility.RandomBool())
					{
						if ((caster as BaseCreature).ControlMaster != null)
							Polymorph((caster as BaseCreature).ControlMaster);
						else
							Polymorph(caster);
					}
					else
						Polymorph(caster);
			}
		}

		public override void OnGotMeleeAttack(Mobile attacker)
		{
			base.OnGotMeleeAttack(attacker);

			if (attacker is PlayerMobile)
			{
				if (Utility.RandomChance(5))	// 5% chance to spawn more vampire bats
					SpawnBats(attacker);
			}
			else if (attacker is BaseCreature && (attacker as BaseCreature).Controlled == true)
			{	// don't use pets!
				if (Utility.RandomChance(20))	// 20% chance to spawn more vampire bats
					if (Utility.RandomBool())
					{
						if ((attacker as BaseCreature).ControlMaster != null)
							SpawnBats((attacker as BaseCreature).ControlMaster);
						else
							SpawnBats(attacker);
					}
					else
						SpawnBats(attacker);
			}
		}

#endregion Do Special Ability

		public override bool OnBeforeDeath()
		{
			if (this.BatForm)
				this.Body = 0x190;

			this.AIObject.Deactivate();
			Effects.PlaySound(this, this.Map, 648);
			this.FixedParticles(0x3709, 10, 30, 5052, EffectLayer.LeftFoot);
			this.PlaySound(0x208);

			Item item;
			switch (Utility.Random(10))
			{
				// Tarot, 4773 (mirror type, 4774); another type of Tarot, 4775 (mirror type, 4776)
				case 0:
					{
						item = new Item(4773);
						item.Weight = 1;
						PackItem(item);
					}
					break;

				case 1:
					{
						item = new Item(4774);
						item.Weight = 1;
						PackItem(item);
					}
					break;

				case 2:
					{
						item = new Item(4775);
						item.Weight = 1;
						PackItem(item);
					}
					break;

				case 3:
					{
						item = new Item(4776);
						item.Weight = 1;
						PackItem(item);
					}
					break;

				// Shackles, 4706 (mirror type, 4707)
				case 4:
					{
						item = new Item(4706);
						item.Weight = 1;
						PackItem(item);
					}
					break;

				case 5:
					{
						item = new Item(4707);
						item.Weight = 1;
						PackItem(item);
					}
					break;

				//"Kazikli Voyvoda"
				case 6:
					{
						item = new Item(7712);
						item.Weight = 1;
						item.Name = "Kazikli Voyvoda";
						PackItem(item);
					}
					break;
				case 7:
					{
						item = new Item(7713);
						item.Weight = 2;
						// http://www.spells4free.com/Article/vampires--7-Shurpu-Kishpu-The-Book-Of-Dreaming/547
						item.Name = Utility.RandomBool() ? "Shurpu Kishpu" : "Hekal Tiamat";
						PackItem(item);
					}
					break;

				// "a mask of vampiric death" 
				case 8:
					{
						item = new Item(5147);
						item.Weight = 2;
						item.Hue = 1001;
						item.Layer = Layer.Helm;
						item.Name = "a mask of vampiric death";
						PackItem(item);
					}
					break;
				case 9:
					{
						item = new Item(5148);
						item.Weight = 2;
						item.Hue = 1001;
						item.Layer = Layer.Helm;
						item.Name = "a mask of vampiric death";
						PackItem(item);
					}
					break;
			}

			return base.OnBeforeDeath();
		}

		public VladDracula(Serial serial)
			: base(serial)
		{
		}

		public override void GenerateLoot()
		{
			Item blood = new BloodVial();
			blood.Name = "blood of " + this.Name;
			PackItem(blood);

			// since Vlad's secpter was newbied, lets generate a fresh one
			Scepter weapon = new Scepter();
			Loot.ImbueWeaponOrArmor(weapon, 6, 0, Utility.RandomBool());
			PackItem(weapon, false);
		}

		public override void DistributedLoot()
		{
			// distribute loot
			new ChampLootPack(this).DistributedLoot();
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int)0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
		}
	}
}
