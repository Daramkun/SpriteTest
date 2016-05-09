using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpriteTest
{
	public enum TransitionState
	{
		None,
		Begin,
		Pretransition,
		PretransitionEnd,
		Posttransition,
		End,
	}

	public interface ISceneTransitor
	{
		TransitionState Transitioning ( TransitionState currentState, GameObject scene, GameTime gameTime );
	}
}
