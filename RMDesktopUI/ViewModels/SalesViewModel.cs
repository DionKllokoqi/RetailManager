﻿using AutoMapper;
using Caliburn.Micro;
using RMDesktopUI.Library.Api;
using RMDesktopUI.Library.Helpers;
using RMDesktopUI.Library.Models;
using RMDesktopUI.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RMDesktopUI.ViewModels
{
    public class SalesViewModel : Screen
    {
        #region Properties

        #region Private

        private IProductEndPoint _productEndPoint;
        private IConfigHelper _configHelper;
        private ISaleEndPoint _saleEndPoint;
        private IMapper _mapper;
        private readonly StatusInfoViewModel _status;
        private BindingList<ProductDisplayModel> _products;
        private int _itemQuantity = 1;
        private BindingList<CartItemDisplayModel> _cart = new BindingList<CartItemDisplayModel>();
        private ProductDisplayModel _selectedProduct;
        private CartItemDisplayModel _selectedCartItem;
        private IWindowManager _window;

        #endregion

        #region Public

        public BindingList<ProductDisplayModel> Products
        {
            get { return _products; }
            set
            {
                _products = value;
                // triggered only when overwritting entire products list
                NotifyOfPropertyChange(() => Products);
            }
        }
        public BindingList<CartItemDisplayModel> Cart
        {
            get { return _cart; }
            set
            {
                _cart = value;
                NotifyOfPropertyChange(() => Cart);
            }
        }
        public ProductDisplayModel SelectedProduct
        {
            get { return _selectedProduct; }
            set
            {
                _selectedProduct = value;
                NotifyOfPropertyChange(() => SelectedProduct);
                NotifyOfPropertyChange(() => CanAddToCart);
            }
        }
        public CartItemDisplayModel SelectedCartItem
        {
            get { return _selectedCartItem; }
            set
            {
                _selectedCartItem = value;
                NotifyOfPropertyChange(() => SelectedCartItem);
                NotifyOfPropertyChange(() => CanRemoveFromCart);
            }
        }
        /// <summary>
        /// Quantity of a specific product that the user wants to add to cart.
        /// Property is bound to Item Quantity TextBox in View.
        /// </summary>
        public int ItemQuantity
        {
            get { return _itemQuantity; }
            set
            {
                _itemQuantity = value;
                NotifyOfPropertyChange(() => ItemQuantity);
                NotifyOfPropertyChange(() => CanAddToCart);
            }
        }
        public bool CanAddToCart
        {
            get
            {
                bool output = false;

                // Make sure something is selected
                // Make sure we have an item quantity
                if (ItemQuantity > 0 && SelectedProduct?.QuantityInStock >= ItemQuantity)
                {
                    output = true;
                }

                return output;
            }
        }
        public bool CanRemoveFromCart
        {
            get
            {
                bool output = false;

                // Make sure something is selected
                if (SelectedCartItem != null && SelectedCartItem.QuantityInCart > 0)
                {
                    output = true;
                }

                return output;
            }
        }
        public bool CanCheckOut
        {
            get
            {
                bool output = false;

                // Make sure something is selected
                if (Cart?.Count > 0)
                {
                    output = true;
                }

                return output;
            }
        }
        public string SubTotal
        {
            get
            {
                var subTotal = CalculateSubTotal();
                return subTotal.ToString("C");
            }
        }
        public string Total
        {
            get
            {
                decimal total = CalculateSubTotal() + CalculateTax();
                return total.ToString("C");
            }
        }
        public string Tax
        {
            get
            {
                var taxAmount = CalculateTax();
                // this displays only two decimal points of the value as a currency
                return taxAmount.ToString("C");
            }
        }

        #endregion

        #endregion

        #region CTOR & Init

        public SalesViewModel(IProductEndPoint productEndPoint, IConfigHelper configHelper, 
            ISaleEndPoint saleEndPoint, IMapper mapper, StatusInfoViewModel status, IWindowManager window)
        {
            _productEndPoint = productEndPoint;
            _saleEndPoint = saleEndPoint;
            _configHelper = configHelper;
            _mapper = mapper;
            _status = status;
            _window = window;
        }

        #endregion

        #region Methods

        protected override async void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            try
            {
                await LoadProducts();
            }
            catch (Exception ex)
            {
                dynamic settings = new ExpandoObject();
                settings.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                settings.ResizeMode = ResizeMode.NoResize;
                settings.Title = "System Error";

                if (string.Equals(ex.Message, "Unauthorized"))
                {
                    _status.UpdateMessage("Unaothorized Access",
                    "You do not have permission to interact with the Sales Form.");
                    _window.ShowDialog(_status, null, settings);
                }
                else
                {
                    _status.UpdateMessage("Fatal Exception", ex.Message);
                    _window.ShowDialog(_status, null, settings);
                }

                TryClose();
            }
        }

        private async Task LoadProducts()
        {
            var productList = await _productEndPoint.GetAll();
            var products = _mapper.Map<List<ProductDisplayModel>>(productList);
            Products = new BindingList<ProductDisplayModel>(products);
        }

        public void AddToCart()
        {
            CartItemDisplayModel existingItem = Cart.FirstOrDefault(x => x.Product == SelectedProduct);

            if (existingItem != null)
            {
                existingItem.QuantityInCart += ItemQuantity;
                //Cart.ResetBindings();
            }
            else
            {
                CartItemDisplayModel item = new CartItemDisplayModel
                {
                    Product = SelectedProduct,
                    QuantityInCart = ItemQuantity
                };
                Cart.Add(item);
            }

            SelectedProduct.QuantityInStock -= ItemQuantity;
            ItemQuantity = 1;
            NotifyOfPropertyChange(() => SubTotal);
            NotifyOfPropertyChange(() => Tax);
            NotifyOfPropertyChange(() => Total);
            NotifyOfPropertyChange(() => CanCheckOut);
        }

        public void RemoveFromCart()
        {
            SelectedCartItem.Product.QuantityInStock += 1;

            if (SelectedCartItem.QuantityInCart > 1)
            {
                SelectedCartItem.QuantityInCart -= 1;
            }
            else
            {
                Cart.Remove(SelectedCartItem);
            }

            NotifyOfPropertyChange(() => SubTotal);
            NotifyOfPropertyChange(() => Tax);
            NotifyOfPropertyChange(() => Total);
            NotifyOfPropertyChange(() => CanCheckOut);
            NotifyOfPropertyChange(() => CanAddToCart);
        }

        public async Task CheckOut()
        {
            // Create a sale model
            SaleModel sale = new SaleModel();

            foreach (var item in Cart)
            {
                sale.SaleDetails.Add(new SaleDetailModel
                {
                    ProductId = item.Product.Id,
                    Quantity = item.QuantityInCart
                });
            }

            // Post to the API
            await _saleEndPoint.PostSale(sale);

            await ResetSalesViewModel();
        }

        private decimal CalculateSubTotal()
        {
            decimal subTotal = 0;

            foreach (var item in Cart)
            {
                subTotal += item.Product.RetailPrice * item.QuantityInCart;
            }

            return subTotal;
        }

        private decimal CalculateTax()
        {
            decimal taxAmount = 0;
            decimal taxRate = _configHelper.GetTaxRate();

            taxAmount = Cart
                .Where(x => x.Product.IsTaxable)
                .Sum(x => x.Product.RetailPrice * x.Product.QuantityInStock * taxRate);

            //foreach (var item in Cart)
            //{
            //    if (item.Product.IsTaxable)
            //    {
            //        taxAmount += item.Product.RetailPrice * item.QuantityInCart * taxRate;
            //    }
            //}

            return taxAmount;
        } 

        private async Task ResetSalesViewModel()
        {
            Cart = new BindingList<CartItemDisplayModel>();
            await LoadProducts();

            NotifyOfPropertyChange(() => SubTotal);
            NotifyOfPropertyChange(() => Tax);
            NotifyOfPropertyChange(() => Total);
            NotifyOfPropertyChange(() => CanCheckOut);
        }

        #endregion
    }
}
