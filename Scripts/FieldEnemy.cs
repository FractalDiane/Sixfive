using Godot;
using System;

public class FieldEnemy : KinematicBody
{
	[Export]
	private float speed = 1f;

	[Export]
	private BattleUI.Opponent enemy;
	
	private const float GRAVITY = -0.2f;

	private Vector3 vel = new Vector3(0, 0, 0);
	private bool alert = false;

	private Timer timerMove;
	private Timer timerStop;
	private AnimatedSprite3D spr;

	// ================================================================
	
	public override void _Ready()
	{
		timerMove = GetNode<Timer>("TimerMove");
		timerStop = GetNode<Timer>("TimerStop");
		spr = GetNode<AnimatedSprite3D>("Sprite");

		timerMove.WaitTime = (float)GD.RandRange(1.5, 2.8);
		timerMove.Start();
	}


	public override void _PhysicsProcess(float delta)
	{
		if (alert)
		{
			var transform = GetGlobalTransform().LookingAt(Player.singleton.GetGlobalTransform().origin, new Vector3(0, 1, 0));
			vel.x = -transform.basis.z.x;
			vel.z = -transform.basis.z.z;
			timerMove.Start();
		}

		spr.FlipH = vel.x > 0;

		//vel.y += GRAVITY * delta * 60f;
		MoveAndSlide(vel * speed, new Vector3(0, 1, 0));
	}

	// ================================================================

	private void Move()
	{
		float angle = (float)GD.RandRange(0, 359);
		float dir = Mathf.Deg2Rad(angle);
		vel.x = Mathf.Cos(dir);
		vel.z = Mathf.Sin(dir);

		timerStop.WaitTime = (float)GD.RandRange(2, 4);
		timerStop.Start();
	}


	private void Stop()
	{
		vel = Vector3.Zero;
		timerMove.WaitTime = (float)GD.RandRange(1.5, 4);
		timerMove.Start();
	}


	private void VisionAreaEntered(Node body)
	{
		if (body.IsInGroup("Player")  && Player.State == Player.ST.Move)
			alert = true;
	}


	private void VisionAreaExited(Node body)
	{
		if (body.IsInGroup("Player"))
			alert = false;
	}


	private void EncounterAreaEntered(Node body)
	{
		if (body.IsInGroup("Player") && Player.State == Player.ST.Move)
		{
			BattleUI.BattleStart(enemy, true, this);
		}
	}
}
