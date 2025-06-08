using System;
using System.Collections.Generic;
using System.Linq;

namespace FractionTrainer
{
    public enum TestLevelType
    {
        AssembleFraction,    
        MultipleChoice,      
        FindPairs            
    }

    public class GameStateManager
    {
        private readonly Random _random = new Random();
        private const int TotalLevels = 15;
        private const int MaxLives = 5;

        public int CurrentLevel { get; private set; }
        public int Lives { get; private set; }
        public List<TestLevelType> LevelSequence { get; private set; }

        public bool IsGameOver => Lives <= 0;
        public bool IsGameWon => CurrentLevel > TotalLevels;

        public GameStateManager()
        {
            LevelSequence = new List<TestLevelType>();
            StartNewGame();
        }

        public void StartNewGame()
        {
            CurrentLevel = 1;
            Lives = MaxLives;
            GenerateLevelSequence();
        }

        private void GenerateLevelSequence()
        {
            LevelSequence.Clear();
            var levelTypes = Enum.GetValues(typeof(TestLevelType)).Cast<TestLevelType>().ToList();
            for (int i = 0; i < TotalLevels; i++)
            {
                // Просто добавляем случайный тип уровня
                LevelSequence.Add(levelTypes[_random.Next(levelTypes.Count)]);
            }
        }

        public TestLevelType GetCurrentLevelType()
        {
            if (CurrentLevel > 0 && CurrentLevel <= TotalLevels)
            {
                return LevelSequence[CurrentLevel - 1];
            }
            return TestLevelType.AssembleFraction; // Запасной вариант
        }

        public void CorrectAnswer()
        {
            CurrentLevel++;
        }

        public void IncorrectAnswer()
        {
            if (Lives > 0)
            {
                Lives--;
            }
        }
    }
}
