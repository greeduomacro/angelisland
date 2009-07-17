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

/* Engines/IOBSystem/Items/GuardPost.cs
 * CHANGELOG:
 *	05/25/09, plasma
 *		- Fixed a bug with spawning logic
 *		-	Changed single-click LabelTo
 *		- Added RefereshSpawnTime method
 *	04/06/09, plasma
 *		Implemented OnChop
 *	10/26/08, plasma
 *		Initial creation
 */

using System;
using System.Collections;
using System.Collections.Generic;
using Server;
using Server.Guilds;
using Server.Mobiles;
using Server.Engines.IOBSystem;
using Server.Engines.IOBSystem.Attributes;
using Server.Engines.IOBSystem.Gumps.GuardPostGump;

namespace Server.Items
{
	/// <summary>
	/// NOTE: These are NOT to be created directly, [Constructable] is only provided for testing.
	/// There are register / unregister methods in the relevant KinCityData to add / remove these properly!
	/// </summary>
	public class KinGuardPost : Spawner, IChopable
	{
		public enum HireSpeeds
		{
			Fast,
			Medium,
			Slow
		}

		#region fields
		
		private KinFactionCities m_City = KinFactionCities.Cove;	//Faction city this post belongs to
		private PlayerMobile m_Owner = null;											//Faction member who owns this guard post
		private int m_Silver = 0;																	//Silver stock to respawn new guards with		
		private int m_SpawnTimeMinutes = 0;
		private KinFactionGuardTypes m_GuardType;
		private DateTime m_NextSpawnTime = DateTime.Now;
		private DateTime m_NextMaintTime = DateTime.Now;
		private FightMode m_FightMode = FightMode.Strongest;
		private FightStyle m_FightStyle = FightStyle.Default;
		bool m_Reset = false; //used in the timer to reset time after monsters are reduced to 0

		#endregion

		#region properties

		/// <summary>
		/// Gets or sets the owner.
		/// </summary>
		/// <value>The owner.</value>
		[CommandProperty(AccessLevel.GameMaster)]
		public PlayerMobile Owner
		{
			get { return m_Owner; }
			set { m_Owner = value; }
		}

		/// <summary>
		/// Gets or sets the city.
		/// </summary>
		/// <value>The city.</value>
		[CommandProperty(AccessLevel.GameMaster)]
		public KinFactionCities City
		{
			get { return m_City; }
			set { m_City = value; }
		}

		/// <summary>
		/// Gets or sets the silver.
		/// </summary>
		/// <value>The silver.</value>
		[CommandProperty(AccessLevel.GameMaster)]
		public int Silver
		{
			get { return m_Silver; }
			set 
			{ 
				m_Silver = value;
				if (Running == false && m_Silver >= HireCost)
				{
					//Spawn(0);
					Running = true;
				}
			}
		}

		/// <summary>
		/// Gets the kin city region.
		/// </summary>
		/// <value>The kin city region.</value>
		public KinCityRegion KinCityRegion
		{
			get { return KinCityRegion.GetKinCityAt(this); }
		}

		/// <summary>
		/// Gets or sets the hire speed.
		/// </summary>
		/// <value>The hire speed.</value>
		[CommandProperty(AccessLevel.GameMaster)]
		public HireSpeeds HireSpeed
		{
			get { return GetHireSpeed(); }
			set { SetHireSpeed(value); }
		}


		/// <summary>
		/// Gets or sets the type of the guard.
		/// </summary>
		/// <value>The type of the guard.</value>
		[CommandProperty(AccessLevel.GameMaster,  AccessLevel.Administrator)]
		public KinFactionGuardTypes GuardType
		{
			get { return m_GuardType; }
			set 
			{
				m_GuardType = value;
				if (CreaturesName.Count == 1)
				{
					CreaturesName[0] = (value).ToString();
					
				}
				else
				{
					CreaturesName.Add( (value).ToString());
				}
			}	
		}

		/*
		[CommandProperty(AccessLevel.GameMaster)]
		public FightStyle FightStyle 
		{
			get { return m_FightStyle; }
			set { m_FightStyle = value; }
		}
		*/

		/// <summary>
		/// Gets or sets the fight mode.
		/// </summary>
		/// <value>The fight mode.</value>
		[CommandProperty(AccessLevel.GameMaster)]
		public FightMode FightMode
		{
			get { return m_FightMode; }
			set { m_FightMode= value; }
		}

		/// <summary>
		/// Gets or sets the spawn time minutes.
		/// </summary>
		/// <value>The spawn time minutes.</value>
		[CommandProperty(AccessLevel.GameMaster)]
		public int SpawnTimeMinutes
		{
			get { return m_SpawnTimeMinutes ; }
			set { m_SpawnTimeMinutes = value; }
		}

		/// <summary>
		/// Gets or sets the next spawn time.
		/// </summary>
		/// <value>The next spawn time.</value>
		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime NextSpawnTime
		{
			get { return m_NextSpawnTime; }
			set { m_NextSpawnTime = value; }
		}

		/// <summary>
		/// Gets the next maint time.
		/// </summary>
		/// <value>The next maint time.</value>
		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime NextMaintTime
		{
			get { return m_NextMaintTime; }
		}

		/// <summary>
		/// Returns silver maintenance cost based on guard type using reflection
		/// </summary>
		/// <returns></returns>
		[CommandProperty(AccessLevel.GameMaster)]
		public int MaintCost
		{
			get
			{
				return KinSystem.GetGuardMaintCost(m_GuardType);
			}
		}

		/// <summary>
		/// Returns silver guard hire cost based on guard type using reflection
		/// </summary>
		/// <returns></returns>
		[CommandProperty(AccessLevel.GameMaster)]
		public int HireCost
		{
			get
			{
				return KinSystem.GetGuardHireCost(m_GuardType);
			}
		}

		/// <summary>
		/// Returns cost type of the guard using reflection, or defaults to Medium
		/// </summary>
		/// <returns></returns>
		[CommandProperty(AccessLevel.GameMaster)]
		public int CostType
		{
			get
			{
				return KinSystem.GetGuardCostType(m_GuardType);
			}																		
		}

		#endregion

		#region ctors / dtors

		/// <summary>
		/// Initializes a new instance of the <see cref="KinGuardPost"/> class.
		/// </summary>
		/// <param name="itemID">The item ID.</param>
		/// <param name="owner">The owner.</param>
		/// <param name="guardType">Type of the guard.</param>
		/// <param name="city">The city.</param>
		public KinGuardPost(int itemID, PlayerMobile owner, KinFactionGuardTypes guardType, KinFactionCities city)			
		{
			this.ItemID = itemID;
			Movable = false;
			Visible = true;
			m_City = city;
			m_Owner = owner;
			m_GuardType = guardType; 
			ArrayList creaturesName = new ArrayList(); //bah *! $ arraylists			
			creaturesName.Add(guardType.ToString()); 

			InitSpawn(1, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1), 0, 4, creaturesName);
		}

		public KinGuardPost(Serial serial)
			: base(serial)
		{
		}

		#endregion

		#region serial

		/// <summary>
		/// Deserializes the specified reader.
		/// </summary>
		/// <param name="reader">The reader.</param>
		public override void Deserialize(GenericReader reader)
		{			
			base.Deserialize(reader);

			TimeSpan ts = TimeSpan.Zero;

			int version = reader.ReadInt();
			switch (version)
			{
				case 1:
					{
						m_Reset = reader.ReadBool();
						ts = reader.ReadDeltaTime() - DateTime.Now;
						m_NextMaintTime = DateTime.Now + ts;						
						ts = reader.ReadDeltaTime() - DateTime.Now;
						m_NextSpawnTime = DateTime.Now + ts;
						m_SpawnTimeMinutes = reader.ReadInt();
						m_GuardType = (KinFactionGuardTypes)reader.ReadInt();
						goto case 0;
					}
				case 0:
					{
						m_City = (KinFactionCities)reader.ReadInt();
						m_Owner = (PlayerMobile)reader.ReadMobile();
						m_Silver = reader.ReadInt();
						break;
					}
			}
		}

		/// <summary>
		/// Serializes the specified writer.
		/// </summary>
		/// <param name="writer">The writer.</param>
		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)1);//version
			//pla: v1
			writer.Write(m_Reset);
			writer.WriteDeltaTime(m_NextMaintTime);
			writer.WriteDeltaTime(m_NextSpawnTime);
			writer.Write((int)m_SpawnTimeMinutes);
			writer.Write((int)m_GuardType);
			//pla: v0
			writer.Write((int)m_City);
			writer.Write(m_Owner);
			writer.Write(m_Silver);
			
		}

		#endregion
		
		#region overrides from Item

		/// <summary>
		/// Spawn override...
		/// </summary>
		public override void Spawn()
		{
		 
			//Before anything else happens, check maintenance time
			if (DateTime.Now > m_NextMaintTime)
			{
				if (m_Silver < MaintCost)
				{
				 //not enough silver so just stop running 
					Running = false;
					return;
				}
				//Decrease silver and update next spawn time
				m_Silver -= MaintCost;
				m_NextMaintTime = DateTime.Now + TimeSpan.FromMinutes(KinSystemSettings.GuardMaintMinutes);
				//Provide maint activity
				if (KinCityRegion != null)
				{
					KinCityRegion.ProcessActivity(KinFactionActivityTypes.GuardPostMaint);
				}
			}

			//Spawn fires every minute, but the guard post has a dynamic spawn time
			//therefore check if we can now spawn ( 0 creatures, these can only spawn one each to keep it simple )
			//if so set the next spawn time based on the speed and then check until this time has passed before spawning
			Defrag();
			if (Creatures.Count == 0)
			{
				if (m_Reset)	//Need a reset flip so the first time a creature is elgible to spawn, the timer is reset
				{
					m_NextSpawnTime = DateTime.Now + TimeSpan.FromMinutes(m_SpawnTimeMinutes);
					m_Reset = false;
					return;
				}
		
				//spawn if poss
				if (DateTime.Now > m_NextSpawnTime && m_Silver >= HireCost)
				{
					m_Silver -= HireCost;
					Spawn(0);
					m_Reset = true;  //set reset to true so the next time a guard dies the next one has to wait the full respawn time
					//Provide hire activity
					if (KinCityRegion != null)
					{
						KinCityRegion.ProcessActivity(KinFactionActivityTypes.GuardPostHire);
					}
				}
				else if (m_Silver < HireCost)
				{
					Running = false;	 
				}				
			}			
		}

		/// <summary>
		/// Fired from Spawner just before the mobile is moved into the world.
		/// </summary>
		/// <param name="m">The m.</param>
		protected override void OnAfterMobileSpawn(Mobile m)
		{
			if (m is BaseCreature && Owner != null)
				PreapareCreature((BaseCreature)m);
		}

		/// <summary>
		/// Called when [single click].
		/// </summary>
		/// <param name="from">From.</param>
		public override void OnSingleClick(Mobile from)
		{
			//Show which city this sigil relates to
			if ((m_Owner != null && m_Owner == from) || (m_Owner != null && from.AccessLevel > AccessLevel.Counselor) )
			{
				string city = Enum.GetName(typeof(KinFactionCities), m_City);
				if (Creatures.Count > 0)
				{
					LabelTo(from,
						string.Format("Guard post for the City of {0} owned by {1}\r\nGuard currently hired, and {2} in funding.",
							city, m_Owner.Name, m_Silver));
				}
				else
				{
					LabelTo(from,
						string.Format("Guard post for the City of {0} owned by {1}\r\nNo guard currently hired, and {2} in funding.",
							city, m_Owner.Name, m_Silver));
				}
			}
			else if (m_Owner != null)
			{
				string city = Enum.GetName(typeof(KinFactionCities), m_City);
				LabelTo(from,	string.Format("Guard post for the City of {0}.",city));
			}
			else
			{
				LabelTo(from, "An unused guard post");
			}			
		}

		/// <summary>
		/// Called when [double click].
		/// </summary>
		/// <param name="from">From.</param>
		public override void OnDoubleClick(Mobile from)
		{
			if( from.AccessLevel > AccessLevel.Counselor)
				base.OnDoubleClick(from);

			//Only faction leader and the player assigned to this post should be able to see the gump
			if (from == Owner || from == KinCityManager.GetCityData(m_City).CityLeader || from.AccessLevel > AccessLevel.Counselor)
			{
				from.CloseGump(typeof(KinGuardPostGump));
				from.SendGump( new KinGuardPostGump(this, from)); 
			}
		}

		/// <summary>
		/// Called when [delete].
		/// </summary>
		public override void OnDelete()
		{
			//Give guard post slots back
			KinCityData data = KinCityManager.GetCityData(m_City);
			if( data == null )
			{
				base.OnDelete();
				return;
			}
			KinCityData.BeneficiaryData bd = data.GetBeneficiary(m_Owner);
			if( bd == null )
			{
				base.OnDelete();
				return;
			}
			bd.UnRegisterGuardPost(this);
			base.OnDelete();
		}

		#endregion

		#region private methods

		/// <summary>
		/// Sets the hire speed.
		/// </summary>
		/// <param name="speed">The speed.</param>
		private void SetHireSpeed(HireSpeeds speed)
		{
			switch (speed)	
			{
				case HireSpeeds.Fast: m_SpawnTimeMinutes = 5; return;
				case HireSpeeds.Medium: m_SpawnTimeMinutes = 15; return;
				case HireSpeeds.Slow: m_SpawnTimeMinutes = 30; return;
			}
		}

		/// <summary>
		/// Gets the hire speed.
		/// </summary>
		/// <returns></returns>
		private HireSpeeds GetHireSpeed()
		{
			if( m_SpawnTimeMinutes <= 5 )
			{
				return HireSpeeds.Fast;
			}
			else if (m_SpawnTimeMinutes <= 15)
			{
				return HireSpeeds.Medium;
			}
			return HireSpeeds.Slow;
		}

		/// <summary>
		/// Setup mob with owner's IOB alignment, the guard post's and the City's fight settings 
		/// </summary>
		/// <param name="bc"></param>
		private void PreapareCreature(BaseCreature bc)
		{
			if (bc == null) return;

			//Set the IOBAlignment to that of the owner
			bc.IOBAlignment = Owner.IOBRealAlignment;
			if (!bc.Owners.Contains(Owner))
				bc.Owners.Add(Owner);

			//Set the fight mode and fight style to that the owner chose
			//NAND wipe the strongest, weakest and closest flags
			// 0x7
			bc.FightMode &= ~FightMode.Strongest;
			bc.FightMode &= ~FightMode.Weakest;
			bc.FightMode &= ~FightMode.Closest;

			//OR in the one we care about
			bc.FightMode |= m_FightMode;

			////////////////////////////////////////////////
			//PLASMA: Fightsyle not being used currently
			////////////////////////////////////////////////
			//NAND out mage and melee
			//0x3
			//bc.FightStyle &= ~FightStyle.Magic;
			//bc.FightStyle &= ~FightStyle.Melee;

			//OR in the one we care about
			//bc.FightStyle |= m_FightStyle;
			////////////////////////////////////////////////


			//NAND out the attack settings
			//1F
			bc.FightMode &= ~FightMode.All; bc.FightMode &= ~FightMode.Aggressor;
			bc.FightMode &= ~FightMode.Criminal; bc.FightMode &= ~FightMode.Murderer;
			bc.FightMode &= ~FightMode.Evil;
			

			//OR in the ones we care about
			KinCityData data = KinCityManager.GetCityData(City);
			if (data != null)
			{
				switch (data.GuardOption)
				{
					case KinCityData.GuardOptions.None:
					case KinCityData.GuardOptions.LordBritish:
						//These two cases shouldn't be possible 
						break;
					case KinCityData.GuardOptions.FactionOnly:
						{
							bc.FightMode |= FightMode.Aggressor;
							break;
						}
					case KinCityData.GuardOptions.FactionAndReds:
						{
							bc.FightMode |= FightMode.Aggressor;
							bc.FightMode |= FightMode.Murderer;
							break;
						}
					case KinCityData.GuardOptions.FactionAndRedsAndCrim:
						{
							bc.FightMode |= FightMode.Aggressor;
							bc.FightMode |= FightMode.Murderer;
							bc.FightMode |= FightMode.Criminal;
							break;
						}
					case KinCityData.GuardOptions.Everyone:
						{
							bc.FightMode |= FightMode.Aggressor;
							bc.FightMode |= FightMode.All;
							break;
						}
					case KinCityData.GuardOptions.RedsAndCrim:
						{
							bc.FightMode |= FightMode.Aggressor;
							bc.FightMode |= FightMode.Murderer;
							bc.FightMode |= FightMode.Criminal;
							break;
						}
					case KinCityData.GuardOptions.Crim:
						{
							bc.FightMode |= FightMode.Aggressor;
							bc.FightMode |= FightMode.Criminal;
							break;
						}
					default:
						break;
				}

			}
			//bc.FightMode |= m_FightStyle;
			bc.AIObject.Think();
		}

		#endregion

		#region public methods

		/// <summary>
		/// Updates the exisitng guards with the current fight mode / style settings (for when guard option is changed for the city)
		/// </summary>
		public void UpdateExisitngGuards()
		{
			foreach (Mobile m in Creatures)
			{
				if( m is BaseCreature )
				{
					PreapareCreature(m as BaseCreature);
				}
			}
		}

		/// <summary>
		/// Forces an update of the next spawn time, used from the gump
		/// </summary>
		public void RefreshNextSpawnTime()
		{
			//If fast, make sure one spawns right now, otherwise apply normal rules
			if (HireSpeed == HireSpeeds.Fast)
			{
				m_NextSpawnTime = DateTime.Now;
			}
			else
			{
				m_NextSpawnTime = DateTime.Now + TimeSpan.FromMinutes(m_SpawnTimeMinutes); 
			}
		}

		#endregion

		#region IChopable Members

		/// <summary>
		/// Called when [chop].
		/// </summary>
		/// <param name="from">From.</param>
		void IChopable.OnChop(Mobile from)
		{
			if (from == m_Owner || from == KinCityManager.GetCityData(m_City).CityLeader || from.AccessLevel > AccessLevel.Counselor)
			{
				Delete();
			}
		}

		#endregion
	}

}
