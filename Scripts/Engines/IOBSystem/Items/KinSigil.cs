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

/* Engines/IOBSystem/Items/KinSigil.cs
 * CHANGELOG:
 *	06/30/09, plasma
 *		- Added summon masters to the battle calculations
 *		- Changed scout reports to work with all cities not just those owned by your kin
 *	06/27/09, plasma
 *		Added scout reports to show decay time
 *		Added v1 serial and m_ScheduledMode
 *	05/25/09, plasma
 *		Added annoucements
 *	11/11/08, plasma
 *		Added RemoveDefensePoints() method
 *	04/02/08, plasma
 *		Changed a IOBAlignment to IOBRealAlignment that was missed
 *	07/26/08, plasma
 *		Initial creation
 */

using System;
using System.Collections.Generic;
using Server;
using Server.Guilds;
using Server.Mobiles;
using Server.Engines.IOBSystem;
using Server.Engines.IOBSystem.Logging;

namespace Server.Items
{
	public class KinSigil : Item
	{

		#region members

		private bool m_Active;									//Controls whether the timers will start or not
		private DateTime m_Time;								//Next vortex spawn time, or current vortex expire time
		private KinFactionCities m_City;				//Faction city this sigil is assigned to
		private Point3D m_VortexSpawnLocation;	//Location for the vortex to spawn at
		private PowerVortex m_Vortex;						//Power vortex from this sigil 
		private Timer m_Timer = null;						//Multi purpose timer
		private bool m_ScheduledMode = false;		//If this is true then the activity model will be ignored totally

		private Dictionary<PlayerMobile, double> m_DefensePoints  //defense capture points 
			= new Dictionary<PlayerMobile, double>();

		#endregion

		#region properties


		/// <summary>
		/// If set to true then the acitivty model well be ignored
		/// </summary>
		[CommandProperty(AccessLevel.GameMaster)]
		public bool ScheduledMode
		{
			get { return m_ScheduledMode; }
			set
			{
				m_ScheduledMode = value;
			}
		}

		/// <summary>
		/// Gets or sets the next event time.
		/// </summary>
		/// <value>The next event time.</value>
		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime NextEventTime
		{
			get { return m_Time; }
			set
			{
				//You can only set/adjust the time when not in the capture phase.
				if (m_Active && !InCapturePhase())
					m_Time = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="KinSigil"/> is active.
		/// </summary>
		/// <value><c>true</c> if active; otherwise, <c>false</c>.</value>
		[CommandProperty(AccessLevel.GameMaster)]
		public bool Active
		{
			get { return m_Active; }
			set
			{
				if (m_Active == value) return;


				if (value)
				{

					//If capture is not on that don't allow switch on
					if (!KinSystemSettings.CityCaptureEnabled)
					{
						SendMessageToLocalGM("Kin faction city capture is not currently switched on.");
						m_Active = false;
						return;
					}

					//if this sigil hasn't been registered yet then don't allow 
					if (KinCityManager.IsSigilRegistered(this) == false)
					{
						SendMessageToLocalGM("This sigil is yet to be registered to a city.");
						m_Active = false;
						return;
					}

					//Check the vortex has a spawn location
					if (VortexSpawnLocation == new Point3D())
					{
						SendMessageToLocalGM("Please first assign a vortex spawn location.");
						m_Active = false;
						return;
					}

					//some sanity checks to make sure the vortex is not around (should be impossible)										
					if (m_Vortex != null && !m_Vortex.Deleted)
						m_Vortex.Delete();

					//set the new spawn time, the heartbeat will pick it up from there
					SetNewSpawnTime();
				}
				else
				{
					//if switched off, kill timer and if capture phase then remove vortex					
					if (m_Timer != null && m_Timer.Running)
						m_Timer.Running = false;
					if (InCapturePhase())
						m_Vortex.Delete();
				}

				m_Active = value;
			}
		}

		/// <summary>
		/// Gets or sets the vortex spawn location.
		/// </summary>
		/// <value>The vortex spawn location.</value>
		[CommandProperty(AccessLevel.GameMaster)]
		public Point3D VortexSpawnLocation
		{
			get { return m_VortexSpawnLocation; }
			set { m_VortexSpawnLocation = value; }
		}

		/// <summary>
		/// Gets or sets the vortex.
		/// </summary>
		/// <value>The vortex.</value>
		[CommandProperty(AccessLevel.GameMaster)]
		public PowerVortex Vortex
		{
			get { return m_Vortex; }
			set { m_Vortex = value; }
		}

		/// <summary>
		/// Gets or sets the faction city.
		/// </summary>
		/// <value>The faction city.</value>
		[CommandProperty(AccessLevel.GameMaster)]
		public KinFactionCities FactionCity
		{
			get { return m_City; }
			set
			{
				//check if the new city already has a sigil registered
				KinCityData data = KinCityManager.GetCityData(value);
				if (data == null)
					return; //should be impossible

				if (data.Sigil != null && !data.Sigil.Deleted)
				{
					SendMessageToLocalGM("That City already has a Sigil registered with it.");
					return;
				}

				//unregister this sigil if it's registered elsewhere already
				if (KinCityManager.IsSigilRegistered(this))
					KinCityManager.UnRegisterSigil(this);

				m_City = value;

				//Now register the sigil with the manager class
				if (KinCityManager.RegisterSigil(this) == false)
				{
					SendMessageToLocalGM("Sigil failed to register!");
					return;
				}
				SendMessageToLocalGM("Sigil successfully registed to the city of " + m_City.ToString() + ".");
			}
		}

		#endregion

		#region ctors / dtors

		/// <summary>
		/// Initializes a new instance of the <see cref="KinSigil"/> class.
		/// </summary>
		[Constructable]
		public KinSigil()
			: base(0x1869)
		{
			Movable = false;
			Light = LightType.WestBig;


		}

		/// <summary>
		/// Initializes a new instance of the <see cref="KinSigil"/> class.
		/// </summary>
		/// <param name="serial">The serial.</param>
		public KinSigil(Serial serial)
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
			TimeSpan ts = TimeSpan.Zero;
			bool timerActive = false;

			base.Deserialize(reader);

			int version = reader.ReadInt();
			switch (version)
			{
				case 1:
					{
						m_ScheduledMode = reader.ReadBool();
						goto case 0;
					}
				case 0:
					{
						m_City = (KinFactionCities)reader.ReadInt();
						m_Vortex = (PowerVortex)reader.ReadMobile();
						ts = reader.ReadDeltaTime() - DateTime.Now;
						m_VortexSpawnLocation = reader.ReadPoint3D();
						m_Active = reader.ReadBool();
						timerActive = reader.ReadBool();

						break;
					}
			}

			if (timerActive)
			{
				//Sort out the timer state: 
				//Whilst in capture phase, _time will represent the expire time of the current vortex, so start timer with new delay
				if (InCapturePhase())
				{
					if (ts == TimeSpan.Zero)
					{
						//something weird happened, start expire timer afresh from kinsettings
						m_Time = DateTime.Now + TimeSpan.FromHours(KinSystemSettings.VortexExpireMinutes);
					}
					else
					{
						m_Time = DateTime.Now + ts;
					}

					if (m_Time < DateTime.Now)
					{
						//Vortex should have expired already, set it for 1 minute
						ts = TimeSpan.FromMinutes(1);
					}
					m_Timer = new VortexExpireTimer(ts, this);
				}
				else //if there's no vortex then the time represents the next spawn time
				{
					//Apply the delta and create a timer
					m_Time = DateTime.Now + ts;
					m_Timer = new VortexSpawnTimer(this);
				}
				//Only start the timer if Active 				
				if (m_Active)
					m_Timer.Start();

			}
			else
			{
				m_Time = DateTime.Now + ts;
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
			writer.Write(m_ScheduledMode);				//Scheduled mode
			//pla: v0
			writer.Write((int)m_City);						//City 
			writer.Write(m_Vortex);								//Vortex mob
			writer.WriteDeltaTime(m_Time);				//Delta time for current phase
			writer.Write(m_VortexSpawnLocation);	//Vortex spawn location
			writer.Write(m_Active);								//Active
			writer.Write((m_Timer != null ? m_Timer.Running : false)); //Timer state
		}

		#endregion

		#region overrides from Item

		public override void OnDoubleClick(Mobile from)
		{
			base.OnDoubleClick(from);
			if (!(from is PlayerMobile))
				return;

			PlayerMobile pm = ((PlayerMobile)from);

			if (pm.IOBRealAlignment == IOBAlignment.None) return;

			KinCityData cd = KinCityManager.GetCityData(FactionCity);
			if (cd == null) return;

			from.SendMessage("Target the silver that you wish to hire the scout party with");
			from.Target = new ScoutingSilverTarget(this);

		}

		/// <summary>
		/// Called when [single click].
		/// </summary>
		/// <param name="from">From.</param>
		public override void OnSingleClick(Mobile from)
		{
			//Show which city this sigil relates to
			if (KinCityManager.IsSigilRegistered(this))
			{
				string city = Enum.GetName(typeof(KinFactionCities), m_City);
				LabelTo(from, "Kin Sigil for the City of " + city);
			}
			else
			{
				LabelTo(from, "An unused Kin Sigil");
			}

			//If this is a GM also show which phase the sigil is in and the time to the next
			//vortex spawn, or the expire time of the current vortex
			if (from.AccessLevel >= AccessLevel.GameMaster)
			{
				if (!KinCityManager.IsSigilRegistered(this))
				{
					LabelTo(from, "[Not assigned to a city]");
					return;
				}

				if (m_Active)
				{
					if (InCapturePhase())
					{
						LabelTo(from, "Capture phase in progress!");
						LabelTo(from, "Vortex expire time : " + m_Time.ToLongDateString() + " " + m_Time.ToLongTimeString());
					}
					else
					{
						if (IsVortexSpawning())
						{
							LabelTo(from, "Final stage active.");
							LabelTo(from, "Vortex will spawn at : " + m_Time.ToLongDateString() + " " + m_Time.ToLongTimeString());
						}
						else
						{
							LabelTo(from, "Awaiting next heartbeat.");
							LabelTo(from, "Current next spawn time : " + m_Time.ToLongDateString() + " " + m_Time.ToLongTimeString());
						}

					}
				}
				else
				{
					LabelTo(from, "Currently Inactive.  Please switch me on!");
				}
			}
		}

		/// <summary>
		/// Called when [delete].
		/// </summary>
		public override void OnDelete()
		{
			base.OnDelete();
			if (KinCityManager.IsSigilRegistered(this))
				KinCityManager.UnRegisterSigil(this);
		}

		#endregion

		#region methods

		/// <summary>
		/// Sets the new spawn time.
		/// </summary>
		private void SetNewSpawnTime()
		{
			if (m_Timer != null && m_Timer.Running)
				m_Timer.Stop();

			m_Time = DateTime.Now + TimeSpan.FromMinutes(KinSystemSettings.BaseCaptureMinutes);
		}

		/// <summary>
		/// Starts a new vortex spawner timer for the remaining time left
		/// </summary>
		public void StartVortexSpawnTimer()
		{
			if (m_Timer != null && m_Timer.Running)
				m_Timer.Stop();

			m_Timer = new VortexSpawnTimer(this);
			m_Timer.Start();
		}

		/// <summary>
		/// Retunrs true if in capture phase (vortex is spawned)
		/// </summary>
		/// <returns></returns>
		public bool InCapturePhase()
		{
			return (m_Active && m_Vortex != null && m_Vortex.Alive && !m_Vortex.Deleted);
		}

		/// <summary>
		/// Determines whether the vortex is in its spawning stage.
		/// </summary>
		/// <returns>
		/// 	<c>true</c> if vortex is spawning; otherwise, <c>false</c>.
		/// </returns>
		public bool IsVortexSpawning()
		{
			return (m_Active && m_Timer is VortexSpawnTimer && m_Timer.Running);
		}

		/// <summary>
		/// Called if the vortex was not killed quick enough
		/// </summary>
		public void VortexTimeOut()
		{
			if (m_Vortex != null && !m_Vortex.Deleted) //sanity
			{
				m_Vortex.TimedOut = true;
				m_Vortex.Kill();
				//Hand town over to golem controllers and set next spawn time
				KinCityManager.TransferOwnership(m_City, IOBAlignment.None, null);
				SetNewSpawnTime();
			}
		}

		/// <summary>
		/// Called when the vortex is sucessfully killed
		/// All the points calculation and processing happens here
		/// </summary>
		public void VortexDeath()
		{
			CaptureData logData = new CaptureData();
			logData.City = m_City.ToString();
			logData.CaptureTime = DateTime.Now;
			logData.LogTime = DateTime.Now;

			//turn off expire timer
			if (m_Timer != null && m_Timer.Running)
				m_Timer.Stop();

			if (KinSystemSettings.OutputCaptureData)
				SendMessageToLocalGM(string.Format("Vortex for the city of {0} has been destroyed", m_City.ToString()));

			Dictionary<PlayerMobile, double> individualCapturePoints				//dual purpose damage dictionary and capture points 
					= new Dictionary<PlayerMobile, double>();
			Dictionary<IOBAlignment, int> kinDamageSpread										//kin damage spread dictionary
					= new Dictionary<IOBAlignment, int>();
			KinCapturePointList kinCapturePoints = new KinCapturePointList();			//final kin capture point list
			KinCapturePointList finalCapturePoints = new KinCapturePointList();		//final individual capture point list

			IOBAlignment winningKin = IOBAlignment.None;										//Winning kin
			double capturePointsPerVortexDamage = 0.0;											//Standardised capture points per vortex damage point
			double capturePointsPerDefensePoint = 0.0;											//Standardised capture points per defense point 
			double totalCapturePoints = 0.0;																//total capture points
			int totalVortexDamage = 0;																			//total vortex damage

			//First, cycle thru the vortex's damage list and populate individual and kin damage dictionaries
			foreach (DamageEntry de in m_Vortex.DamageEntries)
			{
				PlayerMobile pm = null;
				//Check this is a real factioner or a factioner's pet
				if (de.Damager is PlayerMobile && ((PlayerMobile)de.Damager).IsRealFactioner)
				{
					pm = ((PlayerMobile)de.Damager);
				}
				else if (de.Damager is BaseCreature)
				{
					BaseCreature bc = ((BaseCreature)de.Damager);
					if (bc.ControlMaster != null && bc.ControlMaster is PlayerMobile && ((PlayerMobile)bc.ControlMaster).IsRealFactioner)
						pm = ((PlayerMobile)bc.ControlMaster);
					else if (bc.Summoned && bc.SummonMaster != null && bc.SummonMaster is PlayerMobile && ((PlayerMobile)bc.SummonMaster).IsRealFactioner)
						pm = ((PlayerMobile)bc.SummonMaster);
				}

				if (pm == null)
					continue;

				//add this player and the damage to the dictionary, to be converted into capture points in the next stage
				if (individualCapturePoints.ContainsKey(pm))
					individualCapturePoints[pm] += de.DamageGiven;
				else
					individualCapturePoints.Add(pm, de.DamageGiven);

				//keep running total of all damage 
				totalVortexDamage += de.DamageGiven;

				//also add this damage to the kin dictionary for later
				if (!kinDamageSpread.ContainsKey(pm.IOBRealAlignment))
					kinDamageSpread.Add(pm.IOBRealAlignment, de.DamageGiven);
				else
					kinDamageSpread[pm.IOBRealAlignment] += de.DamageGiven;
			}

			//add anyone else from the defense points into the damage list, with 0 damage
			foreach (PlayerMobile pm in m_DefensePoints.Keys)
				if (!individualCapturePoints.ContainsKey(pm))
					individualCapturePoints.Add(pm, 0.0);

			//add up defense points, this will act as the total amount of capture points avaliable
			foreach (KeyValuePair<PlayerMobile, double> pair in m_DefensePoints)
				totalCapturePoints += pair.Value;

			//Make sure we have at least 1 capture point to work with
			if (totalCapturePoints == 0) totalCapturePoints = 1;

			//standardise each damage point of the vortex damage so the vortex is worth KinSystemSettings % of the points
			capturePointsPerVortexDamage = ((totalCapturePoints * KinSystemSettings.VortexCaptureProportion) / totalVortexDamage);

			//the remainder contributes to defenders
			capturePointsPerDefensePoint = ((totalCapturePoints * (1.0 - KinSystemSettings.VortexCaptureProportion) / totalCapturePoints));

			//output data thus far to local GM's journal 
			if (KinSystemSettings.OutputCaptureData)
			{
				//Individual damage
				SendMessageToLocalGM("Individual damage to vortex:");
				foreach (KeyValuePair<PlayerMobile, double> kvp in individualCapturePoints)
				{
					Player p = new Player(kvp.Key);
					p.Value = kvp.Value;
					logData.IndividualVortexDamage.Add(p);
					SendMessageToLocalGM(string.Format("Player {0} inflicted {1} points of damage", kvp.Key.Name, kvp.Value));
				}

				//Total damage
				SendMessageToLocalGM(string.Format("Total damage inflicted on vortex: {0}", totalVortexDamage));
				logData.TotalVortexDamage = totalVortexDamage;
				//Damage split by Kin
				SendMessageToLocalGM("Kin damage to vortex:");
				foreach (KeyValuePair<IOBAlignment, int> kvp in kinDamageSpread)
				{
					StringDoublePair k = new StringDoublePair();
					k.Name = kvp.Key.ToString();
					k.Value = kvp.Value;
					logData.KinDamageSpread.Add(k);
					SendMessageToLocalGM(string.Format("Kin {0} inflicted {1} points of damage", kvp.Key.ToString(), kvp.Value));
				}
				//Individual defense points
				SendMessageToLocalGM("Individual defense points earnt:");
				foreach (KeyValuePair<PlayerMobile, double> kvp in m_DefensePoints)
				{
					Player p = new Player(kvp.Key);
					p.Value = kvp.Value;
					logData.IndividualDefensePoints.Add(p);
					SendMessageToLocalGM(string.Format("Player {0} earnt {1} defense points", kvp.Key.Name, kvp.Value));
				}

				//Total defense points
				SendMessageToLocalGM(string.Format("Total defense (capture) points earnt: {0}", totalCapturePoints));
				logData.TotalCapturePoints = totalCapturePoints;

				//Calculated points
				SendMessageToLocalGM(string.Format("Capture points per vortex damage : {0}", capturePointsPerVortexDamage));
				SendMessageToLocalGM(string.Format("Capture points per defense point : {0}", capturePointsPerDefensePoint));
			}


			//Reformat the individual list into capture points. 
			List<PlayerMobile> pms = new List<PlayerMobile>(); //Temp list
			foreach (PlayerMobile pm in individualCapturePoints.Keys)
				pms.Add(pm);

			foreach (PlayerMobile pm in pms)
			{
				//reformat damage points
				individualCapturePoints[pm] *= capturePointsPerVortexDamage;
				if (m_DefensePoints.ContainsKey(pm))
				{
					//Add any more capture points from defense
					individualCapturePoints[pm] += (m_DefensePoints[pm] * capturePointsPerDefensePoint);
				}
				//add this player's total capture points towards kin total
				kinCapturePoints.AddPoints(pm.IOBRealAlignment, individualCapturePoints[pm]);
			}

			//Free up temp list!
			pms.Clear(); pms = null;

			//Sort kin capture points (desc)
			kinCapturePoints.Sort();

			//More output data
			if (KinSystemSettings.OutputCaptureData)
			{
				SendMessageToLocalGM("Total individual capture points:");
				foreach (KeyValuePair<PlayerMobile, double> kvp in individualCapturePoints)
				{
					Player p = new Player(kvp.Key);
					p.Value = kvp.Value;
					logData.IndividualCapturePoints.Add(p);
					SendMessageToLocalGM(string.Format("Player {0} earnt {1} total capture points", kvp.Key.Name, kvp.Value));
				}

				SendMessageToLocalGM("Total kin capture points:");
				foreach (KinCapturePoints points in kinCapturePoints)
				{
					StringDoublePair k = new StringDoublePair();
					k.Name = points.Obj.ToString();
					k.Value = points.Points;
					logData.KinCapturePoints.Add(k);
					SendMessageToLocalGM(string.Format("Kin {0} earnt {1} total capture points", points.Obj.ToString(), points.Points));
				}
			}

			//Now find a winner - but the winning kin must also have done at least x% of the vortex damage
			for (int i = 0; i < kinCapturePoints.Count; ++i)
			{
				IOBAlignment kin = IOBAlignment.None;
				if (kinCapturePoints[i].Obj is IOBAlignment)
					kin = (IOBAlignment)kinCapturePoints[i].Obj;

				int damage = 0;

				if (kinDamageSpread.ContainsKey(kin))
					damage = kinDamageSpread[kin];
				else
					damage = 0;

				if (damage >= (totalVortexDamage * KinSystemSettings.VortexMinDamagePercentage))
				{
					//found our winner	
					winningKin = kin;
					break;
				}
				else
				{
					if (KinSystemSettings.OutputCaptureData)
						SendMessageToLocalGM(string.Format("Kin {0} would have won, but didn't do enough damage to the vortex", kin.ToString()));
				}

			}

			logData.WinningKin = winningKin.ToString();

			if (winningKin == IOBAlignment.None || winningKin == IOBAlignment.Healer || winningKin == IOBAlignment.OutCast)
			{
				//this shouldn't really happen.
				//Hand city over to golem controllers
				KinCityManager.TransferOwnership(m_City, IOBAlignment.None, null);
			}

			//Find and move all of this kin into the final capture point list
			foreach (KeyValuePair<PlayerMobile, double> pair in individualCapturePoints)
				if (pair.Key.IOBRealAlignment == winningKin)
					finalCapturePoints.AddPoints(pair.Key, pair.Value);

			//woo sanity
			if (finalCapturePoints.Count == 0)
			{
				//this should never happen.
				//Hand city over to golem controllers
				KinCityManager.TransferOwnership(m_City, IOBAlignment.None, null);
			}

			//Sort capture points list desc
			finalCapturePoints.Sort();

			int totalBenes = 0;

			//now we want the top x% of this list to act as beneficiaries for the town, with a cap
			if (finalCapturePoints.Count < KinSystemSettings.BeneficiaryCap)
			{
				totalBenes = finalCapturePoints.Count;
			}
			else
			{
				//plasma: removing the qualification % for now, it's too inhibitive with a little amount of players.
				totalBenes = KinSystemSettings.BeneficiaryCap;
				//totalBenes = (int)Math.Round((double)finalCapturePoints.Count * KinSystemSettings.BeneficiaryQualifyPercentage, 0);
				//Should never happen, but possible depending on the KinSystemSettings variables
				if (totalBenes < 1)
					totalBenes = 1;
			}

			//Send message to winners
			List<PlayerMobile> winners = new List<PlayerMobile>();
			for (int i = 0; i < totalBenes; ++i)
				winners.Add(finalCapturePoints[i].Obj as PlayerMobile);

			//Send message to all
			KinSystem.SendKinMessage(string.Format("The City of {0} has fallen to the {1}! ", m_City.ToString(), winningKin == IOBAlignment.None ? "Golem Controller Lord" : IOBSystem.GetIOBName(winningKin)));

			winners.ForEach(delegate(PlayerMobile pm)
			{
				pm.SendMessage("You have qualified as a beneficiary of {0}.  Head to the city's control board to vote for a City leader.", m_City.ToString());
			});

			if (KinSystemSettings.OutputCaptureData)
			{
				SendMessageToLocalGM("Final beneficiaries of town:");
				foreach (PlayerMobile pm in winners)
				{
					Player p = new Player(pm);
					logData.BeneficiariesCpaturePoints.Add(p);
					SendMessageToLocalGM(pm.Name);
				}
			}

			//Hand city over to the kin and its beneficiaries
			KinCityManager.TransferOwnership(m_City, winningKin, winners);

			m_DefensePoints.Clear();

			if (KinSystemSettings.OutputCaptureData)
				KinFactionLogs.Instance.AddEntityToSerialize(logData);

			//Fianlly set the base next vortex spawn time for this city/sigil 
			SetNewSpawnTime();
		}

		public void AddDefensePoints(PlayerMobile pm, double points)
		{
			if (pm == null)
				return;

			if (m_DefensePoints.ContainsKey(pm))
			{
				m_DefensePoints[pm] += points;
			}
			else
			{
				m_DefensePoints.Add(pm, points);
			}
		}

		public bool RemoveDefensePoints(PlayerMobile pm, double points)
		{
			if (pm == null)
				return false;

			if (m_DefensePoints.ContainsKey(pm) && m_DefensePoints[pm] > 0.0)
			{
				m_DefensePoints[pm] -= points;
				if (m_DefensePoints[pm] < 0) m_DefensePoints[pm] = 0.0;
				return true;
			}

			return false;
		}


		private void SendMessageToLocalGM(string message)
		{
			foreach (Mobile m in this.GetMobilesInRange(10))
				if (m.AccessLevel >= AccessLevel.GameMaster)
					m.SendMessage(message);
		}

		/// <summary>
		/// Called if the vortex is deleted (!)
		/// </summary>
		public void VortexDelete()
		{
			if (m_Vortex == null) return;
			//turn off expire timer
			if (m_Timer != null && m_Timer.Running)
				m_Timer.Stop();

			//Hand city over to golem controllers
			//KinCityManager.TransferOwnership(m_City, IOBAlignment.None, null);
			m_DefensePoints.Clear();
			SetNewSpawnTime();
		}

		/// <summary>
		/// Spawns a new power vortex at the _vortexSpawnLocation
		/// Starts the vortex expire timer.
		/// </summary>
		public void SpawnVortex()
		{
			//switch off scheduled mode..
			m_ScheduledMode = false;
			//clear points from last time just in case
			m_DefensePoints.Clear();
			m_Vortex = new PowerVortex(this);
			m_Vortex.MoveToWorld(m_VortexSpawnLocation, this.Map);
			//Start vortex expire timer			
			m_Timer = new VortexExpireTimer(TimeSpan.FromMinutes(KinSystemSettings.VortexExpireMinutes), this);
			m_Timer.Start();
			//Set the time variable to the end time so we can preserve on server restart
			m_Time = DateTime.Now + TimeSpan.FromMinutes(KinSystemSettings.VortexExpireMinutes);
			//Announce!
			KinSystem.SendKinMessage(string.Format("A power vortex targeting the City of {0} has appeared!", m_City.ToString()));
		}

		#endregion

		#region timers

		/// <summary>
		/// Deals with the delay to spawn the vortex.
		/// Handles displaying messages indicating progress toward vortex spawn
		/// </summary>
		private class VortexSpawnTimer : Timer
		{
			private KinSigil _sigil;

			/// <summary>
			/// Initializes a new instance of the <see cref="VortexSpawnTimer"/> class.
			/// </summary>
			/// <param name="sigil">The sigil.</param>
			public VortexSpawnTimer(KinSigil sigil)
				: base(TimeSpan.FromMinutes(0)) //just a default, set in the ctor
			{
				//This timer gets started once, and it will call itself once every half the timespan that is left.
				//This goes on until the timer will fire in less than 10 seconds, it then stops displaying messages.
				_sigil = sigil;
				SetNextAnnouceDelay();
			}

			/// <summary>
			/// Sets the next annouce delay.
			/// </summary>
			private void SetNextAnnouceDelay()
			{
				//calculate and set next change time
				TimeSpan t = (_sigil.m_Time) - DateTime.Now;
				Delay = TimeSpan.FromMinutes(t.TotalMinutes / 2);
			}

			/// <summary>
			/// Called when [tick].
			/// </summary>
			protected override void OnTick()
			{
				//just in case
				if (_sigil == null || _sigil.Deleted)
					return;

				if (_sigil.m_Time < DateTime.Now)
				{
					//vortex spawning time
					_sigil.SpawnVortex();
				}
				else
				{
					//TODO: This will not "Hum" but will start to emit stronger light source.
					if (Delay > TimeSpan.FromSeconds(10))
					{
						_sigil.PublicOverheadMessage(Server.Network.MessageType.Emote, 0xFF, true, "*hums*"); //temp
						//Calculate next event time
						SetNextAnnouceDelay();
					}
					//Restart the timer
					Start();
				}
			}
		}

		/// <summary>
		/// Deals the vortex expire time once spawned
		/// </summary>
		private class VortexExpireTimer : Timer
		{
			private KinSigil _sigil;

			public VortexExpireTimer(TimeSpan delay, KinSigil sigil)
				: base(delay)
			{
				_sigil = sigil;
			}

			protected override void OnTick()
			{
				if (_sigil == null || _sigil.Deleted)  //just in case
					return;

				//vortex expired
				//indicate timeout to the sigil
				_sigil.VortexTimeOut();
			}
		}

		#endregion

		#region capture point class and list

		/// <summary>
		/// General purpose capture point 
		/// </summary>
		private class KinCapturePoints : IComparable<KinCapturePoints>  //Have this as a class to avoid value type stuff
		{
			private object _obj;			//kin align or playermob
			private double _points;

			public object Obj
			{
				get { return _obj; }
				set { _obj = value; }
			}

			public double Points
			{
				get { return _points; }
				set { _points = value; }
			}

			/// <summary>
			/// Player ctor
			/// </summary>
			/// <param name="pm"></param>
			/// <param name="points"></param>
			public KinCapturePoints(Object obj, double points)
			{
				this._obj = obj;
				this._points = points;
			}

			/// <summary>
			/// Sorts descending
			/// </summary>
			/// <param name="other"></param>
			/// <returns></returns>
			public int CompareTo(KinCapturePoints other)
			{
				return -_points.CompareTo(other.Points);
			}

			public void AddPoints(double points)
			{
				this._points += points;
			}
		}

		//plasma:  I did have this all using generics, but it didnt't work too well as 
		//we need to store reference OR value types as the T (pm/kin)

		private class KinCapturePointList : List<KinCapturePoints>
		{
			public void AddPoints(PlayerMobile pm, double points)
			{
				foreach (KinCapturePoints point in this)
					if (point.Obj is PlayerMobile && (PlayerMobile)point.Obj == pm)
					{
						point.AddPoints(points);
						return;
					}
				Add(new KinCapturePoints(pm, points));
			}

			public void AddPoints(IOBAlignment kin, double points)
			{
				foreach (KinCapturePoints point in this)
					if (point.Obj is IOBAlignment && (IOBAlignment)point.Obj == kin)
					{
						point.AddPoints(points);
						return;
					}
				Add(new KinCapturePoints(kin, points));
			}

		}

		#endregion

		#region silver target for scouting report

		private class ScoutTimer : Timer
		{
			private Mobile m_From = null;
			private string m_Message = string.Empty;

			public ScoutTimer(Mobile from, string message)
				: base(TimeSpan.FromMinutes(3))
			{
				m_From = from;
				m_Message = message;
			}

			protected override void OnTick()
			{
				base.OnTick();
				m_From.SendMessage(m_Message);
			}
		}

		private class ScoutingSilverTarget : Targeting.Target
		{
			KinSigil m_KinSigil = null;

			public ScoutingSilverTarget(KinSigil sigil)
				: base(1, false, Server.Targeting.TargetFlags.None)
			{
				if (sigil == null || sigil.Deleted) return;
				m_KinSigil = sigil;
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				base.OnTarget(from, targeted);

				if (from.GetDistanceToSqrt(m_KinSigil.Location) > 3)
				{
					from.SendMessage("You are too far away to do that.");
					return;
				}

				if (!(targeted != null && targeted is Silver))
				{
					from.SendMessage("You may only pay with silver");
					return;
				}

				Silver silver = (Silver)targeted;

				if (silver.Amount < 499)
				{
					from.SendMessage("You need at least 500 silver for a scouting report.");
					return;
				}

				int totalAmount = silver.Amount;
				silver.Delete();

				int newAmount = totalAmount - 500;
				int modifier = (int)Math.Round((double)(newAmount / 100), 0.0);

				//75% chance for correct results with 0 mod
				// add 5% for each whole mod point
				if (modifier > 5) modifier = 5;  //25% max  (1k silver)

				string scoutReport = string.Empty;
				int days = GetDaysUntilSpawn();
				if (!Utility.RandomChance(75 + (5 * modifier)))
				{

					//25% chance to be two days off
					if (Utility.RandomChance(25))
					{
						days += (Utility.RandomBool() ? 2 : -2);
					}
					else //otherwise one day
					{
						days += (Utility.RandomBool() ? 1 : -1);
					}

					if (days < 0) days = 0;
					if (days < 10) days = 10;
				}

				scoutReport = string.Format("Your scouts report that in its current standing, the City of {0} has about {1} days before it falls.", m_KinSigil.FactionCity.ToString(), days);

				if (Utility.RandomChance(5))
				{
					scoutReport = "Your scout party did not make it back alive.";
				}

				from.SendMessage("Your scouts will report back within a few minutes.");
				ScoutTimer timer = new ScoutTimer(from, scoutReport);
				timer.Start();
			}

			private int GetDaysUntilSpawn()
			{
				KinCityData cd = KinCityManager.GetCityData(m_KinSigil.FactionCity);
				if (cd == null) return 0;
				TimeSpan ts = (DateTime.Now - m_KinSigil.NextEventTime);
				return Math.Abs(ts.Days);
			}

		}


		#endregion
	}

}
