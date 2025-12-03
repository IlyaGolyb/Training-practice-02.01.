using System;
using System.Windows.Forms;
using МозаикаERP.Forms;

namespace МозаикаERP
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Показываем форму авторизации
            using (var loginForm = new LoginForm())
            {
                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    // Если авторизация успешна, запускаем главную форму
                    Application.Run(new MainForm());
                }
            }
        }
    }
}