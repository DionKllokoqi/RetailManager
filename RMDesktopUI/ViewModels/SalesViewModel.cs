using Caliburn.Micro;
using RMDesktopUI.Library.Api;
using RMDesktopUI.Library.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RMDesktopUI.ViewModels
{
    public class SalesViewModel : Screen
    {
        private BindingList<ProductModel> _products;
        private int _itemQuantity = 1;
        private BindingList<CartItemModel> _cart = new BindingList<CartItemModel>();
        private IProductEndPoint _productEndPoint;
        private ProductModel _selectedProduct;

        public BindingList<ProductModel> Products
        {
            get { return _products; }
            set 
            {
                _products = value;
                // triggered only when overwritting entire products list
                NotifyOfPropertyChange(() => Products);
            }
        }
        public BindingList<CartItemModel> Cart
        {
            get { return _cart; }
            set 
            { 
                _cart = value;
                NotifyOfPropertyChange(() => Cart);
            }
        }
        public ProductModel SelectedProduct
        {
            get { return _selectedProduct; }
            set 
            { 
                _selectedProduct = value;
                NotifyOfPropertyChange(() => SelectedProduct);
                NotifyOfPropertyChange(() => CanAddToCart);
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

                return output;
            }
        }
        public bool CanCheckOut
        {
            get
            {
                bool output = false;

                // Make sure something is selected

                return output;
            }
        }
        public string SubTotal
        {
            get
            {
                decimal subTotal = 0;
                foreach (var item in Cart)
                {
                    subTotal += item.Product.RetailPrice * item.QuantityInCart;
                }
                return subTotal.ToString("C");
            }
        }
        public string Total
        {
            get
            {
                // ToDo: Replace with calculation
                return "$0.00";
            }
        }
        public string Tax
        {
            get
            {
                // ToDo: Replace with calculation
                return "$0.00";
            }
        }

        public SalesViewModel(IProductEndPoint productEndPoint)
        {
            _productEndPoint = productEndPoint;
        }

        protected override async void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            await LoadProducts();
        }

        private async Task LoadProducts()
        {
            var productList = await _productEndPoint.GetAll();
            Products = new BindingList<ProductModel>(productList);
        }

        public void AddToCart()
        {
            CartItemModel existingItem = Cart.FirstOrDefault(x => x.Product == SelectedProduct);

            if (existingItem != null)
            {
                existingItem.QuantityInCart += ItemQuantity;
                Cart.ResetBindings();
            }
            else
            {
                CartItemModel item = new CartItemModel 
                {
                    Product = SelectedProduct, 
                    QuantityInCart = ItemQuantity 
                };
                Cart.Add(item);
            }

            SelectedProduct.QuantityInStock -= ItemQuantity;
            ItemQuantity = 1;
            NotifyOfPropertyChange(() => SubTotal);
        }

        public void RemoveFromCart()
        {
            NotifyOfPropertyChange(() => SubTotal);
        }

        public void CheckOut()
        {

        }
    }
}
