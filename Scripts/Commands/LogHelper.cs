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

/* Scripts/Commands/LogHelper.cs
 * ChangeLog
 *	07/23/08, weaver
 *		Automated IPooledEnumerable optimizations. 1 loops updated.
 *	8/28/07, Adam
 *		Add new EventSink for ItemAdded via [add
 *		Dedesign LogHelper EventSink logic to be static and not instance based.
 *  3/28/07, Adam
 *      Add protections around Cheater()
 *  3/26/07, Adam
 *      Limit game console output to the first 25 items
 *	01/07/07 - Pix
 *		Added new LogException override: LogException(Exception ex, string additionalMessage)
 *	10/20/06, Adam
 *		Put back auto-watchlisting and comments from Cheater logging.
 *		Removed auto-watchlisting and comments from TrackIt logging.
 *	10/20/06, Pix
 *		Removed auto-watchlisting and comments from Cheater logging.
 *	10/17/06, Adam
 *		Add new Cheater() logging functions.
 *	9/9/06, Adam
 *		- Add Name and Serial for type Item display
 *		- normalized LogType.Item and LogType.ItemSerial
 *  01/09/06, Taran Kain
 *		Added m_Finished, Crashed/Shutdown handlers to make sure we write the log
 *  12/24/05, Kit
 *		Added ItemSerial log type that adds serial number to standered item log type.
 *	11/14/05, erlein
 *		Added extra function to clear in-memory log.
 *  10/18/05, erlein
 *		Added constructors with additional parameter to facilitate single line logging.
 *	03/28/05, erlein
 *		Added additional parameter for Log() to allow more cases
 *		where generic item and mobile logging can take place.
 *		Normalized format of common fields at start of each log line.
 *	03/26/05, erlein
 *		Added public interface to m_Count via Count so can add
 *		allowance for headers & footers.
 *	03/25/05, erlein
 *		Updated to log decimal serials instead of hex.
 *		Replaced root type name output with serial for items
 *		with mobile roots.
 *	03/23/05, erlein
 *		Initial creation
 */

using System;
using System.IO;
using System.Collections;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Accounting;

namespace Server.Scripts.Commands
{
	public class LogHelper
	{
		private ArrayList m_LogFile;
		private string m_LogFilename;
		private int m_MaxOutput = 25;   // only display first 25 lines
		private int m_Count;
		private static ArrayList m_OpenLogs = new ArrayList();
		public static ArrayList OpenLogs { get { return m_OpenLogs; } }

		public int Count
		{
			get
			{
				return m_Count;
			}
			set
			{
				m_Count = value;
			}
		}

		private bool m_Overwrite;
		private bool m_SingleLine;
		private DateTime m_StartTime;
		private bool m_Finished;

		private Mobile m_Person;


		// Construct with : LogHelper(string filename (, Mobile mobile ) (, bool overwrite) )

		// Default append, no mobile constructor
		public LogHelper(string filename)
		{
			m_Overwrite = false;
			m_LogFilename = filename;
			m_SingleLine = false;

			Start();
		}

		// Mob spec. constructor
		public LogHelper(string filename, Mobile from)
		{
			m_Overwrite = false;
			m_Person = from;
			m_LogFilename = filename;
			m_SingleLine = false;

			Start();
		}

		// Overwrite spec. constructor
		public LogHelper(string filename, bool overwrite)
		{
			m_Overwrite = overwrite;
			m_LogFilename = filename;
			m_SingleLine = false;

			Start();
		}

		// Overwrite and singleline constructor
		public LogHelper(string filename, bool overwrite, bool sline)
		{
			m_Overwrite = overwrite;
			m_LogFilename = filename;
			m_SingleLine = sline;

			Start();
		}

		// Overwrite + mobile spec. constructor
		public LogHelper(string filename, Mobile from, bool overwrite)
		{
			m_Overwrite = overwrite;
			m_Person = from;
			m_LogFilename = filename;
			m_SingleLine = false;

			Start();
		}

		// Overwrite, mobile spec. and singleline constructor
		public LogHelper(string filename, Mobile from, bool overwrite, bool sline)
		{
			m_Overwrite = overwrite;
			m_Person = from;
			m_LogFilename = filename;
			m_SingleLine = sline;

			Start();
		}

		private static int m_LogExceptionCount = 0;
		public static void LogException(Exception ex)
		{
			if (m_LogExceptionCount++ > 100)
				return;

			try
			{
				LogHelper Logger = new LogHelper("Exception.log", false);
				string text = String.Format("{0}\r\n{1}", ex.Message, ex.StackTrace);
				Logger.Log(LogType.Text, text);
				Logger.Finish();
				Console.WriteLine(text);
			}
			catch
			{
				// do nothing here as we do not want to enter a "cycle of doom!"
				//  Basically, we do not want the caller to catch an exception here, and call
				//  LogException() again, where it throws another exception, and so forth
			}
		}

		public static void LogException(Exception ex, string additionalMessage)
		{
			try
			{
				LogHelper Logger = new LogHelper("Exception.log", false);
				string text = String.Format("{0}\r\n{1}\r\n{2}", additionalMessage, ex.Message, ex.StackTrace);
				Logger.Log(LogType.Text, text);
				Logger.Finish();
				Console.WriteLine(text);
			}
			catch
			{
				// do nothing here as we do not want to enter a "cycle of doom!"
				//  Basically, we do not want the caller to catch an exception here, and call
				//  LogException() again, where it throws another exception, and so forth
			}
		}

		public static void Cheater(Mobile from, string text)
		{
			try
			{
				Cheater(from, text, false);
			}
			catch (Exception ex) { EventSink.InvokeLogException(new LogExceptionEventArgs(ex)); }
		}

		public static void Cheater(Mobile from, string text, bool accomplice)
		{
			if (from is PlayerMobile == false)
				return;

			// log what's going on
			TrackIt(from, text, accomplice);

			//Add to watchlist
			(from as PlayerMobile).WatchList = true;

			//Add comment to account
			Account a = (from as PlayerMobile).Account as Account;
			if (a != null)
				a.Comments.Add(new AccountComment("System", text));
		}

		public static void TrackIt(Mobile from, string text, bool accomplice)
		{
			LogHelper Logger = new LogHelper("Cheater.log", false);
			Logger.Log(LogType.Mobile, from, text);
			if (accomplice == true)
			{
				IPooledEnumerable eable = from.GetMobilesInRange(24);
				foreach (Mobile m in eable)
				{
					if (m is PlayerMobile && m != from)
						Logger.Log(LogType.Mobile, m, "Possible accomplice.");
				}
				eable.Free();
			}
			Logger.Finish();
		}

		// Clear in memory log
		public void Clear()
		{
			m_LogFile.Clear();
		}

		public static void Initialize()
		{
			EventSink.AddItem += new AddItemEventHandler(EventSink_AddItem);
			EventSink.Crashed += new CrashedEventHandler(EventSink_Crashed); ;
			EventSink.Shutdown += new ShutdownEventHandler(EventSink_Shutdown);
			EventSink.LogException += new LogExceptionEventHandler(EventSink_LogException);
		}

		// Record start time and init counter + list
		private void Start()
		{
			m_StartTime = DateTime.Now;
			m_Count = 0;
			m_Finished = false;
			m_LogFile = new ArrayList();

			if (!m_SingleLine)
				m_LogFile.Add(string.Format("Log start : {0}", m_StartTime));

			m_OpenLogs.Add(this);
		}

		// Log all the data and close the file
		public void Finish()
		{
			if (!m_Finished)
			{
				m_Finished = true;
				TimeSpan ts = DateTime.Now - m_StartTime;

				if (!m_SingleLine)
					m_LogFile.Add(string.Format("Completed in {0} seconds, {1} entr{2} logged", ts.TotalSeconds, m_Count, m_Count == 1 ? "y" : "ies"));

				// Report

				string sFilename = "logs/" + m_LogFilename;
				StreamWriter LogFile = null;

				try
				{
					LogFile = new StreamWriter(sFilename, !m_Overwrite);
				}
				catch (Exception e)
				{
					Console.WriteLine("Failed to open logfile '{0}' for writing : {1}", sFilename, e);
				}

				// Loop through the list stored and log
				for (int i = 0; i < m_LogFile.Count; i++)
				{

					if (LogFile != null)
						LogFile.WriteLine(m_LogFile[i]);

					// Send message to the player too
					if (m_Person is PlayerMobile)
					{
						m_MaxOutput--;

						if (m_MaxOutput > 0)
						{
							if (i + 1 < m_LogFile.Count && i != 0)
								m_Person.SendMessage(((string)m_LogFile[i]).Replace(" ", ""));
							else
								m_Person.SendMessage((string)m_LogFile[i]);
						}
						else if (m_MaxOutput == 0)
						{
							m_Person.SendMessage("Skipping remainder of output. See log file.");
						}
					}
				}

				// If successfully opened a stream just now, close it off!

				if (LogFile != null)
					LogFile.Close();

				if (m_OpenLogs.Contains(this))
					m_OpenLogs.Remove(this);
			}
		}

		// Add data to list to be logged : Log( (LogType ,) object (, additional) )

		// Default to mixed type
		public void Log(object data)
		{
			this.Log(LogType.Mixed, data, null);
		}

		// Default to no additional
		public void Log(LogType logtype, object data)
		{
			this.Log(logtype, data, null);
		}


		// Specify LogType
		public void Log(LogType logtype, object data, string additional)
		{
			string LogLine = "";

			if (logtype == LogType.Mixed)
			{

				// Work out most appropriate in absence of specific

				if (data is Mobile)
					logtype = LogType.Mobile;
				else if (data is Item)
					logtype = LogType.Item;
				else
					logtype = LogType.Text;

			}

			switch (logtype)
			{

				case LogType.Mobile:

					Mobile mob = (Mobile)data;
					LogLine = string.Format("{0}:Loc({1},{2},{3}):{4}:Mob({5})({6}):{7}:{8}:{9}",
								mob.GetType().Name,
								mob.Location.X, mob.Location.Y, mob.Location.Z,
								mob.Map,
								mob.Name,
								mob.Serial,
								mob.Region.Name,
								mob.Account,
								additional);

					break;

				case LogType.ItemSerial:
				case LogType.Item:

					Item item = (Item)data;
					object root = item.RootParent;

					if (root is Mobile)
						// Item loc, map, root type, root name
						LogLine = string.Format("{0}:Loc{1}:{2}:{3}({4}):Mob({5})({6}):{7}",
							item.GetType().Name,
							item.GetWorldLocation(),
							item.Map,
							item.Name,
							item.Serial,
							((Mobile)root).Name,
							((Mobile)root).Serial,
							additional
						);
					else
						// Item loc, map
						LogLine = string.Format("{0}:Loc{1}:{2}:{3}({4}):{5}",
							item.GetType().Name,
							item.GetWorldLocation(),
							item.Map,
							item.Name,
							item.Serial,
							additional
						);

					break;

				case LogType.Text:

					LogLine = data.ToString();
					break;
			}

			// If this is a "single line" loghelper instance, we need to replace
			// out newline characters
			if (m_SingleLine)
			{
				LogLine = LogLine.Replace("\n", " || ");
				LogLine = m_StartTime.ToString().Replace(":", ".") + ":" + LogLine;
			}

			m_LogFile.Add(LogLine);
			m_Count++;

		}

		private static void EventSink_Crashed(CrashedEventArgs e)
		{
			for (int ix = 0; ix < LogHelper.OpenLogs.Count; ix++)
			{
				LogHelper lh = LogHelper.OpenLogs[ix] as LogHelper;
				if (lh != null)
					lh.Finish();
			}
		}

		private static void EventSink_Shutdown(ShutdownEventArgs e)
		{
			for (int ix = 0; ix < LogHelper.OpenLogs.Count; ix++)
			{
				LogHelper lh = LogHelper.OpenLogs[ix] as LogHelper;
				if (lh != null)
					lh.Finish();
			}
		}

		private static void EventSink_AddItem(AddItemEventArgs e)
		{
			LogHelper lh = new LogHelper("AddItem.log", false, true);
			lh.Log(LogType.Mobile, e.from, String.Format("Used [Add Item to create ItemID:{0}, Serial:{1}", e.item.ItemID.ToString(), e.item.Serial.ToString()));
			lh.Finish();
		}

		private static void EventSink_LogException(LogExceptionEventArgs e)
		{	// exception passed up from the server core
			try { LogException(e.Exception); }
			catch { Console.WriteLine("Nested exception while processing: {0}", e.Exception.Message); }	// do not call LogException
		}

	}

	public enum LogType
	{
		Mobile,
		Item,
		Mixed,
		Text,
		ItemSerial
	}

}
