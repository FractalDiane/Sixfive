using Godot;
using System;

public class MainCamera : Camera
{
	public static Camera singleton;

	MainCamera()
	{
		singleton = this;
	}

	// ================================================================
	
	public override void _Ready()
	{
		
	}
}
