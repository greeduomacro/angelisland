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
using Server.Mobiles;

namespace Server.ContextMenus
{
	public class TeachEntry : ContextMenuEntry
	{
		private SkillName m_Skill;
		private BaseCreature m_Mobile;
		private Mobile m_From;

		public TeachEntry(SkillName skill, BaseCreature m, Mobile from, bool enabled)
			: base(6000 + (int)skill, 4)
		{
			m_Skill = skill;
			m_Mobile = m;
			m_From = from;

			if (!enabled)
				Flags |= Network.CMEFlags.Disabled;
		}

		public override void OnClick()
		{
			if (!m_From.CheckAlive())
				return;

			m_Mobile.Teach(m_Skill, m_From, 0, false);
		}
	}
}
