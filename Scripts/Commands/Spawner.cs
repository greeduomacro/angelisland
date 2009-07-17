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

/* Changelog 
 * 1/07/05, Darva
 *		First checkin.
 *
 */
using System;
using Server.Mobiles;
using Server.Targeting;
namespace Server.Scripts.Commands
{
	public class SpawnerCmd
	{
		public static void Initialize()
		{
			Register();
		}

		public static void Register()
		{
			Server.Commands.Register( "Spawner", AccessLevel.GameMaster, new CommandEventHandler( Spawner_OnCommand ) );
		}

		[Usage( "Spawner" )]
		[Description( "Moves you to the spawner of the targeted creature, if any." )]
		private static void Spawner_OnCommand( CommandEventArgs e )
		{
			e.Mobile.Target = new SpawnerTarget();
		}

		private class SpawnerTarget : Target
		{
			public SpawnerTarget( ) : base( -1, true, TargetFlags.None )
			{
			}

			protected override void OnTarget( Mobile from, object o )
			{
				if ( o is BaseCreature )
					{	
					if (((BaseCreature)o).Spawner != null)
						{
							BaseCreature bc = (BaseCreature)o;
							from.MoveToWorld(bc.Spawner.Location, bc.Spawner.Map);
						}
					else
						{
							from.SendMessage("That mobile is homeless");
						}
					}
				else
					{
						from.SendMessage("Why would that have a spawner?");
					}
			}
		}
	}
}