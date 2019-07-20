using Godot;
using System;

public class Path4 : Spatial
{
	[Export]
	private AudioStream sceneMusic;

	// ================================================================
	
	public override void _Ready()
	{
		
	}

	// ================================================================

	private void PlaySceneMusic()
	{
		Controller.PlayMusic(sceneMusic);
	}


	private void SceneBattle()
	{
		Player.ShowInteract(false);
		BattleUI.BattleStart(BattleUI.Opponent.BossZincel, false, boss: true);
	}
}
