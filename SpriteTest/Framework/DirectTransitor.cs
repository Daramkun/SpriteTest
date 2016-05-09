using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpriteTest
{
	public class DirectTransitor : ISceneTransitor
	{
		public TransitionState Transitioning ( TransitionState currentState, GameObject scene, GameTime gameTime )
		{
			if ( currentState == TransitionState.Begin )
				return TransitionState.PretransitionEnd;
			else if ( currentState == TransitionState.PretransitionEnd )
				return TransitionState.End;
			else throw new ArgumentException ();
		}
	}
}
