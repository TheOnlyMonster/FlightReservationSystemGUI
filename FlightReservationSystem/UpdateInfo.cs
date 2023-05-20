﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic.ApplicationServices;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace FlightReservationSystem
{
    public partial class UpdateInfo : MainMenu
    {
        public UpdateInfo()
        {
            InitializeComponent();
        }

        private void UpdateInfo_Load(object sender, EventArgs e)
        {
            this.FirstNameTextBox.Text = Customer.Instance.fname;
            this.CityTextBox.Text = Customer.Instance.City;
            this.CountryTextBox.Text = Customer.Instance.Country;
            this.LastNameTextBox.Text = Customer.Instance.lname;
            this.ExpirayDateTextBox.Text = Customer.Instance.ExpirayDate;
            this.CVVTextBox.Text = Customer.Instance.Cvv.ToString();
            this.CardNumberTextBox.Text = Customer.Instance.CardNum.ToString();


        }
        private string GetExistingValueFromCustomer(string columnName, string customerId, SqlConnection connection)
        {
            string query = $"SELECT {columnName} FROM CustomerTable WHERE CustomerID = @CustomerID";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@CustomerID", customerId);
                return command.ExecuteScalar()?.ToString();
            }
        }
        private string GetExistingValueFromUser(string columnName, string userId, SqlConnection connection)
        {
            string query = $"SELECT {columnName} FROM UserTable WHERE UserID = @userID";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@userID", userId);
                return command.ExecuteScalar()?.ToString();
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            using (SqlConnection connection = new SqlConnection(databaseConnection))
            {
                connection.Open();

                string userQuery = "UPDATE UserTable SET";
                string customerQuery = "UPDATE CustomerTable SET";
                List<string> userParameters = new List<string>();
                List<string> customerParameters = new List<string>();

                if (!string.Equals(FirstNameTextBox.Text, GetExistingValueFromUser("Fname", Customer.Instance.Id, connection)))
                {
                    userParameters.Add("Fname = @Fname");
                    Customer.Instance.fname = this.FirstNameTextBox.Text;
                }

                if (!string.Equals(LastNameTextBox.Text, GetExistingValueFromUser("Lname", Customer.Instance.Id, connection)))
                {
                    userParameters.Add("Lname = @Lname");
                    Customer.Instance.lname = this.LastNameTextBox.Text;


                }

                if (!string.Equals(PasswordTextBox.Text, GetExistingValueFromUser("Password", Customer.Instance.Id, connection))&&!string.IsNullOrEmpty(PasswordTextBox.Text))
                {
                    userParameters.Add("Password = @Password");
                    Customer.Instance.Password = this.PasswordTextBox.Text;
                }

                if (userParameters.Count > 0)
                {
                    userQuery += " " + string.Join(", ", userParameters);
                    userQuery += " WHERE UserID = @UserID";
                    using (SqlCommand userCommand = new SqlCommand(userQuery, connection))
                    {
                        userCommand.Parameters.AddWithValue("@UserID", Customer.Instance.Id);
                        userCommand.Parameters.AddWithValue("@Fname", FirstNameTextBox.Text);
                        userCommand.Parameters.AddWithValue("@Lname", LastNameTextBox.Text);
                        userCommand.Parameters.AddWithValue("@Password", PasswordTextBox.Text);
                        userCommand.ExecuteNonQuery();
                    }
                }

                if (!string.Equals(CityTextBox.Text, GetExistingValueFromCustomer("City", Customer.Instance.Id, connection)))
                {
                    customerParameters.Add("City = @City");
                    Customer.Instance.City = this.CityTextBox.Text;
                }

                if (!string.Equals(CountryTextBox.Text, GetExistingValueFromCustomer("Country", Customer.Instance.Id, connection)))
                {
                    customerParameters.Add("Country = @Country");
                    Customer.Instance.Country = this.CountryTextBox.Text;
                }

                if (!string.Equals(CardNumberTextBox.Text, GetExistingValueFromCustomer("CardNum", Customer.Instance.Id, connection)))
                {
                    customerParameters.Add("CardNum = @CardNum");
                    Customer.Instance.CardNum = this.CardNumberTextBox.Text;
                }

                if (!string.Equals(CVVTextBox.Text, GetExistingValueFromCustomer("CVV", Customer.Instance.Id, connection)))
                {
                    customerParameters.Add("CVV = @CVV");
                    Customer.Instance.Cvv = this.CVVTextBox.Text;
                }

                if (!string.Equals(ExpirayDateTextBox.Text, GetExistingValueFromCustomer("ExpiryDate", Customer.Instance.Id, connection)))
                {
                    customerParameters.Add("ExpiryDate = @ExpiryDate");
                    Customer.Instance.ExpirayDate = this.ExpirayDateTextBox.Text;


                }

                if (customerParameters.Count > 0)
                {
                    customerQuery += " " + string.Join(", ", customerParameters);
                    customerQuery += " WHERE CustomerID = @CustomerID";
                    using (SqlCommand customerCommand = new SqlCommand(customerQuery, connection))
                    {
                        customerCommand.Parameters.AddWithValue("@CustomerID", Customer.Instance.Id);
                        customerCommand.Parameters.AddWithValue("@City", CityTextBox.Text);
                        customerCommand.Parameters.AddWithValue("@Country", CountryTextBox.Text);
                        customerCommand.Parameters.AddWithValue("@CardNum", CardNumberTextBox.Text);
                        customerCommand.Parameters.AddWithValue("@CVV", CVVTextBox.Text);
                        customerCommand.Parameters.AddWithValue("@ExpiryDate", ExpirayDateTextBox.Text);
                        customerCommand.ExecuteNonQuery();
                    }
                }

                connection.Close();
            }
        }
    }
}
