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
using System.IO;
using Microsoft.Win32;
using Server;

namespace Server.Misc
{
	public class DataPath
	{
		/* If you have not installed Ultima Online,
		 * or wish the server to use a seperate set of datafiles,
		 * change the 'CustomPath' value, example:
		 * 
		 * private const string CustomPath = @"C:\Program Files\Ultima Online";
		 */
		private static string CustomPath = null;

		/* The following is a list of files which a required for proper execution:
		 * 
		 * Multi.idx
		 * Multi.mul
		 * VerData.mul
		 * TileData.mul
		 * Map*.mul
		 * StaIdx*.mul
		 * Statics*.mul
		 * MapDif*.mul
		 * MapDifL*.mul
		 * StaDif*.mul
		 * StaDifL*.mul
		 * StaDifI*.mul
		 */

		public static void Configure()
		{
			string pathReg = GetExePath( "Ultima Online" );
			string pathTD = GetExePath( "Ultima Online Third Dawn" );

			if ( CustomPath != null )
				Core.DataDirectories.Add( CustomPath );

			if ( pathReg != null )
				Core.DataDirectories.Add( pathReg );

			if ( pathTD != null )
				Core.DataDirectories.Add( pathTD );

			if ( Core.DataDirectories.Count == 0 )
			{
				Console.WriteLine( "Enter the Ultima Online directory:" );
				Console.Write( "> " );

				Core.DataDirectories.Add( Console.ReadLine() );
			}
		}

		private static string GetExePath( string subName )
		{
			try
			{
				using ( RegistryKey key = Registry.LocalMachine.OpenSubKey( String.Format( @"SOFTWARE\Origin Worlds Online\{0}\1.0", subName ) ) )
				{
					if ( key == null )
						return null;

					string v = key.GetValue( "ExePath" ) as string;

					if ( v == null || v.Length <= 0 )
						return null;

					if ( !File.Exists( v ) )
						return null;

					v = Path.GetDirectoryName( v );

					if ( v == null )
						return null;

					return v;
				}
			}
			catch
			{
				return null;
			}
		}
	}
} 
