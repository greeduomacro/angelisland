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
using System.Collections;
using Server.Targeting;
using Server.Network;

/* Scripts/Engines/AI/Team/Team.cs
 * CHANGE LOG
 *	9/18/05, Adam
 *		nada
 */

/*  
 * NPC could use a team objets.. 
 * 
 * -List of members
 * -List of ennemy teams
 * -List of ally team
 * -Team could be set automaticaly at mobile creation by the region system
 * -Team could be the owner of a common timer instead of one by creature
 *
 * 
 * 
 */

namespace Server.Mobiles
{
	public class Team
	{
		//private ArrayList m_arAlly;
		//private ArrayList m_arFoe;
		
		//private ArrayList m_arMember;

		public static void Initialize()
		{

		}
	}
}