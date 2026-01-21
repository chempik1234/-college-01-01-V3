using SP2023UserDanisV32.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace SP2023UserDanisV32.Windows
{
	/// <summary>
	/// Логика взаимодействия для PriorityWindow.xaml
	/// </summary>
	public partial class PriorityWindow : Window
	{
		IEnumerable<Agent> agents;
		public int TotalAgents { get => agents.Count(); }

		public PriorityWindow(IEnumerable<Agent> agents)
		{
			this.agents = agents;
			InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			if (int.TryParse(PriorityInput.Text, out int newPriority)) {

				if (newPriority < 0)
				{
					MessageBox.Show("Необходимо ввести неотрицательное целое число", "Ошибка ввода");
					return;
				}

				var ctx = ShelestV3DanisEntities.GetContext();
				foreach (var item in agents)
				{
					ctx.AgentPriorityHistory.Add(new AgentPriorityHistory()
					{
						Agent = item,
						ChangeDate = DateTime.Now,
						PriorityValue = newPriority,
					});
				}
				ctx.SaveChanges();
			}else
			{
				MessageBox.Show("Необходимо ввести неотрицательное целое число", "Ошибка ввода");
			}
        }
    }
}
