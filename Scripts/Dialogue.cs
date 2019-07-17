using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class Dialogue : Sprite
{
	[Signal]
	public delegate void dialogue_ended();

	private const int BOX_TARGET_X = 600;
	private const int BOX_TARGET_Y = 150;

	private float boxOriginX = -1;
	private float boxOriginY = -1;

	private List<string> dlgText = new List<string>();
	private int textPage = 0;
	private float interval = 0.02f;

	private Spatial target = null;

	private float t = 0;
	private int disp = 0;
	private int sound = 0;

	private float sc = 0;
	private float alpha = 0;

	private bool buffer = false;

	private bool active = false;
	private bool roll = false;

	// Refs
	//private Sprite box;
	//private Sprite pointer;
	private Label text;
	private AnimationPlayer animPlayer;

	private Timer timerStartRoll;
	private Timer timerRollText;
	private Timer timerChangePage;
	private Timer timerBuffer;
	private Timer timerDestroy;

	// ================================================================

	//public List<string> DlgText { get => dlgText; set => dlgText = value; }
	public Spatial Target { get => target; set => target = value; }

	// ================================================================
	
	public override void _Ready()
	{
		//box = GetNode<Sprite>("DialogueBox");
		//pointer = GetNode<Sprite>("DialogueBox").GetNode<Sprite>("Pointer");
		text = GetNode<Label>("Text");
		animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

		timerStartRoll = GetNode<Timer>("TimerStartRoll");
		timerRollText = GetNode<Timer>("TimerRollText");
		timerChangePage = GetNode<Timer>("TimerChangePage");
		timerBuffer = GetNode<Timer>("TimerBuffer");
		timerDestroy = GetNode<Timer>("TimerDestroy");

		//box.Scale = new Vector2(0, 0);
	}


	public override void _Process(float delta)
	{
		text.VisibleCharacters = disp;
		text.Text = dlgText[textPage];

		Interaction();
	}

	// ================================================================

	public void Start()
	{
		// sound start play
		GetNode<AnimationPlayer>("AnimationPlayer").Play("Appear");

		active = true;
		GetNode<Timer>("TimerStartRoll").Start();
	}


	public void LoadDialogue(string[] text)
	{
		foreach (string s in text)
			dlgText.Add(s);
	}

	// ================================================================

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void Interaction()
	{
		if (!buffer && active && Input.IsActionJustPressed("action"))
		{
			if (disp < dlgText[textPage].Length)
			{
				disp = dlgText[textPage].Length;
				buffer = true;
				timerBuffer.Start();
			}
			else
			{
				// sound advance play
				if (textPage < dlgText.Count - 1)
				{
					textPage++;
					disp = 0;
					buffer = true;
					timerChangePage.Start();
				}
				else
				{
					// sound end play
					animPlayer.Play("Disappear");
					disp = 0;
					active = false;
					timerDestroy.Start();
				}
			}
		}
	}


	private void StartRoll()
	{
		timerRollText.WaitTime = interval;
		timerRollText.Start();
		// target start talking
		roll = true;
	}

	private void RollText()
	{
		if (active && disp < dlgText[textPage].Length)
		{
			// sound type set pitch scale
			// sound type play
			/* if (dlgText[textPage][disp] == '|' || dlgText[textPage][disp] == '{')
				if (dlgText[textPage][disp] == '|')
					// timer wait set wait time 0.15
				if (dlgText[textPage][disp] == '{')
					// timer wait set wait time 0.33
				dlgText[textPage][disp - 1] = ''; */
			disp++;
		}
		else
		{
			timerRollText.Stop();
		}
	}


	private void ChangePage()
	{
		StartRoll();
		timerBuffer.Start();
	}


	private void ResetBuffer()
	{
		buffer = false;
	}


	private void Destroy()
	{
		Player.State = Player.ST.Move;
		// target interacting false
		// hide target interaction marker
		EmitSignal(nameof(dialogue_ended));
		QueueFree();
	}
}
