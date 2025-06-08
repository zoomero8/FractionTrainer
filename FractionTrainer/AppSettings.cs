namespace FractionTrainer
{
    public class AppSettings
    {
        // Это свойство будет хранить выбранную тему (Light или Dark)
        public Theme CurrentTheme { get; set; } = Theme.Light; // По умолчанию светлая

        // Сюда можно будет в будущем добавить другие настройки,
        // например, HeartColorHex, если захотите вернуть их.
    }
}