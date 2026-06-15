using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TachinomiOrderSystem.Database;
using TachinomiOrderSystem.Models;
using TachinomiOrderSystem.Services;

namespace TachinomiOrderSystem.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseManager _dbManager;
        private readonly MenuService _menuService;

        private MenuItem? _selectedMenuItem;
        private int _orderQuantity = 1;
        private int _grandTotal;

        // Visibility Flags for Dialogs and Panels
        private bool _isOrderConfirmVisible;
        private bool _isCheckoutVisible;
        private bool _isCheckoutCompleteVisible;
        private bool _isDatabaseWarningVisible;
        private bool _isHistoryVisible;
        private string _databaseWarningMessage = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        // Collections
        public List<MenuItem> MenuItems { get; private set; }
        public ObservableCollection<OrderItem> OrderedHistory { get; } = new();
        public ObservableCollection<TachinomiOrderHistoryDto> CheckoutHistory { get; } = new();

        // Properties
        public MenuItem? SelectedMenuItem
        {
            get => _selectedMenuItem;
            set { _selectedMenuItem = value; OnPropertyChanged(); }
        }

        public int OrderQuantity
        {
            get => _orderQuantity;
            set
            {
                if (_orderQuantity != value)
                {
                    _orderQuantity = Math.Clamp(value, 1, 10);
                    OnPropertyChanged();
                }
            }
        }

        public int GrandTotal
        {
            get => _grandTotal;
            set { _grandTotal = value; OnPropertyChanged(); }
        }

        public bool IsOrderConfirmVisible
        {
            get => _isOrderConfirmVisible;
            set { _isOrderConfirmVisible = value; OnPropertyChanged(); }
        }

        public bool IsCheckoutVisible
        {
            get => _isCheckoutVisible;
            set { _isCheckoutVisible = value; OnPropertyChanged(); }
        }

        public bool IsCheckoutCompleteVisible
        {
            get => _isCheckoutCompleteVisible;
            set { _isCheckoutCompleteVisible = value; OnPropertyChanged(); }
        }

        public bool IsDatabaseWarningVisible
        {
            get => _isDatabaseWarningVisible;
            set { _isDatabaseWarningVisible = value; OnPropertyChanged(); }
        }

        public bool IsHistoryVisible
        {
            get => _isHistoryVisible;
            set { _isHistoryVisible = value; OnPropertyChanged(); }
        }

        public string DatabaseWarningMessage
        {
            get => _databaseWarningMessage;
            set { _databaseWarningMessage = value; OnPropertyChanged(); }
        }

        public bool IsSqlAvailable => _dbManager.IsSqlAvailable;

        // Commands
        public ICommand SelectMenuItemCommand { get; private set; } = null!;
        public ICommand IncrementQuantityCommand { get; private set; } = null!;
        public ICommand DecrementQuantityCommand { get; private set; } = null!;
        public ICommand ConfirmOrderCommand { get; private set; } = null!;
        public ICommand CancelOrderConfirmCommand { get; private set; } = null!;
        public ICommand ShowCheckoutCommand { get; private set; } = null!;
        public ICommand ConfirmCheckoutCommand { get; private set; } = null!;
        public ICommand CloseCheckoutCommand { get; private set; } = null!;
        public ICommand CloseDatabaseWarningCommand { get; private set; } = null!;
        public ICommand ResetSessionCommand { get; private set; } = null!;
        
        // History Commands
        public ICommand ToggleHistoryCommand { get; private set; } = null!;
        public ICommand ClearHistoryCommand { get; private set; } = null!;

        public MainViewModel()
        {
            _dbManager = new DatabaseManager();
            _menuService = new MenuService();
            MenuItems = new List<MenuItem>();

            InitializeCommands();
            InitializeDb();
        }

        private void InitializeCommands()
        {
            SelectMenuItemCommand = new RelayCommand(p => ExecuteSelectMenuItem(p));
            IncrementQuantityCommand = new RelayCommand(_ => OrderQuantity++);
            DecrementQuantityCommand = new RelayCommand(_ => OrderQuantity--);
            ConfirmOrderCommand = new RelayCommand(_ => ExecuteConfirmOrder());
            CancelOrderConfirmCommand = new RelayCommand(_ => IsOrderConfirmVisible = false);
            ShowCheckoutCommand = new RelayCommand(_ => IsShowCheckout());
            ConfirmCheckoutCommand = new RelayCommand(_ => ExecuteConfirmCheckout());
            CloseCheckoutCommand = new RelayCommand(_ => IsCheckoutVisible = false);
            CloseDatabaseWarningCommand = new RelayCommand(_ => IsDatabaseWarningVisible = false);
            ResetSessionCommand = new RelayCommand(_ => ResetSession());
            ToggleHistoryCommand = new RelayCommand(_ => ExecuteToggleHistory());
            ClearHistoryCommand = new RelayCommand(_ => ExecuteClearHistory());
        }

        private void InitializeDb()
        {
            bool success = _dbManager.InitializeDatabase();
            if (!success)
            {
                DatabaseWarningMessage = "SQL Server LocalDB への接続に失敗しました。\nアプリケーションは一時的に「オフライン・デモモード」で起動します。";
                IsDatabaseWarningVisible = true;
                MenuItems = _menuService.GetMenu(); // static fallback
            }
            else
            {
                MenuItems = _dbManager.GetMenuItems();
                LoadHistoryFromDb();
            }
            OnPropertyChanged(nameof(MenuItems));
        }

        private void LoadHistoryFromDb()
        {
            CheckoutHistory.Clear();
            if (IsSqlAvailable)
            {
                var history = _dbManager.GetCheckoutHistory();
                foreach (var item in history)
                {
                    CheckoutHistory.Add(item);
                }
            }
        }

        private void ExecuteSelectMenuItem(object? parameter)
        {
            if (parameter is MenuItem item)
            {
                SelectedMenuItem = item;
                OrderQuantity = 1;
                IsOrderConfirmVisible = true;
            }
        }

        private void ExecuteConfirmOrder()
        {
            if (SelectedMenuItem == null) return;

            // Check if item is already in OrderedHistory, increment quantity if so
            var existingOrder = OrderedHistory.FirstOrDefault(o => o.Item.Id == SelectedMenuItem.Id);
            if (existingOrder != null)
            {
                existingOrder.Quantity += OrderQuantity;
            }
            else
            {
                var newOrder = new OrderItem(SelectedMenuItem, OrderQuantity);
                OrderedHistory.Insert(0, newOrder);
            }

            GrandTotal += SelectedMenuItem.Price * OrderQuantity;
            IsOrderConfirmVisible = false;
        }

        private void IsShowCheckout()
        {
            if (OrderedHistory.Count == 0) return; // Prevent empty checkout
            IsCheckoutVisible = true;
        }

        private void ExecuteConfirmCheckout()
        {
            IsCheckoutVisible = false;

            if (IsSqlAvailable && OrderedHistory.Count > 0)
            {
                var currentOrders = OrderedHistory.ToList();
                var currentTotal = GrandTotal;

                System.Threading.Tasks.Task.Run(() =>
                {
                    _dbManager.SaveTachinomiOrder(currentTotal, currentOrders, out _);
                    // Reload checkout history in UI
                    App.Current?.Dispatcher.Invoke(() =>
                    {
                        LoadHistoryFromDb();
                    });
                });
            }

            IsCheckoutCompleteVisible = true;
        }

        public void ResetSession()
        {
            OrderedHistory.Clear();
            GrandTotal = 0;
            IsCheckoutCompleteVisible = false;
        }

        private void ExecuteToggleHistory()
        {
            IsHistoryVisible = !IsHistoryVisible;
            if (IsHistoryVisible)
            {
                LoadHistoryFromDb();
            }
        }

        private void ExecuteClearHistory()
        {
            if (IsSqlAvailable)
            {
                System.Threading.Tasks.Task.Run(() =>
                {
                    _dbManager.ClearAllHistory();
                    App.Current?.Dispatcher.Invoke(() =>
                    {
                        CheckoutHistory.Clear();
                    });
                });
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
