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

/* Scripts/Commands/Abstracted/Implementors/RegionCommandImplementor.cs
 * CHANGELOG
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */


using System;
using System.Collections;
using Server;

namespace Server.Scripts.Commands
{
	public class RegionCommandImplementor : BaseCommandImplementor
	{
		public RegionCommandImplementor()
		{
			Accessors = new string[]{ "Region" };
			SupportRequirement = CommandSupport.Region;
			SupportsConditionals = true;
			AccessLevel = AccessLevel.Administrator;
			Usage = "Region <command> [condition]";
			Description = "Invokes the command on all appropriate mobiles in your current region. Optional condition arguments can further restrict the set of objects.";
		}

		public override void Compile( Mobile from, BaseCommand command, ref string[] args, ref object obj )
		{
			try
			{
				ObjectConditional cond = ObjectConditional.Parse( from, ref args );

				bool items, mobiles;

				if ( !CheckObjectTypes( command, cond, out items, out mobiles ) )
					return;

				Region reg = from.Region;

				ArrayList list = new ArrayList();

				if ( mobiles )
				{
					foreach ( Mobile mob in reg.Mobiles.Values )
					{
						if ( cond.CheckCondition( mob ) )
							list.Add( mob );
					}
				}
				else
				{
					command.LogFailure( "This command does not support mobiles." );
					return;
				}

				obj = list;
			}
			catch ( Exception ex )
			{
				LogHelper.LogException(ex);
				from.SendMessage( ex.Message );
			}
		}
	}
}