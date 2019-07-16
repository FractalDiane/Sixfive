using Godot;
using System;


public class NPC : KinematicBody
{
	[Export]
	private bool isObject = false;

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
		if (Input.IsActionJustPressed("action") && inArea)
		{
			inArea = false;
			if (isObject)
				Player.ShowInteract(false);

			Controller.DisplayDialogue(dialogue[dialogueSet], this);
		}
	}

	// ================================================================

	private void SightEntered(Area area)
	{
		if (area.IsInGroup("PlayerSight"))
		{
			inArea = true;
			if (isObject)
				Player.ShowInteract(true);
		}
	}


	private void SightExited(Area area)
	{
		if (area.IsInGroup("PlayerSight"))
		{
			inArea = false;
			if (isObject)
				Player.ShowInteract(false);
		}
	}


	private void DialogueEnd()
	{
		if (advanceSet)
			dialogueSet = Mathf.Min(++dialogueSet, maxSet);

		inArea = true;
		if (isObject)
			Player.ShowInteract(true);
	}
}
