using Godot;
using System;

public class SavePoint : StaticBody
{
	[Export]
	private AudioStream saveSound;

	private bool inArea = false;

	// Refs
	private PackedScene healParts = GD.Load<PackedScene>("res://Instances/Attack Animations/AnimHeal.tscn");

	// ================================================================
	
	public override void _Process(float delta)
	{
		if (Input.IsActionJustPressed("action") && inArea)
		{
			inArea = false;
			Player.ShowInteract(false);
			Controller.PlaySoundBurst(saveSound);
			var anim = (Spatial)healParts.Instance();
			anim.Translation = Player.singleton.Translation;
			GetTree().GetRoot().AddChild(anim);
			Controller.SaveMessage();
			Controller.SaveGame();
		}
	}

	// ================================================================

	private void AreaEntered(Node body)
	{
		if (body.IsInGroup("Player") && Player.State == Player.ST.Move)
		{
			Player.ShowInteract(true);
			inArea = true;
		}
	}


	private void AreaExited(Node body)
	{
		if (body.IsInGroup("Player") && Player.State == Player.ST.Move)
		{
			Player.ShowInteract(false);
			inArea = false;
		}
	}
}
