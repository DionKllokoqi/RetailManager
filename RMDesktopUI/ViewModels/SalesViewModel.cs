using Caliburn.Micro;
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
        private BindingList<string> _products;
        private int _itemQuantity;
        private BindingList<string> _cart;

        public BindingList<string> Products
        {
            get { return _products; }
            set 
            {
                _products = value;
                // triggered only when overwritting entire products list
                NotifyOfPropertyChange(() => Products);
            }
        }
        public BindingList<string> Cart
        {
            get { return _cart; }
            set 
            { 
                _cart = value;
                NotifyOfPropertyChange(() => Cart);
            }
        }
        public int ItemQuantity
        {
            get { return _itemQuantity; }
            set 
            {
                _itemQuantity = value;
                NotifyOfPropertyChange(() => ItemQuantity);
            }
        }
        public bool CanAddToCart
        {
            get
            {
                bool output = false;

                // Make sure something is selected
                // Make sure we have an item quantity


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
                // ToDo: Replace with calculation
                return "$0.00";
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

        public void AddToCart()
        {

        }

        public void RemoveFromCart()
        {

        }

        public void CheckOut()
        {

        }
    }
}
