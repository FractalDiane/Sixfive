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
		{"intro", 0},
		{"read_note", 0}
	};

	private Vector3 preBattlePosition;
	private Vector2 preBattleFace;
	private string preBattleScene;
	private NodePath preBattleEnemy = null;
	private AudioStream prebattleMusic;

	private AudioStream currentMusic;

	// Refs
	private PackedScene soundBurstRef = GD.Load<PackedScene>("res://Instances/SoundBurst.tscn");
	private PackedScene dialogueRef = GD.Load<PackedScene>("res://Instances/Dialogue.tscn");

	private AnimationPlayer animPlayer;
	private AnimationPlayer animPlayer2;
	private AudioStreamPlayer soundTransition;
	private AudioStreamPlayer music;

	// ================================================================

	public static Vector3 PreBattlePosition { get => Controller.singleton.preBattlePosition; set => Controller.singleton.preBattlePosition = value; }
	public static Vector2 PreBattleFace { get => Controller.singleton.preBattleFace; set => Controller.singleton.preBattleFace = value; }
	public static string PreBattleScene { get => Controller.singleton.preBattleScene; set => Controller.singleton.preBattleScene = value; }
	public static NodePath PreBattleEnemy { get => Controller.singleton.preBattleEnemy; set => Controller.singleton.preBattleEnemy = value; }
	public static AudioStream PreBattleMusic { get => Controller.singleton.prebattleMusic; set => Controller.singleton.prebattleMusic = value; }
	public static AudioStream CurrentMusic { get => Controller.singleton.currentMusic; set => Controller.singleton.currentMusic = value; }

	// ================================================================
	
	public override void _Ready()
	{
		GD.Randomize();
		animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		animPlayer2 = GetNode<AnimationPlayer>("AnimationPlayer2");
		soundTransition = GetNode<AudioStreamPlayer>("SoundTransition");
		music = GetNode<AudioStreamPlayer>("Music");
	}


	public override void _Process(float delta)
	{
		if (Input.IsActionJustPressed("sys_fullscreen"))
			OS.WindowFullscreen ^= true;
	}

	// ================================================================

	public static void SetFlag(string flag, int value)
	{
		Controller.singleton.flag[flag] = value;
	}


	public static int Flag(string flag)
	{
		return Controller.singleton.flag[flag];
	}
	

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
		BattleUI.ShowUI(false);
		Player.State = Player.ST.NoInput;
		var dlg = (Dialogue)Controller.singleton.dialogueRef.Instance();
		dlg.LoadDialogue(text);
		dlg.Target = host;
		dlg.Connect("dialogue_ended", host, "DialogueEnd");
		dlg.Start();
		Controller.singleton.GetTree().GetRoot().AddChild(dlg);
	}


	public static void Transition()
	{
		Controller.singleton.soundTransition.Play();
		Controller.singleton.animPlayer2.Play("Transition");
	}


	public static void PlayMusic(AudioStream music, float offset = 0)
	{
		Controller.singleton.music.VolumeDb = 0;
		Controller.singleton.music.Stream = music;
		Controller.singleton.music.Play(offset);
	}


	public static void FadeOutMusic(float time)
	{
		Controller.singleton.GetNode<AnimationPlayer>("AnimationPlayerMusic").PlaybackSpeed = 1f / time;
		Controller.singleton.GetNode<AnimationPlayer>("AnimationPlayerMusic").Play("Fadeout Music");
	}


	public static void StopMusic()
	{
		Controller.singleton.music.Stop();
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
