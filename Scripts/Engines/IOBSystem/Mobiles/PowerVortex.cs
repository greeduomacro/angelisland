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

/* /Scripts/Engines/IOBSystem/PowerVortex.cs
 * ChangeLog
 *	07/03/09, plasma
 *		Toned life leech down
 *	06/30/09, plasma
 *		Added summon masters into special attacks except life-leech
 *	06/07/09, plasma
 *		Tone down damage a bit
 *	05/26/09, plasma
 *		Fix null ref in OnDamage caused by the pet dying mid-method
 *	05/25/09, plasma
 *		- Finished guardian implementation
 *		- Made vortex harder in general
 *		- Added a damage reflect attack
 *		- Added a health-sap attack for tamers
 *	11/11/08, plasma
 *		Made immune to lethal poison
 *  01/09/08, plasma
 *		Initial creation
 */

using System;
using Server;
using Server.Items;
using Server.Regions;
using Server.Engines.IOBSystem;
using System.Collections;
using System.Collections.Generic;

namespace Server.Mobiles
{
	public class PowerVortex : BaseCreature
	{

		private KinSigil m_Sigil;    //sigil reference
		private bool m_TimedOut;			//Set by the sigil if timed out 
		private List<GolemController> m_Guardians = new List<GolemController>();

		public KinSigil Sigil
		{
			get { return m_Sigil; }
		}

		public bool TimedOut
		{
			set { m_TimedOut = value; }
		}

		public override Poison PoisonImmune
		{
			get
			{
				return Poison.Lethal;
			}
		}

		private int GetRandomNoise()
		{
			switch (Utility.RandomMinMax(1, 5))
			{
				//Shadowlord noises (it's the GCL, honest!)
				case 1: return 0x47E;
				case 2: return 0x47F;
				case 3: return 0x480;
				case 4: return 0x481;
				case 5: return 0x482;
			}
			return 0x47F;
		}

		public override void OnDamage(int amount, Mobile from, bool willKill)
		{
			base.OnDamage(amount, from, willKill);

			Mobile controlMaster = null;

			if (amount == 0 || from == null || from.Deleted || !from.Alive || willKill) return;

			if (from is BaseCreature)
			{
				//pet
				BaseCreature bc = ((BaseCreature)from);
				if (bc.Controlled && bc.ControlMaster != null)
				{
					controlMaster = ((Mobile)bc.ControlMaster);
					
					//This is someone's pet, reflect the damage back on them double
					from.Damage(amount*2, this);

					//Have a small chance to perform a life-leeching move on the tamer
					if (controlMaster.Alive && Utility.RandomChance(5))
					{
						int max = 20;
						int min = 5;

						if (controlMaster.Hits < max)
						{
							max = controlMaster.Hits - 1;
						}
						if (controlMaster.Hits < min)
						{
							min = controlMaster.Hits / 2;
						}
						int damage = Utility.RandomMinMax(min, max);
						controlMaster.Damage(damage, this);
						this.Hits += damage;
						Effects.PlaySound(Location, Map, GetRandomNoise());
						controlMaster.SendMessage("You feel the power vortex sapping your health!");
					}
				}
				else if (bc.Summoned && bc.SummonMaster != null)
				{
					controlMaster = ((Mobile)bc.SummonMaster);
					//This is someone's summon , reflect the damage back on them
					from.Damage(amount, this);
					if (Utility.RandomChance(10)) Effects.PlaySound(Location, Map, GetRandomNoise());
				}
			}
			else
			{
				//player				
				if (amount / 2 > from.Hits)
				{
					//this will never actually kill a player, but will come close.
					from.Damage(from.Hits - 1, this);
				}
				else
				{
					from.Damage(amount / 2, this);
				}
			}

			int maxGuardians = 0;
			if (Hits < (HitsMax * 0.7)) maxGuardians = 2;
			if (Hits < (HitsMax * 0.5)) maxGuardians = 3;
			if (Hits < (HitsMax * 0.3)) maxGuardians = 4;

			DefragGuardians();
			if (m_Guardians.Count < maxGuardians && Utility.RandomChance(10))
			{
				GolemController gc = new GolemController();

				if (controlMaster != null && controlMaster.Alive && !controlMaster.Deleted)
				{
					gc.ForceTarget(controlMaster);
				}

				gc.Home = Location;

				Point3D location = Location;

				if (from.Hidden)
				{
					from.SendMessage("You are forced out of hiding by the immense power!");
					from.RevealingAction();
				}

				m_Guardians.Add(gc);
				gc.MoveToWorld(location, Map);
				gc.OnThink();

				Effects.PlaySound(Location, Map, 0x81);
			}
		}

		private void DefragGuardians()
		{
			for (int i = m_Guardians.Count - 1; i >= 0; --i)
			{
				if (m_Guardians[i] == null || m_Guardians[i].Deleted || !m_Guardians[i].Alive)
					m_Guardians.RemoveAt(i);
			}
		}

		public override void OnDamagedBySpell(Mobile from)
		{
			base.OnDamagedBySpell(from);
		}

		public override bool DeleteCorpseOnDeath { get { return true; } }

		[Constructable]
		public PowerVortex(KinSigil sigil)
			: base(AIType.AI_Melee, FightMode.Aggressor | FightMode.Weakest | FightMode.Int, 6, 1, 0.199, 0.350)
		{
			Name = "a power vortex";
			Body = 164;

			m_Sigil = sigil;

			SetStr(300);
			SetDex(200);
			SetInt(100);

			SetHits(11000);  //temp : not sure how much this will be yet
			SetStam(250);
			SetMana(0);

			SetDamage(25, 30);

			SetSkill(SkillName.MagicResist, 200.0);
			SetSkill(SkillName.Tactics, 30.0);
			SetSkill(SkillName.Wrestling, 100.0);

			CantWalk = true;

			Fame = 0;
			Karma = 0;
			Hue = 443;
			VirtualArmor = 60;
		}

		public override int GetAngerSound()
		{
			return 0x15;
		}

		public override int GetAttackSound()
		{
			return 0x28;
		}

		public PowerVortex(Serial serial)
			: base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)1); // version PLASMA:check this was v0 before??
			writer.WriteMobileList(new ArrayList(m_Guardians));
			writer.Write(m_Sigil);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
			switch (version)
			{
				case 1:
					{
						foreach (Mobile m in reader.ReadMobileList())
							if (m is GolemController)
								m_Guardians.Add(m as GolemController);
						goto case 0;
					}
				case 0:
					{
						m_Sigil = (KinSigil)reader.ReadItem();
						break;
					}
			}
		}

		public override void OnDeath(Container c)
		{
			base.OnDeath(c);
			//tell the sigil we've died if it didn't time out
			if (!m_TimedOut)
				m_Sigil.VortexDeath();
		}

		public override void OnDelete()
		{
			base.OnDelete();
			if (m_Sigil != null)
				m_Sigil.VortexDelete();
		}

		public override void Damage(int amount)
		{



			base.Damage(amount);
		}
	}
}
