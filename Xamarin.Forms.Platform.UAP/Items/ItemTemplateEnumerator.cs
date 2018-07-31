﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Xamarin.Forms.Platform.UWP
{
	internal static class TemplatedItemSourceFactory
	{
		internal static object Create(IEnumerable itemsSource, DataTemplate itemTemplate)
		{
			switch (itemsSource)
			{
				case IList list when itemsSource is INotifyCollectionChanged:
					return new ObservableItemTemplateCollection(list, itemTemplate);
				//case IList list:

			}

			return new ItemTemplateEnumerator(itemsSource, itemTemplate);
		}
	}

	internal class ObservableItemTemplateCollection : ObservableCollection<ItemTemplatePair>
	{
		readonly IList _innerSource;
		readonly DataTemplate _itemTemplate;

		public ObservableItemTemplateCollection(IList itemsSource, DataTemplate itemTemplate)
		{
			if (!(itemsSource is INotifyCollectionChanged notifyCollectionChanged))
			{
				throw new ArgumentException($"{nameof(itemsSource)} must implement {nameof(INotifyCollectionChanged)}");
			}

			_innerSource = itemsSource;
			_itemTemplate = itemTemplate;

			for (int n = 0; n < itemsSource.Count; n++)
			{
				Add(new ItemTemplatePair (itemTemplate, itemsSource[n]));
			}

			notifyCollectionChanged.CollectionChanged += InnerCollectionChanged;
		}

		void InnerCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			switch (args.Action)
			{
				case NotifyCollectionChangedAction.Add:
					Add(args);
					break;
				case NotifyCollectionChangedAction.Move:
					break;
				case NotifyCollectionChangedAction.Remove:
					break;
				case NotifyCollectionChangedAction.Replace:
					break;
				case NotifyCollectionChangedAction.Reset:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		void Add(NotifyCollectionChangedEventArgs args)
		{
			// TODO hartez 2018-07-31 01:47 PM Figure out what scenarios might cause a NewStartingIndex of -1
			// I'm worried that the IndexOf lookup could cause problems when the list has duplicate items
			var startIndex = args.NewStartingIndex > -1 ? args.NewStartingIndex : _innerSource.IndexOf(args.NewItems[0]);

			for (int n = args.NewItems.Count - 1; n >= 0; n--)
			{
				Insert(startIndex, new ItemTemplatePair(_itemTemplate, args.NewItems[n]));
			}
		}
	}

	internal class ItemTemplateEnumerator : IEnumerable, IEnumerator
	{
		readonly DataTemplate _formsDataTemplate;
		readonly IEnumerator _innerEnumerator;

		public ItemTemplateEnumerator(IEnumerable itemsSource, DataTemplate formsDataTemplate)
		{
			_formsDataTemplate = formsDataTemplate;
			_innerEnumerator = itemsSource.GetEnumerator();
		}
		public IEnumerator GetEnumerator()
		{
			return this;
		}

		public bool MoveNext()
		{
			var moveNext = _innerEnumerator.MoveNext();
			
			if (moveNext)
			{
				Current = new ItemTemplatePair(_formsDataTemplate, _innerEnumerator.Current);
			}

			return moveNext;
		}

		public void Reset()
		{
			_innerEnumerator.Reset();
		}

		public object Current { get; private set; }
	}
}