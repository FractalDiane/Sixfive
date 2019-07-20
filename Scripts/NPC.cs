using Godot;
using System;


public class NPC : KinematicBody
{
	[Signal]
	public delegate void dialogue_started();

	[Signal]
	public delegate void dialogue_ended();

	[Export]
	private bool isObject = false;

	[Export]
	private bool neutralArea = false;

	[Export]
	private Godot.Collections.Array<string[]> dialogue;

	[Export]
	private bool advanceSet = true;

	[Export]
	private int maxSet;

	private int dialogueSet = 0;
	private bool inArea = false;

	// ================================================================
	
	public override void _Ready()
	{

	}


	public override void _Process(float delta)
	{
		if (Input.IsActionJustPressed("action") && inArea && !BattleUI.PreBattle)
		{
			Player.Stop();
			inArea = false;
			if (isObject)
				Player.ShowInteract(false);

			EmitSignal(nameof(dialogue_started));
			Controller.DisplayDialogue(dialogue[dialogueSet], this);
		}

		
	}

	// ================================================================

	private void SightEntered(Area area)
	{
		if ((area.IsInGroup("PlayerSight") && !neutralArea) || (area.IsInGroup("PlayerBody") && neutralArea))
		{
			inArea = true;
			if (isObject)
				Player.ShowInteract(true);
		}
	}


	private void SightExited(Area area)
	{
		if ((area.IsInGroup("PlayerSight") && !neutralArea) || (area.IsInGroup("PlayerBody") && neutralArea))
		{
			inArea = false;
			if (isObject)
				Player.ShowInteract(false);
		}
	}


	private void DialogueEnd()
	{
		EmitSignal(nameof(dialogue_ended));
		
		if (advanceSet)
			dialogueSet = Mathf.Min(++dialogueSet, maxSet);

		inArea = true;
		if (isObject)
			Player.ShowInteract(true);
	}
}
