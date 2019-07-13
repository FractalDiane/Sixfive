using Godot;
using System;
using System.Collections.Generic;

public class Controller : Node
{
	public static Controller singleton;

	Controller()
	{
		singleton = this;
	}

	private Dictionary<string, int> flag = new Dictionary<string, int>()
	{

	};


	// Refs
	private PackedScene soundBurstRef = GD.Load<PackedScene>("res://Instances/SoundBurst.tscn");

	private AnimationPlayer animPlayer;

	// ================================================================
	
	public override void _Ready()
	{
		GD.Randomize();
		animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
	}

	// ================================================================

	public static void PlaySoundBurst(AudioStream sound, float volume = 0f, float pitch = 1f)
	{
		var sb = (AudioStreamPlayer)Controller.singleton.soundBurstRef.Instance();
		sb.Stream = sound;
		sb.VolumeDb = volume;
		sb.PitchScale = pitch;
		sb.Play();
		Controller.singleton.GetTree().GetRoot().AddChild(sb);
	}


	public static void GotoScene(string targetScene)
	{
		Controller.singleton.GotoScenePre();
		Controller.singleton.GetTree().ChangeScene(targetScene);
		Controller.singleton.GotoScenePost();
	}


	public static void Fade(bool fadeout, float time)
	{
		Controller.singleton.animPlayer.PlaybackSpeed = 1f / time;
		Controller.singleton.animPlayer.Play(fadeout ? "Fadeout" : "Fadein");
	}

	// ================================================================

	private void GotoScenePre()
	{

	}


	private void GotoScenePost()
	{
	//	Player.ReacquireCamera();
	}
}
