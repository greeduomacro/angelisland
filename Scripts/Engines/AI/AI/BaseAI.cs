/*
 *	This program is the CONFIDENTIAL and PROPRIETARY property
 *	of Tomasello Software LLC. Any unauthorized use, reproduction or
 *	transfer of this computer program is strictly prohibited.
 *
 *      Copyright (c) 2004 Tomasello Software LLC.
 *	This is an unpublished work, and is subject to limited distribution and
 *	restricted disclosure only. ALL RIGHTS RESERVED.
 *
 *			RESTRICTED RIGHTS LEGEND
 *	Use, duplication, or disclosure by the Government is subject to
 *	restrictions set forth in subparagraph (c)(1)(ii) of the Rights in
 * 	Technical Data and Computer Software clause at DFARS 252.227-7all013.
 *
 *	Angel Island UO Shard	Version 1.0
 *			Release A
 *			March 25, 2004
 */

/* Scripts/Engines/AI/AI/BaseAI.cs
 * CHANGELOG
 *	06/07/09 plasma
 *		Improve constant focus logic
 *	05/25/09, plasma
 *		Re-implemented the ConstantFocus property of basecreature properly.
 *		If you set something as the constant focus, the mob will now focus entirely on that
 *		Even if they are hidden.  There is additional logic to break this if they are not within 20 squares, dead, etc.
 *	1/13/09, Adam
 *		Fix a bad cast in MoveTo
 *	1/10/09, Adam
 *		Total rewrite of 'reveal' implementation
 *	1/9/09, Adam
 *		Make CanReveal available to BaseAI
 *	1/1/09, Adam
 *		Add new Serialization model for creature AI.
 *		BaseCreature will already be serializing this data when I finish the upgrade, so all you need to do is add your data to serialize. 
 *		Make sure to use the SaveFlags to optimize saves by not writing stuff that is non default.
 *	12/31/08, Adam
 *		- Add Bandage support		
 *		- Add generic DequipWeapon procedure
 *	12/30/08, Adam
 *		- Move the bandage stuff into it's own region
 *		- Add IsDamaged and IsPoisoned properties
 *	12/19,08, Adam
 *		** total rewrite of AcquireFocusMob() **
 *		We split the FightMode into two different bitmasks: 
 *		(1) The TYPE of creature to focus on (murderer, Evil, Aggressor, etc.)
 *		(2) The SELECTION parameters (closest, smartest, strongest, etc.)
 *		We then enumerate each value contained in the TYPE bitmask and pass each one to the
 *		AcquireFocusMobWorker() function along with the SELECTION mask.
 *		AcquireFocusMobWorker() will perform a similar enumeration over the SELECTION mask
 *		to build a sorted list of compound selection criteria, for instance Closest and Strongest. 
 *		Differences from OSI: Most creatures will act the same as on OSI; and if they don�t, we probably
 *		set the FightMode flags wrong for that creature. The real difference is the flexibility to do things
 *		not supported on OSI like creating compound aggression formulas like: 
 *		�Focus on all Evil and Criminal players while attacking the Weakest first with the highest Intelligence�
 *	12/09/08, Adam
 *		In AcquireFocusMob() we need to check if bitmack >= 1 not simply > 1
 *			if (((int)acqType & (int)m_FightModeValues[ix]) >= 1)
 *	12/07/08, Adam
 *		Redesign AcquireFocusMob() to loop through the ON bits in the FightMode parameter and passing them onto 
 *		AcquireFocusMobWorker(). This change allows us to create mobs with like Strongest | Closest | Evil FightModes
 *	07/23/08, weaver
 *		Automated IPooledEnumerable optimizations. 1 loops updated.
 *	7/1/08, weaver
 *		Added ActionType.Chase and empty base handling definition.
 *	4/22/08. Adam
 *		Better handling of the CurrentWayPoint.Deleted case.
 *	9/1/07, Adam
 *		In OnTick() check to see if the AI has changed, if so, stop the timer and return
 *		i.e if (m_Owner.m_Mobile.AIObject != m_Owner)
 *	7/15/06, Adam
 *		Added new creature memory heap. This heap is used by the new AcquireFocusMobFromMemory() function
 *		to return creatures previously seen and filtered by AcquireFocus(), but which may now be hidden (or in another state.)
 *		the AcquireFocusMobFromMemory() is build upon a low-level simple heap-memory API which is implemented as a hash table
 *		and has a built-in short memory (cleanup) system. The .Value paramater of the heap is currently unused, but will come 
 *		in handy for future needs. Just be sure to update AcquireFocusMobFromMemory() accordingly.
 *  06/18/07 Taran Kain
 *		Added AI_Dragon
 *	03/28/07 Taran Kain
 *		Added ChickenAI
 *	3/6/07, Pix
 *		Reverted last change, which wasn't fully tested and was checked in accidentally.
 *	02/01/07, Pix
 *		Fixed bug in which pets don't attack when in guard mode and master isn't around (dead or gone).
 *		Fixed bug preventing pets in guard mode to not use spells (missing target processing logic).
 *  12/21/06, Kit
 *      And I say let the untamed ghost pets end! Added in missing isbonded = false to release command.
 *  10/10/06, Kit
 *		Revert B1 completly.
 *  8/31/06, Kit
 *		Made sector deactivation clear hidden mob memory list of any AI's.
 *  8/22/06, Kit
 *		Fixed bug with provoked creature on a player not breaking when player is dead.
 *  8/13/06, Kit
 *		Various tweaks to use new Creature flags.
 *  7/26/06, Kit
 *		Fixed accidental merge of code in WalkRandomHome, causeing creatures to not return to home locations of 0.
 *  7/20/06, Kit
 *		Fixed bug with FightMode evil controlled creatures wandering instead of attacking
 *		other aggressors.
 *  7/02/06, Kit
 *		Made hiding, cause lost of bard target and provoke.
 *	6/14/06, Adam
 *		Eliminate call to Dupicate IsEnemy() function in BaceCreature as it was mistanking being called when
 *			the other one was supposed to be called.
 *	6/11/06, Adam
 *		Convert NavStar console output to DebugSay
 *  06/07/06, Kit
 *		Fixed bug with beacons/navstar crashing server when mob remembered a null beacon.
 *  05/16/06, Kit
 *		Removed old bondedrelease gump replaced with new BondedPetReleaseGump
 *		Made it so that releaseing a dead bonded pet deletes the pet.
 *		removed old pet confusion code
 *  05/15/06 Taran Kain
 *		Integrated SectorPathAlgorithm into NavStar AI.
 *  04/30/06, Kit
 *		Removed previous BondedPetCanAttack logic/function.
 *  04/29/06, Kit
 *		Added new BondedPetCanAttack function for new all kill and guard mode pet logic.
 *		Set exception in TransformDelay to allow dead bonded pets to move at full normal speed.
 *  04/22/06, Kit
 *		Added check to follow logic that if creature CanRun to use running or walking acording to masters state.
 *		Rewrote ArmWeaponXXX routines/Add Generic EquipWeapon routine.
 *		Fixed direction bug with MoveTo causeing creatures if in range to not face enemy.
 *  04/17/06, Kit
 *		Added Bool Variable UsesRegs for if creature needs reagents to cast.
 *  04/15/06, Kit
 *		Modified Follow logic via WalkMobileRange to allow mobiles to follow master onboard ships.
 *	4/8/06, Pix
 *		Uses new CoreAI.IOBJoinEnabled bit.
 *	04/06/06, weaver
 *		Added logging of any attacking commands issued to have pets attack their owner.
 *	04/04/06, weaver
 *		Added logging of any attacking commands issued by tamers to have their pets attack each other.
 *  01/06/06, Kit
 *		Added playertype bandage use, useitembytype, ArmWeaponByType()
 *  12/28/05, Kit
 *		Added fightmode Player logic to AI_SORT, added IsScaryCOndition check to scary logic check.
 *	12/16/05, Adam
 *		Comment out debug logic
 *  12/10/05, Kit
 *		Added Vampire_AI.
 *  12/05/05, Kit
 *		NavStar Changes
 *  11/29/05, Kit
 *		Added MoveToNavPoint(), NavStar FSM state code, and checks for playerrangesensitive for use with NavStar.
 *	11/23/05, Adam
 *		Taran added code to have the mobile 'turn to the direction' set in the spawner
 *		as per the SR. 
 *		Adjustment: have movile read the m_Mobile.Spawner.MobileDirection instead of m_Mobile.Spawner.Direction
 *  11/07/05, Kit
 *		Added ARM/DISARM functions for weapons, moved GetPackItems function from HumanMageAI to here for use with ARM/DISARM
 *	10/06/05, erlein
 *		Altered confusion text to reflect anger at owner.
 *	10/05/05, erlein
 *		Added angered sound effect, animation chance and text feedback to owner on command issue to confused pets.
 *	10/02/05, erlein
 *		Removed aggressor list flush on "stop" command to moderate tamer control over their pets.
 *		Added confusion check on command interpretation via context menus and speech.
 *	9/19/05, Adam
 *		a. I think it was a bug in the RunUO to reset the 'bCheckIt' flag even when we are confronted with
 *		and ememy. Just because something is Aggressor, Evil, or Criminal should not mean that they should
 *		ignore their enemies.
 *		We now differentiate between generic enimies, and OppositionGroups. OppositionGroups are now considered
 *		regardless of fight mode.
 *		b. remove the early exit from AcquireFocusMob for the FightMode.Aggressor condition
 *		c. add debug thingie to AcquireFocusMob(): search for bDebug
 *	8/11/05, erlein
 *		Added flushing of aggressor list on "stop" command.
 *  6/04/05, Kit
 *		Added CouncilMemberAI
 *  5/30/05, Kit
 *		Added HumanMageAI, added checks for if mobile can run, and if damage should slow it
 *		Added initial support for new FSM Hunt state, Added MoveToSound() function,
 *		updated Movement functions to check if monster is running, and if so to set Running Flag for movement
 *	5/05/05, Kit
 *		Evil Mage AI support added
 *	4/30/05, Kit
 *		Added in support for Kin attacking players/kin in custom regions(DRDT) defined as a IOB region
 *	4/28/05, Kit
 *		Added back in getValuefrom check in Acquire function for default creatures to resolve problems with
 *		creatures attacking tamer doing all kill vs pet
 *	4/27/05. Kit
 *		Added in support for AI that has a prefeered type of mob to attack
 *		Fixed problems were fightmodes did not actually choose their first mob accordingly
 *	4/26/05, Pix
 *		Removed caster and guild invulnerability from non-controlled summons.
 *	4/21/05, Adam
 *		Remove special code that prevents controls masters from sicing
 *		their pets and summons on aligned players/kin
 *		See: EndPickTarget
 *	4/03/05, Kit
 *		Added AI_Genie Type
 *	3/31/05, Pix
 *		Change IOBStronghold to IOBRegion
 *	3/31/05, Pix
 *		Added check for IOB flagging/player attacking.
 *	03/31/05, erlein
 *		- Added calls to CheckHerding() in DoOrderNone, DoOrderCome and
 *		DoOrderGuard so can be herded in "stop", "come" and "guard" modes as
 *		well as "follow".
 *		- Added clearance of TargetLocation on command from creature's master
 *		to cancel the herding of creature.
 *  02/15/05, Pixie
 *		CHANGED FOR RUNUO 1.0.0 MERGE.
 * 01/13/05 - Taran Kain
 *		Fixed orcish IOB commands, didn't seem to work. Should now.
 * 01/12/05 - Taran Kain
 *		Added orcish commands "lat stai", "lat clomp" and "lat follow".
 * 01/03/05 - Pix
 *		Added AI_Suicide
 * 01/03/05 - Pix
 *		Made sure Tamables couldn't be made IOBFollowers.
 * 12/29/04, Pix
 *		Moved dismissal routines to BaseCreature.
 * 12/29/04, Pix
 *		Made it so non-IOBAligned mobs won't attack IOBFollowers.
 * 12/28/04, Pix
 *		Added a "grace distance" of +- 5 to dismissing IOBFollowers.
 *		Added message for successful dismissal.
 * 12/28/04, Pix
 *		Now IOBFollowers can't be dismissed unless they're close enough to their home.
 * 12/22/04, Pix
 *		Fixed dismissing control-slot issue.
 * 12/21/04, Pix
 *		Another fix for EndPickTarget.
 * 12/21/04, Pix
 *		Changed EndPickTarget so that IOB owners of pets can't target their own bretheren.
 * 12/20/04, Pix
 *		Added check for IOBAlignment == None
 * 12/20/04, Pix
 *		Added come/stay/stop commands for IOBFollowers.
 *		Added IOBEquipped check before processing command.
 * 12/14/04, Pix
 *		Fixed EV and BS so they attack again.
 * 12/09/04, Pix
 *		First TEST!! code checkin for IOB Factions.
 *  9/11/04, Pix
 *		Added check for guild for summoned creatures (BS/EV) which find their own targets.  They now
 *		won't attack guildmates.
 *	7/14/04, mith
 *		DoOrderAttack(): Added check to verify that target can be seen by the creature.
 *	7/13/04 smerX
 *		Added AIType.AI_Council
 *	6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */


using System;
using System.Collections;
using System.Collections.Generic;
using Server;
using Server.Scripts.Commands;
using Server.Items;
using Server.Multis;
using Server.Targeting;
using Server.Targets;
using Server.Network;
using Server.ContextMenus;
using Server.Engines.Quests;
using Server.Regions;
using Server.Engines.Quests.Necro;
using Server.Spells;
using Server.Spells.First;
using Server.Spells.Second;
using Server.Spells.Third;
using Server.Spells.Fourth;
using Server.Spells.Fifth;
using Server.Spells.Sixth;
using Server.Spells.Seventh;
using Server.Engines;
using MoveImpl = Server.Movement.MovementImpl;
using Server.PathAlgorithms;
using Server.PathAlgorithms.SlowAStar;
using Server.PathAlgorithms.FastAStar;
using Server.PathAlgorithms.Sector;
using Server.Engines.PartySystem;
using Server.Guilds;
using Server.Gumps;
using Server.Engines.IOBSystem;

namespace Server.Mobiles
{
	public enum AIType
	{
		AI_Use_Default,
		AI_Melee,
		AI_Animal,
		AI_Archer,
		AI_Healer,
		AI_Vendor,
		AI_Mage,
		AI_Berserk,
		AI_Predator,
		AI_Thief,
		AI_Council,
		AI_Suicide,
		AI_Genie,
		AI_HumanMage,
		AI_BaseHybrid,
		AI_CouncilMember,
		AI_Vamp,
		AI_Chicken,
		AI_Dragon,
		AI_Hybrid
	}

	public enum ActionType
	{
		Wander,
		Combat,
		Guard,
		Hunt,
		NavStar,
		Flee,
		Backoff,
		Interact,
		Chase
	}

	public enum WeaponArmStatus
	{
		NotFound,
		Success,
		HandFull,
		AlreadyArmed
	}

	public abstract class BaseAI : SerializableObject
	{
		public Timer m_Timer;
		protected ActionType m_Action;
		private DateTime m_NextStopGuard;
		private DateTime m_NextStopHunt;
		public BaseCreature m_Mobile;

		//plasma: this new field is used with the constant focus target type
		//it will cause the constant target to temporarily break off to something else.
		//this is because if a tamer is invis and the mob is getting owned by the pets, it
		//will do nothing but heal until the tamer reappears.
		private DateTime m_TargetHideTime = DateTime.MinValue;

		#region Bandages
		public int BandageCount
		{
			get
			{
				Container pack = m_Mobile.Backpack;
				if (pack == null) return 0;
				Item bandage = pack.FindItemByType(typeof(Bandage));
				return bandage != null ? bandage.Amount : 0;
			}
		}
		public Bandage GetBandage()
		{
			Container pack = m_Mobile.Backpack;
			if (pack == null) return null;
			return pack.FindItemByType(typeof(Bandage)) as Bandage;
		}
		private BandageContext m_Bandage;
		private DateTime m_BandageStart;
		public DateTime BandageTime
		{
			get { return m_BandageStart; }
			set { m_BandageStart = value; }
		}

		public TimeSpan TimeUntilBandage
		{
			get
			{
				if (m_Bandage != null && m_Bandage.Timer == null)
					m_Bandage = null;

				if (m_Bandage == null)
					return TimeSpan.MaxValue;

				TimeSpan ts = (m_BandageStart + m_Bandage.Timer.Delay) - DateTime.Now;

				if (ts < TimeSpan.FromSeconds(-1.0))
				{
					m_Bandage = null;
					return TimeSpan.MaxValue;
				}

				if (ts < TimeSpan.Zero)
					ts = TimeSpan.Zero;

				return ts;
			}
		}

		public bool StartBandage(Mobile from, Mobile to)
		{
			if (BandageCount >= 1)
			{
				m_Bandage = null;
				m_Bandage = BandageContext.BeginHeal(from, to);
				m_BandageStart = DateTime.Now;
				if (m_Bandage != null)
				{	// alls well, consume the bandage!
					Bandage bandage = GetBandage();
					if (bandage != null)
						bandage.Consume();
					return true;
				}
			}
			return false;
		}
		#endregion Bandages

		#region Potions

		public int HealPotCount
		{
			get
			{
				Container pack = m_Mobile.Backpack;
				if (pack == null) return 0;
				Item[] p = pack.FindItemsByType(typeof(BaseHealPotion));
				return (p != null) ? p.Length : 0;
			}
		}

		public BaseHealPotion GetHealPot()
		{
			Container pack = m_Mobile.Backpack;
			if (pack == null) return null;
			return pack.FindItemByType(typeof(BaseHealPotion)) as BaseHealPotion;
		}

		public int CurePotCount
		{
			get
			{
				Container pack = m_Mobile.Backpack;
				if (pack == null) return 0;
				Item[] p = pack.FindItemsByType(typeof(BaseCurePotion));
				return (p != null) ? p.Length : 0;
			}
		}

		public BaseCurePotion GetCurePot()
		{
			Container pack = m_Mobile.Backpack;
			if (pack == null) return null;
			return pack.FindItemByType(typeof(BaseCurePotion)) as BaseCurePotion;
		}

		public int RefreshPotCount
		{
			get
			{
				Container pack = m_Mobile.Backpack;
				if (pack == null) return 0;
				Item[] p = pack.FindItemsByType(typeof(BaseRefreshPotion));
				return (p != null) ? p.Length : 0;
			}
		}

		public BaseRefreshPotion GetRefreshPot()
		{
			Container pack = m_Mobile.Backpack;
			if (pack == null) return null;
			return pack.FindItemByType(typeof(BaseRefreshPotion)) as BaseRefreshPotion;
		}

		public int AgilityPotCount
		{
			get
			{
				Container pack = m_Mobile.Backpack;
				if (pack == null) return 0;
				Item[] p = pack.FindItemsByType(typeof(BaseAgilityPotion));
				return (p != null) ? p.Length : 0;
			}
		}

		public BaseAgilityPotion GetAgilityPot()
		{
			Container pack = m_Mobile.Backpack;
			if (pack == null) return null;
			return pack.FindItemByType(typeof(BaseAgilityPotion)) as BaseAgilityPotion;
		}

		public int StrengthPotCount
		{
			get
			{
				Container pack = m_Mobile.Backpack;
				if (pack == null) return 0;
				Item[] p = pack.FindItemsByType(typeof(BaseStrengthPotion));
				return (p != null) ? p.Length : 0;
			}
		}

		public BaseStrengthPotion GetStrengthPot()
		{
			Container pack = m_Mobile.Backpack;
			if (pack == null) return null;
			return pack.FindItemByType(typeof(BaseStrengthPotion)) as BaseStrengthPotion;
		}

		public virtual bool DrinkCure(Mobile from)
		{
			if (CurePotCount >= 1)
			{
				bool requip = false;
				BaseCurePotion Pot = GetCurePot();
				if (Pot.RequireFreeHand && BasePotion.HasFreeHand(m_Mobile) == false)
					requip = DequipWeapon();
				Pot.Drink(from);
				if (requip)
					EquipWeapon();
				if (Pot.Deleted == true)	// it won't be deleted if we were not poisoned.
					return true;
				else
					return false;
			}
			else
				return false;

		}
		public virtual bool DrinkHeal(Mobile from)
		{
			if (HealPotCount >= 1)
			{
				bool requip = false;
				BaseHealPotion Pot = GetHealPot();
				if (Pot.RequireFreeHand && BasePotion.HasFreeHand(m_Mobile) == false)
					requip = DequipWeapon();
				Pot.Drink(from);
				if (requip)
					EquipWeapon();
				if (Pot.Deleted == true)	// it won't be deleted if we tried to drink it too soon.
					return true;
				else
					return false;
			}
			else
				return false;
		}
		public virtual bool DrinkRefresh(Mobile from)
		{
			if (RefreshPotCount >= 1)
			{
				bool requip = false;
				BaseRefreshPotion Pot = GetRefreshPot();
				if (Pot.RequireFreeHand && BasePotion.HasFreeHand(m_Mobile) == false)
					requip = DequipWeapon();
				Pot.Drink(from);
				if (requip)
					EquipWeapon();
				if (Pot.Deleted == true)	// it won't be deleted if we are at full stam.
					return true;
				else
					return false;
			}
			else
				return false;
		}
		public virtual bool DrinkAgility(Mobile from)
		{
			if (AgilityPotCount >= 1)
			{
				bool requip = false;
				BaseAgilityPotion Pot = GetAgilityPot();
				if (Pot.RequireFreeHand && BasePotion.HasFreeHand(m_Mobile) == false)
					requip = DequipWeapon();
				Pot.Drink(from);
				if (requip)
					EquipWeapon();
				if (Pot.Deleted == true)	// it won't be deleted if you are already under a similar effect
					return true;
				else
					return false;
			}
			else
				return false;
		}
		public virtual bool DrinkStrength(Mobile from)
		{
			if (StrengthPotCount >= 1)
			{
				bool requip = false;
				BaseStrengthPotion Pot = GetStrengthPot();
				if (Pot.RequireFreeHand && BasePotion.HasFreeHand(m_Mobile) == false)
					requip = DequipWeapon();
				Pot.Drink(from);
				if (requip)
					EquipWeapon();
				if (Pot.Deleted == true)	// it won't be deleted if you are already under a similar effect
					return true;
				else
					return false;
			}
			else
				return false;
		}
		#endregion Potions

		public bool IsDamaged
		{
			get { return (m_Mobile.Hits < m_Mobile.HitsMax); }
		}

		public bool IsPoisoned
		{
			get { return m_Mobile.Poisoned; }
		}

		public bool IsAllowed(FightStyle flag)
		{
			return ((m_Mobile.FightStyle & flag) == flag);
		}

		public bool CrossHeals
		{
			get { return m_Mobile.GetFlag(CreatureFlags.CrossHeals); }
			set { m_Mobile.SetFlag(CreatureFlags.CrossHeals, value); }
		}

		public bool UsesBandages
		{
			get { return m_Mobile.GetFlag(CreatureFlags.UsesBandages); }
			set { m_Mobile.SetFlag(CreatureFlags.UsesBandages, value); }
		}

		public bool UsesPotions
		{
			get { return m_Mobile.GetFlag(CreatureFlags.UsesPotions); }
			set { m_Mobile.SetFlag(CreatureFlags.UsesPotions, value); }
		}

		public bool DmgSlowsMovement
		{
			get { return m_Mobile.GetFlag(CreatureFlags.DamageSlows); }
			set { m_Mobile.SetFlag(CreatureFlags.DamageSlows, value); }
		}

		public bool CanRun
		{
			get { return m_Mobile.GetFlag(CreatureFlags.CanRun); }
			set { m_Mobile.SetFlag(CreatureFlags.CanRun, value); }
		}

		public bool CanReveal
		{
			get { return m_Mobile.GetFlag(CreatureFlags.CanReveal); }
			set { m_Mobile.SetFlag(CreatureFlags.CanReveal, value); }
		}

		public bool UsesRegs
		{
			get { return m_Mobile.GetFlag(CreatureFlags.UsesRegeants); }
			set { m_Mobile.SetFlag(CreatureFlags.UsesRegeants, value); }
		}

		public ArrayList GetPackItems(Mobile from)
		{
			ArrayList list = new ArrayList();

			foreach (Item item in from.Items)
			{
				if (item.Movable && item != from.Backpack)
					list.Add(item);
			}

			if (from.Backpack != null)
			{
				list.AddRange(from.Backpack.Items);
			}

			return list;
		}

		public bool FindWeapon(Mobile m)
		{
			Container pack = m.Backpack;

			if (pack == null)
				return false;

			Item weapon = pack.FindItemByType(typeof(BaseWeapon));
			Item weaponOne = m.FindItemOnLayer(Layer.OneHanded);
			Item weaponTwo = m.FindItemOnLayer(Layer.TwoHanded);
			if (weapon == null && weaponOne == null && weaponTwo == null)
				return false;

			return true;
		}

		public bool EquipWeapon()
		{
			Container pack = m_Mobile.Backpack;

			if (pack == null)
				return false;

			Item weapon = pack.FindItemByType(typeof(BaseWeapon));

			if (weapon == null)
				return false;

			return m_Mobile.EquipItem(weapon);
		}

		public bool DequipWeapon()
		{
			Container pack = m_Mobile.Backpack;

			if (pack == null)
				return false;

			Item weapon = m_Mobile.Weapon as Item;

			if (weapon != null && weapon.Parent == m_Mobile && !(weapon is Fists))
			{
				pack.DropItem(weapon);
				return true;
			}

			return false;
		}

		public WeaponArmStatus ArmWeaponByType(Type type)
		{
			Container pack = m_Mobile.Backpack;

			if (pack == null)
				return WeaponArmStatus.NotFound;

			Item weapon = m_Mobile.Weapon as Item;

			if (weapon.GetType() == type)
				return WeaponArmStatus.AlreadyArmed;

			Item item = pack.FindItemByType(type);

			bool FoundWeapon = false;
			bool HandBlocked = false;

			if (item == null)
				return WeaponArmStatus.NotFound;
			else
				FoundWeapon = true;

			if (weapon.Layer == Layer.OneHanded || weapon.Layer == Layer.TwoHanded)
				HandBlocked = true;

			if (m_Mobile.EquipItem(item))
			{
				return WeaponArmStatus.Success;
			}

			if (FoundWeapon && HandBlocked)
				return WeaponArmStatus.HandFull;

			return WeaponArmStatus.NotFound;

		}


		public WeaponArmStatus ArmOneHandedWeapon()
		{
			Item weapon = m_Mobile.Weapon as Item;
			if (weapon.Layer == Layer.OneHanded)
				return WeaponArmStatus.AlreadyArmed;

			bool HandBlocked = false;
			bool FoundWeapon = false;

			if (weapon.Layer == Layer.TwoHanded)
				HandBlocked = true;

			ArrayList list = GetPackItems(m_Mobile);
			foreach (Item item in list)
			{
				if (item is BaseWeapon && item.Layer == Layer.OneHanded)
				{
					FoundWeapon = true;
					if (m_Mobile.EquipItem(item))
					{
						return WeaponArmStatus.Success;
					}
				}
			}
			if (!FoundWeapon)
				return WeaponArmStatus.NotFound;
			if (FoundWeapon && HandBlocked)
				return WeaponArmStatus.HandFull;

			return WeaponArmStatus.NotFound;
		}

		public WeaponArmStatus ArmTwoHandedWeapon()
		{
			Item weapon = m_Mobile.Weapon as Item;
			if (weapon.Layer == Layer.TwoHanded)
				return WeaponArmStatus.AlreadyArmed;

			bool HandBlocked = false;
			bool FoundWeapon = false;

			Item IsArmed = m_Mobile.FindItemOnLayer(Layer.TwoHanded);

			if (weapon.Layer == Layer.OneHanded || IsArmed is BaseShield)
				HandBlocked = true;

			ArrayList list = GetPackItems(m_Mobile);
			foreach (Item item in list)
			{
				if (item is BaseWeapon && item.Layer == Layer.TwoHanded)
				{
					FoundWeapon = true;
					if (m_Mobile.EquipItem(item))
					{
						return WeaponArmStatus.Success;
					}
				}
			}
			if (!FoundWeapon)
				return WeaponArmStatus.NotFound;
			if (FoundWeapon && HandBlocked)
				return WeaponArmStatus.HandFull;

			return WeaponArmStatus.NotFound;
		}

		public bool ArmShield()
		{
			Item IsArmed = m_Mobile.FindItemOnLayer(Layer.TwoHanded);
			if (IsArmed is BaseShield)
				return true;

			ArrayList list = GetPackItems(m_Mobile);
			foreach (Item item in list)
			{
				if (item is BaseShield && item.Layer == Layer.TwoHanded)
				{
					if (m_Mobile.EquipItem(item))
						return true;
				}
			}

			return false;
		}

		public void DisarmOneHandedWeapon()
		{
			m_Mobile.ClearHand(m_Mobile.FindItemOnLayer(Layer.OneHanded));
		}

		public void DisarmTwoHandedWeapon()
		{
			Item shield = m_Mobile.FindItemOnLayer(Layer.TwoHanded);
			if (shield != null && shield is BaseShield)
				return;

			m_Mobile.ClearHand(m_Mobile.FindItemOnLayer(Layer.TwoHanded));
		}

		public void DisarmShield()
		{
			Item shield = m_Mobile.FindItemOnLayer(Layer.TwoHanded);
			if (shield != null && shield is BaseShield)
				m_Mobile.ClearHand(m_Mobile.FindItemOnLayer(Layer.TwoHanded));

		}

		public bool CheckIfHaveItem(Type type)
		{
			Container pack = m_Mobile.Backpack;

			if (pack == null)
				return false;

			Item item = pack.FindItemByType(type);

			if (item == null)
				return false;

			return true;
		}

		public virtual bool UseItemByType(Type type)
		{
			Container pack = m_Mobile.Backpack;

			if (pack == null)
				return false;

			Item item = pack.FindItemByType(type);

			if (item == null)
				return false;

			item.OnDoubleClick(m_Mobile);

			return true;
		}


		public BaseAI(BaseCreature m)
		{
			m_Mobile = m;

			m_Timer = new AITimer(this);
			m_Timer.Start();

			Action = ActionType.Wander;
		}

		public ActionType Action
		{
			get
			{
				return m_Action;
			}
			set
			{
				m_Action = value;
				OnActionChanged();
			}
		}

		public virtual bool WasNamed(string speech)
		{
			string name = m_Mobile.Name;

			return (name != null && Insensitive.StartsWith(speech, name));
		}

		private class InternalEntry : ContextMenuEntry
		{
			private Mobile m_From;
			private BaseCreature m_Mobile;
			private BaseAI m_AI;
			private OrderType m_Order;

			public InternalEntry(Mobile from, int number, int range, BaseCreature mobile, BaseAI ai, OrderType order)
				: base(number, range)
			{
				m_From = from;
				m_Mobile = mobile;
				m_AI = ai;
				m_Order = order;

				if (mobile.IsDeadPet && (order == OrderType.Guard || order == OrderType.Attack || order == OrderType.Transfert || order == OrderType.Drop))
					Enabled = false;
			}

			public override void OnClick()
			{
				if (!m_Mobile.Deleted && m_Mobile.Controlled && m_From == m_Mobile.ControlMaster && m_From.CheckAlive())
				{
					switch (m_Order)
					{
						case OrderType.Follow:
						case OrderType.Attack:
						case OrderType.Transfert:
							{
								m_AI.BeginPickTarget(m_From, m_Order);
								break;
							}
						case OrderType.Release:
							{
								if (m_Mobile.IOBFollower)
								{
									m_Mobile.AttemptIOBDismiss();
								}
								else if (m_Mobile.Summoned)
									goto default;
								else
									m_From.SendGump(new Gumps.ConfirmReleaseGump(m_From, m_Mobile));

								break;
							}

						default:
							{
								if (m_Mobile.CheckControlChance(m_From))
									m_Mobile.ControlOrder = m_Order;

								break;
							}
					}
				}
			}
		}

		public virtual void GetContextMenuEntries(Mobile from, ArrayList list)
		{
			if (from.Alive && m_Mobile.Controlled && from == m_Mobile.ControlMaster && from.InRange(m_Mobile, 14) && !m_Mobile.IOBFollower)
			{
				list.Add(new InternalEntry(from, 6107, 14, m_Mobile, this, OrderType.Guard));  // Command: Guard
				list.Add(new InternalEntry(from, 6108, 14, m_Mobile, this, OrderType.Follow)); // Command: Follow

				if (!m_Mobile.Summoned)
					list.Add(new InternalEntry(from, 6109, 14, m_Mobile, this, OrderType.Drop));   // Command: Drop

				list.Add(new InternalEntry(from, 6111, 14, m_Mobile, this, OrderType.Attack)); // Command: Kill
				list.Add(new InternalEntry(from, 6112, 14, m_Mobile, this, OrderType.Stop));   // Command: Stop
				list.Add(new InternalEntry(from, 6114, 14, m_Mobile, this, OrderType.Stay));   // Command: Stay

				if (!m_Mobile.Summoned)
					list.Add(new InternalEntry(from, 6113, 14, m_Mobile, this, OrderType.Transfert)); // Transfer

				list.Add(new InternalEntry(from, 6118, 14, m_Mobile, this, OrderType.Release)); // Release
			}

			if (m_Mobile.IOBFollower && m_Mobile.IOBLeader == from && !m_Mobile.Tamable)
			{
				list.Add(new InternalEntry(from, 6129, 14, m_Mobile, this, OrderType.Release)); //Dismiss
			}
		}

		public virtual void BeginPickTarget(Mobile from, OrderType order)
		{
			if (m_Mobile.Deleted || !m_Mobile.Controlled || from != m_Mobile.ControlMaster || !from.InRange(m_Mobile, 14) || from.Map != m_Mobile.Map)
				return;

			if (from.Target == null)
			{
				if (order == OrderType.Transfert)
					from.SendLocalizedMessage(502038); // Click on the person to transfer ownership to.

				from.Target = new AIControlMobileTarget(this, order);
			}
			else if (from.Target is AIControlMobileTarget)
			{
				AIControlMobileTarget t = (AIControlMobileTarget)from.Target;

				if (t.Order == order)
					t.AddAI(this);
			}
		}

		public virtual void EndPickTarget(Mobile from, Mobile target, OrderType order)
		{
			// adam: sanity
			if (from == null)
			{
				Console.WriteLine("(from == null) in BaseAI::EndPickTarget");
				//return;
			}

			if (m_Mobile.Deleted || !m_Mobile.Controlled || from != m_Mobile.ControlMaster || !from.InRange(m_Mobile, 14) || from.Map != m_Mobile.Map || !from.CheckAlive())
				return;

			//Special case for if it's an iob follower!
			if (m_Mobile.IOBFollower && m_Mobile.IOBLeader == from)
			{
				if (target is BaseCreature)
				{
					BaseCreature bc = (BaseCreature)target;
					if (bc.IOBAlignment != IOBAlignment.None)
					{
						if (bc.IOBAlignment == m_Mobile.IOBAlignment)
						{
							//Won't attack same IOBAlignment
						}
						else
						{
							m_Mobile.ControlTarget = target;
							m_Mobile.ControlOrder = order;
						}
					}
					else
					{
						m_Mobile.SayTo(from, "Your follower refuses to attack that creature");
					}
				}
				else if (target is PlayerMobile)
				{
					if (CoreAI.IsDynamicFeatureSet(CoreAI.FeatureBits.IOBShardWide)
						|| (Server.Engines.IOBSystem.IOBRegions.IsInIOBRegion(from)
						&& Server.Engines.IOBSystem.IOBRegions.IsInIOBRegion(target)))
					{
						PlayerMobile pm = (PlayerMobile)target;
						if (pm.IOBAlignment == m_Mobile.IOBAlignment || pm.IOBAlignment == IOBAlignment.None)
						{
							//Won't attack same IOBAlignment
						}
						else
						{
							m_Mobile.ControlTarget = target;
							m_Mobile.ControlOrder = order;
						}
					}
					else
					{
						m_Mobile.SayTo(from, "Your follower refuses to attack that here.");
					}
				}
				return;
			}

			if (order == OrderType.Attack && target is BaseCreature && (((BaseCreature)target).IsScaryToPets && ((BaseCreature)target).IsScaryCondition()) && m_Mobile.IsScaredOfScaryThings)
			{
				m_Mobile.SayTo(from, "Your pet refuses to attack this creature!");
				return;
			}

			if (m_Mobile.CheckControlChance(from))
			{
				// wea: targetted mobile and fixed control destination
				// 
				if (CoreAI.TempInt == 2 && order == OrderType.Attack)
				{
					bool success = false;

					if (target is BaseCreature)
					{
						// Is the controlmaster the same?
						if (m_Mobile.ControlMaster == ((BaseCreature)target).ControlMaster)
							success = true;
					}
					else if (target is PlayerMobile)
					{
						// Are we targetting the controlmaster?
						if (m_Mobile.ControlMaster == target)
							success = true;

					}

					if (success)
					{
						// Log this
						LogHelper Logger = new LogHelper("allguardbug.log", false, true);
						Logger.Log(LogType.Text, string.Format("{0}:{1}:{2}:{3}", from, m_Mobile, order, target));
						Logger.Finish();

						// Send a message to all staff in range
						IPooledEnumerable eable = m_Mobile.GetClientsInRange(75);
						foreach (NetState state in eable)
						{
							if (state.Mobile.AccessLevel >= AccessLevel.Counselor)
								m_Mobile.PrivateOverheadMessage(MessageType.Regular, 123, true, string.Format("My master just ordered me ({0}) to {1} the mobile ({2})", m_Mobile, order, target), state);
						}
					}
				}

				m_Mobile.ControlTarget = target;
				m_Mobile.ControlOrder = order;
			}

		}

		public virtual bool HandlesOnSpeech(Mobile from)
		{
			if (from.AccessLevel >= AccessLevel.GameMaster)
				return true;

			if (from.Alive && m_Mobile.Controlled && m_Mobile.Commandable && from == m_Mobile.ControlMaster)
				return true;

			//Pix: This is needed for the "join me" command issued to bretheren
			if (from.Alive && m_Mobile.IOBAlignment != IOBAlignment.None)
				return true;

			return (from.Alive && from.InRange(m_Mobile.Location, 3) && m_Mobile.IsHumanInTown());
		}

		private static SkillName[] m_KeywordTable = new SkillName[]
			{
				SkillName.Parry,
				SkillName.Healing,
				SkillName.Hiding,
				SkillName.Stealing,
				SkillName.Alchemy,
				SkillName.AnimalLore,
				SkillName.ItemID,
				SkillName.ArmsLore,
				SkillName.Begging,
				SkillName.Blacksmith,
				SkillName.Fletching,
				SkillName.Peacemaking,
				SkillName.Camping,
				SkillName.Carpentry,
				SkillName.Cartography,
				SkillName.Cooking,
				SkillName.DetectHidden,
				SkillName.Discordance,//??
				SkillName.EvalInt,
				SkillName.Fishing,
				SkillName.Provocation,
				SkillName.Lockpicking,
				SkillName.Magery,
				SkillName.MagicResist,
				SkillName.Tactics,
				SkillName.Snooping,
				SkillName.RemoveTrap,
				SkillName.Musicianship,
				SkillName.Poisoning,
				SkillName.Archery,
				SkillName.SpiritSpeak,
				SkillName.Tailoring,
				SkillName.AnimalTaming,
				SkillName.TasteID,
				SkillName.Tinkering,
				SkillName.Veterinary,
				SkillName.Forensics,
				SkillName.Herding,
				SkillName.Tracking,
				SkillName.Stealth,
				SkillName.Inscribe,
				SkillName.Swords,
				SkillName.Macing,
				SkillName.Fencing,
				SkillName.Wrestling,
				SkillName.Lumberjacking,
				SkillName.Mining,
				SkillName.Meditation
			};

		public virtual void OnSpeech(SpeechEventArgs e)
		{
			if (e.Mobile.Alive && e.Mobile.InRange(m_Mobile.Location, 3) && m_Mobile.IsHumanInTown())
			{
				if (e.HasKeyword(0x9D) && WasNamed(e.Speech)) // *move*
				{
					if (m_Mobile.Combatant != null)
					{
						// I am too busy fighting to deal with thee!
						m_Mobile.PublicOverheadMessage(MessageType.Regular, 0x3B2, 501482);
					}
					else
					{
						// Excuse me?
						m_Mobile.PublicOverheadMessage(MessageType.Regular, 0x3B2, 501516);
						WalkRandomInHome(2, 2, 1);
					}
				}
				else if (e.HasKeyword(0x9E) && WasNamed(e.Speech)) // *time*
				{
					if (m_Mobile.Combatant != null)
					{
						// I am too busy fighting to deal with thee!
						m_Mobile.PublicOverheadMessage(MessageType.Regular, 0x3B2, 501482);
					}
					else
					{
						int generalNumber;
						string exactTime;

						Clock.GetTime(m_Mobile, out generalNumber, out exactTime);

						m_Mobile.PublicOverheadMessage(MessageType.Regular, 0x3B2, generalNumber);
					}
				}
				else if (e.HasKeyword(0x6C) && WasNamed(e.Speech)) // *train
				{
					if (m_Mobile.Combatant != null)
					{
						// I am too busy fighting to deal with thee!
						m_Mobile.PublicOverheadMessage(MessageType.Regular, 0x3B2, 501482);
					}
					else
					{
						bool foundSomething = false;

						Skills ourSkills = m_Mobile.Skills;
						Skills theirSkills = e.Mobile.Skills;

						for (int i = 0; i < ourSkills.Length && i < theirSkills.Length; ++i)
						{
							Skill skill = ourSkills[i];
							Skill theirSkill = theirSkills[i];

							if (skill != null && theirSkill != null && skill.Base >= 60.0 && m_Mobile.CheckTeach(skill.SkillName, e.Mobile))
							{
								double toTeach = skill.Base / 3.0;

								if (toTeach > 42.0)
									toTeach = 42.0;

								if (toTeach > theirSkill.Base)
								{
									int number = 1043059 + i;

									if (number > 1043107)
										continue;

									if (!foundSomething)
										m_Mobile.Say(1043058); // I can train the following:

									m_Mobile.Say(number);

									foundSomething = true;
								}
							}
						}

						if (!foundSomething)
							m_Mobile.Say(501505); // Alas, I cannot teach thee anything.
					}
				}
				else
				{
					SkillName toTrain = (SkillName)(-1);

					for (int i = 0; toTrain == (SkillName)(-1) && i < e.Keywords.Length; ++i)
					{
						int keyword = e.Keywords[i];

						if (keyword == 0x154)
						{
							toTrain = SkillName.Anatomy;
						}
						else if (keyword >= 0x6D && keyword <= 0x9C)
						{
							int index = keyword - 0x6D;

							if (index >= 0 && index < m_KeywordTable.Length)
								toTrain = m_KeywordTable[index];
						}
					}

					if (toTrain != (SkillName)(-1) && WasNamed(e.Speech))
					{
						if (m_Mobile.Combatant != null)
						{
							// I am too busy fighting to deal with thee!
							m_Mobile.PublicOverheadMessage(MessageType.Regular, 0x3B2, 501482);
						}
						else
						{
							Skills skills = m_Mobile.Skills;
							Skill skill = skills[toTrain];

							if (skill == null || skill.Base < 60.0 || !m_Mobile.CheckTeach(toTrain, e.Mobile))
							{
								m_Mobile.Say(501507); // 'Tis not something I can teach thee of.
							}
							else
							{
								m_Mobile.Teach(toTrain, e.Mobile, 0, false);
							}
						}
					}
				}
			}

			string heardspeech = e.Speech;
			if (!m_Mobile.IOBFollower && m_Mobile.IOBAlignment != IOBAlignment.None && !m_Mobile.Tamable) // if we're NOT already following someone, listen for join command
			{
				if (CoreAI.IsDynamicFeatureSet(CoreAI.FeatureBits.IOBJoinEnabled))
				{
					if (heardspeech.ToLower().IndexOf("join me") != -1)
					{
						if (WasNamed(heardspeech))
						{
							if (e.Mobile is PlayerMobile)
							{
								PlayerMobile thispm = (PlayerMobile)e.Mobile;
								if (m_Mobile.IOBAlignment == thispm.IOBAlignment && thispm.IOBEquipped) //has to be same alignment
								{
									m_Mobile.AttemptIOBJoin(thispm);
									return;
								}
							}
						}
						else if (heardspeech.ToLower().IndexOf("all") != -1)
						{
							if (e.Mobile is PlayerMobile)
							{
								PlayerMobile thispm = (PlayerMobile)e.Mobile;
								if (m_Mobile.IOBAlignment == thispm.IOBAlignment && thispm.IOBEquipped) //has to be same alignment
								{
									m_Mobile.AttemptIOBJoin(thispm);
									return;
								}
							}
						}
					}
				}
			}

			if (m_Mobile.Controlled && m_Mobile.Commandable && !m_Mobile.IOBFollower)
			{
				m_Mobile.DebugSay("I listen");

				if (e.Mobile.Alive && e.Mobile == m_Mobile.ControlMaster)
				{
					m_Mobile.DebugSay("Its from my master");

					// erl: clear herding attempt, if one exists
					m_Mobile.TargetLocation = Point2D.Zero;

					int[] keywords = e.Keywords;
					string speech = e.Speech;

					// allow orcs to command in their own language
					if (speech.ToLower() == "lat stai") // all stay
					{
						if (m_Mobile.CheckControlChance(e.Mobile))
						{
							m_Mobile.ControlTarget = null;
							m_Mobile.ControlOrder = OrderType.Stay;
						}
						return;
					}
					if (speech.ToLower() == "lat clomp")
					{
						BeginPickTarget(e.Mobile, OrderType.Attack);
						return;
					}
					if (speech.ToLower() == "lat follow")
					{
						BeginPickTarget(e.Mobile, OrderType.Follow);
						return;
					}

					// First, check the all*
					for (int i = 0; i < keywords.Length; ++i)
					{
						int keyword = keywords[i];

						switch (keyword)
						{
							case 0x164: // all come
								{
									if (m_Mobile.CheckControlChance(e.Mobile))
									{
										m_Mobile.ControlTarget = null;
										m_Mobile.ControlOrder = OrderType.Come;
									}

									return;
								}
							case 0x165: // all follow
								{
									BeginPickTarget(e.Mobile, OrderType.Follow);
									return;
								}
							case 0x166: // all guard
								{
									if (m_Mobile.CheckControlChance(e.Mobile))
									{
										m_Mobile.ControlTarget = null;
										m_Mobile.ControlOrder = OrderType.Guard;
									}
									return;
								}
							case 0x167: // all stop
								{
									if (m_Mobile.CheckControlChance(e.Mobile))
									{
										m_Mobile.ControlTarget = null;
										m_Mobile.ControlOrder = OrderType.Stop;
									}
									return;
								}
							case 0x168: // all kill
							case 0x169: // all attack
								{
									BeginPickTarget(e.Mobile, OrderType.Attack);
									return;
								}
							case 0x16B: // all guard me
								{
									if (m_Mobile.CheckControlChance(e.Mobile))
									{
										m_Mobile.ControlTarget = e.Mobile;
										m_Mobile.ControlOrder = OrderType.Guard;
									}
									return;
								}
							case 0x16C: // all follow me
								{
									if (m_Mobile.CheckControlChance(e.Mobile))
									{
										m_Mobile.ControlTarget = e.Mobile;
										m_Mobile.ControlOrder = OrderType.Follow;
									}
									return;
								}
							case 0x170: // all stay
								{
									if (m_Mobile.CheckControlChance(e.Mobile))
									{
										m_Mobile.ControlTarget = null;
										m_Mobile.ControlOrder = OrderType.Stay;
									}
									return;
								}
						}
					}

					// No all*, so check *command
					for (int i = 0; i < keywords.Length; ++i)
					{
						int keyword = keywords[i];

						switch (keyword)
						{
							case 0x155: // *come
								{
									if (WasNamed(speech) && m_Mobile.CheckControlChance(e.Mobile))
									{
										m_Mobile.ControlTarget = null;
										m_Mobile.ControlOrder = OrderType.Come;
									}

									return;
								}
							case 0x156: // *drop
								{
									if (!m_Mobile.IsDeadPet && !m_Mobile.Summoned && WasNamed(speech) && m_Mobile.CheckControlChance(e.Mobile))
									{
										m_Mobile.ControlTarget = null;
										m_Mobile.ControlOrder = OrderType.Drop;
									}

									return;
								}
							case 0x15A: // *follow
								{
									if (WasNamed(speech) && m_Mobile.CheckControlChance(e.Mobile))
										BeginPickTarget(e.Mobile, OrderType.Follow);

									return;
								}
							case 0x15C: // *guard
								{
									if (!m_Mobile.IsDeadPet && WasNamed(speech) && m_Mobile.CheckControlChance(e.Mobile))
									{
										m_Mobile.ControlTarget = null;
										m_Mobile.ControlOrder = OrderType.Guard;
									}

									return;
								}
							case 0x15D: // *kill
							case 0x15E: // *attack
								{
									if (!m_Mobile.IsDeadPet && WasNamed(speech) && m_Mobile.CheckControlChance(e.Mobile))
										BeginPickTarget(e.Mobile, OrderType.Attack);

									return;
								}
							case 0x15F: // *patrol
								{
									if (WasNamed(speech) && m_Mobile.CheckControlChance(e.Mobile))
									{
										m_Mobile.ControlTarget = null;
										m_Mobile.ControlOrder = OrderType.Patrol;
									}

									return;
								}
							case 0x161: // *stop
								{
									if (WasNamed(speech) && m_Mobile.CheckControlChance(e.Mobile))
									{
										m_Mobile.ControlTarget = null;
										m_Mobile.ControlOrder = OrderType.Stop;
									}

									return;
								}
							case 0x163: // *follow me
								{
									if (WasNamed(speech) && m_Mobile.CheckControlChance(e.Mobile))
									{
										m_Mobile.ControlTarget = e.Mobile;
										m_Mobile.ControlOrder = OrderType.Follow;
									}

									return;
								}
							case 0x16D: // *release
								{
									if (WasNamed(speech) && m_Mobile.CheckControlChance(e.Mobile))
									{
										if (!m_Mobile.Summoned)
										{
											e.Mobile.SendGump(new Gumps.ConfirmReleaseGump(e.Mobile, m_Mobile));
										}
										else
										{
											m_Mobile.ControlTarget = null;
											m_Mobile.ControlOrder = OrderType.Release;
										}
									}
									return;
								}

							case 0x16E: // *transfer
								{
									if (!m_Mobile.IsDeadPet && WasNamed(speech) && m_Mobile.CheckControlChance(e.Mobile))
									{
										if (m_Mobile.Summoned)
											e.Mobile.SendLocalizedMessage(1005487); // You cannot transfer ownership of a summoned creature.
										else
											BeginPickTarget(e.Mobile, OrderType.Transfert);
									}

									return;
								}
							case 0x16F: // *stay
								{
									if (WasNamed(speech) && m_Mobile.CheckControlChance(e.Mobile))
									{
										m_Mobile.ControlTarget = null;
										m_Mobile.ControlOrder = OrderType.Stay;
									}

									return;
								}
						}
					}
				}
			}
			else if (m_Mobile.IOBFollower && !m_Mobile.Tamable)
			{
				m_Mobile.DebugSay("I listen to IOBLeader");

				if (e.Mobile.Alive && e.Mobile == m_Mobile.ControlMaster && ((PlayerMobile)e.Mobile).IOBEquipped)
				{
					m_Mobile.DebugSay("Its from my IOBLeader master");

					int[] keywords = e.Keywords;
					string speech = e.Speech;

					// allow orcs to command in their own language
					if (speech.ToLower() == "lat stai") // all stay
					{
						m_Mobile.ControlTarget = null;
						m_Mobile.ControlOrder = OrderType.Stay;
						return;
					}
					if (speech.ToLower() == "lat clomp")
					{
						BeginPickTarget(e.Mobile, OrderType.Attack);
						return;
					}
					if (speech.ToLower() == "lat follow")
					{
						m_Mobile.ControlTarget = e.Mobile;
						m_Mobile.ControlOrder = OrderType.Follow;
						return;
					}

					// First, check the all*
					for (int i = 0; i < keywords.Length; ++i)
					{
						int keyword = keywords[i];

						switch (keyword)
						{
							case 0x164: // all come
								{
									m_Mobile.ControlTarget = null;
									m_Mobile.ControlOrder = OrderType.Come;
									return;
								}
							case 0x165: // all follow
							case 0x16C: // all follow me
								{
									m_Mobile.ControlTarget = e.Mobile;
									m_Mobile.ControlOrder = OrderType.Follow;
									return;
								}
							case 0x167: // all stop
								{
									m_Mobile.ControlTarget = null;
									m_Mobile.ControlOrder = OrderType.Stop;
									return;
								}
							case 0x168: // all kill
							case 0x169: // all attack
								{
									BeginPickTarget(e.Mobile, OrderType.Attack);
									return;
								}
							case 0x170: // all stay
								{
									m_Mobile.ControlTarget = null;
									m_Mobile.ControlOrder = OrderType.Stay;
									return;
								}
						}
					}

					// No all*, so check *command
					for (int i = 0; i < keywords.Length; ++i)
					{
						int keyword = keywords[i];

						switch (keyword)
						{
							case 0x155: // *come
								{
									if (WasNamed(speech))
									{
										m_Mobile.ControlTarget = null;
										m_Mobile.ControlOrder = OrderType.Come;
									}
									return;
								}
							case 0x15A: // *follow
							case 0x163: // *follow me
								{
									if (WasNamed(speech))
									{
										m_Mobile.ControlTarget = e.Mobile;
										m_Mobile.ControlOrder = OrderType.Follow;
									}

									return;
								}
							case 0x161: // *stop
								{
									if (WasNamed(speech))
									{
										m_Mobile.ControlTarget = null;
										m_Mobile.ControlOrder = OrderType.Stop;
									}

									return;
								}
							case 0x15D: // *kill
							case 0x15E: // *attack
								{
									if (!m_Mobile.IsDeadPet && WasNamed(speech))
										BeginPickTarget(e.Mobile, OrderType.Attack);

									return;
								}
							case 0x16F: // *stay
								{
									if (WasNamed(speech))
									{
										m_Mobile.ControlTarget = null;
										m_Mobile.ControlOrder = OrderType.Stay;
									}
									return;
								}
						}
					}

					if (speech.ToLower().IndexOf("all dismiss") != -1)
					{
						m_Mobile.AttemptIOBDismiss();
						return;
					}
					else if (speech.ToLower().IndexOf("dismiss") != -1)
					{
						if (WasNamed(speech))
						{
							m_Mobile.AttemptIOBDismiss();
						}
						return;
					}

				}
			}
			else
			{
				if (e.Mobile.AccessLevel >= AccessLevel.GameMaster)
				{
					m_Mobile.DebugSay("Its from a GM");

					if (m_Mobile.FindMyName(e.Speech, true))
					{
						string[] str = e.Speech.Split(' ');
						int i;

						for (i = 0; i < str.Length; i++)
						{
							string word = str[i];

							if (Insensitive.Equals(word, "obey"))
							{
								m_Mobile.SetControlMaster(e.Mobile);

								if (m_Mobile.Summoned)
									m_Mobile.SummonMaster = e.Mobile;

								return;
							}
						}
					}
				}
			}
		}

		public virtual bool Think()
		{
			if (m_Mobile == null || m_Mobile.Deleted)
				return false;

			if (CheckFlee())
				return true;

			switch (Action)
			{
				case ActionType.Wander:
					m_Mobile.OnActionWander();
					return DoActionWander();

				case ActionType.Combat:
					m_Mobile.OnActionCombat();
					return DoActionCombat();

				case ActionType.Guard:
					m_Mobile.OnActionGuard();
					return DoActionGuard();

				case ActionType.Hunt:
					m_Mobile.OnActionHunt();
					return DoActionHunt();

				case ActionType.NavStar:
					m_Mobile.OnActionNavStar();
					return DoActionNavStar();

				case ActionType.Flee:
					m_Mobile.OnActionFlee();
					return DoActionFlee();

				case ActionType.Interact:
					m_Mobile.OnActionInteract();
					return DoActionInteract();

				case ActionType.Backoff:
					m_Mobile.OnActionBackoff();
					return DoActionBackoff();

				// wea: new chasing action
				case ActionType.Chase:
					m_Mobile.OnActionChase();
					return DoActionChase();

				default:
					return false;
			}
		}

		public virtual void OnActionChanged()
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
					m_Mobile.Warmode = true;
					m_Mobile.FocusMob = null;
					m_Mobile.CurrentSpeed = m_Mobile.ActiveSpeed;
					break;

				case ActionType.Guard:
					m_Mobile.Warmode = true;
					m_Mobile.FocusMob = null;
					m_Mobile.Combatant = null;
					m_Mobile.CurrentSpeed = m_Mobile.ActiveSpeed;
					m_NextStopGuard = DateTime.Now + TimeSpan.FromSeconds(10);
					m_Mobile.CurrentSpeed = m_Mobile.ActiveSpeed;
					break;

				case ActionType.Hunt:
					m_Mobile.FocusMob = null;
					m_Mobile.Combatant = null;
					m_NextStopHunt = DateTime.Now + TimeSpan.FromSeconds(20);
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
			}
		}

		public virtual bool OnAtWayPoint()
		{
			return true;
		}

		public virtual bool DoActionWander()
		{

			if (((BaseCreature)m_Mobile).NavDestination != NavDestinations.None)
				Action = ActionType.NavStar;

			if (m_Mobile.CurrentWayPoint != null && m_Mobile.CurrentWayPoint.Deleted)
				m_Mobile.CurrentWayPoint = null;

			if (CheckHerding())
			{
				m_Mobile.DebugSay("Praise the shepherd!");
			}
			else if (m_Mobile.CurrentWayPoint != null)
			{
				WayPoint point = m_Mobile.CurrentWayPoint;
				if ((point.X != m_Mobile.Location.X || point.Y != m_Mobile.Location.Y) && point.Map == m_Mobile.Map && point.Parent == null && !point.Deleted)
				{
					m_Mobile.DebugSay("I will move towards my waypoint.");
					DoMove(m_Mobile.GetDirectionTo(m_Mobile.CurrentWayPoint));
				}
				else if (OnAtWayPoint())
				{
					m_Mobile.DebugSay("I will go to the next waypoint");
					m_Mobile.CurrentWayPoint = point.NextPoint;
					if (point.NextPoint != null && point.NextPoint.Deleted)
						m_Mobile.CurrentWayPoint = point.NextPoint = point.NextPoint.NextPoint;
				}
			}
			else if (m_Mobile.IsAnimatedDead)
			{
				// animated dead follow their master
				Mobile master = m_Mobile.SummonMaster;

				if (master != null && master.Map == m_Mobile.Map && master.InRange(m_Mobile, m_Mobile.RangePerception))
					MoveTo(master, false, 1);
				else
					WalkRandomInHome(2, 2, 1);
			}
			else if (CheckMove())
			{
				if (!m_Mobile.CheckIdle())
					WalkRandomInHome(2, 2, 1);
			}

			if (m_Mobile.Combatant != null && !m_Mobile.Combatant.Deleted && m_Mobile.Combatant.Alive && !m_Mobile.Combatant.IsDeadBondedPet)
			{
				m_Mobile.Direction = m_Mobile.GetDirectionTo(m_Mobile.Combatant);
			}

			return true;
		}

		public virtual bool DoActionCombat()
		{
			Mobile c = m_Mobile.Combatant;

			if (c == null || c.Deleted || c.Map != m_Mobile.Map || !c.Alive || c.IsDeadBondedPet || !m_Mobile.CanSee(c))
				Action = ActionType.Wander;
			else
				m_Mobile.Direction = m_Mobile.GetDirectionTo(c);

			return true;
		}

		public virtual bool DoActionGuard()
		{
			if (DateTime.Now < m_NextStopGuard)
			{
				m_Mobile.DebugSay("I am on guard");
				m_Mobile.Turn(Utility.Random(0, 2) - 1);
			}
			else
			{
				m_Mobile.DebugSay("I stop be in Guard");
				Action = ActionType.Wander;
			}

			return true;
		}

		public virtual bool DoActionHunt()
		{
			if (DateTime.Now < m_NextStopHunt)
			{
				m_Mobile.DebugSay("I am Hunting");
				WalkRandomInHome(2, 2, 1);
			}
			else
			{
				m_Mobile.DebugSay("I have Stopped Hunting");
				Action = ActionType.Wander;
			}

			return true;
		}

		public virtual bool DoActionNavStar()
		{
			if (AcquireFocusMob(m_Mobile.RangePerception, m_Mobile.FightMode, false, false, true))
			{
				if (m_Mobile.Debug)
					m_Mobile.DebugSay("I am going to attack {0}", m_Mobile.FocusMob.Name);

				m_Mobile.Combatant = m_Mobile.FocusMob;
				Action = ActionType.Combat;


			}

			else if (((BaseCreature)m_Mobile).NavDestination == NavDestinations.None && ((BaseCreature)m_Mobile).NavPoint == Point3D.Zero)
			{
				Action = ActionType.Wander;
			}

			else if (((BaseCreature)m_Mobile).NavDestination != NavDestinations.None && ((BaseCreature)m_Mobile).NavPoint == Point3D.Zero)
			{
				((BaseCreature)m_Mobile).DebugSay("NavStar: Mob requesting Beacon");
				NavStar.AddRequest(m_Mobile, ((BaseCreature)m_Mobile).NavDestination);
				NavStar.DoRequest();

				if (!SectorPathAlgorithm.SameIsland(m_Mobile.Location, ((BaseCreature)m_Mobile).NavPoint))
				{
					((BaseCreature)m_Mobile).DebugSay("Initial path check failed, no path found to destination, aborting");
					((BaseCreature)m_Mobile).NavDestination = NavDestinations.None;
					((BaseCreature)m_Mobile).NavPoint = Point3D.Zero;
					((BaseCreature)m_Mobile).Beacon = null;
					Action = ActionType.Wander;
				}

			}
			else if (((BaseCreature)m_Mobile).NavDestination != NavDestinations.None && ((BaseCreature)m_Mobile).NavPoint != Point3D.Zero)
			{

				MoveToNavPoint(((BaseCreature)m_Mobile).NavPoint, this.CanRun);

				if (m_Mobile.InRange(((BaseCreature)m_Mobile).NavPoint, 6))
				{

					if (((BaseCreature)m_Mobile).Beacon != null)
					{

						if (((BaseCreature)m_Mobile).Beacon.GetWeight((int)((BaseCreature)m_Mobile).NavDestination) != 0)
						{
							((BaseCreature)m_Mobile).DebugSay("NavStar: Mob getting new Beacon");
							((BaseCreature)m_Mobile).NavPoint = Point3D.Zero;
							NavPath = null;

						}
						else if (((BaseCreature)m_Mobile).Beacon.GetWeight((int)((BaseCreature)m_Mobile).NavDestination) == 0)
						{
							((BaseCreature)m_Mobile).DebugSay("NavStar: Mob Arrived at Destination");

							((BaseCreature)m_Mobile).NavDestination = NavDestinations.None;
							((BaseCreature)m_Mobile).NavPoint = Point3D.Zero;
							((BaseCreature)m_Mobile).Beacon = null;
							Action = ActionType.Wander;
						}
					}
					else
						((BaseCreature)m_Mobile).Beacon = null;
				}

			}

			return true;
		}

		public virtual bool DoActionFlee()
		{
			Mobile from = m_Mobile.FocusMob;

			if (from == null || from.Deleted || from.Map != m_Mobile.Map)
			{
				m_Mobile.DebugSay("I have lost im");
				Action = ActionType.Guard;
				return true;
			}

			if (WalkMobileRange(from, 1, true, m_Mobile.RangePerception * 2, m_Mobile.RangePerception * 3))
			{
				m_Mobile.DebugSay("I Have fled");
				Action = ActionType.Guard;
				return true;
			}
			else
			{
				m_Mobile.DebugSay("I am fleeing!");
			}

			return true;
		}

		public virtual bool DoActionInteract()
		{
			return true;
		}

		public virtual bool DoActionBackoff()
		{
			return true;
		}

		// wea: chasing someone / something
		public virtual bool DoActionChase()
		{
			return true;
		}

		public virtual bool Obey()
		{
			if (m_Mobile.Deleted)
				return false;

			switch (m_Mobile.ControlOrder)
			{
				case OrderType.None:
					return DoOrderNone();

				case OrderType.Come:
					return DoOrderCome();

				case OrderType.Drop:
					return DoOrderDrop();

				case OrderType.Friend:
					return DoOrderFriend();

				case OrderType.Guard:
					return DoOrderGuard();

				case OrderType.Attack:
					return DoOrderAttack();

				case OrderType.Patrol:
					return DoOrderPatrol();

				case OrderType.Release:
					return DoOrderRelease();

				case OrderType.Stay:
					return DoOrderStay();

				case OrderType.Stop:
					return DoOrderStop();

				case OrderType.Follow:
					return DoOrderFollow();

				case OrderType.Transfert:
					return DoOrderTransfert();

				default:
					return false;
			}
		}

		public virtual void OnCurrentOrderChanged()
		{
			if (m_Mobile.Deleted || m_Mobile.ControlMaster == null || m_Mobile.ControlMaster.Deleted)
				return;

			switch (m_Mobile.ControlOrder)
			{
				case OrderType.None:
					m_Mobile.ControlMaster.RevealingAction();
					m_Mobile.Home = m_Mobile.Location;
					m_Mobile.CurrentSpeed = m_Mobile.PassiveSpeed;
					m_Mobile.PlaySound(m_Mobile.GetIdleSound());
					m_Mobile.Warmode = false;
					m_Mobile.Combatant = null;
					break;

				case OrderType.Come:
					m_Mobile.ControlMaster.RevealingAction();
					m_Mobile.CurrentSpeed = m_Mobile.ActiveSpeed;
					m_Mobile.PlaySound(m_Mobile.GetIdleSound());
					m_Mobile.Warmode = false;
					m_Mobile.Combatant = null;
					break;

				case OrderType.Drop:
					m_Mobile.ControlMaster.RevealingAction();
					m_Mobile.CurrentSpeed = m_Mobile.PassiveSpeed;
					m_Mobile.PlaySound(m_Mobile.GetIdleSound());
					m_Mobile.Warmode = true;
					m_Mobile.Combatant = null;
					break;

				case OrderType.Friend:
					m_Mobile.ControlMaster.RevealingAction();

					break;
				case OrderType.Guard:
					m_Mobile.ControlMaster.RevealingAction();
					m_Mobile.CurrentSpeed = m_Mobile.ActiveSpeed;
					m_Mobile.PlaySound(m_Mobile.GetIdleSound());
					m_Mobile.Warmode = true;
					m_Mobile.Combatant = null;
					break;

				case OrderType.Attack:
					m_Mobile.ControlMaster.RevealingAction();
					m_Mobile.CurrentSpeed = m_Mobile.ActiveSpeed;
					m_Mobile.PlaySound(m_Mobile.GetIdleSound());

					m_Mobile.Warmode = true;
					m_Mobile.Combatant = null;
					break;

				case OrderType.Patrol:
					m_Mobile.ControlMaster.RevealingAction();
					m_Mobile.CurrentSpeed = m_Mobile.ActiveSpeed;
					m_Mobile.PlaySound(m_Mobile.GetIdleSound());
					m_Mobile.Warmode = false;
					m_Mobile.Combatant = null;
					break;

				case OrderType.Release:
					m_Mobile.ControlMaster.RevealingAction();
					m_Mobile.CurrentSpeed = m_Mobile.PassiveSpeed;
					m_Mobile.PlaySound(m_Mobile.GetIdleSound());
					m_Mobile.Warmode = false;
					m_Mobile.Combatant = null;
					break;

				case OrderType.Stay:
					m_Mobile.ControlMaster.RevealingAction();
					m_Mobile.CurrentSpeed = m_Mobile.PassiveSpeed;
					m_Mobile.PlaySound(m_Mobile.GetIdleSound());
					m_Mobile.Warmode = false;
					m_Mobile.Combatant = null;
					break;

				case OrderType.Stop:
					m_Mobile.ControlMaster.RevealingAction();
					m_Mobile.CurrentSpeed = m_Mobile.PassiveSpeed;
					m_Mobile.PlaySound(m_Mobile.GetIdleSound());
					m_Mobile.Warmode = false;
					m_Mobile.Combatant = null;
					break;

				case OrderType.Follow:
					m_Mobile.ControlMaster.RevealingAction();
					m_Mobile.CurrentSpeed = m_Mobile.ActiveSpeed;
					m_Mobile.PlaySound(m_Mobile.GetIdleSound());

					m_Mobile.Warmode = false;
					m_Mobile.Combatant = null;
					break;

				case OrderType.Transfert:
					m_Mobile.ControlMaster.RevealingAction();
					m_Mobile.CurrentSpeed = m_Mobile.PassiveSpeed;
					m_Mobile.PlaySound(m_Mobile.GetIdleSound());

					m_Mobile.Warmode = false;
					m_Mobile.Combatant = null;
					break;
			}
		}

		public virtual bool DoOrderNone()
		{
			if (CheckHerding())
			{
				m_Mobile.DebugSay("Praise the shepherd!");
				return true;
			}

			m_Mobile.DebugSay("I have no order");
			WalkRandomInHome(3, 2, 1);

			if (m_Mobile.Combatant != null && !m_Mobile.Combatant.Deleted && m_Mobile.Combatant.Alive && !m_Mobile.Combatant.IsDeadBondedPet)
			{
				m_Mobile.Warmode = true;
				m_Mobile.Direction = m_Mobile.GetDirectionTo(m_Mobile.Combatant);
			}
			else
			{
				m_Mobile.Warmode = false;
			}

			return true;
		}

		public virtual bool DoOrderCome()
		{
			if (CheckHerding())
			{
				m_Mobile.DebugSay("Praise the shepherd!");
				return true;
			}

			if (m_Mobile.ControlMaster != null && !m_Mobile.ControlMaster.Deleted)
			{
				int iCurrDist = (int)m_Mobile.GetDistanceToSqrt(m_Mobile.ControlMaster);

				if (iCurrDist > m_Mobile.RangePerception)
				{
					m_Mobile.DebugSay("I have lost my master. I stay here");
					m_Mobile.ControlTarget = null;
					m_Mobile.ControlOrder = OrderType.None;
				}
				else
				{
					m_Mobile.DebugSay("My master told me come");

					if (WalkMobileRange(m_Mobile.ControlMaster, 1, false, 0, 1))
					{
						if (m_Mobile.Combatant != null && !m_Mobile.Combatant.Deleted && m_Mobile.Combatant.Alive && !m_Mobile.Combatant.IsDeadBondedPet)
						{
							m_Mobile.Warmode = true;
							m_Mobile.Direction = m_Mobile.GetDirectionTo(m_Mobile.Combatant);
						}
						else
						{
							m_Mobile.Warmode = false;
						}
					}
				}
			}

			return true;
		}

		public virtual bool DoOrderDrop()
		{
			if (m_Mobile.IsDeadPet || m_Mobile.Summoned)
				return true;

			m_Mobile.DebugSay("I drop my stuff for my master");

			Container pack = m_Mobile.Backpack;

			if (pack != null)
			{
				ArrayList list = pack.Items;

				for (int i = list.Count - 1; i >= 0; --i)
					if (i < list.Count)
						((Item)list[i]).MoveToWorld(m_Mobile.Location, m_Mobile.Map);
			}

			m_Mobile.ControlTarget = null;
			m_Mobile.ControlOrder = OrderType.None;

			return true;
		}

		public virtual bool CheckHerding()
		{
			Point2D target = m_Mobile.TargetLocation;

			if (target == Point2D.Zero)
				return false; // Creature is not being herded

			double distance = m_Mobile.GetDistanceToSqrt(target);

			if (distance < 1 || distance > 20)
			{
				if (distance < 1 && target.X == 1076 && target.Y == 450 && (m_Mobile is HordeMinionFamiliar))
				{
					PlayerMobile pm = m_Mobile.ControlMaster as PlayerMobile;

					if (pm != null)
					{
						QuestSystem qs = pm.Quest;

						if (qs is DarkTidesQuest)
						{
							QuestObjective obj = qs.FindObjective(typeof(FetchAbraxusScrollObjective));

							if (obj != null && !obj.Completed)
							{
								m_Mobile.AddToBackpack(new ScrollOfAbraxus());
								obj.Complete();
							}
						}
					}
				}

				m_Mobile.TargetLocation = Point2D.Zero;
				return false; // At the target or too far away
			}

			DoMove(m_Mobile.GetDirectionTo(target));

			return true;
		}

		public virtual bool DoOrderFollow()
		{
			if (CheckHerding())
			{
				m_Mobile.DebugSay("Praise the shepherd!");
			}
			else if (m_Mobile.ControlTarget != null && !m_Mobile.ControlTarget.Deleted && m_Mobile.ControlTarget != m_Mobile)
			{
				int iCurrDist = (int)m_Mobile.GetDistanceToSqrt(m_Mobile.ControlTarget);

				if (iCurrDist > m_Mobile.RangePerception)
				{
					m_Mobile.DebugSay("I have lost the one a follow. I stay here");
					if (m_Mobile.Combatant != null && !m_Mobile.Combatant.Deleted && m_Mobile.Combatant.Alive && !m_Mobile.Combatant.IsDeadBondedPet)
					{
						m_Mobile.Warmode = true;
						m_Mobile.Direction = m_Mobile.GetDirectionTo(m_Mobile.Combatant);
					}
					else
					{
						m_Mobile.Warmode = false;
					}
				}
				else
				{
					if (m_Mobile.Debug)
						m_Mobile.DebugSay("My master told me to follow: {0}", m_Mobile.ControlTarget.Name);


					if (WalkMobileRange(m_Mobile.ControlTarget, 1, false, 0, 1))
					{
						if (m_Mobile.Combatant != null && !m_Mobile.Combatant.Deleted && m_Mobile.Combatant.Alive && !m_Mobile.Combatant.IsDeadBondedPet)
						{
							m_Mobile.Warmode = true;
							m_Mobile.Direction = m_Mobile.GetDirectionTo(m_Mobile.Combatant);
						}
						else
						{
							m_Mobile.Warmode = false;
						}
					}
				}
			}

			else
			{
				m_Mobile.DebugSay("I have nobody to follow, lets relax");
				m_Mobile.ControlTarget = null;
				m_Mobile.ControlOrder = OrderType.None;
			}

			return true;
		}

		public virtual bool DoOrderFriend()
		{
			m_Mobile.DebugSay("This order is not yet coded");

			return true;
		}

		public virtual bool DoOrderGuard()
		{
			if (m_Mobile.IsDeadPet)
				return true;

			if (CheckHerding())
			{
				m_Mobile.DebugSay("Praise the shepherd!");
				return true;
			}


			Mobile controlMaster = m_Mobile.ControlMaster;

			if (controlMaster == null || controlMaster.Deleted)
				return true;

			Mobile combatant = m_Mobile.Combatant;

			ArrayList aggressors = controlMaster.Aggressors;

			if (aggressors.Count > 0)
			{
				for (int i = 0; i < aggressors.Count; ++i)
				{
					AggressorInfo info = (AggressorInfo)aggressors[i];
					Mobile attacker = info.Attacker;

					if (attacker != null && !attacker.Deleted && attacker.GetDistanceToSqrt(m_Mobile) <= m_Mobile.RangePerception)
					{
						if (controlMaster.Alive)
						{
							if (combatant == null || attacker.GetDistanceToSqrt(controlMaster) < combatant.GetDistanceToSqrt(controlMaster))
							{
								if (m_Mobile.CanSee(attacker) && attacker.Alive)
								{
									combatant = attacker;
								}
							}
						}
						else
						{
							if (combatant == null || attacker.GetDistanceToSqrt(m_Mobile) < combatant.GetDistanceToSqrt(m_Mobile))
							{
								if (m_Mobile.CanSee(attacker) && attacker.Alive)
								{
									combatant = attacker;
								}
							}
						}
					}
				}

				if (combatant != null)
				{
					m_Mobile.DebugSay("Crap, my master has been attacked! I will atack one of those bastards!");
				}
			}

			//PIX addition (for pet's not defending themselves): My master isn't being attacked, 
			//    so check MY aggressors IFF my master's not alive or isn't around me
			if (combatant == null && (!controlMaster.Alive || m_Mobile.GetDistanceToSqrt(controlMaster) > m_Mobile.RangePerception))
			{
				aggressors = m_Mobile.Aggressors;

				if (aggressors.Count > 0)
				{
					for (int i = 0; i < aggressors.Count; ++i)
					{
						AggressorInfo info = (AggressorInfo)aggressors[i];
						Mobile attacker = info.Attacker;

						if (attacker != null && !attacker.Deleted && attacker.GetDistanceToSqrt(m_Mobile) <= m_Mobile.RangePerception)
						{
							if (combatant == null || attacker.GetDistanceToSqrt(m_Mobile) < combatant.GetDistanceToSqrt(m_Mobile))
							{
								if (m_Mobile.CanSee(attacker) && attacker.Alive)
								{
									combatant = attacker;
								}
							}
						}
					}

					if (combatant != null)
					{
						m_Mobile.DebugSay("I'm in guard mode, being attacked, but my master isn't around, so defend myself!");
					}
				}
			}

			if (combatant != null && combatant != m_Mobile && combatant != m_Mobile.ControlMaster && !combatant.Deleted && combatant.Alive && !combatant.IsDeadBondedPet && m_Mobile.CanSee(combatant) && m_Mobile.CanBeHarmful(combatant, false) && combatant.Map == m_Mobile.Map)
			{
				m_Mobile.DebugSay("Die! Die! Die!");

				m_Mobile.Combatant = combatant;
				m_Mobile.FocusMob = combatant;
				DoActionCombat();

				//PIX: We need to call Think() here or spellcasting mobs in guard
				// mode will never target their spells.
				Think();
			}
			else
			{
				m_Mobile.DebugSay("My master told me to guard him, but from what? Nobody knows! Sometimes I wonder..");

				m_Mobile.Warmode = false;

				WalkMobileRange(controlMaster, 1, false, 0, 1);
			}

			return true;
		}

		public virtual bool DoOrderAttack()
		{
			if (m_Mobile.IsDeadPet)
				return true;

			if (m_Mobile.ControlTarget == null || m_Mobile.ControlTarget.Deleted || m_Mobile.ControlTarget.Map != m_Mobile.Map || !m_Mobile.ControlTarget.Alive || m_Mobile.ControlTarget.IsDeadBondedPet || !m_Mobile.CanSee(m_Mobile.ControlTarget))
			{
				m_Mobile.DebugSay("I think he might be dead. He's not anywhere around here at least. That's cool. I'm glad he's dead.");

				m_Mobile.ControlTarget = null;
				m_Mobile.ControlOrder = OrderType.None;

				if ((m_Mobile.FightMode & FightMode.All) > 0 || (m_Mobile.FightMode & FightMode.Aggressor) > 0)
				{
					Mobile newCombatant = null;
					double newScore = 0.0;

					ArrayList list = m_Mobile.Aggressors;

					for (int i = 0; i < list.Count; ++i)
					{
						Mobile aggr = ((AggressorInfo)list[i]).Attacker;

						if (aggr.Map != m_Mobile.Map || !aggr.InRange(m_Mobile.Location, m_Mobile.RangePerception) || !m_Mobile.CanSee(aggr))
							continue;

						if (aggr.IsDeadBondedPet || !aggr.Alive)
							continue;

						double aggrScore = m_Mobile.GetValueFrom(aggr, FightMode.Closest, false);

						if ((newCombatant == null || aggrScore > newScore) && m_Mobile.InLOS(aggr))
						{
							newCombatant = aggr;
							newScore = aggrScore;
						}
					}

					if (newCombatant != null)
					{
						m_Mobile.ControlTarget = newCombatant;
						m_Mobile.ControlOrder = OrderType.Attack;
						m_Mobile.Combatant = newCombatant;
						m_Mobile.DebugSay("But -that- is not dead. Here we go again...");
						Think();
					}
				}
			}
			else
			{
				m_Mobile.DebugSay("I fight im!");
				Think();
			}

			return true;
		}

		public virtual bool DoOrderPatrol()
		{
			m_Mobile.DebugSay("This order is not yet coded");
			return true;
		}

		private class BondedPetDeathTimer : Timer
		{
			private Mobile owner;


			public BondedPetDeathTimer(Mobile target)
				: base(TimeSpan.FromSeconds(1.0))
			{
				owner = target;
				Priority = TimerPriority.FiftyMS;
			}

			protected override void OnTick()
			{
				if (owner != null)
				{
					owner.BoltEffect(0);
					owner.Kill();
					owner.BoltEffect(0);

				}
			}
		}

		public void ReleaseBondedPet()
		{
			if (m_Mobile != null)
			{
				m_Mobile.Frozen = true;
				m_Mobile.DebugSay("My master release me");

				m_Mobile.PlaySound(41);

				m_Mobile.SetControlMaster(null);
				m_Mobile.SummonMaster = null;

				m_Mobile.IsBonded = false;
				m_Mobile.BondingBegin = DateTime.MinValue;
				m_Mobile.OwnerAbandonTime = DateTime.MinValue;

				m_Mobile.BoltEffect(0);

				if (m_Mobile.DeleteOnRelease || m_Mobile.IsDeadPet)
					m_Mobile.Delete();
				else
				{
					BondedPetDeathTimer t = new BondedPetDeathTimer(m_Mobile);
					t.Start();
				}

			}
		}

		public virtual bool DoOrderRelease()
		{
			/*
			if(m_Mobile.IsBonded && !m_Mobile.IsDeadBondedPet )
			{
				m_Mobile.ControlMaster.SendGump(new ReleaseBondedGump(m_Mobile));
				return false;
			}
			*/

			m_Mobile.DebugSay("My master release me");

			m_Mobile.PlaySound(m_Mobile.GetAngerSound());

			m_Mobile.SetControlMaster(null);
			m_Mobile.SummonMaster = null;

			m_Mobile.IsBonded = false;
			m_Mobile.BondingBegin = DateTime.MinValue;
			m_Mobile.OwnerAbandonTime = DateTime.MinValue;

			if (m_Mobile.DeleteOnRelease || m_Mobile.IsDeadPet)
				m_Mobile.Delete();

			return true;
		}

		public virtual bool DoOrderStay()
		{
			if (CheckHerding())
				m_Mobile.DebugSay("Praise the shepherd!");
			else
				m_Mobile.DebugSay("My master told me to stay");

			//m_Mobile.Direction = m_Mobile.GetDirectionTo( m_Mobile.ControlMaster );

			return true;
		}

		public virtual bool DoOrderStop()
		{
			if (m_Mobile.ControlMaster == null || m_Mobile.ControlMaster.Deleted)
				return true;

			m_Mobile.DebugSay("My master told me to stop.");

			m_Mobile.Direction = m_Mobile.GetDirectionTo(m_Mobile.ControlMaster);
			m_Mobile.Home = m_Mobile.Location;

			m_Mobile.ControlTarget = null;
			m_Mobile.ControlOrder = OrderType.None;

			return true;
		}

		private class TransferItem : Item
		{
			private BaseCreature m_Creature;

			public TransferItem(BaseCreature creature)
				: base(ShrinkTable.Lookup(creature))
			{
				m_Creature = creature;

				Movable = false;
				Name = creature.Name;
			}

			public TransferItem(Serial serial)
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

				Delete();
			}

			public override bool AllowSecureTrade(Mobile from, Mobile to, Mobile newOwner, bool accepted)
			{
				if (!base.AllowSecureTrade(from, to, newOwner, accepted))
					return false;

				if (Deleted || m_Creature == null || m_Creature.Deleted || m_Creature.ControlMaster != from || !from.CheckAlive() || !to.CheckAlive())
					return false;

				if (from.Map != m_Creature.Map || !from.InRange(m_Creature, 14))
					return false;

				if (accepted && !m_Creature.CanBeControlledBy(to))
				{
					string args = String.Format("{0}\t{1}\t ", to.Name, from.Name);

					from.SendLocalizedMessage(1043248, args); // The pet refuses to be transferred because it will not obey ~1_NAME~.~3_BLANK~
					to.SendLocalizedMessage(1043249, args); // The pet will not accept you as a master because it does not trust you.~3_BLANK~
					return false;
				}
				else if (accepted && !m_Creature.CanBeControlledBy(from))
				{
					string args = String.Format("{0}\t{1}\t ", to.Name, from.Name);

					from.SendLocalizedMessage(1043250, args); // The pet refuses to be transferred because it will not obey you sufficiently.~3_BLANK~
					to.SendLocalizedMessage(1043251, args); // The pet will not accept you as a master because it does not trust ~2_NAME~.~3_BLANK~
				}
				else if (accepted && (to.Followers + m_Creature.ControlSlots) > to.FollowersMax)
				{
					to.SendLocalizedMessage(1049607); // You have too many followers to control that creature.

					return false;
				}

				return true;
			}

			public override void OnSecureTrade(Mobile from, Mobile to, Mobile newOwner, bool accepted)
			{
				if (Deleted)
					return;

				Delete();

				if (m_Creature == null || m_Creature.Deleted || m_Creature.ControlMaster != from || !from.CheckAlive() || !to.CheckAlive())
					return;

				if (from.Map != m_Creature.Map || !from.InRange(m_Creature, 14))
					return;

				if (accepted)
				{
					//normal test for not bonded transfer
					if (m_Creature.SetControlMaster(to))
					{
						if (m_Creature.Summoned)
							m_Creature.SummonMaster = to;

						m_Creature.ControlTarget = to;
						m_Creature.ControlOrder = OrderType.Follow;

						m_Creature.BondingBegin = DateTime.MinValue;
						m_Creature.OwnerAbandonTime = DateTime.MinValue;
						m_Creature.IsBonded = false;

						m_Creature.PlaySound(m_Creature.GetIdleSound());

						string args = String.Format("{0}\t{1}\t{2}", from.Name, m_Creature.Name, to.Name);

						from.SendLocalizedMessage(1043253, args); // You have transferred your pet to ~3_GETTER~.
						to.SendLocalizedMessage(1043252, args); // ~1_NAME~ has transferred the allegiance of ~2_PET_NAME~ to you.
					}

				}
			}
		}

		public virtual bool DoOrderTransfert()
		{
			if (m_Mobile.IsDeadPet)
				return true;

			Mobile from = m_Mobile.ControlMaster;
			Mobile to = m_Mobile.ControlTarget;

			if (from != to && from != null && !from.Deleted && to != null && !to.Deleted && to.Player)
			{
				if (m_Mobile.Debug)
					m_Mobile.DebugSay("Begin transfer with {0}", to.Name);

				if (!m_Mobile.CanBeControlledBy(to))
				{
					string args = String.Format("{0}\t{1}\t ", to.Name, from.Name);

					from.SendLocalizedMessage(1043248, args); // The pet refuses to be transferred because it will not obey ~1_NAME~.~3_BLANK~
					to.SendLocalizedMessage(1043249, args); // The pet will not accept you as a master because it does not trust you.~3_BLANK~
				}
				else if (!m_Mobile.CanBeControlledBy(from))
				{
					string args = String.Format("{0}\t{1}\t ", to.Name, from.Name);

					from.SendLocalizedMessage(1043250, args); // The pet refuses to be transferred because it will not obey you sufficiently.~3_BLANK~
					to.SendLocalizedMessage(1043251, args); // The pet will not accept you as a master because it does not trust ~2_NAME~.~3_BLANK~
				}
				else
				{
					NetState fromState = from.NetState, toState = to.NetState;

					if (fromState != null && toState != null)
					{
						if (from.HasTrade)
						{
							from.SendLocalizedMessage(1010507); // You cannot transfer a pet with a trade pending
						}
						else if (to.HasTrade)
						{
							to.SendLocalizedMessage(1010507); // You cannot transfer a pet with a trade pending
						}
						else
						{
							Container c = fromState.AddTrade(toState);
							c.DropItem(new TransferItem(m_Mobile));
						}
					}
				}
			}

			m_Mobile.ControlTarget = null;
			m_Mobile.ControlOrder = OrderType.Stay;

			return true;
		}

		public virtual bool DoBardPacified()
		{
			if (DateTime.Now < m_Mobile.BardEndTime)
			{
				m_Mobile.DebugSay("I am pacified, I wait");
				m_Mobile.Combatant = null;
				m_Mobile.Warmode = false;

			}
			else
			{
				m_Mobile.DebugSay("I no more Pacified");
				m_Mobile.BardPacified = false;
			}

			return true;
		}

		public virtual bool DoBardProvoked()
		{
			if (DateTime.Now >= m_Mobile.BardEndTime && (m_Mobile.BardMaster == null || m_Mobile.BardMaster.Deleted || m_Mobile.BardMaster.Map != m_Mobile.Map || m_Mobile.GetDistanceToSqrt(m_Mobile.BardMaster) > m_Mobile.RangePerception))
			{
				m_Mobile.DebugSay("I have lost my provoker");
				m_Mobile.BardProvoked = false;
				m_Mobile.BardMaster = null;
				m_Mobile.BardTarget = null;

				m_Mobile.Combatant = null;
				m_Mobile.Warmode = false;
			}
			else
			{
				if (m_Mobile.BardTarget == null || m_Mobile.BardTarget.Deleted || m_Mobile.BardTarget.Map != m_Mobile.Map || m_Mobile.GetDistanceToSqrt(m_Mobile.BardTarget) > m_Mobile.RangePerception || m_Mobile.BardTarget.Hidden || !m_Mobile.BardTarget.Alive)
				{
					m_Mobile.DebugSay("I have lost my provoke target");
					m_Mobile.BardProvoked = false;
					m_Mobile.BardMaster = null;
					m_Mobile.BardTarget = null;

					m_Mobile.Combatant = null;
					m_Mobile.Warmode = false;
				}
				else
				{
					m_Mobile.Combatant = m_Mobile.BardTarget;
					m_Action = ActionType.Combat;

					m_Mobile.OnThink();
					Think();
				}
			}

			return true;
		}

		public virtual bool MoveToSound(Point3D m, bool run, int Range)
		{
			if (m_Mobile.Deleted || m_Mobile.DisallowAllMoves || m == Point3D.Zero)
				return false;

			if (m_Mobile.InRange(m, Range))
			{
				m_Path = null;
				return true;
			}

			if (m_Path != null && m_Path.Goal == (IPoint3D)m)
			{
				if (m_Path.Follow(run, Range))
				{
					m_Path = null;
					return true;
				}
			}

			else
			{
				m_Path = new PathFollower(m_Mobile, (IPoint3D)m);
				m_Path.Mover = new MoveMethod(DoMoveImpl);

				if (m_Path.Follow(run, Range))
				{
					m_Path = null;
					return true;
				}
			}

			return false;
		}

		public virtual bool MoveToNavPoint(Point3D m, bool run)
		{
			if (m_Mobile.Deleted || m_Mobile.DisallowAllMoves || m == Point3D.Zero)
				return false;


			if (NavPath != null)
			{

				NavPath.Follow(run, 1);

			}

			else
			{

				NavPath = new PathFollower(m_Mobile, (IPoint3D)m);
				NavPath.Mover = new MoveMethod(DoMoveImpl);
				NavPath.Follow(run, 1);

			}

			return false;
		}


		public virtual void WalkRandom(int iChanceToNotMove, int iChanceToDir, int iSteps)
		{
			if (m_Mobile.Deleted || m_Mobile.DisallowAllMoves)
				return;

			for (int i = 0; i < iSteps; i++)
			{
				if (Utility.Random(8 * iChanceToNotMove) <= 8)
				{
					int iRndMove = Utility.Random(0, 8 + (9 * iChanceToDir));

					switch (iRndMove)
					{
						case 0:
							DoMove(Direction.Up);
							break;
						case 1:
							DoMove(Direction.North);
							break;
						case 2:
							DoMove(Direction.Left);
							break;
						case 3:
							DoMove(Direction.West);
							break;
						case 5:
							DoMove(Direction.Down);
							break;
						case 6:
							DoMove(Direction.South);
							break;
						case 7:
							DoMove(Direction.Right);
							break;
						case 8:
							DoMove(Direction.East);
							break;
						default:
							DoMove(m_Mobile.Direction);
							break;
					}
				}
			}
		}

		public class BondedPetReleaseGump : Gump
		{
			private BaseCreature m_Pet;

			public BondedPetReleaseGump(BaseCreature pet)
				: base(50, 50)
			{

				bool bStatLoss = true;

				m_Pet = pet;

				AddPage(0);

				AddBackground(10, 10, 265, bStatLoss ? 275 : 140, 0x242C);

				AddItem(205, 30, 0x4);
				AddItem(227, 30, 0x5);

				AddItem(180, 68, 0xCAE);
				AddItem(195, 80, 0xCAD);
				AddItem(218, 85, 0xCB0);

				AddHtml(30, 30, 150, 75, "<div align=CENTER>Wilt thou sanctify the release of:</div>", false, false); // <div align=center>Wilt thou sanctify the resurrection of:</div>
				AddHtml(30, 70, 150, 25, String.Format("<div align=CENTER>{0}</div>", pet.Name), true, false);

				if (bStatLoss)
				{
					string statlossmessage = "By releasing your bonded companion, the spirit link between master and follower will be shattered. Alas, the companion's life will be lost in the process.";
					AddHtml(30, 105, 195, 135, String.Format("<div align=CENTER>{0}</div>", statlossmessage), true, false);
				}

				AddButton(40, bStatLoss ? 245 : 105, 0x81A, 0x81B, 1, GumpButtonType.Reply, 0); // Okay
				AddButton(110, bStatLoss ? 245 : 105, 0x819, 0x818, 0, GumpButtonType.Reply, 0); // Cancel
			}

			public override void OnResponse(NetState state, RelayInfo info)
			{
				if (m_Pet.Deleted || !m_Pet.IsBonded)
					return;

				PlayerMobile pm = state.Mobile as PlayerMobile;

				if (pm == null)
					return;

				pm.CloseGump(typeof(BondedPetReleaseGump));

				if (info.ButtonID == 1) // continue
				{
					m_Pet.AIObject.ReleaseBondedPet();
				}
				else
				{
					pm.SendMessage("You decide not to release your companion.");
				}
			}
		}


		public double TransformMoveDelay(double delay)
		{
			bool isPassive = (delay == m_Mobile.PassiveSpeed);
			bool isControled = (m_Mobile.Controlled || m_Mobile.Summoned);

			if (delay == 0.2)
				delay = 0.3;
			else if (delay == 0.25)
				delay = 0.45;
			else if (delay == 0.3)
				delay = 0.6;
			else if (delay == 0.4)
				delay = 0.9;
			else if (delay == 0.5)
				delay = 1.05;
			else if (delay == 0.6)
				delay = 1.2;
			else if (delay == 0.8)
				delay = 1.5;

			if (isPassive)
				delay += 0.2;

			if (!isControled)
			{
				delay += 0.1;
			}
			else if (m_Mobile.Controlled)
			{
				if (m_Mobile.ControlOrder == OrderType.Follow && m_Mobile.ControlTarget == m_Mobile.ControlMaster)
					delay *= 0.5;

				delay -= 0.075;
			}

			//if taking damage should slow down our creature
			if (DmgSlowsMovement && !m_Mobile.IsDeadBondedPet)
			{
				double offset = (double)m_Mobile.Hits / m_Mobile.HitsMax;

				if (offset < 0.0)
					offset = 0.0;
				else if (offset > 1.0)
					offset = 1.0;

				offset = 1.0 - offset;

				delay += (offset * 0.8);
			}

			if (delay < 0.0)
				delay = 0.0;

			return delay;
		}

		private DateTime m_NextMove;

		public DateTime NextMove
		{
			get { return m_NextMove; }
			set { m_NextMove = value; }
		}

		public virtual bool CheckMove()
		{
			return (DateTime.Now >= m_NextMove);
		}

		public virtual bool DoMove(Direction d)
		{
			return DoMove(d, false);
		}

		public virtual bool DoMove(Direction d, bool badStateOk)
		{
			MoveResult res = DoMoveImpl(d);

			return (res == MoveResult.Success || res == MoveResult.SuccessAutoTurn || (badStateOk && res == MoveResult.BadState));
		}

		private static Queue m_Obstacles = new Queue();

		public virtual MoveResult DoMoveImpl(Direction d)
		{
			if (m_Mobile.Deleted || m_Mobile.Frozen || m_Mobile.Paralyzed || (m_Mobile.Spell != null && m_Mobile.Spell.IsCasting) || m_Mobile.DisallowAllMoves)
				return MoveResult.BadState;
			else if (!CheckMove())
				return MoveResult.BadState;

			// This makes them always move one step, never any direction changes
			m_Mobile.Direction = d;

			TimeSpan delay = TimeSpan.FromSeconds(TransformMoveDelay(m_Mobile.CurrentSpeed));

			m_NextMove += delay;

			if (m_NextMove < DateTime.Now)
				m_NextMove = DateTime.Now;

			m_Mobile.Pushing = false;

			MoveImpl.IgnoreMovableImpassables = (m_Mobile.CanMoveOverObstacles && !m_Mobile.CanDestroyObstacles);

			if ((m_Mobile.Direction & Direction.Mask) != (d & Direction.Mask))
			{
				bool v = m_Mobile.Move(d);

				MoveImpl.IgnoreMovableImpassables = false;
				return (v ? MoveResult.Success : MoveResult.Blocked);
			}
			else if (!m_Mobile.Move(d))
			{
				bool wasPushing = m_Mobile.Pushing;

				bool blocked = true;

				bool canOpenDoors = m_Mobile.CanOpenDoors;
				bool canDestroyObstacles = m_Mobile.CanDestroyObstacles;

				if (canOpenDoors || canDestroyObstacles)
				{
					m_Mobile.DebugSay("My movement was blocked, I will try to clear some obstacles.");

					Map map = m_Mobile.Map;

					if (map != null)
					{
						int x = m_Mobile.X, y = m_Mobile.Y;
						Movement.Movement.Offset(d, ref x, ref y);

						int destroyables = 0;

						IPooledEnumerable eable = map.GetItemsInRange(new Point3D(x, y, m_Mobile.Location.Z), 1);

						foreach (Item item in eable)
						{
							if (canOpenDoors && item is BaseDoor && (item.Z + item.ItemData.Height) > m_Mobile.Z && (m_Mobile.Z + 16) > item.Z)
							{
								if (item.X != x || item.Y != y)
									continue;

								BaseDoor door = (BaseDoor)item;

								if (!door.Locked || !door.UseLocks())
									m_Obstacles.Enqueue(door);

								if (!canDestroyObstacles)
									break;
							}
							else if (canDestroyObstacles && item.Movable && item.ItemData.Impassable && (item.Z + item.ItemData.Height) > m_Mobile.Z && (m_Mobile.Z + 16) > item.Z)
							{
								if (!m_Mobile.InRange(item.GetWorldLocation(), 1))
									continue;

								m_Obstacles.Enqueue(item);
								++destroyables;
							}
						}

						eable.Free();

						if (destroyables > 0)
							Effects.PlaySound(new Point3D(x, y, m_Mobile.Z), m_Mobile.Map, 0x3B3);

						if (m_Obstacles.Count > 0)
							blocked = false; // retry movement

						while (m_Obstacles.Count > 0)
						{
							Item item = (Item)m_Obstacles.Dequeue();

							if (item is BaseDoor)
							{
								m_Mobile.DebugSay("Little do they expect, I've learned how to open doors. Didn't they read the script??");
								m_Mobile.DebugSay("*twist*");

								((BaseDoor)item).Use(m_Mobile);
							}
							else
							{
								if (m_Mobile.Debug)
									m_Mobile.DebugSay("Ugabooga. I'm so big and tough I can destroy it: {0}", item.GetType().Name);

								if (item is Container)
								{
									Container cont = (Container)item;

									for (int i = 0; i < cont.Items.Count; ++i)
									{
										Item check = (Item)cont.Items[i];

										if (check.Movable && check.ItemData.Impassable && (item.Z + check.ItemData.Height) > m_Mobile.Z)
											m_Obstacles.Enqueue(check);
									}

									cont.Destroy();
								}
								else
								{
									item.Delete();
								}
							}
						}

						if (!blocked)
							blocked = !m_Mobile.Move(d);
					}
				}

				if (blocked)
				{
					int offset = (Utility.RandomDouble() >= 0.6 ? 1 : -1);

					for (int i = 0; i < 2; ++i)
					{
						m_Mobile.TurnInternal(offset);

						if (m_Mobile.Move(m_Mobile.Direction))
						{
							MoveImpl.IgnoreMovableImpassables = false;
							return MoveResult.SuccessAutoTurn;
						}
					}

					MoveImpl.IgnoreMovableImpassables = false;
					return (wasPushing ? MoveResult.BadState : MoveResult.Blocked);
				}
				else
				{
					MoveImpl.IgnoreMovableImpassables = false;
					return MoveResult.Success;
				}
			}

			MoveImpl.IgnoreMovableImpassables = false;
			return MoveResult.Success;
		}

		public virtual void WalkRandomInHome(int iChanceToNotMove, int iChanceToDir, int iSteps)
		{
			if (m_Mobile.Deleted || m_Mobile.DisallowAllMoves)
				return;

			if (m_Mobile.Home == Point3D.Zero)
			{
				WalkRandom(iChanceToNotMove, iChanceToDir, iSteps);
			}
			else
			{
				for (int i = 0; i < iSteps; i++)
				{
					if (m_Mobile.RangeHome != 0)
					{
						int iCurrDist = (int)m_Mobile.GetDistanceToSqrt(m_Mobile.Home);

						if (iCurrDist < m_Mobile.RangeHome * 2 / 3)
						{
							WalkRandom(iChanceToNotMove, iChanceToDir, 1);
						}
						else if (iCurrDist > m_Mobile.RangeHome)
						{
							DoMove(m_Mobile.GetDirectionTo(m_Mobile.Home));
						}
						else
						{
							if (Utility.Random(10) > 5)
							{
								DoMove(m_Mobile.GetDirectionTo(m_Mobile.Home));
							}
							else
							{
								WalkRandom(iChanceToNotMove, iChanceToDir, 1);
							}
						}
					}
					else
					{
						if (m_Mobile.Location != m_Mobile.Home)
						{
							DoMove(m_Mobile.GetDirectionTo(m_Mobile.Home));
						}
						else
						{
							if (m_Mobile.Spawner != null)
								m_Mobile.Direction = m_Mobile.Spawner.MobileDirection;
						}
					}
				}
			}
		}

		public virtual bool CheckFlee()
		{
			if (m_Mobile.CheckFlee())
			{
				Mobile combatant = m_Mobile.Combatant;

				if (combatant == null)
				{
					WalkRandom(1, 2, 1);
				}
				else
				{
					Direction d = combatant.GetDirectionTo(m_Mobile);

					d = (Direction)((int)d + Utility.RandomMinMax(-1, +1));

					m_Mobile.Direction = d;
					m_Mobile.Move(d);
				}

				return true;
			}

			return false;
		}

		protected PathFollower m_Path;

		public PathFollower NavPath;

		public virtual void OnTeleported()
		{
			if (m_Path != null)
			{
				m_Mobile.DebugSay("Teleported; repathing");
				m_Path.ForceRepath();
			}
		}
		/*
		public virtual bool MoveTo(Mobile m, bool run, int range)
		{
			if (m_Mobile.Deleted || m_Mobile.DisallowAllMoves || m == null || m.Deleted)
				return false;

			Direction dirTo;

			dirTo = m_Mobile.GetDirectionTo(m);
			// Add the run flag
			if (run)
				dirTo = dirTo | Direction.Running;

			if (m_Mobile.InRange(m, range))
			{
				m_Mobile.Direction = dirTo; //make sure we point toward are enemy
				m_Path = null;
				return true;
			}

			if (m_Path != null && m_Path.Goal == m)
			{
				if (m_Path.Follow(run, 1))
				{
					m_Path = null;
					return true;
				}
			}
			else if (!DoMove(dirTo, true))
			{
				m_Path = new PathFollower(m_Mobile, m);
				m_Path.Mover = new MoveMethod(DoMoveImpl);

				if (m_Path.Follow(run, 1))
				{
					m_Path = null;
					return true;
				}
			}
			else
			{
				m_Path = null;
				return true;
			}

			return false;
		}*/

		public virtual bool MoveTo(Mobile m, bool run, int range)
		{
			if (m_Mobile.Deleted || m_Mobile.DisallowAllMoves || m == null || m.Deleted)
				return false;

			return MoveTo(m.Location, run, range);
		}

		public bool MoveTo(Point3D px, bool run, int range)
		{
			Direction dirTo;

			dirTo = m_Mobile.GetDirectionTo(px);
			// Add the run flag
			if (run)
				dirTo = dirTo | Direction.Running;

			if (m_Mobile.InRange(px, range))
			{
				m_Mobile.Direction = dirTo; //make sure we point toward are enemy
				m_Path = null;
				return true;
			}

			if (m_Path != null && m_Path.Goal == (IPoint3D)px)
			{
				if (m_Path.Follow(run, 1))
				{
					m_Path = null;
					return true;
				}
			}
			else if (!DoMove(dirTo, true))
			{
				m_Path = new PathFollower(m_Mobile, px);
				m_Path.Mover = new MoveMethod(DoMoveImpl);

				if (m_Path.Follow(run, 1))
				{
					m_Path = null;
					return true;
				}
			}
			else
			{
				m_Path = null;
				return true;
			}

			return false;
		}

		/*
		 *  Walk at range distance from mobile
		 *
		 *	iSteps : Number of steps
		 *	bRun   : Do we run
		 *	iWantDistMin : The minimum distance we want to be
		 *  iWantDistMax : The maximum distance we want to be
		 *
		 */
		public virtual bool WalkMobileRange(Mobile m, int iSteps, bool bRun, int iWantDistMin, int iWantDistMax)
		{
			if (m_Mobile.Deleted || m_Mobile.DisallowAllMoves)
				return false;

			Map map = m_Mobile.Map;

			if (m != null)
			{
				//if were a human mob that can run show that to the world walk/run accordingly
				if (CanRun)
				{
					if ((m.Direction & Direction.Running) != 0)
						bRun = true;
					else
						bRun = false;
				}

				for (int i = 0; i < iSteps; i++)
				{
					// Get the curent distance
					int iCurrDist = (int)m_Mobile.GetDistanceToSqrt(m);

					if (iCurrDist < iWantDistMin || iCurrDist > iWantDistMax)
					{
						bool needCloser = (iCurrDist > iWantDistMax);
						bool needFurther = !needCloser;

						if (needCloser && m_Path != null && m_Path.Goal == m)
						{

							if (m_Path.Follow(bRun, 1))
								m_Path = null;
						}
						else
						{
							Direction dirTo;

							if (iCurrDist > iWantDistMax)
								dirTo = m_Mobile.GetDirectionTo(m);
							else
								dirTo = m.GetDirectionTo(m_Mobile);

							// Add the run flag
							if (bRun)
								dirTo = dirTo | Direction.Running;

							if (!DoMove(dirTo, true) && needCloser)
							{
								IPooledEnumerable eable = map.GetItemsInRange(m_Mobile.Location, 10);
								foreach (Item item in eable)
								{
									if (item is BaseBoat && ((BaseBoat)item).Contains(m) && (((BaseBoat)item).PPlank.IsOpen || ((BaseBoat)item).SPlank.IsOpen) && !((BaseBoat)item).Contains(m_Mobile))
									{
										if (((BaseBoat)item).PPlank.IsOpen)
											((BaseBoat)item).PPlank.OnDoubleClick(m_Mobile);
										else
											((BaseBoat)item).SPlank.OnDoubleClick(m_Mobile);
									}
								}

								m_Path = new PathFollower(m_Mobile, m);
								m_Path.Mover = new MoveMethod(DoMoveImpl);

								if (m_Path.Follow(bRun, 1))
									m_Path = null;
							}
							else
							{
								m_Path = null;
							}
						}
					}
					else
					{
						return true;
					}
				}

				// Get the curent distance
				int iNewDist = (int)m_Mobile.GetDistanceToSqrt(m);

				if (iNewDist >= iWantDistMin && iNewDist <= iWantDistMax)
					return true;
				else
					return false;
			}

			return false;
		}


		public enum SortDirection
		{
			Ascending,
			Descending
		}

		public class AI_Sort : IComparer
		{
			private SortDirection m_direction = SortDirection.Ascending;
			private Mobile m;
			FightMode type;
			public AI_Sort(SortDirection direction, Mobile target, FightMode acqType)
			{
				m_direction = direction;
				m = target;
				type = acqType;
			}

			/// <summary>
			/// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
			/// </summary>
			/// <param name="x">The first object to compare.</param>
			/// <param name="y">The second object to compare.</param>
			/// <returns>
			/// Value Condition Less than zero <paramref name="x"/> is less than <paramref name="y"/>. Zero <paramref name="x"/> equals <paramref name="y"/>. Greater than zero <paramref name="x"/> is greater than <paramref name="y"/>.
			/// </returns>
			/// <exception cref="T:System.ArgumentException">Neither <paramref name="x"/> nor <paramref name="y"/> implements the <see cref="T:System.IComparable"/> interface.-or- <paramref name="x"/> and <paramref name="y"/> are of different types and neither one can handle comparisons with the other. </exception>
			int IComparer.Compare(object x, object y)
			{
				Mobile mobileX = (Mobile)x;
				Mobile mobileY = (Mobile)y;

				if (mobileX == null && mobileY == null)
				{
					return 0;
				}
				else if (mobileX == null && mobileY != null)
				{
					return (this.m_direction == SortDirection.Ascending) ? -1 : 1;
				}
				else if (mobileX != null && mobileY == null)
				{
					return (this.m_direction == SortDirection.Ascending) ? 1 : -1;
				}
				else
				{
					switch (type)
					{
						case FightMode.Weakest:
							{
								return (this.m_direction == SortDirection.Ascending) ?
									mobileX.Hits.CompareTo(mobileY.Hits) :
									mobileY.Hits.CompareTo(mobileX.Hits);
							}
						case FightMode.Int:
							{
								return (this.m_direction == SortDirection.Ascending) ?
									mobileX.Int.CompareTo(mobileY.Int) :
									mobileY.Int.CompareTo(mobileX.Int);
							}
						case FightMode.Str:	// same as FightMode.Strongest
							{
								return (this.m_direction == SortDirection.Ascending) ?
									mobileX.Str.CompareTo(mobileY.Str) :
									mobileY.Str.CompareTo(mobileX.Str);
							}
						case FightMode.Dex:
							{
								return (this.m_direction == SortDirection.Ascending) ?
									mobileX.Dex.CompareTo(mobileY.Dex) :
									mobileY.Dex.CompareTo(mobileX.Dex);
							}
						case FightMode.Closest:
							{
								return (this.m_direction == SortDirection.Ascending) ?
									mobileX.GetDistanceToSqrt(m).CompareTo(mobileY.GetDistanceToSqrt(m)) :
									mobileY.GetDistanceToSqrt(m).CompareTo(mobileX.GetDistanceToSqrt(m));
							}
						default:
							{	// do not move list items unless you need to since this sort is called multiple times
								//	to provide a compound sort
								return 0;
							}
					}
				}
			}
		}

		/* ** total rewrite of AcquireFocusMob() **
		 * We split the FightMode into two different bitmasks: 
		 * (1) The TYPE of creature to focus on (murderer, Evil, Aggressor, etc.)
		 * (2) The SELECTION parameters (closest, smartest, strongest, etc.)
		 * We then enumerate each value contained in the TYPE bitmask and pass each one to the
		 * AcquireFocusMobWorker() function along with the SELECTION mask.
		 * AcquireFocusMobWorker() will perform a similar enumeration over the SELECTION mask
		 * to build a sorted list of compound selection criteria, for instance Closest and Strongest. 
		 * Differences from OSI: Most creatures will act the same as on OSI; and if they don�t, we probably
		 * set the FightMode flags wrong for that creature. The real difference is the flexibility to do things
		 * not supported on OSI like creating compound aggression formulas like: 
		 * �Focus on all Evil and Criminal players while attacking the Weakest first with the highest Intelligence�
		 */
		private static ArrayList m_FightModeValues = new ArrayList(Enum.GetValues(typeof(FightMode)));
		public bool AcquireFocusMob(int iRange, FightMode acqType, bool bPlayerOnly, bool bFacFriend, bool bFacFoe)
		{
			// separate the flags from the target types
			FightMode acqFlags = (FightMode)((uint)acqType & 0xFFFF0000);
			acqType = (FightMode)((uint)acqType & 0x0000FFFF);

			//Use a redefined priority list if one is present
			IList sourcePriority = m_FightModeValues;
			if (m_Mobile != null && m_Mobile is BaseCreature && !m_Mobile.Deleted && ((BaseCreature)m_Mobile).FightModePriority != null)
				sourcePriority = ((BaseCreature)m_Mobile).FightModePriority;

			try
			{	// if we have something to acquire
				if (acqType > 0)
					// for each enum value, check the passed-in acqType for a match
					for (int ix = 0; ix < sourcePriority.Count; ix++)
					{	// if this fight-mode-value exists in the acqType
						if (((int)acqType & (int)sourcePriority[ix]) >= 1)
							if (AcquireFocusMobWorker(iRange, (FightMode)sourcePriority[ix], acqFlags, bPlayerOnly, bFacFriend, bFacFoe))
								return true;
					}
			}
			catch (Exception ex)
			{
				LogHelper.LogException(ex);
			}
			finally
			{	// we moved the delay setting code out of AcquireFocusMobWorker(AFM) since we need to call it once for each acqType.
				// If we were to leave it in AFM then we would only be able to call AFM the first time through the loop.
				// the new timing model is where are allowed to process ALL acquire types before being forced to wait
				if (m_Mobile.NextReacquireTime <= DateTime.Now)
					m_Mobile.NextReacquireTime = DateTime.Now + m_Mobile.ReacquireDelay;
			}

			// no one to fight
			return false;
		}

		/*
		 * Here we check to acquire a target from our surronding
		 * 
		 *  iRange : The range
		 *  acqType : A type of acquire we want (closest, strongest, etc)
		 *  bPlayerOnly : Don't bother with other creatures or NPCs, want a player
		 *  bFacFriend : Check people in my faction
		 *  bFacFoe : Check people in other factions
		 * 
		 */
		public virtual bool AcquireFocusMobWorker(int iRange, FightMode acqType, FightMode acqFlags, bool bPlayerOnly, bool bFacFriend, bool bFacFoe)
		{
			if (m_Mobile.Deleted)
				return false;

			if (m_Mobile.BardProvoked)
			{
				if (m_Mobile.BardTarget == null || m_Mobile.BardTarget.Deleted)
				{
					m_Mobile.FocusMob = null;
					return false;
				}
				else
				{
					m_Mobile.FocusMob = m_Mobile.BardTarget;
					return (m_Mobile.FocusMob != null);
				}
			}
			else if (m_Mobile.Controlled)
			{
				if (m_Mobile.ControlTarget == null || m_Mobile.ControlTarget.Deleted || !m_Mobile.ControlTarget.Alive || m_Mobile.ControlTarget.IsDeadBondedPet || !m_Mobile.InRange(m_Mobile.ControlTarget, m_Mobile.RangePerception * 2))
				{
					m_Mobile.FocusMob = null;
					return false;
				}
				else
				{
					m_Mobile.FocusMob = m_Mobile.ControlTarget;
					return (m_Mobile.FocusMob != null);
				}
			}

			if (m_Mobile.ConstantFocus != null && m_Mobile.ConstantFocus.Alive && !m_Mobile.ConstantFocus.Deleted && (!m_Mobile.ConstantFocus.Hidden || (m_Mobile.ConstantFocus.Hidden && CanReveal)) && m_Mobile.ConstantFocus.GetDistanceToSqrt(m_Mobile) < 20)
			{
				m_Mobile.DebugSay("Acquired my constant focus");
				if (m_Mobile.ConstantFocus.Hidden)
				{
					//plasma: set the hidden time here unless it's already set
					if (m_TargetHideTime == DateTime.MinValue)
					{
						m_TargetHideTime = DateTime.Now;
						m_Mobile.FocusMob = m_Mobile.ConstantFocus;
						return true;
					}
					else
					{
						//Otherwise, check to see if they've been hidden for 10 seconds
						//that is easily enough time for the mob to reveal, so if they haven't,
						//it's likely they are too busy healing from the pets to do anything else.
						//In this case we break the constant focus for now.
						if (DateTime.Now - m_TargetHideTime < TimeSpan.FromSeconds(10))
						{
							m_Mobile.FocusMob = m_Mobile.ConstantFocus;
							return true;
						}
					}
				}
				else
				{
					//reset hidden time
					if (m_TargetHideTime != DateTime.MinValue)
					{
						m_TargetHideTime = DateTime.MinValue;
					}
					m_Mobile.FocusMob = m_Mobile.ConstantFocus;
					return true;
				}
			}

			if (m_Mobile.NextReacquireTime > DateTime.Now)
			{
				m_Mobile.FocusMob = null;
				return false;
			}

			m_Mobile.DebugSay("Acquiring {0}...", acqType);

			Map map = m_Mobile.Map;

			if (map != null)
			{
				Mobile newFocusMob = null;

				// add each mobile found to our list
				IPooledEnumerable eable = map.GetMobilesInRange(m_Mobile.Location, iRange);
				ArrayList list = new ArrayList();
				foreach (Mobile m in eable)
				{
					if (m != null)
						list.Add(m);
				}
				eable.Free();

				// now sort our list based on the fight AI flags, weakest,closest,strongest,etc
				if (acqFlags > 0)
					// for each enum value, check the passed-in acqType for a match
					for (int ix = 0; ix < m_FightModeValues.Count; ix++)
					{	// if this fight-mode-flag exists in the acqFlags
						if (((int)acqFlags & (int)m_FightModeValues[ix]) >= 1)
						{	// sort the list N times to percolate the 'best fit' values to the head of the list
							SortDirection direction = (((FightMode)m_FightModeValues[ix] & (FightMode.Weakest | FightMode.Closest)) > 0) ? SortDirection.Ascending : SortDirection.Descending;
							list.Sort(new AI_Sort(direction, m_Mobile, (FightMode)m_FightModeValues[ix]));
						}
					}

				// build a quick lookup table if we are conditional-attack AI to see if they should be attacked
				//	we use thie 'memory'to remember hidden players
				Dictionary<Mobile, object> Fought = new Dictionary<Mobile, object>();
				if ((acqType & FightMode.All) == 0)
				{
					Mobile mx;
					for (int a = 0; a < m_Mobile.Aggressors.Count; ++a)
					{
						mx = (m_Mobile.Aggressors[a] as AggressorInfo).Attacker;
						if (mx != null && mx.Deleted == false && mx.Alive && !mx.IsDeadBondedPet && m_Mobile.CanBeHarmful(mx, false) && (m_Mobile.Aggressors[a] as AggressorInfo).Expired == false && Fought.ContainsKey(mx) == false)
							Fought[mx] = null;
					}
					for (int a = 0; a < m_Mobile.Aggressed.Count; ++a)
					{
						mx = (m_Mobile.Aggressed[a] as AggressorInfo).Defender;
						if (mx != null && mx.Deleted == false && mx.Alive && !mx.IsDeadBondedPet && m_Mobile.CanBeHarmful(mx, false) && (m_Mobile.Aggressed[a] as AggressorInfo).Expired == false && Fought.ContainsKey(mx) == false)
							Fought[mx] = null;
					}
				}

				// okay, pick a target - the first one *should* be the best match since our list is sorted
				foreach (Mobile m in list)
				{
					if (m.Deleted || m.Blessed)
						continue;

					// Let's not target ourselves...
					if (m == m_Mobile)
						continue;

					// Dead targets are invalid.
					if (!m.Alive || m.IsDeadBondedPet)
						continue;

					// Staff members cannot be targeted.
					if (m.AccessLevel > AccessLevel.Player)
						continue;

					// Does it have to be a player?
					if (bPlayerOnly && !m.Player)
						continue;

					// Can't acquire a target we can't see.
					if (!m_Mobile.CanSee(m))
						continue;

					if (m_Mobile.Summoned && m_Mobile.SummonMaster != null)
					{
						// If this is a summon, it can't target its controller.
						if (m == m_Mobile.SummonMaster)
							continue;

						// It also must abide by harmful spell rules.
						if (!Server.Spells.SpellHelper.ValidIndirectTarget(m_Mobile.SummonMaster, m))
							continue;

						// Animated creatures cannot attack players directly.
						if (m is PlayerMobile && m_Mobile.IsAnimatedDead)
							continue;
					}

					// If we only want faction friends, make sure it's one.
					if (bFacFriend && !m_Mobile.IsFriend(m))
						continue;

					// Same goes for faction enemies.
					if (bFacFoe && !m_Mobile.IsEnemy(m, BaseCreature.RelationshipFilter.None))
						continue;

					// process conditional-attack AI to see if they should attack
					if ((acqType & FightMode.All) == 0)
					{
						// Only acquire this mobile if it attacked us.
						//	All conditional-attack mobiles respond to aggression
						bool bValid = false;

						// Aggressors and Aggressed
						if (!bValid)
							bValid = Fought.ContainsKey(m);

						// even these conditional-attack mobiles can still have enemies like a faction, or team opposition
						if (!bValid)
							bValid = m_Mobile.IsEnemy(m, BaseCreature.RelationshipFilter.Faction);

						// Okay, if we're not pissed off yet, attack if Evil
						if ((acqType & FightMode.Evil) > 0 && !bValid)
						{
							if (m is BaseCreature && ((BaseCreature)m).Controlled && ((BaseCreature)m).ControlMaster != null)
								bValid = (((BaseCreature)m).ControlMaster.Karma < 0);
							else
								bValid = (m.Karma < 0);
						}

						// Okay, if we're not pissed off yet, attack if Criminal
						if ((acqType & FightMode.Criminal) > 0 && !bValid)
						{
							if (m is BaseCreature && ((BaseCreature)m).Controlled && ((BaseCreature)m).ControlMaster != null)
								bValid = (((BaseCreature)m).ControlMaster.Criminal);
							else
								bValid = (m.Criminal);
						}

						// Okay, if we're not pissed off yet, attack if Murderer
						if ((acqType & FightMode.Murderer) > 0 && !bValid)
						{
							if (m is PlayerMobile)
								bValid = (m as PlayerMobile).Kills >= 5;
						}

						// Alright, noting here to piss-us-off, keep looking
						if (!bValid)
							continue;
					}

					// If it's an enemy factioned mobile, make sure we can be harmful to it.
					if (bFacFoe && !bFacFriend && !m_Mobile.CanBeHarmful(m, false))
						continue;

					// faction friends are cool
					if (IOBSystem.IsFriend(m_Mobile, m) == true && m_Mobile.IsTeamOpposition(m) == false)
						continue;

					//Pix: this is the case where a non-IOBAligned mob is trying to target a IOBAligned mob AND the IOBAligned mob is an IOBFollower
					if (m is BaseCreature && IOBSystem.IsIOBAligned(m) && !IOBSystem.IsIOBAligned(m_Mobile) && (m as BaseCreature).IOBFollower == true)
						continue;

					// slow so we do it last (I believe)
					if (m_Mobile.InLOS(m) == false)
						continue;

					// gotcha ya little bastard!
					newFocusMob = m;

					// exit as soon as we have a target. 
					// this is both optimum and needed as we sorted our list in order of Prefeered Targets
					if (newFocusMob != null)
						break;
				}

				// the new focus mobile
				m_Mobile.FocusMob = newFocusMob;

				// we remember a few things about this mobile like last known location
				//	'memory' is used for smart mobiles that know how to reveal/detect hidden
				// we only remember for 10 seconds
				if (m_Mobile.FocusMob != null)
					Remember(m_Mobile.FocusMob, 10);
			}

			return (m_Mobile.FocusMob != null);
		}

		public virtual void Deactivate()
		{
			if (m_Mobile.PlayerRangeSensitive && m_Mobile.NavDestination == NavDestinations.None)
			{
				m_Timer.Stop();
			}
		}

		public virtual void Activate()
		{
			if (!m_Timer.Running)
			{
				m_Timer.Delay = TimeSpan.Zero;
				m_Timer.Start();

			}
		}

		/*
		 *  The mobile changed it speed, we must ajust the timer
		 */
		public virtual void OnCurrentSpeedChanged()
		{
			m_Timer.Stop();
			m_Timer.Delay = TimeSpan.FromSeconds(Utility.RandomDouble());
			m_Timer.Interval = TimeSpan.FromSeconds(Math.Max(0.0, m_Mobile.CurrentSpeed));
			m_Timer.Start();
		}

		/*
		 *  The Timer object
		 */
		private class AITimer : Timer
		{
			private BaseAI m_Owner;
			public AITimer(BaseAI owner)
				: base(TimeSpan.FromSeconds(Utility.RandomDouble()), TimeSpan.FromSeconds(Math.Max(0.0, owner.m_Mobile.CurrentSpeed)))
			{
				m_Owner = owner;

				//m_bDetectHidden = false;
				//m_NextDetectHidden = DateTime.Now;

				Priority = TimerPriority.FiftyMS;
			}

			protected override void OnTick()
			{

				if (m_Owner.m_Mobile == null || m_Owner.m_Mobile.Deleted)
				{
					Stop();
					return;
				}
				else if (m_Owner.m_Mobile.AIObject != m_Owner)
				{	// ai was changed, yet there was still a tick in the queue
					Stop();
					return;
				}
				else if (m_Owner.m_Mobile.Map == null || m_Owner.m_Mobile.Map == Map.Internal)
				{

					return;
				}
				else if ((m_Owner.m_Mobile.PlayerRangeSensitive && m_Owner.m_Mobile.NavDestination == NavDestinations.None))//have to check this in the timer....
				{
					Sector sect = m_Owner.m_Mobile.Map.GetSector(m_Owner.m_Mobile);
					if (!sect.Active)
					{
						m_Owner.Deactivate();
						return;
					}
				}

				m_Owner.m_Mobile.OnThink();

				if (m_Owner.m_Mobile.Deleted)
				{
					Stop();
					return;
				}
				else if (m_Owner.m_Mobile.Map == null || m_Owner.m_Mobile.Map == Map.Internal)
				{
					return;
				}

				if (m_Owner.m_Mobile.BardPacified)
				{
					m_Owner.DoBardPacified();
				}
				else if (m_Owner.m_Mobile.BardProvoked)
				{
					m_Owner.DoBardProvoked();
				}
				else
				{
					if (!m_Owner.m_Mobile.Controlled)
					{
						if (!m_Owner.Think())
						{
							Stop();
							return;
						}
					}
					else
					{
						if (!m_Owner.Obey())
						{
							Stop();
							return;
						}
					}
				}
			}
		}

		#region AIMemory
		// ObjectMemory is either a mobile or an item. We only support Mobile for now
		//	the memory management below is independant and doesn't care
		public class ObjectMemory
		{
			private Mobile m_mobile;
			private Point3D m_lastKnownLocation;
			private int m_seconds;
			public Point3D LastKnownLocation { get { return m_lastKnownLocation; } }
			private DateTime m_Expiration;
			public DateTime RefreshTime { get { return DateTime.Now + TimeSpan.FromSeconds(m_seconds); } }
			public DateTime Expiration { get { return m_Expiration; } set { m_Expiration = value; } }
			public ObjectMemory(object ox, int seconds)
			{	// may be null if ox is not a mobile
				m_mobile = ox as Mobile;
				m_seconds = seconds;

				if (m_mobile != null)
				{	// save the last known location for this mobile
					m_lastKnownLocation = m_mobile.Location;
				}

				// unless refreshed, this object will expire in X seconds
				m_Expiration = RefreshTime;
			}
		};
		private Hashtable m_MemoryCache = new Hashtable();

		public void TidyMemory()
		{
			// first clreanup the LOS cache
			ArrayList cleanup = new ArrayList();
			foreach (DictionaryEntry de in m_MemoryCache)
			{	// list expired elements
				if (de.Value == null) continue;
				if (DateTime.Now > (de.Value as ObjectMemory).Expiration)
					cleanup.Add(de.Key as object);
			}

			foreach (object ox in cleanup)
			{	// remove expired elements
				if (ox == null) continue;
				if (m_MemoryCache.Contains(ox))
					m_MemoryCache.Remove(ox);
			}
		}

		public void Remember(object ox, int seconds)
		{
			TidyMemory();
			if (ox == null) return;
			if (Recall(ox) != null)
			{	// we already know about this guy - just refresh
				Refresh(ox);
				//(World.FindMobile(1) as Mobile).Say(String.Format("Refreshing {0}", (ox as Mobile).Name ));
				return;
			}
			m_MemoryCache[ox] = new ObjectMemory(ox, seconds);
			//(World.FindMobile(1) as Mobile).Say(String.Format("Remembering {0}", (ox as Mobile).Name));
		}

		public void Forget(object ox)
		{
			TidyMemory();
			if (ox == null) return;
			if (m_MemoryCache.Contains(ox))
			{
				m_MemoryCache.Remove(ox);
				//(World.FindMobile(1) as Mobile).Say(String.Format("Forgetting {0}", (ox as Mobile).Name));
			}
		}

		public bool Recall(Mobile mx)
		{
			return Recall(mx as object) != null;
		}

		public ObjectMemory Recall(object ox)
		{
			TidyMemory();
			if (ox == null) return null;
			if (m_MemoryCache.Contains(ox))
				return m_MemoryCache[ox] as ObjectMemory;
			return null;
		}

		public void Refresh(object ox)
		{
			TidyMemory();
			if (ox == null) return;
			if (m_MemoryCache.Contains(ox))
				(m_MemoryCache[ox] as ObjectMemory).Expiration = (m_MemoryCache[ox] as ObjectMemory).RefreshTime;
		}

		#endregion AIMemory

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
