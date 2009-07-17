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

/* Scripts\Engines\IOBSystem\Misc\TextDefinition.cs
 * ChangeLog
 *	12/16/08, Adam
 *		Ported from RunUO 2.0
 */

using System;

namespace Server
{
	public class TextDefinition
	{
		private int m_Number;
		private string m_String;

		public int Number{ get{ return m_Number; } }
		public string String{ get{ return m_String; } }

		public TextDefinition( int number ) : this( number, null )
		{
		}

		public TextDefinition( string text ) : this( 0, text )
		{
		}

		public TextDefinition( int number, string text )
		{
			m_Number = number;
			m_String = text;
		}

		public override string ToString()
		{
			if ( m_Number > 0 )
				return "#" + m_Number.ToString();
			else if ( m_String != null )
				return m_String;

			return "(empty)";
		}

		public static void AddTo( ObjectPropertyList list, TextDefinition def )
		{
			if ( def != null && def.m_Number > 0 )
				list.Add( def.m_Number );
			else if ( def != null && def.m_String != null )
				list.Add( def.m_String );
		}

		public static implicit operator TextDefinition ( int v )
		{
			return new TextDefinition( v );
		}

		public static implicit operator TextDefinition ( string s )
		{
			return new TextDefinition( s );
		}

		public static implicit operator int ( TextDefinition m )
		{
			if ( m == null )
				return 0;

			return m.m_Number;
		}

		public static implicit operator string ( TextDefinition m )
		{
			if ( m == null )
				return null;

			return m.m_String;
		}

		public static void AddHtmlText( Server.Gumps.Gump g, int x, int y, int width, int height, TextDefinition text, bool back, bool scroll, int numberColor, int stringColor )
		{
			if( text != null && text.Number > 0 )
				if( numberColor >= 0 )
					g.AddHtmlLocalized( x, y, width, height, text.Number, numberColor, back, scroll );
				else
					g.AddHtmlLocalized( x, y, width, height, text.Number, back, scroll );
			else if( text != null && text.String != null )
				if( stringColor >= 0 )
					g.AddHtml( x, y, width, height, String.Format( "<BASEFONT COLOR=#{0:X6}>{1}</BASEFONT>", stringColor, text.String ), back, scroll );
				else			
					g.AddHtml( x, y, width, height, text.String, back, scroll );
		}
		public static void AddHtmlText( Server.Gumps.Gump g, int x, int y, int width, int height, TextDefinition text, bool back, bool scroll )
		{
			AddHtmlText( g, x, y, width, height, text, back, scroll, -1, -1 );
		}

		public static void SendMessageTo( Mobile m, TextDefinition def )
		{
			if( def.Number > 0 )
				m.SendLocalizedMessage( def.Number );
			else if( def.String != null )
				m.SendMessage( def.String );
		}
	}
}