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

/* Scripts/Regions/Jail.cs
 * CHANGELOG:
 *  *	04/24/09, plasma
 *		Commented out all regions, replaced with DRDT
 *	3/6/05: Pixie
 *		Addition of Jail.Special stuff
 */

using System;
using System.Collections;
using System.IO;
using Server;
using Server.Mobiles;
using Server.Spells;
using Server.Spells.Seventh;
using Server.Spells.Fourth;
using Server.Spells.Sixth;
using Server.Scripts.Commands;

namespace Server.Regions
{
	public class Jail : Region
	{
		private static string[] SpecialNames;

		public static void Initialize()
		{
			//Region.AddRegion( new Jail( Map.Felucca ) );
			// Region.AddRegion( new Jail( Map.Trammel ) );
			
			SpecialNames = new string[0];
			//load special
			ArrayList special = new ArrayList();

			try 
			{
				using (StreamReader sr = new StreamReader("Data\\Special.txt")) 
				{
					String line;
					while ((line = sr.ReadLine()) != null) 
					{
						if( line != null && line.Length > 0 )
						{
							special.Add(line);
						}
					}
				}
			}
			catch (Exception e) 
			{
				LogHelper.LogException(e);
				Console.WriteLine("The Data\\Special.txt file could not be read:");
				Console.WriteLine(e.Message);
			}
			
			SpecialNames = (string[])special.ToArray( typeof(string) );
		}

		public static bool IsInSpecial( string name )
		{
			try
			{
				if( SpecialNames != null )
				{
					for( int i=0; i<SpecialNames.Length; i++ )
					{
						if( SpecialNames[i] != null )
						{
							if( name.ToLower() == SpecialNames[i].ToLower() )
							{
								return true;
							}
						}
					}
				}
			}
			catch (Exception ex) { EventSink.InvokeLogException(new LogExceptionEventArgs(ex)); }

			return false;
		}

		public Jail( Map map ) : base( "", "Jail", map )
		{
		}

		public override bool AllowBenificial( Mobile from, Mobile target )
		{
			if ( from.AccessLevel == AccessLevel.Player )
				from.SendMessage( "You may not do that in jail." );

			return ( from.AccessLevel > AccessLevel.Player );
		}

		public override bool AllowHarmful( Mobile from, Mobile target )
		{
			if ( from.AccessLevel == AccessLevel.Player )
				from.SendMessage( "You may not do that in jail." );

			return ( from.AccessLevel > AccessLevel.Player );
		}

		public override bool AllowHousing( Mobile from, Point3D p )
		{
			return false;
		}

		public override void AlterLightLevel( Mobile m, ref int global, ref int personal )
		{
			global = LightCycle.JailLevel;
		}

		public override bool OnBeginSpellCast( Mobile from, ISpell s )
		{
			if ( from.AccessLevel == AccessLevel.Player )
				from.SendLocalizedMessage( 502629 ); // You cannot cast spells here.

			return ( from.AccessLevel > AccessLevel.Player );
		}

		public override bool OnSkillUse( Mobile from, int Skill )
		{
			if ( from.AccessLevel == AccessLevel.Player )
				from.SendMessage( "You may not use skills in jail." );

			return ( from.AccessLevel > AccessLevel.Player );
		}

		public override bool OnCombatantChange( Mobile from, Mobile Old, Mobile New )
		{
			return ( from.AccessLevel > AccessLevel.Player );
		}

	}
}