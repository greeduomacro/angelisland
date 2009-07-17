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
using System.Runtime.InteropServices;

namespace Server
{
	public enum ZLibError : int
	{
		Z_OK = 0,
		Z_STREAM_END = 1,
		Z_NEED_DICT = 2,
		Z_ERRNO = (-1),
		Z_STREAM_ERROR = (-2),
		Z_DATA_ERROR = (-3),
		Z_MEM_ERROR = (-4),
		Z_BUF_ERROR = (-5),
		Z_VERSION_ERROR = (-6),
	}

	public enum ZLibCompressionLevel : int
	{
		Z_NO_COMPRESSION = 0,
		Z_BEST_SPEED = 1,
		Z_BEST_COMPRESSION = 9,
		Z_DEFAULT_COMPRESSION = (-1)
	}

	public class ZLib
	{
		[DllImport("./server/bin/zlib.dll")]
		public static extern string zlibVersion();
		[DllImport("./server/bin/zlib.dll")]
		public static extern ZLibError compress( byte[] dest, ref int destLength, byte[] source, int sourceLength );
		[DllImport("./server/bin/zlib.dll")]
		public static extern ZLibError compress2( byte[] dest, ref int destLength, byte[] source, int sourceLength, ZLibCompressionLevel level );
		[DllImport("./server/bin/zlib.dll")]
		public static extern ZLibError uncompress( byte[] dest, ref int destLen, byte[] source, int sourceLen );
	}
}
