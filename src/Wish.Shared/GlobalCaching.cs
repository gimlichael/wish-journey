using Cuemon.Runtime.Caching;

namespace Wish.Shared
{
	public static class GlobalCaching
	{
		private static readonly Lazy<SlimMemoryCache> Singleton = new(() => new SlimMemoryCache(), LazyThreadSafetyMode.ExecutionAndPublication);

		public static SlimMemoryCache Cache => Singleton.Value;
	}
}
