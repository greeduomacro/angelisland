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
using Server.Network;
using Server.Prompts;

namespace Server.Engines.Help
{
	public class PagePrompt : Prompt
	{
		private PageType m_Type;

		public PagePrompt( PageType type )
		{
			m_Type = type;
		}

		public override void OnCancel( Mobile from )
		{
			from.SendLocalizedMessage( 501235, "", 0x35 ); // Help request aborted.
		}

		public override void OnResponse( Mobile from, string text )
		{
			from.SendLocalizedMessage( 501234, "", 0x35 ); /* The next available Counselor/Game Master will respond as soon as possible.
															* Please check your Journal for messages every few minutes.
															*/

			PageQueue.Enqueue( new PageEntry( from, text, m_Type ) );
		}
	}
} 
