using System.Threading.Tasks;

namespace Aquacloud.Data
{
	public static class MockDataExtensionMethods
	{
		public static void LoadMockData(this INoteEntryStore store)
		{
			NoteEntry a = new NoteEntry
			{
				Title = "Sprint Planning Meeting",
				Text = "1. Scope 2. Backlog 3. Duration"
			};

			NoteEntry b = new NoteEntry
			{
				Title = "Daily Scrum Stand-up",
				Text = "1. Yesterday 2. Today 3. Impediments"
			};

			NoteEntry c = new NoteEntry
			{
				Title = "Sprint Retrospective",
				Text = "1. Reflection 2. Actions"
			};

			Task.WhenAll(
					store.AddAsync(a),
					store.AddAsync(b),
					store.AddAsync(c))
			.ConfigureAwait(false);
		}
	}
}