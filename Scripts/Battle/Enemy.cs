using Godot;
using System;

public class Enemy : KinematicBody
{
	[Export]
	protected string name = string.Empty;

	[Export]
	protected int maxHP = 5;

	protected int hp;

	private Vector3 vel = new Vector3(0, 0, 0);

	private const float GRAVITY = -0.2f;

	// Refs
	private Particles partsAttack;

	// ================================================================

	public int Hp { get => hp; set => hp = value; }

	// ================================================================
	
	public override void _Ready()
	{
		partsAttack = GetNode<Particles>("PartsAttack");
	}


	public override void _PhysicsProcess(float delta)
	{
		vel.y += GRAVITY * delta * 60f;

		vel = MoveAndSlide(vel, new Vector3(0, 1, 0));
	}

	// ================================================================

	public void Initialize()
	{
		hp = maxHP;
	}
}
