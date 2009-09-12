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

/* Scripts/Commands/Abstracted/Implementors/SingleCommandImplementor.cs
 * CHANGELOG
 *  6/14/04, Pix
 *		Removed debugging message.
 *  6/7/04, Pix
 *		Reverted the previous fix, but skipped the check for IsAccessible
 *		if it gets the BountyCommand.
 *  6/7/04, Pix
 *		1.0RC0's OnTarget added a check to the new IsAccessible command which
 *		broke the Bounty command.  Removing the check for IsAccessible.
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */


using System;
using System.Collections;
using Server;
using Server.Targeting;

namespace Server.Scripts.Commands
{
	public class SingleCommandImplementor : BaseCommandImplementor
	{
		public SingleCommandImplementor()
		{
			Accessors = new string[] { "Single" };
			SupportRequirement = CommandSupport.Single;
			AccessLevel = AccessLevel.Counselor;
			Usage = "Single <command>";
			Description = "Invokes the command on a single targeted object. This is the same as just invoking the command directly.";
		}

		public override void Register(BaseCommand command)
		{
			base.Register(command);

			for (int i = 0; i < command.Commands.Length; ++i)
				Server.Commands.Register(command.Commands[i], command.AccessLevel, new CommandEventHandler(Redirect));
		}

		public void Redirect(CommandEventArgs e)
		{
			BaseCommand command = (BaseCommand)Commands[e.Command];

			if (command == null)
				e.Mobile.SendMessage("That is either an invalid command name or one that does not support this modifier.");
			else if (e.Mobile.AccessLevel < command.AccessLevel)
				e.Mobile.SendMessage("You do not have access to that command.");
			else if (command.ValidateArgs(this, e))
				Process(e.Mobile, command, e.Arguments);
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

			if (command is BountySystem.BountyCommand)
			{
				//from.SendMessage("Use the bountycommand, Luke!");
			}
			else if (!BaseCommand.IsAccessible(from, targeted))
			{
				from.SendMessage("That is not accessible.");
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
		}
	}
}