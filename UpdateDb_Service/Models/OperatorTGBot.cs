using Core.DBModels;
using Core.Models;
using Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace UpdateDb_Service.Models
{
    public class OperatorTGBot
    {
        private readonly static string settingsFile = CoreSettings.b2bDataLocalDir + CoreSettings.SettingsPath + "/" + CoreSettings.OperatorTGBotSettingsFileName;
        private static ITelegramBotClient botClient;

        public static long operatorId=0;

        public static void LoadSettings()
        {
            if (File.Exists(settingsFile))
            {
                XDocument xDoc = XDocument.Load(settingsFile);
                XElement xAdminTGBotSettings = xDoc.Root;
                long.TryParse(xAdminTGBotSettings.Element("Operator").Attribute("Id").Value, out operatorId);
            }
            else
            {
                XDocument xDoc = new XDocument();
                XElement xOperatorTGBotSettings = new XElement("OperatorTelegramBotSettings");
                XElement xOperator = new XElement("Operator");
                xOperator.Add(new XAttribute("Id", "0"));

                xOperatorTGBotSettings.Add(xOperator);
                xDoc.Add(xOperatorTGBotSettings);

                xDoc.Save(settingsFile);
            }
        }
        
        private static void UpdateOperatorSettings()
        {
            XDocument xDoc = XDocument.Load(settingsFile);
            XElement xAdminTGBotSettings = xDoc.Root;
            xAdminTGBotSettings.Element("Operator").Attribute("Id").Value = operatorId.ToString();
            xDoc.Save(settingsFile);
        }

        public static void StartBot()
        {
            LoadSettings();
            botClient = new TelegramBotClient("1338661891:AAHYl_0yStZDDZWz6yga7lIEYJsxcRxBZaI");
            botClient.OnMessage += Bot_OnMessage;
            botClient.StartReceiving();
        }

        public static async void SendMessageToOperator(string message)
        {
            await botClient.SendTextMessageAsync(operatorId, message, Telegram.Bot.Types.Enums.ParseMode.Html);
        }

        public static async void DropOperator()
        {
            await botClient.SendTextMessageAsync(operatorId, "Вы были отключены от бота администратором");
            operatorId = 0;
            UpdateOperatorSettings();
        }

        public static async Task<bool> ConnectOperatorAsync(long Id)
        {
            try
            {
                await botClient.SendTextMessageAsync(Id, "Вы подключены к боту");
                operatorId = Id;
                UpdateOperatorSettings();
                return true;
            }
            catch
            {
                return false;
            }
        }

        static async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            if (e.Message.Chat.Id == operatorId)
            {
                //Operator commands
                if (e.Message.Text == "DbStatus")
                {
                    await botClient.SendTextMessageAsync(e.Message.Chat.Id, Broadcaster.GetDbDataStatus(), Telegram.Bot.Types.Enums.ParseMode.Html);
                    return;
                }
            }
            else
            {
                //All commands
                if (e.Message.Text == "ConnectMe")
                {
                    AdminTGBot.SendMessageToAllAdmins($"Оператор просит подключить его к боту. Id: {e.Message.Chat.Id}");
                    await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Запрос выслан администраторам. При подтверждении Вам будет выслано сообщение.");
                    return;
                }
            }
        }

    }
}
