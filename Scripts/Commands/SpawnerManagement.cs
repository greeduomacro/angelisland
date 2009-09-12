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

/* Scripts/Commands/SpawnerManagement.cs
 * CHANGELOG:
 *  6/29/06, Kit
 *		updated save function to work with spawner templates, do not save mobile/item template data.
 *	6/25/06: Kit
 *		Initial Version
 */

using System;
using System.IO;
using System.Collections;
using System.Reflection;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Gumps;
using Server.Prompts;
using Server.Targeting;
using Server.Misc;
using Server.Multis;

namespace Server.Scripts.Commands
{
	public class SaveSpawners
	{
		public static void Usage(Mobile to)
		{
			to.SendMessage("Usage: SaveSpawners[<FileName> <X1> <Y1> <X2> <Y2>");
		}

		public static void Initialize()
		{
			Server.Commands.Register("SaveSpawners", AccessLevel.Administrator, new CommandEventHandler(SaveSpawners_OnCommand));
		}

		public static void CopyProperties(Item dest, Item src)
		{
			PropertyInfo[] props = src.GetType().GetProperties();

			for (int i = 0; i < props.Length; i++)
			{
				try
				{
					if (props[i].CanRead && props[i].CanWrite)
					{
						props[i].SetValue(dest, props[i].GetValue(src, null), null);
					}
				}
				catch
				{

				}
			}
		}

		[Usage("SaveSpawners")]
		[Description("Saves All spawners in designated X/Y to specificed file name.")]
		public static void SaveSpawners_OnCommand(CommandEventArgs e)
		{

			if (e.Arguments.Length == 5)
			{
				int count = 0;
				int x1, y1, x2, y2;
				string FileName = e.Arguments[0].ToString();
				try
				{
					x1 = Int32.Parse(e.Arguments[1]);
					y1 = Int32.Parse(e.Arguments[2]);
					x2 = Int32.Parse(e.Arguments[3]);
					y2 = Int32.Parse(e.Arguments[4]);
				}
				catch
				{
					Usage(e.Mobile);
					return;
				}
				//adjust rect				
				if (x1 > x2)
				{
					int x3 = x1;
					x1 = x2;
					x2 = x3;
				}
				if (y1 > y2)
				{
					int y3 = y1;
					y1 = y2;
					y2 = y3;
				}
				string itemIdxPath = Path.Combine("Saves/Spawners/", FileName + ".idx");
				string itemBinPath = Path.Combine("Saves/Spawners/", FileName + ".bin");

				try
				{
					ArrayList list = new ArrayList();
					foreach (Item item in Server.World.Items.Values)
					{
						if (item is Spawner)
						{
							if (item.X >= x1 && item.Y >= y1 && item.X < x2 && item.Y < y2 && item.Map == e.Mobile.Map)
								list.Add(item);
						}
					}

					if (list.Count > 0)
					{
						try
						{
							string folder = Path.GetDirectoryName(itemIdxPath);

							if (!Directory.Exists(folder))
							{
								Directory.CreateDirectory(folder);
							}

						}
						catch
						{
							e.Mobile.SendMessage("An error occured while trying to create Spawner folder.");
						}

						count = list.Count;
						GenericWriter idx;
						GenericWriter bin;

						idx = new BinaryFileWriter(itemIdxPath, false);
						bin = new BinaryFileWriter(itemBinPath, true);

						idx.Write((int)list.Count);

						for (int i = 0; i < list.Count; ++i)
						{
							long start = bin.Position;
							Spawner temp = new Spawner();
							CopyProperties(temp, (Spawner)list[i]);

							idx.Write((long)start);
							//dont save template data as we cant load it back properly
							temp.TemplateItem = null;
							temp.TemplateMobile = null;
							temp.CreaturesName = ((Spawner)list[i]).CreaturesName;
							temp.Serialize(bin);

							idx.Write((int)(bin.Position - start));
							temp.Delete();
						}
						idx.Close();
						bin.Close();
					}
				}
				catch (Exception ex)
				{
					LogHelper.LogException(ex);
					System.Console.WriteLine("Exception Caught in SaveSpawner code: " + ex.Message);
					System.Console.WriteLine(ex.StackTrace);
				}

				e.Mobile.SendMessage("{0} Spawners Saved.", count);
			}
			else
			{
				Usage(e.Mobile);
			}
		}

	}

	public class WipeSpawners
	{
		public static void Usage(Mobile to)
		{
			to.SendMessage("Usage: WipeSpawners[<X1> <Y1> <X2> <Y2>");
		}

		public static void Initialize()
		{
			Server.Commands.Register("WipeSpawners", AccessLevel.Administrator, new CommandEventHandler(WipeSpawners_OnCommand));
		}

		[Usage("WipeSpawners")]
		[Description("Wipes All spawners in designated X/Y area.")]
		public static void WipeSpawners_OnCommand(CommandEventArgs e)
		{
			if (e.Arguments.Length == 4)
			{
				int count = 0;
				int x1, y1, x2, y2;
				try
				{
					x1 = Int32.Parse(e.Arguments[1]);
					y1 = Int32.Parse(e.Arguments[2]);
					x2 = Int32.Parse(e.Arguments[3]);
					y2 = Int32.Parse(e.Arguments[4]);
				}
				catch
				{
					Usage(e.Mobile);
					return;
				}
				//adjust rect				
				if (x1 > x2)
				{
					int x3 = x1;
					x1 = x2;
					x2 = x3;
				}
				if (y1 > y2)
				{
					int y3 = y1;
					y1 = y2;
					y2 = y3;
				}

				try
				{
					ArrayList list = new ArrayList();
					foreach (Item item in Server.World.Items.Values)
					{
						if (item is Spawner)
						{
							if (item.X >= x1 && item.Y >= y1 && item.X < x2 && item.Y < y2 && item.Map == e.Mobile.Map)
								list.Add(item);
						}
					}

					if (list.Count > 0)
					{

						count = list.Count;
						for (int i = 0; i < list.Count; ++i)
						{
							((Item)list[i]).Delete();
						}
					}
				}
				catch (Exception ex)
				{
					LogHelper.LogException(ex);
					System.Console.WriteLine("Exception Caught in WipeSpawner code: " + ex.Message);
					System.Console.WriteLine(ex.StackTrace);
				}

				e.Mobile.SendMessage("{0} Spawners Deleted.", count);
			}
			else
			{
				Usage(e.Mobile);
			}
		}

	}

	public class ActivateSpawners
	{
		public static void Usage(Mobile to)
		{
			to.SendMessage("Usage: ActivateSpawners[<X1> <Y1> <X2> <Y2>");
		}

		public static void Initialize()
		{
			Server.Commands.Register("ActivateSpawners", AccessLevel.Administrator, new CommandEventHandler(ActivateSpawners_OnCommand));
		}

		[Usage("ActivateSpawners")]
		[Description("Activates All spawners in designated X/Y area.")]
		public static void ActivateSpawners_OnCommand(CommandEventArgs e)
		{
			if (e.Arguments.Length == 4)
			{
				int count = 0;
				int x1, y1, x2, y2;
				try
				{
					x1 = Int32.Parse(e.Arguments[0]);
					y1 = Int32.Parse(e.Arguments[1]);
					x2 = Int32.Parse(e.Arguments[2]);
					y2 = Int32.Parse(e.Arguments[3]);
				}
				catch
				{
					Usage(e.Mobile);
					return;
				}
				//adjust rect				
				if (x1 > x2)
				{
					int x3 = x1;
					x1 = x2;
					x2 = x3;
				}
				if (y1 > y2)
				{
					int y3 = y1;
					y1 = y2;
					y2 = y3;
				}

				try
				{
					ArrayList list = new ArrayList();
					foreach (Item item in Server.World.Items.Values)
					{
						if (item is Spawner)
						{
							if (item.X >= x1 && item.Y >= y1 && item.X < x2 && item.Y < y2 && item.Map == e.Mobile.Map)
								list.Add(item);
						}
					}

					if (list.Count > 0)
					{

						count = list.Count;
						for (int i = 0; i < list.Count; ++i)
						{
							((Spawner)list[i]).Running = true;
							((Spawner)list[i]).Respawn();
						}
					}
				}
				catch (Exception ex)
				{
					LogHelper.LogException(ex);
					System.Console.WriteLine("Exception Caught in ActivateSpawner code: " + ex.Message);
					System.Console.WriteLine(ex.StackTrace);
				}

				e.Mobile.SendMessage("{0} Spawners Activated.", count);
			}
			else
			{
				Usage(e.Mobile);
			}
		}

	}

	public class DeactivateSpawners
	{
		public static void Usage(Mobile to)
		{
			to.SendMessage("Usage: DeactivateSpawners[<X1> <Y1> <X2> <Y2>");
		}

		public static void Initialize()
		{
			Server.Commands.Register("DeactivateSpawners", AccessLevel.Administrator, new CommandEventHandler(DeactivateSpawners_OnCommand));
		}

		[Usage("DeactivateSpawners")]
		[Description("Deactivates All spawners in designated X/Y area.")]
		public static void DeactivateSpawners_OnCommand(CommandEventArgs e)
		{
			if (e.Arguments.Length == 4)
			{
				int count = 0;
				int x1, y1, x2, y2;
				try
				{
					x1 = Int32.Parse(e.Arguments[0]);
					y1 = Int32.Parse(e.Arguments[1]);
					x2 = Int32.Parse(e.Arguments[2]);
					y2 = Int32.Parse(e.Arguments[3]);
				}
				catch
				{
					Usage(e.Mobile);
					return;
				}
				//adjust rect				
				if (x1 > x2)
				{
					int x3 = x1;
					x1 = x2;
					x2 = x3;
				}
				if (y1 > y2)
				{
					int y3 = y1;
					y1 = y2;
					y2 = y3;
				}

				try
				{
					ArrayList list = new ArrayList();
					foreach (Item item in Server.World.Items.Values)
					{
						if (item is Spawner)
						{
							if (item.X >= x1 && item.Y >= y1 && item.X < x2 && item.Y < y2 && item.Map == e.Mobile.Map)
								list.Add(item);
						}
					}

					if (list.Count > 0)
					{

						count = list.Count;
						for (int i = 0; i < list.Count; ++i)
						{
							((Spawner)list[i]).Running = false;
							((Spawner)list[i]).RemoveCreatures();
						}
					}
				}
				catch (Exception ex)
				{
					LogHelper.LogException(ex);
					System.Console.WriteLine("Exception Caught in DeactivateSpawner code: " + ex.Message);
					System.Console.WriteLine(ex.StackTrace);
				}

				e.Mobile.SendMessage("{0} Spawners Deactivated.", count);
			}
			else
			{
				Usage(e.Mobile);
			}
		}

	}

	public class LoadSpawners
	{
		private interface IEntityEntry
		{
			long Position { get; }
			int Length { get; }
			object Object { get; }
		}

		private sealed class ItemEntry : IEntityEntry
		{
			private Item m_Item;
			private long m_Position;
			private int m_Length;

			public object Object
			{
				get
				{
					return m_Item;
				}
			}

			public long Position
			{
				get
				{
					return m_Position;
				}
			}

			public int Length
			{
				get
				{
					return m_Length;
				}
			}

			public ItemEntry(Item item, long pos, int length)
			{
				m_Item = item;
				m_Position = pos;
				m_Length = length;
			}
		}

		public static void Initialize()
		{
			Server.Commands.Register("LoadSpawners", AccessLevel.Administrator, new CommandEventHandler(LoadSpawners_OnCommand));
		}

		[Usage("LoadSpawners")]
		[Description("Loads All spawners in from saved spawner file.")]
		public static void LoadSpawners_OnCommand(CommandEventArgs e)
		{
			int count = 0;
			int itemCount = 0;
			Hashtable m_Items;
			if (e.Arguments.Length == 1)
			{
				string FileName = e.Arguments[0].ToString();
				string itemIdxPath = Path.Combine("Saves/Spawners/", FileName + ".idx");
				string itemBinPath = Path.Combine("Saves/Spawners/", FileName + ".bin");

				try
				{
					ArrayList items = new ArrayList();
					if (File.Exists(itemIdxPath))
					{
						using (FileStream idx = new FileStream(itemIdxPath, FileMode.Open, FileAccess.Read, FileShare.Read))
						{
							BinaryReader idxReader = new BinaryReader(idx);

							itemCount = idxReader.ReadInt32();
							count = itemCount;

							m_Items = new Hashtable(itemCount);

							for (int i = 0; i < itemCount; ++i)
							{
								long pos = idxReader.ReadInt64();
								int length = idxReader.ReadInt32();

								Item item = null;

								try
								{
									item = new Spawner();
								}
								catch (Exception ex) { EventSink.InvokeLogException(new LogExceptionEventArgs(ex)); }

								if (item != null)
								{
									items.Add(new ItemEntry(item, pos, length));
									World.AddItem(item);

								}
							}

							idxReader.Close();
						}


					}
					else
					{
						e.Mobile.SendMessage("File Not Found {0}.idx", FileName);
					}

					if (File.Exists(itemBinPath))
					{
						using (FileStream bin = new FileStream(itemBinPath, FileMode.Open, FileAccess.Read, FileShare.Read))
						{
							BinaryFileReader reader = new BinaryFileReader(new BinaryReader(bin));

							for (int i = 0; i < items.Count; ++i)
							{
								ItemEntry entry = (ItemEntry)items[i];
								Item item = (Item)entry.Object;

								if (item != null)
								{
									reader.Seek(entry.Position, SeekOrigin.Begin);

									try
									{
										item.Deserialize(reader);

										if (reader.Position != (entry.Position + entry.Length))
											throw new Exception(String.Format("***** Bad serialize on {0} *****", item.GetType()));

										item.MoveToWorld(item.Location, item.Map);
									}
									catch (Exception ex) { EventSink.InvokeLogException(new LogExceptionEventArgs(ex)); }
								}
							}

							reader.Close();
						}

					}
					else
					{
						e.Mobile.SendMessage("File Not Found {0}.bin", FileName);
					}

				}
				catch (Exception ex)
				{
					LogHelper.LogException(ex);
					System.Console.WriteLine("Exception Caught in LoadSpawner code: " + ex.Message);
					System.Console.WriteLine(ex.StackTrace);
				}

				e.Mobile.SendMessage("{0} Spawners Loaded.", count);
			}
			else
			{
				e.Mobile.SendMessage("[Usage <FileName>");
			}
		}
	}
}

