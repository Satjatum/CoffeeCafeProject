using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace CoffeeCafeProject
{
    public partial class FrmMenu : Form
    {        
        public FrmMenu()
        {
            InitializeComponent();
        }

        private void FrmMenu_Load(object sender, EventArgs e)
        {
            pbMenuImage.Image = null;
            tbMenuId.Clear();
            tbMenuName.Clear();
            tbMenuPrice.Clear();
            btSave.Enabled = true;
            btUpdate.Enabled = false;
            btDelete.Enabled = false;
        }
    }
}
