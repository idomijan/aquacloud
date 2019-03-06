using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Aquacloud.Data;

namespace Aquacloud
{
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();
			entries.ItemTapped += OnItemTapped;
			newEntry.Completed += OnAddNewEntry;
		}

		//ADD NEW NOTE
		private async void OnAddNewEntry(object sender, EventArgs e)
		{
			string text = newEntry.Text;
			if (!string.IsNullOrEmpty(text)) {
				NoteEntry item = new NoteEntry { Title = text };
				await App.Entries.AddAsync(item);
				await Navigation.PushAsync(new NoteEntryEditPage(item));
				newEntry.Text = string.Empty;
			}
		}

		private async void OnItemTapped(object sender, ItemTappedEventArgs e)
		{
			NoteEntry item = e.Item as NoteEntry;
			await Navigation.PushAsync(new NoteEntryEditPage(item));
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();
			entries.ItemsSource = await App.Entries.GetAllAsync();
		}
	}
}
