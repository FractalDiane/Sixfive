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
		{"read_note", 0}
	};

	private Vector3 preBattlePosition;
	private Vector2 preBattleFace;
	private string preBattleScene;


	// Refs
	private PackedScene soundBurstRef = GD.Load<PackedScene>("res://Instances/SoundBurst.tscn");
	private PackedScene dialogueRef = GD.Load<PackedScene>("res://Instances/Dialogue.tscn");

	private AnimationPlayer animPlayer;

	// ================================================================

	public static Vector3 PreBattlePosition { get => Controller.singleton.preBattlePosition; set => Controller.singleton.preBattlePosition = value; }
	public static Vector2 PreBattleFace { get => Controller.singleton.preBattleFace; set => Controller.singleton.preBattleFace = value; }
	public static string PreBattleScene { get => Controller.singleton.preBattleScene; set => Controller.singleton.preBattleScene = value; }

	// ================================================================
	
	public override void _Ready()
	{
		GD.Randomize();
		animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
	}


	public override void _Process(float delta)
	{
		if (Input.IsActionJustPressed("sys_fullscreen"))
			OS.WindowFullscreen ^= true;
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


	public static void DisplayDialogue(string[] text, Spatial host)
	{
		Player.State = Player.ST.NoInput;
		var dlg = (Dialogue)Controller.singleton.dialogueRef.Instance();
		dlg.LoadDialogue(text);
		dlg.Target = host;
		dlg.Connect("dialogue_ended", host, "DialogueEnd");
		dlg.Start();
		Controller.singleton.GetTree().GetRoot().AddChild(dlg);
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
