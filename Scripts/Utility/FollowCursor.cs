using Godot;
using System;

public class FollowCursor : Particles2D
{

	// ================================================================
	
	public override void _Process(float delta)
	{
		Position = GetGlobalMousePosition();
	}
}
