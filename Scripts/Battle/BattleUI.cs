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
	private AudioStream soundHurt;

	[Export]
	private AudioStream soundHeal;

	[Export]
	private AudioStream soundDefense;

	[Export]
	private AudioStream battleMusic;

	[Export]
	private AudioStream bossMusic;
	
	public enum Opponent { Blob, Tensor, Magma, BossZincel };
	private Opponent currentOpponent;
	private int enemyAttackIndex;
	private string currentEnemyAnim;
	private bool boss = false;

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

	private Suit currentAttackSuit;
	private int currentAttackNumber;

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

	private List<CardStruct> cardStash = new List<CardStruct>()
	{
		new CardStruct(Suit.Spade, 2, 1),
		new CardStruct(Suit.Spade, 2, 1),
		new CardStruct(Suit.Spade, 2, 2),
		new CardStruct(Suit.Spade, 2, 2),
		new CardStruct(Suit.Spade, 2, 2),
		new CardStruct(Suit.Spade, 2, 2),
		new CardStruct(Suit.Spade, 2, 2),
		new CardStruct(Suit.Spade, 3, 3),
		new CardStruct(Suit.Spade, 3, 2),
		new CardStruct(Suit.Spade, 3, 2),
		new CardStruct(Suit.Spade, 3, 3),
		new CardStruct(Suit.Heart, 3, 2),
		new CardStruct(Suit.Heart, 4, 2),
		new CardStruct(Suit.Heart, 5, 3),
		new CardStruct(Suit.Heart, 6, 3),
		new CardStruct(Suit.Club, 1, 1),
		new CardStruct(Suit.Club, 1, 1),
		new CardStruct(Suit.Club, 2, 2),
		new CardStruct(Suit.Club, 2, 1),
		new CardStruct(Suit.Diamond, 2, 1),
		new CardStruct(Suit.Diamond, 2, 1),
		new CardStruct(Suit.Diamond, 3, 2),
		new CardStruct(Suit.Diamond, 5, 3),
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

	private bool preBattle = false;
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
	private Sprite sprCards;
	private Sprite sprDraw;
	private Sprite sprPass;
	private Sprite sprFive;
	private Sprite sprSix;
	private Sprite sprEnemyHealth;
	private Label jokerLabel;
	private Label hpLabel;
	private Label mpLabel;
	private Label defenseLabel;
	private Label cardsLabel;
	private Label enemyHpLabel;
	private Label enemyNameLabel;

	private Enemy enemyRef;

	private MeshInstance battleBackground;

	private PackedScene cardRef = GD.Load<PackedScene>("res://Instances/Battle/Card.tscn");
	private PackedScene battleNumberRef = GD.Load<PackedScene>("res://Instances/Battle/BattleNumber.tscn");
	private PackedScene partsAttackRef = GD.Load<PackedScene>("res://Instances/Particles/PartsAttack.tscn");
	private PackedScene partsHealRef = GD.Load<PackedScene>("res://Instances/Attack Animations/AnimHeal.tscn");

	private PackedScene enemyBlob = GD.Load<PackedScene>("res://Instances/Enemies/EnemyBlob.tscn");
	private PackedScene enemyTensor = GD.Load<PackedScene>("res://Instances/Enemies/EnemyTensor.tscn");
	private PackedScene enemyMagma = GD.Load<PackedScene>("res://Instances/Enemies/EnemyMagma.tscn");
	private PackedScene enemyBossZincel = GD.Load<PackedScene>("res://Instances/Enemies/Bosses/EnemyBossZincel.tscn");

	// Animation paths
	private const string PATH_SPADES1 = "res://Instances/Attack Animations/AnimSpades1.tscn";

	// ================================================================

	public static bool PreBattle { get => BattleUI.singleton.preBattle; set => BattleUI.singleton.preBattle = value; }
	public static int NumJokers { get => BattleUI.singleton.numJokers; }
	public static int NumJokersCurrent { set => BattleUI.singleton.numJokersCurrent = value; }
	public static int PlayerHP { get => BattleUI.singleton.playerHP; set => BattleUI.singleton.playerHP = value; }
	public static int PlayerHPCap { get => BattleUI.singleton.playerHPCap; set => BattleUI.singleton.playerHPCap = value; }
	public static int PlayerMP { get => BattleUI.singleton.playerMP; set => BattleUI.singleton.playerMP = value; }
	public static bool Five { get => BattleUI.singleton.five; set => BattleUI.singleton.five = value; }
	public static bool JokerThisTurn { set => BattleUI.singleton.jokerThisTurn = value; }
	public static bool DrawThisTurn { set => BattleUI.singleton.drawnThisTurn = value; }
	public static int HandSize { get => BattleUI.singleton.handSize; set => BattleUI.singleton.handSize = value; }
	public static bool BattleMode { get => BattleUI.singleton.battleMode; set => BattleUI.singleton.battleMode = value; }
	public static bool Gameover { get => BattleUI.singleton.gameover; set => BattleUI.singleton.gameover = value; }
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
		sprCards = GetNode<Sprite>("CardCount");
		sprDraw = GetNode<Sprite>("DrawButton");
		sprPass = GetNode<Sprite>("PassButton");
		sprFive = GetNode<Sprite>("Five");
		sprSix = GetNode<Sprite>("Six");
		sprEnemyHealth = GetNode<Sprite>("EnemyHealthbar");
		jokerLabel = GetNode<Label>("JokerLabel");
		cardsLabel = GetNode<Sprite>("CardCount").GetNode<Label>("CardCountLabel");
		hpLabel = GetNode<Sprite>("Healthbar").GetNode<Label>("Label");
		mpLabel = GetNode<Sprite>("Manabar").GetNode<Label>("Label");
		defenseLabel = GetNode<Sprite>("Defensebar").GetNode<Label>("Label");
		enemyHpLabel = GetNode<Sprite>("EnemyHealthbar").GetNode<Label>("Label");
		enemyNameLabel = GetNode<Sprite>("EnemyHealthbar").GetNode<Label>("Name");
	}


	public override void _Process(float delta)
	{
		sprDraw.Visible = battleMode;
		sprCards.Visible = battleMode;
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
		cardsLabel.Text = $"x {BattleUI.singleton.cardStashCurrentBattle.Count.ToString()}";
		hpLabel.Text = playerHP.ToString();
		mpLabel.Text = $"{playerMP.ToString()}/{playerMPCap}";
		defenseLabel.Text = playerDefense.ToString();

		if (battleMode)
		{
			enemyHpLabel.Text = enemyRef.Hp.ToString();
			enemyNameLabel.Text = enemyRef.EnemyName;

			if (victory && !afterVictory && (Input.IsActionJustPressed("click_left") || Input.IsActionJustPressed("action") || Input.IsActionJustPressed("move_jump")))
			{
				animPlayerVictory.Play("Victory Fade");
				timerEndBattle.Start();
				afterVictory = true;
			}
		}

		if (Input.IsActionJustPressed("ui_show") && !battleMode && Player.State == Player.ST.Move)
			ShowUI(!uiVisible);
	}

	// ================================================================

	public static void Initialize()
	{
		BattleUI.singleton.Visible = true;
	}

	public static void BattleStart(Opponent opponent, bool onField, FieldEnemy targetEnemy = null, bool boss = false)
	{
		BattleUI.singleton.preBattle = true;
		Controller.PreBattlePosition = Player.singleton.Translation;
		Controller.PreBattleFace = new Vector2(1, 1);
		Controller.PreBattleScene = BattleUI.singleton.GetTree().CurrentScene.Filename;
		if (onField)
			Controller.PreBattleEnemy = targetEnemy.GetPath().ToString().Substring(6);

		Player.State = Player.ST.Battle;
		Player.Vel = Vector3.Zero;


		BattleUI.singleton.currentOpponent = opponent;
		BattleUI.singleton.boss = boss;

		Controller.Transition();
		Controller.FadeOutMusic(1f);
		BattleUI.Five = false;
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

		BattleUI.singleton.currentAttackSuit = suit;
		BattleUI.singleton.currentAttackNumber = number;
		BattleUI.singleton.GetNode<Timer>("TimerAttackDelay").Start();
	}

	// ================================================================

	private void BattleStart2()
	{
		Controller.PreBattleMusic = Controller.CurrentMusic;
		Controller.PlayMusic(boss ? bossMusic : battleMusic);
		PlayerMP = Mathf.FloorToInt((float)BattleUI.singleton.playerMPCap / 2f);
		Controller.GotoScene(BATTLE_SCENE);
		Player.singleton.Translation = new Vector3(0, 0.35f, 0);
		Player.NeutralizeRotation();
		Player.PlayAnimation("dr_walk");
		BattleUI.singleton.animPlayerSixfive.Play("Switch to Six");
		
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

			case Opponent.Magma:
			{
				var e = (EnemyMagma)BattleUI.singleton.enemyMagma.Instance();
				e.Initialize();
				e.Translation = new Vector3(ENEMY_X, ENEMY_Y, ENEMY_Z);
				BattleUI.singleton.GetTree().GetRoot().AddChild(e);
				BattleUI.singleton.enemyRef = e;
				break;
			}

			case Opponent.BossZincel:
			{
				var e = (EnemyBossZincel)BattleUI.singleton.enemyBossZincel.Instance();
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
		//GD.Print($"Selected enemy attack index {enemyAttackIndex}");
		RandomizeHand();
		InstantiateCardsInHand();
		preBattle = false;
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


	private void PlayerAttack()
	{
		int num = currentAttackNumber;

		switch (currentAttackSuit)
		{
			case Suit.Spade: // Attack
				SuitSpade();
				break;

			case Suit.Club: // Defend
			{
				SuitClub();
				break;
			}

			case Suit.Heart: // Heal
			{
				SuitHeart();
				break;
			}

			case Suit.Diamond: // Wild
			{
				switch (Mathf.RoundToInt((float)GD.RandRange(0, 2)))
				{
					case 0: // Spade
						SuitSpade();
						break;

					case 1: // Club
						SuitClub();
						break;

					case 2: // Heart
						SuitHeart();
						break;
				}

				break;
			}
		}
	}


	private void SuitSpade()
	{
		string path;
		Vector3 pos;
		switch (Mathf.RoundToInt((float)GD.RandRange(0, 2)))
		{
			case 0:
				path = PATH_SPADES1;
				pos = new Vector3(enemyRef.Translation.x, enemyRef.Translation.y, enemyRef.Translation.z + 0.03f);
				break;

			default:
				path = PATH_SPADES1;
				pos = new Vector3(enemyRef.Translation.x, enemyRef.Translation.y, enemyRef.Translation.z + 0.03f);
				break;
		}

		var anim = (Spatial)GD.Load<PackedScene>(path).Instance();
		anim.Translation = pos;
		anim.GetNode<Timer>("TimerDamage").Connect("timeout", this, "DamageEnemy");
		anim.GetNode<AnimationPlayer>("AnimationPlayer").Play("Animation");
		GetTree().GetRoot().AddChild(anim);
	}


	private void SuitClub()
	{
		GetNode<Timer>("TimerShieldDelay").Start();
	}


	private void SuitHeart()
	{
		Controller.PlaySoundBurst(BattleUI.singleton.soundHeal);
		var anim = (Spatial)partsHealRef.Instance();
		anim.Translation = Player.singleton.Translation;
		GetTree().GetRoot().AddChild(anim);

		BattleUI.singleton.playerHP = Mathf.Min(BattleUI.singleton.playerHP + currentAttackNumber, BattleUI.singleton.playerHPCap);
		BattleUI.singleton.SpawnDamageNumber(currentAttackNumber, new Vector2(BattleUI.singleton.GetBattleCamera().UnprojectPosition(Player.singleton.Translation).x, 340), true);
		BattleUI.singleton.timerEndPlayerTurn.Start();
	}


	private void AnimShield()
	{
		BattleUI.singleton.playerDefense = currentAttackNumber;
		var anim = (Spatial)GD.Load<PackedScene>("res://Instances/Attack Animations/AnimShield.tscn").Instance();
		anim.Translation = Player.singleton.Translation;
		GetTree().GetRoot().AddChild(anim);
		BattleUI.singleton.timerEndPlayerTurn.Start();
	}


	private void DamageEnemy()
	{
		Controller.PlaySoundBurst(BattleUI.singleton.soundHurt);
		BattleUI.singleton.enemyRef.Hp = Mathf.Max(BattleUI.singleton.enemyRef.Hp - currentAttackNumber, 0);
		//BattleUI.singleton.SpawnParticles(BattleUI.singleton.partsAttackRef, BattleUI.singleton.enemyRef.Translation);
		SpawnDamageNumber(currentAttackNumber, new Vector2(BattleUI.singleton.GetBattleCamera().UnprojectPosition(BattleUI.singleton.enemyRef.Translation).x, 400), false);
		BattleUI.singleton.enemyRef.PlayAnimation("hurt");
				
		if (BattleUI.singleton.enemyRef.Hp <= 0)
		{
			BattleUI.singleton.GetNode<Timer>("TimerEnemyDie").Start();
		}
		else
		{
			BattleUI.singleton.GetNode<Timer>("TimerEnemyHurt").Start();
			BattleUI.singleton.timerEndPlayerTurn.Start();
		}
	}


	private void DamagePlayer()
	{
		Controller.PlaySoundBurst(BattleUI.singleton.soundHurt);
	}


	private int EnemySelectAttack()
	{
		switch (currentOpponent)
		{
			case Opponent.Blob:
				return Mathf.RoundToInt((float)GD.RandRange(0, 2));

			case Opponent.Tensor:
			{
				int num = Mathf.RoundToInt((float)GD.RandRange(0, 4));
				string anim;
				switch (num)
				{
					case 0:
						anim = "idle";
						break;
					case 1:
						anim = "prep1";
						break;
					case 2:
						anim = "prep2";
						break;
					case 3:
						anim = "idle";
						break;
					case 4:
						anim = "idle";
						break;
					default:
						anim = "idle";
						break;
				}

				enemyRef.PlayAnimation(anim);
				currentEnemyAnim = anim;

				return num;
			}

			case Opponent.Magma:
			{
				int num = Mathf.RoundToInt((float)GD.RandRange(0, 4));
				string anim;
				switch (num)
				{
					case 0:
						anim = "idle";
						break;
					case 1:
						anim = "prep1";
						break;
					case 2:
						anim = "prep2";
						break;
					case 3:
						anim = "idle";
						break;
					case 4:
						anim = "idle";
						break;
					default:
						anim = "idle";
						break;
				}

				enemyRef.PlayAnimation(anim);
				currentEnemyAnim = anim;

				return num;
			}

			case Opponent.BossZincel:
			{
				int num = Mathf.RoundToInt((float)GD.RandRange(0, 4));
				string anim;
				switch (num)
				{
					case 0:
						anim = "idle";
						break;
					case 1:
						anim = "prep1";
						break;
					case 2:
						anim = "prep2";
						break;
					case 3:
						anim = "idle";
						break;
					case 4:
						anim = "idle";
						break;
					default:
						anim = "idle";
						break;
				}

				enemyRef.PlayAnimation(anim);
				currentEnemyAnim = anim;

				return num;
			}
				

			default:
				return 0;
		}
	}


	private void EnemyAttack()
	{
		int finalPower = Mathf.Max(currentAttackNumber - playerDefense, 0);
		playerHP = Mathf.Max(playerHP - finalPower, 0);

		SpawnDamageNumber(finalPower, new Vector2(GetBattleCamera().UnprojectPosition(Player.singleton.Translation).x, 340), false);

		if (finalPower > 0)
		{
			Controller.PlaySoundBurst(BattleUI.singleton.soundHurt);
			Player.PlayAnimation("hurt");
			if (playerHP > 0)
				BattleUI.singleton.GetNode<Timer>("TimerPlayerHurt").Start();
		}
			

		if (playerHP <= 0)
		{
			gameover = true;
			GetNode<Timer>("TimerGameOver").Start();
		}
		else
		{
			BattleUI.singleton.GetNode<Timer>("TimerEndEnemyTurn").Start();
		}
	}


	private void EnemyTurn()
	{
		DeallocateCards();
		
		string path;
		switch (currentOpponent)
		{
			/* *case Opponent.Blob:
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
			} */

			case Opponent.Tensor:
			{
				switch (enemyAttackIndex)
				{
					case 0:
						//EnemyAttack(2);
						currentAttackNumber = 2;
						path = "res://Instances/Attack Animations/AnimEnemyAttack1.tscn";
						break;
					case 1:
						//EnemyAttack(3);
						currentAttackNumber = 3;
						path = "res://Instances/Attack Animations/AnimEnemyAttack2.tscn";
						break;
					case 2:
					//	EnemyAttack(4);
					currentAttackNumber = 4;
						path = "res://Instances/Attack Animations/AnimEnemyAttack3.tscn";
						break;
					case 3:
						//EnemyAttack(2);
						currentAttackNumber = 2;
						path = "res://Instances/Attack Animations/AnimEnemyAttack1.tscn";
						break;
					case 4:
						//EnemyAttack(2);
						currentAttackNumber = 2;
						path = "res://Instances/Attack Animations/AnimEnemyAttack1.tscn";
						break;
					default:
						currentAttackNumber = 2;
						path = "res://Instances/Attack Animations/AnimEnemyAttack1.tscn";
						break;
				}

				break;
			}

			case Opponent.Magma:
			{
				switch (enemyAttackIndex)
				{
					case 0:
						//EnemyAttack(2);
						currentAttackNumber = 2;
						path = "res://Instances/Attack Animations/AnimEnemyAttack1.tscn";
						break;
					case 1:
						//EnemyAttack(3);
						currentAttackNumber = 3;
						path = "res://Instances/Attack Animations/AnimEnemyAttack2.tscn";
						break;
					case 2:
					//	EnemyAttack(4);
					currentAttackNumber = 4;
						path = "res://Instances/Attack Animations/AnimEnemyAttack3.tscn";
						break;
					case 3:
						//EnemyAttack(2);
						currentAttackNumber = 2;
						path = "res://Instances/Attack Animations/AnimEnemyAttack1.tscn";
						break;
					case 4:
						//EnemyAttack(2);
						currentAttackNumber = 2;
						path = "res://Instances/Attack Animations/AnimEnemyAttack1.tscn";
						break;
					default:
						currentAttackNumber = 2;
						path = "res://Instances/Attack Animations/AnimEnemyAttack1.tscn";
						break;
				}

				break;
			}

			case Opponent.BossZincel:
			{
				switch (enemyAttackIndex)
				{
					case 0:
						//EnemyAttack(2);
						currentAttackNumber = 2;
						path = "res://Instances/Attack Animations/AnimEnemyAttack1.tscn";
						break;
					case 1:
						//EnemyAttack(3);
						currentAttackNumber = 3;
						path = "res://Instances/Attack Animations/AnimEnemyAttack2.tscn";
						break;
					case 2:
					//	EnemyAttack(4);
					currentAttackNumber = 4;
						path = "res://Instances/Attack Animations/AnimEnemyAttack3.tscn";
						break;
					case 3:
						//EnemyAttack(2);
						currentAttackNumber = 2;
						path = "res://Instances/Attack Animations/AnimEnemyAttack1.tscn";
						break;
					case 4:
						//EnemyAttack(2);
						currentAttackNumber = 2;
						path = "res://Instances/Attack Animations/AnimEnemyAttack1.tscn";
						break;
					default:
						currentAttackNumber = 2;
						path = "res://Instances/Attack Animations/AnimEnemyAttack1.tscn";
						break;
				}

				break;
			}

			default:
				path = string.Empty;
				break;
		}

		var anim = (Spatial)GD.Load<PackedScene>(path).Instance();
		anim.Translation = Player.singleton.Translation;
		anim.GetNode<Timer>("TimerDamage").Connect("timeout", this, "EnemyAttack");
		anim.GetNode<AnimationPlayer>("AnimationPlayer").Play("Animation");
		GetTree().GetRoot().AddChild(anim);

		//if (!gameover)
		//	timerEndEnemyTurn.Start();
	}


	private void PlayerTurn()
	{
		playerMP = Mathf.Min(drawnThisTurn ? ++playerMP : playerMP + 2, playerMPCap);
		playerDefense = 0;
		jokerThisTurn = false;
		drawnThisTurn = false;
		SwapDimension();
		ShowUIMin(true);
		if (hand.Count == 0 && cardStashCurrentBattle.Count == 0)
		{
			cardStashCurrentBattle = new List<CardStruct>(cardStash);
			RandomizeHand();
		}
		
		InstantiateCardsInHand();
		enemyAttackIndex = EnemySelectAttack();
		//GD.Print($"Player has {cardStashCurrentBattle.Count} cards left");
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
		five = false;
		ShowUI(false);
		Controller.GotoScene(Controller.PreBattleScene);
		Player.singleton.Translation = Controller.PreBattlePosition;
		numJokersCurrent = numJokers;
		drawnThisTurn = false;
		jokerThisTurn = false;
		Controller.PlayMusic(Controller.PreBattleMusic);
		// set player face
		Player.State = Player.ST.Move;
		battleMode = false;
		Controller.Fade(false, 0.5f);
		GetNode<Timer>("TimerDestroyFieldEnemy").Start();
		GetNode<Timer>("TimerAfterBattleEvent").Start();
	}


	private void GameOver()
	{
		Controller.FadeOutMusic(1f);
		Controller.Fade(true, 1f);
		GetNode<Timer>("TimerGameOver2").Start();
	}


	private void GameOver2()
	{
		Controller.PlayMusic(Controller.MusicGameOver);
		Controller.GotoScene("res://Scenes/GameOver.tscn");
		Controller.Fade(false, 1.5f);
	}


	private void PlayerHurtEnd()
	{
		Player.PlayAnimation("dr_walk");
	}


	private void EnemyHurtEnd()
	{
		enemyRef.PlayAnimation(currentEnemyAnim);
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


	private void AfterBattleEvent()
	{
		if (GetTree().CurrentScene.Filename == "res://Scenes/Path4.tscn")
		{
			GetTree().GetRoot().GetNode<Spatial>("Scene").GetNode<NPC>("NPCZincel").QueueFree();
			GetTree().GetRoot().GetNode<Spatial>("Scene").GetNode<StaticBody>("StaticBody").QueueFree();
			Controller.SetFlag("boss", 1);
		}
	}
}
