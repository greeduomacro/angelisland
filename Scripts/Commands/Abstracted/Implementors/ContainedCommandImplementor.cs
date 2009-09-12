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

using System;
using System.Collections;
using Server;
using Server.Items;
using Server.Targeting;

namespace Server.Scripts.Commands
{
	public class ContainedCommandImplementor : BaseCommandImplementor
	{
		public ContainedCommandImplementor()
		{
			Accessors = new string[] { "Contained" };
			SupportRequirement = CommandSupport.Contained;
			AccessLevel = AccessLevel.GameMaster;
			Usage = "Contained <command> [condition]";
			Description = "Invokes the command on all child items in a targeted container. Optional condition arguments can further restrict the set of objects.";
		}

		public override void Process(Mobile from, BaseCommand command, string[] args)
		{
			if (command.ValidateArgs(this, new CommandEventArgs(from, command.Commands[0], GenerateArgString(args), args)))
				from.BeginTarget(-1, command.ObjectTypes == ObjectTypes.All, TargetFlags.None, new TargetStateCallback(OnTarget), new object[] { command, args });
		}

		public void OnTarget(Mobile from, object targeted, object state)
		{
			if (!BaseCommand.IsAccessible(from, targeted))
			{
				from.SendMessage("That is not accessible.");
				return;
			}

			object[] states = (object[])state;
			BaseCommand command = (BaseCommand)states[0];
			string[] args = (string[])states[1];

			if (command.ObjectTypes == ObjectTypes.Mobiles)
				return; // sanity check

			if (!(targeted is Container))
			{
				from.SendMessage("That is not a container.");
			}
			else
			{
				try
				{
					ObjectConditional cond = ObjectConditional.Parse(from, ref args);

					bool items, mobiles;

					if (!CheckObjectTypes(command, cond, out items, out mobiles))
						return;

					if (!items)
					{
						from.SendMessage("This command only works on items.");
						return;
					}

					Container cont = (Container)targeted;

					Item[] found;

					if (cond.Type == null)
						found = cont.FindItemsByType(typeof(Item), true);
					else
						found = cont.FindItemsByType(cond.Type, true);

					ArrayList list = new ArrayList();

					for (int i = 0; i < found.Length; ++i)
					{
						if (cond.CheckCondition(found[i]))
							list.Add(found[i]);
					}

					RunCommand(from, list, command, args);
				}
				catch (Exception e)
				{
					LogHelper.LogException(e);
					from.SendMessage(e.Message);
				}
			}
		}
	}
}