using Godot;
using System;

public class SceneTag : Node
{
	[Export]
	private string sceneName = string.Empty;

	[Export]
	private AudioStream sceneMusic;

	[Export]
	private bool startSilent = false;

	[Export]
	private string musicConditionFlag = string.Empty;

	// ================================================================
	
	public override void _Ready()
	{
		if (sceneMusic != Controller.CurrentMusic)
		{
			if ((startSilent && Controller.Flag(musicConditionFlag) == 1) || !startSilent)
			{
				Controller.PlayMusic(sceneMusic);
				Controller.CurrentMusic = sceneMusic;
			}
		}
	}
}
