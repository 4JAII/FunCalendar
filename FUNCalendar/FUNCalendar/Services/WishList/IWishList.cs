﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FUNCalendar.Models;

namespace FUNCalendar.Services
{
    public interface IWishList:INotifyPropertyChanged
    {
        ExtendedObservableCollection<WishItem> SortedWishList { get; }
        WishItem DisplayWishItem { get; set; }
        ExtendedObservableCollection<WishItem> WishListForCalendar { get; }
        bool DateWithWishList { get; }
        bool IsAscending { get; set; }
        void SortByID();
        void SortByName();
        void SortByPrice();
        void SortByDate();
        void Reverse();
        void AddWishItem(WishItem wishItem);
        void UpdateList(List<WishItem> list);
        void SetDisplayWishItem(WishItem wishItem);
        void SetDisplayWishItem(int wishID);
        void Remove(WishItem wishItem);
        void EditWishItem(WishItem deleteWishItem,WishItem addWishItem);
        void SetWishListForCalendar(DateTime date);
        void ClearWishListForCalendar();
        void SetDateWithWishList(DateTime date);
    }
}
