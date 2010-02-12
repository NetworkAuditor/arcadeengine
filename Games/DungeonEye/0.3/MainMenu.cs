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
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml;
using ArcEngine;
using ArcEngine.Asset;
using ArcEngine.Graphic;
using ArcEngine.Input;
using ArcEngine.Utility.ScreenManager;
using DungeonEye.Gui;



namespace DungeonEye
{
	/// <summary>
	/// Main menu class
	/// </summary>
	public class MainMenu : GameScreen
	{

		/// <summary>
		/// Constructor
		/// </summary>
		public MainMenu()
		{
			Buttons = new List<ScreenButton>(3);

		}


		/// <summary>
		/// Load content
		/// </summary>
		public override void LoadContent()
		{
			ResourceManager.LoadBank("data/MainMenu.bnk");

	
			Tileset = ResourceManager.CreateAsset<TileSet>("Main Menu");
			Tileset.Scale = new SizeF(2.0f, 2.0f);

			Font = ResourceManager.CreateSharedAsset<Font2d>("intro");
			Font.GlyphTileset.Scale = new SizeF(2.0f, 2.0f);

			StringTable = ResourceManager.CreateAsset<StringTable>("main");

			
			Buttons.Add(new ScreenButton("", new Rectangle(156, 324, 340, 14)));
			Buttons[0].Selected += new EventHandler(LoadGameEvent);

			Buttons.Add(new ScreenButton("", new Rectangle(156, 342, 340, 14)));
			Buttons[1].Selected += new EventHandler(StartGameEvent);

			Buttons.Add(new ScreenButton("", new Rectangle(156, 360, 340, 14)));
			Buttons[2].Selected += new EventHandler(OptionEvent);

			Buttons.Add(new ScreenButton("", new Rectangle(156, 378, 340, 14)));
			Buttons[3].Selected += new EventHandler(QuitEvent);
		}



		/// <summary>
		/// Unload contents
		/// </summary>
		public override void UnloadContent()
		{
			if (Tileset != null)
				Tileset.Dispose();

			if (Font != null)
				Font.Dispose();
		}




		/// <summary>
		/// Option entry event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void OptionEvent(object sender, EventArgs e)
		{
			ScreenManager.AddScreen(new OptionMenu());
		}



		/// <summary>
		/// Quits the game
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void QuitEvent(object sender, EventArgs e)
		{
			ScreenManager.Game.Exit();
		}


		/// <summary>
		/// Start a new party
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void StartGameEvent(object sender, EventArgs e)
		{
			ScreenManager.AddScreen(new CharGen());
		}



		/// <summary>
		/// Load a party in progress
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void LoadGameEvent(object sender, EventArgs e)
		{
			if (!System.IO.File.Exists("savegame.xml"))
				return;
			Team team = new Team(null);
			team.SaveGame = "savegame.xml";


			ScreenManager.AddScreen(team);
		}




		#region Update & draw


		/// <summary>
		/// Update logic
		/// </summary>
		/// <param name="time"></param>
		/// <param name="hasFocus"></param>
		/// <param name="isCovered"></param>
		public override void Update(GameTime time, bool hasFocus, bool isCovered)
		{
			// No focus byebye
			if (!hasFocus)
				return;


			// Does the default language changed ?
			if (Game.LanguageName != StringTable.LanguageName)
			{
				StringTable.LanguageName = Game.LanguageName;

				for (int id = 0; id < Buttons.Count; id++)
					Buttons[id].Text = StringTable.GetString(id+1);
			}

			Point mousePos = Mouse.Location;
			for (int id = 0; id < Buttons.Count; id++)
			{
				ScreenButton button = Buttons[id];

				// Mouse over ?
				if (button.Rectangle.Contains(mousePos))
				{
					//button.TextColor = Color.FromArgb(255, 85, 85);
					MenuID = id;
					if (Mouse.IsNewButtonDown(System.Windows.Forms.MouseButtons.Left))
						button.OnSelectEntry();
				}

			}

			// Bye bye
			if (Keyboard.IsNewKeyPress(System.Windows.Forms.Keys.Escape))
				ScreenManager.Game.Exit();

			// Run intro
			if (Keyboard.IsNewKeyPress(System.Windows.Forms.Keys.I))
				ScreenManager.AddScreen(new IntroScreen());

	
			if (Keyboard.IsNewKeyPress(System.Windows.Forms.Keys.Up))
			{
				MenuID--;
				if (MenuID < 0)
					MenuID = Buttons.Count - 1;
			}
			else if (Keyboard.IsNewKeyPress(System.Windows.Forms.Keys.Down))
			{
				MenuID++;
				if (MenuID >= Buttons.Count)
					MenuID = 0;

			}
			else if (Keyboard.IsNewKeyPress(System.Windows.Forms.Keys.Enter))
			{
				Buttons[MenuID].OnSelectEntry();
			}
		}


		/// <summary>
		/// Drawing
		/// </summary>
		public override void Draw()
		{
			// Clears the background
			Display.ClearBuffers();
			Display.Color = Color.White;


			// Background
			Tileset.Draw(1, Point.Empty);

			// Draw buttons
			for (int id = 0; id < Buttons.Count; id++)
			{
				ScreenButton button = Buttons[id];

				Point point = button.Rectangle.Location;

				// Text
				Font.DrawText(point, id == MenuID ? Color.FromArgb(255, 85, 85) : Color.White, button.Text);

			}


			// Draw the cursor or the item in the hand
			Display.Color = Color.White;
			Tileset.Draw(0, Mouse.Location);
	
		}

		#endregion



		#region Properties

		/// <summary>
		/// 
		/// </summary>
		TileSet Tileset;


		/// <summary>
		/// 
		/// </summary>
		Font2d Font;

		/// <summary>
		/// List of buttons
		/// </summary>
		List<ScreenButton> Buttons;


		/// <summary>
		/// Current MenuID
		/// </summary>
		int MenuID;

		/// <summary>
		/// String table
		/// </summary>
		StringTable StringTable;


		#endregion

	}
}
