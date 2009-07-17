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

/* Scripts/Items/Clothing/BaseClothing.cs
 * CHANGE LOG
 * 5/2/08, Adam
 *		Update use of HideAttributes to be simpler.
 * 5/1/08, Adam
 *		Use HideAttributes filtering to hide the [Exceptional] tag.
 *	01/04/07, Pix
 *		Fixed stat-effect items.
 *	01/02/07, Pix
 *		Stat-effect magic items no longer stack.
 *  7/20/06, Rhiannon
 *		Fixed order of precedence bug when setting hitpoints of exceptional and low quality clothing during deserialization.
 *  07/20/06, Rhiannon
 *		Added special case for cloth gloves to Scissor() so they produce white cloth when undyed.
 *  06/26/06, Kit
 *		Added region spell checks to all magic clothing effects, follow region casting rules!!
 *	6/22/06, Pix
 *		Added special message in CanEquip for Outcast alignment
 *	6/15/06, Pix
 *		Clarified IOB refusal message in CanEquip.
 *	06/06/06, Pix
 *		Changes for Kin System
 *	11/10/05, erlein
 *		Removed PlayerConstructed property and added deserialization to pack out old data.
 *	10/7/05, erlein
 *		Modified deserialization so that non-exceptional, non-charged clothing is not made regular loottype.
 *	10/7/05, erlein
 *		Fixed deserialization to not include clothing with magic charges in its newbification.
 *	10/7/05, erlein
 *		Fixed clothing wear so there's a hitpoint check.
 *	10/7/05, erlein
 *		Altered clothing wear so that the piece is not destroyed but instead made tattered and unwearable upon full depletion of hitpoints.
 *	10/7/05, Adam
 *		In order to keep players from farming new characters for newbie clothes
 *		we are moving this valuable resource into the hands of crafters.
 *		Exceptionally crafted clothes are now newbied. They do however wear out.
 *	10/6/05, erlein
 *		Added ranged weapon type check in OnHit() so also performs consistently higher damage
 *		than other weapon types (along with bladed, which did originally).
 *	10/04/05, Pix
 *		Changed OnAdded for IOB item equipping to use new GetIOBName() function.
 *	2/10/05, erlein
 *		Added HitPoints, MaxHitPoints and related code.
 *	9/18/05, Adam
 *		Add Scissorable attribute
 *	7/26/05, erlein
 *		Automated removal of AoS resistance related function calls. 1 lines removed.
 *  1/15/05, Adam
 *		add serialization of the new dyable attr (version 5 of baseclothing)
 *  01/15/05, Froste
 *      Added a Dyable bool to all items, added a check for m_Dyable in the Dye override, added to deserialize for legacy items created in loot.cs
 *	01/04/05, Pix
 *		Changed IOB requirement from 36 hours to 10 days
 *	12/19/04, Adam
 *		1. In IsMagicAllowed() change BaseHat to Boots to match the possible values
 *			returned from Loot.RandomClothingOrJewelry()
 *			This fixes the "boots" returned as a dead magic item.
 *		2. In SetRandomMagicEffect() change NewMagicType to use Utility.RandomList()
 *		3. In SetRandomMagicEffect() change NewLevel to use Utility.RandomMinMax(MinLevel, MaxLevel)
 *	11/11/04, Adam
 *		Make sure m is PlayerMobile before casting!
 *  11/10/04, Froste
 *      Normalized IOB messages to lowercase, normal sentence structure
 *	11/10/04, Adam
 *		Backout IOB naming as it is now handled elsewhere (Loot)
 *	11/07/04, Darva
 *		Added display of iob type on single click.
 *	11/07/04, Pigpen
 *		Updated OnAdded and OnRemoved to reflect new mechanics of IOBSystem.
 *	11/05/04, Pigpen
 *		Added IOBAlignment prop. for IOBsystem.
 *  8/9/04 - Pixie
 *		Explicitly cleaned up timers.
 *	7/25/04 smerX
 *		A new timer is initiated onAdded
 *  7/6/04 - Pixie
 *		Added cunning, agility, strength, feeblemind, clumsy, weaken, curse, nightsight clothing
 *		Made effects stackable.
 *	05/11/2004 - Pulse
 *	Completed changes to implement magic clothing.
 *	changes include:
 *		* several new properties: magic type, number of charges, and identified flag
 *		* updated GetProperties and OnSingleClick to include magic properties
 *		* MagicEffect enumeration for various available spell types
 *		* MagicEffectTimer class to implement spell timing effects and control charge usage
 *		* an IsMagicAllowed function that runs a case statement to determine whether or not magic can be set on an item
 *			All clothing items can be made magic through the [props command for Game Master or higher access level
 *			but internal routines for setting item magic relies on IsMagicAllowed result
 *		* SetMagic and SetRandomMagicEffect to allow setting an existing clothing item to some
 *			type of magic and level
 *		* "Apply" routines for the various magic effects
 *		* an AddStatBonus routine used by the Bless effect.
 */

using System;
using System.Collections;
using Server;
using Server.Network;
using Server.Engines.Craft;
using Server.Spells;
using Server.Spells.Fifth;
using Server.Targeting;
using Server.Mobiles;
using Server.Spells.First;
using Server.Spells.Second;
using Server.Spells.Third;
using Server.Spells.Sixth;

namespace Server.Items
{
	public enum ClothingQuality
	{
		Low,
		Regular,
		Exceptional
	}

	public enum MagicEffect
	{
		None = 0,
		MagicReflect,
		Invisibility,
		Bless,
		Agility,		//4
		Cunning,		//5
		Strength,		//6
		NightSight,		//7
		Curse,			//8
		Clumsy,			//9
		Feeblemind,		//10
		Weakness,		//11
	}

	public interface IArcaneEquip
	{
		bool IsArcane { get; }
		int CurArcaneCharges { get; set; }
		int MaxArcaneCharges { get; set; }
	}

	public abstract class BaseClothing : Item, IDyable, IScissorable
	{
		private Mobile m_Crafter;
		private ClothingQuality m_Quality;
		private MagicEffect m_MagicType;
		private int m_MagicCharges;
		private bool m_Identified;
		private IOBAlignment m_IOBAlignment;	//Pigpen - Addition for IOB System
		private Timer m_StatEffectTimer;
		private Timer m_InvisTimer;
        private bool m_Dyable = true;			// Froste - Addition for better dye control
		private bool m_Scissorable = true;		// Adam - Addition for better Scissor control


		// erl: additions for clothing wear
		// ..

  		private short m_HitPoints;
		private short m_MaxHitPoints;
		
		public virtual int InitMinHits{ get{ return 40; } }
 		public virtual int InitMaxHits{ get{ return 50; } }

		// ..

        [CommandProperty(AccessLevel.GameMaster)]
		public Mobile Crafter
		{
			get { return m_Crafter; }
			set { m_Crafter = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public ClothingQuality Quality
		{
			get { return m_Quality; }
			set { m_Quality = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public MagicEffect MagicType
		{
			get { return m_MagicType; }
			set { m_MagicType = value; }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int MagicCharges
		{
			get { return m_MagicCharges; }
			set { m_MagicCharges = value; }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Identified
		{
			get { return m_Identified; }
			set { m_Identified = value; }
		}

		[CommandProperty(AccessLevel.GameMaster)]  //Pigpen - Addition for IOB System
		public IOBAlignment IOBAlignment
		{
			get { return m_IOBAlignment; }
			set { m_IOBAlignment = value; }
		}

        [CommandProperty(AccessLevel.GameMaster)] // Froste - Addition for better dye control
        public bool Dyable
        {
            get { return m_Dyable; }
            set { m_Dyable = value; }
        }

		[CommandProperty(AccessLevel.GameMaster)] // Adam - Addition for better Scissor control
		public bool Scissorable
		{
			get { return m_Scissorable; }
			set { m_Scissorable = value; }
		}

		[CommandProperty( AccessLevel.GameMaster )] // erl - for clothing wear
		public short MaxHitPoints
		{
			get{ return m_MaxHitPoints; }
			set{ m_MaxHitPoints = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)] // erl - for clothing wear
		public short HitPoints
		{
			get
			{
				return m_HitPoints;
			}
			set
			{
				if ( value != m_HitPoints && MaxHitPoints != 0 )
				{
					m_HitPoints = value;

					if ( m_HitPoints > MaxHitPoints )
						m_HitPoints = MaxHitPoints;

					InvalidateProperties();

					if ( m_HitPoints == (m_MaxHitPoints / 10) )
					{
						if ( Parent is Mobile )
							((Mobile)Parent).LocalOverheadMessage( MessageType.Regular, 0x3B2, 1061121 ); // Your equipment is severely damaged.
					}
				}
			}
		}

        public BaseClothing(int itemID, Layer layer) : this( itemID, layer, 0 )
		{
		}

		public BaseClothing(int itemID, Layer layer, int hue) : base( itemID )
		{
			Layer = layer;
			Hue = hue;
			m_Quality = ClothingQuality.Regular;
			m_MagicType = MagicEffect.None;
			m_MagicCharges = 0;
			m_Identified = true;
			m_IOBAlignment = IOBAlignment.None;		//Pigpen - Addition for IOB System
			m_InvisTimer = null;
			m_StatEffectTimer = null;
         m_Dyable = true;								//Froste - Addition for dye control
			m_Scissorable = true;						// Adam - Addition for better Scissor control
			
			// erl: added for clothing wear
			m_HitPoints = m_MaxHitPoints = (short) Utility.RandomMinMax( InitMinHits, InitMaxHits );

		}

		public BaseClothing(Serial serial) : base( serial )
		{
		}

		public override bool CheckPropertyConfliction(Mobile m)
		{
			if (base.CheckPropertyConfliction(m))
				return true;

			if (Layer == Layer.Pants)
				return (m.FindItemOnLayer(Layer.InnerLegs) != null);

			if (Layer == Layer.Shirt)
				return (m.FindItemOnLayer(Layer.InnerTorso) != null);

			return false;
		}

		public bool IsMagicAllowed()
		{
			if (this is Boots)
				return true;

			if (this is Cloak)
				return true;

			if (this is BodySash)
				return true;

			if (this is ThighBoots)
				return true;

			return false;
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			if (m_Crafter != null)
				list.Add(1050043, m_Crafter.Name); // crafted by ~1_NAME~

			if (m_Quality == ClothingQuality.Exceptional)
				list.Add(1060636); // exceptional

			if (Identified == true && MagicCharges > 0)
			{
				string MagicName;
				switch (MagicType)
				{
					case MagicEffect.MagicReflect:
						MagicName = "Magic Reflection";
						break;
					case MagicEffect.Invisibility:
						MagicName = "Invisibility";
						break;
					case MagicEffect.Bless:
						MagicName = "Bless";
						break;
					case MagicEffect.Agility:
						MagicName = "Agility";
						break;
					case MagicEffect.Cunning:
						MagicName = "Cunning";
						break;
					case MagicEffect.Strength:
						MagicName = "Strength";
						break;
					case MagicEffect.NightSight:
						MagicName = "Night Sight";
						break;
					case MagicEffect.Curse:
						MagicName = "Curse";
						break;
					case MagicEffect.Clumsy:
						MagicName = "Clumsy";
						break;
					case MagicEffect.Feeblemind:
						MagicName = "Feeblemind";
						break;
					case MagicEffect.Weakness:
						MagicName = "Weakness";
						break;
					default:
						MagicName = "Unknown";
						break;
				}
				string MagicProp = String.Format("{0} - Charges:{1}", MagicName, MagicCharges);
				list.Add(MagicProp);
			}
			else if (Identified == false && MagicCharges > 0)
				list.Add("Unidentified");
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

			if (m_Quality == ClothingQuality.Exceptional)
				attrs.Add(new EquipInfoAttribute(1018305 - (int)m_Quality));

			int number;

			if (Name == null)
			{
				number = LabelNumber;
			}
			else
			{
				this.LabelTo(from, Name);
				number = 1041000;
			}

			if (Identified == false && MagicCharges > 0)
				attrs.Add(new EquipInfoAttribute(1038000)); // unidentified
			else if (Identified == true && MagicCharges > 0)
			{
				switch (MagicType)
				{
					case MagicEffect.MagicReflect:
						attrs.Add(new EquipInfoAttribute(1044416, m_MagicCharges)); // magic reflection
						break;
					case MagicEffect.Invisibility:
						attrs.Add(new EquipInfoAttribute(1044424, m_MagicCharges)); // invisibility
						break;
					case MagicEffect.Bless:
						attrs.Add(new EquipInfoAttribute(1044397, m_MagicCharges)); // bless
						break;
					case MagicEffect.Agility:
						attrs.Add(new EquipInfoAttribute(1044389, m_MagicCharges)); // agility
						break;
					case MagicEffect.Cunning:
						attrs.Add(new EquipInfoAttribute(1044390, m_MagicCharges)); // cunning
						break;
					case MagicEffect.Strength:
						attrs.Add(new EquipInfoAttribute(1044396, m_MagicCharges)); // strength
						break;
					case MagicEffect.NightSight:
						attrs.Add(new EquipInfoAttribute(1044387, m_MagicCharges)); // night sight
						break;
					case MagicEffect.Curse:
						attrs.Add(new EquipInfoAttribute(1044407, m_MagicCharges)); // curse
						break;
					case MagicEffect.Clumsy:
						attrs.Add(new EquipInfoAttribute(1044382, m_MagicCharges)); // clumsy
						break;
					case MagicEffect.Feeblemind:
						attrs.Add(new EquipInfoAttribute(1044384, m_MagicCharges)); // feeblemind
						break;
					case MagicEffect.Weakness:
						attrs.Add(new EquipInfoAttribute(1044388, m_MagicCharges)); // weaken
						break;
				}
			}

			if (attrs.Count == 0 && Crafter == null && Name != null)
				return;

			EquipmentInfo eqInfo = new EquipmentInfo(number, m_Crafter, false, (EquipInfoAttribute[])attrs.ToArray(typeof(EquipInfoAttribute)));

			from.Send(new DisplayEquipmentInfo(this, eqInfo));
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)8);					// version
			writer.Write((short)m_HitPoints);		// erl - added for clothing wear
			writer.Write((short)m_MaxHitPoints);	// erl - added for clothing wear
			writer.Write((bool)m_Scissorable);		// Adam - Addition for better Scissor control
			writer.Write((bool)m_Dyable);			// Adam - Save the dyable attr
			writer.Write((int)m_IOBAlignment);		// Pigpen - Addition for IOB System
			writer.Write((int)m_MagicType);
			writer.Write((int)m_MagicCharges);
			writer.Write((bool)m_Identified);

			writer.Write((Mobile)m_Crafter);
			writer.Write((int)m_Quality);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 8: //erl - added to handle packing out of PlayerConstructed property
				{
                	goto case 7;
				}
				case 7: //erl - added for clothing wear
				{
					m_HitPoints = reader.ReadShort();
					m_MaxHitPoints = reader.ReadShort();
					goto case 6;
				}
				case 6: //Adam - Addition for Scissorable attribute
				{
					m_Scissorable = reader.ReadBool();
					goto case 5;
				}
				case 5: //Adam - Addition for Dyable attribute
				{
					m_Dyable = reader.ReadBool();
					goto case 4;
				}
				case 4: //Pigpen - Addition for IOB System
				{
					m_IOBAlignment = (IOBAlignment)reader.ReadInt();
					goto case 3;
				}
				case 3:
				{
					m_MagicType = (MagicEffect)reader.ReadInt();
					m_MagicCharges = reader.ReadInt();
					m_Identified = reader.ReadBool();
                    goto case 2;
				}
				case 2:
				{
					// erl: this is the old PlayerConstructed flag, which will no longer
					// exist for anything over version 7... made obsolete by PlayerCrafted

					if( version < 8 )
						PlayerCrafted = reader.ReadBool();

					goto case 1;
				}
				case 1:
				{
					m_Crafter = reader.ReadMobile();
					m_Quality = (ClothingQuality)reader.ReadInt();
					break;
				}
				case 0:
				{
					m_Crafter = null;
					m_Quality = ClothingQuality.Regular;
					break;
				}
			}

			if (version < 5)				// Adam - addition for dye control
			{								// Allow for other non-dyable clothes outside the IOB system
				if (m_IOBAlignment != IOBAlignment.None)
					m_Dyable = false;
			}

			if (version < 7)
			{
			   // erl: this pre-dates hit point additions, so calculate values
			   // ..

			   // Check the quality of the piece. If it's exceptional or low, we want
			   // the piece's hitpoint to reflect this

				int iMax = InitMaxHits;
				int iMin = InitMinHits;

  			   if( Quality == ClothingQuality.Exceptional )
				{
				   // Add 50% to both

				   iMax = ( iMax * 3 ) / 2; // Fixed order of precedence bug
				   iMin = ( iMin * 3 ) / 2;
			    }
				else if(Quality == ClothingQuality.Low )
				{
				   // Lose 20% to both

				   iMax = ( iMax * 4) / 5; // Fixed order of precedence bug
				   iMin = ( iMin * 4) / 5;
			   }

        		m_HitPoints = m_MaxHitPoints = (short) Utility.RandomMinMax( iMin, iMax );

  			}

			// adam: To keep players from farming new characters for newbie clothes
			//	we are moving this valuable resource into the hands of crafters.
			if (version <= 7)
			{
				if( Quality == ClothingQuality.Exceptional && MagicCharges == 0 )
				{
					// make exceptional clothes newbied
					LootType = LootType.Newbied;
				}
				else if( MagicCharges > 0 )
				{
					// erl: explicitly change these pieces so they aren't newbied
					LootType = LootType.Regular;
				}
			}

		}

		public override void OnAdded(object parent)
		{
			base.OnAdded(parent);

			if (parent is PlayerMobile)
			{
				PlayerMobile Wearer = (PlayerMobile)parent;
				if (this.IOBAlignment != IOBAlignment.None)
				{
					Wearer.OnEquippedIOBItem( this.IOBAlignment );
				}

				// if charges > 0 apply the magic effect
				if (MagicCharges > 0)
				{
					// apply magic effect to wearer
					switch (MagicType)
					{
						case MagicEffect.MagicReflect:
							if (ApplyMagicReflectEffect(Wearer))
								MagicCharges--;
							break;
						case MagicEffect.Invisibility:
							if (ApplyInvisibilityEffect(Wearer))
								MagicCharges--;
							break;
						case MagicEffect.Bless:
							//if( ApplyBlessEffect(Wearer) )
							if (ApplyStatEffect(Wearer, true, true, true, 10))
								MagicCharges--;
							break;
						case MagicEffect.Agility:
							if (ApplyStatEffect(Wearer, false, true, false, 10))
								MagicCharges--;
							break;
						case MagicEffect.Cunning:
							if (ApplyStatEffect(Wearer, false, false, true, 10))
								MagicCharges--;
							break;
						case MagicEffect.Strength:
							if (ApplyStatEffect(Wearer, true, false, false, 10))
								MagicCharges--;
							break;
						case MagicEffect.NightSight:
							if (ApplyNightSight(Wearer))
								MagicCharges--;
							break;
						case MagicEffect.Curse:
							if (ApplyStatEffect(Wearer, true, true, true, -10))
								MagicCharges--;
							break;
						case MagicEffect.Clumsy:
							if (ApplyStatEffect(Wearer, false, true, false, -10))
								MagicCharges--;
							break;
						case MagicEffect.Feeblemind:
							if (ApplyStatEffect(Wearer, false, false, true, -10))
								MagicCharges--;
							break;
						case MagicEffect.Weakness:
							if (ApplyStatEffect(Wearer, true, false, false, -10))
								MagicCharges--;
							break;
						default:
							break;
					}
				}
			}
		}

		public override void OnRemoved(object parent)
		{
			if (parent is PlayerMobile)
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

				if (this.IOBAlignment != IOBAlignment.None) //Pigpen - Addition for IOB System
				{
					((PlayerMobile)parent).IOBEquipped = false;
				}

				if (m_StatEffectTimer != null)
				{
					if (m_StatEffectTimer.Running)
					{
						string StrName = String.Format("[Magic] {0} Offset:item-{1}", StatType.Str, this.Serial);
						string IntName = String.Format("[Magic] {0} Offset:item-{1}", StatType.Int, this.Serial);
						string DexName = String.Format("[Magic] {0} Offset:item-{1}", StatType.Dex, this.Serial);

						Wearer.RemoveStatMod(StrName);
						Wearer.RemoveStatMod(IntName);
						Wearer.RemoveStatMod(DexName);

						Wearer.CheckStatTimers();

						m_StatEffectTimer.Stop();
						m_StatEffectTimer = null;
					}
				}
			}

		}


		// erl: Added to control clothing wear

		public virtual void OnHit(BaseWeapon weapon)
		{
         // 25% chance to lower durability

			if ( 25 > Utility.Random( 100 ) )
			{
  			   // Clothing is more susceptible to slashing weapons than
			   // any other and wouldn't be affected by bashing weapons
			   // at all
				int deduction = 0;

				if ( weapon.Type == WeaponType.Slashing || weapon.Type == WeaponType.Ranged )
					deduction = 1;
				else if( weapon.Type != WeaponType.Bashing && weapon.Type != WeaponType.Fists )  // fists + bashing weapons, which would not damage clothes
					deduction = Utility.Random( 2 );

	  			HitPoints -= (short) deduction;
			}
		}
		
		// erl: Added to prevent worn out clothing from being equipped

		public override bool OnEquip( Mobile from )
		{
			if( HitPoints == 0 )
			{
				from.SendMessage("You feel that this clothing is too tattered to be worn.");
				return false;
			}
			else
			{
				return true;
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

		public override bool CanEquip(Mobile m)
		{
			if (!base.CanEquip(m))
				return false;

			if ((m != null) && (m is PlayerMobile))
			{
				PlayerMobile pm = (PlayerMobile)m;

				if (this.IOBAlignment != IOBAlignment.None)
				{
					if (pm.IOBEquipped == true)
					{
						pm.SendMessage("You cannot equip more than one item of brethren at a time.");
						return false;
					}
					if( pm.IOBAlignment != this.IOBAlignment )
					{
						if( pm.IOBAlignment == IOBAlignment.None )
						{
							pm.SendMessage( "You cannot equip a kin item without your guild aligning itself to a kin." );
						}
						else if( pm.IOBAlignment == IOBAlignment.OutCast )
						{
							pm.SendMessage( "You cannot equip a kin item while you are outcast from your kin." );
						}
						else
						{
							pm.SendMessage( "You cannot equip items of another kin." );
						}
						return false;
					}
				}
			}
			return true;
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

			StatMod mod = target.GetStatMod(name);

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
			if (mod != null && mod.Offset < 0)
			{
				target.AddStatMod(new StatMod(type, name, mod.Offset + offset, duration));
				return true;
			}
			// If they have no effect or the current effect is weaker than the new effect
				// Apply the new effect
			else if (mod == null || mod.Offset < offset)
			{
				target.AddStatMod(new StatMod(type, name, offset, duration));
				return true;
			}
			// They already have an effect equal to or greater than the new effect
			// do nothing.
			return false;
		}

		public void SetMagic(MagicEffect Effect, int Charges)
		{
			if (IsMagicAllowed())
			{
				m_MagicType = Effect;
				m_MagicCharges = Charges;
				Identified = false;
			}
		}

		public void SetRandomMagicEffect(int MinLevel, int MaxLevel)
		{
			if (IsMagicAllowed())
			{
				if (MinLevel < 1 || MaxLevel > 3)
					return;

				int NewMagicType;

				// list all supported magic types
				NewMagicType = Utility.RandomList((int)MagicEffect.MagicReflect, (int)MagicEffect.Invisibility, (int)MagicEffect.Bless, (int)MagicEffect.Agility, (int)MagicEffect.Cunning, (int)MagicEffect.Strength, (int)MagicEffect.NightSight);
				m_MagicType = (MagicEffect)NewMagicType;

				int NewLevel = Utility.RandomMinMax(MinLevel, MaxLevel);
				switch (NewLevel)
				{
					case 1:
						m_MagicCharges = Utility.Random(1, 5);
						break;
					case 2:
						m_MagicCharges = Utility.Random(4, 11);
						break;
					case 3:
						m_MagicCharges = Utility.Random(9, 20);
						break;
					default:
						// should never happen
						m_MagicCharges = 0;
						break;
				}
				Identified = false;
			}
		}


		public virtual bool Dye(Mobile from, DyeTub sender)
		{
			if (Deleted)
				return false;
			else if (RootParent is Mobile && from != RootParent)
				return false;

            if (m_Dyable == false) // Added check for new Dyable bool
            {
                from.SendLocalizedMessage(sender.FailMessage);
                return false;
            }
            else
            {
                Hue = sender.DyedHue;

                return true;
            }
        }

		public bool Scissor(Mobile from, Scissors scissors)
		{
			if (!IsChildOf(from.Backpack))
			{
				from.SendLocalizedMessage(502437); // Items you wish to cut must be in your backpack.
				return false;
			}

			CraftSystem system = DefTailoring.CraftSystem;

			CraftItem item = system.CraftItems.SearchFor(GetType());

			if (item != null && m_Scissorable && item.Ressources.Count == 1 && item.Ressources.GetAt(0).Amount >= 2)
			{
				try
				{
					Type resourceType = null;

					if (this is BaseShoes)
					{
						CraftResourceInfo info = CraftResources.GetInfo(((BaseShoes)this).Resource);

						if (info != null && info.ResourceTypes.Length > 0)
							resourceType = info.ResourceTypes[0];
					}

					// Undyed cloth gloves, when cut, produce 0-hued cloth.
					if ( this is ClothGloves && this.Hue == 1001 )
						this.Hue = 0;

					if (resourceType == null)
						resourceType = item.Ressources.GetAt(0).ItemType;

					Item res = (Item)Activator.CreateInstance(resourceType);


					ScissorHelper(from, res, PlayerCrafted ? (item.Ressources.GetAt(0).Amount / 2) : 1);

					res.LootType = LootType.Regular;

					return true;
				}
				catch (Exception ex) { EventSink.InvokeLogException(new LogExceptionEventArgs(ex)); }
			}

			from.SendLocalizedMessage(502440); // Scissors can not be used on that to produce anything.
			return false;
		}


		private class MagicEffectTimer : Timer
		{
			private PlayerMobile m_Wearer;
			private BaseClothing m_Clothing;

			public MagicEffectTimer(PlayerMobile Wearer, BaseClothing Clothing, TimeSpan Duration) : base (Duration)
			{
				m_Wearer = Wearer;
				m_Clothing = Clothing;
			}

			protected override void OnTick()
			{
				if (m_Wearer != null)
				{
					if (m_Clothing != null)
					{
						switch (m_Clothing.MagicType)
						{
							case MagicEffect.Invisibility:
								if (m_Clothing.MagicCharges > 0)
								{
									m_Wearer.Hidden = true;
									m_Clothing.MagicCharges--;
									Start();
								}
								else
								{
									Stop();
									m_Wearer.RevealingAction();
								}
								break;
							case MagicEffect.Bless:
							case MagicEffect.Agility:
							case MagicEffect.Clumsy:
							case MagicEffect.Cunning:
							case MagicEffect.Curse:
							case MagicEffect.Feeblemind:
							case MagicEffect.Strength:
							case MagicEffect.Weakness:
								if (m_Clothing.MagicCharges > 0)
								{
									m_Clothing.MagicCharges--;
									Start();
								}
								else
								{
									Stop();
									string StrName = String.Format("[Magic] {0} Offset:item-{1}", StatType.Str, m_Clothing.Serial);
									string IntName = String.Format("[Magic] {0} Offset:item-{1}", StatType.Int, m_Clothing.Serial);
									string DexName = String.Format("[Magic] {0} Offset:item-{1}", StatType.Dex, m_Clothing.Serial);

									m_Wearer.RemoveStatMod(StrName);
									m_Wearer.RemoveStatMod(IntName);
									m_Wearer.RemoveStatMod(DexName);

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
