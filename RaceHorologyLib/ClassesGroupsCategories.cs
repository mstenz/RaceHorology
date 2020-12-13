﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RaceHorologyLib
{

  public class CategoryVM
  {
    public ObservableCollection<ParticipantCategory> Items { get; }

    CollectionViewSource _itemsWONewItem; //!< Just there to fill the comboboxes in the DataGrid for the classes, otherwise the "new items placeholder" will appear
    public System.ComponentModel.ICollectionView FilteredItems { get { return _itemsWONewItem.View; } }

    public CategoryVM()
    {
      Items = new ObservableCollection<ParticipantCategory>();

      _itemsWONewItem = new CollectionViewSource();
      _itemsWONewItem.Source = Items;
    }

    public void Assign(IList<ParticipantCategory> categories, bool delete)
    {
      if (delete)
        Items.Clear();
      Items.InsertRange(categories);
      Items.Sort(new StdComparer());
    }
  }


}
