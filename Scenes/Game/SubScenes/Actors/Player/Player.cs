using Godot;
using System;
using System.Collections.Generic;

public partial class Player : Actor
{
	// Properties //
	
	private PlayerStats _playerStats;

	private Timer _dodgeTimer;
	private Timer _dodgeStunTimer;
	
	// input buffer properties
	private Timer _inputBufferTimer;
	private StringName _bufferedAction = null;
	private string[] _bufferableActions = new[] { "dodge", "punch", "block" };

	public override string State
	{
		get => base.State;
		
		protected set
		{
			string state = value;

			switch (value)
			{
				case "dodge_left":
				case "dodge_right":
				case "dodge_down":
					break;
				
				case "dodge_stun":
					GetNode<Timer>("DodgeStunTimer").Start();
					break;
			}
			
			base.State = state;
		}
	}


	// Node Functions //
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();

		_playerStats = _stats as PlayerStats;

		_bufferedAction = null;
		
		// initialise timers
		_dodgeTimer = GetNode<Timer>("DodgeTimer");
		_dodgeTimer.WaitTime = _playerStats.DodgeDuration;
		
		_dodgeStunTimer = GetNode<Timer>("DodgeStunTimer");
		_dodgeStunTimer.WaitTime = _playerStats.DodgeStunDuration;
		
		_inputBufferTimer = GetNode<Timer>("InputBufferTimer");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		base._Process(delta);
		
		// check if any bufferable actions have been pressed

		foreach (string action in _bufferableActions)
		{
			if (Input.IsActionPressed(action))
			{
				_inputBufferTimer.Start();
				_bufferedAction = action;
			}
		}

		float fDelta = (float)delta;

		switch (State)
		{
			case "idle":
			case "move_up":
			case "move_down":
			case "move_left":
			case "move_right":

				Vector2 move = Input.GetVector("move_left", "move_right", 
					"move_down", "move_up");
	
				Move(move);
				
				GD.Print(Velocity.Z);

				float halfPi = Mathf.Pi * .5f;
				float angle = move.Angle() - halfPi;

				/*
				if (move != Vector2.Zero)
				{
					switch (Mathf.RoundToInt(angle / halfPi))
					{
						case 1: Animation = "move_up";
							break;
						case 0: Animation = "move_right";
							break;
						case-1: Animation = "move_down";
							break;
						case 2: Animation = "move_left";
							break;
					}
				}
				else
				{
					Animation = "idle";
				}
				*/

				if (_bufferedAction == "dodge")
				{
					if (move.X < -0.8)
					{
						State = "dodge_left";
					}
					else if (move.X > 0.8)
					{
						State = "dodge_right";
					}
					else
					{
						State = "dodge_down";
					}
					
					_dodgeTimer.Start();
				}
				
				break;
			
			case "dodge_left": ProcessDodge(Vector2.Left);
				break;
			
			case "dodge_right": ProcessDodge(Vector2.Right);
				break;
			
			case "dodge_down": ProcessDodge(Vector2.Up);
				break;
		}
		
		MoveAndSlide();
	}
	
	
	// Other Functions //

	void ProcessDodge(Vector2 direction)
	{
		Move(direction * _playerStats.DodgeSpeedMultiplier);
	}

	void OnDodgeEnd()
	{
		State = "dodge_stun";
	}
	
	void OnDodgeStunEnd()
	{
		State = "idle";
	}

	void OnInputBufferEnd()
	{
		_bufferedAction = null;
	}
}
