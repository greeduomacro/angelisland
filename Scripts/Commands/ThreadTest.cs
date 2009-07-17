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

/* Scripts\Commands\ThreadTest.cs
 *  CHANGELOG:
 *  5/6/08, Adam
 *		Turned off taran's thread manager as we're not using it and it makes profiling a bit tougher
 */

using System;
using System.Threading;

#if JobManager
namespace Server
{
	/// <summary>
	/// Summary description for ThreadTest.
	/// </summary>
	public class ThreadTest
	{
		public static void Initialize()
		{
			Server.Commands.Register("StartJM", AccessLevel.Administrator, new CommandEventHandler(StartJM_Cmd));
			Server.Commands.Register("JMStats", AccessLevel.Administrator, new CommandEventHandler(JMStats_Cmd));
		}

		public static void StartJM_Cmd(CommandEventArgs e)
		{
			Timer.DelayCall(TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(100), 100, new TimerCallback(StressTester));
		}

		public static void StressTester()
		{	
			ThreadJob job = new ThreadJob(new JobWorker(StressWorker), Utility.RandomMinMax(50, 5000), new JobCompletedCallback(StressCallback));
			job.Start((JobPriority)Utility.RandomMinMax(0, 4));
		}

		public static object StressWorker(object parms)
		{
			int delay = (int)parms;

			Console.WriteLine("Beginning sleep for {0}ms.", delay);
			Thread.Sleep(delay);
			Console.WriteLine("Finished sleeping for {0}ms.", delay);

			return delay;
		}

		public static void StressCallback(ThreadJob job)
		{
			Console.WriteLine("Callback with results for job {0}", job.Results);
		}

		public static void JMStats_Cmd(CommandEventArgs e)
		{
			Console.WriteLine("JobManager stats:");
			Console.WriteLine("MinThreadCount:				{0}", JobManager.MinThreadCount);
			Console.WriteLine("MaxThreadCount:				{0}", JobManager.MaxThreadCount);
			Console.WriteLine("CurrentThreadCount:			{0}", JobManager.CurrentThreadCount);
			Console.WriteLine("ReadyThreadCount:			{0}", JobManager.ReadyThreadCount);
			Console.WriteLine("RunningThreadCount:			{0}", JobManager.RunningThreadCount);
			Console.WriteLine("TotalEnqueuedJobs:			{0}", JobManager.TotalEnqueuedJobs);
			Console.WriteLine("IdleThreadLifespan:			{0}ms", JobManager.IdleThreadLifespan);
			Console.WriteLine("PriorityPromotionDelay:			{0}ms", JobManager.PriorityPromotionDelay);
			Console.WriteLine("LastError: {0}", JobManager.LastError);
		}
	}
}
#endif
