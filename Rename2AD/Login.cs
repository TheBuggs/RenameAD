using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Rename2AD
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }

        private void txtUsername_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btnOK.PerformClick();
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btnOK.PerformClick();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if ((this.txtUsername.Text).Length > 2 && (this.txtPassword.Text).Length > 0)
            {
                bool isExistNewName = false;
                bool hasError = false;
                try
                {
                    Http req = new Http(Program.Hostname, this.txtUsername.Text);

                    string jsonResult = req.sendJSON();
                    jsonResult = jsonResult.Replace('{', ' ');
                    jsonResult = jsonResult.Replace('}', ' ');
                    string[] words = jsonResult.Split(',');

                    Dictionary<string, string> json = new Dictionary<string, string>();

                    foreach (string row in words)
                    {
                        string[] word = row.Split(':');
                        if (word.Length == 2)
                        {
                            json.Add(word[0].ToString().Trim(), word[1].ToString().Trim());
                        }
                    }

                    string exist;
                    if (!json.TryGetValue("\"exist\"", out exist))
                    {
                        hasError = true;
                        return;
                    }

                    if (Int32.Parse(exist) == 1)
                    {
                        isExistNewName = true;
                    }
                }
                catch (Exception ex)
                {
                    hasError = true;
                }

                if (isExistNewName)
                {
                    MessageBox.Show("Компютър с това име съществува в активната директория!\nМоля опитайте с друго.", "Внимание", MessageBoxButtons.OK,
                                      MessageBoxIcon.Asterisk);
                    Application.Exit();
                }
                else
                {
                    if (hasError)
                    {
                        DialogResult errorQuestion = MessageBox.Show("Не може да се установи дали съществува компютър с такова име.\nЖелаете ли да продължите?", "Грешка",
                                   MessageBoxButtons.YesNo, MessageBoxIcon.Error);

                        if (errorQuestion == DialogResult.Yes)
                        {
                            Application.Exit();
                        }
                    } 
                    else
                    {
                        try
                        {
                            DialogResult dialogResult = MessageBox.Show("Сигурни ли сте?", "Warning",
                                                            MessageBoxButtons.YesNo,
                                                            MessageBoxIcon.Exclamation);

                            if (dialogResult == DialogResult.Yes)
                            {
                                SendData send = null;
                                Dictionary<string, string> dict = null;

                                try
                                {
                                    send = new SendData(Program.RealHostname.Name, System.Environment.MachineName, this.txtUsername.Text);
                                    dict = send.sendJSON();
                                }
                                catch { }

                                bool renamed = Rename.Run(this.txtUsername.Text, Regex.Unescape(this.txtPassword.Text));

                                if (renamed)
                                {
                                    try
                                    {
                                        send.sendErrorJSON();
                                    }
                                    catch { }

                                    string message = "Успешно преименувахте този компютър!\n\n Рестартиране!\n\n";
                                    string caption = "Информация";

                                    DialogResult dialogResultRestart = MessageBox.Show(message, caption, MessageBoxButtons.YesNo,
                                            MessageBoxIcon.Asterisk);


                                    if (dialogResultRestart == DialogResult.Yes)
                                    {
                                        System.Diagnostics.Process.Start("shutdown", "/r /t 0");
                                    }
                                    else
                                    {
                                        Application.Exit();
                                    }
                                }
                                else
                                {
                                    DialogResult dialogResultError = MessageBox.Show("Възникна грешка при опит за добавяне или преименуване на машината.\nЖелаете ли да затворите приложението или ще опитате с друг потребител или парола?", "Грешка",
                                    MessageBoxButtons.YesNo, MessageBoxIcon.Error);

                                    if (dialogResultError == DialogResult.Yes)
                                    {
                                        Application.Exit();
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Грешка: " + ex.Message + "\n", "Системна грешка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Имате непопълнена информация!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
