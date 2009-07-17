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

namespace Server.Engines.PartySystem
{
	public sealed class PartyEmptyList : Packet
	{
		public PartyEmptyList( Mobile m ) : base( 0xBF )
		{
			EnsureCapacity( 7 );

			m_Stream.Write( (short) 0x0006 );
			m_Stream.Write( (byte) 0x02 );
			m_Stream.Write( (byte) 0 );
			m_Stream.Write( (int) m.Serial );
		}
	}

	public sealed class PartyMemberList : Packet
	{
		public PartyMemberList( Party p ) : base( 0xBF )
		{
			EnsureCapacity( 7 + p.Count*4 );

			m_Stream.Write( (short) 0x0006 );
			m_Stream.Write( (byte) 0x01 );
			m_Stream.Write( (byte) p.Count );

			for ( int i = 0; i < p.Count; ++i )
				m_Stream.Write( (int) p[i].Mobile.Serial );
		}
	}

	public sealed class PartyRemoveMember : Packet
	{
		public PartyRemoveMember( Mobile removed, Party p ) : base( 0xBF )
		{
			EnsureCapacity( 11 + p.Count*4 );

			m_Stream.Write( (short) 0x0006 );
			m_Stream.Write( (byte) 0x02 );
			m_Stream.Write( (byte) p.Count );

			m_Stream.Write( (int) removed.Serial );

			for ( int i = 0; i < p.Count; ++i )
				m_Stream.Write( (int) p[i].Mobile.Serial );
		}
	}

	public sealed class PartyTextMessage : Packet
	{
		public PartyTextMessage( bool toAll, Mobile from, string text ) : base( 0xBF )
		{
			if ( text == null )
				text = "";

			EnsureCapacity( 12 + text.Length*2 );

			m_Stream.Write( (short) 0x0006 );
			m_Stream.Write( (byte) (toAll ? 0x04 : 0x03) );
			m_Stream.Write( (int) from.Serial );
			m_Stream.WriteBigUniNull( text );
		}
	}

	public sealed class PartyInvitation : Packet
	{
		public PartyInvitation( Mobile leader ) : base( 0xBF )
		{
			EnsureCapacity( 10 );

			m_Stream.Write( (short) 0x0006 );
			m_Stream.Write( (byte) 0x07 );
			m_Stream.Write( (int) leader.Serial );
		}
	}
}�