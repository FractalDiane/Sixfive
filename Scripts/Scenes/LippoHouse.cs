using Godot;
using System;

public class LippoHouse : Spatial
{
	[Export]
	private AudioStream soundKnock;

	[Export]
	private AudioStream soundPaper;

	[Export]
	private AudioStream sceneMusic;

	// Refs
	private PackedScene tutorialRef = GD.Load<PackedScene>("res://Instances/Tutorial.tscn");

	// ================================================================
	
	public override void _Ready()
	{
		if (Controller.Flag("intro") == 0)
			GetNode<Timer>("TimerIntro1").Start();
		
		if (Controller.Flag("read_note") == 1)
		{
			GetNode<NPC>("Note").QueueFree();
			GetNode<Door>("DoorArea").GetNode<CollisionShape>("CollisionShape").Disabled = false;
		}
	}

	// ================================================================

	public void ReadNote()
	{
		Controller.SetFlag("read_note", 1);
		GetNode<NPC>("Note").QueueFree();
		GetNode<Door>("DoorArea").GetNode<CollisionShape>("CollisionShape").Disabled = false;
	}

	// ================================================================

	private void Intro1()
	{
		Controller.PlaySoundBurst(soundKnock);
		GetNode<Timer>("TimerIntro2").Start();
	}


	private void Intro2()
	{
		Player.PlayAnimation("annoyed");
		GetNode<Timer>("TimerIntro3").Start();
	}


	private void Intro3()
	{
		Controller.PlaySoundBurst(soundPaper, volume: -20f);
		GetNode<Timer>("TimerIntro4").Start();
	}


	private void Intro4()
	{
		Player.Jump();
		Player.singleton.RotationDegrees = new Vector3(0, 0, 0);
		Player.PlayAnimation("dr_jumpup");
		GetNode<Timer>("TimerIntro5").Start();
	}


	private void Intro5()
	{
		Player.PlayAnimation("dr_jumpdown");
		GetNode<Timer>("TimerIntro6").Start();
	}


	private void Intro6()
	{
		Player.PlayAnimation("annoyed");
		GetNode<Timer>("TimerIntro7").Start();
	}


	private void Intro7()
	{
		BattleUI.Initialize();
		Controller.PlayMusic(sceneMusic);
		Controller.CurrentMusic = sceneMusic;
		Player.PlayAnimation("dr");
		Player.State = Player.ST.Move;
		var tut = (Sprite)tutorialRef.Instance();
		GetTree().GetRoot().AddChild(tut);
		Controller.SetFlag("intro", 1);
	}
}
