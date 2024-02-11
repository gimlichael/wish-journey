using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wish.Shared
{
	public static class TaskExtensions
	{
		public static async Task<List<T>> ToListAsync<T>(this Task<IEnumerable<T>> source)
		{
			var presult = await source.ConfigureAwait(false);
			return presult.ToList();
		}
	}
}
