using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//Nick Baker, Jack Kalina, Vicente Bernal, Reese Lodwick
//2/12/2020
//Scarlet Meadow monogame project
//this stores frame count, frame textures, frame time, frame order, etc of an animation
namespace Scarlet_Meadow_Monogame
{
	/// <summary>
	/// this stores frame count, frame textures, frame time, frame order, etc of an animation
	/// </summary>
	public class Animation
    {

		/// <summary>
		/// How many pixels to offset this image from the position of the object on screen
		/// </summary>
		private Point offset;

		/// <summary>
		/// the amount to scale the frames of this animation by when drawing it
		/// </summary>
		private float scaleFactor;

		/// <summary>
		/// the index of the current frame
		/// </summary>
		private int currentFrameIndex;

		/// <summary>
		/// The amount of time (in milliseconds) to display each frame for
		/// </summary>
		private int frameTime;

		/// <summary>
		/// the current amount of time that the current frame has been displayed for
		/// </summary>
		private int tick;

		/// <summary>
		/// the array of frames for this animation
		/// </summary>
		private Texture2D[] frames;

		/// <summary>
		/// the order to play each frame in. the ints in here correspond to indices for the "frames" array
		/// </summary>
		private int[] frameOrder;

		/// <summary>
		/// Sets up the properties for this animation and stores its frames.
		/// </summary>
		/// <param name="frames">the array of frames for this animation</param>
		/// <param name="frameTime">The amount of time (in milliseconds) to display each frame for</param>
		/// <param name="frameOrder">the order to play each frame in. the ints in here correspond to indices for the "frames" array
		/// If null, the animation plays frames in the default order they are in inside the array</param>
		/// <param name="offset">How many pixels to offset this image from the position of the object on screen</param>
		/// <param name="scaleFactor">the amount to scale the frames of this animation by when drawing it</param>
		public Animation(Texture2D[] frames, int frameTime, int[] frameOrder, Point offset, float scaleFactor) {
			tick = 0;

			//store our frame data, time, and scale factor
			this.frames = frames;
			this.frameTime = frameTime;
			this.scaleFactor = scaleFactor;

			//if the frame order is null...
			if (frameOrder == null) 
			{
				//...make a frame order that is just the default order
				this.frameOrder = new int[frames.Length];
				for(int i = 0; i < frames.Length; i++) {
					this.frameOrder[i] = i;
				}
			}
			//if the frame order array is not null...
			else 
			{
				//...store it!
				this.frameOrder = frameOrder;
			}


			//if offset is null, then it should just be a default point
			if(offset == null) {
				this.offset = new Point();
			}
			//if not, store it!
			else
			{

				this.offset = offset;
			}
		}
		
		/// <summary>
		/// Gets the current frame of the animation
		/// </summary>
		public Texture2D CurrentFrame {
			get{ return frames[frameOrder[currentFrameIndex]]; }
		}
		
		/// <summary>
		/// Gets the offset on screen to render the animation at
		/// </summary>
		public Point Offset {
			get{ return offset; }
		}

		/// <summary>
		/// Gets how much to scale the image of the animation
		/// </summary>
		public float ScaleFactor
		{
			get { return scaleFactor; }
		}
		
		/// <summary>
		/// Resets the animation's timer back to 0 and its current frame back to the start
		/// </summary>
		public void Reset() {
			tick = 0;
			currentFrameIndex = 0;
		}
	
		/// <summary>
		/// counts up on the tick, when enough time has elapsed, go to the next frame
		/// </summary>
		/// <param name="gameTime">keeping track of how much time has elapsed in each frame of the game</param>
		public void Update(GameTime gameTime) {
			//if frame time is 0, dont update it, just keep it still
			if(frameTime > 0) {
				//add the elapsed time
				tick += (int)gameTime.ElapsedGameTime.TotalMilliseconds;

				//if enough time has passed
				if(tick >= frameTime) {
					//reset the clock
					tick = 0;


					//then go to the next frame:

					//if we are at the end of the frame order, go back to the start
					if(currentFrameIndex + 1 == frameOrder.Length) {
						currentFrameIndex = 0;
					}
					//if not, keep going forward
					else {
						currentFrameIndex++;
					}
				}
			}
		}

		/// <summary>
		/// draws the animation's current frame to the screen
		/// </summary>
		/// <param name="sb">the spritebatch to draw to</param>
		/// <param name="transform">the gameobject's transform. used to get the position in the game world to draw to</param>
		/// <param name="cameraOffset">the current offset/position of the camera</param>
		/// <param name="color">the color to tint the animation</param>
		/// <param name="facingLeft">if this is true, the animation is drawn flipped so its facing to the left</param>
		public void Draw(SpriteBatch sb, Rectangle transform, Vector2 cameraOffset, Color color, bool facingLeft)
		{
			//determines the top left position to start drawing from and the the width and height to draw with
			Rectangle destinationRectangle = new Rectangle(transform.X - (int)cameraOffset.X + Offset.X,
				transform.Y - (int)cameraOffset.Y + Offset.Y,
				(int)(CurrentFrame.Width * ScaleFactor),
				(int)(CurrentFrame.Height * ScaleFactor));
			if (facingLeft)
			{
				//if this object is facing left, flip it horizontally
				//(all sprites should be drawn facing right originally)
				sb.Draw(CurrentFrame, destinationRectangle, null, color, 0, Vector2.Zero, SpriteEffects.FlipHorizontally, 0);
			}
			else
			{
				sb.Draw(CurrentFrame, destinationRectangle, color);
			}
		}

		/// <summary>
		/// Gets a copy of an animation array, with references to new animations
		/// </summary>
		/// <param name="arrayToCopy">the animation array to copy</param>
		/// <returns>the final copy of the animation array</returns>
		public static Animation[] GetCopy(Animation[] arrayToCopy)
		{
			//makes a new array with the same length
			Animation[] finalCopy = new Animation[arrayToCopy.Length];

			//going through the array and getting a copy of each animation
			for(int i = 0; i < arrayToCopy.Length; i++)
			{
				if (arrayToCopy[i] != null)
					finalCopy[i] = arrayToCopy[i].GetCopy();
				else
					finalCopy[i] = null;
			}

			//finally return the copy
			return finalCopy;
		}

		/// <summary>
		/// Copies the data of an animation to a new reference and returns the copy
		/// </summary>
		/// <returns>the copy of the animation</returns>
		public Animation GetCopy()
		{
			return new Animation(frames, frameTime, frameOrder, offset, scaleFactor);
		}
	}
}
