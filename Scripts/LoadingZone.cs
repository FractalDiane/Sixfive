using Godot;
using System;

public class LoadingZone : Area
{
	[Export(PropertyHint.File, "*.tscn")]
	private string targetScene = string.Empty;

	[Export]
	private Vector3 targetPosition;

	// Refs
	private Timer timer;

	// ================================================================
	
	public override void _Ready()
	{
		timer = GetNode<Timer>("Timer");
	}

	// ================================================================

	private void PlayerEntered(Node body)
	{
		if (body.IsInGroup("Player"))
		{
			Player.State = Player.ST.NoInput;
			Player.Stop();
			Controller.Fade(true, 0.5f);
			timer.Start();
		}
	}


	private void TimerTimeout()
	{
		Controller.GotoScene(targetScene);
		Player.singleton.Translation = targetPosition;
		Player.SnapCamera();
		Controller.Fade(false, 0.5f);
		Player.State = Player.ST.Move;
	}
}
