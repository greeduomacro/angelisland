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

/* scripts/commands/Aggressors.cs
 * 	CHANGELOG:
 * 	3/24/05, Kitaras
 *	Initial Version
 */

using System;
using System.Collections;
using System.IO;
using System.Xml;
using Server;
using Server.Mobiles;
using Server.Items;
using Server.Scripts.Commands;

namespace Server.Scripts.Commands
{

	public class AggressorsCommand : BaseCommand
	{

		public static void Initialize()
		{
			TargetCommands.Register(new AggressorsCommand());
		}

		public AggressorsCommand()
		{
			AccessLevel = AccessLevel.GameMaster;
			Supports = CommandSupport.Simple;
			Commands = new string[] { "Aggressors", "Aggres" };
			ObjectTypes = ObjectTypes.Mobiles;

			Usage = "Aggressors <target>";
			Description = "Lists the aggressor list of the target";
		}

		public override void Execute(CommandEventArgs e, object obj)
		{
			Mobile m = obj as Mobile;
			Mobile from = e.Mobile;

			if (m != null)
			{
				ArrayList aggressors = m.Aggressors;
				if (aggressors.Count > 0)
				{
					for (int i = 0; i < aggressors.Count; ++i)
					{

						AggressorInfo info = (AggressorInfo)aggressors[i];
						Mobile temp = info.Attacker;
						from.SendMessage("Aggressor:{0} '{1}' Ser:{2}, Time:{3}, Expired:{4}",
									(temp is PlayerMobile ? ((PlayerMobile)temp).Account.ToString() : ((Mobile)temp).Name),
									temp.GetType().Name,
									temp.Serial,
									info.LastCombatTime.TimeOfDay,
									info.Expired);
					}
				}
			}
			else
			{
				AddResponse("Please target a mobile.");
			}
		}

	}


}

