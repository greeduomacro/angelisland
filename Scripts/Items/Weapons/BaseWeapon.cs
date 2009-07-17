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
 * 	Technical Data and Computer Software clause at DFARS 252.227-7013.
 *
 *	Angel Island UO Shard	Version 1.0
 *			Release A
 *			March 25, 2004
 */

/* Scripts/Items/Weapons/BaseWeapon.cs
 * ChangeLog:
 *  11/9/08, Adam
 *      Replace old MaxHits and Hits with MaxHitPoints and HitPoints (RunUO 2.0 compatibility)
 *	07/23/08, weaver
 *		Automated IPooledEnumerable optimizations. 2 loops updated.
 * 5/2/08, Adam
 *		Update use of HideAttributes to be simpler.
 *	01/03/07, plasma
 *		Remove all duel challenge system code
 *	11/20/07, Adam
 *		Change damage bonus based on strength to take into account the new mobile.STRBonusCap.
 *		This new STRBonusCap allows playerMobiles to have super STR while 'capping' the STR bonus whereby preventing one-hit killing.
 *  6/1/07, Adam
 *      Add check for new item.HideAttributes bool for suppressing display attributes
 *	03/23/07, Pix
 *		Addressed the 'greyed out' on singleclick thing with oldschool labeled weapson.
 *		Re-added new type display of attributes for named weapons.
 *	03/19/07, Pix
 *		Modified single click to display poison and charges in an old-school manner
 *	01/03/07, Pix
 *		Changed CoreAI.RangedCorrosionModifier's effect to be extra chances to reduce poison corrosion on bows
 *	01/03/07, Pix
 *		Changed ranged corrosion message.
 *		Now only give ranged corrosion message 30% of the time (anti-spam!)
 *	01/02/07, Pix
 *		Added Corrosion to ranged weapons, with switches in CoreAI.
 *		Added logic for sealed bows to not corrode.
 *	01/21/06, Pix
 *		Removed test condition for new version of concussion.
 *	12/22/05, Pix
 *		Compilable first version of concussion for test.
 *	12/21/05, Adam
 *		Condition the Concussion changes on a temp variable 'CoreAI.TempInt'
 *		to allow us to merge into Prod without risk (will be turned on TC only)
 *	12/21/05, Pix
 *		First version of concussion for test.
 *	12/19/05, Pix
 *		Reverted concussion change for further review/testing.
 *	12/17/05, Pix
 *		Changed Concussion's effect to just halve mana.
 *  12/09/05, Kit
 *		Added check to OnHit() to check if creature has a weapon immunity and if so override damage
 *		cleanedup/removed some AOS code not used/needed.
 *	11/10/05, erlein
 *		Removed PlayerConstructed property and added deserialization to pack out old data.
 *  11/09/05, Kit
 *		Added check to GetPoisonBasedOnSkillAndPoison function to always return -1 level poison
 *		if poison base skill is 0. Was returning full poison strenght 1% of time. 
 *  11/07/05, Kit
 *		Added check to allow monsters to deal damage based on weapon vs creature min/max damage.
 *		Allowed creatures to use concussion/crushing/para blow.
 *	10/16/05, Pix
 *		Changed how corrosion works with the poisoning skill.
 *		Added GetPoisonBasedOnSkillAndPoison function.
 *	10/2/05, erlein
 *		Added OnHit() code to handle clothing wear.
 *	7/26/05, erlein
 *		Automated removal of AoS resistance related function calls. 11 lines removed.
 *	7/26/05, Adam
 *		Massive AOS cleanout
 *	7/23/05, Adam
 *		Remove all Necromancy, and Chivalry nonsense
 *	3/7/05, mith
 *		CorrodeWeapon(): Added conditional so that if weapon is Ranged (bow/xbow) it doesn't corrode.
 *	1/12/05, mith
 *		OnMiss(), removed Ranged weapon corrosion.
 *	11/28/04, Adam
 *		Move the OnEquip() logic for bows to BaseRanged where it belongs.
 *	11/28/04, Adam
 *		1. Add TRY to OnEquip
 *		2. if (this.Type == WeaponType.Ranged && from is PlayerMobile)
 *  7/10/04, Old Salty
 * 		Changed ScaleDamageOld so that Lumberjacking gives 20% rather than 30% bonus at GM
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 *	5/28/04 smerX
 *		Modified Hit Chance
 *	5/25/04, mith
 *		Modified GetBaseDamage() to take into account the fact that
 *		all monsters use this routine to calculate how much damage they deal as well.
 * 5/12/04, smerX
 *	Changed Para Blow time from 2 seconds to 3 seconds
 * 4/25/04, smerX
 *	Added HasAbilityReady flag
 *	Added special ability functionality in OnHit
 * 4/20/04, pixie
 *  Added poison corrosion ...
 * 4/19/04, mith
 *	If player attempts to equip a bow with a PoisonCloth with > 0 charges on it,
 *		any spells they are casting or are waiting to target will be cancelled.
 * 3/25/04 changes by mith
 *	modified CheckSkill call to use max value of 100.0 instead of 120.0.
 */
using System;
using System.Text;
using System.Collections;
using Server.Network;
using Server.Targeting;
using Server.Mobiles;
using Server.Spells;
using Server.Scripts.Commands;

namespace Server.Items
{
	public abstract class BaseWeapon : Item, IWeapon
	{
		/* Weapon internals work differently now (Mar 13 2003)
		 *
		 * The attributes defined below default to -1.
		 * If the value is -1, the corresponding virtual 'Aos/Old' property is used.
		 * If not, the attribute value itself is used. Here's the list:
		 *  - MinDamage
		 *  - MaxDamage
		 *  - Speed
		 *  - HitSound
		 *  - MissSound
		 *  - StrRequirement, DexRequirement, IntRequirement
		 *  - WeaponType
		 *  - WeaponAnimation
		 *  - MaxRange
		 */

		// Instance values. These values must are unique to each weapon.
		private WeaponDamageLevel m_DamageLevel;
		private WeaponAccuracyLevel m_AccuracyLevel;
		private WeaponDurabilityLevel m_DurabilityLevel;
		private WeaponQuality m_Quality;
		private Mobile m_Crafter;
		private Poison m_Poison;
		private int m_PoisonCharges;
		private bool m_Identified;
		private int m_Hits;
		private int m_MaxHits;
		private SlayerName m_Slayer;
		private SkillMod m_SkillMod, m_MageMod;
		private CraftResource m_Resource;

		//private bool m_Cursed; // Is this weapon cursed via Curse Weapon necromancer spell? Temporary; not serialized.
		//private bool m_Consecrated; // Is this weapon blessed via Consecrate Weapon paladin ability? Temporary; not serialized.

		//private AosAttributes m_AosAttributes;
		//private AosWeaponAttributes m_AosWeaponAttributes;

		// Overridable values. These values are provided to override the defaults which get defined in the individual weapon scripts.
		private int m_StrReq, m_DexReq, m_IntReq;
		private int m_MinDamage, m_MaxDamage;
		private int m_HitSound, m_MissSound;
		private int m_Speed;
		private int m_MaxRange;
		private SkillName m_Skill;
		private WeaponType m_Type;
		private WeaponAnimation m_Animation;
		// private int m_DieRolls, m_DieMax, m_AddConstant;

		public virtual WeaponAbility PrimaryAbility{ get{ return null; } }
		public virtual WeaponAbility SecondaryAbility{ get{ return null; } }

		public virtual int DefMaxRange{ get{ return 1; } }
		public virtual int DefHitSound{ get{ return 0; } }
		public virtual int DefMissSound{ get{ return 0; } }
		public virtual SkillName DefSkill{ get{ return SkillName.Swords; } }
		public virtual WeaponType DefType{ get{ return WeaponType.Slashing; } }
		public virtual WeaponAnimation DefAnimation{ get{ return WeaponAnimation.Slash1H; } }

		public virtual int AosStrengthReq{ get{ return 0; } }
		public virtual int AosDexterityReq{ get{ return 0; } }
		public virtual int AosIntelligenceReq{ get{ return 0; } }
		public virtual int AosMinDamage{ get{ return 0; } }
		public virtual int AosMaxDamage{ get{ return 0; } }
		public virtual int AosSpeed{ get{ return 0; } }
		public virtual int AosMaxRange{ get{ return DefMaxRange; } }
		public virtual int AosHitSound{ get{ return DefHitSound; } }
		public virtual int AosMissSound{ get{ return DefMissSound; } }
		public virtual SkillName AosSkill{ get{ return DefSkill; } }
		public virtual WeaponType AosType{ get{ return DefType; } }
		public virtual WeaponAnimation AosAnimation{ get{ return DefAnimation; } }

		public virtual int OldStrengthReq{ get{ return 0; } }
		public virtual int OldDexterityReq{ get{ return 0; } }
		public virtual int OldIntelligenceReq{ get{ return 0; } }
		public virtual int OldMinDamage{ get{ return 0; } }
		public virtual int OldMaxDamage{ get{ return 0; } }
		public virtual int OldSpeed{ get{ return 0; } }
		public virtual int OldMaxRange{ get{ return DefMaxRange; } }
		public virtual int OldHitSound{ get{ return DefHitSound; } }
		public virtual int OldMissSound{ get{ return DefMissSound; } }
		public virtual SkillName OldSkill{ get{ return DefSkill; } }
		public virtual WeaponType OldType{ get{ return DefType; } }
		public virtual WeaponAnimation OldAnimation{ get{ return DefAnimation; } }

		public virtual int OldDieRolls{ get{ return 0; } }
		public virtual int OldDieMax{ get{ return 0; } }
		public virtual int OldAddConstant{ get{ return 0; } }

		public virtual int InitMinHits{ get{ return 0; } }
		public virtual int InitMaxHits{ get{ return 0; } }


		/*
		[CommandProperty( AccessLevel.GameMaster )]
		public AosAttributes Attributes
		{
			get{ return m_AosAttributes; }
			set{}
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public AosWeaponAttributes WeaponAttributes
		{
			get{ return m_AosWeaponAttributes; }
			set{}
		}
		*/

		/*
		[CommandProperty( AccessLevel.GameMaster )]
		public bool Cursed
		{
			get{ return m_Cursed; }
			set{ m_Cursed = value; }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public bool Consecrated
		{
			get{ return m_Consecrated; }
			set{ m_Consecrated = value; }
		}
		*/

		[CommandProperty( AccessLevel.GameMaster )]
		public bool Identified
		{
			get{ return m_Identified; }
			set{ m_Identified = value; InvalidateProperties(); }
		}

        
        [CommandProperty( AccessLevel.GameMaster )]
        public int HitPoints
        {   
            get { return m_Hits; }
            set
            {
                if (m_Hits == value)
                    return;

                m_Hits = value;
                InvalidateProperties();

                if (m_Hits <= (m_MaxHits / 10))
                {
                    if (Parent is Mobile)
                        ((Mobile)Parent).LocalOverheadMessage(MessageType.Regular, 0x3B2, 1061121); // Your equipment is severely damaged.
                }
            }
        }

        [CommandProperty( AccessLevel.GameMaster )]
        public int MaxHitPoints
        {   
            get { return m_MaxHits; }
            set { m_MaxHits = value; InvalidateProperties(); }
        }

		[CommandProperty( AccessLevel.GameMaster )]
		public int PoisonCharges
		{
			get{ return m_PoisonCharges; }
			set{ m_PoisonCharges = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public Poison Poison
		{
			get{ return m_Poison; }
			set{ m_Poison = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public WeaponQuality Quality
		{
			get{ return m_Quality; }
			set{ UnscaleDurability(); m_Quality = value; ScaleDurability(); InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public Mobile Crafter
		{
			get{ return m_Crafter; }
			set{ m_Crafter = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public SlayerName Slayer
		{
			get{ return m_Slayer; }
			set{ m_Slayer = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public CraftResource Resource
		{
			get{ return m_Resource; }
			set{ UnscaleDurability(); m_Resource = value; Hue = CraftResources.GetHue( m_Resource ); InvalidateProperties(); ScaleDurability(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public WeaponDamageLevel DamageLevel
		{
			get{ return m_DamageLevel; }
			set{ m_DamageLevel = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public WeaponDurabilityLevel DurabilityLevel
		{
			get{ return m_DurabilityLevel; }
			set{ UnscaleDurability(); m_DurabilityLevel = value; InvalidateProperties(); ScaleDurability(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int MaxRange
		{
			get{ return ( m_MaxRange == -1 ? Core.AOS ? AosMaxRange : OldMaxRange : m_MaxRange ); }
			set{ m_MaxRange = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public WeaponAnimation Animation
		{
			get{ return ( m_Animation == (WeaponAnimation)(-1) ? Core.AOS ? AosAnimation : OldAnimation : m_Animation ); }
			set{ m_Animation = value; }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public WeaponType Type
		{
			get{ return ( m_Type == (WeaponType)(-1) ? Core.AOS ? AosType : OldType : m_Type ); }
			set{ m_Type = value; }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public SkillName Skill
		{
			get{ return ( m_Skill == (SkillName)(-1) ? Core.AOS ? AosSkill : OldSkill : m_Skill ); }
			set{ m_Skill = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int HitSound
		{
			get{ return ( m_HitSound == -1 ? Core.AOS ? AosHitSound : OldHitSound : m_HitSound ); }
			set{ m_HitSound = value; }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int MissSound
		{
			get{ return ( m_MissSound == -1 ? Core.AOS ? AosMissSound : OldMissSound : m_MissSound ); }
			set{ m_MissSound = value; }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int MinDamage
		{
			get{ return ( m_MinDamage == -1 ? Core.AOS ? AosMinDamage : OldMinDamage : m_MinDamage ); }
			set{ m_MinDamage = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int MaxDamage
		{
			get{ return ( m_MaxDamage == -1 ? Core.AOS ? AosMaxDamage : OldMaxDamage : m_MaxDamage ); }
			set{ m_MaxDamage = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int Speed
		{
			get{ return ( m_Speed == -1 ? Core.AOS ? AosSpeed : OldSpeed : m_Speed ); }
			set{ m_Speed = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int StrRequirement
		{
			get{ return ( m_StrReq == -1 ? Core.AOS ? AosStrengthReq : OldStrengthReq : m_StrReq ); }
			set{ m_StrReq = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int DexRequirement
		{
			get{ return ( m_DexReq == -1 ? Core.AOS ? AosDexterityReq : OldDexterityReq : m_DexReq ); }
			set{ m_DexReq = value; }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int IntRequirement
		{
			get{ return ( m_IntReq == -1 ? Core.AOS ? AosIntelligenceReq : OldIntelligenceReq : m_IntReq ); }
			set{ m_IntReq = value; }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public WeaponAccuracyLevel AccuracyLevel
		{
			get
			{
				return m_AccuracyLevel;
			}
			set
			{
				if ( m_AccuracyLevel != value )
				{
					m_AccuracyLevel = value;

					if ( UseSkillMod )
					{
						if ( m_AccuracyLevel == WeaponAccuracyLevel.Regular )
						{
							if ( m_SkillMod != null )
								m_SkillMod.Remove();

							m_SkillMod = null;
						}
						else if ( m_SkillMod == null && Parent is Mobile )
						{
							m_SkillMod = new DefaultSkillMod( SkillName.Tactics, true, (int)m_AccuracyLevel * 5 );
							((Mobile)Parent).AddSkillMod( m_SkillMod );
						}
						else if ( m_SkillMod != null )
						{
							m_SkillMod.Value = (int)m_AccuracyLevel * 5;
						}
					}

					InvalidateProperties();
				}
			}
		}

		public void UnscaleDurability()
		{
			int scale = 100 + GetDurabilityBonus();

			m_Hits = (m_Hits * 100) / scale;
			m_MaxHits = (m_MaxHits * 100) / scale;
			InvalidateProperties();
		}

		public void ScaleDurability()
		{
			int scale = 100 + GetDurabilityBonus();

			m_Hits = (m_Hits * scale) / 100;
			m_MaxHits = (m_MaxHits * scale) / 100;
			InvalidateProperties();
		}

		public int GetDurabilityBonus()
		{
			int bonus = 0;

			if ( m_Quality == WeaponQuality.Exceptional )
				bonus += 20;

			switch ( m_DurabilityLevel )
			{
				case WeaponDurabilityLevel.Durable: bonus += 20; break;
				case WeaponDurabilityLevel.Substantial: bonus += 50; break;
				case WeaponDurabilityLevel.Massive: bonus += 70; break;
				case WeaponDurabilityLevel.Fortified: bonus += 100; break;
				case WeaponDurabilityLevel.Indestructible: bonus += 120; break;
			}

			/*
			if ( Core.AOS )
			{
				bonus += m_AosWeaponAttributes.DurabilityBonus;

				CraftResourceInfo resInfo = CraftResources.GetInfo( m_Resource );
				CraftAttributeInfo attrInfo = null;

				if ( resInfo != null )
					attrInfo = resInfo.AttributeInfo;

				if ( attrInfo != null )
					bonus += attrInfo.WeaponDurability;
			}
			*/

			return bonus;
		}

		public int GetLowerStatReq()
		{
			if ( !Core.AOS )
				return 0;
return 0;
/*
			int v = m_AosWeaponAttributes.LowerStatReq;

			CraftResourceInfo info = CraftResources.GetInfo( m_Resource );

			if ( info != null )
			{
				CraftAttributeInfo attrInfo = info.AttributeInfo;

				if ( attrInfo != null )
					v += attrInfo.WeaponLowerRequirements;
			}

			if ( v > 100 )
				v = 100;

			return v;
*/
		}

		public static void BlockEquip( Mobile m, TimeSpan duration )
		{
			if ( m.BeginAction( typeof( BaseWeapon ) ) )
				new ResetEquipTimer( m, duration ).Start();
		}

		private class ResetEquipTimer : Timer
		{
			private Mobile m_Mobile;

			public ResetEquipTimer( Mobile m, TimeSpan duration ) : base( duration )
			{
				m_Mobile = m;
			}

			protected override void OnTick()
			{
				m_Mobile.EndAction( typeof( BaseWeapon ) );
			}
		}

		public override bool CheckConflictingLayer( Mobile m, Item item, Layer layer )
		{
			if ( base.CheckConflictingLayer( m, item, layer ) )
				return true;

			if ( this.Layer == Layer.TwoHanded && layer == Layer.OneHanded )
				return true;
			else if ( this.Layer == Layer.OneHanded && layer == Layer.TwoHanded && !(item is BaseShield) && !(item is BaseEquipableLight) )
				return true;

			return false;
		}

		public override bool CanEquip( Mobile from )
		{
			if ( from.Dex < DexRequirement )
			{
				from.SendMessage( "You are not nimble enough to equip that." );
				return false;
			}
			else if ( from.Str < AOS.Scale( StrRequirement, 100 - GetLowerStatReq() ) )
			{
				from.SendLocalizedMessage( 500213 ); // You are not strong enough to equip that.
				return false;
			}
			else if ( from.Int < IntRequirement )
			{
				from.SendMessage( "You are not smart enough to equip that." );
				return false;
			}
			else if ( !from.CanBeginAction( typeof( BaseWeapon ) ) )
			{
				return false;
			}
			else
			{
				return base.CanEquip( from );
			}
		}

		public virtual bool UseSkillMod{ get{ return !Core.AOS; } }

		public override bool OnEquip( Mobile from )
		{

			try
			{
				/*
				int strBonus = m_AosAttributes.BonusStr;
				int dexBonus = m_AosAttributes.BonusDex;
				int intBonus = m_AosAttributes.BonusInt;

				if ((strBonus != 0 || dexBonus != 0 || intBonus != 0))
				{
					Mobile m = from;

					string modName = this.Serial.ToString();

					if (strBonus != 0)
						m.AddStatMod(new StatMod(StatType.Str, modName + "Str", strBonus, TimeSpan.Zero));

					if (dexBonus != 0)
						m.AddStatMod(new StatMod(StatType.Dex, modName + "Dex", dexBonus, TimeSpan.Zero));

					if (intBonus != 0)
						m.AddStatMod(new StatMod(StatType.Int, modName + "Int", intBonus, TimeSpan.Zero));
				}
				*/

				from.NextCombatTime = DateTime.Now + GetDelay(from);

				if (UseSkillMod && m_AccuracyLevel != WeaponAccuracyLevel.Regular)
				{
					if (m_SkillMod != null)
						m_SkillMod.Remove();

					m_SkillMod = new DefaultSkillMod(SkillName.Tactics, true, (int)m_AccuracyLevel * 5);
					from.AddSkillMod(m_SkillMod);
				}

				/*
				if (Core.AOS && m_AosWeaponAttributes.MageWeapon != 0)
				{
					if (m_MageMod != null)
						m_MageMod.Remove();

					m_MageMod = new DefaultSkillMod(SkillName.Magery, true, -m_AosWeaponAttributes.MageWeapon);
					from.AddSkillMod(m_MageMod);
				}
				*/
			}
			catch (Exception ex)
			{
				LogHelper.LogException(ex);
				if (from is PlayerMobile)
				{
					PlayerMobile pm = (PlayerMobile)from;
					Console.WriteLine("Exception" + ex + from.Name);
				}
				else
					Console.WriteLine("Exception" + ex);
			}

			return true;
		}

		public override void OnAdded( object parent )
		{
			base.OnAdded( parent );

			if ( parent is Mobile )
			{
				((Mobile)parent).CheckStatTimers();
				((Mobile)parent).Delta( MobileDelta.WeaponDamage );
			}
		}

		public override void OnRemoved( object parent )
		{
			if ( parent is Mobile )
			{
				Mobile m = (Mobile)parent;
				BaseWeapon weapon = m.Weapon as BaseWeapon;

				string modName = this.Serial.ToString();

				m.RemoveStatMod( modName + "Str" );
				m.RemoveStatMod( modName + "Dex" );
				m.RemoveStatMod( modName + "Int" );

				if ( weapon != null )
					m.NextCombatTime = DateTime.Now + weapon.GetDelay( m );

				if ( UseSkillMod && m_SkillMod != null )
				{
					m_SkillMod.Remove();
					m_SkillMod = null;
				}

				if ( m_MageMod != null )
				{
					m_MageMod.Remove();
					m_MageMod = null;
				}

				m.CheckStatTimers();

				m.Delta( MobileDelta.WeaponDamage );
			}
		}

		public virtual SkillName GetUsedSkill( Mobile m, bool checkSkillAttrs )
		{
			SkillName sk;

			/*
			if ( checkSkillAttrs && m_AosWeaponAttributes.UseBestSkill != 0 )
			{
				double swrd = m.Skills[SkillName.Swords].Value;
				double fenc = m.Skills[SkillName.Fencing].Value;
				double arch = m.Skills[SkillName.Archery].Value;
				double mcng = m.Skills[SkillName.Macing].Value;
				double val;

				sk = SkillName.Swords;
				val = swrd;

				if ( fenc > val ){ sk = SkillName.Fencing; val = fenc; }
				if ( arch > val ){ sk = SkillName.Archery; val = arch; }
				if ( mcng > val ){ sk = SkillName.Macing; val = mcng; }
			}
			else if ( m_AosWeaponAttributes.MageWeapon != 0 )
			{
				sk = SkillName.Magery;
			}
			else*/
			{
				sk = Skill;

				if ( sk != SkillName.Wrestling && !m.Player && !m.Body.IsHuman && m.Skills[SkillName.Wrestling].Value > m.Skills[sk].Value )
					sk = SkillName.Wrestling;
			}

			return sk;
		}

		public virtual double GetAttackSkillValue( Mobile attacker, Mobile defender )
		{
			return attacker.Skills[GetUsedSkill( attacker, true )].Value;
		}

		public virtual double GetDefendSkillValue( Mobile attacker, Mobile defender )
		{
			return defender.Skills[GetUsedSkill( defender, false )].Value;
		}

		public virtual bool CheckHit( Mobile attacker, Mobile defender )
		{
			BaseWeapon atkWeapon = attacker.Weapon as BaseWeapon;
			BaseWeapon defWeapon = defender.Weapon as BaseWeapon;

			Skill atkSkill = attacker.Skills[atkWeapon.Skill];
			Skill defSkill = defender.Skills[defWeapon.Skill];

			double atkValue = atkWeapon.GetAttackSkillValue( attacker, defender );
			double defValue = defWeapon.GetDefendSkillValue( attacker, defender );

			//attacker.CheckSkill( atkSkill.SkillName, defValue - 20.0, 120.0 );
			//defender.CheckSkill( defSkill.SkillName, atkValue - 20.0, 120.0 );

			double ourValue, theirValue;

			int bonus = GetHitChanceBonus();

			if ( Core.AOS )
			{
				if ( atkValue <= -20.0 )
					atkValue = -19.9;

				if ( defValue <= -20.0 )
					defValue = -19.9;

				bonus += AosAttributes.GetValue( attacker, AosAttribute.AttackChance );

				//if ( Spells.Chivalry.DivineFurySpell.UnderEffect( attacker ) )
					//bonus += 10; // attacker gets 10% bonus when they're under divine fury

				ourValue = (atkValue + 20.0) * (100 + bonus);

				bonus = AosAttributes.GetValue( defender, AosAttribute.DefendChance );

				//if ( Spells.Chivalry.DivineFurySpell.UnderEffect( defender ) )
					//bonus -= 20; // defender loses 20% bonus when they're under divine fury

				double discordanceScalar = 0.0;

				if ( SkillHandlers.Discordance.GetScalar( attacker, ref discordanceScalar ) )
					bonus += (int)(discordanceScalar * 100);

				theirValue = (defValue + 20.0) * (100 + bonus);

				bonus = 0;
			}
			else
			{
				if ( atkValue <= -50.0 )
					atkValue = -49.9;

				if ( defValue <= -50.0 )
					defValue = -49.9;

				ourValue = (atkValue + 50.0);
				theirValue = (defValue + 50.0);
			}

			double chance = ourValue / (theirValue * 1.8);

			chance *= 1.0 + ((double)bonus / 100);

			if ( Core.AOS && chance < 0.02 )
				chance = 0.02;

			WeaponAbility ability = WeaponAbility.GetCurrentAbility( attacker );

			if ( ability != null )
				chance *= ability.AccuracyScalar;

			return attacker.CheckSkill( atkSkill.SkillName, chance );

			//return ( chance >= Utility.RandomDouble() );
		}

		public virtual TimeSpan GetDelay( Mobile m )
		{
			int speed = this.Speed;

			if ( speed == 0 )
				return TimeSpan.FromHours( 1.0 );

			double delayInSeconds;

			if ( Core.AOS )
			{
				int v = (m.Stam + 100) * speed;

				int bonus = AosAttributes.GetValue( m, AosAttribute.WeaponSpeed );

				//if ( Spells.Chivalry.DivineFurySpell.UnderEffect( m ) )
					//bonus += 10;

				double discordanceScalar = 0.0;

				if ( SkillHandlers.Discordance.GetScalar( m, ref discordanceScalar ) )
					bonus += (int)(discordanceScalar * 100);

				v += AOS.Scale( v, bonus );

				if ( v <= 0 )
					v = 1;

				delayInSeconds = Math.Floor( 40000.0 / v ) * 0.5;
			}
			else
			{
				int v = (m.Stam + 100) * speed;

				if ( v <= 0 )
					v = 1;

				delayInSeconds = 15000.0 / v;
			}

			return TimeSpan.FromSeconds( delayInSeconds );
		}

		public virtual TimeSpan OnSwing( Mobile attacker, Mobile defender )
		{
			bool canSwing = true;

			if ( Core.AOS )
			{
				canSwing = ( !attacker.Paralyzed && !attacker.Frozen );

				if ( canSwing )
				{
					Spell sp = attacker.Spell as Spell;

					canSwing = ( sp == null || !sp.IsCasting || !sp.BlocksMovement );
				}
			}

			if ( canSwing && attacker.HarmfulCheck( defender ) )
			{
				attacker.DisruptiveAction();

				if ( attacker.NetState != null )
					attacker.Send( new Swing( 0, attacker, defender ) );

				if ( attacker is BaseCreature )
				{
					BaseCreature bc = (BaseCreature)attacker;
					WeaponAbility ab = bc.GetWeaponAbility();

					if ( ab != null )
					{
						if ( bc.WeaponAbilityChance > Utility.RandomDouble() )
							WeaponAbility.SetCurrentAbility( bc, ab );
						else
							WeaponAbility.ClearCurrentAbility( bc );
					}
				}

				if ( CheckHit( attacker, defender ) )
					OnHit( attacker, defender );
				else
					OnMiss( attacker, defender );
			}

			return GetDelay( attacker );
		}

		public virtual int GetHitAttackSound( Mobile attacker, Mobile defender )
		{
			int sound = attacker.GetAttackSound();

			if ( sound == -1 )
				sound = HitSound;

			return sound;
		}

		public virtual int GetHitDefendSound( Mobile attacker, Mobile defender )
		{
			return defender.GetHurtSound();
		}

		public virtual int GetMissAttackSound( Mobile attacker, Mobile defender )
		{
			if ( attacker.GetAttackSound() == -1 )
				return MissSound;
			else
				return -1;
		}

		public virtual int GetMissDefendSound( Mobile attacker, Mobile defender )
		{
			return -1;
		}

		public virtual int AbsorbDamageAOS( Mobile attacker, Mobile defender, int damage )
		{
			double positionChance = Utility.RandomDouble();
			BaseArmor armor;

			if ( positionChance < 0.07 )
				armor = defender.NeckArmor as BaseArmor;
			else if ( positionChance < 0.14 )
				armor = defender.HandArmor as BaseArmor;
			else if ( positionChance < 0.28 )
				armor = defender.ArmsArmor as BaseArmor;
			else if ( positionChance < 0.43 )
				armor = defender.HeadArmor as BaseArmor;
			else if ( positionChance < 0.65 )
				armor = defender.LegsArmor as BaseArmor;
			else
				armor = defender.ChestArmor as BaseArmor;

			if ( armor != null )
				armor.OnHit( this, damage ); // call OnHit to lose durability

			if ( defender.Player || defender.Body.IsHuman )
			{
				BaseShield shield = defender.FindItemOnLayer( Layer.TwoHanded ) as BaseShield;

				bool blocked = false;

				if ( shield != null )
				{
					double chance = ( defender.Skills[SkillName.Parry].Value * 0.0030 );

					blocked = defender.CheckSkill( SkillName.Parry, chance );
				}
				else if ( !(defender.Weapon is Fists) && !(defender.Weapon is BaseRanged) )
				{
					double chance = ( defender.Skills[SkillName.Parry].Value * 0.0015 );

					blocked = ( chance > Utility.RandomDouble() ); // Only skillcheck if wielding a shield
				}

				if ( blocked )
				{
					defender.FixedEffect( 0x37B9, 10, 16 );
					damage = 0;

					if ( shield != null )
					{
						double halfArmor = shield.ArmorRating / 2.0;
						int absorbed = (int)(halfArmor + (halfArmor*Utility.RandomDouble()));

						if ( absorbed < 2 )
							absorbed = 2;

						if ( Type == WeaponType.Bashing )
							shield.HitPoints -= absorbed / 2;
						else
							shield.HitPoints -= Utility.Random( 2 );
					}
				}
			}

			return damage;
		}

		public virtual int AbsorbDamage( Mobile attacker, Mobile defender, int damage )
		{
			if ( Core.AOS )
				return AbsorbDamageAOS( attacker, defender, damage );

			double chance = Utility.RandomDouble();
			BaseArmor armor;

			// Array of clothing types affected by this hit
			Type[] tclothing;
			tclothing = new Type[3];

			if ( chance < 0.07 )
				armor = defender.NeckArmor as BaseArmor;
				// no clothing reached
			else if ( chance < 0.14 )
				armor = defender.HandArmor as BaseArmor;
				// no clothing reached
			else if ( chance < 0.28 )
			{
				armor = defender.ArmsArmor as BaseArmor;
				// hits anything with arms
				tclothing[0] = typeof( FancyShirt );
				tclothing[1] = typeof( BaseOuterTorso );
				tclothing[2] = typeof( JesterSuit );
			}
			else if ( chance < 0.43 )
			{
				armor = defender.HeadArmor as BaseArmor;
				// hits hats!!
				tclothing[0] = typeof( BaseHat );
			}
			else if ( chance < 0.65 )
			{
				armor = defender.LegsArmor as BaseArmor;
				// hits anything with potential leg area
				tclothing[0] = typeof( BaseOuterLegs );
				tclothing[1] = typeof( BasePants );
				tclothing[2] = typeof( BaseOuterTorso );
			}
			else
			{
				armor = defender.ChestArmor as BaseArmor;
				// square in the chest - inner and outter
				tclothing[0] = typeof( BaseMiddleTorso );
				tclothing[1] = typeof( BaseOuterTorso );
				tclothing[2] = typeof( BaseShirt );
			}

			// Loop through types of clothing we're dealing with
			// and cast out appropriate items from mobile

			for( int i = 0; i < 3; i ++ )
			{
				BaseClothing clothing;
				clothing = null;

				if( tclothing[i] == null )
					// No more clothing was affected by this hit
					break;

				// Check for the types identified as being accessible via hit
				// and cast for OnHit() access

				if( tclothing[i] == typeof(BaseOuterTorso) )
					clothing = defender.FindItemOnLayer( Layer.OuterTorso )
									as BaseClothing;

				else if( tclothing[i] == typeof(BaseMiddleTorso) )
					clothing = defender.FindItemOnLayer( Layer.MiddleTorso )
									as BaseClothing;

				else if( tclothing[i] == typeof(FancyShirt) ||
						 tclothing[i] == typeof(BaseShirt) )
					clothing = defender.FindItemOnLayer( Layer.Shirt )
									as BaseClothing;

				else if( tclothing[i] == typeof(BasePants) )
					clothing = defender.FindItemOnLayer( Layer.Pants )
									as BaseClothing;

				else if( tclothing[i] == typeof(BaseHat) )
					clothing = defender.FindItemOnLayer( Layer.Helm )
									as BaseClothing;
									
				// If clothing was hit, let it know

				if( clothing != null )
					clothing.OnHit(this);
			}

			if ( armor != null )
				damage = armor.OnHit( this, damage );

			BaseShield shield = defender.FindItemOnLayer( Layer.TwoHanded ) as BaseShield;
			if ( shield != null )
				damage = shield.OnHit( this, damage );

			int virtualArmor = defender.VirtualArmor + defender.VirtualArmorMod;

			if ( virtualArmor > 0 )
			{
				double scalar;

				if ( chance < 0.14 )
					scalar = 0.07;
				else if ( chance < 0.28 )
					scalar = 0.14;
				else if ( chance < 0.43 )
					scalar = 0.15;
				else if ( chance < 0.65 )
					scalar = 0.22;
				else
					scalar = 0.35;

				int from = (int)(virtualArmor * scalar) / 2;
				int to = (int)(virtualArmor * scalar);

				damage -= Utility.Random( from, (to - from) + 1 );
			}

			return damage;
		}

		public virtual int GetPackInstinctBonus( Mobile attacker, Mobile defender )
		{
			if ( attacker.Player || defender.Player )
				return 0;

			BaseCreature bc = attacker as BaseCreature;

			if ( bc == null || bc.PackInstinct == PackInstinct.None || (!bc.Controlled && !bc.Summoned) )
				return 0;

			Mobile master = bc.ControlMaster;

			if ( master == null )
				master = bc.SummonMaster;

			if ( master == null )
				return 0;

			int inPack = 1;

			IPooledEnumerable eable = defender.GetMobilesInRange( 1 );
			foreach ( Mobile m in eable)
			{
				if ( m != attacker && m is BaseCreature )
				{
					BaseCreature tc = (BaseCreature)m;

					if ( (tc.PackInstinct & bc.PackInstinct) == 0 || (!tc.Controlled && !tc.Summoned) )
						continue;

					Mobile theirMaster = tc.ControlMaster;

					if ( theirMaster == null )
						theirMaster = tc.SummonMaster;

					if ( master == theirMaster && tc.Combatant == defender )
						++inPack;
				}
			}
			eable.Free();

			if ( inPack >= 5 )
				return 100;
			else if ( inPack >= 4 )
				return 75;
			else if ( inPack >= 3 )
				return 50;
			else if ( inPack >= 2 )
				return 25;

			return 0;
		}

		private static bool m_InDoubleStrike;

		public static bool InDoubleStrike
		{
			get{ return m_InDoubleStrike; }
			set{ m_InDoubleStrike = value; }
		}

		public virtual void OnHit( Mobile attacker, Mobile defender )
		{
			PlaySwingAnimation( attacker );
			PlayHurtAnimation( defender );

			attacker.PlaySound( GetHitAttackSound( attacker, defender ) );
			defender.PlaySound( GetHitDefendSound( attacker, defender ) );

			int damage = ComputeDamage( attacker, defender );

			CheckSlayerResult cs = CheckSlayers( attacker, defender );

			if ( cs != CheckSlayerResult.None )
			{
				if ( cs == CheckSlayerResult.Slayer )
					defender.FixedEffect( 0x37B9, 10, 5 );

				damage *= 2;
			}
			
			int packInstinctBonus = GetPackInstinctBonus( attacker, defender );

			if ( packInstinctBonus != 0 )
				damage += AOS.Scale( damage, packInstinctBonus );
			
			if ( attacker is BaseCreature )
				((BaseCreature)attacker).AlterMeleeDamageTo( defender, ref damage );

			if ( defender is BaseCreature )
				((BaseCreature)defender).AlterMeleeDamageFrom( attacker, ref damage );

			WeaponAbility a = WeaponAbility.GetCurrentAbility( attacker );

			damage = AbsorbDamage( attacker, defender, damage );

			if ( damage < 1 )
				damage = 1;
			
			AddBlood( attacker, defender, damage );

			// adam: always returns phys=100;
			int phys, fire, cold, pois, nrgy;
			GetDamageTypes( attacker, out phys, out fire, out cold, out pois, out nrgy );


			//check if creature has some immunity to weapon being used, if so reduce damage according to immunity.
			if(defender is BaseCreature)
			{
				BaseWeapon atkWeapon = attacker.Weapon as BaseWeapon;
				((BaseCreature)defender).CheckWeaponImmunity(atkWeapon, damage, out damage);
			}

			int damageGiven = damage;

			AOS.ArmorIgnore = ( a is ArmorIgnore );

			damageGiven = AOS.Damage( defender, attacker, damage, phys, fire, cold, pois, nrgy );

			AOS.ArmorIgnore = false;

			if ( m_MaxHits > 0 && ((MaxRange <= 1 && (defender is Slime || defender is ToxicElemental)) || Utility.Random( 25 ) == 0) ) // Stratics says 50% chance, seems more like 4%..
			{
				if ( MaxRange <= 1 && (defender is Slime || defender is ToxicElemental) )
					attacker.LocalOverheadMessage( MessageType.Regular, 0x3B2, 500263 ); // *Acid blood scars your weapon!*
					{
						if ( m_Hits > 1 )
							--HitPoints;
						else
							Delete();
					}
			}

			if ( attacker is VampireBatFamiliar )
			{
				BaseCreature bc = (BaseCreature)attacker;
				Mobile caster = bc.ControlMaster;

				if ( caster == null )
					caster = bc.SummonMaster;

				if ( caster != null && caster.Map == bc.Map && caster.InRange( bc, 2 ) )
					caster.Hits += damage;
				else
					bc.Hits += damage;
			}

			if ( attacker is BaseCreature )
				((BaseCreature)attacker).OnGaveMeleeAttack( defender );

			if ( defender is BaseCreature )
				((BaseCreature)defender).OnGotMeleeAttack( attacker );

			if ( a != null )
				a.OnHit( attacker, defender, damage );

			TimeSpan AbilityDelay = TimeSpan.FromSeconds( 20.0 );

            Mobile atkr = attacker;

            if (atkr.HasAbilityReady && atkr.Mana >= 15 && atkr.NextAbilityTime <= DateTime.Now)
            {
                atkr.HasAbilityReady = false;
                Item weapon = atkr.FindItemOnLayer(Layer.TwoHanded);

                if (weapon is BaseBashing)
                {
                    if (atkr.Mana < 15)
                    {
                        atkr.LocalOverheadMessage(MessageType.Emote, 0x3B2, false, "Insufficient mana.");
                        atkr.HasAbilityReady = false;

                    }
                    else
                    {
                        double crushbonus = damage * 0.5;
                        int crush = (int)crushbonus;

                        atkr.Mana -= 15;
                        atkr.HasAbilityReady = false;
                        atkr.NextAbilityTime = DateTime.Now + AbilityDelay;
                        atkr.SendLocalizedMessage(1060090);	// You have delivered a crushing blow!

                        defender.SendLocalizedMessage(1060091); // You take extra damage from the crushing attack!
                        defender.PlaySound(0x1E1);
                        defender.FixedParticles(0, 1, 0, 9946, EffectLayer.Head);
                        defender.Damage(crush, atkr);		// brings dmg total up to 150%

                        Effects.SendMovingParticles(new Entity(Serial.Zero, new Point3D(defender.X, defender.Y, defender.Z + 50), defender.Map), new Entity(Serial.Zero, new Point3D(defender.X, defender.Y, defender.Z + 20), defender.Map), 0xFB4, 1, 0, false, false, 0, 3, 9501, 1, 0, EffectLayer.Head, 0x100);

                    }

                }
                else if (weapon is BasePoleArm || weapon is BaseAxe)
                {
                    if (atkr.Mana < 15)
                    {
                        atkr.LocalOverheadMessage(MessageType.Emote, 0x3B2, false, "Insufficient mana.");
                        atkr.HasAbilityReady = false;
                    }
                    else
                    {
                        atkr.Mana -= 15;
                        atkr.SendLocalizedMessage(1060165); // You have delivered a concussion!
                        atkr.HasAbilityReady = false;
                        atkr.NextAbilityTime = DateTime.Now + AbilityDelay;

                        defender.SendLocalizedMessage(1060166); // You feel disoriented!
                        int mana_test = Math.Max(defender.Mana / 2, defender.ManaMax / 2);
                        if (mana_test < defender.Mana)
                        {
                            defender.Mana = mana_test;
                        }
                        defender.PlaySound(0x213);
                        defender.FixedParticles(0x377A, 1, 32, 9949, 1153, 0, EffectLayer.Head);
                    }
                }
                else if (weapon is BaseSpear)
                {
                    if (atkr.Mana < 15)
                    {
                        atkr.LocalOverheadMessage(MessageType.Emote, 0x3B2, false, "Insufficient mana.");
                        atkr.HasAbilityReady = false;
                    }
                    else
                    {
                        defender.SendMessage("You receive a paralyzing blow!");
                        defender.Freeze(TimeSpan.FromSeconds(3.0));	// paras defender
                        defender.FixedEffect(0x376A, 9, 32);
                        defender.PlaySound(0x204);

                        atkr.HasAbilityReady = false;
                        atkr.NextAbilityTime = DateTime.Now + AbilityDelay;
                        atkr.SendMessage("You deliver a paralyzing blow!");
                        atkr.Mana -= 15;
                        atkr.HasAbilityReady = false;
                    }
                }
                else
                {
                    atkr.HasAbilityReady = false;
                }
            }

			CorrodeWeapon(attacker);
		}

		public virtual double GetAosDamage( Mobile attacker, int min, int random, double div )
		{
			double scale = 1.0;

			scale += attacker.Skills[SkillName.Inscribe].Value * 0.001;

			if ( attacker.Player )
			{
				scale += attacker.Int * 0.001;
				scale += AosAttributes.GetValue( attacker, AosAttribute.SpellDamage ) * 0.01;
			}

			int baseDamage = min + (int)(attacker.Skills[SkillName.EvalInt].Value / div);

			double damage = Utility.RandomMinMax( baseDamage, baseDamage + random );

			return damage * scale;
		}

		public virtual void DoMagicArrow( Mobile attacker, Mobile defender )
		{
			if ( !attacker.CanBeHarmful( defender, false ) )
				return;

			attacker.DoHarmful( defender );

			double damage = GetAosDamage( attacker, 3, 1, 10.0 );

			attacker.MovingParticles( defender, 0x36E4, 5, 0, false, true, 3006, 4006, 0 );
			attacker.PlaySound( 0x1E5 );

			SpellHelper.Damage( TimeSpan.FromSeconds( 1.0 ), defender, attacker, damage, 0, 100, 0, 0, 0 );
		}

		public virtual void DoHarm( Mobile attacker, Mobile defender )
		{
			if ( !attacker.CanBeHarmful( defender, false ) )
				return;

			attacker.DoHarmful( defender );

			double damage = GetAosDamage( attacker, 6, 3, 6.5 );

			if ( !defender.InRange( attacker, 2 ) )
				damage *= 0.25; // 1/4 damage at > 2 tile range
			else if ( !defender.InRange( attacker, 1 ) )
				damage *= 0.50; // 1/2 damage at 2 tile range

			defender.FixedParticles( 0x374A, 10, 30, 5013, 1153, 2, EffectLayer.Waist );
			defender.PlaySound( 0x0FC );

			SpellHelper.Damage( TimeSpan.Zero, defender, attacker, damage, 0, 0, 100, 0, 0 );
		}

		public virtual void DoFireball( Mobile attacker, Mobile defender )
		{
			if ( !attacker.CanBeHarmful( defender, false ) )
				return;

			attacker.DoHarmful( defender );

			double damage = GetAosDamage( attacker, 6, 3, 5.5 );

			attacker.MovingParticles( defender, 0x36D4, 7, 0, false, true, 9502, 4019, 0x160 );
			attacker.PlaySound( 0x15E );

			SpellHelper.Damage( TimeSpan.FromSeconds( 1.0 ), defender, attacker, damage, 0, 100, 0, 0, 0 );
		}

		public virtual void DoLightning( Mobile attacker, Mobile defender )
		{
			if ( !attacker.CanBeHarmful( defender, false ) )
				return;

			attacker.DoHarmful( defender );

			double damage = GetAosDamage( attacker, 6, 3, 5.0 );

			defender.BoltEffect( 0 );

			SpellHelper.Damage( TimeSpan.Zero, defender, attacker, damage, 0, 0, 0, 0, 100 );
		}

		public virtual void DoDispel( Mobile attacker, Mobile defender )
		{
			bool dispellable = false;

			if ( defender is BaseCreature )
				dispellable = ((BaseCreature)defender).Summoned && !((BaseCreature)defender).IsAnimatedDead;

			if ( !dispellable )
				return;

			if ( !attacker.CanBeHarmful( defender, false ) )
				return;

			attacker.DoHarmful( defender );

			Spells.Spell sp = new Spells.Sixth.DispelSpell( attacker, null );

			if ( sp.CheckResisted( defender ) )
			{
				defender.FixedEffect( 0x3779, 10, 20 );
			}
			else
			{
				Effects.SendLocationParticles( EffectItem.Create( defender.Location, defender.Map, EffectItem.DefaultDuration ), 0x3728, 8, 20, 5042 );
				Effects.PlaySound( defender, defender.Map, 0x201 );

				defender.Delete();
			}
		}

		public virtual void DoAreaAttack( Mobile from, int sound, int hue, int phys, int fire, int cold, int pois, int nrgy )
		{
			Map map = from.Map;

			if ( map == null )
				return;

			ArrayList list = new ArrayList();

			IPooledEnumerable eable = from.GetMobilesInRange( 10 );
			foreach ( Mobile m in eable)
			{
				if ( from != m && SpellHelper.ValidIndirectTarget( from, m ) && from.CanBeHarmful( m, false ) && from.InLOS( m ) )
					list.Add( m );
			}
			eable.Free();

			if ( list.Count == 0 )
				return;

			Effects.PlaySound( from.Location, map, sound );

			// TODO: What is the damage calculation?

			for ( int i = 0; i < list.Count; ++i )
			{
				Mobile m = (Mobile)list[i];

				double scalar = (11 - from.GetDistanceToSqrt( m )) / 10;

				if ( scalar > 1.0 )
					scalar = 1.0;
				else if ( scalar < 0.0 )
					continue;

				from.DoHarmful( m, true );
				m.FixedEffect( 0x3779, 1, 15, hue, 0 );
				AOS.Damage( m, from, (int)(GetBaseDamage( from ) * scalar), phys, fire, cold, pois, nrgy );
			}
		}

		public virtual CheckSlayerResult CheckSlayers( Mobile attacker, Mobile defender )
		{
			BaseWeapon atkWeapon = attacker.Weapon as BaseWeapon;
			SlayerEntry atkSlayer = SlayerGroup.GetEntryByName( atkWeapon.Slayer );

			if ( atkSlayer != null && atkSlayer.Slays( defender ) )
				return CheckSlayerResult.Slayer;

			BaseWeapon defWeapon = defender.Weapon as BaseWeapon;
			SlayerEntry defSlayer = SlayerGroup.GetEntryByName( defWeapon.Slayer );

			if ( defSlayer != null && defSlayer.Group.Opposition.Super.Slays( attacker ) )
				return CheckSlayerResult.Opposition;

			return CheckSlayerResult.None;
		}

		public virtual void AddBlood( Mobile attacker, Mobile defender, int damage )
		{
			if ( damage <= 2 )
				return;

			Direction d = defender.GetDirectionTo( attacker );

			int maxCount = damage / 15;

			if ( maxCount < 1 )
				maxCount = 1;
			else if ( maxCount > 4 )
				maxCount = 4;

			for( int i = 0; i < Utility.Random( 1, maxCount ); ++i )
			{
				int x = defender.X;
				int y = defender.Y;

				switch( d )
				{
					case Direction.North:
						x += Utility.Random( -1, 3 );
						y += Utility.Random( 2 );
						break;
					case Direction.East:
						y += Utility.Random( -1, 3 );
						x += Utility.Random( -1, 2 );
						break;
					case Direction.West:
						y += Utility.Random( -1, 3 );
						x += Utility.Random( 2 );
						break;
					case Direction.South:
						x += Utility.Random( -1, 3 );
						y += Utility.Random( -1, 2 );
						break;
					case Direction.Up:
						x += Utility.Random( 2 );
						y += Utility.Random( 2 );
						break;
					case Direction.Down:
						x += Utility.Random( -1, 2 );
						y += Utility.Random( -1, 2 );
						break;
					case Direction.Left:
						x += Utility.Random( 2 );
						y += Utility.Random( -1, 2 );
						break;
					case Direction.Right:
						x += Utility.Random( -1, 2 );
						y += Utility.Random( 2 );
						break;
				}

				new Blood().MoveToWorld( new Point3D( x, y, defender.Z ), defender.Map );
			}
		}

		public virtual void GetDamageTypes( Mobile wielder, out int phys, out int fire, out int cold, out int pois, out int nrgy )
		{
			/*
			if ( wielder is BaseCreature )
			{
				BaseCreature bc = (BaseCreature)wielder;

				phys = bc.PhysicalDamage;
				fire = bc.FireDamage;
				cold = bc.ColdDamage;
				pois = bc.PoisonDamage;
				nrgy = bc.EnergyDamage;
			}
			else
			*/
			{
				/*
				CraftResourceInfo resInfo = CraftResources.GetInfo( m_Resource );

				if ( resInfo != null )
				{
					CraftAttributeInfo attrInfo = resInfo.AttributeInfo;

					if ( attrInfo != null )
					{
						fire = attrInfo.WeaponFireDamage;
						cold = attrInfo.WeaponColdDamage;
						pois = attrInfo.WeaponPoisonDamage;
						nrgy = attrInfo.WeaponEnergyDamage;
						phys = 100 - fire - cold - pois - nrgy;
						return;
					}
				}
				*/

				phys = 100;
				fire = 0;
				cold = 0;
				pois = 0;
				nrgy = 0;
			}
		}

		public virtual void OnMiss( Mobile attacker, Mobile defender )
		{
			PlaySwingAnimation( attacker );
			attacker.PlaySound( GetMissAttackSound( attacker, defender ) );
			defender.PlaySound( GetMissDefendSound( attacker, defender ) );

			WeaponAbility ability = WeaponAbility.GetCurrentAbility( attacker );

			if ( ability != null )
				ability.OnMiss( attacker, defender );

			//Only ranged weapons are corroded on misses too.
			if ( this.Type == WeaponType.Ranged )
			{
				CorrodeWeapon(attacker);
			}
		}

		public virtual void GetBaseDamageRange( Mobile attacker, out int min, out int max )
		{
			if ( attacker is BaseCreature )
			{
				BaseCreature c = (BaseCreature)attacker;

				if ( c.DamageMin >= 0 )
				{
					min = c.DamageMin;
					max = c.DamageMax;
					return;
				}

				if ( this is Fists && !attacker.Body.IsHuman )
				{
					min = attacker.Str / 28;
					max = attacker.Str / 28;
					return;
				}
			}

			min = MinDamage;
			max = MaxDamage;
		}

		public virtual double GetBaseDamage( Mobile attacker )
		{
			int min, max;
			double damage = 0;
			//only do this for mobs set to not use human weapon damage.
			if ( (attacker is BaseCreature) && ((BaseCreature)attacker).UsesHumanWeapons == false)
			{
				GetBaseDamageRange( attacker, out min, out max );
				damage = Utility.RandomMinMax( min, max );
			}
			else
			{
				for ( int i = 1; i <= OldDieRolls; i++ )
					damage += Utility.RandomMinMax( 1, OldDieMax );

				damage += OldAddConstant;
			}

			return damage;

			//	return Utility.RandomMinMax( min, max );
		}
		public virtual double GetBonus( double value, double scalar, double threshold, double offset )
		{
			double bonus = value * scalar;

			if ( value >= threshold )
				bonus += offset;

			return bonus / 100;
		}

		public virtual int GetHitChanceBonus()
		{
			if ( !Core.AOS )
				return 0;

			int bonus = 0;

			switch ( m_AccuracyLevel )
			{
				case WeaponAccuracyLevel.Accurate:		bonus += 02; break;
				case WeaponAccuracyLevel.Surpassingly:	bonus += 04; break;
				case WeaponAccuracyLevel.Eminently:		bonus += 06; break;
				case WeaponAccuracyLevel.Exceedingly:	bonus += 08; break;
				case WeaponAccuracyLevel.Supremely:		bonus += 10; break;
			}

			return bonus;
		}

		public virtual int GetDamageBonus()
		{
			int bonus = VirtualDamageBonus;

			switch ( m_Quality )
			{
				case WeaponQuality.Low:			bonus -= 20; break;
				case WeaponQuality.Exceptional:	bonus += 20; break;
			}

			switch ( m_DamageLevel )
			{
				case WeaponDamageLevel.Ruin:	bonus += 15; break;
				case WeaponDamageLevel.Might:	bonus += 20; break;
				case WeaponDamageLevel.Force:	bonus += 25; break;
				case WeaponDamageLevel.Power:	bonus += 30; break;
				case WeaponDamageLevel.Vanq:	bonus += 35; break;
			}

			return bonus;
		}

		public virtual void GetStatusDamage( Mobile from, out int min, out int max )
		{
			int baseMin, baseMax;

			GetBaseDamageRange( from, out baseMin, out baseMax );

			if ( Core.AOS )
			{
				min = (int)ScaleDamageAOS( from, baseMin, false, false );
				max = (int)ScaleDamageAOS( from, baseMax, false, false );
			}
			else
			{
				min = (int)ScaleDamageOld( from, baseMin, false, false );
				max = (int)ScaleDamageOld( from, baseMax, false, false );
			}

			if ( min < 1 )
				min = 1;

			if ( max < 1 )
				max = 1;
		}

		public virtual double ScaleDamageAOS( Mobile attacker, double damage, bool checkSkills, bool checkAbility )
		{
			if ( checkSkills )
			{
				attacker.CheckSkill( SkillName.Tactics, 0.0, 100.0 ); // Passively check tactics for gain
				attacker.CheckSkill( SkillName.Anatomy, 0.0, 100.0 ); // Passively check Anatomy for gain

				if ( Type == WeaponType.Axe )
					attacker.CheckSkill( SkillName.Lumberjacking, 0.0, 100.0 ); // Passively check Lumberjacking for gain
			}

			double strengthBonus = GetBonus( attacker.Str,										0.300, 100.0,  5.00 );
			double  anatomyBonus = GetBonus( attacker.Skills[SkillName.Anatomy].Value,			0.500, 100.0,  5.00 );
			double  tacticsBonus = GetBonus( attacker.Skills[SkillName.Tactics].Value,			0.625, 100.0,  6.25 );
			double   lumberBonus = GetBonus( attacker.Skills[SkillName.Lumberjacking].Value,	0.200, 100.0, 10.00 );

			if ( Type != WeaponType.Axe )
				lumberBonus = 0.0;

			double totalBonus = strengthBonus + anatomyBonus + tacticsBonus + lumberBonus + ((double)(GetDamageBonus() + AosAttributes.GetValue( attacker, AosAttribute.WeaponDamage )) / 100);

			//if ( TransformationSpell.UnderTransformation( attacker, typeof( HorrificBeastSpell ) ) )
				//totalBonus += 0.25;

			//if ( Spells.Chivalry.DivineFurySpell.UnderEffect( attacker ) )
				//totalBonus += 0.1;

			double discordanceScalar = 0.0;

			if ( SkillHandlers.Discordance.GetScalar( attacker, ref discordanceScalar ) )
				totalBonus += discordanceScalar * 2;

			damage += (damage * totalBonus);

			WeaponAbility a = WeaponAbility.GetCurrentAbility( attacker );

			if ( checkAbility && a != null )
				damage *= a.DamageScalar;

			return damage;
		}

		public virtual int VirtualDamageBonus{ get{ return 0; } }

		public virtual int ComputeDamageAOS( Mobile attacker, Mobile defender )
		{
			return (int)ScaleDamageAOS( attacker, GetBaseDamage( attacker ), true, true );
		}

		public virtual double ScaleDamageOld( Mobile attacker, double damage, bool checkSkills, bool checkAbility )
		{
			if ( checkSkills )
			{
				attacker.CheckSkill( SkillName.Tactics, 0.0, 100.0 ); // Passively check tactics for gain
				attacker.CheckSkill( SkillName.Anatomy, 0.0, 100.0 ); // Passively check Anatomy for gain

				if ( Type == WeaponType.Axe )
					attacker.CheckSkill( SkillName.Lumberjacking, 0.0, 100.0 ); // Passively check Lumberjacking for gain
			}

			/* Compute tactics modifier
			 * :   0.0 = 50% loss
			 * :  50.0 = unchanged
			 * : 100.0 = 50% bonus
			 */
			double tacticsBonus = (attacker.Skills[SkillName.Tactics].Value - 50.0) / 100.0;

			/* Compute strength modifier
			 * : 1% bonus for every 5 strength
			 */
			int tempStr = (attacker.STRBonusCap > 0 && attacker.Str > attacker.STRBonusCap) ? attacker.STRBonusCap : attacker.Str;
			double strBonus = (tempStr / 5.0) / 100.0;

			/* Compute anatomy modifier
			 * : 1% bonus for every 5 points of anatomy
			 * : +10% bonus at Grandmaster or higher
			 */
			double anatomyValue = attacker.Skills[SkillName.Anatomy].Value;
			double anatomyBonus = (anatomyValue / 5.0) / 100.0;

			if ( anatomyValue >= 100.0 )
				anatomyBonus += 0.1;

			/* Compute lumberjacking bonus
			 * : 1% bonus for every 5 points of lumberjacking
			 * : +10% bonus at Grandmaster or higher
			 */
			double lumberBonus;

			if ( Type == WeaponType.Axe )
			{
				double lumberValue = attacker.Skills[SkillName.Lumberjacking].Value;

				lumberBonus = (lumberValue / 5.0) / 100.0;

			//	if ( lumberValue >= 100.0 )        									//These 2 lines commented out
			//		lumberBonus += 0.1;												//by Old Salty
			}
			else
			{
				lumberBonus = 0.0;
			}

			// New quality bonus:
			double qualityBonus = ((int)m_Quality - 1) * 0.2;

			// Apply bonuses
			damage += (damage * tacticsBonus) + (damage * strBonus) + (damage * anatomyBonus) + (damage * lumberBonus) + (damage * qualityBonus) + ((damage * VirtualDamageBonus) / 100);

			// Old quality bonus:
#if false
			/* Apply quality offset
			 * : Low         : -4
			 * : Regular     :  0
			 * : Exceptional : +4
			 */
			damage += ((int)m_Quality - 1) * 4.0;
#endif

			/* Apply damage level offset
			 * : Regular : 0
			 * : Ruin    : 1
			 * : Might   : 3
			 * : Force   : 5
			 * : Power   : 7
			 * : Vanq    : 9
			 */
			if ( m_DamageLevel != WeaponDamageLevel.Regular )
				damage += (2.0 * (int)m_DamageLevel) - 1.0;

			// Halve the computed damage and return
			damage /= 2.0;

			WeaponAbility a = WeaponAbility.GetCurrentAbility( attacker );

			if ( checkAbility && a != null )
				damage *= a.DamageScalar;

			return (int)damage;
		}

		public virtual int ComputeDamage( Mobile attacker, Mobile defender )
		{
			if ( Core.AOS )
				return ComputeDamageAOS( attacker, defender );

			return (int)ScaleDamageOld( attacker, GetBaseDamage( attacker ), true, true );
		}

		public virtual void PlayHurtAnimation( Mobile from )
		{
			int action;
			int frames;

			switch ( from.Body.Type )
			{
				case BodyType.Sea:
				case BodyType.Animal:
				{
					action = 7;
					frames = 5;
					break;
				}
				case BodyType.Monster:
				{
					action = 10;
					frames = 4;
					break;
				}
				case BodyType.Human:
				{
					action = 20;
					frames = 5;
					break;
				}
				default: return;
			}

			if ( from.Mounted )
				return;

			from.Animate( action, frames, 1, true, false, 0 );
		}

		public virtual void PlaySwingAnimation( Mobile from )
		{
			int action;

			switch ( from.Body.Type )
			{
				case BodyType.Sea:
				case BodyType.Animal:
				{
					action = Utility.Random( 5, 2 );
					break;
				}
				case BodyType.Monster:
				{
					switch ( Animation )
					{
						default:
						case WeaponAnimation.Wrestle:
						case WeaponAnimation.Bash1H:
						case WeaponAnimation.Pierce1H:
						case WeaponAnimation.Slash1H:
						case WeaponAnimation.Bash2H:
						case WeaponAnimation.Pierce2H:
						case WeaponAnimation.Slash2H: action = Utility.Random( 4, 3 ); break;
						case WeaponAnimation.ShootBow:  return; // 7
						case WeaponAnimation.ShootXBow: return; // 8
					}

					break;
				}
				case BodyType.Human:
				{
					if ( !from.Mounted )
					{
						action = (int)Animation;
					}
					else
					{
						switch ( Animation )
						{
							default:
							case WeaponAnimation.Wrestle:
							case WeaponAnimation.Bash1H:
							case WeaponAnimation.Pierce1H:
							case WeaponAnimation.Slash1H: action = 26; break;
							case WeaponAnimation.Bash2H:
							case WeaponAnimation.Pierce2H:
							case WeaponAnimation.Slash2H: action = 29; break;
							case WeaponAnimation.ShootBow: action = 27; break;
							case WeaponAnimation.ShootXBow: action = 28; break;
						}
					}

					break;
				}
				default: return;
			}

			from.Animate( action, 7, 1, true, false, 0 );
		}

		private static void SetSaveFlag( ref SaveFlag flags, SaveFlag toSet, bool setIf )
		{
			if ( setIf )
				flags |= toSet;
		}

		private static bool GetSaveFlag( SaveFlag flags, SaveFlag toGet )
		{
			return ( (flags & toGet) != 0 );
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			int version = 8;
			writer.Write( (int) version ); // version

			SaveFlag flags = SaveFlag.None;

			SetSaveFlag( ref flags, SaveFlag.DamageLevel,		m_DamageLevel != WeaponDamageLevel.Regular );
			SetSaveFlag( ref flags, SaveFlag.AccuracyLevel,		m_AccuracyLevel != WeaponAccuracyLevel.Regular );
			SetSaveFlag( ref flags, SaveFlag.DurabilityLevel,	m_DurabilityLevel != WeaponDurabilityLevel.Regular );
			SetSaveFlag( ref flags, SaveFlag.Quality,			m_Quality != WeaponQuality.Regular );
			SetSaveFlag( ref flags, SaveFlag.Hits,				m_Hits != 0 );
			SetSaveFlag( ref flags, SaveFlag.MaxHits,			m_MaxHits != 0 );
			SetSaveFlag( ref flags, SaveFlag.Slayer,			m_Slayer != SlayerName.None );
			SetSaveFlag( ref flags, SaveFlag.Poison,			m_Poison != null );
			SetSaveFlag( ref flags, SaveFlag.PoisonCharges,		m_PoisonCharges != 0 );
			SetSaveFlag( ref flags, SaveFlag.Crafter,			m_Crafter != null );
			SetSaveFlag( ref flags, SaveFlag.Identified,		m_Identified != false );
			SetSaveFlag( ref flags, SaveFlag.StrReq,			m_StrReq != -1 );
			SetSaveFlag( ref flags, SaveFlag.DexReq,			m_DexReq != -1 );
			SetSaveFlag( ref flags, SaveFlag.IntReq,			m_IntReq != -1 );
			SetSaveFlag( ref flags, SaveFlag.MinDamage,			m_MinDamage != -1 );
			SetSaveFlag( ref flags, SaveFlag.MaxDamage,			m_MaxDamage != -1 );
			SetSaveFlag( ref flags, SaveFlag.HitSound,			m_HitSound != -1 );
			SetSaveFlag( ref flags, SaveFlag.MissSound,			m_MissSound != -1 );
			SetSaveFlag( ref flags, SaveFlag.Speed,				m_Speed != -1 );
			SetSaveFlag( ref flags, SaveFlag.MaxRange,			m_MaxRange != -1 );
			SetSaveFlag( ref flags, SaveFlag.Skill,				m_Skill != (SkillName)(-1) );
			SetSaveFlag( ref flags, SaveFlag.Type,				m_Type != (WeaponType)(-1) );
			SetSaveFlag( ref flags, SaveFlag.Animation,			m_Animation != (WeaponAnimation)(-1) );
			SetSaveFlag( ref flags, SaveFlag.Resource,			m_Resource != CraftResource.Iron );
			SetSaveFlag( ref flags, SaveFlag.xAttributes,		false ); // turned off in version 8
			SetSaveFlag( ref flags, SaveFlag.xWeaponAttributes,	false ); // turned off in version 8
//			SetSaveFlag( ref flags, SaveFlag.Open1,		 ); 
//			SetSaveFlag( ref flags, SaveFlag.OldDieRolls,		OldDieRolls != -1 );
//			SetSaveFlag( ref flags, SaveFlag.OldDieMax,			OldDieMax != -1 );
//			SetSaveFlag( ref flags, SaveFlag.OldAddConstant,	OldAddConstant != -1 );

			writer.Write( (int) flags );

			if ( GetSaveFlag( flags, SaveFlag.DamageLevel ) )
				writer.Write( (int) m_DamageLevel );

			if ( GetSaveFlag( flags, SaveFlag.AccuracyLevel ) )
				writer.Write( (int) m_AccuracyLevel );

			if ( GetSaveFlag( flags, SaveFlag.DurabilityLevel ) )
				writer.Write( (int) m_DurabilityLevel );

			if ( GetSaveFlag( flags, SaveFlag.Quality ) )
				writer.Write( (int) m_Quality );

			if ( GetSaveFlag( flags, SaveFlag.Hits ) )
				writer.Write( (int) m_Hits );

			if ( GetSaveFlag( flags, SaveFlag.MaxHits ) )
				writer.Write( (int) m_MaxHits );

			if ( GetSaveFlag( flags, SaveFlag.Slayer ) )
				writer.Write( (int) m_Slayer );

			if ( GetSaveFlag( flags, SaveFlag.Poison ) )
				Poison.Serialize( m_Poison, writer );

			if ( GetSaveFlag( flags, SaveFlag.PoisonCharges ) )
				writer.Write( (int) m_PoisonCharges );

			if ( GetSaveFlag( flags, SaveFlag.Crafter ) )
				writer.Write( (Mobile) m_Crafter );

			if ( GetSaveFlag( flags, SaveFlag.StrReq ) )
				writer.Write( (int) m_StrReq );

			if ( GetSaveFlag( flags, SaveFlag.DexReq ) )
				writer.Write( (int) m_DexReq );

			if ( GetSaveFlag( flags, SaveFlag.IntReq ) )
				writer.Write( (int) m_IntReq );

			if ( GetSaveFlag( flags, SaveFlag.MinDamage ) )
				writer.Write( (int) m_MinDamage );

			if ( GetSaveFlag( flags, SaveFlag.MaxDamage ) )
				writer.Write( (int) m_MaxDamage );

			if ( GetSaveFlag( flags, SaveFlag.HitSound ) )
				writer.Write( (int) m_HitSound );

			if ( GetSaveFlag( flags, SaveFlag.MissSound ) )
				writer.Write( (int) m_MissSound );

			if ( GetSaveFlag( flags, SaveFlag.Speed ) )
				writer.Write( (int) m_Speed );

			if ( GetSaveFlag( flags, SaveFlag.MaxRange ) )
				writer.Write( (int) m_MaxRange );

			if ( GetSaveFlag( flags, SaveFlag.Skill ) )
				writer.Write( (int) m_Skill );

			if ( GetSaveFlag( flags, SaveFlag.Type ) )
				writer.Write( (int) m_Type );

			if ( GetSaveFlag( flags, SaveFlag.Animation ) )
				writer.Write( (int) m_Animation );

			if ( GetSaveFlag( flags, SaveFlag.Resource ) )
				writer.Write( (int) m_Resource );

			// turned off in version 8
			//if ( GetSaveFlag( flags, SaveFlag.xAttributes ) )
				//m_AosAttributes.Serialize( writer );

			//if ( GetSaveFlag( flags, SaveFlag.xWeaponAttributes ) )
				//m_AosWeaponAttributes.Serialize( writer );

//			if ( GetSaveFlag( flags, SaveFlag.OldDieRolls ) )
//				writer.Write( OldDieRolls );
//
//			if ( GetSaveFlag( flags, SaveFlag.OldDieMax ) )
//				writer.Write( OldDieMax );
//
//			if ( GetSaveFlag( flags, SaveFlag.OldAddConstant ) )
//				writer.Write( OldAddConstant );
		}

		[Flags]
		private enum SaveFlag
		{
			None					= 0x00000000,
			DamageLevel				= 0x00000001,
			AccuracyLevel			= 0x00000002,
			DurabilityLevel			= 0x00000004,
			Quality					= 0x00000008,
			Hits					= 0x00000010,
			MaxHits					= 0x00000020,
			Slayer					= 0x00000040,
			Poison					= 0x00000080,
			PoisonCharges			= 0x00000100,
			Crafter					= 0x00000200,
			Identified				= 0x00000400,
			StrReq					= 0x00000800,
			DexReq					= 0x00001000,
			IntReq					= 0x00002000,
			MinDamage				= 0x00004000,
			MaxDamage				= 0x00008000,
			HitSound				= 0x00010000,
			MissSound				= 0x00020000,
			Speed					= 0x00040000,
			MaxRange				= 0x00080000,
			Skill					= 0x00100000,
			Type					= 0x00200000,
			Animation				= 0x00400000,
			Resource				= 0x00800000,
			xAttributes				= 0x01000000,
			xWeaponAttributes		= 0x02000000,
			Open1					= 0x04000000,	
			OldDieRolls				= 0x08000000,
			OldDieMax				= 0x10000000,
			OldAddConstant			= 0x20000000
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
			SaveFlag flags = (SaveFlag)reader.ReadInt();

			switch ( version )
			{
				case 8:
				{
					// turnned off AOS attributes
					goto case 7;
				}
				case 7:
				{
					goto case 6;
				}
				case 6:
				{
					goto case 5;
				}
				case 5:
				{
					if ( GetSaveFlag( flags, SaveFlag.DamageLevel ) )
						m_DamageLevel = (WeaponDamageLevel)reader.ReadInt();

					if ( GetSaveFlag( flags, SaveFlag.AccuracyLevel ) )
						m_AccuracyLevel = (WeaponAccuracyLevel)reader.ReadInt();

					if ( GetSaveFlag( flags, SaveFlag.DurabilityLevel ) )
						m_DurabilityLevel = (WeaponDurabilityLevel)reader.ReadInt();

					if ( GetSaveFlag( flags, SaveFlag.Quality ) )
						m_Quality = (WeaponQuality)reader.ReadInt();
					else
						m_Quality = WeaponQuality.Regular;

					if ( GetSaveFlag( flags, SaveFlag.Hits ) )
						m_Hits = reader.ReadInt();

					if ( GetSaveFlag( flags, SaveFlag.MaxHits ) )
						m_MaxHits = reader.ReadInt();

					if ( GetSaveFlag( flags, SaveFlag.Slayer ) )
						m_Slayer = (SlayerName)reader.ReadInt();

					if ( GetSaveFlag( flags, SaveFlag.Poison ) )
						m_Poison = Poison.Deserialize( reader );

					if ( GetSaveFlag( flags, SaveFlag.PoisonCharges ) )
						m_PoisonCharges = reader.ReadInt();

					if ( GetSaveFlag( flags, SaveFlag.Crafter ) )
						m_Crafter = reader.ReadMobile();

					if ( GetSaveFlag( flags, SaveFlag.Identified ) )
						m_Identified = ( version >= 6 || reader.ReadBool() );

					if ( GetSaveFlag( flags, SaveFlag.StrReq ) )
						m_StrReq = reader.ReadInt();
					else
						m_StrReq = -1;

					if ( GetSaveFlag( flags, SaveFlag.DexReq ) )
						m_DexReq = reader.ReadInt();
					else
						m_DexReq = -1;

					if ( GetSaveFlag( flags, SaveFlag.IntReq ) )
						m_IntReq = reader.ReadInt();
					else
						m_IntReq = -1;

					if ( GetSaveFlag( flags, SaveFlag.MinDamage ) )
						m_MinDamage = reader.ReadInt();
					else
						m_MinDamage = -1;

					if ( GetSaveFlag( flags, SaveFlag.MaxDamage ) )
						m_MaxDamage = reader.ReadInt();
					else
						m_MaxDamage = -1;

					if ( GetSaveFlag( flags, SaveFlag.HitSound ) )
						m_HitSound = reader.ReadInt();
					else
						m_HitSound = -1;

					if ( GetSaveFlag( flags, SaveFlag.MissSound ) )
						m_MissSound = reader.ReadInt();
					else
						m_MissSound = -1;

					if ( GetSaveFlag( flags, SaveFlag.Speed ) )
						m_Speed = reader.ReadInt();
					else
						m_Speed = -1;

					if ( GetSaveFlag( flags, SaveFlag.MaxRange ) )
						m_MaxRange = reader.ReadInt();
					else
						m_MaxRange = -1;

					if ( GetSaveFlag( flags, SaveFlag.Skill ) )
						m_Skill = (SkillName)reader.ReadInt();
					else
						m_Skill = (SkillName)(-1);

					if ( GetSaveFlag( flags, SaveFlag.Type ) )
						m_Type = (WeaponType)reader.ReadInt();
					else
						m_Type = (WeaponType)(-1);

					if ( GetSaveFlag( flags, SaveFlag.Animation ) )
						m_Animation = (WeaponAnimation)reader.ReadInt();
					else
						m_Animation = (WeaponAnimation)(-1);

					if ( GetSaveFlag( flags, SaveFlag.Resource ) )
						m_Resource = (CraftResource)reader.ReadInt();
					else
						m_Resource = CraftResource.Iron;

					// obsolete from version 8 on
					if (version < 8)
					{
						AosAttributes dmy_AosAttributes;
						AosWeaponAttributes dmy_AosWeaponAttributes;

						if ( GetSaveFlag( flags, SaveFlag.xAttributes ) )
							dmy_AosAttributes = new AosAttributes( this, reader );
						//else
							//dmy_AosAttributes = new AosAttributes( this );

						if ( GetSaveFlag( flags, SaveFlag.xWeaponAttributes ) )
							dmy_AosWeaponAttributes = new AosWeaponAttributes( this, reader );
						//else
							//dmy_AosWeaponAttributes = new AosWeaponAttributes( this );
					}

					if ( UseSkillMod && m_AccuracyLevel != WeaponAccuracyLevel.Regular && Parent is Mobile )
					{
						m_SkillMod = new DefaultSkillMod( SkillName.Tactics, true, (int)m_AccuracyLevel * 5 );
						((Mobile)Parent).AddSkillMod( m_SkillMod );
					}

					/*if ( Core.AOS && m_AosWeaponAttributes.MageWeapon != 0 && Parent is Mobile )
					{
						m_MageMod = new DefaultSkillMod( SkillName.Magery, true, -m_AosWeaponAttributes.MageWeapon );
						((Mobile)Parent).AddSkillMod( m_MageMod );
					}*/

                    // erl: made obsolete by PlayerCrafted in version 9
            		//if (version < 9)
					//{
						//if ( GetSaveFlag( flags, SaveFlag.PlayerConstructed ) )
							//PlayerCrafted = true;
					//}

					break;
				}
				case 4:
				{
					m_Slayer = (SlayerName)reader.ReadInt();

					goto case 3;
				}
				case 3:
				{
					m_StrReq = reader.ReadInt();
					m_DexReq = reader.ReadInt();
					m_IntReq = reader.ReadInt();

					goto case 2;
				}
				case 2:
				{
					m_Identified = reader.ReadBool();

					goto case 1;
				}
				case 1:
				{
					m_MaxRange = reader.ReadInt();

					goto case 0;
				}
				case 0:
				{
					if ( version == 0 )
						m_MaxRange = 1; // default

					if ( version < 5 )
					{
						m_Resource = CraftResource.Iron;
						//m_AosAttributes = new AosAttributes( this );
						//m_AosWeaponAttributes = new AosWeaponAttributes( this );
					}

					m_MinDamage = reader.ReadInt();
					m_MaxDamage = reader.ReadInt();

					m_Speed = reader.ReadInt();

					m_HitSound = reader.ReadInt();
					m_MissSound = reader.ReadInt();

					m_Skill = (SkillName)reader.ReadInt();
					m_Type = (WeaponType)reader.ReadInt();
					m_Animation = (WeaponAnimation)reader.ReadInt();
					m_DamageLevel = (WeaponDamageLevel)reader.ReadInt();
					m_AccuracyLevel = (WeaponAccuracyLevel)reader.ReadInt();
					m_DurabilityLevel = (WeaponDurabilityLevel)reader.ReadInt();
					m_Quality = (WeaponQuality)reader.ReadInt();

					m_Crafter = reader.ReadMobile();

					m_Poison = Poison.Deserialize( reader );
					m_PoisonCharges = reader.ReadInt();

					if ( m_StrReq == OldStrengthReq )
						m_StrReq = -1;

					if ( m_DexReq == OldDexterityReq )
						m_DexReq = -1;

					if ( m_IntReq == OldIntelligenceReq )
						m_IntReq = -1;

					if ( m_MinDamage == OldMinDamage )
						m_MinDamage = -1;

					if ( m_MaxDamage == OldMaxDamage )
						m_MaxDamage = -1;

					if ( m_HitSound == OldHitSound )
						m_HitSound = -1;

					if ( m_MissSound == OldMissSound )
						m_MissSound = -1;

					if ( m_Speed == OldSpeed )
						m_Speed = -1;

					if ( m_MaxRange == OldMaxRange )
						m_MaxRange = -1;

					if ( m_Skill == OldSkill )
						m_Skill = (SkillName)(-1);

					if ( m_Type == OldType )
						m_Type = (WeaponType)(-1);

					if ( m_Animation == OldAnimation )
						m_Animation = (WeaponAnimation)(-1);

					if ( UseSkillMod && m_AccuracyLevel != WeaponAccuracyLevel.Regular && Parent is Mobile )
					{
						m_SkillMod = new DefaultSkillMod( SkillName.Tactics, true, (int)m_AccuracyLevel * 5);
						((Mobile)Parent).AddSkillMod( m_SkillMod );
					}

					break;
				}
			}
/*
			int strBonus = m_AosAttributes.BonusStr;
			int dexBonus = m_AosAttributes.BonusDex;
			int intBonus = m_AosAttributes.BonusInt;

			if ( this.Parent is Mobile && (strBonus != 0 || dexBonus != 0 || intBonus != 0) )
			{
				Mobile m = (Mobile)this.Parent;

				string modName = this.Serial.ToString();

				if ( strBonus != 0 )
					m.AddStatMod( new StatMod( StatType.Str, modName + "Str", strBonus, TimeSpan.Zero ) );

				if ( dexBonus != 0 )
					m.AddStatMod( new StatMod( StatType.Dex, modName + "Dex", dexBonus, TimeSpan.Zero ) );

				if ( intBonus != 0 )
					m.AddStatMod( new StatMod( StatType.Int, modName + "Int", intBonus, TimeSpan.Zero ) );
			}
*/
			if ( Parent is Mobile )
				((Mobile)Parent).CheckStatTimers();

			if ( m_Hits <= 0 && m_MaxHits <= 0 )
			{
				m_Hits = m_MaxHits = Utility.RandomMinMax( InitMinHits, InitMaxHits );
			}

			//if ( version < 6 )
				//PlayerCrafted = true; // we don't know, so, assume it's crafted
		}

		
		public BaseWeapon( int itemID ) : base( itemID )
		{
			Layer = (Layer)ItemData.Quality;

			m_Quality = WeaponQuality.Regular;
			m_StrReq = -1;
			m_DexReq = -1;
			m_IntReq = -1;
			m_MinDamage = -1;
			m_MaxDamage = -1;
			m_HitSound = -1;
			m_MissSound = -1;
			m_Speed = -1;
			m_MaxRange = -1;
			m_Skill = (SkillName)(-1);
			m_Type = (WeaponType)(-1);
			m_Animation = (WeaponAnimation)(-1);

			m_Hits = m_MaxHits = Utility.RandomMinMax( InitMinHits, InitMaxHits );

			m_Resource = CraftResource.Iron;

			//m_AosAttributes = new AosAttributes( this );
			//m_AosWeaponAttributes = new AosWeaponAttributes( this );
		}

		public BaseWeapon( Serial serial ) : base( serial )
		{
		}

		private string GetNameString()
		{
			string name = this.Name;

			if ( name == null )
				name = String.Format( "#{0}", LabelNumber );

			return name;
		}

		[Hue, CommandProperty( AccessLevel.GameMaster )]
		public override int Hue
		{
			get{ return base.Hue; }
			set{ base.Hue = value; InvalidateProperties(); }
		}

		public override void AddNameProperty( ObjectPropertyList list )
		{
			int oreType;

			if ( Hue == 0 )
			{
				oreType = 0;
			}
			else
			{
				switch ( m_Resource )
				{
					case CraftResource.DullCopper:		oreType = 1053108; break; // dull copper
					case CraftResource.ShadowIron:		oreType = 1053107; break; // shadow iron
					case CraftResource.Copper:			oreType = 1053106; break; // copper
					case CraftResource.Bronze:			oreType = 1053105; break; // bronze
					case CraftResource.Gold:			oreType = 1053104; break; // golden
					case CraftResource.Agapite:			oreType = 1053103; break; // agapite
					case CraftResource.Verite:			oreType = 1053102; break; // verite
					case CraftResource.Valorite:		oreType = 1053101; break; // valorite
					case CraftResource.SpinedLeather:	oreType = 1061118; break; // spined
					case CraftResource.HornedLeather:	oreType = 1061117; break; // horned
					case CraftResource.BarbedLeather:	oreType = 1061116; break; // barbed
					case CraftResource.RedScales:		oreType = 1060814; break; // red
					case CraftResource.YellowScales:	oreType = 1060818; break; // yellow
					case CraftResource.BlackScales:		oreType = 1060820; break; // black
					case CraftResource.GreenScales:		oreType = 1060819; break; // green
					case CraftResource.WhiteScales:		oreType = 1060821; break; // white
					case CraftResource.BlueScales:		oreType = 1060815; break; // blue
					default: oreType = 0; break;
				}
			}

			if ( oreType != 0 )
				list.Add( 1053099, "#{0}\t{1}", oreType, GetNameString() ); // ~1_oretype~ ~2_armortype~
			else if ( Name == null )
				list.Add( LabelNumber );
			else
				list.Add( Name );
		}

		public override bool AllowEquipedCast( Mobile from )
		{
			if ( base.AllowEquipedCast( from ) )
				return true;

			//return ( m_AosAttributes.SpellChanneling != 0 );
			return false;
		}

		public virtual int ArtifactRarity
		{
			get{ return 0; }
		}

		public virtual int GetLuckBonus()
		{
			CraftResourceInfo resInfo = CraftResources.GetInfo( m_Resource );

			if ( resInfo == null )
				return 0;

			CraftAttributeInfo attrInfo = resInfo.AttributeInfo;

			if ( attrInfo == null )
				return 0;

			return attrInfo.WeaponLuck;
		}

		public override void GetProperties( ObjectPropertyList list )
		{
			base.GetProperties( list );

			if ( m_Crafter != null )
				list.Add( 1050043, m_Crafter.Name ); // crafted by ~1_NAME~

			if ( m_Quality == WeaponQuality.Exceptional )
				list.Add( 1060636 ); // exceptional


			if ( ArtifactRarity > 0 )
				list.Add( 1061078, ArtifactRarity.ToString() ); // artifact rarity ~1_val~

			if ( this is IUsesRemaining && ((IUsesRemaining)this).ShowUsesRemaining )
				list.Add( 1060584, ((IUsesRemaining)this).UsesRemaining.ToString() ); // uses remaining: ~1_val~

			if ( m_Poison != null && m_PoisonCharges > 0 )
				list.Add( 1062412 + m_Poison.Level, m_PoisonCharges.ToString() );

			if ( m_Slayer != SlayerName.None )
				list.Add( 1017383 + (int)m_Slayer );


			int prop;
/*
			if ( (prop = m_AosWeaponAttributes.UseBestSkill) != 0 )
				list.Add( 1060400 ); // use best weapon skill

			if ( (prop = (GetDamageBonus() + m_AosAttributes.WeaponDamage)) != 0 )
				list.Add( 1060401, prop.ToString() ); // damage increase ~1_val~%

			if ( (prop = m_AosAttributes.DefendChance) != 0 )
				list.Add( 1060408, prop.ToString() ); // defense chance increase ~1_val~%

			if ( (prop = m_AosAttributes.BonusDex) != 0 )
				list.Add( 1060409, prop.ToString() ); // dexterity bonus ~1_val~

			if ( (prop = m_AosAttributes.EnhancePotions) != 0 )
				list.Add( 1060411, prop.ToString() ); // enhance potions ~1_val~%

			if ( (prop = m_AosAttributes.CastRecovery) != 0 )
				list.Add( 1060412, prop.ToString() ); // faster cast recovery ~1_val~

			if ( (prop = m_AosAttributes.CastSpeed) != 0 )
				list.Add( 1060413, prop.ToString() ); // faster casting ~1_val~

			if ( (prop = (GetHitChanceBonus() + m_AosAttributes.AttackChance)) != 0 )
				list.Add( 1060415, prop.ToString() ); // hit chance increase ~1_val~%

			if ( (prop = m_AosWeaponAttributes.HitColdArea) != 0 )
				list.Add( 1060416, prop.ToString() ); // hit cold area ~1_val~%

			if ( (prop = m_AosWeaponAttributes.HitDispel) != 0 )
				list.Add( 1060417, prop.ToString() ); // hit dispel ~1_val~%

			if ( (prop = m_AosWeaponAttributes.HitEnergyArea) != 0 )
				list.Add( 1060418, prop.ToString() ); // hit energy area ~1_val~%

			if ( (prop = m_AosWeaponAttributes.HitFireArea) != 0 )
				list.Add( 1060419, prop.ToString() ); // hit fire area ~1_val~%

			if ( (prop = m_AosWeaponAttributes.HitFireball) != 0 )
				list.Add( 1060420, prop.ToString() ); // hit fireball ~1_val~%

			if ( (prop = m_AosWeaponAttributes.HitHarm) != 0 )
				list.Add( 1060421, prop.ToString() ); // hit harm ~1_val~%

			if ( (prop = m_AosWeaponAttributes.HitLeechHits) != 0 )
				list.Add( 1060422, prop.ToString() ); // hit life leech ~1_val~%

			if ( (prop = m_AosWeaponAttributes.HitLightning) != 0 )
				list.Add( 1060423, prop.ToString() ); // hit lightning ~1_val~%

			if ( (prop = m_AosWeaponAttributes.HitLowerAttack) != 0 )
				list.Add( 1060424, prop.ToString() ); // hit lower attack ~1_val~%

			if ( (prop = m_AosWeaponAttributes.HitLowerDefend) != 0 )
				list.Add( 1060425, prop.ToString() ); // hit lower defense ~1_val~%

			if ( (prop = m_AosWeaponAttributes.HitMagicArrow) != 0 )
				list.Add( 1060426, prop.ToString() ); // hit magic arrow ~1_val~%

			if ( (prop = m_AosWeaponAttributes.HitLeechMana) != 0 )
				list.Add( 1060427, prop.ToString() ); // hit mana leech ~1_val~%

			if ( (prop = m_AosWeaponAttributes.HitPhysicalArea) != 0 )
				list.Add( 1060428, prop.ToString() ); // hit physical area ~1_val~%

			if ( (prop = m_AosWeaponAttributes.HitPoisonArea) != 0 )
				list.Add( 1060429, prop.ToString() ); // hit poison area ~1_val~%

			if ( (prop = m_AosWeaponAttributes.HitLeechStam) != 0 )
				list.Add( 1060430, prop.ToString() ); // hit stamina leech ~1_val~%

			if ( (prop = m_AosAttributes.BonusHits) != 0 )
				list.Add( 1060431, prop.ToString() ); // hit point increase ~1_val~

			if ( (prop = m_AosAttributes.BonusInt) != 0 )
				list.Add( 1060432, prop.ToString() ); // intelligence bonus ~1_val~

			if ( (prop = m_AosAttributes.LowerManaCost) != 0 )
				list.Add( 1060433, prop.ToString() ); // lower mana cost ~1_val~%

			if ( (prop = m_AosAttributes.LowerRegCost) != 0 )
				list.Add( 1060434, prop.ToString() ); // lower reagent cost ~1_val~%
*/
			if ( (prop = GetLowerStatReq()) != 0 )
				list.Add( 1060435, prop.ToString() ); // lower requirements ~1_val~%

			//if ( (prop = (GetLuckBonus() + m_AosAttributes.Luck)) != 0 )
				//list.Add( 1060436, prop.ToString() ); // luck ~1_val~
/*
			if ( (prop = m_AosWeaponAttributes.MageWeapon) != 0 )
				list.Add( 1060438, prop.ToString() ); // mage weapon -~1_val~ skill

			if ( (prop = m_AosAttributes.BonusMana) != 0 )
				list.Add( 1060439, prop.ToString() ); // mana increase ~1_val~

			if ( (prop = m_AosAttributes.RegenMana) != 0 )
				list.Add( 1060440, prop.ToString() ); // mana regeneration ~1_val~

			if ( (prop = m_AosAttributes.ReflectPhysical) != 0 )
				list.Add( 1060442, prop.ToString() ); // reflect physical damage ~1_val~%

			if ( (prop = m_AosAttributes.RegenStam) != 0 )
				list.Add( 1060443, prop.ToString() ); // stamina regeneration ~1_val~

			if ( (prop = m_AosAttributes.RegenHits) != 0 )
				list.Add( 1060444, prop.ToString() ); // hit point regeneration ~1_val~

			if ( (prop = m_AosWeaponAttributes.SelfRepair) != 0 )
				list.Add( 1060450, prop.ToString() ); // self repair ~1_val~

			if ( (prop = m_AosAttributes.SpellChanneling) != 0 )
				list.Add( 1060482 ); // spell channeling

			if ( (prop = m_AosAttributes.SpellDamage) != 0 )
				list.Add( 1060483, prop.ToString() ); // spell damage increase ~1_val~%

			if ( (prop = m_AosAttributes.BonusStam) != 0 )
				list.Add( 1060484, prop.ToString() ); // stamina increase ~1_val~

			if ( (prop = m_AosAttributes.BonusStr) != 0 )
				list.Add( 1060485, prop.ToString() ); // strength bonus ~1_val~

			if ( (prop = m_AosAttributes.WeaponSpeed) != 0 )
				list.Add( 1060486, prop.ToString() ); // swing speed increase ~1_val~%
*/
/*
			int phys, fire, cold, pois, nrgy;

			GetDamageTypes( null, out phys, out fire, out cold, out pois, out nrgy );

			if ( phys != 0 )
				list.Add( 1060403, phys.ToString() ); // physical damage ~1_val~%

			if ( fire != 0 )
				list.Add( 1060405, fire.ToString() ); // fire damage ~1_val~%

			if ( cold != 0 )
				list.Add( 1060404, cold.ToString() ); // cold damage ~1_val~%

			if ( pois != 0 )
				list.Add( 1060406, pois.ToString() ); // poison damage ~1_val~%

			if ( nrgy != 0 )
				list.Add( 1060407, nrgy.ToString() ); // energy damage ~1_val~%
*/
			list.Add( 1061168, "{0}\t{1}", MinDamage.ToString(), MaxDamage.ToString() ); // weapon damage ~1_val~ - ~2_val~
			list.Add( 1061167, Speed.ToString() ); // weapon speed ~1_val~

			if ( MaxRange > 1 )
				list.Add( 1061169, MaxRange.ToString() ); // range ~1_val~

			int strReq = AOS.Scale( StrRequirement, 100 - GetLowerStatReq() );

			if ( strReq > 0 )
				list.Add( 1061170, strReq.ToString() ); // strength requirement ~1_val~

			if ( Layer == Layer.TwoHanded )
				list.Add( 1061171 ); // two-handed weapon
			else
				list.Add( 1061824 ); // one-handed weapon

			//if ( m_AosWeaponAttributes.UseBestSkill == 0 && m_AosWeaponAttributes.MageWeapon == 0 )
			{
				switch ( Skill )
				{
					case SkillName.Swords:  list.Add( 1061172 ); break; // skill required: swordsmanship
					case SkillName.Macing:  list.Add( 1061173 ); break; // skill required: mace fighting
					case SkillName.Fencing: list.Add( 1061174 ); break; // skill required: fencing
					case SkillName.Archery: list.Add( 1061175 ); break; // skill required: archery
				}
			}

			if ( m_Hits > 0 && m_MaxHits > 0 )
				list.Add( 1060639, "{0}\t{1}", m_Hits, m_MaxHits ); // durability ~1_val~ / ~2_val~
		}

		public virtual string OldName
		{
			get
			{
				return null;
			}
		}
		public virtual string OldArticle
		{
			get
			{
				return "a";
			}
		}

		public override void OnSingleClick(Mobile from)
		{
			if (this.HideAttributes == true)
			{
				base.OnSingleClick(from);
				return;
			}

			ArrayList attrs = new ArrayList();

            if (DisplayLootType)
            {
                if (LootType == LootType.Blessed)
                    attrs.Add(new EquipInfoAttribute(1038021)); // blessed
                else if (LootType == LootType.Cursed)
                    attrs.Add(new EquipInfoAttribute(1049643)); // cursed
            }

            if (Name != null || OldName == null) // only use the new ([X/Y/Z]) method on things we don't have OldNames for
            {
                if (m_Quality == WeaponQuality.Exceptional)
                    attrs.Add(new EquipInfoAttribute(1018305 - (int)m_Quality));

                if (m_Identified)
                {
                    if (m_Slayer != SlayerName.None)
                        attrs.Add(new EquipInfoAttribute(1017383 + (int)m_Slayer));

                    if (m_DurabilityLevel != WeaponDurabilityLevel.Regular)
                        attrs.Add(new EquipInfoAttribute(1038000 + (int)m_DurabilityLevel));

                    if (m_DamageLevel != WeaponDamageLevel.Regular)
                        attrs.Add(new EquipInfoAttribute(1038015 + (int)m_DamageLevel));

                    if (m_AccuracyLevel != WeaponAccuracyLevel.Regular)
                        attrs.Add(new EquipInfoAttribute(1038010 + (int)m_AccuracyLevel));
                }
                else if (m_Slayer != SlayerName.None || m_DurabilityLevel != WeaponDurabilityLevel.Regular || m_DamageLevel != WeaponDamageLevel.Regular || m_AccuracyLevel != WeaponAccuracyLevel.Regular)
                {
                    attrs.Add(new EquipInfoAttribute(1038000)); // Unidentified
                }
            }

            if (Name != null || OldName == null)
            {
                if (m_Poison != null && m_PoisonCharges > 0)
                    attrs.Add(new EquipInfoAttribute(1017383, m_PoisonCharges));
            }

        

			int number;

			if (Name == null)
			{
				if (OldName == null)
				{
					number = LabelNumber;
				}
				else
				{
					string oldname = OldName;
					string article = OldArticle;
					//yay!  Show us the old way!
					if (m_Quality == WeaponQuality.Exceptional)
					{
						oldname = "exceptional " + oldname;
						article = "an";
					}

					if (m_Identified)
					{
						if (m_Slayer != SlayerName.None)
						{
							oldname = m_Slayer.ToString().ToLower() + " " + oldname;
							article = "a";
						}

						if (m_AccuracyLevel != WeaponAccuracyLevel.Regular)
						{
							if (m_AccuracyLevel == WeaponAccuracyLevel.Accurate)
							{
								oldname = "accurate " + oldname;
								article = "an";
							}
							else
							{
								oldname = m_AccuracyLevel.ToString().ToLower() + " accurate " + oldname;
								if (m_AccuracyLevel == WeaponAccuracyLevel.Eminently || m_AccuracyLevel == WeaponAccuracyLevel.Exceedingly)
								{
									article = "an";
								}
								else
								{
									article = "a";
								}
							}
						}

						if (m_DurabilityLevel != WeaponDurabilityLevel.Regular)
						{
							oldname = m_DurabilityLevel.ToString().ToLower() + " " + oldname;
							if (m_DurabilityLevel == WeaponDurabilityLevel.Indestructible)
							{
								article = "an";
							}
							else
							{
								article = "a";
							}
						}

						if (m_DamageLevel != WeaponDamageLevel.Regular)
						{
							if (m_DamageLevel == WeaponDamageLevel.Vanq) //silly people abbreviated vanquishing in the enumeration!
							{
								oldname = oldname + " of vanquishing";
							}
							else
							{
								oldname = oldname + " of " + m_DamageLevel.ToString().ToLower();
							}
						}
					}
					else if( m_Slayer != SlayerName.None 
						     || m_DurabilityLevel != WeaponDurabilityLevel.Regular 
						     || m_DamageLevel != WeaponDamageLevel.Regular 
						     || m_AccuracyLevel != WeaponAccuracyLevel.Regular )
					{
						oldname = "magic " + oldname;
						article = "a";
					}

					//crafted-by goes at the end
					if (m_Crafter != null)
					{
						oldname += " crafted by " + m_Crafter.Name;
					}

					if (m_Poison != null && m_PoisonCharges > 0)
					{
						oldname = "poisoned " + oldname;
						oldname = (oldname + ", charges: " + m_PoisonCharges);
						article = "a";
					}

					//finally, add the article
					oldname = article + " " + oldname;

					this.LabelTo(from, oldname);
					number = 1041000;
				}
			}
			else
			{
				this.LabelTo(from, Name);
				number = 1041000;
			}

			if (attrs.Count == 0 && Crafter == null && Name != null)
				return;

			if (Name != null || OldName == null)
			{
				EquipmentInfo eqInfo = new EquipmentInfo(number, m_Crafter, false, (EquipInfoAttribute[])attrs.ToArray(typeof(EquipInfoAttribute)));
				from.Send(new DisplayEquipmentInfo(this, eqInfo));
			}
			else
			{
				if (attrs.Count > 0)
				{
					EquipmentInfo eqInfo = new EquipmentInfo(number, null, false, (EquipInfoAttribute[])attrs.ToArray(typeof(EquipInfoAttribute)));
					from.Send(new DisplayEquipmentInfo(this, eqInfo));
				}
			}
		}

/*OLD OnSingleClick
		public override void OnSingleClick( Mobile from )
		{
			ArrayList attrs = new ArrayList();

			if ( DisplayLootType )
			{
				if ( LootType == LootType.Blessed )
					attrs.Add( new EquipInfoAttribute( 1038021 ) ); // blessed
				else if ( LootType == LootType.Cursed )
					attrs.Add( new EquipInfoAttribute( 1049643 ) ); // cursed
			}

			if ( m_Quality == WeaponQuality.Exceptional )
				attrs.Add( new EquipInfoAttribute( 1018305 - (int)m_Quality ) );

			if ( m_Identified )
			{
				if ( m_Slayer != SlayerName.None )
					attrs.Add( new EquipInfoAttribute( 1017383 + (int)m_Slayer ) );

				if ( m_DurabilityLevel != WeaponDurabilityLevel.Regular )
					attrs.Add( new EquipInfoAttribute( 1038000 + (int)m_DurabilityLevel ) );

				if ( m_DamageLevel != WeaponDamageLevel.Regular )
					attrs.Add( new EquipInfoAttribute( 1038015 + (int)m_DamageLevel ) );

				if ( m_AccuracyLevel != WeaponAccuracyLevel.Regular )
					attrs.Add( new EquipInfoAttribute( 1038010 + (int)m_AccuracyLevel ) );
			}
			else if ( m_Slayer != SlayerName.None || m_DurabilityLevel != WeaponDurabilityLevel.Regular || m_DamageLevel != WeaponDamageLevel.Regular || m_AccuracyLevel != WeaponAccuracyLevel.Regular )
			{
				attrs.Add( new EquipInfoAttribute( 1038000 ) ); // Unidentified
			}

			if ( m_Poison != null && m_PoisonCharges > 0 )
				attrs.Add( new EquipInfoAttribute( 1017383, m_PoisonCharges ) );

			int number;

			if ( Name == null )
			{
				number = LabelNumber;
			}
			else
			{
				this.LabelTo( from, Name );
				number = 1041000;
			}

			if ( attrs.Count == 0 && Crafter == null && Name != null )
				return;

			EquipmentInfo eqInfo = new EquipmentInfo( number, m_Crafter, false, (EquipInfoAttribute[])attrs.ToArray( typeof( EquipInfoAttribute ) ) );

			from.Send( new DisplayEquipmentInfo( this, eqInfo ) );
		}
*/

		private static BaseWeapon m_Fists; // This value holds the default--fist--weapon

		public static BaseWeapon Fists
		{
			get{ return m_Fists; }
			set{ m_Fists = value; }
		}

		public Poison GetPoisonBasedOnSkillAndPoison( Mobile mob, Poison poisonOnWeapon )
		{
//CURRENT METHOD:
			if( !(this is BaseRanged) )
			{
				//For non-ranged weapons
				//check if we're not using MeleePoisonSkillFactor
				if( !CoreAI.IsDynamicFeatureSet(CoreAI.FeatureBits.MeleePoisonSkillFactor) )
				{
					return poisonOnWeapon;
				}
			}

			int poisonSkill = (int)mob.Skills[SkillName.Poisoning].Base;

			if( ( Utility.Random(100) <= poisonSkill ) && (poisonSkill != 0) )
			{
				return poisonOnWeapon;
			}
			else
			{
				return Poison.GetPoison( poisonOnWeapon.Level - 1 );
			}

//ALTERNATE METHOD: With min/max skilllevels per poison level
//			double min = 0; //10% chance to poison at level
//			double max = 100; //90% chance to poison at level
//			switch(poisonOnWeapon.Level)
//			{
//				case 0: //lesser
//					min = 0;
//					max = 40;
//					break;
//				case 1: //regular
//					min = 20;
//					max = 60;
//					break;
//				case 2: //greater
//					min = 40;
//					max = 80;
//					break;
//				case 3: //deadly
//					min = 60;
//					max = 100;
//					break;
//				case 4: //lethal -- should never happen
//					min = 80;
//					max = 120;
//					break;
//				default:
//					break;
//			}
//			double poisonSkill = mob.Skills[SkillName.Poisoning].Value;
//			double chanceToPoisonCurrentLevel = 0;
//			if( poisonSkill < min )
//				chanceToPoisonCurrentLevel = 10.0;
//			else if( poisonSkill > max )
//				chanceToPoisonCurrentLevel = 90.0;
//			else
//				chanceToPoisonCurrentLevel = 10 + (80 * (poisonSkill-min)/(max-min));
//
//			if( Utility.Random(100) <= chanceToPoisonCurrentLevel )
//				return poisonOnWeapon;
//			else
//				return Poison.GetPoison( poisonOnWeapon.Level - 1 );
		}

		public void CorrodeWeapon( Mobile attacker )
		{
			//no corrosion on sealed ranged weapons
			if (this.Type == WeaponType.Ranged)
			{
				if (this is SealedBow || this is SealedCrossbow || this is SealedHeavyCrossbow)
				{
					return;
				}
				else if (false == CoreAI.IsDynamicFeatureSet(CoreAI.FeatureBits.RangedCorrosion))
				{
					return;
				}			
			}

			//Corrode the weapon only if it's poisoned!
			if ( Poison != null && PoisonCharges > 0 )
			{
				int corrodeWeapon = 0; //this is the number of hits to subtract
				if( Poison == Poison.Lethal )
					corrodeWeapon += 5;
				else if( Poison == Poison.Deadly )
					corrodeWeapon += 4;
				else if( Poison == Poison.Greater )
					corrodeWeapon += 3;
				else if( Poison == Poison.Regular )
					corrodeWeapon += 2;
				else if( Poison == Poison.Lesser )
					corrodeWeapon += 1;

				double attackersPoisonSkill = attacker.Skills[SkillName.Poisoning].Value;
				//lessen the effect if attacker has poisoning skill high enough

				//New poisoning corrosion reduction
				if( attackersPoisonSkill > 0 )
				{
					//Up to 4 chances to reduce corrosion @ GM
					// 3 chances at 99.9-80.0
					// 2 chances at 79.9-60.0
					// 1 chance at 59.9-40.0
					// 0 chances at 39.9-0
					while( attackersPoisonSkill > 39.9 )
					{
						if( Utility.RandomBool() )
						{
							corrodeWeapon -= 1;
						}
						attackersPoisonSkill -= 20.0;
					}
				}

				if (this.Type == WeaponType.Ranged)
				{
					int rangedCorrosionReductionChances = CoreAI.RangedCorrosionModifier;

					while (rangedCorrosionReductionChances > 0)
					{
						if (Utility.RandomBool())
						{
							corrodeWeapon -= 1;
						}
						rangedCorrosionReductionChances -= 1;
					}
				}

				//Old poisoning corrosion reduction
				/*
				if( attackersPoisonSkill > 99.0 )
					corrodeWeapon = corrodeWeapon-2;
				else if( attackersPoisonSkill > 50.0 )
					corrodeWeapon = corrodeWeapon-1;

				//Make sure we've not eliminated corrosion completely with the compensation
				//for poisoning skill.
				if( corrodeWeapon < 1 )
					corrodeWeapon = 1;
				*/

				if( corrodeWeapon > 0 )
				{
					if (this.Type == WeaponType.Ranged)
					{
						//since ranged weapons corrode on every shot, only give them the message sometimes
						if (Utility.RandomDouble() <= 0.30)
						{
							attacker.SendMessage("The poison soaks into the bow, weakening your weapon.");
						}
					}
					else
					{
						attacker.SendMessage("Blood mixes with poison and weakens your weapon.");
					}

					//do the corrosion!
					if ( m_MaxHits > 0 )
					{
						if ( m_Hits > corrodeWeapon )
						{
                            HitPoints = HitPoints - corrodeWeapon;
						}
						else
						{
							Delete();
						}
					}
				}
			}
		}
	}

	public enum CheckSlayerResult
	{
		None,
		Slayer,
		Opposition
	}
}
