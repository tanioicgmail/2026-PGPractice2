using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using KURAOrderSystem.Database;
using KURAOrderSystem.Models;
using KURAOrderSystem.Services;

namespace KURAOrderSystem.ViewModels
{
    public partial class MainViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseManager _dbManager;
        private readonly IMenuService _menuService;

        private string _selectedCategory = "にぎり";
        private SushiItem? _selectedSushi;
        private int _orderQuantity = 1;

        // Visibility Flags for Popups
        private bool _isOrderConfirmVisible;
        private bool _isStaffCallVisible;
        private bool _isCheckoutVisible;
        private bool _isCheckoutCompleteVisible;
        private bool _isDatabaseWarningVisible;
        private string _databaseWarningMessage = string.Empty;

        // NEW: Bikkura Pon settings & Delivery alerts
        private bool _isBikkuraPonEnabled = true; // Default to ON (participating)
        private bool _isDeliveryNotificationVisible;
        private string _arrivingSushiName = string.Empty;
        private int _arrivingSushiQuantity = 1;

        // Ordering & Plate Stats
        private int _plateCount;
        private int _grandTotal;

        public event PropertyChangedEventHandler? PropertyChanged;

        // Collections
        public ObservableCollection<SushiItem> SushiItems { get; }
        public ObservableCollection<SushiItem> FilteredSushiItems { get; } = new();
        public ObservableCollection<OrderItem> OrderedHistory { get; } = new();

        public List<string> Categories { get; } = new()
        {
            "にぎり",
            "軍艦・細巻",
            "サイドメニュー",
            "デザート・ドリンク"
        };

        // Properties
        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (_selectedCategory != value)
                {
                    _selectedCategory = value;
                    OnPropertyChanged();
                    FilterItems();
                }
            }
        }

        public SushiItem? SelectedSushi
        {
            get => _selectedSushi;
            set
            {
                _selectedSushi = value;
                OnPropertyChanged();
            }
        }

        public int OrderQuantity
        {
            get => _orderQuantity;
            set
            {
                if (_orderQuantity != value)
                {
                    _orderQuantity = Math.Clamp(value, 1, 4);
                    OnPropertyChanged();
                }
            }
        }

        public bool IsOrderConfirmVisible
        {
            get => _isOrderConfirmVisible;
            set { _isOrderConfirmVisible = value; OnPropertyChanged(); }
        }

        public bool IsStaffCallVisible
        {
            get => _isStaffCallVisible;
            set { _isStaffCallVisible = value; OnPropertyChanged(); }
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

        public string DatabaseWarningMessage
        {
            get => _databaseWarningMessage;
            set { _databaseWarningMessage = value; OnPropertyChanged(); }
        }

        // NEW: Bikkura Pon Settings
        public bool IsBikkuraPonEnabled
        {
            get => _isBikkuraPonEnabled;
            set
            {
                if (_isBikkuraPonEnabled != value)
                {
                    _isBikkuraPonEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        // NEW: Delivery Notification
        public bool IsDeliveryNotificationVisible
        {
            get => _isDeliveryNotificationVisible;
            set { _isDeliveryNotificationVisible = value; OnPropertyChanged(); }
        }

        public string ArrivingSushiName
        {
            get => _arrivingSushiName;
            set { _arrivingSushiName = value; OnPropertyChanged(); }
        }

        public int ArrivingSushiQuantity
        {
            get => _arrivingSushiQuantity;
            set { _arrivingSushiQuantity = value; OnPropertyChanged(); }
        }

        public int PlateCount
        {
            get => _plateCount;
            set
            {
                _plateCount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BikkuraPonProgress));
                OnPropertyChanged(nameof(BikkuraPonMilestone));
            }
        }

        public int BikkuraPonProgress => PlateCount % 5;
        public int BikkuraPonMilestone => 5 - BikkuraPonProgress;

        public int GrandTotal
        {
            get => _grandTotal;
            set { _grandTotal = value; OnPropertyChanged(); }
        }

        public string TableNumber => "テーブル 15";

        public bool IsSqlAvailable => _dbManager.IsSqlAvailable;

        public MainViewModel()
        {
            _dbManager = new DatabaseManager();
            _menuService = new MenuService();

            // Load Menu Items from Service
            SushiItems = new ObservableCollection<SushiItem>(_menuService.GetMenu());

            // Initialize Commands (defined in MainViewModel.Commands.cs)
            InitializeCommands();

            // Filter items initially
            FilterItems();

            // Initialize database on startup (defined in MainViewModel.Operations.cs)
            InitializeDb();
        }

        private void FilterItems()
        {
            FilteredSushiItems.Clear();
            foreach (var item in SushiItems.Where(i => i.Category == SelectedCategory))
            {
                FilteredSushiItems.Add(item);
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
