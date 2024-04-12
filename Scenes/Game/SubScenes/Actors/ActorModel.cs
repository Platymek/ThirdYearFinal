using Godot;
using System;

public partial class ActorModel : Node
{
	// Properties //

	[Export] public bool _checkLeftHand = false;
	[Export] private bool _checkRightHand = false;

	[Export] private bool _checkLeftFoot = false;
	[Export] private bool _checkRightFoot = false;

	[Export] private string _animationPrefix;

	[Export] public float NextAnimationLength = 0;

	[Export] public string Animation
	{
		get => _animationPlayer.CurrentAnimation;

		set
		{
			if (_animationPlayer == null) return;

			_animationPlayer?.Play($"{_animationPrefix}{value}");

			if (NextAnimationLength > 0)
			{
				_animationPlayer.SpeedScale = (float)(_animationPlayer.CurrentAnimationLength
					/ NextAnimationLength);

				NextAnimationLength = 0;
			}
			else
			{
				_animationPlayer.SpeedScale = 1;
			}
		}
	}

	[Export] private Area3D _leftHandHurtbox;
	[Export] private Area3D _rightHandHurtbox;

	[Export] private Area3D _leftFootHurtbox;
	[Export] private Area3D _rightFootHurtbox;

	[Export] public StandardMaterial3D StandardMaterial3D;

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
