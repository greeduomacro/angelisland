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

/* /Scripts/Engines/IOBSystem/Logging/KinLogging.cs	
 *	07/03/09, plasma
 *		Reduced the log sizes a bit by abbreivating the elements (more work to do here yet)
 *	01/14/09, plasma
 *		Initial creation
 */
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using Server;
using Server.Mobiles;
using Server.Engines.IOBSystem.Attributes;
using System.IO;

namespace Server.Engines.IOBSystem.Logging
{


	#region serializable data classes

	/// <summary>
	/// Data output from the heartbeat job that runs every two hours
	/// Note that XML serialization is ALWAYS version tolerant no matter what happens
	/// </summary>

	public class Player
	{

		public Player(PlayerMobile pm)
		{
			if (pm == null || pm.Deleted) return;

			PlayerName = pm.Name;
			PlayerGuild = pm.Guild.Name;
			PlayerGuildAbbreviation = pm.Guild.Abbreviation;
			PlayerIOB = pm.IOBRealAlignment.ToString();
		}

		public Player() { }

		[XmlElement(ElementName="Pn", Namespace="")]
		public string PlayerName;
		[XmlElement(ElementName="Pg", Namespace="")]
		public string PlayerGuild;
		[XmlElement(ElementName="Pga", Namespace="")]
		public string PlayerGuildAbbreviation;
		[XmlElement(ElementName="IOB", Namespace="")]
		public string PlayerIOB;
		[XmlElement(ElementName="V", Namespace="")]
		public double Value;
	}

	
	public class StringDoublePair
	{
		[XmlElement(ElementName="S")]
		public string Name;
		[XmlElement(ElementName="V")]
		public double Value;
	}
	
	public class ActivityProcessing
	{
		[XmlElement(ElementName="LT")]
		public DateTime LogTime;
		[XmlElement]
		public string Kin;
		[XmlElement]
		public string City;
		[XmlElement(ElementName = "PDT")]
		public DateTime PreviousDecayTime;
		[XmlElement(ElementName = "PP")]
		public int PointsProcessed;
		[XmlElement(ElementName = "TDM")]
		public int TimeDeltaMinutes;
		[XmlElement(ElementName = "NDT")]
		public DateTime NewDecayTime;
	}

	/// <summary>
	/// Represents one activity 
	/// </summary>
	public class ActivityGranular
	{
		[XmlElement(ElementName = "LT")]
		public DateTime LogTime;
		[XmlElement]
		public string Kin;
		[XmlElement]
		public string City;
		[XmlElement(ElementName = "AT")]
		public string ActivityType;
	}

	public class CaptureData
	{

		[XmlElement(ElementName = "LT")]
		public DateTime LogTime;
		[XmlElement]
		public string City;
		[XmlElement(ElementName = "WK")]
		public string WinningKin;
		[XmlElement(ElementName = "CT")]
		public DateTime CaptureTime;
		[XmlElement(ElementName = "TCP")]
		public double TotalCapturePoints;
		[XmlElement(ElementName = "TVD")]
		public int TotalVortexDamage;
		[XmlArray(ElementName = "IVD")]
		public List<Player> IndividualVortexDamage = new List<Player>();
		[XmlArray(ElementName = "IDP")]
		public List<Player> IndividualDefensePoints = new List<Player>();
		[XmlArray(ElementName = "ICP")]
		public List<Player> IndividualCapturePoints = new List<Player>();
		[XmlArray(ElementName = "KCP")]
		public List<StringDoublePair> KinCapturePoints = new List<StringDoublePair>();
		[XmlArray(ElementName = "KDS")]
		public List<StringDoublePair> KinDamageSpread = new List<StringDoublePair>();
		[XmlArray(ElementName = "BCP")]
		public List<Player> BeneficiariesCpaturePoints = new List<Player>();

	}

	public class DailyCitySummary
	{
		public DailyCitySummary(KinCityData data)
		{
			if (data == null) return;
			LogTime = DateTime.Now;
			Kin = data.ControlingKin.ToString();
			City = data.City.ToString();
			Leader = new Player(data.CityLeader as PlayerMobile);
			data.BeneficiaryDataList.ForEach(delegate(KinCityData.BeneficiaryData bd)
			{
				Beneficiaries.Add(new Player(bd.Pm));
			});
			Treasury = data.Treasury;
			TaxRate = data.TaxRate;
			NPCFlags = data.NPCCurrentFlags;
			GuardOption = (int)data.GuardOption;
			GuardPostSlots = string.Format("{0} | {1}", KinSystem.GetCityGuardPostSlots(data.City), data.UnassignedGuardPostSlots);
			foreach( KinCityData.BeneficiaryData bd in data.BeneficiaryDataList )
				foreach( Items.KinGuardPost gp in bd.GuardPosts )
					GuardPostData.Add( new GuardPost(gp));
		}

		public DailyCitySummary() { }

		[XmlElement(ElementName = "LT")]
		public DateTime LogTime;
		[XmlElement]
		public string Kin;
		[XmlElement]
		public string City;
		[XmlElement]
		public Player Leader;
		[XmlArray]
		public List<Player> Beneficiaries = new List<Player>();
		[XmlElement]
		public int Treasury;
		[XmlElement]
		public double TaxRate;
		[XmlElement]
		public long NPCFlags;
		[XmlElement]
		public int GuardOption;
		[XmlElement]
		///In the form of  total | assigned
		public string GuardPostSlots;
		[XmlArray]
		public List<GuardPost> GuardPostData = new List<GuardPost>();

	}

	public class PVPPoints
	{
		public PVPPoints(PlayerMobile pm)
		{
			Player = new Player(pm);
			Solo = pm.KinSoloPoints;
			Team = pm.KinTeamPoints;
			Power = pm.KinPowerPoints;
		}

		public PVPPoints() { }

		[XmlElement]
		public Player Player;
		[XmlElement(ElementName = "S")]
		public double Solo;
		[XmlElement(ElementName = "T")]
		public double Team;
		[XmlElement(ElementName = "P")]
		public double Power;
	}

	public class GuardPost
	{

		public GuardPost(Items.KinGuardPost gp)
		{
			if (gp == null || gp.Deleted) return;
			Owner = new Player(gp.Owner);
			Silver = gp.Silver;
			FightMode = (int)gp.FightMode;
			HireSpeed = (int)gp.HireSpeed;
			GuardType = gp.CreaturesName.Count > 0 ? gp.CreaturesName[0].ToString() : string.Empty;
		}
		
		public GuardPost() { }

		[XmlElement]
		public Player Owner;
		[XmlElement]
		public int Silver;
		[XmlElement(ElementName = "FM")]
		public int FightMode;
		[XmlElement(ElementName = "HS")]
		public int HireSpeed;
		[XmlElement(ElementName = "GT")]
		public string GuardType;
	}

	#endregion


	#region logger implementation

	[XmlRoot(Namespace="|")]
	public class KinFactionLogs : IXmlSerializable
	{

		#region singleton

		private static KinFactionLogs m_Instance = null;

		public static KinFactionLogs Instance
		{
			get { return m_Instance; }
		}

		static KinFactionLogs()
		{
			m_Instance = new KinFactionLogs();
		}

		#endregion

		public static void Initialize()
		{
			Commands.Register("KinLogs", AccessLevel.Administrator, new CommandEventHandler(OnCommand));
			EventSink.WorldLoad += new WorldLoadEventHandler(EventSink_WorldLoad);
			EventSink.WorldSave += new WorldSaveEventHandler(EventSink_WorldSave);
		}

		static void EventSink_WorldSave(WorldSaveEventArgs e)
		{
			Console.WriteLine("Account information Saving...");

			if (!Directory.Exists("Saves/KinFactions"))
				Directory.CreateDirectory("Saves/KinFactions");

			string filePath = Path.Combine("Saves/KinFactions", "pendingkinlogs.xml");
				/*
			if (File.Exists(filePath))
			{
				try
				{
					File.Delete(filePath);
				}
				catch (IOException ex)
				{
					System.Console.WriteLine("Caught exception in KinFactionLogs.Save whilst attempt to delete pendingkinlogs.xml: {0}", ex.Message);
					System.Console.WriteLine(ex.StackTrace);
				}
				catch (Exception ex)
				{
					System.Console.WriteLine("Caught exception in KinFactionLogs.Save whilst attempt to delete pendingkinlogs.xml: {0}", ex.Message);
					System.Console.WriteLine(ex.StackTrace);
				}

			}
					*/
			bool bNotSaved = true;
			int attempt = 0;
			while (bNotSaved && attempt < 3)
			{
				try
				{
					XmlSerializer xs = new XmlSerializer(typeof(KinFactionLogs));
					using (StreamWriter sw = new StreamWriter(filePath))
					{
						xs.Serialize(sw, m_Instance );
					}
					 
					bNotSaved = false;
				}
				catch (Exception ex)
				{
					//LogHelper.LogException(ex);
					System.Console.WriteLine("Caught exception in KinFactionLogs.Save: {0}", ex.Message);
					System.Console.WriteLine(ex.StackTrace);
					System.Console.WriteLine("Will attempt to recover three times.");
					attempt++;
				}
			}
		}

		static void EventSink_WorldLoad()
		{
			Console.Write("KinFactionLogData Loading...");

			string filePath = Path.Combine("Saves/KinFactions", "pendinglogs.xml");

			if (!File.Exists(filePath))
				return;

			XmlSerializer x = new XmlSerializer(typeof(KinFactionLogs));
			try
			{
				using (StreamReader sr = new StreamReader(filePath))
				{
					KinFactionLogs temp = (KinFactionLogs)x.Deserialize(sr);
					m_Instance.m_EntitesToSerialize = temp.m_EntitesToSerialize;	
				}
			}
			catch
			{
				Console.WriteLine();
				Console.WriteLine("Warning: KinFactionLogs instance load failed");
			}
		}

		public static void OnCommand(CommandEventArgs e)
		{
			KinCityManager.ProcessAndOutputLogs();
		}

		private List<object> m_EntitesToSerialize = new List<object>();

		public void AddEntityToSerialize(object entity)
		{
			if (entity != null)
			{
				m_EntitesToSerialize.Add(entity);
			}
		}

		public void XMLSerialize()
		{
			string fileName = string.Format(@"logs\KinFactionStats_{0}.xml", DateTime.Now.ToString().Replace("/", "-").Replace(":", "-"));
			try
			{
				XmlSerializer xs = new XmlSerializer(typeof(KinFactionLogs));
				using (System.IO.StreamWriter sw = new System.IO.StreamWriter(fileName))
				{
					xs.Serialize(sw, m_Instance);
				}
			}
			catch (XmlException ex)
			{
				EventSink.InvokeLogException(new LogExceptionEventArgs(ex));
				Console.WriteLine("Warning: KinFactionLogs XMLSerialize failed");
				//throw;
			}
			catch (Exception ex)
			{
				EventSink.InvokeLogException(new LogExceptionEventArgs(ex));
				Console.WriteLine("Warning: KinFactionLogs XMLSerialize failed");
				//throw;
			}
		}

		/// <summary>
		/// Clear entities waiting to be serialized
		/// </summary>
		public void ClearEntities()
		{
			//Clear stuff to be written
			m_EntitesToSerialize.Clear();
		}

		#region IXmlSerializable Members

		System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema()
		{
			return null;
		}

		void IXmlSerializable.ReadXml(XmlReader reader)
		{
			try
			{
				XmlSerializer x = new XmlSerializer(typeof(ActivityProcessing));
				m_EntitesToSerialize.Add(x.Deserialize(reader));
				x = new XmlSerializer(typeof(ActivityGranular));
				m_EntitesToSerialize.Add(x.Deserialize(reader));
				x = new XmlSerializer(typeof(CaptureData));
				m_EntitesToSerialize.Add(x.Deserialize(reader));
				x = new XmlSerializer(typeof(DailyCitySummary));
				m_EntitesToSerialize.Add(x.Deserialize(reader));
				x = new XmlSerializer(typeof(PVPPoints));
				m_EntitesToSerialize.Add(x.Deserialize(reader));
			}
			catch (XmlException ex)
			{
				EventSink.InvokeLogException(new LogExceptionEventArgs(ex));
				Console.WriteLine("Warning: KinFactionLogs IXmlSerializable.ReadXML failed");
			}
			catch (Exception ex)
			{
				EventSink.InvokeLogException(new LogExceptionEventArgs(ex));
				Console.WriteLine("Warning: KinFactionLogs IXmlSerializable.ReadXML failed");
			}
		}

		void IXmlSerializable.WriteXml(XmlWriter writer)
		{
			try
			{
				//sort the list by type name :)
				m_EntitesToSerialize.Sort(delegate(object entityA, object entityB)
				{
					return entityA.GetType().ToString().CompareTo(entityB.GetType().ToString());
				});
				foreach (object entity in m_EntitesToSerialize)
				{
					XmlSerializer xs = new XmlSerializer(entity.GetType());
					xs.Serialize(writer, entity);
				}
			}
			catch (XmlException ex)
			{
				EventSink.InvokeLogException(new LogExceptionEventArgs(ex));
				Console.WriteLine("Warning: KinFactionLogs IXmlSerializable.WriteXML failed");
			}
			catch (Exception ex)
			{
				EventSink.InvokeLogException(new LogExceptionEventArgs(ex));
				Console.WriteLine("Warning: KinFactionLogs IXmlSerializable.WriteXML failed");
			}
			
		}

		#endregion
	}

	#endregion
}

