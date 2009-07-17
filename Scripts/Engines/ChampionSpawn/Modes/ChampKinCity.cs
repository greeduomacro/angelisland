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

/* Scripts/Engines/ChampionSpawn/Modes/ChampKinCity.cs
 *	ChangeLog:
 *	04/28/2009, plasma
 *		Fixed a logic bug and removed a bit of redundant code
 *  04/27/2009, plasma
 *		Upgraded spawning logic
 *	04/07/2009, plasma
 *		Initial creation
 * 
 **/
using System;
using System.Collections;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Engines.IOBSystem;

namespace Server.Engines.ChampionSpawn
{	
	public class ChampKinCity: ChampEngine
	{
		private Queue<Spawner> m_CitySpawners = new Queue<Spawner>();

		/// <summary>
		/// Gets the kin city region.
		/// </summary>
		/// <value>The kin city region.</value>
		private KinCityRegion KinCityRegion
		{
			get
			{
				//Try and find out what faction city this is in 
				return KinCityRegion.GetKinCityAt(this);
			}
		}

		/// <summary>
		/// Activates this instance.
		/// </summary>
		protected override void Activate()
		{
			if (KinCityRegion != null)
			{
				//Add the city spawners into a queue..
				RefreshSpawnerList();
			}
			base.Activate();
		}

		private void RefreshSpawnerList()
		{
			//reload spawner list
			m_CitySpawners.Clear();
			m_CitySpawners = new Queue<Spawner>(GetSpawnersInRange());
		}

		

		private List<Spawner> GetSpawnersInRange()
		{
			//based on level data, which has range scale applied to it
			List<Spawner> results = new List<Spawner>();
			IPooledEnumerable eable = this.GetItemsInRange(Lvl_MaxRange);
			try
			{
				if (eable != null)
				{
					foreach (Item i in eable)
					{
						if (i is Spawner)
						{
							results.Add((Spawner)i);
						}
					}
				}
			}
			finally
			{
				eable.Free();
				eable = null;
			}
			return results;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ChampKinCity"/> class.
		/// </summary>
		[Constructable]
		public ChampKinCity() : base()
		{
			// gfx off
			Graphics = false;
			// and restart timer for a few of seconds
			m_bRestart = true;
			SpawnType = ChampLevelData.SpawnTypes.KinCity;
			m_RestartDelay = TimeSpan.FromSeconds(5);
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="ChampKinCity"/> class.
		/// </summary>
		/// <param name="serial">The serial.</param>
		public ChampKinCity(Serial serial)
			: base(serial)
		{
		}

		 #region serialize

		/// <summary>
		/// Serializes the specified writer.
		/// </summary>
		/// <param name="writer">The writer.</param>
		public override void Serialize(GenericWriter writer)
		{
			base.Serialize (writer);

			writer.Write( (int) 1 );
			writer.WriteItemList<Spawner>(new List<Spawner>(m_CitySpawners));
		}

		/// <summary>
		/// Deserializes the specified reader.
		/// </summary>
		/// <param name="reader">The reader.</param>
		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch ( version )
			{
				case 1:
					{
						m_CitySpawners = new Queue<Spawner>(reader.ReadItemList<Spawner>());
						goto case 0;
					}
				case 0:break;
			}
		}
		 #endregion

		/// <summary>
		/// Called when [single click].
		/// </summary>
		/// <param name="from">From.</param>
		public override void OnSingleClick(Mobile from)
		{
			if( from.AccessLevel >= AccessLevel.GameMaster )
			{			
				// this is a gm, allow normal text from base and champ indicator
				LabelTo( from, "Faction City Champ" );
				base.OnSingleClick(from);
			}
		}

		/// <summary>
		/// Advances the level.
		/// </summary>
		protected override void AdvanceLevel()
		{
			base.AdvanceLevel();

			//Provide activity
			if (KinCityRegion != null)
			{
				KinCityRegion.ProcessActivity(KinFactionActivityTypes.GCChampLevel);
				//reload spawner list
				RefreshSpawnerList(); 
			}
		}

		/// <summary>
		/// Preps the mob.
		/// </summary>
		/// <param name="m">The m.</param>
		protected override void PrepMob(Mobile m)
		{
			BaseCreature bc = m as BaseCreature;
			if (bc != null)
			{
				//use the first spawner in the queue here as it would have just been Peek()'d by GetSpawnLocation
				Spawner s = null;
				if (m_CitySpawners.Count > 0)
					s = m_CitySpawners.Dequeue();
				
				bc.Tamable = false;
				bc.Home = s == null ? Location : s.Location;
				bc.RangeHome = 10;
				bc.NavDestination = m_NavDest;

				//if we have a navdestination as soon as we spawn start on it
				if (bc.NavDestination != NavDestinations.None)
					bc.AIObject.Think();

					if (s != null) m_CitySpawners.Enqueue(s);
			}
		}

		protected override Point3D GetSpawnLocation(Mobile m)
		{
			//Map map = Map;

			CanFitFlags flags = CanFitFlags.requireSurface;
			if (m != null && m.CanSwim == true) flags |= CanFitFlags.canSwim;
			if (m != null && m.CantWalk == true) flags |= CanFitFlags.cantWalk;

			if (Map == null)
				return Location;

			Spawner s = null;
			if (m_CitySpawners.Count > 0)
				s = m_CitySpawners.Peek(); //don't remove it yet, PrepMob wants to use it first.

			if (s != null)
			{
				// Try 10 times to find a spawnable location near the next spawner.
				for (int i = 0; i < 10; i++)
				{
					int x;
					int y;
					
					x = (Utility.Random((s.X + (Utility.RandomBool() ? 10 : -10))) - s.X);
					y = (Utility.Random((s.Y + (Utility.RandomBool() ? 10 : -10))) - s.Y);
					
					int z = Map.GetAverageZ(x, y);

					if (Map.CanSpawnMobile(new Point2D(x, y), this.Z, flags) && !NearPlayer(new Point3D(x, y, this.Z)))
						return new Point3D(x, y, this.Z);
					if (Map.CanSpawnMobile(new Point2D(x, y), z, flags) && !NearPlayer(new Point3D(x, y, z)))
						return new Point3D(x, y, z);
				}
			}
			// Try 10 more times to find a any spawnable location.
			for (int i = 0; i < 10; i++)
			{
				int x;
				int y;
			
				x = Location.X + (Utility.Random((Lvl_MaxRange * 2) + 1) - Lvl_MaxRange);
				y = Location.Y + (Utility.Random((Lvl_MaxRange * 2) + 1) - Lvl_MaxRange);
		
				int z = Map.GetAverageZ(x, y);

				if (Map.CanSpawnMobile(new Point2D(x, y), this.Z, flags))
					return new Point3D(x, y, this.Z);
				if (Map.CanSpawnMobile(new Point2D(x, y), z, flags))
					return new Point3D(x, y, z);

			}

		

			// Experimental, property-based, error reporting.
			//  cannot find a valid location to spawn this creature
			Lvl_LevelError = LevelErrors.No_Location;

			return Location;
		}

		/// <summary>
		/// Called when [delete].
		/// </summary>
		public override void OnDelete()
		{
			IOBSystem.KinCityRegion region = IOBSystem.KinCityRegion.GetKinCityAt(Map, Location);
			if (region != null)
			{
				((KinCityRegionStone)region.GetRegionControler()).UnRegisterChamp(this);
			}
			base.OnDelete();
		}

		/// <summary>
		/// Called when [location change].
		/// </summary>
		/// <param name="oldLoc">The old loc.</param>
		public override void OnLocationChange(Point3D oldLoc)
		{
			IOBSystem.KinCityRegion region = IOBSystem.KinCityRegion.GetKinCityAt(Map == Map.Internal == true ? Map.Felucca : Map ,oldLoc);
			if (region != null)
			{
				((KinCityRegionStone)region.GetRegionControler()).UnRegisterChamp(this);
			}

			base.OnLocationChange(oldLoc);

			region = IOBSystem.KinCityRegion.GetKinCityAt(Map == Map.Internal == true ? Map.Felucca : Map, Location);
			if (region != null)
			{
				((KinCityRegionStone)region.GetRegionControler()).RegisterChamp(this);
				this.SendMessage("Champ successfully registered to the city of" + region.Name);
			}

			if (KinCityRegion != null)
			{
				//Add the city spawners into a queue..
				RefreshSpawnerList();
			}
			
		}
	}
}
