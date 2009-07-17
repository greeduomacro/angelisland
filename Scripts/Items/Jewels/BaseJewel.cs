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

/* Scripts/Items/Jewels/BaseJewel.cs
 * CHANGE LOG
 * 5/2/08, Adam
 *		Update use of HideAttributes to be simpler.
 * 5/1/08, Adam
 *		Use HideAttributes filtering to hide the [Exceptional] tag.
 *	01/04/07, Pix
 *		Fixed stat-effect items.
 *	01/02/07, Pix
 *		Stat-effect magic items no longer stack.
 *  06/26/06, Kit
 *		Added region spell checks to all magic jewlery effects, follow region casting rules!!
 *	8/18/05, erlein
 *		Added code necessary to support maker's mark and exceptional chance.
 *	7/26/05, erlein
 *		Automated removal of AoS resistance related function calls. 6 lines removed.
 *	7/26/05, Adam
 *		Massive AOS cleanout
 *	12/19/04, Adam
 *		1. In SetRandomMagicEffect() change NewMagicType to use explicit Utility.RandomList()
 *		2. In SetRandomMagicEffect() change NewLevel to use Utility.RandomMinMax(MinLevel, MaxLevel)
 *  8/9/04 - Pixie
 *		Explicitly cleaned up timers.
 *	7/25/04 smerX
 *		A new timer is initiated OnAdded
 *  7/6/04 - Pixie
 *		Added cunning, agility, strength, feeblemind, clumsy, weaken, curse, nightsight jewelry
 *	6/25/04 - Pixie
 *		Fixed jewelry so that they didn't spawn outside of the appropriate range
 *		(bracelets were spawning with teleport and rings/bracelets were spawning
 *		as unidentified magic items but when id'ed didn't have a property)
// 05/11/2004 - Pulse
//	Completed changes to implement magic jewelry.
//	changes include:
//		* several new properties: magic type, number of charges, and identified flag
//		* updated GetProperties and OnSingleClick to include magic properties
//		* JewelMagicEffect enumeration for various available spell types
//		* MagicEffectTimer class to implement spell timing effects and control charge usage
//		* All jewelry items can be made magic through the [props command for Game Master or higher access level
//		* SetMagic and SetRandomMagicEffect to allow setting an existing jewelry item to some
//			type of magic and level
//		* "Apply" routines for the various magic effects
//		* an AddStatBonus routine used by the Bless effect.
*/

using System;
using System.Collections;
using Server;
using Server.Mobiles;
using Server.Network;
using Server.Spells;
using Server.Spells.First;
using Server.Spells.Second;
using Server.Spells.Third;
using Server.Spells.Fifth;
using Server.Spells.Sixth;

namespace Server.Items
{
	public enum JewelMagicEffect
	{
		None = 0,
		MagicReflect,	//1
		Invisibility,	//2
		Bless,			//3
		Teleport,		//4
		Agility,		//5
		Cunning,		//6
		Strength,		//7
		NightSight,		//8
		Curse,			//9
		Clumsy,			//10
		Feeblemind,		//11
		Weakness,		//12
	}

	public enum GemType
	{
		None,
		StarSapphire,
		Emerald,
		Sapphire,
		Ruby,
		Citrine,
		Amethyst,
		Tourmaline,
		Amber,
		Diamond
	}

	public enum JewelQuality
	{
		Low,
		Regular,
		Exceptional
	}

	public abstract class BaseJewel : Item
	{
		//private AosAttributes m_AosAttributes;
		//private AosElementAttributes m_AosResistances;
		//private AosSkillBonuses m_AosSkillBonuses;
		private CraftResource m_Resource;
		private GemType m_GemType;
		private JewelMagicEffect m_MagicType;
		private int m_MagicCharges;
		private bool m_Identified;
		private Timer m_StatEffectTimer;
		private Timer m_InvisTimer;
		private Mobile m_Crafter;
		private JewelQuality m_Quality;

/*
		[CommandProperty( AccessLevel.GameMaster )]
		public AosAttributes Attributes
		{
			get{ return m_AosAttributes; }
			set{}
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public AosElementAttributes Resistances
		{
			get{ return m_AosResistances; }
			set{}
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public AosSkillBonuses SkillBonuses
		{
			get{ return m_AosSkillBonuses; }
			set{}
		}
*/
		[CommandProperty( AccessLevel.GameMaster )]
		public CraftResource Resource
		{
			get{ return m_Resource; }
			set{ m_Resource = value; Hue = CraftResources.GetHue( m_Resource ); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public GemType GemType
		{
			get{ return m_GemType; }
			set{ m_GemType = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public JewelMagicEffect MagicType
		{
			get { return m_MagicType; }
			set { m_MagicType = value; }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int MagicCharges
		{
			get { return m_MagicCharges; }
			set { m_MagicCharges = value; }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public bool Identified
		{
			get { return m_Identified; }
			set { m_Identified = value; }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public Mobile Crafter
		{
			get
			{
				return m_Crafter;
			}
			set
			{
				m_Crafter = value;
				InvalidateProperties();
			}
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public JewelQuality Quality
		{
			get{ return m_Quality; }
			set{ m_Quality = value; InvalidateProperties(); }
		}

		public virtual int BaseGemTypeNumber{ get{ return 0; } }

		public override int LabelNumber
		{
			get
			{
				if ( m_GemType == GemType.None )
					return base.LabelNumber;

				return BaseGemTypeNumber + (int)m_GemType - 1;
			}
		}

		public virtual int ArtifactRarity{ get{ return 0; } }

		public BaseJewel( int itemID, Layer layer ) : base( itemID )
		{
			//m_AosAttributes = new AosAttributes( this );
			//m_AosResistances = new AosElementAttributes( this );
			//m_AosSkillBonuses = new AosSkillBonuses( this );
			m_Resource = CraftResource.Iron;
			m_GemType = GemType.None;
			m_MagicType = JewelMagicEffect.None;
			m_MagicCharges = 0;
			m_Identified = true;
			Layer = layer;
			m_InvisTimer = null;
			m_StatEffectTimer = null;
		}

		public override void OnAdded( object parent )
		{
			/*if ( Core.AOS && parent is Mobile )
			{
				Mobile from = (Mobile)parent;

				m_AosSkillBonuses.AddTo( from );

				int strBonus = m_AosAttributes.BonusStr;
				int dexBonus = m_AosAttributes.BonusDex;
				int intBonus = m_AosAttributes.BonusInt;

				if ( strBonus != 0 || dexBonus != 0 || intBonus != 0 )
				{
					string modName = this.Serial.ToString();

					if ( strBonus != 0 )
						from.AddStatMod( new StatMod( StatType.Str, modName + "Str", strBonus, TimeSpan.Zero ) );

					if ( dexBonus != 0 )
						from.AddStatMod( new StatMod( StatType.Dex, modName + "Dex", dexBonus, TimeSpan.Zero ) );

					if ( intBonus != 0 )
						from.AddStatMod( new StatMod( StatType.Int, modName + "Int", intBonus, TimeSpan.Zero ) );
				}

				from.CheckStatTimers();
			}
			else*/ if (!Core.AOS && parent is PlayerMobile )
			{
				PlayerMobile Wearer = (PlayerMobile)parent;

				// if charges > 0 apply the magic effect
				if ( MagicCharges > 0 )
				{
					// apply magic effect to wearer if appropriate
					switch (MagicType)
					{
						case JewelMagicEffect.MagicReflect:
							if (ApplyMagicReflectEffect(Wearer))
								MagicCharges--;
							break;
						case JewelMagicEffect.Invisibility:
							if (ApplyInvisibilityEffect(Wearer))
								MagicCharges--;
							break;
						case JewelMagicEffect.Bless:
							//if( ApplyBlessEffect(Wearer) )
							if( ApplyStatEffect(Wearer, true, true, true, 10 ) )
								MagicCharges--;
							break;
						case JewelMagicEffect.Agility:
							if( ApplyStatEffect(Wearer, false, true, false, 10 ) )
								MagicCharges--;
							break;
						case JewelMagicEffect.Cunning:
							if( ApplyStatEffect(Wearer, false, false, true, 10 ) )
								MagicCharges--;
							break;
						case JewelMagicEffect.Strength:
							if( ApplyStatEffect(Wearer, true, false, false, 10 ) )
								MagicCharges--;
							break;
						case JewelMagicEffect.NightSight:
							if( ApplyNightSight(Wearer) )
								MagicCharges--;
							break;
						case JewelMagicEffect.Curse:
							if( ApplyStatEffect(Wearer, true, true, true, -10 ) )
								MagicCharges--;
							break;
						case JewelMagicEffect.Clumsy:
							if( ApplyStatEffect(Wearer, false, true, false, -10 ) )
								MagicCharges--;
							break;
						case JewelMagicEffect.Feeblemind:
							if( ApplyStatEffect(Wearer, false, false, true, -10 ) )
								MagicCharges--;
							break;
						case JewelMagicEffect.Weakness:
							if( ApplyStatEffect(Wearer, true, false, false, -10 ) )
								MagicCharges--;
							break;
						default:
							break;
					}
				}

			}
		}

		public override void OnRemoved( object parent )
		{
			/*if ( Core.AOS && parent is Mobile )
			{
				Mobile from = (Mobile)parent;

				m_AosSkillBonuses.Remove();

				string modName = this.Serial.ToString();

				from.RemoveStatMod( modName + "Str" );
				from.RemoveStatMod( modName + "Dex" );
				from.RemoveStatMod( modName + "Int" );

				from.CheckStatTimers();
			}*/
			if (!Core.AOS && parent is PlayerMobile )
			{
				PlayerMobile Wearer = (PlayerMobile)parent;

				if (m_InvisTimer != null)
				{
					if (m_InvisTimer.Running)
					{
						Wearer.RevealingAction();
						m_InvisTimer.Stop();
						m_InvisTimer = null;
					}
				}

				if (m_StatEffectTimer != null)
				{
					if (m_StatEffectTimer.Running)
					{
						string StrName = String.Format("[Magic] {0} Offset:item-{1}", StatType.Str, this.Serial);
						string IntName = String.Format("[Magic] {0} Offset:item-{1}", StatType.Int, this.Serial);
						string DexName = String.Format("[Magic] {0} Offset:item-{1}", StatType.Dex, this.Serial);

						Wearer.RemoveStatMod( StrName );
						Wearer.RemoveStatMod( IntName );
						Wearer.RemoveStatMod( DexName );
						Wearer.CheckStatTimers();
						m_StatEffectTimer.Stop();
						m_StatEffectTimer = null;
					}
				}
			}

		}

		public bool ApplyMagicReflectEffect(PlayerMobile Wearer)
		{
			if (Wearer == null)
				return false;

			Spell spell = new MagicReflectSpell(Wearer,null); 
	
			if ( Wearer.MagicDamageAbsorb > 0 )
			{
				Wearer.SendMessage("The magic of this item is already protecting you.");
				return false;
			}
			else if (Wearer.Region.OnBeginSpellCast( Wearer, spell ) == false)
			{
				Wearer.SendMessage("The magic normally within this object seems absent.");
				return false;
			}	
			else if ( !Wearer.CanBeginAction( typeof( DefensiveSpell ) ) )
			{
				Wearer.SendLocalizedMessage( 1005385 ); // The spell will not adhere to you at this time.
				return false;
			}
			else
			{
				if ( Wearer.BeginAction( typeof( DefensiveSpell ) ) )
				{
					int value = (int)((Utility.Random(51) + 50) + (Utility.Random(51) + 50)); // Random value of up to 100 for magery and up to 100 for scribing - lowest though is 50 magery/50 scribing equivalent strength
					value = (int)(8 + (value/200)*7.0);//absorb from 8 to 15 "circles"

					Wearer.MagicDamageAbsorb = value;

					Wearer.FixedParticles( 0x375A, 10, 15, 5037, EffectLayer.Waist );
					Wearer.PlaySound( 0x1E9 );
					Wearer.SendMessage("You feel the magic of the item envelope you.");
					return true;
				}
				else
				{
					Wearer.SendLocalizedMessage( 1005385 ); // The spell will not adhere to you at this time.
					return false;
				}
			}
		}

		public bool ApplyInvisibilityEffect(PlayerMobile Wearer)
		{
			Spell spell = new InvisibilitySpell(Wearer,null); 

			if (Wearer == null)
				return false;
						
			if (Wearer.Hidden == true)
			{
				// player is already invisible...do nothing
				return false;
			}
			else if (Wearer.Region.OnBeginSpellCast( Wearer, spell ) == false)
			{
				Wearer.SendMessage("The magic normally within this object seems absent.");
				return false;
			}	
			else
			{
				// hide the player, set a timer to check for additional charges or reveal
				Wearer.Hidden = true;

				if (m_InvisTimer != null)
				{
					m_InvisTimer.Stop();
					m_InvisTimer = null;
				}

				m_InvisTimer = new MagicEffectTimer (Wearer, this, TimeSpan.FromSeconds(120));

				m_InvisTimer.Start();
				return true;
			}
		}

		public bool ApplyNightSight(PlayerMobile Wearer)
		{
			Spell spell = new NightSightSpell(Wearer,null);

			if( Wearer == null )
				return false;

			if (Wearer.Region.OnBeginSpellCast( Wearer, spell ) == false)
			{
				Wearer.SendMessage("The magic normally within this object seems absent.");
				return false;
			}	
			//Pix: this was borrowed from the NightSight spell...
			else if( Wearer.BeginAction( typeof( LightCycle ) ) )
			{
				new LightCycle.NightSightTimer( Wearer ).Start();

				int level = 25;

				Wearer.LightLevel = level;

				Wearer.FixedParticles( 0x376A, 9, 32, 5007, EffectLayer.Waist );
				Wearer.PlaySound( 0x1E3 );

				return true;
			}

			return false;
		}

		public bool ApplyStatEffect(PlayerMobile Wearer, bool bStr, bool bDex, bool bInt, int change)
		{
			Spell spell = null;
		
			if( Wearer == null )
				return false;

			// Try to apply bless to all stats
			int BlessOffset = change;
			bool AppliedStr = false;
			bool AppliedInt = false;
			bool AppliedDex = false;
			if( bStr )
			{
				if(BlessOffset > 0)
				{
					spell = new StrengthSpell(Wearer, null);
				}
				else
					spell = new WeakenSpell(Wearer, null);

				if (Wearer.Region.OnBeginSpellCast( Wearer, spell ) == false)
				{
					Wearer.SendMessage("The magic normally within this object seems absent.");
					return false;
				}	

				AppliedStr = AddStatBonus(Wearer, BlessOffset, StatType.Str, TimeSpan.Zero);
			}
			if( bInt )
			{
				if(BlessOffset > 0)
				{
					spell = new CunningSpell(Wearer, null);
				}
				else
					spell = new FeeblemindSpell(Wearer, null);

				if (Wearer.Region.OnBeginSpellCast( Wearer, spell ) == false)
				{
					Wearer.SendMessage("The magic normally within this object seems absent.");
					return false;
				}	
				AppliedInt = AddStatBonus(Wearer, BlessOffset, StatType.Int, TimeSpan.Zero);
			}
		
			if( bDex )
			{
				if(BlessOffset > 0)
				{
					spell = new AgilitySpell(Wearer, null);
				}
				else
					spell = new ClumsySpell(Wearer, null);

				if (Wearer.Region.OnBeginSpellCast( Wearer, spell ) == false)
				{
					Wearer.SendMessage("The magic normally within this object seems absent.");
					return false;
				}	
				AppliedDex = AddStatBonus(Wearer, BlessOffset, StatType.Dex, TimeSpan.Zero);
			}
			Wearer.CheckStatTimers();
			// If any stats were adjusted, start timer to remove the stats after effect expires
			// return that spell was successful
			if (AppliedStr || AppliedInt || AppliedDex) /* 7/25/04 smerX */
			{
				if( m_StatEffectTimer != null )
				{
					m_StatEffectTimer.Stop();
					m_StatEffectTimer = null;
				}

				m_StatEffectTimer = new MagicEffectTimer(Wearer, this, TimeSpan.FromSeconds(120));

				m_StatEffectTimer.Start();
				return true;
			}
			else
			{
				return false;
			}
		}

		public bool AddStatBonus(Mobile target, int offset, StatType type, TimeSpan duration)
		{
			if (target == null)
				return false;

			string name = String.Format("[Magic] {0} Offset:item-{1}", type, this.Serial);
			string itemtypename = String.Format("[Magic] {0} Offset:item-", type);

			StatMod mod = target.GetStatMod( name );

			for (int i = 0; i < target.StatMods.Count; i++)
			{
				StatMod sm = target.StatMods[i] as StatMod;
				if (sm != null)
				{
					if (sm.Name.IndexOf(itemtypename) == 0)
					{
						//found this item statmod type already - don't apply
						return false;
					}
				}
			}

			// If they have a negative effect on them, replace the effect with
			// the negative effect plus the new positive effect
			if ( mod != null && mod.Offset < 0 )
			{
				target.AddStatMod( new StatMod( type, name, mod.Offset + offset, duration ) );
				return true;
			}
			// If they have no effect or the current effect is weaker than the new effect
			// Apply the new effect
			else if ( mod == null || mod.Offset < offset )
			{
				target.AddStatMod( new StatMod( type, name, offset, duration ) );
				return true;
			}
			// They already have an effect equal to or greater than the new effect
			// do nothing.
			return false;
		}

		public void SetMagic(JewelMagicEffect Effect, int Charges)
		{
			// Only allow Teleport to be set on Rings
			if (Effect == JewelMagicEffect.Teleport)
			{
				if (this is BaseRing)
				{
					m_MagicType = Effect;
					m_MagicCharges = Charges;
					Identified = false;
				}
			}
			else
			{
				m_MagicType = Effect;
				m_MagicCharges = Charges;
				Identified = false;
			}
		}

		public void SetRandomMagicEffect(int MinLevel, int MaxLevel)
		{
			if (MinLevel < 1 || MaxLevel > 3)
				return;

			int NewMagicType;

			if (this is BaseRing)
			{
				NewMagicType = Utility.RandomList((int)JewelMagicEffect.MagicReflect,
					(int)JewelMagicEffect.Invisibility,(int)JewelMagicEffect.Bless,
					(int)JewelMagicEffect.Teleport,(int)JewelMagicEffect.Agility,
					(int)JewelMagicEffect.Cunning,(int)JewelMagicEffect.Strength,
					(int)JewelMagicEffect.NightSight);
			}
			else
			{
				// no teleporting for non-rings
				NewMagicType = Utility.RandomList((int)JewelMagicEffect.MagicReflect,
					(int)JewelMagicEffect.Invisibility, (int)JewelMagicEffect.Bless,
					/*(int)JewelMagicEffect.Teleport,*/ (int)JewelMagicEffect.Agility,
					(int)JewelMagicEffect.Cunning, (int)JewelMagicEffect.Strength,
					(int)JewelMagicEffect.NightSight);
			}

			m_MagicType = (JewelMagicEffect) NewMagicType;

			int NewLevel = Utility.RandomMinMax(MinLevel, MaxLevel);
			switch (NewLevel)
			{
				case 1:
					m_MagicCharges = Utility.Random (1, 5);
					break;
				case 2:
					m_MagicCharges = Utility.Random (4, 11);
					break;
				case 3:
					m_MagicCharges = Utility.Random (9, 20);
					break;
				default:
					// should never happen
					m_MagicCharges = 0;
					break;
			}
			Identified = false;
		}

		public BaseJewel( Serial serial ) : base( serial )
		{
		}

		public override void GetProperties( ObjectPropertyList list )
		{
			base.GetProperties( list );
/*
			m_AosSkillBonuses.GetProperties( list );

			int prop;

			if ( (prop = ArtifactRarity) > 0 )
				list.Add( 1061078, prop.ToString() ); // artifact rarity ~1_val~

			if ( (prop = m_AosAttributes.WeaponDamage) != 0 )
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

			if ( (prop = m_AosAttributes.AttackChance) != 0 )
				list.Add( 1060415, prop.ToString() ); // hit chance increase ~1_val~%

			if ( (prop = m_AosAttributes.BonusHits) != 0 )
				list.Add( 1060431, prop.ToString() ); // hit point increase ~1_val~

			if ( (prop = m_AosAttributes.BonusInt) != 0 )
				list.Add( 1060432, prop.ToString() ); // intelligence bonus ~1_val~

			if ( (prop = m_AosAttributes.LowerManaCost) != 0 )
				list.Add( 1060433, prop.ToString() ); // lower mana cost ~1_val~%

			if ( (prop = m_AosAttributes.LowerRegCost) != 0 )
				list.Add( 1060434, prop.ToString() ); // lower reagent cost ~1_val~%

			if ( (prop = m_AosAttributes.Luck) != 0 )
				list.Add( 1060436, prop.ToString() ); // luck ~1_val~

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
			if (Identified == true && MagicCharges > 0)
			{
				string MagicName;
				switch(MagicType)
				{
					case JewelMagicEffect.MagicReflect:
						MagicName = "magic reflection";
						break;
					case JewelMagicEffect.Invisibility:
						MagicName = "invisibility";
						break;
					case JewelMagicEffect.Bless:
						MagicName = "bless";
						break;
					case JewelMagicEffect.Teleport:
						MagicName = "teleport";
						break;
					case JewelMagicEffect.Agility:
						MagicName = "agility";
						break;
					case JewelMagicEffect.Cunning:
						MagicName = "cunning";
						break;
					case JewelMagicEffect.Strength:
						MagicName = "strength";
						break;
					case JewelMagicEffect.NightSight:
						MagicName = "night sight";
						break;
					case JewelMagicEffect.Curse:
						MagicName = "curse";
						break;
					case JewelMagicEffect.Clumsy:
						MagicName = "clumsy";
						break;
					case JewelMagicEffect.Feeblemind:
						MagicName = "feeblemind";
						break;
					case JewelMagicEffect.Weakness:
						MagicName = "weakness";
						break;
					default:
						MagicName = "Unknown";
						break;
				}
				string MagicProp = String.Format("{0} - charges:{1}", MagicName, MagicCharges);
				list.Add(MagicProp);
			}
			else if (Identified == false && MagicCharges > 0)
				list.Add("Unidentified");

			if ( m_Crafter != null )
				list.Add( 1050043, m_Crafter.Name ); // crafted by ~1_NAME~

			if ( m_Quality == JewelQuality.Exceptional )
				list.Add( 1060636 ); // exceptional
		}

		public override void OnSingleClick( Mobile from )
		{
			if (this.HideAttributes == true)
			{
				base.OnSingleClick(from);
				return;
			}

			ArrayList attrs = new ArrayList();

			if ( DisplayLootType )
			{
				if ( LootType == LootType.Blessed )
					attrs.Add( new EquipInfoAttribute( 1038021 ) ); // blessed
				else if ( LootType == LootType.Cursed )
					attrs.Add( new EquipInfoAttribute( 1049643 ) ); // cursed
			}

			if ( m_Quality == JewelQuality.Exceptional )
				attrs.Add( new EquipInfoAttribute( 1018305 - (int)m_Quality ) );

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

			if (Identified == false && MagicCharges > 0)
				attrs.Add( new EquipInfoAttribute( 1038000 ) ); // unidentified
			else if (Identified == true && MagicCharges > 0)
			{
				switch(MagicType)
				{
					case JewelMagicEffect.MagicReflect:
						attrs.Add( new EquipInfoAttribute(1044416, m_MagicCharges) ); // magic reflection
						break;
					case JewelMagicEffect.Invisibility:
						attrs.Add( new EquipInfoAttribute(1044424, m_MagicCharges) ); // invisibility
						break;
					case JewelMagicEffect.Bless:
						attrs.Add( new EquipInfoAttribute(1044397, m_MagicCharges) ); // bless
						break;
					case JewelMagicEffect.Teleport:
						attrs.Add( new EquipInfoAttribute(1044402, m_MagicCharges) ); // teleport
						break;
					case JewelMagicEffect.Agility:
						attrs.Add( new EquipInfoAttribute(1044389, m_MagicCharges) ); // agility
						break;
					case JewelMagicEffect.Cunning:
						attrs.Add( new EquipInfoAttribute(1044390, m_MagicCharges) ); // cunning
						break;
					case JewelMagicEffect.Strength:
						attrs.Add( new EquipInfoAttribute(1044396, m_MagicCharges) ); // strength
						break;
					case JewelMagicEffect.NightSight:
						attrs.Add( new EquipInfoAttribute(1044387, m_MagicCharges) ); // night sight
						break;
					case JewelMagicEffect.Curse:
						attrs.Add( new EquipInfoAttribute(1044407, m_MagicCharges) ); // curse
						break;
					case JewelMagicEffect.Clumsy:
						attrs.Add( new EquipInfoAttribute(1044382, m_MagicCharges) ); // clumsy
						break;
					case JewelMagicEffect.Feeblemind:
						attrs.Add( new EquipInfoAttribute(1044384, m_MagicCharges) ); // feeblemind
						break;
					case JewelMagicEffect.Weakness:
						attrs.Add( new EquipInfoAttribute(1044388, m_MagicCharges) ); // weaken
						break;
				}
			}

			if ( attrs.Count == 0 && Name != null && m_Crafter == null )
				return;

			EquipmentInfo eqInfo = new EquipmentInfo( number, m_Crafter, false, (EquipInfoAttribute[])attrs.ToArray( typeof( EquipInfoAttribute ) ) );

			from.Send( new DisplayEquipmentInfo( this, eqInfo ) );
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			int version = 5;
			writer.Write( (int) version );

			writer.Write( (Mobile) m_Crafter );
			writer.Write( (short) m_Quality );

			writer.Write( (int) m_MagicType );
			writer.Write( (int) m_MagicCharges );
			writer.Write( (bool) m_Identified );

			writer.WriteEncodedInt( (int) m_Resource );
			writer.WriteEncodedInt( (int) m_GemType );

			// removed in version 4
			//m_AosAttributes.Serialize( writer );
			//m_AosResistances.Serialize( writer );
			//m_AosSkillBonuses.Serialize( writer );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch ( version )
			{
				case 5:
				{
					// erl: New "crafted by" and quality properties

					m_Crafter = reader.ReadMobile();
					m_Quality = (JewelQuality)reader.ReadShort();
					goto case 4;
				}
				case 4:
				{
					// remove AOS crap
					// see case 1 below
					goto case 3;
				}
				case 3:
				{
					m_MagicType = (JewelMagicEffect) reader.ReadInt();
					m_MagicCharges = reader.ReadInt();
					m_Identified = reader.ReadBool();

					goto case 2;
				}
				case 2:
				{
					m_Resource = (CraftResource)reader.ReadEncodedInt();
					m_GemType = (GemType)reader.ReadEncodedInt();

					goto case 1;
				}
				case 1:
				{
					// pack these out of furture versions.
					if (version < 4)
					{
						AosAttributes dmy_AosAttributes;
						AosElementAttributes dmy_AosResistances;
						AosSkillBonuses dmy_AosSkillBonuses;
						dmy_AosAttributes = new AosAttributes( this, reader );
						dmy_AosResistances = new AosElementAttributes( this, reader );
						dmy_AosSkillBonuses = new AosSkillBonuses( this, reader );

						if ( Core.AOS && Parent is Mobile )
							dmy_AosSkillBonuses.AddTo( (Mobile)Parent );

						int strBonus = dmy_AosAttributes.BonusStr;
						int dexBonus = dmy_AosAttributes.BonusDex;
						int intBonus = dmy_AosAttributes.BonusInt;

						if ( Parent is Mobile && (strBonus != 0 || dexBonus != 0 || intBonus != 0) )
						{
							Mobile m = (Mobile)Parent;

							string modName = Serial.ToString();

							if ( strBonus != 0 )
								m.AddStatMod( new StatMod( StatType.Str, modName + "Str", strBonus, TimeSpan.Zero ) );

							if ( dexBonus != 0 )
								m.AddStatMod( new StatMod( StatType.Dex, modName + "Dex", dexBonus, TimeSpan.Zero ) );

							if ( intBonus != 0 )
								m.AddStatMod( new StatMod( StatType.Int, modName + "Int", intBonus, TimeSpan.Zero ) );
						}
					}

					if ( Parent is Mobile )
						((Mobile)Parent).CheckStatTimers();

					break;
				}
				case 0:
				{
					// pack these out of furture versions.
					if (version < 4)
					{
						AosAttributes dmy_AosAttributes;
						AosElementAttributes dmy_AosResistances;
						AosSkillBonuses dmy_AosSkillBonuses;
						dmy_AosAttributes = new AosAttributes( this );
						dmy_AosResistances = new AosElementAttributes( this );
						dmy_AosSkillBonuses = new AosSkillBonuses( this );
					}

					break;
				}
			}

			if ( version < 2 )
			{
				m_Resource = CraftResource.Iron;
				m_GemType = GemType.None;
			}

			if ( version < 5 )
			{
				m_Quality = JewelQuality.Regular;
			}
		}

		private class MagicEffectTimer : Timer
		{
			private PlayerMobile m_Wearer;
			private BaseJewel m_Jewel;

			public MagicEffectTimer(PlayerMobile Wearer, BaseJewel Jewel, TimeSpan Duration) : base (Duration)
			{
				m_Wearer = Wearer;
				m_Jewel = Jewel;
			}

			protected override void OnTick()
			{
				if (m_Wearer != null)
				{
					if (m_Jewel != null)
					{
						switch(m_Jewel.MagicType)
						{
							case JewelMagicEffect.Invisibility:
								if (m_Jewel.MagicCharges > 0)
								{
									m_Wearer.Hidden = true;
									m_Jewel.MagicCharges--;
									Start();
								}
								else
								{
									Stop();
									m_Wearer.RevealingAction();
								}
								break;
							case JewelMagicEffect.Bless:
							case JewelMagicEffect.Agility:
							case JewelMagicEffect.Clumsy:
							case JewelMagicEffect.Cunning:
							case JewelMagicEffect.Curse:
							case JewelMagicEffect.Feeblemind:
							case JewelMagicEffect.Strength:
							case JewelMagicEffect.Weakness:
								if (m_Jewel.MagicCharges > 0)
								{
									m_Jewel.MagicCharges--;
									Start();
								}
								else
								{
									Stop();
									string StrName = String.Format("[Magic] {0} Offset:item-{1}", StatType.Str, m_Jewel.Serial);
									string IntName = String.Format("[Magic] {0} Offset:item-{1}", StatType.Int, m_Jewel.Serial);
									string DexName = String.Format("[Magic] {0} Offset:item-{1}", StatType.Dex, m_Jewel.Serial);

									m_Wearer.RemoveStatMod( StrName );
									m_Wearer.RemoveStatMod( IntName );
									m_Wearer.RemoveStatMod( DexName );

									m_Wearer.CheckStatTimers();
								}
								break;
							default:
								Stop();
								break;
						}
					}
				}
			}

		}
	}
}
