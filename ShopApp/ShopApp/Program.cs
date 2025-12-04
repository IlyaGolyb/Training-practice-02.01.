using System;
using System.Windows.Forms;
using ShoeStoreLLC.Forms;
using ShoeStoreLLC.Data;

namespace ShoeStoreLLC
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Упрощенная проверка подключения
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                }
            }
            catch
            {
                MessageBox.Show("Не удалось подключиться к базе данных. Проверьте настройки подключения.",
                    "Ошибка подключения", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Показать форму входа
            var loginForm = new LoginForm();
            if (loginForm.ShowDialog() == DialogResult.OK)
            {
                // Авторизованный пользователь
                Application.Run(new MainForm(loginForm.CurrentUser));
            }
            else if (loginForm.DialogResult == DialogResult.Yes)
            {
                // Гость
                Application.Run(new MainForm(null));
            }
        }
    }
}