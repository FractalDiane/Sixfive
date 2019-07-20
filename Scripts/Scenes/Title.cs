using Godot;
using System;

public class Title : Node2D
{
	[Export(PropertyHint.File, "*.tscn")]
	private string startScene = string.Empty;

	[Export]
	private Vector3 startPosition;

	[Export]
	private AudioStream menuMusic;

	[Export]
	private AudioStream soundMenu1;

	[Export]
	private AudioStream soundMenu2;
	
	[Export]
	private AudioStream soundMenu3;

	[Export]
	private AudioStream soundSelect;

	private bool hover1 = false;
	private bool hover2 = false;
	private bool hover3 = false;

	private bool selected = false;
	private bool creditsOpen = false;

	private const float COLOR_LOWER = 0.3f;
	private const float LERP_WEIGHT = 0.05f;
	private const float SELECT_VOLUME = -12f;

	private float col1 = COLOR_LOWER;
	private float col2 = COLOR_LOWER;
	private float col3 = COLOR_LOWER;

	// Refs
	private Label option1;
	private Label option2;
	private Label option3;

	// ================================================================
	
	public override void _Ready()
	{
		option1 = GetNode<Label>("Option1");
		option2 = GetNode<Label>("Option2");
		option3 = GetNode<Label>("Option3");

		Player.State = Player.ST.Cutscene;

		Controller.PlayMusic(menuMusic, 6.6f);
		//BattleUI.Initialize();
	}


	public override void _Process(float delta)
	{
		col1 = Mathf.Lerp(col1, hover1 ? 1 : COLOR_LOWER, LERP_WEIGHT);
		option1.Modulate = new Color(col1, col1, col1, 1);
		col2 = Mathf.Lerp(col2, hover2 ? 1 : COLOR_LOWER, LERP_WEIGHT);
		option2.Modulate = new Color(col2, col2, col2, 1);
		col3 = Mathf.Lerp(col3, hover3 ? 1 : COLOR_LOWER, LERP_WEIGHT);
		option3.Modulate = new Color(col3, col3, col3, 1);

		if (Input.IsActionJustPressed("click_left") && !selected)
		{
			if (hover1)
			{
				selected = true;
				Controller.PlaySoundBurst(soundSelect);
				//GetNode<AnimationPlayer>("AnimationPlayer").Play("Fadeout");
				Controller.FadeOutMusic(3f);
				Controller.Fade(true, 1.5f);
				GetNode<Timer>("TimerStart").Start();
				hover1 = false;
			}

			if (hover2)
			{
				selected = true;
				Controller.PlaySoundBurst(soundSelect);
				GetNode<AnimationPlayer>("AnimationPlayer").Play("HideStuff");
				GetNode<Timer>("TimerShowCredits").Start();
				hover2 = false;
				
			}

			if (hover3)
			{
				selected = true;
				Controller.PlaySoundBurst(soundSelect);
				Controller.FadeOutMusic(2f);
				GetNode<AnimationPlayer>("AnimationPlayer").Play("Fadeout");
				GetNode<Timer>("TimerQuit").Start();
				hover3 = false;
			}

			if (creditsOpen)
			{
				GetNode<AnimationPlayer>("AnimationPlayer").Play("HideCredits");
				GetNode<Timer>("TimerHideCredits").Start();
				selected = true;
			}
		}
	}

	// ================================================================

	private void HoverStartStart()
	{
		if (!selected && !creditsOpen)
		{
			Controller.PlaySoundBurst(soundMenu1, SELECT_VOLUME);
			hover1 = true;
		}
	}


	private void HoverStartEnd()
	{
		if (!selected && !creditsOpen)
			hover1 = false;
	}


	private void HoverCreditsStart()
	{
		if (!selected && !creditsOpen)
		{
			Controller.PlaySoundBurst(soundMenu2, SELECT_VOLUME);
			hover2 = true;
		}
	}


	private void HoverCreditsEnd()
	{
		if (!selected && !creditsOpen)
			hover2 = false;
	}


	private void HoverExitStart()
	{
		if (!selected && !creditsOpen)
		{
			Controller.PlaySoundBurst(soundMenu3, SELECT_VOLUME);
			hover3 = true;
		}
	}


	private void HoverExitEnd()
	{
		if (!selected && !creditsOpen)
			hover3 = false;
	}


	private void StartGame()
	{
		Controller.GotoScene(startScene);
		Player.singleton.Translation = startPosition;
		Player.singleton.RotationDegrees = new Vector3(90, 90, 0);
		Player.PlayAnimation("asleep");
		BattleUI.ShowUI(false);
		Player.SnapCamera();
		Controller.Fade(false, 3f);
	}


	private void ShowCredits()
	{
		GetNode<AnimationPlayer>("AnimationPlayer").Play("ShowCredits");
		creditsOpen = true;
		selected = false;
	}


	private void HideCredits()
	{
		creditsOpen = false;
		GetNode<AnimationPlayer>("AnimationPlayer").Play("ShowStuff");
		selected = false;
	}


	private void Exit()
	{
		GetTree().Quit();
	}
}
