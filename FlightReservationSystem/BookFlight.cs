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
using System.Globalization;

namespace FlightReservationSystem
{

    public partial class BookFlight : MainMenu
    {
        private Dictionary<string,double> FlightClasses = new Dictionary<string, double>();
        public BookFlight()
        {
            InitializeComponent();
        }

        private void bookFlightComboBox_Changed(object sender, EventArgs e)
        {
            string query = "SELECT FlightNo, deptDate, deptCountry, arrivalCountry, expectedArrival, AvailableSeats, Rank1Price, Rank2Price, Rank3Price FROM Flight " +
                    "WHERE deptCountry = @deptCountry AND arrivalCountry = @arrivalCountry AND CAST(deptDate AS DATE) = CAST(@deptDate AS DATE) AND AvailableSeats <> 0";
            if (this.deptCountriesComboBox.SelectedItem == null || this.arrivalCountriesComboBox.SelectedItem == null)
            {
                query = "SELECT FlightNo, deptDate, deptCountry, arrivalCountry, expectedArrival, AvailableSeats,Rank1Price, Rank2Price, Rank3Price FROM Flight where AvailableSeats <> 0";
            }

            flightDataGrid.Rows.Clear();
            DataTable dataTable = new DataTable();
            using (SqlConnection connection = new SqlConnection(databaseConnection))
            {
                connection.Open();

                SqlCommand command = new SqlCommand(query, connection);
                if (this.deptCountriesComboBox.SelectedItem != null && this.arrivalCountriesComboBox.SelectedItem != null)
                {
                    command.Parameters.AddWithValue("@deptCountry", this.deptCountriesComboBox.SelectedItem.ToString());
                    command.Parameters.AddWithValue("@arrivalCountry", this.arrivalCountriesComboBox.SelectedItem.ToString());
                    command.Parameters.AddWithValue("@deptDate", this.deptDateTimePicker.Value);
                }

                SqlDataAdapter adapter = new SqlDataAdapter(command);
                adapter.Fill(dataTable);
                connection.Close();
            }
            foreach (DataRow row in dataTable.Rows)
            {
                object[] rowData = row.ItemArray;
                flightDataGrid.Rows.Add(rowData);
            }
        }
        private void deptCountriesComboBox_OnDropDown(object sender, EventArgs e)
        {
            deptCountriesComboBox.Items.Clear();
            fillComboBox($"Select DISTINCT deptCountry from Flight where AvailableSeats <> 0", deptCountriesComboBox);
        }
        private void arrivalCountriesComboBox_OnDropDown(object sender, EventArgs e)
        {
            arrivalCountriesComboBox.Items.Clear();
            fillComboBox($"Select DISTINCT arrivalCountry from Flight where AvailableSeats <> 0", arrivalCountriesComboBox);
        }

        private void fillComboBox(string query, ComboBox comboBox)
        {
            using (SqlConnection connection = new SqlConnection(databaseConnection))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader sqlDataReader = command.ExecuteReader();
                while (sqlDataReader.Read())
                {
                    comboBox.Items.Add(sqlDataReader.GetString(0));
                }
                connection.Close();
            }
        }

        private void flightDataGrid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            FlightClasses.Clear();
            string[] SelectionOptions = {"A Class", "B Class", "C Class" };
            this.ClassComboBox.DataSource = SelectionOptions;
            if (e.RowIndex >= 0)
            {
                DataGridViewRow selectedRow = flightDataGrid.Rows[e.RowIndex];
                this.flightNoTextBox.Text = selectedRow.Cells["FlightNo"].Value.ToString();
                this.seatsAvailableTextBox.Text = selectedRow.Cells["AvailableSeats"].Value.ToString();
                this.arrivalCountryTextBox.Text = selectedRow.Cells["arrivalCountry"].Value.ToString();
                this.arrivalDateTextBox.Text = selectedRow.Cells["expectedArrivalDate"].Value.ToString();
                this.deptCountryTextBox.Text = selectedRow.Cells["deptCountry"].Value.ToString();
                FlightClasses.Add("A Class",Convert.ToDouble(selectedRow.Cells["Rank1Price"].Value));
                FlightClasses.Add("B Class",Convert.ToDouble(selectedRow.Cells["Rank2Price"].Value));
                FlightClasses.Add("C Class",Convert.ToDouble(selectedRow.Cells["Rank3Price"].Value));
                this.ClassPriceTextBox.Text = selectedRow.Cells["Rank1Price"].Value.ToString();
            }
        }
        private void ClassComboBox_SelectedValueChanged(object sender, EventArgs e)
        {   
            string selectedClass = ClassComboBox.Text;
            if (FlightClasses.ContainsKey(selectedClass))
            {
                double price = FlightClasses[selectedClass];
                ClassPriceTextBox.Text = price.ToString();
                TotalPriceTextBox.Text = (Convert.ToDouble(seatsNumericUpDown.Value) * FlightClasses[this.ClassComboBox.Text]).ToString(); 
            }
            else
            {
                ClassPriceTextBox.Text = string.Empty;
            }
        }
        private void BookFlight_Load(object sender, EventArgs e)
        {
            bookFlightComboBox_Changed(sender, e);
            this.passportNumberTextBox.Text = Customer.Instance.PassportNumber;
            this.creditCardExpiryDateTextBox.Text = Customer.Instance.ExpirayDate;
            this.creditCardTextBox.Text = Customer.Instance.CardNum;
            this.cvvCreditCardTextBox.Text = Customer.Instance.Cvv;
        }

        private void seatsNumericUpDown_UpButton(object sender, EventArgs e)
        {
            if (Convert.ToInt32(seatsAvailableTextBox.Text) < 7) {
                if (seatsNumericUpDown.Value >= Convert.ToInt32(seatsAvailableTextBox.Text)) {
                    seatsNumericUpDown.Value = Convert.ToInt32(seatsAvailableTextBox.Text);
                }
            }
            else if (seatsNumericUpDown.Value >= 7)
            {
                seatsNumericUpDown.Value = 7;
            }
            TotalPriceTextBox.Text = (Convert.ToDouble(seatsNumericUpDown.Value) * FlightClasses[this.ClassComboBox.Text]).ToString(); 
        }

        private void confirmButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(flightNoTextBox.Text) || string.IsNullOrEmpty(seatsAvailableTextBox.Text) || string.IsNullOrEmpty(arrivalCountryTextBox.Text) || string.IsNullOrEmpty(arrivalDateTextBox.Text) || string.IsNullOrEmpty(deptCountryTextBox.Text))
            {
                MessageBox.Show("You must select a Flight first!");
            }
            else
            {
                string ticketQuery = "INSERT INTO BookingDetails (CustomerID, FlightNo, BookingDate,SeatAssignment ,TicketPrice, Rank, Status) Values (@CustomerID, @FlightNo, @BookingDate, @SeatAssignment, @TicketPrice, @Rank, @Status);";
                string flightUpdateQuery = "Update Flight set AvailableSeats = @AvailableSeats where FlightNo = @FlightNo;";
                string updatePassportDetails = "Update CustomerTable SET PassportNumber = @PassportNumber, PassportExpirationDate = @PassportExpirationDate, CardNum = @CardNum, CVV = @CVV, ExpiryDate = @ExpiryDate where CustomerID = @CustomerID;";
                using (SqlConnection connection = new SqlConnection(databaseConnection)) {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(ticketQuery, connection)) {
                        command.Parameters.AddWithValue("@CustomerID", Customer.Instance.Id);
                        command.Parameters.AddWithValue("@FlightNo", int.Parse(this.flightNoTextBox.Text));
                        command.Parameters.AddWithValue("@BookingDate",DateTime.Now);
                        command.Parameters.AddWithValue("@SeatAssignment",2);
                        command.Parameters.AddWithValue("@TicketPrice",Decimal.Parse(this.ClassPriceTextBox.Text));
                        command.Parameters.AddWithValue("@Rank",this.ClassComboBox.Text);
                        command.Parameters.AddWithValue("@Status",1);
                        for (int i = 0; i < seatsNumericUpDown.Value; i++) {      
                            command.ExecuteNonQuery();
                        }
                    }
                    using(SqlCommand command = new SqlCommand(flightUpdateQuery, connection)) {
                        command.Parameters.AddWithValue("@AvailableSeats", int.Parse(this.seatsAvailableTextBox.Text));
                        command.Parameters.AddWithValue("@FlightNo", int.Parse(this.flightNoTextBox.Text));
                        command.ExecuteNonQuery();
                    }
                 using (SqlCommand command = new SqlCommand(updatePassportDetails, connection))
                {
                    command.Parameters.AddWithValue("@PassportNumber", this.passportNumberTextBox.Text);

                    // Convert the date to the desired format

                    command.Parameters.AddWithValue("@PassportExpirationDate", this.passportDateTimePicker.Text);
                    command.Parameters.AddWithValue("@CardNum", this.creditCardTextBox.Text);
                    command.Parameters.AddWithValue("@CVV", int.Parse(this.cvvCreditCardTextBox.Text));
                    command.Parameters.AddWithValue("@ExpiryDate", this.creditCardExpiryDateTextBox.Text);
                    command.Parameters.AddWithValue("@CustomerID", Customer.Instance.Id);

                    command.ExecuteNonQuery();
                }
            
                        
                    Customer.Instance.PassportNumber = this.passportNumberTextBox.Text;
                    Customer.Instance.ExpirayDate = this.creditCardExpiryDateTextBox.Text;
                    Customer.Instance.PassportExpirayDate = this.passportDateTimePicker.Text;
                    Customer.Instance.CardNum = this.creditCardTextBox.Text;
                    Customer.Instance.Cvv = this.cvvCreditCardTextBox.Text;
                    MessageBox.Show("The Flight has been Confirmed!");
                    connection.Close();
                }
            }
        }
    }
}