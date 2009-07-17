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

/* Scripts\Engines\ChampionSpawn\Champs\Seasonal\Spring\Mobiles\PirateChamp.cs	
 * ChangeLog:
 *	1/1/09, Adam
 *		- Add potions and bandages
 *			Now uses real potions and real bandages
 *		- Cross heals is now turned off
 *		- Smart AI upgrade (adds healing with bandages)
 *	07/23/08, weaver
 *		Automated IPooledEnumerable optimizations. 1 loops updated.
 *  3/22/07, Adam
 *      Created; based largely on Neira stats
 */

using System;
using System.Collections;
using Server.Items;
using Server.Targeting;
using Server.Spells;
using Server.Scripts.Commands;			// log helper

namespace Server.Mobiles
{
    [CorpseName("corpse of a salty seadog")]
    public class PirateChamp : BaseCreature
    {
        private TimeSpan m_SpeechDelay = TimeSpan.FromSeconds(45.0); // time between pirate speech
        public DateTime m_NextSpeechTime;
        private MetalChest m_MetalChest = null;

        [Constructable]
        public PirateChamp()
			: base(AIType.AI_Hybrid, FightMode.All | FightMode.Closest, 10, 1, 0.2, 0.4)
        {
			BardImmune = true;
			FightStyle = FightStyle.Melee | FightStyle.Magic | FightStyle.Smart | FightStyle.Bless | FightStyle.Curse;
			UsesHumanWeapons = false;
			UsesBandages = true;
			UsesPotions = true;
			CanRun = true;
			CanReveal = true; // magic and smart

            Title = "the hoard guardian";
            Hue = Utility.RandomSkinHue();

            SetStr(305, 425);
            SetDex(72, 150);
            SetInt(505, 750);

            SetHits(4800);
            SetStam(102, 300);

            SetDamage(25, 35);

            SetSkill(SkillName.EvalInt, 120.0);
            SetSkill(SkillName.Magery, 120.0);
            SetSkill(SkillName.Meditation, 120.0);
            SetSkill(SkillName.MagicResist, 150.0);
            SetSkill(SkillName.Swords, 97.6, 100.0);
            SetSkill(SkillName.Tactics, 97.6, 100.0);
            SetSkill(SkillName.Wrestling, 97.6, 100.0);

            SpeechHue = Utility.RandomDyedHue();

            Fame = 22500;
            Karma = -22500;

            InitBody();
            InitOutfit();

            VirtualArmor = 30;

            m_NextSpeechTime = DateTime.Now + m_SpeechDelay;

			PackItem(new Bandage(Utility.RandomMinMax(VirtualArmor, VirtualArmor * 2)));
			PackStrongPotions(6, 12);
			PackItem(new Pouch());
        }

        public PirateChamp(Serial serial)
            : base(serial)
        {
        }

        public override int TreasureMapLevel { get { return 5; } }
        public override bool AlwaysMurderer { get { return true; } }
        public override bool CanRummageCorpses { get { return true; } }
        public override bool ShowFameTitle { get { return false; } }
        public override bool ClickTitle { get { return true; } }

        public override void InitBody()
        {
            if (Female = Utility.RandomBool())
            {
                Body = 0x191;
                // "Lizzie" "the Black"
                Name = NameList.RandomName("pirate_female");
            }
            else
            {
                Body = 0x190;
                Name = NameList.RandomName("pirate_male") + " " + NameList.RandomName("pirate_color") + NameList.RandomName("pirate_part");

            }
        }

        public override void InitOutfit()
        {
            WipeLayers();

            // black captain's hat
            TricorneHat hat = CaptainsHat();
            hat.LootType = LootType.Newbied;
            AddItem(hat);

            if (Utility.RandomBool())
            {
                Item shirt = new Shirt(Utility.RandomRedHue());
                AddItem(shirt);
            }

            Item sash = new BodySash(0x85);
            Item hair = new Item(Utility.RandomList(0x203B, 0x203C, 0x203D, 0x2044, 0x2045, 0x2047, 0x2049, 0x204A));
            Item pants = new LongPants(Utility.RandomRedHue());
            Item boots = new Boots(Utility.RandomRedHue());
            hair.Hue = Utility.RandomHairHue();
            hair.Layer = Layer.Hair;
            hair.Movable = false;

            Item sword;
            if (Utility.RandomBool())
                sword = new Scimitar();
            else
                sword = new Cutlass();

			sword.LootType = LootType.Newbied;

            AddItem(hair);
            AddItem(sash);
            AddItem(pants);
            AddItem(boots);
            AddItem(sword);

            if (!this.Female)
            {
                Item beard = new Item(Utility.RandomList(0x203E, 0x203F, 0x2040, 0x2041, 0x204B, 0x204C, 0x204D));
                beard.Hue = hair.Hue;
                beard.Layer = Layer.FacialHair;
                beard.Movable = false;
                AddItem(beard);
            }
        }

        public override void OnThink()
        {
            if (DateTime.Now >= m_NextSpeechTime)
            {
                Mobile combatant = this.Combatant;

                if (combatant != null && combatant.Map == this.Map && combatant.InRange(this, 8))
                {
                    int phrase = Utility.Random(4);

                    switch (phrase)
                    {
                        case 0: this.Say(true, "Lights and liver!"); break;
                        case 1: this.Say(true, "Arr! Get ye a-swabbin' or yer life ends now!"); break;
                        case 2: this.Say(true, "I'll rip off yer fins and burn ya to slow fire!"); break;
                        case 3: this.Say(true, "Keel haul ye we will!"); break;
                    }

                    m_NextSpeechTime = DateTime.Now + m_SpeechDelay;
                }

                base.OnThink();
            }

        }

        public override void OnDamagedBySpell(Mobile caster)
        {
            if (caster == this)
                return;

            // Adam: 12% chance to spawn a Bone Knight
            if (Utility.RandomChance(12))
                SpawnBoneKnight(caster);
        }

        public void SpawnBoneKnight(Mobile caster)
        {
            Mobile target = caster;

            if (Map == null || Map == Map.Internal)
                return;

            int helpers = 0;
            ArrayList mobs = new ArrayList();
            IPooledEnumerable eable = this.GetMobilesInRange(10);
            foreach (Mobile m in eable)
            {
                if (m is BoneKnight)
                    ++helpers;

                if (m is PlayerMobile && m.Alive == true && m.Hidden == false && m.AccessLevel <= AccessLevel.Player)
                    mobs.Add(m);
            }
            eable.Free();

            if (helpers < 5)
            {
                BaseCreature helper = new BoneKnight();

                helper.Team = this.Team;
                helper.Map = Map;

                bool validLocation = false;

                // pick a random player to focus on
                //  if there are no players, we will stay with the caster
                if (mobs.Count > 0)
                    target = mobs[Utility.Random(mobs.Count)] as Mobile;

                for (int j = 0; !validLocation && j < 10; ++j)
                {
                    int x = target.X + Utility.Random(3) - 1;
                    int y = target.Y + Utility.Random(3) - 1;
                    int z = Map.GetAverageZ(x, y);

                    if (validLocation = Map.CanFit(x, y, this.Z, 16, CanFitFlags.requireSurface))
                        helper.Location = new Point3D(x, y, Z);
                    else if (validLocation = Map.CanFit(x, y, z, 16, CanFitFlags.requireSurface))
                        helper.Location = new Point3D(x, y, z);
                }

                if (!validLocation)
                    helper.Location = target.Location;

                helper.Combatant = target;
            }
        }

        public override void GenerateLoot()
        {
            int phrase = Utility.Random(2);
            switch (phrase)
            {
                case 0: this.Say(true, "Heh! On to Davy Jones' lockarrr.."); break;
                case 1: this.Say(true, "Sink me!"); break;
            }

            // build 'hoard' loot
            BuildChest();
        }

        public void BuildChest()
        {
            m_MetalChest = new MetalChest();
            m_MetalChest.Name = "Dead Man's Chest";
            m_MetalChest.Hue = Utility.RandomMetalHue();
            m_MetalChest.Movable = false;

            // level 5 chest logic
            m_MetalChest.RequiredSkill = 100;
            m_MetalChest.LockLevel = m_MetalChest.RequiredSkill - 10;
            m_MetalChest.MaxLockLevel = m_MetalChest.RequiredSkill + 40;

            // reset the trap
            m_MetalChest.TrapEnabled = true;
            m_MetalChest.TrapPower = 5 * 25;	// level 5
            m_MetalChest.Locked = true;
            m_MetalChest.TrapType = Utility.RandomBool() ? TrapType.PoisonTrap : TrapType.ExplosionTrap;

            // setup timmed release logic
            string[] lines = new string[4];
            lines[0] = "Movable true";
            lines[1] = "TrapEnabled false";
            lines[2] = "TrapPower 0";
            lines[3] = "Locked true";

            // the chest will become movable in 10-25 minutes
            DateTime SetTime = DateTime.Now + TimeSpan.FromMinutes(Utility.RandomMinMax(10, 25));
            new TimedSet(m_MetalChest, SetTime, lines).MoveItemToIntStorage();

            // add loot
            FillChest();

            // move the chest to world;
            m_MetalChest.MoveToWorld(Location, Map);

        }

        private void FillChest()
        {
            int RaresDropped = 0;
            LogHelper Logger = new LogHelper("PirateChampChest.log", false);

            // 25 piles * 1200 = 30K gold
            for (int ix = 0; ix < 25; ix++)
            {   // force the separate piles
                Gold gold = new Gold(800, 1200);
                gold.Stackable = false;
                m_MetalChest.DropItem(gold);
                gold.Stackable = true;
            }

            // "a smelly old mackerel"
            if (Utility.RandomChance(10))
            {
                Item ii;
                ii = new BigFish();
                ii.Name = "a smelly old mackerel";
                ii.Weight = 5;
                m_MetalChest.DropItem(ii);
                RaresDropped++;
                Logger.Log(LogType.Item, ii);
            }

            // single gold ingot weight 12
            if (Utility.RandomChance(10 * 2))
            {
                Item ii;
                if (Utility.RandomBool())
                    ii = new Item(7145);
                else
                    ii = new Item(7148);

                ii.Weight = 12;
                m_MetalChest.DropItem(ii);
                RaresDropped++;
                Logger.Log(LogType.Item, ii);
            }

            // 3 gold ingots 12*3
            if (Utility.RandomChance(5 * 2))
            {
                Item ii;
                if (Utility.RandomBool())
                    ii = new Item(7146);
                else
                    ii = new Item(7149);

                ii.Weight = 12 * 3;
                m_MetalChest.DropItem(ii);
                RaresDropped++;
                Logger.Log(LogType.Item, ii);
            }

            // 5 gold ingots 12*5
            if (Utility.RandomChance(1 * 2))
            {
                Item ii;
                if (Utility.RandomBool())
                    ii = new Item(7147);
                else
                    ii = new Item(7150);

                ii.Weight = 12 * 5;
                m_MetalChest.DropItem(ii);
                RaresDropped++;
                Logger.Log(LogType.Item, ii);
            }

            // single silver ingot weight 6
            if (Utility.RandomChance(10 * 2))
            {
                Item ii;
                if (Utility.RandomBool())
                    ii = new Item(7157);
                else
                    ii = new Item(7160);

                ii.Weight = 6;
                m_MetalChest.DropItem(ii);
                RaresDropped++;
                Logger.Log(LogType.Item, ii);
            }

            // 3 silver ingots 6*3
            if (Utility.RandomChance(5 * 2))
            {
                Item ii;
                if (Utility.RandomBool())
                    ii = new Item(7158);
                else
                    ii = new Item(7161);

                ii.Weight = 6 * 3;
                m_MetalChest.DropItem(ii);
                RaresDropped++;
                Logger.Log(LogType.Item, ii);
            }

            // 5 silver ingots 6*5
            if (Utility.RandomChance(1 * 2))
            {
                Item ii;
                if (Utility.RandomBool())
                    ii = new Item(7159);
                else
                    ii = new Item(7162);

                ii.Weight = 6 * 5;
                m_MetalChest.DropItem(ii);
                RaresDropped++;
                Logger.Log(LogType.Item, ii);
            }

            // rolled map w1
            if (Utility.RandomChance(20 * 2))
            {
                Item ii;
                if (Utility.RandomBool())
                    ii = new Item(5357);
                else
                    ii = new Item(5358);

                ii.Weight = 1;
                m_MetalChest.DropItem(ii);
                RaresDropped++;
                Logger.Log(LogType.Item, ii);
            }

            // ship plans
            if (Utility.RandomChance(10 * 2))
            {
                Item ii;
                if (Utility.RandomBool())
                    ii = new Item(5361);
                else
                    ii = new Item(5362);

                ii.Weight = 1;
                m_MetalChest.DropItem(ii);
                RaresDropped++;
                Logger.Log(LogType.Item, ii);
            }

            // ship model
            if (Utility.RandomChance(5 * 2))
            {
                Item ii;
                if (Utility.RandomBool())
                    ii = new Item(5363);
                else
                    ii = new Item(5364);

                ii.Weight = 3;
                m_MetalChest.DropItem(ii);
                RaresDropped++;
                Logger.Log(LogType.Item, ii);
            }

            // "scale shield" w6
            if (Utility.RandomChance(1))
            {
                Item ii;
                if (Utility.RandomBool())
                    ii = new Item(7110);
                else
                    ii = new Item(7111);

                ii.Name = "scale shield";
                ii.Weight = 6;
                m_MetalChest.DropItem(ii);
                RaresDropped++;
                Logger.Log(LogType.Item, ii);
            }

            // level 5 chest regs & gems
            TreasureMapChest.PackRegs(m_MetalChest, 5 * 10);
            TreasureMapChest.PackGems(m_MetalChest, 5 * 5);

            // level 5 magic items
            DungeonTreasureChest.PackMagicItem(m_MetalChest, 3, 3, 0.20);
            DungeonTreasureChest.PackMagicItem(m_MetalChest, 3, 3, 0.10);
            DungeonTreasureChest.PackMagicItem(m_MetalChest, 3, 3, 0.05);

            // an a level 5 treasure map
            m_MetalChest.DropItem(new TreasureMap(5, Map.Felucca));

            Logger.Finish();
        }

        public override void Damage(int amount, Mobile from)
        {
            Mobile combatant = this.Combatant;

            if (combatant != null && combatant.Map == this.Map && combatant.InRange(this, 8))
            {
                if (Utility.RandomBool())
                {

                    int phrase = Utility.Random(4);

                    switch (phrase)
                    {
                        case 0: this.Say(true, "Har! The mackerel wiggles!"); break;
                        case 1: this.Say(true, "Ye stink like a rotten clam! Bring it on yet!?"); break;
                        case 2: this.Say(true, "Arr, treacherous monkey!"); break;
                        case 3: this.Say(true, "Ye'll not get my swag!"); break;
                    }

                    m_NextSpeechTime = DateTime.Now + m_SpeechDelay;
                }
            }

            base.Damage(amount, from);
        }

        public override void GetContextMenuEntries(Mobile from, ArrayList list)
        {
            base.GetContextMenuEntries(from, list);

            for (int i = 0; i < list.Count; ++i)
            {
                if (list[i] is ContextMenus.PaperdollEntry)
                    list.RemoveAt(i--);
            }
        }

        private TricorneHat CaptainsHat()
        {
            // black captain's hat
            TricorneHat hat = new TricorneHat();
            hat.Name = "a pirate hat";
            hat.Hue = 0x01;
            hat.Dyable = false;
            return hat;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

        }
    }
}
