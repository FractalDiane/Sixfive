using Godot;
using System;

public class Door : Area
{
	[Export]
	private AudioStream doorSound;

	[Export(PropertyHint.File, "*.tscn")]
	private string targetScene = string.Empty;

	[Export]
	private Vector3 targetPosition;

	/* [Export(PropertyHint.Enum, "Up Left,Up Right,Down Left,Down Right")]
	private int targetDirection; */

	private bool inArea = false;

	// Refs
	private Timer timer;

	// ================================================================

	public bool InArea { set => inArea = value; }

	// ================================================================
	
	public override void _Ready()
	{
		timer = GetNode<Timer>("Timer");
	}


	public override void _Process(float delta)
	{
		if (Input.IsActionJustPressed("action") && inArea && !BattleUI.PreBattle)
		{
			Player.State = Player.ST.NoInput;
			Player.Stop();
			Controller.PlaySoundBurst(doorSound);
			Controller.Fade(true, 0.5f);
			timer.Start();
		}
	}

	// ================================================================

	private void TimerTimeout()
	{
		Controller.GotoScene(targetScene);
		Player.singleton.Translation = targetPosition;

		/* switch (targetDirection)
		{
			case 0:
				Player.Face = new Vector2(-1, -1);
				break;
			case 1:
				Player.Face = new Vector2(1, -1);
				break;
			case 2:
				Player.Face = new Vector2(-1, 1);
				break;
			case 3:
				Player.Face = new Vector2(1, 1);
				break;
		} */

		//Player.Face = new Vector2(1, 1);

		Player.SnapCamera();
		Controller.Fade(false, 0.5f);
		Player.State = Player.ST.Move;
	}
}
