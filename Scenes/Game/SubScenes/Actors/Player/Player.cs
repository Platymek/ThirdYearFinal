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
			if (Input.IsActionJustPressed(action))
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

				float angle = move.Rotated(-Mathf.Pi * .5f).Angle();

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

				if (_bufferedAction != null)
				{
					// only dodge if the user is pointing the stick in a direction they can dodge in
					if (_bufferedAction == "dodge" && (angle is > Mathf.Pi * .2f or < Mathf.Pi * -.2f))
					{
						switch (angle)
						{
							// if the stick is pointing backwards, dodge back
							case > Mathf.Pi * .6f:
							case < Mathf.Pi * -.6f:
							case 0:
								State = "dodge_down";
								break;
							// otherwise, if the stick is pointing right, dodge right
							case < 0:
								State = "dodge_right";
								break;
							// else, dodge left
							default:
								State = "dodge_left";
								break;
						}

						_dodgeTimer.Start();
					}
					else if (_bufferedAction == "punch")
					{
						Hurt(0, 4);
					}

					_bufferedAction = null;
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
