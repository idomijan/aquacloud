using System.Collections.Generic;
using System.Threading.Tasks;
using Aquacloud.Data;

namespace Aquacloud
{
	public interface INoteEntryStore
    {
		Task<NoteEntry> GetByIdAsync(string id);
		Task<IEnumerable<NoteEntry>> GetAllAsync();
		Task AddAsync(NoteEntry entry);
		Task UpdateAsync(NoteEntry entry);
		Task DeleteAsync(NoteEntry entry);
	}
}
