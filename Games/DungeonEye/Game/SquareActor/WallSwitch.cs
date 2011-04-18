﻿#region Licence
//
//This file is part of ArcEngine.
//Copyright (C)2008-2011 Adrien Hémery ( iliak@mimicprod.net )
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
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Xml;
using ArcEngine.Graphic;
using ArcEngine;
using DungeonEye.Script;


namespace DungeonEye
{
	/// <summary>
	/// Wall switch actor
	/// </summary>
	public class WallSwitch : SquareActor
	{


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="square">Parent square handle</param>
		public WallSwitch(Square square) : base(square)
		{
			ActivatedDecoration = -1;
			DeactivatedDecoration = -1;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="location"></param>
		/// <param name="side"></param>
		/// <returns></returns>
		public override bool OnClick(Point location, CardinalPoint side)
		{
			// No wall side or no decoration or already used and not reusable
			if (side != Side || Square.Maze.Decoration == null || (WasUsed && !Reusable))
				return false;


			// Not in the decoration zone
			if (!Square.Maze.Decoration.IsPointInside(IsActivated ? ActivatedDecoration : DeactivatedDecoration, location))
				return false;
	

			// Does an item is required ?
			if (!string.IsNullOrEmpty(ActivateItem))
			{

				// Picklock
				if (GameScreen.Team.ItemInHand.Name == "PickLock")
				{
					// TODO: already unlocked => "It's already unlocked"
					if (PickLock())
					{
						GameMessage.AddMessage("You pick the lock.", GameColors.Green);
					}
					else
					{
						GameMessage.AddMessage("You failed to pick the lock", GameColors.Yellow);
					}

					return true;
				}

				// No item in hand or not the good item
				if (GameScreen.Team.ItemInHand == null || GameScreen.Team.ItemInHand.Name != ActivateItem)
				{
					GameMessage.AddMessage("You need a key to open this lock");
					return false;
				}

				// Consume item
				if (ConsumeItem)
					GameScreen.Team.SetItemInHand(null);
			}

			WasUsed = true;
			Toggle();

			return true;
		}


		/// <summary>
		/// Try to picklock the switch
		/// </summary>
		/// <returns>True if the lockpick succeeded, otherwise false</returns>
		public bool PickLock()
		{
			return false;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="batch"></param>
		/// <param name="field"></param>
		/// <param name="position"></param>
		/// <param name="direction"></param>
		public override void Draw(SpriteBatch batch, ViewField field, ViewFieldPosition position, CardinalPoint direction)
		{
			// Foreach wall side
			foreach (TileDrawing td in DisplayCoordinates.GetWalls(position))
			{
				// Not the good side
				if (Compass.GetDirectionFromView(direction, td.Side) != Side)
					continue;

				DecorationSet decoset = Square.Maze.Decoration;
				if (decoset == null)
					return;

				Decoration deco = decoset.GetDecoration(IsActivated ? ActivatedDecoration : DeactivatedDecoration);
				if (deco == null)
					return;

				deco.DrawDecoration(batch, decoset, position, Compass.IsSideFacing(direction, Side));
			}
		}



		#region I/O


		/// <summary>
		/// 
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		public override bool Load(XmlNode xml)
		{
			if (xml == null || xml.Name != Tag)
				return false;

			foreach (XmlNode node in xml)
			{
				switch (node.Name.ToLower())
				{
					//case "target":
					//{
					//    if (Target == null)
					//        Target = new DungeonLocation();

					//    Target.Load(xml);
					//}
					//break;

					case "decoration":
					{
						ActivatedDecoration = int.Parse(node.Attributes["activated"].Value);
						DeactivatedDecoration = int.Parse(node.Attributes["deactivated"].Value);
					}
					break;

					case "consumeitem":
					{
						ConsumeItem = bool.Parse(node.InnerText);
					}
					break;

					case "reusable":
					{
						Reusable = bool.Parse(node.InnerText);
					}
					break;

					case "wasused":
					{
						WasUsed = bool.Parse(node.InnerText);
					}
					break;

					case "side":
					{
						Side = (CardinalPoint) Enum.Parse(typeof(CardinalPoint), node.InnerText);
					}
					break;

					case "activateitem":
					{
						ActivateItem = node.InnerText;
					}
					break;

					case "picklock":
					{
						LockLevel = int.Parse(node.InnerText);
					}
					break;

					default:
					{
						base.Load(node);
					}
					break;
				}

			}

			return true;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override bool Save(XmlWriter writer)
		{
			if (writer == null)
				return false;


			writer.WriteStartElement(Tag);


			writer.WriteStartElement("decoration");
			writer.WriteAttributeString("activated", ActivatedDecoration.ToString());
			writer.WriteAttributeString("deactivated", DeactivatedDecoration.ToString());
			writer.WriteEndElement();

			writer.WriteElementString("side", Side.ToString());
			writer.WriteElementString("reusable", Reusable.ToString());
			writer.WriteElementString("wasused", WasUsed.ToString());
			writer.WriteElementString("activateitem", ActivateItem);
			writer.WriteElementString("consumeitem", ConsumeItem.ToString());
			writer.WriteElementString("picklock", LockLevel.ToString());

			base.Save(writer);

			writer.WriteEndElement();

			return true;
		}



		#endregion



		#region Properties

		/// <summary>
		/// 
		/// </summary>
		public const string Tag = "wallswitch";



		/// <summary>
		/// Switch is already used
		/// </summary>
		public bool WasUsed
		{
			get;
			private set;
		}


		/// <summary>
		/// Activated decoration id
		/// </summary>
		public int ActivatedDecoration
		{
			get;
			set;
		}


		/// <summary>
		/// Deactivated decoration id
		/// </summary>
		public int DeactivatedDecoration
		{
			get;
			set;
		}


		/// <summary>
		/// Item needed to activate the switch
		/// </summary>
		public string ActivateItem
		{
			get;
			set;
		}


		/// <summary>
		/// Consume item on use
		/// </summary>
		public bool ConsumeItem
		{
			get;
			set;
		}


		/// <summary>
		/// Wall side
		/// </summary>
		public CardinalPoint Side
		{
			get;
			set;
		}


		/// <summary>
		/// Defines if the switch can be used repeatedly. 
		/// Otherwise, after one use, the switch will no longer function.
		/// </summary>
		public bool Reusable
		{
			get;
			set;
		}


		/// <summary>
		/// Pick lock level required to unlock the switch
		/// </summary>
		public int LockLevel
		{
			get;
			set;
		}

		#endregion
	}
}