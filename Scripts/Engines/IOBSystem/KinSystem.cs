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

/* Scripts/Engines/IOBSystem/KinSystem.cs
 * CHANGELOG:
 *	06/29/09, plasma
 *		Added some overloads for broadcasting
 *	05/25/09, plasma
 *		- Prevent power point transfer if target is in stat
 *	04/27/09, plasma	
 *		- Added GetCityGuardPostSlots reflection method for new KinFactionCityAttribute
 *		- Changed guard reflection methods to return the new custom hire/maint/slot values if provided as priority.
 *	01/25/09, plasma
 *		Added some global Get methods guard information
 *  12/08/08, plasma
 *			Moved speech handler into Keywords.cs
 *  10/02/08, plasma
 *			Add some output to the player indicating they've
 *			earnt some defense capture points
 *	02/24/08, plasma
 *			Add awarding of defense capture points
 *  12/3/07, Pix
 *      Added IOBAlignement.Healer for kin-healers instead of overloading Outcast
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Regions;
using Server.Engines.IOBSystem.Attributes;

namespace Server.Engines.IOBSystem
{

	class KinSystem
	{

		#region Misc global methods

		#region broadcasting

		public enum BroadcastOptions
		{
			IncludeKin,
			ExcludeKin
		}

		public static void SendKinMessage(string message)
		{
			if (string.IsNullOrEmpty(message)) return;

			foreach (Mobile msg in World.Mobiles.Values)
			{
				if (msg != null && msg is PlayerMobile)
				{
					PlayerMobile pm = (PlayerMobile)msg;
					if (pm.IOBRealAlignment != IOBAlignment.None)
					{
						pm.SendMessage(message);
					}
				}
			}
		}

		public static void SendKinMessage(BroadcastOptions kinMode, IOBAlignment[] aligns, string message)
		{
			if (kinMode == BroadcastOptions.IncludeKin)
			{
				//just forward this request to the overload
				SendKinMessage(aligns, message);
				return;
			}

			//build list of kins to send to
			List<IOBAlignment> kin = new List<IOBAlignment>();

			//add all valid kins first
			foreach (IOBAlignment align in Enum.GetValues(typeof(IOBAlignment)))
				if (align == IOBAlignment.None || align == IOBAlignment.OutCast || align == IOBAlignment.Healer) continue;
				else kin.Add(align);

			//then remove the excluded ones (just easier this way, where the hell is LINQ when you need it ? :< )
			for (int i = kin.Count - 1; i >= 0;  i--)
				foreach (IOBAlignment align in aligns)
					if (kin[i] == align)
						kin.RemoveAt(i);

			SendKinMessage(kin.ToArray(), message);

		}

		public static void SendKinMessage(IOBAlignment align, string message)
		{
			SendKinMessage(new IOBAlignment[] { align }, message);
		}

		private static void SendKinMessage(IOBAlignment[] aligns, string message)
		{
			if (string.IsNullOrEmpty(message))
				return;

			foreach (Mobile msg in World.Mobiles.Values)
			{
				if (msg != null && msg is PlayerMobile)
				{
					PlayerMobile pm = (PlayerMobile)msg;
					foreach( IOBAlignment align in aligns )
						if (pm.IOBRealAlignment == align)
						{
							pm.SendMessage(message);
							break;
						}
				}
			}
		}

		#endregion

		#region guard reflection stuff

		/// <summary>
		/// Returns value from the Description attribute of the enum value, or ToString() if one doesn't exist
		/// NOTE: This isn't really factions sepecific!
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static string GetEnumTypeDescription<T>(T type)
		{
			Type t = typeof(T);

			if (t.IsEnum)
			{
				FieldInfo fi = t.GetField(type.ToString());
				foreach (DescriptionAttribute da in fi.GetCustomAttributes(typeof(DescriptionAttribute), true))
					return da.Description;
			}

			return type.ToString();
		}

		/// <summary>
		/// Returns silver maintenance cost based on guard type using reflection
		/// </summary>
		/// <returns></returns>
		public static int GetGuardMaintCost(KinFactionGuardTypes guardType)
		{
			//Default to medium cost
			int cost = KinSystemSettings.GuardTypeMediumSilverCost;
			Type t = SpawnerType.GetType(guardType.ToString());
			if (t == null) return cost;
			foreach (KinFactionGuardTypeAttribute att in t.GetCustomAttributes(typeof(KinFactionGuardTypeAttribute), true))
			{
				//Although foreach is used here, there will only be one of these attributes.
				//Use the custom value if it was provided
				if (att.CustomMaintCost > 0)
				{
					cost = att.CustomMaintCost;
				}
				else
				{
					switch (att.GuardCostType)
					{
						case KinFactionGuardCostTypes.LowCost: cost = KinSystemSettings.GuardTypeLowSilverMaintCost; break;
						case KinFactionGuardCostTypes.MediumCost: cost = KinSystemSettings.GuardTypeMediumSilverMaintCost; break;
						case KinFactionGuardCostTypes.HighCost: cost = KinSystemSettings.GuardTypeHighSilverMaintCost; break;
					}
				}
			}
			return cost;
		}

		/// <summary>
		/// Returns silver guard hire cost based on guard type using reflection
		/// </summary>
		/// <returns></returns>
		public static int GetGuardHireCost(KinFactionGuardTypes guardType)
		{
			//Default to medium cost
			int cost = KinSystemSettings.GuardTypeMediumSilverCost;
			Type t = SpawnerType.GetType(guardType.ToString());
			if (t == null) return cost;
			foreach (KinFactionGuardTypeAttribute att in t.GetCustomAttributes(typeof(KinFactionGuardTypeAttribute), true))
			{	 //Although foreach is used here, there will only be one of these attributes.
				//Use the custom value if it was provided
				if (att.CustomHireCost > 0)
				{
					cost = att.CustomHireCost;
				}
				else
				{
					switch (att.GuardCostType)
					{
						case KinFactionGuardCostTypes.LowCost: cost = KinSystemSettings.GuardTypeLowSilverCost; break;
						case KinFactionGuardCostTypes.MediumCost: cost = KinSystemSettings.GuardTypeMediumSilverCost; break;
						case KinFactionGuardCostTypes.HighCost: cost = KinSystemSettings.GuardTypeHighSilverCost; break;
					}
				}
			}
			return cost;
		}

		/// <summary>
		/// Returns cost type of the guard using reflection, or defaults to Medium. This is the amount of slots required for the guard.
		/// </summary>
		/// <returns></returns>
		public static int GetGuardCostType(KinFactionGuardTypes guardType)
		{
			//Default to medium cost
			int cost = (int)KinFactionGuardCostTypes.MediumCost;
			Type t = SpawnerType.GetType(guardType.ToString());
			if (t == null) return cost;
			//Although foreach is used here, there will only be one of these attributes.
			foreach (KinFactionGuardTypeAttribute att in t.GetCustomAttributes(typeof(KinFactionGuardTypeAttribute), true))
			{
				//Use the custom value if it was provided
				if (att.CustomSlotCost > 0)
				{
					cost = att.CustomSlotCost;
				}
				else
				{
					cost = (int)att.GuardCostType;
				}
			}
			return cost;
		}

		/// <summary>
		/// Returns list of guard types a kin is eligible to hire
		/// </summary>
		/// <param name="alignment"></param>
		/// <returns></returns>
		public static List<KinFactionGuardTypes> GetEligibleGuardTypes(IOBAlignment alignment)
		{
			List<KinFactionGuardTypes> results = new List<KinFactionGuardTypes>();
			foreach (KinFactionGuardTypes guardType in Enum.GetValues(typeof(KinFactionGuardTypes)))
			{
				try
				{
					//See if this type exists and has a KinFactionGuardTypeAttribute
					Type t = SpawnerType.GetType(guardType.ToString().ToLower());
					if (t != null)
					{
						bool addToList = true;
						KinFactionGuardTypeAttribute[] att = (KinFactionGuardTypeAttribute[])t.GetCustomAttributes(typeof(KinFactionGuardTypeAttribute), true);

						//Note here that if there isn't an attribute, all kins get added 
						if (att != null && att.Length > 0 && att[0].EligibleKin != null && att[0].EligibleKin.Count > 0)
						{
							if (att[0].EligibleKin != null && !att[0].EligibleKin.Contains(alignment))
							{
								addToList = false;
							}
						}
						if (addToList)
						{
							results.Add(guardType);
						}
					}
				}
				catch
				{


				}
			}
			return results;
		}

		#endregion

		#region city reflection stuff

		/// <summary>
		/// Returns the amount of slots assigned to this city when ownership transfers
		/// </summary>
		/// <returns></returns>
		public static int GetCityGuardPostSlots(KinFactionCities city)
		{
			int slots = KinSystemSettings.CityGuardSlots;

			//Default 
			Type t = typeof(KinFactionCities);

			FieldInfo fi = t.GetField(city.ToString());
			foreach (KinFactionCityAttribute att in fi.GetCustomAttributes(typeof(KinFactionCityAttribute), true))
				if (att.GuardSlots > 0)
					slots = att.GuardSlots;

			return slots;
		}

		#endregion

		#endregion

		#region Apply Statloss

		public static void RefreshStatloss(PlayerMobile pm, bool regular)
		{
			if (KinSystemSettings.StatLossEnabled)
			{
				if (regular)
				{
					pm.RemoveStatlossSkillMods();
					for (int i = 0; i < 49; i++)
					{
						pm.AddSkillMod(new KinStatlossSkillMod((SkillName)i, pm));
					}
				}
				else
				{
					pm.RemoveStatlossSkillMods();
					for (int i = 0; i < 49; i++)
					{
						pm.AddSkillMod(new KinHealerStatlossSkillMod((SkillName)i, pm));
					}
				}
			}
		}

		#endregion

		#region Get Damage Spread

		public static void GetDamageSpread(PlayerMobile pm, ref double damageByFactioners, ref double damageBySameFaction, ref double damageByOthers)
		{
			foreach (DamageEntry de in pm.DamageEntries)
			{
				if (de.HasExpired)
				{
					continue;
				}

				if (de.Damager is PlayerMobile)
				{
					if (((PlayerMobile)de.Damager).IsRealFactioner)
					{
						if (((PlayerMobile)de.Damager).IOBAlignment != pm.IOBAlignment)
						{
							damageByFactioners += de.DamageGiven;
						}
						else
						{
							damageBySameFaction += de.DamageGiven;
						}
					}
					else
					{
						damageByOthers += de.DamageGiven;
					}
				}
				else if (de.Damager is BaseCreature)
				{
					if (((BaseCreature)de.Damager).IOBAlignment != IOBAlignment.None)
					{
						damageByFactioners += de.DamageGiven;
					}
					else
					{
						damageByOthers += de.DamageGiven;
					}
				}
				else
				{
					//*shrugs*
				}
			}
		}

		#endregion

		#region Do Power Point Transfer

		public static void AwardPowerPoints(PlayerMobile pm)
		{
			//plasma: no power points yet
			return;
			if (KinSystemSettings.PointsEnabled)
			{
				if (pm.LastKiller is PlayerMobile)
				{
					PlayerMobile lastkiller = pm.LastKiller as PlayerMobile;
					if (lastkiller.IsRealFactioner &&
							lastkiller.IOBRealAlignment != pm.IOBRealAlignment)
					{
						double loserPowerPts = pm.KinPowerPoints;
						double PPLost = loserPowerPts * 0.5;
						double PPGained = PPLost * 0.5;
						pm.KinPowerPoints -= PPLost;
						lastkiller.KinPowerPoints += PPGained;
						if (PPGained > 0)
						{
							lastkiller.SendMessage("You have gained {0:0.00} power points for slaying your enemy.", PPGained);
						}
					}
				}
			}
		}

		#endregion

		public static void OnDeath(PlayerMobile pm)
		{
			//time-saver - if we're not pointing or statlossing, just leave.
			if (KinSystemSettings.PointsEnabled == false
					|| KinSystemSettings.StatLossEnabled == false)
			{ return; }

			if (pm.IsRealFactioner)
			{
				//Plasma: don't give points if we're in stat
				if (!pm.IsInStatloss)
				{
					AwardPowerPoints(pm);
				}

				double damageByFactioners = 0.0;
				double damageBySameFaction = 0.0;
				double damageByOthers = 0.0;

				GetDamageSpread(pm, ref damageByFactioners, ref damageBySameFaction, ref damageByOthers);

				if (pm.IsInStatloss == false)
				{
					if (KinSystemSettings.PointsEnabled
							&& damageByFactioners > damageByOthers)
					{
						object winner = GetMostDamager(pm);
						AwardPoints(winner, pm);
					}
				}

				if (damageByFactioners > damageByOthers)
				{
					RefreshStatloss(pm, true);
				}
			}
			else if (pm.IOBAlignment == IOBAlignment.OutCast ||
							 pm.IOBAlignment == IOBAlignment.Healer)
			{
				if (pm.KinBeneficialTime > DateTime.Now)
				{
					double damageByFactioners = 0.0;
					double damageByOthers = 0.0;
					double damageBySameFaction = 0.0;

					GetDamageSpread(pm, ref damageByFactioners, ref damageBySameFaction, ref damageByOthers);

					if ((damageByFactioners + damageBySameFaction) > damageByOthers)
					{
						RefreshStatloss(pm, false);
					}
				}
			}
		}

		#region Award Points

		public static void AwardPoints(object winner, PlayerMobile pm)
		{
			if (winner != null)
			{
				if (winner is PlayerMobile)
				{
					PlayerMobile pmWinner = (PlayerMobile)winner;

					int aggressorCount = 0;
					foreach (AggressorInfo ai in pm.Aggressors)
					{
						if (ai.Expired == false)
						{
							aggressorCount++;
						}
					}

					if (aggressorCount == 1)
					{
						//winner killed loser solo
						double loserPts = pm.KinSoloPoints;

						if (loserPts > -5.0)
						{
							double toAward = loserPts * 0.10;
							if (toAward < 1.0) toAward = 1.0;

							pm.KinSoloPoints -= toAward;
							pmWinner.KinSoloPoints += toAward;

							pmWinner.SendMessage("You've earned {0:0.00} individual points for slaying your enemy unassisted.", toAward);

							//Don't award defense capture points for killing players under 400 skill
							if (pm.SkillsTotal > 400)
							{
								//see if the loser is in a capture zone
								if (KinCityManager.InCaptureArea(pm))
								{
									//find out if the loser was within range of any power vortexes
									foreach (PowerVortex pv in KinCityManager.GetVortexesInRange(pm))
									{
										//Apply 1 defense capture point to the vortex's sigil
										pv.Sigil.AddDefensePoints(pmWinner, 1.0);
										pmWinner.SendMessage("You've earned 1 defense capture point for slaying your enemy unassisted.");
										if (pv.Sigil.RemoveDefensePoints(pm, 1.0))
											pm.SendMessage("You've lost a defense capture point.");
									}
								}
							}
						}
						else
						{
							pmWinner.SendMessage("You gain no individual points from vanquishing that feeble foe.");
						}
					}
					else
					{
						//winner had some help
						double loserPts = pm.KinTeamPoints;

						if (loserPts > -5.0)
						{
							double toAward = loserPts * 0.10;
							if (toAward < 1.0) toAward = 1.0;

							pm.KinTeamPoints -= toAward;
							pmWinner.KinTeamPoints += toAward;

							pmWinner.SendMessage("You've earned {0:0.00} team points for slaying your enemy.", toAward);

							//Don't award defense capture points for killing players under 400 skill
							if (pm.SkillsTotal > 400)
							{
								if (KinCityManager.InCaptureArea(pm))
								{
									//Find out if the loser was within range of any power vortexes
									foreach (PowerVortex pv in KinCityManager.GetVortexesInRange(pm))
									{
										pv.Sigil.AddDefensePoints(pmWinner, 1.0);
										pmWinner.SendMessage("You've earned 1 defense capture point for slaying your enemy.");
										if (pv.Sigil.RemoveDefensePoints(pm, 1.0))
											pm.SendMessage("You've lost a defense capture point.");
									}
								}
							}

						}
						else
						{
							pmWinner.SendMessage("You gain no team points from vanquishing that feeble foe.");
						}
					}
				}
				else
				{
					//winner is party
					if (winner is Server.Engines.PartySystem.Party)
					{
						Server.Engines.PartySystem.Party party = (Server.Engines.PartySystem.Party)winner;

						double loserPts = pm.KinTeamPoints;
						double toAward = 0.0;
						if (loserPts > -5.0)
						{
							toAward = loserPts * 0.10;
							if (toAward < 1.0) toAward = 1.0;
						}

						double toAwardEach = toAward / party.Members.Count;

						foreach (Server.Engines.PartySystem.PartyMemberInfo pmi in party.Members)
						{
							if (pmi != null)
							{
								if (pmi.Mobile != null)
								{
									if (pmi.Mobile is PlayerMobile)
									{
										if (toAward > 0)
										{
											((PlayerMobile)pmi.Mobile).KinTeamPoints += toAwardEach;
											((PlayerMobile)pmi.Mobile).SendMessage("You've earned {0:0.00} points for your party slaying your enemy.", toAwardEach);

											//Don't award defense capture points for killing players under 400 skill
											if (pm.SkillsTotal > 400)
											{
												if (KinCityManager.InCaptureArea(pm))
												{
													double defensePoints = (1 / party.Members.Count);
													//find out if they are within range of any power vortexes
													foreach (PowerVortex pv in KinCityManager.GetVortexesInRange(pm))
													{
														//Only ever give out 1 capture point per kill
														pv.Sigil.AddDefensePoints((pmi.Mobile as PlayerMobile), defensePoints);
														pmi.Mobile.SendMessage("You've earned {0:0.00} of a defense capture point for slaying your enemy assisted.", defensePoints);
														if (pv.Sigil.RemoveDefensePoints(pm, defensePoints))
															pm.SendMessage(string.Format("You've lost {0:0.00} of a defense capture point.", defensePoints));
													}
												}
											}

										}
										else
										{
											((PlayerMobile)pmi.Mobile).SendMessage("You gain nothing from vanquishing that feeble foe.");
										}
									}
								}
							}
						}
					}
				}
			}
		}

		#endregion

		#region Get Most Damager Functions (PM and BC)

		public static object GetMostDamager(PlayerMobile pm)
		{
			System.Collections.Hashtable ht_Groups = new System.Collections.Hashtable();

			//need to find out who to assign the points to
			foreach (AggressorInfo ai in pm.Aggressors)
			{
				if (ai.Attacker is PlayerMobile)
				{
					int totalPoints = 0;
					foreach (DamageEntry de in pm.DamageEntries)
					{
						if (!de.HasExpired && de.Damager == ai.Attacker)
						{
							totalPoints += de.DamageGiven;
						}
					}

					if (ai.Attacker.Party == null)
					{
						ht_Groups.Add(ai.Attacker, totalPoints);
					}
					else
					{
						if (ht_Groups.Contains(ai.Attacker.Party))
						{
							int prev = (int)ht_Groups[ai.Attacker.Party];
							ht_Groups[ai.Attacker.Party] = prev + totalPoints;
						}
						else
						{
							ht_Groups.Add(ai.Attacker.Party, totalPoints);
						}
					}
				}
			}

			object winner = null;
			//decide who gets the points
			foreach (object key in ht_Groups.Keys)
			{
				if (winner == null)
				{
					winner = key;
				}
				else
				{
					int winTotal = (int)ht_Groups[winner];
					int thisTotal = (int)ht_Groups[key];
					if (thisTotal > winTotal)
					{
						winner = key;
					}
				}
			}

			return winner;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="bc"></param>
		/// <returns></returns>
		public static object GetMostDamager(BaseCreature bc)
		{
			try
			{
				double damageByFactioners = 0.0;
				double damageBySameFaction = 0.0;
				double damageByOthers = 0.0;

				foreach (DamageEntry de in bc.DamageEntries)
				{
					if (de.HasExpired)
					{
						continue;
					}

					if (de.Damager is PlayerMobile)
					{
						if (((PlayerMobile)de.Damager).IsRealFactioner)
						{
							if (((PlayerMobile)de.Damager).IOBAlignment != bc.IOBAlignment)
							{
								damageByFactioners += de.DamageGiven;
							}
							else
							{
								damageBySameFaction += de.DamageGiven;
							}
						}
						else
						{
							damageByOthers += de.DamageGiven;
						}
					}
					else if (de.Damager is BaseCreature)
					{
						if (((BaseCreature)de.Damager).IOBAlignment != IOBAlignment.None)
						{
							damageByFactioners += de.DamageGiven;
						}
						else
						{
							damageByOthers += de.DamageGiven;
						}
					}
					else
					{
						//*shrugs*
					}
				}


				if (KinSystemSettings.PointsEnabled && damageByFactioners > damageByOthers)
				{
					System.Collections.Hashtable ht_Groups = new System.Collections.Hashtable();

					//need to find out who to assign the points to
					foreach (AggressorInfo ai in bc.Aggressors)
					{
						if (ai.Attacker is PlayerMobile)
						{
							int totalPoints = 0;
							foreach (DamageEntry de in bc.DamageEntries)
							{
								if (!de.HasExpired && de.Damager == ai.Attacker)
								{
									totalPoints += de.DamageGiven;
								}
							}

							if (ai.Attacker.Party == null)
							{
								ht_Groups.Add(ai.Attacker, totalPoints);
							}
							else
							{
								if (ht_Groups.Contains(ai.Attacker.Party))
								{
									int prev = (int)ht_Groups[ai.Attacker.Party];
									ht_Groups[ai.Attacker.Party] = prev + totalPoints;
								}
								else
								{
									ht_Groups.Add(ai.Attacker.Party, totalPoints);
								}
							}
						}
					}

					object winner = null;
					//decide who gets the points
					foreach (object key in ht_Groups.Keys)
					{
						if (winner == null)
						{
							winner = key;
						}
						else
						{
							int winTotal = (int)ht_Groups[winner];
							int thisTotal = (int)ht_Groups[key];
							if (thisTotal > winTotal)
							{
								winner = key;
							}
						}
					}

					return winner;
				}
			}
			catch (Exception e)
			{
				Scripts.Commands.LogHelper.LogException(e);
			}

			//we get here if we aren't set to use points
			return null;
		}

		#endregion

	}

	#region KinStatlossSkillMod class
	public class KinStatlossSkillMod : TimedSkillMod
	{
		public KinStatlossSkillMod(SkillName skill, PlayerMobile p )
			: base(skill, true, p.Skills[skill].Base * KinSystemSettings.StatLossPercentageSkills * -1, DateTime.Now.AddMinutes(KinSystemSettings.StatLossDurationMinutes))
		{
		}
	}

	public class KinHealerStatlossSkillMod : TimedSkillMod
	{
		public KinHealerStatlossSkillMod(SkillName skill, PlayerMobile p )
			: base(skill, true, p.Skills[skill].Base * KinSystemSettings.StatLossPercentageSkills * -1 * KinSystemSettings.KinHealerModifier, DateTime.Now.AddMinutes(KinSystemSettings.StatLossDurationMinutes))
		{
		}
	}
	#endregion
}
