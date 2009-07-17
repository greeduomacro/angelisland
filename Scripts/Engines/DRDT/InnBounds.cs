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

/* /Scripts/Engines/DRDT/CustomRegion.cs
 *  05/03/05, Kit
 *	 Initial creation
 */
using System;
using Server;
using System.Collections;
using Server.Regions;
using Server.Targeting;
using Server.Items;

namespace Server.Scripts.Commands
{
	public class InnBounds
	{
		public static void Initialize() 
		{
			Server.Commands.Register( "InnBounds", AccessLevel.GameMaster, new CommandEventHandler( InnBounds_OnCommand ) ); 
		}

		[Usage( "InnBounds" )]
		[Description( "Displays the bounding area of Inn's in a targeted RegionControl." )]
		private static void InnBounds_OnCommand( CommandEventArgs e ) 
		{
			e.Mobile.Target = new RegionBoundTarget();
			e.Mobile.SendMessage( "Target a Mobile or RegionControl" );
			e.Mobile.SendMessage( "Please note that Players will also be able to see the bounds of the Inn." );
		}

		private class RegionBoundTarget : Target
		{
			public RegionBoundTarget() : base( -1, false, TargetFlags.None )
			{
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
			
				if( targeted is RegionControl )
				{
					Region r = ((RegionControl)targeted).MyRegion;

					if ( r == null || r.InnBounds == null || r.InnBounds.Count == 0 )
					{
						from.SendMessage( "No Inns defined for targeted RegionControl." );
						return;
					}

					from.SendMessage( "Displaying targeted RegionControl's Inns..." );
					
					ShowInnBounds( r );
				}
				else
				{
					from.SendMessage( "That is not a RegionControl" );
				}
			}
		}


		public static void ShowRectBounds( Rectangle2D r, Map m )
		{
			if( m == Map.Internal || m == null )
				return;

			Point3D p1 = new Point3D( r.X, r.Y - 1, 0 );	//So we dont' need to create a new one each point
			Point3D p2 = new Point3D( r.X, r.Y + r.Height - 1, 0 );	//So we dont' need to create a new one each point

			Effects.SendLocationEffect( new Point3D( r.X -1, r.Y - 1, m.GetAverageZ( r.X, r.Y -1 ) ) , m, 251, 75, 1, 1151, 3 );	//Top Corner	//Testing color

			for( int x = r.X; x <= ( r.X + r.Width -1 ); x++ )
			{
				p1.X = x;
				p2.X = x;

				p1.Z = m.GetAverageZ( p1.X, p1.Y );
				p2.Z = m.GetAverageZ( p2.X, p2.Y );

				Effects.SendLocationEffect( p1, m, 249, 75, 1, 1151, 3 );	//North bound
				Effects.SendLocationEffect( p2, m, 249, 75, 1, 1151, 3 );	//South bound
			}

			p1 = new Point3D( r.X -1 , r.Y -1 , 0 );
			p2 = new Point3D( r.X + r.Width - 1, r.Y, 0 );

			for( int y = r.Y; y <= ( r.Y + r.Height -1 ); y++ )
			{
				p1.Y = y;
				p2.Y = y;

				p1.Z = m.GetAverageZ( p1.X, p1.Y );
				p2.Z = m.GetAverageZ( p2.X, p2.Y );

				Effects.SendLocationEffect( p1, m, 250, 75, 1, 1151, 3 );	//West Bound
				Effects.SendLocationEffect( p2, m, 250, 75, 1, 1151, 3 );	//East Bound
			}
		}


		public static void ShowInnBounds( Region r )
		{
			if( r == null || r.InnBounds == null || r.InnBounds.Count == 0)
				return;

			ArrayList c = r.InnBounds;

			for( int i = 0; i < c.Count; i++ )
			{
				if( c[i] is Rectangle2D )
					ShowRectBounds( (Rectangle2D)c[i], r.Map );
			}
		}
	}
}
