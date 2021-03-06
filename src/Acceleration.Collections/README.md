# Acceleration.Collections

Helpers and convenience methods for `System.Collections`

* `ICollection<T>.Add(IEnumerable<T>)` Add to a collection in bulk
* `ICollection<T>.Add(params T[])` Add to a collection in bulk
* `ICollection<T>.RandomElement()` - pull a random element
* `ICollection<T>.RandomElement(n)` - pull N unique random elements
* `IDictionary<Tk,Tv>.Get(Tk, Tv)` - Get a key from the
  dictionary, or return a default value
* `IDictionary<Tk,Tv>.Get(Tk, Func<Tk,Tv>)` - Get a key from the
  dictionary, or calculate a default value
* `IDictionary<Tk,Tv>.Ensure(Tk, Func<Tk,Tv>)` - Ensure this key is in
  this dictionary, setting it if needed
* `IDictionary.Ensure(Tk, Func<Tk,Tv>)` - Ensure this key is in
  this dictionary, setting it if needed
* `IDictionary<Tk,Tv>.Update(IEnumerable<KeyValuePair<TKey,
  TValue>>)` - Update the dictionary from another source. Overwrites
  keys that exist in both