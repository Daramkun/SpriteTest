using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpriteTest
{
	public enum SceneContainMethod
	{
		Flat = 0,
		Stack = 1,
	}

	public sealed class SceneContainer : Scene
	{
		Stack<Scene> sceneList;
		Scene currentScene;
		Scene nextScene;

		public ISceneTransitor SceneTransitor { get; set; }
		public TransitionState TransitionState { get; set; }
		public SceneContainMethod ContainMethod { get; set; }

		public SceneContainer ( Scene firstScene )
		{
			sceneList = new Stack<Scene> ();
			if ( firstScene != null )
			{
				firstScene.Parent = this;
				sceneList.Push ( currentScene = firstScene );
			}

			ContainMethod = SceneContainMethod.Flat;
			SceneTransitor = new DirectTransitor ();
			TransitionState = TransitionState.None;

			( Children.Add ( new FPSCalculator () ) as FPSCalculator ).PrintFPS = true;
		}

		public void Transition ( Scene node )
		{
			if ( TransitionState != TransitionState.None ) return;
			nextScene = node;
			TransitionState = TransitionState.Begin;
		}

		public void PreviousTransition ()
		{
			if ( TransitionState != TransitionState.None ) return;
			if ( ContainMethod != SceneContainMethod.Stack ) return;
			if ( sceneList.Count == 1 ) return;
			nextScene = null;
			TransitionState = TransitionState.Begin;
		}

		public override void OnInitialize ()
		{
			currentScene.OnInitialize ();
			base.OnInitialize ();
		}

		public override void OnUninitialize ()
		{
			while ( sceneList.Count > 0 )
				sceneList.Pop ().OnUninitialize ();
			base.OnUninitialize ();
		}

		public override void OnUpdate ( GameTime gameTime )
		{
			if ( currentScene != null )
				currentScene.OnUpdate ( gameTime );
			base.OnUpdate ( gameTime );
		}

		public override void OnPostProcess ()
		{
			if ( currentScene != null )
				currentScene.OnPostProcess ();
			base.OnPostProcess ();
		}

		public override void OnDraw ( GameTime gameTime )
		{
			if ( currentScene != null )
				currentScene.OnDraw ( gameTime );

			if ( TransitionState != TransitionState.None )
			{
				TransitionState = SceneTransitor.Transitioning ( TransitionState, sceneList.Peek (), gameTime );
				if ( TransitionState == TransitionState.PretransitionEnd )
				{
					switch ( ContainMethod )
					{
						case SceneContainMethod.Flat:
							sceneList.Pop ().OnUninitialize ();
							nextScene.OnInitialize ();
							nextScene.Parent = this;
							sceneList.Push ( currentScene = nextScene );
							nextScene = null;
							break;
						case SceneContainMethod.Stack:
							if ( nextScene == null )
							{
								GameObject nn = sceneList.Pop ();
								nn.Parent = null;
								nn.OnUninitialize ();
								currentScene = null;
								nextScene = null;
							}
							else
							{
								nextScene.OnInitialize ();
								sceneList.Push ( currentScene = nextScene );
								nextScene = null;
							}
							break;
					}
					gameTime.Reset ();
				}
				else if ( TransitionState == TransitionState.End )
				{
					TransitionState = TransitionState.None;
					nextScene = null;
				}
			}
			base.OnDraw ( gameTime );
		}

		public override void OnPostDraw ()
		{
			if ( currentScene != null )
				currentScene.OnPostDraw ();
		}
	}
}
