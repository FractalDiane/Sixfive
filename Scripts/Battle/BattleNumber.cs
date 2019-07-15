using Godot;
using System;

public class BattleNumber : Node2D
{
	private const float GRAVITY = 980f;
	private const float STARTSPEED_X = 8f;
	private const float STARTSPEED_Y = -500f;

	private Vector2 vel = new Vector2(STARTSPEED_X, STARTSPEED_Y);

	// Refs
	//private Label text;

	// ================================================================
	
	public override void _Ready()
	{
		//text = GetNode<Label>("Label");
	}


	public override void _PhysicsProcess(float delta)
	{
		vel.y += GRAVITY * delta;
		Translate(vel * delta);

		
	}

	// ================================================================

	public void SetNumber(int number)
	{
		GetNode<Label>("Label").Text = number.ToString();
	}


	public void SetHealing(bool value)
	{
		GetNode<Label>("Label").AddColorOverride("font_color", value ? new Color("#70f7bb") : new Color("#ffffff"));
	}
}
