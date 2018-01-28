﻿using Prism.Mvvm;
using Prism.Navigation;
using Reactive.Bindings;
using FUNCalendar.Models;
using FUNCalendar.Services;
using System;
using System.Reactive.Disposables;
using System.Collections.Generic;
using Reactive.Bindings.Extensions;
using Prism.Services;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using System.Reactive.Threading;
using System.Reactive;
using System.Reactive.Linq;

namespace FUNCalendar.ViewModels
{
    public class CalendarPageViewModel : BindableBase, INavigationAware, IDisposable
    {
        /* 全てのリストを初期化 */
        private static ReactiveProperty<bool> canInitialize = new ReactiveProperty<bool>();
        private IStorageService _storageService;
        private ILoadingMessage _loadingMessage;

        private IWishList _wishList;
        private IToDoList _todoList;
        private IHouseholdAccounts _householdAccounts;
        private ICalendar _calendar;
        private IPageDialogService _pageDialogService;
        private INavigationService _navigationService;

        /* 表示用データ */
        private string currentMonth;
        public string CurrentMonth
        {
            get { return this.currentMonth; }
            set { this.SetProperty(ref this.currentMonth, value); }
        }
        private string currentYear;
        public string CurrentYear
        {
            get { return this.currentYear; }
            set { this.SetProperty(ref this.currentYear, value); }
        }
        private string currentDate;
        public string CurrentDate
        {
            get { return this.currentDate; }
            set { this.SetProperty(ref this.currentDate, value); }
        }
        private string calendarYear;
        public string CalendarYear
        {
            get { return this.calendarYear; }
            set { this.SetProperty(ref this.calendarYear, value); }
        }
        private string calendarMonth;
        public string CalendarMonth
        {
            get { return this.calendarMonth; }
            set { this.SetProperty(ref this.calendarMonth, value); }
        }

        public ReactiveProperty<bool> IsEndRefreshing { get; private set; } = new ReactiveProperty<bool>();

        public ReactiveCommand TapCommand { get; private set; }
        public ReactiveCommand BackPrevMonth { get; private set; }
        public ReactiveCommand GoNextMonth { get; private set; }

        /* 表示用リスト */
        public ReadOnlyReactiveCollection<VMDate> DisplayCalendar { get; private set; }
        /* 画面遷移用 */
        public AsyncReactiveCommand NavigationRegisterPageCommand { get; private set; }
        /* 購読解除用 */
        private CompositeDisposable Disposable { get; } = new CompositeDisposable();

        public CalendarPageViewModel(ILoadingMessage loadingMessage, IWishList wishList, IToDoList todoList, IHouseholdAccounts householdAccounts, IStorageService storageService, ICalendar calendar, INavigationService navigationService, IPageDialogService pageDialogService)
        {
            this._storageService = storageService;
            this._loadingMessage = loadingMessage;
            this._wishList = wishList;
            this._todoList = todoList;
            this._householdAccounts = householdAccounts;
            
            canInitialize.Subscribe(async _ =>
            {
                await _storageService.InitializeAsync(this._wishList, this._todoList, this._householdAccounts);
                await _storageService.ReadFile();

            });

            this._calendar = calendar;
            _calendar.SetHasList(_wishList);

            this._pageDialogService = pageDialogService;
            this._navigationService = navigationService;
            NavigationRegisterPageCommand = new AsyncReactiveCommand();

            TapCommand = new ReactiveCommand();
            BackPrevMonth = new ReactiveCommand();
            GoNextMonth = new ReactiveCommand();

            CalendarYear = string.Format("{0}年", _calendar.CurrentYear.ToString());
            CalendarMonth = string.Format("{0}月", _calendar.CurrentMonth.ToString());
            CurrentYear = string.Format("{0}年", DateTime.Now.ToString("yyyy"));
            CurrentMonth = string.Format("{0}月", DateTime.Now.ToString("%M"));
            CurrentDate = string.Format("{0}日", DateTime.Now.ToString("%d"));

            DisplayCalendar = _calendar.ListedAMonthDateData.ToReadOnlyReactiveCollection(x => new VMDate(x)).AddTo(Disposable);
            IsEndRefreshing.Value = true;
            DisplayCalendar.ObserveAddChanged().Buffer(42).Subscribe(_ =>
            {
                IsEndRefreshing.Value = true;
                _loadingMessage.Hide();
            });

            TapCommand.Subscribe(async (obj) =>
            {
                _calendar.SetDisplayDate(VMDate.ToDate(obj as VMDate));
                await _navigationService.NavigateAsync($"/RootPage/NavigationPage/CalendarDetailPage");
            });

            BackPrevMonth.Subscribe(() =>
            {
                if (!IsEndRefreshing.Value) return;
                IsEndRefreshing.Value = false;
                _loadingMessage.Show("読み込み中");
                _calendar.BackPrevMonth();
                _calendar.SetHasList(_wishList);
                CalendarYear = string.Format("{0}年", _calendar.CurrentYear.ToString());
                CalendarMonth = string.Format("{0}月", _calendar.CurrentMonth.ToString());
            });

            GoNextMonth.Subscribe(() =>
            {
                if (!IsEndRefreshing.Value) return;
                IsEndRefreshing.Value = false;
                _loadingMessage.Show("読み込み中");
                _calendar.GoNextMonth();
                _calendar.SetHasList(_wishList);
                CalendarYear = string.Format("{0}年", _calendar.CurrentYear.ToString());
                CalendarMonth = string.Format("{0}月", _calendar.CurrentMonth.ToString());
            });

            NavigationRegisterPageCommand.Subscribe(async () =>
            {
                var result = await _pageDialogService.DisplayActionSheetAsync("登録するアイテムの種類を選択", "キャンセル", "", "ToDo", "WishList", "家計簿");
                switch (result)
                {
                    case "ToDo":
                        await this._navigationService.NavigateAsync($"/NavigationPage/ToDoListRegisterPage");
                        break;
                    case "WishList":
                        await this._navigationService.NavigateAsync($"/NavigationPage/WishListRegisterPage");
                        break;
                    case "家計簿":
                        await this._navigationService.NavigateAsync($"/NavigationPage/HouseholdAccountsListRegisterPage");
                        break;
                }
            });
        }

        public void OnNavigatedFrom(NavigationParameters parameters)
        {

        }

        public void OnNavigatedTo(NavigationParameters parameters)
        {
            canInitialize.Value = true;
        }

        public void OnNavigatingTo(NavigationParameters parameters)
        {

        }

        public void Dispose()
        {
            Disposable.Dispose();
        }
    }
}