using Microsoft.Win32;
using SP2023UserDanisV32.DataModel;
using SP2023UserDanisV32.Utils;
using System;
using System.ComponentModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace SP2023UserDanisV32.Pages
{
	/// <summary>
	/// Логика взаимодействия для AgentForm.xaml
	/// </summary>
	public partial class AgentForm : Page, INotifyPropertyChanged
	{
		public Agent EditedModel;

		ShelestV3DanisEntities ctx = ShelestV3DanisEntities.GetContext();

		bool createNew = false;
		bool resetFileName = false;
		string selectedFileName;

		int initialPriority = 0; // if we change priority, we add AgentPriorityHistory record

		public event PropertyChangedEventHandler PropertyChanged;

		public AgentForm(Agent context)
		{
			if (context == null)
			{
				createNew = true;
				EditedModel = new Agent();
			}
			else
			{
				EditedModel = context;
				initialPriority = EditedModel.CurrentPriority;

				ctx.Entry(EditedModel).Collection(a => a.ProductSale).Load();
			}

			EditedModel.Priority = EditedModel.CurrentPriority;

			InitializeComponent();
			DataContext = EditedModel;

			AgentTypeDropdown.ItemsSource = ctx.AgentType.ToList();

			ProductSaleDG.ItemsSource = EditedModel.ProductSale.ToList();

			ProductsDropDown.ItemsSource = ctx.Product.ToList();
			AgentsDropDown.ItemsSource = ctx.Agent.ToList();
		}

		private void LogoButton_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new OpenFileDialog();
			dialog.Multiselect = false;
			dialog.Filter = "Images|*.png;*.jpg";
			if (dialog.ShowDialog() == true)
			{
				resetFileName = false;
				selectedFileName = dialog.FileName;
			} else
			{
				resetFileName = true;
				selectedFileName = string.Empty;
			}
		}

		private void DeleteBtn_Click(object sender, RoutedEventArgs e)
		{
			if (EditedModel.ProductSale.Count > 0)
			{
				MessageBox.Show(string.Format("Нельзя удалить агента: есть записи о продажах ему (всего {0:d})", EditedModel.ProductSale.Count), "Удаление запрещено");
				return;
			}

			try
			{
				ctx.AgentPriorityHistory.RemoveRange(ctx.AgentPriorityHistory.Where(p => p.Agent == EditedModel));
				ctx.Shop.RemoveRange(ctx.Shop.Where(p => p.Agent == EditedModel));
				ctx.Agent.Remove(EditedModel);
				ctx.SaveChanges();

				MessageBox.Show("Успещно удалено!", "Успех");

				SingletonManager.Navigate(new AgentPage());
			}
			catch (Exception ex)
			{
				MessageBox.Show("Ошибка удаления!\n\nПодробнее: " + ex.Message, "Ошибка БД");
			}
		}

		private void SaveBtn_Click(object sender, RoutedEventArgs e)
		{
			if (!ValidateData()) { return; }

			bool needToUploadImage = !string.IsNullOrEmpty(selectedFileName);

			string oldLogoPath = EditedModel.Logo;
			string newLogoValue = string.Empty;

			if (needToUploadImage)
				newLogoValue = GenerateLogoName();
				EditedModel.Logo = newLogoValue;

			try
			{
				if (createNew)
				{
					ctx.Agent.Add(EditedModel);
				}
				else
				{
					ctx.Entry(EditedModel).State = System.Data.Entity.EntityState.Modified;
				}

				if (initialPriority != EditedModel.Priority)
				{
					ctx.AgentPriorityHistory.Add(new AgentPriorityHistory() { Agent = EditedModel, ChangeDate = DateTime.Now, PriorityValue = EditedModel.Priority });
				}

				ctx.SaveChanges();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Ошибка сохранения агента. Подробнее: " + ex.Message, "Ошибка БД");
				return;
			}

			try
			{
				foreach (var sale in EditedModel.ProductSale)
				{
					if (sale.ID == 0) // New
					{
						ctx.ProductSale.Add(sale);
					}
					else // existing
					{
						ctx.Entry(sale).State = EntityState.Modified;
					}
				}
				ctx.SaveChanges();
			}
			catch (Exception ex) {
				MessageBox.Show("Ошибка сохранения продаж. Подробнее: " + ex.Message, "Ошибка БД");
				return;
			}

			if (createNew)
			{
				if (needToUploadImage)
					UploadNewLogoFile(newLogoValue);
			}
			else
			{
				if (resetFileName || needToUploadImage)
				{
					DeleteExistingLogoFile(oldLogoPath);
					if (needToUploadImage)
					{
						UploadNewLogoFile(newLogoValue);
					}
				}
			}

			MessageBox.Show("Успешно сохранено", "Успех");
		}

		private bool ValidateData()
		{
			StringBuilder sb = new StringBuilder();
			if (string.IsNullOrWhiteSpace(EditedModel.Title))
			{
				sb.AppendLine("* Наименование не должно быть пустым");
			}
			if (string.IsNullOrWhiteSpace(EditedModel.Address))
			{
				sb.AppendLine("* Адрес не должен быть пустым");
			}
			if (string.IsNullOrWhiteSpace(EditedModel.INN))
			{
				sb.AppendLine("* ИНН не должен быть пустым");
			}
			if (string.IsNullOrWhiteSpace(EditedModel.KPP)) {
				sb.AppendLine("* КПП не должен быть пустым");
			}
			if (string.IsNullOrWhiteSpace(EditedModel.Phone))
			{
				sb.AppendLine("* Номер телефона не должен быть пустым");
			}
			if (string.IsNullOrWhiteSpace(EditedModel.Email))
			{
				sb.AppendLine("* Email не должен быть пустым");
			}
			if (string.IsNullOrWhiteSpace(EditedModel.DirectorName))
			{
				sb.AppendLine("* Имя директора не должно быть пустым");
			}
			if (EditedModel.Priority < 0)
			{
				sb.AppendLine("* Приоритет должен быть целым неотрицательным числом");
			}
			
			if (sb.Length > 0)
			{
				MessageBox.Show(sb.ToString(), "Ошибка ввода");
			}

			return sb.Length == 0;
		}

		private string GenerateLogoName()
		{
			string selectedFileExtension = selectedFileName.Split('.').Last();  // png jpg

			string generatedFilename = DateTime.Now.ToString().Replace('.', '-').Replace(':', '-').Replace(' ', '-') + "_logo." + selectedFileExtension; // agent_1_logo.png

			return "/agents/" + generatedFilename;
		}

		private void UploadNewLogoFile(string logo) // YES /agents/agent_1.png or something  NOT ../../Media/agents/agent_1.png
		{
			try
			{
				File.Copy(selectedFileName, UniversalUtils.PathToMedia(logo), overwrite: true);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Ошибка сохранения файла\n\nПуть:" + logo + "\n\nВыполнение кода продолжится\n\nПодробнее: " + ex.Message, "Ошибка сохранения файла");
			}
		}

		private void DeleteExistingLogoFile(string oldLogoPath)
		{
			try
			{
				File.Delete(UniversalUtils.PathToMedia(oldLogoPath));
			}
			catch (FileNotFoundException)
			{
				MessageBox.Show("Предупреждение: файл не найден, удаление пропущено. Выполнение кода продолжится", "Предупреждение");
			}
			catch (Exception e)
			{
				MessageBox.Show("Предупреждение: ошибка удаления файла.\n\nВыполнение кода продолжится\n\nПодробнее: " + e.Message, "Предупреждение");
			}
		}

		private void BackBtn_Click(object sender, RoutedEventArgs e)
		{
			SingletonManager.Navigate(new AgentPage());
		}

		private void ProductSaleDG_AddingNewItem(object sender, AddingNewItemEventArgs e)
		{
			var newSale = new ProductSale()
			{
				Agent = EditedModel,
				SaleDate = DateTime.Now,
				ProductCount = 1
			};
			e.NewItem = newSale;

			// ctx.ProductSale.Add(newSale);
			EditedModel.ProductSale.Add(newSale);
        }

		private void ProductSaleDG_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
		{
			if (e.EditAction == DataGridEditAction.Commit)
			{
				var sale = e.Row.DataContext as ProductSale;
				if (sale != null)
				{
					if (sale.ID == 0)
					{
						sale.Agent = EditedModel;
					}

					if (sale.ProductID == 0 || sale.Product == null)
					{
						MessageBox.Show("Выберите продукт для продажи", "Ошибка",
								  MessageBoxButton.OK, MessageBoxImage.Warning);
						return;
					}

					ctx.SaveChanges();
				}
			}
        }

		private void DeleteSale_Click(object sender, RoutedEventArgs e)
		{
			Button btn = sender as Button;
			if (btn != null)
			{
				var sale = btn.DataContext as ProductSale;
				if (sale != null)
				{
					ctx.ProductSale.Remove(sale);
					EditedModel.ProductSale.Remove(sale);
					if (sale.ID != 0)
						ctx.SaveChanges();
				}
			}
		}

		private void AddSale_Click(object sender, RoutedEventArgs e)
		{
			var newSale = new ProductSale
			{
				AgentID = EditedModel.ID,
				Agent = EditedModel,
				SaleDate = DateTime.Now,
				ProductCount = 1
			};

			EditedModel.ProductSale.Add(newSale);
			ctx.ProductSale.Add(newSale);

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EditedModel.ProductSale)));
		}
	}
}
