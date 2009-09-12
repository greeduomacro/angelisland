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

/* Scripts/Commands/Abstracted/Implementors/MultiCommandImplementor.cs
 * CHANGELOG
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */

using System;
using System.Collections;
using Server;
using Server.Targeting;

namespace Server.Scripts.Commands
{
	public class MultiCommandImplementor : BaseCommandImplementor
	{
		public MultiCommandImplementor()
		{
			Accessors = new string[] { "Multi", "m" };
			SupportRequirement = CommandSupport.Multi;
			AccessLevel = AccessLevel.Counselor;
			Usage = "Multi <command>";
			Description = "Invokes the command on multiple targeted objects.";
		}

		public override void Process(Mobile from, BaseCommand command, string[] args)
		{
			if (command.ValidateArgs(this, new CommandEventArgs(from, command.Commands[0], GenerateArgString(args), args)))
				from.BeginTarget(-1, command.ObjectTypes == ObjectTypes.All, TargetFlags.None, new TargetStateCallback(OnTarget), new object[] { command, args });
		}

		public void OnTarget(Mobile from, object targeted, object state)
		{
			object[] states = (object[])state;
			BaseCommand command = (BaseCommand)states[0];
			string[] args = (string[])states[1];

			if (!BaseCommand.IsAccessible(from, targeted))
			{
				from.SendMessage("That is not accessible.");
				from.BeginTarget(-1, command.ObjectTypes == ObjectTypes.All, TargetFlags.None, new TargetStateCallback(OnTarget), new object[] { command, args });
				return;
			}

			switch (command.ObjectTypes)
			{
				case ObjectTypes.Both:
					{
						if (!(targeted is Item) && !(targeted is Mobile))
						{
							from.SendMessage("This command does not work on that.");
							return;
						}

						break;
					}
				case ObjectTypes.Items:
					{
						if (!(targeted is Item))
						{
							from.SendMessage("This command only works on items.");
							return;
						}

						break;
					}
				case ObjectTypes.Mobiles:
					{
						if (!(targeted is Mobile))
						{
							from.SendMessage("This command only works on mobiles.");
							return;
						}

						break;
					}
			}

			RunCommand(from, targeted, command, args);

			from.BeginTarget(-1, command.ObjectTypes == ObjectTypes.All, TargetFlags.None, new TargetStateCallback(OnTarget), new object[] { command, args });
		}
	}
}