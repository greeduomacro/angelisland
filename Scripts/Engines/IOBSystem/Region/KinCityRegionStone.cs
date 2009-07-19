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

/* Scripts\Engines\IOBSystem\Region\KinCityRegionStone.cs
 * CHANGELOG:
 *	07/19/09, plasma
 *		- More NULL checks in case of missing or corrupt city data
 *	06/27/09, plasma
 *		Added a bunch of OnEnter() messages
 *	05/25/09, plasma
 *		Added v3 and enemy annoucement.
 *		I previously added the vast majority of this class, must have forgetten the changelog entry
 *	1/18/09, Adam
 *		Initial Creation
 */

using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using Server;
using Server.Guilds;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Multis;
using Server.Regions;
using Server.Engines.ChampionSpawn;
using Server.Engines.IOBSystem;

namespace Server.Engines.IOBSystem
{
	public class KinCityRegionStone : Server.Items.RegionControl
	{

		private Queue<PlayerMobile> m_VisitorQueue = new Queue<PlayerMobile>();
		private Queue<PlayerMobile> m_ShopperQueue = new Queue<PlayerMobile>();
		private List<KinFactionActivityTypes> m_Activity = new List<KinFactionActivityTypes>();
		private KinFactionCities m_City = KinFactionCities.Cove;
		private List<ChampKinCity> m_Champs = new List<ChampKinCity>();
		private DateTime m_LastAnnounceTime = DateTime.Now;

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
		/// Gets the city tax rate.
		/// </summary>
		/// <value>The city tax rate.</value>
		public double CityTaxRate
		{
			get { return CityData.TaxRate; }
		}

		/// <summary>
		/// Gets the city data.
		/// </summary>
		/// <value>The city data.</value>
		private KinCityData CityData
		{
			get { return KinCityManager.GetCityData(m_City); }
		}

		/// <summary>
		/// Kins the city manager_ on golem controller.
		/// </summary>
		/// <param name="city">The city.</param>
		/// <param name="on">if set to <c>true</c> [on].</param>
		public void KinCityManager_OnGolemController(bool on)
		{
			m_Champs.ForEach(delegate(ChampKinCity champ)
			{
				if (champ != null && !champ.Deleted)
				{
					champ.Active = on;
					if (!on)
					{
						//Clear GCs out
						champ.WipeMonsters();
					}
				}
			});
		}

		/// <summary>
		/// Handles the guard change, will switch on/off the guards depending if the new guard option is Lord British
		/// </summary>
		/// <param name="city">The city.</param>
		/// <param name="guardOption">The guard option.</param>
		public void KinCityManager_OnChangeGuards(KinCityData.GuardOptions guardOption)
		{
			if (guardOption == KinCityData.GuardOptions.LordBritish)
			{
				if (IsGuarded == false)
					IsGuarded = true;
			}
			else
			{
				if (IsGuarded == true)
					IsGuarded = false;
			}
		}

		/// <summary>
		/// Processes the activity.
		/// </summary>
		/// <param name="type">The type.</param>
		public void ProcessActivity(KinFactionActivityTypes type)
		{
			KinCityManager.ProcessActivityDelta(City, type);
		}

		/// <summary>
		/// Processes the visitor.
		/// </summary>
		/// <param name="pm">The pm.</param>
		private void ProcessVisitor(PlayerMobile pm)
		{
			if (CityData == null) return;
				if (CityData.ControlingKin != IOBAlignment.None && pm.IOBRealAlignment != CityData.ControlingKin && pm.IOBRealAlignment != IOBAlignment.None )
			{
				if (DateTime.Now > m_LastAnnounceTime.AddMinutes(15))
				{
					m_LastAnnounceTime = DateTime.Now;
					KinSystem.SendKinMessage(CityData.ControlingKin, string.Format("The City of {0} is under attack!  Come quickly to defend!", CityData.City.ToString()));

					//a 25% chance this will be broadcast to all kin
					if (Utility.RandomChance(25))
					{
						KinSystem.SendKinMessage( KinSystem.BroadcastOptions.ExcludeKin,  new IOBAlignment[] { CityData.ControlingKin, pm.IOBRealAlignment }, string.Format("Spies report enemy Kin activity within the City of {0}..", CityData.City.ToString()));
					}
				}
			}

			if (m_VisitorQueue.Contains(pm)) return;

			if (m_VisitorQueue.Count > KinSystemSettings.A_MaxVisitors)
				m_VisitorQueue.Dequeue();

			m_VisitorQueue.Enqueue(pm);

			if (pm.IOBRealAlignment == IOBAlignment.None || pm.IOBRealAlignment == CityData.ControlingKin)
			{
				//good (or non factioned) guys, + activity
				ProcessActivity(KinFactionActivityTypes.FriendlyVisitor);
			}
			else
			{
				//bad dudes, - activity
				ProcessActivity(KinFactionActivityTypes.Visitor);
			}

		}

		/// <summary>
		/// Processes the shopper.
		/// </summary>
		/// <param name="pm">The pm.</param>
		private void ProcessShopper(PlayerMobile pm)
		{
			if (pm == null) return;
			if (m_ShopperQueue.Contains(pm)) return;

			if (m_VisitorQueue.Count > KinSystemSettings.A_MaxShoppers)
				m_VisitorQueue.Dequeue();

			m_VisitorQueue.Enqueue(pm);
			if (CityData == null) return;
			if (pm.IOBRealAlignment == IOBAlignment.None || pm.IOBRealAlignment == CityData.ControlingKin)
			{
				//good (or non factioned) guys, + activity
				ProcessActivity(KinFactionActivityTypes.FriendlySale);
			}
			else
			{
				//bad dudes, - activity
				ProcessActivity(KinFactionActivityTypes.Sale);
			}

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="KinCityRegionStone"/> class.
		/// </summary>
		[Constructable]
		public KinCityRegionStone()
			: base()
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="KinCityRegionStone"/> class.
		/// </summary>
		/// <param name="serial">The serial.</param>
		public KinCityRegionStone(Serial serial)
			: base(serial)
		{
		}

		/// <summary>
		/// Deletes this instance.
		/// </summary>
		public override void Delete()
		{
			try
			{
				m_Champs.Clear();
				m_VisitorQueue.Clear();
				m_ShopperQueue.Clear();
			}
			catch (Exception ex) { EventSink.InvokeLogException(new LogExceptionEventArgs(ex)); }

			base.Delete();
		}

		public override CustomRegion CreateRegion(Server.Items.RegionControl rc, Map map)
		{
			return new KinCityRegion(rc, map);
		}

		/// <summary>
		/// Process vendor sales
		/// </summary>
		/// <param name="m"></param>
		/// <param name="totalCost"></param>
		public void OnVendorBuy(Mobile m, int totalTax)
		{
			ProcessShopper(m as PlayerMobile);
			KinCityManager.ProcessSale(City, m, totalTax);
		}

		/// <summary>
		/// Called when [death].
		/// </summary>
		/// <param name="m">The m.</param>
		/// <returns></returns>
		public void OnDeath(Mobile m)
		{
			if (m is GolemController)
			{
				ProcessActivity(KinFactionActivityTypes.GCDeath);
			}
			else if (m is PlayerMobile)
			{
				PlayerMobile pm = ((PlayerMobile)m);
				if (pm.IOBRealAlignment == IOBAlignment.None) return;
				KinCityData data = KinCityManager.GetCityData(City);
				if (data == null) return;
				if (data.ControlingKin == pm.IOBRealAlignment)
				{
					ProcessActivity(KinFactionActivityTypes.FriendlyDeath);
				}
				else
				{
					ProcessActivity(KinFactionActivityTypes.Death);
				}
			}
			else if (m is BaseCreature)
			{
				if (((BaseCreature)m).Spawner is KinGuardPost)
				{
					ProcessActivity(KinFactionActivityTypes.GuardDeath);
				}
			}
		}

		/// <summary>
		/// Called when [enter].
		/// </summary>
		/// <param name="m">The m.</param>
		public void OnEnter(Mobile m)
		{
			if (!(m is PlayerMobile)) return;
			PlayerMobile pm = (PlayerMobile)m;
			ProcessVisitor(pm);

			if (CityData.ControlingKin == IOBAlignment.None)
			{
				pm.SendMessage(string.Format("Warning: You have entered the City of {0}.  This City is under the rule of the Golem Controller Lord.", City.ToString()));
			}
			else if (pm.IOBRealAlignment == CityData.ControlingKin)
			{
				pm.SendMessage(string.Format("You have entered the friendly City of {0}.", City.ToString()));
			}
			else if (pm.IOBRealAlignment == IOBAlignment.None && CityData.GuardOption != KinCityData.GuardOptions.LordBritish)
			{
				pm.SendMessage(string.Format("Warning: You have entered the City of {0}, under the rule of {1}.  This is a Kin Wars Zone, and you may not be safe here.", City.ToString(), IOBSystem.GetIOBName(CityData.ControlingKin)));
			}
			else if (CityData.CityLeader is PlayerMobile && pm == (PlayerMobile)CityData.CityLeader)
			{
				pm.SendMessage(string.Format("You have entered the City of {0}.  This City is under your control.  Welcome back, {1}.", City.ToString(), pm.Name));
			}
			else 
			{
				pm.SendMessage(string.Format("You have entered the City of {0}, under the rule of {1}.  This is a Kin Wars Zone.", City.ToString(), IOBSystem.GetIOBName(CityData.ControlingKin)));
			}
		}

		/// <summary>
		/// Called when [exit].
		/// </summary>
		/// <param name="m">The m.</param>
		public void OnExit(Mobile m)
		{
			//if (m is PlayerMobile) ProcessVisitor(m as PlayerMobile);
		}

		//	public void On

		/// <summary>
		/// Serializes the specified writer.
		/// </summary>
		/// <param name="writer">The writer.</param>
		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)3); // version
			writer.Write(m_LastAnnounceTime);
			writer.WriteMobileList<PlayerMobile>(new List<PlayerMobile>(m_ShopperQueue));
			writer.WriteMobileList<PlayerMobile>(new List<PlayerMobile>(m_VisitorQueue));
			writer.Write((int)m_City);
			writer.WriteItemList<ChampKinCity>(m_Champs);
		}

		/// <summary>
		/// Deserializes the specified reader.
		/// </summary>
		/// <param name="reader">The reader.</param>
		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 3:
					{
						m_LastAnnounceTime = reader.ReadDateTime();
						goto case 2;
					}

				case 2:
					{
						m_ShopperQueue = new Queue<PlayerMobile>(reader.ReadMobileList<PlayerMobile>());
						goto case 1;
					}
				case 1:
					{
						m_VisitorQueue = new Queue<PlayerMobile>(reader.ReadMobileList<PlayerMobile>());
						m_City = (KinFactionCities)reader.ReadInt();
						m_Champs = reader.ReadItemList<ChampKinCity>();
						goto case 0;
					}
				case 0:
					break;
			}

		}

		/// <summary>
		/// Registers champ.
		/// </summary>
		/// <param name="champ">The champ.</param>
		/// <returns></returns>
		public bool RegisterChamp(ChampKinCity champ)
		{
			if (m_Champs.Contains(champ)) return false;
			m_Champs.Add(champ);
			return true;
		}

		/// <summary>
		/// Unregister champ.
		/// </summary>
		/// <param name="champ">The champ.</param>
		/// <returns></returns>
		public bool UnRegisterChamp(ChampKinCity champ)
		{
			if (m_Champs.Contains(champ))
			{
				m_Champs.Remove(champ);
				return true;
			}
			return false;
		}
	}
}
