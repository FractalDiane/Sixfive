using Godot;
using System;
using System.Collections.Generic;

public class BattleUI : Node2D
{
	public static BattleUI singleton;

	BattleUI()
	{
		singleton = this;
	}

	public enum Opponent { Blob };

	private bool five = false;
	private bool battleMode = false;
	private bool uiVisible = false;
	private bool uiBuffer = false;

	// PLAYER STATS
	private int playerHPCap = 20;
	private int playerHP = 20;
	private int playerMPCap = 5;
	private int playerMP = 0;

	private const string BATTLE_SCENE = "res://Scenes/Battle.tscn";
	private const float ENEMY_X = 2f;
	private const float ENEMY_Y = 0.35f;
	private const float ENEMY_Z = 0f;

	public enum Suit { Spade, Club, Heart, Diamond };

	private struct CardStruct
	{
		public CardStruct(Suit suit, int number, int manaCost)
		{
			this.suit = suit;
			this.number = number;
			this.manaCost = manaCost;
		}
		
		public Suit suit;
		public int number;
		public int manaCost;
	}

	private List<CardStruct> cardStash = new List<CardStruct>()//;
	{
		new CardStruct(Suit.Spade, 2, 1),
		new CardStruct(Suit.Spade, 2, 1),
		new CardStruct(Suit.Spade, 2, 1),
		new CardStruct(Suit.Spade, 3, 2),
		new CardStruct(Suit.Spade, 3, 2),
		new CardStruct(Suit.Heart, 3, 3),
		new CardStruct(Suit.Heart, 3, 3),
		new CardStruct(Suit.Heart, 3, 2),
		new CardStruct(Suit.Club, 1, 1),
		new CardStruct(Suit.Club, 1, 1),
		new CardStruct(Suit.Club, 2, 2),
		new CardStruct(Suit.Diamond, 2, 3),
		new CardStruct(Suit.Diamond, 2, 3),
		new CardStruct(Suit.Diamond, 2, 3),
	};

	private int numJokers = 1;
	private int numJokersCurrent = 1;

	private List<CardStruct> hand = new List<CardStruct>();

	private HashSet<Card> instancedCards = new HashSet<Card>();

	// Refs
	private AnimationPlayer animPlayer;
	private Timer timerSetup;
	private Timer timerUIBuffer;
	private Sprite sprDraw;
	private Sprite sprFive;
	private Sprite sprSix;

	private PackedScene cardRef = GD.Load<PackedScene>("res://Instances/Battle/Card.tscn");
	private PackedScene battleNumberRef = GD.Load<PackedScene>("res://Instances/Battle/BattleNumber.tscn");

	private PackedScene enemyBlob = GD.Load<PackedScene>("res://Instances/Enemies/EnemyBlob.tscn");

	// ================================================================

	public static int PlayerHP { get => BattleUI.singleton.playerHP; set => BattleUI.singleton.playerHP = value; }
	public static int PlayerMP { get => BattleUI.singleton.playerMP; set => BattleUI.singleton.playerMP = value; }
	//public static bool UiVisible { get => BattleUI.singleton.uiVisible; }

	// ================================================================
	
	public override void _Ready()
	{
		animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		timerSetup = GetNode<Timer>("TimerBattleSetup");
		timerUIBuffer = GetNode<Timer>("TimerUIBuffer");
		sprDraw = GetNode<Sprite>("DrawButton");
		sprFive = GetNode<Sprite>("Five");
		sprSix = GetNode<Sprite>("Six");
	}


	public override void _Process(float delta)
	{
		sprDraw.Visible = battleMode;
		sprFive.Visible = battleMode;
		sprSix.Visible = battleMode;

		if (Input.IsActionJustPressed("debug_1"))
			BattleStart(Opponent.Blob);

		if (Input.IsActionJustPressed("ui_show") && !battleMode && Player.State == Player.ST.Move)
		{
			ShowUI(!uiVisible);
		}
	}

	// ================================================================

	public static void BattleStart(Opponent opponent)
	{
		Player.State = Player.ST.Battle;
		Controller.GotoScene(BATTLE_SCENE);
		Player.singleton.Translation = new Vector3(0, 0.35f, 0);
		Player.Vel = Vector3.Zero;
		BattleUI.singleton.battleMode = true;

		switch (opponent)
		{
			case Opponent.Blob:
			{
				var e = (EnemyBlob)BattleUI.singleton.enemyBlob.Instance();
				e.Translation = new Vector3(ENEMY_X, ENEMY_Y, ENEMY_Z);
				BattleUI.singleton.GetTree().GetRoot().AddChild(e);
				break;
			}
		}

		BattleUI.singleton.timerSetup.Start();
	}


	public static void ShowUI(bool show)
	{
		if (!BattleUI.singleton.uiBuffer && BattleUI.singleton.uiVisible != show)
		{
			BattleUI.singleton.animPlayer.Play(show ? "Show UI" : "Hide UI");
			BattleUI.singleton.uiVisible = show;

			BattleUI.singleton.uiBuffer = true;
			BattleUI.singleton.timerUIBuffer.Start();
		}
	}

	// ================================================================

	private void BattleSetup()
	{
		MainCamera.singleton.Current = false;
		ShowUI(true);
		RandomizeHand();
		InstantiateCards();
	}


	private void RandomizeHand()
	{
		hand.Clear();
		List<CardStruct> cardsSet = new List<CardStruct>(cardStash);
		
		for (int i = 0; i < 3; i++)
		{
			int index = Mathf.RoundToInt((float)GD.RandRange(0, cardsSet.Count - 1));
			hand.Add(cardsSet[index]);
			cardsSet.RemoveAt(index);
		}
	}


	private void InstantiateCards()
	{
		for (int i = 0; i < hand.Count; i++)
		{
			var cardInst = (Card)cardRef.Instance();
			cardInst.SetSuit((Card.Suit)(int)hand[i].suit);
			cardInst.SetNumber(hand[i].number);
			cardInst.SetCost(hand[i].manaCost);
			cardInst.Position = new Vector2(360 + 212 * i, 695);
			instancedCards.Add(cardInst);
			GetTree().GetRoot().AddChild(cardInst);
		}
	}


	private void ResetBuffer()
	{
		uiBuffer = false;
	}
}
