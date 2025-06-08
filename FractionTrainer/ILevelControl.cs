using System;

namespace FractionTrainer
{
    public interface ILevelControl
    {
        /// <summary>
        /// Событие, которое возникает, когда пользователь завершает уровень.
        /// Возвращает true, если ответ правильный, и false, если неправильный.
        /// </summary>
        event EventHandler<bool> LevelCompleted;
    }
}