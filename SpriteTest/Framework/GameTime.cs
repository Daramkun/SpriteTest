using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpriteTest
{
	public sealed class GameTime
	{
		Stopwatch stopwatch = new Stopwatch ();

		public TimeSpan ElapsedGameTime { get; private set; }
		public TimeSpan TotalGameTime { get; private set; }

		public GameTime () { stopwatch.Start (); }
		public void Reset () { stopwatch.Restart (); }

		internal void Update ()
		{
			ElapsedGameTime = stopwatch.Elapsed;
			TotalGameTime += ElapsedGameTime;

			Reset ();
		}
	}
}
