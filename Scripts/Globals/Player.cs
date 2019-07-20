using Godot;
using System;
using System.Runtime.CompilerServices;

public class Player : KinematicBody
{
	public static Player singleton;

	Player()
	{
		singleton = this;
	}

	private Vector3 vel = new Vector3(0, 0, 0);
	private float rotSpeed = 0f;
	private int rotDir = 1;
	private float targetAngle = 0f;

	private Vector2 face = new Vector2(1, 1);
	private bool walking = false;
	private bool jumping = false;

	private const float WALKSPEED = 1;
	private const float GRAVITY = -0.2f;
	private const float JUMP_FORCE = 3.5f;

	private int sound = -1;

	private float sightStartX;
	private float sightStartZ;

	public enum ST { Move, Battle, Cutscene, NoInput };
	private ST state = ST.Move;

	// Refs
	private AnimatedSprite3D spr;
	private Area sight;
	private Camera camera;
	private Sprite3D interact;
	private Timer timerShowUI;

	// ================================================================

	public static ST State { get => Player.singleton.state; set => Player.singleton.state = value; }
	public static Vector2 Face { get => Player.singleton.face; set => Player.singleton.face = value; }
	public static Vector3 Vel { get => Player.singleton.vel; set => Player.singleton.vel = value; }

	// ================================================================
	
	public override void _Ready()
	{
		spr = GetNode<AnimatedSprite3D>("Sprite");
		sight = GetNode<Area>("Sight");
		camera = MainCamera.singleton;
		interact = GetNode<Sprite3D>("Interaction");
		timerShowUI = GetNode<Timer>("TimerShowUI");

		sightStartX = sight.Translation.x;
		sightStartZ = sight.Translation.z;
	}


	public override void _PhysicsProcess(float delta)
	{
		switch (state)
		{
			case ST.Move:
			{
				Movement(delta);
				walking = (vel.x != 0 || vel.z != 0) && state == ST.Move;
				break;
			}

			case ST.Battle:
			{
				spr.Play("dr_walk");
				break;
			}
		}

		if (walking && !BattleUI.BattleMode)
		{
			BattleUI.ShowUI(false);
			timerShowUI.Start();
		}
			

		jumping = !IsOnFloor();
		if (!IsOnFloor())
			vel.y += GRAVITY * delta * 60f;

		if (state != ST.Battle)
			spr.RotationDegrees = new Vector3(spr.RotationDegrees.x, Mathf.Clamp(spr.RotationDegrees.y + rotSpeed * Mathf.Abs(spr.RotationDegrees.y - targetAngle) * rotDir, 0, 179), spr.RotationDegrees.z);
	
		if (state != ST.Battle && state != ST.Cutscene)
			SpriteAnimation();

		SightMovement();
		MoveAndSlide(vel * WALKSPEED, new Vector3(0, 1, 0));

		// add if here
		CamreaMovement();
	}

	// ================================================================

	public static void Stop()
	{
		Player.singleton.state = ST.NoInput;
		Player.singleton.vel.x = 0;
		Player.singleton.vel.z = 0;
		Player.singleton.walking = false;
	}


	public static void NeutralizeRotation()
	{
		Player.singleton.spr.RotationDegrees = Vector3.Zero;
	}


	public static void SnapCamera()
	{
		Player.singleton.camera.Translation = new Vector3(Player.singleton.Translation.x, Player.singleton.camera.Translation.y, Player.singleton.camera.Translation.z + 1.5f);
	}


	public static void PlayAnimation(string anim)
	{
		Player.singleton.spr.Play(anim);
	}


	public static void ShowInteract(bool show)
	{
		Player.singleton.interact.Visible = show;
	}


	public static void Jump()
	{
		Player.singleton.vel.y = JUMP_FORCE;
		Player.singleton.jumping = true;
	}

	// ================================================================

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void Movement(float delta)
	{
		int x1 = Input.IsActionPressed("move_right") ? 1 : 0;
		int x2 = Input.IsActionPressed("move_left") ? 1 : 0;
		int z1 = Input.IsActionPressed("move_down") ? 1 : 0;
		int z2 = Input.IsActionPressed("move_up") ? 1 : 0;

		vel.x = x1 - x2;
		vel.z = z1 - z2;

		if (Input.IsActionJustPressed("move_up"))
			face.y = -1;

		if (Input.IsActionJustPressed("move_down"))
			face.y = 1;

		if (Input.IsActionJustPressed("move_left"))
		{
			if (face.x != -1)
			{
				rotSpeed = 0.07f * delta * 60;
				rotDir = 1;
				targetAngle = 179;
				face.x = -1;
			}

			if (!Input.IsActionPressed("move_up"))
				face.y = 1;
		}

		if (Input.IsActionJustPressed("move_right"))
		{
			if (face.x != 1)
			{
				rotSpeed = 0.07f * delta * 60;
				rotDir = -1;
				targetAngle = 0;
				face.x = 1;
			}

			if (!Input.IsActionPressed("move_up"))
				face.y = 1;
		}

		if (Input.IsActionJustPressed("move_jump") && IsOnFloor())
		{
			vel.y = JUMP_FORCE;
			jumping = true;
		}
	}


	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void SpriteAnimation()
	{
		switch (face.y)
		{
			case -1: // Facing up
			{
				if (face.x == -1 && spr.RotationDegrees.y > 90)
				{
					spr.Play(walking ? "ul_walk" : "ul");
					if (jumping)
						spr.Play("ul_jumpup");
				}
				else if (face.x == 1 && spr.RotationDegrees.y < 90)
				{
					spr.Play(walking ? "ur_walk" : "ur");
					if (jumping)
						spr.Play("ur_jumpup");
				}

				break;
			}
			case 1: // Facing down
			{
				if (face.x == -1 && spr.RotationDegrees.y > 90)
				{
					spr.Play(walking ? "dl_walk" : "dl");
					if (jumping)
						spr.Play(vel.y > 0 ? "dl_jumpup" : "dl_jumpdown");
				}
				else if (face.x == 1 && spr.RotationDegrees.y < 90)
				{
					spr.Play(walking ? "dr_walk" : "dr");
					if (jumping)
						spr.Play(vel.y > 0 ? "dr_jumpup" : "dr_jumpdown");
				}

				break;
			}
		}
	}


	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void SightMovement()
	{
		sight.Translation = new Vector3(sightStartX * face.x, sight.Translation.y, sight.Translation.z);
	}


	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void CamreaMovement()
	{
		if (camera.Translation.x < Translation.x)
			camera.Translation = new Vector3(Mathf.Min(camera.Translation.x + (0.05f * Mathf.Abs(Translation.x - camera.Translation.x)), Translation.x), camera.Translation.y, camera.Translation.z);
		else if (camera.Translation.x > Translation.x)
			camera.Translation = new Vector3(Mathf.Max(camera.Translation.x - (0.05f * Mathf.Abs(Translation.x - camera.Translation.x)), Translation.x), camera.Translation.y, camera.Translation.z);

		if (camera.Translation.z < Translation.z + 1.5f)
			camera.Translation = new Vector3(camera.Translation.x, camera.Translation.y, Mathf.Min(camera.Translation.z + (0.05f * Mathf.Abs(Translation.z + 1.5f - camera.Translation.z)), Translation.z + 1.5f));
		else if (camera.Translation.z > Translation.z + 1.5f)
			camera.Translation = new Vector3(camera.Translation.x, camera.Translation.y, Mathf.Max(camera.Translation.z - (0.05f * Mathf.Abs(Translation.z + 1.5f - camera.Translation.z)), Translation.z + 1.5f));
	}


	private void BodyAreaEntered(Area area)
	{
		if (area.IsInGroup("Interactible"))
			interact.Show();

		if (area.IsInGroup("Door"))
		{
			var doorCast = (Door)area;
			doorCast.InArea = true;
		}
	}


	private void BodyAreaExited(Area area)
	{
		if (area.IsInGroup("Interactible"))
			interact.Hide();

		if (area.IsInGroup("Door"))
		{
			var doorCast = (Door)area;
			doorCast.InArea = false;
		}
	}


	private void SightAreaEntered(Area area)
	{
		if (area.IsInGroup("InteractibleSight"))
			interact.Show();
	}


	private void SightAreaExited(Area area)
	{
		if (area.IsInGroup("InteractibleSight"))
			interact.Hide();
	}


	private void ShowUI()
	{
		if (!BattleUI.BattleMode && state == ST.Move)
			BattleUI.ShowUI(true);
	}
}
