using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace AppoMobi.Specials;

/// <summary> 
/// Represents a dynamic data collection that provides notifications when items get added, removed, or when the whole list is refreshed. 
/// </summary> 
/// <typeparam name="T"></typeparam> 
public class ObservableRangeCollection<T> : ObservableCollection<T>
{

    /// <summary> 
    /// Initializes a new instance of the System.Collections.ObjectModel.ObservableCollection(Of T) class. 
    /// </summary> 
    public ObservableRangeCollection()
        : base()
    {
    }

    /// <summary> 
    /// Initializes a new instance of the System.Collections.ObjectModel.ObservableCollection(Of T) class that contains elements copied from the specified collection. 
    /// </summary> 
    /// <param name="collection">collection: The collection from which the elements are copied.</param> 
    /// <exception cref="System.ArgumentNullException">The collection parameter cannot be null.</exception> 
    public ObservableRangeCollection(IEnumerable<T> collection)
        : base(collection)
    {
    }

    /// <summary>
    /// Adds the elements of the specified collection to the end of the ObservableCollection(Of T),
    /// raising a single Add notification carrying the items and the correct starting index, so
    /// index-aware virtualizing consumers (e.g. DrawnUI SkiaLayout) can handle the append
    /// incrementally and keep scroll position.
    /// </summary>
    public void AddRange(IEnumerable<T> collection)
    {
        if (collection == null)
            throw new ArgumentNullException(nameof(collection));

        CheckReentrancy();

        int startIndex = Count;
        var changedItems = collection is List<T> ? (List<T>)collection : new List<T>(collection);
        if (changedItems.Count == 0)
            return;

        foreach (var i in changedItems)
            Items.Add(i);

        OnPropertyChanged(new PropertyChangedEventArgs("Count"));
        OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, changedItems, startIndex));
    }

    /// <summary>
    /// Removes the first occurence of each item in the specified collection from
    /// ObservableCollection(Of T), raising a single Reset notification: scattered items have no
    /// single starting index, so Reset is the only truthful event (consumers rebuild).
    /// For removing a CONSECUTIVE block prefer <see cref="RemoveRange(int, int)"/> — it raises an
    /// index-aware Remove that virtualizing consumers handle incrementally without a rebuild.
    /// </summary>
    public void RemoveRange(IEnumerable<T> collection)
    {
        if (collection == null)
            throw new ArgumentNullException(nameof(collection));

        CheckReentrancy();

        bool removedAny = false;
        foreach (var i in collection)
            removedAny |= Items.Remove(i);

        if (!removedAny)
            return;

        OnPropertyChanged(new PropertyChangedEventArgs("Count"));
        OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }



    /// <summary>
    /// Removes <paramref name="count"/> contiguous items starting at <paramref name="index"/>, raising
    /// a single Remove notification carrying the correct OldStartingIndex. This lets index-aware
    /// virtualizing consumers (e.g. DrawnUI SkiaLayout) handle the removal incrementally and keep scroll
    /// position, instead of the index-less Reset that RemoveRange(IEnumerable) raises.
    /// </summary>
    public void RemoveRange(int index, int count)
    {
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index));
        if (count < 0 || index + count > Count)
            throw new ArgumentOutOfRangeException(nameof(count));
        if (count == 0)
            return;

        CheckReentrancy();

        var removed = new List<T>(count);
        for (int i = 0; i < count; i++)
            removed.Add(Items[index + i]);

        for (int i = 0; i < count; i++)
            Items.RemoveAt(index);

        OnPropertyChanged(new PropertyChangedEventArgs("Count"));
        OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Remove, removed, index));
    }

    /// <summary>
    /// Inserts a contiguous range starting at <paramref name="index"/>, raising a single Add
    /// notification with the correct NewStartingIndex so index-aware virtualizing consumers can handle
    /// the prepend/insert incrementally (e.g. a backward-scrolling moving window).
    /// </summary>
    public void InsertRange(int index, IEnumerable<T> collection)
    {
        if (collection == null)
            throw new ArgumentNullException(nameof(collection));
        if (index < 0 || index > Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        CheckReentrancy();

        var added = collection is List<T> list ? list : new List<T>(collection);
        if (added.Count == 0)
            return;

        var at = index;
        foreach (var item in added)
            Items.Insert(at++, item);

        OnPropertyChanged(new PropertyChangedEventArgs("Count"));
        OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Add, added, index));
    }

    /// <summary>
    /// Clears the current collection and replaces it with the specified item.
    /// </summary>
    public void Replace(T item) => ReplaceRange(new T[] { item });

    /// <summary>
    /// Clears the current collection and replaces it with the specified collection.
    /// Raises a single Replace notification (not Reset) so virtualizing consumers (e.g. DrawnUI
    /// SkiaScroll) can swap item contexts in place and keep scroll position, instead of treating it
    /// as a brand-new collection and jumping back to the top.
    /// NOTE: emits a multi-item Replace. Consumers that do not support range Replace (some native
    /// MAUI/WPF CollectionView bindings) may need the Reset behavior - use <see cref="ReplaceRangeReset"/>.
    /// </summary>
    public void ReplaceRange(IEnumerable<T> collection)
    {
        if (collection == null)
            throw new ArgumentNullException(nameof(collection));

        CheckReentrancy();

        var oldItems = new List<T>(Items);

        Items.Clear();
        foreach (var item in collection)
            Items.Add(item);

        OnPropertyChanged(new PropertyChangedEventArgs("Count"));
        OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));

        if (oldItems.Count == 0 && Items.Count == 0)
            return;

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Replace,
            new List<T>(Items),
            oldItems,
            0));
    }

    /// <summary>
    /// Clears the current collection and replaces it with the specified collection, raising a single
    /// Reset notification. Use when the consumer does not support range Replace notifications.
    /// </summary>
    public void ReplaceRangeReset(IEnumerable<T> collection)
    {
        if (collection == null)
            throw new ArgumentNullException(nameof(collection));

        CheckReentrancy();

        Items.Clear();
        foreach (var i in collection)
            Items.Add(i);

        OnPropertyChanged(new PropertyChangedEventArgs("Count"));
        OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

}