﻿using System.Windows.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using ArcEngine;
using ArcEngine.Asset;
using ArcEngine.Graphic;
using ArcEngine.Input;
using ArcEngine.ScreenManager;
using DungeonEye.Gui;

namespace DungeonEye
{
	/// <summary>
	/// Charactere generation
	/// </summary>
	public class CharGen : GameScreen
	{

		/// <summary>
		/// Constructor
		/// </summary>
		public CharGen()
		{

		}




		/// <summary>
		/// Loads content
		/// </summary>
		public override void LoadContent()
		{
			Tileset = ResourceManager.CreateSharedAsset<TileSet>("CharGen");
			Tileset.Scale = new SizeF(2.0f, 2.0f);


			Font = ResourceManager.CreateAsset<TextureFont>("intro");


			PlayButton = new ScreenButton(string.Empty, new Rectangle(48, 362, 166, 32));
			PlayButton.Selected += new EventHandler(PlayButton_Selected);
		}



		#region Events


		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void PlayButton_Selected(object sender, EventArgs e)
		{
			ScreenManager.AddScreen(new Team());
			ExitScreen();
		}


		#endregion


		#region Updates & Draws


		/// <summary>
		/// 
		/// </summary>
		/// <param name="time"></param>
		/// <param name="hasFocus"></param>
		/// <param name="isCovered"></param>
		public override void Update(GameTime time, bool hasFocus, bool isCovered)
		{
			if (Keyboard.IsNewKeyPress(Keys.Escape))
				ExitScreen();


			if (PlayButton.Rectangle.Contains(Mouse.Location) && Mouse.IsNewButtonDown(System.Windows.Forms.MouseButtons.Left))
				PlayButton.OnSelectEntry();

		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="device"></param>
		public override void Draw()
		{
			// Clears the background
			Display.ClearBuffers();
			Display.Color = Color.White;


			// Background
			Tileset.Draw(0, Point.Empty);


			Font.Color = Color.White;


			if (CurrentHero == null)
			{
				Font.DrawText(new Point(304, 160), "Select the box of");
				Font.DrawText(new Point(304, 178), "the character you");
				Font.DrawText(new Point(304, 196), "wish to create or");
				Font.DrawText(new Point(304, 212), "view.");
			}


			// Team is ready, game can begin...
			if (IsTeamReadyToPlay())
			{
				Tileset.Draw(1, new Point(48, 362));
			}



			// Draw the cursor or the item in the hand
			Display.Color = Color.White;
			Tileset.Draw(999, Mouse.Location);


		}


		#endregion



		/// <summary>
		/// Returns true if the team is ready to play
		/// </summary>
		/// <returns></returns>
		bool IsTeamReadyToPlay()
		{
			return false;
		}



		#region Properties

		/// <summary>
		/// Tileset
		/// </summary>
		TileSet Tileset;


		/// <summary>
		/// 
		/// </summary>
		TextureFont Font;


		/// <summary>
		/// Play button
		/// </summary>
		ScreenButton PlayButton;



		/// <summary>
		/// Heroes in the team
		/// </summary>
		Hero[] Heroes;


		/// <summary>
		/// Currently selected hero
		/// </summary>
		Hero CurrentHero;

		#endregion
	}
}
