using Godot;
using System;
using System.Runtime.CompilerServices;

public class Card : Sprite
{
	[Export]
	private Texture spadeCard;

	[Export]
	private Texture clubCard;

	[Export]
	private Texture heartCard;

	[Export]
	private Texture diamondCard;

	[Export]
	private AudioStream soundHover;

	[Export]
	private AudioStream soundClick;

	[Export]
	private AudioStream soundDiscard;

	public enum Suit { Spade, Club, Heart, Diamond };
	private Suit cardSuit;

	private int number;
	private int cost;
	private int handIndex;

	private bool discarded = false;
	private bool selected = false;

	private bool hover = false;
	private float instrAlpha = 0f;

	private Vector2 startPosition;
	private float hoverPosition;
	private float discardPosition;
	private float selectPosition;

	private const float LERP_WEIGHT = 0.1f / 60;
	private const float LERP_WEIGHT_2 = 0.08f / 60;
	private const float LERP_WEIGHT_3 = 0.075f / 60;
	private const int IDLE_Y = 695;

	// Refs
	//private Label numberLabel;
	//private Label costLabel;
	private Label instr;

	// ================================================================

	public int Number { get => number; }
	public int Cost { get => cost; }
	public int HandIndex { set => handIndex = value; }
	public bool Selected { get => selected; }
	public bool Discarded { get => discarded; set => discarded = value; }

	// ================================================================
	
	public override void _Ready()
	{
	//	numberLabel = GetNode<Label>("Number");
	//	costLabel = GetNode<Label>("Cost");
		instr = GetNode<Label>("Instructions");

		startPosition = new Vector2(Position.x, IDLE_Y);
		hoverPosition = startPosition.y - 50;
		discardPosition = startPosition.y + 440;
		selectPosition = startPosition.y - 500;
	}
	

	public override void _PhysicsProcess(float delta)
	{
		if (hover && !discarded && !selected)
		{
			Position = new Vector2(Position.x, Mathf.Lerp(Position.y, hoverPosition, 1 - Mathf.Pow(LERP_WEIGHT, delta)));

			if (Input.IsActionJustPressed("click_left") && Clickable())
			{
				Controller.PlaySoundBurst(soundClick);
				selected = true;
				BattleUI.RemoveCardFromHand(handIndex);
				BattleUI.ClickCard((BattleUI.Suit)(int)cardSuit, number, cost);
			}

			if (Input.IsActionJustPressed("click_right"))
			{
				Controller.PlaySoundBurst(soundDiscard);
				BattleUI.RemoveCardFromHand(handIndex);
				//BattleUI.HandSize--;
				discarded = true;
			}
		}
		else
			Position = new Vector2(Position.x, Mathf.Lerp(Position.y, startPosition.y, 1 - Mathf.Pow(LERP_WEIGHT, delta)));

		if (discarded)
			Position = new Vector2(Position.x, Mathf.Lerp(Position.y, discardPosition, 1 - Mathf.Pow(LERP_WEIGHT_3, delta)));

		if (selected)
		{
			if (Scale.x > 0.01f)
			{
				Position = new Vector2(Position.x, Mathf.Lerp(Position.y, selectPosition, 1 - Mathf.Pow(LERP_WEIGHT_2, delta)));
				float newScale = Mathf.Lerp(Scale.x, 0, 1 - Mathf.Pow(LERP_WEIGHT_2, delta));
				Scale = new Vector2(newScale, newScale);
			}
			else
				Hide();
		}
			

		if (cardSuit == Suit.Heart || cardSuit == Suit.Diamond)
			Modulate = selected || (BattleUI.Five && cost <= BattleUI.PlayerMP) ? new Color(1, 1, 1, 1) : new Color(1, 1, 1, 0.33f);
		else
			Modulate = selected || (!BattleUI.Five && cost <= BattleUI.PlayerMP) ? new Color(1, 1, 1, 1) : new Color(1, 1, 1, 0.33f);
	
		instrAlpha = Mathf.Clamp(instrAlpha + (0.05f * (hover && !discarded && !selected && Clickable() ? 1 : -1)), 0, 1);
		instr.SelfModulate = new Color(1, 1, 1, instrAlpha);
	}

	// ================================================================

	public void SetSuit(Suit suit)
	{
		cardSuit = suit;
		switch (suit)
		{
			case Suit.Spade:
			{
				Texture = spadeCard;
				GetNode<Label>("Number").SelfModulate = new Color(0, 0, 0);
				GetNode<Label>("Desc").Text = "Attack";
				GetNode<Label>("Desc").SelfModulate = new Color(0, 0, 0);
				break;
			}
				
			case Suit.Club:
			{
				Texture = clubCard;
				GetNode<Label>("Number").SelfModulate = new Color(0, 0, 0);
				GetNode<Label>("Desc").Text = "Defend";
				GetNode<Label>("Desc").SelfModulate = new Color(0, 0, 0);
				break;
			}
			
			
			case Suit.Heart:
			{
				Texture = heartCard;
				GetNode<Label>("Number").SelfModulate = new Color(1, 0, 0);
				GetNode<Label>("Desc").Text = "Heal";
				GetNode<Label>("Desc").SelfModulate = new Color(0, 0, 0);
				break;
			}
				
			case Suit.Diamond:
			{
				Texture = diamondCard;
				GetNode<Label>("Number").SelfModulate = new Color(1, 0, 0);
				GetNode<Label>("Desc").Text = "Wild";
				GetNode<Label>("Desc").SelfModulate = new Color(0, 0, 0);
				break;
			}
		}
	}


	public void SetNumber(int value)
	{
		number = value;
		GetNode<Label>("Number").Text = value.ToString();
	}


	public void SetCost(int value)
	{
		cost = value;
		GetNode<Label>("Cost").Text = value.ToString();
	}

	// ================================================================

	private void HoverStart()
	{
		if (!discarded && !selected)
		{
			Controller.PlaySoundBurst(soundHover);
			hover = true;
		}
	}


	private void HoverEnd()
	{
		if (!discarded && !selected)
			hover = false;
	}


	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool Clickable()
	{
		if (cost > BattleUI.PlayerMP)
			return false;

		switch (cardSuit)
		{
			case Suit.Spade:
				return !BattleUI.Five;
			case Suit.Club:
				return !BattleUI.Five;
			case Suit.Heart:
				return BattleUI.Five;
			case Suit.Diamond:
				return BattleUI.Five;
			default:
				return false;
		}
	}
}
