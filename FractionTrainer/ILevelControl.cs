using System;

namespace FractionTrainer
{
    public interface ILevelControl
    {
        event EventHandler<bool> LevelCompleted;
    }
}