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

/* Scripts/Engines/Spawner/SpawnerCache.cs
 * Changelog:
 *	04/27/09, plasma
 *		Add region name convert for nujelm and skarabrae to Nujel'm and Skara Brae
 *	04/18/09, plasma
 *		Was neccesary to include spawners without something spawned, for factions
 *	5/21/08, Adam
 *		Cleanup up console output to match other outout
 *	5/2/08, Adam
 *		Move the Region checks to where we try to select a spawner because regions have not been loaded at the time
 *		the spawner cache is loaded.
 *	4/24/08, Adam
 *		Fixed a bug in GetRandomSpawner() where we were dereferencing 'spawners' before it was initialized. 
 *		I replaced 'spawners' with 'random' and all seems good.
 *	03/17/08, plasma
 *		Initial creation
 */

using System;
using System.IO;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Regions;
using Server.Engines;
using Server.Scripts.Commands;
using System.Reflection;

namespace Server.Mobiles
{
	public static class SpawnerCache
	{

		public enum SpawnerType
		{
			None = 0,
			Overland
		}

		private static List<Spawner> m_Spawners = new List<Spawner>();

		public static List<Spawner> Spawners
		{
			get { return m_Spawners; }
		}

		public static void Configure()
		{
			EventSink.WorldLoad += new WorldLoadEventHandler(OnLoad);
		}

		public static void OnLoad()
		{
			Console.Write("Loading spawner cache...");
			int count = LoadSpawnerCache();
			Console.WriteLine("done ({0} spawners loaded.)", count.ToString());
		}

		public static Spawner GetRandomSpawner()
		{
			return GetRandomSpawner(SpawnerType.None);
		}

		public static Spawner GetRandomSpawner(SpawnerType type)
		{
			// if still empty, fail
			if (m_Spawners.Count == 0)
				return null;

			Spawner spawner = null;
			Utility.TimeCheck tc = new Utility.TimeCheck();
			tc.Start();
			//try to find an appropriate spawner
			for (int count = 0; count < m_Spawners.Count * 2; ++count)
			{
				// pick one at random..
				Spawner random = m_Spawners[Utility.Random(m_Spawners.Count)] as Spawner;
				Region region = Server.Region.Find(random.Location, Map.Felucca);

				// test if this spawner satisfies type required
				switch (type)
				{
					case SpawnerType.Overland:
						{
							// Must be running
							if (!random.Running)
								continue;

							if (region != null)
							{	// No Towns
								if (IsTown(region.Name))
									continue;

								// no green acres, inside houses, etc..
								if (IsValidRegion(random.Location, region) == false)
									continue;
							}

							break;
						}

					default:
						{
							if (region != null)
							{
								// no green acres, inside houses, etc..
								if (IsValidRegion(random.Location, region) == false)
									continue;
							}

							break;
						}
				}

				//this is a good candidate!
				spawner = random;
				break;
			}
			tc.End();
			if (tc.Elapsed() > 30)
			{
				LogHelper logger = new LogHelper("SpawnerCache");
				logger.Log("Warning:  Spawner overland search took " + tc.Elapsed().ToString() + "ms");
			}

			return spawner;

		}

		public static List<Spawner> GetSpawnersByRegion(string regionName)
		{
			if (regionName == "SkaraBrae") regionName = "Skara Brae";
			if (regionName == "Nujelm") regionName = "Nujel'm";
			Region region = Region.GetByName(regionName, Map.Felucca);
			return GetSpawnersByRegion(region);
		}

		public static List<Spawner> GetSpawnersByRegion(Region region)
		{
			if (region == null) return null;
			List<Spawner> spawners = new List<Spawner>();
			//time this search, log if it takes longer than .30 seconds
			Utility.TimeCheck tc = new Utility.TimeCheck();
			tc.Start();
			foreach (Spawner s in m_Spawners)
				if (Region.Find(s.Location, s.Map) == region)
					spawners.Add(s);
			tc.End();
			if (tc.Elapsed() > 30)
			{
				LogHelper logger = new LogHelper("SpawnerCache");
				logger.Log("Warning:  Spawner search by region for " + region.Name + " took " + tc.Elapsed().ToString() + "ms");
			}
			return spawners;
		}

		public static int LoadSpawnerCache()
		{
			// empty the list
			m_Spawners.Clear();
			foreach (Item item in World.Items.Values)
			{
				if (item is Server.Mobiles.Spawner)
				{
					Spawner spawner = item as Spawner;

					// felucca only
					if (spawner.Map == null || spawner.Map != Map.Felucca)
						continue;	// don't want one of these

					// make sure there's something spawned
					//if (spawner.Creatures.Count < 1)
					//	continue;

					// no spawning in water
					Tile landTile = spawner.Map.Tiles.GetLandTile(spawner.X, spawner.Y);
					if ((TileData.LandTable[landTile.ID & 0x3FFF].Flags & TileFlag.Impassable) != 0)
						continue;	// don't want one of these

					// we will exclude by regions during selection.
					// We can't exclude by region here because the regions have not been loaded yet
					m_Spawners.Add(spawner);
				}
			}

			// how many did we get?
			return m_Spawners.Count;
		}

		private static bool IsValidRegion(Point3D location, Region region)
		{
			// exclude all funky locations
			// if we cannot put a house there, and it isn't a town, then we don't want to spawn there
			if (region == null) return false;
			if (region.AllowHousing(new Mobile(), location) == false && !IsTown(region.Name)) return false;
			if (region is Server.Regions.AngelIsland) return false;
			if (region is GreenAcres) return false;
			if (region is HouseRegion) return false;
			if (region is Server.Regions.Jail) return false;
			return true;
		}

		private static bool IsTown(string regionName)
		{
			if (string.IsNullOrEmpty(regionName)) return false;

			//this will do for the time being, until all are drdt 
			switch (regionName)
			{
				case "Cove":
				case "Britain":
				case "Jhelom":
				case "Minoc":
				case "Trinsic":
				case "Vesper":
				case "Yew":
				case "Serpent's Hold":
				case "Skara Brae":
				case "Nujel'm":
				case "Moonglow":
				case "Magincia":
				case "Buccaneer's Den":
				case "Delucia":
				case "Papua":
					return true;
			}

			return false;

		}

	}



}