using EasyCaching.Core;
using Microsoft.AspNetCore.OutputCaching;

namespace Api.Middleware
{
    public class DiskOutputCacheStore : IOutputCacheStore
    {
        private readonly Dictionary<string, HashSet<string>> _taggedEntries = new();
        private readonly object _tagsLock = new();

        private IEasyCachingProvider _provider;
        private OutputCacheOptions _options;

        public DiskOutputCacheStore(IEasyCachingProvider provider, OutputCacheOptions options)
        {
            _provider = provider;
            _options = options;
        }

        public ValueTask EvictByTagAsync(string tag, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();

            // Taken from https://source.dot.net/#Microsoft.AspNetCore.OutputCaching/Memory/MemoryOutputCacheStore.cs,194c6c0d54b6b08d

            ArgumentNullException.ThrowIfNull(tag);

            lock (_tagsLock)
            {
                if (_taggedEntries.TryGetValue(tag, out var keys))
                {
                    if (keys != null && keys.Count > 0)
                    {
                        // If MemoryCache changed to run eviction callbacks inline in Remove, iterating over keys could throw
                        // To prevent allocating a copy of the keys we check if the eviction callback ran,
                        // and if it did we restart the loop.

                        var i = keys.Count;
                        while (i > 0)
                        {
                            var oldCount = keys.Count;
                            foreach (var key in keys)
                            {
                                _provider.Remove(key);
                                i--;
                                if (oldCount != keys.Count)
                                {
                                    // eviction callback ran inline, we need to restart the loop to avoid
                                    // "collection modified while iterating" errors
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return ValueTask.CompletedTask;
        }

        public ValueTask<byte[]?> GetAsync(string key, CancellationToken cancellationToken)
        {
            var response = _provider.Get<byte[]>(key);
            return ValueTask.FromResult(response?.Value);
        }

        public ValueTask SetAsync(string key, byte[] value, string[]? tags, TimeSpan validFor, CancellationToken cancellationToken)
        {
            _provider.Set<byte[]>(key, value, validFor);
            return ValueTask.CompletedTask;
        }
    }
}