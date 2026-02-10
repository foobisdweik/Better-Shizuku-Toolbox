namespace Shizuku.Services.Pipelines;

/// <summary>
/// A fixed-capacity circular buffer for storing the last N items.
/// Thread-safe for single-writer, single-reader usage.
/// </summary>
public sealed class CircularBuffer<T>
{
    private readonly T[] _items;
    private int _head;
    private int _count;
    private readonly object _lock = new();

    public CircularBuffer(int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity));

        _items = new T[capacity];
    }

    public int Capacity => _items.Length;

    public int Count
    {
        get { lock (_lock) return _count; }
    }

    /// <summary>Push an item, overwriting the oldest if full.</summary>
    public void Push(T item)
    {
        lock (_lock)
        {
            _items[_head] = item;
            _head = (_head + 1) % _items.Length;
            if (_count < _items.Length)
                _count++;
        }
    }

    /// <summary>
    /// Copy all items to an array in chronological order (oldest first).
    /// </summary>
    public T[] ToArray()
    {
        lock (_lock)
        {
            var result = new T[_count];
            if (_count == 0)
                return result;

            var start = _count < _items.Length ? 0 : _head;
            for (var i = 0; i < _count; i++)
            {
                result[i] = _items[(start + i) % _items.Length];
            }

            return result;
        }
    }

    /// <summary>Clear all items.</summary>
    public void Clear()
    {
        lock (_lock)
        {
            Array.Clear(_items, 0, _items.Length);
            _head = 0;
            _count = 0;
        }
    }
}
