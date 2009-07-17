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

/* Scripts\Engines\ChampionSpawn\Champs\Neira.cs
 * CHANGELOG
 *  03/09/07, plasma    
 *      Removed cannedevil namespace reference
 *  01/05/07, plasma
 *      Changed CannedEvil namespace to ChampionSpawn for cleanup!
 *	9/12/06, Adam
 *		- Rename to weapon "Corpse Cleaver"
 *		- Add magic attributes to weapon (when it drops) to make it more rare (vanq ia more rare than power)
 *  9/11/06, Rhiannon
 *		Added confirmation that the weapon Neira is carrying when she dies is a scimitar.
 *  9/10/06, Rhiannon
 *		Named sword "Unholy Terror"
 *  8/31/06, Rhiannon
 *		Added 3% chance for Neira to drop her sword.
 *  8/16/06, Rhiannon
 *		Changed speed settings to match SpeedInfo table.
 *	7/26/05, erlein
 *		Automated removal of AoS resistance related function calls. 6 lines removed.
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 *  3/23/04, mith
 *		Removed spawn of gold items in pack.
 */

using System;
using System.Collections;
using Server;
using Server.Items;
using Server.Engines.ChampionSpawn;

namespace Server.Mobiles
{
    public class Neira : BaseChampion
    {
        public override ChampionSkullType SkullType { get { return ChampionSkullType.Death; } }

        [Constructable]
        public Neira()
            : base(AIType.AI_Mage, 0.175, 0.350)
        {
            Name = "Neira";
            Title = "the necromancer";
            Body = 401;
            Hue = 0x83EC;
            BardImmune = true;

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

            Fame = 22500;
            Karma = -22500;

            VirtualArmor = 30;
            Female = true;

            Item shroud = new HoodedShroudOfShadows();

            shroud.Movable = false;

            AddItem(shroud);

            Scimitar weapon = new Scimitar();

            // 3% chance for Neira's sword to drop.
            if (Utility.RandomDouble() <= 0.97)
            {
                weapon.LootType = LootType.Newbied;
            }
            else
            {
                weapon.LootType = LootType.Regular;
            }

            weapon.Name = "Corpse Cleaver";
            weapon.Hue = 38;
            weapon.Movable = false;

            AddItem(weapon);

            new SkeletalMount().Rider = this;
        }

        public override void GenerateLoot()
        {
            //AddLoot( LootPack.UltraRich, 4 );
            //AddLoot( LootPack.FilthyRich );
            //AddLoot( LootPack.Meager );
        }

        public override bool OnBeforeDeath()
        {
            IMount mount = this.Mount;

            if (mount != null)
                mount.Rider = null;

            if (mount != null && mount is Mobile)
                ((Mobile)mount).Delete();

            // Neira's sword can't drop unless it's movable
            if (Weapon is Scimitar)
            {	// add some attributes now to make it a tad more interesting
                Scimitar weapon = Loot.ImbueWeaponOrArmor(Weapon as Scimitar, 6, 0, false) as Scimitar;
                weapon.Movable = true;
            }

            return base.OnBeforeDeath();
        }

        public override bool AlwaysMurderer { get { return true; } }
        public override Poison PoisonImmune { get { return Poison.Deadly; } }
        public override bool AutoDispel { get { return true; } }

        public override bool ShowFameTitle { get { return false; } }
        public override bool ClickTitle { get { return false; } }

        public override void OnGaveMeleeAttack(Mobile defender)
        {
            base.OnGaveMeleeAttack(defender);

            if (0.1 >= Utility.RandomDouble()) // 10% chance to drop or throw an unholy bone
                AddUnholyBone(defender, 0.25);
        }

        public override void Damage(int amount, Mobile from)
        {
            base.Damage(amount, from);

            if (from != null && from is PlayerMobile && from.InRange(this, 10) || from != null && from is BaseCreature && ((BaseCreature)from).Controlled)
            {
                if (0.1 >= Utility.RandomDouble()) // 10% chance to drop or throw an unholy bone
                    AddUnholyBone(from, 0.25);
            }
        }

        public override void AlterDamageScalarFrom(Mobile caster, ref double scalar)
        {
            base.AlterDamageScalarFrom(caster, ref scalar);

            if (0.1 >= Utility.RandomDouble()) // 10% chance to throw an unholy bone
                AddUnholyBone(caster, 1.0);
        }

        public void AddUnholyBone(Mobile target, double chanceToThrow)
        {
            if (chanceToThrow >= Utility.RandomDouble())
            {
                Direction = GetDirectionTo(target);
                MovingEffect(target, 0xF7E, 10, 1, true, false, 0x496, 0);
                new DelayTimer(this, target).Start();
            }
            else
            {
                new UnholyBone().MoveToWorld(Location, Map);
            }
        }

        private class DelayTimer : Timer
        {
            private Mobile m_Mobile;
            private Mobile m_Target;

            public DelayTimer(Mobile m, Mobile target)
                : base(TimeSpan.FromSeconds(1.0))
            {
                m_Mobile = m;
                m_Target = target;
            }

            protected override void OnTick()
            {
                if (m_Mobile.CanBeHarmful(m_Target))
                {
                    m_Mobile.DoHarmful(m_Target);
                    AOS.Damage(m_Target, m_Mobile, Utility.RandomMinMax(10, 20), 100, 0, 0, 0, 0);
                    new UnholyBone().MoveToWorld(m_Target.Location, m_Target.Map);
                }
            }
        }

        public Neira(Serial serial)
            : base(serial)
        {
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
