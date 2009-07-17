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

/* Scripts/Items/Armor/BaseArmor.cs
 * ChangeLog
 * 5/2/08, Adam
 *		Update use of HideAttributes to be simpler.
 * 5/1/08, Adam
 *		Use HideAttributes filtering to hide the [Exceptional] tag.
 *  6/1/07, Adam
 *      Add check for new item.HideAttributes bool for suppressing display attributes
 *	03/23/07, Pix
 *		Addressed the 'greyed out' on singleclick thing with oldschool labeled armor.
 *		Re-added new type display of attributes for named armor.
 *	6/22/06, Pix
 *		Added special message in CanEquip for Outcast alignment
 *	6/15/06, Pix
 *		Clarified IOB refusal message in CanEquip.
 *	06/06/06, Pix
 *		Changes for Kin System
 *	11/10/05, erlein
 *		Removed PlayerConstructed property and added deserialization to pack out old data.
 *  10/10/05 TK
 *		Changed some ints to doubles for more of a floating-point math pipeline.
 *  10/08/05 Taran Kain
 *		Made DexBonus dependant on resource type
 *		Changed calculated dex bonus to be dependant on wearer's strength
 *		Added hook to Mobile.StatChange event
 *	10/04/05, Pix
 *		Changed OnAdded for IOB item equipping to use new GetIOBName() function.
 *	7/26/05, erlein
 *		Automated removal of AoS resistance related function calls. 15 lines removed.
 *	7/26/05, Adam
 *		Massive AOS cleanout
 *	2/18/05, Pixie
 *		Change to use ArmorRatingScaled instead of ArmorRating in OnHit()
 *	2/16/05, Pixie
 *		Tweaks to make armor work in 1.0.0
 *  02/15/05, Pixie
 *		CHANGED FOR RUNUO 1.0.0 MERGE.
 *  02/13/05, Pix
 *		Fixed potential bad cast in CanEquip().
 *	01/04/05, Pix
 *		Changed IOB requirement from 36 hours to 10 days
 *  11/10/04, Froste
 *      Normalized IOB messages to lowercase, normal sentence structure
 *	11/07/04, Pigpen
 *		Updated OnAdded and OnRemoved to reflect new mechanics of IOBSystem.
 *	11/05/04, Pigpen
 *		Added IOBAlignment prop. for IOBsystem.
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 *	26,may,04 - changes made by Old Salty:
 *		commented lines 638-651 to remove armor rating bonus
 *		added lines 480-485 to give leather types a durability bonus
 *	18,march,04 edited lines 442-457 for durabily change
 *	18,march,04 edited lines 611-621 for ar changes
 *	23,march,04 uploaded
 */

using System;
using System.Collections;
using Server;
using Server.Network;
using Server.Engines.Craft;
using Server.Mobiles;
using Server.Targeting;
using AMA = Server.Items.ArmorMeditationAllowance;
using AMT = Server.Items.ArmorMaterialType;
using ABT = Server.Items.ArmorBodyType;

namespace Server.Items
{
	public abstract class BaseArmor : Item, /*IArmor,*/ IScissorable
	{
		/* Armor internals work differently now (Jun 19 2003)
		 * 
		 * The attributes defined below default to -1.
		 * If the value is -1, the corresponding virtual 'Aos/Old' property is used.
		 * If not, the attribute value itself is used. Here's the list:
		 *  - ArmorBase
		 *  - StrBonus
		 *  - DexBonus
		 *  - IntBonus
		 *  - StrReq
		 *  - DexReq
		 *  - IntReq
		 *  - MeditationAllowance
		 */

		// Instance values. These values must are unique to each armor piece.
		private int m_MaxHitPoints;
		private int m_HitPoints;
		private Mobile m_Crafter;
		private ArmorQuality m_Quality;
		private ArmorDurabilityLevel m_Durability;
		private ArmorProtectionLevel m_Protection;
		private CraftResource m_Resource;
		private IOBAlignment m_IOBAlignment; //Pigpen - Addition for IOB System
		private bool m_Identified;
		//private int m_PhysicalBonus, m_FireBonus, m_ColdBonus, m_PoisonBonus, m_EnergyBonus;

		// Adam: obsolete with version 8
		//private AosAttributes m_AosAttributes;
		//private AosArmorAttributes m_AosArmorAttributes;
		//private AosSkillBonuses m_AosSkillBonuses;

		// Overridable values. These values are provided to override the defaults which get defined in the individual armor scripts.
		private int m_ArmorBase = -1;
		private double m_StrBonus = -1, m_DexBonus = -1, m_IntBonus = -1;
		private int m_StrReq = -1, m_DexReq = -1, m_IntReq = -1;
		private AMA m_Meditate = (AMA)(-1);


		public virtual bool AllowMaleWearer{ get{ return true; } }
		public virtual bool AllowFemaleWearer{ get{ return true; } }

		public abstract AMT MaterialType{ get; }

		public virtual int RevertArmorBase{ get{ return ArmorBase; } }
		public virtual int ArmorBase{ get{ return 0; } }

		public virtual AMA DefMedAllowance{ get{ return AMA.None; } }
		public virtual AMA AosMedAllowance{ get{ return DefMedAllowance; } }
		public virtual AMA OldMedAllowance{ get{ return DefMedAllowance; } }


		public virtual int AosStrBonus{ get{ return 0; } }
		public virtual int AosDexBonus{ get{ return 0; } }
		public virtual int AosIntBonus{ get{ return 0; } }
		public virtual int AosStrReq{ get{ return 0; } }
		public virtual int AosDexReq{ get{ return 0; } }
		public virtual int AosIntReq{ get{ return 0; } }


		public virtual int OldStrBonus{ get{ return 0; } }
		public virtual int OldDexBonus{ get{ return 0; } }
		public virtual int OldIntBonus{ get{ return 0; } }
		public virtual int OldStrReq{ get{ return 0; } }
		public virtual int OldDexReq{ get{ return 0; } }
		public virtual int OldIntReq{ get{ return 0; } }

		// Core Management Console variable
		public static int StrFactor = 30;

		[CommandProperty( AccessLevel.GameMaster )]
		public AMA MeditationAllowance
		{
			get{ return ( m_Meditate == (AMA)(-1) ? Core.AOS ? AosMedAllowance : OldMedAllowance : m_Meditate ); }
			set{ m_Meditate = value; }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int BaseArmorRating
		{
			get{ return ( m_ArmorBase == -1 ? ArmorBase : m_ArmorBase ); }
			set{ m_ArmorBase = value; Invalidate(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public double StrBonus
		{
			get{ return ( m_StrBonus == -1 ? Core.AOS ? AosStrBonus : OldStrBonus : m_StrBonus ); }
			set{ m_StrBonus = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public double DexBonus
		{
			get
			{
				double bonus = ( m_DexBonus == -1 ? OldDexBonus : m_DexBonus );
				if (m_Resource >= CraftResource.Iron && m_Resource <= CraftResource.Valorite)
					return bonus + (Math.Abs(bonus) * ((int)m_Resource - 1) / 16.0);
				else
					return bonus;
			}
			set{ m_DexBonus = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public double IntBonus
		{
			get{ return ( m_IntBonus == -1 ? Core.AOS ? AosIntBonus : OldIntBonus : m_IntBonus ); }
			set{ m_IntBonus = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int StrRequirement
		{
			get{ return ( m_StrReq == -1 ? Core.AOS ? AosStrReq : OldStrReq : m_StrReq ); }
			set{ m_StrReq = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int DexRequirement
		{
			get{ return ( m_DexReq == -1 ? Core.AOS ? AosDexReq : OldDexReq : m_DexReq ); }
			set{ m_DexReq = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int IntRequirement
		{
			get{ return ( m_IntReq == -1 ? Core.AOS ? AosIntReq : OldIntReq : m_IntReq ); }
			set{ m_IntReq = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public bool Identified
		{
			get{ return m_Identified; }
			set{ m_Identified = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]  //Pigpen - Addition for IOB System
		public IOBAlignment IOBAlignment
		{
			get{ return m_IOBAlignment; }
			set{ m_IOBAlignment = value; }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public CraftResource Resource
		{
			get
			{
				return m_Resource;
			}
			set
			{
				if ( m_Resource != value )
				{
					UnscaleDurability();

					m_Resource = value;
					Hue = CraftResources.GetHue( m_Resource );

					Invalidate();
					InvalidateProperties();


					ScaleDurability();
				}
			}
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int MaxHitPoints
		{
			get{ return m_MaxHitPoints; }
			set{ m_MaxHitPoints = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int HitPoints
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

					if ( m_HitPoints <= 0 )
						Delete();
					else if ( m_HitPoints > MaxHitPoints )
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


		[CommandProperty( AccessLevel.GameMaster )]
		public Mobile Crafter
		{
			get{ return m_Crafter; }
			set{ m_Crafter = value; InvalidateProperties(); }
		}

		
		[CommandProperty( AccessLevel.GameMaster )]
		public ArmorQuality Quality
		{
			get{ return m_Quality; }
			set{ UnscaleDurability(); m_Quality = value; Invalidate(); InvalidateProperties(); ScaleDurability(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public ArmorDurabilityLevel Durability
		{
			get{ return m_Durability; }
			set{ UnscaleDurability(); m_Durability = value; ScaleDurability(); InvalidateProperties(); }
		}

		public virtual int ArtifactRarity
		{
			get{ return 0; }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public ArmorProtectionLevel ProtectionLevel
		{
			get
			{
				return m_Protection;
			}
			set
			{
				if ( m_Protection != value )
				{
					m_Protection = value;

					Invalidate();
					InvalidateProperties();

				}
			}
		}
/*
		[CommandProperty( AccessLevel.GameMaster )]
		public AosAttributes Attributes
		{
			get{ return m_AosAttributes; }
			set{}
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public AosArmorAttributes ArmorAttributes
		{
			get{ return m_AosArmorAttributes; }
			set{}
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public AosSkillBonuses SkillBonuses
		{
			get{ return m_AosSkillBonuses; }
			set{}
		}
*/
		public int ComputeStatReq( StatType type )
		{
			int v;

			if ( type == StatType.Str )
				v = StrRequirement;
			else if ( type == StatType.Dex )
				v = DexRequirement;
			else
				v = IntRequirement;

			return AOS.Scale( v, 100 - GetLowerStatReq() );
		}

		public double ComputeStatBonus( StatType type, Mobile wearer )
		{
			// BE CAREFUL. Do NOT make bonuses interdependant on each other!!! Doing so will cause an infinite recursion loop in StatChange event.
			// ie when calculating DexBonus, you can use Str or Int but using Dex will cause a loop.
			// using Str in calc'ing DexBonus and ALSO using Dex in calc'ing StrBonus will also cause a loop.
			if ( type == StatType.Str )
				return StrBonus;/* + Attributes.BonusStr;*/
			else if ( type == StatType.Dex )
			{
				int strdelt = wearer.Str - StrRequirement;
				if (strdelt > 40)
					strdelt = 40;
				if (strdelt < 0)
					strdelt = 0;
				return DexBonus - (Math.Abs(DexBonus) * ((double)StrFactor - strdelt) / 40.0);
			}
			else
				return IntBonus;/* + Attributes.BonusInt;*/
		}

		/*
		[CommandProperty( AccessLevel.GameMaster )]
		public int PhysicalBonus{ get{ return m_PhysicalBonus; } set{ m_PhysicalBonus = value; InvalidateProperties(); } }

		[CommandProperty( AccessLevel.GameMaster )]
		public int FireBonus{ get{ return m_FireBonus; } set{ m_FireBonus = value; InvalidateProperties(); } }

		[CommandProperty( AccessLevel.GameMaster )]
		public int ColdBonus{ get{ return m_ColdBonus; } set{ m_ColdBonus = value; InvalidateProperties(); } }

		[CommandProperty( AccessLevel.GameMaster )]
		public int PoisonBonus{ get{ return m_PoisonBonus; } set{ m_PoisonBonus = value; InvalidateProperties(); } }

		[CommandProperty( AccessLevel.GameMaster )]
		public int EnergyBonus{ get{ return m_EnergyBonus; } set{ m_EnergyBonus = value; InvalidateProperties(); } }
*/

/*
*/
		public virtual int InitMinHits{ get{ return 0; } }
		public virtual int InitMaxHits{ get{ return 0; } }

		[CommandProperty( AccessLevel.GameMaster )]
		public ArmorBodyType BodyPosition
		{
			get
			{
				switch ( this.Layer )
				{
					default:
					case Layer.Neck:		return ArmorBodyType.Gorget;
					case Layer.TwoHanded:	return ArmorBodyType.Shield;
					case Layer.Gloves:		return ArmorBodyType.Gloves;
					case Layer.Helm:		return ArmorBodyType.Helmet;
					case Layer.Arms:		return ArmorBodyType.Arms;

					case Layer.InnerLegs:
					case Layer.OuterLegs:
					case Layer.Pants:		return ArmorBodyType.Legs;

					case Layer.InnerTorso:
					case Layer.OuterTorso:
					case Layer.Shirt:		return ArmorBodyType.Chest;
				}
			}
		}

		public void DistributeBonuses( int amount )
		{
			/*
			for ( int i = 0; i < amount; ++i )
			{
				switch ( Utility.Random( 5 ) )
				{
					case 0: ++m_PhysicalBonus; break;
					case 1: ++m_FireBonus; break;
					case 2: ++m_ColdBonus; break;
					case 3: ++m_PoisonBonus; break;
					case 4: ++m_EnergyBonus; break;
				}
			}

			InvalidateProperties();
			*/
		}

		public CraftAttributeInfo GetResourceAttrs()
		{
			CraftResourceInfo info = CraftResources.GetInfo( m_Resource );

			if ( info == null )
				return CraftAttributeInfo.Blank;

			return info.AttributeInfo;
		}

		public int GetProtOffset()
		{
			switch ( m_Protection )
			{
				case ArmorProtectionLevel.Guarding: return 1;
				case ArmorProtectionLevel.Hardening: return 2;
				case ArmorProtectionLevel.Fortification: return 3;
				case ArmorProtectionLevel.Invulnerability: return 4;
			}

			return 0;
		}

		public void UnscaleDurability()
		{
			int scale = 100 + GetDurabilityBonus();

			m_HitPoints = (m_HitPoints * 100) / scale;
			m_MaxHitPoints = (m_MaxHitPoints * 100) / scale;
			InvalidateProperties();
		}

		public void ScaleDurability()
		{
			int scale = 100 + GetDurabilityBonus();

			m_HitPoints = (m_HitPoints * scale) / 100;
			m_MaxHitPoints = (m_MaxHitPoints * scale) / 100;
			InvalidateProperties();
		}

		public int GetDurabilityBonus()
		{
			int bonus = 0;

			if ( m_Quality == ArmorQuality.Exceptional )
				bonus += 20;
			if ( m_Resource == CraftResource.Valorite ) //Changed for durabbility by Sam
				bonus += 120;
			if ( m_Resource == CraftResource.Verite ) //Changed for durabbility by Sam
				bonus += 100;
			if ( m_Resource == CraftResource.Agapite ) //Changed for durabbility by Sam
				bonus += 70;
			if ( m_Resource == CraftResource.Gold ) //Changed for durabbility by Sam
				bonus += 50;
			if ( m_Resource == CraftResource.Bronze ) //Changed for durabbility by Sam
				bonus += 20;
			if ( m_Resource == CraftResource.Copper ) //Changed for durabbility by Sam
				bonus += 15;
			if ( m_Resource == CraftResource.ShadowIron ) //Changed for durabbility by Sam
				bonus += 10;
			if ( m_Resource == CraftResource.DullCopper ) //Changed for durabbility by Sam
				bonus += 5;
			if ( m_Resource == CraftResource.SpinedLeather ) //added by OldSalty
				bonus += 20;
			if ( m_Resource == CraftResource.HornedLeather ) //added by OldSalty
				bonus += 40;
			if ( m_Resource == CraftResource.BarbedLeather ) //added by OldSalty
				bonus += 60;
			
			
			switch ( m_Durability )
			{
				case ArmorDurabilityLevel.Durable: bonus += 20; break;
				case ArmorDurabilityLevel.Substantial: bonus += 50; break;
				case ArmorDurabilityLevel.Massive: bonus += 70; break;
				case ArmorDurabilityLevel.Fortified: bonus += 100; break;
				case ArmorDurabilityLevel.Indestructible: bonus += 120; break;
			}
/*
			if ( Core.AOS )
			{
				bonus += m_AosArmorAttributes.DurabilityBonus;

				CraftResourceInfo resInfo = CraftResources.GetInfo( m_Resource );
				CraftAttributeInfo attrInfo = null;

				if ( resInfo != null )
					attrInfo = resInfo.AttributeInfo;

				if ( attrInfo != null )
					bonus += attrInfo.ArmorDurability;
			}
*/
			return bonus;
		}

		public bool Scissor( Mobile from, Scissors scissors )
		{
			if ( !IsChildOf( from.Backpack ) )
			{
				from.SendLocalizedMessage( 502437 ); // Items you wish to cut must be in your backpack.
				return false;
			}

			CraftSystem system = DefTailoring.CraftSystem;

			CraftItem item = system.CraftItems.SearchFor( GetType() );

			if ( item != null && item.Ressources.Count == 1 && item.Ressources.GetAt( 0 ).Amount >= 2 )
			{
				try
				{
					Item res = (Item)Activator.CreateInstance( CraftResources.GetInfo( m_Resource ).ResourceTypes[0] );

					ScissorHelper( from, res, PlayerCrafted ? (item.Ressources.GetAt( 0 ).Amount / 2) : 1 );
					return true;
				}
				catch (Exception ex) { EventSink.InvokeLogException(new LogExceptionEventArgs(ex)); }
			}

			from.SendLocalizedMessage( 502440 ); // Scissors can not be used on that to produce anything.
			return false;
		}

		public static void ValidateMobile( Mobile m )
		{
			for ( int i = m.Items.Count - 1; i >= 0; --i )
			{
				if ( i >= m.Items.Count )
					continue;

				Item item = (Item)m.Items[i];

				if ( item is BaseArmor )
				{
					BaseArmor armor = (BaseArmor)item;

					if ( !armor.AllowMaleWearer && m.Body.IsMale && m.AccessLevel < AccessLevel.GameMaster )
					{
						if ( armor.AllowFemaleWearer )
							m.SendLocalizedMessage( 1010388 ); // Only females can wear this.
						else
							m.SendMessage( "You may not wear this." );

						m.AddToBackpack( armor );
					}
					else if ( !armor.AllowFemaleWearer && m.Body.IsFemale && m.AccessLevel < AccessLevel.GameMaster )
					{
						if ( armor.AllowMaleWearer )
							m.SendMessage( "Only males can wear this." );
						else
							m.SendMessage( "You may not wear this." );

						m.AddToBackpack( armor );
					}
				}
			}
		}

		public int GetLowerStatReq()
		{
			if ( !Core.AOS )
				return 0;
return 0;/*
			int v = m_AosArmorAttributes.LowerStatReq;

			CraftResourceInfo info = CraftResources.GetInfo( m_Resource );

			if ( info != null )
			{
				CraftAttributeInfo attrInfo = info.AttributeInfo;

				if ( attrInfo != null )
					v += attrInfo.ArmorLowerRequirements;
			}

			if ( v > 100 )
				v = 100;

			return v;*/
		}

		public override void OnAdded( object parent )
		{
			if ( parent is PlayerMobile )
			{
				PlayerMobile Wearer = (PlayerMobile)parent;
				if (this.IOBAlignment != IOBAlignment.None)
				{
					Wearer.OnEquippedIOBItem( this.IOBAlignment );
				}
			}
		
			if ( parent is Mobile )
			{
				Mobile from = (Mobile)parent;

				//if ( Core.AOS )
					//m_AosSkillBonuses.AddTo( from );

				from.Delta( MobileDelta.Armor ); // Tell them armor rating has changed
			}
		}

		public virtual double ArmorRating
		{
			get
			{
				int ar = ArmorBase;

				if ( m_Protection != ArmorProtectionLevel.Regular )
					ar += 10 + (5 * (int)m_Protection);

				#region OLD adjustments for resource type
//				switch ( m_Resource )
//				{
//					case CraftResource.DullCopper:		ar += 1; break;//changed by sam for armor value
//					case CraftResource.ShadowIron:		ar += 2; break;//changed by sam for armor value
//					case CraftResource.Copper:			ar += 4; break;//changed by sam for armor value
//					case CraftResource.Bronze:			ar += 6; break;//changed by sam for armor value
//					case CraftResource.Gold:			ar += 8; break;//changed by sam for armor value
//					case CraftResource.Agapite:			ar += 12; break;//changed by sam for armor value
//					case CraftResource.Verite:			ar += 16; break;//changed by sam for armor value
//					case CraftResource.Valorite:		ar += 20; break;//changed by sam for armor value
//					case CraftResource.SpinedLeather:	ar += 2; break;//changed by sam for armor value
//					case CraftResource.HornedLeather:	ar += 8; break;//changed by sam for armor value
//					case CraftResource.BarbedLeather:	ar += 12; break;//changed by sam for armor value
//				}
				#endregion

				ar += -8 + (8 * (int)m_Quality);

				return ar;
			}
		}

		public double ArmorRatingScaled
		{
			get
			{
				return ( ArmorRating * ArmorScalar );
			}
		}

		public virtual double ArmorScalar
		{
			get
			{
				int pos = (int)BodyPosition;

				if ( pos >= 0 && pos < m_ArmorScalars.Length )
					return m_ArmorScalars[pos];

				return 1.0;
			}
		}

		private static double[] m_ArmorScalars = { 0.07, 0.07, 0.14, 0.15, 0.22, 0.35 };

		public static double[] ArmorScalars
		{
			get
			{
				return m_ArmorScalars;
			}
			set
			{
				m_ArmorScalars = value;
			}
		}



		protected void Invalidate()
		{
			if ( Parent is Mobile )
				((Mobile)Parent).Delta( MobileDelta.Armor ); // Tell them armor rating has changed
		}

		public BaseArmor( Serial serial ) :  base( serial )
		{
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

		[Flags]
		private enum SaveFlag
		{
			None				= 0x00000000,
			Attributes			= 0x00000001,
			ArmorAttributes		= 0x00000002,
			PhysicalBonus		= 0x00000004,
			FireBonus			= 0x00000008,
			ColdBonus			= 0x00000010,
			PoisonBonus			= 0x00000020,
			EnergyBonus			= 0x00000040,
			Identified			= 0x00000080,
			MaxHitPoints		= 0x00000100,
			HitPoints			= 0x00000200,
			Crafter				= 0x00000400,
			Quality				= 0x00000800,
			Durability			= 0x00001000,
			Protection			= 0x00002000,
			Resource			= 0x00004000,
			BaseArmor			= 0x00008000,
			StrBonus			= 0x00010000,
			DexBonus			= 0x00020000,
			IntBonus			= 0x00040000,
			StrReq				= 0x00080000,
			DexReq				= 0x00100000,
			IntReq				= 0x00200000,
			MedAllowance		= 0x00400000,
			SkillBonuses		= 0x00800000,
			PlayerConstructed	= 0x01000000,	// erl: removed in version 9
			IOBAlignment		= 0x02000000
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			int version = 9;
			writer.Write( (int) version ); // version

			SaveFlag flags = SaveFlag.None;

			SetSaveFlag( ref flags, SaveFlag.IOBAlignment,		m_IOBAlignment != IOBAlignment.None );
			SetSaveFlag( ref flags, SaveFlag.Attributes,		false ); // removed in version 8
			SetSaveFlag( ref flags, SaveFlag.ArmorAttributes,	false ); // removed in version 8
			SetSaveFlag( ref flags, SaveFlag.PhysicalBonus,		false ); // removed in version 8
			SetSaveFlag( ref flags, SaveFlag.FireBonus,			false ); // removed in version 8
			SetSaveFlag( ref flags, SaveFlag.ColdBonus,			false ); // removed in version 8
			SetSaveFlag( ref flags, SaveFlag.PoisonBonus,		false ); // removed in version 8
			SetSaveFlag( ref flags, SaveFlag.EnergyBonus,		false ); // removed in version 8
			SetSaveFlag( ref flags, SaveFlag.Identified,		m_Identified != false );
			SetSaveFlag( ref flags, SaveFlag.MaxHitPoints,		m_MaxHitPoints != 0 );
			SetSaveFlag( ref flags, SaveFlag.HitPoints,			m_HitPoints != 0 );
			SetSaveFlag( ref flags, SaveFlag.Crafter,			m_Crafter != null );
			SetSaveFlag( ref flags, SaveFlag.Quality,			m_Quality != ArmorQuality.Regular );
			SetSaveFlag( ref flags, SaveFlag.Durability,		m_Durability != ArmorDurabilityLevel.Regular );
			SetSaveFlag( ref flags, SaveFlag.Protection,		m_Protection != ArmorProtectionLevel.Regular );
			SetSaveFlag( ref flags, SaveFlag.Resource,			m_Resource != DefaultResource );
			SetSaveFlag( ref flags, SaveFlag.BaseArmor,			m_ArmorBase != -1 );
			SetSaveFlag( ref flags, SaveFlag.StrBonus,			m_StrBonus != -1 );
			SetSaveFlag( ref flags, SaveFlag.DexBonus,			m_DexBonus != -1 );
			SetSaveFlag( ref flags, SaveFlag.IntBonus,			m_IntBonus != -1 );
			SetSaveFlag( ref flags, SaveFlag.StrReq,			m_StrReq != -1 );
			SetSaveFlag( ref flags, SaveFlag.DexReq,			m_DexReq != -1 );
			SetSaveFlag( ref flags, SaveFlag.IntReq,			m_IntReq != -1 );
			SetSaveFlag( ref flags, SaveFlag.MedAllowance,		m_Meditate != (AMA)(-1) );
			SetSaveFlag( ref flags, SaveFlag.SkillBonuses,		false ); // removed in version 8
			SetSaveFlag( ref flags, SaveFlag.PlayerConstructed,	false ); // removed in version 9

			writer.WriteEncodedInt( (int) flags );

			if ( GetSaveFlag( flags, SaveFlag.IOBAlignment ) )
				writer.WriteEncodedInt( (int) m_IOBAlignment );;

/*
		if ( GetSaveFlag( flags, SaveFlag.Attributes ) )
			m_AosAttributes.Serialize( writer );

		if ( GetSaveFlag( flags, SaveFlag.ArmorAttributes ) )
			m_AosArmorAttributes.Serialize( writer );

		if ( GetSaveFlag( flags, SaveFlag.PhysicalBonus ) )
			writer.WriteEncodedInt( (int) m_PhysicalBonus );

		if ( GetSaveFlag( flags, SaveFlag.FireBonus ) )
			writer.WriteEncodedInt( (int) m_FireBonus );

		if ( GetSaveFlag( flags, SaveFlag.ColdBonus ) )
			writer.WriteEncodedInt( (int) m_ColdBonus );

		if ( GetSaveFlag( flags, SaveFlag.PoisonBonus ) )
			writer.WriteEncodedInt( (int) m_PoisonBonus );

		if ( GetSaveFlag( flags, SaveFlag.EnergyBonus ) )
			writer.WriteEncodedInt( (int) m_EnergyBonus );
*/
			if ( GetSaveFlag( flags, SaveFlag.MaxHitPoints ) )
				writer.WriteEncodedInt( (int) m_MaxHitPoints );

			if ( GetSaveFlag( flags, SaveFlag.HitPoints ) )
				writer.WriteEncodedInt( (int) m_HitPoints );

			if ( GetSaveFlag( flags, SaveFlag.Crafter ) )
				writer.Write( (Mobile) m_Crafter );

			if ( GetSaveFlag( flags, SaveFlag.Quality ) )
				writer.WriteEncodedInt( (int) m_Quality );

			if ( GetSaveFlag( flags, SaveFlag.Durability ) )
				writer.WriteEncodedInt( (int) m_Durability );

			if ( GetSaveFlag( flags, SaveFlag.Protection ) )
				writer.WriteEncodedInt( (int) m_Protection );

			if ( GetSaveFlag( flags, SaveFlag.Resource ) )
				writer.WriteEncodedInt( (int) m_Resource );

			if ( GetSaveFlag( flags, SaveFlag.BaseArmor ) )
				writer.WriteEncodedInt( (int) m_ArmorBase );

			if ( GetSaveFlag( flags, SaveFlag.StrBonus ) )
				writer.WriteEncodedInt( (int) m_StrBonus );

			if ( GetSaveFlag( flags, SaveFlag.DexBonus ) )
				writer.WriteEncodedInt( (int) m_DexBonus );

			if ( GetSaveFlag( flags, SaveFlag.IntBonus ) )
				writer.WriteEncodedInt( (int) m_IntBonus );

			if ( GetSaveFlag( flags, SaveFlag.StrReq ) )
				writer.WriteEncodedInt( (int) m_StrReq );

			if ( GetSaveFlag( flags, SaveFlag.DexReq ) )
				writer.WriteEncodedInt( (int) m_DexReq );

			if ( GetSaveFlag( flags, SaveFlag.IntReq ) )
				writer.WriteEncodedInt( (int) m_IntReq );

			if ( GetSaveFlag( flags, SaveFlag.MedAllowance ) )
				writer.WriteEncodedInt( (int) m_Meditate );

			//if ( GetSaveFlag( flags, SaveFlag.SkillBonuses ) )
				//m_AosSkillBonuses.Serialize( writer );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch ( version )
			{
				case 9:
				{
					// erl - added to handle packing out of PlayerConstructed property
					goto case 8;
				}
				case 8:
				{
					// removed all AOS attributes
					goto case 7;
				}
				case 7:
				case 6:
				case 5:
				{
					SaveFlag flags = (SaveFlag)reader.ReadEncodedInt();

					if ( GetSaveFlag( flags, SaveFlag.IOBAlignment ) )
						m_IOBAlignment = (IOBAlignment)reader.ReadEncodedInt();


					// obsolete AOS attributes from version 8 on
					if (version < 8)
					{
						AosAttributes dmy_AosAttributes;
						AosArmorAttributes dmy_AosArmorAttributes;

						if ( GetSaveFlag( flags, SaveFlag.Attributes ) )
							dmy_AosAttributes = new AosAttributes( this, reader );
						//else
							//dmy_AosAttributes = new AosAttributes( this );

						if ( GetSaveFlag( flags, SaveFlag.ArmorAttributes ) )
							dmy_AosArmorAttributes = new AosArmorAttributes( this, reader );
						//else
							//dmy_AosArmorAttributes = new AosArmorAttributes( this );

						// read and throw away
						int foox;
						if ( GetSaveFlag( flags, SaveFlag.PhysicalBonus ) )
							foox = reader.ReadEncodedInt(); // m_PhysicalBonus

						if ( GetSaveFlag( flags, SaveFlag.FireBonus ) )
							foox = reader.ReadEncodedInt(); // m_FireBonus

						if ( GetSaveFlag( flags, SaveFlag.ColdBonus ) )
							foox = reader.ReadEncodedInt(); // m_ColdBonus

						if ( GetSaveFlag( flags, SaveFlag.PoisonBonus ) )
							foox = reader.ReadEncodedInt(); // m_PoisonBonus

						if ( GetSaveFlag( flags, SaveFlag.EnergyBonus ) )
							foox = reader.ReadEncodedInt(); // m_EnergyBonus
					}

					if ( GetSaveFlag( flags, SaveFlag.Identified ) )
						m_Identified = ( version >= 7 || reader.ReadBool() );

					if ( GetSaveFlag( flags, SaveFlag.MaxHitPoints ) )
						m_MaxHitPoints = reader.ReadEncodedInt();

					if ( GetSaveFlag( flags, SaveFlag.HitPoints ) )
						m_HitPoints = reader.ReadEncodedInt();

					if ( GetSaveFlag( flags, SaveFlag.Crafter ) )
						m_Crafter = reader.ReadMobile();

					if ( GetSaveFlag( flags, SaveFlag.Quality ) )
						m_Quality = (ArmorQuality)reader.ReadEncodedInt();
					else
						m_Quality = ArmorQuality.Regular;

					if ( version == 5 && m_Quality == ArmorQuality.Low )
						m_Quality = ArmorQuality.Regular;

					if ( GetSaveFlag( flags, SaveFlag.Durability ) )
						m_Durability = (ArmorDurabilityLevel)reader.ReadEncodedInt();

					if ( GetSaveFlag( flags, SaveFlag.Protection ) )
						m_Protection = (ArmorProtectionLevel)reader.ReadEncodedInt();

					if ( GetSaveFlag( flags, SaveFlag.Resource ) )
						m_Resource = (CraftResource)reader.ReadEncodedInt();
					else
						m_Resource = DefaultResource;

					if ( m_Resource == CraftResource.None )
						m_Resource = DefaultResource;

					if ( GetSaveFlag( flags, SaveFlag.BaseArmor ) )
						m_ArmorBase = reader.ReadEncodedInt();
					else
						m_ArmorBase = -1;

					if ( GetSaveFlag( flags, SaveFlag.StrBonus ) )
						m_StrBonus = reader.ReadEncodedInt();
					else
						m_StrBonus = -1;

					if ( GetSaveFlag( flags, SaveFlag.DexBonus ) )
						m_DexBonus = reader.ReadEncodedInt();
					else
						m_DexBonus = -1;

					if ( GetSaveFlag( flags, SaveFlag.IntBonus ) )
						m_IntBonus = reader.ReadEncodedInt();
					else
						m_IntBonus = -1;

					if ( GetSaveFlag( flags, SaveFlag.StrReq ) )
						m_StrReq = reader.ReadEncodedInt();
					else
						m_StrReq = -1;

					if ( GetSaveFlag( flags, SaveFlag.DexReq ) )
						m_DexReq = reader.ReadEncodedInt();
					else
						m_DexReq = -1;

					if ( GetSaveFlag( flags, SaveFlag.IntReq ) )
						m_IntReq = reader.ReadEncodedInt();
					else
						m_IntReq = -1;

					if ( GetSaveFlag( flags, SaveFlag.MedAllowance ) )
						m_Meditate = (AMA)reader.ReadEncodedInt();
					else
						m_Meditate = (AMA)(-1);

					// obsolete AOS attributes from version 8 on
					if (version < 8)
					{
						AosSkillBonuses dmy_AosSkillBonuses;
						if ( GetSaveFlag( flags, SaveFlag.SkillBonuses ) )
							dmy_AosSkillBonuses = new AosSkillBonuses( this, reader );
					}

					// obsolete PlayerConstructed property from version 9 on
					if (version < 9)
					{
						if ( GetSaveFlag( flags, SaveFlag.PlayerConstructed ) )
							PlayerCrafted = true;
					}

					break;
				}
				case 4:
				{
					// obsolete
					AosAttributes dmy_AosAttributes;
					AosArmorAttributes dmy_AosArmorAttributes;
					dmy_AosAttributes = new AosAttributes( this, reader );
					dmy_AosArmorAttributes = new AosArmorAttributes( this, reader );
					goto case 3;
				}
				case 3:
				{
					int dummy;
					dummy = reader.ReadInt();
					dummy = reader.ReadInt();
					dummy = reader.ReadInt();
					dummy = reader.ReadInt();
					dummy = reader.ReadInt();
					goto case 2;
				}
				case 2:
				case 1:
				{
					m_Identified = reader.ReadBool();
					goto case 0;
				}
				case 0:
				{
					m_ArmorBase = reader.ReadInt();
					m_MaxHitPoints = reader.ReadInt();
					m_HitPoints = reader.ReadInt();
					m_Crafter = reader.ReadMobile();
					m_Quality = (ArmorQuality)reader.ReadInt();
					m_Durability = (ArmorDurabilityLevel)reader.ReadInt();
					m_Protection = (ArmorProtectionLevel)reader.ReadInt();

					AMT mat = (AMT)reader.ReadInt();

					if ( m_ArmorBase == RevertArmorBase )
						m_ArmorBase = -1;

					/*m_BodyPos = (ArmorBodyType)*/reader.ReadInt();

					if ( version < 4 )
					{
						// Adam: (Leave for Adam to remove)
						//m_AosAttributes = new AosAttributes( this );
						//m_AosArmorAttributes = new AosArmorAttributes( this );
					}

					if ( version < 3 && m_Quality == ArmorQuality.Exceptional )
						DistributeBonuses( 6 );

					if ( version >= 2 )
					{
						m_Resource = (CraftResource)reader.ReadInt();
					}
					else
					{
						OreInfo info;

						switch ( reader.ReadInt() )
						{
							default:
							case 0: info = OreInfo.Iron; break;
							case 1: info = OreInfo.DullCopper; break;
							case 2: info = OreInfo.ShadowIron; break;
							case 3: info = OreInfo.Copper; break;
							case 4: info = OreInfo.Bronze; break;
							case 5: info = OreInfo.Gold; break;
							case 6: info = OreInfo.Agapite; break;
							case 7: info = OreInfo.Verite; break;
							case 8: info = OreInfo.Valorite; break;
						}

						m_Resource = CraftResources.GetFromOreInfo( info, mat );
					}

					m_StrBonus = reader.ReadInt();
					m_DexBonus = reader.ReadInt();
					m_IntBonus = reader.ReadInt();
					m_StrReq = reader.ReadInt();
					m_DexReq = reader.ReadInt();
					m_IntReq = reader.ReadInt();

					if ( m_StrBonus == OldStrBonus )
						m_StrBonus = -1;

					if ( m_DexBonus == OldDexBonus )
						m_DexBonus = -1;

					if ( m_IntBonus == OldIntBonus )
						m_IntBonus = -1;

					if ( m_StrReq == OldStrReq )
						m_StrReq = -1;

					if ( m_DexReq == OldDexReq )
						m_DexReq = -1;

					if ( m_IntReq == OldIntReq )
						m_IntReq = -1;

					m_Meditate = (AMA)reader.ReadInt();

					if ( m_Meditate == OldMedAllowance )
						m_Meditate = (AMA)(-1);

					if ( m_Resource == CraftResource.None )
					{
						if ( mat == ArmorMaterialType.Studded || mat == ArmorMaterialType.Leather )
							m_Resource = CraftResource.RegularLeather;
						else if ( mat == ArmorMaterialType.Spined )
							m_Resource = CraftResource.SpinedLeather;
						else if ( mat == ArmorMaterialType.Horned )
							m_Resource = CraftResource.HornedLeather;
						else if ( mat == ArmorMaterialType.Barbed )
							m_Resource = CraftResource.BarbedLeather;
						else
							m_Resource = CraftResource.Iron;
					}

					if ( m_MaxHitPoints == 0 && m_HitPoints == 0 )
						m_HitPoints = m_MaxHitPoints = Utility.RandomMinMax( InitMinHits, InitMaxHits );

					break;
				}
			}

			//if ( m_AosSkillBonuses == null )
				//m_AosSkillBonuses = new AosSkillBonuses( this );
/*
			if ( Core.AOS && Parent is Mobile )
				m_AosSkillBonuses.AddTo( (Mobile)Parent );
*/
			if (Parent is Mobile)
			{
				((Mobile)Parent).StatChange += new StatChangeHandler(ApplyStatBonuses);
				ApplyStatBonuses(Parent as Mobile, StatType.All);
			}

			if ( Parent is Mobile )
				((Mobile)Parent).CheckStatTimers();


// erl: PlayerConstructed is now obsolete, replaced with PlayerCrafted at item level
// ..
//			if ( version < 7 )
//				m_PlayerConstructed = true; // we don't know, so, assume it's crafted
// ..

		}

		public virtual CraftResource DefaultResource{ get{ return CraftResource.Iron; } }

		public BaseArmor( int itemID ) :  base( itemID )
		{
			m_Quality = ArmorQuality.Regular;
			m_Durability = ArmorDurabilityLevel.Regular;
			m_Crafter = null;
			m_IOBAlignment = IOBAlignment.None; //Pigpen - Addition for IOB System

			m_Resource = DefaultResource;
			Hue = CraftResources.GetHue( m_Resource );

			m_HitPoints = m_MaxHitPoints = Utility.RandomMinMax( InitMinHits, InitMaxHits );

			this.Layer = (Layer)ItemData.Quality;

			// Adam: obsolete
			//m_AosAttributes = new AosAttributes( this );
			//m_AosArmorAttributes = new AosArmorAttributes( this );
			//m_AosSkillBonuses = new AosSkillBonuses( this );
		}

		public override bool CanEquip( Mobile from )
		{
			if (this.IOBAlignment != IOBAlignment.None)
			{
				if( from is PlayerMobile )
				{
					PlayerMobile pm = (PlayerMobile)from;
					if ( pm.IOBEquipped == true )
					{
						pm.SendMessage( "You cannot equip more than one item of brethren at a time." );
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

			if ( !AllowMaleWearer && from.Body.IsMale && from.AccessLevel < AccessLevel.GameMaster )
			{
				if ( AllowFemaleWearer )
					from.SendLocalizedMessage( 1010388 ); // Only females can wear this.
				else
					from.SendMessage( "You may not wear this." );

				return false;
			}
			else if ( !AllowFemaleWearer && from.Body.IsFemale && from.AccessLevel < AccessLevel.GameMaster )
			{
				if ( AllowMaleWearer )
					from.SendMessage( "Only males can wear this." );
				else
					from.SendMessage( "You may not wear this." );

				return false;
			}
			else
			{
				double strBonus = ComputeStatBonus( StatType.Str, from );
				double dexBonus = ComputeStatBonus( StatType.Dex, from );
				double intBonus = ComputeStatBonus( StatType.Int, from );

				int strReq = ComputeStatReq( StatType.Str );
				int dexReq = ComputeStatReq( StatType.Dex );
				int intReq = ComputeStatReq( StatType.Int );

				if ( from.Dex < dexReq || (from.Dex + dexBonus) < 1 )
				{
					from.SendLocalizedMessage( 502077 ); // You do not have enough dexterity to equip this item.
					return false;
				} 
				else if ( from.Str < strReq || (from.Str + strBonus) < 1 )
				{
					from.SendLocalizedMessage( 500213 ); // You are not strong enough to equip that.
					return false;
				}
				else if ( from.Int < intReq || (from.Int + intBonus) < 1 )
				{
					from.SendMessage( "You are not smart enough to equip that." );
					return false;
				}
			}

			return base.CanEquip( from );
		}

		public override bool CheckPropertyConfliction( Mobile m )
		{
			if ( base.CheckPropertyConfliction( m ) )
				return true;

			if ( Layer == Layer.Pants )
				return ( m.FindItemOnLayer( Layer.InnerLegs ) != null );

			if ( Layer == Layer.Shirt )
				return ( m.FindItemOnLayer( Layer.InnerTorso ) != null );

			return false;
		}

		public void ApplyStatBonuses(Mobile wearer, StatType stat)
		{
			// BE CAREFUL! This function is an event handler for Mobile.StatChange. AddStatMod invokes StatChange - ie, the potential here for an
			// infinite recursion loop is VERY real. Make sure that you do not make stats interdependant on bonuses!!!
			// ie when calculating DexBonus, you can use Str or Int but using Dex will cause a loop.
			// using Str in calc'ing DexBonus and ALSO using Dex in calc'ing StrBonus will also cause a loop.
			// See examples.
			wearer.CheckStatTimers();

			string modName = this.Serial.ToString();

			/* EXAMPLES
			 * 
			 * if ((stat & StatType.Dex) != 0)
			 * {
			 *	   Here I can add statmods for Str and Int but not Dex.
			 * }
			 * if ((stat & StatType.Int) != 0 && (stat & StatType.Str) != 0)
			 * {
			 *     Here I can only add statmods for Dex, because the calculation is using Int and Str
			 * }
			 */
			
			if ((stat & StatType.Str) != 0) // since we're handling Str, we're not allowed to modify it
			{
				double dexBonus = ComputeStatBonus( StatType.Dex, wearer );
				if (dexBonus != 0  && (wearer.GetStatMod(modName + "Dex") == null || wearer.GetStatMod(modName + "Dex").Offset != dexBonus))
					wearer.AddStatMod( new StatMod( StatType.Dex, modName + "Dex", dexBonus, TimeSpan.Zero ) );
				if (dexBonus == 0)
					wearer.RemoveStatMod(modName + "Dex");
			}
		}

		public override bool OnEquip( Mobile from )
		{
			from.StatChange += new StatChangeHandler(ApplyStatBonuses);
			ApplyStatBonuses(from, StatType.All);
			
			return base.OnEquip( from );
		}

		public override void OnRemoved( object parent )
		{
			if ( parent is Mobile )
			{
				Mobile m = (Mobile)parent;
				string modName = this.Serial.ToString();

				m.StatChange -= new StatChangeHandler(ApplyStatBonuses); // this *must* be before the stat mod removals, or recursion loop happens

				m.RemoveStatMod( modName + "Str" );
				m.RemoveStatMod( modName + "Dex" );
				m.RemoveStatMod( modName + "Int" );

				//if ( Core.AOS )
					//m_AosSkillBonuses.Remove();

				((Mobile)parent).Delta( MobileDelta.Armor ); // Tell them armor rating has changed
				m.CheckStatTimers();
			}
			
			if ( parent is PlayerMobile )
			{
				if ( this.IOBAlignment != IOBAlignment.None ) //Pigpen - Addition for IOB System
				{
					((PlayerMobile)parent).IOBEquipped = false;
				}
			}

			base.OnRemoved( parent );
		}

		public virtual int OnHit( BaseWeapon weapon, int damageTaken )
		{
			double HalfAr = ArmorRatingScaled / 2.0;
			int Absorbed = (int)(HalfAr + HalfAr*Utility.RandomDouble());

			damageTaken -= Absorbed;
			if ( damageTaken < 0 ) 
				damageTaken = 0;

			if ( Absorbed < 2 )
				Absorbed = 2;

			if ( 25 > Utility.Random( 100 ) ) // 25% chance to lower durability
			{
				/*if ( Core.AOS && m_AosArmorAttributes.SelfRepair > Utility.Random( 10 ) )
				{
					HitPoints += 2;
				}
				else*/
				{
					if ( weapon.Type == WeaponType.Bashing )
						HitPoints -= Absorbed / 2;
					else
						HitPoints -= Utility.Random( 2 );
				}
			}

			return damageTaken;
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

			if ( m_Quality == ArmorQuality.Exceptional )
			{
				if ( oreType != 0 )
					list.Add( 1053100, "#{0}\t{1}", oreType, GetNameString() ); // exceptional ~1_oretype~ ~2_armortype~
				else
					list.Add( 1050040, GetNameString() ); // exceptional ~1_ITEMNAME~
			}
			else
			{
				if ( oreType != 0 )
					list.Add( 1053099, "#{0}\t{1}", oreType, GetNameString() ); // ~1_oretype~ ~2_armortype~
				else if ( Name == null )
					list.Add( LabelNumber );
				else
					list.Add( Name );
			}
		}

		public override bool AllowEquipedCast( Mobile from )
		{
			if ( base.AllowEquipedCast( from ) )
				return true;

			//return ( m_AosAttributes.SpellChanneling != 0 );
			return false;
		}

		public virtual int GetLuckBonus()
		{
			CraftResourceInfo resInfo = CraftResources.GetInfo( m_Resource );

			if ( resInfo == null )
				return 0;

			CraftAttributeInfo attrInfo = resInfo.AttributeInfo;

			if ( attrInfo == null )
				return 0;

			return attrInfo.ArmorLuck;
		}

		public override void GetProperties( ObjectPropertyList list )
		{
			base.GetProperties( list );

			if ( m_Crafter != null )
				list.Add( 1050043, m_Crafter.Name ); // crafted by ~1_NAME~

			//m_AosSkillBonuses.GetProperties( list );

			int prop;

			if ( (prop = ArtifactRarity) > 0 )
				list.Add( 1061078, prop.ToString() ); // artifact rarity ~1_val~
/*
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
*/
			if ( (prop = GetLowerStatReq()) != 0 )
				list.Add( 1060435, prop.ToString() ); // lower requirements ~1_val~%
/*
			if ( (prop = (GetLuckBonus() + m_AosAttributes.Luck)) != 0 )
				list.Add( 1060436, prop.ToString() ); // luck ~1_val~

			if ( (prop = m_AosArmorAttributes.MageArmor) != 0 )
				list.Add( 1060437 ); // mage armor

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

			if ( (prop = m_AosArmorAttributes.SelfRepair) != 0 )
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

			if ( (prop = GetDurabilityBonus()) > 0 )
				list.Add( 1060410, prop.ToString() ); // durability ~1_val~%

			if ( (prop = ComputeStatReq( StatType.Str )) > 0 )
				list.Add( 1061170, prop.ToString() ); // strength requirement ~1_val~

			if ( m_HitPoints > 0 && m_MaxHitPoints > 0 )
				list.Add( 1060639, "{0}\t{1}", m_HitPoints, m_MaxHitPoints ); // durability ~1_val~ / ~2_val~
		}

		public virtual string OldName
		{
			get
			{
				return null;
			}
		}

		public override void OnSingleClick( Mobile from )
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
                if (m_Quality == ArmorQuality.Exceptional)
                    attrs.Add(new EquipInfoAttribute(1018305 - (int)m_Quality));

                if (m_Identified)
                {
                    if (m_Durability != ArmorDurabilityLevel.Regular)
                        attrs.Add(new EquipInfoAttribute(1038000 + (int)m_Durability));

                    if (m_Protection > ArmorProtectionLevel.Regular && m_Protection <= ArmorProtectionLevel.Invulnerability)
                        attrs.Add(new EquipInfoAttribute(1038005 + (int)m_Protection));
                }
                else if (m_Durability != ArmorDurabilityLevel.Regular || (m_Protection > ArmorProtectionLevel.Regular && m_Protection <= ArmorProtectionLevel.Invulnerability))
                {
                    attrs.Add(new EquipInfoAttribute(1038000)); // Unidentified
                }
            }
            
			int number;

			if ( Name == null )
			{
				if (OldName == null)
				{
					number = LabelNumber;
				}
				else
				{
					string oldname = OldName;
					//yay!  Show us the old way!
					if (m_Quality == ArmorQuality.Exceptional)
					{
						oldname = "exceptional " + oldname;
					}

					if (m_Identified)
					{
						if (m_Durability != ArmorDurabilityLevel.Regular)
						{
							//attrs.Add(new EquipInfoAttribute(1038000 + (int)m_Durability));
							oldname = m_Durability.ToString().ToLower() + " " + oldname;
						}

						if (m_Protection > ArmorProtectionLevel.Regular && m_Protection <= ArmorProtectionLevel.Invulnerability)
						{
							//attrs.Add(new EquipInfoAttribute(1038005 + (int)m_Protection));
							oldname = oldname + " of " + m_Protection.ToString().ToLower();
						}
					}
					else if (m_Durability != ArmorDurabilityLevel.Regular 
						     || (m_Protection > ArmorProtectionLevel.Regular && m_Protection <= ArmorProtectionLevel.Invulnerability))
					{
						oldname = "magic " + oldname;
					}

					//crafted-by goes at the end
					if (m_Crafter != null)
					{
						oldname += " crafted by " + m_Crafter.Name;
					}

					this.LabelTo(from, oldname);
					number = 1041000;
				}
			}
			else
			{
				this.LabelTo( from, Name );
				number = 1041000;
			}

			if ( attrs.Count == 0 && Crafter == null && Name != null )
				return;

			if (OldName == null)
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
	}
}
