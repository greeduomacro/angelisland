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

/* /Scripts/Engines/Party/DeclineTimer.cs
 * Changelog:
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */

using System;
using System.Collections;
using Server;

namespace Server.Engines.PartySystem
{
	public class DeclineTimer : Timer
	{
		private Mobile m_Mobile, m_Leader;

		private static Hashtable m_Table = new Hashtable();

		public static void Start( Mobile m, Mobile leader )
		{
			DeclineTimer t = (DeclineTimer)m_Table[m];

			if ( t != null )
				t.Stop();

			m_Table[m] = t = new DeclineTimer( m, leader );
			t.Start();
		}

		private DeclineTimer( Mobile m, Mobile leader ) : base( TimeSpan.FromSeconds( 10.0 ) )
		{
			m_Mobile = m;
			m_Leader = leader;
		}

		protected override void OnTick()
		{
			m_Table.Remove( m_Mobile );

			if ( m_Mobile.Party == m_Leader && PartyCommands.Handler != null )
				PartyCommands.Handler.OnDecline( m_Mobile, m_Leader );
		}
	}
}