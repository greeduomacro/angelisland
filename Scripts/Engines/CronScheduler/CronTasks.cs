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

/* Scripts\Engines\CronScheduler\CronTasks.cs
 * CHANGELOG:
 *	6/26/09 - Adam
 *		correct serial value for the summer champ
 *	4/6/09, Adam
 *		Turn off ServerWarsAES - to many player complaints
 *	1/4/09, Adam
 *		Add IntMapStorage protection like we do for Items
 *	11/12/08, Adam
 *		Calculate and email Consumer Price Index log
 *	10/10/08, Pix
 *		Added call to TownshipSettings.CalculateFeesBasedOnServerActivity in CAccountStatistics.
 *	9/26/08, Adam
 *		Increase MobileLifespanCleanup from 10 seconds to 30 seconds max
 *	9/25/08, Adam
 *		Add "Performing routine maintenance, please wait." to MobileLifespanCleanup().
 *		We had improved MobileLifespanCleanup() MUCH (see comments below) but the performance stills causes a little lag
 *			we're therefore moving the call to once per day (3:29 AM) and adding a system message
 *	7/3/08, Adam
 *		Update ServerWarsAES to send a mass email announcing the 'cross shard challange'
 *	7/1/08, Adam
 *		Change over from the in-game email model to the distribution list email model (managed by the web server)	
 *		lets keep the old code around for future needs, but for now lets use our mailing list
 *		NOTE: If you change this make sure to remove the password from the subject line
 *	5/19/08. Adam
 *		Return Death Star to hourly execution since we resolved the slow performance issue.
 *	5/11/08, Adam
 *		- Remove sorting of lifespan lists
 *		- Add enhanced profiler tracking
 *	5/7/08, Adam
 *		Add JetBrains dotTrace profiler stuff.
 *		To use the API for profiling your applications, install JetBrains dotTrace:
 *		Reference JetBrains.dotTrace.Api.dll (located in the dotTrace installation directory) in your application project. 
 *		Insert the following code in a place of code you want to profile: 
 *		JetBrains.dotTrace.Api.CPUProfiler.Start();
 *			//The code which you want to profile
 *		JetBrains.dotTrace.Api.CPUProfiler.StopAndSaveSnapShot(); 
 *		Start the profiled application from within dotTrace, and in the Control Application dialog, clear Start profiling application immediately check box. 
 *	5/3/08. Adam
 *		MobileLifespanCleanup
 *		- Sort the list of mobiled to deleted base on age, oldest first.
 *			We do this to ensure we deleted the oldest first should we run out of time and have to abort the cleanup
 *		- Reschedule the remainder of the cleanup 1 hour from the time we ran out of time .. this is for TEST purposes only.
 *			I want to see how long the secon cleanup takes (I have a funny feeling it won't be slow like the first one .. Maybe Garbage Collection
 *	4/24/08, Adam
 *		Remove the periodic spawner cache reload from the Cron Scheduler as this is loaded once on server up.
 *	4/7/08, Adam
 *		- merge the Nth-Day-Of-The-Month checking into the Register() function.
 *		Add reference to syntax document
 *		http://en.wikipedia.org/wiki/Cron
 *		- Add the notion of: Cron.CronLimit.isldom.must_not_be_ldom to the Register() function.
 *			See the full explanation in CronScheduler.cs comments section.
 *	3/20/08, Adam
 *		I'm rewriting the heartbeat system (Engines\Heartbeat\heartbeat.cs)
 *		Over time we added lots of functionality to heartbeat, and it's time for a major cleanup.
 *		I'm using the system from my shareware product WinCron.
 */

using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Threading;
using System.IO;
using System.Xml;
using Server.Gumps;
using Server.Mobiles;
using Server.Items;
using Server.Misc;						// TestCenter
using Server.Network;
using Server.Accounting;				// Emailer
using Server.Scripts.Commands;			// log helper
using Server.Engines;
using Server.Engines.OverlandSpawner;	// OverlandSpawner
using Server.Engines.IOBSystem;
using Server.SMTP;								// new SMTP engine
using Server.Regions;
using Server.Guilds;
using Server.Engines.ChampionSpawn;		// champs
//using JetBrains.dotTrace.Api;			// profiler

namespace Server.Engines.CronScheduler
{
	class CronTasks
	{
		public static void Initialize()
		{	// Add your scheduled task here and implementation below
			Cron.QueueIdleTask(new CFreezeDryInit().FreezeDryInit);

			// Vendor Restock - every 30 minutes
			Cron.Register(new CVendorRestock().VendorRestock, "*/30 * * * *");

			// Item Decay - every 30 minutes
			Cron.Register(new CItemDecay().ItemDecay, "*/30 * * * *");

			// Account cleanup - daily (7:01 AM)
			Cron.Register(new CAccountCleanup().AccountCleanup, "1 7 * * *");		// minute 1 of hour

			// Check NPCWork - every 15 minutes
			Cron.Register(new CNPCWork().NPCWork, "*/15 * * * *");

			// Town Crier - every 10 minutes
			Cron.Register(new CTownCrier().TownCrier, "*/10 * * * *");

			// internal map mobile/item cleanup - every 60 minutes
			Cron.Register(new CIntMapMobileCleanup().IntMapMobileCleanup, "3 * * * *");	// minute 3 of hour
			Cron.Register(new CIntMapItemCleanup().IntMapItemCleanup, "7 * * * *");	// minute 7 of hour

			// find players using the robot skills - every 60 minutes
			Cron.Register(new CFindSkill().FindSkill, "9 * * * *");		// minute 9 of hour

			// expire old mobiles - once per day
			// We were doing this every 60 minutes but were STILL seeing 3-4 second lags. See comments above
			Cron.Register(new CMobileLifespanCleanup().MobileLifespanCleanup, "29 3 * * *");	// minute 29 of hour of 3 o'clock hour

			// Killer time cleanup - every 30 minutes
			Cron.Register(new CKillerTimeCleanup().KillerTimeCleanup, "*/30 * * * *");

			// House Decay - every 10 minutes
			Cron.Register(new CHouseDecay().HouseDecay, "*/10 * * * *");

			// Murder Count Decay - every 15 minutes
			Cron.Register(new CMurderCountDecay().MurderCountDecay, "*/15 * * * *");

			// Plant Growth - daily (6:13 AM)
			Cron.Register(new CPlantGrowth().PlantGrowth, "13 6 * * *");		// minute 13 of 6 o'clock hour

			// Overland Merchant CHANCE - every hour
			Cron.Register(new COverlandSystem().OverlandSystem, "17 * * * *");	// minute 17 of every hour

			// Reload the Spawner Cache - daily (6:19 AM)
			// no longer used - cache is loaded on server-up
			// Cron.Register(new CReloadSpawnerCache().ReloadSpawnerCache, "19 6 * * *");	// minute 19 of 6 o'clock hour

			// send an email reminder to players to donate
			Cron.Register(new CEmailDonationReminder().EmailDonationReminder, "21 6 20 * *");	// minute 21 the 20th of each month

			// email tester job
			//Cron.Register(new CTestJob().TestJob, "* * * * *");	

			// email a report of weapon distribution among players
			Cron.Register(new CWeaponDistribution().WeaponDistribution, "0 21 * * *"); // 9:00 PM 

			// Server Wars - Automated Event System (3:00 - 6:00 PM on 1st Sunday)
			// Note: set 24 hour advance notice
			//Cron.Register(new CServerWarsAES().ServerWarsAES, "0 15 * * 6");			// 3:00 PM on Saturday
			//Cron.Register(null, new CServerWarsAES().ServerWarsAES, "0 15 * * 6", true, new Cron.CronLimit(1, DayOfWeek.Saturday,Cron.CronLimit.isldom.must_not_be_ldom));

			// TEST: ** Crazy Map Day - Automated Event System (10:00 PM on 2nd Tuesday) **
			//Cron.Register(null, new CCrazyMapDayAES().CrazyMapDayAES, "45 15 * * 1", true, new Cron.CronLimit(1, DayOfWeek.Monday, Cron.CronLimit.isldom.must_not_be_ldom));

			// Town Invasion - Automated Event System (12:00 noon on 3rd Sunday)
			// Note: set 24 hour advance notice
			//Cron.Register(new CTownInvasionAES().TownInvasionAES, "0 12 * * 6");		// 12:00 noon on Saturday
			Cron.Register(null, new CTownInvasionAES().TownInvasionAES, "0 12 * * 6", true, new Cron.CronLimit(3, DayOfWeek.Saturday, Cron.CronLimit.isldom.must_not_be_ldom));

			// Kin Ransom Quest - Automated Event System (12:00 noon on 4th Sunday)
			// Note: set 24 hour advance notice
			//Cron.Register(new CKinRansomAES().KinRansomAES, "0 12 * * 6");			// 12:00 noon on Saturday
			Cron.Register(null, new CKinRansomAES().KinRansomAES, "0 12 * * 6", true, new Cron.CronLimit(4, DayOfWeek.Saturday, Cron.CronLimit.isldom.must_not_be_ldom));

			// Crazy Map Day - Automated Event System (12:00 noon on 5th Sunday)
			// Note: set 24 hour advance notice
			//Cron.Register(new CCrazyMapDayAES().CrazyMapDayAES, "0 12 * * 6");		// 12:00 noon on Saturday
			Cron.Register(null, new CCrazyMapDayAES().CrazyMapDayAES, "0 12 * * 6", true, new Cron.CronLimit(5, DayOfWeek.Saturday, Cron.CronLimit.isldom.must_not_be_ldom));		

			// every 24 hours cleanup orphaned guildstones (6:22 AM)
			Cron.Register(new CGuildstoneCleanup().GuildstoneCleanup, "22 6 * * *");	// minute 22 of 6 o'clock hour

			// every 24 hours cleanup orphaned Player NPCs (6:23 AM)
			Cron.Register(new CPlayerNPCCleanup().PlayerNPCCleanup, "23 6 * * *");		// minute 23 of 6 o'clock hour

			// every 24 hours 'standard' cleanup (6:24 AM)
			Cron.Register(new CStandardCleanup().StandardCleanup, "24 6 * * *");		// minute 24 of 6 o'clock hour

			// every 24 hours Strongbox cleanup (6:25 AM)
			Cron.Register(new CStrongboxCleanup().StrongboxCleanup, "25 6 * * *");		// minute 25 of 6 o'clock hour

			// every 10 minutes collect account statistics
			Cron.Register(new CAccountStatistics().AccountStatistics, "*/10 * * * *");

			// every hour run through guild fealty checks
			Cron.Register(new CGuildFealty().GuildFealty, "42 * * * *");			// minute 42 of each hour

			// every hour run through PlayerQuestCleanup checks
			Cron.Register(new CPlayerQuestCleanup().PlayerQuestCleanup, "43 * * * *");			// minute 43 of each hour

			// every 5 minutes run through PlayerQuest announcements
			Cron.Register(new CPlayerQuestAnnounce().PlayerQuestAnnounce, "*/5 * * * *");			// minute 43 of each hour

			// every 24 hours rotate player command logs (6:26 AM)
			Cron.Register(new CLogRotation().LogRotation, "26 6 * * 1");		// minute 26 of 6 o'clock hour on Monday

			// at 2AM backup the complete RunUO directory (~8min)
			// (at 1:50 issue warning)
			Cron.Register(new CBackupRunUO().BackupRunUO, "50 1 * * *");		// 2AM

			// at 4AM we backup only the world files (~4min)
			// (at 3:50 issue warning)
			Cron.Register(new CBackupWorldData().BackupWorldData, "50 3 * * *");	// 4AM

			// Township Charges - 9 AM daily
			Cron.Register(new CTownshipCharges().TownshipCharges, "0 9 * * *"); //9 AM every day

			// WealthTracker - 3 AM daily
			Cron.Register(new CWealthTracker().WealthTracker, "3 3 * * *"); //3rd minute of the 3AM hour

			// ConsumerPriceIndex - 3 AM daily
			Cron.Register(new CConsumerPriceIndex().ConsumerPriceIndex, "4 3 * * *"); //4th minute of the 3AM hour


			// Kin Sigils - bi-hourly 
			Cron.Register(new CKinFactions().KinFactions, "0 */2 * * *");	// every 2 hours


			// Kin Logging - 1AM daily
			Cron.Register(new CKinFactionsLogging().KinFactionsLogging, "0 1 * * *");		// 1 AM Every day

			// http://www.infoplease.com/ce6/weather/A0844225.html
			Cron.Register(new CSpringChamp().SpringChamp, "0 0 21 3 *");	// spring (vamp), about Mar. 21, (12:00 AM)
			Cron.Register(new CSummerChamp().SummerChamp, "0 0 22 6 *");	// summer (pirate), about June 22, (12:00 AM)
			Cron.Register(new CAutumnChamp().AutumnChamp, "0 0 23 9 *");	// autumn (bob), about Sept. 23, (12:00 AM)
			Cron.Register(new CWinterChamp().WinterChamp, "0 0 22 12 *");	// winter (Azothu), about Dec. 22, (12:00 AM)
		}
	}

	/* ---------------------------------------------------------------------------------------------------
	 * Above are the scheduled jobs, and below are the scheduled jobs' implementations.
	 * Please follow convention!
	 * The convention is:
	 * 1. The class name for job Foo is CFoo, and the main wraper function in CFoo is Foo(), and the worker function is FooWorker().
	 *    (This allows the built-in reporting system to accurately report what's running and when, and how long it took.)
	 * 2. Each implementation is wrapped in a so named #region
	 * 3. Each implementation displays the elapsed time and any other meaningful status. (No spamming the consol)
	 * 4. Even though the CronScheduler wraps all calls in a try-catch block, you should add a try-catch block to your implementation so that detailed errors can be logged.
	 * 5. As always, all catch blocks MUST make use of LogHelper.LogException(e) to log the exception.
	 * Note: You should never need to touch CronScheduler.cs. If you think you need to muck around in there, please let Adam know first.
	 * ---------------------------------------------------------------------------------------------------
	 */

	#region VendorRestock
	// restock the vendors
	class CVendorRestock
	{
		public void VendorRestock()
		{
			System.Console.WriteLine("Vendor restock check started ... ");
			Utility.TimeCheck tc = new Utility.TimeCheck();
			tc.Start();
			int vendorsChecked = VendorRestockWorker();
			tc.End();
			System.Console.WriteLine("checked {0} vendors in {1}", vendorsChecked, tc.TimeTaken);
		}

		private int VendorRestockWorker()
		{
			int iChecked = 0;

			ArrayList restock = new ArrayList();
			foreach (Mobile m in World.Mobiles.Values)
			{
				if (m is IVendor)
				{
					iChecked++;

					if (((IVendor)m).LastRestock + ((IVendor)m).RestockDelay < DateTime.Now)
					{
						restock.Add(m);
					}
				}
			}

			for (int i = 0; i < restock.Count; i++)
			{
				IVendor vend = (IVendor)restock[i];
				vend.Restock();
				vend.LastRestock = DateTime.Now;
			}

			return iChecked;
		}
	}
	#endregion VendorRestock

	#region ItemDecay
	// decay items on the ground
	class CItemDecay
	{
		public void ItemDecay()
		{
			System.Console.WriteLine("Item decay started ... ");
			Utility.TimeCheck tc = new Utility.TimeCheck();
			tc.Start();
			int ItemsDeleted = 0;
			int ItemsChecked = ItemDecayWorker(out ItemsDeleted);
			tc.End();
			System.Console.WriteLine("checked {0} in {1} - {2} Items Deleted", ItemsChecked, tc.TimeTaken, ItemsDeleted);
		}

		private bool ItemDecayRule(Item item)
		{
			bool rules = item.Decays			// item decays 
				&& item.Parent == null			// not on a player
				&& item.Map != Map.Internal;	// things on the internal map are dealt with elsewhere

			return rules && (item.LastMoved + item.DecayTime) <= DateTime.Now;
		}

		private int ItemDecayWorker(out int ItemsDeleted)
		{
			int iChecked = 0;
			ItemsDeleted = 0;
			try
			{
				ArrayList decaying = new ArrayList();
				// refresh spawner items (spawner items don't decay)
				foreach (Item item in Server.World.Items.Values)
				{
					if (item as Spawner != null)
						if ((item as Spawner).Running)
							(item as Spawner).Refresh();
				}

				// tag items to decay
				foreach (Item item in Server.World.Items.Values)
				{
					iChecked++;

					if (ItemDecayRule(item))
					{
						decaying.Add(item);
					}
				}

				// decay tagged items
				for (int i = 0; i < decaying.Count; ++i)
				{
					Item item = (Item)decaying[i];

					if (item.OnDecay())
					{
						if (CoreAI.DebugItemDecayOutput)
						{
							System.Console.WriteLine("Decaying {0} at {1} in {2}", item.ToString(), item.X + " " + item.Y + " " + item.Z, item.Map);
						}
						item.Delete();
						ItemsDeleted++;
					}
				}
			}
			catch (Exception e)
			{
				LogHelper.LogException(e);
				System.Console.WriteLine("Exception Caught in Item Decay Heartbeat code: " + e.Message);
				System.Console.WriteLine(e.StackTrace);
			}
			return iChecked;
		}
	}
	#endregion ItemDecay

	#region AccountCleanup
	// remove old/unused accounts
	class CAccountCleanup
	{
		public void AccountCleanup()
		{
			System.Console.Write("Account Cleanup started ... ");
			Utility.TimeCheck tc = new Utility.TimeCheck();
			tc.Start();
			int AcctsDeleted = 0;
			int AcctsChecked = AccountCleanupWorker(out AcctsDeleted);
			tc.End();
			System.Console.WriteLine("checked {0} in {1} - {2} Accounts Deleted", AcctsChecked, tc.TimeTaken, AcctsDeleted);
		}

		private int AccountCleanupRule(Account acct)
		{

			bool empty = true;	// any characters on the account?
			bool staff = false;	// is it a staff account?

			for (int i = 0; i < 5; ++i)
			{
				if (acct[i] != null)
					empty = false;	// not empty

				if (acct[i] != null && acct[i].AccessLevel > AccessLevel.Player)
					staff = true;	// a staff
			}

			// RULE1;
			// if empty AND > 7 days old, AND never logged in
			if (empty)
			{
				// never logged in
				if (acct.LastLogin == acct.Created)
				{
					// 7 days old
					TimeSpan delta = DateTime.Now - acct.LastLogin;
					if (delta.TotalDays > 7)
					{
						return 1;
					}
				}
			}

			// RULE2;
			// if empty AND inactive for 30 days (they have logged in at some point)
			if (empty)
			{
				TimeSpan delta = DateTime.Now - acct.LastLogin;
				if (delta.TotalDays > 30)
				{
					return 2;
				}
			}


			// RULE3 WARNING ONLY;
			// trim all non-staff accounts inactive for 360 days
			if (staff == false)
			{
				TimeSpan delta = DateTime.Now - acct.LastLogin;
				if (delta.TotalDays == 360)
				{	// account will be deleted in about 5 days
					EmailCleanupWarning(acct);
				}
			}


			// RULE3;
			// trim all non-staff accounts inactive for 365 days
			if (staff == false)
			{
				TimeSpan delta = DateTime.Now - acct.LastLogin;
				if (delta.TotalDays > 365)
				{
					return 3;
				}
			}

			// RULE4 - TestCenter only
			// trim all non-staff account and not logged in for 30 days
			if (TestCenter.Enabled == true && staff == false)
			{
				TimeSpan delta = DateTime.Now - acct.LastLogin;
				if (delta.TotalDays > CoreAI.TCAcctCleanupDays)
				{
					return 4;
				}
			}
			/*
									// RULE5
									// only one character, less than 5 minutes game play time, older than 30 days
									if (!empty && !staff && acct.Count == 1)
									{
											int place = 0; //hold place of character on account

											for (int i = 0; i < acct.Length; i++ )
											{
													if (acct[i] != null)
													{
															place = i;
															break;
													}
											}

											TimeSpan delta = DateTime.Now - acct.LastLogin;
											TimeSpan gamePlayDelta = acct.LastLogin - acct[place].CreationTime;

											if (delta.TotalDays > 30 && gamePlayDelta.TotalMinutes < 5)
											{
													return 5;
											}
									}
			*/
			// this account should not be deleted
			return 0;
		}

		private void EmailCleanupWarning(Account a)
		{
			try
			{	// only on the production shard
				if (TestCenter.Enabled == false)
					if (a.EmailAddress != null && SmtpDirect.CheckEmailAddy(a.EmailAddress, false) == true)
					{
						string subject = "Angel Island: Your account will be deleted in about 5 days";
						string body = String.Format("\nThis message is to inform you that your account '{0}' will be deleted on {1} if it remains unused.\n\nIf you decide not to return to Angel Island, we would like wish you well and thank you for playing our shard.\n\nBest Regards,\n  The Angel Island Team\n\n", a.ToString(), DateTime.Now + TimeSpan.FromDays(5));
						Emailer mail = new Emailer();
						mail.SendEmail(a.EmailAddress, subject, body, false);
					}
			}
			catch (Exception ex)
			{
				LogHelper.LogException(ex);
			}
		}

		private int AccountCleanupWorker (out int AcctsDeleted)
		{
			int iChecked = 0;
			AcctsDeleted = 0;

			if (CoreAI.TCAcctCleanupEnable == false)
				return 0;

			try
			{
				ArrayList results = new ArrayList();

				foreach (Account acct in Accounts.Table.Values)
				{
					iChecked++;
					if (AccountCleanupRule(acct) != 0)
					{
						results.Add(acct);
					}
				}

				if (results.Count > 0)
				{
					LogHelper Logger = new LogHelper("accountDeletion.log", false);
					for (int i = 0; i < results.Count; i++)
					{
						AcctsDeleted++;
						Account acct = (Account)results[i];

						// log it
						string temp = string.Format("Rule:{3}, Username:{0}, Created:{1}, Last Login:{4}, Email:{2}",
							acct.Username,
							acct.Created,
							acct.EmailAddress,
							AccountCleanupRule(acct),
							acct.LastLogin);
						Logger.Log(LogType.Text, temp);

						// delete it!
						acct.Delete();
					}
					Logger.Finish();
				}
			}
			catch (Exception e)
			{
				LogHelper.LogException(e);
				System.Console.WriteLine("Exception Caught in Account Cleanup code: " + e.Message);
				System.Console.WriteLine(e.StackTrace);
			}
			return iChecked;
		}
	}
	#endregion AccountCleanup

	#region NPCWork
	// see if any NPCs need to work (recall to spawner)
	class CNPCWork
	{
		public void NPCWork()
		{
			System.Console.Write("Npc work started ... ");
			Utility.TimeCheck tc = new Utility.TimeCheck();
			tc.Start();
			int npcsworked = 0;
			int mobileschecked = NPCWorkWorker(out npcsworked);
			tc.End();
			System.Console.WriteLine("checked {0} in {1} - {2} Npcs Worked", mobileschecked, tc.TimeTaken, npcsworked);
		}

		private int NPCWorkWorker(out int mobsworked)
		{
			int iChecked = 0;
			mobsworked = 0;
			try
			{
				ArrayList ToDoWork = new ArrayList();
				foreach (Mobile m in World.Mobiles.Values)
				{
					if (m is BaseCreature)
					{
						iChecked++;
						BaseCreature bc = (BaseCreature)m;

						if (!bc.Controlled && !bc.IOBFollower && !bc.Blessed && !bc.IsStabled && !bc.IsHumanInTown())
						{
							if (bc.CheckWork())
							{
								ToDoWork.Add(bc);
								//System.Console.WriteLine("adding npc for work");
							}
						}
					}

				}

				if (ToDoWork.Count > 0)
				{
					for (int i = 0; i < ToDoWork.Count; i++)
					{
						mobsworked++;
						System.Console.Write("{0}, ", ((BaseCreature)ToDoWork[i]).Location);

						((BaseCreature)ToDoWork[i]).OnThink();
						((BaseCreature)ToDoWork[i]).AIObject.Think();
					}
				}
			}
			catch (Exception e)
			{
				LogHelper.LogException(e);
				System.Console.WriteLine("Exception Caught in Npc Work code: " + e.Message);
				System.Console.WriteLine(e.StackTrace);
			}

			return iChecked;
		}
	}
	#endregion NPCWork

	#region TownCrier
	// process global town crier mewssages
	class CTownCrier
	{
		public void TownCrier()
		{
			System.Console.Write("Processing global Town Crier messages ... ");
			Utility.TimeCheck tc = new Utility.TimeCheck();
			tc.Start();
			int CrierMessages = 0;
			int CriersChecked = TownCrierWorker(out CrierMessages);
			tc.End();
			System.Console.WriteLine("checked {0} in {1} - {2} messages", CriersChecked, tc.TimeTaken, CrierMessages);
		}

		private int TownCrierWorker(out int CrierMessages)
		{
			int CriersChecked = 0;
			CrierMessages = 0;

			try
			{
				CrierMessages = TCCS.TheList.Count;
				if (CrierMessages > 0)
				{
					//jumpstart the town criers if they're not already going.
					ArrayList instances = Server.Mobiles.TownCrier.Instances;
					CriersChecked = instances.Count;
					for (int i = 0; i < instances.Count; ++i)
						((Server.Mobiles.TownCrier)instances[i]).ForceBeginAutoShout();
				}
			}
			catch (Exception e)
			{
				LogHelper.LogException(e);
				System.Console.WriteLine("Exception Caught in Town Crier code: " + e.Message);
				System.Console.WriteLine(e.StackTrace);
			}

			return CriersChecked;
		}
	}
	#endregion TownCrier

	#region IntMapMobileCleanup
	// internal map mobile cleanup
	class CIntMapMobileCleanup
	{
		public void IntMapMobileCleanup()
		{
			System.Console.Write("Internal MOB cleanup started ... ");
			Utility.TimeCheck tc = new Utility.TimeCheck();
			tc.Start();
			int mobilesexpired = 0;
			int mobileschecked = IntMapMobileCleanupWorker(out mobilesexpired);
			tc.End();
			System.Console.WriteLine("checked {0} in {1} - {2} expired", mobileschecked, tc.TimeTaken, mobilesexpired);
		}

		private int IntMapMobileCleanupWorker(out int mobsdeleted)
		{
			int iChecked = 0;
			mobsdeleted = 0;
			try
			{
				// first time if we see something we think we should delete, we simply mark it 
				//	as something to delete. If we see it again and we still think we should
				//	delete it, we delete it, otherwise, clear the mark.
				ArrayList list = new ArrayList();
				foreach (Mobile mob in World.Mobiles.Values)
				{
					iChecked++;
					if (
						mob is Mobile 
						&& mob.Map == Map.Internal
						&& mob.Deleted == false
						&& !mob.SpawnerTempMob		// spawner template Mobile no deleteing!
						&& !mob.IsIntMapStorage		// int storage item no deleteing!
						)	
					{
						if (mob.Account == null)
						{
							// if rider ignore
							if (mob is BaseMount)
							{
								BaseMount bm = mob as BaseMount;
								if (bm.Rider != null)
								{	// don't even think about it
									mob.ToDelete = false;
									continue;
								}
							}

							// if stabled ignore
							if (mob is BaseCreature)
							{
								BaseCreature bc = mob as BaseCreature;
								if (bc.IsStabled == true)
								{	// don't even think about it
									mob.ToDelete = false;
									continue;
								}
							}

							// if already marked, add to delete list
							if (mob.ToDelete == true)
							{
								list.Add(mob);
							}
							else // mark to delete
							{
								mob.ToDelete = true;
							}
						}
						else
							mob.ToDelete = false;
					}
				}

				// cleanup
				foreach (Mobile m in list)
				{
					mobsdeleted++;
					m.Delete();
				}
			}
			catch (Exception e)
			{
				LogHelper.LogException(e);
				System.Console.WriteLine("Exception Caught in IntMapMobileCleanup removal code: " + e.Message);
				System.Console.WriteLine(e.StackTrace);
			}

			return iChecked;
		}
	}
	#endregion IntMapMobileCleanup

	#region GuildstoneCleanup
	// delete orphaned guild stones
	class CGuildstoneCleanup
	{
		public void GuildstoneCleanup()
		{
			System.Console.Write("Guildstone cleanup started ... ");
			Utility.TimeCheck tc = new Utility.TimeCheck();
			tc.Start();
			int ItemsExpired = 0;
			int ItemsChecked = GuildstoneCleanupWorker(out ItemsExpired);
			tc.End();
			System.Console.WriteLine("checked {0} in {1} - {2} expired", ItemsChecked, tc.TimeTaken, ItemsExpired);
		}

		private int GuildstoneCleanupWorker (out int NumberDeleted)
		{
			int iChecked = 0;
			NumberDeleted = 0;
			try
			{
				LogHelper Logger = new LogHelper("GuildstoneCleanup.log", false);

				// first time we see something we think we should delete, we simply mark it 
				//	as something to delete. If we see it again and we still think we should
				//	delete it, we delete it.
				ArrayList list = new ArrayList();
				foreach (Item i in World.Items.Values)
				{
					// only look at guildstones
					if (!(i is Guildstone)) continue;

					iChecked++;
					if (i.Map != Map.Internal)		// not internal (internal cleanup done elsewhere)
					{
						if (i.Parent == null		// not being carried
							&& !i.SpawnerTempItem	// spawner template item no deleteing!
							&& !i.IsIntMapStorage	// int storage item no deleteing!
							&& !i.Deleted			// duh
							&& !InHouse(i))			// not in a house
						{

							// if already marked, add to delete list
							if (i.ToDelete == true)
							{
								list.Add(i);
								Logger.Log(LogType.Item, i, "(Deleted)");
							}

								// mark to delete
							else
							{
								i.ToDelete = true;
								Logger.Log(LogType.Item, i, "(Marked For Deletion)");
							}
						}
						else
						{
							if (i.ToDelete == true)
								Logger.Log(LogType.Item, i, "(Unmarked For Deletion)");
							i.ToDelete = false;
						}
					}
				}

				// Cleanup
				foreach (Item item in list)
				{
					NumberDeleted++;
					item.Delete();
				}

				Logger.Finish();
			}
			catch (Exception e)
			{
				LogHelper.LogException(e);
				System.Console.WriteLine("Exception Caught in GuildstoneCleanup removal code: " + e.Message);
				System.Console.WriteLine(e.StackTrace);
			}

			return iChecked;
		}

		private bool InHouse(object k)
		{
			Mobile m = k as Mobile;
			Item i = k as Item;

			if (i != null)
				return (Region.Find(i.Location, i.Map) is HouseRegion);
			if (m != null)
				return (Region.Find(m.Location, m.Map) is HouseRegion);
			else
				return false;
		}
	}
	#endregion GuildstoneCleanup

	#region PlayerNPCCleanup
	// delete player owned vendors and barkeeps that are orphaned
	class CPlayerNPCCleanup
	{
		public void PlayerNPCCleanup()
		{
			System.Console.Write("Player NPC cleanup started ... ");
			Utility.TimeCheck tc = new Utility.TimeCheck();
			tc.Start();
			int ItemsExpired = 0;
			int ItemsChecked = PlayerNPCCleanupWorker(out ItemsExpired);
			tc.End();
			System.Console.WriteLine("checked {0} in {1} - {2} expired", ItemsChecked, tc.TimeTaken, ItemsExpired);
		}

		private int PlayerNPCCleanupWorker(out int NumberDeleted)
		{
			int iChecked = 0;
			NumberDeleted = 0;
			try
			{
				LogHelper Logger = new LogHelper("PlayerNPCCleanup.log", false);

				// first time we see something we think we should delete, we simply mark it 
				//	as something to delete. If we see it again and we still think we should
				//	delete it, we delete it.
				ArrayList list = new ArrayList();
				foreach (Mobile i in World.Mobiles.Values)
				{
					// only look at Player owned NPCs
					bool PlayerNPC =
						i is PlayerBarkeeper ||
						i is PlayerVendor ||
						i is RentedVendor ||
						i is HouseSitter;

					if (!PlayerNPC) continue;

					iChecked++;
					if (i.Map != Map.Internal)		// not internal (internal cleanup done elsewhere)
					{
						if (!i.SpawnerTempMob		// spawner template Mobile no deleteing!
							&& !InHouse(i)			// not in a house (not with a mouse)
							&& !i.Deleted			// duh
							&& !GmPlaced(i))		// not GM placed
						{

							// if already marked, add to delete list
							if (i.ToDelete == true)
							{
								list.Add(i);
								Logger.Log(LogType.Mobile, i, "(Deleted)");
							}

								// mark to delete
							else
							{
								i.ToDelete = true;
								Logger.Log(LogType.Mobile, i, "(Marked For Deletion)");
							}
						}
						else
						{
							if (i.ToDelete == true)
								Logger.Log(LogType.Mobile, i, "(Unmarked For Deletion)");
							i.ToDelete = false;
						}
					}
				}

				// Cleanup
				foreach (Mobile m in list)
				{
					NumberDeleted++;
					m.Delete();
				}

				Logger.Finish();
			}
			catch (Exception e)
			{
				LogHelper.LogException(e);
				System.Console.WriteLine("Exception Caught in PlayerNPCCleanup removal code: " + e.Message);
				System.Console.WriteLine(e.StackTrace);
			}

			return iChecked;
		}

		private bool GmPlaced(Mobile m)
		{
			// player vendors, regular and rented: probably never GM Placed
			if (m is PlayerVendor)
			{
				PlayerVendor cx = m as PlayerVendor;
				if (cx.Owner != null && cx.Owner.AccessLevel > AccessLevel.Player)
					return true;
			}

			// PlayerBarkeeper: Used for work deco and player statues
			if (m is PlayerBarkeeper)
			{
				PlayerBarkeeper cx = m as PlayerBarkeeper;
				if (cx.Owner != null && cx.Owner.AccessLevel > AccessLevel.Player)
					return true;
			}

			// HouseSitter: Don't know why we would ever have one of these GM placed
			if (m is HouseSitter)
			{
				HouseSitter cx = m as HouseSitter;
				if (cx.Owner != null && cx.Owner.AccessLevel > AccessLevel.Player)
					return true;
			}

			// Barkeeps on a spawner are protected. like the Warden in prison
			if (m is BaseCreature)
			{
				BaseCreature cx = m as BaseCreature;
				if (cx.Spawner != null)
					return true;
			}

			return false;
		}

		private bool InHouse(object k)
		{
			Mobile m = k as Mobile;
			Item i = k as Item;

			if (i != null)
				return (Region.Find(i.Location, i.Map) is HouseRegion);
			if (m != null)
				return (Region.Find(m.Location, m.Map) is HouseRegion);
			else
				return false;
		}
	}
	#endregion PlayerNPCCleanup

	#region StandardCleanup
	// standard RunUO server startup cleanup code - bankbox, hair, etc.
	class CStandardCleanup
	{
		public void StandardCleanup()
		{
			System.Console.Write("Standard cleanup started ... ");
			Utility.TimeCheck tc = new Utility.TimeCheck();
			tc.Start();
			int ItemsExpired = 0;
			int ItemsChecked = StandardCleanupWorker(out ItemsExpired);
			tc.End();
			System.Console.WriteLine("checked {0} in {1} - {2} expired", ItemsChecked, tc.TimeTaken, ItemsExpired);
		}

		private int StandardCleanupWorker(out int NumberDeleted)
		{
			int iChecked = 0;
			NumberDeleted = 0;

			try
			{
				ArrayList items = new ArrayList();
				//ArrayList commodities = new ArrayList();

				int boxes = 0;

				foreach (Item item in World.Items.Values)
				{
					iChecked++;

					if (item.Deleted == true)
					{   // ignore
						continue;
					}
					else if (item is BankBox)
					{
						BankBox box = (BankBox)item;
						Mobile owner = box.Owner;

						if (owner == null)
						{
							items.Add(box);
							++boxes;
						}
						else if (!owner.Player && box.Items.Count == 0)
						{
							items.Add(box);
							++boxes;
						}

						continue;
					}
					else if ((item.Layer == Layer.Hair || item.Layer == Layer.FacialHair))
					{
						object rootParent = item.RootParent;

						if (rootParent is Mobile && item.Parent != rootParent && ((Mobile)rootParent).AccessLevel == AccessLevel.Player)
						{
							items.Add(item);
							continue;
						}
					}

					if (item.Parent != null || item.Map != Map.Internal || item.HeldBy != null)
						continue;

					if (item.Location != Point3D.Zero)
						continue;

					if (!IsBuggable(item))
						continue;

					items.Add(item);
				}

				//for ( int i = 0; i < commodities.Count; ++i )
				//	items.Remove( commodities[i] );

				if (items.Count > 0)
				{
					NumberDeleted = items.Count;

					if (boxes > 0)
						Console.WriteLine("Cleanup: Detected {0} inaccessible items, including {1} bank boxes, removing..", items.Count, boxes);
					else
						Console.WriteLine("Cleanup: Detected {0} inaccessible items, removing..", items.Count);

					for (int i = 0; i < items.Count; ++i)
						((Item)items[i]).Delete();
				}
			}
			catch (Exception e)
			{
				LogHelper.LogException(e);
				System.Console.WriteLine("Exception Caught in Standard Cleanup removal code: " + e.Message);
				System.Console.WriteLine(e.StackTrace);
			}

			return iChecked;
		}

		private bool IsBuggable(Item item)
		{
			if (item is Fists)
				return false;

			if (item is ICommodity || item is Multis.BaseBoat
				|| item is Fish || item is BigFish
				|| item is BasePotion || item is Food || item is CookableFood
				|| item is SpecialFishingNet || item is BaseMagicFish
				|| item is Shoes || item is Sandals
				|| item is Boots || item is ThighBoots
				|| item is TreasureMap || item is MessageInABottle
				|| item is BaseArmor || item is BaseWeapon
				|| item is BaseClothing
				|| (item is BaseJewel && Core.AOS))
				return true;

			return false;
		}
	}
	#endregion StandardCleanup

	#region StrongboxCleanup
	// delete orphaned strong boxes
	class CStrongboxCleanup
	{
		public void StrongboxCleanup()
		{
			System.Console.Write("Strongbox cleanup started ... ");
			Utility.TimeCheck tc = new Utility.TimeCheck();
			tc.Start();
			int ItemsExpired = 0;
			int ItemsChecked = StrongboxCleanupWorker(out ItemsExpired);
			tc.End();
			System.Console.WriteLine("checked {0} in {1} - {2} expired", ItemsChecked, tc.TimeTaken, ItemsExpired);
		}

		private int StrongboxCleanupWorker(out int NumberDeleted)
		{
			int iChecked = 0;
			NumberDeleted = 0;
			try
			{
				LogHelper Logger = new LogHelper("StrongboxCleanup.log", false);

				// first time we see something we think we should delete, we simply mark it 
				//	as something to delete. If we see it again and we still think we should
				//	delete it, we delete it.
				ArrayList list = new ArrayList();
				foreach (Item i in World.Items.Values)
				{
					// only look at guildstones
					if (!(i is StrongBox)) continue;
					StrongBox sb = i as StrongBox;

					iChecked++;
					if (i.Map != Map.Internal)			// not internal (internal cleanup done elsewhere)
					{
						if (sb.Owner != null					// it is owned
							&& sb.House != null					// in a house
														&& !sb.Deleted                          // duh
							&& !sb.House.IsCoOwner(sb.Owner))	// yet owner is not a co owner of the house
						{

							// if already marked, add to delete list
							if (i.ToDelete == true)
							{
								list.Add(i);
								Logger.Log(LogType.Item, i, "(Deleted)");
							}

							// mark to delete
							else
							{
								i.ToDelete = true;
								Logger.Log(LogType.Item, i, "(Marked For Deletion)");
							}
						}
						else
						{
							if (i.ToDelete == true)
								Logger.Log(LogType.Item, i, "(Unmarked For Deletion)");
							i.ToDelete = false;
						}
					}
				}

				// Cleanup
				foreach (Item item in list)
				{
					NumberDeleted++;
					item.Delete();
				}

				Logger.Finish();
			}
			catch (Exception e)
			{
				LogHelper.LogException(e);
				System.Console.WriteLine("Exception Caught in StrongboxCleanup removal code: " + e.Message);
				System.Console.WriteLine(e.StackTrace);
			}

			return iChecked;
		}
	}
	#endregion StrongboxCleanup

	#region IntMapItemCleanup
	// internal map item cleanup
	class CIntMapItemCleanup
	{
		public void IntMapItemCleanup()
		{
			System.Console.Write("Internal Item cleanup started ... ");
			Utility.TimeCheck tc = new Utility.TimeCheck();
			tc.Start();
			int ItemsExpired = 0;
			int ItemsChecked = IntMapItemCleanupWorker(out ItemsExpired);
			tc.End();
			System.Console.WriteLine("checked {0} in {1} - {2} expired", ItemsChecked, tc.TimeTaken, ItemsExpired);
		}

		private int IntMapItemCleanupWorker(out int ItemsDeleted)
		{
			int iChecked = 0;
			ItemsDeleted = 0;
			try
			{
				// first time we see something we think we should delete, we simply mark it 
				//	as something to delete. If we see it again and we still think we should
				//	delete it, we delete it.
				ArrayList list = new ArrayList();
				foreach (Item i in World.Items.Values)
				{
					iChecked++;
					if (i.Map == Map.Internal)
					{
						if (i is Guildstone && !i.IsIntMapStorage)
						{
							try
							{
								string strLog = string.Format(
									"Guildstone on internal map not marked as IsIntMapStorage - please investigate: serial: {0}",
									i.Serial.ToString());

								LogHelper.LogException(new Exception("Pixie Needs To Investigate"), strLog);
							}
							catch (Exception e)
							{
								LogHelper.LogException(e, "Pixie needs to investigate this.");
							}

							i.ToDelete = false;
						}
						else
							if (i.Parent == null			// no parent
							&& !i.SpawnerTempItem			// spawner template item no deleteing!
							&& !i.IsIntMapStorage			// int storage item no deleteing!
							&& !(i is AutomatedEventSystem)	// these are items! don't delete them!
							&& !(i is Fists)				// why?
							&& !(i is MountItem)			// why?
							&& !(i is EffectItem)			// why?
							&& !(i is ICommodity)			// delete this? we no longer do commodity deeds like this
							&& !i.Deleted                   // duh. 
							&& i.PlayerQuest == false		// it's a special PlayerQuest item?
							&& !IsVendorStock(i))			// is this vendor stock?
							// && !(i is BaseQuest) && !(i is QuestObjective)
							{
								// if already marked, add to delete list
								if (i.ToDelete == true)
								{
									list.Add(i);
								}

									// mark to delete
								else
								{
									i.ToDelete = true;
								}
							}
							else
							{
								i.ToDelete = false;
							}
					}
				}

				// Cleanup
				foreach (Item item in list)
				{
					ItemsDeleted++;
					item.Delete();
				}
			}
			catch (Exception e)
			{
				LogHelper.LogException(e);
				System.Console.WriteLine("Exception Caught in IntMapItemCleanup removal code: " + e.Message);
				System.Console.WriteLine(e.StackTrace);
			}

			return iChecked;
		}

		private bool IsVendorStock(Item i)
		{
			return i.GetType().DeclaringType == typeof(GenericBuyInfo);
		}
	}
	#endregion IntMapItemCleanup

	#region FindSkill
	// find and log players using certain skills
	class CFindSkill
	{
		public void FindSkill()
		{
			System.Console.Write("FindSkill routine started ... ");
			Utility.TimeCheck tc = new Utility.TimeCheck();
			tc.Start();
			int ResourceGatherers = FindSkillWorker();
			tc.End();
			System.Console.WriteLine("checked in {0} - {1} player(s) matched", tc.TimeTaken, ResourceGatherers);
		}

		private int FindSkillWorker()
		{
			int ResourceGatherers = 0;

			try
			{
				ArrayList LJMobMatches = Server.Scripts.Commands.FindSkill.FindSkillMobs(SkillName.Lumberjacking, 2);
				ArrayList MineMobMatches = Server.Scripts.Commands.FindSkill.FindSkillMobs(SkillName.Mining, 2);
				ResourceGatherers = LJMobMatches.Count + MineMobMatches.Count;
			}
			catch (Exception e)
			{
				LogHelper.LogException(e);
				System.Console.WriteLine("Exception Caught in FindSkill code: " + e.Message);
				System.Console.WriteLine(e.StackTrace);
			}

			return ResourceGatherers;
		}
	}
	#endregion FindSkill

	#region MobileLifespanCleanup
	// cleanup mobiled that have outlived their useful lifespan (rotating population)
	class CMobileLifespanCleanup
	{
		public void MobileLifespanCleanup()
		{
			Server.World.Broadcast(0x35, true, "Performing routine maintenance, please wait.");
			System.Console.Write("MOB lifespan cleanup started ... ");
			Utility.TimeCheck tc = new Utility.TimeCheck();
			tc.Start();
			int mobilesexpired = 0;
			//CPUProfiler.Start();							// ** PROFILER START ** 
			int mobileschecked = MobileLifespanCleanup(out mobilesexpired);
			tc.End();
			System.Console.WriteLine("checked {0} in {1} - {2} expired", mobileschecked, tc.TimeTaken, mobilesexpired);
			//CPUProfiler.StopAndSaveSnapShot();				// ** PROFILER STOP ** 
			Server.World.Broadcast(0x35, true, "Routine maintenance complete. The entire process took {0}.", tc.TimeTaken);
		}

		private int MobileLifespanCleanup(out int mobsdeleted)
		{
			int result = 0;
			result = MobileLifespanCleanupWorker(out mobsdeleted, 30.00); // keep deleting for 30 seconds max
			return result;

		}

		private int MobileLifespanCleanupWorker(out int mobsdeleted, double timeout)
		{
			int iChecked = 0;
			mobsdeleted = 0;
			try
			{
				ArrayList ToDelete = new ArrayList();
				foreach (Mobile m in World.Mobiles.Values)
				{
					// mobiles on the internal map are handled elsewhere (includes stabled)
					// Note: PlayerVendors are not BaseCreature, and are handled elsewhere
					if (m is BaseCreature && m.Map != Map.Internal)
					{
						iChecked++;
						BaseCreature bc = (BaseCreature)m;

						// process exclusions here
						if (!bc.Controlled				// summons, pets, IOB followers, hire fighters
							&& !bc.SpawnerTempMob		// spawner template mobs, no deleteing
							&& !(bc is BaseVendor)		// house sitters, barkeeps, auctioneers (Idea: exclude if Admin owned or InHouseRegion only. Others could be deleted.)
							&& !bc.IOBFollower			// probably handled by !bc.Controled
							&& !bc.Blessed				// Zoo animals etc.
							&& !bc.Deleted)				// duh.
						{
							// process inclusions here
							if (bc.IsPassedLifespan() && bc.Hits == bc.HitsMax)
							{
								bool bSkip = false;

								// if the creature has not thought in the last little while, then he is said to be Hibernating
								if (bc.Hibernating == false)
									bSkip = true;

								if (!bSkip)
									ToDelete.Add(bc);
							}
						}
					}
				}

				if (ToDelete.Count > 0)
				{
					// ToDelete.Sort(new AgeCheck());
					Utility.TimeCheck tc = new Utility.TimeCheck();
					tc.Start();
					for (int i = 0; i < ToDelete.Count; i++)
					{
						mobsdeleted++;
						((BaseCreature)ToDelete[i]).Delete();

						// Adam: do not spend too much time here
						//	this is no longer an issue as we fixed the very slow region.InternalExit and sector.OnLeave
						if (tc.Elapsed() > timeout)
							break;
					}
					tc.End();
					if (ToDelete.Count - mobsdeleted > 0)
						System.Console.WriteLine("Ran out of time, skipping {0} mobiles", ToDelete.Count - mobsdeleted);
				}
			}
			catch (Exception e)
			{
				LogHelper.LogException(e);
				System.Console.WriteLine("Exception Caught in MOBCleanup removal code: " + e.Message);
				System.Console.WriteLine(e.StackTrace);
			}

			return iChecked;
		}

		/*
		public void MobileLifespanCleanupExtern(double timeout)
		{
				System.Console.Write("MOB lifespan cleanup started ... ");
				Utility.TimeCheck tc = new Utility.TimeCheck();
				tc.Start();
				int mobilesexpired = 0;
				int mobileschecked = MobileLifespanCleanupWorker(out mobilesexpired, timeout);
				tc.End();
				System.Console.WriteLine("checked {0} in {1} - {2} expired", mobileschecked, tc.TimeTaken, mobilesexpired);
		}*/
	}
	#endregion MobileLifespanCleanup

	#region KillerTimeCleanup
	// report murder timeout logic
	class CKillerTimeCleanup
	{
		public void KillerTimeCleanup()
		{
			System.Console.Write("KillerTime cleanup started ... ");
			Utility.TimeCheck tc = new Utility.TimeCheck();
			tc.Start();
			int mobileschecked = KillerTimeCleanupWorker();
			tc.End();
			System.Console.WriteLine("checked " + mobileschecked + " in " + tc.TimeTaken);
		}

		private int KillerTimeCleanupWorker()
		{
			int mobileschecked = 0;

			try
			{
				mobileschecked = Server.Mobiles.PlayerMobile.DoGlobalCleanKillerTimes();
			}
			catch (Exception e)
			{
				LogHelper.LogException(e);
				System.Console.WriteLine("Exception Caught in KillerTimeCleanup code: " + e.Message);
				System.Console.WriteLine(e.StackTrace);
			}

			return mobileschecked;
		}
	}
	#endregion KillerTimeCleanup

	#region HouseDecay
	// decay houses
	class CHouseDecay
	{
		public void HouseDecay()
		{
			System.Console.Write("House Decay started ... ");
			System.Console.Write("\n");
			Utility.TimeCheck tc = new Utility.TimeCheck();
			tc.Start();
			int houseschecked = HouseDecayWorker();
			tc.End();
			System.Console.WriteLine("checked " + houseschecked + " houses in: " + tc.TimeTaken);
		}

		private int HouseDecayWorker()
		{
			int houseschecked = 0;

			try
			{
				foreach (Mobile mob in World.Mobiles.Values)
				{
					Mobiles.HouseSitter hs = mob as Mobiles.HouseSitter;

					if (hs != null)
					{
						hs.RefreshHouseIfNeeded();
					}
				}

				houseschecked = Server.Multis.BaseHouse.CheckAllHouseDecay();
			}
			catch (Exception e)
			{
				LogHelper.LogException(e);
				System.Console.WriteLine("Exception Caught in House Decay code: " + e.Message);
				System.Console.WriteLine(e.StackTrace);
			}

			return houseschecked;
		}
	}
	#endregion HouseDecay

	#region MurderCountDecay
	// decay murder counts
	class CMurderCountDecay
	{
		public void MurderCountDecay()
		{
			System.Console.Write("Murder Count Decay started ... ");
			Utility.TimeCheck tc = new Utility.TimeCheck();
			tc.Start();
			int mobileschecked = MurderCountDecayWorker();
			tc.End();
			System.Console.WriteLine("checked " + mobileschecked + " characters in: " + tc.TimeTaken);
		}

		private int MurderCountDecayWorker()
		{
			int mobileschecked = 0;
			try
			{
				mobileschecked = Server.Mobiles.PlayerMobile.DoGlobalDecayKills();
			}
			catch (Exception e)
			{
				LogHelper.LogException(e);
				System.Console.WriteLine("Exception Caught in MurderCountDecay code: " + e.Message);
				System.Console.WriteLine(e.StackTrace);
			}

			return mobileschecked;
		}
	}
	#endregion MurderCountDecay

	#region PlantGrowth
	// grow the plants
	class CPlantGrowth
	{
		public void PlantGrowth()
		{
			System.Console.Write("Plant Growth started ... ");
			Utility.TimeCheck tc = new Utility.TimeCheck();
			tc.Start();
			int plantschecked = PlantGrowthWorker();
			tc.End();
			System.Console.WriteLine("checked " + plantschecked + " plants in: " + tc.TimeTaken);
		}

		private int PlantGrowthWorker()
		{
			int plantschecked = 0;
			try
			{
				plantschecked = Server.Engines.Plants.PlantSystem.DoGlobalPlantGrowthCheck();
			}
			catch (Exception e)
			{
				LogHelper.LogException(e);
				System.Console.WriteLine("Exception Caught in PlantGrowth code: " + e.Message);
				System.Console.WriteLine(e.StackTrace);
			}

			return plantschecked;
		}
	}
	#endregion PlantGrowth

	#region OverlandSystem
	// produce an overland mobile
	class COverlandSystem
	{
		public void OverlandSystem()
		{
			System.Console.Write("Overland Merchant spawn ... ");
			if (Utility.RandomChance(10))
			{
				Utility.TimeCheck tc = new Utility.TimeCheck();
				tc.Start();
				Point3D location;
				bool result = OverlandSystem(out location);
				tc.End();
				if (result == false)
					System.Console.WriteLine("Failed to spawn an Overland Mobile");
				else
					System.Console.WriteLine("Spawned an Overland Mobile at {0} in:{1}", location, tc.TimeTaken);
			}
			else
				System.Console.WriteLine("None at this time");
		}

		private bool OverlandSystem(out Point3D location)
		{
			bool result = true;
			location = new Point3D(0, 0, 0);
			try
			{
				OverlandSpawner.OverlandSpawner om = new OverlandSpawner.OverlandSpawner();
				Mobile m = null;
				if (om != null)
				{
					switch (Utility.Random(3))
					{
						case 0: m = om.OverlandSpawnerSpawn(new OverlandBandit()); break;
						case 1: m = om.OverlandSpawnerSpawn(new OverlandMerchant()); break;
						case 2: m = om.OverlandSpawnerSpawn(new OverlandTreasureHunter()); break;
					}
				}
				if (m == null)
					result = false;
				else
					location = m.Location;
			}
			catch (Exception e)
			{
				LogHelper.LogException(e);
				System.Console.WriteLine("Exception Caught in PlantGrowth code: " + e.Message);
				System.Console.WriteLine(e.StackTrace);
			}

			return result;
		}
	}
	#endregion OverlandSystem

	// no longer used -- cache is loaded on server-up
	#region ReloadSpawnerCache
	// reload the spawner cache - being obsoleted 
	/*class CReloadSpawnerCache
	{
		public void ReloadSpawnerCache()
		{
			System.Console.Write("Reloading Spawner Cache ... ");
			Utility.TimeCheck tc = new Utility.TimeCheck();
			tc.Start();
			int count = ReloadSpawnerCacheWorker();
			tc.End();
			System.Console.WriteLine("Spawner cache reloaded with {0} spawners in:{1}", count, tc.TimeTaken);
		}

		private int ReloadSpawnerCacheWorker()
		{
			int UnitsChecked = 0;
			try
			{
				OverlandSpawner.OverlandSpawner om = new OverlandSpawner.OverlandSpawner();
				if (om != null)
					UnitsChecked = om.LoadSpawnerCache();
			}
			catch (Exception e)
			{
				LogHelper.LogException(e);
				System.Console.WriteLine("Exception Caught in ReloadSpawnerCache code: " + e.Message);
				System.Console.WriteLine(e.StackTrace);
			}

			return UnitsChecked;
		}
	}*/
	#endregion ReloadSpawnerCache

	#region EmailDonationReminder
	// send email donation reminders
	class CEmailDonationReminder
	{
		public void EmailDonationReminder()
		{
			if (TestCenter.Enabled == false)
			{
				System.Console.Write("Building list of donation reminders ... ");
				Utility.TimeCheck tc = new Utility.TimeCheck();
				tc.Start();
				int Reminders = 0;
				int AcctsChecked = EmailDonationReminderWorker(out Reminders);
				tc.End();
				System.Console.WriteLine("checked {0} in {1} - {2} Accounts reminders", AcctsChecked, tc.TimeTaken, Reminders);
			}
		}

		private int EmailDonationReminderWorker(out int Reminders)
		{
			int iChecked = 0;
			Reminders = 0;
			try
			{
				// loop through the accouints looking for current users
				ArrayList results = new ArrayList();
				/*foreach (Account acct in Accounts.Table.Values)
				{
					iChecked++;
					if (acct.DoNotSendEmail == false)
						if (EmailHelpers.RecentLogin(acct, 10) == true)
						{
							Reminders++;
							results.Add(acct.EmailAddress);
						}
				}*/

				// Adam: lets keep the above code around for future needs, but for now lets use our mailing list
				//	NOTE: If you change this make sure to remove the password from the subject line
				string password = Environment.GetEnvironmentVariable("DISTLIST_PASSWORD");
				if (password == null || password.Length == 0)
					throw new ApplicationException("the password for distribution list access is not set.");
				Reminders = 1;
				iChecked = 1;
				results.Clear();
				results.Add("announcements@game-master.net");

				if (Reminders > 0)
				{
					Server.Engines.CronScheduler.DonationReminderMsg mr = new Server.Engines.CronScheduler.DonationReminderMsg();
					// okay, now hand the list of users off to our mailer daemon
					new Emailer().SendEmail(results, password + mr.m_subject, mr.m_body, false);
				}

			}
			catch (Exception e)
			{
				LogHelper.LogException(e);
				System.Console.WriteLine("Exception Caught in Donation Reminder code: " + e.Message);
				System.Console.WriteLine(e.StackTrace);
			}
			return iChecked;
		}
	}
	#endregion EmailDonationReminder

	#region WeaponDistribution
	// email a report of weapon distribution among players
	class CWeaponDistribution
	{
		public void WeaponDistribution()
		{
			if (TestCenter.Enabled == false)
			{
				System.Console.Write("Emailing a report of weapon distribution among players ... ");
				Utility.TimeCheck tc = new Utility.TimeCheck();
				tc.Start();
				int mobileschecked = WeaponDistributionWorker();
				tc.End();
				System.Console.WriteLine("checked " + mobileschecked + " characters in: " + tc.TimeTaken);
			}
		}

		private int WeaponDistributionWorker()
		{
			ArrayList toCatalog = new ArrayList();
			foreach (Mobile m in World.Mobiles.Values)
			{
				// if player and logged in and not staff...
				if (m is PlayerMobile && m.Map != Map.Internal && m.AccessLevel == AccessLevel.Player)
				{
					toCatalog.Add(m);
				}
			}

			int Archery = 0, PureArchery = 0;
			int Fencing = 0, PureFencing = 0;
			int Macing = 0, PureMacing = 0;
			int Magery = 0, PureMagery = 0;
			int Swords = 0, PureSwords = 0;

			for (int i = 0; i < toCatalog.Count; i++)
			{
				// Catalog online players only
				PlayerMobile pm = toCatalog[i] as PlayerMobile;
				if (pm != null)
				{
					int PrevTotal = Archery + Fencing + Macing + Magery + Swords;
					Archery += pm.Skills[SkillName.Archery].Base >= 90 ? 1 : 0;
					Fencing += pm.Skills[SkillName.Fencing].Base >= 90 ? 1 : 0;
					Macing += pm.Skills[SkillName.Macing].Base >= 90 ? 1 : 0;
					Magery += pm.Skills[SkillName.Magery].Base >= 90 ? 1 : 0;
					Swords += pm.Skills[SkillName.Swords].Base >= 90 ? 1 : 0;

					// now record 'pure' temps
					if ((Archery + Fencing + Macing + Magery + Swords) - PrevTotal == 1)
					{
						PureArchery += pm.Skills[SkillName.Archery].Base >= 90 ? 1 : 0;
						PureFencing += pm.Skills[SkillName.Fencing].Base >= 90 ? 1 : 0;
						PureMacing += pm.Skills[SkillName.Macing].Base >= 90 ? 1 : 0;
						PureMagery += pm.Skills[SkillName.Magery].Base >= 90 ? 1 : 0;
						PureSwords += pm.Skills[SkillName.Swords].Base >= 90 ? 1 : 0;
					}
				}
			}

			int totalFighters = toCatalog.Count;

			string archery = String.Format("{0:f2}% archers of which {1:f2}% are pure\n",
				percentage(totalFighters, Archery),
				percentage(Archery, PureArchery));

			string fencing = String.Format("{0:f2}% fencers of which {1:f2}% are pure\n",
				percentage(totalFighters, Fencing),
				percentage(Fencing, PureFencing));

			string macing = String.Format("{0:f2}% macers of which {1:f2}% are pure\n",
				percentage(totalFighters, Macing),
				percentage(Macing, PureMacing));

			string magery = String.Format("{0:f2}% mages of which {1:f2}% are pure\n",
				percentage(totalFighters, Magery),
				percentage(Magery, PureMagery));

			string swords = String.Format("{0:f2}% swordsmen of which {1:f2}% are pure\n",
				percentage(totalFighters, Swords),
				percentage(Swords, PureSwords));

			string body = String.Format(
				"There are {0} players logged in.\n" +
				archery + fencing + macing +
				magery + swords,
				toCatalog.Count);

			// okay, now send this report off
			new Emailer().SendEmail("luke@tomasello.com", "weapon distribution among players", body, false);

			return toCatalog.Count;
		}

		// totalFighters != 0 ? (Archery / totalFighters) * 100 : 0,
		private double percentage(int lside, int rside)
		{
			return (double)lside != 0.0 ? ((double)rside / (double)lside) * 100.0 : 0.0;
		}
	}
	#endregion WeaponDistribution

	#region ServerWarsAES
	// Kick off an automated Server Wars (AES)
	class CServerWarsAES
	{
		public void ServerWarsAES()
		{	// 1st Sunday of the month. However, we schedule it 24 hours prior!!
			System.Console.Write("AES Server Wars initiated ... ");
			Utility.TimeCheck tc = new Utility.TimeCheck();
			tc.Start();
			ServerWarsAES ServerWars = new ServerWarsAES();
			int Reminders = 0;
			if (TestCenter.Enabled == false && ServerWars != null)
				ServerWarsReminders(ServerWars, out Reminders);
			tc.End();
			System.Console.WriteLine("AES Server Wars scheduled.");
		}

		private int ServerWarsReminders(ServerWarsAES ServerWars, out int Reminders)
		{
			int iChecked = 0;
			Reminders = 0;
			try
			{
				// loop through the accouints looking for current users
				ArrayList results = new ArrayList();
				/*foreach (Account acct in Accounts.Table.Values)
				{
					iChecked++;
					if (acct.DoNotSendEmail == false)
						if (EmailHelpers.RecentLogin(acct, 120) == true)
						{
							Reminders++;
							results.Add(acct.EmailAddress);
						}
				}*/

				// Adam: lets keep the above code around for future needs, but for now lets use our mailing list
				//	NOTE: If you change this make sure to remove the password from the subject line
				string password = Environment.GetEnvironmentVariable("DISTLIST_PASSWORD");
				if (password == null || password.Length == 0)
					throw new ApplicationException("the password for distribution list access is not set.");
				Reminders = 1;
				iChecked = 1;
				results.Clear();
				results.Add("announcements@game-master.net");

				if (Reminders > 0)
				{
					Server.Engines.CronScheduler.ServerWarsMsg er = new Server.Engines.CronScheduler.ServerWarsMsg();

					// from Ransome AES
					DateTime EventStartTime = ServerWars.EventStartTime;		// DateTime.Now.AddDays(1.0);		// 24 hours from now
					DateTime EventEndTime = ServerWars.EventEndTime;			// EventStartTime.AddHours(3.0);	// 3 hour event
					DateTime EventStartTimeEastern = EventStartTime + TimeSpan.FromHours(3);

					// Sunday, November 20th
					string subject = String.Format(password + er.m_subject,
						EventStartTime);				// Sunday, November 20th

					string body = String.Format(er.m_body,

						// "Angel Island will be hosting another Cross Shard Challenge this {0:D} from {0:t} - {1:t} PM Pacific Time."
						EventStartTime,					// Sunday, November 20th / 3:00 PM
						EventEndTime,					// 6:00 PM End

						// "({0:t} PM Pacific Time is {2:t} Eastern)"
						EventStartTimeEastern,			// 6:00 PM Eastern Start

						// "Server Wars are actually scheduled for {0:t} PM, but sometimes start from 3-5 minutes later. If you login at {3:t}, you should be safe.\n" +
						EventStartTime + TimeSpan.FromMinutes(15)	// 3:15 PM Start
						
						// "If you want to be there from the moment it begins, login at {0:t} and simply wait for the \"Server Wars have begun!\" global announcement.\n" +
						);

					// okay, now hand the list of users off to our mailer daemon
					new Emailer().SendEmail(results, subject, body, false);
				}

			}
			catch (Exception e)
			{
				LogHelper.LogException(e);
				System.Console.WriteLine("Exception Caught in Ransome Quest Donation Reminder code: " + e.Message);
				System.Console.WriteLine(e.StackTrace);
			}
			return iChecked;
		}
	}
	#endregion ServerWarsAES

	#region TownInvasionAES
	// Kick off an automated Town Invasion (AES)
	class CTownInvasionAES
	{
		public void TownInvasionAES()
		{	// 3nd Sunday of the month. However, we schedule it 24 hours prior!!
			System.Console.Write("AES Town Invasion initiated ... ");
			Utility.TimeCheck tc = new Utility.TimeCheck();
			tc.Start();
			TownInvasionAES TownInvasion = new TownInvasionAES();
			tc.End();
			System.Console.WriteLine("AES Town Invasion scheduled.");
		}
	}
	#endregion TownInvasionAES

	#region KinRansomAES
	// Kick off an automated Kin Ransom Quest (AES)
	class CKinRansomAES
	{
		public void KinRansomAES()
		{	// 4nd Sunday of the month. However, we schedule it 24 hours prior!!
			System.Console.Write("AES Kin Ransom Quest initiated ... ");
			Utility.TimeCheck tc = new Utility.TimeCheck();
			tc.Start();
			KinRansomAES KinRansom = new KinRansomAES();
			int Reminders = 0;
			if (TestCenter.Enabled == false && KinRansom != null)
				RansomQuestReminders(KinRansom, out Reminders);
			tc.End();
			System.Console.WriteLine("AES Kin Ransom Quest scheduled with {0} reminders sent.", Reminders);
		}

		private int RansomQuestReminders(KinRansomAES KinRansom, out int Reminders)
		{
			int iChecked = 0;
			Reminders = 0;
			try
			{
				// loop through the accouints looking for current users
				ArrayList results = new ArrayList();
				/*foreach (Account acct in Accounts.Table.Values)
				{
					iChecked++;
					if (acct.DoNotSendEmail == false)
						if (EmailHelpers.RecentLogin(acct, 120) == true)
						{
							Reminders++;
							results.Add(acct.EmailAddress);
						}
				}*/

				// Adam: lets keep the above code around for future needs, but for now lets use our mailing list
				//	NOTE: If you change this make sure to remove the password from the subject line
				string password = Environment.GetEnvironmentVariable("DISTLIST_PASSWORD");
				if (password == null || password.Length == 0)
					throw new ApplicationException("the password for distribution list access is not set.");
				Reminders = 1;
				iChecked = 1;
				results.Clear();
				results.Add("announcements@game-master.net");

				if (Reminders > 0)
				{
					Server.Engines.CronScheduler.RansomQuestReminderMsg er = new Server.Engines.CronScheduler.RansomQuestReminderMsg();

					// from Ransome AES
					DateTime EventStartTime = KinRansom.EventStartTime;		// DateTime.Now.AddDays(1.0);		// 24 hours from now
					DateTime EventEndTime = KinRansom.EventEndTime;			// EventStartTime.AddHours(3.0);	// 3 hour event
					//DateTime ChestOpenTime = KinRansom.ChestOpenTime;		// EventEndTime.AddMinutes(-15.0);	// at 2hrs and 45 min, open the chest

					// Sunday, November 20th
					string subject = String.Format(password + er.m_subject,
						EventStartTime);				// Sunday, November 20th

					string body = String.Format(er.m_body,
						EventStartTime					// Sunday, November 20th
						);

					// okay, now hand the list of users off to our mailer daemon
					new Emailer().SendEmail(results, subject, body, false);
				}

			}
			catch (Exception e)
			{
				LogHelper.LogException(e);
				System.Console.WriteLine("Exception Caught in Ransome Quest Donation Reminder code: " + e.Message);
				System.Console.WriteLine(e.StackTrace);
			}
			return iChecked;
		}

	}
	#endregion KinRansomAES

	#region CrazyMapDayAES
	// Kick off an automated Crazy Map Day (AES)
	class CCrazyMapDayAES
	{
		public void CrazyMapDayAES()
		{	// 5th Sunday of the month. However, we schedule it 24 hours prior!!
			System.Console.Write("AES Crazy Map Day initiated ... ");
			Utility.TimeCheck tc = new Utility.TimeCheck();
			tc.Start();
			CrazyMapDayAES CrazyMapDay = new CrazyMapDayAES();
			tc.End();
			System.Console.WriteLine("AES Crazy Map Day scheduled.");
		}
	}
	#endregion CrazyMapDayAES

	#region AccountStatistics
	// calculate account statistics and log
	class CAccountStatistics
	{
		public void AccountStatistics()
		{
			System.Console.Write("Account Statistics running... ");

			Utility.TimeCheck tc = new Utility.TimeCheck();
			tc.Start();
			AccountStatisticsWorker();
			tc.End();
			System.Console.WriteLine("Done in " + tc.TimeTaken);
		}

		private void AccountStatisticsWorker()
		{
			try
			{
				DateTime dtNow = DateTime.Now;
				//Get Number Online Now
				int numberOnline = Server.Network.NetState.Instances.Count;
				int numberUniqueIPOnline = 0;

				Hashtable ipNS = new Hashtable();
				foreach (NetState n in Server.Network.NetState.Instances)
				{
					if (!ipNS.ContainsKey(n.Address.ToString()))
						ipNS.Add(n.Address.ToString(), null);
				}
				numberUniqueIPOnline = ipNS.Keys.Count;

				//Get the number of accounts accessed in the last 24 hours
				int numberAccessedInLastDay = 0;
				int numberAccessedInLastHour = 0;
				int numberAccessedInLastWeek = 0;

				//Unique IPs
				int numberUniqueIPLastDay = 0;
				int numberUniqueIPLastWeek = 0;
				Hashtable ipsDay = new Hashtable();
				Hashtable ipsWeek = new Hashtable();

				int accountsCreatedLastDay = 0;
				int accountsCreatedLastWeek = 0;

				ArrayList accountList = new ArrayList(Server.Accounting.Accounts.Table.Values);
				foreach (Account a in accountList)
				{
					bool bOnline = false;
					for (int j = 0; j < 5; ++j)
					{
						Mobile check = a[j];

						if (check == null)
							continue;

						if (check.NetState != null)
						{
							bOnline = true;
							break;
						}
					}

					if (bOnline || a.LastLogin > dtNow.AddHours(-1.0))
					{
						numberAccessedInLastHour++;
					}
					if (bOnline || a.LastLogin > dtNow.AddDays(-1.0))
					{
						numberAccessedInLastDay++;
						if (a.LoginIPs.Length > 0)
						{
							if (!ipsDay.ContainsKey(a.LoginIPs[0]))
								ipsDay.Add(a.LoginIPs[0], null);
						}
					}
					if (bOnline || a.LastLogin > dtNow.AddDays(-7.0))
					{
						numberAccessedInLastWeek++;
						if (a.LoginIPs.Length > 0)
						{
							if (!ipsWeek.ContainsKey(a.LoginIPs[0]))
								ipsWeek.Add(a.LoginIPs[0], null);
						}
					}

					if (a.Created > dtNow.AddDays(-1.0))
					{
						accountsCreatedLastDay++;
					}
					if (a.Created > dtNow.AddDays(-7.0))
					{
						accountsCreatedLastWeek++;
					}
				}

				numberUniqueIPLastDay = ipsDay.Keys.Count;
				numberUniqueIPLastWeek = ipsWeek.Keys.Count;

				//What else?

				try
				{
					Server.Township.TownshipSettings.CalculateFeesBasedOnServerActivity(numberAccessedInLastWeek);
				}
				catch (Exception cfbosaException)
				{
					LogHelper.LogException(cfbosaException);
				}

				string savePath = "Logs\\AccountStats.log";
				//Output to file:
				using (StreamWriter writer = new StreamWriter(new FileStream(savePath, FileMode.Append, FileAccess.Write, FileShare.Read)))
				{
					writer.WriteLine("{0}:{1}:{2}:{3}:{4}:{5}:{6}:{7}:{8}:{9}",
						dtNow.ToString(), //0
						numberOnline, //1
						numberUniqueIPOnline, //2
						numberAccessedInLastHour, //3
						numberAccessedInLastDay, //4
						numberAccessedInLastWeek, //5
						numberUniqueIPLastDay, //6
						numberUniqueIPLastWeek, //7
						accountsCreatedLastDay, //8
						accountsCreatedLastWeek //9
						);

					writer.Flush();
					writer.Close();
				}
			}
			catch (Exception e)
			{
				LogHelper.LogException(e);
				System.Console.WriteLine("Heartbeat.AccountStatistics exceptioned");
				System.Console.WriteLine(e.Message);
				System.Console.WriteLine(e.StackTrace);
			}
		}
	}
	#endregion AccountStatistics

	#region GuildFealty
	// calculate guild fealty
	class CGuildFealty
	{
		public void GuildFealty()
		{
			System.Console.Write("Counting fealty votes for guildmasters...");
			Utility.TimeCheck tc = new Utility.TimeCheck();
			tc.Start();
			int guildschecked = GuildFealtyWorker();
			tc.End();
			System.Console.WriteLine("checked " + guildschecked + " guilds in: " + tc.TimeTaken);
		}

		private int GuildFealtyWorker()
		{
			try
			{
				int count = 0;
				foreach (Guild g in BaseGuild.List.Values)
				{
					count++;
					if (g.LastFealty + TimeSpan.FromDays(1.0) < DateTime.Now)
						g.CalculateGuildmaster();
				}
				return count;
			}
			catch (Exception e)
			{
				LogHelper.LogException(e);
				Console.WriteLine("Exception while running Heartbeat.GuildFealty() job:");
				Console.WriteLine(e);
				return 0;
			}
		}
	}
	#endregion GuildFealty

	#region PlayerQuestCleanup
	// cleanup unclaimed player quest stuffs
	class CPlayerQuestCleanup
	{
		public void PlayerQuestCleanup()
		{
			System.Console.Write("Checking decay of PlayerQuest items...");
			Utility.TimeCheck tc = new Utility.TimeCheck();
			tc.Start();
			int PlayerQuestsDeleted;
			int PlayerQuestsChecked = PlayerQuestCleanupWorker(out PlayerQuestsDeleted);
			tc.End();
			System.Console.WriteLine("checked {0} quests in {1}, {2} deleted.",
					PlayerQuestsChecked, tc.TimeTaken, PlayerQuestsDeleted);
		}

		private int PlayerQuestCleanupWorker(out int PlayerQuestsDeleted)
		{
			PlayerQuestsDeleted = 0;
			int count = 0;
			LogHelper Logger = null;
			try
			{
				ArrayList ToDelete = new ArrayList();

				// find expired
				foreach (Item ix in World.Items.Values)
				{
					if (ix is BaseContainer == false) continue;
					BaseContainer bc = ix as BaseContainer;
					if (bc.PlayerQuest == false) continue;
					if (bc.Deleted == true) continue;

					count++;
					// expiration date. See sister check in PlayerQuestDeed.cs
					if (DateTime.Now > bc.LastMoved + TimeSpan.FromHours(24.0))
						ToDelete.Add(bc);
				}

				// okay, now create the log file
				if (ToDelete.Count > 0)
					Logger = new LogHelper("PlayerQuest.log", false);

				// cleanup
				for (int i = 0; i < ToDelete.Count; i++)
				{
					BaseContainer bc = ToDelete[i] as BaseContainer;
					if (bc != null)
					{
						// record the expiring quest chest
						Logger.Log(LogType.Item, bc, "Player Quest prize being deleted because the quest has expired.");
						bc.Delete();
						PlayerQuestsDeleted++;
					}
				}

			}
			catch (Exception e)
			{
				LogHelper.LogException(e);
				Console.WriteLine("Exception while running Heartbeat.PlayerQuestCleanup() job");
				Console.WriteLine(e);
			}
			finally
			{
				if (Logger != null)
					Logger.Finish();
			}

			return count;
		}
	}
	#endregion PlayerQuestCleanup

	#region PlayerQuestAnnounce
	// announce player quests
	class CPlayerQuestAnnounce
	{
		public void PlayerQuestAnnounce()
		{
			System.Console.Write("Checking for PlayerQuest announcements...");
			Utility.TimeCheck tc = new Utility.TimeCheck();
			tc.Start();
			int PlayerQuestsAnnounced;
			int PlayerQuestsChecked = PlayerQuestAnnounceWorker(out PlayerQuestsAnnounced);
			tc.End();
			System.Console.WriteLine("checked {0} quests in {1}, {2} announced.",
					PlayerQuestsChecked, tc.TimeTaken, PlayerQuestsAnnounced);
		}

		private int PlayerQuestAnnounceWorker(out int PlayerQuestsAnnounced)
		{
			PlayerQuestsAnnounced = 0;
			//LogHelper Logger = new LogHelper("PlayerQuest.log", false);
			int count = 0;

			try
			{
				count = PlayerQuestManager.Announce(out PlayerQuestsAnnounced);
			}
			catch (Exception e)
			{
				LogHelper.LogException(e);
				Console.WriteLine("Exception while running Heartbeat.PlayerQuestsAnnounce() job");
				Console.WriteLine(e);
			}
			finally
			{
				//Logger.Finish();
			}

			return count;
		}
	}
	#endregion PlayerQuestAnnounce

	#region LogRotation
	// rotate the command logs
	class CLogRotation
	{
		public void LogRotation()
		{
			System.Console.Write("Rotating player command logs...");
			Utility.TimeCheck tc = new Utility.TimeCheck();
			tc.Start();
			bool ok = LogRotationWorker();
			tc.End();
			System.Console.WriteLine("rotation {0} in {1}.",
					ok ? "completed" : "failed", tc.TimeTaken);
		}

		private bool LogRotationWorker()
		{
			try
			{
				RotateLogs.RotateNow();
				return true;
			}
			catch (Exception e)
			{
				LogHelper.LogException(e);
				Console.WriteLine("Exception while running Heartbeat.LogRotation() job");
				Console.WriteLine(e);
			}
			return false;
		}
	}
	#endregion LogRotation

	#region BackupRunUO
	// at 2AM backup the complete RunUO directory (~8min)
	class CBackupRunUO
	{
		public void BackupRunUO()
		{
			System.Console.Write("Sending maintenance message...");
			DateTime now = DateTime.Now;
			DateTime when = new DateTime(now.Year, now.Month, now.Day, 2, 0, 0, 0);
			TimeSpan delta = when - now;
			string text = String.Format("Maintenance in {0:00.00} minutes and should last for {1} minutes.", delta.TotalMinutes, 8);
			World.Broadcast(0x35, true, text);
			System.Console.WriteLine(text);
			System.Console.WriteLine("Done.");
		}
	}
	#endregion BackupRunUO

	#region BackupWorldData
	// at 4AM we backup only the world files (~4min)
	class CBackupWorldData
	{
		public void BackupWorldData()
		{
			System.Console.Write("Sending maintenance message...");
			DateTime now = DateTime.Now;
			DateTime when = new DateTime(now.Year, now.Month, now.Day, 4, 0, 0, 0);
			TimeSpan delta = when - now;
			string text = String.Format("Maintenance in {0:00.00} minutes and should last for {1} minutes.", delta.TotalMinutes, 4);
			World.Broadcast(0x35, true, text);
			System.Console.WriteLine(text);
			System.Console.WriteLine("Done.");
		}
	}
	#endregion BackupWorldData

	#region TownshipCharges
	// calculate township charges
	class CTownshipCharges
	{
		public void TownshipCharges()
		{
			System.Console.Write("Performing Township Charges...");
			Utility.TimeCheck tc = new Utility.TimeCheck();
			tc.Start();
			int townshipcount = TownshipStone.AllTownshipStones.Count;
			int townshipsremoved = TownshipStone.DoAllTownshipCharges();
			tc.End();
			System.Console.WriteLine("removed {0} of {1} townships in: {2}.", townshipsremoved, townshipcount, tc.TimeTaken);
		}
	}
	#endregion TownshipCharges

	#region FreezeDryInit
	// freezedry containers that need it after startup .. run once after startup
	class CFreezeDryInit
	{
		public void FreezeDryInit()
		{
			System.Console.Write("Freeze Dry starup init...");
			Utility.TimeCheck tc = new Utility.TimeCheck();
			tc.Start();
			int scheduled = FreezeDryInitWorker();
			tc.End();
			System.Console.WriteLine("{0} containers scheduled for freeze drying in: {1}.", scheduled, tc.TimeTaken);
		}

		private int FreezeDryInitWorker()
		{
			int scheduled = 0;

			try
			{
				foreach (Item i in World.Items.Values)
				{
					if (i as Container != null)
					{
						Container cx = i as Container;
						if (cx.ScheduleFreeze())
							scheduled++;
					}
				}
			}
			catch (Exception e)
			{
				LogHelper.LogException(e);
				System.Console.WriteLine("FreezeDryInit code: " + e.Message);
				System.Console.WriteLine(e.StackTrace);
			}

			return scheduled;
		}
	}
	#endregion FreezeDryInit

	#region WealthTracker
	// see who's making all the money on the shard (look for exploits)
	class CWealthTracker
	{
		public void WealthTracker()
		{
			System.Console.Write("Collecting WealthTracker information...");
			Utility.TimeCheck tc = new Utility.TimeCheck();
			tc.Start();
			WealthTrackerWorker();
			tc.End();
			System.Console.WriteLine("WealthTracker complete : {0}.", tc.TimeTaken);
		}

		private void WealthTrackerWorker()
		{
			try
			{
				int limit = 10;     // top 10 farmers
				int timeout = 90;   // active within the last 90 minutes

				// compile the constrained list
				Server.Engines.WealthTracker.IPDomain[] list = Server.Engines.WealthTracker.ReportCompiler(limit, timeout);

				LogHelper Logger1 = new LogHelper("WealthTrackerNightly.log", true);	// this one gets emailed each night
				LogHelper Logger2 = new LogHelper("WealthTracker.log", false);			// this is a running account

				// write a super minimal report
				for (int ix = 0; ix < list.Length; ix++)
				{
					Server.Engines.WealthTracker.IPDomain node = list[ix] as Server.Engines.WealthTracker.IPDomain;
					Server.Engines.WealthTracker.AccountDomain ad = Server.Engines.WealthTracker.GetFirst(node.accountList) as Server.Engines.WealthTracker.AccountDomain; // just first account
					Mobile m = Server.Engines.WealthTracker.GetFirst(ad.mobileList) as Mobile;                   // just first mobile
					string sx = String.Format("mob:{2}, gold:{0}, loc:{1}", node.gold, node.location, m);
					Logger1.Log(LogType.Text, sx);
					Logger2.Log(LogType.Text, sx);
				}

				Logger1.Finish();
				Logger2.Finish();
			}
			catch (Exception e)
			{
				LogHelper.LogException(e);
				System.Console.WriteLine("WealthTracker code: " + e.Message);
				System.Console.WriteLine(e.StackTrace);
			}
		}
	}
	#endregion WealthTracker

	#region ConsumerPriceIndex
	// see who's making all the money on the shard (look for exploits)
	class CConsumerPriceIndex
	{
		public void ConsumerPriceIndex()
		{
			System.Console.Write("Collecting ConsumerPriceIndex information...");
			Utility.TimeCheck tc = new Utility.TimeCheck();
			tc.Start();
			ConsumerPriceIndexWorker();
			tc.End();
			System.Console.WriteLine("ConsumerPriceIndex complete : {0}.", tc.TimeTaken);
		}

		private void ConsumerPriceIndexWorker()
		{
			try
			{
				LogHelper Logger1 = new LogHelper("ConsumerPriceIndexNightly.log", true);	// this one gets emailed each night
				LogHelper Logger2 = new LogHelper("ConsumerPriceIndex.log", false);			// this is a running account

				string s1, s2;
				Scripts.Commands.Diagnostics.CPI_Worker(out s1, out s2);
				Logger1.Log(LogType.Text, s1);
				Logger1.Log(LogType.Text, s2);
				Logger2.Log(LogType.Text, s1);
				Logger2.Log(LogType.Text, s2);

				Logger1.Finish();
				Logger2.Finish();
			}
			catch (Exception e)
			{
				LogHelper.LogException(e);
				System.Console.WriteLine("ConsumerPriceIndex code: " + e.Message);
				System.Console.WriteLine(e.StackTrace);
			}
		}
	}
	#endregion ConsumerPriceIndex

	#region KinFactions
	// do kin faction processing
	class CKinFactions
	{
		public void KinFactions()
		{
			System.Console.Write("Processing faction kin sigils....");
			Utility.TimeCheck tc = new Utility.TimeCheck();
			tc.Start();
			KinCityManager.ProcessSigils();
			tc.End();
			System.Console.WriteLine("Kin faction sigil processing complete : {0}.", tc.TimeTaken);
		}
	}
	// do kin faction logging processing
	class CKinFactionsLogging
	{
		public void KinFactionsLogging()
		{
			System.Console.Write("Processing faction logs....");
			Utility.TimeCheck tc = new Utility.TimeCheck();
			tc.Start();
			KinCityManager.ProcessAndOutputLogs();
			tc.End();
			System.Console.WriteLine("Kin faction logs complete : {0}.", tc.TimeTaken);
		}
	}
	#endregion KinFactions

	#region SpringChamp
	// turn on spring champ / turn off winter champ
	class CSpringChamp
	{
		public void SpringChamp()			// turn on spring champ / turn off winter champ()
		{
			System.Console.Write("Processing seasonal champ....");
			Utility.TimeCheck tc = new Utility.TimeCheck();
			tc.Start();
			SpringChampWorker();
			tc.End();
			System.Console.WriteLine("Seasonal champ processing complete : {0}.", tc.TimeTaken);
		}

		// turn on spring champ / turn off winter champ
		private void SpringChampWorker()
		{
			try
			{
				// turn on the Spring Champ
				ChampHelpers.ToggleChamp(0x4002BF49, true);

				// turn off the Winter Champ
				ChampHelpers.ToggleChamp(0x400A3DA6, false);
			}
			catch (Exception e)
			{
				LogHelper.LogException(e);
				System.Console.WriteLine("Seasonal champ code: " + e.Message);
				System.Console.WriteLine(e.StackTrace);
			}
		}
	}
	#endregion SpringChamp

	#region SummerChamp
	// turn on summer champ / turn off spring champ
	class CSummerChamp
	{
		public void SummerChamp()			// turn on summer champ / turn off spring champ()
		{
			System.Console.Write("Processing seasonal champ....");
			Utility.TimeCheck tc = new Utility.TimeCheck();
			tc.Start();
			SummerChampWorker();
			tc.End();
			System.Console.WriteLine("Seasonal champ processing complete : {0}.", tc.TimeTaken);
		}

		private void SummerChampWorker()
		{
			try
			{
				// turn on summer champ - Vamp
				ChampHelpers.ToggleChamp(0x40000096, true);

				// turn off spring champ - Pirate
				ChampHelpers.ToggleChamp(0x4002BF49, false);
			}
			catch (Exception e)
			{
				LogHelper.LogException(e);
				System.Console.WriteLine("Seasonal champ code: " + e.Message);
				System.Console.WriteLine(e.StackTrace);
			}
		}
	}
	#endregion SummerChamp

	#region AutumnChamp
	// turn on autumn champ / turn off summer champ
	class CAutumnChamp
	{
		public void AutumnChamp()			// turn on autumn champ / turn off summer champ()
		{
			System.Console.Write("Processing seasonal champ....");
			Utility.TimeCheck tc = new Utility.TimeCheck();
			tc.Start();
			AutumnChampWorker();
			tc.End();
			System.Console.WriteLine("Seasonal champ processing complete : {0}.", tc.TimeTaken);
		}

		private void AutumnChampWorker()
		{
			try
			{
				// turn on autumn champ
				ChampHelpers.ToggleChamp(0x4001A454, true);

				// turn off summer champ
				ChampHelpers.ToggleChamp(0x40143E2B, false);
			}
			catch (Exception e)
			{
				LogHelper.LogException(e);
				System.Console.WriteLine("Seasonal champ code: " + e.Message);
				System.Console.WriteLine(e.StackTrace);
			}
		}
	}
	#endregion AutumnChamp

	#region WinterChamp
	// turn on winter champ / turn off autumn champ
	class CWinterChamp
	{
		public void WinterChamp()			// turn on winter champ / turn off autumn champ()
		{
			System.Console.Write("Processing seasonal champ....");
			Utility.TimeCheck tc = new Utility.TimeCheck();
			tc.Start();
			WinterChampWorker();
			tc.End();
			System.Console.WriteLine("Seasonal champ processing complete : {0}.", tc.TimeTaken);
		}

		private void WinterChampWorker()
		{
			try
			{
				// turn on winter champ
				ChampHelpers.ToggleChamp(0x400A3DA6, true);

				// turn off autumn champ
				ChampHelpers.ToggleChamp(0x4001A454, false);
			}
			catch (Exception e)
			{
				LogHelper.LogException(e);
				System.Console.WriteLine("Seasonal champ code: " + e.Message);
				System.Console.WriteLine(e.StackTrace);
			}
		}
	}
	#endregion WinterChamp

	#region CHAMP HELPERS
	public class ChampHelpers
	{
		public static void ToggleChamp(int champ, bool state)
		{
			try
			{
				// turn on a Champ
				ChampEngine Champ = World.FindItem(champ) as ChampEngine;
				if (Champ == null)
					new ApplicationException(String.Format("World.FindItem({0:X}) as ChampEngine", champ));
				else
				{	// state will be true for ON and false for OFF
					// We disable a champ by turning off the restart timer
					// We enable by turning on the restart timer and making sure the Active property is true

					// leave Active true so the current champ will finish i.e., don't turn off, but ensure it's turned on
					if (state == true)
						Champ.Active = state;

					// kill the restart timer so that it doesn't restart
					Champ.RestartTimer = state;
				}
			}
			catch (Exception e)
			{
				LogHelper.LogException(e);
				System.Console.WriteLine("Seasonal champ code: " + e.Message);
				System.Console.WriteLine(e.StackTrace);
			}

		}
	}
	#endregion

	#region EMAIL HELPERS
	public class EmailHelpers
	{
		public static bool RecentLogin(Account acct, int days)
		{
			bool empty = true;		// any characters on the account?
			for (int i = 0; i < 5; ++i)
			{
				if (acct[i] != null)
					empty = false;	// not empty
			}

			// if not empty AND active within the last N days, send a reminder
			if (empty == false)
			{
				TimeSpan delta = DateTime.Now - acct.LastLogin;
				if (delta.TotalDays <= days)
				{	// send a reminder
					return true;
				}
			}

			// no reminder should be sent
			return false;
		}
	}
	#endregion EMAIL HELPERS
	
}

