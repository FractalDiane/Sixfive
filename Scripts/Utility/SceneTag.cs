using Godot;
using System;

public class SceneTag : Node
{
	[Export]
	private string sceneName = string.Empty;

	[Export]
	private AudioStream sceneMusic;

	// ================================================================
	
	public override void _Ready()
	{
		if (sceneMusic != Controller.CurrentMusic)
		{
			Controller.PlayMusic(sceneMusic);
			Controller.CurrentMusic = sceneMusic;
		}
	}
}
