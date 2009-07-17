/*	
 *	This program is the CONFIDENTIAL and PROPRIETARY property
 *	of Tomasello Software LLC. Any unauthorized use, reproduction or
 *	transfer of this computer program is strictly prohibited.
 *
 *      Copyright (c) 2004 Tomasello Software LLC.
 *	This is an unpublished work, and is subject to limited distribution and
 *	restricted disclosure only. ALL RIGHTS RESERVED.
 *
 *			RESTRICTED RIGHTS LEGEND
 *	Use, duplication, or disclosure by the Government is subject to
 *	restrictions set forth in subparagraph (c)(1)(ii) of the Rights in
 * 	Technical Data and Computer Software clause at DFARS 252.227-7013.
 *
 *	Angel Island UO Shard	Version 1.0
 *			Release A
 *			March 25, 2004
 */

/* Scripts/Mobiles/PlayerMobile.cs
 * ChangeLog:
 *	07/06/09, plasma
 *		Add forwarder to Stealing for reverse pickpocket in CheckNonlocalDrop()
 *	3/22/09, Adam
 *		In RTTResult, Randomize next test to reduce predictability.
 *	1/7/09, Adam
 *		Add a missing Packet Acquire/Release for SendGuildChat
 *	09/25/08, Adam
 *		Dismount players OnLogin
 *	09/24/08, Adam
 *		Add a new system for calculating player movement speed.
 *		We added a new non-serialized date time variable m_LastTimeMark to track the last marker passed.
 *		please the Server.Items.MarkTime object
 *	09/23/08, Adam
 *		You must be AccessLevel > AccessLevel.Player to get mounted speed in ComputeMovementSpeed()
 *	07/30/08, weaver
 *		Fixed spirit cohesion so that it uses LastDeathTime instead of damage entries (which don't
 *		appear to work anymore).
 *	07/27/08, weaver
 *		Correctly remove gumps from the NetState object on CloseGump() (integrated RunUO fix).
 *	07/23/08, weaver
 *		Automated IPooledEnumerable optimizations. 3 loops updated.
 *	7/17/08, Adam
 *		Add new ZCode Mini Game save/restore
 *	4/24/08, Adam
 *		Change public virtual TimeSpan ComputeMovementSpeed( Direction dir ) to public override TimeSpan ComputeMovementSpeed( Direction dir )
 *		As I believe this was the intent (to override the mobile version.)
 *	4/2/08, Pix
 *		Added LeatherEmbroidering bit.
 *	2/26/08, Adam
 *		Remove breedting test logic as it will mess up server wars (which now has TC enabled):
 *			// BREEDING TEST! TC ONLY!
 * 			if (TestCenter.Enabled)
 * 				FollowersMax = 500;
 *	2/24/08, plasma
 *		Added IOBRealAlignment prop
 *  1/22/08, Adam
 *		merge Shopkeeper skill with NpcGuild guild system (MerchantGuild)
 *      - new I/O optimization system
 *      - Add NPCGuild stuffs to I/O optimization system
 *	1/20/08, Adam
 *		Add support for the new Shopkeeper skill
 *  03/01/07, plasma
 *			Remove all duel challenge system related code
 *  12/4/07, Pix
 *      Added LastDeathTime variable/property.
 *      Reworked SpiritCohesive() to use this property instead of the damageentries.
 *  12/3/07, Pix
 *      Added IOBAlignement.Healer for kin-healers instead of overloading Outcast
 *  11/29/07, Pix
 *      Fixed OnKinBeneficial() to use the right variable.
 *	11/21/07, Adam
 *		Change the BaseWeapon damage bonus (based on strength) to take into account the new mobile.STRBonusCap.
 *		This new STRBonusCap allows playerMobiles to have super STR while 'capping' the STR bonus whereby preventing one-hit killing.
 *		The STRBonusCap for PlayerMobiles is defaulted to 140 (100 max STR + legal buffs.)
 *		Note: Only new PlayerMobiles will default to the 140 cap, existing players will be set to 'no cap' or zero (0)
 *	08/26/07, Pix
 *		Changes for RTT.
 *		Changes for Duel Challenge System.
 *	08/01/07, Pix
 *		Added consequences for blue-healers with the Fight Brokers.
 *	7/28/07, Adam
 *		Ghost Blindness:
 *		if the ghost shouldGoBlind BUT the RegionAllowsSight, reschedule blindness
 *	6/16/07, Pix
 *		Make sure the GoBlind() call was the result of the latest death, not a previous death.
 *  5/23/07, Adam
 *      Make sure we don't GoBlind() if the player has resed.
 *  5/21/07, Adam
 *      - Filter boats during ghost blindness
 *      - Allow pets to be seen
 *  5/21/07, Adam
 *      Add Ghost blindness
 *          - After your body decays you will see the message "You feel yourself slipping into the ethereal world."
 *          - once you enter the ethereal world, you will not see other corporeal life except for the NPC healer
 *          - once you enter the ethereal world, you will see no corpses but your own
 *          - sight is restored with resurection
 *	4/03/07, Pix
 *		Tweak to RTT to set the time-to-next-test when the test is taken instead of when the 
 *		response is given.  This fixes an accidental closing/disconnect where they don't 
 *		answer.
 *  03/20/07, plasma
 *      Overrode new BoneDecayTime property to extend delay if a ship captain (corpse has key)
 *	03/27/07, Pix
 *		Implemented RTT for AFK resource gathering thwarting.
 *  03/12/07, plasma,
 *      Changed OnDroppedItemToWorld to prevent dropping stuff next to a TillerMan
 *  2/26/07, Adam
 *      StopMRCapture may be called with CommandEventArgs == null
 *      Add protection.
 *	2/05/07 Taran Kain
 *		Added temporary MovementReqLogger class, added packet throttling flexibility
 *  2/5/07, Adam
 *      Remove ProcessItem override (for guildstones)
 *  1/08/07 Taran Kain
 *      Moved anti-macro code, GSGG logic here
 *      Added in PlayerMobile-specific skillcheck logic
 *  01/07/07, Kit
 *      Re-enabled context menus, after accidental disabling.
 *  01/07/07, Kit
 *      Added netstate check to report, reporting a offline player would crash shard.
 *  12/21/06, Adam
 *      Don't invoke Use() on FakeContainers when a user logs in
 *      We clear the ReadyState in OnLogin()
 *	12/08/06 Taran Kain
 *		Added same TC-only code to default PlayerMobile ctor
 *	12/07/06 Taran Kain
 *		Added TC-only code to reset FollowersMax to 500.
 *	11/25/06, Pix
 *		Added staff-announce for characters of watched accounts logging in.
 *  11/22/06, Rhiannon
 *      Added target's IP to report log header.
 *	11/20/06 Taran Kain
 *		Overrode StrMax, DexMax, IntMax to return 100 - standard human statcaps.
 *		Added PlayerMobile StamRegenRate logic.
 *	11/19/06, Pix
 *		Changes for fixing guild and ally chat colors.
 *	11/19/06, Pix
 *		Removed test code.  Sorry!
 *	11/19/06, Pix
 *		Watchlist enhancements
 *  11/18/06, Adam
 *      Comment out some justice award crap
 *	10/17/06, Adam,
 *		- pixie: Add check for login-on-preview house-exploit
 *		- Add call to Cheater() logging system
 *	9/25/06, Adam
 *		Remove all unused code from context menu:
 *			We don't have insurance, we don't allow house-exit, and we don't have Justice Protectors
 *	9/25/06, Pix
 *		Added ability for players to remove themselves from a house via context menu.
 *  9/02/06, Kit
 *		Added additional checks to IsIsolated due to crash of 9/2, added try/catch. 
 *  8/19/06, Kit
 *		Added Check to CanBeHarmful to prevent harmful actions to players in a
 *		NoExternalHarmful enabled DRDT region.
 *		Added CanSee and IsIsolated routines for hiding items/multis with DRDT Isolation zones.
 *  8/20/06, Rhiannon
 *		Added override for PlaySound to allow for control of music via [FilterMusic.
 *  8/13/06, Rhiannon
 *		Added location to report log header.
 *  8/05/06, Rhiannon
 *		Added PlayList, Playing, and FilterMusic properties.
 *	8/03/06, weaver
 *		Added LastLagTime.
 *		Reformatted comments.
 *	7/24/06, Rhiannon
 *		Changed test for lockdown message to display to Administrators and Owners.
 *	7/23/06, Pix
 *		O/C guilds always display kin type.
 *		Single-clicking self while hidden no longer displays kin type.
 * 	7/18/06, Rhiannon
 *		Added serial numbers to report log header.
 *	7/10/06, Pix
 *		Removed penalty from harming Hires of the same kin.
 *	7/5/06, Pix
 *		Removed non-aligned blue healer turning outcast for healing a kin player - this 
 *		was allowing people to PK kin-aligned people without possibility of getting a murder count.
 *	7/4/06, Pix
 *		Made kin alignment show only when guild titles are displayed.
 *	7/01/06, Pix
 *		Now OnBeneficialAction will turn a non-kin outcast when he heals/etc any PC kin
 *		Overrode OnSingleClick() to show kin alignment.
 *	6/24/06, Pix
 *		Fixed OnBeneficialAction exception.
 *	6/22/06, Pix
 *		Changed outcast flagging for beneficial actions to ignore combat with other players and pets/summons
 *	6/19/06, Pix
 *		Added KinAggression() call when PM does a beneficial action on someone involved in combat with his kin.
 *	6/18/06, Pix
 *		Added KinAggression 'timer' for new OutCast Kin Type.
 *	06/09/06, Pix
 *		Fixed IOBAlignment PROPERTY for recently-unguilded people.
 *	06/06/06, Pix
 *		Changes for Kin System
 *	5/03/06, weaver
 *		Added logging of players logging in and out of game.
 *	5/02/06, weaver
 *		Added IsIsolatedFrom() check to CanBeHarmful() override.
 *	4/30/06, weaver
 *		Added IsIsolatedFrom() to handle custom region based isolation.
 *		Added IsIsolatedFrom() check to CanSee() override.
 *	3/8/06, Pix
 *		Make sure that [report logging files never have illegal characters.
 *	3/1/06, Adam
 *		Remove LastStealTime as we already have LastStoleAt
 *		PS. LastStealTime was not being used.
 *	2/28/06, weaver
 *		Added LastStealTime to store last time player targetted to steal.
 *	2/26/06, Pix
 *		Added call to mobile.RemoveGumps() to remove all ressurectiongumps
 *		when we're alive and we see that there's a ressurectiongump in our gumplist
 *		(in the same code that prevents a dead PM from walking if they have a ressurectiongump)
 *	2/10/06, Adam
 *		Add new override ProcessItem() to process (in a generic way) an item the player is carrying.
 *		This facility us used for placement of a guild stone that is now carried on the player instead
 *		of in deed form (as the FreezeDry system and guild deeds were not compatable.)
 *	01/09/06 Taran Kain
 *		Added Speech recording capabilities.
 *	01/03/06 - Pix
 *		Implemented SendAlliedChat for Allied guild chat.
 *	12/01/05, Pix
 *		Added WatchList PlayerFlag and staff notification when WatchListed player logs in.
 *	11/29/05, weaver
 *		Altered utilisation of SavageKinPaintExpiration time so that only HueMod is adapted for
 *		the effect, not BodyMod.
 *	11/20/05, Pix
 *		Commented out extraneous DecayKills() call on Serialize();
 *	11/19/05, Kit
 *		Removed SavageKinPaintExperation from OnDeath, added check to Resurrect() if savagekinpaint experation not 0
 *		set bodyvalue/hue for savagepaint.
 *	11/07/05, Kit
 *		Moved HasAbilityReady variable and timer for special weapon moves to Mobile.cs for AI use.
 *	11/06/05 Taran Kain
 *		Changed Message() to MortalDeathMessage() for code clarity
 *	10/10/05 TK
 *		Changed some ints to doubles for more of a floating-point math pipeline
 *	10/08/05 Taran Kain
 *		Changed RemoveAllStatMods property to use ClearStatMods() function
 *	9/23/05, Pix
 *		bugfix: now offline decay resets when you get a murdercount.
 *	9/08/05, weaver
 *		Added Embroidering & Etching bitflags.
 *	09/02/05 TK
 *		Added a bit more blood to Mortal deaths
 *	08/28/05 TK
 *		Changed Mortal flag to be part of PlayerFlags, to save some ram
 *		Made sure Mortal players can't be ressurected - 10sec period where they're a ghost
 *		Made sure if AccessLevel > Player, go thru with everything except delete
 *	08/27/05 TK
 *		Added PlayerMobile.Mortal, the Permadeath flag
 *		Added checks in OnDeath to delete player if Mortal=true
 *	8/02/05, Pix
 *		Added check in addition to InitialInnocent check to see whether the basecreature is controled
 *	7/28/05, Pix
 *		Now, if the player has a deathrobe already on resurrect, use that instead of creating a new one.
 *	7/26/05, weaver
 *		Automated removal of AoS resistance related function calls. 32 lines removed.
 *	7/26/05, Adam
 *		Massive AOS cleanout
 *	7/23/05, Adam
 *		Remove all Necromancy, and Chivalry nonsense
 *	7/21/05, weaver
 *		Removed some code referencing resistance variables & redundant resist orientated functions
 *	7/7/05, weaver
 *		Added storage of LastResurrectTime for use with Spirit Cohesion checks.
 *		Made SpiritCohesion &  LastResurrectTime accessible via [props.
 *	7/6/05, weaver
 *		Fixed Spirit Cohesion delays so accessed via CoreAI & fixed
 *		FindDamageEntryFor() call to pass this.LastKiller.
 *	7/5/05, Pix
 *		Added new Guild Chat functionality.
 *	6/16/05, Adam
 *		Removed the line "NOTICE:" from non-activated accounts.
 *		It's not really needed.
 *	6/15/05, Pix
 *		Added pester message on login for Profile activation.
 *	6/13/05, weaver
 *		Added CohesionBaseDelay + removed static initialization (now
 *		controlled through core management console).
 *	6/8/05, weaver
 *		Added SpiritCohesion property and SpiritCohesive() function.
 *	5/30/05, Kit
 *		Added overrided PlaySound() to send sounds players make to monsters withen radius
 *	5/02/05, Kit
 *		Added LastRegion to playermobile for use with DRDT system.
 *	4/30/05, Pix
 *		Made Alchemist reduction linearly dependent on the amount of alchemy you have.
 *		You only get the full reduction if you're at GM alchemy.
 *	04/27/05, weaver
 *		Added read-only Counselor access to DecayTimeShort & DecayTimeLong
 *	4/23/05, Pix
 *		Added CoreAI.ExplosionPotionAlchemyReduction
 *	04/20/05, Pix
 *		Fixed the 'bonus' for having alchemy re: purple pots exploding in your pack.
 *	04/19/05, Pix
 *		Now DecayTimeShort property uses the min of online decay time and offline decay time
 *		Fixed offline decay time messing up online decay time :-O
 *	04/19/05, Pix
 *		Now uses CoreAI.OfflineShortsDecayHours
 *	04/18/05, Pix
 *		Added offline short term murder decay (only if it's turned on).
 *		Added potential exploding of carried explosion potions.
 *	03/09/05, weaver
 *		Added WoodEngraving bitflag.
 *	02/28/05, Adam
 *		remove references to 'PayedInsurance' (no more insurance)
 *		reuse the  flag as 'Fixed' (item.cs)
 *	02/25/05, Adam
 *		remove references to 'Insured' (no more insurance)
 *		reuse the  flag as 'PlayerCrafted' (item.cs)
 *	02/19/05, weaver
 *		Added LastSkillUsed & LastSkillTime for use with new [FindSkill command.
 *	2/16/05, Pixie
 *		Tweaks to make armor work in 1.0.0
 *	02/15/05, Pixie
 *		CHANGED FOR RUNUO 1.0.0 MERGE.
 *	01/10/05, Pix
 *		Replaced NextMurderCountTime with KillerTimes arraylist for controlling repeated counting.
 *	01/10/05, Pix
 *		Added variable to store last lockpick used time.
 *	01/05/05, Pix
 *		Changed IOB requirement from 36 hours to 10 days
 *		Added IOBJoinRestrictionTime.
 *	12/30/04, Pix
 *		Removed AggressiveAction code put in 2 days ago.
 *	12/28/04, Pix
 *		Fixed compiler warning.
 *	12/28/04, Pix
 *		Added AggressiveAction override so we can check for a player attacking another player
 *		who is wearing an IOB of the same type.
 *	12/26/04, Pix
 *		Fix for controlslot change with rank change: followers gets re-calculated on
 *		login.
 *	12/24/04, Pix
 *		Fixed display of IOBRankTime in [props
 *	12/24/04, Adam
 *		Hack Removed.
 *	12/24/04, Adam
 *		Add HACK to insure IOBRankTime gets updated without the player having to do anything.
 *			I added a hack to DoGlobalDecayKills() which is called every 15 minutes for Murder Counts
 *			This code should be removed and replaced with the "right answer"
 *	12/21/04, Pixie
 *		Now resets the iobalignment if we're out of iobtime and not wearing iob
 *	12/20/04, Pixie
 *		Added IOBStartedWearing and IOBRank time to keep track of the IOB Ranks
 *	12/01/04, Pixie
 *		In OnBeforeDeath(), made sure that the player isn't holding anything on their cursor that might
 *		not get dropped.
 *	11/07/04, Pixie
 *		Fixed a problem with short and long timers getting truncated to 4/20 hours instead of being
 *		calculated to what they should be.
 *	11/07/04, Pigpen
 *		Changed it so if a player is wearing an IOB and they die, the timer is set to 36 hours. This is
 *		changed so that players cannot get out of there IOB group by any means other than intended.
 *	11/05/04, Pigpen
 *		Added m_IOBAlignment; Added IOBEquipped; Added IOBTimer. Needed for the new IOBSystem, Enum Values
 *		for IOBAlignment contained in Engines\IOBSystem\IOBAlignEnum.cs
 *	10/16/04, Darva
 *		Added m_LastStoleAt as the timer to control banking after stealing.
 *		This is -not- serialized, as if the theif manages to log, your item
 *		is gone anyway.
 *	9/25/04 - Pix
 *		Added m_LastResynchTime to facilitate 2-minute time period between
 *		uses of the [resynch command.
 *	9/16/04 Pixie
 *		Added static DoGlobalDecayKills() which the Heartbeat system uses.
 *		It Decays kills on every PlayerMobile.
 *	8/30/04 smerX
 *		Reinstated visRemove command..
 *	8/27/04, Adam
 *		Backout smerx's vislist stuff. To recover, revert this file to rev 1.27
 *	8/26/04, Pix
 *		Changed so that reds keep their newbie items.  Also added a switch for this: AllowRedsToKeepNewbieItems
 *	8/7/04, mith
 *		Added temporary NextMurderCountTime variable until I can fix notoriety/flagging.
 *	7/11/04, Pixie
 *		Added 2 properties to give some insight/control over StatMods (from jewelry/clothing)
 *		StatModCount shows the number of mods the player is currently under (3 for bless/curse, 1 for others)
 *		RemoveAllStatMods if set to true will remove all effects the player is under.
 *	7/8/04, Pixie
 *		Fixed murder count decay code in Resurrect (was adding time to reds when it shouldn't have)
 *		Made sure when toggling the Inmate flag that we only add/subtract the time once.
 *	6/16/04, Pixie
 *		Added GSGG factor.
 *	6/5/04, Pix
 *		Merged in 1.0RC0 code.
 *	5/26/04, mith
 *		Modified Resurrect() and OnBeforeDeath() to modify murder count decay times to prevent people from macroing counts on AI as ghosts.
 *	5/13/04, mith
 *		Added RetainPackLocsOnDeath property, overrriding the property from Mobile.
 *		This was returning Core.AOS as the true/false value. Set it to always be true (no more messy packs on resurrection).
 *	5/2/04, Pixie
 *		Cleaned up the way we modify MurderCount timers based on whether the player is an Inmate or not.
 *		Added DecayKills method which we call from various places to reset our count timers.
 *		Added DecayTimeLong and DecayTimeShort properties to debug problems easier. Displays the amount of time until the next countis decayed.
 *		Added code to decay kills when player says "i must consider my sins" in addition to code that already existed in serialize. This way, if time between saves are increased
 *			players can still decay their counts at the appropriate time by simply checking how many counts they have left.
 *	4/29/04, mith
 *		Modified code that sets murder count decay timers to also check if Inmates are Alive or not
 *		If Inmates are sitting around as ghosts, their counts decay as if they were not on Angel Island (8/40).
 *	4/24/04, adam
 *		Commented out "bool gainedPath = false;"
 *	4/24/04, mith
 *		Commented Justice award in OnDeath() since virtues are disabled.
 *	4/10/04 change by Pixie
 *		Added ReduceKillTimersByHours(double hours) for the ParoleOfficer
 *	4/10/04 change by mith
 *		Fixed a typo in Serialize with m_LongTermElapse.
 *	4/9/04 changes by mith
 *		Added code to reset count decay time based on Inmate flag for Serialize() event.
 *	4/1/04, changes by mith
 *		Testing values for ShortTerm and LongTerm count decay. Must test next time I have 4 hours of free time.
 *	3/29/04, changes by mith
 *		Added PlayerFlag.Inmate, to be modified on entrance/exit of Angel Island.
 */

#pragma warning disable 429, 162

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Server;
using Server.Misc;
using Server.Items;
using Server.Gumps;
using Server.Multis;
using Server.Engines.Help;
using Server.ContextMenus;
using Server.Network;
using Server.Spells;
using Server.Spells.Fifth;
using Server.Spells.Seventh;
using Server.Targeting;
using Server.Engines.Quests;
using Server.Accounting;
using Server.Scripts.Commands;
using Server.Regions;

namespace Server.Mobiles
{
	[Flags]
	public enum PlayerFlag // First 16 bits are reserved for default-distro use, start custom flags at 0x00010000
	{
		None				= 0x00000000,
		Glassblowing		= 0x00000001,
		Masonry				= 0x00000002,
		SandMining			= 0x00000004,
		StoneMining			= 0x00000008,
		ToggleMiningStone	= 0x00000010,
		KarmaLocked			= 0x00000020,
		AutoRenewInsurance	= 0x00000040,
		UseOwnFilter		= 0x00000080,
		PublicMyRunUO		= 0x00000100,
		PagingSquelched		= 0x00000200,
		Inmate				= 0x00010000,	//inmate at Angel Island
		IOBEquipped			= 0x00020000,	//Pigpen - Addition for IOB Sytem
		WoodEngraving		= 0x00040000,	//weaver - added to allow perma prop.
		Mortal				= 0x00080000,	//TK - Permadeath
		Embroidering		= 0x00100000,	//weaver - added to allow perma prop.
		Etching				= 0x00200000,	//weaver - added to allow perma prop.
		Watched             = 0x00400000,	//Pix: added for staff watch list,
		LeatherEmbroidering = 0x00800000,   //Pix - leather embroidery
	}

	public enum NpcGuild
	{
		None,
		MagesGuild,
		WarriorsGuild,
		ThievesGuild,
		RangersGuild,
		HealersGuild,
		MinersGuild,
		MerchantsGuild,
		TinkersGuild,
		TailorsGuild,
		FishermensGuild,
		BardsGuild,
		BlacksmithsGuild
	}

	public class PlayerMobile : Mobile
    {
        #region Save Flags (optimize read/write data)
        [Flags]
        private enum SaveFlag
        {
            None            = 0x00000000,
            NPCGuild        = 0x00000001,   // save npc guild releated data if set
			ZCodeMiniGame	= 0x00000002,   // Does this player have a saved ZCode Mini Game?
        }
        private void SetSaveFlag(ref SaveFlag flags, SaveFlag toSet, bool setIf)
        {
            if (setIf)
                flags |= toSet;
        }

        private bool GetSaveFlag(SaveFlag flags, SaveFlag toGet)
        {
            return ((flags & toGet) != 0);
        }

        #endregion
        #region Ghost Blindness
        private DateTime m_SightExpire = DateTime.MaxValue;
        private bool m_Blind=false;
        [CommandProperty(AccessLevel.GameMaster)]
        public bool Blind
        {
            get
            {
                return m_Blind;
            }
            set
            {
                if (m_Blind != value)
                {
                    m_Blind = value;

                    try
                    {
                        if (Map != null)
                        {
                            Packet p = null;

                            IPooledEnumerable eable = Map.GetObjectsInRange(Location);

                            foreach (object ob in eable)
                            {
                                if (ob == null)
                                    continue;

                                // if we cannot see those (because we are blind), remove them from view
                                if (!this.CanSee(ob))
                                {
                                    if (p == null)
                                        if (ob is Item)
                                            p = (ob as Item).RemovePacket;
                                        else if (ob is Mobile)
                                            p = (ob as Mobile).RemovePacket;

                                    this.Send(p);
                                    p = null;
                                }
                                else
                                {
                                    if (ob is Mobile)
                                    {
                                        this.Send(new MobileIncoming(this, (ob as Mobile)));

                                        if (ObjectPropertyList.Enabled)
                                            this.Send(OPLPacket);
                                    }

                                    if (ob is Item && this.NetState != null)
                                        (ob as Item).SendInfoTo(this.NetState);
                                }
                            }

                            eable.Free();
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.LogException(ex);
                    }
                }
            }
        }

		private bool RegionAllowsSight()
		{
			CustomRegion cr = CustomRegion.FindDRDTRegion(this);
			if (cr != null)
			{
				RegionControl rc = cr.GetRegionControler();
				if (rc != null && rc.GhostBlindness == false)
					return true;
			}
			return false;
		}

        // called on a timer
        private void GoBlind()
        {
			// process rules
			// make sure the player is dead and the timer has not been reset
			bool result = true;
			result = result && this.Alive == false;					// we're not alive now
			result = result && m_SightExpire != DateTime.MaxValue;	// the timer hasn't been reset
			result = result && DateTime.Now >= m_SightExpire;		// this call is a result of the last sightexpire time
			bool shouldGoBlind = result;							// record if we shouldGoBlind
			bool GhostSight = RegionAllowsSight();					// does this region allow sight for ghosts
			result = result && GhostSight == false;					// rule result

			// okay, looks like we should make the player blind         
            if (result == true) 
            {
                Blind = true;                       // go blind
                m_SightExpire = DateTime.MaxValue;  // kill timer
                SendMessage("You feel yourself slipping into the ethereal world.");
            }
			// if the ghost shouldGoBlind BUT the RegionAllowsSight, reschedule blindness
			else if (shouldGoBlind == true && GhostSight == true)
				Timer.DelayCall(TimeSpan.FromMinutes(1), new TimerCallback(GoBlind));
        }
        #endregion
        #region Shopkeeper points system (Shorthand for NpcGuildPoints)
        // [view shopkeeper points through the NPCGuildPoints]
		public double ShopkeeperPoints
		{
			get { return m_NpcGuildPoints; }
            set { m_NpcGuildPoints = value; }
		}
        // [view this property through the NpcGuild property]
        public bool Shopkeeper
        {
            get { return NpcGuild == NpcGuild.MerchantsGuild; }
        }
		#endregion

		private const int STRBonusCapDefault = 140; // 100 STR + legal buffs
		public DateTime m_LastResynchTime;

		private class CountAndTimeStamp
		{
			private int m_Count;
			private DateTime m_Stamp;

			public CountAndTimeStamp()
			{
			}

			public DateTime TimeStamp { get{ return m_Stamp; } }
			public int Count
			{
				get { return m_Count; }
				set	{ m_Count = value; m_Stamp = DateTime.Now; }
			}
		}

        public bool IsStaff
        {
            get { return this.AccessLevel > AccessLevel.Player; } 
        }
        
        private bool AllowRedsToKeepNewbieItems
        {
            get { return true; }
        }
    
        private Queue m_PlayList = null;
		public Queue PlayList
		{
			get { return m_PlayList; }
			set { m_PlayList = value; }
		}

		private bool m_FilterMusic = false;

		public bool FilterMusic
		{
			get { return m_FilterMusic; }
			set { m_FilterMusic = value; }
		}

		private bool m_Playing = false;

		public bool Playing
		{
			get { return m_Playing; }
			set { m_Playing = value; }
		}

		private struct SpeechRecordEntry
		{
			public DateTime Time;
			public string Speech;

			public SpeechRecordEntry(string text)
			{
				Speech = text;
				Time = DateTime.Now;
			}
		}
		
		private Queue m_SpeechRecord;

		public Queue SpeechRecord
		{
			get
			{
				return m_SpeechRecord;
			}
		}

		private DateTime m_Reported;
		private LogHelper m_ReportLogger;
		private Timer m_ReportLogStopper;
		private TimeSpan ReportTime { get { return TimeSpan.FromMinutes(5); } }

		private DesignContext m_DesignContext;

		private Region LastRegion = null;

		private DateTime m_LastGuildChange;
		private IOBAlignment m_LastGuildIOBAlignment;

		public override void OnGuildChange( Server.Guilds.BaseGuild oldGuild )
		{
			InvalidateMyRunUO();

			m_LastGuildChange = DateTime.Now;
			Guilds.Guild og = oldGuild as Guilds.Guild;
			if( og != null )
			{
				m_LastGuildIOBAlignment = og.IOBAlignment;
			}
			else
			{
				m_LastGuildIOBAlignment = IOBAlignment.None;
			}

			base.OnGuildChange( oldGuild );
		}

		#region IOB stuff

		private double m_IOBKillPoints;
        private double m_KinSoloPoints;
        private double m_KinTeamPoints;

        [CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
        public double KinPowerPoints
        {
            get { return m_IOBKillPoints; }
            set { m_IOBKillPoints = value; }
        }
        [CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
        public double KinSoloPoints
        {
            get { return m_KinSoloPoints; }
            set { m_KinSoloPoints = value; }
        }
        [CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
        public double KinTeamPoints
        {
            get { return m_KinTeamPoints; }
            set { m_KinTeamPoints = value; }
        }

		public double AwardKinPowerPoints(double points)
		{
            double awarded = 0;
            if (Engines.IOBSystem.KinSystemSettings.PointsEnabled)
            {
                if ((KinPowerPoints + points) >= 100.0)
                {
                    awarded = 100.0 - KinPowerPoints;
                    KinPowerPoints = 100.0;
                }
                else
                {
                    awarded = points;
                    KinPowerPoints += points;
                }
            }
			return awarded;
		}

		[CommandProperty(AccessLevel.Counselor)]
		public bool IsInStatloss
		{
			get
			{
                if (Engines.IOBSystem.KinSystemSettings.StatLossEnabled)
                {
                    if (this.StatModCount > 0)
                    {
                        foreach (object o in this.StatMods)
                        {
                            if (o is Engines.IOBSystem.KinStatlossSkillMod
                                || o is Engines.IOBSystem.KinHealerStatlossSkillMod)
                            {
                                return true;
                            }
                        }
                    }
                }
				return false;
			}
		}

        public void RemoveStatlossSkillMods()
        {
            this.RemoveSkillModsOfType(typeof(Engines.IOBSystem.KinStatlossSkillMod));
            this.RemoveSkillModsOfType(typeof(Engines.IOBSystem.KinHealerStatlossSkillMod));
        }

		//private IOBAlignment m_IOBAlignment;
		private bool m_IOBEquipped;

		private DateTime m_KinAggressionTime = DateTime.MinValue;
		private DateTime m_KinBeneficialTime = DateTime.MinValue;

		[CommandProperty( AccessLevel.Counselor, AccessLevel.Administrator )]
		public DateTime KinAggressionTime
		{
			get{ return m_KinAggressionTime; }
			set{ m_KinAggressionTime = value; }
		}

		[CommandProperty( AccessLevel.Counselor, AccessLevel.Administrator )]
		public DateTime KinBeneficialTime
		{
			get { return m_KinBeneficialTime; }
			set { m_KinBeneficialTime = value; }
		}

		public void OnKinAggression()
		{
			KinAggressionTime = DateTime.Now + TimeSpan.FromMinutes(Engines.IOBSystem.KinSystemSettings.KinAggressionMinutes);
		}

        public void OnKinBeneficial()
        {
            if (Engines.IOBSystem.KinSystemSettings.KinNameHueEnabled)
            {
                if (KinBeneficialTime < DateTime.Now)
                {
                    //If we're currently NOT outcast due to healing:
                    this.SendMessage("You have done a beneficial action on a kin, you are now participating in the kin system.");
                    this.SendMessage("You are freely attackable by everyone in the kin system.");
                    if (Engines.IOBSystem.KinSystemSettings.StatLossEnabled)
                    {
                        this.SendMessage("If you die to other kin system participants, you will suffer stat loss.");
                        this.SendMessage("This will be in effect for {0:0.00} minutes from the last beneficial action to kin that you perform.", Engines.IOBSystem.KinSystemSettings.KinBeneficialMinutes);
                    }
                }

                KinBeneficialTime = DateTime.Now + TimeSpan.FromMinutes(Engines.IOBSystem.KinSystemSettings.KinBeneficialMinutes);
            }
        }

		[CommandProperty( AccessLevel.GameMaster )]
		public IOBAlignment IOBAlignment
		{
			get
			{
				if( KinAggressionTime > DateTime.Now )
				{
					return IOBAlignment.OutCast;
				}
				if (KinBeneficialTime > DateTime.Now)
				{
					return IOBAlignment.Healer;
				}

				return IOBRealAlignment;
				
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public IOBAlignment IOBRealAlignment
		{
			get
			{
				if (this.Guild != null)
				{
					Guilds.Guild g = this.Guild as Guilds.Guild;
					if (g != null)
					{
						return g.IOBAlignment;
					}
				}
				if (this.m_LastGuildIOBAlignment != IOBAlignment.None)
				{
					if (this.m_LastGuildChange + TimeSpan.FromDays(7.0) > DateTime.Now)
					{
						return this.m_LastGuildIOBAlignment;
					}
				}
				return IOBAlignment.None;
			}
		}

		public bool IsRealFactioner
		{
				get
				{
						if (IOBAlignment == IOBAlignment.None)
						{
								return false;
						}

						if (IOBAlignment == IOBAlignment.OutCast || IOBAlignment == IOBAlignment.Healer)
						{
							if (this.Guild != null)
							{
								Guilds.Guild g = this.Guild as Guilds.Guild;
								if (g != null && g.IOBAlignment == IOBAlignment.None)
								{
									return false;
								}
							}
						else
						{
							if (this.m_LastGuildIOBAlignment != IOBAlignment.None
								&& this.m_LastGuildChange.AddDays(7.0) > DateTime.Now)
							{
								return true;
							}
							else
							{
								//if no guild and outcast, then we're not really aligned.
								return false;
							}
						}
					}

					return true;
			}
		}

		public bool OnEquippedIOBItem( IOBAlignment iobalignment )
		{
			if ( this.IOBEquipped == false )
			{
				this.IOBEquipped = true; 
			}

			return IOBEquipped;
		}

		private TimeSpan m_IOBRankTime;
		private DateTime m_IOBStartedWearing;

		private DateTime m_IOBJoinRestrictionTime;

		[CommandProperty( AccessLevel.GameMaster )]
		public DateTime IOBJoinRestrictionTime
		{
			get{ return m_IOBJoinRestrictionTime; }
			set{ m_IOBJoinRestrictionTime = value; }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public TimeSpan IOBRankTime
		{
			get
			{
				if( IOBEquipped && m_IOBStartedWearing > DateTime.MinValue )
				{
					return (m_IOBRankTime + (DateTime.Now - m_IOBStartedWearing));
				}
				return m_IOBRankTime;
			}
			set{ m_IOBRankTime = value; }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public IOBRank IOBRank
		{
			get
			{
				if( IOBAlignment != IOBAlignment.None )
				{
					TimeSpan totalRankTime = m_IOBRankTime; //this is to keep track of "running rank time" - basically ranktime + time logged-in and wearing
					if( this.m_IOBStartedWearing > DateTime.MinValue )
					{
						totalRankTime += (DateTime.Now - m_IOBStartedWearing);
					}

					if( totalRankTime > TimeSpan.FromHours(72.0) )
					{
						return IOBRank.SecondTier;
					}
					else if( totalRankTime > TimeSpan.FromHours(36.0) )
					{
						return IOBRank.FirstTier;
					}
					else
					{
						return IOBRank.None;
					}
				}
				else
				{
					return IOBRank.None;
				}
			}
		}

		public void ResetIOBRankTime()
		{
			m_IOBRankTime = TimeSpan.FromHours(0.0);
			if( IOBEquipped )
				m_IOBStartedWearing = DateTime.Now;
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public bool IOBEquipped
		{
			get{ return m_IOBEquipped; }
			set
			{
				if( value == false && m_IOBEquipped == true ) //if we're going from true->false
				{
					if( m_IOBStartedWearing > DateTime.MinValue ) //make sure it's a valid value
					{
						m_IOBRankTime += ( DateTime.Now - m_IOBStartedWearing );
						m_IOBStartedWearing = DateTime.MinValue;
					}
				}
				else if( value == true && m_IOBEquipped == false ) //if we're going from false->true
				{
					m_IOBStartedWearing = DateTime.Now;
				}

				m_IOBEquipped = value;
			}
		}

		#endregion

		private string m_WatchReason = "";
		[CommandProperty(AccessLevel.Counselor)]
		public string WatchReason
		{
			get { return m_WatchReason; }
			set { m_WatchReason = value; }
		}
        
		private DateTime m_WatchExpire;
		[CommandProperty(AccessLevel.Counselor)]
		public DateTime WatchExpire
		{
			get { return m_WatchExpire; }
			set { m_WatchExpire = value; }
		}

		private PlayerFlag m_Flags;
		private int m_StepsTaken;
		private int m_Profession;

		private DateTime m_LastStoleAt;

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Mortal
		{
			get { return GetFlag(PlayerFlag.Mortal); }
			set { SetFlag(PlayerFlag.Mortal, value); }
		}

		public Region LastRegionIn
		{
			get {return LastRegion; }
			set {LastRegion = value;}
		}

		private SkillName m_LastSkillUsed;	// wea: For recording last skill
		public SkillName LastSkillUsed 
		{ 									// used & time for [FindSkill
			get 
			{								// ||---
				return m_LastSkillUsed;
			}
			set 
			{
				m_LastSkillUsed = value;
			}
		}

		private DateTime m_LastSkillTime;
		public DateTime LastSkillTime 
		{
			get 
			{
				return m_LastSkillTime;
			}
			set 
			{
				m_LastSkillTime = value;
			}
		}									// ------||

		// wea: Keeps check of last resurrect date/time
		private DateTime m_LastResurrectTime;

		[CommandProperty( AccessLevel.GameMaster )]
		public DateTime LastResurrectTime 
		{
			get 
			{
				return m_LastResurrectTime;
			}
			set 
			{
				m_LastResurrectTime = value;
			}
		}
        
		// wea: Added to control SpiritCohesion and how it affects
		// resurrection
		private int m_SpiritCohesion;

		[CommandProperty( AccessLevel.GameMaster )]
		public int SpiritCohesion 
		{
			get 
			{
				return m_SpiritCohesion;
			}
			set 
			{
				m_SpiritCohesion = value;
			}
		}

		// Pix - modified to use LastDeathTime property of PlayerMobile
		// Note - a return of true means they can ressurect
		//        a return of false means they can't resurrect
		public bool SpiritCohesive()
		{
			try
			{
				// Decrement SpiritCohesion according to LastResurrectTime
				if (SpiritCohesion > 0)
				{
					SpiritCohesion -= (int)((DateTime.Now - LastResurrectTime).TotalSeconds / CoreAI.CohesionLowerDelay);
					if (SpiritCohesion < 0)
					{
						SpiritCohesion = 0;
					}
				}

				TimeSpan TimeSinceDeath = (DateTime.Now - LastDeathTime);
				TimeSpan CohesionTime = TimeSpan.FromSeconds(CoreAI.CohesionBaseDelay + (SpiritCohesion * CoreAI.CohesionFactor));
				
				if (TimeSinceDeath < CohesionTime)
				{
					return false;
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("Logging exception");
				LogHelper.LogException(e);
			}

			return true;
		}

		// wea: Keeps track of lag report times
		private DateTime m_LastLagTime;

		[CommandProperty( AccessLevel.GameMaster )]
		public DateTime LastLagTime 
		{
			get 
			{
				return m_LastLagTime;
			}
			set 
			{
				m_LastLagTime = value;
			}
		}
        
		
		// Temporary variable until we get flagging system working properly.
		//private DateTime m_NextMurderCountTime;

		private DateTime m_LastUsedLockpick; //Pix: for usage issue with lockpicks
		public DateTime LastUsedLockpick { get{ return m_LastUsedLockpick; } set{ m_LastUsedLockpick = value; } }

		private DateTime[] m_LastSkillGainTime;
		public DateTime[] LastSkillGainTime
		{
			get{ return m_LastSkillGainTime; }
			set{ m_LastSkillGainTime = value; }
		}

		public DateTime LastStoleAt
		{
			get{ return m_LastStoleAt;}
			set{ m_LastStoleAt = value;}
		}


		[CommandProperty( AccessLevel.GameMaster )]
		public int Profession
		{
			get{ return m_Profession; }
			set{ m_Profession = value; }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int StatModCount
		{
			get{ return StatMods.Count; }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public bool RemoveAllStatMods
		{
			get{ return false; }
			set
			{
				if( value )
				{
					ClearStatMods();
				}
			}
		}

		public int StepsTaken
		{
			get{ return m_StepsTaken; }
			set{ m_StepsTaken = value; }
		}

		#region ZCodeMiniGames
		private int m_ZCodeMiniGameID;
		public int ZCodeMiniGameID { get { return m_ZCodeMiniGameID; } set { m_ZCodeMiniGameID = value; } }
		private byte[] m_ZCodeMiniGameData;
		public byte[] ZCodeMiniGameData { get { return m_ZCodeMiniGameData; } set { m_ZCodeMiniGameData = value; } }
		#endregion

		#region NpcGuild
		private NpcGuild m_NpcGuild;
        private DateTime m_NpcGuildJoinTime;
        private TimeSpan m_NpcGuildGameTime;
        private double m_NpcGuildPoints;
        
        [CommandProperty(AccessLevel.GameMaster, AccessLevel.Administrator)]
        public double NpcGuildPoints
        {
            get { return m_NpcGuildPoints; }
            set { m_NpcGuildPoints = value; }
        }

		[CommandProperty( AccessLevel.GameMaster )]
		public NpcGuild NpcGuild
		{
			get{ return m_NpcGuild; }
			set{ m_NpcGuild = value; }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public DateTime NpcGuildJoinTime
		{
			get{ return m_NpcGuildJoinTime; }
			set{ m_NpcGuildJoinTime = value; }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public TimeSpan NpcGuildGameTime
		{
			get{ return m_NpcGuildGameTime; }
			set{ m_NpcGuildGameTime = value; }
        }
        public void OnNpcGuildJoin(NpcGuild newGuild)
        {
            // set membership
            m_NpcGuildPoints = 0;
            NpcGuild = newGuild;
            NpcGuildJoinTime = DateTime.Now;
            NpcGuildGameTime = GameTime;
        }
        // called by the NPC guild master when a player resigns
        public void OnNpcGuildResign()
        {
            // clear membership
            m_NpcGuildPoints = 0;
            NpcGuild = NpcGuild.None;
            NpcGuildJoinTime = DateTime.MinValue;
            NpcGuildGameTime = TimeSpan.Zero;
        }
        #endregion NpcGuild

        public PlayerFlag Flags
		{
			get{ return m_Flags; }
			set{ m_Flags = value; }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public bool PagingSquelched
		{
			get{ return GetFlag( PlayerFlag.PagingSquelched ); }
			set{ SetFlag( PlayerFlag.PagingSquelched, value ); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public bool Glassblowing
		{
			get{ return GetFlag( PlayerFlag.Glassblowing ); }
			set{ SetFlag( PlayerFlag.Glassblowing, value ); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public bool Masonry
		{
			get{ return GetFlag( PlayerFlag.Masonry ); }
			set{ SetFlag( PlayerFlag.Masonry, value ); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public bool SandMining
		{
			get{ return GetFlag( PlayerFlag.SandMining ); }
			set{ SetFlag( PlayerFlag.SandMining, value ); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public bool StoneMining
		{
			get{ return GetFlag( PlayerFlag.StoneMining ); }
			set{ SetFlag( PlayerFlag.StoneMining, value ); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public bool WoodEngraving
		{
			get{ return GetFlag( PlayerFlag.WoodEngraving ); }
			set{ SetFlag( PlayerFlag.WoodEngraving, value ); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public bool Embroidering
		{
			get{ return GetFlag( PlayerFlag.Embroidering ); }
			set{ SetFlag( PlayerFlag.Embroidering, value ); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool LeatherEmbroidering
		{
			get { return GetFlag(PlayerFlag.LeatherEmbroidering); }
			set { SetFlag(PlayerFlag.LeatherEmbroidering, value); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public bool Etching
		{
			get{ return GetFlag( PlayerFlag.Etching ); }
			set{ SetFlag( PlayerFlag.Etching, value ); }
		}

		[CommandProperty( AccessLevel.Counselor )]
		public bool WatchList
		{
			get{ return GetFlag( PlayerFlag.Watched ); }
			set{ SetFlag( PlayerFlag.Watched, value ); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public bool ToggleMiningStone
		{
			get{ return GetFlag( PlayerFlag.ToggleMiningStone ); }
			set{ SetFlag( PlayerFlag.ToggleMiningStone, value ); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public bool KarmaLocked
		{
			get{ return GetFlag( PlayerFlag.KarmaLocked ); }
			set{ SetFlag( PlayerFlag.KarmaLocked, value ); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public bool AutoRenewInsurance
		{
			get{ return GetFlag( PlayerFlag.AutoRenewInsurance ); }
			set{ SetFlag( PlayerFlag.AutoRenewInsurance, value ); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public bool UseOwnFilter
		{
			get{ return GetFlag( PlayerFlag.UseOwnFilter ); }
			set{ SetFlag( PlayerFlag.UseOwnFilter, value ); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public bool PublicMyRunUO
		{
			get{ return GetFlag( PlayerFlag.PublicMyRunUO ); }
			set{ SetFlag( PlayerFlag.PublicMyRunUO, value ); InvalidateMyRunUO(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public bool Inmate
		{
			get{ return GetFlag( PlayerFlag.Inmate ); }
			set
			{
				bool bWasInmate = GetFlag( PlayerFlag.Inmate );
				SetFlag( PlayerFlag.Inmate, value );
				if ( value && !bWasInmate )
				{
					//Going from non-inmate to inmate, make sure our counts are reduced
					//reduce shorttermelapse to a max of 4 hours from now
					if( (m_ShortTermElapse - GameTime) > TimeSpan.FromHours(4) )
						if ( this.Alive )
							m_ShortTermElapse = GameTime + TimeSpan.FromHours(4);
						else
							m_ShortTermElapse = GameTime + TimeSpan.FromHours(8);
					//reduce longtermelapse to a max of 20 hours from now
					if( (m_LongTermElapse - GameTime) > TimeSpan.FromHours(20) )
						if ( this.Alive )
							m_LongTermElapse = GameTime + TimeSpan.FromHours(20);
						else
							m_LongTermElapse = GameTime + TimeSpan.FromHours(40);
				}
				else if( !value && bWasInmate )
				{
					//going from inmate to non-inmate,
					//add back on the difference in long and short-term
					//count times for non-inmates.
					m_ShortTermElapse += TimeSpan.FromHours(4);
					m_LongTermElapse += TimeSpan.FromHours(20);
				}
				else
				{
					//setting flag to the same thing, don't touch anything
				}

				InvalidateMyRunUO();
			}
		}

		[CommandProperty( AccessLevel.Counselor, AccessLevel.GameMaster )]
		public TimeSpan DecayTimeShort
		{
			get
			{
				if( CoreAI.OfflineShortsDecay != 0 )
				{
					//if we're using offline shortterm decay, return the min of online and offline decay
					TimeSpan onlineDecay = m_ShortTermElapse - GameTime;
					TimeSpan offlineDecay = (this.m_LastShortDecayed.AddHours( CoreAI.OfflineShortsDecayHours ) - DateTime.Now);

					if( onlineDecay < offlineDecay )
					{
						return onlineDecay;
					}
					else
					{
						return offlineDecay;
					}
				}
				else
				{
					//not using offline decay ... just return online decay time
					return m_ShortTermElapse - GameTime;
				}
			}
		}
	    
		[CommandProperty( AccessLevel.Counselor, AccessLevel.GameMaster )]
		public TimeSpan DecayTimeLong
		{
			get{ return m_LongTermElapse - GameTime; }
		}

		

		//public DateTime NextMurderCountTime
		//{
		//	get{ return m_NextMurderCountTime; }
		//	set{ m_NextMurderCountTime = value; }
		//}

		public ArrayList KillerTimes;

		public static Direction GetDirection4( Point3D from, Point3D to )
		{
			int dx = from.X - to.X;
			int dy = from.Y - to.Y;

			int rx = dx - dy;
			int ry = dx + dy;

			Direction ret;

			if ( rx >= 0 && ry >= 0 )
				ret = Direction.West;
			else if ( rx >= 0 && ry < 0 )
				ret = Direction.South;
			else if ( rx < 0 && ry < 0 )
				ret = Direction.East;
			else
				ret = Direction.North;

			return ret;
		}

		public override bool OnDroppedItemToWorld( Item item, Point3D location )
		{
			if ( !base.OnDroppedItemToWorld( item, location ) )
				return false;

            //plasma, 03/12/07
            //Check here to see if we are trying to drop on an adjacent location
            //to a TillerMan, and if so prevent the drop.
            else if (!BaseBoat.DropFitResult(location, Map, Z))
                return false;

			BounceInfo bi = item.GetBounce();

			if ( bi != null )
			{
				Type type = item.GetType();

                if (type.IsDefined(typeof(FurnitureAttribute), true) || type.IsDefined(typeof(DynamicFlipingAttribute), true))
                {
                    object[] objs = type.GetCustomAttributes(typeof(FlipableAttribute), true);

                    if (objs != null && objs.Length > 0)
                    {
                        FlipableAttribute fp = objs[0] as FlipableAttribute;

                        if (fp != null)
                        {
                            int[] itemIDs = fp.ItemIDs;

                            Point3D oldWorldLoc = bi.m_WorldLoc;
                            Point3D newWorldLoc = location;

                            if (oldWorldLoc.X != newWorldLoc.X || oldWorldLoc.Y != newWorldLoc.Y)
                            {
                                Direction dir = GetDirection4(oldWorldLoc, newWorldLoc);

                                if (itemIDs.Length == 2)
                                {
                                    switch (dir)
                                    {
                                        case Direction.North:
                                        case Direction.South: item.ItemID = itemIDs[0]; break;
                                        case Direction.East:
                                        case Direction.West: item.ItemID = itemIDs[1]; break;
                                    }
                                }
                                else if (itemIDs.Length == 4)
                                {
                                    switch (dir)
                                    {
                                        case Direction.South: item.ItemID = itemIDs[0]; break;
                                        case Direction.East: item.ItemID = itemIDs[1]; break;
                                        case Direction.North: item.ItemID = itemIDs[2]; break;
                                        case Direction.West: item.ItemID = itemIDs[3]; break;
                                    }
                                }
                            }
                        }
                    }
                }
                
			}

			return true;
		}

		public bool GetFlag( PlayerFlag flag )
		{
			return ( (m_Flags & flag) != 0 );
		}

		public void SetFlag( PlayerFlag flag, bool value )
		{
			if ( value )
				m_Flags |= flag;
			else
				m_Flags &= ~flag;
		}

		public DesignContext DesignContext
		{
			get{ return m_DesignContext; }
			set{ m_DesignContext = value; }
		}

		public static void Initialize()
		{
			if ( FastwalkPrevention )
			{
				PacketHandler ph = PacketHandlers.GetHandler( 0x02 );

				ph.ThrottleCallback = new ThrottlePacketCallback( MovementThrottle_Callback );
			}

			EventSink.Login += new LoginEventHandler( OnLogin );
			EventSink.Logout += new LogoutEventHandler( OnLogout );
			EventSink.Connected += new ConnectedEventHandler( EventSink_Connected );
			EventSink.Disconnected += new DisconnectedEventHandler( EventSink_Disconnected );

            Commands.Register("gsgg", AccessLevel.Administrator, new CommandEventHandler(GSGG_OnCommand));
        }

        private int m_LastGlobalLight = -1, m_LastPersonalLight = -1;

		public override void OnNetStateChanged()
		{
			m_LastGlobalLight = -1;
			m_LastPersonalLight = -1;
		}

		public override void ComputeBaseLightLevels( out int global, out int personal )
		{
			global = LightCycle.ComputeLevelFor( this );
			personal = this.LightLevel;
		}

		public override void CheckLightLevels( bool forceResend )
		{
			NetState ns = this.NetState;

			if ( ns == null )
				return;

			int global, personal;

			ComputeLightLevels( out global, out personal );

			if ( !forceResend )
				forceResend = ( global != m_LastGlobalLight || personal != m_LastPersonalLight );

			if ( !forceResend )
				return;

			m_LastGlobalLight = global;
			m_LastPersonalLight = personal;

			ns.Send( GlobalLightLevel.Instantiate( global ) );
			ns.Send( new PersonalLightLevel( this, personal ) );
		}


		private static void OnLogin( LoginEventArgs e )
		{
			Mobile from = e.Mobile;

			// wea: log the fact that they've logged in
			Server.Scripts.Commands.CommandLogging.LogChangeClient( e.Mobile, true );

			//			SacrificeVirtue.CheckAtrophy( from );
			//			JusticeVirtue.CheckAtrophy( from );
			//			CompassionVirtue.CheckAtrophy( from );

			if( from is PlayerMobile )
			{
				PlayerMobile pm = (PlayerMobile)from;

				// unmount player
				try
				{
					IMount mount = pm.Mount;
					if (mount != null)
						mount.Rider = null;
				}
				catch (Exception exc)
				{
					LogHelper.LogException(exc);
					System.Console.WriteLine("Caught non-fatal exception in PlayerMobile.OnLogin: " + exc.Message);
					System.Console.WriteLine(exc.StackTrace);
				}

				pm.m_LastResynchTime = DateTime.Now;
				pm.m_InmateLastDeathTime = DateTime.Now; //have to set this to now, otherwise it'd be exploitable.

				//recalculate follower control slots
				try
				{
					int slots = 0;
					Mobile master = from;

					foreach ( Mobile m in World.Mobiles.Values )
					{
						if ( m is BaseCreature )
						{
							BaseCreature bc = (BaseCreature)m;

							if ( (bc.Controlled && bc.ControlMaster == master) || (bc.Summoned && bc.SummonMaster == master) )
							{
								slots += bc.ControlSlots;
							}
						}
					}
					pm.Followers = slots;
				}
				catch(Exception exc)
				{
					LogHelper.LogException(exc);
					System.Console.WriteLine("Caught non-fatal exception in PlayerMobile.OnLogin: " + exc.Message);
					System.Console.WriteLine(exc.StackTrace);
				}

				//Tell staff that a watchlist player has logged in
				if( pm.WatchList )
				{
					if (pm.WatchExpire > DateTime.Now)
					{
						Server.Scripts.Commands.CommandHandlers.BroadcastMessage(AccessLevel.Counselor,
							0x482,
							String.Format("WatchListed player {0} has logged in.", pm.Name));
					}
					else
					{
						//clean up watching
						pm.WatchList = false;
					}
				}
				Account acct = pm.Account as Account;
				if (acct != null && acct.Watched)
				{
					if (acct.WatchExpire > DateTime.Now)
					{
						Server.Scripts.Commands.CommandHandlers.BroadcastMessage(AccessLevel.Counselor,
							0x482,
							String.Format("WatchListed account {0} (char: {1}) has logged in.", acct.Username, pm.Name));
					}
					else
					{
						//clean up watching
						acct.Watched = false;
					}
				}


				try
				{
					Sector s = pm.Map.GetSector(pm);
					foreach(BaseMulti mul in s.Multis.Values)
					{
						if (mul == null)
							continue;

						if( mul is PreviewHouse )
						{
							if( mul.Contains( pm ) )
							{
								LogHelper.Cheater(pm,"Trying to use the 'preview house' exploit",true);
								Server.Point3D jail = new Point3D( 5295, 1174, 0 );
								pm.MoveToWorld( jail, Map.Felucca );
								break;
							}
						}
					}
				}
				catch(Exception exc)
                {
                    LogHelper.LogException(exc);
                }

				if (pm.Guild != null)
				{
					try
					{
						foreach (TownshipStone ts in TownshipStone.AllTownshipStones)
						{
							if (ts.Guild == pm.Guild)
							{
								if (ts.RLDaysLeftInFund < 7.0)
								{
									string tsMessage = string.Format(
										"Your guild's township has {0:0.00} days left in its fund before the township is demolished."
										, ts.RLDaysLeftInFund	
										);
									//pm.SendMessage("Your guild's township has {0:0.00} days left in its fund before the township is demolished.", ts.RLDaysLeftInFund);
									from.SendGump(new NoticeGump(1060637, 30720, tsMessage, 0xFFC000, 300, 140, null, null));
								}
							}
						}
					}
					catch (Exception tse)
					{
						LogHelper.LogException(tse, "Pixie: township 7- day warning");
					}
				}

			}

            // adam: Don't invoke Use() on this FakeContainer when a user logsin
            try
            {
                Container pack = e.Mobile.Backpack;
                if (pack != null)
                {
                    Item[] items = pack.FindItemsByType(typeof(FakeContainer), true);
                    for (int ix = 0; ix < items.Length; ix++)
                        (items[ix] as FakeContainer).ClearReadyState();
                }
            }
            catch (Exception exc)
            {
                LogHelper.LogException(exc);
            }

			if ( AccountHandler.LockdownLevel > AccessLevel.Player )
			{
				string notice;

				Accounting.Account acct = from.Account as Accounting.Account;

				if ( acct == null || !acct.HasAccess( from.NetState ) )
				{
					if ( from.AccessLevel == AccessLevel.Player )
						notice = "The server is currently under lockdown. No players are allowed to log in at this time.";
					else
						notice = "The server is currently under lockdown. You do not have sufficient access level to connect.";

					Timer.DelayCall( TimeSpan.FromSeconds( 1.0 ), new TimerStateCallback( Disconnect ), from );
				}
				else if ( from.AccessLevel >= AccessLevel.Administrator )
				{
					notice = "The server is currently under lockdown. As you are an administrator (or owner), you may change this from the [Admin gump.";
				}
				else
				{
					notice = "The server is currently under lockdown. You have sufficient access level to connect.";
				}

				from.SendGump( new NoticeGump( 1060637, 30720, notice, 0xFFC000, 300, 140, null, null ) );
			}

			Accounting.Account account = from.Account as Accounting.Account;
			if( account != null )
			{
				if( account.AccountActivated == false )
				{
					from.SendMessage( 0x35, "Your account is not yet activated." );
					from.SendMessage( 0x35, "Password recovery is not possible without account activation." );
					from.SendMessage( 0x35, "Please type [profile to activate your account." );
				}
			}
		}

		private bool m_NoDeltaRecursion;

		public void ValidateEquipment()
		{
			if ( m_NoDeltaRecursion || Map == null || Map == Map.Internal )
				return;

			if ( this.Items == null )
				return;

			m_NoDeltaRecursion = true;
			Timer.DelayCall( TimeSpan.Zero, new TimerCallback( ValidateEquipment_Sandbox ) );
		}

		private void ValidateEquipment_Sandbox()
		{
			try
			{
				if ( Map == null || Map == Map.Internal )
					return;

				ArrayList items = this.Items;

				if ( items == null )
					return;

				bool moved = false;

				int str = this.Str;
				int dex = this.Dex;
				int intel = this.Int;

				Mobile from = this;

				for ( int i = items.Count - 1; i >= 0; --i )
				{
					if ( i >= items.Count )
						continue;

					Item item = (Item)items[i];

					if ( item is BaseWeapon )
					{
						BaseWeapon weapon = (BaseWeapon)item;

						bool drop = false;

						if ( dex < weapon.DexRequirement )
							drop = true;
						else if ( str < AOS.Scale( weapon.StrRequirement, 100 - weapon.GetLowerStatReq() ) )
							drop = true;
						else if ( intel < weapon.IntRequirement )
							drop = true;

						if ( drop )
						{
							string name = weapon.Name;

							if ( name == null )
								name = String.Format( "#{0}", weapon.LabelNumber );

							from.SendLocalizedMessage( 1062001, name ); // You can no longer wield your ~1_WEAPON~
							from.AddToBackpack( weapon );
							moved = true;
						}
					}
					else if ( item is BaseArmor )
					{
						BaseArmor armor = (BaseArmor)item;

						bool drop = false;

						if ( !armor.AllowMaleWearer && from.Body.IsMale && from.AccessLevel < AccessLevel.GameMaster )
						{
							drop = true;
						}
						else if ( !armor.AllowFemaleWearer && from.Body.IsFemale && from.AccessLevel < AccessLevel.GameMaster )
						{
							drop = true;
						}
						else
						{
							double strBonus = armor.ComputeStatBonus( StatType.Str, this );
							double dexBonus = armor.ComputeStatBonus( StatType.Dex, this );
							double intBonus = armor.ComputeStatBonus( StatType.Int, this );

							int strReq = armor.ComputeStatReq( StatType.Str );
							int dexReq = armor.ComputeStatReq( StatType.Dex );
							int intReq = armor.ComputeStatReq( StatType.Int );

							if ( dex < dexReq || (dex + dexBonus) < 1 )
								drop = true;
							else if ( str < strReq || (str + strBonus) < 1 )
								drop = true;
							else if ( intel < intReq || (intel + intBonus) < 1 )
								drop = true;
						}

						if ( drop )
						{
							string name = armor.Name;

							if ( name == null )
								name = String.Format( "#{0}", armor.LabelNumber );

							if ( armor is BaseShield )
								from.SendLocalizedMessage( 1062003, name ); // You can no longer equip your ~1_SHIELD~
							else
								from.SendLocalizedMessage( 1062002, name ); // You can no longer wear your ~1_ARMOR~

							from.AddToBackpack( armor );
							moved = true;
						}
					}
				}

				if ( moved )
					from.SendLocalizedMessage( 500647 ); // Some equipment has been moved to your backpack.
			}
			catch ( Exception e )
			{
				LogHelper.LogException(e);
				Console.WriteLine( e );
			}
			finally
			{
				m_NoDeltaRecursion = false;
			}
		}

		public override void Delta( MobileDelta flag )
		{
			base.Delta( flag );

			if ( (flag & MobileDelta.Stat) != 0 )
				ValidateEquipment();

			if ( (flag & (MobileDelta.Name | MobileDelta.Hue)) != 0 )
				InvalidateMyRunUO();
		}

		private static void Disconnect( object state )
		{
			NetState ns = ((Mobile)state).NetState;
			
			if ( ns != null )
				ns.Dispose();
		}

		private static void OnLogout( LogoutEventArgs e )
		{
		}

		private static void EventSink_Connected( ConnectedEventArgs e )
		{
			PlayerMobile pm = e.Mobile as PlayerMobile;

			if ( pm != null )
			{
				pm.m_SessionStart = DateTime.Now;

				if ( pm.m_Quest != null )
					pm.m_Quest.StartTimer();

				if( pm.IOBEquipped )
				{
					pm.m_IOBStartedWearing = DateTime.Now;
				}
			}
		}

		private static void EventSink_Disconnected( DisconnectedEventArgs e )
		{
			Mobile from = e.Mobile;
			DesignContext context = DesignContext.Find( from );

			if ( context != null )
			{
				/* Client disconnected
				 *  - Remove design context
				 *  - Eject client from house
				 */

				// Remove design context
				DesignContext.Remove( from );

				// Eject client from house
				from.RevealingAction();

				from.MoveToWorld( context.Foundation.BanLocation, context.Foundation.Map );
			}

			PlayerMobile pm = e.Mobile as PlayerMobile;

			if ( pm != null )
			{
				pm.m_GameTime += (DateTime.Now - pm.m_SessionStart);

				if ( pm.m_Quest != null )
					pm.m_Quest.StopTimer();


				if( pm.IOBEquipped )
				{
					if( pm.m_IOBStartedWearing > DateTime.MinValue )
					{
						pm.m_IOBRankTime += (DateTime.Now - pm.m_IOBStartedWearing);
					}
				}
				pm.m_IOBStartedWearing = DateTime.MinValue; //always set this to minvalue when logged out
			}

			// wea: log the fact that they've disconnected
			Server.Scripts.Commands.CommandLogging.LogChangeClient( e.Mobile, false );

		}

		public override void RevealingAction()
		{
			if ( m_DesignContext != null )
				return;

			Spells.Sixth.InvisibilitySpell.RemoveTimer( this );

			base.RevealingAction();
		}

		public override void OnSubItemAdded( Item item )
		{
			if ( AccessLevel < AccessLevel.GameMaster && item.IsChildOf( this.Backpack ) )
			{
				int maxWeight = WeightOverloading.GetMaxWeight( this );
				int curWeight = Mobile.BodyWeight + this.TotalWeight;

				if ( curWeight > maxWeight )
					this.SendLocalizedMessage( 1019035, true, String.Format( " : {0} / {1}", curWeight, maxWeight ) );
			}
		}

		public override bool CanBeHarmful( Mobile target, bool message, bool ignoreOurBlessedness )
		{
			// wea: added call to IsIsolatedFrom to prevent harmful actions in their entirety if mobile is isolated from *this*
			if ( m_DesignContext != null || ((target is PlayerMobile) && ( ((PlayerMobile)target).m_DesignContext != null || ((PlayerMobile)target).IsIsolatedFrom(this) )) )
				return false;

			try
			{
				RegionControl regstone = null;
				CustomRegion reg = null;
				if(target !=null)
					reg = CustomRegion.FindDRDTRegion(target);
				if(reg !=null )
					regstone = reg.GetRegionControler();

				//if your in a region area spells will fail if disallowed, prevents the run outside of area precast
				//run back into region then release spell ability
				if(this != null && target != null && this.Region != target.Region && regstone != null && regstone.NoExternalHarmful 
					&& this.AccessLevel == AccessLevel.Player)
				{
					this.SendMessage( "You cannot harm them in that area." );
					return false;
				}

			}
			catch(NullReferenceException e)
			{
				LogHelper.LogException(e);
				Console.WriteLine("{0} Caught exception.", e);
			}
			catch (Exception ex) { EventSink.InvokeLogException(new LogExceptionEventArgs(ex)); }

			if ( (target is BaseVendor && ((BaseVendor)target).IsInvulnerable) || target is PlayerVendor || target is TownCrier )
			{
				if ( message )
				{
					if ( target.Title == null )
						SendMessage( "{0} the vendor cannot be harmed.", target.Name );
					else
						SendMessage( "{0} {1} cannot be harmed.", target.Name, target.Title );
				}

				return false;
			}

			return base.CanBeHarmful( target, message, ignoreOurBlessedness );
		}

		public override void PlaySound( int soundID )
		{
			PlaySound( soundID, false );
		}

		// Overrided PlaySound to control playing of music
		public void PlaySound( int soundID, bool IsNote )
		{
			if ( soundID == -1 )
				return;

			if ( Map != null )
			{
				Packet p = null;

				IPooledEnumerable eable = Map.GetClientsInRange( Location );

				foreach ( NetState state in eable )
				{
					if ( state.Mobile.CanSee( this ) )
					{
						// If the mobile is a player who has toggled FilterMusic on, don't play.
						if ( IsNote && state.Mobile is PlayerMobile
							&& ( (PlayerMobile)state.Mobile).FilterMusic )
							continue;
					
						if (p == null)
							p = Packet.Acquire(new PlaySound(soundID, this));

						state.Send( p );
					}
				}

				Packet.Release(p);

				eable.Free();
			}
		}

		public override bool CanBeBeneficial( Mobile target, bool message, bool allowDead )
		{
			if ( m_DesignContext != null || (target is PlayerMobile && ((PlayerMobile)target).m_DesignContext != null) )
				return false;

			return base.CanBeBeneficial( target, message, allowDead );
		}

        public override bool CheckContextMenuDisplay(IEntity target)
        {
            return (m_DesignContext == null);
        }

		public override void OnItemAdded( Item item )
		{
			base.OnItemAdded( item );

			if ( item is BaseArmor || item is BaseWeapon )
			{
				Hits=Hits; Stam=Stam; Mana=Mana;
			}

			if ( item is BaseWeapon )
				this.HasAbilityReady = false;

			InvalidateMyRunUO();
		}

		public override void OnItemRemoved( Item item )
		{
			base.OnItemRemoved( item );

			if ( item is BaseArmor || item is BaseWeapon )
			{
				Hits=Hits; Stam=Stam; Mana=Mana;
			}

			if ( item is BaseWeapon )
				this.HasAbilityReady = false;

			InvalidateMyRunUO();
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public override double ArmorRating
		{
			get
			{
				BaseArmor ar;
				double rating = 0.0;

				ar = NeckArmor as BaseArmor;
				if ( ar != null )
					rating += ar.ArmorRatingScaled;

				ar = HandArmor as BaseArmor;
				if ( ar != null )
					rating += ar.ArmorRatingScaled;

				ar = HeadArmor as BaseArmor;
				if ( ar != null )
					rating += ar.ArmorRatingScaled;

				ar = ArmsArmor as BaseArmor;
				if ( ar != null )
					rating += ar.ArmorRatingScaled;

				ar = LegsArmor as BaseArmor;
				if ( ar != null )
					rating += ar.ArmorRatingScaled;

				ar = ChestArmor as BaseArmor;
				if ( ar != null )
					rating += ar.ArmorRatingScaled;

				ar = ShieldArmor as BaseArmor;
				if ( ar != null )
					rating += ar.ArmorRatingScaled;

				return VirtualArmor + VirtualArmorMod + rating;
			}
		}

		public override int StrMax
		{
			get
			{
				return 100;
			}
			set
			{
			}
		}

		public override int IntMax
		{
			get
			{
				return 100;
			}
			set
			{
			}
		}

		public override int DexMax
		{
			get
			{
				return 100;
			}
			set
			{
			}
		}

		public override int HitsMax
		{
			get
			{
				int strBase;
				double strOffs = GetStatOffset( StatType.Str );

				if ( Core.AOS )
				{
					strBase = this.Str;
					strOffs += AosAttributes.GetValue( this, AosAttribute.BonusHits );
				}
				else
				{
					strBase = this.RawStr;
				}

				return (strBase / 2) + 50 + (int)strOffs;
			}
		}

		public override int StamMax
		{
			get{ return base.StamMax + AosAttributes.GetValue( this, AosAttribute.BonusStam ); }
		}

		public override TimeSpan StamRegenRate
		{
			get
			{
				double maxFoodBonus = 3.5; //Seconds maximum quicker to gain stamina

				TimeSpan foodbonus = TimeSpan.FromSeconds(maxFoodBonus * Hunger / 20);

				if( foodbonus > TimeSpan.FromSeconds(maxFoodBonus) )
				{
					foodbonus = TimeSpan.FromSeconds(maxFoodBonus);
				}

				return base.StamRegenRate - foodbonus;
			}
		}

		public override int ManaMax
		{
			get{ return base.ManaMax + AosAttributes.GetValue( this, AosAttribute.BonusMana ); }
		}

		public override bool Move( Direction d )
		{
			NetState ns = this.NetState;

			if ( ns != null )
			{
				//GumpCollection gumps = ns.Gumps;
                List<Gump> gumps = new List<Gump>(ns.Gumps);

				for ( int i = 0; i < gumps.Count; ++i )
				{
					if ( gumps[i] is ResurrectGump )
					{
						if ( Alive )
						{
							CloseGumps( typeof( ResurrectGump ) );
						}
						else
						{
							SendLocalizedMessage( 500111 ); // You are frozen and cannot move.
							return false;
						}
					}
				}
			}

			TimeSpan speed = ComputeMovementSpeed( d );

			if ( !base.Move( d ) )
				return false;

			m_NextMovementTime += speed;
			return true;
		}

		public override bool CheckMovement( Direction d, out int newZ )
		{
			DesignContext context = m_DesignContext;

			if ( context == null )
				return base.CheckMovement( d, out newZ );

			HouseFoundation foundation = context.Foundation;

			newZ = foundation.Z + HouseFoundation.GetLevelZ( context.Level );

			int newX = this.X, newY = this.Y;
			Movement.Movement.Offset( d, ref newX, ref newY );

			int startX = foundation.X + foundation.Components.Min.X + 1;
			int startY = foundation.Y + foundation.Components.Min.Y + 1;
			int endX = startX + foundation.Components.Width - 1;
			int endY = startY + foundation.Components.Height - 2;

			return ( newX >= startX && newY >= startY && newX < endX && newY < endY && Map == foundation.Map );
		}
			 
		public override bool AllowItemUse( Item item )
		{
			return DesignContext.Check( this );
		}

		public override bool AllowSkillUse( SkillName skill )
		{
			return DesignContext.Check( this );
		}

		public override bool CheckNonlocalDrop(Mobile from, Item item, Item target)
		{
			bool baseResult = base.CheckNonlocalDrop(from, item, target);
			if (!baseResult)
			{
				//Reverse pickpocket code
				//Only check this if the base fails, this makes the amount of times it is called greatly reduced
				if (SkillHandlers.Stealing.CheckReversePickpocket(from, item, target)) return true;
			}
			return baseResult;
		}

		private bool m_LastProtectedMessage;
		private int m_NextProtectionCheck = 10;

		public virtual void RecheckTownProtection()
		{
			m_NextProtectionCheck = 10;

			Regions.GuardedRegion reg = this.Region as Regions.GuardedRegion;
			bool isProtected = ( reg != null && reg.IsGuarded );

			if ( isProtected != m_LastProtectedMessage )
			{
				if ( isProtected )
					SendLocalizedMessage( 500112 ); // You are now under the protection of the town guards.
				else
					SendLocalizedMessage( 500113 ); // You have left the protection of the town guards.

				m_LastProtectedMessage = isProtected;
			}
		}

		public override void MoveToWorld( Point3D loc, Map map )
		{
			base.MoveToWorld( loc, map );

			RecheckTownProtection();
		}

		public override void SetLocation( Point3D loc, bool isTeleport )
		{
			base.SetLocation( loc, isTeleport );

			if ( isTeleport || --m_NextProtectionCheck == 0 )
				RecheckTownProtection();
		}

		public override void GetContextMenuEntries( Mobile from, ArrayList list )
		{
			base.GetContextMenuEntries( from, list );
/*
 * Adam: Remove all this unused code.
 * We don't have insurance, we don't allow house-exit, and we don't have Justice Protectors
 * 
			if ( from == this )
			{
				if ( m_Quest != null )
					m_Quest.GetContextMenuEntries( list );

				if ( Alive && InsuranceEnabled )
				{
					list.Add( new CallbackEntry( 6201, new ContextCallback( ToggleItemInsurance ) ) );

					if ( AutoRenewInsurance )
						list.Add( new CallbackEntry( 6202, new ContextCallback( CancelRenewInventoryInsurance ) ) );
					else
						list.Add( new CallbackEntry( 6200, new ContextCallback( AutoRenewInventoryInsurance ) ) );
				}

				// TODO: Toggle champ titles

				BaseHouse house = BaseHouse.FindHouseAt( this );
				if( house == null ) //Pix: additional check for house
				{
					Region reg = this.Region;
					if( reg != null && reg is HouseRegion )
					{
						house = ((HouseRegion)reg).House;
					}
				}

				if ( house != null ) //&& house.IsAosRules )
					list.Add( new CallbackEntry( 6207, new ContextCallback( LeaveHouse ) ) );


				if ( m_JusticeProtectors.Count > 0 )
					list.Add( new CallbackEntry( 6157, new ContextCallback( CancelProtection ) ) );
			}
*/			
		}

		private void CancelProtection()
		{
			for ( int i = 0; i < m_JusticeProtectors.Count; ++i )
			{
				Mobile prot = (Mobile)m_JusticeProtectors[i];

				string args = String.Format( "{0}\t{1}", this.Name, prot.Name );

				prot.SendLocalizedMessage( 1049371, args ); // The protective relationship between ~1_PLAYER1~ and ~2_PLAYER2~ has been ended.
				this.SendLocalizedMessage( 1049371, args ); // The protective relationship between ~1_PLAYER1~ and ~2_PLAYER2~ has been ended.
			}

			m_JusticeProtectors.Clear();
		}

		private void ToggleItemInsurance()
		{
			if ( !CheckAlive() )
				return;

			BeginTarget( -1, false, TargetFlags.None, new TargetCallback( ToggleItemInsurance_Callback ) );
			SendLocalizedMessage( 1060868 ); // Target the item you wish to toggle insurance status on <ESC> to cancel
		}

		private bool CanInsure( Item item )
		{
			if ( item is Container )
				return false;

			if ( item is Spellbook || item is Runebook || item is PotionKeg )
				return false;

			if ( item.Stackable )
				return false;

			if ( item.LootType == LootType.Cursed )
				return false;

			if ( item.ItemID == 0x204E ) // death shroud
				return false;

			return true;
		}

		private void ToggleItemInsurance_Callback( Mobile from, object obj )
		{
			if ( !CheckAlive() )
				return;

			Item item = obj as Item;

			if ( item == null || !item.IsChildOf( this ) )
			{
				BeginTarget( -1, false, TargetFlags.None, new TargetCallback( ToggleItemInsurance_Callback ) );
				SendLocalizedMessage( 1060871, "", 0x23 ); // You can only insure items that you have equipped or that are in your backpack
			}
				// Adam: no more insurance
				/*else if ( item.Insured )
				{
					item.Insured = false;

					SendLocalizedMessage( 1060874, "", 0x35 ); // You cancel the insurance on the item

					BeginTarget( -1, false, TargetFlags.None, new TargetCallback( ToggleItemInsurance_Callback ) );
					SendLocalizedMessage( 1060868, "", 0x23 ); // Target the item you wish to toggle insurance status on <ESC> to cancel
				}*/
			else if ( !CanInsure( item ) )
			{
				BeginTarget( -1, false, TargetFlags.None, new TargetCallback( ToggleItemInsurance_Callback ) );
				SendLocalizedMessage( 1060869, "", 0x23 ); // You cannot insure that
			}
			else if ( item.LootType == LootType.Blessed || item.LootType == LootType.Newbied || item.BlessedFor == from )
			{
				BeginTarget( -1, false, TargetFlags.None, new TargetCallback( ToggleItemInsurance_Callback ) );
				SendLocalizedMessage( 1060870, "", 0x23 ); // That item is blessed and does not need to be insured
				SendLocalizedMessage( 1060869, "", 0x23 ); // You cannot insure that
			}
			else
			{
				// Adam: no more insurance
				/*if ( !item.PayedInsurance )
				{
					if ( Banker.Withdraw( from, 600 ) )
					{
						SendLocalizedMessage( 1060398, "600" ); // ~1_AMOUNT~ gold has been withdrawn from your bank box.
						item.PayedInsurance = true;
					}
					else
					{
						SendLocalizedMessage( 1061079, "", 0x23 ); // You lack the funds to purchase the insurance
						return;
					}
				}*/

				// Adam: no more insurance
				//item.Insured = true;

				SendLocalizedMessage( 1060873, "", 0x23 ); // You have insured the item

				BeginTarget( -1, false, TargetFlags.None, new TargetCallback( ToggleItemInsurance_Callback ) );
				SendLocalizedMessage( 1060868, "", 0x23 ); // Target the item you wish to toggle insurance status on <ESC> to cancel
			}
		}

		private void AutoRenewInventoryInsurance()
		{
			if ( !CheckAlive() )
				return;

			SendLocalizedMessage( 1060881, "", 0x23 ); // You have selected to automatically reinsure all insured items upon death
			AutoRenewInsurance = true;
		}

		private void CancelRenewInventoryInsurance()
		{
			if ( !CheckAlive() )
				return;

			SendLocalizedMessage( 1061075, "", 0x23 ); // You have cancelled automatically reinsuring all insured items upon death
			AutoRenewInsurance = false;
		}

		// TODO: Champ titles, toggle

		private void LeaveHouse()
		{
			BaseHouse house = BaseHouse.FindHouseAt( this );

			if( house == null ) //Pix: additional check for house
			{
				Region reg = this.Region;
				if( reg != null && reg is HouseRegion )
				{
					house = ((HouseRegion)reg).House;
				}
			}

			if ( house != null )
				this.Location = house.BanLocation;
		}

		private delegate void ContextCallback();

		private class CallbackEntry : ContextMenuEntry
		{
			private ContextCallback m_Callback;

			public CallbackEntry( int number, ContextCallback callback ) : this( number, -1, callback )
			{
			}

			public CallbackEntry( int number, int range, ContextCallback callback ) : base( number, range )
			{
				m_Callback = callback;
			}

			public override void OnClick()
			{
				if ( m_Callback != null )
					m_Callback();
			}
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( this == from && !Warmode )
			{
				IMount mount = Mount;

				if ( mount != null && !DesignContext.Check( this ) )
					return;
			}

			base.OnDoubleClick( from );
		}

		public override void DisplayPaperdollTo( Mobile to )
		{
			if ( DesignContext.Check( this ) )
				base.DisplayPaperdollTo( to );
		}

		private static bool m_NoRecursion;

		protected override void OnLocationChange( Point3D oldLocation )
		{
			CheckLightLevels( false );
			LastRegionIn = this.Region;
			DesignContext context = m_DesignContext;

			if ( context == null || m_NoRecursion )
				return;

			m_NoRecursion = true;

			HouseFoundation foundation = context.Foundation;

			int newX = this.X, newY = this.Y;
			int newZ = foundation.Z + HouseFoundation.GetLevelZ( context.Level );

			int startX = foundation.X + foundation.Components.Min.X + 1;
			int startY = foundation.Y + foundation.Components.Min.Y + 1;
			int endX = startX + foundation.Components.Width - 1;
			int endY = startY + foundation.Components.Height - 2;

			if ( newX >= startX && newY >= startY && newX < endX && newY < endY && Map == foundation.Map )
			{
				if ( Z != newZ )
					Location = new Point3D( X, Y, newZ );

				m_NoRecursion = false;
				return;
			}

			Location = new Point3D( foundation.X, foundation.Y, newZ );
			Map = foundation.Map;

			m_NoRecursion = false;
		}

		protected override void OnMapChange( Map oldMap )
		{
			DesignContext context = m_DesignContext;

			if ( context == null || m_NoRecursion )
				return;

			m_NoRecursion = true;

			HouseFoundation foundation = context.Foundation;

			if ( Map != foundation.Map )
				Map = foundation.Map;

			m_NoRecursion = false;
		}

		public override void OnDamage( int amount, Mobile from, bool willKill )
		{
			if ( amount > (Core.AOS ? 25 : 0) )
			{
				BandageContext c = BandageContext.GetContext( this );

				if ( c != null )
					c.Slip();
			}

			WeightOverloading.FatigueOnDamage( this, amount );

			base.OnDamage( amount, from, willKill );
		}

		public static int ComputeSkillTotal( Mobile m )
		{
			int total = 0;

			for ( int i = 0; i < m.Skills.Length; ++i )
				total += m.Skills[i].BaseFixedPoint;

			return ( total / 10 );
		}

		public override void Resurrect()
		{
			bool wasAlive = this.Alive;

			if (Mortal && AccessLevel == AccessLevel.Player)
			{
				SendMessage("Thy soul was too closely intertwined with thy flesh - thou'rt unable to incorporate a new body.");
				return;
			}

			base.Resurrect();

			// Savage kin paint re-application

			if(this.SavagePaintExpiration != TimeSpan.Zero)
			{
				// this.BodyMod = ( this.Female ? 184 : 183 );
				this.HueMod = 0;
			}

			if ( this.Alive && !wasAlive )
			{
                // restore sight to blinded ghosts
                Blind = false;                          // we can see again
                m_SightExpire = DateTime.MaxValue;      // kill timer

				bool bNewDeathrobe = true;
				if( this.Backpack != null )
				{
					Item oldDeathrobe = this.Backpack.FindItemByType( typeof( DeathRobe ), false );
					if( oldDeathrobe != null )
					{
						bNewDeathrobe = false;
						EquipItem( oldDeathrobe );
					}
				}
				if( bNewDeathrobe )
				{
					Item deathRobe = new DeathRobe();

					if ( !EquipItem( deathRobe ) )
						deathRobe.Delete();
				}

				if ( Inmate )
				{
					//When resrrecting, make sure our counts are reduced

					TimeSpan deadtime = TimeSpan.FromMinutes(0.0);
					if( m_InmateLastDeathTime == DateTime.MinValue )
					{
						//effectively 0 deadtime if it's set to minvalue
					}
					else
					{
						deadtime = DateTime.Now - m_InmateLastDeathTime;
					}

					//reduce short term by 4 hours minus half the time spent dead (modulo 8 hours)
					m_ShortTermElapse -= (TimeSpan.FromHours(4.0) - TimeSpan.FromSeconds( (deadtime.TotalSeconds%28800)/2 ) );
					//reduce long term by 20 hours minus half the time spent dead (modulo 40 hours)
					m_LongTermElapse -= (TimeSpan.FromHours(20.0) - TimeSpan.FromSeconds( (deadtime.TotalSeconds%144000)/2 ) );
				}

				InvalidateMyRunUO();
			}
		}

		// wea: Added to perform SpiritCohesion update on resurrect
		public override void OnAfterResurrect()
		{
			// Set last res time so we know how long they've had alive
			LastResurrectTime = DateTime.Now;
						
			if (LastDeathTime == null)
			{
				SpiritCohesion = 0;
				return;
			}
			
			TimeSpan TimeSinceDeath = (DateTime.Now - LastDeathTime);

			if (TimeSinceDeath < TimeSpan.FromSeconds(CoreAI.CohesionLowerDelay))
			{
				SpiritCohesion++;
			}
			else
			{
				SpiritCohesion = 0;
			}

			return;
		}

#if THIS_IS_NOT_USED
		private Mobile m_InsuranceAward;
		private int m_InsuranceCost;
		private int m_InsuranceBonus;
#endif

		private DateTime m_InmateLastDeathTime;

		public override bool OnBeforeDeath()
		{
#if THIS_IS_NOT_USED
			m_InsuranceCost = 0;
			m_InsuranceAward = base.FindMostRecentDamager( false );

			if ( m_InsuranceAward != null && !m_InsuranceAward.Player )
				m_InsuranceAward = null;

			if ( m_InsuranceAward is PlayerMobile )
				((PlayerMobile)m_InsuranceAward).m_InsuranceBonus = 0;
#endif
			if ( Inmate )
			{
				m_InmateLastDeathTime = DateTime.Now;

				// If they die as an Inmate, reset their kill timers to 8/40
				m_ShortTermElapse += TimeSpan.FromHours(4);
				m_LongTermElapse += TimeSpan.FromHours(20);

				InvalidateMyRunUO();
			}

			//make sure that the player isn't holding anything...
			try
			{
				Item held = Holding;
				if( held != null )
				{
					held.ClearBounce();
					if( Backpack != null )
					{
						Backpack.DropItem( held );
					}
				}
				Holding = null;
			}
			catch (Exception ex) { EventSink.InvokeLogException(new LogExceptionEventArgs(ex)); }

			return base.OnBeforeDeath();
		}

		private bool IsSameRealIOB(Mobile target)
		{
			bool bReturn = false;

			if( target != null )
			{
				IOBAlignment ta = IOBAlignment.None;
				if( target is BaseCreature )
				{
					BaseCreature bc = target as BaseCreature;
					ta = bc.IOBAlignment;
				}
				else if( target is PlayerMobile )
				{
					PlayerMobile pma = target as PlayerMobile;
					ta = pma.IOBAlignment;
				}

				IOBAlignment myRealAlignment = this.IOBAlignment;
				if( this.IOBAlignment == IOBAlignment.OutCast || this.IOBAlignment == IOBAlignment.Healer)
				{
					Guilds.Guild g = this.Guild as Guilds.Guild;
					if( g != null )
					{
						myRealAlignment = g.IOBAlignment;
					}
				}

				if( ta == myRealAlignment )
				{
					bReturn = true;
				}
			}

			return bReturn;
		}

		public override void OnBeneficialAction(Mobile target, bool isCriminal)
		{
			try
			{
				if( this.IOBAlignment != IOBAlignment.None && this != target )
				{
					bool bFound = false; //saves processing of Aggressors if we find one in Aggressed

					//Check those the target has aggressed
					for(int i=0; i<target.Aggressed.Count; i++)
					{
						Mobile a = ((AggressorInfo)target.Aggressed[i]).Defender;
						if( a is PlayerMobile )
						{
							//ignore actions between players
						}
						else if( a is BaseCreature && ( ((BaseCreature)a).Summoned || ((BaseCreature)a).Tamable ) )
						{
							//ignore summons and tames
						}
						else if( a is BaseHire )
						{
							//ignore hires
						}
						else if( IsSameRealIOB(a) )
						{
							this.OnKinAggression();
							bFound = true;
							break;
						}
					}

					if( !bFound )
					{
						//Check those that have aggressed the target
						for(int i=0; i<target.Aggressors.Count; i++)
						{
							Mobile a = ((AggressorInfo)target.Aggressors[i]).Attacker;
							if( a is PlayerMobile )
							{
								//ignore actions between players
							}
							else if( a is BaseCreature && ( ((BaseCreature)a).Summoned || ((BaseCreature)a).Tamable ) )
							{
								//ignore summons and tames
							}
							else if( a is BaseHire )
							{
								//ignore hires
							}
							else if( IsSameRealIOB(a) )
							{
								this.OnKinAggression();
								bFound = true;
								break;
							}
						}
					}
				}
				//Pix 7/5/06 - removed due to problems
				//else if( this.IOBAlignment == IOBAlignment.None )
				//{
				//	if( target is PlayerMobile )
				//	{
				//		PlayerMobile pmt = target as PlayerMobile;
				//		if( pmt.IOBAlignment != IOBAlignment.None )
				//		{
				//			this.OnKinAggression();
				//		}
				//	}
				//}
			}
			catch(Exception e)
			{
				LogHelper.LogException(e);
				Console.WriteLine("Problem with PM.OnBeneficialAction - Tell PIXIE:");
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
			}

			//Deal with faction-healers
			try
			{
				if (this != target)
				{
					if (this.IsRealFactioner == false) //if we're NOT a real factioner
					{
						if (target is PlayerMobile)
						{
							PlayerMobile pmTarget = target as PlayerMobile;
							if (pmTarget.IOBAlignment != IOBAlignment.None)
							{
								this.OnKinBeneficial();
							}
						}
					}
				}
			}
			catch (Exception fhe)
			{
				LogHelper.LogException(fhe);
			}

			//Deal with fightbroker interferers.
			try
			{
				if (this != target)
				{
					if (!FightBroker.IsAlreadyRegistered(this)
						&&
						(FightBroker.IsAlreadyRegistered(target) || FightBroker.IsHealerInterferer(target)))
					{
						FightBroker.AddHealerInterferer(this);
					}
				}
			}
			catch (Exception e)
			{
				LogHelper.LogException(e);
			}

			base.OnBeneficialAction (target, isCriminal);
		}


		private bool CheckInsuranceOnDeath( Item item )
		{
			// Adam: no more insurance
			/*
			if ( InsuranceEnabled && item.Insured )
			{
				if ( AutoRenewInsurance )
				{
					int cost = ( m_InsuranceAward == null ? 600 : 300 );

					if ( Banker.Withdraw( this, cost ) )
					{
						m_InsuranceCost += cost;
						item.PayedInsurance = true;
					}
					else
					{
						SendLocalizedMessage( 1061079, "", 0x23 ); // You lack the funds to purchase the insurance
						item.PayedInsurance = false;
						item.Insured = false;
					}
				}
				else
				{
					item.PayedInsurance = false;
					item.Insured = false;
				}

				if ( m_InsuranceAward != null )
				{
					if ( Banker.Deposit( m_InsuranceAward, 300 ) )
					{
						if ( m_InsuranceAward is PlayerMobile )
							((PlayerMobile)m_InsuranceAward).m_InsuranceBonus += 300;
					}
				}

				return true;
			}

			*/
			return false;
		}

		public override DeathMoveResult GetParentMoveResultFor( Item item )
		{
			/*
			if (this.IsInChallenge)
				return DeathMoveResult.RemainEquiped;
			*/
			if ( CheckInsuranceOnDeath( item ) )
				return DeathMoveResult.MoveToBackpack;

			if( AllowRedsToKeepNewbieItems && item.LootType == LootType.Newbied )
				return DeathMoveResult.MoveToBackpack;

			return base.GetParentMoveResultFor( item );
		}

		public override DeathMoveResult GetInventoryMoveResultFor( Item item )
		{
			/*
			if (this.IsInChallenge)
				return DeathMoveResult.RemainEquiped;
			*/
			if (CheckInsuranceOnDeath(item))
				return DeathMoveResult.MoveToBackpack;

			if( AllowRedsToKeepNewbieItems && item.LootType == LootType.Newbied )
				return DeathMoveResult.MoveToBackpack;

			return base.GetInventoryMoveResultFor( item );
		}

		public override void OnDeath( Container c )
		{
			base.OnDeath( c );

            //Deal with any death-effects for factioners
            Engines.IOBSystem.KinSystem.OnDeath(this);

            // ghosts now go blind after their body decays
            m_SightExpire = DateTime.Now + CorpseDecayTime();
            m_SightExpire += BoneDecayTime();
            Timer.DelayCall(m_SightExpire - DateTime.Now, new TimerCallback(GoBlind));

			HueMod = -1;
			NameMod = null;
			//SavagePaintExpiration = TimeSpan.Zero;
			
			SetHairMods( -1, -1 );

			PolymorphSpell.StopTimer( this );
			IncognitoSpell.StopTimer( this );
			DisguiseGump.StopTimer( this );

			EndAction( typeof( PolymorphSpell ) );
			EndAction( typeof( IncognitoSpell ) );

			MeerMage.StopEffect( this, false );

			if ( m_PermaFlags.Count > 0 )
			{
				m_PermaFlags.Clear();

				if ( c is Corpse )
					((Corpse)c).Criminal = true;

				if ( SkillHandlers.Stealing.ClassicMode )
					Criminal = true;
			}
#if THIS_IS_NOT_USED
			if ( this.Kills >= 5 && false /*DateTime.Now >= m_NextJustAward*/ )
			{
				Mobile m = FindMostRecentDamager( false );

				if ( m != null && m.Player )
				{
					// bool gainedPath = false;

					int theirTotal = ComputeSkillTotal( m );
					int ourTotal = ComputeSkillTotal( this );

					int pointsToGain = 1 + ((theirTotal - ourTotal) / 50);

					if ( pointsToGain < 1 )
						pointsToGain = 1;
					else if ( pointsToGain > 4 )
						pointsToGain = 4;

					/*					if ( VirtueHelper.Award( m, VirtueName.Justice, pointsToGain, ref gainedPath ) )
					 *					{
					 *						if ( gainedPath )
					 *							m.SendLocalizedMessage( 1049367 ); // You have gained a path in Justice!
					 *						else
					 *							m.SendLocalizedMessage( 1049363 ); // You have gained in Justice.
					 *
					 *						m.FixedParticles( 0x375A, 9, 20, 5027, EffectLayer.Waist );
					 *						m.PlaySound( 0x1F7 );
					 *
					 *						m_NextJustAward = DateTime.Now + TimeSpan.FromMinutes( pointsToGain * 2 );
					 *					}
					 */
					this.Aggressors.Clear();
				}
			}
#endif

#if THIS_IS_NOT_USED
			if ( m_InsuranceCost > 0 )
				SendLocalizedMessage( 1060398, m_InsuranceCost.ToString() ); // ~1_AMOUNT~ gold has been withdrawn from your bank box.

			if ( m_InsuranceAward is PlayerMobile )
			{
				PlayerMobile pm = (PlayerMobile)m_InsuranceAward;

				if ( pm.m_InsuranceBonus > 0 )
					pm.SendLocalizedMessage( 1060397, pm.m_InsuranceBonus.ToString() ); // ~1_AMOUNT~ gold has been deposited into your bank box.
			}
#endif
			if (Mortal)
			{
				Effects.SendBoltEffect(this, false, 100);
				PlaySound(586);
				for (int i = 0; i < 3; i++)
				{
					Point3D p = new Point3D(Location);
					p.X += Utility.RandomMinMax(-1, 1);
					p.Y += Utility.RandomMinMax(-1, 1);
					new Blood(Utility.Random(0x122A, 5), 120.0).MoveToWorld(p, Map);
				}


				this.Frozen = true;
				Timer.DelayCall(TimeSpan.FromSeconds(5.0), new TimerCallback(MortalDeathMessage));
				if (AccessLevel == AccessLevel.Player)
					Timer.DelayCall(TimeSpan.FromSeconds(10.0), new TimerCallback(Delete));
			}

            this.LastDeathTime = DateTime.Now;
			this.ClearDamageEntries();
		}

		//ada: New system to gauge player mobile travel speed as they move over MarkTime objects
		//	please the Server.Items.MarkTime object
		private DateTime m_LastTimeMark = DateTime.MinValue;
		public DateTime LastTimeMark { get { return m_LastTimeMark;} set {m_LastTimeMark = value;}}

        //Pix: note that this doesn't need to be serialized
        private DateTime m_LastDeathTime = DateTime.MinValue;
        [CommandProperty(AccessLevel.Counselor)]
        public DateTime LastDeathTime
        {
            get
            {
                return m_LastDeathTime;
            }
            set
            {
                m_LastDeathTime = value;
            }
        }

        //pla: Override the bone decay value
        public override TimeSpan BoneDecayTime()
        {
            //If this is a commander on his boat then extend the bone decay delay
            BaseBoat boat;
            boat = BaseBoat.FindBoatAt(this);
            if (boat != null && (boat.HasKey(this) || boat.CorpseHasKey(this)))
                return TimeSpan.FromMinutes(20.0);
            else
                return base.BoneDecayTime();
        }

		public void MortalDeathMessage()
		{
			this.SendMessage(0x22, "Thou art dead. Fear thy fate not; pale Death with impartial tread beats at the poor man's cottage door and at the palaces of kings.");
		}

		// Store where items were OnDeath and keep them there, rather than makingthe pack a mess.
		public override bool RetainPackLocsOnDeath{ get{ return true; } }

		private ArrayList m_PermaFlags;
		private ArrayList m_VisList;
		private Hashtable m_AntiMacroTable;
		private TimeSpan m_GameTime;
		private TimeSpan m_ShortTermElapse;
		private TimeSpan m_LongTermElapse;
		private DateTime m_SessionStart;
		private DateTime m_LastEscortTime;
		private DateTime m_NextSmithBulkOrder;
		private DateTime m_NextTailorBulkOrder;
		private DateTime m_SavagePaintExpiration;
		private SkillName m_Learning = (SkillName)(-1);

		private DateTime m_LastShortDecayed;

		[CommandProperty( AccessLevel.GameMaster )]
		public DateTime LastShortDecayed
		{
			get{ return m_LastShortDecayed; }
		}

		public void ReduceKillTimersByHours( double hours )
		{
			m_ShortTermElapse -= TimeSpan.FromHours( hours );
			m_LongTermElapse -= TimeSpan.FromHours( hours );

			DecayKills();
		}

		public static int DoGlobalDecayKills()
		{
			int count = 0;
			try
			{
				foreach ( Mobile m in World.Mobiles.Values )
				{
					if( m is PlayerMobile )
					{
						((PlayerMobile)m).DecayKills();
						count++;
					}
				}
			}
			catch(Exception e)
			{
				LogHelper.LogException(e);
				System.Console.WriteLine("Error in DoGlobalDecayKills");
				System.Console.WriteLine(e.Message);
				System.Console.WriteLine(e.StackTrace);
			}
			return count;
		}

		public static int DoGlobalCleanKillerTimes()
		{
			int count = 0;

			//clean up KillerTimes
			try
			{
				foreach ( Mobile m in World.Mobiles.Values )
				{
					if( m is PlayerMobile )
					{
						count++;
						PlayerMobile pm = (PlayerMobile)m;
						if( pm.KillerTimes != null )
						{
							for(int i=pm.KillerTimes.Count-1; i>=0; i-- )
							{
								if( DateTime.Now - ((ReportMurdererGump.KillerTime)pm.KillerTimes[i]).Time > TimeSpan.FromMinutes(5.0) )
								{
									pm.KillerTimes.RemoveAt(i);
								}
							}
						}
					}
				}
			}
			catch(Exception exc)
			{
				LogHelper.LogException(exc);
				System.Console.WriteLine("Exception Caught in DoGlobalCleanKillerTimes removal code: " + exc.Message);
				System.Console.WriteLine(exc.StackTrace);
			}

			return count;
		}

		public void DecayKills()
		{
			if ( m_ShortTermElapse < this.GameTime
				|| ( (CoreAI.OfflineShortsDecay != 0) && ((DateTime.Now - m_LastShortDecayed) > TimeSpan.FromHours(CoreAI.OfflineShortsDecayHours)) )
				)
			{
				m_LastShortDecayed = DateTime.Now;

				if ( Inmate && Alive )
				{
					m_ShortTermElapse = this.GameTime + TimeSpan.FromHours( 4 );
				}
				else
				{
					m_ShortTermElapse = this.GameTime + TimeSpan.FromHours( 8 );
				}

				if ( ShortTermMurders > 0 )
					--ShortTermMurders;
			}

			if ( m_LongTermElapse < this.GameTime )
			{
				if ( Inmate && Alive )
				{
					m_LongTermElapse = this.GameTime + TimeSpan.FromHours( 20 );
				}
				else
				{
					m_LongTermElapse = this.GameTime + TimeSpan.FromHours( 40 );
				}

				if ( Kills > 0 )
					--Kills;
			}
		}

		public SkillName Learning
		{
			get{ return m_Learning; }
			set{ m_Learning = value; }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public TimeSpan SavagePaintExpiration
		{
			get
			{
				TimeSpan ts = m_SavagePaintExpiration - DateTime.Now;

				if ( ts < TimeSpan.Zero )
					ts = TimeSpan.Zero;

				return ts;
			}
			set
			{
				m_SavagePaintExpiration = DateTime.Now + value;
			}
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public TimeSpan NextSmithBulkOrder
		{
			get
			{
				TimeSpan ts = m_NextSmithBulkOrder - DateTime.Now;

				if ( ts < TimeSpan.Zero )
					ts = TimeSpan.Zero;

				return ts;
			}
			set
			{
				try{ m_NextSmithBulkOrder = DateTime.Now + value; }
				catch (Exception ex) { EventSink.InvokeLogException(new LogExceptionEventArgs(ex)); }
			}
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public TimeSpan NextTailorBulkOrder
		{
			get
			{
				TimeSpan ts = m_NextTailorBulkOrder - DateTime.Now;

				if ( ts < TimeSpan.Zero )
					ts = TimeSpan.Zero;

				return ts;
			}
			set
			{
				try{ m_NextTailorBulkOrder = DateTime.Now + value; }
				catch (Exception ex) { EventSink.InvokeLogException(new LogExceptionEventArgs(ex)); }
			}
		}

		public DateTime LastEscortTime
		{
			get{ return m_LastEscortTime; }
			set{ m_LastEscortTime = value; }
		}

		public PlayerMobile()
		{
			this.STRBonusCap = STRBonusCapDefault;		// have player mobiles start out with a capped STR bonus when using weapons
			m_LastSkillGainTime = new DateTime[52];
			m_VisList = new ArrayList();
			m_PermaFlags = new ArrayList();
			m_AntiMacroTable = new Hashtable();
			m_BOBFilter = new Engines.BulkOrders.BOBFilter();
			m_GameTime = TimeSpan.Zero;

			if ( Inmate && Alive )
			{
				m_ShortTermElapse = TimeSpan.FromHours( 4.0 );
				m_LongTermElapse = TimeSpan.FromHours( 20.0 );
			}
			else
			{
				m_ShortTermElapse = TimeSpan.FromHours( 8.0 );
				m_LongTermElapse = TimeSpan.FromHours( 40.0 );
			}

			m_JusticeProtectors = new ArrayList();

			m_LastSkillUsed     = new SkillName();		// wea: for [FindSkill
			m_LastSkillTime     = new DateTime();		//

			m_SpiritCohesion = 0;						// wea: for Spirit Cohesion
			m_LastResurrectTime = new DateTime();

			m_SpeechRecord = new Queue();				// TK: [report command
			m_Reported = DateTime.MinValue;

			InvalidateMyRunUO();
		}

		public override bool MutateSpeech( ArrayList hears, ref string text, ref object context )
		{
			if ( Alive )
				return false;

			if ( Core.AOS )
			{
				for ( int i = 0; i < hears.Count; ++i )
				{
					object o = hears[i];

					if ( o != this && o is Mobile && ((Mobile)o).Skills[SkillName.SpiritSpeak].Value >= 100.0 )
						return false;
				}
			}

			return base.MutateSpeech( hears, ref text, ref context );
		}

		public void RecordSpeech(Mobile speaker, string text, string note)
		{
			if (!(speaker is PlayerMobile))
				return;

			string msg = "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + speaker.Name + " " + "(acct " + ((Account)speaker.Account).Username + ") " + (note != null ? "(" + note + ") " : "") + ": " + text;
			m_SpeechRecord.Enqueue(new SpeechRecordEntry(msg));
			while (((SpeechRecordEntry)m_SpeechRecord.Peek()).Time < DateTime.Now - TimeSpan.FromMinutes(5.0))
				m_SpeechRecord.Dequeue();

			if (m_ReportLogger != null)
			{
				m_ReportLogger.Log(LogType.Text, msg);
			}
		}

		public void Report(Mobile from)
		{
			if (m_ReportLogger != null)
			{
				m_ReportLogger.Log(LogType.Text, "\n**** Reported again by " + from.Name + " ****\n");
				m_ReportLogger.Finish();
				if (m_ReportLogStopper != null)
					m_ReportLogStopper.Stop();
			}
		
			m_Reported = DateTime.Now;
			m_ReportLogger = new LogHelper( GetReportLogName(m_Reported.ToString("MM-dd-yyyy HH-mm-ss")) );
			m_ReportLogger.Log(LogType.Text, String.Format("{0} (acct {1}, SN {2}, IP {3}) reported by {4} (acct {5}, SN {6}) at {7}, at {8}.\r\n\r\n",
				this.Name, ((Account)this.Account).Username, this.Serial, ((this.NetState != null) ? this.NetState.ToString() : ""), from.Name, ((Account)from.Account).Username, from.Serial, DateTime.Now, from.Location));
            //Console.WriteLine("{0} (acct {1}, SN {2}, IP {3}) reported by {4} (acct {5}, SN {6}) at {7}, at {8}.\r\n\r\n",
            //    this.Name, ((Account)this.Account).Username, this.Serial, this.NetState.ToString(), from.Name, ((Account)from.Account).Username, from.Serial, DateTime.Now, from.Location);

			while (m_SpeechRecord.Count > 0)
				m_ReportLogger.Log(LogType.Text, ((SpeechRecordEntry)m_SpeechRecord.Dequeue()).Speech);

			m_ReportLogStopper = Timer.DelayCall(ReportTime, new TimerCallback(EndReport));
		}

		private string GetReportLogName(string datestring)
		{
			string filename = String.Format("{0} {1}.log", datestring, this.Name);

			char[] illegalcharacters = {'\\', '/', ':', '*', '?', '\"', '<', '>', '|'};

			if( filename.IndexOfAny( illegalcharacters ) != -1 )
			{
				for( int i=0; i<illegalcharacters.Length; i++ )
				{										  
					filename = filename.Replace( illegalcharacters[i], '_' );
				}
			}

			return filename.Trim();
		}

		private void EndReport()
		{
			if (m_ReportLogger != null)
			{
				m_ReportLogger.Finish();
				m_ReportLogger = null;
			}
			if (m_ReportLogStopper != null)
			{
				m_ReportLogStopper.Stop();
				m_ReportLogStopper = null;
			}
		}

		public override void OnSaid(SpeechEventArgs e)
		{
			base.OnSaid (e);

			RecordSpeech(e.Mobile, e.Speech, (e.Blocked ? "blocked" : null));
		}

		public override bool HandlesOnSpeech(Mobile from)
		{
			if (m_ReportLogger != null && from != this)
				return true;

			return base.HandlesOnSpeech (from);
		}

		public override void OnSpeech(SpeechEventArgs e)
		{
			base.OnSpeech (e);

			if (e.Mobile == this)
				return;

			RecordSpeech(e.Mobile, e.Speech, null);
		}


		public override void SendAlliedChat( string text )
		{
			Server.Guilds.Guild g = this.Guild as Server.Guilds.Guild;
			if( g != null )
			{
				//g.AlliedMessage( string.Format("[Ally][{0} [{1}]]: {2}", this.Name, g.Abbreviation, text ) );
				g.AlliedChat(text, this);

				//Let GM+ overhear
				Packet p = null;
				IPooledEnumerable eable = this.GetClientsInRange( 8 );
				foreach ( NetState ns in eable)
				{
					Mobile mob = ns.Mobile;

					if ( mob != null
						&& mob.AccessLevel >= AccessLevel.GameMaster
						&& mob.AccessLevel > this.AccessLevel  )
					{
						if ( p == null )
							p = Packet.Acquire( new UnicodeMessage( this.Serial, this.Body, MessageType.Regular, this.SpeechHue, 3, this.Language, this.Name, String.Format( "[Allied]: {0}", text ) ));

						ns.Send( p );
					}
				}
				eable.Free();
				//end GM+ overhear

				Packet.Release(p);

				// record speech
				RecordSpeech(this, text, "allied");
			}
			else
			{
				this.SendMessage( 76, "You can't send a message to your allies if you don't belong to a guild." );
			}
		}

		public override void SendGuildChat( string text )
		{
			Server.Guilds.Guild g = this.Guild as Server.Guilds.Guild;
			if( g != null )
			{
				//g.GuildMessage( string.Format("[Guild][{0}]: {1}", this.Name, text) );
				g.GuildChat(text, this);

				//Let GM+ overhear
				Packet p = null;
				IPooledEnumerable eable = this.GetClientsInRange( 8 );
				foreach ( NetState ns in eable)
				{
					Mobile mob = ns.Mobile;

					if ( mob != null
						&& mob.AccessLevel >= AccessLevel.GameMaster
						&& mob.AccessLevel > this.AccessLevel  )
					{
						if ( p == null )
							p = Packet.Acquire( new UnicodeMessage( this.Serial, this.Body, MessageType.Regular, this.SpeechHue, 3, this.Language, this.Name, String.Format( "[Guild]: {0}", text ) ));

						ns.Send( p );
					}
				}
				eable.Free();
				//end GM+ overhear

				Packet.Release(p);

				// record speech
				RecordSpeech(this, text, "guild");
			}
			else
			{
				this.SendMessage( 76, "You can't send a message to your guild if you don't belong to one." );
			}
		}

		public override void Damage( int amount, Mobile from )
		{
			//if ( Spells.Necromancy.EvilOmenSpell.CheckEffect( this ) )
			//amount = (int)(amount * 1.25);

			//Mobile oath = Spells.Necromancy.BloodOathSpell.GetBloodOath( from );
			/*
					if ( oath == this )
					{
						amount = (int)(amount * 1.1);
						from.Damage( amount, from );
					}
			*/
			base.Damage( amount, from );

			//Explosion Potion Check
			if( amount >= CoreAI.ExplosionPotionSensitivityLevel )
			{
				if( this.Backpack != null )
				{
					Item[] explosionPotions = this.Backpack.FindItemsByType( typeof(Server.Items.BaseExplosionPotion), true );
					for( int i=0; i<explosionPotions.Length; i++ )
					{
						double chance = CoreAI.ExplosionPotionChance;
						double alchyskill = this.Skills[SkillName.Alchemy].Value;

						//NOTE: chance will ALWAYS be 0 for a GM alchemist
						chance *= ( (100.0 - alchyskill)/100.0 );

						if( Utility.RandomDouble() < chance )
						{
							((Server.Items.BaseExplosionPotion)explosionPotions[i]).Explode(this, false, this.Location, this.Map);
						}
					}
				}
			}

		}

		public override ApplyPoisonResult ApplyPoison( Mobile from, Poison poison )
		{
			if ( !Alive )
				return ApplyPoisonResult.Immune;

			//if ( Spells.Necromancy.EvilOmenSpell.CheckEffect( this ) )
			//return base.ApplyPoison( from, PoisonImpl.IncreaseLevel( poison ) );

			return base.ApplyPoison( from, poison );
		}

		public PlayerMobile( Serial s ) : base( s )
		{
			m_LastSkillGainTime = new DateTime[52];

			m_VisList = new ArrayList();
			m_AntiMacroTable = new Hashtable();
			m_SpeechRecord = new Queue();
			m_Reported = DateTime.MinValue;
			InvalidateMyRunUO();
		}

		public ArrayList VisibilityList
		{
			get{ return m_VisList; }
		}

		public void RemoveVis( int indexnum ) //added 08/30/04 smerX
		{
			if ( m_VisList.Count >= indexnum )
			{
				m_VisList.RemoveAt( indexnum );
			}
		}

		public ArrayList PermaFlags
		{
			get{ return m_PermaFlags; }
		}

		public override int Luck{ get{ return AosAttributes.GetValue( this, AosAttribute.Luck ); } }

		public override bool IsHarmfulCriminal( Mobile target )
		{
			if ( SkillHandlers.Stealing.ClassicMode && target is PlayerMobile && ((PlayerMobile)target).m_PermaFlags.Count > 0 )
			{
				int noto = Notoriety.Compute( this, target );

				if ( noto == Notoriety.Innocent )
					target.Delta( MobileDelta.Noto );

				return false;
			}

			if ( target is BaseCreature
				&& ((BaseCreature)target).InitialInnocent
				&& ((BaseCreature)target).Controlled == false )
			{
				return false;
			}

			return base.IsHarmfulCriminal( target );
		}

		public bool AntiMacroCheck( Skill skill, object obj )
		{
			if ( obj == null || m_AntiMacroTable == null || this.AccessLevel != AccessLevel.Player )
				return true;

			Hashtable tbl = (Hashtable)m_AntiMacroTable[skill];
			if ( tbl == null )
				m_AntiMacroTable[skill] = tbl = new Hashtable();

			CountAndTimeStamp count = (CountAndTimeStamp)tbl[obj];
			if ( count != null )
			{
				if ( count.TimeStamp + AntiMacroExpire <= DateTime.Now )
				{
					count.Count = 1;
					return true;
				}
				else
				{
					++count.Count;
					if ( count.Count <= Allowance )
						return true;
					else
						return false;
				}
			}
			else
			{
				tbl[obj] = count = new CountAndTimeStamp();
				count.Count = 1;

				return true;
			}
		}

		private void RevertHair()
		{
			SetHairMods( -1, -1 );
		}

		private Engines.BulkOrders.BOBFilter m_BOBFilter;

		public Engines.BulkOrders.BOBFilter BOBFilter
		{
			get{ return m_BOBFilter; }
		}

        private SaveFlag ReadSaveBits(GenericReader reader, int currentVersion, int firstVersion)
        {
            if (currentVersion < firstVersion)
                return SaveFlag.None;
            else
                return (SaveFlag)reader.ReadInt();
        }

        private SaveFlag WriteSaveBits(GenericWriter writer)
        {   // calculate save flags
            SaveFlag flags = SaveFlag.None;
			SetSaveFlag(ref flags, SaveFlag.NPCGuild, NpcGuild != NpcGuild.None);
			SetSaveFlag(ref flags, SaveFlag.ZCodeMiniGame, ZCodeMiniGameID != 0);
            writer.Write((int)flags);
            return flags;
        }

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
            SaveFlag saveFlags = ReadSaveBits(reader, version, 30);

			///////////////////////////////////////////////////
			// put all normal serialization below this line
			///////////////////////////////////////////////////

	        switch ( version )
			{
				case 32:
					{
						//Adam: v32 add mini game ID and save data.
						if (GetSaveFlag(saveFlags, SaveFlag.ZCodeMiniGame) == true)
						{
							m_ZCodeMiniGameID = reader.ReadInt();				// hash code of the string naming the mini game
							int size = reader.ReadInt();						// saved game size
							m_ZCodeMiniGameData = new byte[size];				// allocate a new game buffer
							for (int ix = 0; ix < size; ix++)
								m_ZCodeMiniGameData[ix] = reader.ReadByte();	// saved game
						}
						goto case 31;
					}
                case 31:
                {
                    if (GetSaveFlag(saveFlags, SaveFlag.NPCGuild) == true)
                        m_NpcGuildPoints = reader.ReadDouble();

                    goto case 30;
                }
                case 30: // Adam: v.30 Dummy version, removed NPCGuild vars when not needed
                {
                    goto case 29;
                }
				case 29: //Pla: Dummy version, removed duel system vars
				{
					goto case 28;
				}
                case 28: //Pix: Kin Faction additions
                {
                    m_KinSoloPoints = reader.ReadDouble();
                    m_KinTeamPoints = reader.ReadDouble();

                    goto case 27;
                }
				case 27: //Pix: challenge duel system
				{
					//pla: not used anymore
					if (version < 29)
					{
						//m_iChallengeDuelWins = reader.ReadInt();
						//m_iChallengeDuelLosses = reader.ReadInt();
						reader.ReadInt();
						reader.ReadInt();
					}
					
					goto case 26;
				}
                case 26: //Adam: ghost blindness
                {
                    m_Blind = reader.ReadBool();
                    m_SightExpire = reader.ReadDateTime();
                    if (m_SightExpire != DateTime.MaxValue)
                    {
                        if (m_SightExpire <= DateTime.Now)
                            Timer.DelayCall(TimeSpan.Zero, new TimerCallback(GoBlind));
                        else
                            Timer.DelayCall(m_SightExpire - DateTime.Now, new TimerCallback(GoBlind));
                    }
                           
                    goto case 25;
                }
				case 25: //Pix: WatchList enhancements
				{
					m_WatchReason = reader.ReadString();
					m_WatchExpire = reader.ReadDateTime();
					goto case 24;
				}
				case 24: // Rhi: FilterMusic
				{
					m_FilterMusic = reader.ReadBool();
					goto case 23;
				}
				case 23: // Pix: IOB System changes
				{
					m_IOBKillPoints = reader.ReadDouble();
					m_LastGuildIOBAlignment = (IOBAlignment)reader.ReadInt();
					m_LastGuildChange = reader.ReadDateTime();

					goto case 22;
				}
				case 22:
				{
					m_Reported = reader.ReadDateTime();
					if (m_Reported > DateTime.Now - ReportTime)
					{
						m_ReportLogger = new LogHelper( GetReportLogName(m_Reported.ToString("MM-dd-yyyy HH-mm-ss")), false );
						m_ReportLogStopper = Timer.DelayCall(ReportTime - (DateTime.Now - m_Reported), new TimerCallback(EndReport));
					}
					goto case 21;
				}
				case 21:
				{
					LastRegion = Region.Find( this.Location, this.Map );
					goto case 20;
				}
				case 20: //Pix: Offline short count decay
				{
					m_LastShortDecayed = reader.ReadDateTime();
					goto case 19;
				}
				case 19: //Pix - for IOB Ranks
				{
					m_IOBRankTime = reader.ReadTimeSpan();
					goto case 18;
				}
				case 18: //Pigpen - Addition for IOB Sytem
				{
					if( version < 23 )
					{
						//m_IOBAlignment = (IOBAlignment)reader.ReadInt();
						//IOBTimer = reader.ReadTimeSpan();
						reader.ReadInt();
						reader.ReadTimeSpan();
					}
					m_IOBEquipped = reader.ReadBool();
					goto case 16;
				}
				case 17: // changed how DoneQuests is serialized
				case 16:
				{
					m_Quest = QuestSerializer.DeserializeQuest( reader );

					if ( m_Quest != null )
						m_Quest.From = this;

					int count = reader.ReadEncodedInt();

					if ( count > 0 )
					{
						m_DoneQuests = new ArrayList();

						for ( int i = 0; i < count; ++i )
						{
							Type questType = QuestSerializer.ReadType( QuestSystem.QuestTypes, reader );
							DateTime restartTime;

							if ( version < 17 )
								restartTime = DateTime.MaxValue;
							else
								restartTime = reader.ReadDateTime();

							m_DoneQuests.Add( new QuestRestartInfo( questType, restartTime ) );
						}
					}

					m_Profession = reader.ReadEncodedInt();
					goto case 15;
				}
				case 15:
				{
					m_LastCompassionLoss = reader.ReadDeltaTime();
					goto case 14;
				}
				case 14:
				{
					m_CompassionGains = reader.ReadEncodedInt();

					if ( m_CompassionGains > 0 )
						m_NextCompassionDay = reader.ReadDeltaTime();

					goto case 13;
				}
				case 13: // just removed m_PayedInsurance list
				case 12:
				{
					m_BOBFilter = new Engines.BulkOrders.BOBFilter( reader );
					goto case 11;
				}
				case 11:
				{
					if ( version < 13 )
					{
						ArrayList payed = reader.ReadItemList();
						// Adam: no more insurance
						//for ( int i = 0; i < payed.Count; ++i )
						//((Item)payed[i]).PayedInsurance = true;
					}

					goto case 10;
				}
				case 10:
				{
					if ( reader.ReadBool() )
					{
						m_HairModID = reader.ReadInt();
						m_HairModHue = reader.ReadInt();
						m_BeardModID = reader.ReadInt();
						m_BeardModHue = reader.ReadInt();

						// We cannot call SetHairMods( -1, -1 ) here because the items have not yet loaded
						Timer.DelayCall( TimeSpan.Zero, new TimerCallback( RevertHair ) );
					}

					goto case 9;
				}
				case 9:
				{
					SavagePaintExpiration = reader.ReadTimeSpan();

					if ( SavagePaintExpiration > TimeSpan.Zero )
					{
						// BodyMod = ( Female ? 184 : 183 );
						HueMod = 0;
					}

					goto case 8;
				}
				case 8:
				{
                    if (version < 30)
                    {
                        m_NpcGuild = (NpcGuild)reader.ReadInt();
                        m_NpcGuildJoinTime = reader.ReadDateTime();
                        m_NpcGuildGameTime = reader.ReadTimeSpan();
                    }
                    else if (GetSaveFlag(saveFlags, SaveFlag.NPCGuild) == true)
                    {
                        m_NpcGuild = (NpcGuild)reader.ReadInt();
                        m_NpcGuildJoinTime = reader.ReadDateTime();
                        m_NpcGuildGameTime = reader.ReadTimeSpan();
                    }
					goto case 7;
				}
				case 7:
				{
					m_PermaFlags = reader.ReadMobileList();
					goto case 6;
				}
				case 6:
				{
					NextTailorBulkOrder = reader.ReadTimeSpan();
					goto case 5;
				}
				case 5:
				{
					NextSmithBulkOrder = reader.ReadTimeSpan();
					goto case 4;
				}
				case 4:
				{
					m_LastJusticeLoss = reader.ReadDeltaTime();
					m_JusticeProtectors = reader.ReadMobileList();
					goto case 3;
				}
				case 3:
				{
					m_LastSacrificeGain = reader.ReadDeltaTime();
					m_LastSacrificeLoss = reader.ReadDeltaTime();
					m_AvailableResurrects = reader.ReadInt();
					goto case 2;
				}
				case 2:
				{
					m_Flags = (PlayerFlag)reader.ReadInt();
					goto case 1;
				}
				case 1:
				{
					m_LongTermElapse = reader.ReadTimeSpan();
					m_ShortTermElapse = reader.ReadTimeSpan();
					m_GameTime = reader.ReadTimeSpan();
					goto case 0;
				}
				case 0:
				{
					break;
				}
			}

			if ( m_PermaFlags == null )
				m_PermaFlags = new ArrayList();

			if ( m_JusticeProtectors == null )
				m_JusticeProtectors = new ArrayList();

			if ( m_BOBFilter == null )
				m_BOBFilter = new Engines.BulkOrders.BOBFilter();

			ArrayList list = this.Stabled;

			for ( int i = 0; i < list.Count; ++i )
			{
				BaseCreature bc = list[i] as BaseCreature;

				if ( bc != null )
					bc.IsStabled = true;
			}

			//Pix: this is for safety... to make sure it's set
			m_InmateLastDeathTime = DateTime.MinValue;

			//Pix: make sure this is set to minvalue for loading
			m_IOBStartedWearing = DateTime.MinValue;

			//wea: SpiritCohesion is not persistent across saves
			m_SpiritCohesion = 0;

			//wea: For spirit cohesion, last resurrect time
			m_LastResurrectTime = DateTime.MinValue;

		}

		public override void Serialize( GenericWriter writer )
        {
            #region garbage?
            //cleanup our anti-macro table
			foreach ( Hashtable t in m_AntiMacroTable.Values )
			{
				ArrayList remove = new ArrayList();
				foreach ( CountAndTimeStamp time in t.Values )
				{
					if ( time.TimeStamp + AntiMacroExpire <= DateTime.Now )
						remove.Add( time );
				}

				for (int i=0;i<remove.Count;++i)
					t.Remove( remove[i] );
			}
            #endregion
            base.Serialize( writer );
            int version = 32;                           // updates for NPCGuils 'smart serialization'
			writer.Write( version );                    // write the version    
            SaveFlag saveFlags = WriteSaveBits(writer); // calculate and write the bits that describe what we will write

			///////////////////////////////////////////////////
			// put all normal serialization below this line
			///////////////////////////////////////////////////

			//Adam: v32 add mini game ID and save data.
			if (GetSaveFlag(saveFlags, SaveFlag.ZCodeMiniGame) == true)
			{	// assert (record) this case 
				if (Misc.Diagnostics.Assert(m_ZCodeMiniGameData != null && m_ZCodeMiniGameData.Length > 0, "In PlayerMobile.cs the following is NOT true: m_ZCodeMiniGameData != null && m_ZCodeMiniGameData.Length > 0"))
				{
					writer.Write(m_ZCodeMiniGameID);									// hash code of the string naming the mini game
					writer.Write(m_ZCodeMiniGameData.Length);							// buffer size
					writer.Write(m_ZCodeMiniGameData, 0, m_ZCodeMiniGameData.Length);	// saved game
				}
			}

            //Adam: v31 Add in new points tracker for NPCGuilds
            if (GetSaveFlag(saveFlags, SaveFlag.NPCGuild) == true)
                writer.Write(m_NpcGuildPoints);

            //Adam: v.30 Dummy version, removed NPCGuild vars when not needed

			//Pla: v.29 - dummy version for duel system removal

          //Pix: v.28 - Kin Faction Stuff
          writer.Write(m_KinSoloPoints);
          writer.Write(m_KinTeamPoints);

			//Pix: v.27 - Challenge Duel
			//Pla: No longer used as of v.29
			//writer.Write(m_iChallengeDuelWins);
			//writer.Write(m_iChallengeDuelLosses);

          //Adam: v.26
          writer.Write(m_Blind);
          writer.Write(m_SightExpire);

			//Pix: v.25 Watchlist enhancements
			writer.Write(m_WatchReason);
			writer.Write(m_WatchExpire);

			//Rhi: [FilterMusic
			writer.Write( m_FilterMusic );

			//PIX: new IOB funcionality
			writer.Write( m_IOBKillPoints );
			writer.Write( (int)m_LastGuildIOBAlignment );
			writer.Write( this.m_LastGuildChange );

			// tk - [report
			writer.Write(m_Reported);

			//Pix: Offline short count decay
			writer.Write( m_LastShortDecayed );

			//Pix: TimeSpan for RANK of bretheren
			TimeSpan ranktime = m_IOBRankTime;
			if( IOBEquipped && m_IOBStartedWearing > DateTime.MinValue )
			{
				ranktime += (DateTime.Now - m_IOBStartedWearing);
			}
			writer.Write( ranktime );

			//Pix: 3/26/06 - changes to IOB system
			// no longer store IOBAlignment or IOBTimer in PMs
			//writer.Write( (int) m_IOBAlignment );
			//writer.Write( IOBTimer );
			writer.Write( (bool) m_IOBEquipped );

			QuestSerializer.Serialize( m_Quest, writer );

			if ( m_DoneQuests == null )
			{
				writer.WriteEncodedInt( (int) 0 );
			}
			else
			{
				writer.WriteEncodedInt( (int) m_DoneQuests.Count );

				for ( int i = 0; i < m_DoneQuests.Count; ++i )
				{
					QuestRestartInfo restartInfo = (QuestRestartInfo)m_DoneQuests[i];

					QuestSerializer.Write( (Type) restartInfo.QuestType, QuestSystem.QuestTypes, writer );
					writer.Write( (DateTime) restartInfo.RestartTime );
				}
			}

			writer.WriteEncodedInt( (int) m_Profession );

			writer.WriteDeltaTime( m_LastCompassionLoss );

			writer.WriteEncodedInt( m_CompassionGains );

			if ( m_CompassionGains > 0 )
				writer.WriteDeltaTime( m_NextCompassionDay );

			m_BOBFilter.Serialize( writer );

			bool useMods = ( m_HairModID != -1 || m_BeardModID != -1 );

			writer.Write( useMods );

			if ( useMods )
			{
				writer.Write( (int) m_HairModID );
				writer.Write( (int) m_HairModHue );
				writer.Write( (int) m_BeardModID );
				writer.Write( (int) m_BeardModHue );
			}

			writer.Write( SavagePaintExpiration );

            // Adam: Version 30 optimization: Only write values if we belong to a guild
            if (GetSaveFlag(saveFlags, SaveFlag.NPCGuild) == true)
            {
                writer.Write((int)m_NpcGuild);
                writer.Write((DateTime)m_NpcGuildJoinTime);
                writer.Write((TimeSpan)m_NpcGuildGameTime);
            }

			writer.WriteMobileList( m_PermaFlags, true );

			writer.Write( NextTailorBulkOrder );

			writer.Write( NextSmithBulkOrder );

			writer.WriteDeltaTime( m_LastJusticeLoss );
			writer.WriteMobileList( m_JusticeProtectors, true );

			writer.WriteDeltaTime( m_LastSacrificeGain );
			writer.WriteDeltaTime( m_LastSacrificeLoss );
			writer.Write( m_AvailableResurrects );

			writer.Write( (int) m_Flags );

			writer.Write( m_LongTermElapse );
			writer.Write( m_ShortTermElapse );
			writer.Write( this.GameTime );
		}

		public void ResetKillTime()
		{
			if ( Inmate && Alive )
			{
				m_ShortTermElapse = this.GameTime + TimeSpan.FromHours( 4 );
				m_LongTermElapse = this.GameTime + TimeSpan.FromHours( 20 );
			}
			else
			{
				m_ShortTermElapse = this.GameTime + TimeSpan.FromHours( 8 );
				m_LongTermElapse = this.GameTime + TimeSpan.FromHours( 40 );
			}

			//also reset last short decay (for offline decay)
			m_LastShortDecayed = DateTime.Now;
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public TimeSpan GameTime
		{
			get
			{
				if ( NetState != null )
					return m_GameTime + (DateTime.Now - m_SessionStart);
				else
					return m_GameTime;
			}
		}

		// wea: check region info access level and determine isolation

		public bool IsIsolatedFrom( Mobile m )
		{
			if( m == this || AccessLevel > AccessLevel.Player || m.Region == this.Region )
				return false;

			if(m == null)
				return false;

			if( Region is CustomRegion )
			{
				if( ((CustomRegion) Region).GetRegionControler().IsIsolated )
				{
					if( m.Region is CustomRegion )
					{
						if( !((CustomRegion) m.Region).GetRegionControler().IsIsolated )
							return true;
						else
							return false;
					}
					else
						return true;
				}
			}

			return false;
		}

		public bool IsIsolatedFrom( Item item )
		{
			if(item == null || item.Deleted)
				return false;
			try
			{
				Region reg;
				//first quick check no reg checking needed yet
				if( item.ParentMobile == this || this.AccessLevel > AccessLevel.Player)
					return false;
				//okay we need to figure out items region, look if its on a parent or not first
				if(item.ParentMobile != null)
					reg = CustomRegion.FindDRDTRegion(item, item.ParentMobile.Location);
				else if(item.Parent == null)
					reg = CustomRegion.FindDRDTRegion(item, item.Location);
				else
				{  //worse case scenario, its not on a mobile and its nested, use recursiveness
					Item temp = item;
					while(temp.Parent != null)
					{
						temp = (Item)temp.Parent;
					}
					reg = CustomRegion.FindDRDTRegion(temp, temp.Location);
				}
							
				//region check
				if(reg != null && reg == this.Region)
					return false;

				if( Region is CustomRegion )
				{
					if( ((CustomRegion) Region).GetRegionControler().IsIsolated )
					{
						if( reg is CustomRegion )
						{
							if( !((CustomRegion)reg).GetRegionControler().IsIsolated )
								return true;
							else
								return false;
						}
						else
							return true;
					}
				}
			}
			catch(Exception e)
			{
				LogHelper.LogException(e);
				Console.WriteLine("Exception caught in IsIsolatedFrom(item) please send to Zen!");
				Console.WriteLine("{0} Caught exception.", e);
				Console.WriteLine(e.StackTrace);
			}
			return false;
		}

		public override bool CanSee( Mobile m )
		{
			// wea: if we're isolated from someone, we can't see them
			if( IsIsolatedFrom( m ) )
				return false;

			if ( m is PlayerMobile && ((PlayerMobile)m).m_VisList.Contains( this ) )
				return true;

            #region Ghost Blindness
            // if we are blind (mobiles)
            if (m_Blind == true && this.AccessLevel == AccessLevel.Player)
            {
                if (! (m == this || m is BaseHealer ||                                       // IF NOT: me or a healer
                      (m is BaseCreature && (m as BaseCreature).ControlMaster == this)) )    // or one of my pets 
                    return false;
            }
            #endregion

            return base.CanSee( m );
		}

    	public override bool CanSee( Item item )
		{
			if(IsIsolatedFrom(item))
				return false;

			if ( m_DesignContext != null && m_DesignContext.Foundation.IsHiddenToCustomizer( item ) )
				return false;

            #region Ghost Blindness
            // blind filtering
            if (m_Blind == true && this.AccessLevel == AccessLevel.Player)
            {
                // if we are blind (corpses)
                if (item is Corpse && (item as Corpse).Owner != this) // if not my corpse
                    return false;

                // if we are blind (boats)
                // we do not see boats but our own
                if (item is BaseBoat && BaseBoat.FindBoatAt(this) != item) // if not my boat
                    return false;

                // if we are blind, check for actionable boat parts not counted as part of the multi
                // we do not see boat parts but our own
                BaseBoat boatAtItem = BaseBoat.FindBoatAt(item);
                if (boatAtItem != null && (                                 // boat at item
                    boatAtItem.PPlank.ItemID == item.ItemID ||              // there 4 facings for all of these boat parts!
                    boatAtItem.SPlank.ItemID == item.ItemID ||              //  this is why we pull them from the boat instead of 
                    boatAtItem.TillerMan.ItemID == item.ItemID ||           //  checking the ItemIDs directally
                    boatAtItem.Hold.ItemID == item.ItemID )                 //  ----
                    && BaseBoat.FindBoatAt(this) != boatAtItem)             // and if not my boat
                    return false;
            }
            #endregion

            return base.CanSee( item );
		}

		#region Quest stuff
		private QuestSystem m_Quest;
		private ArrayList m_DoneQuests;

		public QuestSystem Quest
		{
			get{ return m_Quest; }
			set{ m_Quest = value; }
		}

		public ArrayList DoneQuests
		{
			get{ return m_DoneQuests; }
			set{ m_DoneQuests = value; }
		}
		#endregion

		#region MyRunUO Invalidation
		private bool m_ChangedMyRunUO;

		public bool ChangedMyRunUO
		{
			get{ return m_ChangedMyRunUO; }
			set{ m_ChangedMyRunUO = value; }
		}

		public void InvalidateMyRunUO()
		{
			if ( !Deleted && !m_ChangedMyRunUO )
			{
				m_ChangedMyRunUO = true;
				Engines.MyRunUO.MyRunUO.QueueMobileUpdate( this );
			}
		}

		public override void OnKillsChange( int oldValue )
		{
			InvalidateMyRunUO();
		}

		public override void OnGenderChanged( bool oldFemale )
		{
			InvalidateMyRunUO();
		}

		public override void OnGuildTitleChange( string oldTitle )
		{
			InvalidateMyRunUO();
		}

		public override void OnKarmaChange( int oldValue )
		{
			InvalidateMyRunUO();
		}

		public override void OnFameChange( int oldValue )
		{
			InvalidateMyRunUO();
		}

		public override void OnSkillChange( SkillName skill, double oldBase )
		{
			InvalidateMyRunUO();
		}

		public override void OnAccessLevelChanged( AccessLevel oldLevel )
		{
			InvalidateMyRunUO();
		}

		public override void OnRawStatChange( StatType stat, int oldValue )
		{

			if ( this.AccessLevel < AccessLevel.GameMaster )
			{
				if ( this.StatCap > 225 )
					this.StatCap = 225;
				if ( this.RawDex > 100 )
					this.RawDex = 100;
				if ( this.RawInt > 100 )
					this.RawInt = 100;
				if ( this.RawStr > 100 )
					this.RawStr = 100;
			}

			InvalidateMyRunUO();
		}

		public override void OnDelete()
		{
			InvalidateMyRunUO();
		}
		#endregion

		// this fast walk code is from  Ingvarr on the runuo.com boards
		//	http://www.runuo.com/forums/script-support/46364-speed-hack-detection-help-2.html
		//	not yet tested
		#region Fastwalk Prevention (RUNUO)
		/*
		private static bool FastwalkPrevention = true; // Is fastwalk prevention enabled?
		private static TimeSpan FastwalkThreshold = TimeSpan.FromSeconds(0.095);

		private DateTime m_NextMovementTime;

		public virtual bool UsesFastwalkPrevention { get { return (AccessLevel < AccessLevel.GameMaster); } }

		public virtual TimeSpan ComputeMovementSpeed(Direction dir)
		{
			if ((dir & Direction.Mask) != (this.Direction & Direction.Mask))
				return TimeSpan.Zero;

			bool running = ((dir & Direction.Running) != 0);

			bool onHorse = (this.Mount != null);

			if (onHorse)
				return (running ? TimeSpan.FromSeconds(0.1) : TimeSpan.FromSeconds(0.2)) - TimeSpan.FromSeconds(0.005);

			return (running ? TimeSpan.FromSeconds(0.2) : TimeSpan.FromSeconds(0.4)) - TimeSpan.FromSeconds(0.005);
		}

		public static bool MovementThrottle_Callback(NetState ns)
		{
			PlayerMobile pm = ns.Mobile as PlayerMobile;

			if (pm == null || !pm.UsesFastwalkPrevention)
				return true;

			if (pm.m_NextMovementTime == DateTime.MinValue)
			{
				// has not yet moved
				pm.m_NextMovementTime = DateTime.Now;
				return true;
			}

			TimeSpan ts = pm.m_NextMovementTime - DateTime.Now;

			if (ts < TimeSpan.Zero)
			{
				// been a while since we've last moved
				pm.m_NextMovementTime = DateTime.Now;
				return true;
			}

			return (ts < FastwalkThreshold);
		}
		*/
		#endregion

		#region Fastwalk Prevention
		public static bool FastwalkPrevention = true; // Is fastwalk prevention enabled?
		public static TimeSpan FastwalkThreshold = TimeSpan.FromSeconds( 0.4 ); // Fastwalk prevention will become active after 0.4 seconds
        public static AccessLevel FastWalkAccessOverride = AccessLevel.GameMaster;

		private DateTime m_NextMovementTime;

		public virtual bool UsesFastwalkPrevention{ get{ return ( AccessLevel < FastWalkAccessOverride ); } }

		public override TimeSpan ComputeMovementSpeed( Direction dir )
		{
			if ( (dir & Direction.Mask) != (this.Direction & Direction.Mask) )
				return TimeSpan.Zero;

			bool running = ( (dir & Direction.Running) != 0 );

			bool onHorse = (AccessLevel > AccessLevel.Player) && ( this.Mount != null );

			if ( onHorse )
				return ( running ? TimeSpan.FromSeconds( 0.1 ) : TimeSpan.FromSeconds( 0.2 ) );

			return ( running ? TimeSpan.FromSeconds( 0.2 ) : TimeSpan.FromSeconds( 0.4 ) );
		}

        public static int ThrottleCallThreshold = 10;
        public static int ThrottleRunWarningThreshold = 10;
        public static TimeSpan ThrottleCountPeriod = TimeSpan.FromSeconds(1.0);

        public static bool MovementThrottle_Callback( NetState ns )
		{
            if (!FastwalkPrevention)
                return true;

			PlayerMobile pm = ns.Mobile as PlayerMobile;

            if (pm == null || !pm.UsesFastwalkPrevention)
                return true;

            MovementReqCapture.HitMR(pm);

			if ( pm.m_NextMovementTime == DateTime.MinValue )
			{
				// has not yet moved
				pm.m_NextMovementTime = DateTime.Now;

                return true;
			}

			TimeSpan ts = pm.m_NextMovementTime - DateTime.Now;

			if ( ts < TimeSpan.Zero )
			{
				// been a while since we've last moved
				pm.m_NextMovementTime = DateTime.Now;

                return true;
			}

            if (ts <= FastwalkThreshold)
            {
                return true;
            }
            else
            {
                return false; // this packet is being throttled
            }
		}
		#endregion

		#region Enemy of One
		private Type m_EnemyOfOneType;
		private bool m_WaitingForEnemy;

		public Type EnemyOfOneType
		{
			get{ return m_EnemyOfOneType; }
			set
			{
				Type oldType = m_EnemyOfOneType;
				Type newType = value;

				if ( oldType == newType )
					return;

				m_EnemyOfOneType = value;

				DeltaEnemies( oldType, newType );
			}
		}

		public bool WaitingForEnemy
		{
			get{ return m_WaitingForEnemy; }
			set{ m_WaitingForEnemy = value; }
		}

		private void DeltaEnemies( Type oldType, Type newType )
		{
			IPooledEnumerable eable = this.GetMobilesInRange( 18 );
			foreach ( Mobile m in eable)
			{
				Type t = m.GetType();

				if ( t == oldType || t == newType )
					Send( new MobileMoving( m, Notoriety.Compute( this, m ) ) );
			}
			eable.Free();
		}
		#endregion

		#region Hair and beard mods
		private int m_HairModID = -1, m_HairModHue;
		private int m_BeardModID = -1, m_BeardModHue;

		public void SetHairMods( int hairID, int beardID )
		{
			if ( hairID == -1 )
				InternalRestoreHair( true, ref m_HairModID, ref m_HairModHue );
			else if ( hairID != -2 )
				InternalChangeHair( true, hairID, ref m_HairModID, ref m_HairModHue );

			if ( beardID == -1 )
				InternalRestoreHair( false, ref m_BeardModID, ref m_BeardModHue );
			else if ( beardID != -2 )
				InternalChangeHair( false, beardID, ref m_BeardModID, ref m_BeardModHue );
		}

		private Item CreateHair( bool hair, int id, int hue )
		{
			if ( hair )
				return Server.Items.Hair.CreateByID( id, hue );
			else
				return Server.Items.Beard.CreateByID( id, hue );
		}

		private void InternalRestoreHair( bool hair, ref int id, ref int hue )
		{
			if ( id == -1 )
				return;

			Item item = FindItemOnLayer( hair ? Layer.Hair : Layer.FacialHair );

			if ( item != null )
				item.Delete();

			if ( id != 0 )
				AddItem( CreateHair( hair, id, hue ) );

			id = -1;
			hue = 0;
		}

		private void InternalChangeHair( bool hair, int id, ref int storeID, ref int storeHue )
		{
			Item item = FindItemOnLayer( hair ? Layer.Hair : Layer.FacialHair );

			if ( item != null )
			{
				if ( storeID == -1 )
				{
					storeID = item.ItemID;
					storeHue = item.Hue;
				}

				item.Delete();
			}
			else if ( storeID == -1 )
			{
				storeID = 0;
				storeHue = 0;
			}

			if ( id == 0 )
				return;

			AddItem( CreateHair( hair, id, 0 ) );
		}
		#endregion

		#region Virtue stuff
		private DateTime m_LastSacrificeGain;
		private DateTime m_LastSacrificeLoss;
		private int m_AvailableResurrects;

		public DateTime LastSacrificeGain{ get{ return m_LastSacrificeGain; } set{ m_LastSacrificeGain = value; } }
		public DateTime LastSacrificeLoss{ get{ return m_LastSacrificeLoss; } set{ m_LastSacrificeLoss = value; } }
		public int AvailableResurrects{ get{ return m_AvailableResurrects; } set{ m_AvailableResurrects = value; } }

		//private DateTime m_NextJustAward;
		private DateTime m_LastJusticeLoss;
		private ArrayList m_JusticeProtectors;

		public DateTime LastJusticeLoss{ get{ return m_LastJusticeLoss; } set{ m_LastJusticeLoss = value; } }
		public ArrayList JusticeProtectors{ get{ return m_JusticeProtectors; } set{ m_JusticeProtectors = value; } }

		private DateTime m_LastCompassionLoss;
		private DateTime m_NextCompassionDay;
		private int m_CompassionGains;

		public DateTime LastCompassionLoss{ get{ return m_LastCompassionLoss; } set{ m_LastCompassionLoss = value; } }
		public DateTime NextCompassionDay{ get{ return m_NextCompassionDay; } set{ m_NextCompassionDay = value; } }
		public int CompassionGains{ get{ return m_CompassionGains; } set{ m_CompassionGains = value; } }
		#endregion

		public override void OnSingleClick(Mobile from)
		{
			if ( Deleted )
				return;
			else if ( AccessLevel == AccessLevel.Player && DisableHiddenSelfClick && Hidden && from == this )
				return;

            if (Engines.IOBSystem.KinSystemSettings.ShowKinSingleClick)
            {
                if (this.Guild != null && (this.DisplayGuildTitle || this.Guild.Type != Guilds.GuildType.Regular))
                {
                    if (this.IOBAlignment != IOBAlignment.None)
                    {
                        string text = string.Format("[{0}]", Server.Engines.IOBSystem.IOBSystem.GetIOBName(this.IOBAlignment));
                        PrivateOverheadMessage(MessageType.Regular, SpeechHue, true, text, from.NetState);
                    }
                }
            }

            if (Engines.IOBSystem.KinSystemSettings.ShowStatloss)
            {
                if (this.IsInStatloss)
                {
                    PrivateOverheadMessage(MessageType.Regular, SpeechHue, true, "[STATLOSS]", from.NetState);
                }
            }

			base.OnSingleClick (from);
        }

        #region SkillCheck

        private const bool AntiMacroCode = false;		//Change this to false to disable anti-macro code

        private static TimeSpan AntiMacroExpire = TimeSpan.FromMinutes(5.0); //How long do we remember targets/locations?
        private const int Allowance = 3;	//How many times may we use the same location/target for gain
        private const int LocationSize = 5; //The size of eeach location, make this smaller so players dont have to move as far

        public static double GSGG = 0.0;
        
        private static bool[] UseAntiMacro = new bool[]
		{
			// true if this skill uses the anti-macro code, false if it does not
			false,// Alchemy = 0,
			false,// Anatomy = 1,
			false,// AnimalLore = 2,
			false,// ItemID = 3,
			false,// ArmsLore = 4,
			false,// Parry = 5,
			false,// Begging = 6,
			false,// Blacksmith = 7,
			false,// Fletching = 8,
			false,// Peacemaking = 9,
			false,// Camping = 10,
			false,// Carpentry = 11,
			false,// Cartography = 12,
			false,// Cooking = 13,
			false,// DetectHidden = 14,
			false,// Discordance = 15,
			false,// EvalInt = 16,
			false,// Healing = 17,
			false,// Fishing = 18,
			false,// Forensics = 19,
			false,// Herding = 20,
			false,// Hiding = 21,
			false,// Provocation = 22,
			false,// Inscribe = 23,
			false,// Lockpicking = 24,
			false,// Magery = 25,
			false,// MagicResist = 26,
			false,// Tactics = 27,
			false,// Snooping = 28,
			false,// Musicianship = 29,
			false,// Poisoning = 30,
			false,// Archery = 31,
			false,// SpiritSpeak = 32,
			false,// Stealing = 33,
			false,// Tailoring = 34,
			false,// AnimalTaming = 35,
			false,// TasteID = 36,
			false,// Tinkering = 37,
			false,// Tracking = 38,
			false,// Veterinary = 39,
			false,// Swords = 40,
			false,// Macing = 41,
			false,// Fencing = 42,
			false,// Wrestling = 43,
			false,// Lumberjacking = 44,
			false,// Mining = 45,
			false,// Meditation = 46,
			false,// Stealth = 47,
			false,// RemoveTrap = 48,
			false,// Necromancy = 49,
			false,// Focus = 50,
			false,// Chivalry = 51
		};
        
        protected override bool CheckSkill(Skill skill, object amObj, double chance)
        {
            if (skill == null)
                return false;

            LastSkillUsed = skill.SkillName;
            LastSkillTime = DateTime.Now;

            return base.CheckSkill(skill, amObj, chance);
        }

        protected override bool AllowGain(Skill skill, object obj)
        {
            if (AntiMacroCode && UseAntiMacro[skill.Info.SkillID] && !AntiMacroCheck(skill, obj))
                return false;
            
            DateTime lastgain = LastSkillGainTime[skill.SkillID];

            TimeSpan totalDesiredMinimum = TimeSpan.FromHours(GSGG);
            TimeSpan minTimeBetweenGains = new TimeSpan(0);

            if (skill.Base > 80.0 && skill.Base < 90.0)
                minTimeBetweenGains = TimeSpan.FromSeconds((totalDesiredMinimum.TotalSeconds / 4) / 100);
            else if (skill.Base >= 90.0 && skill.Base < 95.0)
                minTimeBetweenGains = TimeSpan.FromSeconds((totalDesiredMinimum.TotalSeconds / 4) / 50);
            else if (skill.Base >= 95.0 && skill.Base < 99.0)
                minTimeBetweenGains = TimeSpan.FromSeconds((totalDesiredMinimum.TotalSeconds / 4) / 40);
            else if (skill.Base >= 99.0)
                minTimeBetweenGains = TimeSpan.FromSeconds((totalDesiredMinimum.TotalSeconds / 4) / 10);
            else //skill is <= 80.0, ignore it
                minTimeBetweenGains = TimeSpan.FromSeconds(0.1);

            if (minTimeBetweenGains > (DateTime.Now - lastgain))
                return false;

            if (Region is Regions.Jail)
                return false;

            // check base here because we need to know it returns true to be able to set LastSkillGainTime
            if (!base.AllowGain(skill, obj))
                return false;
            LastSkillGainTime[skill.SkillID] = DateTime.Now;
            return true;
        }

        public static void GSGG_OnCommand(CommandEventArgs e)
        {
            try
            {
                if (e.Length == 0)
                {
                    e.Mobile.SendMessage("GSGG is " + GSGG + " hours.");
                }
                else
                {
                    string strParam = e.GetString(0);
                    double param = Double.Parse(strParam);
                    if (param < 0) param = 0.0;

                    e.Mobile.SendMessage("Setting GSGG to " + param + " hours.");
                    GSGG = param;
                }
            }
            catch (Exception exc)
            {
                LogHelper.LogException(exc);
                e.Mobile.SendMessage("There was a problem with the [gsgg command!!  See console log");
                System.Console.WriteLine("Error with [GSGG!");
                System.Console.WriteLine(exc.Message);
                System.Console.WriteLine(exc.StackTrace);
            }
        }

        #endregion


		#region Reverse Turing Test for AFK-checking
		private int m_RTTFailures = 0;
		private DateTime m_RTTNextTest = DateTime.MinValue;
		private double m_MinutesUntilNextTest = 5.0;
		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public int RTTFailures
		{
			get
			{
				return m_RTTFailures;
			}
            set
            {
                m_RTTFailures = value;
            }
		}
		public void RTTResult(bool passed)
		{
			if (passed)
			{
				m_RTTFailures = 0;
				if (m_MinutesUntilNextTest < 40.0)
				{	// Randomize to reduce predictability.
					m_MinutesUntilNextTest *= 2;
					m_MinutesUntilNextTest += Utility.RandomList(-3,-2,-1,1,2,3);
				}
			}
			else
			{	// Randomize to reduce predictability.
				m_MinutesUntilNextTest = 5.0;
				m_MinutesUntilNextTest += Utility.RandomList(0, 1, 2, 3);
			}

			//hard check to ensure max of 40.0
			if (m_MinutesUntilNextTest >= 40.0)
			{	// Randomize to reduce predictability.
				m_MinutesUntilNextTest = 40.0;
				m_MinutesUntilNextTest += Utility.RandomList(-13, -12, -11, 11, 12, 13); 
			}

			if (passed)
			{
				m_RTTNextTest = DateTime.Now.AddMinutes(m_MinutesUntilNextTest);
			}
		}
		public bool RTT(string notification)
		{
			return RTT(notification, false);
		}
        public bool RTT(string notification, bool bForced)
        {
            return RTT(notification, bForced, 0, "");
        }
		public bool RTT(string notification, bool bForced, int mode, string strSkillName)
		{
			bool bDoTest = false;

			if (m_RTTNextTest == DateTime.MinValue)
			{
				m_RTTNextTest = DateTime.Now.AddMinutes(5.0);
			}
			else if (DateTime.Now > m_RTTNextTest)
			{
				m_RTTNextTest = DateTime.Now.AddMinutes(5.0);
				bDoTest = true;
			}

			bool bReturn = (m_RTTFailures == 0);

			//Safety-hit to make sure the counter is reset with failures
			if (m_RTTFailures > 1) m_MinutesUntilNextTest = 5.0;

			if (m_RTTFailures > 10)
			{
				try
				{
					//10+ failures in a row, assume we've got an AFK macroer - auto [macroer him!
					PJUM.MacroerCommand.ReportAsMacroer(null, this);
				}
				catch (Exception exc)
				{
					Scripts.Commands.LogHelper.LogException(exc);
				}
			}

			if (bForced)
			{
				bDoTest = true;
				m_RTTNextTest = DateTime.Now.AddMinutes(m_MinutesUntilNextTest);
			}

			if (bDoTest)
			{
                switch (mode)
                {
                    case 2:
                        this.SendGump(new RTT.SmallPagedRTTGump(this, notification, strSkillName));
                        break;
                    default:
                        this.SendGump(new RTT.RTTGump(this, notification, strSkillName));
                        break;
                }
				m_RTTFailures++;
			}

			return bReturn;
		}

		#endregion
	}

    public class MovementReqCapture
    {
        private static bool m_Capturing = false;
        private static Dictionary<PlayerMobile, MemoryStream> m_Table = null;
        private static DateTime m_Started = DateTime.MinValue;
        private static int m_Count = 0;

        public static void Initialize()
        {
            Server.Commands.Register("[beginmrcapture", AccessLevel.Administrator, new CommandEventHandler(BeginMRCapture));
            Server.Commands.Register("[stopmrcapture", AccessLevel.Administrator, new CommandEventHandler(StopMRCapture));
            Server.Commands.Register("[mrcapturestatus", AccessLevel.GameMaster, new CommandEventHandler(MRCaptureStatus));
        }

        public static void BeginMRCapture(CommandEventArgs e)
        {
            m_Capturing = true;
            m_Table = new Dictionary<PlayerMobile,MemoryStream>();
            m_Started = DateTime.Now;
            m_Count = 0;

            Packet p = new AsciiMessage(Serial.MinusOne, -1, MessageType.Regular, 0, 3, "System", "Beginning MovementReq capture. Halting after 30 minutes or 1,000,000 hits.");
            foreach (NetState n in NetState.Instances)
            {
                if (n.Mobile != null && n.Mobile.AccessLevel >= AccessLevel.GameMaster)
                    n.Send(p);
            }
        }

        // Adam: may be called with CommandEventArgs == null
        public static void StopMRCapture(CommandEventArgs e)
        {
            try
            {
                using (FileStream fs = new FileStream("MRCapture.dat", FileMode.Create, FileAccess.Write))
                {
                    using (BinaryWriter writer = new BinaryWriter(fs))
                    {
                        foreach (PlayerMobile pm in m_Table.Keys)
                        {
                            writer.Write((string)pm.Name);
                            writer.Write((int)pm.Serial);
                            writer.Write((string)(((Account)pm.Account).Username));
                            writer.Write((long)m_Table[pm].Length);
                            writer.Write((byte[])m_Table[pm].ToArray());
                        }

                        writer.Close();
                    }

                    fs.Close();
                }

                if (e != null && e.Mobile != null)
                    e.Mobile.SendMessage("Capture data written to MRCapture.dat in the main directory.");
            }
            catch (Exception ex)
            {
                if (e != null && e.Mobile != null)
                    e.Mobile.SendMessage(ex.Message);
            }

            m_Capturing = false;
            m_Table = null;
            m_Started = DateTime.MinValue;
            m_Count = 0;

            Packet p = new AsciiMessage(Serial.MinusOne, -1, MessageType.Regular, 0, 3, "System", "Ended MovementReq capture.");
            foreach (NetState n in NetState.Instances)
            {
                if (n.Mobile != null && n.Mobile.AccessLevel >= AccessLevel.GameMaster)
                    n.Send(p);
            }
        }

        public static void MRCaptureStatus(CommandEventArgs e)
        {
            if (m_Capturing)
                e.Mobile.SendMessage("MR Capture has been running for {0} minutes, with {1} hits.", (DateTime.Now - m_Started).Minutes, m_Count);
            else
                e.Mobile.SendMessage("MR Capture not running.");
        }

        public static void HitMR(PlayerMobile pm)
        {
            if (!m_Capturing)
                return;

            if (m_Started + TimeSpan.FromMinutes(30.0) <= DateTime.Now)
            {
                StopMRCapture(null);
                return;
            }

            if (!m_Table.ContainsKey(pm))
                m_Table.Add(pm, new MemoryStream());
            
            m_Table[pm].Write(BitConverter.GetBytes(DateTime.Now.ToBinary()), 0, 8);
            m_Count++;
        }
    }
}
