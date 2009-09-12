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

/* Scripts/Engines/AI/AI/VampAI.cs
 * ChangeLog:
 *	1/1/09, Adam
 *		Add new Serialization model for creature AI.
 *		BaseCreature will already be serializing this data when I finish the upgrade, so all you need to do is add your data to serialize. 
 *		Make sure to use the SaveFlags to optimize saves by not writing stuff that is non default.
 *	07/23/08, weaver
 *		Automated IPooledEnumerable optimizations. 1 loops updated.
 *	7/5/08, weaver
 *		Fixed comment.
 *	7/4/08, weaver
 *		Additional safety added to new chase mode to prevent null ref exceptions and
 *		handle cases where chase mode is unsuccessful in chasing down the vamp's opponent
 *		(for various reasons).
 *	7/3/08, weaver
 *		Added Chase handling.
 *		Fixed spelling mistake ('peasent').
 *	3/18/08, Adam
 *		Redesign Stun/Hypnotize logic
 *			HYPNOTIZE: may only hypnotize if holding a weapon OR you are a bat (otherwise we stun)
 *				both bats and human-form vamps can hypnotize
 *				Note: WalkingDead and Vampire Champ hold weapons
 *			STUN PUNCH: 50% chance to try an stun punch if we have stamina available, we are human form and our hands are free.
 *				(Hypnotize on the other hand is only used if the vampire has something in his hands)
 *	3/16/08, Adam
 *		Fix selector logic in "Messages to say" region
 *		Fix spelling and grammar
 *		Fix Hypnotize to work: Was not being called unless c.Frozen (which makes no sense)
 *	3/15/08, Adam
 *		general cleanup:
 *			- update calle to DoTransform to match new implementation
 *			- eliminate explicit setting of bool batform. We now use a readonly BatForm prop that's keyed on the body value.
 *  8/13/06, Kit
 *		Change damage movement setting to use new call.
 *	6/11/06, Adam
 *		Convert NavStar console output to DebugSay
 *  12/28/05, Kit
 *		Changed Acquire function to not player only(apperes bugged) now using new fightmode.Player.
 *  12/24/05, Kit
 *		Added flying logic and OnFailedMove() flight logic.
 *	12/23/05, Adam
 *		1. DoActionCombat: Change trg.AccessLevel < AccessLevel.GameMaster to trg.AccessLevel == AccessLevel.Player
 *		Reason: We don't want AccessLevel.Counselor attacked / targeted
 *		2. DoActionCombat: on exception return false;
 *		3. DoActionCombat: Wrap all in a try/catch until we know what crashed:
 *		Exception:
 *		System.NullReferenceException: Object reference not set to an instance of an object.
 *		at Server.Mobiles.VampireAI.DoActionCombat()
 *		at Server.Mobiles.BaseAI.Think()
 *		at Server.Mobiles.AITimer.OnTick()
 *		at Server.Timer.Slice()
 *		at Server.Core.Main(String[] args)
 *  12/23/05, Kit
 *		Added extra null check to pet logic code.
 *  12/18/05 Kit,
 *		Added in detect hidden usage, and life recovery code via attacking animals/etc
 *  12/17/05 Kit, 
 *		Fixed various bugs, corrected grammer on messages, added hypnotize ability use/flee mode
 *	12/10/05 Kit
 *	Initial Creation.
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
using Server.Spells.NPC;
using Server.Misc;
using Server.Regions;
using Server;
using Server.SkillHandlers;
using Server.Scripts.Commands;

namespace Server.Mobiles
{
	public class VampireAI : BaseAI
	{
		private TimeSpan m_StunDelay = TimeSpan.FromSeconds(Utility.RandomMinMax(7, 10)); // time between stun attempts
		public DateTime m_NextStunTime;

		private TimeSpan m_SpeakDelay = TimeSpan.FromSeconds(18); // time between talking
		public DateTime m_NextSpeakTime;

		private TimeSpan m_HypnotizeDelay = TimeSpan.FromSeconds(23); // time between hypnotizeing
		public DateTime m_NextHypnotizeTime;

		private double m_ActiveSpeedFast = 0.15; // old kit value (0.05);		// not serialized
		public double ActiveSpeedFast { get { return m_ActiveSpeedFast; } set { m_ActiveSpeedFast = value; } }
		public double ActiveSpeedNormal { get { return 0.15; } }

		public VampireAI(BaseCreature m)
			: base(m)
		{
			DmgSlowsMovement = false;
			CanRun = true;

			m_NextStunTime = DateTime.Now + m_StunDelay;
			m_NextSpeakTime = DateTime.Now + m_SpeakDelay;
			m_NextHypnotizeTime = DateTime.Now + m_HypnotizeDelay;
		}

		public override void OnActionChanged()
		{
			switch (Action)
			{
				case ActionType.Wander:
					m_Mobile.Warmode = false;
					m_Mobile.Combatant = null;
					m_Mobile.FocusMob = null;
					m_Mobile.CurrentSpeed = m_Mobile.PassiveSpeed;
					break;

				case ActionType.Combat:
					m_Mobile.Warmode = false;
					//m_Mobile.FocusMob = null; //we want to keep are focus mob
					m_Mobile.CurrentSpeed = m_Mobile.ActiveSpeed;
					break;

				case ActionType.Guard:
					base.OnActionChanged();
					break;

				case ActionType.Hunt:
					base.OnActionChanged();
					break;

				case ActionType.NavStar:
					m_Mobile.Warmode = false;
					m_Mobile.FocusMob = null;
					m_Mobile.CurrentSpeed = m_Mobile.ActiveSpeed;
					break;

				case ActionType.Flee:
					m_Mobile.Warmode = true;
					m_Mobile.FocusMob = null;
					m_Mobile.CurrentSpeed = m_Mobile.ActiveSpeed;
					break;

				case ActionType.Interact:
					m_Mobile.Warmode = false;
					m_Mobile.CurrentSpeed = m_Mobile.PassiveSpeed;
					break;

				case ActionType.Backoff:
					m_Mobile.Warmode = false;
					m_Mobile.CurrentSpeed = m_Mobile.PassiveSpeed;
					break;

				// wea: chase action - turn into a bat if not one already
				case ActionType.Chase:
					if (m_Mobile is Vampire && ((Vampire)m_Mobile).BatForm != true && !m_Mobile.Paralyzed) //only do this if were not a bat already
					{
						((Vampire)m_Mobile).DoTransform((Vampire)m_Mobile, 317, ((Vampire)m_Mobile).toBatForm); // become a bat
						m_Mobile.ActiveSpeed = ActiveSpeedFast;
						m_Mobile.CurrentSpeed = ActiveSpeedFast; //speed up where air born now
						m_Mobile.CanFly = true;
					}
					break;
			}
		}


		// wea: added chase action
		public override bool DoActionChase()
		{
			try
			{
				Mobile opponent = m_Mobile.Combatant; //this will get set to null if something happens to the combat timer

				if (opponent != null && !opponent.Deleted && opponent.Alive && !opponent.IsDeadBondedPet && m_Mobile.CanSee(opponent) &&
					m_Mobile.CanBeHarmful(opponent, false) && opponent.Map == m_Mobile.Map && m_Mobile.InLOS(opponent))
				{
					// Make sure they're still out of range and reset to combat mode if they're not
					if (m_Mobile.InRange(opponent, 6))
					{
						// Reset to combat for now
						Action = ActionType.Combat;
						return true;
					}

					// Run to them!
					RunTo(opponent, CanRun);
				}
				else
				{
					// Combatant is null, so fall into regular, combat mode detecting
					Action = ActionType.Combat;
				}
			}
			catch (Exception e)
			{
				// Log an exception
				EventSink.InvokeLogException(new LogExceptionEventArgs(e));
			}

			return base.DoActionChase();
		}


		public override bool DoActionWander()
		{
			m_Mobile.CanFly = false; //we only worry about this dureing combat

			if (m_Mobile.ActiveSpeed == ActiveSpeedFast) //if for some reason were still in bat pursuit mode fix that!
			{
				m_Mobile.ActiveSpeed = ActiveSpeedNormal;
				m_Mobile.CurrentSpeed = ActiveSpeedNormal;
				m_Mobile.CanFly = false;
			}

			if (m_Mobile.Hits < m_Mobile.HitsMax && (FoodAround()))
			{
				Action = ActionType.Combat;
				return true;
			}

			if (AcquireFocusMob(m_Mobile.RangePerception, m_Mobile.FightMode, false, false, true))
			{
				if (m_Mobile.Debug)
					m_Mobile.DebugSay("I have detected {0}, attacking", m_Mobile.FocusMob.Name);

				m_Mobile.Combatant = m_Mobile.FocusMob;
				Action = ActionType.Combat;
				return true;
			}
			else
			{
				base.DoActionWander();
			}

			return true;
		}

		public override bool DoActionGuard()
		{
			m_Mobile.CanFly = false;

			m_Mobile.NextReacquireTime = DateTime.Now;
			if (AcquireFocusMob(m_Mobile.RangePerception, m_Mobile.FightMode, false, false, true))
			{
				if (m_Mobile.Debug)
					m_Mobile.DebugSay("I have detected {0}, attacking", m_Mobile.FocusMob.Name);

				m_Mobile.Combatant = m_Mobile.FocusMob;
				Action = ActionType.Combat;
				return true;
			}
			else
			{
				base.DoActionGuard();
			}

			return true;
		}


		public override bool DoActionCombat()
		{
			try
			{

				Mobile c = m_Mobile.Combatant; //this will get set to null if something happens via combattimer

				//m_Mobile.Warmode = false; //setting this to false will cause combatant to become null!!

				if (m_Mobile.FocusMob != null && m_Mobile.FocusMob.Hidden)
				{
					m_Mobile.UseSkill(SkillName.DetectHidden);
					Target targ = m_Mobile.Target;
					targ.Invoke(m_Mobile, m_Mobile); //target are self
					if (!m_Mobile.FocusMob.Hidden) //we revealed set are combatant back right
					{
						Mobile oldCombatant = m_Mobile.FocusMob;
						m_Mobile.Combatant = oldCombatant; //switch are target back to who we revealed
						c = m_Mobile.Combatant;
					}
				}

				if (c == null || c.Deleted || !c.Alive || c.IsDeadBondedPet || !m_Mobile.CanSee(c) || !m_Mobile.CanBeHarmful(c, false) || c.Map != m_Mobile.Map)
				{
					// Our combatant is deleted, dead, hidden, or we cannot hurt them
					// Try to find another combatant
					m_Mobile.NextReacquireTime = DateTime.Now;
					if (AcquireFocusMob(m_Mobile.RangePerception, m_Mobile.FightMode, false, false, true))
					{
						if (m_Mobile.Debug)
							m_Mobile.DebugSay("Something happened to my combatant, so I am going to fight {0}", m_Mobile.FocusMob.Name);

						m_Mobile.Combatant = c = m_Mobile.FocusMob;
						m_Mobile.FocusMob = c; //remember who we would Acquired so we can reveal them later!

					}
					else
					{
						m_Mobile.DebugSay("Something happened to my combatant, and nothing is around. I am on guard.");
						Action = ActionType.Guard;
						return true;
					}
				}

				//Mobile got to far away find another target
				if (!m_Mobile.InLOS(c))
				{
					if (AcquireFocusMob(m_Mobile.RangePerception, m_Mobile.FightMode, false, false, true))
					{
						m_Mobile.Combatant = c = m_Mobile.FocusMob;
						m_Mobile.FocusMob = null;
					}
				}

				try
				{

					if (c != null && c.Alive && m_Mobile.CanSee(c) && (c is BaseCreature))
					{
						//check if there are players around if so forget about the pet there afraid of us anyways
						IPooledEnumerable eable = m_Mobile.GetMobilesInRange(m_Mobile.RangePerception);
						foreach (Mobile trg in eable)
						{
							if (trg != null && trg != m_Mobile && trg is PlayerMobile && !trg.Hidden && trg.AccessLevel == AccessLevel.Player && trg.Alive && m_Mobile.CanSee(trg))
							{
								//we found a player in range
								c = trg;
								m_Mobile.Combatant = trg;
							}
						}
						eable.Free();
					}
				}
				catch (Exception exc)
				{
					LogHelper.LogException(exc);
					System.Console.WriteLine("catch I: Send to Zen please: ");
					System.Console.WriteLine("Exception caught in Vampire.OnCombat: " + exc.Message);
					System.Console.WriteLine(exc.StackTrace);
					return false;
				}

				// STUN PUNCH: 50% chance to try an stun punch if we have stamina available, we are human form and our hands are free.
				//	(Hypnotize on the other hand is only used if the vampire has something in his hands)
				if (DateTime.Now >= m_NextStunTime)
				{
					bool stunChance = Utility.RandomBool();
					bool isBat = m_Mobile is Vampire && ((Vampire)m_Mobile).BatForm == true;
					bool isFrozen = c != null && c.Frozen;
					if (m_Mobile.Stam >= 40 && stunChance == true && !m_Mobile.StunReady && Fists.HasFreeHands(m_Mobile) && isFrozen == false &&
						m_Mobile.Skills[SkillName.Wrestling].Value >= 80.0 && m_Mobile.Skills[SkillName.Anatomy].Value >= 80.0 &&
						isBat == false)
					{
						EventSink.InvokeStunRequest(new StunRequestEventArgs(m_Mobile));
						m_NextStunTime = DateTime.Now + m_StunDelay;
					}
				}

				if (m_Mobile is Vampire && ((Vampire)m_Mobile).BatForm == true)
					m_Mobile.CanFly = true;

				// wea: switch to chase action type instead of hacking at it
				if (!m_Mobile.InRange(c, 6)) //Mobile is 6 tiles away or more Take to the air unless we're already chasing them!!
				{
					Action = ActionType.Chase;
					RunTo(c, CanRun);
					return true;
				}

				if (m_Mobile.InRange(c, 1)) //ok where close we will walk and look cool vs trying to run all fast 1 tile at a time 
				{	// Design error: See above comment
					if (m_Mobile is Vampire && ((Vampire)m_Mobile).BatForm == true && (CheckNight(m_Mobile)) && (CheckLand())) //change to human form
					{
						((Vampire)m_Mobile).DoTransform((Vampire)m_Mobile, (m_Mobile.Female) ? 0x191 : 0x190, ((Vampire)m_Mobile).Hue, ((Vampire)m_Mobile).toHumanForm);
						m_Mobile.ActiveSpeed = ActiveSpeedNormal;
						m_Mobile.CurrentSpeed = ActiveSpeedNormal;
						m_Mobile.CanFly = false;
					}
					RunTo(c, false); //walk no need to run
				}
				//taunt if there stunned
				if (DateTime.Now >= m_NextSpeakTime && c.Frozen && c is PlayerMobile && m_Mobile is Vampire && ((Vampire)m_Mobile).BatForm == false && m_Mobile.InRange(c, 1))
				{
					string s = GetMsg(c);
					m_Mobile.Say(s);
					m_NextSpeakTime = DateTime.Now + m_SpeakDelay;
				}

				// HYPNOTIZE: may only hypnotize if holding a weapon OR you are a bat (otherwise we stun)
				//	both bats and human-form vamps can hypnotize
				// Note: WalkingDead and Vampire Champ hold weapons
				if (DateTime.Now >= m_NextHypnotizeTime)
				{
					if (c != null)
					{
						bool isBat = m_Mobile is Vampire && ((Vampire)m_Mobile).BatForm == true;
						bool bardPacified = c is BaseCreature && ((BaseCreature)c).BardPacified == true;
						if ((Fists.HasFreeHands(m_Mobile) == false || isBat == true) && bardPacified == false && m_Mobile.InRange(c, 1))
						{
							if (m_Mobile is Vampire)
							{
								if (((Vampire)m_Mobile).Hypnotize(c))
								{
									if (c is PlayerMobile)
										m_Mobile.Emote("Look into my eyes...");
									else
										m_Mobile.Emote("I am your master, do not fight me.");

									m_NextHypnotizeTime = DateTime.Now + m_HypnotizeDelay;
								}
							}
						}
					}
				}

				RunTo(c, CanRun);

				if (m_Mobile.Hits < m_Mobile.HitsMax * 20 / 100 && (c is PlayerMobile || (c is BaseCreature && c.HitsMax >= 200)))
				{
					// We are low on health, should we flee?
					bool flee = false;

					if (m_Mobile.Hits < c.Hits)
					{
						// We are more hurt than them

						int diff = c.Hits - m_Mobile.Hits;

						flee = (Utility.Random(0, 100) < (10 + diff)); // (10 + diff)% chance to flee
					}
					else
					{
						flee = Utility.Random(0, 100) < 10; // 10% chance to flee
					}

					if (flee)
					{
						if (m_Mobile.Debug)
							m_Mobile.DebugSay("I am going to flee from {0}", c.Name);

						Action = ActionType.Flee;
					}
				}
				return true;
			}
			catch (Exception exc)
			{
				LogHelper.LogException(exc);
				System.Console.WriteLine("catch II: Send to Zen please: ");
				System.Console.WriteLine("Exception caught in Vampire.OnCombat: " + exc.Message);
				System.Console.WriteLine(exc.StackTrace);
				return false;
			}
		}

		public bool FoodAround()
		{
			Map map = m_Mobile.Map;
			if (map != null)
			{
				ArrayList list = new ArrayList();
				IPooledEnumerable eable = map.GetMobilesInRange(m_Mobile.Location, 10);
				//add each mobile found to are list
				foreach (Mobile m in eable)
				{
					if (m != null)
						list.Add(m);
				}
				foreach (Mobile m in list)
				{
					if (m != null && !m.Hidden && m_Mobile.CanSee(m) && m is BaseCreature && (((BaseCreature)m).AI == AIType.AI_Animal || ((BaseCreature)m).AI == AIType.AI_Predator)) //look for a animal
					{
						m_Mobile.Combatant = m;
						m_Mobile.FocusMob = m;
						m_Mobile.DoHarmful(m);
						return true;
					}
				}
				return false;
			}
			return false;
		}
		public override bool DoActionFlee()
		{
			if (m_Mobile.Hits > m_Mobile.HitsMax / 2)
			{
				m_Mobile.DebugSay("I am stronger now, so I will continue fighting");
				Action = ActionType.Combat;
			}
			//were still hurt bad see if theres some food around if weve got a bit of distance
			if (m_Mobile.FocusMob != null && !m_Mobile.InRange(m_Mobile.FocusMob, 20) && FoodAround()) //weve got some distance
			{
				Action = ActionType.Combat;
				return true;
			}
			//fuck there still close or there isnt any food around run for it!
			else
			{
				//we flee as a bat
				if (m_Mobile is Vampire && ((Vampire)m_Mobile).BatForm != true && !m_Mobile.Paralyzed) //only do this if were not a bat already
				{
					((Vampire)m_Mobile).DoTransform((Vampire)m_Mobile, 317, ((Vampire)m_Mobile).toBatForm); // become a bat
					m_Mobile.ActiveSpeed = ActiveSpeedFast;
					m_Mobile.CurrentSpeed = ActiveSpeedFast; //speed up where air born now
					m_Mobile.CanFly = true;
				}

				if (m_Mobile is Vampire && ((Vampire)m_Mobile).BatForm == true && !m_Mobile.Paralyzed && m_Mobile.CurrentSpeed != ActiveSpeedFast) //only do this if were not a bat already
				{
					m_Mobile.ActiveSpeed = ActiveSpeedFast;
					m_Mobile.CurrentSpeed = ActiveSpeedFast; //speed up where air born now
					m_Mobile.CanFly = true;
				}

				m_Mobile.FocusMob = m_Mobile.Combatant;

				if (m_Mobile.FocusMob != null && m_Mobile.InRange(m_Mobile.FocusMob, 42))
					RunFrom(m_Mobile.FocusMob);
				else //looks like we escaped them alright
				{
					if (m_Mobile is Vampire && ((Vampire)m_Mobile).BatForm == true && (CheckNight(m_Mobile))) //change to human form
					{
						((Vampire)m_Mobile).DoTransform((Vampire)m_Mobile, (m_Mobile.Female) ? 0x191 : 0x190, ((Vampire)m_Mobile).Hue, ((Vampire)m_Mobile).toHumanForm);
						m_Mobile.ActiveSpeed = ActiveSpeedNormal;
						m_Mobile.CurrentSpeed = ActiveSpeedNormal;
						m_Mobile.CanFly = false;
					}

					Action = ActionType.Combat;
					return true;
				}
			}
			return true;
		}

		static public bool CheckNight(Mobile m)
		{
			if (m == null)
				return false;

			int hours, minutes;
			Server.Items.Clock.GetTime(m.Map, m.X, m.Y, out hours, out minutes);

#if DEBUG
			// every 5 minutes (for 1 minute)
			if (minutes % 5 == 0)
			{
				return true;
			}
			else
			{
				return false;
			}

#else

			if (hours <= 5 || hours >= 21) //9pm to 6am uo time
				return true;
			else
				return false;
#endif

		}

		public bool CheckLand()
		{
			try
			{
				//we only land/aka go human form on tiles that are walkable
				if (m_Mobile != null && m_Mobile.Map != null)
				{
					Map map = m_Mobile.Map;
					Tile[] tiles = map.Tiles.GetStaticTiles(m_Mobile.X, m_Mobile.Y, true);
					Tile landTile = map.Tiles.GetLandTile(m_Mobile.X, m_Mobile.Y);
					bool landBlocks = (TileData.LandTable[landTile.ID & 0x3FFF].Flags & TileFlag.Impassable) != 0;
					if (landBlocks)
						return false; //were over a landtile thats not walkable normally

					for (int i = 0; i < tiles.Length; ++i)
					{
						Tile check = tiles[i];
						ItemData itemData = TileData.ItemTable[check.ID & 0x3FFF];

						//were on a static item tile thats impassable normally dont land!
						if ((itemData.Flags & TileFlag.Impassable) != 0) // Impassable
						{
							return false;
						}
					}
					//Whatever tile it is its not impassable we can land and go human
					return true;
				}
			}
			catch (Exception exc)
			{
				LogHelper.LogException(exc);
				System.Console.WriteLine("Send to Zen please: ");
				System.Console.WriteLine("Exception caught in Vampire.CheckLand: " + exc.Message);
				System.Console.WriteLine(exc.StackTrace);
			}
			return false;
		}


		#region Messages to say
		string GetMsg(Mobile target) //returns string to say based on targets gender/fame/etc
		{
			bool Female = target.Female;
			int fame = target.Fame;
			string tosay = null;

			//special check for orcs all other iob factions are humanish.. and worth eating :P
			if (target is PlayerMobile && ((PlayerMobile)target).IOBAlignment == IOBAlignment.Orcish && ((PlayerMobile)target).IOBEquipped == true && (Utility.RandomBool()))
			{
				tosay = "Gah, Orcish blood! Better to end your pathetic existence than taint my blood with that of your race.";
				return tosay;
			}

			if (Female)
			{
				switch (Utility.Random(4))
				{
					case 0:
						{
							tosay = "We can't have you leaving now can we love?";
							break;
						}
					case 1:
						{
							tosay = "My lady it will all end soon fear not.";
							break;
						}
					case 2: //lower end fame
						{
							if (fame < 3700)
							{
								tosay = "Your commoner's blood is hardly worth the effort woman, end your struggles.";
								break;
							}
							else
								goto default;
						}
					case 3: //higher end fame
						{
							if (fame >= 7000)
							{
								tosay = "Your noble blood, so rich my once to be Countess.";
								break;
							}
							else
								goto default;
						}
					default: //say nothing
						{
							tosay = "";
							break;
						}
				}
			}

			if (!Female) //male sayings
			{
				switch (Utility.RandomMinMax(0, 3))
				{
					case 0:
						{
							tosay = "Fear not my lord it's only your life thats slipping away.";
							break;
						}
					case 1:
						{
							tosay = "";
							break;
						}
					case 2: //lower end fame
						{
							if (fame < 3700)
							{
								tosay = "To the grave with you peasant!";
								break;
							}
							else
								goto default;
						}
					case 3: //higher end fame
						{
							if (fame >= 7000)
							{
								tosay = "Your blood shall bring me much delight my noble Lord.";
								break;
							}
							else
								goto default;
						}
					default: //say nothing
						{
							tosay = "";
							break;
						}
				}
			}
			return tosay;
		}
		#endregion
		public void RunTo(Mobile m, bool Run)
		{
			if (m == null)
				return;

			if (!MoveTo(m, Run, 1))
				OnFailedMove();

			else if (m_Mobile.InRange(m, m_Mobile.RangeFight - 1))
			{
				RunFrom(m);
			}

		}
		public void RunFrom(Mobile m)
		{
			if (m == null)
				return;

			Run((m_Mobile.GetDirectionTo(m) - 4) & Direction.Mask);
		}

		public void Run(Direction d)
		{
			if ((m_Mobile.Spell != null && m_Mobile.Spell.IsCasting) || m_Mobile.Paralyzed || m_Mobile.Frozen || m_Mobile.DisallowAllMoves)
				return;

			m_Mobile.Direction = d | Direction.Running;

			DoMove(m_Mobile.Direction, true);

		}
		public void OnFailedMove()
		{
			try
			{
				//try an see if the tile blocking us is one we can fly over if so transform
				if (m_Mobile != null && m_Mobile.Combatant != null && m_Mobile is Vampire && ((Vampire)m_Mobile).BatForm == false)
				{
					Direction moving = m_Mobile.Direction;

					int x = m_Mobile.X, y = m_Mobile.Y;
					//get the tile that blocked us from the direction we were trying to go.
					switch (moving & Direction.Mask)
					{
						case Direction.North: --y; break;
						case Direction.Right: ++x; --y; break;
						case Direction.East: ++x; break;
						case Direction.Down: ++x; ++y; break;
						case Direction.South: ++y; break;
						case Direction.Left: --x; ++y; break;
						case Direction.West: --x; break;
						case Direction.Up: --x; --y; break;
					}

					Map map = m_Mobile.Map;
					Tile[] tiles = map.Tiles.GetStaticTiles(x, y, true);
					TileFlag ImpassableSurface = TileFlag.Impassable | TileFlag.Surface;

					for (int i = 0; i < tiles.Length; ++i)
					{
						Tile check = tiles[i];
						ItemData itemData = TileData.ItemTable[check.ID & 0x3FFF];

						if ((itemData.Flags & ImpassableSurface) != 0) // Impassable || Surface
						{
							for (int n = 0; n < m_Mobile.FlyArray.Length; n++)
							{
								if (check.ID == m_Mobile.FlyArray[n])
								{
									//its one of the statics we can fly over!
									//transform to bat mode!
									((Vampire)m_Mobile).DoTransform((Vampire)m_Mobile, 317, ((Vampire)m_Mobile).toBatForm); // become a bat
									m_Mobile.ActiveSpeed = ActiveSpeedFast;
									m_Mobile.CurrentSpeed = ActiveSpeedFast; //speed up where air born now
									m_Mobile.CanFly = true;
									return;
								}
							}
						}

					}
				}

				else
				{
					m_Mobile.DebugSay("I am stuck");
				}
			}

			catch (Exception exc)
			{
				LogHelper.LogException(exc);
				System.Console.WriteLine("Send to Zen please: ");
				System.Console.WriteLine("Exception caught in Vampire.OnFailedMove: " + exc.Message);
				System.Console.WriteLine(exc.StackTrace);
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
