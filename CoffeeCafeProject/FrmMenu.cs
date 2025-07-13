using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq.Expressions;
using System.Windows.Forms;

namespace CoffeeCafeProject
{
    public partial class FrmMenu : Form
    {
        byte[] menuImage;
        public FrmMenu()
        {
            InitializeComponent();
        }

        private Image convertByteArrayToImage(byte[] byteArrayIn)
        {
            if (byteArrayIn == null || byteArrayIn.Length == 0)
            {
                return null;
            }
            try
            {
                using (MemoryStream ms = new MemoryStream(byteArrayIn))
                {
                    return Image.FromStream(ms);
                }
            }
            catch (ArgumentException ex)
            {
                // อาจเกิดขึ้นถ้า byte array ไม่ใช่ข้อมูลรูปภาพที่ถูกต้อง
                Console.WriteLine("Error converting byte array to image: " + ex.Message);
                return null;
            }
        }

        private void getAllMenuToListView()
        {
            //กำหนด Connect String เพื่อติดต่อฐานข้อมูล
            //string connectionString = @"Server=SATJATUM\SQLEXPRESS01;Database=coffee_cafe_db;Trusted_Connection=True";
            using (SqlConnection sqlConnection = new SqlConnection(ConfigDb.connectionString))
            {
                try
                {
                    sqlConnection.Open();

                    string strSQL = "SELECT menuId, menuName, menuPrice, menuImage FROM menu_tb";

                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter(strSQL, sqlConnection))
                    {
                        //เอาข้อมูลที่ได้จาก strSQL เป็นก้อนใน dataAdapter มาทำให้เป็นตารางโดยใส่ไว้ใน DataTable
                        DataTable dataTable = new DataTable();
                        dataAdapter.Fill(dataTable);

                        //ตั้งค่า ListView
                        lvShowAllMenu.Items.Clear();
                        lvShowAllMenu.Columns.Clear();
                        lvShowAllMenu.FullRowSelect = true;
                        lvShowAllMenu.View = View.Details;

                        if (lvShowAllMenu.SmallImageList == null)
                        {
                            lvShowAllMenu.SmallImageList = new ImageList();
                            lvShowAllMenu.SmallImageList.ImageSize = new Size(50, 50);
                            lvShowAllMenu.SmallImageList.ColorDepth = ColorDepth.Depth32Bit;
                        }
                        lvShowAllMenu.SmallImageList.Images.Clear();

                        lvShowAllMenu.Columns.Add("รูปเมนู", 80, HorizontalAlignment.Left);
                        lvShowAllMenu.Columns.Add("รหัสเมนู", 100, HorizontalAlignment.Left);
                        lvShowAllMenu.Columns.Add("ชื่อเมนู", 150, HorizontalAlignment.Left);
                        lvShowAllMenu.Columns.Add("ราคาเมนู", 100, HorizontalAlignment.Left);

                        foreach (DataRow dataRow in dataTable.Rows)
                        {
                            ListViewItem item = new ListViewItem(); //สร้าง ITem เพื่อเก็บข้อมูลในแต่ละรายการ
                            //เอารูปใส่ใน Item
                            Image menuImage = null;
                            if (dataRow["menuImage"] != DBNull.Value)
                            {
                                byte[] imgByte = (byte[])dataRow["menuImage"];
                                //แปลงข้อมูลรูปจากฐานข้อมูล Binary ให้เป็นรูป
                                menuImage = convertByteArrayToImage(imgByte);
                            }
                            string imageKey = null;
                            if (menuImage != null)
                            {
                                imageKey = $"menu_{dataRow["menuId"]}";
                                lvShowAllMenu.SmallImageList.Images.Add(imageKey, menuImage);
                                item.ImageKey = imageKey;
                            }
                            else
                            {
                                item.ImageIndex = -1;
                            }
                            // เอาแต่ละรายการใส่ใน Item
                            item.SubItems.Add(dataRow["menuId"].ToString());
                            item.SubItems.Add(dataRow["menuName"].ToString());
                            item.SubItems.Add(dataRow["menuPrice"].ToString());

                            //เอาข้อมูลใน Item 
                            lvShowAllMenu.Items.Add(item);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("พบข้อผิดพลาด กรุณาลองใหม่หรือติดต่อ IT : " + ex.Message);
                }
            }
        }

        private void FrmMenu_Load(object sender, EventArgs e)
        {
            getAllMenuToListView();
            pbMenuImage.Image = null;
            menuImage = null;
            tbMenuId.Clear();
            tbMenuName.Clear();
            tbMenuPrice.Clear();
            btSave.Enabled = true;
            btUpdate.Enabled = false;
            btDelete.Enabled = false;
        }

        private byte[] convertImageToByteArray(Image image, ImageFormat imageFormat)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, imageFormat);
                return ms.ToArray();
            }
        }

        private void btSelectMenuImage_Click(object sender, EventArgs e)
        {
            //เปิด File Dialog  ให้เลือกรูปโดยฟิวเตอร์เฉพาะไฟล์ jpg/png
            //แล้วนำรูปทื่เลือกไปแสดงที่ pbMenuImage
            //แล้วแปลงเป็นร Binary/Byte เก็บในตัวแปรเพื่อเอาไว้บันทึก DB
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = @"C:\";
            openFileDialog.Filter = "Image File (*.jpg;*.png)|*.jpg;*.png";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                //เอารูปที่เลือกไปแสดงที่ pcbProImage
                pbMenuImage.Image = Image.FromFile(openFileDialog.FileName);
                //ตรวจสอบ Format ของรูป แล้วส่งรูปไปแปลงเป็น Binary/Byte เก็บในตัวแปร
                if (pbMenuImage.Image.RawFormat == ImageFormat.Jpeg)
                {
                    menuImage = convertImageToByteArray(pbMenuImage.Image, ImageFormat.Jpeg);

                }
                else
                {
                    menuImage = convertImageToByteArray(pbMenuImage.Image, ImageFormat.Png);
                }
            }
        }
        private void ShowWarningMSG(string msg)
        {

            MessageBox.Show(msg, "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        }

        private void btSave_Click(object sender, EventArgs e)
        {
            if (menuImage == null)
            {
                ShowWarningMSG("กรุณาเลือกรูปเมนู");
            }
            else if (tbMenuName.Text.Trim() == "")
            {
                ShowWarningMSG("กรุณากรอกชื่อเมนู");
            }
            else if (tbMenuPrice.Text.Trim() == "")
            {
                ShowWarningMSG("กรุณากรอกราคาเมนู");
            }
            else
            {
                //string connectionString = @"Server=SATJATUM\SQLEXPRESS01;Database=coffee_cafe_db;Trusted_Connection=True";

                using (SqlConnection sqlConnection = new SqlConnection(ConfigDb.connectionString))
                {
                    try
                    {
                        sqlConnection.Open();

                        //เช็คเมนูถ้ามีจำนวนเมนูมากกว่า 10 
                        string countSql = "SELECT COUNT(*) FROM menu_tb";
                        using (SqlCommand countCommand = new SqlCommand(countSql, sqlConnection))
                        {
                            int rowCount = (int)countCommand.ExecuteScalar();
                            if (rowCount == 10)
                            {
                                ShowWarningMSG("ไม่สามารถเพิ่มเมนูได้ เนื่องจากมีเมนูได้เแค่ 10");
                                return;
                            }
                        }

                        SqlTransaction sqlTransaction = sqlConnection.BeginTransaction();


                        string strSQL = "INSERT INTO menu_tb (menuName, menuPrice, menuImage) " +
                                         "VALUES (@menuName, @menuPrice, @menuImage)";

                        using (SqlCommand sqlCommand = new SqlCommand(strSQL, sqlConnection, sqlTransaction))
                        {
                            sqlCommand.Parameters.Add("@menuName", SqlDbType.NVarChar, 100).Value = tbMenuName.Text;
                            sqlCommand.Parameters.Add("@menuPrice", SqlDbType.Float).Value = float.Parse(tbMenuPrice.Text);
                            sqlCommand.Parameters.Add("@menuImage", SqlDbType.Image).Value = menuImage;

                            sqlCommand.ExecuteNonQuery();
                            sqlTransaction.Commit();


                            MessageBox.Show("บันทึกเรียบร้อย", "ผลการทำงาน", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            getAllMenuToListView();
                            menuImage = null;
                            pbMenuImage.Image = null;
                            tbMenuId.Clear();
                            tbMenuName.Clear();
                            tbMenuPrice.Clear();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("พบข้อผิดพลาด กรุณาลองใหม่หรือติดต่อ IT : " + ex.Message);

                    }
                }
            }

        }

        private void tbMenuPrice_KeyPress(object sender, KeyPressEventArgs e)
        {
            // อนุญาตให้กดปุ่ม backspace
            if (char.IsControl(e.KeyChar))
                return;

            // ตรวจสอบว่าเป็นตัวเลขหรือไม่
            if (char.IsDigit(e.KeyChar))
                return;

            // ตรวจสอบว่าเป็นจุดทศนิยมหรือไม่ และยังไม่มีจุดใน textbox
            if (e.KeyChar == '.' && !((TextBox)sender).Text.Contains("."))
                return;

            // ถ้าไม่ใช่ตัวเลข หรือ จุดที่มากกว่าหนึ่งจุด ให้ยกเลิกการพิมพ์
            e.Handled = true;
        }

        private void lvShowAllMenu_ItemActivate(object sender, EventArgs e)
        {
            //เอาข้อมูลของรายการที่เลือกไปแสดงที่หน้าจอ 
            tbMenuId.Text = lvShowAllMenu.SelectedItems[0].SubItems[1].Text;
            tbMenuName.Text = lvShowAllMenu.SelectedItems[0].SubItems[2].Text;
            tbMenuPrice.Text = lvShowAllMenu.SelectedItems[0].SubItems[3].Text;

            var item = lvShowAllMenu.SelectedItems[0];
            if (!string.IsNullOrEmpty(item.ImageKey) && lvShowAllMenu.SmallImageList.Images.ContainsKey(item.ImageKey))
            {
                pbMenuImage.Image = lvShowAllMenu.SmallImageList.Images[item.ImageKey];
            }
            else
            {
                pbMenuImage.Image = null;
            }

            btSave.Enabled = false;
            btUpdate.Enabled = true;
            btDelete.Enabled = true;

        }

        private void btUpdate_Click(object sender, EventArgs e)
        {
            if (tbMenuName.Text.Trim() == "")
            {
                ShowWarningMSG("กรุณากรอกชื่อเมนู");
            }
            else if (tbMenuPrice.Text.Trim() == "")
            {
                ShowWarningMSG("กรุณากรอกราคาเมนู");
            }
            else
            {
                //string connectionString = @"Server=SATJATUM\SQLEXPRESS01;Database=coffee_cafe_db;Trusted_Connection=True";

                using (SqlConnection sqlConnection = new SqlConnection(ConfigDb.connectionString))
                {
                    try
                    {
                        sqlConnection.Open();

                        SqlTransaction sqlTransaction = sqlConnection.BeginTransaction();


                        string strSQL = "";
                        if (menuImage == null)
                        {
                            // ถ้าไม่มีการเปลี่ยนรูป ให้ไม่อัพเดทคอลัมน์ menuImage
                            strSQL = "UPDATE menu_tb SET menuName = @menuName, menuPrice = @menuPrice WHERE menuId = @menuId";
                        }
                        else
                        {
                            // ถ้ามีการเปลี่ยนรูป ให้ทำการอัพเดทคอลัมน์ menuImage ด้วย
                            strSQL = "UPDATE menu_tb SET menuName = @menuName, menuPrice = @menuPrice, menuImage = @menuImage WHERE menuId = @menuId";
                        }

                        using (SqlCommand sqlCommand = new SqlCommand(strSQL, sqlConnection, sqlTransaction))
                        {
                            sqlCommand.Parameters.Add("@menuId", SqlDbType.Int).Value = int.Parse(tbMenuId.Text);
                            sqlCommand.Parameters.Add("@menuName", SqlDbType.NVarChar, 100).Value = tbMenuName.Text;
                            sqlCommand.Parameters.Add("@menuPrice", SqlDbType.Float).Value = float.Parse(tbMenuPrice.Text);
                            if (menuImage != null)
                            {
                                sqlCommand.Parameters.Add("@menuImage", SqlDbType.Image).Value = menuImage;
                            }

                            sqlCommand.ExecuteNonQuery();
                            sqlTransaction.Commit();


                            MessageBox.Show("แก้ไขเรียบร้อยแล้ว", "ผลการทำงาน", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            getAllMenuToListView();
                            menuImage = null;
                            pbMenuImage.Image = null;
                            tbMenuId.Clear();
                            tbMenuName.Clear();
                            tbMenuPrice.Clear();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("พบข้อผิดพลาด กรุณาลองใหม่หรือติดต่อ IT : " + ex.Message);

                    }
                }
            }
        }

        private void btDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("ต้องการลบเมนูหรือไม่", "ยีนยัน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                //string connectionString = @"Server=SATJATUM\SQLEXPRESS01;Database=coffee_cafe_db;Trusted_Connection=True";

                using (SqlConnection sqlConnection = new SqlConnection(ConfigDb.connectionString))
                {
                    try
                    {
                        sqlConnection.Open();

                        SqlTransaction sqlTransaction = sqlConnection.BeginTransaction(); //ใช้กับ Insert/update/delete

                        //คำสั่ง SQL
                        String strSql = "DELETE FROM menu_tb WHERE menuId=@menuId";

                        //กำหนดค่าให้กับ SQL Parameter และสั่งให้คำสั่ง SQL ทำงาน
                        using (SqlCommand sqlCommand = new SqlCommand(strSql, sqlConnection, sqlTransaction))
                        {
                            sqlCommand.Parameters.Add("@menuId", SqlDbType.Int).Value = int.Parse(tbMenuId.Text);

                            sqlCommand.ExecuteNonQuery();
                            sqlTransaction.Commit();


                            MessageBox.Show("ลบเรียบร้อย", "ผลการทำงาน", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            getAllMenuToListView();
                            menuImage = null;
                            pbMenuImage.Image = null;
                            tbMenuId.Clear();
                            tbMenuName.Clear();
                            tbMenuPrice.Clear();
                        }

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("พบข้อผิดพลาด กรุณาลองใหม่หรือติดต่อ IT : " + ex.Message);
                    }
                }
            }
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            getAllMenuToListView();
            pbMenuImage.Image = null;
            menuImage = null;
            tbMenuId.Clear();
            tbMenuName.Clear();
            tbMenuPrice.Clear();
            btSave.Enabled = true;
            btUpdate.Enabled = false;
            btDelete.Enabled = false;
        }

        private void btClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
