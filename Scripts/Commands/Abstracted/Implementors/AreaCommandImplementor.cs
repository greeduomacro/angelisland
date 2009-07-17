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

/* Scripts/Commands/Abstracted/Implementors/AreaCommandImplementor.cs
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
	public class AreaCommandImplementor : BaseCommandImplementor
	{
		public AreaCommandImplementor()
		{
			Accessors = new string[]{ "Area", "Group" };
			SupportRequirement = CommandSupport.Area;
			SupportsConditionals = true;
			AccessLevel = AccessLevel.GameMaster;
			Usage = "Area <command> [condition]";
			Description = "Invokes the command on all appropriate objects in a targeted area. Optional condition arguments can further restrict the set of objects.";
		}

		public override void Process( Mobile from, BaseCommand command, string[] args )
		{
			BoundingBoxPicker.Begin( from, new BoundingBoxCallback( OnTarget ), new object[]{ command, args } );
		}

		public void OnTarget( Mobile from, Map map, Point3D start, Point3D end, object state )
		{
			try
			{
				object[] states = (object[])state;
				BaseCommand command = (BaseCommand)states[0];
				string[] args = (string[])states[1];

				ObjectConditional cond = ObjectConditional.Parse( from, ref args );

				Rectangle2D rect = new Rectangle2D( start.X, start.Y, end.X - start.X + 1, end.Y - start.Y + 1 );

				bool items, mobiles;

				if ( !CheckObjectTypes( command, cond, out items, out mobiles ) )
					return;

				IPooledEnumerable eable;

				if ( items && mobiles )
					eable = map.GetObjectsInBounds( rect );
				else if ( items )
					eable = map.GetItemsInBounds( rect );
				else if ( mobiles )
					eable = map.GetMobilesInBounds( rect );
				else
					return;

				ArrayList objs = new ArrayList();

				foreach ( object obj in eable )
				{
					if ( mobiles && obj is Mobile && !BaseCommand.IsAccessible( from, obj ) )
						continue;

					if ( cond.CheckCondition( obj ) )
						objs.Add( obj );
				}

				eable.Free();

				RunCommand( from, objs, command, args );
			}
			catch ( Exception ex )
			{
				LogHelper.LogException(ex);
				from.SendMessage( ex.Message );
			}
		}

		public void OnTarget( Mobile from, object targeted, object state )
		{
			try
			{
				object[] states = (object[])state;
				BaseCommand command = (BaseCommand)states[0];
				string[] args = (string[])states[1];

				switch ( command.ObjectTypes )
				{
					case ObjectTypes.Both:
					{
						if ( !(targeted is Item) && !(targeted is Mobile) )
						{
							from.SendMessage( "This command does not work on that." );
							return;
						}

						break;
					}
					case ObjectTypes.Items:
					{
						if ( !(targeted is Item) )
						{
							from.SendMessage( "This command only works on items." );
							return;
						}

						break;
					}
					case ObjectTypes.Mobiles:
					{
						if ( !(targeted is Mobile) )
						{
							from.SendMessage( "This command only works on mobiles." );
							return;
						}

						break;
					}
				}

				RunCommand( from, targeted, command, args );

				from.BeginTarget( -1, command.ObjectTypes == ObjectTypes.All, TargetFlags.None, new TargetStateCallback( OnTarget ), new object[]{ command, args } );
			}
			catch ( Exception ex )
			{
				LogHelper.LogException(ex);
				from.SendMessage( ex.Message );
			}
		}
	}
}