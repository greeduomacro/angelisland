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

/* Scripts/Engines/AI/AI/HumanMageAI.cs
 * CHANGELOG
 *	1/1/09, Adam
 *		Add new Serialization model for creature AI.
 *		BaseCreature will already be serializing this data when I finish the upgrade, so all you need to do is add your data to serialize. 
 *		Make sure to use the SaveFlags to optimize saves by not writing stuff that is non default.
 *	12/30/08, Adam
 *		Make sure equipped weapons are dropped and reequipped if we are drinking a potion and 
 *			Pot.RequireFreeHand && BasePotion.HasFreeHand(m) == false
 *	12/28/08, Adam
 *		Redesign to work with the all new HybridAI (add PreferMagic() method)
 *  10/31/06, Kit
 *		Fixed bug with trapped pouchs and them being stoled(cancel spell target if item doesnt exsist)
 *  08/13/06, Kit
 *		Various tweaks, added new expermintal movement code RunAround()
 *  12/10/05, Kit
 *		Various tweaks, added in HealPot usage if potions available, check for nox spells if target is poisonable.
 *  11/07/05, Kit
 *		Moved GetPackItems funtion to BaseAI
 *  6/05/05, Kit
 *		Fixed problem with when at below 15 mana mobile would no longer run, but only stand still and attempt to heal
 *  5/30/05, Kit
 *		Initial Creation
 *		HumanMageAI always uses combos, traps pouchs and drinks pots when available,
 *		Casts magic reflect or reactive armor, takes down reflect on enemys before dumping.
 */
using System;
using System.Collections;
using Server.Targeting;
using Server.Network;
using Server.Mobiles;
using Server.Items;
using Server.Spells;
using Server.Spells.First;
using Server.Spells.Second;
using Server.Spells.Third;
using Server.Spells.Fourth;
using Server.Spells.Fifth;
using Server.Spells.Sixth;
using Server.Spells.Seventh;
using Server.Misc;
using Server.Regions;
using Server.SkillHandlers;
using CalcMoves = Server.Movement.Movement;
using MoveImpl = Server.Movement.MovementImpl;


namespace Server.Mobiles
{
	public class HumanMageAI : MageAI
	{
		private bool m_EnemyCountersPara;
		private bool m_RegainingMana;

		public bool EnemyCountersPara { get { return m_EnemyCountersPara; } set { m_EnemyCountersPara = value; } }
		public bool RegainingMana { get { return m_RegainingMana; } set { m_RegainingMana = value; } }

		public override bool SmartAI
		{
			get { return true; }
		}

		public HumanMageAI(BaseCreature m)
			: base(m)
		{
			DmgSlowsMovement = false;
			CanRun = true;
			UsesPotions = false;
		}

		public virtual bool PreferMagic()
		{
			return true;
		}

		public virtual Pouch FindPouch(Mobile from)
		{
			ArrayList list = GetPackItems(from);
			Pouch pouch;

			foreach (Item item in list)
			{
				if (item is Pouch)
				{
					pouch = (Pouch)item;
					return pouch;
				}
			}

			return null;
		}

		public virtual void UseTrapPouch(Mobile from)
		{
			Pouch pouch = FindPouch(from);
			if (pouch != null && pouch.TrapType == TrapType.MagicTrap)
				pouch.ExecuteTrap(from);
		}

		public virtual void TrapPouch(Mobile from)
		{
			Spell spell = null;
			Pouch pouch = FindPouch(from);
			if (pouch != null && pouch.TrapType == TrapType.None)
			{
				spell = new MagicTrapSpell(from, null);
				spell.Cast();
			}
		}

		public virtual bool CheckCanPoison(Mobile m)
		{
			if (m is PlayerMobile)
				return true;
			if (m != null && m is BaseCreature)
			{
				double total = m_Mobile.Skills[SkillName.Magery].Value + m_Mobile.Skills[SkillName.Poisoning].Value;
				int level;

				if (total > 170.0)
					level = 2;
				else if (total > 130.0)
					level = 1;
				else
					level = 0;


				Poison p = ((BaseCreature)m).PoisonImmune;

				if (p != null && p.Level >= level)
					return false;
			}
			return true;

		}
		public override Spell ChooseSpell(Mobile c)
		{

			if (c is PlayerMobile && SmartAI && (c.Spell is MagicTrapSpell || c.Spell is MagicArrowSpell))
			{
				m_EnemyCountersPara = true;
			}

			if (c.Int > 70 && m_Mobile.MagicDamageAbsorb <= 0 && m_Mobile.Mana > 20 && m_Mobile.Hits > 60 && m_Mobile.CanBeginAction(typeof(DefensiveSpell)))
			{
				Spell temp = c.Spell as Spell;

				if (temp == null || (temp != null && temp.IsCasting && (int)temp.Circle <= (int)SpellCircle.Fourth))
					return new MagicReflectSpell(m_Mobile, null);
			}

			if (c.Dex > 60 && m_Mobile.MeleeDamageAbsorb <= 0 && m_Mobile.Mana > 20 && m_Mobile.Hits > 30 && m_Mobile.CanBeginAction(typeof(DefensiveSpell)))
				return new ReactiveArmorSpell(m_Mobile, null);


			Spell spell = null;

			int healChance = (m_Mobile.Hits == 0 ? m_Mobile.HitsMax : (m_Mobile.HitsMax / m_Mobile.Hits));

			switch (Utility.Random(1 + healChance))
			{
				default:
				case 0: // Heal ourself
					{
						if (HealPotCount >= 1 && m_Mobile.Hits < (m_Mobile.HitsMax - 30))
							DrinkHeal(m_Mobile);
						else if (m_Mobile.Hits < (m_Mobile.HitsMax - 35) && m_Mobile.Hits >= 45)
							spell = new GreaterHealSpell(m_Mobile, null);
						else if (m_Mobile.Hits < (m_Mobile.HitsMax - 10))
							spell = new HealSpell(m_Mobile, null);
						break;
					}

				case 1: // Set up a combo
					{
						//para them and med up until we have mana for a dump
						if (m_Mobile.Mana < 85 && m_Mobile.Mana > 2)
						{
							m_RegainingMana = true;
							//if there low on life and we have the mana try an finish them		
							if (m_Mobile.Mana > 20 && c.Hits < 28)
								spell = new EnergyBoltSpell(m_Mobile, null);

							if (m_Mobile.Mana > 12 && c.Hits < 15)
								spell = new LightningSpell(m_Mobile, null);

							if (c.Paralyzed && !c.Poisoned)
							{
								if (c.Hits < 45 && m_Mobile.Mana > 40)
									spell = new ExplosionSpell(m_Mobile, null);

								if (c.Hits < 30)
									spell = new EnergyBoltSpell(m_Mobile, null);

								m_Mobile.DebugSay("I am going to meditate");

								m_Mobile.UseSkill(SkillName.Meditation);
							}
							else if (!c.Poisoned && m_EnemyCountersPara == false && m_Mobile.Mana > 40)
							{
								spell = new ParalyzeSpell(m_Mobile, null);
							}
							else
							{
								if (m_Mobile.InRange(c, 4))
									RunFrom(c);
								if (!m_Mobile.InRange(c, 6))
									RunTo(c, CanRun);

								//m_Mobile.UseSkill( SkillName.Meditation );

							}
						}

						if (m_Mobile.Mana > 85)
						{
							m_RegainingMana = false;
							Combo = 0;

						}

						break;
					}
			}

			return spell;
		}

		public override Spell DoCombo(Mobile c)
		{
			//m_Mobile.Say("doing human AI combo");
			if (c is PlayerMobile && SmartAI && (c.Spell is MagicTrapSpell || c.Spell is MagicArrowSpell))
			{
				m_EnemyCountersPara = true;
			}

			Spell spell = null;

			if (Combo == 0)
			{
				//m_Mobile.Say( "combo phase 1" );
				spell = new ExplosionSpell(m_Mobile, null);
				++Combo; // Move to next spell

			}

			if (Combo == 1)
			{
				//m_Mobile.Say( "combo phase 1" );
				spell = new ExplosionSpell(m_Mobile, null);
				++Combo; // Move to next spell

			}
			else if (Combo == 2)
			{
				//m_Mobile.Say( "combo phase 2" );
				if (!c.Poisoned && (CheckCanPoison(c)))
					spell = new PoisonSpell(m_Mobile, null);

				++Combo; // Move to next spell

			}

			else if (Combo == 3)
			{

				//m_Mobile.Say( "combo phase 3" );
				if (c.Poisoned || !CheckCanPoison(c))
					spell = new WeakenSpell(m_Mobile, null);

				if (!c.Poisoned && (CheckCanPoison(c)))
					spell = new PoisonSpell(m_Mobile, null);

				++Combo; // Move to next spell

			}

			else if (Combo == 4)
			{
				//	m_Mobile.Say( "combo phase 4 ebolt" );

				if (!c.Poisoned && (CheckCanPoison(c)))
				{
					spell = new PoisonSpell(m_Mobile, null);
					Combo = 4;
				}

				else
					spell = new EnergyBoltSpell(m_Mobile, null);

				++Combo; // Move to next spell

			}
			else if (Combo == 5)
			{
				//m_Mobile.Say( "combo phase 5" );
				if (c.Poisoned)
				{
					if (c.Hits < 20 && m_Mobile.Mana >= 20)
						spell = new EnergyBoltSpell(m_Mobile, null);

					else
						spell = new HarmSpell(m_Mobile, null);

					Combo = 5; // Move to next spell

				}

				if (!c.Poisoned)
				{
					if (m_Mobile.Mana > 20)
						spell = new EnergyBoltSpell(m_Mobile, null);

					if (m_Mobile.Mana < 19)
						spell = new LightningSpell(m_Mobile, null);
					Combo = -1; // Reset combo state

				}

			}

			return spell;
		}

		public override bool DoActionCombat()
		{
			m_Mobile.DebugSay("doing Base HumanMageAI combataction");
			Mobile c = m_Mobile.Combatant;
			m_Mobile.Warmode = true;


			if (c == null || c.Deleted || !c.Alive || c.IsDeadBondedPet || !m_Mobile.CanSee(c) || !m_Mobile.CanBeHarmful(c, false) || c.Map != m_Mobile.Map)
			{
				// Our combatant is deleted, dead, hidden, or we cannot hurt them
				// Try to find another combatant

				if (AcquireFocusMob(m_Mobile.RangePerception, m_Mobile.FightMode, false, false, true))
				{
					if (m_Mobile.Debug)
						m_Mobile.DebugSay("Something happened to my combatant, so I am going to fight {0}", m_Mobile.FocusMob.Name);

					m_Mobile.Combatant = c = m_Mobile.FocusMob;
					m_Mobile.FocusMob = null;
				}
				else
				{
					m_Mobile.DebugSay("Something happened to my combatant, and nothing is around. I am on guard.");
					Action = ActionType.Guard;
					return true;
				}
			}

			if (!m_Mobile.InLOS(c))
			{
				if (AcquireFocusMob(m_Mobile.RangePerception, m_Mobile.FightMode, false, false, true))
				{
					m_Mobile.Combatant = c = m_Mobile.FocusMob;
					m_Mobile.FocusMob = null;
				}
			}

			if (c is PlayerMobile && SmartAI && (c.Spell is MagicTrapSpell || c.Spell is MagicArrowSpell))
			{
				m_EnemyCountersPara = true;
			}

			if (c.Paralyzed || c.Frozen)
			{
				if (m_Mobile.InRange(c, 1))
					RunFrom(c);
			}

			if (m_Mobile.Paralyzed)
			{
				UseTrapPouch(m_Mobile);

			}
			TrapPouch(m_Mobile);

			if (SmartAI && !m_Mobile.StunReady && m_Mobile.Skills[SkillName.Wrestling].Value >= 80.0 && m_Mobile.Skills[SkillName.Anatomy].Value >= 80.0)
				EventSink.InvokeStunRequest(new StunRequestEventArgs(m_Mobile));

			if (!m_Mobile.InRange(c, m_Mobile.RangePerception))
			{
				// They are somewhat far away, can we find something else?

				if (AcquireFocusMob(m_Mobile.RangePerception, m_Mobile.FightMode, false, false, true))
				{
					m_Mobile.Combatant = m_Mobile.FocusMob;
					m_Mobile.FocusMob = null;
				}
				else if (!m_Mobile.InRange(c, m_Mobile.RangePerception * 3))
				{
					m_Mobile.Combatant = null;
				}

				c = m_Mobile.Combatant;

				if (c == null)
				{
					m_Mobile.DebugSay("My combatant has fled, so I am on guard");
					Action = ActionType.Guard;

					return true;
				}
			}

			if (m_Mobile.Hits < m_Mobile.HitsMax * 20 / 100)
			{
				// We are low on health, should we flee?

				bool flee = false;

				if (m_Mobile.Hits < c.Hits)
				{
					// We are more hurt than them

					int diff = c.Hits - m_Mobile.Hits;

					flee = (Utility.Random(0, 100) > (10 + diff)); // (10 + diff)% chance to flee
				}
				else
				{
					flee = Utility.Random(0, 100) > 10; // 10% chance to flee
				}

				if (flee)
				{
					if (m_Mobile.Debug)
						m_Mobile.DebugSay("I am going to flee from {0}", c.Name);

					Action = ActionType.Flee;
					return true;
				}
			}


			if (m_Mobile.Spell == null && DateTime.Now > m_NextCastTime && m_Mobile.InRange(c, 12))
			{
				// We are ready to cast a spell
				Spell spell = null;
				Mobile toDispel = FindDispelTarget(true);

				//try an cure with a pot first if the poison is serious or where in the middle of dumping
				if (UsesPotions && (m_Mobile.Poisoned && m_Mobile.Poison.Level >= 3) || m_Mobile.Poisoned && Combo != -1)
				{
					DrinkCure(m_Mobile);
				}

				if (m_Mobile.Poisoned) // Top cast priority is cure
				{
					spell = new CureSpell(m_Mobile, null);
					try
					{
						if ((((m_Mobile.Skills[SkillName.Magery].Value / (m_Mobile.Poison.Level + 1)) - 20) * 7.5) > 50)
						{
							spell = new CureSpell(m_Mobile, null);
						}
						else
						{
							spell = new ArchCureSpell(m_Mobile, null);
						}
					}
					catch
					{
						spell = new CureSpell(m_Mobile, null);
					}
				}

				//were hurt they have atleast half life and were to low on mana to finish them start healing
				else if (m_Mobile.Hits < 70 && c.Hits > 50 && m_Mobile.Mana < 30)
					spell = new HealSpell(m_Mobile, null);

				else if (toDispel != null) // Something dispellable is attacking us
				{
					spell = DoDispel(toDispel);
				}
				//take down reflect on are enemy if its up
				else if (c.MagicDamageAbsorb > 5)
				{
					m_Mobile.DebugSay("Takeing Down Reflect");
					spell = new FireballSpell(m_Mobile, null);
				}
				else if (Combo != -1) // We are doing a spell combo
				{
					spell = DoCombo(c);
				}
				else
				{
					spell = ChooseSpell(c);
				}

				if (SmartAI && toDispel != null)
				{
					if (m_Mobile.InRange(toDispel, 10))
						RunFrom(toDispel);
					else if (!m_Mobile.InRange(toDispel, 12))
						RunTo(toDispel, CanRun);
				}
				else
				{
					if (m_RegainingMana == false)
						RunTo(c, CanRun);
				}

				if (spell != null && spell.Cast())
				{
					TimeSpan delay;
					//spell cast time is equal to the delay for the spells.
					delay = spell.GetCastDelay() + spell.GetCastRecovery();

					m_NextCastTime = DateTime.Now + delay;
				}
			}
			else if ((m_Mobile.Spell == null || !m_Mobile.Spell.IsCasting) && m_RegainingMana == false)
			{
				RunTo(c, CanRun);
			}

			return true;
		}

		public override void ProcessTarget(Target targ)
		{
			bool isDispel = (targ is DispelSpell.InternalTarget);
			bool isParalyze = (targ is ParalyzeSpell.InternalTarget);
			bool isTeleport = (targ is TeleportSpell.InternalTarget);
			bool isReveal = (targ is RevealSpell.InternalTarget);
			bool isTrap = (targ is MagicTrapSpell.InternalTarget);
			bool teleportAway = false;

			Mobile toTarget = null;

			if (isTrap)
			{
				Pouch p = FindPouch(m_Mobile);
				if (p != null)
				{
					targ.Invoke(m_Mobile, p);
				}
				else
					targ.Cancel(m_Mobile, TargetCancelType.Canceled);
			}


			if (isReveal)
			{
				targ.Invoke(m_Mobile, m_Mobile);
			}
			if (isDispel)
			{
				toTarget = FindDispelTarget(false);

				if (!SmartAI && toTarget != null)
					RunTo(toTarget, CanRun);
				else if (toTarget != null && m_Mobile.InRange(toTarget, 10))
					RunFrom(toTarget);
			}

			else if (SmartAI && (isParalyze || isTeleport))
			{
				toTarget = FindDispelTarget(true);

				if (toTarget == null)
				{
					toTarget = m_Mobile.Combatant;

					if (toTarget != null)
						RunTo(toTarget, CanRun);
				}
				else if (m_Mobile.InRange(toTarget, 10))
				{
					RunFrom(toTarget);
					teleportAway = true;
				}
				else
				{
					teleportAway = true;
				}
			}

			else
			{
				if (m_Mobile.ControlOrder == OrderType.Come && isTeleport)
				{
					toTarget = m_Mobile.ControlMaster;
				}
				else
				{
					toTarget = m_Mobile.Combatant;
				}

				if (toTarget != null)
					RunTo(toTarget, CanRun);
			}

			if ((targ.Flags & TargetFlags.Harmful) != 0 && toTarget != null)
			{
				if ((targ.Range == -1 || m_Mobile.InRange(toTarget, targ.Range)) && m_Mobile.CanSee(toTarget) && m_Mobile.InLOS(toTarget))
				{
					targ.Invoke(m_Mobile, toTarget);
				}
				else if (isDispel)
				{
					targ.Cancel(m_Mobile, TargetCancelType.Canceled);
				}
			}
			else if ((targ.Flags & TargetFlags.Beneficial) != 0)
			{
				targ.Invoke(m_Mobile, m_Mobile);
			}
			else if (isTeleport && toTarget != null)
			{
				Map map = m_Mobile.Map;

				if (map == null)
				{
					targ.Cancel(m_Mobile, TargetCancelType.Canceled);
					return;
				}

				int px, py;

				if (teleportAway)
				{
					int rx = m_Mobile.X - toTarget.X;
					int ry = m_Mobile.Y - toTarget.Y;

					double d = m_Mobile.GetDistanceToSqrt(toTarget);

					px = toTarget.X + (int)(rx * (10 / d));
					py = toTarget.Y + (int)(ry * (10 / d));
				}
				else
				{
					px = toTarget.X;
					py = toTarget.Y;
				}

				for (int i = 0; i < m_Offsets.Length; i += 2)
				{
					int x = m_Offsets[i], y = m_Offsets[i + 1];

					Point3D p = new Point3D(px + x, py + y, 0);

					LandTarget lt = new LandTarget(p, map);

					if ((targ.Range == -1 || m_Mobile.InRange(p, targ.Range)) && m_Mobile.InLOS(lt) && map.CanSpawnMobile(px + x, py + y, lt.Z) && !SpellHelper.CheckMulti(p, map))
					{
						targ.Invoke(m_Mobile, lt);
						return;
					}
				}

				int teleRange = targ.Range;

				if (teleRange < 0)
					teleRange = 12;

				for (int i = 0; i < 10; ++i)
				{
					Point3D randomPoint = new Point3D(m_Mobile.X - teleRange + Utility.Random(teleRange * 2 + 1), m_Mobile.Y - teleRange + Utility.Random(teleRange * 2 + 1), 0);

					LandTarget lt = new LandTarget(randomPoint, map);

					if (m_Mobile.InLOS(lt) && map.CanSpawnMobile(lt.X, lt.Y, lt.Z) && !SpellHelper.CheckMulti(randomPoint, map))
					{
						targ.Invoke(m_Mobile, new LandTarget(randomPoint, map));
						return;
					}
				}

				targ.Cancel(m_Mobile, TargetCancelType.Canceled);
			}
			else
			{
				targ.Cancel(m_Mobile, TargetCancelType.Canceled);
			}
		}

		#region Serialize
		private SaveFlags m_flags;

		[Flags]
		private enum SaveFlags
		{	// 0x00 - 0x800 reserved for version
			unused = 0x1000
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			int version = 1;								// current version (up to 4095)
			m_flags = m_flags | (SaveFlags)version;			// save the version and flags
			writer.Write((int)m_flags);

			// add your version specific stuffs here.
			// Make sure to use the SaveFlags for conditional Serialization
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			m_flags = (SaveFlags)reader.ReadInt();				// grab the version an flags
			int version = (int)(m_flags & (SaveFlags)0xFFF);	// maskout the version

			// add your version specific stuffs here.
			// Make sure to use the SaveFlags for conditional Serialization
			switch (version)
			{
				default: break;
			}

		}
		#endregion Serialize
	}
}

