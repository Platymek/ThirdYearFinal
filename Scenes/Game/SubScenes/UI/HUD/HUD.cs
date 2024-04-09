using Godot;
using System;

public partial class HUD : Control
{
	[Export] private Actor _player;
	[Export] private Actor _opponent;
	[Export] private Timer _timer;

	[ExportGroup("Labels")]
	[Export] private ProgressBar _playerHealth;
	[Export] private ProgressBar _opponentHealth;
	[Export] private Label _time;
	[Export] private Label _roundLabel;

    public override void _Ready()
    {
        base._Ready();

		// set round label to say the current round progress
        _roundLabel.Text 
			= $"Round {GetNode<Global>("/root/Global").CurrentRoundProgress}";
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
		_playerHealth.Value = _player.HealthPercentage;
		_opponentHealth.Value = _opponent.HealthPercentage;

		if (_timer.IsStopped())
		{
			_time.Text = $"{_timer.WaitTime}s";
		}
		else
		{
			_time.Text = $"{(int)_timer.TimeLeft}s";
		}
	}
}
