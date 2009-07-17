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

namespace Server.Misc
{
	public enum ProfanityAction
	{
		None,			// no action taken
		Disallow,		// speech is not displayed
		Criminal,		// makes the player criminal, not killable by guards
		CriminalAction,	// makes the player criminal, can be killed by guards
		Disconnect,		// player is kicked
		Other			// some other implementation
	}

	public class ProfanityProtection
	{
		private static bool Enabled = false;
		private static ProfanityAction Action = ProfanityAction.Disallow; // change here what to do when profanity is detected

		public static void Initialize()
		{
			if ( Enabled )
				EventSink.Speech += new SpeechEventHandler( EventSink_Speech );
		}

		private static bool OnProfanityDetected( Mobile from, string speech )
		{
			switch ( Action )
			{
				case ProfanityAction.None: return true;
				case ProfanityAction.Disallow: return false;
				case ProfanityAction.Criminal: from.Criminal = true; return true;
				case ProfanityAction.CriminalAction: from.CriminalAction( false ); return true;
				case ProfanityAction.Disconnect:
				{
					NetState ns = from.NetState;

					if ( ns != null )
						ns.Dispose();

					return false;
				}
				default:
				case ProfanityAction.Other: // TODO: Provide custom implementation if this is chosen
				{
					return true;
				}
			}
		}

		private static void EventSink_Speech( SpeechEventArgs e )
		{
			Mobile from = e.Mobile;

			if ( from.AccessLevel > AccessLevel.Player )
				return;

			if ( !NameVerification.Validate( e.Speech, 0, int.MaxValue, true, true, false, int.MaxValue, m_Exceptions, m_Disallowed, m_StartDisallowed ) )
				e.Blocked = !OnProfanityDetected( from, e.Speech );
		}

		private static char[] m_Exceptions = new char[]
			{
				' ', '-', '.', '\'', '"', ',', '_', '+', '=', '~', '`', '!', '^', '*', '\\', '/', ';', ':', '<', '>', '[', ']', '{', '}', '?', '|', '(', ')', '%', '$', '&', '#', '@'
			};

		private static string[] m_StartDisallowed = new string[]{};

		private static string[] m_Disallowed = new string[]
			{
				"jigaboo",
				"chigaboo",
				"wop",
				"kyke",
				"kike",
				"tit",
				"spic",
				"prick",
				"piss",
				"lezbo",
				"lesbo",
				"felatio",
				"dyke",
				"dildo",
				"chinc",
				"chink",
				"cunnilingus",
				"cum",
				"cocksucker",
				"cock",
				"clitoris",
				"clit",
				"ass",
				"hitler",
				"penis",
				"nigga",
				"nigger",
				"klit",
				"kunt",
				"jiz",
				"jism",
				"jerkoff",
				"jackoff",
				"goddamn",
				"fag",
				"blowjob",
				"bitch",
				"asshole",
				"dick",
				"pussy",
				"snatch",
				"cunt",
				"twat",
				"shit",
				"fuck"
			};
	}
} 
