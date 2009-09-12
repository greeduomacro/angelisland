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

/* Scripts/Commands/FindMobile.cs
 * CHANGELOG:
 *	7/2/07, Adam
 *		Add FindMobileByName as well	
 *	04/23/05, Kit
 *		Changed to use reflection as per finditem and search propertys for all types derived from Mobile.
 *	03/25/05, erlein
 *		Integrated with LogHelper class.
 *		Altered argument handling so concatenates and matches strings too.
 *	03/23/05, erlein
 *		Moved to /Scripts/Commands/FindMobile.cs (for Find* command normalization).
 *		Changed namespace to Server.Scripts.Commands.
 *	3/7/05, Adam
 *		Add player's account name to output
 *	3/6/05: Pixie
 *		Initial Version
 */

using System;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Scripts.Commands;
using System.Reflection;
using System.Collections;
using System.Text.RegularExpressions;

namespace Server.Scripts.Commands
{

	public class FindMobile
	{
		public static void Initialize()
		{
			Server.Commands.Register("FindMobile", AccessLevel.Administrator, new CommandEventHandler(FindMobile_OnCommand));
			Server.Commands.Register("FindMobileByName", AccessLevel.GameMaster, new CommandEventHandler(FindMobileByName_OnCommand));
		}

		[Usage("FindMobile <property> <value>")]
		[Description("Finds all Mobiles with property matching value.")]
		public static void FindMobile_OnCommand(CommandEventArgs e)
		{
			if (e.Length > 1)
			{

				LogHelper Logger = new LogHelper("findMobile.log", e.Mobile, false);

				// Extract property & value from command parameters

				string sProp = e.GetString(0);
				string sVal = "";

				if (e.Length > 2)
				{

					sVal = e.GetString(1);

					// Concatenate the strings
					for (int argi = 2; argi < e.Length; argi++)
						sVal += " " + e.GetString(argi);
				}
				else
					sVal = e.GetString(1);

				Regex PattMatch = new Regex("= \"*" + sVal, RegexOptions.IgnoreCase);

				// Loop through assemblies and add type if has property

				Type[] types;
				Assembly[] asms = ScriptCompiler.Assemblies;

				ArrayList MatchTypes = new ArrayList();

				for (int i = 0; i < asms.Length; ++i)
				{
					types = ScriptCompiler.GetTypeCache(asms[i]).Types;

					foreach (Type t in types)
					{

						if (typeof(Mobile).IsAssignableFrom(t))
						{

							// Reflect type
							PropertyInfo[] allProps = t.GetProperties(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public);

							foreach (PropertyInfo prop in allProps)
								if (prop.Name.ToLower() == sProp.ToLower())
									MatchTypes.Add(t);
						}
					}
				}

				// Loop items and check vs. types

				foreach (Mobile m in World.Mobiles.Values)
				{
					Type t = m.GetType();
					bool match = false;

					foreach (Type MatchType in MatchTypes)
					{
						if (t == MatchType)
						{
							match = true;
							break;
						}
					}

					if (match == false)
						continue;

					// Reflect instance of type (matched)

					if (PattMatch.IsMatch(Properties.GetValue(e.Mobile, m, sProp)))
						Logger.Log(LogType.Mobile, m);

				}

				Logger.Finish();
			}
			else
			{
				// Badly formatted
				e.Mobile.SendMessage("Format: FindMobile <property> <value>");
			}
		}

		[Usage("FindMobileByName <name>")]
		[Description("Finds all Mobiles with specified name.")]
		public static void FindMobileByName_OnCommand(CommandEventArgs e)
		{
			if (e.Length == 1)
			{

				LogHelper Logger = new LogHelper("findMobile.log", e.Mobile, true);

				// The name to find
				string sName = e.GetString(0);

				foreach (Mobile m in World.Mobiles.Values)
				{
					PlayerMobile pm = m as PlayerMobile;
					if (pm != null)
						if (pm.Name.ToLower() == sName.ToLower())
							Logger.Log(LogType.Mobile, m, String.Format("Online: {0}", ((bool)(pm.NetState != null)).ToString()));
				}

				Logger.Finish();
			}
			else
			{
				// Badly formatted
				e.Mobile.SendMessage("Usage: FindMobileByName <name>");
			}
		}
	}
}
