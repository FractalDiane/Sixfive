using Godot;
using System;

public class Enemy : KinematicBody
{
	[Export]
	protected string name = string.Empty;

	[Export]
	protected SpriteFrames spriteFrames;

	[Export]
	protected int maxHP = 5;

	protected int hp;

	private Vector3 vel = new Vector3(0, 0, 0);

	private const float GRAVITY = -0.2f;

	// ================================================================
	
	public override void _Ready()
	{
		hp = maxHP;
	}


	public override void _PhysicsProcess(float delta)
	{
		vel.y += GRAVITY * delta * 60f;

		vel = MoveAndSlide(vel, new Vector3(0, 1, 0));
	}
}
