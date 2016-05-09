using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpriteTest
{
	public class FPSCalculator : GameObject
	{
		TimeSpan sum;
		int frame;

		double fps;

		public bool PrintFPS { get; set; }
		public float FPS { get { return ( float ) fps; } }

		public override void OnUpdate ( GameTime gameTime )
		{
			sum += gameTime.ElapsedGameTime;
			++frame;

			if ( sum.TotalSeconds >= 1 )
			{
				fps = frame + frame * ( sum.TotalSeconds - 1 );
				frame = 0;
				sum -= TimeSpan.FromSeconds ( 1 );

				if ( PrintFPS )
					Print ();
			}
		}

		private void Print ()
		{
			Debug.WriteLine ( "FPS: {0}", FPS );
		}
	}
}
