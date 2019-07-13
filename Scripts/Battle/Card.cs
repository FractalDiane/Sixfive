using Godot;
using System;

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

	public enum Suit { Spade, Club, Heart, Diamond };
	private Suit cardSuit;

	private int number;
	private int cost;

	private bool hover = false;

	private Vector2 startPosition;
	private float hoverPosition;

	private const float LERP_WEIGHT = 0.075f;

	// Refs
	//private Label numberLabel;
	//private Label costLabel;

	// ================================================================

	public int Number { get => number; }
	public int Cost { get => cost; }

	// ================================================================
	
	public override void _Ready()
	{
	//	numberLabel = GetNode<Label>("Number");
	//	costLabel = GetNode<Label>("Cost");

		startPosition = Position;
		hoverPosition = startPosition.y - 50;
	}


	public override void _Process(float delta)
	{
		if (hover)
			Position = new Vector2(Position.x, Mathf.Lerp(Position.y, hoverPosition, LERP_WEIGHT));
		else
			Position = new Vector2(Position.x, Mathf.Lerp(Position.y, startPosition.y, LERP_WEIGHT));
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
		hover = true;
	}


	private void HoverEnd()
	{
		hover = false;
	}
}
