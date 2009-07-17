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

/* /Scripts/Engines/IOBSystem/KinCityManager.cs
 * CHANGELOG:
 *	07/03/09, plasma
 *		- Removed the -2 hours thing in the acitivty. I seem to be attempting to turn the hands of time myself rather than allowing that priviledge to the universe at large
 *	06/28/09, plasma
 *		- Added a check in ProcessSigils that skips activity offsets and the base -2 hours if the sigil has ScheduledMode set to true
 *	05/25/09, plasma
 *		- Hooked up the activty model amounts in the GetActivityDeltaAmount, lol.
 *		-	Changed tax method as it now only recieves the tax not the sale amount
 *	04/27/09, plasma
 *		- Changed unassigned guard post slots to use new attribute reflection method
 *		- Removed check that prevents controller cities being setup if they are already owned by controllers
 *			This is a pain when testing because you have to keep alternating between kin owned and GC to test
 *		- Made GCs absorb treasury if any exists when city falls to them
 *		-	Added LB change guards callback timer and refactored related logic
 *		- Added new event handler for LB guard change warning
 *	04/08/09, plasma
 *		Add / implement OnGolemController event
 *	04/07/09, plasma
 *		Add / implement OnChangeGuard event
 *	04/06/09, plasma
 *		Moved the voting methods into KinCityData.cs as they are more data-centric
 *	05/15/08: Plasma
 *		Moved vote processing to the heartbeat job and such
 *	04/09/08: Plasma
 *		Added NPC smitching on/off functionality
 *	01/08/08: Plasma
 *		Initial Version
 */

using System;
using System.IO;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Regions;
using Server.Mobiles;
using Server.Engines.IOBSystem.Logging;

namespace Server.Engines.IOBSystem
{
	#region Kin City Manager

	/// <summary>
	/// Provides methods for obtaining, modifying, loading and saving of Kin city ownership, rules and procedures
	/// </summary>
	public static class KinCityManager
	{

		public delegate void ChangeGuardsEventHandler(KinFactionCities city, KinCityData.GuardOptions guardOption);
		public delegate void GolemControllerEventHandler(KinFactionCities city, bool on);
		public delegate void LBChangeWarningEventHandler(KinFactionCities city);
		public static event ChangeGuardsEventHandler OnChangeGuards;
		public static event GolemControllerEventHandler OnGolemController;
		public static event LBChangeWarningEventHandler OnLBChangeWarning;

		//Holds all the city data
		private static Dictionary<KinFactionCities, KinCityData> _cityData = new Dictionary<KinFactionCities, KinCityData>();

		#region Reflection Methods

		/// <summary>
		/// Initializes this instance.
		/// </summary>
		public static void Initialize()
		{
			//Forces the heartbeat job
			Server.Commands.Register("SigilUpdate", AccessLevel.GameMaster, new CommandEventHandler(SigilUpdate_OnCommand));
		}

		/// <summary>
		/// Configures this instance.
		/// </summary>
		public static void Configure()
		{
			EventSink.WorldLoad += new WorldLoadEventHandler(OnLoad);
			EventSink.WorldSave += new WorldSaveEventHandler(OnSave);
		}

		#endregion

		#region Save/Load

		public static void OnSave(WorldSaveEventArgs e)
		{
			try
			{
				Console.WriteLine("KinCityManager Saving...");
				if (!Directory.Exists("Saves/AngelIsland"))
					Directory.CreateDirectory("Saves/AngelIsland");

				string filePath = Path.Combine("Saves/AngelIsland", "KinCityManager.bin");

				GenericWriter writer;
				writer = new BinaryFileWriter(filePath, true);

				writer.Write(1); //version

				//v1 below
				//write out the city data class'
				writer.Write(_cityData.Count);
				foreach (KeyValuePair<KinFactionCities, KinCityData> pair in _cityData)
					pair.Value.Save(writer);

				writer.Close();
			}
			catch (Exception ex)
			{
				System.Console.WriteLine("Error saving KinCityManager!");
				Scripts.Commands.LogHelper.LogException(ex);
			}
		}

		public static void OnLoad()
		{
			try
			{
				Console.WriteLine("KinCityManager Loading...");
				string filePath = Path.Combine("Saves/AngelIsland", "KinCityManager.bin");

				if (!File.Exists(filePath))
				{
					Console.Write("Kin faction city data file not found.  Generating default city data...");
					foreach (int city in Enum.GetValues(typeof(KinFactionCities)))
						_cityData.Add((KinFactionCities)city, new KinCityData((KinFactionCities)city));
					Console.WriteLine("done.");
					return;
				}

				BinaryFileReader datreader = new BinaryFileReader(new BinaryReader(new FileStream(filePath, FileMode.Open, FileAccess.Read)));
				int version = datreader.ReadInt();

				switch (version)
				{
					case 1:
						{
							int cityCount = datreader.ReadInt();
							if (cityCount > 0)
							{
								for (int i = 0; i < cityCount; ++i)
								{
									KinCityData data = new KinCityData(datreader);
									_cityData.Add(data.City, data);
								}
							}
							break;
						}
				}

				datreader.Close();
			}
			catch (Exception ex)
			{
				System.Console.WriteLine("Error loading KinCityManager!");
				Scripts.Commands.LogHelper.LogException(ex);
			}
		}
		#endregion

		#region Sigil Registering

		/// <summary>
		/// Attempts to register a KinSigil into the city
		/// </summary>
		/// <param name="pv"></param>
		/// <returns></returns>
		public static bool RegisterSigil(KinSigil sigil)
		{
			if (sigil == null)
				return false;

			foreach (KeyValuePair<KinFactionCities, KinCityData> pair in _cityData)
			{
				if (pair.Value.Sigil == sigil || (pair.Key == sigil.FactionCity && pair.Value.Sigil != null && !pair.Value.Sigil.Deleted))
				{
					return false;  //This sigil is already registered, or the city already has an active sigil registered
				}
				else if (pair.Key == sigil.FactionCity)
				{
					pair.Value.Sigil = sigil;
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Attempts to unregister a KinSigil from the relevant city.  Note this will not delete the sigil. 
		/// </summary>
		/// <param name="pv"></param>
		/// <returns></returns>
		public static bool UnRegisterSigil(KinSigil sigil)
		{
			if (sigil == null)
				return false;

			foreach (KeyValuePair<KinFactionCities, KinCityData> pair in _cityData)
			{
				if (pair.Value.Sigil == sigil)
				{
					pair.Value.Sigil = null;
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Ronseal
		/// </summary>
		/// <param name="sigil"></param>
		/// <returns></returns>
		public static bool IsSigilRegistered(KinSigil sigil)
		{
			if (sigil == null)
				return false;

			foreach (KeyValuePair<KinFactionCities, KinCityData> pair in _cityData)
				if (pair.Value.Sigil == sigil)
					return true;

			return false;
		}

		#endregion

		#region Info / Gets

		/// <summary>
		/// Returns the city data class for a given city
		/// </summary>
		/// <param name="city"></param>
		/// <returns></returns>
		public static KinCityData GetCityData(KinFactionCities city)
		{
			//check this city exists in the collection
			if (_cityData == null || (!_cityData.ContainsKey(city)))
				return null;
			return _cityData[city];
		}


		/// <summary>
		/// Determines if an area is within a capture area. (this will probably be deperecated)
		/// </summary>
		/// <param name="m">The mobile</param>
		/// <returns></returns>
		public static bool InCaptureArea(Mobile m)
		{
			if (m == null || m.Deleted)
				return false;

			CustomRegion cr = CustomRegion.FindDRDTRegion(m);
			if (cr != null)
			{
				RegionControl rc = cr.GetRegionControler();
				if (rc != null)
				{
					if (rc.CaptureArea)
					{
						return true;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Retuns a list of active vortexes within "defense" range of the pm
		/// </summary>
		/// <param name="pm"></param>
		/// <returns></returns>
		public static List<PowerVortex> GetVortexesInRange(PlayerMobile pm)
		{
			List<PowerVortex> pvs = new List<PowerVortex>();
			foreach (KeyValuePair<KinFactionCities, KinCityData> pair in _cityData)
			{
				KinSigil sigil = pair.Value.Sigil;
				if (sigil == null) continue;
				if (sigil.InCapturePhase())
				{
					Console.WriteLine("mobile's distance from vortex " + sigil.FactionCity.ToString() + " " + (pm.GetDistanceToSqrt(sigil.Vortex)).ToString());
					if (pm.GetDistanceToSqrt(sigil.Vortex) <= KinSystemSettings.CaptureDefenseRange)
					{
						pvs.Add(sigil.Vortex);
					}
				}
			}
			return pvs;
		}

		#endregion

		#region NPC Spawners

		/// <summary>
		///	GCs are toggled seperately as they are not a normal part of the townspeople
		/// </summary>
		/// <param name="city"></param>
		/// <param name="on"></param>
		public static void SetGolemControllers(KinFactionCities city, bool on)
		{
			//PLASMA: This is not being used atm as I wrote ChampKinCity to do this.
			List<Type> typesOn = new List<Type>();
			List<Type> typesOff = new List<Type>();

			if (on)
			{
				typesOn.Add(typeof(GolemController));
			}
			else
			{
				typesOff.Add(typeof(GolemController));
			}

			SetNPCSpawners(city, typesOn, typesOff);
		}

		/// <summary>
		/// Switches on/off the spawners in a given city's region where they spawn any of the provided types
		/// </summary>
		/// <param name="city"></param>
		/// <param name="types"></param>
		/// <param name="on"></param>
		private static void SetNPCSpawners(KinFactionCities city, List<Type> typesOn, List<Type> typesOff)
		{
			List<Spawner> regionSpawners = GetCitySpawners(city);
			if (regionSpawners == null || regionSpawners.Count == 0)
				return;

			List<Spawner> toSwitchOn = new List<Spawner>();	//Holds list of spawners that need changing
			List<Spawner> toSwitchOff = new List<Spawner>();	//Holds list of spawners that need changing

			foreach (Spawner spawner in regionSpawners)
			{
				bool found = false;
				//Check to see if the spawner contains any of the required NPC types to switch on
				foreach (Type t in typesOn)
				{
					foreach (string s in spawner.CreaturesName)
						if (t.Name.ToLower() == s)
						{
							found = true;
							if (spawner.Running) //already doing what it should be
								break;

							//Found a match, add this to the switch on list and break out of the loop
							toSwitchOn.Add(spawner);
							break;
						}
					//break out if found
					if (found)
						break;
				}

				//if not found, check against the off types
				if (!found)
				{
					//Check to see if the spawner contains any of the required NPC types to switch off
					foreach (Type t in typesOff)
					{
						foreach (string s in spawner.CreaturesName)
							if (t.Name.ToLower() == s)
							{
								found = true;
								if (!spawner.Running) //already doing what it should be
									break;

								//Found a match, add this to the switch off list and break out of the loop
								toSwitchOff.Add(spawner);
								break;
							}
						//break out if found
						if (found)
							break;
					}
				}
			}

			//switch on any required spawners
			foreach (Spawner spawner in toSwitchOn)
			{
				spawner.Start();
				spawner.Spawn();
			}

			//switch off any required spawners
			foreach (Spawner spawner in toSwitchOff)
			{
				//switch spawner off
				spawner.Stop();
				//delete the active mobiles and remove them from the creature list
				spawner.RemoveCreatures();
			}

		}

		/// <summary>
		/// Gets the city spawners.
		/// </summary>
		/// <param name="city">The city.</param>
		/// <returns></returns>
		private static List<Spawner> GetCitySpawners(KinFactionCities city)
		{
			return SpawnerCache.GetSpawnersByRegion(city.ToString());
		}

		/// <summary>
		/// Updates the city NPC spawners.
		/// </summary>
		/// <param name="city">The city.</param>
		public static void UpdateCityNPCSpawners(KinFactionCities city)
		{
			KinCityData data = GetCityData(city);
			if (data == null) //woo sanity
				return;

			List<Type> switchOn = new List<Type>();
			List<Type> switchOff = new List<Type>();
			List<Type> addTo = null;

			//Populate the spawners to always switch off (kin stuff, fighters, rangers, etc)
			switchOff.Add(typeof(Fighter));
			switchOff.Add(typeof(Ranger));
			switchOff.Add(typeof(Paladin));

			foreach (KinCityData.NPCFlags npcFlag in Enum.GetValues(typeof(KinCityData.NPCFlags)))
			{
				if (npcFlag == KinCityData.NPCFlags.None)
					continue;

				if (data.GetNPCFlag(npcFlag)) //set reference to relevant collection
					addTo = switchOn;
				else
					addTo = switchOff;

				switch (npcFlag) //add NPC types
				{
					case KinCityData.NPCFlags.Animal:
						{
							addTo.Add(typeof(AnimalTrainer));
							addTo.Add(typeof(Veterinarian));
							break;
						}
					case KinCityData.NPCFlags.Bank:
						{
							addTo.Add(typeof(Banker));
							addTo.Add(typeof(Minter));
							break;
						}
					case KinCityData.NPCFlags.Carpenter:
						{
							addTo.Add(typeof(GeneralContractor));
							addTo.Add(typeof(Carpenter));
							addTo.Add(typeof(RealEstateBroker));
							addTo.Add(typeof(Architect));
							addTo.Add(typeof(StoneCrafter));
							break;
						}
					case KinCityData.NPCFlags.EatDrink:
						{
							addTo.Add(typeof(Barkeeper));
							addTo.Add(typeof(Waiter));
							addTo.Add(typeof(Cook));
							addTo.Add(typeof(TavernKeeper));
							addTo.Add(typeof(Farmer));
							addTo.Add(typeof(Butcher));
							addTo.Add(typeof(Fisherman));
							break;
						}
					case KinCityData.NPCFlags.FightBroker:
						{
							addTo.Add(typeof(FightBroker));
							break;
						}
					case KinCityData.NPCFlags.Gypsy:
						{
							addTo.Add(typeof(GypsyTrader));
							break;
						}
					case KinCityData.NPCFlags.Healer:
						{
							addTo.Add(typeof(Healer));
							addTo.Add(typeof(EvilHealer));
							addTo.Add(typeof(HealerGuildmaster));
							break;
						}
					case KinCityData.NPCFlags.Inn:
						{
							addTo.Add(typeof(InnKeeper));
							break;
						}
					case KinCityData.NPCFlags.Mages:
						{
							addTo.Add(typeof(Mage));
							addTo.Add(typeof(MageGuildmaster));
							addTo.Add(typeof(Alchemist));
							addTo.Add(typeof(Herbalist));
							addTo.Add(typeof(HairStylist));
							break;
						}
					case KinCityData.NPCFlags.Misc:
						{
							addTo.Add(typeof(Tinker));
							addTo.Add(typeof(TinkerGuildmaster));
							addTo.Add(typeof(Furtrader));
							addTo.Add(typeof(Tanner));
							addTo.Add(typeof(BardGuildmaster));
							addTo.Add(typeof(Bard));
							addTo.Add(typeof(Bowyer));
							addTo.Add(typeof(MerchantGuildmaster));
							addTo.Add(typeof(Shipwright));
							addTo.Add(typeof(Mapmaker));
							addTo.Add(typeof(Scribe));
							addTo.Add(typeof(Jeweler));
							addTo.Add(typeof(Baker));
							addTo.Add(typeof(MinerGuildmaster));
							addTo.Add(typeof(WarriorGuildmaster));
							break;
						}
					case KinCityData.NPCFlags.Patrol:
						{
							addTo.Add(typeof(PatrolGuard));
							break;
						}
					case KinCityData.NPCFlags.Provisioner:
						{
							addTo.Add(typeof(Provisioner));
							addTo.Add(typeof(Cobbler));
							break;
						}
					case KinCityData.NPCFlags.Quest:
						{
							addTo.Add(typeof(BaseEscortable));
							addTo.Add(typeof(Noble));
							addTo.Add(typeof(SeekerOfAdventure));
							break;
						}
					case KinCityData.NPCFlags.Smith:
						{
							addTo.Add(typeof(Blacksmith));
							addTo.Add(typeof(BlacksmithGuildmaster));
							break;
						}
					case KinCityData.NPCFlags.Tailor:
						{
							addTo.Add(typeof(Tailor));
							addTo.Add(typeof(Weaver));
							addTo.Add(typeof(TailorGuildmaster));
							break;
						}
					case KinCityData.NPCFlags.TownCrier:
						{
							addTo.Add(typeof(TownCrier));
							break;
						}
					case KinCityData.NPCFlags.WeaponArmour:
						{
							addTo.Add(typeof(Armorer));
							addTo.Add(typeof(Weaponsmith));
							break;
						}

				}
			}

			SetNPCSpawners(city, switchOn, switchOff);

		}

		#endregion

		#region Ownership

		/// <summary>
		/// Transfers owership of a city to a kin or golem controller king
		/// </summary>
		/// <param name="city"></param>
		/// <param name="winners"></param>
		public static void TransferOwnership(KinFactionCities city, IOBAlignment kin, List<PlayerMobile> winners)
		{
			KinCityData cd = GetCityData(city);
			if (cd == null)
			{
				Console.WriteLine("Error in KinCityManager.TransferOwnership() - City Data not found");
				return;
			}

			//Set props that apply to both GC and Kin
			cd.CityLeader = null;
			cd.CaptureTime = DateTime.Now;
			cd.ClearAllGuardPosts();
			cd.ClearActivityDelta();

			if (kin == IOBAlignment.None) //GCs!
			{
				cd.ControlingKin = kin;
				//setup defaults for a controller city
				cd.IsVotingStage = false;
				cd.TaxRate = 0.0;
				cd.BeneficiaryDataList.Clear();
				cd.UnassignedGuardPostSlots = 0;
				//Absorb treasury
				cd.EmptyTreasury();
				ChangeGuards(city, KinCityData.GuardOptions.None, true);
				cd.ClearNPCFLags();
				//Update townspeople spawners and switch on GCs
				UpdateCityNPCSpawners(cd.City);
				if (OnGolemController != null)
				{
					OnGolemController(cd.City, true);
				}
			}
			else
			{
				//check to see if the city was previously owned by the controllers
				if (cd.ControlingKin == IOBAlignment.None)
				{
					//if so then apply default settings for a town
					cd.SetAllNPCFlags();
					ChangeGuards(city, KinCityData.GuardOptions.None, true);
					//Set last change time so they can change the guards immediately
					cd.LastGuardChangeTime = DateTime.Now.AddHours(-KinSystemSettings.GuardChangeTimeHours);
					//Update townspeople spawners and switch off GCs
					UpdateCityNPCSpawners(cd.City);
					SetGolemControllers(cd.City, false);
					if (OnGolemController != null)
					{
						OnGolemController(cd.City, false);
					}
				}
				else
				{
					cd.LastGuardChangeTime = DateTime.Now.AddHours(-KinSystemSettings.GuardChangeTimeHours);
				}

				cd.ControlingKin = kin;
				//Assign voting info
				cd.BeneficiaryDataList.Clear();
				foreach (PlayerMobile pm in winners)
					cd.BeneficiaryDataList.Add(new KinCityData.BeneficiaryData(pm, 0));

				//Change the guards to none if it is LB incase the new owners are all red
				if (cd.GuardOption == KinCityData.GuardOptions.LordBritish)
					ChangeGuards(cd.City, KinCityData.GuardOptions.None, true);

				//Skip voting if only one beneficiary
				if (cd.BeneficiaryDataList.Count == 1)
				{
					cd.CityLeader = cd.BeneficiaryDataList[0].Pm;
				}
				else
				{
					cd.IsVotingStage = true;
				}

				cd.UnassignedGuardPostSlots = KinSystem.GetCityGuardPostSlots(city);
				//Voting is controlled by heartbeat
			}

		}

		#endregion

		#region Heartbeat / Command

		/// <summary>
		/// Performs the heartbeat job on [sigilupdate
		/// </summary>
		/// <param name="args">The <see cref="Server.CommandEventArgs"/> instance containing the event data.</param>
		public static void SigilUpdate_OnCommand(CommandEventArgs args)
		{
			ProcessSigils();
		}

		/// <summary>
		/// Function called from heartbeat job
		/// </summary>
		public static void ProcessSigils()
		{

			//This job is called every two hours, and is responsible for updating the cities' next capture times based on their 
			//capture points. If a city will fall in less than four hours, its precise vortex spawn timer is started.

			//run if factions is on
			if (KinSystemSettings.CityCaptureEnabled)
			{
				//iterate through all the cities and update their capture time as neccesary
				foreach (KinFactionCities e in Enum.GetValues(typeof(KinFactionCities)))
				{
					KinCityData data = GetCityData(e);
					if (data == null)
						continue;  //should be impossible except "Other"
					Logging.ActivityProcessing act = new ActivityProcessing();
					act.City = data.City.ToString();
					act.Kin = data.ControlingKin.ToString();
					act.LogTime = DateTime.Now;

					//sanity
					if (data.Sigil == null || data.Sigil.Deleted)
					{
						Console.WriteLine("Warning: The faction city of " + e.ToString() + "'s sigil appears to have been deleted or not yet assigned.");
						continue;
					}

					if (!data.Sigil.Active)
						continue;

					//see if this city is in a voting stage which has now expired
					if (data.HasVotingStageExpired)
					{
						//process vote results and move on
						data.ProcessVotes();
					}
					/*
				//if not, but city is still within voting stage then ignore
				else if (data.IsVotingStage)
				{
					continue;
				}		*/

					//Ignore if a vortex is already spawned, or in the process of spawning
					if (data.Sigil.InCapturePhase() || data.Sigil.IsVortexSpawning())
						continue;

					//Update the control points / times
					//////////////////////////////////////////////////////////////////////////////
					//Plasma: Explanation on how this works!
					//////////////////////////////////////////////////////////////////////////////
					//A city starts off with 7 days on the clock. Each day passed will always reduce this by one day.
					//Each two hours, the actvity delta is taken which will be sitting at a max of +/- 100 
					//This is used to work out a percentage of pro or negative activity.
					//Note that pro activity counts for only a third that of negativity.  This levels the algorithm
					//as it is constantly declining anyway and thus is balanced.
					//////////////////////////////////////////////////////////////////////////////
					
					if (data.Sigil.NextEventTime == DateTime.MinValue) return;

					act.PointsProcessed = data.ActivityDelta;
					act.PreviousDecayTime = data.Sigil.NextEventTime;

					//plasma: skip all activity stuff if the sigil is in scheduled mode
					if (!data.Sigil.ScheduledMode)
					{
						//data.Sigil.NextEventTime = data.Sigil.NextEventTime.AddHours(-2.0);
						int activityDelta = data.ActivityDelta;
						int delayMinutes = 0;
						if (activityDelta > 0)
						{
							//Work out the slight additional increase that at max each beat will allow four extra days in delay
							delayMinutes = Convert.ToInt32(Math.Floor((double)(activityDelta / 100.0) * 36));
						}
						else
						{
							//Work out the possibly massive additional decrease (up to 2.6 hours EXTRA so thats 3.6 hours)
							delayMinutes = -Convert.ToInt32(Math.Floor((double)(activityDelta / 100.0) * 160));
						}

						//Reduce/increase minutes based on activity
						data.Sigil.NextEventTime = data.Sigil.NextEventTime.AddMinutes(delayMinutes);
					}

					act.NewDecayTime = data.Sigil.NextEventTime;
					act.TimeDeltaMinutes = (act.PreviousDecayTime - act.NewDecayTime).Minutes;
					KinFactionLogs.Instance.AddEntityToSerialize(act);

					//Wipe activity
					data.ClearActivityDelta();

					//If the city's new capture time is in under 4 hours, start vortex spawn timer
					TimeSpan t = data.Sigil.NextEventTime - DateTime.Now;
					if (t <= TimeSpan.FromHours(4))
						data.Sigil.StartVortexSpawnTimer();

				}

			}
			else
			{
				Console.WriteLine("Nothing to do.. Faction town capture not switched on in KinSettings");
			}
		}

		#endregion

		#region Guards

		/// <summary>
		/// Changes the guards.
		/// </summary>
		/// <param name="city">The city.</param>
		/// <param name="guardOption">The guard option.</param>
		public static void ChangeGuards(KinFactionCities city, KinCityData.GuardOptions guardOption)
		{
			ChangeGuards(city, guardOption, false);
		}

		/// <summary>
		/// Changes the guards.
		/// </summary>
		/// <param name="city">The city.</param>
		/// <param name="guardOption">The guard option.</param>
		public static void ChangeGuards(KinFactionCities city, KinCityData.GuardOptions guardOption, bool overrideTimeout)
		{
			KinCityData cityData = GetCityData(city);
			if (cityData == null) return;
			if (cityData.GuardOption == guardOption) return;

			if (!overrideTimeout)
			{
				if (DateTime.Now <= cityData.LastGuardChangeTime + TimeSpan.FromHours(KinSystemSettings.GuardChangeTimeHours))
					return;
			}
			cityData.LastGuardChangeTime = DateTime.Now;

			if (guardOption == KinCityData.GuardOptions.LordBritish)
			{
				LBGuardTimer timer = new LBGuardTimer(city);
				timer.Start();
			}
			else
			{
				cityData.GuardOption = guardOption;
				//Switch off patrol npc guard type
				cityData.SetNPCFlag(KinCityData.NPCFlags.Patrol, false);
				if (guardOption == KinCityData.GuardOptions.None)
				{
					cityData.ClearAllGuardPosts();
				}

				//Update existing guards with the new rules
				foreach (KinCityData.BeneficiaryData bd in cityData.BeneficiaryDataList)
					foreach (KinGuardPost kgp in bd.GuardPosts)
						if (kgp != null && !kgp.Deleted)
							kgp.UpdateExisitngGuards();

				//Raise event for the regions to sort themselves out with the new changes
				if (KinCityManager.OnChangeGuards != null)
				{
					KinCityManager.OnChangeGuards(city, guardOption);
				}
			}
		}

		#endregion

		#region Activity

		/// <summary>
		/// Processes the sale, adding the tax to the treasury.
		/// </summary>
		/// <param name="city">The city.</param>
		/// <param name="vendor">The vendor.</param>
		/// <param name="totalCost">The tax to add</param>
		public static void ProcessSale(KinFactionCities city, Mobile vendor, int totalTax)
		{
			KinCityData data = GetCityData(city);
			if (data.ControlingKin != IOBAlignment.None)
				data.AddToTreasury(totalTax);
		}

		/// <summary>
		/// Processes the activity delta. (Forwards the data on to the relevant city)
		/// </summary>
		/// <param name="city">The city.</param>
		/// <param name="delta">The delta.</param>
		public static void ProcessActivityDelta(KinFactionCities city, KinFactionActivityTypes type)
		{
			KinCityData data = GetCityData(city);
			if (data == null) return;
			//log that shit
			Logging.ActivityGranular act = new Server.Engines.IOBSystem.Logging.ActivityGranular();
			act.ActivityType = type.ToString();
			act.City = city.ToString();
			act.Kin = data.ControlingKin.ToString();
			act.LogTime = DateTime.Now;
			Logging.KinFactionLogs.Instance.AddEntityToSerialize(act);
			data.ProcessActivityDelta(GetActivityDeltaAmount(type));
		}


		/// <summary>
		/// Gets the activty delta amount.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns></returns>
		public static int GetActivityDeltaAmount(KinFactionActivityTypes type)
		{
			switch (type)
			{
				case KinFactionActivityTypes.FriendlyVisitor:
					return KinSystemSettings.A_F_Visitor;
				case KinFactionActivityTypes.Visitor:
					return KinSystemSettings.A_Visitor;
				case KinFactionActivityTypes.FriendlySale:
					return KinSystemSettings.A_F_Sale;
				case KinFactionActivityTypes.Sale:
					return KinSystemSettings.A_Sale;
				case KinFactionActivityTypes.GuardPostMaint:
					return KinSystemSettings.A_GPMaint;
				case KinFactionActivityTypes.GuardPostHire:
					return KinSystemSettings.A_GPHire;
				case KinFactionActivityTypes.GuardDeath:
					return KinSystemSettings.A_GDeath;
				case KinFactionActivityTypes.FriendlyDeath:
					return KinSystemSettings.A_F_Death;
				case KinFactionActivityTypes.Death:
					return KinSystemSettings.A_Death;
				case KinFactionActivityTypes.GCChampLevel:
					return KinSystemSettings.A_GCChampLevel;
				case KinFactionActivityTypes.GCDeath:
					return KinSystemSettings.A_GCDeath;
				default:
					break;
			}
			return 0;
		}
		#endregion

		#region Logging

		/// <summary>
		/// 
		/// </summary>
		public static void ProcessAndOutputLogs()
		{
			//Create daily summary for each city and add to log manager
			foreach (KeyValuePair<KinFactionCities, KinCityData> kvp in _cityData)
			{
				KinFactionLogs.Instance.AddEntityToSerialize(new DailyCitySummary(kvp.Value));
			}

			//Build list of pvp points to log
			foreach (Mobile m in World.Mobiles.Values)
			{
				if (m == null || !(m is PlayerMobile) || m.Deleted)
					continue;
				PlayerMobile pm = ((PlayerMobile)m);
				if (pm.IOBRealAlignment == IOBAlignment.None)
					continue;
				if (pm.KinPowerPoints > 0 || pm.KinSoloPoints > 0 || pm.KinTeamPoints > 0)
				{
					KinFactionLogs.Instance.AddEntityToSerialize(new PVPPoints(pm));
				}
			}

			//Serialize & Clear logs
			KinFactionLogs.Instance.XMLSerialize();
			KinFactionLogs.Instance.ClearEntities();
		}

		#endregion

		#region LB Guard callback timer thing

		private class LBGuardTimer : Timer
		{
			int m_Cycle = 0;											//What's going on at the moment
			KinFactionCities m_City = KinFactionCities.Cove;

			public LBGuardTimer(KinFactionCities city)
				: base(TimeSpan.FromMinutes(1))
			{
				m_City = city;
			}

			protected override void OnTick()
			{
				if (m_Cycle != 5)
				{
					//Display warning message
					if (KinCityManager.OnLBChangeWarning != null)
					{
						KinCityManager.OnLBChangeWarning(m_City);
						++m_Cycle;
						Start();
					}
				}
				else
				{
					//Switch on LB guards
					KinCityData cityData = KinCityManager.GetCityData(m_City);
					if (cityData != null)
					{
						//Don't change the guards if it's somehow now owned by the GCs
						if (cityData.ControlingKin == IOBAlignment.None) return;
						cityData.GuardOption = KinCityData.GuardOptions.LordBritish;
						cityData.ClearAllGuardPosts();
						//Switch on patrol npc guard type
						cityData.SetNPCFlag(KinCityData.NPCFlags.Patrol, true);
						//Raise event for the regions to sort themselves out with the new changes
						if (KinCityManager.OnChangeGuards != null)
						{
							KinCityManager.OnChangeGuards(m_City, KinCityData.GuardOptions.LordBritish);
						}
					}
				}
			}
		}

		#endregion
	}

	#endregion

}
