using Godot;
using System;

public class LippoHouse : Spatial
{

	// ================================================================
	
	public override void _Ready()
	{
		if (Controller.Flag("read_note") == 1)
		{
			GetNode<NPC>("Note").QueueFree();
			GetNode<Door>("DoorArea").GetNode<CollisionShape>("CollisionShape").Disabled = false;
		}
	}

	// ================================================================

	public void ReadNote()
	{
		Controller.SetFlag("read_note", 1);
		GetNode<NPC>("Note").QueueFree();
		GetNode<Door>("DoorArea").GetNode<CollisionShape>("CollisionShape").Disabled = false;
	}
}
