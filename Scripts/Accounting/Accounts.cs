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

/* Scripts/Accounting/Accounts.cs
 * CHANGELOG
 *  12/12/07, Adam
 *      we delay adding the IPAddress to the IPDatabase give us time to check for it in the WelcomeTimer
 *	12/6/07, Adam
 *		Add new IP Database functionality
 *			- hashtable to hold the addresses
 *			- add function to mask off last octet and add it to the database
 *			- lookup function to mask and check to see if that IP address is known
 *  6/11/05, Pix
 *		Added GetBool() for parsing ease.
 *	1/28/05, Pix
 *		Added try/catch around accounts.xml save
 *		Now will retry 3 times to save...
 */

using System;
using System.Collections;
using System.IO;
using System.Xml;
using Server.Scripts.Commands;
using Server.Network;

namespace Server.Accounting
{
	public class Accounts
	{
		private static Hashtable m_Accounts = new Hashtable();
		private static Hashtable m_IPDatabase = new Hashtable();

		public static void Configure()
		{
			EventSink.WorldLoad += new WorldLoadEventHandler( Load );
			EventSink.WorldSave += new WorldSaveEventHandler( Save );
		}

		public static void Initialize()
		{
			EventSink.Login += new LoginEventHandler(OnLogin);
			EventSink.Connected += new ConnectedEventHandler(OnConnected);
		}
		
		private static void OnLogin(LoginEventArgs e)
		{   // we delay adding the IPAddress to give us time to check for it in the WelcomeTimer
            if (e.Mobile != null && e.Mobile.NetState != null)
                new IPLogTimer(e.Mobile.NetState.Address).Start();
		}
		
		private static void OnConnected(ConnectedEventArgs e)
        {   // we delay adding the IPAddress to give us time to check for it in the WelcomeTimer
			if (e.Mobile != null && e.Mobile.NetState != null)
                new IPLogTimer(e.Mobile.NetState.Address).Start();
		}

		public static void IPDatabaseAdd(System.Net.IPAddress ax)
		{
			if (ax == null)
				return;
			string left=null;
			Byte[] bytes = ax.GetAddressBytes();
			if (bytes.Length != 4)
			{
				Console.WriteLine("IPAddress error: {0}", ax.ToString());
				return;
			}
			// get first 3 octets only to mask out the whole range covered by the 4th octet
			left += bytes[0].ToString() + ".";
			left += bytes[1].ToString() + ".";
			left += bytes[2].ToString();
			m_IPDatabase[left] = null;			
		}

		static Accounts()
		{
		}

		public static Hashtable Table
		{
			get
			{
				return m_Accounts;
			}
		}

		public static Account GetAccount( string username )
		{
			return m_Accounts[username] as Account;
		}

		public static Account AddAccount( string user, string pass )
		{
			Account a = new Account( user, pass );
			if ( m_Accounts.Count == 0 )
				a.AccessLevel = AccessLevel.Administrator;

			m_Accounts[a.Username] = a;

			return a;
		}

		public static int GetInt32( string intString, int defaultValue )
		{
			try
			{
				return XmlConvert.ToInt32( intString );
			}
			catch
			{
				try
				{
					return Convert.ToInt32( intString );
				}
				catch
				{
					return defaultValue;
				}
			}
		}

		public static DateTime GetDateTime( string dateTimeString, DateTime defaultValue )
		{
			try
			{
				return XmlConvert.ToDateTime( dateTimeString );
			}
			catch
			{
				try
				{
					return DateTime.Parse( dateTimeString );
				}
				catch
				{
					return defaultValue;
				}
			}
		}

		public static string GetAttribute( XmlElement node, string attributeName )
		{
			return GetAttribute( node, attributeName, null );
		}

		public static string GetAttribute( XmlElement node, string attributeName, string defaultValue )
		{
			if ( node == null )
				return defaultValue;

			XmlAttribute attr = node.Attributes[attributeName];

			if ( attr == null )
				return defaultValue;

			return attr.Value;
		}

		public static string GetText( XmlElement node, string defaultValue )
		{
			if ( node == null )
				return defaultValue;

			return node.InnerText;
		}

		public static bool GetBool( XmlElement node, bool defaultValue )
		{
			if( node == null )
				return defaultValue;

			string strVal = GetText( node, "xxx" );
			if( strVal == "xxx" ) return defaultValue;
			else if( strVal == "true" ) return true;
			else if( strVal == "false" ) return false;
			else return defaultValue;
		}

		public static void Save( WorldSaveEventArgs e )
		{
			Console.WriteLine("Account information Saving...");

			if ( !Directory.Exists( "Saves/Accounts" ) )
				Directory.CreateDirectory( "Saves/Accounts" );

			string filePath = Path.Combine( "Saves/Accounts", "accounts.xml" );

			bool bNotSaved = true;
			int attempt = 0;
			while( bNotSaved && attempt < 3 )
			{
				try
				{
					attempt++;
					using ( StreamWriter op = new StreamWriter( filePath ) )
					{
						XmlTextWriter xml = new XmlTextWriter( op );

						xml.Formatting = Formatting.Indented;
						xml.IndentChar = '\t';
						xml.Indentation = 1;

						xml.WriteStartDocument( true );

						xml.WriteStartElement( "accounts" );

						xml.WriteAttributeString( "count", m_Accounts.Count.ToString() );

						foreach ( Account a in Accounts.Table.Values )
							a.Save( xml );

						xml.WriteEndElement();

						xml.Close();

						bNotSaved = false;
					}
				}
				catch( Exception ex )
				{
					LogHelper.LogException(ex);
					System.Console.WriteLine("Caught exception in Accounts.Save: {0}", ex.Message);
					System.Console.WriteLine(ex.StackTrace);
					System.Console.WriteLine("Will attempt to recover three times.");
				}
			}
		}

		public static bool IPLookup(System.Net.IPAddress ip)
		{
			if (ip == null || m_IPDatabase == null)
				return false;

			string left = null;
			Byte[] bytes = ip.GetAddressBytes();
			if (bytes.Length != 4)
			{
				Console.WriteLine("IPAddress error: {0}", ip.ToString());
				return false;
			}
			// get first 3 octets only to mask out the whole range covered by the 4th octet
			left += bytes[0].ToString() + ".";
			left += bytes[1].ToString() + ".";
			left += bytes[2].ToString();
			return  m_IPDatabase.ContainsKey(left);
		}

		public static void Load()
		{
            int accounts=0, ips=0;
			Console.Write("Account information Loading...");

			m_Accounts = new Hashtable( 32, 1.0f, CaseInsensitiveHashCodeProvider.Default, CaseInsensitiveComparer.Default );
			m_IPDatabase = new Hashtable(32, 1.0f, CaseInsensitiveHashCodeProvider.Default, CaseInsensitiveComparer.Default);

			string filePath = Path.Combine( "Saves/Accounts", "accounts.xml" );

			if ( !File.Exists( filePath ) )
				return;

			XmlDocument doc = new XmlDocument();
			doc.Load( filePath );

			XmlElement root = doc["accounts"];

			foreach ( XmlElement account in root.GetElementsByTagName( "account" ) )
			{
				try
				{
					// build the account database
					Account acct = new Account( account );
					m_Accounts[acct.Username] = acct;
                    accounts++;

					// build the IP database
                    foreach (System.Net.IPAddress ax in acct.LoginIPs)
                    {
                        ips++;
                        IPDatabaseAdd(ax);
                    }
				}
				catch
				{
                    Console.WriteLine();
					Console.WriteLine( "Warning: Account instance load failed" );
				}
			}

            Console.WriteLine("done ({0} accounts, {1}/{2} IP addresses)", accounts, m_IPDatabase.Count, ips);
		}
	}

    public class IPLogTimer : Timer
    {
        private System.Net.IPAddress m_IPAddress;

        public IPLogTimer(System.Net.IPAddress IPAddress)
            : base(TimeSpan.FromSeconds(30.0))
        {
            m_IPAddress = IPAddress;
        }

        protected override void OnTick()
        {
            try
            {
                Accounts.IPDatabaseAdd(m_IPAddress);
                Stop();
            }
            catch (Exception ex)
            {
                LogHelper.LogException(ex);
            }
        }
    }
}