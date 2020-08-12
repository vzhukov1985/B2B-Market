using Core.Services;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.InputFiles;

namespace UpdateDb_Service.Models
{
    public class Admin
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public static class AdminTGBot
    {
        private readonly static string settingsFile = CoreSettings.b2bDataLocalDir + CoreSettings.SettingsPath + "/" + CoreSettings.AdminTGBotSettingsFileName;
        private static ITelegramBotClient botClient;

        private static List<Admin> admins;
        private static string AddAdminPwdHash = "";
        private static List<long> AddAsAdminRequestIds;
        private static List<long> ChangeAdminPwdRequestIdsStep1;
        private static List<long> ChangeAdminPwdRequestIdsStep2;
        private static List<long> DropAdminRequestIds;
        private static List<long> ConnectOperatorRequestIds;

        private static void LoadSettings()
        {
            if (File.Exists(settingsFile))
            {
                XDocument xDoc = XDocument.Load(settingsFile);
                XElement xAdminTGBotSettings = xDoc.Root;
                List<XElement> xAdmins = xAdminTGBotSettings.Element("Administrators").Elements("Admin").ToList();

                foreach (XElement xAdmin in xAdmins)
                    admins.Add(new Admin { Id = xAdmin.Attribute("Id").Value, Username = xAdmin.Attribute("Username").Value, FirstName = xAdmin.Attribute("FirstName").Value, LastName = xAdmin.Attribute("LastName").Value });

                AddAdminPwdHash = xAdminTGBotSettings.Element("AddNewAdminPwdHash").Value;
            }
            else
            {
                XDocument xDoc = new XDocument();
                XElement xAdminTGBotSettings = new XElement("AdminTelegramBotSettings");
                XElement xAdmins = new XElement("Administrators");
                xAdminTGBotSettings.Add(xAdmins);

                XElement xPwdHash = new XElement("AddNewAdminPwdHash", "");
                xAdminTGBotSettings.Add(xPwdHash);

                xDoc.Add(xAdminTGBotSettings);

                xDoc.Save(settingsFile);
            }
        }

        private static void AddAdmin(long newAdminId, string newAdminUsername, string newAdminFirstName, string newAdminLastName)
        {
            Admin newAdmin = new Admin { Id = newAdminId.ToString(), Username = newAdminUsername, FirstName = newAdminFirstName, LastName = newAdminLastName };
            admins.Add(newAdmin);

            XDocument xDoc = XDocument.Load(settingsFile);
            XElement xAdminTGBotSettings = xDoc.Root;
            XElement xAdmins = xAdminTGBotSettings.Element("Administrators"); ;

            XElement xNewAdmin = new XElement("Admin");
            xNewAdmin.Add(new XAttribute("Id", newAdmin.Id));
            xNewAdmin.Add(new XAttribute("Username", newAdmin.Username == null ? "" : newAdmin.Username));
            xNewAdmin.Add(new XAttribute("FirstName", newAdmin.FirstName == null ? "" : newAdmin.FirstName));
            xNewAdmin.Add(new XAttribute("LastName", newAdmin.LastName == null ? "" : newAdmin.LastName));

            xAdmins.Add(xNewAdmin);

            xDoc.Save(settingsFile);
        }

        private static bool DropAdmin(string strId)
        {
            Admin admin = admins.Where(a => a.Id == strId).FirstOrDefault();
            if (admin != null)
            {
                admins.Remove(admin);
                XDocument xDoc = XDocument.Load(settingsFile);
                XElement xAdminTGBotSettings = xDoc.Root;
                List<XElement> xAdmins = xAdminTGBotSettings.Element("Administrators").Elements("Admin").ToList();
                xAdmins.Where(x => x.Attribute("Id").Value == strId).Remove();
                xDoc.Save(settingsFile);
                return true;
            }
            else
            {
                return false;
            }
        }


        private static void ChangeAddAdminPwdHash(string newPwd)
        {
            AddAdminPwdHash = Authentication.HashPassword(newPwd);
            XDocument xDoc = XDocument.Load(settingsFile);
            XElement xAdminTGBotSettings = xDoc.Root;
            XElement xPwdHash = xAdminTGBotSettings.Element("AddNewAdminPwdHash");
            xPwdHash.Value = AddAdminPwdHash;
            xDoc.Save(settingsFile);
        }

        public static async void SendMessageToAllAdmins(string message)
        {
            foreach (Admin admin in admins)
            {
                await botClient.SendTextMessageAsync(long.Parse(admin.Id), message, Telegram.Bot.Types.Enums.ParseMode.Html);
            }
        }

        public static void StartBot()
        {
            admins = new List<Admin>();
            AddAsAdminRequestIds = new List<long>();
            ChangeAdminPwdRequestIdsStep1 = new List<long>();
            ChangeAdminPwdRequestIdsStep2 = new List<long>();
            DropAdminRequestIds = new List<long>();
            ConnectOperatorRequestIds = new List<long>();
            LoadSettings();
            botClient = new TelegramBotClient("1193198147:AAFjrPo3p1Gmyj-SCpDOI7t1JrTJq6HmDy0");
            botClient.OnMessage += Bot_OnMessage;
            botClient.StartReceiving();
        }

        static async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            if (admins.Select(a => a.Id).Contains(e.Message.Chat.Id.ToString()))
            {
                //Administrators commands
                if (ChangeAdminPwdRequestIdsStep1.Contains(e.Message.Chat.Id))
                {
                    if (Authentication.CheckPassword(e.Message.Text, CoreSettings.SuperAdminTGBotPasswordHash))
                    {
                        ChangeAdminPwdRequestIdsStep2.Add(e.Message.Chat.Id);
                        await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Super-admin пароль введен верно. Введите новый пароль для добавления администраторов:");
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Super-admin пароль введен НЕВЕРНО");
                    }
                    ChangeAdminPwdRequestIdsStep1.Remove(e.Message.Chat.Id);
                    return;
                }
                if (ChangeAdminPwdRequestIdsStep2.Contains(e.Message.Chat.Id))
                {
                    ChangeAddAdminPwdHash(e.Message.Text);
                    ChangeAdminPwdRequestIdsStep2.Remove(e.Message.Chat.Id);
                    await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Пароль для добавления администраторов изменен");
                    return;
                }
                if (DropAdminRequestIds.Contains(e.Message.Chat.Id))
                {
                    if (DropAdmin(e.Message.Text))
                    {
                        await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Админ удален из списка");
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Админ с введенным Id не найден");
                    }
                    DropAdminRequestIds.Remove(e.Message.Chat.Id);
                    return;
                }
                if (ConnectOperatorRequestIds.Contains(e.Message.Chat.Id))
                {
                    if (long.TryParse(e.Message.Text, out long newId) && await OperatorTGBot.ConnectOperatorAsync(newId) == true)
                    {
                        await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Новый оператор подключен");
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Неверный Id");
                    }
                    ConnectOperatorRequestIds.Remove(e.Message.Chat.Id);
                    return;
                }


                if (e.Message.Text == "AddMeAsAdmin")
                {
                    await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Вы уже являетесь администратором");
                    return;
                }

                if (e.Message.Text == "UpdateDb")
                {
                    await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Поиск новых выгрузок...");
                    UpdateDbProcessor.UpdateDb();
                    await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Обновление БД завершено");
                    return;
                }

                if (e.Message.Text == "AgentLog")
                {
                    string logFilePath = CoreSettings.b2bDataLocalDir + "/" + CoreSettings.LogFileName;
                    if (File.Exists(logFilePath))
                    {
                        using (var stream = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            InputOnlineFile file = new InputOnlineFile(stream, "AgentLog.txt");
                            await botClient.SendDocumentAsync(e.Message.Chat.Id, file);
                        }
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Log-файл не существует");
                    }
                    return;
                }

                if (e.Message.Text == "ChangeAddAdminPassword")
                {
                    ChangeAdminPwdRequestIdsStep1.Add(e.Message.Chat.Id);
                    await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Введите super-admin пароль:");
                    return;
                }

                if (e.Message.Text == "ShowAllAdmins")
                {
                    string mes = "";
                    int count = 0;
                    mes += "<b>Текущие администраторы:</b>\n";
                    foreach (var admin in admins)
                    {
                        count++;
                        mes += $"{count}. {admin.FirstName} {admin.LastName}, Username: {admin.Username}, Id: {admin.Id}\n";
                    }
                    await botClient.SendTextMessageAsync(e.Message.Chat.Id, mes, Telegram.Bot.Types.Enums.ParseMode.Html);
                }
                if (e.Message.Text == "DropAdmin")
                {
                    DropAdminRequestIds.Add(e.Message.Chat.Id);
                    await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Введите Id:");
                }
                if (e.Message.Text == "DropOperator")
                {
                    if (OperatorTGBot.operatorId != 0)
                    {
                        OperatorTGBot.DropOperator();
                        await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Оператор был отключен от бота");
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Нет подключенного к боту оператора");
                    }
                    return;
                }
                if (e.Message.Text == "ConnectOperator")
                {
                    if (OperatorTGBot.operatorId == 0)
                    {
                        ConnectOperatorRequestIds.Add(e.Message.Chat.Id);
                        await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Введите Id нового оператора:");
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(e.Message.Chat.Id, "К системе уже подключен оператор. Отключите имеющегося");
                        return;
                    }
                    return;
                }
                if (e.Message.Text == "DbStatus")
                {
                    await botClient.SendTextMessageAsync(e.Message.Chat.Id, Broadcaster.GetDbDataStatus(), Telegram.Bot.Types.Enums.ParseMode.Html);
                    return;
                }

            }
            else
            {
                //Checking password when adding as Admin 
                if (AddAsAdminRequestIds.Contains(e.Message.Chat.Id))
                {
                    string pwdHash = AddAdminPwdHash == "" ? CoreSettings.SuperAdminTGBotPasswordHash : AddAdminPwdHash;
                    if (Authentication.CheckPassword(e.Message.Text, pwdHash))
                    {
                        AddAdmin(e.Message.Chat.Id, e.Message.Chat.Username, e.Message.Chat.FirstName, e.Message.Chat.LastName);
                        await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Пароль введен верно. Вы добавлены в администраторы");
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Пароль введен НЕВЕРНО. Вы не добавлены в администраторы");
                    }
                    AddAsAdminRequestIds.Remove(e.Message.Chat.Id);
                    return;
                }

                if (e.Message.Text == "AddMeAsAdmin")
                {
                    AddAsAdminRequestIds.Add(e.Message.Chat.Id);
                    await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Введите пароль:");
                    return;
                }
            }
        }

    }
}
