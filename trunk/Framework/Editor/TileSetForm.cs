﻿#region Licence
//
//This file is part of ArcEngine.
//Copyright (C)2008-2009 Adrien Hémery ( iliak@mimicprod.net )
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
using System.Text;
using System.Windows.Forms;
using System.Xml;
using ArcEngine.Asset;
using ArcEngine.Forms;
using ArcEngine.Graphic;
using ArcEngine.Interface;


namespace ArcEngine.Editor
{
	/// <summary>
	/// TileSet form editor
	/// </summary>
	internal partial class TileSetForm : AssetEditorBase
	{

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="node">Xml node</param>
		public TileSetForm(XmlNode node)
		{
			InitializeComponent();

			//
			Node = node;
			TileSet = new TileSet();

			TileSelection = new SelectionTool();
			CollisionSelection = new SelectionTool();


			GLTileControl.MouseWheel += new MouseEventHandler(GLTileControl_MouseWheel);
			GLTextureControl.MouseWheel += new MouseEventHandler(GLTextureControl_MouseWheel);


			// Set zoom value
			ZoomBox.SelectedIndex = 0;

		}


		/// <summary>
		/// Save the asset to the manager
		/// </summary>
		public override void Save()
		{
			StringBuilder sb = new StringBuilder();
			using (XmlWriter writer = XmlWriter.Create(sb))
				TileSet.Save(writer);

			string xml = sb.ToString();
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xml);

			ResourceManager.AddAsset<TileSet>(TileSet.Name, doc.DocumentElement);
		}


		/// <summary>
		/// Uncheck all checkboxes
		/// </summary>
		/// <param name="exclude"></param>
		void UncheckButtons(ToolStripButton exclude)
		{
			ToolStripButton[] boxes = new ToolStripButton[]
			{
				SelectionBox,
				HotSpotBox,
				ColisionBox,
			};

			foreach (ToolStripButton box in boxes)
			{
				if (box != exclude)
					box.Checked = false;
			}
		}


		#region GlTextureControl


		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void GLTextureControl_MouseWheel(object sender, MouseEventArgs e)
		{
			if (e.Delta > 0)
			{
				ZoomInButton.PerformClick();
			}
			else if (e.Delta < 0)
			{
				ZoomOutButton.PerformClick();
			}
		}


		/// <summary>
		/// Paint event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void GLTextureControl_Paint(object sender, PaintEventArgs e)
		{
			GLTextureControl.MakeCurrent();
			Display.ViewPort = new Rectangle(Point.Empty, GLTextureControl.Size);


			// Background color
			Display.ClearBuffers();


			// No texture 
			if (TileSet.Texture == null)
			{
				GLTextureControl.SwapBuffers();
				return;
			}

			Batch.Begin();


			// Background texture
			Rectangle dst = new Rectangle(Point.Empty, GLTextureControl.Size);
			Batch.Draw(CheckerBoard, dst, dst, Color.White);


			// Get zoom value
			float zoomvalue = float.Parse((string)ZoomBox.SelectedItem);


			// Draw the tile texture
			Vector4 zoom = new Vector4(
			TextureOffset.X,
			TextureOffset.Y,
			TileSet.Texture.Bounds.Width * zoomvalue,
			TileSet.Texture.Bounds.Height * zoomvalue);

			Vector4 src = new Vector4(0.0f, 0.0f, TileSet.Texture.Size.Width, TileSet.Texture.Size.Height);

			Batch.Draw(TileSet.Texture, zoom, src, Color.White);



			// If we have some tiles to draw
			if (TileSet.Count != 0)
			{
				// Draw the selection box with sizing handles
				if (CurrentTile != null)
				{
					TileSelection.Zoom = zoomvalue;
					TileSelection.Offset = TextureOffset;
					CurrentTile.Rectangle = TileSelection.Rectangle;
					TileSelection.Draw(Batch);
				}

			}

			Batch.End();
			GLTextureControl.SwapBuffers();
		}


		/// <summary>
		/// OnResize event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void GLTextureControl_Resize(object sender, EventArgs e)
		{
			GLTextureControl.MakeCurrent();
			Display.ViewPort = new Rectangle(Point.Empty, GLTextureControl.Size);
		}

		/// <summary>
		/// OnMouseMove
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void GLTextureControl_MouseMove(object sender, MouseEventArgs e)
		{
			// If scrolling with the middle mouse button
			if (e.Button == MouseButtons.Middle)
			{

				// Smooth the value
				TextureOffset.X -= (LastMousePos.X - e.X) / 1;
				TextureOffset.Y -= (LastMousePos.Y - e.Y) / 1;

				// Store last mouse location
				LastMousePos = e.Location;

				return;
			}

			// Prints the location of the mouse
			int zoomvalue = int.Parse((string)ZoomBox.SelectedItem);
			PositionLabel.Text = (int)((e.Location.X - TextureOffset.X) / zoomvalue) + "," + (int)((e.Location.Y - TextureOffset.Y) / zoomvalue);

			// Prints the size of the current tile
			SizeLabel.Text = TileSelection.Rectangle.Width + "," + TileSelection.Rectangle.Height;


			// Resize the tile
			//if (TilesBox.Items.Count > 0)
			//	TileBox.OnMouseMove(e);

		}


		/// <summary>
		/// MouseUp event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void GLTextureControl_MouseUp(object sender, MouseEventArgs e)
		{
			// Transmit event to the selection box
			if (TileSelection != null)
				TileSelection.OnMouseUp(e);
		}


		/// <summary>
		/// MouseDown event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void GLTextureControl_MouseDown(object sender, MouseEventArgs e)
		{
			// Size the selection box
			if (TileSelection.MouseTool == SelectionTool.MouseTools.NoTool && e.Button == MouseButtons.Left)
			{
				int zoomvalue = int.Parse((string)ZoomBox.SelectedItem);

				TileSelection.Rectangle.X = (e.Location.X - TextureOffset.X) / zoomvalue;
				TileSelection.Rectangle.Y = (e.Location.Y - TextureOffset.Y) / zoomvalue;
				TileSelection.Rectangle.Width = 0;
				TileSelection.Rectangle.Height = 0;

				TileSelection.MouseTool = SelectionTool.MouseTools.SizeDownRight;

			}


			// Transmit event to the selection box
			if (TileSelection != null)
				TileSelection.OnMouseDown(e);

			// Pan the texture
			if (e.Button == MouseButtons.Middle)
				LastMousePos = e.Location;
		}


		/// <summary>
		/// On PreviewKeyDown
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnPreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{

			switch (e.KeyData)
			{
				case Keys.Up:
					{
						if (e.Shift)
							TileSelection.Rectangle.Height--;
						else
							TileSelection.Rectangle.Y--;
					}
					break;
				case Keys.Down:
					{
						if (e.Shift)
							TileSelection.Rectangle.Height++;
						else
							TileSelection.Rectangle.Y++;
					}
					break;
				case Keys.Left:
					{
						if (e.Shift)
							TileSelection.Rectangle.Width--;
						else
							TileSelection.Rectangle.X--;
					}
					break;
				case Keys.Right:
					{
						if (e.Shift)
							TileSelection.Rectangle.Width++;
						else
							TileSelection.Rectangle.X++;
					}
					break;

				// Previous tile
				case Keys.PageUp:
					{
						TileIDBox.Value = Math.Max(TileIDBox.Value - 1, 0);
					}
					break;

				// Next tile
				case Keys.PageDown:
					{
						TileIDBox.Value = Math.Min(TileIDBox.Value + 1, TileIDBox.Maximum);
					}
					break;

				default:
					return;
			}

			e.IsInputKey = true;
		}



		#endregion



		#region GLTileControl

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void GLTileControl_MouseWheel(object sender, MouseEventArgs e)
		{
			if (e.Delta > 0)
			{
				ZoomInButton.PerformClick();
			}
			else if (e.Delta < 0)
			{
				ZoomOutButton.PerformClick();
			}

		}

		/// <summary>
		/// OnResize event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void GLTileControl_Resize(object sender, EventArgs e)
		{
			GLTileControl.MakeCurrent();
			Display.ViewPort = new Rectangle(Point.Empty, GLTileControl.Size);
		}


		/// <summary>
		/// OnPaint event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void GLTileControl_Paint(object sender, PaintEventArgs e)
		{
			
			GLTileControl.MakeCurrent();
			Display.ClearBuffers();


			Batch.Begin();


			// Background texture
			Rectangle dst = new Rectangle(Point.Empty, GLTileControl.Size);
			Batch.Draw(CheckerBoard, dst, dst, Color.White);
			

		//	CurrentTile = tileSet.GetTile((int)TileIDBox.Value);

			// No tiles, no draw !
			if (CurrentTile != null)
			{
				// Get zoom value
				float zoomvalue = float.Parse((string)ZoomBox.SelectedItem);

				// Draw the tile
				Vector4 zoom = new Vector4();
				zoom.X = TileOffset.X;
				zoom.Y = TileOffset.Y;
				zoom.Z = CurrentTile.Rectangle.Width * zoomvalue;
				zoom.W = CurrentTile.Rectangle.Height * zoomvalue;

				// Texture source
				Vector4 src = new Vector4(
					CurrentTile.Rectangle.X, CurrentTile.Rectangle.Y,
					CurrentTile.Size.Width, CurrentTile.Size.Height);

				Batch.Draw(TileSet.Texture, zoom, src, Color.White);


				// Draw Collision box
				if (CollisionSelection != null && ColisionBox.Checked)
				{
					CollisionSelection.Zoom = zoomvalue;
					CollisionSelection.Offset = TileOffset;
					CurrentTile.CollisionBox = CollisionSelection.Rectangle;
					CollisionSelection.Draw(Batch);
				}

				// Draw origin
				if (HotSpotBox.Checked)
				{
					Point pos = Point.Empty;
					pos.X = (int)(CurrentTile.Origin.X * zoomvalue + TileOffset.X);
					pos.Y = (int)(CurrentTile.Origin.Y * zoomvalue + TileOffset.Y);

					Rectangle rect = new Rectangle(pos, new Size((int)zoomvalue, (int)zoomvalue));
					Batch.DrawRectangle(rect, Color.Red);

				}
			}


			Batch.End();
			GLTileControl.SwapBuffers();
		}


		/// <summary>
		/// OnMouseMove
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void GLTileControl_MouseMove(object sender, MouseEventArgs e)
		{
			// If scrolling with the middle mouse button
			if (e.Button == MouseButtons.Middle)
			{

				// Smooth the value
				//Point pos = Point.Empty;
				TileOffset.X -= (LastMousePos.X - e.X) / 1;
				TileOffset.Y -= (LastMousePos.Y - e.Y) / 1;

				// Store last mouse location
				LastMousePos = e.Location;

				return;
			}

			// Prints the location of the mouse
			//int zoomvalue = Int32.Parse((string)ZoomBox.SelectedItem) / 100;
			int zoomvalue = int.Parse((string)ZoomBox.SelectedItem);
			PositionLabel.Text = (int)((e.Location.X - TileOffset.X) / zoomvalue) + "," + (int)((e.Location.Y - TileOffset.Y) / zoomvalue);

			// Prints the size of the current tile
			SizeLabel.Text = CollisionSelection.Rectangle.Width + "," + CollisionSelection.Rectangle.Height;

			// Set the collision box
			if (CollisionSelection != null && ColisionBox.Checked)
				CollisionSelection.OnMouseMove(e);


			// Set the hotspot
			if (HotSpotBox.Checked && e.Button == MouseButtons.Left)
			{
				if (CurrentTile != null)
				{
					// Get zoom value
					//int zoomvalue = Int32.Parse((string)ZoomBox.SelectedItem) / 100;

					Point pos = Point.Empty;
					pos.X = (e.Location.X - TileOffset.X) / zoomvalue;
					pos.Y = (e.Location.Y - TileOffset.Y) / zoomvalue;

					CurrentTile.Origin = pos;
				}

			}

		}


		/// <summary>
		/// OnMouseDown
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void GLTileControl_MouseDown(object sender, MouseEventArgs e)
		{
			// Size the selection box
			if (CollisionSelection.MouseTool == SelectionTool.MouseTools.NoTool &&
				e.Button == MouseButtons.Left &&
				ColisionBox.Checked)
			{
				int zoomvalue = Int32.Parse((string)ZoomBox.SelectedItem);

				CollisionSelection.Rectangle.X = (e.Location.X - TileOffset.X) / zoomvalue;
				CollisionSelection.Rectangle.Y = (e.Location.Y - TileOffset.Y) / zoomvalue;
				CollisionSelection.Rectangle.Width = 0;
				CollisionSelection.Rectangle.Height = 0;

				CollisionSelection.MouseTool = SelectionTool.MouseTools.SizeDownRight;
			}


			// Transmit event to the selection box
			if (CollisionSelection != null && ColisionBox.Checked)
				CollisionSelection.OnMouseDown(e);


			// Set the hotspot
			if (HotSpotBox.Checked && e.Button == MouseButtons.Left)
			{
				if (CurrentTile != null)
				{
					// Get zoom value
					int zoomvalue = Int32.Parse((string)ZoomBox.SelectedItem);

					Point pos = Point.Empty;
					pos.X = (e.Location.X - TileOffset.X) / zoomvalue;
					pos.Y = (e.Location.Y - TileOffset.Y) / zoomvalue;

					CurrentTile.Origin = pos;
				}

			}

			// Pan the texture
			if (e.Button == MouseButtons.Middle)
				LastMousePos = e.Location;

		}

		/// <summary>
		/// OnMouseUp
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void GLTileControl_MouseUp(object sender, MouseEventArgs e)
		{
			// Transmit event to the selection box
			if (CollisionSelection != null && ColisionBox.Checked)
				CollisionSelection.OnMouseUp(e);
		}

		#endregion



		#region Events

		/// <summary>
		/// Loads the form
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void TileSetForm_Load(object sender, EventArgs e)
		{
			GLTileControl.MakeCurrent();
			Display.RenderState.Blending = true;
			Display.BlendingFunction(BlendingFactorSource.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

			GLTextureControl.MakeCurrent();
			Display.RenderState.Blending = true;
			Display.BlendingFunction(BlendingFactorSource.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);


			Batch = new SpriteBatch();

			CheckerBoard = new Texture2D(ResourceManager.GetInternalResource("ArcEngine.Resources.checkerboard.png"));
			CheckerBoard.HorizontalWrap = TextureWrapFilter.Repeat;
			CheckerBoard.VerticalWrap = TextureWrapFilter.Repeat;

			TileSet.Load(Node);


			// Available textures
			//TexturesBox.BeginUpdate();
			//TexturesBox.Items.Clear();
			//foreach (string name in ResourceManager.GetBinaryList(".png$"))
			//    TexturesBox.Items.Add(name);
			//TexturesBox.EndUpdate();

/*
			// Rectangle * 2
			foreach (int id in tileSet.Tiles)
			{
				Tile tile = tileSet.GetTile(id);
				Rectangle rect = tile.Rectangle;
				rect = new Rectangle(rect.X * 2, rect.Y * 2, rect.Width *2, rect.Height * 2);
				tile.Rectangle = rect;
			}
*/
/*
			// Origin x 2
			foreach (int id in tileSet.Tiles)
			{
				Tile tile = tileSet.GetTile(id);
				tile.Origin = new Point(tile.Origin.X * 2, tile.Origin.Y * 2);
			}
*/	
			
			// Build Cell list
			//RebuildCellList();
		}


		/// <summary>
		/// Timer Tick
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void RenderTimer_Tick(object sender, EventArgs e)
		{
			GLTextureControl.Invalidate();
			GLTileControl.Invalidate();
		}



		/// <summary>
		/// Selected Tile changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void TilesBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (TileSet == null)
				return;


			CurrentTile = TileSet.GetTile((int)TileIDBox.Value);
			TilePropertyGrid.SelectedObject = CurrentTile;

			if (CurrentTile == null)
				return;

			TileSelection.Rectangle = CurrentTile.Rectangle;
			CollisionSelection.Rectangle = CurrentTile.CollisionBox;


			// Prints the size of the current tile
			SizeLabel.Text = TileSelection.Rectangle.Width + "," + TileSelection.Rectangle.Height;



		}



		/// <summary>
		/// Zoom in the texture preview panel
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ZoomInButton_Click(object sender, EventArgs e)
		{
			int index = ZoomBox.SelectedIndex + 1;
			if (index >= ZoomBox.Items.Count)
				return;

			ZoomBox.SelectedIndex = index;
		}


		/// <summary>
		/// Zoom out the texture preview panel
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ZoomOutButton_Click(object sender, EventArgs e)
		{
			int index = ZoomBox.SelectedIndex - 1;
			if (index < 0)
				return;

			ZoomBox.SelectedIndex = index;
		}


		/// <summary>
		/// Erases current Tile
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void EraseTileButton_Click(object sender, EventArgs e)
		{

			if (MessageBox.Show("Remove this tile ?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
				return;

			TileSet.Remove((int)TileIDBox.Value);


			//int index = TilesBox.SelectedIndex;
			//RebuildCellList();

			//if (TilesBox.Items.Count > 0)
			//    TilesBox.SelectedIndex = Math.Max(0, index - 1);

		}


		/// <summary>
		/// Recenter the view
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ActualSizeButton_Click(object sender, EventArgs e)
		{
			ZoomBox.SelectedIndex = 0;

			TextureOffset = Point.Empty;
			TileOffset = Point.Empty;
		}




		/// <summary>
		/// HotSpot mode
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void HotSpotButton_Click(object sender, EventArgs e)
		{
			ColisionBox.Checked = false;
		}


		/// <summary>
		/// Collision box mode
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CollisionButton_Click(object sender, EventArgs e)
		{
			HotSpotBox.Checked = false;
		}


		/// <summary>
		/// Form closing
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void TileSetForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			DialogResult result = MessageBox.Show("TileSet Editor", "Save modifciations ?", MessageBoxButtons.YesNoCancel);
			if (result == DialogResult.Cancel)
			{
				e.Cancel = true;
				return;
			}
			else if (result == DialogResult.Yes)
				Save();

			if (Batch != null)
				Batch.Dispose();
			Batch = null;

			if (TileSet != null)
				TileSet.Dispose();
			TileSet = null;

			if (CheckerBoard != null)
				CheckerBoard.Dispose();
			CheckerBoard = null;
		}


		/// <summary>
		/// Change the texture
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void TexturesBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (TexturesBox.SelectedItem == null)
				return;

			TileSet.LoadTexture(TexturesBox.SelectedItem as string);
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ToggleStripButtons(object sender, EventArgs e)
		{
			ToolStripButton button = sender as ToolStripButton;

			if (button.Checked)
				UncheckButtons(button);
		}



		#endregion



		#region Properties

		/// <summary>
		/// Handle to the asset
		/// </summary>
		public override IAsset Asset
		{
			get
			{
				return TileSet;
			}
		}


		/// <summary>
		/// Background texture
		/// </summary>
		Texture2D CheckerBoard;


		/// <summary>
		/// Tile to edit
		/// </summary>
		Tile CurrentTile;


		/// <summary>
		/// Current TileSet
		/// </summary>
		TileSet TileSet;


		/// <summary>
		/// Tile selection box
		/// </summary>
		SelectionTool TileSelection;

		/// <summary>
		/// Collision box
		/// </summary>
		SelectionTool CollisionSelection;


		/// <summary>
		/// Offset of the texture
		/// </summary>
		Point TextureOffset;


		/// <summary>
		/// Offset of the tile
		/// </summary>
		Point TileOffset;


		/// <summary>
		/// Offset of the mouse last update
		/// </summary>
		Point LastMousePos;


		/// <summary>
		/// 
		/// </summary>
		XmlNode Node;


		/// <summary>
		/// Spritebatch
		/// </summary>
		SpriteBatch Batch;

		#endregion

	}


}
