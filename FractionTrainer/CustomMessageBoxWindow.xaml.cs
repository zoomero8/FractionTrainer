using System.Windows;

namespace FractionTrainer
{
    public partial class CustomMessageBoxWindow : Window
    {
        public string MessageTitle { get; set; } // Используем обычные свойства для простоты
        public string MessageText { get; set; }

        // Конструктор для простоты можно оставить пустым или принимать параметры сразу
        public CustomMessageBoxWindow(string message, string title = "Сообщение")
        {
            InitializeComponent();
            this.MessageTitle = title;
            this.MessageText = message;
        }

        // Можно было бы использовать DependencyProperties для Title и Message,
        // но для простого диалога обычных свойств, установленных перед ShowDialog(), достаточно.

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Устанавливаем текст после загрузки окна, чтобы XAML элементы были доступны
            TitleTextBlock.Text = MessageTitle;
            MessageTextBlock.Text = MessageText;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true; // Указывает, что диалог завершился успешно (если нужно отслеживать)
            this.Close();
        }

        // Статический метод для удобного вызова, имитирующий MessageBox.Show
        public static void Show(string message, string title = "Сообщение", Window owner = null)
        {
            CustomMessageBoxWindow customBox = new CustomMessageBoxWindow(message, title);
            if (owner != null)
            {
                customBox.Owner = owner;
            }
            else
            {
                // Попытка установить активное окно или главное окно как Owner, если оно не указано
                if (Application.Current?.Windows.Count > 0)
                {
                    var activeWindow = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
                    customBox.Owner = activeWindow ?? Application.Current.MainWindow;
                }
            }
            customBox.Loaded += customBox.Window_Loaded; // Подписываемся на Loaded здесь
            customBox.ShowDialog();
        }
    }
}