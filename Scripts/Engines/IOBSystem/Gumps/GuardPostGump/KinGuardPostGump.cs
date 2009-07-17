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
 *			March 27, 2007
 */

/* Scripts/Engines/IOBSystem/Gumps/KinGuardPostGump.cs
 * CHANGELOG:
 *	02/10/08 - Plasma,
 *		Initial creation
 */

using System;
using System.Collections.Generic;
using System.Text;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Gumps;
using Server.Engines.IOBSystem;
using Server.Engines.IOBSystem.Gumps.GuardPostGump;
using Server.Engines.CommitGump;

namespace Server.Engines.IOBSystem.Gumps.GuardPostGump
{
	/// <summary>
	///
	/// </summary>
	public partial class KinGuardPostGump : CommitGumpBase
	{

		public KinGuardPostGump(KinGuardPost guardPost, Mobile from)
			: this(guardPost , 1, from)
		{
		}


		public KinGuardPostGump(KinGuardPost guardPost, int page, Mobile from)
			: this(guardPost, page, null, from)
		{
		}

		public KinGuardPostGump(int page, GumpSession session, Mobile from)
			: this(null, page, session, from)
		{

		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="city"></param>
		/// <param name="page"></param>
		/// <param name="session"></param>
		private KinGuardPostGump(KinGuardPost guardPost, int page, GumpSession session, Mobile from)
			: base(page, session, from)
		{
			if (Session["GuardPost"] == null)
			{
				Session["GuardPost"] = guardPost;
				SetCurrentPage();
				if (CurrentPage != null) CurrentPage.Create();
			}
			
		}

		protected override void RegisterEntities()
		{
			m_EntityRegister.Add(0, typeof(PageMain));
		}

		protected override void SetCurrentPage()
		{
			//Create whatever other page currently selected
			switch (Page)
			{
				case 1:
					{		 
						CurrentPage = new PageMain(this);
						break;
					}
			}
		}
	}

}



