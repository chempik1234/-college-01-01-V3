using SP2023UserDanisV32.DataModel;
using SP2023UserDanisV32.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SP2023UserDanisV32.Pages
{
	/// <summary>
	/// Логика взаимодействия для AgentPage.xaml
	/// </summary>
	public partial class AgentPage : Page, INotifyPropertyChanged
	{
		public IEnumerable<Agent> Agents { get; set; }
		public IEnumerable<AgentType> FilterItems { get; set; }

		public string SearchText { get; set; }
		public AgentType SearchType { get; set; }

		public int PerPage = 10;
		private int pageIndex = 0;
		public int PageIndex { get { return pageIndex + 1; } }

		public event PropertyChangedEventHandler PropertyChanged;

		public string Lt { get => "<"; }
		public string Gt { get => ">"; }

		public IEnumerable<string> SortingItems { get; set; }

		public AgentPage()
		{
			// filter source
			var filterItems = ShelestV3DanisEntities.GetContext().AgentType.OrderBy(p => p.Title).ToList();
			filterItems.Insert(0, new AgentType() { ID = 0, Title = "Все типы" });

			FilterItems = filterItems;

			// sorting source
			SortingItems = new string[7]
			{
				"Сортировка",
				"Наименование (A-Z, А-Я)",
				"Наименование (Я-А, Z-A)",
				"Размер скидки (ниже)",
				"Размер скидки (выше)",
				"Приоритет (ниже)",
				"Приоритет (выше)"
			};

			InitializeComponent();

			RefreshAgentsList();

			DataContext = this;

			SortDropdown.SelectedIndex = 0;
			FilterDropdown.SelectedIndex = 0;

			UpdatePriorityBtn();
		}

		private IEnumerable<Agent> AgentsWithCurrentFilters()
		{
			IEnumerable<Agent> agents = ShelestV3DanisEntities.GetContext().Agent;

			// filter by search
			if (!string.IsNullOrEmpty(SearchText))
			{
				agents = agents.Where(p => p.Title.Contains(SearchText) || p.Email.Contains(SearchText) || p.Phone.Contains(SearchText));
			}

			// filter by type
			if (SearchType != null && SearchType.ID > 0)
			{
				agents = agents.Where(p => p.AgentType == SearchType);
			}


			switch (SortDropdown.SelectedIndex)
			{
				case 1:
					agents = agents.OrderBy(p => p.Title);
					break;
				case 2:
					agents = agents.OrderByDescending(p => p.Title);
					break;
				case 3:
					agents = agents.OrderBy(p => p.CurrentDiscount);
					break;
				case 4:
					agents = agents.OrderByDescending(p => p.CurrentDiscount);
					break;
				case 5:
					agents = agents.OrderBy(p => p.CurrentPriority);
					break;
				case 6:
					agents = agents.OrderByDescending(p => p.CurrentPriority);
					break;
			}

			return agents;
		}

		private void RefreshAgentsList()
		{ 
			RefreshPaginationButtons();

			// pagination
			Agents = AgentsWithCurrentFilters().Skip(pageIndex * PerPage).Take(PerPage).ToList();

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Agents)));
		}

		private void RefreshPaginationButtons()
		{
			if (PaginationPanel == null)
				return;

			PaginationPanel.Children.Clear();

			int pagesCount = (int)Math.Ceiling(Convert.ToDecimal(AgentsWithCurrentFilters().Count()) / Convert.ToDecimal(PerPage));

			if (pagesCount < 2)
			{
				NextBtn.Visibility = Visibility.Collapsed;
				PrevBtn.Visibility = Visibility.Collapsed;
				return;
			}

			if (pageIndex >= pagesCount)
				pageIndex = pagesCount - 1;

			if (pageIndex < 0)
				pageIndex = 0;

			PrevBtn.Visibility = (pageIndex == 0) ? Visibility.Collapsed : Visibility.Visible;
			NextBtn.Visibility = (pageIndex == pagesCount - 1) ? Visibility.Collapsed : Visibility.Visible;

			for (int i = 0; i < pagesCount; i++)
			{
				Button btn = new Button();
				btn.FontSize = 16;
				btn.Background = new SolidColorBrush();
				btn.BorderThickness = new Thickness(0, 0, 0, 0);
				btn.Height = 40;
				btn.Content = (i + 1).ToString();
				btn.Click += OnPaginationClick;

				PaginationPanel.Children.Add(btn);
			}
		}

		private void OnPaginationClick(object sender, RoutedEventArgs e)
		{
			Button btn = sender as Button;
			pageIndex = int.Parse(btn.Content.ToString()) - 1;

			RefreshAgentsList();
		}

		private void SearchInput_TextChanged(object sender, TextChangedEventArgs e)
		{
			SearchText = SearchInput.Text;
			RefreshAgentsList();
        }

		private void FilterDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			SearchType = FilterDropdown.SelectedItem as AgentType;
			RefreshAgentsList();
		}

		private void SortDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			RefreshAgentsList();
		}

		private void PrevBtn_Click(object sender, RoutedEventArgs e)
		{
			pageIndex -= 1;

			RefreshAgentsList();
		}

		private void NextBtn_Click(object sender, RoutedEventArgs e)
		{
			pageIndex += 1;

			RefreshAgentsList();
		}

		private void AgentsLV_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			UpdatePriorityBtn();
		}

		private void UpdatePriorityBtn()
		{
			PriorityButton.Visibility = (AgentsLV.SelectedItems.Count > 0) ? Visibility.Visible : Visibility.Collapsed;
		}

		private void PriorityButton_Click(object sender, RoutedEventArgs e)
		{
			var list = new List<Agent>();
            foreach (var item in AgentsLV.SelectedItems)
            {
				list.Add(item as Agent);
            }
            new PriorityWindow(list).ShowDialog();

			RefreshAgentsList();
		}

		private void EditBtn_Click(object sender, RoutedEventArgs e)
		{
			if (AgentsLV.SelectedItems.Count != 1)
			{
				MessageBox.Show("Выберите ровно 1 объект для изменения! Выбрано " + AgentsLV.SelectedItems.Count, "Ошибка ввода");
				return;
			}

			SingletonManager.Navigate(new AgentForm(AgentsLV.SelectedItem as Agent));
		}

		private void AddBtn_Click(object sender, RoutedEventArgs e)
		{
			SingletonManager.Navigate(new AgentForm(null));
		}
	}
}
