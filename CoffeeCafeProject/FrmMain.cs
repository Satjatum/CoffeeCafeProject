using System;
using System.Data.SqlClient;
using System.Data;
using System.Windows.Forms;

namespace CoffeeCafeProject
{
    public partial class FrmMain : Form
    {
        //ตัวแปรเก็บราคาเมนู
        float[] menuPrice = new float[10];

        //ตัวแปรเก็บรหัสสมาชิก
        int memberId = 0;
        public FrmMain()
        {
            InitializeComponent();
        }

        private void btMenu_Click(object sender, EventArgs e)
        {
            FrmMenu frmMenu = new FrmMenu();
            frmMenu.ShowDialog();
            ResetForm();
        }

        private void btMember_Click(object sender, EventArgs e)
        {
            FrmMember frmMember = new FrmMember();
            frmMember.ShowDialog();
        }

        // Method Reset Form
        private void ResetForm()
        {
            // ตั้งค่าให้ memberId เป็น 0
            memberId = 0;
            // ให้ rdMemberNo, rdMemberYes ไม่ถูกเลือก
            rdMemberNo.Checked = false;
            rdMemberYes.Checked = false;
            // ให้ tbMemberPhone ว่าง และใช้งานไม่ได้
            tbMemberPhone.Clear();
            tbMemberPhone.Enabled = false;
            // ให้ tbMemberName เป็นข้อความ (ชื่อสมาชิก๗
            tbMemberName.Text = "(ชื่อสมาชิก)";
            // ให้ lbMemberScore เป็น 0
            lbMemberScore.Text = "0";
            // ให้ lbOrderPay เป็น 0.00
            lbOrderPay.Text = "0.00";
            //เคลีย lvOrderMenu 
            lvOrderMenu.Items.Clear();
            lvOrderMenu.Columns.Clear();
            lvOrderMenu.FullRowSelect = true;
            lvOrderMenu.View = View.Details;
            lvOrderMenu.Columns.Add("ชื่อเมนู", 150, HorizontalAlignment.Left);
            lvOrderMenu.Columns.Add("ราคา", 80, HorizontalAlignment.Left);

            //ดึงข้อมูลรายการเมนูมาแสดงที่หน้าจอ และเก็บไว้ใช้กับตอนที่ผู้ใช้เลือกสั่งเมนู
            using (SqlConnection sqlConnection = new SqlConnection(ConfigDb.connectionString))
            {
                try
                {
                    sqlConnection.Open();

                    string strSQL = "SELECT menuName, menuPrice, menuImage  FROM menu_tb";

                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter(strSQL, sqlConnection))
                    {
                        //เอาข้อมูลที่ได้จาก strSQL เป็นก้อนใน dataAdapter มาทำให้เป็นตารางโดยใส่ไว้ใน DataTable
                        DataTable dataTable = new DataTable();
                        dataAdapter.Fill(dataTable);

                        // สร้างตัวแปรอ้างถึง PictureBox และ Button ที่จะเอารูปและชื่อเมนูไปแสดง
                        PictureBox[] pbMenuImage = { pbMenu1, pbMenu2, pbMenu3, pbMenu4, pbMenu5, pbMenu6, pbMenu7, pbMenu8, pbMenu9, pbMenu10 };
                        Button[] btMenuName = { btMenu1, btMenu2, btMenu3, btMenu4, btMenu5, btMenu6, btMenu7, btMenu8, btMenu9, btMenu10 };

                        //เคลีย pbMenuImage และ btMenuName ก่อนที่จะใส่ลงไปใหม่
                        for (int i = 0; i < 10; i++)
                        {
                            pbMenuImage[i].Image = Properties.Resources.menu;
                            btMenuName[i].Text = "เพิ่มเมนู";
                        }

                        // วนลูปเอาข้อมูลที่อยู่ใน dataTable กำหนดให้กับ pbMenuImage, btMenuName, MenuPrice
                        for (int i = 0; i < dataTable.Rows.Count ; i++)
                        {
                            btMenuName[i].Text = dataTable.Rows[i]["menuName"].ToString();
                            menuPrice[i] = float.Parse(dataTable.Rows[i]["menuPrice"].ToString());

                            //เอารูปไปกำหนดให้กับ pbMenuImage
                            if(dataTable.Rows[i]["menuImage"] != DBNull.Value)
                            {
                                byte[] imgByte = (byte[])dataTable.Rows[i]["menuImage"];
                                using (var ms = new System.IO.MemoryStream(imgByte))
                                {
                                    pbMenuImage[i].Image = System.Drawing.Image.FromStream(ms);
                                }
                            }
                            else
                            {
                                pbMenuImage[i].Image = null; // ถ้าไม่มีรูปให้เป็น null
                            }

                        }

                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("พบข้อผิดพลาด กรุณาลองใหม่หรือติดต่อ IT : " + ex.Message);
                }
       
            }
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            ResetForm();
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            ResetForm();
        }

        private void rdMemberNo_CheckedChanged(object sender, EventArgs e)
        {
            tbMemberPhone.Clear();
            tbMemberPhone.Enabled = false;
            tbMemberName.Text = "(ชื่อสมาชิก)";
            lbMemberScore.Text = "0";
            memberId = 0;   
        }

        private void rdMemberYes_CheckedChanged(object sender, EventArgs e)
        {
            tbMemberPhone.Clear();
            tbMemberPhone.Enabled = true;
            tbMemberName.Text = "(ชื่อสมาชิก)";
            lbMemberScore.Text = "0";
        }

        private void tbMemberPhone_KeyUp(object sender, KeyEventArgs e)
        {
            //ตรวจสอบว่าปุ่มที่กดแล้วปล่อยใช้ปุ่ม Enter หรือไม่
            //ถ้าไม่ใช้ก็ไม่ต้องทำอะไร แต่ถ้าใช้ให้เอา เบอร์โทรไปค้นใน Database
            //แล้วชื่อกับแต้มมาโชว์ ส่วนรหัสเอาไว้ใช้บันทึกลง Database
            if (e.KeyCode == Keys.Enter)
            {
                using (SqlConnection sqlConnection = new SqlConnection(ConfigDb.connectionString))
                {
                    try
                    {
                        sqlConnection.Open();

                        string strSQL = "SELECT memberId, memberName, memberScore FROM member_tb WHERE memberPhone=@memberPhone";

                        using (SqlCommand sqlCommand = new SqlCommand(strSQL, sqlConnection))
                        {
                            sqlCommand.Parameters.Add("@memberPhone", SqlDbType.NVarChar, 50).Value = tbMemberPhone.Text;

                            using (SqlDataAdapter dataAdapter = new SqlDataAdapter(sqlCommand))
                            {
                                //เอาข้อมูลที่ได้จาก strSQL เป็นก้อนใน dataAdapter มาทำให้เป็นตารางโดยใส่ไว้ใน DataTable
                                DataTable dataTable = new DataTable();
                                dataAdapter.Fill(dataTable);

                                if (dataTable.Rows.Count == 1)
                                {
                                    tbMemberName.Text = dataTable.Rows[0]["memberName"].ToString();
                                    lbMemberScore.Text = dataTable.Rows[0]["memberScore"].ToString();
                                    memberId = int.Parse(dataTable.Rows[0]["memberId"].ToString());
                                }
                                else
                                {
                                    MessageBox.Show("เบอร์โทรนี้ไม่มีในระบบ กรุณาลองใหม่..!");
                                }
                            }

                        }

                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show("พบข้อผิดพลาด กรุณาลองใหม่หรือติดต่อ IT : " + ex.Message);
                    }
                }
            }
        }

        private void btMenu1_Click(object sender, EventArgs e)
        {
            //ตรวจสอบว่าชื่อปุ่มเป็น "เพิ่มเมนู" หรือไม่ หากใช้ไม่ต้องทำอะไร
            //หากไม่ใช่ให้เพิ่มเมนูที่เลือกลงใน lvOrderMenu แล้วบวกแต้มเพิ่ม และบวกรวมเป็นเงินที่ต้องจ่าย
            if (btMenu1.Text != "เพิ่มเมนู")
            {
                ListViewItem item = new ListViewItem(btMenu1.Text);
                item.SubItems.Add(menuPrice[0].ToString());
                lvOrderMenu.Items.Add(item);

                //บวกแต้มสมาชิก ต้องตรวจสอบว่ามีสมาชิกหรือไม่
                if (tbMemberName.Text != "(ชื่อสมาชิก)")
                {
                    lbMemberScore.Text = (int.Parse(lbMemberScore.Text) + 1).ToString();
                }
            
                //บวกราคาเพิ่ม
                lbOrderPay.Text = (float.Parse(lbOrderPay.Text) + menuPrice[0]).ToString();

            }    
        }

        private void btMenu2_Click(object sender, EventArgs e)
        {
            if (btMenu2.Text != "เพิ่มเมนู")
            {
                ListViewItem item = new ListViewItem(btMenu2.Text);
                item.SubItems.Add(menuPrice[1].ToString());
                lvOrderMenu.Items.Add(item);

                //บวกแต้มสมาชิก
                if (tbMemberName.Text != "(ชื่อสมาชิก)")
                {
                    lbMemberScore.Text = (int.Parse(lbMemberScore.Text) + 1).ToString();
                }

                //บวกราคาเพิ่ม
                lbOrderPay.Text = (float.Parse(lbOrderPay.Text) + menuPrice[1]).ToString();

            }
        }

        private void btMenu3_Click(object sender, EventArgs e)
        {
            if (btMenu3.Text != "เพิ่มเมนู")
            {
                ListViewItem item = new ListViewItem(btMenu3.Text);
                item.SubItems.Add(menuPrice[2].ToString());
                lvOrderMenu.Items.Add(item);

                //บวกแต้มสมาชิก 
                if (tbMemberName.Text != "(ชื่อสมาชิก)")
                {
                    lbMemberScore.Text = (int.Parse(lbMemberScore.Text) + 1).ToString();
                }

                //บวกราคาเพิ่ม
                lbOrderPay.Text = (float.Parse(lbOrderPay.Text) + menuPrice[2]).ToString();

            }
        }

        private void btMenu4_Click(object sender, EventArgs e)
        {
            if (btMenu4.Text != "เพิ่มเมนู")
            {
                ListViewItem item = new ListViewItem(btMenu4.Text);
                item.SubItems.Add(menuPrice[3].ToString());
                lvOrderMenu.Items.Add(item);

                //บวกแต้มสมาชิก
                if (tbMemberName.Text != "(ชื่อสมาชิก)")
                {
                    lbMemberScore.Text = (int.Parse(lbMemberScore.Text) + 1).ToString();
                }

                //บวกราคาเพิ่ม
                lbOrderPay.Text = (float.Parse(lbOrderPay.Text) + menuPrice[3]).ToString();

            }
        }

        private void btMenu5_Click(object sender, EventArgs e)
        {
            if (btMenu5.Text != "เพิ่มเมนู")
            {
                ListViewItem item = new ListViewItem(btMenu5.Text);
                item.SubItems.Add(menuPrice[4].ToString());
                lvOrderMenu.Items.Add(item);

                //บวกแต้มสมาชิก
                if (tbMemberName.Text != "(ชื่อสมาชิก)")
                {
                    lbMemberScore.Text = (int.Parse(lbMemberScore.Text) + 1).ToString();
                }

                //บวกราคาเพิ่ม
                lbOrderPay.Text = (float.Parse(lbOrderPay.Text) + menuPrice[4]).ToString();

            }
        }

        private void btMenu6_Click(object sender, EventArgs e)
        {
            if (btMenu6.Text != "เพิ่มเมนู")
            {
                ListViewItem item = new ListViewItem(btMenu6.Text);
                item.SubItems.Add(menuPrice[5].ToString());
                lvOrderMenu.Items.Add(item);

                //บวกแต้มสมาชิก
                if (tbMemberName.Text != "(ชื่อสมาชิก)")
                {
                    lbMemberScore.Text = (int.Parse(lbMemberScore.Text) + 1).ToString();
                }

                //บวกราคาเพิ่ม
                lbOrderPay.Text = (float.Parse(lbOrderPay.Text) + menuPrice[5]).ToString();

            }
        }

        private void btMenu7_Click(object sender, EventArgs e)
        {
            if (btMenu7.Text != "เพิ่มเมนู")
            {
                ListViewItem item = new ListViewItem(btMenu7.Text);
                item.SubItems.Add(menuPrice[6].ToString());
                lvOrderMenu.Items.Add(item);

                //บวกแต้มสมาชิก
                if (tbMemberName.Text != "(ชื่อสมาชิก)")
                {
                    lbMemberScore.Text = (int.Parse(lbMemberScore.Text) + 1).ToString();
                }

                //บวกราคาเพิ่ม
                lbOrderPay.Text = (float.Parse(lbOrderPay.Text) + menuPrice[6]).ToString();

            }
        }

        private void btMenu8_Click(object sender, EventArgs e)
        {
            if (btMenu8.Text != "เพิ่มเมนู")
            {
                ListViewItem item = new ListViewItem(btMenu8.Text);
                item.SubItems.Add(menuPrice[7].ToString());
                lvOrderMenu.Items.Add(item);

                //บวกแต้มสมาชิก
                if (tbMemberName.Text != "(ชื่อสมาชิก)")
                {
                    lbMemberScore.Text = (int.Parse(lbMemberScore.Text) + 1).ToString();
                }

                //บวกราคาเพิ่ม
                lbOrderPay.Text = (float.Parse(lbOrderPay.Text) + menuPrice[7]).ToString();

            }
        }

        private void btMenu9_Click(object sender, EventArgs e)
        {
            if (btMenu9.Text != "เพิ่มเมนู")
            {
                ListViewItem item = new ListViewItem(btMenu9.Text);
                item.SubItems.Add(menuPrice[8].ToString());
                lvOrderMenu.Items.Add(item);

                //บวกแต้มสมาชิก
                if (tbMemberName.Text != "(ชื่อสมาชิก)")
                {
                    lbMemberScore.Text = (int.Parse(lbMemberScore.Text) + 1).ToString();
                }

                //บวกราคาเพิ่ม
                lbOrderPay.Text = (float.Parse(lbOrderPay.Text) + menuPrice[8]).ToString();

            }
        }

        private void btMenu10_Click(object sender, EventArgs e)
        {
            if (btMenu10.Text != "เพิ่มเมนู")
            {
                ListViewItem item = new ListViewItem(btMenu10.Text);
                item.SubItems.Add(menuPrice[9].ToString());
                lvOrderMenu.Items.Add(item);

                //บวกแต้มสมาชิก
                if (tbMemberName.Text != "(ชื่อสมาชิก)")
                {
                    lbMemberScore.Text = (int.Parse(lbMemberScore.Text) + 1).ToString();
                }

                //บวกราคาเพิ่ม
                lbOrderPay.Text = (float.Parse(lbOrderPay.Text) + menuPrice[9]).ToString();

            }
        }

        private void btSave_Click(object sender, EventArgs e)
        {
            //ตรวจสอบว่ามีรายการสั่งซื้อหรือไม่
            if (lbOrderPay.Text == "0.00")
            {
                MessageBox.Show("กรุณาเลือกเมนูที่ต้องการสั่งซื้อก่อน..!", "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (rdMemberYes.Checked != true && rdMemberNo.Checked != true)
            {
                MessageBox.Show("กรุณาเลือกสถานะสมาชิกก่อน..!", "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (rdMemberYes.Checked == true && tbMemberName.Text == "(ชื่อสมาชิก)")
            {
                MessageBox.Show("กรุณาค้นหาสมาชิกก่อน..!", "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                //ผ่านตรวจสอบมาได้ต้องทำ 3 อย่าง
                //1. บันทึก order_th (INSERT INTO...)
                //2. บันทึก order_detail_tb (INSERT INTO...)
                //3. บันทึกแก้ไขแต้มคะแนนของสมาชิก member_tb กรณีสมาชิก (UPDATE...SET...)
                using (SqlConnection sqlConnection = new SqlConnection(ConfigDb.connectionString))
                {
                    try
                    {
                        sqlConnection.Open();

                        SqlTransaction sqlTransaction = sqlConnection.BeginTransaction();

                        //บันทึกลง Order_th 
                        string strSQLOrder = "INSERT INTO order_tb (memberId, orderPay, createAt, updateAt) "
                                             + "VALUES (@memberId, @orderPay, @createAt, @updateAt); " 
                                             + "SELECT CAST(SCOPE_IDENTITY() AS INT)"; 

                        //ตัวแปรเก็บ orderId
                        int orderId;

                        using (SqlCommand sqlCommand = new SqlCommand(strSQLOrder, sqlConnection, sqlTransaction))
                        {
                            sqlCommand.Parameters.Add("@memberId", SqlDbType.Int).Value = memberId;
                            sqlCommand.Parameters.Add("@orderPay", SqlDbType.Float).Value = float.Parse(lbOrderPay.Text);
                            sqlCommand.Parameters.Add("@createAt", SqlDbType.Date).Value = DateTime.Now;
                            sqlCommand.Parameters.Add("@updateAt", SqlDbType.Date).Value = DateTime.Now;



                            orderId = (int)sqlCommand.ExecuteScalar();
                        }
                        //บันทึกลง Order_detail_tb

                        foreach (ListViewItem item in lvOrderMenu.Items)
                        {
                            string strSQLOrderDetail = "INSERT INTO order_detail_tb (orderId, menuName, menuPrice) "
                                                       + "VALUES (@orderId, @menuName, @menuPrice)";

                            using (SqlCommand sqlCommand = new SqlCommand(strSQLOrderDetail, sqlConnection, sqlTransaction))
                            {
                                sqlCommand.Parameters.Add("@orderId", SqlDbType.Int).Value = orderId;
                                sqlCommand.Parameters.Add("@menuName", SqlDbType.NVarChar, 100).Value = item.SubItems[0].Text;
                                sqlCommand.Parameters.Add("@menuPrice", SqlDbType.Float).Value = float.Parse(item.SubItems[1].Text);

                                sqlCommand.ExecuteNonQuery();
                            }
                        }


                        //แก้ไข memberScore ที่ member_tb  
                        if (rdMemberYes.Checked == true)
                        {
                            string strEditScore = "UPDATE member_tb SET memberScore=@memberScore WHERE memberId=@memberId";

                            using (SqlCommand sqlCommand = new SqlCommand(strEditScore, sqlConnection, sqlTransaction))
                            {
                                sqlCommand.Parameters.Add("@memberScore", SqlDbType.Int).Value = int.Parse(lbMemberScore.Text);
                                sqlCommand.Parameters.Add("@memberId", SqlDbType.Int).Value = memberId;

                                sqlCommand.ExecuteNonQuery();
                            }
                        }


                        //----------------------
                        sqlTransaction.Commit();
                        MessageBox.Show("บันทึกข้อมูลเรียบร้อยแล้ว", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ResetForm();
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show("พบข้อผิดพลาด กรุณาลองใหม่หรือติดต่อ IT : " + ex.Message);
                    }
                }

            }
        }
    }
}
