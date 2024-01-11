using Godot;
using System;

public partial class ActorModel : Node
{
	// Properties //

	[Export] private bool _checkLeftHand = false;
	[Export] private bool _checkRightHand = false;

	[Export] private bool _checkLeftFoot = false;
	[Export] private bool _checkRightFoot = false;

	[Export] private string _animationPrefix;

	[Export] public string Animation
	{
		get => _animationPlayer.CurrentAnimation;

		set
		{
			_animationPlayer.Stop();
			GD.Print($"Animation Played: {value}");
			_animationPlayer.Play($"{_animationPrefix}{value}");
		}
	}


	[Export] private Area3D _leftHandHurtbox;
	[Export] private Area3D _rightHandHurtbox;

	[Export] private Area3D _leftFootHurtbox;
	[Export] private Area3D _rightFootHurtbox;

	private AnimationPlayer _animationPlayer;


	[Signal] public delegate void ActorHurtEventHandler(Actor actor);


	// Node Functions //

	public override void _Ready()
	{
		base._Ready();

		_animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		Animation = "idle";
	}

	public override void _Process(double delta)
	{
		base._Process(delta);


		if (_checkLeftHand)
		{
			CheckHurtbox(_leftHandHurtbox);
		}

		if (_checkRightHand)
		{
			CheckHurtbox(_rightHandHurtbox);
		}

		if (_checkLeftFoot)
		{
			CheckHurtbox(_leftFootHurtbox);
		}

		if (_checkRightFoot)
		{
			CheckHurtbox(_rightFootHurtbox);
		}
	}


	// Other Functions //

	public void CheckHurtbox(Area3D hurtbox)
	{
		foreach (var area3D in hurtbox.GetOverlappingAreas())
		{
			var victim = area3D.GetOwner<Node>();

			if (victim is Actor actor)
			{
				EmitSignal(SignalName.ActorHurt, actor);
			}

		}
	}
}
