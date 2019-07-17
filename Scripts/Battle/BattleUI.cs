using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


public class BattleUI : Node2D
{
	public static BattleUI singleton;

	BattleUI()
	{
		singleton = this;
	}

	[Export]
	private Texture textureBlack;

	[Export]
	private Texture textureRed;

	[Export]
	private AudioStream soundCoinHover;

	[Export]
	private AudioStream soundCoinClick;

	[Export]
	private AudioStream soundDrawHover;

	[Export]
	private AudioStream soundDrawClick;

	[Export]
	private AudioStream soundPassHover;

	[Export]
	private AudioStream soundPassClick;

	[Export]
	private AudioStream battleMusic;
	
	public enum Opponent { Blob, Tensor, Magma };
	private Opponent currentOpponent;
	private int enemyAttackIndex;

	private bool five = false;
	private bool battleMode = false;
	private bool uiVisible = false;
	private bool uiBuffer = false;

	// PLAYER STATS
	private int playerHPCap = 20;
	private int playerHP = 20;
	private int playerMPCap = 5;
	private int playerMP = 0;
	private int playerDefense = 0;

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

	private List<CardStruct> cardStashCurrentBattle = new List<CardStruct>();

	private int numJokers = 1;
	private int numJokersCurrent = 1;

	// Hovers
	private bool hoverJoker = false;
	private bool hoverDraw = false;
	private bool hoverPass = false;

	private List<Tuple<CardStruct, int>> hand = new List<Tuple<CardStruct, int>>();

	private HashSet<Card> instancedCards = new HashSet<Card>();

	private int handSize = 0;

	private HashSet<int> indexChecks = new HashSet<int>(){0, 1, 2};

	// Turn state
	private bool drawnThisTurn = false;
	private bool jokerThisTurn = false;

	private bool victory = false;
	private bool afterVictory = false;
	private bool gameover = false;

	// Refs
	private AnimationPlayer animPlayerJoker;
	private AnimationPlayer animPlayerDraw;
	private AnimationPlayer animPlayerPass;
	private AnimationPlayer animPlayerUI;
	private AnimationPlayer animPlayerSixfive;
	private AnimationPlayer animPlayerVictory;
	private Timer timerSetup;
	private Timer timerUIBuffer;
	private Timer timerEndPlayerTurn;
	private Timer timerEndEnemyTurn;
	private Timer timerEndBattle;
	private Sprite sprJoker;
	private Sprite sprDraw;
	private Sprite sprPass;
	private Sprite sprFive;
	private Sprite sprSix;
	private Sprite sprEnemyHealth;
	private Label jokerLabel;
	private Label hpLabel;
	private Label mpLabel;
	private Label defenseLabel;
	private Label enemyHpLabel;

	private Enemy enemyRef;

	private MeshInstance battleBackground;

	private PackedScene cardRef = GD.Load<PackedScene>("res://Instances/Battle/Card.tscn");
	private PackedScene battleNumberRef = GD.Load<PackedScene>("res://Instances/Battle/BattleNumber.tscn");
	private PackedScene partsAttackRef = GD.Load<PackedScene>("res://Instances/Particles/PartsAttack.tscn");

	private PackedScene enemyBlob = GD.Load<PackedScene>("res://Instances/Enemies/EnemyBlob.tscn");
	private PackedScene enemyTensor = GD.Load<PackedScene>("res://Instances/Enemies/EnemyTensor.tscn");

	// ================================================================

	public static int PlayerHP { get => BattleUI.singleton.playerHP; set => BattleUI.singleton.playerHP = value; }
	public static int PlayerMP { get => BattleUI.singleton.playerMP; set => BattleUI.singleton.playerMP = value; }
	public static bool Five { get => BattleUI.singleton.five; }
	public static int HandSize { get => BattleUI.singleton.handSize; set => BattleUI.singleton.handSize = value; }
	public static bool BattleMode { get => BattleUI.singleton.battleMode; }
	//public static bool UiVisible { get => BattleUI.singleton.uiVisible; }

	// ================================================================
	
	public override void _Ready()
	{
		animPlayerJoker = GetNode<AnimationPlayer>("AnimationPlayerJoker");
		animPlayerDraw = GetNode<AnimationPlayer>("AnimationPlayerDraw");
		animPlayerPass = GetNode<AnimationPlayer>("AnimationPlayerPass");
		animPlayerSixfive = GetNode<AnimationPlayer>("AnimationPlayerSixfive");
		animPlayerUI = GetNode<AnimationPlayer>("AnimationPlayerUI");
		animPlayerVictory = GetNode<AnimationPlayer>("AnimationPlayerVictory");
		timerSetup = GetNode<Timer>("TimerBattleSetup");
		timerUIBuffer = GetNode<Timer>("TimerUIBuffer");
		timerEndPlayerTurn = GetNode<Timer>("TimerEndPlayerTurn");
		timerEndEnemyTurn = GetNode<Timer>("TimerEndEnemyTurn");
		timerEndBattle = GetNode<Timer>("TimerEndBattle");
		sprJoker = GetNode<Sprite>("Joker");
		sprDraw = GetNode<Sprite>("DrawButton");
		sprPass = GetNode<Sprite>("PassButton");
		sprFive = GetNode<Sprite>("Five");
		sprSix = GetNode<Sprite>("Six");
		sprEnemyHealth = GetNode<Sprite>("EnemyHealthbar");
		jokerLabel = GetNode<Label>("JokerLabel");
		hpLabel = GetNode<Sprite>("Healthbar").GetNode<Label>("Label");
		mpLabel = GetNode<Sprite>("Manabar").GetNode<Label>("Label");
		defenseLabel = GetNode<Sprite>("Defensebar").GetNode<Label>("Label");
		enemyHpLabel = GetNode<Sprite>("EnemyHealthbar").GetNode<Label>("Label");
	}


	public override void _Process(float delta)
	{
		sprDraw.Visible = battleMode;
		sprPass.Visible = battleMode;
		sprFive.Visible = battleMode;
		sprSix.Visible = battleMode;
		sprEnemyHealth.Visible = battleMode;

		sprDraw.Modulate = handSize < 3 && !drawnThisTurn ? new Color(1, 1, 1, 1) : new Color(1, 1, 1, 0.4f);
		sprJoker.Modulate = !jokerThisTurn && numJokersCurrent > 0 ? new Color(1, 1, 1, 1) : new Color(1, 1, 1, 0.4f);

		// Clicks
		if (Input.IsActionJustPressed("click_left") && battleMode)
		{
			if (hoverJoker) // Joker
			{
				Controller.PlaySoundBurst(soundCoinClick);
				animPlayerJoker.Play("Flip Joker");
				SwapDimension();
				jokerThisTurn = true;
				numJokersCurrent--;
				hoverJoker = false;
			}

			if (hoverDraw) // Draw
			{
				Controller.PlaySoundBurst(soundDrawClick);
				int index = Mathf.RoundToInt((float)GD.RandRange(0, cardStashCurrentBattle.Count - 1));

				int thisHandIndex;

				// I'm really sorry about this
				HashSet<int> found = new HashSet<int>();
				foreach (var card in hand)
					found.Add(card.Item2);

				found.SymmetricExceptWith(indexChecks);
				thisHandIndex = found.Min();

				hand.Add(new Tuple<CardStruct, int>(cardStashCurrentBattle[index], thisHandIndex));
				handSize++;
				CardStruct thisCard = new CardStruct(
					cardStashCurrentBattle[index].suit,
					cardStashCurrentBattle[index].number,
					cardStashCurrentBattle[index].manaCost
				);

				cardStashCurrentBattle.RemoveAt(index);
				InstantiateCard(thisCard, thisHandIndex);
				drawnThisTurn = true;
				hoverDraw = false;
			}

			if (hoverPass) // Pass
			{
				Controller.PlaySoundBurst(soundPassClick);
				foreach (var card in BattleUI.singleton.instancedCards)
					card.Discarded = true;

				ShowUIMin(false);
				hoverPass = false;
				timerEndPlayerTurn.Start();
			}
		}

		// UI update stuff
		jokerLabel.Text = $"x {numJokersCurrent.ToString()}";
		hpLabel.Text = playerHP.ToString();
		mpLabel.Text = $"{playerMP.ToString()}/{playerMPCap}";
		defenseLabel.Text = playerDefense.ToString();

		if (battleMode)
		{
			enemyHpLabel.Text = enemyRef.Hp.ToString();

			if (victory && !afterVictory && (Input.IsActionJustPressed("click_left" ) || Input.IsActionJustPressed("action") || Input.IsActionJustPressed("move_jump")))
			{
				animPlayerVictory.Play("Victory Fade");
				timerEndBattle.Start();
				afterVictory = true;
			}
		}
			

		if (Input.IsActionJustPressed("debug_1"))
			//BattleStart(Opponent.Blob);
			Controller.Transition();

		if (Input.IsActionJustPressed("debug_2"))
			DeallocateCards();

		if (Input.IsActionJustPressed("ui_show") && !battleMode && Player.State == Player.ST.Move)
			ShowUI(!uiVisible);
	}

	// ================================================================

	public static void BattleStart(Opponent opponent, bool onField, FieldEnemy targetEnemy = null)
	{
		Controller.PreBattlePosition = Player.singleton.Translation;
		Controller.PreBattleFace = new Vector2(1, 1);
		Controller.PreBattleScene = BattleUI.singleton.GetTree().CurrentScene.Filename;
		if (onField)
			Controller.PreBattleEnemy = targetEnemy.GetPath().ToString().Substring(6);

		Player.State = Player.ST.Battle;
		Player.Vel = Vector3.Zero;

		BattleUI.singleton.currentOpponent = opponent;

		Controller.Transition();
		BattleUI.singleton.GetNode<Timer>("TimerBattleTransition").Start();
	}


	public static void ShowUI(bool show)
	{
		if (!BattleUI.singleton.uiBuffer && BattleUI.singleton.uiVisible != show)
		{
			BattleUI.singleton.animPlayerUI.Play(show ? "Show UI" : "Hide UI");
			BattleUI.singleton.uiVisible = show;

			BattleUI.singleton.uiBuffer = true;
			BattleUI.singleton.timerUIBuffer.Start();
		}
	}


	public static void SwapDimension()
	{
		BattleUI.singleton.five ^= true;
		BattleUI.singleton.animPlayerSixfive.Play(BattleUI.singleton.five ? "Switch to Five" : "Switch to Six");

		var mat = (SpatialMaterial)BattleUI.singleton.battleBackground.GetSurfaceMaterial(0);
		mat.AlbedoTexture = BattleUI.singleton.five ? BattleUI.singleton.textureRed : BattleUI.singleton.textureBlack;
	}


	public static void RemoveCardFromHand(int index)
	{
		foreach (var card in BattleUI.singleton.hand)
		{
			if (card.Item2 == index)
			{
				BattleUI.singleton.hand.Remove(card);
				HandSize--;
				break;
			}
		}
	}


	public static void ClickCard(Suit suit, int number, int cost)
	{
		foreach (var card in BattleUI.singleton.instancedCards)
		{
			if (!card.Selected)
				card.Discarded = true;
		}

		PlayerMP -= cost;

		BattleUI.singleton.ShowUIMin(false);

		switch (suit)
		{
			case Suit.Spade: // Attack
			{
				BattleUI.singleton.enemyRef.Hp = Mathf.Max(BattleUI.singleton.enemyRef.Hp - number, 0);
				BattleUI.singleton.SpawnParticles(BattleUI.singleton.partsAttackRef, BattleUI.singleton.enemyRef.Translation);
				BattleUI.singleton.SpawnDamageNumber(number, new Vector2(BattleUI.singleton.GetBattleCamera().UnprojectPosition(BattleUI.singleton.enemyRef.Translation).x, 400), false);
				BattleUI.singleton.enemyRef.PlayAnimation("hurt");
				
				if (BattleUI.singleton.enemyRef.Hp <= 0)
				{
					BattleUI.singleton.GetNode<Timer>("TimerEnemyDie").Start();
					//BattleUI.singleton.victory = true;
				}
				else
				{
					BattleUI.singleton.GetNode<Timer>("TimerEnemyHurt").Start();
					BattleUI.singleton.timerEndPlayerTurn.Start();
				}
					
				break;
			}

			case Suit.Club: // Defend
			{
				BattleUI.singleton.playerDefense = number;

				BattleUI.singleton.timerEndPlayerTurn.Start();
				break;
			}

			case Suit.Heart: // Heal
			{
				BattleUI.singleton.playerHP = Mathf.Min(BattleUI.singleton.playerHP + number, BattleUI.singleton.playerHPCap);
				BattleUI.singleton.SpawnDamageNumber(number, new Vector2(BattleUI.singleton.GetBattleCamera().UnprojectPosition(Player.singleton.Translation).x, 340), true);

				BattleUI.singleton.timerEndPlayerTurn.Start();
				break;
			}

			case Suit.Diamond: // Wild
			{
				break;
			}
		}
	}

	// ================================================================

	private void BattleStart2()
	{
		Controller.PlayMusic(battleMusic);
		PlayerMP = Mathf.FloorToInt((float)BattleUI.singleton.playerMPCap / 2f);
		Controller.GotoScene(BATTLE_SCENE);
		Player.singleton.Translation = new Vector3(0, 0.35f, 0);
		
		switch (currentOpponent)
		{
			case Opponent.Blob:
			{
				var e = (EnemyBlob)BattleUI.singleton.enemyBlob.Instance();
				e.Initialize();
				e.Translation = new Vector3(ENEMY_X, ENEMY_Y, ENEMY_Z);
				BattleUI.singleton.GetTree().GetRoot().AddChild(e);
				BattleUI.singleton.enemyRef = e;
				break;
			}

			case Opponent.Tensor:
			{
				var e = (EnemyTensor)BattleUI.singleton.enemyTensor.Instance();
				e.Initialize();
				e.Translation = new Vector3(ENEMY_X, ENEMY_Y, ENEMY_Z);
				BattleUI.singleton.GetTree().GetRoot().AddChild(e);
				BattleUI.singleton.enemyRef = e;
				break;
			}
		}

		BattleUI.singleton.battleMode = true;
		BattleUI.singleton.timerSetup.Start();
	}

	private void BattleSetup()
	{
		battleBackground = GetTree().GetRoot().GetNode<Spatial>("Scene").GetNode<MeshInstance>("Background");
		MainCamera.singleton.Current = false;
		ShowUI(true);
		cardStashCurrentBattle = new List<CardStruct>(cardStash);
		enemyAttackIndex = EnemySelectAttack();
		GD.Print($"Selected enemy attack index {enemyAttackIndex}");
		RandomizeHand();
		InstantiateCardsInHand();
	}


	private void RandomizeHand()
	{
		hand.Clear();
		handSize = 3;
		
		for (int i = 0; i < 3; i++)
		{
			int index = Mathf.RoundToInt((float)GD.RandRange(0, cardStashCurrentBattle.Count - 1));
			hand.Add(new Tuple<CardStruct, int>(cardStashCurrentBattle[index], i));
			cardStashCurrentBattle.RemoveAt(index);
		}
	}


	private void InstantiateCard(CardStruct card, int index)
	{
		var cardInst = (Card)cardRef.Instance();
		cardInst.SetSuit((Card.Suit)(int)card.suit);
		cardInst.SetNumber(card.number);
		cardInst.SetCost(card.manaCost);
		cardInst.HandIndex = index;
		cardInst.Position = new Vector2(360 + 212 * index, 900);
		instancedCards.Add(cardInst);
		GetTree().GetRoot().AddChild(cardInst);
	}


	private void InstantiateCardsInHand()
	{
		for (int i = 0; i < hand.Count; i++)
		{
			var cardInst = (Card)cardRef.Instance();
			cardInst.SetSuit((Card.Suit)(int)hand[i].Item1.suit);
			cardInst.SetNumber(hand[i].Item1.number);
			cardInst.SetCost(hand[i].Item1.manaCost);
			cardInst.HandIndex = hand[i].Item2;
			cardInst.Position = new Vector2(360 + 212 * hand[i].Item2, 900);
			instancedCards.Add(cardInst);
			GetTree().GetRoot().AddChild(cardInst);
		}
	}


	private int EnemySelectAttack()
	{
		switch (currentOpponent)
		{
			case Opponent.Blob:
				return Mathf.RoundToInt((float)GD.RandRange(0, 2));

			default:
				return 0;
		}
	}


	private void EnemyAttack(int power)
	{
		int finalPower = Mathf.Max(power - playerDefense, 0);
		playerHP = Mathf.Max(playerHP - finalPower, 0);

		SpawnDamageNumber(finalPower, new Vector2(GetBattleCamera().UnprojectPosition(Player.singleton.Translation).x, 340), false);

		if (playerHP <= 0)
		{
			gameover = true;
			GetNode<Timer>("TimerGameOver").Start();
		}
			
	}


	private void EnemyTurn()
	{
		DeallocateCards();
		
		switch (currentOpponent)
		{
			case Opponent.Blob:
			{
				switch (enemyAttackIndex)
				{
					case 0:
						EnemyAttack(2);
						break;
					case 1:
						EnemyAttack(2);
						break;
					case 2:
						EnemyAttack(3);
						break;
				}

				break;
			}
		}

		if (!gameover)
			timerEndEnemyTurn.Start();
	}


	private void PlayerTurn()
	{
		playerMP = Mathf.Min(++playerMP, playerMPCap);
		playerDefense = 0;
		jokerThisTurn = false;
		drawnThisTurn = false;
		SwapDimension();
		ShowUIMin(true);
		InstantiateCardsInHand();
		enemyAttackIndex = EnemySelectAttack();
		GD.Print($"Player has {cardStashCurrentBattle.Count} cards left");
	}


	private void DeallocateCards()
	{
		foreach (var card in instancedCards)
		{
			card.QueueFree();
		}

		instancedCards.Clear();
	}

	
	private void Victory()
	{
		BattleUI.singleton.animPlayerVictory.Play("Victory Appear");
		victory = true;
	}


	private void ShowUIMin(bool show)
	{
		if (!BattleUI.singleton.uiBuffer && BattleUI.singleton.uiVisible != show)
		{
			BattleUI.singleton.animPlayerUI.Play(show ? "Show UI Min" : "Hide UI Min");
			BattleUI.singleton.uiVisible = show;
		}
	}


	private void ResetBuffer()
	{
		uiBuffer = false;
	}


	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private Camera GetBattleCamera()
	{
		return GetTree().GetRoot().GetNode<Spatial>("Scene").GetNode<Camera>("BattleCamera");
	}


	private void SpawnDamageNumber(int power, Vector2 position, bool healing)
	{
		var damageNum = (BattleNumber)battleNumberRef.Instance();
		damageNum.Position = position;
		damageNum.SetNumber(power);
		damageNum.SetHealing(healing);
		GetTree().GetRoot().AddChild(damageNum);
	}


	private void SpawnParticles(PackedScene partsRef, Vector3 position)
	{
		var parts = (Particles)partsRef.Instance();
		parts.Translation = position;
		parts.Emitting = true;
		GetTree().GetRoot().AddChild(parts);
	}


	private void JokerHoverStart()
	{
		if (battleMode && !jokerThisTurn && uiVisible && numJokersCurrent > 0)
		{
			Controller.PlaySoundBurst(soundCoinHover);
			animPlayerJoker.Play("Joker Hover");
			BattleUI.singleton.hoverJoker = true;
		}
	}


	private void JokerHoverEnd()
	{
		if (battleMode && !jokerThisTurn && uiVisible && numJokersCurrent > 0)
			BattleUI.singleton.hoverJoker = false;
	}


	private void DrawHoverStart()
	{
		if (battleMode && !drawnThisTurn && uiVisible && handSize < 3)
		{
			Controller.PlaySoundBurst(soundDrawHover);
			animPlayerDraw.Play("Draw Hover");
			BattleUI.singleton.hoverDraw = true;
		}
	}


	private void DrawHoverEnd()
	{
		if (battleMode && !drawnThisTurn && uiVisible && handSize < 3)
			BattleUI.singleton.hoverDraw = false;
	}


	private void PassHoverStart()
	{
		if (battleMode && uiVisible)
		{
			Controller.PlaySoundBurst(soundPassHover);
			animPlayerPass.Play("Pass Hover");
			BattleUI.singleton.hoverPass = true;
		}
	}


	private void PassHoverEnd()
	{
		if (battleMode && uiVisible)
			BattleUI.singleton.hoverPass = false;
	}


	private void EndBattle()
	{
		DeallocateCards();
		Controller.Fade(true, 0.5f);
		GetNode<Timer>("TimerEndBattle2").Start();
	}


	private void EndBattle2()
	{
		enemyRef.QueueFree();
		victory = false;
		afterVictory = false;
		ShowUI(false);
		Controller.GotoScene(Controller.PreBattleScene);
		Player.singleton.Translation = Controller.PreBattlePosition;
		numJokersCurrent = numJokers;
		// set player face
		Player.State = Player.ST.Move;
		battleMode = false;
		Controller.Fade(false, 0.5f);
		GetNode<Timer>("TimerDestroyFieldEnemy").Start();
	}


	private void GameOver()
	{
		Controller.Fade(true, 1);
		GetNode<Timer>("TimerGameOver2").Start();
	}


	private void GameOver2()
	{
		Controller.GotoScene("res://Scenes/GameOver.tscn");
		Controller.Fade(false, 1.5f);
	}


	private void EnemyHurtEnd()
	{
		enemyRef.PlayAnimation("idle");
	}


	private void EnemyDie()
	{
		enemyRef.Die();
		GetNode<Timer>("TimerVictory").Start();
	}


	private void DestroyFieldEnemy()
	{
		if (Controller.PreBattleEnemy != null)
		{
			GetTree().GetRoot().GetNode<FieldEnemy>(Controller.PreBattleEnemy).QueueFree();
			Controller.PreBattleEnemy = null;
		}
	}
}
