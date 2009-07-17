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
using Server;
using Server.Network;

namespace Server.Engines.Chat
{
	public sealed class ChatMessagePacket : Packet
	{
		public ChatMessagePacket( Mobile who, int number, string param1, string param2 ) : base( 0xB2 )
		{
			if ( param1 == null )
				param1 = String.Empty;

			if ( param2 == null )
				param2 = String.Empty;

			EnsureCapacity( 13 + ((param1.Length + param2.Length) * 2) );

			m_Stream.Write( (ushort) (number - 20) );

			if ( who != null )
				m_Stream.WriteAsciiFixed( who.Language, 4 );
			else
				m_Stream.Write( (int) 0 );

			m_Stream.WriteBigUniNull( param1 );
			m_Stream.WriteBigUniNull( param2 );
		}
	}
} 
