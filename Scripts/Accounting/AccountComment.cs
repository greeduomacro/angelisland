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
using System.Xml;

namespace Server.Accounting
{
	public class AccountComment
	{
		private string m_AddedBy;
		private string m_Content;
		private DateTime m_LastModified;

		/// <summary>
		/// A string representing who added this comment.
		/// </summary>
		public string AddedBy
		{
			get{ return m_AddedBy; }
		}

		/// <summary>
		/// Gets or sets the body of this comment. Setting this value will reset LastModified.
		/// </summary>
		public string Content
		{
			get{ return m_Content; }
			set{ m_Content = value; m_LastModified = DateTime.Now; }
		}

		/// <summary>
		/// The date and time when this account was last modified -or- the comment creation time, if never modified.
		/// </summary>
		public DateTime LastModified
		{
			get{ return m_LastModified; }
		}

		/// <summary>
		/// Constructs a new AccountComment instance.
		/// </summary>
		/// <param name="addedBy">Initial AddedBy value.</param>
		/// <param name="content">Initial Content value.</param>
		public AccountComment( string addedBy, string content )
		{
			m_AddedBy = addedBy;
			m_Content = content;
			m_LastModified = DateTime.Now;
		}

		/// <summary>
		/// Deserializes an AccountComment instance from an xml element.
		/// </summary>
		/// <param name="node">The XmlElement instance from which to deserialize.</param>
		public AccountComment( XmlElement node )
		{
			m_AddedBy = Accounts.GetAttribute( node, "addedBy", "empty" );
			m_LastModified = Accounts.GetDateTime( Accounts.GetAttribute( node, "lastModified" ), DateTime.Now );
			m_Content = Accounts.GetText( node, "" );
		}

		/// <summary>
		/// Serializes this AccountComment instance to an XmlTextWriter.
		/// </summary>
		/// <param name="xml">The XmlTextWriter instance from which to serialize.</param>
		public void Save( XmlTextWriter xml )
		{
			xml.WriteStartElement( "comment" );

			xml.WriteAttributeString( "addedBy", m_AddedBy );
			xml.WriteAttributeString( "lastModified", XmlConvert.ToString( m_LastModified ) );

			xml.WriteString( m_Content );

			xml.WriteEndElement();
		}
	}
}�