using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace Aquacloud.Data
{
	class MemoryEntryStore : INoteEntryStore
    {
		private readonly Dictionary<string, NoteEntry> entries = new Dictionary<string, NoteEntry>();

		public Task<IEnumerable<NoteEntry>> GetAllAsync()
		{
			IEnumerable<NoteEntry> result = entries.Values.ToList();
			return Task.FromResult(result);
		}

		public Task AddAsync(NoteEntry entry)
		{
			entries.Add(entry.Id, entry);
			return Task.CompletedTask;
		}

		public Task UpdateAsync(NoteEntry entry)
		{
			return Task.CompletedTask;
		}

		public Task DeleteAsync(NoteEntry entry)
		{
			entries.Remove(entry.Id);
			return Task.CompletedTask;
		}

		public Task<NoteEntry> GetByIdAsync(string id)
		{
			NoteEntry entry = null;
			entries.TryGetValue(id, out entry);
			return Task.FromResult(entry);
		}
	}
}
