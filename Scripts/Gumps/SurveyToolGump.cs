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
/*      changelog.
 *      
 *    9/23/04 Lego Eater.
 *            Made Survey Tool Gump From Warninggump.cs
 *
 *
 *
 *
 */

using System;
using Server;

namespace Server.Gumps
{
	public delegate void SurveyToolGumpCallback( Mobile from, bool okay, object state );

	public class SurveyToolGump : Gump
	{
		private SurveyToolGumpCallback m_Callback;
		private object m_State;

		public SurveyToolGump( int header, int headerColor, object content, int contentColor, int width, int height, SurveyToolGumpCallback callback, object state ) : base( 50, 50)
		{
			m_Callback = callback;
			m_State = state;

			Closable = true;

			AddPage( 0 );

			AddBackground( 10, 10, 190, 140, 0x242C );

			
			AddHtml( 30, 30, 150, 75, String.Format( "<div align=CENTER>{0}</div>", "This house seems to fit here." ), false, false );

			


			AddButton( 40, 85, 4005, 4007, 0, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 40, 107, 0x81A, 0x81B, 1011036, 32767, false, false ); // okay
		}

		public override void OnResponse( Server.Network.NetState sender, RelayInfo info )
		{
			if ( info.ButtonID == 1 && m_Callback != null )
				m_Callback( sender.Mobile, true, m_State );
			else if ( m_Callback != null )
				m_Callback( sender.Mobile, false, m_State );
		}
	}
} 
