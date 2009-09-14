﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using ArcEngine.Graphic;


namespace ArcEngine.Asset
{

	/// <summary>
	/// Frame animation
	/// </summary>
	public class Frame
	{

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="layer">Layer to get the frame from</param>
		/// <param name="time">Time of the frame</param>
		/// <exception>ArgumentNullExecption</exception>
		public Frame(AnimationLayer layer, TimeSpan time)
		{
			if (layer == null)
				throw new ArgumentNullException("layer");

			Layer = layer;

			KeyFrame next = layer.GetNextKeyFrame(time);
			KeyFrame prev = layer.GetPreviousKeyFrame(time);

			if (next == null | prev == null)
			{
				TileID = -1;
				TextID = -1;
				return;
			}

	
			// Length between to keyframes
			float delta;
			if (next.Time - prev.Time == TimeSpan.Zero)
				delta = 1.0f;
			else
				delta = (float)(time - prev.Time).TotalMilliseconds / (float)(next.Time - prev.Time).TotalMilliseconds;


			Time = time;

			// Tile
			TileID = prev.TileID;
			BgColor = prev.BgColor;
			Point loc  = prev.TileLocation;
			loc.Offset(
				(int)((next.TileLocation.X - prev.TileLocation.X) * delta),
				(int)((next.TileLocation.Y - prev.TileLocation.Y) * delta));
			TileLocation = loc;

			// Text
			TextID = prev.TextID;
			loc = prev.TextLocation;
			loc.Offset(
				(int)((next.TextLocation.X - prev.TextLocation.X) * delta),
				(int)((next.TextLocation.Y - prev.TextLocation.Y) * delta));
			TextLocation = loc;

		}



		/// <summary>
		/// Draws the frame
		/// </summary>
		public void Draw()
		{
			Display.ScissorZone = Layer.Viewport;

			// Display tile
			if(Layer.Animation.TileSet != null && TileID != -1)
				Layer.Animation.TileSet.Draw(TileID, TileLocation);

			// Display text
			if (TextID != -1 && Layer.Animation.StringTable != null && Layer.Animation.Font != null)
			{
				Layer.Animation.Font.DrawText(TextLocation, Layer.Animation.StringTable.GetString(TextID));
			}
		}


		#region Properties

		/// <summary>
		/// Time of the Frame
		/// </summary>
		public TimeSpan Time
		{
			get;
			private set;
		}


		/// <summary>
		/// Location of the tile
		/// </summary>
		public Point TileLocation
		{
			get;
			private set;
		}


		/// <summary>
		/// Id of the tile
		/// </summary>
		public int TileID
		{
			get;
			private set;
		}


		/// <summary>
		/// Background color
		/// </summary>
		public Color BgColor
		{
			get;
			private set;
		}

		#region Text

		/// <summary>
		/// Text to use in the StringTable
		/// </summary>
		public int TextID
		{
			get;
			private set;
		}


		/// <summary>
		/// Location of the text
		/// </summary>
		public Point TextLocation
		{
			get;
			private set;
		}


		/// <summary>
		/// Color of the text
		/// </summary>
		public Color TextColor
		{
			get;
			private set;
		}



		/// <summary>
		/// Layer of the frame
		/// </summary>
		public AnimationLayer Layer
		{
			get;
			private set;
		}


		#endregion


		#endregion
	}
}