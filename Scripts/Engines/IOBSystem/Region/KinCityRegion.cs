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

/* Scripts\Engines\IOBSystem\Region\KinCityRegion.cs
 * CHANGELOG:
 *	04/27/09, plasma
 *		Added in event handler for KinCityManager.OnLBChangeWarning
 *	04/08/09, plasma
 *		Added in event handler for KinCityManager.OnGolemController
 *	04/07/09, plasma
 *		Added in event handler for KinCityManager.OnChangeGuards
 *	1/18/09, Adam
 *		Initial Creation
 */

using System;
using System.Collections;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Spells;
using Server.Mobiles;
using Server.Network;
using Server.Regions;

namespace Server.Engines.IOBSystem
{
	public class KinCityRegion : Server.Regions.CustomRegion
	{
		private KinFactionCities _kinFactionCity = KinFactionCities.Cove;

		/// <summary>
		/// Gets or sets the kin faction city.
		/// </summary>
		/// <value>The kin faction city.</value>
		[CommandProperty(AccessLevel.GameMaster)]
		public KinFactionCities KinFactionCity
		{
			get 
			{
				if (IsControllerGood())
				{
					return ((KinCityRegionStone)m_Controller).City;
				}
				return KinFactionCities.Cove;
			}
			set { _kinFactionCity = value; }
		}

		/// <summary>
		/// Gets the city tax rate.
		/// </summary>
		/// <value>The city tax rate.</value>
		public double CityTaxRate
		{
			get
			{
				if (IsControllerGood())
				{
					return ((KinCityRegionStone)m_Controller).CityTaxRate;
				}
				else
				{
					return 0.0;	 
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="KinCityRegion"/> class.
		/// </summary>
		/// <param name="townstone">The townstone.</param>
		/// <param name="map">The map.</param>
		public KinCityRegion(KinCityRegionStone townstone, Map map) : base(townstone, map)
		{
			Setup();
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="KinCityRegion"/> class.
		/// </summary>
		/// <param name="rc">The rc.</param>
		/// <param name="map">The map.</param>
		public KinCityRegion(RegionControl rc, Map map) : base(rc, map)
		{
			Setup();
		}

		/// <summary>
		/// Porcesses the activity.
		/// </summary>
		/// <param name="type">The activity type.</param>
		public void ProcessActivity(KinFactionActivityTypes type)
		{
			KinCityManager.ProcessActivityDelta(KinFactionCity, type);
		}

		/// <summary>
		/// Setups this instance.
		/// </summary>
		private void Setup()
		{
			this.IsGuarded = false;
			//Hookup self to OnChangeGuard event from the KinCityManager
			KinCityManager.OnChangeGuards += new KinCityManager.ChangeGuardsEventHandler(KinCityManager_OnChangeGuards);
			KinCityManager.OnGolemController += new KinCityManager.GolemControllerEventHandler(KinCityManager_OnGolemController);
			KinCityManager.OnLBChangeWarning += new KinCityManager.LBChangeWarningEventHandler(KinCityManager_OnLBChangeWarning);
		}

		void KinCityManager_OnLBChangeWarning(KinFactionCities city)
		{
			if (IsControllerGood() && city == KinFactionCity)
			{				
				//Deal with this one here because we need access to the mobiles collection
				if (Mobiles != null && Mobiles.Count > 0)
				{
					foreach (KeyValuePair<Serial,Mobile> kvp in Mobiles)
					{
						if (kvp.Value is PlayerMobile)
						{
							kvp.Value.SendMessage("Lord British's guards will be returning to active duty shortly!");
						}
					}
				}
			}
		}

		/// <summary>
		/// Kins the city manager_ on golem controller.
		/// </summary>
		/// <param name="city">The city.</param>
		/// <param name="on">if set to <c>true</c> [on].</param>
		private void KinCityManager_OnGolemController(KinFactionCities city, bool on)
		{
			if (IsControllerGood() && city == KinFactionCity) 
			{
				((KinCityRegionStone)m_Controller).KinCityManager_OnGolemController(on);
			}
		}

		/// <summary>
		/// Handles the guard change, will switch on/off the guards depending if the new guard option is Lord British
		/// </summary>
		/// <param name="city">The city.</param>
		/// <param name="guardOption">The guard option.</param>
		private void KinCityManager_OnChangeGuards(KinFactionCities city, KinCityData.GuardOptions guardOption)
		{
			if (IsControllerGood() && KinFactionCity == city)
			{
				((KinCityRegionStone)m_Controller).KinCityManager_OnChangeGuards(guardOption);
			}
		}

		/// <summary>
		/// Determines whether [is controller good].
		/// </summary>
		/// <returns>
		/// 	<c>true</c> if [is controller good]; otherwise, <c>false</c>.
		/// </returns>
		private bool IsControllerGood()
		{
			return (this.m_Controller != null && this.m_Controller is KinCityRegionStone);
		}

		/// <summary>
		/// Gets the kin city 
		/// </summary>
		/// <param name="m">The m.</param>
		/// <returns></returns>
		public static KinCityRegion GetKinCityAt(Mobile m)
		{
			CustomRegion c = CustomRegion.FindDRDTRegion(m);
			if (c is KinCityRegion)
			{
				return (KinCityRegion)c;
			}

			return null;
		}

		/// <summary>
		/// Gets the kin city
		/// </summary>
		/// <param name="m">The m.</param>
		/// <returns></returns>
		public static KinCityRegion GetKinCityAt(Map map, Point3D point)
		{
			CustomRegion c = CustomRegion.FindDRDTRegion(map, point);
			if (c is KinCityRegion)
			{
				return (KinCityRegion)c;
			}

			return null;
		}

		/// <summary>
		/// Gets the kin city.
		/// </summary>
		/// <param name="i">The i.</param>
		/// <returns></returns>
		public static KinCityRegion GetKinCityAt(Item i)
		{
			CustomRegion c = CustomRegion.FindDRDTRegion(i, i.Location);
			if (c is KinCityRegion)
			{
				return (KinCityRegion)c;
			}

			return null;
		}


		/// <summary>
		/// Called when [enter].
		/// </summary>
		/// <param name="m">The m.</param>
		public override void OnEnter(Mobile m)
		{
			if( IsControllerGood() )
			{
				//forward to controller, which keeps track of everything
				((KinCityRegionStone)this.m_Controller).OnEnter( m );
			}

			base.OnEnter (m);
		}

		/// <summary>
		/// Called when [exit].
		/// </summary>
		/// <param name="m">The m.</param>
		public override void OnExit(Mobile m)
		{
			if( IsControllerGood() )
			{
				//forward to controller, which keeps track of everything
				((KinCityRegionStone)this.m_Controller).OnExit( m );
			}

			base.OnExit (m);
		}

		/// <summary>
		/// Process vendor sales
		/// </summary>
		/// <param name="m">The m.</param>
		/// <param name="totalCost">The total cost.</param>
		public void OnVendorBuy(Mobile m, int totalTax)
		{
			if (IsControllerGood())
			{
				//forward to controller, which keeps track of everything
				((KinCityRegionStone)this.m_Controller).OnVendorBuy(m, totalTax);
			}
		}

		public override bool OnDeath(Mobile m)
		{
			if (IsControllerGood())
			{
				//forward to controller, which keeps track of everything
				((KinCityRegionStone)this.m_Controller).OnDeath(m);
			}
			return base.OnDeath(m);
		}

		/// <summary>
		/// Gets the KC stone.
		/// </summary>
		/// <value>The KC stone.</value>
		public KinCityRegionStone KCStone
		{
			get
			{
				if( IsControllerGood() )
				{
					return (KinCityRegionStone)this.m_Controller;
				}
				else
				{
					return null;
				}
			}
		}


	}
}
