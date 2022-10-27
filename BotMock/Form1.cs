using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace BotMock
{
    public partial class windowBotMock : Form
    {
        public windowBotMock()
        {
            InitializeComponent();
        }


        //Continua al flujo
        private void btnContinue_Click(object sender, EventArgs e)
        {
            this.Hide();
            for(int i=0; i < 15; i++)
            {
                Thread.Sleep(1000);
                Console.WriteLine("Hello I'm Pausing because Im working ");
            }
            this.Show();
        }
        //Refresca la IP
        private void btnTryAgain_Click(object sender, EventArgs e)
        {

        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Saliendo de la aplicación");
            Application.Exit();
        }
    }
}
