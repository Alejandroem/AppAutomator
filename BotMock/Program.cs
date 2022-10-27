using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace BotMock
{
    static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            promptAgain();
        }

        static void promptAgain()
        {
            DialogResult result = MessageBox.Show("Some text", "Some title", MessageBoxButtons.AbortRetryIgnore);
            switch (result)
            {
                case DialogResult.Abort:
                    Application.Exit();
                    break;                
                case DialogResult.Retry:
                    Thread.Sleep(1000);
                    promptAgain();
                    break;
                case DialogResult.Ignore:
                    doWork();
                    promptAgain();
                    break;
            }
        }

        static void doWork()
        {
            for (int i = 0; i < 15; i++)
            {
                Thread.Sleep(1000);
                Console.WriteLine("Hello I'm Pausing because Im working ");
            }
        }
    }
}
