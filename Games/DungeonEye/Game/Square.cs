﻿#region Licence
//
//This file is part of ArcEngine.
//Copyright (C)2008-2010 Adrien Hémery ( iliak@mimicprod.net )
//
//ArcEngine is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//any later version.
//
//ArcEngine is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with Foobar.  If not, see <http://www.gnu.org/licenses/>.
//
#endregion
using System.ComponentModel;
using System.Drawing;
using ArcEngine;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;


namespace DungeonEye
{
	/// <summary>
	/// Represents a block in a maze
	/// 
	/// 
	/// Events
	///  OnTeamEnter     Team enter in the block
	///  OnTeamStand     Team stand in the block
	///  OnTeamLeave     Team leave the block
	///  OnDropItem      An item is dropped on the ground
	///  OnCollectItem   An item is picked up off the ground
	///  OnClick         The user clicked on the block
	/// 
	/// 
	/// </summary>
	public class Square : IDisposable
	{


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="maze">Maze</param>
		public Square(Maze maze)
		{
			if (maze == null)
				throw new ArgumentNullException("maze");

			Location = new DungeonLocation(maze.Name, Point.Empty);
			Maze = maze;
			Type = SquareType.Wall;
			Monsters = new Monster[4];

			Decorations = new int[] { -1, -1, -1, -1 };
			Alcoves = new bool[4];

			Items = new List<Item>[]
			{
				new List<Item>(),
				new List<Item>(),
				new List<Item>(),
				new List<Item>()
			};

		}


		/// <summary>
		/// Initialization
		/// </summary>
		public void Init()
		{
			for (int i = 0; i < 4; i++)
			{
				// Monsters
				if (Monsters[i] != null)
					Monsters[i].OnSpawn();
			}
		}


		/// <summary>
		/// Dispose resources
		/// </summary>
		public void Dispose()
		{
			foreach (Monster monster in Monsters)
			{
				if (monster != null)
					monster.Dispose();
			}
		}



		/// <summary>
		/// To string
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			if (Actor != null)
				sb.Append(Actor);

			else if (MonsterCount > 0)
				sb.Append(" " + MonsterCount + " monster(s)");

			else if (NoMonster)
				sb.Append(" (no monster)");

			else if (NoGhost)
				sb.Append(" (no ghost)");

			else if (IsWall)
			{
				if (IsIllusion)
				{
					sb.Append(" Illusion");
				}
				else
					sb.Append(" Wall");

				if (HasAlcoves)
				{
					sb.Append(" Alcove facing ");
					foreach (CardinalPoint point in Enum.GetValues(typeof(CardinalPoint)))
					{
						if (HasAlcove(point))
							sb.Append(point + " ");
					}
				}
			}
			else
			{
				sb.Append(" Floor");
			}

			if (ItemCount > 0)
				sb.Append(" " + ItemCount + " item(s)");

			return sb.ToString();
		}


		/// <summary>
		/// Update
		/// </summary>
		/// <param name="time"></param>
		public void Update(GameTime time)
		{
			// Update monsters
			if (MonsterCount > 0)
				for (int i = 0; i < 4; i++)
				{
					Monster monster = Monsters[i];
					if (monster == null)
						continue;


					// Monster is dead
					if (monster.IsDead)
					{
						monster.OnDeath();
						Monsters[i] = null;
					}
					else
						monster.Update(time);

				}

			// Actor
			if (Actor != null)
				Actor.Update(time);
		}


		#region Events

		/// <summary>
		/// A hero interacted with a side of the block
		/// </summary>
		/// <param name="location">Location of the mouse</param>
		/// <param name="side">Wall side</param>
		/// <returns>True if the event is processed</returns>
		public bool OnClick(Point location, CardinalPoint side)
		{
			if (Actor == null)
				return false;

			Team team = GameScreen.Team;

			// A door
			if (team.ItemInHand == null)
			{
				Actor.OnClick(location, side);
				return true;
			}

			// An Alcove 
			else if (HasAlcove(side) && DisplayCoordinates.Alcove.Contains(location))
			{
				if (team.ItemInHand != null)
				{
					DropAlcoveItem(side, team.ItemInHand);
					team.SetItemInHand(null);
				}
				else
				{
					team.SetItemInHand(CollectAlcoveItem(side));
				}

				return true;
			}


			return false;
		}


		/// <summary>
		/// Team stand on the block
		/// </summary>
		public void OnTeamStand()
		{
			if (Actor != null)
				Actor.OnTeamStand();
		}


		/// <summary>
		/// Monster stand on the block
		/// </summary>
		/// <param name="monster"></param>
		public void OnMonsterStand(Monster monster)
		{
			if (monster == null)
				return;

			if (Actor != null)
				Actor.OnMonsterStand(monster);
		}


		/// <summary>
		/// Action when the team enter the block
		/// </summary>
		public void OnTeamEnter()
		{
			if (Actor != null)
				Actor.OnTeamEnter();
		}




		/// <summary>
		/// Action when the team leave the block
		/// </summary>
		public void OnTeamLeave()
		{
			if (Actor != null)
				Actor.OnTeamLeave();
		}


		/// <summary>
		/// A monster enters on the block
		/// </summary>
		/// <param name="monster">Monster handle</param>
		public void OnMonsterEnter(Monster monster)
		{
			if (monster == null)
				return;

			if (Actor != null)
				Actor.OnMonsterEnter(monster);
		}


		/// <summary>
		/// Monster leaves the block
		/// </summary>
		/// <param name="monster">Monster handle</param>
		public void OnMonsterLeave(Monster monster)
		{
			if (monster == null)
				return;

			if (Actor != null)
				Actor.OnMonsterLeave(monster);
		}


		/// <summary>
		/// Item is dropped on the block
		/// </summary>
		/// <param name="item">Item handle</param>
		public void OnDroppedItem(Item item)
		{
			if (item == null)
				return;

			if (Actor != null)
				Actor.OnItemDropped(item);
		}


		/// <summary>
		/// Item is collected from the block
		/// </summary>
		/// <param name="item">Item handle</param>
		public void OnCollectedItem(Item item)
		{
			if (item == null)
				return;

			if (Actor != null)
				Actor.OnItemCollected(item);
		}

		#endregion




		#region Decoration

		/// <summary>
		/// Gets if the wall have decoration
		/// </summary>
		/// <param name="from">Facing direction</param>
		/// <param name="side">Wall side</param>
		/// <returns>True if a decoration is present<returns>
		public bool HasDecoration(CardinalPoint from, CardinalPoint side)
		{
			return GetDecorationId(from, side) != -1;
		}


		/// <summary>
		/// Gets a decoration
		/// </summary>
		/// <param name="side">Wall side</param>
		/// <returns>True if a decoration is present<returns>
		public bool HasDecoration(CardinalPoint side)
		{
			return Decorations[(int)side] != -1;
		}


		/// <summary>
		/// Gets the decoration id of a wall
		/// </summary>
		/// <param name="from">Facing direction</param>
		/// <param name="side">Wall side</param>
		/// <returns>Decoration id</returns>
		public int GetDecorationId(CardinalPoint from, CardinalPoint side)
		{
			return Decorations[(int)Compass.GetDirectionFromView(from, side)];
		}

		#endregion


		#region Monsters


		/// <summary>
		/// Get the monster at a given location
		/// </summary>
		/// <param name="position">Location</param>
		/// <returns>Monster handle or null</returns>
		public Monster GetMonster(SquarePosition position)
		{
			// No monster here
			if (MonsterCount == 0)
				return null;

			// Only one monster, so returns this one
			else if (MonsterCount == 1)
			{
				for (int i = 0; i < 4; i++)
					if (Monsters[i] != null)
						return Monsters[i];
			}

			if (position == SquarePosition.Center)
				return null;

			return Monsters[(int)position];
		}


		/// <summary>
		/// Removes a monster
		/// </summary>
		/// <param name="position">Square position</param>
		public void RemoveMonster(SquarePosition position)
		{
			if (position == SquarePosition.Center)
				return;

			Monsters[(int)position] = null;
		}

		#endregion


		#region Alcoves

		/// <summary>
		/// Gets if the wall have an alcove
		/// </summary>
		/// <param name="from">Facing direction</param>
		/// <param name="side">Wall side</param>
		/// <returns>True if an alcove is present<returns>
		public bool HasAlcove(CardinalPoint from, CardinalPoint side)
		{
			return Alcoves[(int)Compass.GetDirectionFromView(from, side)];
		}


		/// <summary>
		/// Gets an alcove
		/// </summary>
		/// <param name="side">Side of the alcove</param>
		/// <returns>True if an alcove is present<returns>
		public bool HasAlcove(CardinalPoint side)
		{
			return Alcoves[(int)side];
		}


		/// <summary>
		/// Creates or removes an alcove
		/// </summary>
		/// <param name="side">Side of the alcove</param>
		/// <param name="create">Create or remove the alcove</param>
		public void SetAlcove(CardinalPoint side, bool create)
		{
			Alcoves[(int)side] = create;
		}


		/// <summary>
		/// Gets items from an alcove 
		/// </summary>
		/// <param name="side">Wall side</param>
		/// <returns>List of items</returns>
		public List<Item> GetAlcoveItems(CardinalPoint side)
		{
			return Items[(int)side];
		}


		/// <summary>
		/// Gets items from an alcove 
		/// </summary>
		/// <param name="from">View point</param>
		/// <param name="side">Wall side</param>
		/// <returns>List of items</returns>
		public List<Item> GetAlcoveItems(CardinalPoint from, CardinalPoint side)
		{
			CardinalPoint[,] tab = new CardinalPoint[,]
			{
				{CardinalPoint.North, CardinalPoint.South, CardinalPoint.West, CardinalPoint.East},
				{CardinalPoint.South, CardinalPoint.North, CardinalPoint.East, CardinalPoint.West},
				{CardinalPoint.West, CardinalPoint.East, CardinalPoint.South, CardinalPoint.North},
				{CardinalPoint.East, CardinalPoint.West, CardinalPoint.North, CardinalPoint.South},
			};

			return GetAlcoveItems(tab[(int)from, (int)side]);
		}


		/// <summary>
		/// Collects an item in an alcove
		/// </summary>
		/// <param name="side">Wall side</param>
		/// <returns>Item handle or null</returns>
		public Item CollectAlcoveItem(CardinalPoint side)
		{
			return CollectItem((SquarePosition)side);
		}


		/// <summary>
		/// Drops an item in an alcove
		/// </summary>
		/// <param name="side">Wall side</param>
		/// <returns>True if the item can go in the alcove, or false</returns>
		public bool DropAlcoveItem(CardinalPoint side, Item item)
		{
			if (item == null || !HasAlcove(side))
				return false;

			Items[(int)side].Add(item);

			return true;
		}


		#endregion


		#region IO

		/// <summary>
		/// 
		/// </summary>
		/// <param name="writer"></param>
		public bool Save(XmlWriter writer)
		{
			if (writer == null)
				return false;


			writer.WriteStartElement(Tag);

			// Location
			Location.Save("location", writer);

			// Type of wall
			writer.WriteStartElement("type");
			writer.WriteAttributeString("value", Type.ToString());
			writer.WriteEndElement();

			if (Actor != null)
				Actor.Save(writer);


			// Wall decoration
			writer.WriteStartElement("decoration");
			foreach (CardinalPoint point in Enum.GetValues(typeof(CardinalPoint)))
				writer.WriteAttributeString(point.ToString(), Decorations[(int)point].ToString());
			writer.WriteEndElement();


			// Alcoves
			foreach (CardinalPoint side in Enum.GetValues(typeof(CardinalPoint)))
			{
				if (HasAlcove(side))
				{
					writer.WriteStartElement("alcove");
					writer.WriteAttributeString("side", side.ToString());
					writer.WriteEndElement();
				}
			}


			// Items
			for (int i = 0; i < 4; i++)
			{
				if (Items[i].Count > 0)
				{

					foreach (Item item in Items[i])
					{
						writer.WriteStartElement("item");
						writer.WriteAttributeString("location", ((SquarePosition)i).ToString());
						writer.WriteAttributeString("name", item.Name);
						writer.WriteEndElement();
					}

				}
			}

			foreach (Monster monster in Monsters)
			{
				if (monster == null)
					continue;

				monster.Save(writer);
			}

			writer.WriteStartElement("nomonster");
			writer.WriteAttributeString("value", NoMonster.ToString());
			writer.WriteEndElement();

			writer.WriteStartElement("noghost");
			writer.WriteAttributeString("value", NoGhost.ToString());
			writer.WriteEndElement();

			writer.WriteEndElement();

			return true;
		}



		/// <summary>
		/// Loads block defintion
		/// </summary>
		/// <param name="xml">Xml handle</param>
		/// <returns></returns>
		public bool Load(XmlNode xml)
		{
			if (xml == null || xml.Name != Tag)
				return false;

			foreach (XmlNode node in xml)
			{
				if (node.NodeType == XmlNodeType.Comment)
					continue;



				switch (node.Name.ToLower())
				{
					case "monster":
					{
						Monster monster = new Monster(Maze);
						monster.Load(node);
						monster.Teleport(this, (SquarePosition)Enum.Parse(typeof(SquarePosition), node.Attributes["position"].Value));
					}
					break;

					case "item":
					{
						SquarePosition loc = (SquarePosition)Enum.Parse(typeof(SquarePosition), node.Attributes["location"].Value);
						Item item = ResourceManager.CreateAsset<Item>(node.Attributes["name"].Value);
						if (item != null)
							Items[(int)loc].Add(item);
					}
					break;

					case "noghost":
					{
						NoGhost = bool.Parse(node.Attributes["value"].Value);
					}
					break;

					case "nomonster":
					{
						NoMonster = bool.Parse(node.Attributes["value"].Value);
					}
					break;

					case "location":
					{
						Location.Load(node);
					}
					break;

					case "type":
					{
						Type = (SquareType)Enum.Parse(typeof(SquareType), node.Attributes["value"].Value, true);
					}
					break;

					case Door.Tag:
					{
						Actor = new Door(this);
						Actor.Load(node);
					}
					break;

					case Teleporter.Tag:
					{
						Actor = new Teleporter(this);
						Actor.Load(node);
					}
					break;

					case PressurePlate.Tag:
					{
						Actor = new PressurePlate(this);
						Actor.Load(node);
					}
					break;

					case Pit.Tag:
					{
						Actor = new Pit(this);
						Actor.Load(node);
					}
					break;

					case ForceField.Tag:
					{
						Actor = new ForceField(this);
						Actor.Load(node);
					}
					break;

					case Stair.Tag:
					{
						Actor = new Stair(this);
						Actor.Load(node);
					}
					break;

					case EventSquare.Tag:
					{
						Actor = new EventSquare(this);
						Actor.Load(node);
					}
					break;

					case "alcove":
					{
						CardinalPoint side = (CardinalPoint)Enum.Parse(typeof(CardinalPoint), node.Attributes["side"].Value);
						SetAlcove(side, true);
					}
					break;

					case "decoration":
					{
						foreach (CardinalPoint point in Enum.GetValues(typeof(CardinalPoint)))
							Decorations[(int)point] = int.Parse(node.Attributes[point.ToString()].Value);
					}
					break;

					default:
					{
						Trace.WriteLine("[Square] Load() : Unknown node \"{0}\"", node.Name);
					}
					break;
				}
			}



			return true;
		}

		#endregion


		#region Items


		/// <summary>
		/// Returns items on the ground at a specific corner
		/// </summary>
		/// <param name="position">Ground position</param>
		/// <returns>List of items</returns>
		public List<Item> GetItems(SquarePosition position)
		{
			return Items[(int)position];
		}


		/// <summary>
		/// Returns items on the ground at a specific corner
		/// </summary>
		/// <param name="from">Facing position</param>
		/// <param name="position">Ground position</param>
		/// <returns>List of items</returns>
		public List<Item> GetItems(CardinalPoint from, SquarePosition position)
		{
			CardinalPoint[,] tab = new CardinalPoint[,]
			{
				{CardinalPoint.North, CardinalPoint.South, CardinalPoint.West, CardinalPoint.East},
				{CardinalPoint.South, CardinalPoint.North, CardinalPoint.East, CardinalPoint.West},
				{CardinalPoint.West, CardinalPoint.East, CardinalPoint.South, CardinalPoint.North},
				{CardinalPoint.East, CardinalPoint.West, CardinalPoint.North, CardinalPoint.South},
			};

			return GetItems((SquarePosition)tab[(int)from, (int)position]);
		}


		/// <summary>
		/// Collects an item on the ground
		/// </summary>
		/// <param name="position">Position in the block</param>
		/// <returns>Item</returns>
		public Item CollectItem(SquarePosition position)
		{
			// No item in the middle of a block
			if (position == SquarePosition.Center)
				return null;

			int count = Items[(int)position].Count;
			if (count == 0)
				return null;

			Item item = Items[(int)position][count - 1];
			Items[(int)position].RemoveAt(count - 1);

			// Call the script
			OnCollectedItem(item);

			return item;
		}


		/// <summary>
		/// Drops an item on the ground
		/// </summary>
		/// <param name="position">Position in the block</param>
		/// <param name="item">Item to drop</param>
		/// <returns>True if the item can be dropped</returns>
		public bool DropItem(SquarePosition position, Item item)
		{
			// No item in the middle of a block
			if (position == SquarePosition.Center)
				throw new ArgumentOutOfRangeException("position", "No items in the middle of a maze block !");

			// Can't drop item in wall
			if (IsWall)
				return false;

			// Actor refuses items
			if (Actor != null && !Actor.AcceptItems)
				return false;

			// Add the item to the ground
			Items[(int)position].Add(item);

			// Call the script
			OnCollectedItem(item);

			return true;
		}


		/// <summary>
		/// Returns all items depending the view point
		/// </summary>
		/// <param name="location">Facing direction</param>
		/// <returns>Returns an array of items
		/// Position 0 : Up left
		/// Position 1 : Up Right
		/// Position 2 : Bottom left
		/// Position 3 : Bottom right</returns>
		public List<Item>[] GetItems(CardinalPoint location)
		{
			// List of items
			List<Item>[] items = new List<Item>[4];


			switch (location)
			{
				case CardinalPoint.North:
				{
					items[0] = Items[0];
					items[1] = Items[1];
					items[2] = Items[2];
					items[3] = Items[3];
				}
				break;
				case CardinalPoint.East:
				{
					items[0] = Items[1];
					items[1] = Items[3];
					items[2] = Items[0];
					items[3] = Items[2];
				}
				break;
				case CardinalPoint.South:
				{
					items[0] = Items[3];
					items[1] = Items[2];
					items[2] = Items[1];
					items[3] = Items[0];
				}
				break;
				case CardinalPoint.West:
				{
					items[0] = Items[2];
					items[1] = Items[0];
					items[2] = Items[3];
					items[3] = Items[1];
				}
				break;
			}


			return items;
		}


		#endregion


		#region Properties

		/// <summary>
		/// 
		/// </summary>
		public const string Tag = "block";


		#region Monsters

		/// <summary>
		/// Number of monster in the square
		/// </summary>
		public int MonsterCount
		{
			get
			{
				int count = 0;

				if (Monsters[0] != null) count++;
				if (Monsters[1] != null) count++;
				if (Monsters[2] != null) count++;
				if (Monsters[3] != null) count++;

				return count;
			}
		}


		/// <summary>
		/// Does the square has monster
		/// </summary>
		public bool HasMonster
		{
			get
			{
				return MonsterCount > 0;
			}
		}

		/// <summary>
		/// Monster list
		/// </summary>
		public Monster[] Monsters
		{
			get;
			private set;
		}

		#endregion


		/// <summary>
		/// Square actor
		/// </summary>
		public SquareActor Actor
		{
			get;
			set;
		}


		/// <summary>
		/// Gets if the wall have alcoves
		/// </summary>
		public bool HasAlcoves
		{
			get
			{
				return (Alcoves[0] || Alcoves[1] || Alcoves[2] || Alcoves[3]);
			}
		}


		/// <summary>
		/// Returns true if the block is a wall or a trick wall
		/// </summary>
		public bool IsWall
		{
			get
			{
				return Type != SquareType.Ground;
			}
		}



		/// <summary>
		/// Is the mazeblock is a pit target
		/// </summary>
		public bool IsPitTarget
		{
			get;
			set;
		}


		/// <summary>
		/// Does the block blocks the team
		/// </summary>
		public bool IsBlocking
		{
			get
			{

				if (Actor != null && Actor.IsBlocking)
					return true;

				else if (IsIllusion)
					return false;

				else if (IsWall)
					return true;

				else if (HasMonster)
					return true;

				return false;
			}
		}


		/// <summary>
		/// Is an illusion block
		/// </summary>
		public bool IsIllusion
		{
			get
			{
				return Type == SquareType.Illusion;
			}

		}


		/// <summary>
		/// Decoration on each wall block
		/// </summary>
		public int[] Decorations;


		/// <summary>
		/// Items on the ground
		/// </summary>
		[Browsable(false)]
		public List<Item>[] Items
		{
			get;
			set;
		}


		/// <summary>
		/// Number of item on ground
		/// </summary>
		public int ItemCount
		{
			get
			{
				int count = 0;
				foreach (List<Item> list in Items)
					count += list.Count;

				return count;

			}
		}


		/// <summary>
		/// Type of the block
		/// </summary>
		public SquareType Type
		{
			get;
			set;
		}


		/// <summary>
		/// Location of the block in the maze
		/// </summary>
		public DungeonLocation Location
		{
			get;
			private set;
		}


		/// <summary>
		/// Maze
		/// </summary>
		public Maze Maze
		{
			get;
			private set;
		}


		/// <summary>
		/// Alcoves
		/// </summary>
		bool[] Alcoves;


		/// <summary>
		/// Marks a square as impassable to any monsters
		/// </summary>
		public bool NoMonster
		{
			get;
			set;
		}


		/// <summary>
		/// Marks a square as impassable to any non-material monsters
		/// </summary>
		public bool NoGhost
		{
			get;
			set;
		}

		#endregion
	}


	/// <summary>
	/// Type of square in the maze
	/// </summary>
	public enum SquareType
	{
		/// <summary>
		/// Ground block
		/// </summary>
		Ground,

		/// <summary>
		/// Wall block
		/// </summary>
		Wall,

		/// <summary>
		/// Illusionary wall
		/// </summary>
		Illusion,

	}


	/// <summary>
	/// Sub position in a square in the maze
	/// 
	/// .-------.
	/// | A | B |
	/// | +---+ |
	/// |-| E |-|
	/// | +---+ |
	/// | C | D |
	/// '-------'
	/// </summary>
	public enum SquarePosition
	{
		/// <summary>
		/// North west
		/// </summary>
		NorthWest = 0,

		/// <summary>
		/// North east
		/// </summary>
		NorthEast = 1,

		/// <summary>
		/// South west
		/// </summary>
		SouthWest = 2,

		/// <summary>
		/// South east
		/// </summary>
		SouthEast = 3,

		/// <summary>
		/// Center position (not for items !)
		/// </summary>
		Center = 4
	}

}
