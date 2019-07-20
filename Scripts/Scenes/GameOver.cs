using Godot;
using System;

public class GameOver : Node2D
{

	// ================================================================

	private void ControllerFade()
	{
		Controller.Fade(true, 1.5f);
	}
	
	private void ReloadSave()
	{
		Controller.LoadGame();
	}
}
