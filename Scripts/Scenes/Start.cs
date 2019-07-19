using Godot;
using System;

public class Start : Node2D
{
	private void StartGame()
	{
		Controller.GotoScene("res://Scenes/2D/Title.tscn");
	}
}
