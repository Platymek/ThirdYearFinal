using Godot;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
public partial class Player : Actor
{
	// Properties //
	
	private PlayerStats _playerStats;
	private PlayerAttackStats _playerAttackStats;
	private ActorModel _model;

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


			if (_playerAttackStats != null)
			{
				_playerAttackStats.Cancellable = false;
			}

			if (_model != null)
			{
				_model.NextAnimationLength = 0;
			}


			switch (value)
			{
				case "idle":

					if (_playerAttackStats != null)
					{
						_playerAttackStats.Cancellable = true;
						_playerAttackStats.DamageReactMutliplier = 1;
						_playerAttackStats.KnockReactMutliplier = 1;
					}

					break;

				
				case "dodge_stun":

					GetNode<Timer>("DodgeStunTimer").Start();
					_playerAttackStats.Cancellable = true;

					break;
				
				
				case "dodge_left":
				case "dodge_right":
				case "dodge_back":

					// cannot cancel dodge into another dodge
					if (State == "dodge_stun") return;
					
					break;
			}

			base.State = state;
		}
	}

	protected override string Animation 
	{ 
		get => base.Animation; 
		set
		{
			base.Animation = value;

			if (_model == null) return;

			_model.NextAnimationLength = (float)GetAnimationCurrentLength();
			_model.Animation = value;
		}
	}


	// Node Functions //

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();

		_playerStats = _stats as PlayerStats;
		_playerAttackStats = AttackStats as PlayerAttackStats;
		_model = GetNode <ActorModel>("Model");

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
			bool actionPressed
				= Input.IsActionJustPressed(action)
				  || (action == "punch" && Input.IsActionPressed(action));
			
			if (actionPressed)
			{
				_inputBufferTimer.Start();
				_bufferedAction = action;
			}
		}

		float fDelta = (float)delta;

		Vector2 move = Input.GetVector("move_left", "move_right",
			"move_down", "move_up");

		float halfPi = Mathf.Pi * .5f;

		float angle = move.Rotated(-halfPi).Angle();


		switch (State)
		{
			case "idle":

				Move(move);

				if (move != Vector2.Zero)
				{
					switch (Mathf.RoundToInt(angle / halfPi))
					{
						case 1:
							_model.Animation = "walk_left";
							break;

						case 0:
							_model.Animation = "walk_forward";
							break;

						case-1:
							_model.Animation = "walk_right";
							break;

						case 2:
							_model.Animation = "walk_back";
							break;
					}
				}
				else
				{
					_model.Animation = "idle";
				}
				
				break;
			

			case "dodge_left": ProcessDodge(Vector2.Left);
				break;
				break;
			
			case "dodge_right": ProcessDodge(Vector2.Right);
				break;
			
			case "dodge_back": ProcessDodge(Vector2.Up);
				break;


			case "punch":

				Move(Vector2.Down * .25f);

				break;

			case "punch_heavy":

				Move(Vector2.Down);

				break;


			case "charge":

				if (_playerAttackStats.PunchChargeState 
					!= PlayerAttackStats.PunchChargeStates.None
					&& !Input.IsActionPressed("punch"))
				{
					switch (_playerAttackStats.PunchChargeState)
					{
						case PlayerAttackStats.PunchChargeStates.Light:

							State = "punch_light";

							break;

						case PlayerAttackStats.PunchChargeStates.Medium:

							State = "punch_medium";

							break;

						case PlayerAttackStats.PunchChargeStates.Heavy:

							State = "punch_heavy";

							break;
					}
				}

				break;


			case "block":

				if (!Input.IsActionPressed("block"))
				{
					State = "block_end";
				}

				break;
		}


		// check buffered action
		if (_bufferedAction != null && _playerAttackStats.Cancellable)
		{
			// only dodge if the user is pointing the stick in a direction they can dodge in
			if (_bufferedAction == "dodge" && (angle is > Mathf.Pi * .2f or < Mathf.Pi * -.2f))
			{
				_model.NextAnimationLength = (float)_dodgeTimer.WaitTime + (float)_dodgeStunTimer.WaitTime;

				switch (angle)
				{
					// if the stick is pointing backwards, dodge back
					case > Mathf.Pi * .6f:
					case < Mathf.Pi * -.6f:
					case 0:
						State = "dodge_back";
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
				State = "charge";
			}
			else if (_bufferedAction == "block")
			{
				State = "block_start";
			}


			_bufferedAction = null;
		}
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

	protected override void Hurt(float damage, float knock)
	{
		base.Hurt(damage, knock);

		if (State == "block_start")
		{
			State = "idle";
			_bufferedAction = null;
		}
	}
}
