﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using GridViewSamples.Controls;

namespace Catrobat.IDE.Store.Controls.ListView
{
    public class BindableGridViewEx : GridView
    {

        public new IList ItemsSource
        {
            get { return (IList)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value);
                base.ItemsSource = value;
            }
        }

        public new static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            "ItemsSource", typeof(IList), typeof(BindableGridViewEx), new PropertyMetadata(null, ItemsSourceChanged));

        private static void ItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((BindableGridViewEx)d).ItemsSource = (IList) e.NewValue;
        }



        public IList BindableSelectedItems
        {
            get { return (IList)GetValue(BindableSelectedItemsProperty); }
            set { SetValue(BindableSelectedItemsProperty, value); }
        }

        public static readonly DependencyProperty BindableSelectedItemsProperty = DependencyProperty.Register(
            "BindableSelectedItems", typeof(IList), typeof(BindableGridViewEx),
            new PropertyMetadata(default(ObservableCollection<object>), SelectedItemsChanged));

        private static void SelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var oldBindableSelectedItems = ((INotifyCollectionChanged)e.OldValue);
            var newBindableSelectedItems = ((INotifyCollectionChanged)e.NewValue);

            if (oldBindableSelectedItems != null)
                oldBindableSelectedItems.CollectionChanged -= ((BindableGridViewEx)d).SelectedItemsOnCollectionChanged;


            if (newBindableSelectedItems != null)
                newBindableSelectedItems.CollectionChanged += ((BindableGridViewEx)d).SelectedItemsOnCollectionChanged;
        }




        public BindableGridViewEx()
        {
            
        }



        private void SelectedItemsOnCollectionChanged(object sender,
            NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (BindableSelectedItems == null)
                return;

            var list = BindableSelectedItems as IList;

            var itemsToRemove = new List<object>();

            foreach (var item in BindableSelectedItems)
            {
                if (!list.Contains(item))
                    itemsToRemove.Add(item);
            }

            foreach (var item in itemsToRemove)
            {
                SelectionChanged -= OnSelectionChanged;
                BindableSelectedItems.Remove(item);
                SelectionChanged += OnSelectionChanged;
            }

            foreach (var item in list)
            {
                if (!BindableSelectedItems.Contains(item))
                {
                    var index = list.IndexOf(item);
                    BindableSelectedItems.Insert(index, item);
                }
            }
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            if (BindableSelectedItems == null)
                return;

            var list = BindableSelectedItems as IList;

            var itemsToRemove = new List<object>();

            foreach (var item in list)
            {
                if (!BindableSelectedItems.Contains(item))
                    itemsToRemove.Add(item);
            }

            foreach (var item in itemsToRemove)
            {
                list.Remove(item);
            }

            foreach (var item in BindableSelectedItems)
            {
                if (!list.Contains(item))
                {
                    var index = BindableSelectedItems.IndexOf(item);
                    list.Insert(index, item);
                }
            }
        }

    }
}
