using SP2023UserDanisV32.Pages;
using System.Windows;

namespace SP2023UserDanisV32
{
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			SingletonManager.MainFrame = MainFrame;
			SingletonManager.Navigate(new AgentPage());
		}
	}
}
