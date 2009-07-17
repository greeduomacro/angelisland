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
using Server.Misc;
using Server.Network;
using Server.Accounting;
using Server.Scripts.Commands;

namespace Server.Engines.Chat
{
	public class ChatSystem
	{
		private static bool m_Enabled = true;

		public static bool Enabled
		{
			get{ return m_Enabled; }
			set{ m_Enabled = value; }
		}

		public static void Initialize()
		{
			PacketHandlers.Register( 0xB5, 0x40, true, new OnPacketReceive( OpenChatWindowRequest ) );
			PacketHandlers.Register( 0xB3, 0, true, new OnPacketReceive( ChatAction ) );
		}

		public static void SendCommandTo( Mobile to, ChatCommand type )
		{
			SendCommandTo( to, type, null, null );
		}

		public static void SendCommandTo( Mobile to, ChatCommand type, string param1 )
		{
			SendCommandTo( to, type, param1, null );
		}

		public static void SendCommandTo( Mobile to, ChatCommand type, string param1, string param2 )
		{
			if ( to != null )
				to.Send( new ChatMessagePacket( null, (int)type + 20, param1, param2 ) );
		}

		public static void OpenChatWindowRequest( NetState state, PacketReader pvSrc )
		{
			Mobile from = state.Mobile;

			if ( !m_Enabled )
			{
				from.SendMessage( "The chat system has been disabled." );
				return;
			}

			pvSrc.Seek( 2, System.IO.SeekOrigin.Begin );
			string chatName = pvSrc.ReadUnicodeStringSafe( ( 0x40 - 2 ) >> 1 ).Trim();

			Account acct = state.Account as Account;

			string accountChatName = null;

			if ( acct != null )
				accountChatName = acct.GetTag( "ChatName" );

			if ( accountChatName != null )
				accountChatName = accountChatName.Trim();

			if ( accountChatName != null && accountChatName.Length > 0 )
			{
				if ( chatName.Length > 0 && chatName != accountChatName )
					from.SendMessage( "You cannot change chat nickname once it has been set." );
			}
			else
			{
				if ( chatName == null || chatName.Length == 0 )
				{
					SendCommandTo( from, ChatCommand.AskNewNickname );
					return;
				}

				if ( NameVerification.Validate( chatName, 2, 31, true, true, true, 0, NameVerification.SpaceDashPeriodQuote ) && chatName.ToLower().IndexOf( "system" ) == -1 )
				{
					// TODO: Optimize this search

					foreach ( Account checkAccount in Accounts.Table.Values )
					{
						string existingName = checkAccount.GetTag( "ChatName" );

						if ( existingName != null )
						{
							existingName = existingName.Trim();

							if ( Insensitive.Equals( existingName, chatName ) )
							{
								from.SendMessage( "Nickname already in use." );
								SendCommandTo( from, ChatCommand.AskNewNickname );
								return;
							}
						}
					}

					accountChatName = chatName;

					if ( acct != null )
						acct.AddTag( "ChatName", chatName );
				}
				else
				{
					from.SendLocalizedMessage( 501173 ); // That name is disallowed.
					SendCommandTo( from, ChatCommand.AskNewNickname );
					return;
				}
			}

			SendCommandTo( from, ChatCommand.OpenChatWindow, accountChatName );
			ChatUser.AddChatUser( from );
		} 

		public static ChatUser SearchForUser( ChatUser from, string name )
		{
			ChatUser user = ChatUser.GetChatUser( name );

			if ( user == null )
				from.SendMessage( 32, name ); // There is no player named '%1'.

			return user;
		}

		public static void ChatAction( NetState state, PacketReader pvSrc )
		{
			if ( !m_Enabled )
				return;

			try
			{
				Mobile from = state.Mobile;
				ChatUser user = ChatUser.GetChatUser( from );

				if ( user == null )
					return;

				string lang = pvSrc.ReadStringSafe( 4 );
				int actionID = pvSrc.ReadInt16();
				string param = pvSrc.ReadUnicodeString();

				ChatActionHandler handler = ChatActionHandlers.GetHandler( actionID );

				if ( handler != null )
				{
					Channel channel = user.CurrentChannel;

					if ( handler.RequireConference && channel == null )
					{
						user.SendMessage( 31 ); /* You must be in a conference to do this.
												 * To join a conference, select one from the Conference menu.
												 */
					}
					else if ( handler.RequireModerator && !user.IsModerator )
					{
						user.SendMessage( 29 ); // You must have operator status to do this.
					}
					else
					{
						handler.Callback( user, channel, param );
					}
				}
				else
				{
					Console.WriteLine( "Client: {0}: Unknown chat action 0x{1:X}: {2}", state, actionID, param );
				}
			}
			catch ( Exception e )
			{
				LogHelper.LogException(e);
				Console.WriteLine( e );
			}
		}
	}
}