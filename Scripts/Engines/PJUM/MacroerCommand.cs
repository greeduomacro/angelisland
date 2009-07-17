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
 *	restrictions set forth in subaragraph (c)(1)(ii) of the Rights in
 * 	Technical Data and Computer Software clause at DFARS 252.227-7013.
 *
 *	Angel Island UO Shard	Version 1.0
 *			Release A
 *			March 25, 2004
 */

/* Scripts/Engines/PJUM/MacroerCommand.cs
 * ChangeLog
 *	3/25/09, Adam
 *		Auto jail player if caught macroing a second time (via the automatic system) within the 8 criminal phase.
 *		Players manually targeted by staff are not auto jailed.
 *	03/27/07, Pix
 *		Implemented RTT for AFK resource gathering thwarting.
 *	02/11/06, Adam
 *		Make common the formatting of sextant coords.
 *	01/13/06, Pix
 *		Changes due to TCCS/PJUM separation.
 *  11/13/05 Taran Kain
 *		Tells the reporting staffmember how many times the player has been reported before.
 *	10/17/05, Adam
 *		Reduced access to AccessLevel.Counselor
 *	3/29/05, Adam
 *		changed sentense to 4 hours from 8
 *  3/28/05 - Pix
 *		Fixed random amount to depend on amount in macroers bank.
 *	3/28/05 - Pix
 *		Now doesn't add a macroer to the list if he's already on the list.
 *	3/27/05 - Pix
 *		Now tries to consume 1-3K from the macroer's bank account
 *	1/27/05 - Pix
 *		Initial Version.
 */

using System;
using Server;
using Server.Accounting;
using Server.BountySystem;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;
using Server.Engines;
using Server.Scripts.Commands;

namespace Server.PJUM
{
	public class MacroerCommand
	{
		public static void Initialize()
		{
			Commands.Register( "Macroer", AccessLevel.Counselor, new CommandEventHandler( Macroer_OnCommand ) );
		}

		public MacroerCommand()//Mobile m, DateTime dt, Location loc)
		{
		}

		public static void Macroer_OnCommand( CommandEventArgs e )
		{
			e.Mobile.Target = new MacroerTarget();
			e.Mobile.SendMessage("Who do you wish to report as a macroer?");
		}

		public static void ReportAsMacroer(Mobile from, PlayerMobile pm)
		{
			string location;
			int xLong = 0, yLat = 0, xMins = 0, yMins = 0;
			bool xEast = false, ySouth = false;
			Map map = pm.Map;
			bool valid = Sextant.Format(pm.Location, map, ref xLong, ref yLat, ref xMins, ref yMins, ref xEast, ref ySouth);

			if (valid)
				location = Sextant.Format(xLong, yLat, xMins, yMins, xEast, ySouth);
			else
				location = "????";

			if (!valid)
				location = string.Format("{0} {1}", pm.X, pm.Y);

			if (map != null)
			{
				Region reg = pm.Region;

				if (reg != map.DefaultRegion)
				{
					location += (" in " + reg);
				}
			}

			//Output command log.
			if (from != null)
			{
				Server.Scripts.Commands.CommandLogging.WriteLine(from, "{0} used [Macroer command on {1}({2}) - at {3}",
					from.Name, pm.Name, pm.Serial, location);
			}

			if (from != null) from.SendMessage("Reporting {0} as an AFK macroer!", pm.Name);
			Account acct = pm.Account as Account;
			int count = 0;
			foreach (AccountComment comm in acct.Comments)
			{
				if (comm.Content.IndexOf(" : reported using the [macroer command") != -1)
					count++;
			}
			if( from != null ) from.SendMessage("{0} has been reported for macroing {1} times before.", pm.Name, count);
			string[] lns = new string[2];
			lns[0] = String.Format("A bounty has been placed on the head of {0} for unlawful resource gathering.", pm.Name);
			lns[1] = String.Format("{0} was last seen at {1}.", pm.Name, location);


			if (PJUM.HasBeenReported(pm))
			{
				if( from != null ) from.SendMessage("{0} has already been reported.", pm.Name);
				if (from == null)
				{ // the system is automatically jailing this player.
					Jail.JailPlayer jt = new Jail.JailPlayer(pm, 3, "Caught macroing again within 8 hours by automated system.", false);
					jt.GoToJail();
				}
			}
			else
			{
				// Adam: changed to 4 hours from 8
				PJUM.AddMacroer(lns, pm, DateTime.Now + TimeSpan.FromHours(4));

				//Add bounty to player
				string name = String.Format("Officer {0}", Utility.RandomBool() ? NameList.RandomName("male") : NameList.RandomName("female"));

				int bountyAmount = 0;
				Container cont = pm.BankBox;
				if (cont != null)
				{
					int iAmountInBank = 0;

					Item[] golds = cont.FindItemsByType(typeof(Gold), true);
					foreach (Item g in golds)
					{
						iAmountInBank += g.Amount;
					}

					int min = Math.Min(iAmountInBank, 1000);
					int max = Math.Min(iAmountInBank, 3000);

					int randomAmount = Utility.RandomMinMax(min, max);

					if (cont.ConsumeTotal(typeof(Gold), randomAmount))
					{
						bountyAmount = randomAmount;
					}
				}
				if (bountyAmount < 1500)
				{
					bountyAmount = Utility.RandomMinMax(1000,3000);
				}

				Bounty bounty = null;
				if( from != null ) bounty = new Bounty((PlayerMobile)from, pm, bountyAmount, name);
				else bounty = new Bounty(null, pm, bountyAmount, name);
				if( bounty != null ) BountyKeeper.Add(bounty);

				//Add comment to account
				Account acc = pm.Account as Account;
				string comment = String.Format("On {0}, {1} caught {2} unattended macroing at {3} : reported using the [macroer command",
					DateTime.Now,
					from!=null?from.Name:"auto-RTT",
					pm.Name,
					location);
				acc.Comments.Add(new AccountComment(from != null ? from.Name : "auto-RTT", comment));
			}
		}
	
		private class MacroerTarget : Target
		{
			public MacroerTarget() : base( 15, false, TargetFlags.None )
			{
			}

			protected override void OnTarget( Mobile from, object targ )
			{
				if( targ is PlayerMobile )
				{
					try
					{
						PlayerMobile pm = (PlayerMobile)targ;
						MacroerCommand.ReportAsMacroer(from, pm);
					}
					catch(Exception except)
					{
						LogHelper.LogException(except);
						System.Console.WriteLine("Caught exception in [macroer command: {0}", except.Message);
						System.Console.WriteLine(except.StackTrace);
					}
				}
				else
				{
					from.SendMessage("Only players are macroers.");
				}
			}
		}

	}
}
