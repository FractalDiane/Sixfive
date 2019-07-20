using Godot;
using System;

public class Path4 : Spatial
{
	[Export]
	private AudioStream sceneMusic;

	// ================================================================
	
	public override void _Ready()
	{
		if (Controller.Flag("boss") == 1)
		{
			GetNode<NPC>("NPCZincel").QueueFree();
			GetNode<StaticBody>("StaticBody").QueueFree();
		}
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
