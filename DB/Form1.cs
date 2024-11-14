using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;

namespace DB
{
    public partial class Form1 : Form
    {
        private SqlConnection sqlConnection = null;
        private SqlTransaction sqlTransaction = null;
        private string connectionString = null;
        private SqlDataAdapter dataAdapter;
        private DataTable dataTable;
        public Form1()
        {
            InitializeComponent();
        }
        private async void Form1_Load(object sender, EventArgs e)
        {

            connectionString = @"Data Source=HP_OMEN;Initial Catalog=AutoParts;Integrated Security=True;";
            sqlConnection = new SqlConnection(connectionString);
            await sqlConnection.OpenAsync();
            DataTable_Products();
            DataTable_Sales();
            DataTable_DefectiveProducts();
            Orders_SuppliersName_ComboBox();
            DefectiveProduct_SupplierName_ComboBox();
            Q1_SupplierCategory_ComboBox();
            Q2_ProductName_Combobox();
            Q3_ProductName_ComboBox();
            Q6_ProductName_ComboBox();
            Q7_SupplierName_ComboBox();
            Q14_ProductName_ComboBox();
            QueryLoadItems();
        }
        // ----------------------- ВВОД ДАННЫХ. ТОВАР -----------------------
        private void Product_AddProduct_Button_Click(object sender, EventArgs e)
        {
            if (Decimal.TryParse(products_ProductPrice_TextBox.Text, out decimal try_productPrice))
            {
                if (int.TryParse(products_CellNumber_TextBox.Text.ToString(), out int try_cellNumber))
                {
                    if (int.TryParse(products_Quantity_TextBox.Text.ToString(), out int try_quantity)) {
                        if (products_SupplierName_TextBox.Text != String.Empty &&
                        products_SupplierCategory_TextBox.Text != String.Empty &&
                        products_ProductName_TextBox.Text != String.Empty &&
                        products_ProductPrice_TextBox.Text != String.Empty &&
                        products_CellNumber_TextBox.Text != String.Empty &&
                        products_Quantity_TextBox.Text != String.Empty)
                        {
                            string supplierName = products_SupplierName_TextBox.Text;
                            string supplierCategory = products_SupplierCategory_TextBox.Text;
                            string productName = products_ProductName_TextBox.Text;
                            decimal productPrice = try_productPrice;
                            DateTime deliveryDate = products_DeliveryDate_DateTimePicker.Value;
                            int cellNumber = Convert.ToInt32(products_CellNumber_TextBox.Text);
                            int quantity = Convert.ToInt32(products_Quantity_TextBox.Text);
                            sqlTransaction = sqlConnection.BeginTransaction();
                            try
                            {
                                SqlCommand cmd = sqlConnection.CreateCommand();
                                cmd.Transaction = sqlTransaction;
                                cmd.CommandText = @"
                    DECLARE @SupplierID INT;
                    IF EXISTS (SELECT 1 FROM Suppliers WHERE SupplierName = @supplier_name AND SupplierCategory = @supplier_category)
                    BEGIN
                        SELECT @SupplierID = SupplierID FROM Suppliers WHERE SupplierName = @supplier_name AND SupplierCategory = @supplier_category;
                    END
                    ELSE
                    BEGIN
                        INSERT INTO Suppliers (SupplierName, SupplierCategory) VALUES (@supplier_name, @supplier_category);
                        SET @SupplierID = SCOPE_IDENTITY();
                    END
                    DECLARE @ProductID INT
                    IF EXISTS (SELECT 1 FROM Products WHERE ProductName = @product_name AND SupplierID = @SupplierID AND DeliveryDate = @delivery_date)
                    BEGIN
                        SELECT @ProductID = ProductID FROM Products WHERE ProductName = @product_name;
                    END
                    ELSE
                    BEGIN
                        SELECT SupplierID FROM Suppliers WHERE SupplierName = @supplier_name;
                        INSERT INTO Products (ProductName, ProductPrice, DeliveryDate, SupplierID) VALUES (@product_name, @product_price, @delivery_date, @SupplierID);
                        SET @ProductID = SCOPE_IDENTITY();
                    END
                    DECLARE @CurrentQuantity INT;
                    IF EXISTS (SELECT 1 FROM Warehouse WHERE ProductID = @ProductID)
                    BEGIN
                        SELECT @CurrentQuantity = Quantity FROM Warehouse WHERE ProductID = @ProductID;
                        UPDATE Warehouse SET Quantity = @CurrentQuantity + @quantity WHERE ProductID = @ProductID;
                    END
                    ELSE
                    BEGIN
                        INSERT INTO Warehouse (CellNumber, ProductID, Quantity) VALUES (@cell_number, @ProductID, @quantity);
                    END";
                                cmd.Parameters.AddWithValue("@supplier_name", supplierName);
                                cmd.Parameters.AddWithValue("@supplier_category", supplierCategory);
                                cmd.Parameters.AddWithValue("@product_name", productName);
                                cmd.Parameters.AddWithValue("@product_price", productPrice);
                                cmd.Parameters.AddWithValue("@delivery_date", deliveryDate);
                                cmd.Parameters.AddWithValue("cell_number", cellNumber);
                                cmd.Parameters.AddWithValue("@quantity", quantity);
                                cmd.ExecuteNonQuery();
                                sqlTransaction.Commit();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Транзакция откатывается из-за ошибки: " + ex.Message);
                                sqlTransaction.Rollback();
                            }
                            products_SupplierName_TextBox.Text = null;
                            products_SupplierCategory_TextBox.Text = null;
                            products_ProductName_TextBox.Text = null;
                            products_ProductPrice_TextBox.Text = null;
                            products_DeliveryDate_DateTimePicker.Update();
                            products_CellNumber_TextBox.Text = null;
                            products_Quantity_TextBox.Text = null;
                            Orders_SuppliersName_ComboBox();
                            DefectiveProduct_SupplierName_ComboBox();
                        }
                        else MessageBox.Show("Все поля должны быть заполнены!");
                    }
                    else MessageBox.Show("Поле 'Количество' заполнено неверно.\nПовторите ввод.");
                }
                else MessageBox.Show("Поле 'Ячейка на складе' заполнено неверно.\nПовторите ввод.");
            }
            else MessageBox.Show("Поле 'Цена продукта' заполнено неверно.\nПовторите ввод.");
        }
        // ----------------------- ВВОД ДАННЫХ. ЗАЯВКА -----------------------    
        private void Orders_SuppliersName_ComboBox()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT SupplierName FROM Suppliers";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Добавляем каждый ProductName в ComboBox
                                string productName = reader["SupplierName"].ToString();
                                orders_SupplierName_ComboBox.Items.Add(productName);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке данных: " + ex.Message);
                }
            }
        }
        private void Orders_ProductName_ComboBox()
        {
            orders_ProductName_ComboBox.Items.Clear();
            // Получаем выбранное значение из первого ComboBox
            string selectedSupplier = orders_SupplierName_ComboBox.Text;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Запрос для выборки всех ProductName по SupplierName
                    string query = @"
                                    SELECT P.ProductName
                                    FROM Products P
                                    JOIN Suppliers S ON P.SupplierID = S.SupplierID
                                    JOIN Warehouse W ON P.ProductID = W.ProductID
                                    WHERE S.SupplierName = @SupplierName
                                    AND W.Quantity > 0;
";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Добавляем параметр @SupplierName в запрос
                        command.Parameters.AddWithValue("@SupplierName", selectedSupplier);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Добавляем каждый ProductName во второй ComboBox
                                string productName = reader["ProductName"].ToString();
                                orders_ProductName_ComboBox.Items.Add(productName);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке данных: " + ex.Message);
                }
            }
        }
        private void Orders_ProductPrice_ComboBox()
        {
            orders_ProductPrice_ComboBox.Items.Clear();
            // Получаем выбранное значение из первого ComboBox
            string selectedProductName = orders_ProductName_ComboBox.Text;
            string selectedSupplierName = orders_SupplierName_ComboBox.Text;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    // Запрос для выборки всех ProductName по SupplierName
                    string query = @"
                                    SELECT P.ProductPrice
                                    FROM Products P
                                    JOIN Suppliers S ON P.SupplierID = S.SupplierID
                                    JOIN Warehouse W ON P.ProductID = W.ProductID
                                    WHERE S.SupplierName = @SupplierName
                                    AND P.ProductName = @ProductName
                                    AND W.Quantity > 0;";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Добавляем параметр @SupplierName в запрос
                        command.Parameters.AddWithValue("@ProductName", selectedProductName);
                        command.Parameters.AddWithValue("@SupplierName", selectedSupplierName);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Добавляем каждый ProductName во второй ComboBox
                                string productPrice = reader["ProductPrice"].ToString();
                                orders_ProductPrice_ComboBox.Items.Add(productPrice);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке данных: " + ex.Message);
                }
            }
        }
        private void Orders_Quantity_ComboBox()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Запрос для получения максимального значения Quantity
                    string query = @"
                                    SELECT W.Quantity AS WarehouseQuantity
                                    FROM Warehouse W
                                    JOIN Products P ON W.ProductID = P.ProductID
                                    JOIN Suppliers S ON P.SupplierID = S.SupplierID
                                    WHERE S.SupplierName = @supplier_name
                                    AND P.ProductName = @product_name
                                    AND P.ProductPrice = @product_price;";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@supplier_name", orders_SupplierName_ComboBox.Text);
                        command.Parameters.AddWithValue("@product_name", orders_ProductName_ComboBox.Text);
                        command.Parameters.AddWithValue("@product_price", Decimal.Parse(orders_ProductPrice_ComboBox.Text.ToString()));
                        object result = command.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            int maxQuantity = Convert.ToInt32(result);

                            // Устанавливаем максимальное значение NumericUpDown
                            orders_Quantity_NumericUpDown.Maximum = maxQuantity;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке данных: " + ex.Message);
                }
            }
        }
        private void Orders_SupplierName_ComboBox_SelectedIndexChanged(object Sender, EventArgs e)
        {
            orders_ProductName_ComboBox.Items.Clear();
            orders_ProductName_ComboBox.Text = null;
            Orders_ProductName_ComboBox();
        }
        private void Orders_ProductName_ComboBox_SelectedIndexChanged(object Sender, EventArgs e)
        {
            orders_ProductPrice_ComboBox.Items.Clear();
            orders_ProductPrice_ComboBox.Text = null;
            Orders_ProductPrice_ComboBox();
        }
        private void Orders_ProductPrice_ComboBox_SelectedIndexChanged(object Sender, EventArgs e)
        {
            orders_Quantity_NumericUpDown.Maximum = 0;
            Orders_Quantity_ComboBox();
        }
        private void Orders_CreateOrder_Button_Click(object Sender, EventArgs e)
        {
            if (orders_CustomerName_TextBox.Text != String.Empty &&
                orders_SupplierName_ComboBox.Text != String.Empty &&
                orders_ProductName_ComboBox.Text != String.Empty &&
                orders_Quantity_NumericUpDown.Value != 0 &&
                orders_OverheadPercentage_TextBox.Text != String.Empty)
            {
                string customerName = orders_CustomerName_TextBox.Text;
                string supplierName = orders_SupplierName_ComboBox.Text;
                string productName = orders_ProductName_ComboBox.Text;
                decimal productPrice = Decimal.Parse(orders_ProductPrice_ComboBox.Text.ToString());
                int orderQuantity = int.Parse(orders_Quantity_NumericUpDown.Value.ToString());
                int overheadPercentage = int.Parse(orders_OverheadPercentage_TextBox.Text);
                decimal salePrice = productPrice + (productPrice / 100 * overheadPercentage);
                sqlTransaction = sqlConnection.BeginTransaction();
                try
                {
                    SqlCommand cmd = sqlConnection.CreateCommand();
                    cmd.Transaction = sqlTransaction;
                    cmd.CommandText = @"
                                    DECLARE @CustomerId INT;
                                    DECLARE @ProductId INT;
                                    DECLARE @OrderId INT;
                                    DECLARE @SaleID INT;

                                    IF NOT EXISTS (SELECT 1 FROM Customers WHERE CustomerName = @customerName)
                                    BEGIN
                                        -- Если клиент не существует, создаем нового клиента
                                        INSERT INTO Customers (CustomerName)
                                        VALUES (@customerName);
                                        SET @CustomerId = SCOPE_IDENTITY();
                                    END
                                    ELSE
                                    BEGIN
                                        -- Если клиент существует, получаем его идентификатор
                                        SELECT @CustomerId = CustomerID FROM Customers WHERE CustomerName = @customerName;
                                    END

                                    SELECT @ProductId = ProductID FROM Products WHERE ProductName = @product_name AND ProductPrice = @product_price;

                                    INSERT INTO CustomerOrders (CustomerID, ProductID, OrderQuantity)
                                    VALUES (@CustomerId, @ProductId, @order_quantity);

                                    SET @OrderId = SCOPE_IDENTITY();

                                    DECLARE @AvailableQuantity INT;
                                    SELECT @AvailableQuantity = Quantity FROM Warehouse WHERE ProductID = @ProductId;

                                    IF @order_quantity >= @AvailableQuantity
                                    BEGIN
                                        -- Удаляем запись из таблицы Warehouse
                                        DELETE FROM Warehouse WHERE ProductID = @ProductId;
                                    END
                                    ELSE
                                    BEGIN
                                        -- Отнимаем заказанное количество от имеющегося на складе
                                        UPDATE Warehouse SET Quantity = Quantity - @order_quantity WHERE ProductID = @ProductId;
                                    END

                                    -- Добавляем продажу
                                    INSERT INTO Sales (SaleDate, ProductID, CustomerID, QuantitySold, SalePrice)
                                    VALUES (GETDATE(), @ProductId, @CustomerId, @order_quantity, @sale_price)
                                    SET @SaleID = SCOPE_IDENTITY();

                                    INSERT INTO OverheadExpenses (SaleID, OverheadPercentage)
                                    VALUES (@SaleID, @overhead_percentage);";
                    cmd.Parameters.AddWithValue("@customerName", customerName);
                    cmd.Parameters.AddWithValue("@supplierName", supplierName);
                    cmd.Parameters.AddWithValue("@product_name", productName);
                    cmd.Parameters.AddWithValue("@product_price", productPrice);
                    cmd.Parameters.AddWithValue("@order_quantity", orderQuantity);
                    cmd.Parameters.AddWithValue("@overhead_percentage", overheadPercentage);
                    cmd.Parameters.AddWithValue("@sale_price", salePrice);
                    cmd.ExecuteNonQuery();
                    sqlTransaction.Commit();
                    MessageBox.Show("Транзакция успешно выполнена.");

                }
                catch (Exception ex)
                {
                    // Откатываем транзакцию в случае ошибки
                    MessageBox.Show("Транзакция откатывается из-за ошибки: " + ex.Message);
                    sqlTransaction.Rollback();
                }
                orders_CustomerName_TextBox.Text = null;
                orders_SupplierName_ComboBox.Items.Clear();
                orders_SupplierName_ComboBox.Text = null;
                orders_ProductName_ComboBox.Items.Clear();
                orders_ProductName_ComboBox.Text = null;
                orders_ProductPrice_ComboBox.Items.Clear();
                orders_ProductPrice_ComboBox.Text = null;
                orders_Quantity_NumericUpDown.Value = 0;
                orders_OverheadPercentage_TextBox.Text = null;
                Orders_SuppliersName_ComboBox();
            }
            else
            {
                MessageBox.Show("Все поля должны быть заполнены!\nКоличество не должно быть равно нулю.");
            }

        }
        // ----------------------- ВВОД ДАННЫХ. БРАК -----------------------
        private void DefectiveProduct_SupplierName_ComboBox()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT SupplierName FROM Suppliers";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Добавляем каждый ProductName в ComboBox
                                string productName = reader["SupplierName"].ToString();
                                defectiveProducts_SupplierName_ComboBox.Items.Add(productName);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке данных: " + ex.Message);
                }
            }
        }
        private void DefectiveProduct_ProductName_ComboBox()
        {
            defectiveProducts_ProductName_ComboBox.Items.Clear();
            // Получаем выбранное значение из первого ComboBox
            string selectedSupplier = defectiveProducts_SupplierName_ComboBox.Text;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    // Запрос для выборки всех ProductName по SupplierName
                    string query = @"
                                    SELECT P.ProductName
                                    FROM Products P
                                    JOIN Suppliers S ON P.SupplierID = S.SupplierID
                                    JOIN Warehouse W ON P.ProductID = W.ProductID
                                    WHERE S.SupplierName = @SupplierName
                                    AND W.Quantity > 0;
";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Добавляем параметр @SupplierName в запрос
                        command.Parameters.AddWithValue("@SupplierName", selectedSupplier);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Добавляем каждый ProductName во второй ComboBox
                                string productName = reader["ProductName"].ToString();
                                defectiveProducts_ProductName_ComboBox.Items.Add(productName);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке данных: " + ex.Message);
                }
            }
        }
        private void DefectiveProduct_ProductPrice_ComboBox()
        {
            defectiveProducts_ProductPrice_ComboBox.Items.Clear();
            string selectedSupplierName = defectiveProducts_SupplierName_ComboBox.Text;
            string selectedProductNamee = defectiveProducts_ProductName_ComboBox.Text;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    // Запрос для выборки всех ProductName по SupplierName
                    string query = @"
                                    SELECT P.ProductPrice
                                    FROM Products P
                                    JOIN Suppliers S ON P.SupplierID = S.SupplierID
                                    JOIN Warehouse W ON P.ProductID = W.ProductID
                                    WHERE S.SupplierName = @SupplierName AND P.ProductName = @ProductName
                                    AND W.Quantity > 0;
";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@SupplierName", selectedSupplierName);
                        command.Parameters.AddWithValue("@ProductName", selectedProductNamee);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string productPrice = reader["ProductPrice"].ToString();
                                defectiveProducts_ProductPrice_ComboBox.Items.Add(productPrice);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке данных: " + ex.Message);
                }
            }
        }
        private void DefectiveProduct_DeliveryDate_ComboBox()
        {
            defectiveProducts_DeliveryDate_ComboBox.Text = null;
            string selectedSupplierName = defectiveProducts_SupplierName_ComboBox.Text;
            string selectedProductNamee = defectiveProducts_ProductName_ComboBox.Text;
            string selectedProductPrice = defectiveProducts_ProductPrice_ComboBox.Text;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    // Запрос для выборки всех ProductName по SupplierName
                    string query = @"
                                    SELECT P.DeliveryDate
                                    FROM Products P
                                    JOIN Suppliers S ON P.SupplierID = S.SupplierID
                                    JOIN Warehouse W ON P.ProductID = W.ProductID
                                    WHERE S.SupplierName = @SupplierName 
                                    AND P.ProductName = @ProductName 
                                    AND P.ProductPrice = @ProductPrice 
                                    AND W.Quantity > 0;";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Добавляем параметр @SupplierName в запрос
                        command.Parameters.AddWithValue("@SupplierName", selectedSupplierName);
                        command.Parameters.AddWithValue("@ProductName", selectedProductNamee);
                        command.Parameters.AddWithValue("@ProductPrice", Decimal.Parse(selectedProductPrice));
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Добавляем каждый ProductPrice во второй ComboBox
                                string deliveryDate = reader["DeliveryDate"].ToString();
                                defectiveProducts_DeliveryDate_ComboBox.Items.Add(deliveryDate);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке данных: " + ex.Message);
                }
            }
        }
        private void DefectiveProduct_Quantity_ComboBox()
        {
            string selectedSupplierName = defectiveProducts_SupplierName_ComboBox.Text;
            string selectedProductName = defectiveProducts_ProductName_ComboBox.Text;
            string selectedProductPrice = defectiveProducts_ProductPrice_ComboBox.Text;
            string selectedDeliveryDate = defectiveProducts_DeliveryDate_ComboBox.Text;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    // Запрос для получения максимального значения Quantity
                    string query = @"
                                    SELECT W.Quantity AS WarehouseQuantity
                                    FROM Warehouse W
                                    JOIN Products P ON W.ProductID = P.ProductID
                                    JOIN Suppliers S ON P.SupplierID = S.SupplierID
                                    WHERE S.SupplierName = @supplier_name
                                    AND P.ProductName = @product_name
                                    AND P.ProductPrice = @product_price
                                    AND P.DeliveryDate = @delivery_date";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@supplier_name", selectedSupplierName);
                        command.Parameters.AddWithValue("@product_name", selectedProductName);
                        command.Parameters.AddWithValue("@product_price", Decimal.Parse(selectedProductPrice));
                        command.Parameters.AddWithValue("@delivery_date", DateTime.Parse(selectedDeliveryDate));
                        object result = command.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            int maxQuantity = Convert.ToInt32(result);
                            // Устанавливаем максимальное значение NumericUpDown
                            defectiveProducts_Quantity_NumbericUpDown.Maximum = maxQuantity;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке данных: " + ex.Message);
                }
            }
        }
        private void DefectiveProduct_Suppliers_ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            defectiveProducts_ProductName_ComboBox.Items.Clear();
            defectiveProducts_ProductName_ComboBox.Text = null;
            defectiveProducts_ProductPrice_ComboBox.Items.Clear();
            defectiveProducts_ProductPrice_ComboBox.Text = null;
            defectiveProducts_DeliveryDate_ComboBox.Items.Clear();
            defectiveProducts_DeliveryDate_ComboBox.Text = null;
            defectiveProducts_Quantity_NumbericUpDown.Value = 0;
            DefectiveProduct_ProductName_ComboBox();
        }
        private void DefectiveProduct_ProductName_ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            defectiveProducts_ProductPrice_ComboBox.Items.Clear();
            defectiveProducts_ProductPrice_ComboBox.Text = null;
            defectiveProducts_DeliveryDate_ComboBox.Items.Clear();
            defectiveProducts_DeliveryDate_ComboBox.Text = null;
            defectiveProducts_Quantity_NumbericUpDown.Value = 0;
            DefectiveProduct_ProductPrice_ComboBox();
        }
        private void DefectiveProduct_ProductPrice_ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            defectiveProducts_DeliveryDate_ComboBox.Items.Clear();
            defectiveProducts_DeliveryDate_ComboBox.Text = null;
            defectiveProducts_Quantity_NumbericUpDown.Value = 0;
            DefectiveProduct_DeliveryDate_ComboBox();
        }
        private void DefectiveProduct_DeliveryDate_ComboBox_SelectedIndexChanged(Object sender, EventArgs e)
        {
            defectiveProducts_Quantity_NumbericUpDown.Value = 0;
            DefectiveProduct_Quantity_ComboBox();
        }
        private void DefectiveProduct_DefectProduct_Button_Click(object sender, EventArgs e)
        {
            if (defectiveProducts_SupplierName_ComboBox.Text != String.Empty &&
                defectiveProducts_ProductName_ComboBox.Text != String.Empty &&
                defectiveProducts_ProductPrice_ComboBox.Text != String.Empty &&
                defectiveProducts_DeliveryDate_ComboBox.Text != String.Empty &&
                defectiveProducts_Quantity_NumbericUpDown.Value != 0)
            {
                string selectedSupplierName = defectiveProducts_SupplierName_ComboBox.Text;
                string selectedProductName = defectiveProducts_ProductName_ComboBox.Text;
                string selectedProductPrice = defectiveProducts_ProductPrice_ComboBox.Text;
                string selectedDeliveryDate = defectiveProducts_DeliveryDate_ComboBox.Text;
                int selectedDefectQuantity = int.Parse(defectiveProducts_Quantity_NumbericUpDown.Value.ToString());
                sqlTransaction = sqlConnection.BeginTransaction();
                try
                {
                    SqlCommand cmd = sqlConnection.CreateCommand();
                    cmd.Transaction = sqlTransaction;
                    cmd.CommandText = @"
                                    DECLARE @ProductID INT;
                                    DECLARE @SupplierID INT;

                                    -- Находим SupplierID для введенного SupplierName
                                    SELECT @SupplierID = SupplierID 
                                    FROM Suppliers 
                                    WHERE SupplierName = @supplierName;

                                    -- Находим ProductID для введенного ProductName и SupplierID
                                    SELECT @ProductID = P.ProductID 
                                    FROM Products P
                                    JOIN Suppliers S ON P.SupplierID = S.SupplierID
                                    WHERE P.ProductName = @product_name
                                    AND P.ProductPrice = @product_price
                                    AND S.SupplierID = @SupplierID
                                    AND P.DeliveryDate = @delivery_date;

                                   -- Проверяем, существует ли запись в таблице DefectiveProducts для указанного ProductID
                                    IF EXISTS (SELECT 1 FROM DefectiveProducts WHERE ProductID = @ProductID AND SupplierID = @SupplierID)
                                    BEGIN
                                        -- Если запись существует, увеличиваем количество бракованных продуктов
                                        UPDATE DefectiveProducts 
                                        SET DefectQuantity = DefectQuantity + @defect_quantity
                                        WHERE ProductID = @ProductID;
                                    END
                                    ELSE
                                    BEGIN
                                        -- Если запись не существует, вставляем новую запись в таблицу DefectiveProducts
                                        INSERT INTO DefectiveProducts (ProductID, DefectQuantity, SupplierID, DefectDate)
                                        VALUES (@ProductID, @defect_quantity, @SupplierID, GETDATE());
                                    END

                                    -- Проверяем, равно ли введенное количество имеющемуся на складе
                                    IF @defect_quantity = (
                                        SELECT Quantity 
                                        FROM Warehouse 
                                        WHERE ProductID = @ProductID
                                    )
                                    BEGIN
                                        -- Удаляем запись из таблицы Warehouse
                                        DELETE FROM Warehouse 
                                        WHERE ProductID = @ProductID;
                                    END
                                    ELSE
                                    BEGIN
                                        -- Уменьшаем количество на складе на введенное количество
                                        UPDATE Warehouse 
                                        SET Quantity = Quantity - @defect_quantity
                                        WHERE ProductID = @ProductID;
                                    END";
                    cmd.Parameters.AddWithValue("@supplierName", selectedSupplierName);
                    cmd.Parameters.AddWithValue("@product_name", selectedProductName);
                    cmd.Parameters.AddWithValue("@product_price", Decimal.Parse(selectedProductPrice));
                    cmd.Parameters.AddWithValue("@delivery_date", DateTime.Parse(selectedDeliveryDate));
                    cmd.Parameters.AddWithValue("@defect_quantity", selectedDefectQuantity);
                    cmd.ExecuteNonQuery();
                    sqlTransaction.Commit();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Транзакция откатывается из-за ошибки: " + ex.Message);
                    sqlTransaction.Rollback();
                }
                defectiveProducts_SupplierName_ComboBox.Items.Clear();
                defectiveProducts_SupplierName_ComboBox.Text = null;
                defectiveProducts_ProductName_ComboBox.Items.Clear();
                defectiveProducts_ProductName_ComboBox.Text = null;
                defectiveProducts_ProductPrice_ComboBox.Items.Clear();
                defectiveProducts_ProductPrice_ComboBox.Text = null;
                defectiveProducts_DeliveryDate_ComboBox.Items.Clear();
                defectiveProducts_DeliveryDate_ComboBox.Text = null;
                defectiveProducts_Quantity_NumbericUpDown.Value = 0;
                DefectiveProduct_SupplierName_ComboBox();
            }
            else
            {
                MessageBox.Show("Все поля должны быть заполнены!\nКоличество не должно быть равно нулю.");
            }
        }
        // ----------------------- ТАБЛИЦЫ ДАННЫХ. ТОВАР -----------------------
        private async void DataTable_Products()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                string query = @"
                SELECT 
                    P.ProductID AS [ProductID],
                    S.SupplierName AS [Supplier Name],
                    P.ProductName AS [Product Name], 
                    S.SupplierCategory AS [Supplier Category], 
                    P.DeliveryDate AS [Delivery Date], 
                    P.ProductPrice AS [Product Price], 
                    W.CellNumber AS [Cell Number], 
                    W.Quantity AS [Quantity]
                FROM 
                    Products P
                JOIN 
                    Suppliers S ON P.SupplierID = S.SupplierID
                JOIN 
                    Warehouse W ON P.ProductID = W.ProductID";

                dataAdapter = new SqlDataAdapter(query, connection);
                SqlCommandBuilder commandBuilder = new SqlCommandBuilder(dataAdapter);

                dataTable = new DataTable();
                dataAdapter.Fill(dataTable);

                BindingSource bindingSource = new BindingSource();
                bindingSource.DataSource = dataTable;
                Products_DataGridView.DataSource = bindingSource;

                // Enable editing in DataGridView
                Products_DataGridView.AllowUserToAddRows = true;
                Products_DataGridView.AllowUserToDeleteRows = true;
                Products_DataGridView.ReadOnly = false;
            }
            /*using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                string query = @"
                    SELECT 
                        P.ProductID AS [ProductID],
                        S.SupplierName AS [Supplier Name],
                        P.ProductName AS [Product Name], 
                        S.SupplierCategory AS [Supplier Category], 
                        P.DeliveryDate AS [Delivery Date], 
                        P.ProductPrice AS [Product Price], 
                        W.CellNumber AS [Cell Number], 
                        W.Quantity AS [Quantity]
                    FROM 
                        Products P
                    JOIN 
                        Suppliers S ON P.SupplierID = S.SupplierID
                    JOIN 
                        Warehouse W ON P.ProductID = W.ProductID";
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                Products_DataGridView.DataSource = dataTable;
            }*/
        }
        private void DataTable_Products_Update_Button_Click(object Sender, EventArgs e)
        {
            DataTable_Products();
        }
        private void DataTable_Products_Delete_Button_Click(object Sender, EventArgs e)
        {
            // Получаем ProductID записи, которую нужно удалить (например, из выбранной строки в DataGridView)
            DataGridViewRow selectedRow = Products_DataGridView.SelectedRows[0];
            int productID = int.Parse(selectedRow.Cells["ProductID"].Value.ToString());
            sqlTransaction = sqlConnection.BeginTransaction();
            try
            {
                SqlCommand cmd = sqlConnection.CreateCommand();
                cmd.Transaction = sqlTransaction;
                cmd.CommandText = @"
                                    DECLARE @CustomerID INT;
                                    SELECT @CustomerID = CustomerID FROM CustomerOrders WHERE ProductID = @ProductId;
                                    DELETE FROM Warehouse WHERE ProductID = @ProductID;
                                    DELETE FROM CustomerOrders WHERE ProductID = @ProductID;
                                    DELETE FROM Customers WHERE CustomerID = @CustomerID;
                                    DELETE FROM Products WHERE ProductID = @ProductID;
                                    DELETE FROM Suppliers WHERE SupplierID NOT IN (SELECT SupplierID FROM Products);";

                cmd.Parameters.AddWithValue("@ProductID", productID);
                cmd.ExecuteNonQuery();
                sqlTransaction.Commit();
                MessageBox.Show("Транзакция успешно выполнена.");

            }
            catch (Exception ex)
            {
                // Откатываем транзакцию в случае ошибки
                MessageBox.Show("Транзакция откатывается из-за ошибки: " + ex.Message);
                sqlTransaction.Rollback();
            }
            DataTable_Products();
        }
        private async void DataTable_Products_Save_Button_Click(object Sender, EventArgs e)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Use SqlCommandBuilder to automatically generate the UpdateCommand, InsertCommand, and DeleteCommand
                SqlCommandBuilder commandBuilder = new SqlCommandBuilder(dataAdapter);

                try
                {
                    // Update the database with the changes made in the DataTable
                    dataAdapter.Update(dataTable);
                    MessageBox.Show("Changes saved to the database.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}");
                }
            }
            /*using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                string query = @"
                    SELECT 
                        P.ProductID AS [ProductID],
                        S.SupplierName AS [Supplier Name],
                        P.ProductName AS [Product Name], 
                        S.SupplierCategory AS [Supplier Category], 
                        P.DeliveryDate AS [Delivery Date], 
                        P.ProductPrice AS [Product Price], 
                        W.CellNumber AS [Cell Number], 
                        W.Quantity AS [Quantity]
                    FROM 
                        Products P
                    JOIN 
                        Suppliers S ON P.SupplierID = S.SupplierID
                    JOIN 
                        Warehouse W ON P.ProductID = W.ProductID";
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                Products_DataGridView.DataSource = dataTable;
                adapter.Update(dataTable);
            }
            MessageBox.Show("Changes saved to the database.");*/
        }
        // ----------------------- ТАБЛИЦЫ ДАННЫХ. ПРОДАЖИ -----------------------
        private async void DataTable_Sales()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                string query = @"
                                SELECT 
                                    S.SaleID AS [SaleID], 
                                    C.CustomerName AS [Customer Name], 
                                    Su.SupplierName AS [Supplier Name],
                                    P.ProductName AS [Product Name], 
                                    P.ProductPrice AS [Product Price], 
                                    S.QuantitySold AS [Quantity Sold], 
                                    S.SaleDate AS [Sale Date],
                                    OE.OverheadPercentage AS [Overhead Percentage], 
                                    S.SalePrice AS [Sale Price]
                                FROM 
                                    Sales S
                                JOIN 
                                    Customers C ON S.CustomerID = C.CustomerID
                                JOIN 
                                    Products P ON S.ProductID = P.ProductID
                                JOIN 
                                    Suppliers Su ON P.SupplierID = Su.SupplierID
                                LEFT JOIN
                                    OverheadExpenses OE ON S.SaleID = OE.SaleID;;";
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                Sales_DataGridView.DataSource = dataTable; // dataGridView - это DataGridView на вашей форме
            }
        }
        private void DataTable_Sales_Update_Button_Click(object Sender, EventArgs e)
        {
            DataTable_Sales();
        }
        private void DataTable_Sales_Delete_Button_Click(object Sender, EventArgs e)
        {
            // Получаем ProductID записи, которую нужно удалить (например, из выбранной строки в DataGridView)
            DataGridViewRow selectedRow = Sales_DataGridView.SelectedRows[0];
            DataGridViewRow salesIDToDelete = Products_DataGridView.SelectedRows[0];
            int saleID = int.Parse(selectedRow.Cells["SaleID"].Value.ToString());
            sqlTransaction = sqlConnection.BeginTransaction();
            try
            {
                SqlCommand cmd = sqlConnection.CreateCommand();
                cmd.Transaction = sqlTransaction;
                cmd.CommandText = @"
                                    -- Удаляем запись из таблицы Warehouse
                                    DELETE FROM OverheadExpenses WHERE SaleID = @SaleID;
                                    DELETE FROM Sales WHERE SaleID = @SaleID;";

                cmd.Parameters.AddWithValue("@SaleID", saleID);
                cmd.ExecuteNonQuery();
                sqlTransaction.Commit();
                MessageBox.Show("Транзакция успешно выполнена.");

            }
            catch (Exception ex)
            {
                // Откатываем транзакцию в случае ошибки
                MessageBox.Show("Транзакция откатывается из-за ошибки: " + ex.Message);
                sqlTransaction.Rollback();
            }
            DataTable_Sales();
        }
        // ----------------------- ТАБЛИЦЫ ДАННЫХ. БРАК -----------------------
        private async void DataTable_DefectiveProducts()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                string query = @"
                                SELECT
                                    D.DefectID AS [DefectID],
                                    S.SupplierName AS [Supplier Name],
                                    P.ProductName AS [Product Name], 
                                    P.ProductPrice AS [Product Price], 
                                    D.DefectQuantity AS [Defect Quantity],
                                    P.DeliveryDate AS [Delivery Date],
                                    D.DefectDate AS [Defect Date]
                                FROM 
                                    DefectiveProducts D
                                JOIN 
                                    Products P ON D.ProductID = P.ProductID
                                JOIN 
                                    Suppliers S ON D.SupplierID = S.SupplierID;";
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                DefectiveProducts_DataGridView.DataSource = dataTable;
            }
        }
        private void DataTable_DefectiveProducts_Update_Button_Click(object Sender, EventArgs e)
        {
            DataTable_DefectiveProducts();
        }
        private void DataTable_DefectiveProducts_Delete_Button_Click(object Sender, EventArgs e)
        {
            DataGridViewRow selectedRow = DefectiveProducts_DataGridView.SelectedRows[0];
            int defectID = int.Parse(selectedRow.Cells["DefectID"].Value.ToString());
            sqlTransaction = sqlConnection.BeginTransaction();
            try
            {
                SqlCommand cmd = sqlConnection.CreateCommand();
                cmd.Transaction = sqlTransaction;
                cmd.CommandText = @"
                                    -- Удаляем запись из таблицы DefectiveProducts
                                    DELETE FROM DefectiveProducts WHERE DefectID = @DefectID;";

                cmd.Parameters.AddWithValue("@DefectID", defectID);
                cmd.ExecuteNonQuery();
                sqlTransaction.Commit();
                MessageBox.Show("Транзакция успешно выполнена.");

            }
            catch (Exception ex)
            {
                // Откатываем транзакцию в случае ошибки
                MessageBox.Show("Транзакция откатывается из-за ошибки: " + ex.Message);
                sqlTransaction.Rollback();
            }
            DataTable_DefectiveProducts();
        }
        // ----------------------- ЗАПРОСЫ -----------------------
        private void QueryLoadItems()
        {
            q1_TextBox.Text = "1. Получить перечень и общее число поставщиков определенной категории, поставляющих указанный вид товара, либо поставивших указанный товар в объеме, не менее заданного за определенный период.";
            q2_TextBox.Text = "2. Получить сведения о конкретном виде деталей: какими поставщиками поставляется, их расценки, время поставки.";
            q3_TextBox.Text = "3. Получить перечень и общее число покупателей, купивших указанный вид товара за некоторый период, либо сделавших покупку товара в объеме, не менее указанного.";
            q4_TextBox.Text = "4. Получить перечень, объем и номер ячейки для всех деталей, хранящихся на складе.";
            q5_TextBox.Text = "5. Вывести в порядке возрастания десять самых продаваемых деталей и десять самых \"дешевых\" поставщиков.";
            q6_TextBox.Text = "6. Получить среднее число продаж на месяц по любому виду деталей.";
            q7_TextBox.Text = "7. Получить долю товара конкретного поставщика в процентах, деньгах, единицах от всего оборота магазина прибыль магазина за указанный период.";
            q8_TextBox.Text = "8. Получить накладные расходы в процентах от объема продаж.";
            q9_TextBox.Text = "9. Получить перечень и общее количество непроданного товара на складе за определенный период (залежалого) и его объем от общего товара в процентах.";
            q10_TextBox.Text = "10. Получить перечень и общее количество бракованного товара, пришедшего за определенный период и список поставщиков, поставивших товар.";
            q11_TextBox.Text = "11. Получить перечень, общее количество и стоимость товара, реализованного за конкретный день.";
            q12_TextBox.Text = "12. Получить кассовый отчет за определенный период.";
            q13_TextBox.Text = "13. Получить инвентаризационную ведомость.";
            q14_TextBox.Text = "14. Получить скорость оборота денежных средств, вложенных в товар (как товар быстро продается).";
            q15_TextBox.Text = "15. Подсчитать сколько пустых ячеек на складе и сколько он сможет вместить товара.";
            q16_TextBox.Text = "16. Получить перечень и общее количество заявок от покупателей на ожидаемый товар, подсчитать на какую сумму даны заявки.";
            qCQ_TextBox.Text = "Введите произвольный запрос:";
            q1_DataGridView.Visible = false;
            q1_1_DataGridView.Visible = false;
            q1_Close_Button.Visible = false;
            q2_DataGridView.Visible = false;
            q2_Close_Button.Visible = false;
            q3_DataGridView.Visible = false;
            q3_Close_Button.Visible = false;
            q4_DataGridView.Visible = false;
            q4_Close_Button.Visible = false;
            q5_DataGridView.Visible = false;
            q5_1_DataGridView.Visible = false;
            q5_Close_Button.Visible = false;
            q6_DataGridView.Visible = false;
            q6_Close_Button.Visible = false;
            q7_DataGridView.Visible = false;
            q7_1_DataGridView.Visible = false;
            q7_Close_Button.Visible = false;
            q8_DataGridView.Visible = false;
            q8_Close_Button.Visible = false;
            q9_DataGridView.Visible = false;
            q9_Close_Button.Visible = false;
            q10_DataGridView.Visible = false;
            q10_Close_Button.Visible = false;
            q11_DataGridView.Visible = false;
            q11_Close_Button.Visible = false;
            q12_DataGridView.Visible = false;
            q12_Close_Button.Visible = false;
            q13_DataGridView.Visible = false;
            q13_Close_Button.Visible = false;
            q14_DataGridView.Visible = false;
            q14_Close_Button.Visible = false;
            q15_DataGridView.Visible = false;
            q15_Close_Button.Visible = false;
            q16_DataGridView.Visible = false;
            q16_Close_Button.Visible = false;
            qCQ_DataGridView.Visible = false;
            qCQ_Close_Button.Visible = false;
            
        }
        // ----------------------- ЗАПРОСЫ. ЗАПРОС #1 -----------------------
        private void Q1_SupplierCategory_ComboBox()
        {
            q1_SupplierCategory_ComboBox.Items.Clear();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT DISTINCT SupplierCategory FROM Suppliers";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string supplierCategory = reader["SupplierCategory"].ToString();
                                q1_SupplierCategory_ComboBox.Items.Add(supplierCategory);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке данных: " + ex.Message);
                }
            }
        }
        private void Q1_SupplierCategory_ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Q1_ProductName_ComboBox();
        }
        private void Q1_ProductName_ComboBox()
        {
            q1_ProductName_ComboBox.Items.Clear();
            string selectedSupplierCategory = q1_SupplierCategory_ComboBox.Text;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = @"
                                    SELECT P.ProductName
                                    FROM Products P
                                    JOIN Suppliers S ON P.SupplierID = S.SupplierID
                                    WHERE S.SupplierCategory = @SupplierCategory";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Добавляем параметр @SupplierName в запрос
                        command.Parameters.AddWithValue("@SupplierCategory", selectedSupplierCategory);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Добавляем каждый ProductName во второй ComboBox
                                string productName = reader["ProductName"].ToString();
                                q1_ProductName_ComboBox.Items.Add(productName);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке данных: " + ex.Message);
                }
            }
        }
        private async void q1_Execute_Button_Click(object sender, EventArgs e)
        {
            if (int.TryParse(q1_MinQuantity_TextBox.Text,out int minQuantity))
            {
                if (q1_SupplierCategory_ComboBox.Text != String.Empty &&
                q1_ProductName_ComboBox.Text != String.Empty &&
                q1_MinQuantity_TextBox.Text != String.Empty)
                {
                    string supplier_category = q1_SupplierCategory_ComboBox.Text;
                    string product_name = q1_ProductName_ComboBox.Text;
                    int min_quantity = int.Parse(q1_MinQuantity_TextBox.Text);
                    DateTime start_date = q1_StartDate_DateTimePicker.Value;
                    DateTime end_date = q1_EndDate_DateTimePicker.Value;
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        await connection.OpenAsync();
                        string firstQuery = @"
                               SELECT 
                            S.SupplierName
                        FROM 
                            Suppliers S
                        JOIN 
                            Products P ON S.SupplierID = P.SupplierID
                        JOIN 
                            Sales Sa ON P.ProductID = Sa.ProductID
                        WHERE 
                            S.SupplierCategory = @supplier_category
                            AND P.ProductName = @product_name
                            AND Sa.QuantitySold >= @min_quantity
                            AND Sa.SaleDate BETWEEN @start_date AND @end_date
                        GROUP BY 
                            S.SupplierName;";
                        SqlCommand cmd = new SqlCommand(firstQuery, connection);
                        cmd.Parameters.AddWithValue("@supplier_category", supplier_category);
                        cmd.Parameters.AddWithValue("@product_name", product_name);
                        cmd.Parameters.AddWithValue("@min_quantity", min_quantity);
                        cmd.Parameters.AddWithValue("@start_date", start_date);
                        cmd.Parameters.AddWithValue("@end_date", end_date);
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        DataTable firstDataTable = new DataTable();
                        adapter.Fill(firstDataTable);
                        q1_DataGridView.DataSource = firstDataTable;
                        string secondQuery = @"
                        SELECT 
                            COUNT(DISTINCT S.SupplierID) AS TotalSuppliers
                        FROM 
                            Suppliers S
                        JOIN 
                            Products P ON S.SupplierID = P.SupplierID
                        JOIN 
                            Sales Sa ON P.ProductID = Sa.ProductID
                        WHERE 
                            S.SupplierCategory = @supplier_category
                            AND P.ProductName = @product_name
                            AND Sa.QuantitySold >= @min_quantity
                            AND Sa.SaleDate BETWEEN @start_date AND @end_date;";
                        cmd = new SqlCommand(secondQuery, connection);
                        cmd.Parameters.AddWithValue("@supplier_category", supplier_category);
                        cmd.Parameters.AddWithValue("@product_name", product_name);
                        cmd.Parameters.AddWithValue("@min_quantity", min_quantity);
                        cmd.Parameters.AddWithValue("@start_date", start_date);
                        cmd.Parameters.AddWithValue("@end_date", end_date);
                        adapter = new SqlDataAdapter(cmd);
                        DataTable secondDataTable = new DataTable();
                        adapter.Fill(secondDataTable);
                        q1_1_DataGridView.DataSource = secondDataTable;
                    }
                    q1_SupplierCategory_ComboBox.Text = null;
                    q1_SupplierCategory_ComboBox.Items.Clear();
                    q1_ProductName_ComboBox.Text = null;
                    q1_ProductName_ComboBox.Items.Clear();
                    q1_MinQuantity_TextBox.Text = null;
                    q1_Panel.Visible = false;
                    q1_DataGridView.Visible = true;
                    q1_1_DataGridView.Visible = true;
                    q1_Close_Button.Visible = true;
                    Q1_SupplierCategory_ComboBox();
                }
                else MessageBox.Show("Все поля должны быть заполнены!");
            }
            else MessageBox.Show("Минимальнео число введено неверно. Повторите ввод.");
            
        }
        private void q1_Close_Button_Click(object sender, EventArgs e)
        {
            Q1_SupplierCategory_ComboBox();
            q1_Close_Button.Visible = false;
            q1_DataGridView.Visible = false;
            q1_1_DataGridView.Visible = false;
            q1_Panel.Visible = true;
        }
        // ----------------------- ЗАПРОСЫ. ЗАПРОС #2 -----------------------
        private void Q2_ProductName_Combobox()
        {
            q2_ProductName_ComboBox.Items.Clear();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT DISTINCT ProductName FROM Products";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string productName = reader["ProductName"].ToString();
                                q2_ProductName_ComboBox.Items.Add(productName);
                            }
                        }
                    }
                }
                catch (Exception ex) { 
                    MessageBox.Show("Ошибка при загрузке данных: " + ex.Message);
                }
            }
        }
        private async void q2_Execute_Button_Click(object sender, EventArgs e)
        {
            if (q2_ProductName_ComboBox.Text != String.Empty)
            {
                string product_name = q2_ProductName_ComboBox.Text;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"
                                SELECT 
                                    S.SupplierName AS [Supplier Name],
                                    P.ProductPrice AS [Product Price],
                                    P.DeliveryDate AS [Delivery Date]
                                FROM 
                                    Products P
                                JOIN 
                                    Suppliers S ON P.SupplierID = S.SupplierID
                                WHERE 
                                    P.ProductName = @product_name;
";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@product_name", product_name);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    q2_DataGridView.DataSource = dataTable;
                }
                q2_ProductName_ComboBox.Text = null;
                q2_ProductName_ComboBox.Items.Clear();
                q2_Panel.Visible = false;
                q2_DataGridView.Visible = true;
                q2_Close_Button.Visible = true;
                Q2_ProductName_Combobox();
            }
            else
            {
                MessageBox.Show("Все поля должны быть заполнены!");
            }
            
        }
        private void q2_Close_Button_Click(object sender, EventArgs e)
        {
            Q2_ProductName_Combobox();
            q2_Close_Button.Visible = false;
            q2_DataGridView.Visible = false;
            q2_Panel.Visible = true;
        }
        // ----------------------- ЗАПРОСЫ. ЗАПРОС #3 -----------------------
        private void Q3_ProductName_ComboBox()
        {
            q3_ProductName_ComboBox.Items.Clear();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT ProductName FROM Products";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string supplierCategory = reader["ProductName"].ToString();
                                q3_ProductName_ComboBox.Items.Add(supplierCategory);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке данных: " + ex.Message);
                }
            }
        }
        private async void q3_Execute_Button_Click(object sender, EventArgs e)
        {
            if (int.TryParse(q3_MinQuantity_TextBox.Text.ToString(), out int minQuantity))
            {
                if (q3_ProductName_ComboBox.Text != String.Empty &&
                q3_MinQuantity_TextBox.Text != String.Empty)
                {
                    string product_name = q3_ProductName_ComboBox.Text;
                    DateTime start_date = q3_StartDate_DateTimePicker.Value;
                    DateTime end_date = q3_EndDate_DateTimePicker.Value;
                    int min_quantity = int.Parse(q3_MinQuantity_TextBox.Text);
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        await connection.OpenAsync();
                        string query = @"
                                SELECT 
                                    C.CustomerName AS [Customer Name], 
                                    COUNT(DISTINCT C.CustomerID) AS [Total Customers]
                                FROM 
                                    Customers C
                                JOIN 
                                    Sales S ON C.CustomerID = S.CustomerID
                                JOIN 
                                    Products P ON S.ProductID = P.ProductID
                                WHERE 
                                    P.ProductName = @product_name
                                    AND S.SaleDate BETWEEN @start_date AND @end_date
                                    AND S.QuantitySold >= @min_quantity
                                GROUP BY 
                                    C.CustomerName;";
                        SqlCommand cmd = new SqlCommand(query, connection);

                        cmd.Parameters.AddWithValue("@product_name", product_name);
                        cmd.Parameters.AddWithValue("@start_date", start_date);
                        cmd.Parameters.AddWithValue("@end_date", end_date);
                        cmd.Parameters.AddWithValue("@min_quantity", min_quantity);
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        q3_DataGridView.DataSource = dataTable;
                    }
                    q3_ProductName_ComboBox.Text = null;
                    q3_ProductName_ComboBox.Items.Clear();
                    q3_MinQuantity_TextBox.Text = null;
                    q3_Panel.Visible = false;
                    q3_DataGridView.Visible = true;
                    q3_Close_Button.Visible = true;
                    Q3_ProductName_ComboBox();
                }
                else
                {
                    MessageBox.Show("Все поля должны быть заполнены!");
                }
            }
            else
            {
                MessageBox.Show("Минимальнео число введено неверно. Повторите ввод.");
            }
            
            
        }

        private void q3_Close_Button_Click(object sender, EventArgs e)
        {
            q3_Close_Button.Visible = false;
            q3_DataGridView.Visible = false;
            q3_Panel.Visible = true;
        }
        // ----------------------- ЗАПРОСЫ. ЗАПРОС #4 -----------------------
        private async void q4_Execute_Button_Click(object sender, EventArgs e)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                string query = @"
                                SELECT 
                                P.ProductName AS [Product Name], 
                                W.Quantity AS [Quantity], 
                                W.CellNumber AS [Cell Number]
                            FROM 
                                Warehouse W
                            JOIN 
                                Products P ON W.ProductID = P.ProductID;";
                SqlCommand cmd = new SqlCommand(query, connection);

                
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                q4_DataGridView.DataSource = dataTable;
            }
            q4_Panel.Visible = false;
            q4_DataGridView.Visible = true;
            q4_Close_Button.Visible = true;
        }
        private void q4_Close_Button_Click(object sender, EventArgs e)
        {
            q4_Close_Button.Visible = false;
            q4_DataGridView.Visible = false;
            q4_Panel.Visible = true;
        }
        // ----------------------- ЗАПРОСЫ. ЗАПРОС #5 -----------------------
        private async void q5_Execute_Button_Click(object sender, EventArgs e)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                string firstQuery = @"
                                -- Десять самых продаваемых деталей
                                SELECT TOP 10 
                                    P.ProductName AS [Product Name], 
                                    SUM(S.QuantitySold) AS [Total Sold]
                                FROM 
                                    Sales S
                                JOIN 
                                    Products P ON S.ProductID = P.ProductID
                                GROUP BY 
                                    P.ProductName
                                ORDER BY 
                                    [Total Sold] DESC;";
                SqlCommand cmd = new SqlCommand(firstQuery, connection);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable firstDataTabler = new DataTable();
                adapter.Fill(firstDataTabler);
                q5_DataGridView.DataSource = firstDataTabler;
                string secondQuery = @"
                                -- Десять самых ""дешевых"" поставщиков
                                SELECT TOP 10 
                                    S.SupplierName AS [Supplier Name], 
                                    MIN(P.ProductPrice) AS [Minimum Price]
                                FROM 
                                    Suppliers S
                                JOIN 
                                    Products P ON S.SupplierID = P.SupplierID
                                GROUP BY 
                                    S.SupplierName
                                ORDER BY 
                                    [Minimum Price] ASC;";
                cmd = new SqlCommand(secondQuery, connection);
                adapter = new SqlDataAdapter(cmd);
                DataTable secondDataTable = new DataTable();
                adapter.Fill(secondDataTable);
                q5_1_DataGridView.DataSource = secondDataTable;
            }
            q5_Panel.Visible = false;
            q5_DataGridView.Visible = true;
            q5_1_DataGridView.Visible = true;
            q5_Close_Button.Visible = true;
        }
        private void q5_Close_Button_Click(object sender, EventArgs e)
        {
            q5_Close_Button.Visible = false;
            q5_DataGridView.Visible = false;
            q5_1_DataGridView.Visible = false;
            q5_Panel.Visible = true;
        }
        // ----------------------- ЗАПРОСЫ. ЗАПРОС #6 -----------------------
        private void Q6_ProductName_ComboBox()
        {
            q6_ProductName_ComboBox.Items.Clear();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT ProductName FROM Products";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string productName = reader["ProductName"].ToString();
                                q6_ProductName_ComboBox.Items.Add(productName);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке данных: " + ex.Message);
                }
            }
        }
        private async void q6_Execute_Button_Click(object sender, EventArgs e)
        {
            if (q6_ProductName_ComboBox.Text != String.Empty)
            {
                string product_name = q6_ProductName_ComboBox.Text;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"
                                SELECT 
                                    P.ProductName AS [Product Name], 
                                    AVG(S.QuantitySold) AS [AvgMonthlySales]
                                FROM 
                                    Sales S
                                JOIN 
                                    Products P ON S.ProductID = P.ProductID
                                WHERE 
                                    P.ProductName = @product_name
                                GROUP BY 
                                    P.ProductName, 
                                    YEAR(S.SaleDate), 
                                    MONTH(S.SaleDate);
";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@product_name", product_name);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    q6_DataGridView.DataSource = dataTable;
                }
                q6_Panel.Visible = false;
                q6_DataGridView.Visible = true;
                q6_Close_Button.Visible = true;
            }
            else
            {
                MessageBox.Show("Все поля должны быть заполнены!");
            }
        }
        private void q6_Close_Button_Click(object sender, EventArgs e)
        {
            Q6_ProductName_ComboBox();
            q6_Close_Button.Visible = false;
            q6_DataGridView.Visible = false;
            q6_Panel.Visible = true;
        }
        // ----------------------- ЗАПРОСЫ. ЗАПРОС #7 -----------------------
        private void Q7_SupplierName_ComboBox()
        {
            q7_SupplierName_ComboBox.Items.Clear();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT SupplierName FROM Suppliers";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string supplierName = reader["SupplierName"].ToString();
                                q7_SupplierName_ComboBox.Items.Add(supplierName);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке данных: " + ex.Message);
                }
            }
        }
        private async void q7_Execute_Button_Click(object sender, EventArgs e)
        {
            if (q7_SupplierName_ComboBox.Text != String.Empty)
            {
                string supplier_name = q7_SupplierName_ComboBox.Text;
                DateTime start_date = q7_StartDate_DateTimePicker.Value;
                DateTime end_date = q7_EndDate_DateTimePicker.Value;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    string firstQuery = @"
                                SELECT 
                                    S.SupplierName AS [Supplier Name],
                                    CAST(ROUND(SUM(Sa.QuantitySold) * 100.0 / 
                                    (SELECT SUM(QuantitySold) FROM Sales WHERE SaleDate BETWEEN @start_date AND @end_date), 2) AS DECIMAL(10, 2)) AS [Percentage Units],
                                    CAST(ROUND(SUM(Sa.QuantitySold), 2) AS DECIMAL(10, 2)) AS [Total Units]
                                FROM 
                                    Suppliers S
                                JOIN 
                                    Products P ON S.SupplierID = P.SupplierID
                                JOIN 
                                    Sales Sa ON P.ProductID = Sa.ProductID
                                WHERE 
                                    S.SupplierName = @supplier_name
                                    AND Sa.SaleDate BETWEEN @start_date AND @end_date
                                GROUP BY 
                                    S.SupplierName;";
                    SqlCommand cmd = new SqlCommand(firstQuery, connection);
                    cmd.Parameters.AddWithValue("@supplier_name", supplier_name);
                    cmd.Parameters.AddWithValue("@start_date", start_date);
                    cmd.Parameters.AddWithValue("@end_date", end_date);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable firstDataTable = new DataTable();
                    adapter.Fill(firstDataTable);
                    q7_DataGridView.DataSource = firstDataTable;
                    string secondQuery = @"
                                -- Доля в деньгах
                                SELECT 
                                    S.SupplierName AS [Supplier Name],
                                     CAST(ROUND(SUM(Sa.SalePrice * Sa.QuantitySold) * 100.0 / 
                                    (SELECT SUM(SalePrice * QuantitySold) FROM Sales WHERE SaleDate BETWEEN @start_date AND @end_date), 2) AS DECIMAL(10, 2)) AS [Percentage Money],
                                    CAST(ROUND(SUM(Sa.SalePrice * Sa.QuantitySold), 2) AS DECIMAL(10, 2)) AS [Total Money]
                                FROM 
                                    Suppliers S
                                JOIN 
                                    Products P ON S.SupplierID = P.SupplierID
                                JOIN 
                                    Sales Sa ON P.ProductID = Sa.ProductID
                                WHERE 
                                    S.SupplierName = @supplier_name
                                    AND Sa.SaleDate BETWEEN @start_date AND @end_date
                                GROUP BY 
                                    S.SupplierName;";
                    cmd = new SqlCommand(secondQuery, connection);
                    cmd.Parameters.AddWithValue("@supplier_name", supplier_name);
                    cmd.Parameters.AddWithValue("@start_date", start_date);
                    cmd.Parameters.AddWithValue("@end_date", end_date);
                    adapter = new SqlDataAdapter(cmd);
                    DataTable secondDataTable = new DataTable();
                    adapter.Fill(secondDataTable);
                    q7_1_DataGridView.DataSource = secondDataTable;
                }
                q7_SupplierName_ComboBox.Items.Clear();
                q7_SupplierName_ComboBox.Text = null;
                q7_Panel.Visible = false;
                q7_DataGridView.Visible = true;
                q7_1_DataGridView.Visible = true;
                q7_Close_Button.Visible = true;
            }
            else
            {
                MessageBox.Show("Все поля должны быть заполнены!");
            }
        }
        private void q7_Close_Button_Click(object sender, EventArgs e)
        {
            Q7_SupplierName_ComboBox();
            q7_Close_Button.Visible = false;
            q7_DataGridView.Visible = false;
            q7_1_DataGridView.Visible = false;
            q7_Panel.Visible = true;
        }
        // ----------------------- ЗАПРОСЫ. ЗАПРОС #8 -----------------------
        private async void q8_Execute_Button_Click(object sender, EventArgs e)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                string query = @"
                                SELECT 
                                    CAST(ROUND(SUM(O.OverheadPercentage * S.QuantitySold * S.SalePrice / 100.0), 2) AS DECIMAL(10, 2)) AS TotalOverheadExpenses,
                                    CAST(ROUND(SUM(S.QuantitySold * S.SalePrice), 2) AS DECIMAL(10, 2)) AS TotalSales,
                                    CAST(ROUND(SUM(O.OverheadPercentage * S.QuantitySold * S.SalePrice / 100.0) * 100.0 / SUM(S.QuantitySold * S.SalePrice), 2) AS DECIMAL(10, 2)) AS OverheadPercentage
                                FROM 
                                    OverheadExpenses O
                                JOIN 
                                    Sales S ON O.SaleID = S.SaleID;";
                SqlCommand cmd = new SqlCommand(query, connection);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                q8_DataGridView.DataSource = dataTable;
            }
            q8_Panel.Visible = false;
            q8_DataGridView.Visible = true;
            q8_Close_Button.Visible = true;
        }
        private void q8_Close_Button_Click(object sender, EventArgs e)
        {
            q8_Close_Button.Visible = false;
            q8_DataGridView.Visible = false;
            q8_Panel.Visible = true;
        }
        // ----------------------- ЗАПРОСЫ. ЗАПРОС #9 -----------------------
        private async void q9_Execute_Button_Click(object sender, EventArgs e)
        {
            DateTime start_date = q9_StartDate_dateTimePicker.Value;
            DateTime end_date = q9_EndDate_DateTimePicker.Value;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                string query = @"
                                SELECT 
                                    P.ProductName AS [Product Name], 
                                    CAST(SUM(W.Quantity) AS DECIMAL(10, 2)) AS [Total Unsold Quantity],
                                    CAST(ROUND(SUM(W.Quantity) * 100.0 / (SELECT SUM(Quantity) FROM Warehouse), 2) AS DECIMAL(10, 2)) AS [Percentage Of Total]
                                FROM 
                                    Warehouse W
                                JOIN 
                                    Products P ON W.ProductID = P.ProductID
                                WHERE 
                                    P.DeliveryDate BETWEEN @start_date AND @end_Date
                                GROUP BY 
                                    P.ProductName;";
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@start_date", start_date);
                cmd.Parameters.AddWithValue("@end_date", end_date);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                q9_DataGridView.DataSource = dataTable;
            }
            q9_Panel.Visible = false;
            q9_DataGridView.Visible = true;
            q9_Close_Button.Visible = true;
        }
        private void q9_Close_Button_Click(object sender, EventArgs e)
        {
            q9_Close_Button.Visible = false;
            q9_DataGridView.Visible = false;
            q9_Panel.Visible = true;
        }
        // ----------------------- ЗАПРОСЫ. ЗАПРОС #10 -----------------------
        private async void q10_Execute_Button_Click(object sender, EventArgs e)
        {
            DateTime start_date = q10_StartDate_DateTimePicker.Value;
            DateTime end_date = q10_EndDate_DateTimePicker.Value;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                string query = @"
                                SELECT 
                                    P.ProductName AS [Product Name], 
                                    S.SupplierName AS [Supplier Name],
                                    CAST(SUM(D.DefectQuantity) AS DECIMAL(10, 2)) AS [Total Defective Quantity]
                                FROM 
                                    DefectiveProducts D
                                JOIN 
                                    Products P ON D.ProductID = P.ProductID
                                JOIN 
                                    Suppliers S ON D.SupplierID = S.SupplierID
                                WHERE 
                                    D.DefectDate BETWEEN @start_date AND @end_date
                                GROUP BY 
                                    P.ProductName, 
                                    S.SupplierName;";
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@start_date", start_date);
                cmd.Parameters.AddWithValue("@end_date", end_date);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                q10_DataGridView.DataSource = dataTable;
            }
            q10_Panel.Visible = false;
            q10_DataGridView.Visible = true;
            q10_Close_Button.Visible = true;
        }
        private void q10_Close_Button_Click(object sender, EventArgs e)
        {
            q10_Close_Button.Visible = false;
            q10_DataGridView.Visible = false;
            q10_Panel.Visible = true;
        }
        // ----------------------- ЗАПРОСЫ. ЗАПРОС #11 -----------------------
        private async void q11_Execute_Button_Click(object sender, EventArgs e)
        {
            DateTime sale_date = q11_SaleDate_DateTimePicker.Value;
            string saleDateString = sale_date.ToString("dd.MM.yyyy");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                string query = @"
                                SELECT 
                                    P.ProductName,
                                    SUM(S.QuantitySold) AS TotalQuantity,
                                    SUM(S.QuantitySold * S.SalePrice) AS TotalSales
                                FROM 
                                    Sales S
                                JOIN 
                                    Products P ON S.ProductID = P.ProductID
                                WHERE 
                                    S.SaleDate = @sale_date
                                GROUP BY 
                                    P.ProductName;";
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@sale_date", DateTime.Parse(saleDateString));
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                q11_DataGridView.DataSource = dataTable;
            }
            q11_Panel.Visible = false;
            q11_DataGridView.Visible = true;
            q11_Close_Button.Visible = true;
        }
        private void q11_Close_Button_Click(object sender, EventArgs e)
        {
            q11_Close_Button.Visible = false;
            q11_DataGridView.Visible = false;
            q11_Panel.Visible = true;
        }
        // ----------------------- ЗАПРОСЫ. ЗАПРОС #12 -----------------------
        private async void q12_Execute_Button_Click(object sender, EventArgs e)
        {
            DateTime start_date = q12_StartDate_DdateTimePicker.Value;
            DateTime end_date = q12_EndDate_DateTimePicker.Value;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                string query = @"
                                SELECT 
                                    CAST(ROUND(SUM(S.SalePrice * S.QuantitySold), 2) AS DECIMAL(10, 2)) AS [Total Sales],
                                    CAST(ROUND(SUM(O.OverheadPercentage * S.SalePrice * S.QuantitySold / 100.0), 2) AS DECIMAL(10, 2)) AS [Total Overhead Expenses],
                                    CAST(ROUND(SUM(S.SalePrice * S.QuantitySold) - SUM(O.OverheadPercentage * S.SalePrice * S.QuantitySold / 100.0), 2) AS DECIMAL(10, 2)) AS [Net Profit]
                                FROM 
                                    Sales S
                                JOIN 
                                    OverheadExpenses O ON S.SaleID = O.SaleID
                                WHERE 
                                    S.SaleDate BETWEEN @start_date AND @end_date;";
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@start_date", start_date);
                cmd.Parameters.AddWithValue("@end_date", end_date);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                q12_DataGridView.DataSource = dataTable;
            }
            q12_Panel.Visible = false;
            q12_DataGridView.Visible = true;
            q12_Close_Button.Visible = true;
        }
        private void q12_Close_Button_Click(object sender, EventArgs e)
        {
            q12_Close_Button.Visible = false;
            q12_DataGridView.Visible = false;
            q12_Panel.Visible = true;
        }
        // ----------------------- ЗАПРОСЫ. ЗАПРОС #13 -----------------------
        private async void q13_Execute_Button_Click(object sender, EventArgs e)
        {

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                string query = @"
                                SELECT 
                                    P.ProductName,
                                    SUM(W.Quantity) AS TotalQuantity
                                FROM 
                                    Warehouse W
                                JOIN 
                                    Products P ON W.ProductID = P.ProductID
                                GROUP BY 
                                    P.ProductName;";
                SqlCommand cmd = new SqlCommand(query, connection);

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                q13_DataGridView.DataSource = dataTable;
            }
            
            q13_Panel.Visible = false;
            q13_DataGridView.Visible = true;
            q13_Close_Button.Visible = true;
        }
        private void q13_Close_Button_Click(object sender, EventArgs e)
        {
            q13_Close_Button.Visible = false;
            q13_DataGridView.Visible = false;
            q13_Panel.Visible = true;
        }
        // ----------------------- ЗАПРОСЫ. ЗАПРОС #14 -----------------------
        private void Q14_ProductName_ComboBox()
        {
            q14_ProductName_ComboBox.Items.Clear();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT ProductName FROM Products";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string productName = reader["ProductName"].ToString();
                                q14_ProductName_ComboBox.Items.Add(productName);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке данных: " + ex.Message);
                }
            }
        }
        private async void q14_Execute_Button_Click(object sender, EventArgs e)
        {
            if (q14_ProductName_ComboBox.Text != String.Empty)
            {
                string product_name = q14_ProductName_ComboBox.Text;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"
                                    SELECT 
                                        P.ProductName AS [Product Name], 
                                        DATEDIFF(DAY, MIN(S.SaleDate), MAX(S.SaleDate)) / COUNT(DISTINCT S.SaleDate) AS [Sales Cycle Days]
                                    FROM 
                                        Sales S
                                    JOIN 
                                        Products P ON S.ProductID = P.ProductID
                                    WHERE 
                                        P.ProductName = @product_name
                                    GROUP BY 
                                        P.ProductName;";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@product_name", product_name);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    q14_DataGridView.DataSource = dataTable;
                }
                q14_Panel.Visible = false;
                q14_DataGridView.Visible = true;
                q14_Close_Button.Visible = true;
            }
            else
            {
                MessageBox.Show("Все поля должны быть заполнены!");
            }
            
        }
        private void q14_Close_Button_Click(object sender, EventArgs e)
        {
            Q14_ProductName_ComboBox();
            q14_Close_Button.Visible = false;
            q14_DataGridView.Visible = false;
            q14_Panel.Visible = true;
        }
        // ----------------------- ЗАПРОСЫ. ЗАПРОС #15 -----------------------
        private async void q15_Execute_Button_Click(object sender, EventArgs e)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                string query = @"
                                SELECT 
                                    COUNT(*) AS [Total Cells],
                                    COUNT(CASE WHEN W.Quantity = 0 THEN 1 END) AS [Empty Cells],
                                    SUM(W.Quantity) AS [Total Stored Items]
                                FROM 
                                    Warehouse W;";
                SqlCommand cmd = new SqlCommand(query, connection);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                q15_DataGridView.DataSource = dataTable;
            }
            q15_Panel.Visible = false;
            q15_DataGridView.Visible = true;
            q15_Close_Button.Visible = true;
        }
        private void q15_Close_Button_Click(object sender, EventArgs e)
        {
            q15_Close_Button.Visible = false;
            q15_DataGridView.Visible = false;
            q15_Panel.Visible = true;
        }
        // ----------------------- ЗАПРОСЫ. ЗАПРОС #16 -----------------------
        private async void q16_Execute_Button_Click(object sender, EventArgs e)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                string query = @"
                                SELECT 
                                    P.ProductName AS [Product Name], 
                                    SUM(O.OrderQuantity) AS [Total Order Quantity],
                                    SUM(O.OrderQuantity * P.ProductPrice) AS [Total Order Amount]
                                FROM 
                                    CustomerOrders O
                                JOIN 
                                    Products P ON O.ProductID = P.ProductID
                                GROUP BY 
                                    P.ProductName;";
                SqlCommand cmd = new SqlCommand(query, connection);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                q16_DataGridView.DataSource = dataTable;
            }
            q16_Panel.Visible = false;
            q16_DataGridView.Visible = true;
            q16_Close_Button.Visible = true;
        }
        private void q16_Close_Button_Click(object sender, EventArgs e)
        {
            q16_Close_Button.Visible = false;
            q16_DataGridView.Visible = false;
            q16_Panel.Visible = true;
        }
        // ----------------------- ЗАПРОСЫ. ЗАПРОС #CQ -----------------------
        private void qCQ_Execute_Button_Click(object sender, EventArgs e)
        {
            string query = qCQ_Query_TextBox.Text;
            try
            {
                SqlCommand cmd = sqlConnection.CreateCommand();
                cmd.CommandText = @query.ToString();                
                cmd.ExecuteNonQuery();
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                qCQ_DataGridView.DataSource = dataTable;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Транзакция откатывается из-за ошибки: " + ex.Message);
                sqlTransaction.Rollback();
            }
            qCQ_Panel.Visible = false;
            qCQ_DataGridView.Visible = true;
            qCQ_Close_Button.Visible = true;
        }
        private void qCQ_Close_Button_Click(object sender, EventArgs e)
        {
            qCQ_Close_Button.Visible = false;
            qCQ_DataGridView.Visible = false;
            qCQ_Panel.Visible = true;
        }        
    }
}
