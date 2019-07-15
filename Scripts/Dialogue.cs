using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class Dialogue : CanvasLayer
{
	[Signal]
	public delegate void dialogue_ended();

	private const int BOX_TARGET_X = 600;
	private const int BOX_TARGET_Y = 150;

	private float boxOriginX = -1;
	private float boxOriginY = -1;

	private List<string> dialogue = new List<string>();
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
	private Sprite box;
	private Sprite pointer;
	private Label text;

	private Timer timerStartRoll;
	private Timer timerRollText;
	private Timer timerChangePage;
	private Timer timerBuffer;
	private Timer timerDestroy;

	// ================================================================
	
	public override void _Ready()
	{
		box = GetNode<Sprite>("DialogueBox");
		pointer = GetNode<Sprite>("DialogueBox").GetNode<Sprite>("Pointer");
		text = GetNode<Sprite>("DialogueBox").GetNode<Label>("Text");

		box.Scale = new Vector2(0, 0);
	}


	public override void _Process(float delta)
	{
		if (active)
		{
			sc = Mathf.Clamp(sc + 0.08f * delta * 60, 0, 1);
			alpha = Mathf.Clamp(alpha + 0.08f * delta * 60, 0, 1);

			if (box.Position.x < BOX_TARGET_X)
				box.Position = new Vector2(Mathf.Min(box.Position.x + 30 * delta * 60, BOX_TARGET_X), box.Position.y);
			if (box.Position.x > BOX_TARGET_X)
				box.Position = new Vector2(Mathf.Max(box.Position.x - 30 * delta * 60, BOX_TARGET_X), box.Position.y);

			box.Position = new Vector2(box.Position.x, Mathf.Max(box.Position.y - 30 * delta * 60, BOX_TARGET_Y));
		}
		else
		{
			sc = Mathf.Clamp(sc - 0.08f * delta * 60, 0, 1);
			alpha = Mathf.Clamp(alpha - 0.08f * delta * 60, 0, 1);

			if (box.Position.x < boxOriginX)
				box.Position = new Vector2(Mathf.Min(box.Position.x + 30 * delta * 60, boxOriginX), box.Position.y);
			if (box.Position.x > boxOriginX)
				box.Position = new Vector2(Mathf.Max(box.Position.x - 30 * delta * 60, boxOriginX), box.Position.y);

			box.Position = new Vector2(box.Position.x, Mathf.Min(box.Position.y + 30 * delta * 60, boxOriginY));
		}

		box.Scale = new Vector2(sc, sc);
		box.Modulate = new Color(1, 1, 1, alpha);
		pointer.Position = new Vector2(MainCamera.singleton.UnprojectPosition(target.Translation).x - 600, pointer.Position.y);

		text.VisibleCharacters = disp;
		text.Text = dialogue[textPage];

		Interaction();
	}

	// ================================================================

	public void Start()
	{
		// sound start play

		// Set box/pointer position
		boxOriginX = MainCamera.singleton.UnprojectPosition(target.Translation).x;
		boxOriginY = MainCamera.singleton.UnprojectPosition(target.Translation).y;
		box.Position = new Vector2(boxOriginX, boxOriginY);
		pointer.Position = new Vector2(boxOriginX - 600, 125);

		active = true;
		timerStartRoll.Start();
	}

	// ================================================================

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void Interaction()
	{
		if (!buffer && active && Input.IsActionJustPressed("action"))
		{
			if (disp < dialogue[textPage].Length)
			{
				disp = dialogue[textPage].Length;
				buffer = true;
				timerBuffer.Start();
			}
			else
			{
				// sound advance play
				if (textPage < dialogue.Count - 1)
				{
					textPage++;
					disp = 0;
					buffer = true;
					timerChangePage.Start();
				}
				else
				{
					// sound end play
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
		if (active && disp < dialogue[textPage].Length)
		{
			// sound type set pitch scale
			// sound type play
			/* if (dialogue[textPage][disp] == '|' || dialogue[textPage][disp] == '{')
				if (dialogue[textPage][disp] == '|')
					// timer wait set wait time 0.15
				if (dialogue[textPage][disp] == '{')
					// timer wait set wait time 0.33
				dialogue[textPage][disp - 1] = ''; */
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
