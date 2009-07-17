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

namespace Server.Items
{
	public enum WeaponQuality
	{
		Low,
		Regular,
		Exceptional
	}

	public enum WeaponType
	{
		Axe,		// Axes, Hatches, etc. These can give concussion blows
		Slashing,	// Katana, Broadsword, Longsword, etc. Slashing weapons are poisonable
		Staff,		// Staves
		Bashing,	// War Hammers, Maces, Mauls, etc. Two-handed bashing delivers crushing blows
		Piercing,	// Spears, Warforks, Daggers, etc. Two-handed piercing delivers paralyzing blows
		Polearm,	// Halberd, Bardiche
		Ranged,		// Bow, Crossbows
		Fists		// Fists
	}

	public enum WeaponDamageLevel
	{
		Regular,
		Ruin,
		Might,
		Force,
		Power,
		Vanq
	}

	public enum WeaponAccuracyLevel
	{
		Regular,
		Accurate,
		Surpassingly,
		Eminently,
		Exceedingly,
		Supremely
	}

	public enum WeaponDurabilityLevel
	{
		Regular,
		Durable,
		Substantial,
		Massive,
		Fortified,
		Indestructible
	}

	public enum WeaponAnimation
	{
		Slash1H = 9,
		Pierce1H = 10,
		Bash1H = 11,
		Bash2H = 12,
		Slash2H = 13,
		Pierce2H = 14,
		ShootBow = 18,
		ShootXBow = 19,
		Wrestle = 31
	}
} 
