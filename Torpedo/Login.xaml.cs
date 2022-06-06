using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using BusinessLogicLayer;

namespace Torpedo
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInputs())
            {
                using (var context = new BusinessLogicContext())
                {
                    var user = context.Login(txtUsername.Text, txtPassword.Text);
                    if (user != null)
                    {
                        GameFlowDirector.User = user;
                        GameFlowDirector.StepForward();
                        this.Close();
                    }
                    else
                    {
                        btnLoginValidation.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        private void btnRegistration_Click(object sender, RoutedEventArgs e)
        {
            btnLoginValidation.Visibility = Visibility.Collapsed;
            txtUsernameValidation.Visibility = Visibility.Collapsed;
            txtPasswordValidation.Visibility = Visibility.Collapsed;
            txtUsername.Text = "";
            txtPassword.Text = "";
            btnLogin.Visibility = Visibility.Collapsed;
            btnRegistration.Visibility = Visibility.Collapsed;
            btnRegister.Visibility = Visibility.Visible;
            btnBack.Visibility = Visibility.Visible;
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInputs())
            {
                using (var context = new BusinessLogicContext())
                {
                    var user = context.Register(new DataContract.UserData()
                    {
                        Password = txtPassword.Text,
                        UserName = txtUsername.Text
                    });
                    if (user != null)
                    {
                        btnLoginValidation.Visibility = Visibility.Collapsed;
                        txtUsernameValidation.Visibility = Visibility.Collapsed;
                        txtPasswordValidation.Visibility = Visibility.Collapsed;
                        btnRegisterValidation.Visibility = Visibility.Collapsed;
                        txtUsername.Text = "";
                        txtPassword.Text = "";
                        btnLogin.Visibility = Visibility.Visible;
                        btnRegistration.Visibility = Visibility.Visible;
                        btnRegister.Visibility = Visibility.Collapsed;
                        btnBack.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        btnRegisterValidation.Visibility = Visibility.Visible;
                    }
                }
            }
        }
        private bool ValidateInputs()
        {
            bool result = true;
            btnLoginValidation.Visibility = Visibility.Collapsed;
            txtUsernameValidation.Visibility = Visibility.Collapsed;
            txtPasswordValidation.Visibility = Visibility.Collapsed;
            btnRegisterValidation.Visibility = Visibility.Collapsed;
            if (String.IsNullOrEmpty(txtUsername.Text))
            {
                txtUsernameValidation.Visibility = Visibility.Visible;
                result = false;
            }
            if (String.IsNullOrEmpty(txtPassword.Text))
            {
                txtPasswordValidation.Visibility = Visibility.Visible;
                result = false;
            }
            return result;
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {

        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
        }
    }
}
